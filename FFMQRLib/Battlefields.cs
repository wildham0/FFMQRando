using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum BattlesQty : int
	{
		[Description("10")]
		Ten = 0,
		[Description("7")]
		Seven,
		[Description("5")]
		Five,
		[Description("3")]
		Three,
		[Description("1")]
		One,
		[Description("Random 1-10")]
		RandomHigh,
		[Description("Random 1-5")]
		RandomLow,

	}
	public class Battlefield
	{ 
		public LocationIds Location { get; set; }
		public BattlefieldRewardType RewardType { get => GetRewardType(); }
		public ushort Value { get; set; }
		public Items Reward { get; set; }

		public static Dictionary<Items, ushort> XpBattlefieldRewardValues = new()
		{
			{ Items.Xp54, 0x0012 },
			{ Items.Xp99, 0x0021 },
			{ Items.Xp540, 0x00B4 },
			{ Items.Xp744, 0x00F8 },
			{ Items.Xp816, 0x0110 },
			{ Items.Xp1068, 0x0164 },
			{ Items.Xp1200, 0x0190 },
			{ Items.Xp2700, 0x0384 },
			{ Items.Xp2808, 0x03A8 },
		};
		public static Dictionary<Items, ushort> GpBattlefieldRewardValues = new()
		{
			{ Items.Gp150, 0x0032 },
			{ Items.Gp300, 0x0064 },
			{ Items.Gp600, 0x00C8 },
			{ Items.Gp900, 0x012C },
			{ Items.Gp1200, 0x0190 },
		};

		public Battlefield(LocationIds location, byte[] rawvalues)
		{
			Location = location;
			byte rewardtype = (byte)(rawvalues[1] & 0xC0);

			if (rewardtype == (byte)BattlefieldRewardType.Item)
			{
				Value = rawvalues[0];
				Reward = (Items)Value;
			}
			else if (rewardtype == (byte)BattlefieldRewardType.Experience)
			{
				Value = (ushort)(rawvalues[0] + (0x100 * (rawvalues[1] & 0x7F)));
				Reward = XpBattlefieldRewardValues.Where(x => x.Value == Value).First().Key;
			}
			else if (rewardtype == (byte)BattlefieldRewardType.Gold)
			{
				Value = (ushort)(rawvalues[0] + (0x100 * (rawvalues[1] & 0x3F)));
				Reward = GpBattlefieldRewardValues.Where(x => x.Value == Value).First().Key;
			}
		}
		public Battlefield(LocationIds location, Items reward)
		{
			Location = location;
			Reward = reward;
		}
		private BattlefieldRewardType GetRewardType()
		{
			if (Reward >= Items.Xp54 && Reward <= Items.Xp2808)
			{
				return BattlefieldRewardType.Experience;
			}
			else if (Reward >= Items.Gp150 && Reward <= Items.Gp1200)
			{
				return BattlefieldRewardType.Gold;
			}
			else
			{
				return BattlefieldRewardType.Item;
			}

		}
		public byte[] GetBytes()
		{
			if (Reward >= Items.Xp54 && Reward <= Items.Xp2808)
			{
				return new byte[] {
					(byte)(XpBattlefieldRewardValues[Reward] & 0x00FF),
					(byte)((byte)BattlefieldRewardType.Experience | ((XpBattlefieldRewardValues[Reward] & 0x7F00) / 0x100))
				};
			}
			else if (Reward >= Items.Gp150 && Reward <= Items.Gp1200)
			{
				return new byte[] {
					(byte)(GpBattlefieldRewardValues[Reward] & 0x00FF),
					(byte)((byte)BattlefieldRewardType.Gold | ((GpBattlefieldRewardValues[Reward] & 0x3F00) / 0x100))
				};
			}
			else
			{
				return new byte[] {
					(byte)Reward,
					(byte)BattlefieldRewardType.Item
				};
			}
		}
	}
	public class Battlefields
	{
		private const int BattlefieldsRewardsBank = 0x07;
		private const int BattlefieldsRewardsOffset = 0xEFA1;

		private const int BattlefieldsQty = 0x14;

		private List<byte> _battlesQty;
		private List<Blob> _rewards;

		private List<Battlefield> battlefields;

		public Battlefields(FFMQRom rom)
		{
			_battlesQty = rom.GetFromBank(0x0C, 0xD4D0, BattlefieldsQty).Chunk(1).Select(x => x[0]).ToList();
			_rewards = rom.GetFromBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, BattlefieldsQty * 2).Chunk(2);
			battlefields = _rewards.Select((x, i) => new Battlefield((LocationIds)(i + 1), x)).ToList();
		}
		public Battlefields()
		{
			battlefields = new()
			{
				new Battlefield(LocationIds.ForestaSouthBattlefield, Items.Xp54),
				new Battlefield(LocationIds.ForestaWestBattlefield, Items.Charm),
				new Battlefield(LocationIds.ForestaEastBattlefield, Items.Gp150),
				new Battlefield(LocationIds.AquariaBattlefield01, Items.Xp99),
				new Battlefield(LocationIds.AquariaBattlefield02, Items.Gp300),
				new Battlefield(LocationIds.AquariaBattlefield03, Items.MagicRing),
				new Battlefield(LocationIds.WintryBattlefield01, Items.Xp99),
				new Battlefield(LocationIds.WintryBattlefield02, Items.Gp600),
				new Battlefield(LocationIds.PyramidBattlefield01, Items.Xp540),
				new Battlefield(LocationIds.LibraBattlefield01, Items.ExitBook),
				new Battlefield(LocationIds.LibraBattlefield02, Items.Xp744),
				new Battlefield(LocationIds.FireburgBattlefield01, Items.Gp900),
				new Battlefield(LocationIds.FireburgBattlefield02, Items.GeminiCrest),
				new Battlefield(LocationIds.FireburgBattlefield03, Items.Xp816),
				new Battlefield(LocationIds.MineBattlefield01, Items.Gp1200),
				new Battlefield(LocationIds.MineBattlefield02, Items.ThunderSeal),
				new Battlefield(LocationIds.MineBattlefield03, Items.Xp1200),
				new Battlefield(LocationIds.VolcanoBattlefield01, Items.Xp1068),
				new Battlefield(LocationIds.WindiaBattlefield01, Items.Xp2808),
				new Battlefield(LocationIds.WindiaBattlefield02, Items.Xp2700),
			};
		}
		public void PlaceItems(ItemsPlacement itemsPlacement)
		{
			var battlefieldsWithItem = itemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.BattlefieldItem && x.Content != Items.None).ToList();

			foreach (var battlefield in battlefieldsWithItem)
			{
				battlefields.Find(x => x.Location == battlefield.Location).Reward = battlefield.Content;
			}
		}
		public void ShuffleBattlefieldRewards(bool shuffleBattlefields, GameLogic gamelogic, ApConfigs apconfigs, MT19337 rng)
		{
			if (apconfigs.ApEnabled)
			{
				SetBattlefieldRewards(shuffleBattlefields, apconfigs.ItemPlacement, gamelogic, rng);
			}
			else
			{
				ShuffleRewards(shuffleBattlefields, gamelogic, rng);
            }
		}
		public void ShuffleRewards(bool enable, GameLogic gamelogic, MT19337 rng)
		{
			if (!enable)
			{
				return;
			}

			List<Items> battlefieldRewards = battlefields.Select(x => x.Reward).ToList();

			battlefields.ForEach(x => x.Reward = rng.TakeFrom(battlefieldRewards));

			UpdateLogic(gamelogic);
		}
		public void SetBattlefieldRewards(bool shuffleBattlefields, List<ApObject> itemsplacement, GameLogic gamelogic, MT19337 rng)
		{
			var xpRewards = battlefields.Where(b => b.RewardType == BattlefieldRewardType.Experience).Select(b => b.Reward).ToList();
			var gpRewards = battlefields.Where(b => b.RewardType == BattlefieldRewardType.Gold).Select(b => b.Reward).ToList();

			List<LocationIds> battlefieldlocations = battlefields.Select(x => x.Location).ToList();

			var battlefieldItems = itemsplacement.Where(x => x.Type == GameObjectType.BattlefieldItem).ToList();
			var battlefieldXp = itemsplacement.Where(x => x.Type == GameObjectType.BattlefieldXp).ToList();
			var battlefieldGp = itemsplacement.Where(x => x.Type == GameObjectType.BattlefieldGp).ToList();

			battlefieldItems.ForEach(x => x.Location = (LocationIds)x.ObjectId);

			if (gpRewards.Count > battlefieldGp.Count)
			{
				if (shuffleBattlefields)
				{
					battlefieldGp = gamelogic.Rooms.SelectMany(r => r.GameObjects)
						.Where(o => o.Type == GameObjectType.BattlefieldGp)
						.Select(o => new ApObject(rng.TakeFrom(gpRewards), o.Location))
						.ToList();
				}
				else
				{ 
					battlefieldGp = battlefields.Where(b => b.RewardType == BattlefieldRewardType.Gold)
						.Select(b => new ApObject(b.Reward, b.Location))
						.ToList();
				}
			}

			if (xpRewards.Count > battlefieldXp.Count)
			{
				if (shuffleBattlefields)
				{
					battlefieldXp = gamelogic.Rooms.SelectMany(r => r.GameObjects)
						.Where(o => o.Type == GameObjectType.BattlefieldXp)
						.Select(o => new ApObject(rng.TakeFrom(xpRewards), o.Location))
						.ToList();
				}
				else
				{
					battlefieldXp = battlefields.Where(b => b.RewardType == BattlefieldRewardType.Experience)
						.Select(b => new ApObject(b.Reward, b.Location))
						.ToList();
				}
			}

			var allBattlefieldPlacements = battlefieldItems.Concat(battlefieldGp).Concat(battlefieldXp).ToList();

			battlefields.ForEach(x => x.Reward = allBattlefieldPlacements.Find(b => b.Location == x.Location).Content);

			UpdateLogic(gamelogic);
		}
		public void UpdateLogic(GameLogic gamelogic)
		{
			Dictionary<BattlefieldRewardType, GameObjectType> battlefieldTypeConverter = new() {
						{ BattlefieldRewardType.Gold, GameObjectType.BattlefieldGp },
						{ BattlefieldRewardType.Experience, GameObjectType.BattlefieldXp },
						{ BattlefieldRewardType.Item, GameObjectType.BattlefieldItem },
					};

			List<GameObjectType> battlefieldObjectTypes = new() { GameObjectType.BattlefieldGp, GameObjectType.BattlefieldXp, GameObjectType.BattlefieldItem };
			var battlefieldsObject = gamelogic.Rooms.SelectMany(r => r.GameObjects.Where(o => battlefieldObjectTypes.Contains(o.Type))).ToList();
			battlefieldsObject.ForEach(x => x.Type = battlefieldTypeConverter[battlefields.ToList().Find(b => (int)b.Location == x.ObjectId).RewardType]);
		}
		public List<BattlefieldRewardType> GetAllRewardType()
		{
			return battlefields.OrderBy(x => x.Location).Select(x => x.RewardType).ToList();
		}
		public void SetBattlesQty(BattlesQty battlesqty, MT19337 rng)
		{
			int battleQty = 10;
			bool randomQty = false;

			switch (battlesqty)
			{
				case BattlesQty.Ten: return;
				case BattlesQty.Seven: battleQty = 7; break;
				case BattlesQty.Five: battleQty = 5; break;
				case BattlesQty.Three: battleQty = 3; break;
				case BattlesQty.One: battleQty = 1; break;
				case BattlesQty.RandomHigh: battleQty = 10; randomQty = true; break;
				case BattlesQty.RandomLow: battleQty = 5; randomQty = true; break;
			}

			for (int i = 0; i < _battlesQty.Count; i++)
			{ 
				_battlesQty[i] = randomQty ? (byte)rng.Between(1, battleQty) : (byte)battleQty;
			}
		}
		public void NewBattlefieldSprites(FFMQRom rom)
		{
			rom.PutInBank(0x07, 0xDDC4, Blob.FromHex("00001F00341C683A443D4A3A5037602E001F2345424548510000F8000C38065C02BC425C22EC167400F8C4A242A2128A00001F00301F6020492D492D4D2D6020001F205F5252525F0000F8000CF8060422B422B4B2B4060400F804FA4A4A4AFA"));
            rom.PutInBank(0x07, 0xDF14, Blob.FromHex("00001F00201F713F7F02433C413E7D3C001F20407D4341430000F8008478827C06FC8EF8FA44CE0C00F884820206BAF2612C612C682E54174808403F7F0000005252516877407F009634963436746AE8121002FCFE0000004A4A8A16EE02FE00612A6228612C682E6020403F7F000000545552515F407F008654461486341674060402FCFE0000002AAA4A8AFA02FE00"));
			rom.PutInBank(0x07, 0xE094, Blob.FromHex("6E2077077F3E7D3C7E3C423D7F0000005F78414343427F00BE1CDE44F6F0FEFCFEFC02FCFE000000E2BA0E020202FE00"));
        }
		public void Write(FFMQRom rom)
		{
			NewBattlefieldSprites(rom);
			rom.PutInBank(0x0C, 0xD4D0, _battlesQty.ToArray());
			rom.PutInBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, battlefields.OrderBy(x => x.Location).SelectMany(x => x.GetBytes()).ToArray());
           
        }
	
	}
}
