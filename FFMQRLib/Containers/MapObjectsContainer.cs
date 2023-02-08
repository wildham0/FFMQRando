using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;

namespace FFMQLib
{
	public enum MapObjectType : byte
	{
		Talk = 0x00,
		Battle = 0x01,
		Chest = 0x02
	}
	public enum EnemiesDensity : int
	{
		[Description("100%")]
		All = 0,
		[Description("75%")]
		ThreeQuarter,
		[Description("50%")]
		Half,
		[Description("25%")]
		Quarter,
		[Description("0%")]
		None,
	}

	public partial class ObjectList
	{
		private List<List<MapObject>> _collections = new List<List<MapObject>>();
		private List<int> _pointerCollectionPairs = new();
		private List<Blob> _areaattributes = new();
		public List<(int, int, int)> ChestList = new();
		private List<Blob> _attributepointers = new();

		private int NewAreaAttributesPointers = 0xB000;
		private int NewAreaAttributesPointersBase = 0xB100;
		private int NewAreaAttributesBank = 0x11;

		public ObjectList(FFMQRom rom)
		{
			_attributepointers = rom.Get(RomOffsets.AreaAttributesPointers, RomOffsets.AreaAttributesPointersQty * 2).Chunk(2);

			var previousPointer = _attributepointers[0].ToUShorts()[0];
			var collectionCount = 0;

			for (int i = 0; i < _attributepointers.Count; i++)
			{
				if (i != 0 && _attributepointers[i].ToUShorts()[0] == previousPointer)
				{
					_pointerCollectionPairs.Add(collectionCount - 1);
					continue;
				}

				_pointerCollectionPairs.Add(collectionCount);

				var address = RomOffsets.AreaAttributesPointersBase + _attributepointers[i].ToUShorts()[0];
				_areaattributes.Add(rom.Get(address, 8));

				address += 8;

				_collections.Add(new List<MapObject>());

				while (rom[address] != 0xFF)
				{
					_collections.Last().Add(new MapObject(address, rom));
					address += RomOffsets.MapObjectsAttributesSize;
				}
				collectionCount++;
				previousPointer = _attributepointers[i].ToUShorts()[0];
			}

			for (int i = 0; i < _collections.Count; i++)
			{
				for (int j = 0; j < _collections[i].Count; j++)
				{
					if (_collections[i][j].Type == MapObjectType.Chest)
					{
						ChestList.Add((_collections[i][j].Value, i, j));
					}
				}
			}
		}
		public void ModifyAreaAttribute(int index, int position, byte value)
		{
			_areaattributes[_pointerCollectionPairs[index]][position] = value;
		}
		public int GetAreaMapId(int area)
		{
			return _areaattributes[area][1];
		}
		public void Write(FFMQRom rom)
		{
			var pointerOffset = _attributepointers[0].ToUShorts()[0];
			var pointersCount = 0;

			for (int i = 0; i < _collections.Count; i++)
			{
				var pointersQty = _pointerCollectionPairs.Where(x => x == i).Count();

				for (int j = 0; j < pointersQty; j++)
				{
					_attributepointers[pointersCount + j] = new byte[] { (byte)(pointerOffset % 0x100), (byte)(pointerOffset / 0x100) };
				}
				pointersCount += pointersQty;
				/*
				// hacky, will need to work area attributes
				if (_collections[i].Any() && _areaattributes[i][2] == 0xFF)
				{
					_areaattributes[i][2] = 0x02;
				}*/
				
				rom.PutInBank(NewAreaAttributesBank, NewAreaAttributesPointersBase + pointerOffset, _areaattributes[i]);

				pointerOffset += 8;

				for (int j = 0; j < _collections[i].Count; j++)
				{
					_collections[i][j].WriteAt(rom, NewAreaAttributesBank, NewAreaAttributesPointersBase + pointerOffset);
					pointerOffset += RomOffsets.MapObjectsAttributesSize;
				}

				rom.PutInBank(NewAreaAttributesBank, NewAreaAttributesPointersBase + pointerOffset, Blob.FromHex("FF"));
				pointerOffset++;
			}

			rom.PutInBank(NewAreaAttributesBank, NewAreaAttributesPointers, _attributepointers.SelectMany(x => x.ToBytes()).ToArray());

			rom.PutInBank(0x03, 0x82D3, Blob.FromHex("00B011"));
			rom.PutInBank(0x03, 0x82E0, Blob.FromHex("07B111")); // Location name // 07b01a
			rom.PutInBank(0x03, 0x8317, Blob.FromHex("00B011"));
			rom.PutInBank(0x03, 0x8324, Blob.FromHex("05B111")); // Location floor // 07b018
			rom.PutInBank(0x03, 0x8353, Blob.FromHex("00B011"));
			rom.PutInBank(0x03, 0x8360, Blob.FromHex("05B111")); // Location floor
			rom.PutInBank(0x0B, 0x81B0, Blob.FromHex("00B011"));

			rom.PutInBank(0x01, 0x90F0, Blob.FromHex("00B111"));
			rom.PutInBank(0x0B, 0x81BD, Blob.FromHex("00B111"));
		}
		public List<MapObject> this[int floorid]
		{
			get => _collections[_pointerCollectionPairs[floorid]];
			set => _collections[_pointerCollectionPairs[floorid]] = value;
		}
	}

