using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using System.Diagnostics;
using static System.Math;
using System.Xml.Linq;
using System.Text.Json;
using System.IO;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using System.Drawing;

namespace FFMQLib
{
	public class MapAttributes
	{ 
		public int TilesProperties { get; set; }
		public List<byte> GraphicRows { get; set; }
        public int MapDimensionId { get; set; }
        public byte Palette { get; set; }
		//private int length = 0x0A;
		public MapAttributes(byte[] data)
		{
			TilesProperties = (data[0] & 0x0F);
            MapDimensionId = (data[0] & 0xF0) / 16;
			Palette = data[1];
			GraphicRows = data.Take(new Range(2, 10)).ToList();
        }
		public MapAttributes()
		{
			GraphicRows = new();
		}
		public byte[] ToArray()
		{
			return new byte[] {	(byte)((MapDimensionId * 16) | (TilesProperties & 0x0F)), Palette }.Concat(GraphicRows.Concat(Enumerable.Repeat((byte)0xFF, 8)).Take(8)).ToArray();
		}
	}

	public class JsonMap
	{ 
		public string Map { get; set; }
		public MapAttributes Attributes { get; set; }

		public JsonMap()
		{
			Attributes = new();
		}
		public JsonMap(MapAttributes mapattributes, string maptiles)
		{
			Attributes = new MapAttributes(mapattributes.ToArray());
			Map = maptiles;
		}
		public byte[] GetMapBytes()
		{
			return Convert.FromBase64String(Map);
		}
	}

	public class Map
	{
		//private int _mapAddress;
		private (int, int) _dimensions;
		private int _mapId;
		private List<byte> _mapUncompressed;
		private List<byte> _mapCompressedData;
		private List<SingleTile> _tileData;
		public MapAttributes Attributes { get; set; }
		public bool ModifiedMap { get; set; }

		public int CompressedMapSize => _mapCompressedData.Count;
		public int SizeX => _dimensions.Item1;
		public int SizeY => _dimensions.Item2;

