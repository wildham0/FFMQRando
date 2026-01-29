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