	public class MapObject
	{
		private List<byte> _array = new();
		private int _address;

		public MapObjectType Type { get; set; }
		public byte Gameflag { get; set; }
		public byte Value { get; set; }
		public bool Solid { get; set; }
		public bool Pushable { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }
		public byte Sprite { get; set; }
		public byte UnknownIndex { get; set; }
		public byte Unknown4 { get; set; }
		public byte Orientation { get; set; }
		public byte Palette { get; set; }
		public byte Behavior { get; set; }
		public byte Layer { get; set; }

		public (byte x, byte y) Coord { 
			get => (X, Y);
			set {
				X = value.x;
				Y = value.y;
			}
		}
		public MapObject(int address, FFMQRom rom)
		{
			_address = address;
			_array = rom.Get(address, RomOffsets.MapObjectsAttributesSize).ToBytes().ToList();

			UpdateValues();
		}
		public MapObject(MapObject mapobject)
		{
			CopyFrom(mapobject);
		}
		public void RawOverwrite(byte[] rawArray)
		{
			_array = rawArray.ToList();

			UpdateValues();
		}
		public void CopyFrom(MapObject mapobject)
		{
			RawOverwrite(mapobject.RawArray());
		}
		private void UpdateArray()
		{
			_array[0] = Gameflag;
			_array[1] = Value;
			_array[2] = (byte)((UnknownIndex * 64) + (Y & 0b0011_1111));
			_array[3] = (byte)((Orientation * 64) + (X & 0b0011_1111));
			_array[4] = (byte)(Palette * 32 + Unknown4 + Behavior);
			_array[5] = (byte)(((int)Type * 8) + (Solid ? 0 : 0b0010_0000) + (Pushable ? 0b0100_0000 : 0) + (Layer & 0b0000_0111) + (_array[5] & 0b1000_0000));
			_array[6] = (byte)((int)Sprite + (_array[6] & 0b1000_0000));
		}

		private void UpdateValues()
		{
			Gameflag = _array[0];
			Value = _array[1];
			Y = (byte)(_array[2] & 0b0011_1111);
			UnknownIndex = (byte)((_array[2] & 0b1100_0000) / 64);
			X = (byte)(_array[3] & 0b0011_1111);
			Orientation = (byte)((_array[3] & 0b1100_0000) / 64);
			Palette = (byte)((_array[4] & 0b1110_0000) / 32);
			Unknown4 = (byte)((_array[4] & 0b0001_0000));
			Behavior = (byte)((_array[4] & 0b0000_1111));
			Type = (MapObjectType)((_array[5] & 0b0001_1000) / 8);
			Solid = (bool)((_array[5] & 0b0010_0000) == 0);
			Pushable = (bool)((_array[5] & 0b0100_0000) != 0);
			Layer = (byte)(_array[5] & 0b0000_0111);
			Sprite = (byte)(_array[6] & 0b0111_1111);
		}
		public void Write(FFMQRom rom)
		{

			UpdateArray();

			rom.Put(_address, _array.ToArray());
		}
		public void WriteAt(FFMQRom rom, int address)
		{
			UpdateArray();

			rom.Put(address, _array.ToArray());
		}
		public void WriteAt(FFMQRom rom, int bank,int address)
		{
			UpdateArray();

			rom.PutInBank(bank, address, _array.ToArray());
		}
		public bool IsNullEntry()
		{
			if (_array[0] == 0xFF)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public byte[] RawArray()
		{
			UpdateArray();
			return _array.ToArray();
		}
		public string RawArrayString()
		{
			string HexAlphabet = "0123456789ABCDEF";

			var newstring = "";
			foreach (var hexvalue in _array)
			{
				newstring += HexAlphabet[hexvalue >> 4];
				newstring += HexAlphabet[hexvalue & 0x0F];
			}

			return newstring;
		}
	}

	public class Chest
	{
		public int ObjectId { get; set; }
		public int AreaId { get; set; }
		public byte AccessRequirements { get; set; }
		public byte TreasureId { get; set; }
		public Items Treasure { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }

		public Chest(int objid, int areaid, byte access, byte x, byte y, byte treasureid, Items treasure)
		{
			ObjectId = objid;
			AreaId = areaid;
			AccessRequirements = access;
			TreasureId = treasureid;
			X = x;
			Y = y;
			Treasure = treasure;
		}
	}
}