		public static readonly byte[] BitConverter = { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
		private const int MapDataAddressesOffset = 0x058735;
		//private const int MapAttributesOffset = 0x058CD9;
		private const int MapDimensionsTableOffset = 0x058540;


		public Map(int mapID, (int x, int y) dimensions, TilesProperties tileprop, MapAttributes attributes, int bank, int offset, FFMQRom rom)
		{
			Attributes = attributes;

            _mapId = mapID;
			_tileData = tileprop[Attributes.TilesProperties];

			ModifiedMap = false;

			_dimensions = dimensions;
			UncompressMapData(bank, offset, rom);
        }
		public JsonMap ConvertToJson()
		{
			return new JsonMap(Attributes, Convert.ToBase64String(_mapUncompressed.ToArray()));
		}
		public void LoadJson(string filename)
		{
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
			JsonMap mapdata;
			//StreamReader mapfile = new StreamReader(assembly.GetManifestResourceStream(filepath), Encoding.UTF8);

			using (StreamReader mapfile = new StreamReader(assembly.GetManifestResourceStream(filepath), Encoding.UTF8))
			{
				mapdata = JsonSerializer.Deserialize<JsonMap>(mapfile.ReadToEnd());
			}

			Attributes = mapdata.Attributes;
			_mapUncompressed = mapdata.GetMapBytes().ToList();
			ModifiedMap = true;

		}
		private void UncompressMapData(int bank, int offset, FFMQRom rom)
		{
            _mapCompressedData = new();
            _mapUncompressed = new();

            List<byte> tempReferenceTable = new();

			var longDataAdress = bank * 0x8000 + offset - 0x8000;
			var refAddress = rom.GetFromBank(bank, offset, 2).ToBytes();
            var refChunkPosition = longDataAdress + 2 + refAddress[1] * 0x100 + refAddress[0]; 
			
            var mapPosition = 0;

            bool decompressionIsOnGoing = true;
            int currrentPosition = longDataAdress + 2;
            _mapCompressedData.AddRange(refAddress);

            while (decompressionIsOnGoing)
            {
                var currentAction = rom.Get(currrentPosition, 2);

                var chunckLength = currentAction[0] & 0x0F;
                if (chunckLength > 0)
                {
                    var refChunk = rom.Get(refChunkPosition, chunckLength);
                    _mapUncompressed.AddRange(refChunk.ToBytes());
                    tempReferenceTable.AddRange(refChunk.ToBytes());
                    refChunkPosition += chunckLength;
                    mapPosition += chunckLength;
                }

                var higherbits = (currentAction[0] & 0xF0) / 16;
                if (higherbits > 0)
                {
                    var targetPosition = mapPosition - currentAction[1] - 1;

                    for (int j = 0; j < higherbits + 2; j++)
                    {
                        _mapUncompressed.Add(_mapUncompressed[targetPosition]);
                        mapPosition++;
                        targetPosition++;
                    }

                    currrentPosition += 2;
                    _mapCompressedData.AddRange(currentAction.ToBytes());
                }
                else if (currentAction[0] == 0x00)
                {
                    decompressionIsOnGoing = false;
                    _mapCompressedData.Add(0x00);
                }
                else
                {
                    currrentPosition++;
                    _mapCompressedData.Add(currentAction[0]);
                }
            }

            _mapCompressedData.AddRange(tempReferenceTable);
            _mapCompressedData.Add(0x00);
        }

		public class ZipAction
		{
			public byte CopyLength { get; set; }
			public byte ChunkLength { get; set; }
			public byte Offset { get; set; }

			public ZipAction(byte copy, byte chunk, byte offset)
			{
				CopyLength = copy;
				ChunkLength = chunk;
				Offset = offset;
			}

			public byte[] GetBytes()
			{
				if (CopyLength == 0x00)
				{
					return new byte[] { ChunkLength };
				}
				else
				{
					return new byte[] { (byte)((Max(0, (CopyLength - 2)) * 0x10) | ChunkLength), Offset };
				}
			}
		}
		public (int, int) Seek(int offset, int currentposition, int searchOffset)
		{
			if (offset >= 0x11 || currentposition + offset >= _mapUncompressed.Count)
			{
				return (searchOffset, offset);
			}

			if (_mapUncompressed[currentposition - searchOffset - 1 + offset] != _mapUncompressed[currentposition + offset])
			{
				return (searchOffset, offset);
			}
			else
			{
				return Seek(offset + 1, currentposition, searchOffset);
			}
		}
		public (int, int) SeekInitialization(int offset, int currentposition)
		{
			(int, int) bestResult = (0, 0);

			int maxPosition = Math.Min(0x100, currentposition);

			for(int i = 0; i < maxPosition; i++)
			{
				var result = Seek(offset, currentposition, i);
				if (result.Item2 >= 0x11)
				{
					bestResult = result;
					break;
				}
				else if (result.Item2 > bestResult.Item2)
				{
					bestResult = result;
				}
			}

			return bestResult;
		}
		public void CompressMap()
		{
			int currentposition = 1;

			List<ZipAction> ActionsList = new();

			bool writeChunkBuffer = false;
			bool delayChunkWrite = false;
			bool writeOrphanChunk = false;
			bool keepCompressing = true;
			int tempChunkSize = 1;
			int tempChunkAddress = 0;
			List<byte> referenceChunks = new();

			//while (currentposition < _dimensions.Item1 * _dimensions.Item2 || writeChunkBuffer != false)
			while (keepCompressing)
			{
				if (currentposition >= ((_dimensions.Item1 * _dimensions.Item2)))
				{
					keepCompressing = false;
					if(tempChunkSize > 0 && !writeChunkBuffer)
					{
						writeChunkBuffer = true;
						writeOrphanChunk = true;
						delayChunkWrite = false;
					}
				}

				if (writeChunkBuffer && !delayChunkWrite)
				{
					byte[] newChunk = new byte[tempChunkSize];
					Array.Copy(_mapUncompressed.ToArray(), tempChunkAddress, newChunk, 0, tempChunkSize);

					referenceChunks.AddRange(_mapUncompressed.GetRange(tempChunkAddress, tempChunkSize).ToList());

					if (writeOrphanChunk)
					{
						ActionsList.Add(new ZipAction(0, (byte)tempChunkSize, 0));
						writeOrphanChunk = false;
					}
					else
					{
						ActionsList.Last().ChunkLength = (byte)tempChunkSize;
					}
					tempChunkSize = 0;
					writeChunkBuffer = false;
				}

				if (!keepCompressing)
				{
					break;
				}

				(int, int) result = SeekInitialization(0, currentposition);

				if (result.Item2 > 2)
				{
					ActionsList.Add(new ZipAction((byte)result.Item2, 0, (byte)result.Item1));
					currentposition += result.Item2;
					if (tempChunkSize > 0)
					{
						writeChunkBuffer = true;
						delayChunkWrite = false;
					}
					continue;
				}

				if (delayChunkWrite)
				{
					delayChunkWrite = false;
					writeOrphanChunk = true;
					writeChunkBuffer = true;
					continue;
				}

				if (tempChunkSize > 0)
				{
					tempChunkSize++;
					currentposition++;
					if (tempChunkSize >= 0x0F)
					{
						delayChunkWrite = true;
					}
				}
				else
				{
					tempChunkSize++;
					tempChunkAddress = currentposition;
					currentposition++;
				}
			}

			ActionsList.Add(new ZipAction(0, 0, 0));
			List<byte> finalResult = ActionsList.SelectMany(x => x.GetBytes()).ToList();
			finalResult.InsertRange(0, Blob.FromUShorts(new ushort[] { (ushort)finalResult.Count }).ToBytes());
			finalResult.AddRange(referenceChunks);
			finalResult.Add(0x00);

			_mapCompressedData = finalResult;
		}
		public void Write(FFMQRom rom, int bank, int address)
		{
			rom.PutInBank(bank, address, _mapCompressedData.ToArray());
		}
		public byte[] GetArray()
		{
			return _mapCompressedData.ToArray();
		}
		public void ModifyMap(int destx, int desty, List<List<byte>> modifications)
		{
			for (int y = 0; y < modifications.Count; y++)
			{
				for (int x = 0; x < modifications[y].Count; x++)
				{
					_mapUncompressed[(destx + x) + ((desty + y) * _dimensions.Item1)] = modifications[y][x];
				}
			}

			ModifiedMap = true;
		}
		public void ModifyMap(int destx, int desty, byte modifications, bool keepLayerData = false)
		{

			if (!keepLayerData)
			{
				_mapUncompressed[destx + (desty * _dimensions.Item1)] = modifications;
			}
			else
			{
				var layervalue = _mapUncompressed[destx + (desty * _dimensions.Item1)] & 0x80;
				_mapUncompressed[destx + (desty * _dimensions.Item1)] = (byte)(modifications | layervalue);
			}

			ModifiedMap = true;
		}
		public void ReplaceAll(byte originaltile, byte newtile)
		{
			for (int i = 0; i < _mapUncompressed.Count; i++)
			{
				if (_mapUncompressed[i] == originaltile)
				{
					_mapUncompressed[i] = newtile;
				}
			}

			ModifiedMap = true;
		}
		public void RandomReplaceTile(MT19337 rng, byte originTile, byte replaceTile, float ratio)
		{
			var tileList = _mapUncompressed.Select((value, index) => new { value, index })
										.Where(x => x.value == originTile)
										.Select(x => x.index)
										.ToList();

			tileList.Shuffle(rng);
			tileList = tileList.GetRange(0, (int)(tileList.Count * ratio));

			foreach (var tile in tileList)
			{
				_mapUncompressed[tile] = replaceTile;
			}
			
			ModifiedMap = true;
		}
		public void DataDump()
		{
			for (int i = 0; i < (_dimensions.Item2); i++)
			{
				var tempmap = _mapUncompressed.GetRange((i * _dimensions.Item1), _dimensions.Item1);

				string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

				Console.WriteLine(myStringOutput);
			}
			Console.WriteLine("----------------");

			string rawMapString = String.Join("", _mapCompressedData.Select(p => p.ToString("X2")).ToArray());
			Console.WriteLine(rawMapString);
		}
		public void WalkableDump()
		{
			for (int i = 0; i < (_dimensions.Item2); i++)
			{
				var tempmap = _mapUncompressed.GetRange((i * _dimensions.Item1), _dimensions.Item1).Select(x => ((_tileData[(x & 0x7F)].PropertyByte1 & 0x07) == 0x07) ? 0xFF : (_tileData[(x & 0x7F)].PropertyByte1 & 0x0F));

				string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

				Console.WriteLine(myStringOutput);
			}
		}
		public byte WalkableByte(int x, int y)
		{
			return (byte)(_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].PropertyByte1 & 0x07);
		}
		public byte this[int x, int y]
		{
			get => (byte)(_mapUncompressed[(y * _dimensions.Item1) + x]);
		}
		public bool IsScriptTile(int x, int y)
		{
			return (_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].PropertyByte2 & 0x80) == 0x80;
		}
		public byte TileValue(int x, int y)
		{
			return (byte)(_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F);
		}
		public void Analysis()
		{
			var individualtile = _mapUncompressed.Distinct().OrderBy(x => x).ToList();

			foreach (var tile in individualtile)
			{ 
				Console.WriteLine(tile);
			}
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
			var tempmap = _mapUncompressed.GetRange(0,(_dimensions.Item1 * _dimensions.Item2)).Select(x => tileconverter(new byte[] { _tileData[(x & 0x7F)].PropertyByte1, _tileData[x & 0x7F].PropertyByte2 })).ToArray();

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
		public void ChestLocationDump(FFMQLib.Areas mapobjects)
		{
			var tempmap = _mapUncompressed.GetRange(0, _dimensions.Item1 * _dimensions.Item2).Select(x => ((_tileData[(x & 0x7F)].PropertyByte1 & 0x07) == 0x07) ? 0xFF : 0x00).ToArray();
			/*
			for (int i = 0; i < 0x6B; i++)
			{
				if (mapobjects.GetAreaMapId(i) == _mapId)
				{
					var validchests = mapobjects.ChestList.Where(x => x.Item2 == i).ToList();

					foreach (var chest in validchests)
					{
						tempmap[chest.Y * _dimensions.Item1 + chest.X] = (byte)chest.TreasureId;
					}
				}
			}*/

			for (int i = 0; i < (_dimensions.Item2); i++)
			{
				string myStringOutput = String.Join("", tempmap[(i * _dimensions.Item1)..((i + 1) * _dimensions.Item1)].Select(p => p == 0x00 ? "__" : p == 0xFF ? "██" : p.ToString("X2")).ToArray());

				Console.WriteLine(myStringOutput);
			}
		}
		public List<byte> UsedBytes()
		{
			return _mapUncompressed.Distinct().ToList();
		}
		/*
		public void ExitLocationDump(FFMQRom.ExitList exits, MapUtilities maputilities)
		{
			var tempmap = _mapUncompressed.GetRange(0, _dimensions.Item1 * _dimensions.Item2).Select(x => ((_tileData[(x & 0x7F)].Byte1 & 0x07) == 0x07) ? 0xFF : 0x00).ToArray();

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
		}*/
	}

