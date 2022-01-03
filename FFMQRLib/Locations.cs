using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
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
		public List<AccessReqs> AccessRequirements { get; set; }

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, List<AccessReqs> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			AccessRequirements = access;
		}

		public TreasureObject(TreasureObject treasure)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
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
