﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using static System.Math;
using System.ComponentModel;

namespace FFMQLib
{
	public enum ItemShuffleChests : int
	{
		[Description("Prioritize")]
		Prioritize = 0,
		[Description("Include")]
		Include,
	}
	public enum ItemShuffleNPCsBattlefields : int
	{
		[Description("Prioritize")]
		Prioritize = 0,
		[Description("Include")]
		Include,
		[Description("Exclude")]
		Exclude,
	}
	public enum ItemShuffleBoxes : int
	{
		[Description("Include")]
		Include = 0,
		[Description("Exclude")]
		Exclude,
	}
	public enum LogicOptions : int
	{
		[Description("Friendly")]
		Friendly = 0,
		[Description("Standard")]
		Standard,
		[Description("Expert")]
		Expert,
	}
	public partial class ItemsPlacement
	{
		private Dictionary<AccessReqs, int> AccessToGp = new()
		{
			{ AccessReqs.Gp150, 150 },
			{ AccessReqs.Gp200, 200 },
			{ AccessReqs.Gp300, 300 },
			{ AccessReqs.Gp500, 500 },
			{ AccessReqs.Gp600, 600 },
			{ AccessReqs.Gp900, 900 },
			{ AccessReqs.Gp1200, 1200 },
		};

		public List<Items> StartingItems { get; set; }
		public List<GameObject> ItemsLocations { get; set;  }
		
		private const int TreasuresOffset = 0x8000;
		private int GpCount;
		private PowerLevel PowerLevel;
		private List<Items> PlacedItems;
		private List<AccessReqs> ProcessedAccessReqs;

		private class RegionWeight
		{ 
			public MapRegions Region { get; set; }
			public int Weight { get; set; }

			public RegionWeight(MapRegions _region, int _weight)
			{
				Region = _region;
				Weight = _weight;
			}
		}
		public ItemsPlacement(Flags flags, List<GameObject> initialGameObjects, Companions companions, ApConfigs apconfigs, FFMQRom rom, MT19337 rng)
		{
			if (apconfigs.ApEnabled)
			{
                PlaceApItems(flags, apconfigs, initialGameObjects, rom, rng);
            }
			else
			{
				PlaceItems(flags, initialGameObjects, companions, rom, rng);
			}
		}
		public void PlaceItems(Flags flags, List<GameObject> initialGameObjects, Companions companions, FFMQRom rom, MT19337 rng)
		{
			bool badPlacement = true;
			int counter = 0;
			//int placedChests = 0;
			int prioritizedLocationsCount = 0;
			int prioritizedItemsCount = 0;
			int looseItemsCount = 0;

			PowerLevel = new(flags.CompanionLevelingType, flags.ProgressiveGear, companions);

			List<Items> consumableList = rom.GetFromBank(0x01, 0x801E, 0xDD).ToBytes().Select(x => (Items)x).ToList();
			List<Items> finalConsumables = rom.GetFromBank(0x01, 0x80F2, 0x04).ToBytes().Select(x => (Items)x).ToList();


			List<RegionWeight> regionsWeight = new() { new RegionWeight(MapRegions.Foresta, 1), new RegionWeight(MapRegions.Aquaria, 1), new RegionWeight(MapRegions.Fireburg, 1), new RegionWeight(MapRegions.Windia, 1) };
			List<GameObjectType> validTypes = new() { GameObjectType.BattlefieldItem, GameObjectType.Box, GameObjectType.Chest, GameObjectType.NPC };

			List<GameObject> initialItemlocations = initialGameObjects.ToList();

			while (badPlacement)
			{
				badPlacement = false;
				//placedChests = 0;

				//bool placedMirror = false;
				//bool placedMask = false;

				regionsWeight.Find(x => x.Region == MapRegions.Foresta).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Aquaria).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Fireburg).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Windia).Weight = 1;

				ItemsList itemsList = new(flags, rng);
				StartingItems = itemsList.Starting;
				GpCount = 0;
				PowerLevel.Initialize();

