﻿using System;
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
		public ApObject()
		{
			Content = Items.None;
			Player = "";
			PlayerId = 0;
			ItemName = "";
			ObjectId = 0;
			Type = GameObjectType.Dummy;
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
		}
	}
	
	public class ApConfigs
	{ 
		public string ItemPlacementYaml { get; set; }
		public string StartingItemsYaml { get; set; }
		public string SetupYaml { get; set; }
		public string RoomsYaml { get; set; }
		public List<ApObject> ItemPlacement { get; set; }
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
			SetupYaml = "";
			RoomsYaml = "";
			ItemPlacement = new();
			StartingItems = new();
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
		public string GenerateRooms(bool crestshuffle, bool battlefieldshuffle, int mapshuffling, int companionshuffling, bool kaelismom, string seed)
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

			bool ap14used = false;
			if (companionshuffling == -1)
			{
				companionshuffling = 0;
				ap14used = true;
            }

			Battlefields = new();
			Overworld = new();
			GameLogic = new();

			if (ap14used)
			{
				var gameobjects = GameLogic.Rooms.SelectMany(r => r.GameObjects).ToList();
				foreach (var gameobject in gameobjects)
				{
					if (gameobject.Access.Contains(AccessReqs.TristamBoneItemGiven))
					{
						gameobject.Access.Remove(AccessReqs.TristamBoneItemGiven);
                        gameobject.Access.Add(AccessReqs.ReubenMine);
                    }
                    if (gameobject.OnTrigger.Contains(AccessReqs.TristamBoneItemGiven))
                    {
                        gameobject.OnTrigger.Remove(AccessReqs.TristamBoneItemGiven);
                        gameobject.OnTrigger.Add(AccessReqs.ReubenMine);
                    }
                }
			}
			// Locations & Logic
			Battlefields.ShuffleBattlefieldRewards(battlefieldshuffle, GameLogic, apconfigs, rng);
			GameLogic.CompanionsShuffle((CompanionsLocationType)companionshuffling, kaelismom, apconfigs, rng);
			GameLogic.CrestShuffle(crestshuffle, false, rng);
			GameLogic.FloorShuffle((MapShufflingMode)mapshuffling, false, rng);
			Overworld.ShuffleOverworld((MapShufflingMode)mapshuffling, GameLogic, Battlefields, new List<LocationIds>(),false, rng);

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
