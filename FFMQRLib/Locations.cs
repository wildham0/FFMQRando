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

	public class ChestLocation
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public byte AccessRequirements { get; set; }

		public ChestLocation(int mapobjid, int mapid, byte access)
		{
			ObjectId = mapobjid;
			MapId = mapid;
			AccessRequirements = access;
		}
	}

	public class TreasureObject
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public Locations Location { get; set; }
		public Items Content { get; set; }
		public TreasureType Type { get; set; }
		public bool IsPlaced { get; set; }
		public List<AccessReqs> AccessRequirements { get; set; }

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, List<AccessReqs> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			AccessRequirements = access;

		}

		public TreasureObject(TreasureObject treasure)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			AccessRequirements = treasure.AccessRequirements.ToList();
		}
	}
	public class GameFlags
	{
		private const int StartingGameFlags = 0x0653A4;
		private byte[] _gameflags;
		private List<(int, int)> BitPos = new List<(int,int)> { (0,0x80), (1,0x40), (2,0x20), (3,0x10), (4,0x08), (5,0x04), (6,0x02), (7,0x01) };

		public GameFlags(FFMQRom rom)
		{
			_gameflags = rom.Get(StartingGameFlags, 0x20);

		}

		public void Write(FFMQRom rom)
		{
			rom.Put(StartingGameFlags, _gameflags);
		}
		public bool this[int flag]
		{
			get => HexToFlag(flag);
			set => FlagToHex(flag, value);
		}

		private bool HexToFlag(int flag)
		{
			var targetbyte = flag / 8;
			var targetbit = BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			return (_gameflags[targetbyte] & targetbit) == targetbit;
		}

		private void FlagToHex(int flag, bool value)
		{
			var targetbyte = flag / 8;
			var targetbit = (byte)BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			if (value)
			{
				_gameflags[targetbyte] |= targetbit;
			}
			else
			{
				_gameflags[targetbyte] &= (byte)~targetbit;
			}
		}

	}
	public class Battlefields
	{

		private List<byte> _battlesQty;
		private const int BattlefieldsQty = 0x14;
		private Items ForestaWestBattlefield = Items.Charm;
		private Items AquariaBattlefield03 = Items.MagicRing;
		private Items LibraBattlefield01 = Items.ExitBook;
		private Items FireburgBattlefield02 = Items.GeminiCrest;
		private Items MineBattlefield02 = Items.ThunderSeal;

		public Battlefields(FFMQRom rom)
		{
			_battlesQty = rom.GetFromBank(0x0C, 0xD4D0, BattlefieldsQty).Chunk(1).Select(x => x[0]).ToList();
		}

		public void PlaceItems(ItemsPlacement itemsPlacement)
		{
			ForestaWestBattlefield = itemsPlacement.ItemsLocations.Find(x => x.Location == Locations.ForestaWestBattlefield).Content;
			AquariaBattlefield03 = itemsPlacement.ItemsLocations.Find(x => x.Location == Locations.AquariaBattlefield03).Content;
			LibraBattlefield01 = itemsPlacement.ItemsLocations.Find(x => x.Location == Locations.LibraBattlefield01).Content;
			FireburgBattlefield02 = itemsPlacement.ItemsLocations.Find(x => x.Location == Locations.FireburgBattlefield02).Content;
			MineBattlefield02 = itemsPlacement.ItemsLocations.Find(x => x.Location == Locations.MineBattlefield02).Content;
		}
		public void SetBattlesQty(Flags flags, MT19337 rng)
		{
			int battleQty = 10;
			bool randomQty = false;

			switch (flags.BattlesQuantity)
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

			rom.PutInBank(0x07, 0xEFA3, new byte[] { (byte)ForestaWestBattlefield });
			rom.PutInBank(0x07, 0xEFAB, new byte[] { (byte)AquariaBattlefield03 });
			rom.PutInBank(0x07, 0xEFB3, new byte[] { (byte)LibraBattlefield01 });
			rom.PutInBank(0x07, 0xEFB9, new byte[] { (byte)FireburgBattlefield02 });
			rom.PutInBank(0x07, 0xEFBF, new byte[] { (byte)MineBattlefield02 });
		}
	
	}
	public class NodeLocations
	{
		private List<Blob> _movementArrows = new();
		private const int OWMovementArrows = 0x03EE84;
		public NodeLocations(FFMQRom rom)
		{
			_movementArrows = rom.Get(OWMovementArrows, (int)Locations.MacsShipDoom * 0x05).Chunk(5);
		}
		public void OpenNodes()
		{
			for (int i = 0; i <= (int)Locations.PazuzusTower; i++)
			{
				for (int y = 1; y < 5; y++)
				{
					if (_movementArrows[i][y] != 0)
					{
						_movementArrows[i][y] = (int)GameFlagsList.HillCollapsed;
					}
				}
			}

			_movementArrows[(int)Locations.LibraBattlefield02][(int)NodeMovementOffset.North] = (int)GameFlagsList.WakeWaterUsed;
			_movementArrows[(int)Locations.LibraTemple][(int)NodeMovementOffset.North] = (int)GameFlagsList.WakeWaterUsed;
			_movementArrows[(int)Locations.VolcanoBattlefield01][(int)NodeMovementOffset.South] = (int)GameFlagsList.WakeWaterUsed;
			_movementArrows[(int)Locations.VolcanoBattlefield01][(int)NodeMovementOffset.West] = (int)GameFlagsList.VolcanoErupted;
			_movementArrows[(int)Locations.Volcano][(int)NodeMovementOffset.East] = (int)GameFlagsList.VolcanoErupted;

			_movementArrows[(int)Locations.PazuzusTower][(int)NodeMovementOffset.North] = 0xDC;
			_movementArrows[(int)Locations.SpencersPlace][(int)NodeMovementOffset.South] = 0xDC;
		}
		public void Write(FFMQRom rom)
		{
			// classData.SelectMany(x => x.MagicPermissions()).ToArray()
			rom.Put(OWMovementArrows, _movementArrows.SelectMany(x => x.ToBytes()).ToArray());
		}


	}

	public class MapUtilities
	{
		private List<Blob> _areaattributes = new();

		public MapUtilities(FFMQRom rom)
		{
			var attributepointers = rom.Get(RomOffsets.AreaAttributesPointers, RomOffsets.AreaAttributesPointersQty * 2).Chunk(2);

			foreach (var pointer in attributepointers)
			{
				var address = RomOffsets.AreaAttributesPointersBase + pointer[1] * 0x100 + pointer[0];
				_areaattributes.Add(rom.Get(address, 8));
			}
		}

		public byte AreaIdToMapId(byte areaid)
		{
			return _areaattributes[(int)areaid][1];
		}
	}
	public partial class FFMQRom : SnesRom
	{


	
	}
}