				ItemsLocations = new(initialItemlocations.Select(x => new GameObject(x)));

				prioritizedLocationsCount = ItemsLocations.Where(x => x.Prioritize == true).Count();

				looseItemsCount = Max(0, itemsList.Count - prioritizedLocationsCount);
				prioritizedItemsCount = Min(prioritizedLocationsCount, itemsList.Count);

				PlacedItems = new();
				ProcessedAccessReqs = new();

				List<Items> nonRequiredItems = flags.SkyCoinMode == SkyCoinModes.Standard ? itemsList.Starting : itemsList.Starting.Append(Items.SkyCoin).ToList();

				List<AccessReqs> accessReqsToProcess = new();
				
				// Apply starting items access
				foreach (var item in nonRequiredItems)
				{
					ProcessRequirements(item, ItemsLocations);
				}

				while (itemsList.Count > 0)
				{
					Items itemToPlace;

					List<GameObject> validLocations = ItemsLocations.Where(x => validTypes.Contains(x.Type) &&
						x.Accessible &&
						x.IsPlaced == false &&
						x.Exclude == false &&
						x.Cost <= GpCount &&
                        (itemsList.NoMoreProgression ? true : x.Location != LocationIds.DoomCastle)
                        ).ToList();

					if (flags.LogicOptions == LogicOptions.Friendly)
					{
						if (!itemsList.MirrorIsPlaced && validLocations.Where(x => x.Location == LocationIds.IcePyramid).Any())
						{
							validLocations = validLocations.Where(x => x.Location != LocationIds.IcePyramid).ToList();
							itemsList.ForcedItem = Items.MagicMirror;
						}
						else if (!itemsList.MaskIsPlaced && validLocations.Where(x => x.Location == LocationIds.Volcano).Any())
						{
							validLocations = validLocations.Where(x => x.Location != LocationIds.Volcano).ToList();
							itemsList.ForcedItem = Items.Mask;
						}
					}

					List<GameObject> validLocationsPriorized = validLocations.Where(x => x.Prioritize == true).ToList();
					List<GameObject> validLocationsLoose = validLocations.Where(x => x.Prioritize == false).ToList();

					int diceRoll = rng.Between(1, itemsList.Count);

					if ((validLocationsPriorized.Any() && validLocationsLoose.Any() && diceRoll <= prioritizedItemsCount) ||
						(validLocationsPriorized.Any() && !validLocationsLoose.Any()))
					{
						validLocations = validLocationsPriorized;
						prioritizedItemsCount--;
					}
					else if (looseItemsCount > 0)
					{
						validLocations = validLocationsLoose;
						looseItemsCount--;
					}
					else
					{
						validLocations = new();
					}

					List<MapRegions> weightedRegionList = new();
					weightedRegionList.AddRange(Enumerable.Repeat(MapRegions.Foresta, 8 / regionsWeight.Find(x => x.Region == MapRegions.Foresta).Weight));
					weightedRegionList.AddRange(Enumerable.Repeat(MapRegions.Aquaria, 16 / regionsWeight.Find(x => x.Region == MapRegions.Aquaria).Weight));
					weightedRegionList.AddRange(Enumerable.Repeat(MapRegions.Fireburg, 16 / regionsWeight.Find(x => x.Region == MapRegions.Fireburg).Weight));
					weightedRegionList.AddRange(Enumerable.Repeat(MapRegions.Windia, 16 / regionsWeight.Find(x => x.Region == MapRegions.Windia).Weight));

					MapRegions favoredRegion = rng.PickFrom(weightedRegionList);

					int validLocationsCount = validLocations.Count;

					List<GameObject> validLocationsFavored = validLocations.Where(x => x.Region == favoredRegion).ToList();

					if (validLocationsFavored.Any())
					{
						validLocations = validLocationsFavored;
					}

					if (!validLocations.Any() && itemsList.Count > 0)
					{
						Console.WriteLine("Attempt " + counter + " - Invalid Placement ");
						var unfiledValidLocations = ItemsLocations.Where(x => !x.Accessible && x.Prioritize).ToList();
						counter++;
						badPlacement = true;
						if (counter >= 100)
						{
							throw new TimeoutException("Logic failure; try another seed.");
						}
						break;
					}
					
					itemToPlace = itemsList.NextItem(validLocationsCount, rng);

					GameObject targetLocation = rng.PickFrom(validLocations);
					targetLocation.Content = itemToPlace;
					targetLocation.IsPlaced = true;
					GpCount += targetLocation.Cost;
					regionsWeight.Find(x => x.Region == targetLocation.Region).Weight++;

					if (targetLocation.Type == GameObjectType.Chest || targetLocation.Type == GameObjectType.Box)
					{ 
						targetLocation.Type = GameObjectType.Chest;
					}
					
					PlacedItems.Add(itemToPlace);
					//Console.WriteLine(Enum.GetName(targetLocation.Location) + "_" + targetLocation.ObjectId + " - " + Enum.GetName(itemToPlace));

					ProcessRequirements(itemToPlace, ItemsLocations);

					/*


					List<AccessReqs> powerLevelsToProcess = UpdatePowerLevel(PlacedItems);

					List<AccessReqs> result = new();

					if (AccessReferences.ItemAccessReq.TryGetValue(itemToPlace, out result) || powerLevelsToProcess.Any())
					{
						if (result != null)
						{
							result = result.Concat(powerLevelsToProcess).ToList();
						}
						else
						{
							result = powerLevelsToProcess;
						}
						
						ProcessRequirements(result);
					}*/
				}

				
				//var unfiledValidLocations = ItemsLocations.Where(x => x.Accessible && x.Content == Items.None).ToList();
				
