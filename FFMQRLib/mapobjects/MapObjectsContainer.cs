using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;
using System.Reflection;

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

	public enum BgBlendingMode : int
	{ 
		BackgroundSolid = 0,
		BackgroundWater,
		ForegroundSolid,
		BackgroundTransparent,
		ForegroundSolidUnderSprite,
		ForegroundTransparent,
		ForegroundSolidMapShaded
	}
	public enum BgType : int
	{
		FixedTile = 0,
		FullStepDuplicateMap, // prop1, prop2 are x,y offset?
		HalfStepSecondMap,
		FullStepTile,
		FullStepTileScrolling,
		HalfStepTile,
		DiagonalScrolling,
	}
	public class BackgroundLayer
	{ 
		public BgBlendingMode BlendingMode { get; set; }
		public BgType Type { get; set; }
		public byte GraphicId { get; set; }
		public byte Property1 { get; set; } // can be scroll speed, or x,y offset depending
		public byte Property2 { get; set; }

		public BackgroundLayer(byte[] data)
		{
			BlendingMode = (BgBlendingMode)((data[0] & 0x70) / 16);
			Type = (BgType)((data[0] & 0x07));
			GraphicId = data[1];
			Property1 = data[2];
			Property2 = data[3];
		}
		public byte[] GetBytes()
		{
			byte bgmode = (byte)(((int)BlendingMode * 16) | (int)Type);
			return new byte[] { bgmode, GraphicId, Property1, Property2 };
		}
	}
	public class BackgroundLayers
	{ 
		List<BackgroundLayer> Entries { get; set; }

		private const int BgBank = 0x0B;
		private const int BgOffset = 0x844F;
		private const int BgSize = 0x04;
		private const int BgQty = 36;

		public BackgroundLayers(FFMQRom rom)
		{
			Entries = rom.GetFromBank(BgBank, BgOffset, BgSize * BgQty).Chunk(BgSize).Select(b => new BackgroundLayer(b)).ToList();
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(BgBank, BgOffset, Entries.SelectMany(b => b.GetBytes()).ToArray());
		}
	}

	public class AreaAttributes
	{ 
		// 0
		// 1 - MapId
		// 2 - SpriteSet
		// 7 - Location Name

	
		public int BackgroundLayer { get; set; }
		private byte[] data;

		public AreaAttributes(byte[] rawdata)
		{
			data = rawdata;
			BackgroundLayer = (((data[6] & 0xe0) / 8) | (data[5] & 0xe0)) / 4;
			data[5] &= 0x1f;
			data[6] &= 0x1f;
			
		}
		public void ModifyByte(int index, byte value)
		{
			data[index] = value;
		}
		public byte GetByte(int index)
		{
			return data[index];
		}
		public byte[] GetBytes()
		{
			data[5] |= (byte)((BackgroundLayer & 0x38) * 4);
			data[6] |= (byte)((BackgroundLayer & 0x07) * 32);

			return data;
		}
	}
	public class Area
	{
		public List<MapObject> Objects { get; set; }
		public AreaAttributes Attributes { get; set; }

		public Area(byte[] attributes, List<MapObject> objects)
		{
			Attributes = new AreaAttributes(attributes);
			Objects = objects;
		}
		public byte[] GetBytes()
		{
			return Attributes.GetBytes().Concat(Objects.SelectMany(o => o.GetBytes())).ToArray();
		}
	}

	public partial class Areas
	{
		public List<(int content, int area, int objectid)> ChestObjects = new();

		public List<Area> Entries { get; set; }
		private List<int> areaPointers;
		private ushort initialPointer;

		private int NewAreaAttributesPointers = 0xB000;
		private int NewAreaAttributesPointersBase = 0xB100;
		private int NewAreaAttributesBank = 0x11;

		private const int MapObjectsAttributesSize = 7;
		private const int AreaAttributesSize = 8;

		private const int AreaAttributesPointers = 0x3AF3B;
		private const int AreaAttributesPointersBase = 0x3B013;
		private const int AreaAttributesPointersQty = 108;
		private const int AreaAttributesPointersSize = 2;

		public Areas(FFMQRom rom)
		{
			var attributepointers = rom.Get(AreaAttributesPointers, AreaAttributesPointersQty * 2).Chunk(2);

			initialPointer = attributepointers[0].ToUShorts()[0];
			var previousPointer = initialPointer;
			var collectionCount = 0;
			
			areaPointers = new();
			Entries = new();

			for (int i = 0; i < attributepointers.Count; i++)
			{
				if (i != 0 && attributepointers[i].ToUShorts()[0] == previousPointer)
				{
					areaPointers.Add(collectionCount - 1);
					continue;
				}

				areaPointers.Add(collectionCount);

				var address = AreaAttributesPointersBase + attributepointers[i].ToUShorts()[0];
				byte[] attributes = rom.Get(address, AreaAttributesSize);
				
				address += AreaAttributesSize;
				var objects = new List<MapObject>();

				while (rom[address] != 0xFF)
				{
					objects.Add(new MapObject(rom.Get(address, MapObjectsAttributesSize)));
					address += MapObjectsAttributesSize;
				}
				Entries.Add(new Area(attributes, objects));
				collectionCount++;
				previousPointer = attributepointers[i].ToUShorts()[0];
			}

			for (int i = 0; i < Entries.Count; i++)
			{
				for (int j = 0; j < Entries[i].Objects.Count; j++)
				{
					if (Entries[i].Objects[j].Type == MapObjectType.Chest)
					{
						ChestObjects.Add((Entries[i].Objects[j].Value, i, j));
					}
				}
			}
		}

		/*
		public ObjectList(FFMQRom rom)
		{
			_attributepointers = rom.Get(AreaAttributesPointers, AreaAttributesPointersQty * 2).Chunk(2);

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

				var address = AreaAttributesPointersBase + _attributepointers[i].ToUShorts()[0];
				_areaattributes.Add(rom.Get(address, 8));

				address += 8;

				_collections.Add(new List<MapObject>());

				while (rom[address] != 0xFF)
				{
					_collections.Last().Add(new MapObject(address, rom));
					address += MapObjectsAttributesSize;
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
		}*/
		public void ModifyAreaAttribute(int index, int position, byte value)
		{
			Entries[areaPointers[index]].Attributes.ModifyByte(position, value);
		}
		public int GetAreaMapId(int area)
		{
			return Entries[area].Attributes.GetByte(1);
		}
		public void SwapMapObjects(int index, int object1, int object2)
		{
			var tempobject = Entries[areaPointers[index]].Objects[object1];
			Entries[areaPointers[index]].Objects[object1] = new MapObject(Entries[areaPointers[index]].Objects[object2]);
			Entries[areaPointers[index]].Objects[object2] = new MapObject(tempobject);
        }
		public void Write(FFMQRom rom)
		{
			int pointerOffset = (int)initialPointer;
			var pointersCount = 0;
			List<byte[]> pointers = new();

			for (int i = 0; i < Entries.Count; i++)
			{
				var pointersQty = areaPointers.Where(x => x == i).Count();
				var currentPointer = new byte[] { (byte)(pointerOffset % 0x100), (byte)(pointerOffset / 0x100) };

				for (int j = 0; j < pointersQty; j++)
				{
					pointers.Add(currentPointer);
				}
				pointersCount += pointersQty;


				var currentEntry = Entries[i].GetBytes().Concat(new byte[] { 0xFF }).ToArray();
				rom.PutInBank(NewAreaAttributesBank, NewAreaAttributesPointersBase + pointerOffset, currentEntry);

				pointerOffset += currentEntry.Length;
			}

			rom.PutInBank(NewAreaAttributesBank, NewAreaAttributesPointers, pointers.SelectMany(x => x).ToArray());

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
			get => Entries[areaPointers[floorid]].Objects;
			//set => Entries[areaPointers[floorid]].Objects = value;
		}
		public string DumpXYLocations()
		{
			string tempstring = "";

			for (int i = 0; i < Entries.Count; i++)
			{
                var tempmap = Enumerable.Repeat(0xFF, 64 * 64).ToArray();

                for (int j = 0; j < Entries[i].Objects.Count; j++)
				{
					tempmap[Entries[i].Objects[j].Y * 64 + Entries[i].Objects[j].X] = j;
                }

                tempstring += i.ToString("X2") + ".\n";
              
				for (int y = 0; y < 64; y++)
                {
                    string myStringOutput = String.Join("", tempmap[(y * 64)..((y+1)*64)].Select(p => p == 0xFF ? "__" : p.ToString("X2")).ToArray());

					tempstring += myStringOutput + "\n";
                }

				tempstring += "\n";

            }

			return tempstring;
		}
	}

	public class MapObject
	{
		private List<byte> _array = new();
		private int _address;
		private const int MapObjectsAttributesSize = 7;

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
		public FacingOrientation Facing { get; set; }
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
		public MapObject()
		{
			_address = 0;
			_array = (new byte[7]).ToList();

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
		public MapObject(byte[] rawArray)
		{
			RawOverwrite(rawArray);
		}
		public void CopyFrom(MapObject mapobject)
		{
			RawOverwrite(mapobject.GetBytes());
		}
		private void UpdateArray()
		{
			_array[0] = Gameflag;
			_array[1] = Value;
			_array[2] = (byte)((UnknownIndex * 64) + (Y & 0b0011_1111));
			_array[3] = (byte)(((byte)Facing * 64) + (X & 0b0011_1111));
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
			Facing = (FacingOrientation)((_array[3] & 0b1100_0000) / 64);
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
		public byte[] GetBytes()
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
}

