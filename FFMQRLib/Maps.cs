using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using static System.Math;

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

	public class GameMaps
	{
		private List<Map> _gameMaps;
		public TilesProperties TilesProperties { get; set; }
		public GameMaps(FFMQRom rom)
		{
			TilesProperties = new TilesProperties(rom);
			_gameMaps = new();

			for (int i = 0; i < 0x2C; i++)
			{
				_gameMaps.Add(new Map(i, TilesProperties, rom));
			}
		}

		public Map this[int mapID]
		{
			get => _gameMaps[mapID];
			set => _gameMaps[mapID] = value;
		}
		public void RandomGiantTreeMessage(MT19337 rng)
		{
			Dictionary<char, List<List<byte>>> letters = new()
			{
				{
					'A',
					new List<List<byte>> { 
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'B',
					new List<List<byte>> { 
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'C',
					new List<List<byte>> { 
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'D',
					new List<List<byte>> { 
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },}
				},
				{
					'E',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'F',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },}
				},
				{
					'G',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'H',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'I',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },}
				},
				{
					'J',
					new List<List<byte>> {
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'K',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'L',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'M',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'N',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'O',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'P',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },}
				},
				{
					'Q',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x03, 0x00 },}
				},
				{
					'R',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'S',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'T',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },}
				},
				{
					'U',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'V',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },}
				},
				{
					'W',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'X',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },}
				},
				{
					'Y',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x00, 0x03, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },}
				},
				{
					'Z',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x03, 0x00, 0x00, 0x00 },
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },}
				},
				{
					'!',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x03, 0x00 },}
				},
				{
					'.',
					new List<List<byte>> {
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x03, 0x00 },}
				},
				{
					'?',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x03, 0x03, 0x00 },
					new List<byte> { 0x00, 0x00, 0x03, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },
					new List<byte> { 0x00, 0x00, 0x00, 0x00 },
					new List<byte> { 0x00, 0x03, 0x00, 0x00 },}
				},
				{
					'\'',
					new List<List<byte>> {
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x03, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },}
				},
				{
					' ',
					new List<List<byte>> {
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },
					new List<byte> { 0x00, 0x00 },}
				},
			};


			List<string> customMessages = new()
			{
				"GO ON+KID!!", // original
				"BORK?+BORK", // wildham
			};

			string newMessage = rng.PickFrom(customMessages);

			List<int> xPositions = new() { 0x09, 0x11 };
			List<int> yPositions = new() { 0x03, 0x09 };

			int currentYindex = 0;
			int currentX = xPositions[currentYindex];

			foreach (char c in newMessage)
			{
				if (c == '+')
				{
					currentYindex++;
					currentX = xPositions[currentYindex];
					continue;
				}

				_gameMaps[(int)MapList.BackgroundD].ModifyMap(currentX, yPositions[currentYindex], letters[c]);

				currentX += letters[c][0].Count;
			}
		}
		public void Write(FFMQRom rom)
		{
			List<int> validBanks = new() { 0x08, 0x13 };
			int currentBank = 0;
			int currentAddress = 0x8000;

			List<byte> newPointersTable = new();



			foreach (var map in _gameMaps)
			{
				if (map.ModifiedMap)
				{
					map.CompressMap();
				}

				if (currentAddress + map.CompressedMapSize > 0xFFFF)
				{
					currentBank++;
					currentAddress = 0x8000;
				}

				newPointersTable.AddRange(new List<byte>() { (byte)(currentAddress % 0x100), (byte)(currentAddress / 0x100), (byte)validBanks[currentBank] });

				map.Write(rom, validBanks[currentBank], currentAddress);
				currentAddress += map.CompressedMapSize;
			}

			rom.Put(RomOffsets.MapDataAddresses, newPointersTable.ToArray());

			TilesProperties.Write(rom);
		}
	}

	public class Map
	{
		private int _mapAddress;
		private byte[] _mapAddressRaw;
		private int _referenceTableAddress;
		private byte[] _referenceTableAddressRaw;
		private (int, int) _dimensions;
		private byte[] _mapAttributes = new byte[0x0A];
		private int _mapId;
		private List<byte> _mapUncompressed;
		private List<byte> _mapCompressedData;
		private List<SingleTile> _tileData;
		public bool ModifiedMap { get; set; }

		public int CompressedMapSize => _mapCompressedData.Count;

		public static readonly byte[] BitConverter = { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };

		public Map(int mapID, TilesProperties tileprop, FFMQRom rom)
		{
			_mapId = mapID;
			_mapAttributes = rom.Get(RomOffsets.MapAttributes + mapID * 0x0A, 0x0A).ToBytes();
			_mapAddressRaw = rom.Get(RomOffsets.MapDataAddresses + (mapID * 3), 3);
			_mapAddress = _mapAddressRaw[2] * 0x8000 + (_mapAddressRaw[1] * 0x100 + _mapAddressRaw[0] - 0x8000);
			_referenceTableAddressRaw = rom.Get(_mapAddress, 2);
			_referenceTableAddress = _mapAddress + 2 + _referenceTableAddressRaw[1] * 0x100 + _referenceTableAddressRaw[0];
			_tileData = tileprop[_mapAttributes[0] & 0x0F];

			_mapCompressedData = new();
			_mapUncompressed = new();

			ModifiedMap = false;

			var tempdimensions = rom.Get(RomOffsets.MapDimensionsTable + (_mapAttributes[0] & 0xF0) / 8, 2);
			_dimensions = (tempdimensions[0], tempdimensions[1]);

			List<byte> tempReferenceTable = new();

			var refChunkPosition = _referenceTableAddress;
			var mapPosition = 0;


			bool decompressionIsOnGoing = true;
			int currrentPosition = _mapAddress + 2;
			_mapCompressedData.AddRange(_referenceTableAddressRaw);

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
		public (int, int) Seek(List<int> validPositions, int offset, int currentposition)
		{
			int bestCandidate = validPositions.First();

			if (offset >= 0x11 || currentposition + offset >= _mapUncompressed.Count)
			{
				return (bestCandidate, offset);
			}

			List<int> tempPositions = new(validPositions);

			foreach (int position in tempPositions)
			{
				if (_mapUncompressed[currentposition - position - 1 + offset] != _mapUncompressed[currentposition + offset])
				{
					validPositions.Remove(position);
				}
			}

			if (validPositions.Any())
			{
				return Seek(validPositions, offset + 1, currentposition);
			}
			else
			{
				return (bestCandidate, offset);
			}
		}
		public void CompressMap()
		{
			List<int> validPositionsTemplate = Enumerable.Range(0, 0x100).ToList();

			int currentposition = 1;

			List<ZipAction> ActionsList = new();

			bool writeChunkBuffer = false;
			bool delayChunkWrite = false;
			bool writeOrphanChunk = false;
			int tempChunkSize = 1;
			int tempChunkAddress = 0;
			List<byte> referenceChunks = new();

			while (currentposition < _dimensions.Item1 * _dimensions.Item2)
			{
				if (writeChunkBuffer && !delayChunkWrite)
				{
					byte[] newChunk = new byte[tempChunkSize];
					Array.Copy(_mapUncompressed.ToArray(), tempChunkAddress, newChunk, 0, tempChunkSize);

					referenceChunks.AddRange(_mapUncompressed.GetRange(tempChunkAddress, tempChunkSize).ToList());

					if (ActionsList.Last().ChunkLength > 0 && writeOrphanChunk)
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

				List<int> validPositions;

				if (currentposition > 0x100)
				{
					validPositions = new(validPositionsTemplate);
				}
				else
				{
					validPositions = new(validPositionsTemplate.Where(x => x < currentposition));
				}

				(int, int) result = Seek(validPositions, 0, currentposition);

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
		public void ModifyMap(int destx, int desty, byte modifications)
		{
			_mapUncompressed[destx + (desty * _dimensions.Item1)] = modifications;

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
				var tempmap = _mapUncompressed.GetRange((i * _dimensions.Item1), _dimensions.Item1).Select(x => ((_tileData[(x & 0x7F)].Byte1 & 0x07) == 0x07) ? 0xFF : (_tileData[(x & 0x7F)].Byte1 & 0x0F));

				string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

				Console.WriteLine(myStringOutput);
			}
		}
		public byte WalkableByte(int x, int y)
		{
			return (byte)(_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].Byte1 & 0x07);
		}
		public bool IsScriptTile(int x, int y)
		{
			return (_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].Byte2 & 0x80) == 0x80;
		}
		public byte TileValue(int x, int y)
		{
			return (byte)(_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F);
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
			var tempmap = _mapUncompressed.GetRange(0,(_dimensions.Item1 * _dimensions.Item2)).Select(x => tileconverter(new byte[] { _tileData[(x & 0x7F)].Byte1, _tileData[x & 0x7F].Byte2 })).ToArray();

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
		public void ChestLocationDump(FFMQRom.ObjectList mapobjects)
		{
			var tempmap = _mapUncompressed.GetRange(0, _dimensions.Item1 * _dimensions.Item2).Select(x => ((_tileData[(x & 0x7F)].Byte1 & 0x07) == 0x07) ? 0xFF : 0x00).ToArray();
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
