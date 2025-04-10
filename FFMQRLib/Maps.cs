﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using System.Diagnostics;
using static System.Math;
using System.Xml.Linq;

namespace FFMQLib
{
	public class TilesProperties
	{
		private List<List<SingleTile>> _tilesProperties;
		private const int MapTileDataOffset = 0x032800;


		public TilesProperties(FFMQRom rom)
		{
			_tilesProperties = rom.Get(MapTileDataOffset, 0x10 * 0x100).Chunk(0x100).Select(x => x.Chunk(0x02).Select(y => new SingleTile(y)).ToList()).ToList();
		}

		public List<SingleTile> this[int propTableID]
		{
			get => _tilesProperties[propTableID];
			set => _tilesProperties[propTableID] = value;
		}

		public void Write(FFMQRom rom)
		{
			rom.Put(MapTileDataOffset, _tilesProperties.SelectMany(x => x.SelectMany(y => y.GetBytes())).ToArray());
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
        private const int MapDataAddressesOffset = 0x8735;
        private const int MapDataAddressesBank = 0x0B;
        private const int MapAttributesOffset = 0x8CD9;
        private const int MapAttributesBank = 0x0B;
        //private const int MapAttributesQty = 0x0A;
        private const int MapAttributesLength = 0x0A;
        private const int MapDimensionsTableOffset = 0x8540;
        private const int MapDimensionsTableBank = 0x0B;

		private const int CloudMapBank = 0x07;
		private const int CloudMapOffset = 0xF6D1;
        private const int NewCloudMapBank = 0x13;
        private const int NewCloudMapOffset = 0xE000;
        public RLEMap CloudsMap { get; set; }
        public GameMaps(FFMQRom rom)
		{
			TilesProperties = new TilesProperties(rom);
			_gameMaps = new();
			var mapAttributesData = rom.GetFromBank(MapAttributesBank, MapAttributesOffset, MapAttributesLength * 0x2C).Chunk(MapAttributesLength).Select(a => new MapAttributes(a)).ToList();
            var mapDataPointers = rom.GetFromBank(MapDataAddressesBank, MapDataAddressesOffset, 3 * 0x2C).Chunk(3);
            var dimensions = rom.GetFromBank(MapDimensionsTableBank, MapDimensionsTableOffset, 2 * 16).Chunk(2);
            //byte[] _mapAddressRaw = rom.Get(MapDataAddressesOffset + (mapID * 3), 3);
            // _mapAddress = _mapAddressRaw[2] * 0x8000 + (_mapAddressRaw[1] * 0x100 + _mapAddressRaw[0] - 0x8000);
            //var tempdimensions = rom.Get(MapDimensionsTableOffset + (Attributes.MapDimensionId * 2), 2);

            //var tempdimensions = rom.Get(MapDimensionsTableOffset + (_mapAttributes[0] & 0xF0) / 8, 2);
            //_dimensions = (tempdimensions[0], tempdimensions[1]);

            for (int i = 0; i < 0x2C; i++)
			{
                //var tempdimensions = rom.GetFromBank(MapDimensionsTableOffset + (Attributes.MapDimensionId * 2), 2);

                _gameMaps.Add(new Map(i,
					(dimensions[mapAttributesData[i].MapDimensionId][0], dimensions[mapAttributesData[i].MapDimensionId][1]),
					TilesProperties,
					mapAttributesData[i],
					mapDataPointers[i][2],
					mapDataPointers[i][1] * 0x100 + mapDataPointers[i][0],
                    rom));
			}

			CloudsMap = new RLEMap((dimensions[mapAttributesData[0].MapDimensionId][0], dimensions[mapAttributesData[0].MapDimensionId][1]),
					CloudMapBank,
					CloudMapOffset,
					rom);
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
				// 6 letters on first line, 4 on second (to cover the whole message)
				" GO ON+KID!!",  // original

				"BORK?+BORK",    // wildham
				" GOOD+ DAY!",   // guardianmarcus
				" WOOP+WOOP",    // Chanigan
				" JERK+BIRD",    // DarkPaladin
				" BEST+ FF!",    // keddril
				" SCI+ENCE",     // kaiten619
				" FLY+HIGH",     // JJBlu
				" LOG+ IN!",     // x10power
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
					for ( ; currentX < 30; currentX += 2)
					{
						_gameMaps[(int)MapList.BackgroundD].ModifyMap(currentX, yPositions[currentYindex], letters[' ']);
					}
					
					currentYindex++;
					currentX = xPositions[currentYindex];
					continue;
				}

				_gameMaps[(int)MapList.BackgroundD].ModifyMap(currentX, yPositions[currentYindex], letters[c]);

				currentX += letters[c][0].Count;
			}

