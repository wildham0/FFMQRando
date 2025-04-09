using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.IO;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Security.Cryptography;
using System.Threading;

namespace FFMQLib
{
	public class ApObject : GameObjectData
	{ 
		public Items Content { get; set; }
		public string Player { get; set; }
		public int PlayerId { get; set; }
		public string ItemName { get; set; }
		public string LocationName { get; set; }
		public ApObject()
		{
			Content = Items.None;
			Player = "";
			PlayerId = 0;
			ItemName = "";
			ObjectId = 0;
			Type = GameObjectType.Dummy;
			LocationName = "";
		}
		public ApObject(Items content, LocationIds location)
		{
			Content = content;
			Player = "";
			PlayerId = 0;
			ItemName = "";
			ObjectId = 0;
			Type = GameObjectType.Dummy;
			Location = location;
			LocationName = "";
		}
	}
	
	public class ApConfigs
	{ 
		public string ItemPlacementYaml { get; set; }
		public string ExternalPlacementYaml { get; set; }
		public string StartingItemsYaml { get; set; }
		public string SetupYaml { get; set; }
		public string RoomsYaml { get; set; }
		public List<ApObject> ItemPlacement { get; set; }
		public List<ApObject> ExternalPlacement { get; set; }
		public List<Items> StartingItems { get; set; }
		public bool ApEnabled { get; set; }
		public string Seed;
		public string Name;
		public string Romname;
		public string Version;
		public string FileName;

