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

namespace FFMQLib
{
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
		public void LessObnoxiousMaps(bool enable, Areas mapobjects, MT19337 rng)
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
					rng.TakeFrom(enemiescollection).Gameflag = (byte)GameFlagIds.ShowEnemies;
				}
			}*/
		}
		public void ShuffledMapChanges(MapShufflingMode mode, Areas gameobjects)
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
			gameobjects[0x55][0x13].Gameflag = (byte)GameFlagIds.ShowEnemies;

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
}
