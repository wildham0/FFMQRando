using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using static System.Math;
using System.ComponentModel;
using System.Transactions;

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

	public enum SeedQuantities : int
	{
		[Description("0")]
		Zero,
		[Description("1")]
		One,
		[Description("2")]
		Two,
		[Description("5")]
		Five,
		[Description("10")]
		Ten,
		[Description("25")]
		TwentyFive,
		[Description("Random 2-10")]
		Random210,
		[Description("Random 0-25")]
		Random025,
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
		public List<GameObject> ItemsLocations { get; set; }

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

			Dictionary<int, Items> originalItems = rom.GetFromBank(0x01, 0x801E, 0xDD)
				.ToBytes()
				.Select((x, i) => (i, (Items)x))
				.ToDictionary(x => x.i, x => x.Item2);

			Dictionary<int, Items> heroChestItems = rom.GetFromBank(0x01, 0x80F2, 0x04)
				.ToBytes()
				.Select((x, i) => (i + 0xF2, (Items)x))
				.ToDictionary(x => x.Item1, x => x.Item2);

			originalItems.Remove(0xF2);
			originalItems.Remove(0xF3);
			originalItems.Remove(0xF4);
			originalItems.Remove(0xF5);
			//originalItems = originalItems.Where()

			//List<Items> consumableList = rom.GetFromBank(0x01, 0x801E, 0xDD).ToBytes().Select(x => (Items)x).ToList();
			//List<Items> finalConsumables = rom.GetFromBank(0x01, 0x80F2, 0x04).ToBytes().Select(x => (Items)x).ToList();


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
						targetLocation.Reset = false;
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
						targetLocation.Reset = false;
					}
				}
			}

			// Place Seeds
			Dictionary<SeedQuantities, int> seedQuantites = new()
			{
				{ SeedQuantities.Zero, 0 },
				{ SeedQuantities.One, 1 },
				{ SeedQuantities.Two, 2 },
				{ SeedQuantities.Five, 5 },
				{ SeedQuantities.Ten, 10 },
				{ SeedQuantities.TwentyFive, 25 },
				{ SeedQuantities.Random210, rng.Between(2,10) },
				{ SeedQuantities.Random025, rng.Between(0,25) },
			};

			int seedCount = seedQuantites[flags.SeedQuantity];
			List<int> originalSeedLocations = new() { 0x3D, 0xA4 };
			List<Items> otherConsumables = new() { Items.CurePotion, Items.HealPotion, Items.Refresher };

			foreach (var seedLocation in originalSeedLocations)
			{
				originalItems[seedLocation] = rng.PickFrom(otherConsumables);
			}

			// Plando seeds
			if (!flags.ShuffleBoxesContent)
			{
				var seedboxes = ItemsLocations.Where(x => !x.IsPlaced && (x.Type == GameObjectType.Box || x.Type == GameObjectType.Chest) && originalSeedLocations.Contains(x.ObjectId)).ToList();

				while(seedboxes.Any() && seedCount > 0)
				{
					var box = rng.TakeFrom(seedboxes);
					box.Content = Items.Seed;
					box.IsPlaced = true;
					box.Reset = !flags.BoxesDontReset;
					box.Type = GameObjectType.Box;
					seedCount--;
				}
			}

			// Place remaining seeds, they can go anywhere
			while (seedCount > 0)
			{
				var potentialSeedLocations = ItemsLocations.Where(x => !x.IsPlaced && validTypes.Contains(x.Type)).ToList();
				var seedLocation = rng.PickFrom(potentialSeedLocations);

				seedLocation.Content = Items.Seed;
				seedLocation.IsPlaced = true;
				if (seedLocation.Type == GameObjectType.Chest || seedLocation.Type == GameObjectType.Box)
				{
					seedLocation.Type = GameObjectType.Box;
					seedLocation.Reset = !flags.BoxesDontReset; // gotta think about this
				}
				seedCount--;
			}

			// Plando other consumable
			if (!flags.ShuffleBoxesContent)
			{
				var boxLocations = ItemsLocations.Where(x => !x.IsPlaced && x.Type == GameObjectType.Box).ToList();

				foreach (var box in boxLocations)
				{
					box.Content = originalItems[box.ObjectId];
					box.IsPlaced = true;
					box.Reset = !flags.BoxesDontReset;
					box.Type = GameObjectType.Box; // redundant
					originalItems.Remove(box.ObjectId);
				}
			}

			// Fill the rest
			var unfilledLocations = ItemsLocations.Where(x => !x.IsPlaced && validTypes.Contains(x.Type)).ToList();
			var remainingItems = originalItems.Select(i => i.Value).ToList();

			foreach (var location in unfilledLocations)
			{
				location.Content = rng.TakeFrom(remainingItems);
				location.IsPlaced = true;
				if (location.Type == GameObjectType.Chest || location.Type == GameObjectType.Box)
				{
					location.Type = GameObjectType.Box;
					location.Reset = !flags.BoxesDontReset;
				}
			}

			// Add the final chests so we can update their properties
			List<GameObject> finalChests = ItemsLocations.Where(x => x.Type == GameObjectType.HeroChest).ToList();
			foreach (var chest in finalChests)
			{
				chest.Content = heroChestItems[chest.ObjectId];
				chest.IsPlaced = true;
				chest.Reset = false;
			}

			/*
			for(int i = 0; i < finalChests.Count; i++)
			{
				finalChests[i].Content = heroChestItems[i.];
				finalChests[i].IsPlaced = true;
			}*/
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
}
