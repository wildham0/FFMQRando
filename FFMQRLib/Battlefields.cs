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
		public BattlefieldRewardType RewardType { get; set; }
		public ushort Value { get; set; }

		public Battlefield(LocationIds location, byte[] rawvalues)
		{
			Location = location;
			RewardType = (BattlefieldRewardType)(rawvalues[1] & 0xC0);

			if (RewardType == BattlefieldRewardType.Item)
			{
				Value = rawvalues[0];
			}
			else if (RewardType == BattlefieldRewardType.Experience)
			{
				Value = (ushort)(rawvalues[0] + (0x100 * (rawvalues[1] & 0x7F)));
			}
			else if (RewardType == BattlefieldRewardType.Gold)
			{
                Value = (ushort)(rawvalues[0] + (0x100 * (rawvalues[1] & 0x3F)));
            }
		}

		public byte[] GetBytes()
		{
			if (RewardType == BattlefieldRewardType.Item)
			{
				return new byte[] { (byte)(Value & 0x00FF), (byte)RewardType };
			}
			else if (RewardType == BattlefieldRewardType.Experience)
			{
                return new byte[] { (byte)(Value & 0x00FF), (byte)((byte)RewardType | ((Value & 0x7F00) / 0x100)) };
			}
			else if (RewardType == BattlefieldRewardType.Gold)
			{
				return new byte[] { (byte)(Value & 0x00FF), (byte)((byte)RewardType | ((Value & 0x3F00) / 0x100)) };
			}
			else
			{
				return new byte[] { 0x00, 0x00 };
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

		//public List<LocationIds> BattlefieldsWithItem;
		private List<Battlefield> battlefields;

		public Battlefields(FFMQRom rom)
		{
			_battlesQty = rom.GetFromBank(0x0C, 0xD4D0, BattlefieldsQty).Chunk(1).Select(x => x[0]).ToList();
			_rewards = rom.GetFromBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, BattlefieldsQty * 2).Chunk(2);
			battlefields = _rewards.Select((x, i) => new Battlefield((LocationIds)(i + 1), x)).ToList();

			/*
            BattlefieldsWithItem = new();

			for (int i = 0; i < _battlesQty.Count; i++)
			{
				if ((BattlefieldRewardType)(_rewards[i][1] & 0b1100_0000) == BattlefieldRewardType.Item)
				{
					BattlefieldsWithItem.Add((LocationIds)(i + 1));
				}
			}*/
		}
		public void PlaceItems(ItemsPlacement itemsPlacement)
		{
			var battlefieldsWithItem = itemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.BattlefieldItem && x.Content != Items.None).ToList();

			foreach (var battlefield in battlefieldsWithItem)
			{
				battlefields.Find(x => x.Location == battlefield.Location).Value = (ushort)battlefield.Content;
			}
		}
		public void ShuffleBattelfieldRewards(bool enable, GameLogic gamelogic, MT19337 rng)
		{
			if (!enable)
			{
				return;
			}

			List<LocationIds> battlefieldlocations = battlefields.Select(x => x.Location).ToList();

			battlefields.ForEach(x => x.Location = rng.TakeFrom(battlefieldlocations));

			UpdateLogic(gamelogic);
        }
        public void SetBattelfieldRewards(bool enable, List<ApObject> itemsplacement, GameLogic gamelogic, MT19337 rng)
        {
            List<LocationIds> battlefieldlocations = battlefields.Select(x => x.Location).ToList();
            var battlefieldPlacement = itemsplacement.Where(x => x.Type == GameObjectType.BattlefieldItem).Select(x => (LocationIds)x.ObjectId).OrderByDescending(x => x).ToList();
			battlefieldlocations = battlefieldlocations.Except(battlefieldPlacement).ToList();

			var itemBattlefields = battlefields.Where(x => x.RewardType == BattlefieldRewardType.Item).ToList();
            var nonItemBattlefields = battlefields.Where(x => x.RewardType != BattlefieldRewardType.Item).ToList();

			itemBattlefields.ForEach(x => x.Location = rng.TakeFrom(battlefieldPlacement));

			if (enable)
			{
				nonItemBattlefields.ForEach(x => x.Location = rng.TakeFrom(battlefieldlocations));
			}

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
        public BattlefieldRewardType GetRewardType(LocationIds targetBattlefield)
		{
			return (BattlefieldRewardType)(_rewards[(int)(targetBattlefield - 1)][1] & 0b1100_0000);
		}
		public List<BattlefieldRewardType> GetAllRewardType()
		{
			return battlefields.OrderBy(x => x.Location).Select(x => x.RewardType).ToList();
		}
        public List<Battlefield> ToList()
        {
            return battlefields;
        }
        public List<LocationIds> BattlefieldsWithItems()
        {
            return battlefields.Where(x => x.RewardType == BattlefieldRewardType.Item).Select(x => x.Location).ToList();
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

		public void Write(FFMQRom rom)
		{
			rom.PutInBank(0x0C, 0xD4D0, _battlesQty.ToArray());
			rom.PutInBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, battlefields.OrderBy(x => x.Location).SelectMany(x => x.GetBytes()).ToArray());
		}
	
	}
}
