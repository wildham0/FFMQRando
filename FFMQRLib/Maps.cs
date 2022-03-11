using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public class TilesProperties
	{
		private List<List<SingleTile>> _tilesProperties;

		public TilesProperties(FFMQRom rom)
		{
			_tilesProperties = rom.Get(RomOffsets.MapTileData, 0x10 * 0x100).Chunk(0x100).Select(x => x.Chunk(0x02).Select(y => new SingleTile(y)).ToList()).ToList();
		}

		public List<SingleTile> this[int propTableID]
		{
			get => _tilesProperties[propTableID];
			set => _tilesProperties[propTableID] = value;
		}

		public void Write(FFMQRom rom)
		{
			rom.Put(RomOffsets.MapTileData, _tilesProperties.SelectMany(x => x.SelectMany(y => y.GetBytes())).ToArray());
		}
	}

	public class SingleTile
	{ 
		public byte Byte1 { get; set; }
		public byte Byte2 { get; set; }

		public SingleTile(byte[] tileprop)
		{
			Byte1 = tileprop[0];
			Byte2 = tileprop[1];
		}

		public byte[] GetBytes()
		{
			return new byte[] { Byte1, Byte2 };
		}
	}
	public partial class FFMQRom : SnesRom
	{
		public static readonly byte[] BitConverter = { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
		public class Map
		{

			private int _address;
			private int _referenceChunksAddress;
			public byte[] _maparray = new byte[128 * 128];
			private (int,int) _dimensions;
			private byte[] _areaattributes = new byte[0x0A];
			private byte[] _tiledata = new byte[0x100];
			private int _mapId;
			private byte[] _rawMap;
			//public int MapId { get; set; }

			public Map(int mapID, FFMQRom rom)
			{
				_mapId = mapID;

				_areaattributes = rom.Get(RomOffsets.MapAttributes + mapID * 0x0A, 0x0A).ToBytes();

				var rawAddress = rom.Get(RomOffsets.MapDataAddresses + (mapID * 3), 3);
				_address = rawAddress[2] * 0x8000 + (rawAddress[1] * 0x100 + rawAddress[0] - 0x8000);
				var rawRefAddress = rom.Get(_address, 2);

				_referenceChunksAddress = _address + 2 + rawRefAddress[1] * 0x100 + rawRefAddress[0];

				var refChunkPosition = _referenceChunksAddress;

				var mapPosition = 0;
				_rawMap = new byte[0];

				for (int i = 2; i < (_referenceChunksAddress - _address); i += 2)
				{

					var currentChunk = rom.Get(_address + i, 2);
					_rawMap += currentChunk;

					var chunckLength = currentChunk[0] & 0x0F;
					if ((currentChunk[0] & 0x0F) > 0)
					{
						Array.Copy(rom.Get(refChunkPosition, chunckLength).ToBytes(), 0, _maparray, mapPosition, chunckLength);
						refChunkPosition += chunckLength;
						mapPosition += chunckLength;
					}

					var higherbits = (currentChunk[0] & 0xF0) / 16;
					if (higherbits > 0)
					{
						var targetPosition = mapPosition - currentChunk[1] - 1;
						var tempArray = new byte[higherbits + 2];

						for (int j = 0; j < higherbits + 2; j++)
						{
							_maparray[mapPosition] = _maparray[targetPosition];
							mapPosition++;
							targetPosition++;
						}
					}
					else
					{
						i--;
					}
				}

				var tempdimensions = rom.Get(RomOffsets.MapDimensionsTable + (_areaattributes[0] & 0xF0) / 8, 2);
				_dimensions = (tempdimensions[0], tempdimensions[1]);

				_tiledata = rom.Get(RomOffsets.MapTileData + (_areaattributes[0] & 0x0F) * 0x100, 0x100).ToBytes();
			}

			public void DataDump()
			{
				for (int i = 0; i < (_dimensions.Item2); i++)
				{
					var tempmap = _maparray[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)];

					string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

					Console.WriteLine(myStringOutput);
				}
				Console.WriteLine("----------------");

				string rawMapString = String.Join("", _rawMap.Select(p => p.ToString("X2")).ToArray());
				Console.WriteLine(rawMapString);
			}

			public void WalkableDump()
			{
				for (int i = 0; i < (_dimensions.Item2); i++)
				{
					var tempmap = _maparray[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)].Select(x => ((_tiledata[(x & 0x7F)*2] & 0x07) == 0x07) ? 0xFF : (_tiledata[(x & 0x7F) * 2] & 0x0F));

					string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

					Console.WriteLine(myStringOutput);
				}
			}
			public byte WalkableByte(int x, int y)
			{ 
				return (byte)(_tiledata[(_maparray[(y * _dimensions.Item1) + x] & 0x7F) * 2] & 0x07);
			}
			public bool IsScriptTile(int x, int y)
			{
				return (_tiledata[((_maparray[(y * _dimensions.Item1) + x] & 0x7F) * 2) + 1] & 0x80) == 0x80;
			}
			public byte TileValue(int x, int y)
			{
				return (byte)(_maparray[(y * _dimensions.Item1) + x] & 0x7F);
			}
			private byte tileconverter(byte[] tiledata)
			{
				if ((tiledata[0] & 0x07) == 0x07)
				{
					return 0xFF;
				}
				else if ((tiledata[0] & 0x07) == 0x0B)
				{
					return 0xC0;
				}
				else if ((tiledata[0] & 0x07) == 0x00 && tiledata[1] > 0x95 && tiledata[1] < 0x9a)
				{
					return 0xFF;
				}
				else
				{
					return (byte)(tiledata[0] & 0x07);
				}
			}
			public void CreateAreas()
			{
				var tempmap = _maparray[0..(_dimensions.Item1 * _dimensions.Item2)].Select(x => tileconverter(new byte[] { _tiledata[(x & 0x7F) * 2], _tiledata[((x & 0x7F) * 2) + 1] })).ToArray();

				byte marker = 0x10;
				//int start = 0;

				while (true)
				{
					var start = tempmap.ToList().FindIndex(x => x < 0x10);
					if (start == -1)
					//if (start > 214)
						break;

					var start_y = start / _dimensions.Item1;
					var start_x = start % _dimensions.Item1;

					Fill(start_x, start_y, tempmap[start_y * _dimensions.Item1 + start_x], tempmap, marker);
					marker++;
				}

				for (int i = 0; i < (_dimensions.Item2); i++)
				{
					string myStringOutput = String.Join("", tempmap[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)].Select(p => p == 0xFF ? "██" : p.ToString("X2")).ToArray());

					Console.WriteLine(myStringOutput);
				}
			}
			private void Fill(int x, int y, int origin, byte[] map, byte marker)
			{
				if (x < 0)
					x = (_dimensions.Item1 - 1);
				if (x >= _dimensions.Item1)
					x = 0;

				if (y < 0)
					y = (_dimensions.Item2 - 1);
				if (y >= _dimensions.Item2)
					y = 0;

				if (!IsInside((byte)map[y * _dimensions.Item1 + x], (byte)origin))
					return;

				int originback;

				if ((map[y * _dimensions.Item1 + x] & 0xC0) == 0xC0)
				{
					originback = origin;
					map[y * _dimensions.Item1 + x] |= BitConverter[origin];
				}
				else
				{
					originback = map[y * _dimensions.Item1 + x];
					map[y * _dimensions.Item1 + x] = marker;
				}

				Fill(x, y + 1, originback, map, marker);
				Fill(x, y - 1, originback, map, marker);
				Fill(x + 1, y, originback, map, marker);
				Fill(x - 1, y, originback, map, marker);
			}
			private bool IsInside(byte tile, byte origin)
			{
				if (tile == 0xFF)
					return false;
				else if (tile < 0xC0 && tile > 0x0F)
					return false;
				else if ((tile & 0xC0) == 0xC0 && (BitConverter[origin] & tile) == 0)
					return true;
				else if (origin == 0x00 || tile == 0x00)
					return true;
				else if (tile == origin)
					return true;
				else
					return false;
			}
			public void ChestLocationDump(ObjectList mapobjects)
			{
				var tempmap = _maparray[0..(_dimensions.Item1 * _dimensions.Item2)].Select(x => ((_tiledata[(x & 0x7F) * 2] & 0x07) == 0x07) ? 0xFF : 0x00).ToArray();

				for (int i = 0; i < 0x6B; i++)
				{
					if (mapobjects.GetAreaMapId(i) == _mapId)
					{
						var validchests = mapobjects.Chests.Where(x => x.AreaId == i).ToList();

						foreach (var chest in validchests)
						{
							tempmap[chest.Y * _dimensions.Item1 + chest.X] = (byte)chest.TreasureId;
						}
					}
				}

				for (int i = 0; i < (_dimensions.Item2); i++)
				{
					string myStringOutput = String.Join("", tempmap[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)].Select(p => p == 0x00 ? "__" : p == 0xFF ? "██" : p.ToString("X2")).ToArray());

					Console.WriteLine(myStringOutput);
				}
			}

			public void ExitLocationDump(ExitList exits, MapUtilities maputilities)
			{
				var tempmap = _maparray[0..(_dimensions.Item1 * _dimensions.Item2)].Select(x => ((_tiledata[(x & 0x7F) * 2] & 0x07) == 0x07) ? 0xFF : 0x00).ToArray();

				var areaexittile = exits.GetAreaExitTiles(_mapId, maputilities);


				foreach (var exittile in areaexittile)
				{
					if ((exittile.Y * _dimensions.Item1 + exittile.X) < (_dimensions.Item2 * _dimensions.Item1))
					{
						tempmap[exittile.Y * _dimensions.Item1 + exittile.X] = (byte)exittile.TargetExit;
					}
				}

				for (int i = 0; i < (_dimensions.Item2); i++)
				{
					string myStringOutput = String.Join("", tempmap[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)].Select(p => p == 0x00 ? "__" : p == 0xFF ? "██" : p.ToString("X2")).ToArray());

					Console.WriteLine(myStringOutput);
				}
			}
		}
	}
	public class MapChanges
	{
		private List<Blob> _pointers;
		private List<Blob> _mapchanges;


		public MapChanges(FFMQRom rom)
		{
			_pointers = rom.GetFromBank(RomOffsets.MapChangesBankOld, RomOffsets.MapChangesPointersOld, RomOffsets.MapChangesQtyOld * 2).Chunk(2);
			_mapchanges = new List<Blob>();

			foreach (var pointer in _pointers)
			{
				var test = pointer.ToUShorts()[0];
				var sizeByte = rom.GetFromBank(RomOffsets.MapChangesBankOld, RomOffsets.MapChangesEntriesOld + pointer.ToUShorts()[0] + 2, 1)[0];
				var size = (sizeByte & 0x0F) * (sizeByte / 0x10);
				_mapchanges.Add(rom.GetFromBank(RomOffsets.MapChangesBankOld, RomOffsets.MapChangesEntriesOld + pointer.ToUShorts()[0], size + 3));
			}
		}
		public byte Add(Blob mapchange)
		{
			if (_mapchanges.Count() >= RomOffsets.MapChangesQtyNew)
			{
				throw new Exception("Too many map changes.");
			}

			var newpointer = _pointers.Last().ToUShorts()[0] + _mapchanges.Last().Length;
			_pointers.Add(new byte[] { (byte)(newpointer % 0x100), (byte)(newpointer / 0x100) });
			_mapchanges.Add(mapchange);
			return (byte)(_mapchanges.Count() - 1);
		}
		public void Modify(int index, int address, byte modification)
		{
			_mapchanges[index][address] = modification;
		}
		public void Replace(int index, Blob mapchange)
		{
			_mapchanges[index] = mapchange;
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(RomOffsets.MapChangesBankNew, RomOffsets.MapChangesPointersNew, _pointers.SelectMany(x => x.ToBytes()).ToArray());
			rom.PutInBank(RomOffsets.MapChangesBankNew, RomOffsets.MapChangesEntriesNew, _mapchanges.SelectMany(x => x.ToBytes()).ToArray());

			// Change LoadMapChange routine
			rom.PutInBank(0x01, 0xC593, Blob.FromHex("008012")); // Change pointers table address
			rom.PutInBank(0x01, 0xC5A0, Blob.FromHex("018112")); // Change Y base
			rom.PutInBank(0x01, 0xC5B6, Blob.FromHex("008112")); // Change X base
			rom.PutInBank(0x01, 0xC5CB, Blob.FromHex("028112")); // Change Size base
			rom.PutInBank(0x01, 0xC5EB, Blob.FromHex("008112")); // Change Entry base
		}
	}
}