		public ApConfigs()
		{
			ItemPlacementYaml = "";
			StartingItemsYaml = "";
			ExternalPlacementYaml = "";
			SetupYaml = "";
			RoomsYaml = "";
			ItemPlacement = new();
			StartingItems = new();
			ExternalPlacement = new();
			ApEnabled = false;
			Seed = "";
			Name = "";
			Romname = "";
			Version = "";
			FileName = "";
		}
		public void ProcessYaml()
		{
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();

			try
			{
				ItemPlacement = deserializer.Deserialize<List<ApObject>>(ItemPlacementYaml);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			if (ExternalPlacementYaml.Length > 0)
			{
				try
				{
					ExternalPlacement = deserializer.Deserialize<List<ApObject>>(ExternalPlacementYaml);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}

			try
			{
				StartingItems = deserializer.Deserialize<List<Items>>(StartingItemsYaml);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			try
			{
				var configsData = deserializer.Deserialize<ApConfigs>(SetupYaml);
				CopySetup(configsData);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			ApEnabled = true;
		}
		public void CopySetup(ApConfigs inputConfigs)
		{
			Seed = inputConfigs.Seed;
			Name = inputConfigs.Name;
			Romname = inputConfigs.Romname;
			Version = inputConfigs.Version;
		}
		public byte[] GetRomName()
		{
			byte[] convertedRomName = new byte[0x15];
			Encoding.UTF8.GetBytes(Romname).Take(0x15).ToArray().CopyTo(convertedRomName, 0);

			return convertedRomName;
		}
		public byte[] GetSeed()
		{
			return Blob.FromHex(Seed);
		}
	}
	public partial class ItemsPlacement
	{
		public void PlaceApItems(Flags flags, ApConfigs apconfigs, List<GameObject> initialGameObjects, FFMQRom rom, MT19337 rng)
		{
			List<Items> consumableList = rom.GetFromBank(0x01, 0x801E, 0xDD).ToBytes().Select(x => (Items)x).ToList();
			List<Items> finalConsumables = rom.GetFromBank(0x01, 0x80F2, 0x04).ToBytes().Select(x => (Items)x).ToList();

			List<Items> consumables = new() { Items.CurePotion, Items.HealPotion, Items.Refresher, Items.Seed };
			List<Items> nonKIs = consumables.Concat(new List<Items>() { Items.BombRefill, Items.ProjectileRefill }).ToList();

			ItemsLocations = new(initialGameObjects.Select(x => new GameObject(x)));
			StartingItems = apconfigs.StartingItems.ToList();

			foreach (var placedObject in apconfigs.ItemPlacement)
			{
				var currentObject = ItemsLocations.Find(x => x.ObjectId == placedObject.ObjectId && x.Type == placedObject.Type);
				if (currentObject == null)
				{
					Console.WriteLine("AP generation: Non matching object at :" + placedObject.ObjectId + " - " + placedObject.Type);
					continue;
				}
				currentObject.Content = (placedObject.Content == Items.APItemFiller) ? Items.APItem : placedObject.Content;
				currentObject.IsPlaced = true;
				if (placedObject.Type == GameObjectType.Chest || placedObject.Type == GameObjectType.Box)
				{
					currentObject.Type = nonKIs.Append(Items.APItemFiller).Contains(placedObject.Content) ? GameObjectType.Box : GameObjectType.Chest;
					if (nonKIs.Contains(placedObject.Content))
					{
						currentObject.Reset = true;
					}
				}
			}

			var unfilledLocations = ItemsLocations.Where(x => x.IsPlaced == false && (x.Type == GameObjectType.NPC || x.Type == GameObjectType.BattlefieldItem || (x.Type == GameObjectType.Chest && x.ObjectId < 0x20))).ToList();

			foreach (var location in unfilledLocations)
			{
				location.Content = rng.PickFrom(consumables);
				location.IsPlaced = true;
				if (location.Type == GameObjectType.Chest || location.Type == GameObjectType.Box)
				{
					location.Type = GameObjectType.Box;
					location.Reset = true;
				}
			}

			// Place consumables
			var consumableBox = ItemsLocations.Where(x => !x.IsPlaced && (x.Type == GameObjectType.Box || x.Type == GameObjectType.Chest) && (x.ObjectId < 0xF2 || x.ObjectId > 0xF5)).ToList();

			foreach (var box in consumableBox)
			{
				if (flags.ShuffleBoxesContent)
				{
					box.Content = rng.TakeFrom(consumableList);
				}
				else
				{
					box.Content = consumableList[box.ObjectId - 0x1E];
				}

				box.IsPlaced = true;
				if (box.Type == GameObjectType.Chest || box.Type == GameObjectType.Box)
				{
					box.Type = GameObjectType.Box;
					box.Reset = true;
				}
			}

			// Add the final chests so we can update their properties
			List<GameObject> finalChests = ItemsLocations.Where(x => x.Type == GameObjectType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList();

			for (int i = 0; i < finalChests.Count; i++)
			{
				finalChests[i].Content = finalConsumables[i];
				finalChests[i].IsPlaced = true;
			}
		}
	}

	public partial class FFMQRom : SnesRom
	{
		public string GenerateRooms(bool crestshuffle, bool battlefieldshuffle, int mapshuffling, int companionshuffling, bool kaelismom, bool? refoverworldshuffle, string seed)
		{
			ApConfigs apconfigs = new();
			seed = seed.PadLeft(8, '0').Substring(0,8);

			var blobseed = Blob.FromHex(seed);
			MT19337 rng;

			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(blobseed);
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			bool ap15used = false;
			bool overworldshuffle = false;
			if (refoverworldshuffle is null)
			{
				ap15used = true;
			}
			else
			{
				overworldshuffle = (bool)refoverworldshuffle;
			}

			Battlefields = new();
			Overworld = new();
			GameLogic = new();

			// Parse options if on 1.5
			if (ap15used)
			{
				switch (mapshuffling)
				{
					case 0:
						overworldshuffle = false;
						mapshuffling = 0;
						break;
					case 1:
						overworldshuffle = true;
						mapshuffling = 0;
						break;
					case 2:
						overworldshuffle = false;
						mapshuffling = 2;
						break;
					case 3:
						overworldshuffle = true;
						mapshuffling = 2;
						break;
					case 4:
						overworldshuffle = true;
						mapshuffling = 3;
						break;
				}
			}
			// Locations & Logic
			Battlefields.ShuffleBattlefieldRewards(battlefieldshuffle, GameLogic, apconfigs, rng);
			GameLogic.CompanionsShuffle((CompanionsLocationType)companionshuffling, kaelismom, apconfigs, rng);
			GameLogic.CrestShuffle(crestshuffle, false, rng);
			GameLogic.FloorShuffle((MapShufflingMode)mapshuffling, false, rng);
			Overworld.ShuffleOverworld(overworldshuffle, (MapShufflingMode)mapshuffling, GameLogic, Battlefields, new List<LocationIds>(),false, rng);

			// Update Logic Requirements to old logic if still on 1.5
			if (ap15used)
			{
				var gameobjects = GameLogic.Rooms.SelectMany(r => r.GameObjects).ToList();
				var links = GameLogic.Rooms.SelectMany(r => r.Links).ToList();

				foreach (var gameobject in gameobjects)
				{
					if (gameobject.Access.Contains(AccessReqs.TreeWitherPerson))
					{
						gameobject.Access.Remove(AccessReqs.TreeWitherPerson);
						gameobject.Access.Add(AccessReqs.Kaeli1);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.TreeWitherPerson))
					{
						gameobject.OnTrigger.Remove(AccessReqs.TreeWitherPerson);
						gameobject.OnTrigger.Add(AccessReqs.Kaeli1);
					}

					if (gameobject.Access.Contains(AccessReqs.HealedPerson))
					{
						gameobject.Access.Remove(AccessReqs.HealedPerson);
						gameobject.Access.Add(AccessReqs.Kaeli2);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.HealedPerson))
					{
						gameobject.OnTrigger.Remove(AccessReqs.HealedPerson);
						gameobject.OnTrigger.Add(AccessReqs.Kaeli2);
					}

					if (gameobject.Access.Contains(AccessReqs.Kaeli))
					{
						gameobject.Access.Remove(AccessReqs.Kaeli);
						gameobject.Access.Add(AccessReqs.Kaeli1);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Kaeli))
					{
						gameobject.OnTrigger.Remove(AccessReqs.Kaeli);
						gameobject.OnTrigger.Add(AccessReqs.Kaeli1);
					}

					if (gameobject.Access.Contains(AccessReqs.Reuben))
					{
						gameobject.Access.Remove(AccessReqs.Reuben);
						gameobject.Access.Add(AccessReqs.Reuben1);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Reuben))
					{
						gameobject.OnTrigger.Remove(AccessReqs.Reuben);
						gameobject.OnTrigger.Add(AccessReqs.Reuben1);
					}

					if (gameobject.Access.Contains(AccessReqs.Phoebe))
					{
						gameobject.Access.Remove(AccessReqs.Phoebe);
						gameobject.Access.Add(AccessReqs.Phoebe1);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Phoebe))
					{
						gameobject.OnTrigger.Remove(AccessReqs.Phoebe);
						gameobject.OnTrigger.Add(AccessReqs.Phoebe1);
					}

					if (gameobject.Access.Contains(AccessReqs.BoneWaterwayBombed))
					{
						gameobject.Access.Remove(AccessReqs.BoneWaterwayBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.BoneWaterwayBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					
					if (gameobject.Access.Contains(AccessReqs.Skull3Bombed))
					{
						gameobject.Access.Remove(AccessReqs.Skull3Bombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Skull3Bombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject); ;
					}

					if (gameobject.Access.Contains(AccessReqs.Wintry3FBombed))
					{
						gameobject.Access.Remove(AccessReqs.Wintry3FBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Wintry3FBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.Wintry2FBombed))
					{
						gameobject.Access.Remove(AccessReqs.Wintry2FBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Wintry2FBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.MineParallelBombed))
					{
						gameobject.Access.Remove(AccessReqs.MineParallelBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.MineParallelBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.MineClimbingBombed))
					{
						gameobject.Access.Remove(AccessReqs.MineClimbingBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.MineClimbingBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.MineCrescentBombed))
					{
						gameobject.Access.Remove(AccessReqs.MineCrescentBombed);
						gameobject.Access.Add(AccessReqs.Bomb);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.MineCrescentBombed))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.PhoebeVisitedCave))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.ReubenVisitedMine))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.Squidite))
					{
						gameobject.Access.Remove(AccessReqs.Squidite);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.Squidite))
					{
						GameLogic.Rooms.Find(r => r.GameObjects.Contains(gameobject)).GameObjects.Remove(gameobject);
					}

					if (gameobject.Access.Contains(AccessReqs.SnowCrab))
					{
						gameobject.Access.Remove(AccessReqs.SnowCrab);
						gameobject.Access.Add(AccessReqs.FreezerCrab);
					}

					if (gameobject.OnTrigger.Contains(AccessReqs.SnowCrab))
					{
						gameobject.OnTrigger.Remove(AccessReqs.SnowCrab);
						gameobject.OnTrigger.Add(AccessReqs.FreezerCrab);
						gameobject.Name = "Freezer Crab";
					}
				}

				foreach (var link in links)
				{
					if (link.Access.Contains(AccessReqs.BoneWaterwayBombed))
					{
						link.Access.Remove(AccessReqs.BoneWaterwayBombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.Skull3Bombed))
					{
						link.Access.Remove(AccessReqs.Skull3Bombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.Wintry2FBombed))
					{
						link.Access.Remove(AccessReqs.Wintry2FBombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.Wintry3FBombed))
					{
						link.Access.Remove(AccessReqs.Wintry3FBombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.MineParallelBombed))
					{
						link.Access.Remove(AccessReqs.MineParallelBombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.MineClimbingBombed))
					{
						link.Access.Remove(AccessReqs.MineClimbingBombed);
						link.Access.Add(AccessReqs.Bomb);
					}

					if (link.Access.Contains(AccessReqs.MineCrescentBombed))
					{
						link.Access.Remove(AccessReqs.MineCrescentBombed);
						link.Access.Add(AccessReqs.Bomb);
					}
				}
			}

			return GameLogic.OutputRooms();
		}
		public void ArchipelagoSupport(bool apenabled)
		{
			ItemFetcher();
			APItem();
			if (apenabled)
			{
				NoCantCarry();
			}
		}
		public void ItemFetcher()
		{
			PutInBank(0x01, 0x82A9, Blob.FromHex("2200801520b3e920f28222108015ea"));
			PutInBank(0x15, 0x8000, Blob.FromHex("eef7199cf8196b"));
			PutInBank(0x15, 0x8010, Blob.FromHex("adb019f0016b08e220aff01f70f013a9018db019a9508dee19a9088def1928a9016b28a9006b"));
			PutInBank(0x15, 0x8550, Blob.FromHex("08c230aff11f708dd10fe230a9008ff01f70286b"));

			TileScripts.AddScript(0x50, new ScriptBuilder(new List<string>
			{
				"05EDF01F70",
				"057F",
				"115F01",
				"0C600101",
				"62",
				"09508515",
				"05fd92[10]",  // Mirror/Mask script
				"17922bf205e1020c05050f",
				"05fc94[10]",
				"2c0021",
				"00"
			}));
		}
		public void APItem()
		{
			// Set sprite bank
			PutInBank(0x00, 0x8525, Blob.FromHex("5c508015eaea"));
			PutInBank(0x15, 0x8050, Blob.FromHex("8d1621ad700029ff00c9f000f007f404005c2b8500f415009c70005c2b85006b"));

			// Set sprite adresss
			PutInBank(0x00, 0xB6B5, Blob.FromHex("5c808015"));
			PutInBank(0x15, 0x8080, Blob.FromHex("9c6500a59e8d7000c9de00b0045cbab600f011a900818562a990038d6400e2305ccbb600a901008562a920008d6400e2305ccbb600"));

			// Set item name
			PutInBank(0x03, 0xB50B, Blob.FromHex("07D080150A13B5"));
			PutInBank(0x15, 0x80D0, Blob.FromHex("0bf0dd80054d0c054320c10c00053df0801500"));
			PutInBank(0x15, 0x80F0, Blob.FromHex("03039aa9ffa2c7b8c0030303")); // AP Item

			// New sprite
			PutInBank(0x15, 0x8100, Blob.FromHex("0000010102031c1f3e3f7f7f7f7f7f7f0001031f2341415d")); // First tile
			PutInBank(0x15, 0x8118, Blob.FromHex("0000c0c020e01cfc3ee27fc17fc1ffdd00c0e0fce2c1c1dd")); // Second tile
			PutInBank(0x15, 0x8280, Blob.FromHex("3e227f417e437c473c271c1f020301013e7f7e7c3c1c0201")); // Third tile + 0x180 from first tile
			PutInBank(0x15, 0x8298, Blob.FromHex("2323c1c121e111f112f21cfc20e0c0c03fff3f1f1e1c20c0")); // Fourth tile

			// New Palette
			PutInBank(0x07, 0xDC74, Blob.FromHex("00000f3f9b429e4b79620f62f9410821")); // Palette 0x390

			// Palette hacks
			PutInBank(0x00, 0x8482, Blob.FromHex("22008515ea")); // zero out b part of x register when loading palette selector
			PutInBank(0x00, 0x848A, Blob.FromHex("22108515ea")); // zero out b part of x register when loading palette selector
			PutInBank(0x00, 0x850f, Blob.FromHex("eaeaea")); // don't clear the b part of x register
			PutInBank(0x15, 0x8500, Blob.FromHex("08e230a988aef400286b"));
			PutInBank(0x15, 0x8510, Blob.FromHex("08e230a998aef700286b"));
		}
		public void NoCantCarry()
		{
			PutInBank(0x00, 0xDB31, Blob.FromHex("80")); // Always branch when opening a chest, even if quantity is 99
		}
	}
}
