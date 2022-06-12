using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum DoomCastleModes : int
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Boss Rush")]
		BossRush,
		[Description("Dark King Only")]
		DarkKingOnly,
	}
	public class DoomCastle
	{
		private int DoomCastleObjectsId = 0x65;

		public DoomCastle(Flags flags, GameMaps gamemaps, MapChanges mapchanges, ObjectList mapobjects, MapSprites mapsrites, GameScriptManager talkscripts, FFMQRom rom)
		{
			if (flags.DoomCastleMode == DoomCastleModes.Standard)
			{
				return;
			}

			byte mapsritesindex = 0x01;
			if (flags.DoomCastleMode == DoomCastleModes.DarkKingOnly)
			{
				gamemaps[(int)MapList.FocusTower].ModifyMap(0x39, 0x00, new List<List<byte>> {
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0x91, 0x91, 0x91, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA2, 0xA9, 0xA5, 0xA5, 0xA2 },
					new List<byte> {  0xB0, 0x96, 0xA5, 0xA5, 0xB0 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0x91, 0x91, 0x91, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA2, 0xA9, 0xA5, 0xA5, 0xA2 },
					new List<byte> {  0xB0, 0x96, 0xA5, 0xA5, 0xB0 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0x91, 0x91, 0x91, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA1, 0xA9, 0xA5, 0xA5, 0xA1 },
					new List<byte> {  0xA2, 0xA9, 0xA5, 0xA5, 0xA2 },
					new List<byte> {  0xB0, 0x96, 0xA5, 0xA5, 0xB0 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
				});

				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0A]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0B]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0C]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0D]));

				mapobjects[DoomCastleObjectsId][0x00].Coord = (0x39, 0x06);
				mapobjects[DoomCastleObjectsId][0x01].Coord = (0x39, 0x07);
				mapobjects[DoomCastleObjectsId][0x02].Coord = (0x3D, 0x0E);
				mapobjects[DoomCastleObjectsId][0x03].Coord = (0x3D, 0x0F);

				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x68][0x13]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x68][0x14]));

				mapobjects[DoomCastleObjectsId][0x04].Coord = (0x3D, 0x00);
				mapobjects[DoomCastleObjectsId][0x05].Coord = (0x3D, 0x01);

				for (int i = 0; i < mapobjects[DoomCastleObjectsId].Count; i++)
				{
					mapobjects[DoomCastleObjectsId][i].Layer = 0x02;
				}
			}
			else if(flags.DoomCastleMode == DoomCastleModes.BossRush)
			{
				gamemaps[(int)MapList.FocusTower].ModifyMap(0x39, 0x00, new List<List<byte>> {
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0xA1, 0xA1, 0x91, 0x91 },
					new List<byte> {  0xA1, 0xA1, 0xA1, 0xA9, 0xA5 },
					new List<byte> {  0xA2, 0xA2, 0xA2, 0xA9, 0xA5 },
					new List<byte> {  0xB0, 0xB0, 0xB0, 0x94, 0x91 },

					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0xA1, 0xA1, 0xA1 },
					new List<byte> {  0xA9, 0xA5, 0xA1, 0xA1, 0xA1 },
					new List<byte> {  0xA9, 0xA5, 0xA2, 0xA2, 0xA2 },
					new List<byte> {  0x84, 0x91, 0xB0, 0xB0, 0xB0 },

					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0xA1, 0xA1, 0x91, 0x91 },
					new List<byte> {  0xA1, 0xA1, 0xA1, 0xA9, 0xA5 },
					new List<byte> {  0xA2, 0xA2, 0xA2, 0xA9, 0xA5 },
					new List<byte> {  0xB0, 0xB0, 0xB0, 0x94, 0x91 },

					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0xA1, 0xA1, 0xA1 },
					new List<byte> {  0xA9, 0xA5, 0xA1, 0xA1, 0xA1 },
					new List<byte> {  0xA9, 0xA5, 0xA2, 0xA2, 0xA2 },
					new List<byte> {  0x84, 0x91, 0xB0, 0xB0, 0xB0 },

					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0x84, 0x91, 0x91, 0x91, 0x91 },
					new List<byte> {  0xA1, 0xA1, 0x91, 0xA1, 0xA1 },
				});

				List<(int x, int y)> coordOffset = new() { (0,0), (1,0), (0,1), (1,1) };
				List<(int id, int map, byte sprite, byte palette, (int x, int y) coords)> bossAttributes = new()
				{
					(0, 0x07, 0x2C, 0x00, (0x39, 0x16)),
					(1, 0x66, 0x34, 0x01, (0x3C, 0x10)),
					(2, 0x67, 0x3C, 0x02, (0x39, 0x0A)),
					(3, 0x68, 0x48, 0x03, (0x3C, 0x04)),
				};

				foreach (var boss in bossAttributes)
				{
					for (int i = 0; i < 4; i++)
					{
						mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[boss.map][0x00 + i]));
						mapobjects[DoomCastleObjectsId][boss.id * 4 + i].Coord = ((byte)(boss.coords.x + coordOffset[i].x), (byte)(boss.coords.y + coordOffset[i].y));
						mapobjects[DoomCastleObjectsId][boss.id * 4 + i].Sprite = boss.sprite;
						mapobjects[DoomCastleObjectsId][boss.id * 4 + i].Palette = boss.palette;
					}
				}

				mapsritesindex = (byte)mapsrites.MapSpriteSets.Count;

				mapsrites.MapSpriteSets.Add(new MapSpriteSet(
					new List<byte> { 0x52, 0x50, 0x56, 0x54, 0x1f, 0x1e },
					new List<SpriteAddressor> {
						new SpriteAddressor(4, 0x13, SpriteSize.Tiles16),
						new SpriteAddressor(7, 0x27, SpriteSize.Tiles16),
						new SpriteAddressor(8, 0x14, SpriteSize.Tiles16),
						new SpriteAddressor(16, 0x15, SpriteSize.Tiles16),
						new SpriteAddressor(24, 0x16, SpriteSize.Tiles16),
					},
					true
					));

				talkscripts.AddScript(0x01,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C10E",
						"230A",
						"2B0E",
						"0700C012",
						"00"
						}));

				talkscripts.AddScript(0x77,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C513",
						"230B",
						"2B0F",
						"0710C012",
						"00"
					}));

				talkscripts.AddScript(0x78,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C906",
						"230C",
						"2B10",
						"0720C012",
						"00"
					}));

				talkscripts.AddScript(0x79,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4CD08",
						"230D",
						"2B11",
						"0730C012",
						"00"
					}));

				// Change script to mae sure the right boss tiles are removed when defeated
				rom.PutInBank(0x12, 0xC000, Blob.FromHex("2a01214046414642464346ffff00"));
				rom.PutInBank(0x12, 0xC010, Blob.FromHex("2a01214446454646464746ffff00"));
				rom.PutInBank(0x12, 0xC020, Blob.FromHex("2a0121484649464A464B46ffff00"));
				rom.PutInBank(0x12, 0xC030, Blob.FromHex("2a01214C464D464E464F46ffff00"));

				// Modify the boss explosion animation to add a check for the boss rush room, and select the correction position accordingly
				rom.PutInBank(0x01, 0xCFB2, Blob.FromHex("22009511eaeaeaeaeaea"));
				rom.PutInBank(0x11, 0x9500, Blob.FromHex("c916f00cc92af008c940f004c965f0016BADE819AABD8B1A4A4A0A0AAAA9006B"));

				// Move boxes
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0A]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0B]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0C]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x67][0x0D]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x68][0x13]));
				mapobjects[DoomCastleObjectsId].Add(new MapObject(mapobjects[0x68][0x14]));

				mapobjects[DoomCastleObjectsId][0x10].Coord = (0x3D, 0x18);
				mapobjects[DoomCastleObjectsId][0x11].Coord = (0x3D, 0x19);
				
				mapobjects[DoomCastleObjectsId][0x12].Coord = (0x3D, 0x0C);
				mapobjects[DoomCastleObjectsId][0x13].Coord = (0x39, 0x06);

				mapobjects[DoomCastleObjectsId][0x14].Coord = (0x39, 0x00);
				mapobjects[DoomCastleObjectsId][0x15].Coord = (0x39, 0x01);

				for (int i = 0; i < mapobjects[DoomCastleObjectsId].Count; i++)
				{
					mapobjects[DoomCastleObjectsId][i].Layer = 0x02;
				}

			}

			mapobjects.ModifyAreaAttribute(DoomCastleObjectsId, 2, mapsritesindex);

			// Kill blocking stones map changes
			mapchanges.Replace(0x0F, Blob.FromHex("0000110F"));
			mapchanges.Replace(0x10, Blob.FromHex("0000110F"));
			mapchanges.Replace(0x11, Blob.FromHex("0000110F"));

			// Add an extra space in focus tower for extra boxes
			gamemaps[(int)MapList.FocusTower].ModifyMap(0x0E, 0x1C, new List<List<byte>> {
					new List<byte> { 0x04, 0x11, 0x11, 0x11, 0x11 },
				});

			// Move boxes
			mapobjects[0x09].Add(new MapObject(mapobjects[0x66][0x0A]));
			mapobjects[0x09].Add(new MapObject(mapobjects[0x66][0x0B]));
			mapobjects[0x09].Add(new MapObject(mapobjects[0x66][0x0C]));
			mapobjects[0x09].Add(new MapObject(mapobjects[0x66][0x0D]));

			mapobjects[0x09][0x0B].Coord = (0x0E, 0x1B);
			mapobjects[0x09][0x0C].Coord = (0x0E, 0x1C);
			mapobjects[0x09][0x0D].Coord = (0x12, 0x1B);
			mapobjects[0x09][0x0E].Coord = (0x12, 0x1C);

			for (int i = 0x0B; i < 0x0F; i++)
			{
				mapobjects[0x09][i].Layer = 0x02;
			}

			// Remove Rex from the sand area
			for (int i = 0x00; i < 0x04; i++)
			{
				mapobjects[0x07][i].Gameflag = 0xFE;
			}
		}
	}
}