	public class RLEMap
	{
		private List<byte> _compressedMap;
        private List<byte> _uncompressedMap;
		private (int x, int y) _dimensions;
		private ushort _length;
		public RLEMap((int x, int y) dimensions, int bank, int offset, FFMQRom rom)
		{
			_dimensions = dimensions;
            _length = rom.GetFromBank(bank, offset, 2).ToUShorts()[0];
			_compressedMap = rom.GetFromBank(bank, offset + 2, _length).ToBytes().ToList();
			_uncompressedMap = new();

            Uncompress();
        }

		private void Uncompress()
		{
			int currentposition = 0;

			while (currentposition < _length)
			{
				byte currentbyte = _compressedMap[currentposition];

				if ((currentbyte & 0x80) > 0)
				{
					currentbyte &= 0x7F;
					currentposition++;
					byte bytelength = _compressedMap[currentposition];

					for (int i = 0; i < (bytelength + 3); i++)
					{
                        _uncompressedMap.Add(currentbyte);
                    }
					currentposition++;
                }
				else
				{
					_uncompressedMap.Add(currentbyte);
					currentposition++;
                }
            }
		}

        public void Compress()
        {
			_compressedMap = new();
            int currentposition = 0;
			int runningCount = 1;

            while (currentposition < _uncompressedMap.Count)
            {
                byte currentbyte = _uncompressedMap[currentposition];
				byte nextbyte = 0xFF;

                if (currentposition < (_uncompressedMap.Count - 1))
				{
                    nextbyte = _uncompressedMap[currentposition + 1];
                }

				if (currentbyte == nextbyte && runningCount < (0xFF + 3))
				{
					runningCount++;
					currentposition++;
					continue;
				}
				else
				{
					// write
					if (runningCount >= 3)
					{
						_compressedMap.Add((byte)(currentbyte | 0x80));
						_compressedMap.Add((byte)(runningCount - 3));
						currentposition++;
						runningCount = 1;
						continue;
					}
					else
					{
						for (int i = 0; i < runningCount; i++)
						{
							_compressedMap.Add(currentbyte);
						}

                        currentposition++;
                        runningCount = 1;
                        continue;
                    }
				}
			}

			_length = (ushort)_compressedMap.Count;
        }
        public void ModifyMap(int destx, int desty, List<List<byte>> modifications)
        {
            for (int y = 0; y < modifications.Count; y++)
            {
                for (int x = 0; x < modifications[y].Count; x++)
                {
                    _uncompressedMap[(destx + x) + ((desty + y) * _dimensions.x)] = modifications[y][x];
                }
            }
        }
        public void ModifyMap(int destx, int desty, byte modifications, bool keepLayerData = false)
        {
            if (!keepLayerData)
            {
                _uncompressedMap[destx + (desty * _dimensions.x)] = modifications;
            }
            else
            {
                var layervalue = _uncompressedMap[destx + (desty * _dimensions.x)] & 0x80;
                _uncompressedMap[destx + (desty * _dimensions.x)] = (byte)(modifications | layervalue);
            }
        }
		public void DrawRow(int desty, int startx, int length, byte tile)
		{
			for (int i = 0; i < length; i++)
			{
                _uncompressedMap[startx + i + (desty * _dimensions.x)] = tile;
            }
		}
        public void DrawColumn(int destx, int starty, int length, byte tile)
        {
            for (int i = 0; i < length; i++)
            {
                _uncompressedMap[destx + ((starty + i) * _dimensions.x)] = tile;
            }
        }
        public byte[] GetArray()
		{
			return Blob.FromUShorts(new ushort[] { _length }) + _compressedMap.ToArray();
		}
    }
	public class MapUtilities
	{
		private List<Blob> _areaattributes = new();

		private const int AreaAttributesPointers = 0x3AF3B;
		private const int AreaAttributesPointersBase = 0x3B013;
		private const int AreaAttributesPointersQty = 108;
		public MapUtilities(FFMQRom rom)
		{
			var attributepointers = rom.Get(AreaAttributesPointers, AreaAttributesPointersQty * 2).Chunk(2);

			foreach (var pointer in attributepointers)
			{
				var address = AreaAttributesPointersBase + pointer[1] * 0x100 + pointer[0];
				_areaattributes.Add(rom.Get(address, 8));
			}
		}
		public byte AreaIdToMapId(byte areaid)
		{
			return _areaattributes[(int)areaid][1];
		}
	}
}
