using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

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

	public partial class FFMQRom : SnesRom
	{

		public class ObjectList
		{
			private List<List<MapObject>> _collections = new List<List<MapObject>>();
			private List<int> _pointerCollectionPairs = new();
			private List<Blob> _areaattributes = new();
			public List<Chest> Chests = new();
			private List<Blob> _attributepointers = new();

			public ObjectList(FFMQRom rom)
			{
				_attributepointers = rom.Get(RomOffsets.AreaAttributesPointers, RomOffsets.AreaAttributesPointersQty * 2).Chunk(2);

				var previousPointer = _attributepointers[0].ToUShorts()[0];
				var collectionCount = 0;

				for (int i = 0; i < _attributepointers.Count; i++)
				{
					if (i != 0 && _attributepointers[i].ToUShorts()[0] == previousPointer)
					{
						_pointerCollectionPairs.Add(collectionCount-1);
						continue;
					}

					_pointerCollectionPairs.Add(collectionCount);

					var address = RomOffsets.AreaAttributesPointersBase + _attributepointers[i].ToUShorts()[0];
					_areaattributes.Add(rom.Get(address, 8));

					address += 8;

					_collections.Add(new List<MapObject>());

					while (rom.Data[address] != 0xFF)
					{
						_collections.Last().Add(new MapObject(address, rom));
						address += RomOffsets.MapObjectsAttributesSize;
					}
					collectionCount++;
					previousPointer = _attributepointers[i].ToUShorts()[0];
				}

				for(int  i = 0; i < _collections.Count; i++)
				{
					for(int j = 0; j < _collections[i].Count; j++)
					{
						if (_collections[i][j].Type == MapObjectType.Chest)
						{
							Chests.Add(new Chest(j, i, 0x00, _collections[i][j].X, _collections[i][j].Y, _collections[i][j].Value, (Items)rom.Data[RomOffsets.TreasuresOffset + _collections[i][j].Value]));
						}
					}
				}
			}
			public void UpdateChests()
			{
				foreach (var chest in Chests)
				{
					_collections[chest.AreaId][chest.ObjectId].Value = (byte)chest.Treasure;
				}
			}
			public int GetAreaMapId(int area)
			{
				return _areaattributes[area][1];
			}
			public void SetEnemiesDensity(Flags flags, MT19337 rng)
			{
				int density = 100;

				switch (flags.EnemiesDensity)
				{
					case EnemiesDensity.All: return;
					case EnemiesDensity.ThreeQuarter: density = 75; break;
					case EnemiesDensity.Half: density = 50; break;
					case EnemiesDensity.Quarter: density = 25; break;
					case EnemiesDensity.None: density = 0; break;
				}

				for (int i = 0; i < _collections.Count; i++)
				{
					var enemiescollection = _collections[i].Where(x => x.Type == MapObjectType.Battle).ToList();
					int totalcount = enemiescollection.Count;
					int toremove = ((100 - density) * totalcount) / 100;

					for (int j = 0; j < toremove; j++)
					{
						rng.TakeFrom(enemiescollection).Gameflag = 0xFE;
					}
				}
			}
			public void ShuffleEnemiesPosition(List<Map> maps, Flags flags, MT19337 rng)
			{
				if (flags.ShuffleEnemiesPosition == false)
				{
					return;
				}

				for (int i = 0; i < _collections.Count; i++)
				{
					var enemiescollection = _collections[i].Where(x => x.Type == MapObjectType.Battle).ToList();
					if (!enemiescollection.Any())
					{
						continue;
					}
					int minx = enemiescollection.Select(x => x.X).Min();
					int maxx = enemiescollection.Select(x => x.X).Max();
					int miny = enemiescollection.Select(x => x.Y).Min();
					int maxy = enemiescollection.Select(x => x.Y).Max();

					var validLayers = enemiescollection.Select(x => x.Layer).Distinct().ToList();
					var targetmap = GetAreaMapId(i);
					List<(byte, byte)> selectedPositions = _collections[i].Where(x => x.Type != MapObjectType.Battle).Select(x => (x.X, x.Y)).ToList();

					foreach (var enemy in enemiescollection)
					{
						bool placed = false;
						while (!placed)
						{
							byte newx = (byte)rng.Between(minx, maxx);
							byte newy = (byte)rng.Between(miny, maxy);
							if (!selectedPositions.Contains((newx, newy)) && validLayers.Contains(maps[targetmap].WalkableByte((int)newx, (int)newy)))
							{
								selectedPositions.Add((newx, newy));
								enemy.X = newx;
								enemy.Y = newy;
								placed = true;
							}
						}
					}
				}
			}
			public void WriteAll(FFMQRom rom)
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
					
					rom.Put(RomOffsets.AreaAttributesPointersBase + pointerOffset, _areaattributes[i]);

					pointerOffset += 8;

					for (int j = 0; j < _collections[i].Count; j++)
					{
						_collections[i][j].WriteAt(rom, RomOffsets.AreaAttributesPointersBase + pointerOffset);
						pointerOffset += RomOffsets.MapObjectsAttributesSize;
					}

					rom.Put(RomOffsets.AreaAttributesPointersBase + pointerOffset, Blob.FromHex("FF"));
					pointerOffset++;
				}

				rom.Put(RomOffsets.AreaAttributesPointers, _attributepointers.SelectMany(x => x.ToBytes()).ToArray());
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
			public byte Orientation { get; set; }
			public byte Palette { get; set; }
			public byte Behavior { get; set; }
			public byte Layer { get; set; }

			public MapObject(int address, FFMQRom rom)
			{
				_address = address;
				_array = rom.Get(address, RomOffsets.MapObjectsAttributesSize).ToBytes().ToList();

				UpdateValues();
			}
			public void RawOverwrite(byte[] rawArray)
			{
				_array = rawArray.ToList();

				UpdateValues();
			}
			private void UpdateArray()
			{
				_array[0] = Gameflag;
				_array[1] = Value;
				_array[2] = (byte)((UnknownIndex * 64) + (Y & 0b0011_1111));
				_array[3] = (byte)((Orientation * 64) + (X & 0b0011_1111));
				_array[4] = (byte)(Palette * 16 + Behavior);
				_array[5] = (byte)(((int)Type * 8) + (Solid ? 0 : 0b0010_0000) + (Pushable ? 0b0100_0000 : 0) + (Layer & 0b0000_0011) + (_array[5] & 0b1000_0100));
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
				Palette = (byte)((_array[4] & 0b1111_0000) / 16);
				Behavior = (byte)((_array[4] & 0b0000_1111));
				Type = (MapObjectType)((_array[5] & 0b0001_1000) / 8);
				Solid = (bool)((_array[5] & 0b0010_0000) == 0);
				Pushable = (bool)((_array[5] & 0b0100_0000) != 0);
				Layer = (byte)(_array[5] & 0b0000_0011);
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
}