			for (; currentX < 30; currentX += 2)
			{
				_gameMaps[(int)MapList.BackgroundD].ModifyMap(currentX, yPositions[currentYindex], letters[' ']);
			}
		}
		public void MoveCloudMap(FFMQRom rom)
		{
			rom.PutInBank(0x0B, 0x85AD, Blob.FromSBytes(new sbyte[] { NewCloudMapBank }));
            rom.PutInBank(0x0B, 0x8586, Blob.FromUShorts(new ushort[] { NewCloudMapOffset }));
        }
		public void LessObnoxiousMaps(bool enable, ObjectList mapobjects, MT19337 rng)
		{
			if (!enable)
			{
				return;
			}

			// Ice Pyramid
			// Add shortcuts to 1F
			_gameMaps[(int)MapList.IcePyramidA].ModifyMap(0x26, 0x17, new List<List<byte>> {
				new List<byte> { 0x06 },
				new List<byte> { 0x06 },
				new List<byte> { 0x07 },
			});

			_gameMaps[(int)MapList.IcePyramidA].ModifyMap(0x1B, 0x0A, new List<List<byte>> {
				new List<byte> { 0x06, 0x05 },
				new List<byte> { 0x06, 0x05 },
				new List<byte> { 0x07, 0x05 },
			});

			_gameMaps[(int)MapList.IcePyramidA].ModifyMap(0x04, 0x16, new List<List<byte>> {
				new List<byte> { 0x04, 0x06, 0x05, 0x04, 0x04 },
				new List<byte> { 0x14, 0x07, 0x05, 0x14, 0x14 },
				new List<byte> { 0x05, 0x05, 0x05, 0x05, 0x05 },
			});

			// Giant Tree
			// Trim down mushrooms
			_gameMaps[(int)MapList.GiantTreeA].RandomReplaceTile(rng, 0x08, 0x10, 0.5f);
			_gameMaps[(int)MapList.GiantTreeA].RandomReplaceTile(rng, 0x3C, 0x1E, 0.5f);
			_gameMaps[(int)MapList.GiantTreeB].RandomReplaceTile(rng, 0x08, 0x10, 0.5f);

			// Extend platform on 1F to reach hook
			_gameMaps[(int)MapList.GiantTreeA].ModifyMap(0x17, 0x05, new List<List<byte>> { 
				new List<byte> { 0x1C, 0x02 },
				new List<byte> { 0x1C, 0x1E },
				new List<byte> { 0x1C, 0x1E },
				new List<byte> { 0x0A, 0x0A },
				new List<byte> { 0x1A, 0x1A },
			});

			// Move Hook
			mapobjects[0x44][0x14].X = 0x11;
			mapobjects[0x44][0x14].Y = 0x05;

			// Open up passage on 5F
			_gameMaps[(int)MapList.GiantTreeB].ModifyMap(0x0A, 0x13, new List<List<byte>> {
				new List<byte> { 0x21 },
				new List<byte> { 0x22 },
				new List<byte> { 0x1E },
			});

			// Pazuzu's Tower
			// Remove enemies from stair cases
			for (int i = 0x5A; i < 0x5F; i++)
			{
				mapobjects[i].Where(x => x.Type == MapObjectType.Battle).ToList().ForEach(x => x.Gameflag = 0xFE);
			}

			// Mine Interior
			// Reduce enemies count for legacy reasons, maybe not needed, keep an eye on it
			/*
			for (int i = 0; i < mapobjects[0x2E].Count; i++)
			{
				var enemiescollection = mapobjects[0x2E].Where(x => x.Type == MapObjectType.Battle).ToList();
				int totalcount = enemiescollection.Count;
				int toremove = ((100 - 25) * totalcount) / 100;

				for (int j = 0; j < toremove; j++)
				{
					rng.TakeFrom(enemiescollection).Gameflag = (byte)NewGameFlagsList.ShowEnemies;
				}
			}*/
		}
		public void ShuffledMapChanges(MapShufflingMode mode, ObjectList gameobjects)
		{
			if (mode == MapShufflingMode.None)
			{
				return;
			}

			// Block Lava Dome Climbing tiles
			_gameMaps[(int)MapList.LavaDomeExterior].ModifyMap(0x1F, 0x0E, new List<List<byte>>() { new() { 0x2E, 0x2E, 0x2E } });
			_gameMaps[(int)MapList.LavaDomeExterior].ModifyMap(0x34, 0x07, new List<List<byte>>() { new() { 0x2E, 0x2E, 0x2E }, new() { 0x00, 0x00, 0x00 }, new() { 0x00, 0x00, 0x00 } });

			// Block Giant Tree 1F Vine
			_gameMaps[(int)MapList.GiantTreeA].ModifyMap(0x2E, 0x31, new List<List<byte>>() { new() { 0x35 }, new() { 0x7E }, new() { 0x7F } });

			// Add Climbining tiles to worm room
			_gameMaps[(int)MapList.GiantTreeA].ModifyMap(0x04, 0x25, new List<List<byte>>() { new() { 0x05 }, new() { 0x05 }, new() { 0x05 } });

			// Move Pazuzu 1F hook ring
			gameobjects[0x53][0x13].X = 0x34;
			gameobjects[0x53][0x13].Y = 0x18;

			// Remove 3F hook ring
			gameobjects[0x55][0x13].Gameflag = (byte)NewGameFlagsList.ShowEnemies;

			// Remove hole in Mac Ship corridor
			_gameMaps[(int)MapList.MacShipInterior].ModifyMap(0x11, 0x20, new List<List<byte>>() { new() { 0x4B } });
		}

