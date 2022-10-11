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
	public partial class FFMQRom : SnesRom
	{
		public void SetDoomCastleMode(Flags flags)
		{
			int DoomCastleObjectsId = 0x65;

			if (flags.DoomCastleMode == DoomCastleModes.Standard)
			{
				return;
			}

			byte mapsritesindex = 0x01;

			if (flags.DoomCastleMode == DoomCastleModes.DarkKingOnly)
			{
				GameMaps[(int)MapList.FocusTower].ModifyMap(0x39, 0x00, new List<List<byte>> {
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

				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0A]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0B]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0C]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0D]));

				MapObjects[DoomCastleObjectsId][0x00].Coord = (0x39, 0x06);
				MapObjects[DoomCastleObjectsId][0x01].Coord = (0x39, 0x07);
				MapObjects[DoomCastleObjectsId][0x02].Coord = (0x3D, 0x0E);
				MapObjects[DoomCastleObjectsId][0x03].Coord = (0x3D, 0x0F);

				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x68][0x13]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x68][0x14]));

				MapObjects[DoomCastleObjectsId][0x04].Coord = (0x3D, 0x00);
				MapObjects[DoomCastleObjectsId][0x05].Coord = (0x3D, 0x01);

				for (int i = 0; i < MapObjects[DoomCastleObjectsId].Count; i++)
				{
					MapObjects[DoomCastleObjectsId][i].Layer = 0x02;
				}
			}
			else if(flags.DoomCastleMode == DoomCastleModes.BossRush)
			{
				GameMaps[(int)MapList.FocusTower].ModifyMap(0x39, 0x00, new List<List<byte>> {
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
						MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[boss.map][0x00 + i]));
						MapObjects[DoomCastleObjectsId][boss.id * 4 + i].Coord = ((byte)(boss.coords.x + coordOffset[i].x), (byte)(boss.coords.y + coordOffset[i].y));
						MapObjects[DoomCastleObjectsId][boss.id * 4 + i].Sprite = boss.sprite;
						MapObjects[DoomCastleObjectsId][boss.id * 4 + i].Palette = boss.palette;
					}
				}

				mapsritesindex = (byte)MapSpriteSets.MapSpriteSets.Count;

				MapSpriteSets.MapSpriteSets.Add(new MapSpriteSet(
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

				TalkScripts.AddScript(0x01,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C10E",
						"230A",
						"2B0E",
						"0700C012",
						"00"
						}));

				TalkScripts.AddScript(0x77,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C513",
						"230B",
						"2B0F",
						"0710C012",
						"00"
					}));

				TalkScripts.AddScript(0x78,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4C906",
						"230C",
						"2B10",
						"0720C012",
						"00"
					}));

				TalkScripts.AddScript(0x79,
					new ScriptBuilder(new List<string> {
						"04",
						"05E4CD08",
						"230D",
						"2B11",
						"0730C012",
						"00"
					}));

				// Change script to make sure the right boss tiles are removed when defeated
				PutInBank(0x12, 0xC000, Blob.FromHex("2a01214046414642464346ffff00"));
				PutInBank(0x12, 0xC010, Blob.FromHex("2a01214446454646464746ffff00"));
				PutInBank(0x12, 0xC020, Blob.FromHex("2a0121484649464A464B46ffff00"));
				PutInBank(0x12, 0xC030, Blob.FromHex("2a01214C464D464E464F46ffff00"));

				// Modify the boss explosion animation to add a check for the boss rush room, and select the correction position accordingly
				PutInBank(0x01, 0xCFB2, Blob.FromHex("22009511eaeaeaeaeaea"));
				PutInBank(0x11, 0x9500, Blob.FromHex("c916f00cc92af008c940f004c965f0016BADE819AABD8B1A4A4A0A0AAAA9006B"));

				// Move boxes
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0A]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0B]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0C]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x67][0x0D]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x68][0x13]));
				MapObjects[DoomCastleObjectsId].Add(new MapObject(MapObjects[0x68][0x14]));

				MapObjects[DoomCastleObjectsId][0x10].Coord = (0x3D, 0x18);
				MapObjects[DoomCastleObjectsId][0x11].Coord = (0x3D, 0x19);

				MapObjects[DoomCastleObjectsId][0x12].Coord = (0x3D, 0x0C);
				MapObjects[DoomCastleObjectsId][0x13].Coord = (0x39, 0x06);

				MapObjects[DoomCastleObjectsId][0x14].Coord = (0x39, 0x00);
				MapObjects[DoomCastleObjectsId][0x15].Coord = (0x39, 0x01);

				for (int i = 0; i < MapObjects[DoomCastleObjectsId].Count; i++)
				{
					MapObjects[DoomCastleObjectsId][i].Layer = 0x02;
				}

			}

			MapObjects.ModifyAreaAttribute(DoomCastleObjectsId, 2, mapsritesindex);

			// Kill blocking stones map changes
			MapChanges.Replace(0x0F, Blob.FromHex("0000110F"));
			MapChanges.Replace(0x10, Blob.FromHex("0000110F"));
			MapChanges.Replace(0x11, Blob.FromHex("0000110F"));

			// Add an extra space in focus tower for extra boxes
			GameMaps[(int)MapList.FocusTower].ModifyMap(0x0E, 0x1C, new List<List<byte>> {
					new List<byte> { 0x04, 0x11, 0x11, 0x11, 0x11 },
				});

			// Move boxes
			MapObjects[0x09].Add(new MapObject(MapObjects[0x66][0x0A]));
			MapObjects[0x09].Add(new MapObject(MapObjects[0x66][0x0B]));
			MapObjects[0x09].Add(new MapObject(MapObjects[0x66][0x0C]));
			MapObjects[0x09].Add(new MapObject(MapObjects[0x66][0x0D]));

			MapObjects[0x09][0x0B].Coord = (0x0E, 0x1B);
			MapObjects[0x09][0x0C].Coord = (0x0E, 0x1C);
			MapObjects[0x09][0x0D].Coord = (0x12, 0x1B);
			MapObjects[0x09][0x0E].Coord = (0x12, 0x1C);

			for (int i = 0x0B; i < 0x0F; i++)
			{
				MapObjects[0x09][i].Layer = 0x02;
			}

			// Remove Rex from the sand area
			for (int i = 0x00; i < 0x04; i++)
			{
				MapObjects[0x07][i].Gameflag = 0xFE;
			}
		}

		public void DoomCastleShortcut(bool enable)
		{
			if (!enable)
			{
				return;
			}

			// Add arrow to doom castle
			NodeLocations.DoomCastleShortcut();

			// Add bridge
			GameMaps[(int)MapList.Overworld].ModifyMap(0x1C, 0x24, new List<List<byte>> {
				new List<byte> {  0x56 },
				new List<byte> {  0x56 },
				new List<byte> {  0x56 },
			});

			// Modify Desert floor to not require megagrenade/dragonclaw
			GameMaps[(int)MapList.FocusTowerBase].ModifyMap(0x1F, 0x11, new List<List<byte>> {
				new List<byte> {  0x05, 0x06, 0x7F },
				new List<byte> {  0x08, 0x0B, 0x2D },
				new List<byte> {  0x08, 0x42, 0x2D },
				new List<byte> {  0x08, 0x0C, 0x2D },
				new List<byte> {  0x08, 0x07, 0x2D },
			});

			GameMaps[(int)MapList.FocusTowerBase].ModifyMap(0x25, 0x10, new List<List<byte>> {
				new List<byte> {  0x05, 0x06, 0x7F },
				new List<byte> {  0x08, 0x07, 0x2D },
				new List<byte> {  0x08, 0x0B, 0x2D },
				new List<byte> {  0x08, 0x42, 0x2D },
				new List<byte> {  0x08, 0x0C, 0x2D },
				new List<byte> {  0x05, 0x19, 0x2D },
				new List<byte> {  0x01, 0x01, 0x2D },
				new List<byte> {  0x03, 0x03, 0x2E },
			});

			GameMaps[(int)MapList.FocusTowerBase].ModifyMap(0x29, 0x1B, new List<List<byte>> {
				new List<byte> {  0x0F, 0x0F, 0x0F, 0x0F },
			});

			GameMaps[(int)MapList.FocusTowerBase].ModifyMap(0x1A, 0x16, new List<List<byte>> {
				new List<byte> {  0x08, 0x08, 0x08, 0x08 },
				new List<byte> {  0x0C, 0x09, 0x0A, 0x08 },
				new List<byte> {  0x07, 0x01, 0x01, 0x13 },
				new List<byte> {  0x07, 0x03, 0x03, 0x13 },
			});
		}
	}
}
			
