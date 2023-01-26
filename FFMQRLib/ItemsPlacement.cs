using System;
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
	public class ItemsPlacement
	{
		public List<Items> StartingItems { get; set; }
		public List<TreasureObject> ItemsLocations { get; }
		
		private const int TreasuresOffset = 0x8000;
		private List<(AccessReqs, List<AccessReqs>)> Events;
		private List<(LocationIds, List<AccessReqs>)> LocationTriggers;

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
		public ItemsPlacement(Flags flags, Battlefields battlefields, Overworld overworld, LocationStructure locations, FFMQRom rom, MT19337 rng)
		{
			bool badPlacement = true;
			int counter = 0;
			int placedChests = 0;
			int prioritizedLocationsCount = 0;
			int prioritizedItemsCount = 0;
			int looseItemsCount = 0;

			List<Items> consumableList = rom.GetFromBank(0x01, 0x801E, 0xDD).ToBytes().Select(x => (Items)x).ToList();
			List<Items> finalConsumables = rom.GetFromBank(0x01, 0x80F2, 0x04).ToBytes().Select(x => (Items)x).ToList();


			List<RegionWeight> regionsWeight = new() { new RegionWeight(MapRegions.Foresta, 1), new RegionWeight(MapRegions.Aquaria, 1), new RegionWeight(MapRegions.Fireburg, 1), new RegionWeight(MapRegions.Windia, 1) };

			List<TreasureObject> initialItemlocations = new(ItemLocations.Generate(flags, battlefields, locations, overworld).ToList());

			while (badPlacement)
			{
				badPlacement = false;
				placedChests = 0;

				bool placedMirror = false;
				bool placedMask = false;

				regionsWeight.Find(x => x.Region == MapRegions.Foresta).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Aquaria).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Fireburg).Weight = 1;
				regionsWeight.Find(x => x.Region == MapRegions.Windia).Weight = 1;

				List<Items> itemsList = RandomizeItemsOrder(flags, rng);

				ItemsLocations = new(initialItemlocations.Select(x => new TreasureObject(x)));

				prioritizedLocationsCount = ItemsLocations.Where(x => x.Prioritize == true).Count();

				looseItemsCount = Max(0, itemsList.Count() - prioritizedLocationsCount);
				prioritizedItemsCount = Min(prioritizedLocationsCount, itemsList.Count());

				List<Items> placedItems = new();

				List<Items> nonRequiredItems = flags.SkyCoinMode == SkyCoinModes.Standard ? StartingItems : StartingItems.Append(Items.SkyCoin).ToList();

				Events = ItemLocations.AccessEvents.Select(x => (x.Key, x.Value)).ToList();
				LocationTriggers = ItemLocations.LocationTriggers.ToList();


				List<AccessReqs> accessReqsToProcess = new();
				// Apply starting items access
				foreach (var item in nonRequiredItems)
				{
					List<AccessReqs> result;
					if(ItemLocations.ItemAccessReq.TryGetValue(item, out result))
					{
						accessReqsToProcess.AddRange(result);
					}
				}

				ProcessRequirements(accessReqsToProcess);

				while (itemsList.Any())
				{
					List<TreasureObject> validLocations = new();

					List<TreasureObject> validLocationsPriorized = ItemsLocations.Where(x => x.Accessible && x.IsPlaced == false && x.Prioritize == true).ToList();
					List<TreasureObject> validLocationsLoose = ItemsLocations.Where(x => x.Accessible && x.IsPlaced == false && x.Prioritize == false && x.Exclude == false).ToList();

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

					List<TreasureObject> validLocationsFavored = validLocations.Where(x => x.Region == favoredRegion).ToList();

					if (validLocationsFavored.Any())
					{
						validLocations = validLocationsFavored;
					}

					if (!validLocations.Any() && itemsList.Any())
					{
						Console.WriteLine("Attempt " + counter + " - Invalid Placement ");
						counter++;
						badPlacement = true;
						if (counter >= 100)
						{
							throw new TimeoutException("Logic failure; try another seed.");
						}
						break;
					}
					
					var itemToPlace = itemsList.First();
					var itemIndex = 0;

					if (flags.LogicOptions == LogicOptions.Friendly)
					{
						if ((!placedMirror && validLocations.Where(x => x.Location == LocationIds.IcePyramid).Any()) || (itemToPlace == Items.MagicMirror))
						{
							validLocations = validLocations.Where(x => x.Location != LocationIds.IcePyramid).ToList();

							if (!validLocations.Any())
							{
								Console.WriteLine("Attempt " + counter + " - Invalid Placement ");
								counter++;
								badPlacement = true;
								break;
							}

							itemIndex = itemsList.FindIndex(x => x == Items.MagicMirror);
							itemToPlace = Items.MagicMirror;
							placedMirror = true;
						}
						else if ((!placedMask && validLocations.Where(x => x.Location == LocationIds.Volcano).Any()) || (itemToPlace == Items.Mask))
						{
							validLocations = validLocations.Where(x => x.Location != LocationIds.Volcano).ToList();

							if (!validLocations.Any())
							{
								Console.WriteLine("Attempt " + counter + " - Invalid Placement ");
								counter++;
								badPlacement = true;
								break;
							}

							itemIndex = itemsList.FindIndex(x => x == Items.Mask);
							itemToPlace = Items.Mask;
							placedMask = true;
						}
					}

					itemsList.RemoveAt(itemIndex);

					TreasureObject targetLocation = rng.PickFrom(validLocations);
					targetLocation.Content = itemToPlace;
					targetLocation.IsPlaced = true;
					regionsWeight.Find(x => x.Region == targetLocation.Region).Weight++;

					if (targetLocation.Type == TreasureType.Chest || targetLocation.Type == TreasureType.Box)
					{ 
						targetLocation.Type = TreasureType.Chest;
						placedChests++;
					}
					placedItems.Add(itemToPlace);
					//Console.WriteLine(Enum.GetName(targetLocation.Location) + "_" + targetLocation.ObjectId + " - " + Enum.GetName(itemToPlace));

					List<AccessReqs> result;

					if (ItemLocations.ItemAccessReq.TryGetValue(itemToPlace, out result))
					{
						ProcessRequirements(result);
					}
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
				var validSkyCoinLocations = ItemsLocations.Where(x => x.IsPlaced == false && x.Prioritize == false && x.Exclude == false && x.Location != LocationIds.DoomCastle).ToList();

				if(validSkyCoinLocations.Count < 40)
                {
					throw new Exception("Sky Coin Pieces error: not enough valid locations");
                }

				for (int i = 0; i < 40; i++)
				{
					TreasureObject targetLocation = rng.TakeFrom(validSkyCoinLocations);
					targetLocation.Content = Items.SkyCoin;
					targetLocation.IsPlaced = true;

					if (targetLocation.Type == TreasureType.Chest || targetLocation.Type == TreasureType.Box)
					{
						targetLocation.Type = TreasureType.Chest;
					}
				}
			}

			// Fill excluded and unfilled locations
			List<Items> consumables = new() { Items.Potion, Items.HealPotion, Items.Refresher, Items.Seed };
			
			var unfilledLocations = ItemsLocations.Where(x => x.IsPlaced == false && (x.Type == TreasureType.NPC || x.Type == TreasureType.Battlefield || (x.Type == TreasureType.Chest && x.ObjectId < 0x20) || x.Type == TreasureType.Dummy)).ToList();

			foreach (var location in unfilledLocations)
			{
				location.Content = rng.PickFrom(consumables);
				location.IsPlaced = true;
				if (location.Type == TreasureType.Chest || location.Type == TreasureType.Box)
				{
					location.Type = TreasureType.Box;
				}
			}

			// Place consumables
			for (int i = 0; i < ItemsLocations.Count; i++)
			{
				if (ItemsLocations[i].IsPlaced == false)
				{

					if (flags.ShuffleBoxesContent)
					{
						ItemsLocations[i].Content = rng.TakeFrom(consumableList);
					}
					else
					{
						ItemsLocations[i].Content = consumableList[ItemsLocations[i].ObjectId - 0x1E];
					}

					ItemsLocations[i].IsPlaced = true;
					if (ItemsLocations[i].Type == TreasureType.Chest || ItemsLocations[i].Type == TreasureType.Box)
					{
						ItemsLocations[i].Type = TreasureType.Box;
					}
				}
			}

			// Add the final chests so we can update their properties
			List<TreasureObject> finalChests = new(ItemLocations.FinalChests());

			for(int i = 0; i < finalChests.Count; i++)
			{
				finalChests[i].Content = finalConsumables[i];
				finalChests[i].IsPlaced = true;
			}

			ItemsLocations.AddRange(finalChests);
		}

		private void ProcessRequirements(List<AccessReqs> accessReqToProcess)
		{
			while (accessReqToProcess.Any())
			{
				var currentReq = accessReqToProcess.First();
				List<LocationIds> newLocations = new();

				// Update Locations
				List<TreasureObject> unaccessibleLocations = ItemsLocations.Where(x => x.Accessible == false).ToList();
				for (int i = 0; i < unaccessibleLocations.Count; i++)
				{
					for (int j = 0; j < unaccessibleLocations[i].AccessRequirements.Count; j++)
					{
						unaccessibleLocations[i].AccessRequirements[j] = unaccessibleLocations[i].AccessRequirements[j].Where(x => x != currentReq).ToList();
						if (!unaccessibleLocations[i].AccessRequirements[j].Any())
						{
							unaccessibleLocations[i].Accessible = true;
							newLocations.Add(unaccessibleLocations[i].Location);
						}
					}
				}

				// Update Events
				List<int> eventToRemove = new();
				for (int i = 0; i < Events.Count; i++)
				{
					Events[i] = (Events[i].Item1, Events[i].Item2.Where(x => x != currentReq).ToList());
					if (!Events[i].Item2.Any())
					{
						accessReqToProcess.Add(Events[i].Item1);
						eventToRemove.Add(i);
					}
				}
				Events = Events.Where((x, i) => !eventToRemove.Contains(i)).ToList();

				// Update LocationsTriggers
				List<int> triggerToRemove = new();
				for (int i = 0; i < LocationTriggers.Count; i++)
				{
					if(newLocations.Contains(LocationTriggers[i].Item1))
                    {
						accessReqToProcess.AddRange(LocationTriggers[i].Item2);
						triggerToRemove.Add(i);
					}
				}
				LocationTriggers = LocationTriggers.Where((x, i) => !triggerToRemove.Contains(i)).ToList();

				accessReqToProcess.Remove(currentReq);
			}
		}
		public void WriteChests(FFMQRom rom)
		{
			foreach (var item in ItemsLocations)
			{
				if (item.Type == TreasureType.Chest || item.Type == TreasureType.Box)
				{
					rom[TreasuresOffset + item.ObjectId] = (byte)item.Content;
				}
			}
		}
		private List<Items> RandomizeItemsOrder(Flags flags, MT19337 rng)
		{
			List<Items> FinalItems = new();
			List<Items> StartingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
			StartingItems = new() { Items.SteelArmor };
			if (flags.RandomStartingWeapon)
			{
				StartingItems.Add(rng.PickFrom(StartingWeapons));
			}
			else
			{
				StartingItems.Add(Items.SteelSword);
			}
			List<Items> ProgressionBombs = new() { Items.Bomb, Items.JumboBomb };
			List<Items> ProgressionSwords = new() { Items.SteelSword, Items.KnightSword, Items.Excalibur };
			List<Items> ProgressionAxes = new() { Items.Axe, Items.BattleAxe, Items.GiantsAxe };
			List<Items> ProgressionClaws = new() { Items.CatClaw, Items.CharmClaw };
			List<Items> InitialProgressionItems = new() { Items.SandCoin, Items.RiverCoin };
			List<Items> ProgressionItems = new()
			{
				Items.TreeWither, // NPC
				Items.VenusKey, // NPC
				Items.MultiKey, // NPC
				Items.ThunderRock, // NPC
				Items.CaptainCap, // NPC
				Items.MobiusCrest,
				Items.DragonClaw, // NPC
				Items.MegaGrenade, //NPC
				Items.Elixir, // NPC
				Items.WakeWater, // NPC
				Items.SunCoin
			};
			List<Items> NonProgressionItems = new()
			{
				Items.Mask,
				Items.MagicMirror,
				Items.LibraCrest,
				Items.GeminiCrest, //Battlefield
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
				Items.CupidLock // NPC
			};


			// SkyCoin
			if (flags.SkyCoinMode == SkyCoinModes.Standard)
			{
				ProgressionItems.Add(Items.SkyCoin);
			}
			else if(flags.SkyCoinMode == SkyCoinModes.StartWith)
			{
				StartingItems.Add(Items.SkyCoin);
			}


			// Remove Starting Items
			ProgressionBombs.RemoveAll(x => StartingItems.Contains(x));
			ProgressionSwords.RemoveAll(x => StartingItems.Contains(x));
			ProgressionAxes.RemoveAll(x => StartingItems.Contains(x));
			ProgressionClaws.RemoveAll(x => StartingItems.Contains(x));
			InitialProgressionItems.RemoveAll(x => StartingItems.Contains(x));
			ProgressionItems.RemoveAll(x => StartingItems.Contains(x));
			NonProgressionItems.RemoveAll(x => StartingItems.Contains(x));
			Gear.RemoveAll(x => StartingItems.Contains(x));

			// Select Inital Progression
			if (ProgressionBombs.Count > 1)
			{
				InitialProgressionItems.Add(rng.TakeFrom(ProgressionBombs));
			}			
			FinalItems.Add(rng.TakeFrom(InitialProgressionItems));

			// Build Progression Items List
			ProgressionItems.Add(rng.TakeFrom(ProgressionSwords));
			ProgressionItems.Add(rng.TakeFrom(ProgressionBombs));
			ProgressionItems.Add(rng.TakeFrom(ProgressionAxes));
			ProgressionItems.Add(rng.TakeFrom(ProgressionClaws));
			ProgressionItems.AddRange(InitialProgressionItems);
			ProgressionItems.Shuffle(rng);

			// Put Everything else in Non Progression Items
			NonProgressionItems.AddRange(ProgressionBombs);
			NonProgressionItems.AddRange(ProgressionSwords);
			NonProgressionItems.AddRange(ProgressionAxes);
			NonProgressionItems.AddRange(ProgressionClaws);

			// Fill 3 tiers
			// Tier3 is 5 random piece of gear
			Gear.Shuffle(rng);
			List<Items> Tier3 = Gear.GetRange(0, 5);
			Gear.RemoveRange(0, 5);

			// Tier1 is 5 random progression item, 5 random non progression+gear
			NonProgressionItems.AddRange(Gear);
			NonProgressionItems.Shuffle(rng);

			List<Items> Tier1 = NonProgressionItems.GetRange(0, 5);
			NonProgressionItems.RemoveRange(0, 5);
			Tier1.AddRange(ProgressionItems.GetRange(0, 5));
			ProgressionItems.RemoveRange(0, 5);
			
			// Tier2 is everything else
			List<Items> Tier2 = ProgressionItems.ToList();
			Tier2.AddRange(NonProgressionItems);

			// Shuffle tiers
			Tier1.Shuffle(rng);
			Tier2.Shuffle(rng);
			Tier3.Shuffle(rng);

			FinalItems = FinalItems.Concat(Tier1).Concat(Tier2).Concat(Tier3).ToList();

			return FinalItems;
		}
		public string GenerateSpoilers(FFMQRom rom, string version, string hash, string flags, string seed)
		{
			List<Items> invalidItems = new() { Items.Potion, Items.HealPotion, Items.Refresher, Items.Seed, Items.BombRefill, Items.ProjectileRefill };
			
			string spoilers = "";

			spoilers += "--- Spoilers File --- \n";
			spoilers += "FFMQR " + version + "\n";
			spoilers += "Flags: " + flags + "\n";
			spoilers += "Seed: " + seed + "\n";
			spoilers += "Hash: " + hash + "\n";

			spoilers += "\n";
			spoilers += "--- Starting Items ---\n";

			foreach (var item in StartingItems)
			{
				spoilers += "  " + item + "\n";
			}

			spoilers += "\n--- Placed Items ---\n";
			var keyItems = ItemsLocations.Where(x => !invalidItems.Contains(x.Content)).ToList();
			var forestaKi = keyItems.Where(x => ItemLocations.ReturnRegion(x.Location) == MapRegions.Foresta).OrderBy(x => x.Location).ToList();
			var aquariaKi = keyItems.Where(x => ItemLocations.ReturnRegion(x.Location) == MapRegions.Aquaria).OrderBy(x => x.Location).ToList();
			var fireburgKi = keyItems.Where(x => ItemLocations.ReturnRegion(x.Location) == MapRegions.Fireburg).OrderBy(x => x.Location).ToList();
			var windiaKi = keyItems.Where(x => ItemLocations.ReturnRegion(x.Location) == MapRegions.Windia).OrderBy(x => x.Location).ToList();

			spoilers += "Foresta\n";
			foreach (var item in forestaKi)
			{
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + item.Content + "\n";
			}

			spoilers += "\nAquaria\n";
			foreach (var item in aquariaKi)
			{
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + item.Content + "\n";
			}

			spoilers += "\nFireburg\n";
			foreach (var item in fireburgKi)
			{
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + item.Content + "\n";
			}

			spoilers += "\nWindia\n";
			foreach (var item in windiaKi)
			{
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + item.Content + "\n";
			}

			return spoilers;
		}
	}

	public partial class FFMQRom : SnesRom
	{




	}
}
