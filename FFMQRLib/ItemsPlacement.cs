using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum ItemShuffle : int
	{
		[Description("Quest Items Only")]
		QuestItemsOnly = 0,
		[Description("All Items")]
		AllItems,
	}
	public class ItemsPlacement
	{
		public List<Items> StartingItems { get; set; }
		public List<TreasureObject> ItemsLocations { get; }

		public ItemsPlacement(FFMQRom rom, Flags flags, MT19337 rng)
		{
			
			bool badPlacement = true;
			int counter = 0;
			int placedChests = 0;

			List<Items> consumableList = rom.GetFromBank(0x01, 0x801D, 0xDE).ToBytes().Select(x => (Items)x).ToList();

			while (badPlacement)
			{
				badPlacement = false;
				placedChests = 0;
				List<Items> itemsList = RandomizeItemsOrder(flags, rng);

				if (flags.ItemShuffle == ItemShuffle.AllItems)
				{
					ItemsLocations = new(ItemLocations.AllEverything(flags, rom.Battlefields).ToList());
				}
				else
				{
					ItemsLocations = new(ItemLocations.AllChestsNPCsBattlefields(flags, rom.Battlefields).ToList());
				}

				List<Items> placedItems = new();

				foreach (var item in StartingItems)
				{
					List<AccessReqs> result;
					if (ItemLocations.ItemAccessReq.TryGetValue(item, out result))
					{
						for (int i = 0; i < ItemsLocations.Count; i++)
						{
							ItemsLocations[i].AccessRequirements = ItemsLocations[i].AccessRequirements.Where(x => !result.Contains(x)).ToList();
						}
					}
				}

				while (itemsList.Any())
				{
					var validLocations = ItemsLocations.Where(x => !x.AccessRequirements.Any() && x.IsPlaced == false && (placedChests >= 0x1C ? (x.Type != TreasureType.Chest && x.Type != TreasureType.Box) : true)).ToList();

					if (!validLocations.Any() && itemsList.Any())
					{
						Console.WriteLine("Attempt " + counter + " - Invalid Placement ");
						counter++;
						badPlacement = true;
						break;
					}

					TreasureObject targetLocation = rng.PickFrom(validLocations);
					var itemToPlace = itemsList.First();

					itemsList.RemoveAt(0);
					targetLocation.Content = itemToPlace;
					targetLocation.IsPlaced = true;
					if(targetLocation.Type == TreasureType.Chest || targetLocation.Type == TreasureType.Box)
					{ 
						targetLocation.Type = TreasureType.Chest;
						placedChests++;
					}
					placedItems.Add(itemToPlace);
					//Console.WriteLine(Enum.GetName(targetLocation.Location) + " - " + Enum.GetName(itemToPlace));

					List<AccessReqs> result;
					if (ItemLocations.ItemAccessReq.TryGetValue(itemToPlace, out result))
					{
						for (int i = 0; i < ItemsLocations.Count; i++)
						{
							ItemsLocations[i].AccessRequirements = ItemsLocations[i].AccessRequirements.Where(x => !result.Contains(x)).ToList();
						}
					}
				}

				var unfiledValidLocations = ItemsLocations.Where(x => !x.AccessRequirements.Any() && x.Content == Items.None).ToList();
				//Console.WriteLine("**** Unfiled Locations ****");
				foreach (var loc in unfiledValidLocations)
				{
					//Console.WriteLine(Enum.GetName(loc.Location) + " - " + loc.ObjectId);
				}
			}

			if (flags.ItemShuffle == ItemShuffle.AllItems)
			{
				for (int i = 0; i < ItemsLocations.Count; i++)
				{
					if (ItemsLocations[i].IsPlaced == false)
					{
						ItemsLocations[i].Content = rng.TakeFrom(consumableList);
						ItemsLocations[i].IsPlaced = true;
						if (ItemsLocations[i].Type == TreasureType.Chest || ItemsLocations[i].Type == TreasureType.Box)
						{
							ItemsLocations[i].Type = TreasureType.Box;
						}
					}
				}
			}
		}
		public void WriteChests(FFMQRom rom)
		{
			foreach (var item in ItemsLocations)
			{
				if (item.Type == TreasureType.Chest || item.Type == TreasureType.Box)
				{
					rom[RomOffsets.TreasuresOffset + item.ObjectId] = (byte)item.Content;
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
			List<Items> ProgressionCoins = new() { Items.SunCoin };
			List<Items> ProgressionItems = new()
			{
				Items.TreeWither, // NPC
				Items.VenusKey, // NPC
				Items.MultiKey, // NPC
				Items.ThunderRock, // NPC
				Items.CaptainCap, // NPC
				Items.MobiusCrest,
				Items.SkyCoin,
				Items.DragonClaw, // NPC
				Items.MegaGrenade, //NPC
				Items.Elixir, // NPC
				Items.WakeWater, // NPC
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
			if (flags.ItemShuffle == ItemShuffle.QuestItemsOnly)
			{
				ProgressionItems.AddRange(InitialProgressionItems);
				ProgressionItems.AddRange(ProgressionCoins); 
			}
			//ProgressionItems.AddRange(InitialProgressionItems);
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
			if (flags.ItemShuffle == ItemShuffle.QuestItemsOnly)
			{
				Tier1.AddRange(ProgressionItems.GetRange(0, 5));
				ProgressionItems.RemoveRange(0, 5);
			}
			else
			{
				Tier1.AddRange(ProgressionItems.GetRange(0, 3));
				ProgressionItems.RemoveRange(0, 3);
				Tier1.AddRange(InitialProgressionItems);
				Tier1.AddRange(ProgressionCoins);
			}
			
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
	}

	public partial class FFMQRom : SnesRom
	{




	}
}
