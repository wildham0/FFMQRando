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
	public class Battlefields
	{
		private const int BattlefieldsRewardsBank = 0x07;
		private const int BattlefieldsRewardsOffset = 0xEFA1;

		private const int BattlefieldsQty = 0x14;

		private List<byte> _battlesQty;
		private List<Blob> _rewards;

		public List<LocationIds> BattlefieldsWithItem;

		public Battlefields(FFMQRom rom)
		{
			_battlesQty = rom.GetFromBank(0x0C, 0xD4D0, BattlefieldsQty).Chunk(1).Select(x => x[0]).ToList();
			_rewards = rom.GetFromBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, BattlefieldsQty * 2).Chunk(2);

			BattlefieldsWithItem = new();

			for (int i = 0; i < _battlesQty.Count; i++)
			{
				if ((BattlefieldRewardType)(_rewards[i][1] & 0b1100_0000) == BattlefieldRewardType.Item)
				{
					BattlefieldsWithItem.Add((LocationIds)(i + 1));
				}
			}
		}
		public void PlaceItems(ItemsPlacement itemsPlacement)
		{
			var battlefieldsWithItem = itemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.Battlefield && x.Content != Items.None).ToList();

			foreach (var battlefield in battlefieldsWithItem)
			{
				_rewards[(int)(battlefield.Location - 1)][0] = (byte)battlefield.Content;
			}
		}
		public void ShuffleBattelfieldRewards(bool enable, Overworld overworld, MT19337 rng)
		{
			if (!enable)
			{
				return;
			}
			
			_rewards.Shuffle(rng);

			BattlefieldsWithItem.Clear();

			for (int i = 0; i < _battlesQty.Count; i++)
			{
				if ((BattlefieldRewardType)(_rewards[i][1] & 0b1100_0000) == BattlefieldRewardType.Item)
				{
					BattlefieldsWithItem.Add((LocationIds)(i + 1));
				}
			}
		}
        public void SetBattelfieldRewards(bool enable, List<ApObject> itemsplacement, MT19337 rng)
        {
            if (!enable)
            {
                return;
            }

            BattlefieldsWithItem.Clear();

            var battlefieldPlacement = itemsplacement.Where(x => x.Type == GameObjectType.Battlefield).Select(x => (int)(x.ObjectId - 1)).OrderByDescending(x => x).ToList();

			List<(int, Blob)> battlefieldPlaced = new();

			foreach (var battlefield in battlefieldPlacement)
			{
                battlefieldPlaced.Add((battlefield, _rewards[battlefield]));
				_rewards.RemoveAt(battlefield);
            }
            
			_rewards.Shuffle(rng);

            battlefieldPlaced = battlefieldPlaced.OrderBy(x => x.Item1).ToList();

            foreach (var battlefield in battlefieldPlaced)
            {
                _rewards.Insert(battlefield.Item1, battlefield.Item2);
                BattlefieldsWithItem.Add((LocationIds)(battlefield.Item1 + 1));
            }
        }
        public BattlefieldRewardType GetRewardType(LocationIds targetBattlefield)
		{
			return (BattlefieldRewardType)(_rewards[(int)(targetBattlefield - 1)][1] & 0b1100_0000);
		}
		public List<BattlefieldRewardType> GetAllRewardType()
		{
			return _rewards.Select(x => (BattlefieldRewardType)(x[1] & 0b1100_0000)).ToList();
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
			rom.PutInBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, _rewards.SelectMany(x => x.ToBytes()).ToArray());
		}
	
	}
}