				/*var unfiledValidLocations = ItemsLocations.Where(x => x.Prioritize == true && x.Content == Items.None).ToList();
				Console.WriteLine("**** Unfiled Locations ****");
				foreach (var loc in unfiledValidLocations)
				{
					Console.WriteLine(Enum.GetName(loc.Location) + " - " + loc.ObjectId);
				}*/
			}

			// Sky Coins
			if (flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				var validSkyCoinLocations = ItemsLocations.Where(x => validTypes.Contains(x.Type) && x.IsPlaced == false && x.Prioritize == false && x.Exclude == false && x.Location != LocationIds.DoomCastle).ToList();

				if(validSkyCoinLocations.Count < 40)
				{
					throw new Exception("Sky Coin Pieces error: not enough valid locations");
				}

				for (int i = 0; i < 40; i++)
				{
					GameObject targetLocation = rng.TakeFrom(validSkyCoinLocations);
					targetLocation.Content = Items.SkyCoin;
					targetLocation.IsPlaced = true;

					if (targetLocation.Type == GameObjectType.Chest || targetLocation.Type == GameObjectType.Box)
					{
						targetLocation.Type = GameObjectType.Chest;
					}
				}
			}

			// Fill excluded and unfilled locations
			List<Items> consumables = new() { Items.CurePotion, Items.HealPotion, Items.Refresher, Items.Seed };
			
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

			for(int i = 0; i < finalChests.Count; i++)
			{
				finalChests[i].Content = finalConsumables[i];
				finalChests[i].IsPlaced = true;
			}
		}
		private void ProcessRequirements2(List<AccessReqs> accessReqToProcess)
		{
			while (accessReqToProcess.Any())
			{
				AddGp(accessReqToProcess);

				if (!accessReqToProcess.Any())
				{
					break;
				}

				var currentReq = accessReqToProcess.First();

				// Update Locations
				List<GameObject> unaccessibleLocations = ItemsLocations.Where(x => x.Accessible == false).ToList();
				for (int i = 0; i < unaccessibleLocations.Count; i++)
				{
					for (int j = 0; j < unaccessibleLocations[i].AccessRequirements.Count; j++)
					{
						unaccessibleLocations[i].AccessRequirements[j] = unaccessibleLocations[i].AccessRequirements[j].Where(x => x != currentReq).ToList();
						if (!unaccessibleLocations[i].AccessRequirements[j].Any())
						{
							unaccessibleLocations[i].Accessible = true;
							if (unaccessibleLocations[i].Type == GameObjectType.Trigger)
							{
								accessReqToProcess.AddRange(unaccessibleLocations[i].OnTrigger);
							}
						}
					}
				}

				accessReqToProcess.Remove(currentReq);
			}
		}
		private void ProcessRequirements(Items itemToPlace, List<GameObject> locations)
		{
			List<AccessReqs> accessReqToProcess = new();

			if (AccessReferences.ItemAccessReq.TryGetValue(itemToPlace, out var result))
			{
				accessReqToProcess.AddRange(result);
			}

			accessReqToProcess.AddRange(PowerLevel.GetNewPowerLevel(itemToPlace, locations));

			while (accessReqToProcess.Any())
			{
				AddGp(accessReqToProcess);

				if (!accessReqToProcess.Any())
				{
					break;
				}

				var currentReq = accessReqToProcess.First();

				// Update Locations
				List<GameObject> unaccessibleLocations = ItemsLocations.Where(x => x.Accessible == false).ToList();
				for (int i = 0; i < unaccessibleLocations.Count; i++)
				{
					for (int j = 0; j < unaccessibleLocations[i].AccessRequirements.Count; j++)
					{
						unaccessibleLocations[i].AccessRequirements[j] = unaccessibleLocations[i].AccessRequirements[j].Where(x => x != currentReq).ToList();
						if (!unaccessibleLocations[i].AccessRequirements[j].Any())
						{
							unaccessibleLocations[i].Accessible = true;
							if (unaccessibleLocations[i].Type == GameObjectType.Trigger)
							{
								accessReqToProcess.AddRange(unaccessibleLocations[i].OnTrigger);
							}
						}
					}
				}

				//ProcessedAccessReqs.Add(currentReq);
				accessReqToProcess.Remove(currentReq);
				accessReqToProcess.AddRange(PowerLevel.GetNewPowerLevel(currentReq, locations));
			}
		}
		private void AddGp(List<AccessReqs> accessReqToProcess)
		{
			var gpAccessList = AccessToGp.Keys.ToList();
			var gpAccessReqToProcess = accessReqToProcess.Intersect(gpAccessList).ToList();

			if (!gpAccessReqToProcess.Any()) return;

			foreach (var access in gpAccessReqToProcess)
			{
				GpCount += AccessToGp[access];
				accessReqToProcess.Remove(access);
			}
		}
		public void WriteChests(FFMQRom rom)
		{
			foreach (var item in ItemsLocations)
			{
				if (item.Type == GameObjectType.Chest || item.Type == GameObjectType.Box)
				{
					rom[TreasuresOffset + item.ObjectId] = (byte)item.Content;
				}
			}
		}
	}

	public class ItemsList
	{ 
		public List<Items> LowProgression { get; set; }
		public List<Items> NonProgression { get; set; }
		public List<Items> HighProgression { get; set; }
		public List<Items> Starting { get; set; }
		public Items ForcedItem { get; set; }
		public bool NoMoreProgression { get => !LowProgression.Any() && !HighProgression.Any(); }
		public int Count { get => LowProgression.Count + NonProgression.Count + HighProgression.Count; }
		public bool MirrorIsPlaced { get => !NonProgression.Contains(Items.MagicMirror); }
        public bool MaskIsPlaced { get => !NonProgression.Contains(Items.Mask); }
        private int lastCoinCount;
		private List<Items> coinsProgression = new() { Items.SandCoin, Items.RiverCoin, Items.SunCoin };
		private int progressionThreshold = 2;
		public Items NextItem(int availablelocations, MT19337 rng)
		{
			if (ForcedItem != Items.None)
			{
				Items item = ForcedItem;
				LowProgression.Remove(item);
				NonProgression.Remove(item);
				HighProgression.Remove(item);
				
				ForcedItem = Items.None;

				return item;
			}
			else if (lastCoinCount > 10 && coinsProgression.Intersect(HighProgression).Any())
			{
				Items item = coinsProgression.Intersect(HighProgression).First();
				HighProgression.Remove(item);
				lastCoinCount = 0;

				return item;
			}
			else if (availablelocations > progressionThreshold || !HighProgression.Concat(LowProgression).Any())
			{
				Items item = rng.PickFrom(NonProgression.Concat(LowProgression).Concat(HighProgression).ToList());

				if (coinsProgression.Contains(item))
				{
					lastCoinCount = 0;
				}
				else
				{
					lastCoinCount++;
				}

				LowProgression.Remove(item);
				NonProgression.Remove(item);
				HighProgression.Remove(item);

				return item;
			}
			else if (availablelocations > 1)
			{
				Items item = rng.PickFrom(LowProgression.Concat(HighProgression).ToList());

				if (coinsProgression.Contains(item))
				{
					lastCoinCount = 0;
				}
				else
				{
					lastCoinCount++;
				}

				LowProgression.Remove(item);
				HighProgression.Remove(item);

				return item;
			}
			else if (availablelocations == 1)
			{
				Items item = HighProgression.Concat(LowProgression).ToList().First();

				if (coinsProgression.Contains(item))
				{
					lastCoinCount = 0;
				}
				else
				{
					lastCoinCount++;
				}

				LowProgression.Remove(item);
				HighProgression.Remove(item);

				return item;
			}
			else
			{
				throw new Exception("Logic error: Not supposed to be there");
			}
		}
		public ItemsList(Flags flags, MT19337 rng)
		{

			lastCoinCount = 0;
			ForcedItem = Items.None;
			progressionThreshold = (flags.LogicOptions == LogicOptions.Friendly || flags.ProgressiveGear) ? 3 : 2;

			List<Items> StartingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
			List<Items> StartingItems = new() { Items.SteelArmor };

			List<Items> ProgressionBombs = new() { Items.Bomb, Items.JumboBomb };
			List<Items> ProgressionSwords = new() { Items.SteelSword, Items.KnightSword, Items.Excalibur };
			List<Items> ProgressionAxes = new() { Items.Axe, Items.BattleAxe, Items.GiantsAxe };
			List<Items> ProgressionClaws = new() { Items.CatClaw, Items.CharmClaw };
			List<Items> ProgressionCoins = new() { Items.SandCoin, Items.RiverCoin };
			List<Items> ProgressionSunCoin = new() { Items.SunCoin };
			List<Items> ProgressionSkyCoin = new() { Items.SkyCoin };

			List<Items> ProgressionItems = new()
			{
				Items.TreeWither, // NPC
				Items.VenusKey, // NPC
				Items.MultiKey, // NPC
				Items.ThunderRock, // NPC
				Items.CaptainsCap, // NPC
				Items.DragonClaw, // NPC
				Items.MegaGrenade, //NPC
				Items.Elixir, // NPC
				Items.Wakewater, // NPC
				Items.LibraCrest,
				Items.GeminiCrest,
				Items.MobiusCrest
			};
			List<Items> NonProgressionItems = new()
			{
				Items.Mask,
				Items.MagicMirror,
			};
			List<Items> Gear = new()
			{
				Items.ExitBook, //Battlefield
				Items.CureBook,
				Items.HealBook,
				Items.LifeBook,
				Items.QuakeBook,
				Items.BlizzardBook,
				Items.FireBook,
				Items.AeroBook,
				Items.ThunderSeal, //Battlefield
				Items.WhiteSeal,
				Items.MeteorSeal,
				Items.FlareSeal,
				Items.SteelHelm, //NPC
				Items.MoonHelm,
				Items.ApolloHelm,
				Items.SteelArmor,
				Items.NobleArmor,
				Items.GaiasArmor,
				Items.SteelShield,
				Items.VenusShield, //Chest-NPC
				Items.AegisShield,
				Items.Charm, //Battlefield
				Items.MagicRing, //Battlefield
				Items.CupidLocket // NPC
			};

			Items startingWeapon = Items.SteelSword;

			if (flags.RandomStartingWeapon)
			{
				startingWeapon = rng.PickFrom(StartingWeapons);
			}
			
			StartingItems.Add(startingWeapon);

			// SkyCoin
			if (flags.SkyCoinMode == SkyCoinModes.StartWith || flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				StartingItems.Add(Items.SkyCoin);
				ProgressionSkyCoin.Remove(Items.SkyCoin);
			}
			else if (flags.SkyCoinMode != SkyCoinModes.Standard)
			{
				ProgressionSkyCoin.Remove(Items.SkyCoin);
			}

			// Remove Starting Items
			ProgressionBombs.RemoveAll(x => StartingItems.Contains(x));
			ProgressionSwords.RemoveAll(x => StartingItems.Contains(x));
			ProgressionAxes.RemoveAll(x => StartingItems.Contains(x));
			ProgressionClaws.RemoveAll(x => StartingItems.Contains(x));
			ProgressionItems.RemoveAll(x => StartingItems.Contains(x));
			ProgressionCoins.RemoveAll(x => StartingItems.Contains(x));
			ProgressionSunCoin.RemoveAll(x => StartingItems.Contains(x));
			ProgressionSkyCoin.RemoveAll(x => StartingItems.Contains(x));
			NonProgressionItems.RemoveAll(x => StartingItems.Contains(x));
			Gear.RemoveAll(x => StartingItems.Contains(x));

			// Collapse Weapons
			if (flags.ProgressiveGear)
			{
				ProgressionItems.AddRange(ProgressionBombs);
				ProgressionItems.AddRange(ProgressionClaws);

				if (startingWeapon != Items.SteelSword)
				{
					ProgressionItems.Add(rng.TakeFrom(ProgressionSwords));
				}

				if (startingWeapon != Items.Axe)
				{
					ProgressionItems.Add(rng.TakeFrom(ProgressionAxes));
				}

				Gear.AddRange(ProgressionSwords);
				Gear.AddRange(ProgressionAxes);
			}
			else
			{
				if (startingWeapon != Items.SteelSword)
				{
					ProgressionItems.Add(rng.TakeFrom(ProgressionSwords));
				}

				if (startingWeapon != Items.Axe)
				{
					ProgressionItems.Add(rng.TakeFrom(ProgressionAxes));
				}

				if (startingWeapon != Items.CatClaw)
				{
					ProgressionItems.Add(rng.TakeFrom(ProgressionClaws));
				}

				if (startingWeapon != Items.Bomb)
				{
					if (!flags.OverworldShuffle && flags.MapShuffling == MapShufflingMode.None)
					{
						// On standard map, raise odds to open up bone dungeon first
						ProgressionCoins.Add(rng.TakeFrom(ProgressionBombs));
					}
					else
					{
						ProgressionItems.Add(rng.TakeFrom(ProgressionBombs));
					}
					
				}

				Gear.AddRange(ProgressionSwords);
				Gear.AddRange(ProgressionAxes);
				Gear.AddRange(ProgressionClaws);
				Gear.AddRange(ProgressionBombs);
			}

			// Shuffle Coins
			ProgressionCoins.Shuffle(rng);

			ProgressionItems.Shuffle(rng);
			LowProgression = ProgressionItems;
			NonProgression = NonProgressionItems.Concat(Gear).Concat(ProgressionSkyCoin).ToList();
			HighProgression = ProgressionCoins.Concat(ProgressionSunCoin).ToList();
			Starting = StartingItems;
		}
	}
}