		public void UpdateCloudMap()
		{
			// We do this manually since we change a lot of things
			List<byte> CloudMap = new() {
			};
		}
		public void Write(FFMQRom rom)
		{
			List<int> validBanks = new() { 0x08, 0x13 };
			int currentBank = 0;
			int currentAddress = 0x8000;

			List<byte> newPointersTable = new();
			List<byte> mapattributes = new();

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

				rom.PutInBank(validBanks[currentBank], currentAddress, map.GetArray());
				currentAddress += map.CompressedMapSize;

                mapattributes.AddRange(map.Attributes.ToArray());
			}

			rom.PutInBank(MapDataAddressesBank, MapDataAddressesOffset, newPointersTable.ToArray());
			rom.PutInBank(MapAttributesBank, MapAttributesOffset, mapattributes.ToArray());

			CloudsMap.Compress();
            rom.PutInBank(NewCloudMapBank, NewCloudMapOffset, CloudsMap.GetArray());

            TilesProperties.Write(rom);
		}
	}

	public class MapAttributes
	{ 
		public int TilesProperties { get; set; }
		public List<byte> GraphicRows { get; set; }
        public int MapDimensionId { get; set; }
        public byte UnknowByte1 { get; set; }
		//private int length = 0x0A;
		public MapAttributes(byte[] data)
		{
			TilesProperties = (data[0] & 0x0F);
            MapDimensionId = (data[0] & 0xF0) / 16;
			UnknowByte1 = data[1];
			GraphicRows = data.Take(new Range(2, 10)).ToList();
        }
		public byte[] ToArray()
		{
			return new byte[] {	(byte)((MapDimensionId * 16) | (TilesProperties & 0x0F)), UnknowByte1 }.Concat(GraphicRows.Concat(Enumerable.Repeat((byte)0xFF, 8)).Take(8)).ToArray();
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



            //byte[] _mapAddressRaw = rom.Get(MapDataAddressesOffset + (mapID * 3), 3);
            //byte[] _mapAddressRaw = rom.GetFromBank(bank, offset, 3);
            //_mapAddress = _mapAddressRaw[2] * 0x8000 + (_mapAddressRaw[1] * 0x100 + _mapAddressRaw[0] - 0x8000);

			_tileData = tileprop[Attributes.TilesProperties];

			ModifiedMap = false;

			//var tempdimensions = rom.Get(MapDimensionsTableOffset + (Attributes.MapDimensionId * 2), 2);
			_dimensions = dimensions;

			UncompressMapData(bank, offset, rom);
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
				var tempmap = _mapUncompressed.GetRange((i * _dimensions.Item1), _dimensions.Item1).Select(x => ((_tileData[(x & 0x7F)].Byte1 & 0x07) == 0x07) ? 0xFF : (_tileData[(x & 0x7F)].Byte1 & 0x0F));

				string myStringOutput = String.Join("", tempmap.Select(p => p.ToString("X2")).ToArray());

				Console.WriteLine(myStringOutput);
			}
		}
		public byte WalkableByte(int x, int y)
		{
			return (byte)(_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].Byte1 & 0x07);
		}
		public byte this[int x, int y]
		{
			get => (byte)(_mapUncompressed[(y * _dimensions.Item1) + x]);
		}
		public bool IsScriptTile(int x, int y)
		{
			return (_tileData[_mapUncompressed[(y * _dimensions.Item1) + x] & 0x7F].Byte2 & 0x80) == 0x80;
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
		public void ChestLocationDump(FFMQLib.ObjectList mapobjects)
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

	public class MapChangeAction
	{
		public byte Area {get; set;}
		private byte gameflag;
		private byte action;
		private byte changeid;

		public MapChangeAction(int id, byte[] initialarray)
		{
			Area = (byte)id;
			gameflag = initialarray[0];
			changeid = initialarray[1];
			action = initialarray[2];
		}
		public MapChangeAction(int id, byte _gameflag, byte _changeid, byte _action)
		{
			Area = (byte)id;
			gameflag = _gameflag;
			changeid = _changeid;
			action = _action;
		}
		public byte[] GetBytes()
		{
			return new byte[] { gameflag, changeid, action };
		}
	}

	public class MapChanges
	{
		private List<Blob> _pointers;
		private List<Blob> _mapchanges;
		private List<MapChangeAction> MapChangeActions;

		private const int MapChangesPointersOld = 0xB93A;
		private const int MapChangesEntriesOld = 0xBA0E;
		private const int MapChangesBankOld = 0x06;
		private const int MapChangesQtyOld = 0x6A;
		private const int MapChangesPointersNew = 0x8000;
		private const int MapChangesEntriesNew = 0x8100;
		private const int MapChangesBankNew = 0x12;
		private const int MapChangesQtyNew = 0x80;

		private const int MapActionsInitialPointers = 0xBE77;
		private const int MapActionsSecondaryPointers = 0xBEE3;
		private const int MapActionsOffset = 0xBF15;
		private const int MapActionsNewPointers = 0x9400;
		private const int MapActionsNewOffset = 0x94D8;
		private const int MapActionsQty = 0x6C;


		public MapChanges(FFMQRom rom)
		{
			_pointers = rom.GetFromBank(MapChangesBankOld, MapChangesPointersOld, MapChangesQtyOld * 2).Chunk(2);
			_mapchanges = new List<Blob>();

			foreach (var pointer in _pointers)
			{
				var test = pointer.ToUShorts()[0];
				var sizeByte = rom.GetFromBank(MapChangesBankOld, MapChangesEntriesOld + pointer.ToUShorts()[0] + 2, 1)[0];
				var size = (sizeByte & 0x0F) * (sizeByte / 0x10);
				_mapchanges.Add(rom.GetFromBank(MapChangesBankOld, MapChangesEntriesOld + pointer.ToUShorts()[0], size + 3));
			}

			MapChangeActions = new();
			var actionInitialPointers = rom.GetFromBank(MapChangesBankOld, MapActionsInitialPointers, MapActionsQty);

			for (int i = 0; i < actionInitialPointers.Length; i++)
			{
				byte individualPointer = actionInitialPointers[i];
				if (individualPointer != 0xFF)
				{
					var actualPointer = rom.GetFromBank(MapChangesBankOld, MapActionsSecondaryPointers + (individualPointer * 2), 2).ToUShorts()[0];

					var action = rom.GetFromBank(MapChangesBankOld, MapActionsOffset + actualPointer, 3);

					while (action[0] != 0xFF)
					{
						MapChangeActions.Add(new MapChangeAction(i, action));
						actualPointer += 3;
						action = rom.GetFromBank(MapChangesBankOld, MapActionsOffset + actualPointer, 3);
					}
				}
			}
		}
		public byte Add(Blob mapchange)
		{
			if (_mapchanges.Count() >= MapChangesQtyNew)
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
        public void Modify(int index, int address, List<byte> modifications)
        {
			for (int i = 0; i < modifications.Count; i++)
			{
                _mapchanges[index][address + i] = modifications[i];
            }
        }
        public void Replace(int index, Blob mapchange)
		{
			_mapchanges[index] = mapchange;
		}
		public void AddAction(int area, byte _gameflag, byte _changeid, byte _action)
		{
			MapChangeActions.Add(new MapChangeAction(area, new byte[] { _gameflag, _changeid, _action }));
		}
		public void RemoveActionByFlag(int area, int flag)
		{
			MapChangeActions.RemoveAll(x => x.Area == area && x.GetBytes()[0] == flag);
		}
		private void UpdatePointers()
		{
			_pointers.Clear();
			ushort currentpointer = 0x0000;

			foreach (var change in _mapchanges)
			{
				_pointers.Add(new byte[] { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) });
				currentpointer += (ushort)change.Length;
			}
		}
		public void ReorderOwMapchanges()
		{
			var inst25 = new MapChangeAction(0, 0x04, 0x00, 0x25);
			var inst22 = new MapChangeAction(0, 0x22, 0x00, 0x22);

			MapChangeActions[4] = inst22;
			MapChangeActions[5] = inst25;
		}
		private void UpdateMapChangeActions(FFMQRom rom)
		{
			ushort currentPointer = 0x0000;
			List<ushort> pointerList = new();
			List<byte> actionData = new();

			for (int i = 0; i < MapActionsQty; i++)
			{
				pointerList.Add(currentPointer);

				var currentChanges = MapChangeActions.Where(x => x.Area == i).ToList();
				foreach (var change in currentChanges)
				{
					actionData.AddRange(change.GetBytes());
					currentPointer += 3;
				}
				actionData.Add(0xFF);
				currentPointer++;
			}

			rom.PutInBank(MapChangesBankNew, MapActionsNewPointers, Blob.FromUShorts(pointerList.ToArray()));
			rom.PutInBank(MapChangesBankNew, MapActionsNewOffset, actionData.ToArray());
		}
		public void Write(FFMQRom rom)
		{
			UpdatePointers();
			UpdateMapChangeActions(rom);

			rom.PutInBank(MapChangesBankNew, MapChangesPointersNew, _pointers.SelectMany(x => x.ToBytes()).ToArray());
			rom.PutInBank(MapChangesBankNew, MapChangesEntriesNew, _mapchanges.SelectMany(x => x.ToBytes()).ToArray());

			// Change LoadMapChange routine
			rom.PutInBank(0x01, 0xC593, Blob.FromHex("008012")); // Change pointers table address
			rom.PutInBank(0x01, 0xC5A0, Blob.FromHex("018112")); // Change Y base
			rom.PutInBank(0x01, 0xC5B6, Blob.FromHex("008112")); // Change X base
			rom.PutInBank(0x01, 0xC5CB, Blob.FromHex("028112")); // Change Size base
			rom.PutInBank(0x01, 0xC5EB, Blob.FromHex("008112")); // Change Entry base

			// Change MapAction routine
			rom.PutInBank(0x01, 0xC8B2, Blob.FromHex("EAEAEAEAEAEAEA")); // skip initial table check
			rom.PutInBank(0x01, 0xC8BF, new byte[] { (MapActionsNewPointers % 0x100), (MapActionsNewPointers / 0x100), MapChangesBankNew }); // new pointers address
			rom.PutInBank(0x01, 0xC8C5, new byte[] { (MapActionsNewOffset % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
			rom.PutInBank(0x01, 0xC8D3, new byte[] { ((MapActionsNewOffset + 1) % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
			rom.PutInBank(0x01, 0xC8DA, new byte[] { ((MapActionsNewOffset + 2) % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
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
