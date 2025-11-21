using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void UnjankOverworld(GameMaps maps, MapChanges mapchanges, MapPalettes palette)
		{
			maps[0].Attributes.GraphicRows[4] = 0x20;
			maps[0].Attributes.GraphicRows[5] = 0x1F;

			// Create tiles
			//byte fullcliff = 0x14;
			byte fullvoid = 0x68;
			byte southvoid = 0x16; // based off 0x60, 40417f7f
			byte westvoid = 0x1C; // based off 0x62, 424b4342
			byte eastvoid = 0x37; // based off 0x63, 4a454544
			byte fullcliff = 0x67; // based off 0x14, 4a454544
			byte libracliff = 0x66;

			/*
			byte cloudNE = 0x76;
            byte cloudN = 0x77;
            byte cloudNW = 0x78;
            byte cloudCe = 0x79;
            byte cloudSE = 0x7a;
            byte cloudS = 0x7b;
            byte cloudSW = 0x7c;
            byte cloudE = 0x7d;
            byte cloudW = 0x7e;
            byte cloudEm = 0x7f;
			*/

			mapchanges.ReorderOwMapchanges();

            // Bad hard coding of overworld tileset graphic
            PutInBank(0x06, 0x8000 + southvoid * 4, Blob.FromHex("80817f7f"));
			PutInBank(0x06, 0x8000 + westvoid * 4, Blob.FromHex("828b8382"));
			PutInBank(0x06, 0x8000 + eastvoid * 4, Blob.FromHex("8a858584"));
			PutInBank(0x06, 0x8000 + eastvoid * 4, Blob.FromHex("8a858584"));
			PutInBank(0x06, 0x8000 + fullcliff * 4, Blob.FromHex("08090908"));

			PutInBank(0x06, 0x8000 + libracliff * 4, Blob.FromHex("174b160d")); // fix cliff
			PutInBank(0x06, 0x9598, Blob.FromHex("174b160d")); // fix cliff

			PutInBank(0x06, 0xA000 + eastvoid, Blob.FromHex("00"));
			PutInBank(0x06, 0xA000 + westvoid, Blob.FromHex("00"));
			PutInBank(0x06, 0xA000 + fullcliff, Blob.FromHex("00"));
			PutInBank(0x06, 0xA000 + libracliff, Blob.FromHex("02"));
			PutInBank(0x06, 0xA566, Blob.FromHex("02")); // libre cliff 2

			PutInBank(0x05, 0xE6C8, GetFromBank(0x05, 0xE848, 0x18)); // Update grass top tile so we can switch palette

			PutInBank(0x05, 0xF460, Blob.FromHex("444644")); // Grass
			PutInBank(0x05, 0xF47A, Blob.FromHex("42")); // Void Corner tile
			PutInBank(0x05, 0xF47C, Blob.FromHex("44")); // Mountain
			PutInBank(0x05, 0xF484, Blob.FromHex("44")); // Mountain ? does it matter since we don't use these anymore?

			// Update overworld map

			byte[,] owcliff = {
				{ 0x34, 0x12, 0x6D, fullcliff, fullcliff, fullcliff, eastvoid },
				{ 0x34, 0x6D, eastvoid, southvoid, southvoid, southvoid, fullvoid },
				{ 0x34, eastvoid, 0x68, 0x68, 0x68, 0x68, 0x68 },
			};

			mapchanges.Modify(0, 0, 3, owcliff);

			/*
			mapchanges.Modify(0, 0x1B, new List<byte> {
				fullcliff, fullcliff, fullcliff, eastvoid,
				0x34, 0x6D, eastvoid, southvoid, southvoid, southvoid, fullvoid,
				0x34, eastvoid
			});*/

			maps[0].ReplaceAll(0x27, 0x26);
			maps[0].ModifyMap(0x1C, 0x29, southvoid);
			maps[0].ModifyMap(0x1D, 0x28, eastvoid);
			maps[0].ModifyMap(0x1E, 0x26, eastvoid);

			maps[0].ModifyMap(0x1F, 0x23, new() {
				new() { fullcliff, fullcliff, eastvoid },
				new() { southvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x22, 0x22, eastvoid);

			maps[0].ModifyMap(0x20, 0x2D, new() {
				new() { westvoid, fullcliff, eastvoid },
				new() { fullvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x25, 0x25, westvoid);

			maps[0].ModifyMap(0x26, 0x2D, westvoid);

			maps[0].ModifyMap(0x27, 0x2E, new() {
				new() { westvoid, fullcliff, eastvoid },
				new() { fullvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x2A, 0x2D, eastvoid);

			maps[0].ModifyMap(0x2B, 0x2B, new() {
				new() { fullcliff, eastvoid },
				new() { southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x2D, 0x2E, new() {
				new() { westvoid, fullcliff, eastvoid },
				new() { fullvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x31, 0x2E, new() {
				new() { westvoid, fullcliff, fullcliff, eastvoid },
				new() { fullvoid, southvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x35, 0x2C, eastvoid);

			maps[0].ModifyMap(0x36, 0x28, new() {
				new() { fullcliff },
				new() { southvoid },
			});

			maps[0].ModifyMap(0x30, 0x21, new() {
				new() { westvoid, fullcliff, eastvoid },
				new() { fullvoid, southvoid, fullvoid },
			});

			maps[0].ModifyMap(0x34, 0x21, new() {
				new() { westvoid, fullcliff, eastvoid },
			});

			// Focus Tower Windia Corner
			maps[0].ModifyMap(0x1E, 0x1D, new() {
				new() { 0x29 },
                new() { 0x34 },
            });

			GameMaps.MoveCloudMap(this);
			GameMaps.CloudsMap.ModifyMap(0x1E, 0x1D, new() {
                new() { 0x77 },
                new() { 0x7B },
            });

			// Cloud map borders
			// Top Fill
			GameMaps.CloudsMap.DrawRow(0, 0, 0x40, 0x79);
            GameMaps.CloudsMap.DrawRow(1, 0, 0x40, 0x79);
            GameMaps.CloudsMap.DrawRow(2, 9, 0x32, 0x7B);

            GameMaps.CloudsMap.ModifyMap(0x08, 0x02, 0x7A);

			// Top Bump          
            GameMaps.CloudsMap.ModifyMap(0x0B, 0x02, new() {
                new() { 0x7C, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x7A },
                new() { 0x7F, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7B, 0x7F },
            });


			// North-East Corner
            GameMaps.CloudsMap.ModifyMap(0x37, 0x02, new() {
                new() { 0x7C, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7E, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7C, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7F, 0x7B, 0x7C, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7F, 0x7F, 0x7F, 0x7B, 0x7B, 0x7B },
            });

            GameMaps.CloudsMap.ModifyMap(0x32, 0x02, new() {
                new() { 0x7C, 0x79, 0x79, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7B, 0x7B, 0x7B, 0x7B, 0x7C },
            });

            // Sides 
            GameMaps.CloudsMap.DrawColumn(0x00, 0x1A, 20, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x01, 0x1A, 20, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x02, 0x1A, 20, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x03, 0x1A, 20, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x04, 0x1A, 20, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x05, 0x1A, 20, 0x7D);
            GameMaps.CloudsMap.DrawColumn(0x3F, 0x1A, 20, 0x7E);

			// Upper West Bump
            GameMaps.CloudsMap.DrawColumn(0x05, 0x12, 4, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x06, 0x12, 4, 0x7D);
            GameMaps.CloudsMap.ModifyMap(0x05, 0x16, 0x7A);

			// South-West Corner
            GameMaps.CloudsMap.DrawColumn(0x05, 0x27, 7, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x06, 0x27, 7, 0x7D);
            GameMaps.CloudsMap.ModifyMap(0x05, 0x26, 0x76);

            // Bottom
            GameMaps.CloudsMap.DrawRow(0x2F, 0, 0x40, 0x79);
            GameMaps.CloudsMap.DrawRow(0x2E, 0, 0x40, 0x77);
            GameMaps.CloudsMap.DrawRow(0x2E, 0, 0x1D, 0x79);
            GameMaps.CloudsMap.DrawRow(0x2D, 6, 0x17, 0x77);
            GameMaps.CloudsMap.ModifyMap(0x1D, 0x2E, 0x76);
            GameMaps.CloudsMap.ModifyMap(0x06, 0x2D, 0x76);

            // Upper East Bump
            GameMaps.CloudsMap.DrawColumn(0x3D, 0x18, 8, 0x7E);
            GameMaps.CloudsMap.DrawColumn(0x3E, 0x18, 8, 0x79);
            GameMaps.CloudsMap.DrawColumn(0x3F, 0x18, 8, 0x79);
            GameMaps.CloudsMap.ModifyMap(0x3E, 0x20, new() { new() { 0x7B, 0x7C } });

            // South-East Corner
            GameMaps.CloudsMap.ModifyMap(0x3B, 0x27, new() {
                new() { 0x7F, 0x7F, 0x7F, 0x77, 0x78 },
                new() { 0x7F, 0x7F, 0x7E, 0x79, 0x79 },
                new() { 0x7F, 0x7F, 0x7E, 0x79, 0x79 },
                new() { 0x7F, 0x7F, 0x78, 0x79, 0x79 },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79 },
                new() { 0x7F, 0x78, 0x79, 0x79, 0x79 },
                new() { 0x78, 0x79, 0x79, 0x79, 0x79 },
            });

            // Volcano Corner
            maps[0].ModifyMap(0x1E, 0x0F, 0x2C);

            GameMaps.CloudsMap.ModifyMap(0x1F, 0x02, new() {
                new() { 0x7B, 0x7C, 0x79, 0x79, 0x79, 0x79, 0x79, 0x79, 0x7A },
                new() { 0x7F, 0x7F, 0x7C, 0x79, 0x79, 0x79, 0x79, 0x7A, 0x7F },
                new() { 0x7F, 0x7F, 0x7E, 0x79, 0x79, 0x79, 0x79, 0x7D, 0x7F },
                new() { 0x7F, 0x7F, 0x78, 0x79, 0x79, 0x79, 0x7A, 0x7F, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79, 0x7A, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x79, 0x76, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x78, 0x79, 0x79, 0x79, 0x79, 0x7D, 0x7F, 0x7F },
                new() { 0x7E, 0x79, 0x79, 0x79, 0x79, 0x7A, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7C, 0x79, 0x79, 0x7A, 0x7F, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x7D, 0x7F, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x7A, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x78, 0x79, 0x7D, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7C, 0x79, 0x7D, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F },
                new() { 0x7F, 0x7F, 0x7B, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F },
            });

            GameMaps.CloudsMap.ModifyMap(0x1B, 0x0C, new() {
                new() { 0x7F, 0x7F, 0x77, 0x7F, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x76, 0x7F },
                new() { 0x7F, 0x78, 0x79, 0x79, 0x7D },
                new() { 0x7E, 0x79, 0x79, 0x7A, 0x7F },
                new() { 0x7F, 0x7C, 0x79, 0x7D, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x76, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x79, 0x7D },
                new() { 0x7F, 0x78, 0x79, 0x7A, 0x7F },
                new() { 0x7F, 0x7C, 0x79, 0x7D, 0x7F },
                new() { 0x7F, 0x7E, 0x79, 0x7D, 0x7F },
                new() { 0x7F, 0x78, 0x79, 0x76, 0x7F },
                new() { 0x7F, 0x7C, 0x79, 0x79, 0x7D },
                new() { 0x7F, 0x7F, 0x7B, 0x7B, 0x7F },
            });

			// Ridge
			GameMaps.CloudsMap.ModifyMap(0x39, 0x19, new() {
				new() { 0x7F, 0x7F, 0x77, 0x77, 0x78 },
				new() { 0x7F, 0x78, 0x79, 0x79, 0x79 },
				new() { 0x7E, 0x79, 0x79, 0x79, 0x79 },
				new() { 0x7F, 0x7B, 0x7B, 0x7B, 0x7C },
			});

			// Palette Switcharoo

			// 0x16 Brown+Ice
			// 0x17 Green+Ice
			// 0x18 Green+Thawed
			// 0x1A Brown+Thawed

			// Set ice color to palette 0x03
			var snowcolor = palette.Palettes[0x16].Colors.GetRange(0x31, 4);
			snowcolor[2] = new SnesColor(palette.Palettes[0x16].Colors[0x03].GetBytes());

			var icecolor = palette.Palettes[0x16].Colors.GetRange(0x30, 8);
			icecolor[3] = new SnesColor(palette.Palettes[0x16].Colors[0x09].GetBytes());
			icecolor[5] = new SnesColor(0, 0, 0);
			icecolor[6] = new SnesColor(palette.Palettes[0x16].Colors[0x16].GetBytes());
			icecolor[7] = new SnesColor(palette.Palettes[0x16].Colors[0x17].GetBytes());

			for (int i = 0; i < 4; i++)
			{
				palette.Palettes[0x16].Colors[0x19 + i].Copy(snowcolor[i]);
				palette.Palettes[0x17].Colors[0x19 + i].Copy(snowcolor[i]);
			}

			for (int i = 0; i < 8; i++)
			{
				palette.Palettes[0x16].Colors[0x28 + i].Copy(icecolor[i]);
				palette.Palettes[0x17].Colors[0x28 + i].Copy(icecolor[i]);
			}

			// Create palette 0x1A
			palette.Palettes.Add(new Palette(palette.Palettes[0x16].GetBytes()));

			var row0 = palette.Palettes[0x18].Colors.GetRange(0, 8);
			var row3 = palette.Palettes[0x18].Colors.GetRange(0x18, 8);
			var row6 = palette.Palettes[0x18].Colors.GetRange(0x30, 8);

			for (int i = 0; i < 8; i++)
			{
				palette.Palettes[0x1A].Colors[0 + i].Copy(row0[i]);
				palette.Palettes[0x1A].Colors[0x18 + i].Copy(row3[i]);
				palette.Palettes[0x1A].Colors[0x30 + i].Copy(row6[i]);
			}
			palette.Palettes[0x1A].Colors[0x01].Copy(palette.Palettes[0x16].Colors[0x09]);
			palette.Palettes[0x1A].Colors[0x02].Copy(palette.Palettes[0x16].Colors[0x0A]);

			palette.Palettes[0x1A].Colors[0x36].Copy(palette.Palettes[0x16].Colors[0x09]);
			palette.Palettes[0x1A].Colors[0x37].Copy(palette.Palettes[0x16].Colors[0x0A]);

            // Overworld status hack
            PutInBank(0x01, 0x9165, Blob.FromHex("22009D11"));
            PutInBank(0x11, 0x9D00, Blob.FromHex("ad910ed01b08c230da8ba904008ff4f27fa92000a2f4f2a0f5f2547f7fabfa285cb8830b"));

            // branch farther for instruction 28
            PutInBank(0x01, 0xC875, Blob.FromHex("27"));
			// check if were on overworld, if not normal behaviour, else we do extra gameflag checks
			PutInBank(0x01, 0xC884, Blob.FromHex("ad910ed00622609D118003adee198d191980e7eaeaeaeaeaea22409D11"));
			// instruction 2500 > moved + new water tile update routine
			PutInBank(0x01, 0xF615, Blob.FromHex("22809D1122e09D1160"));

			// new routines
			PutInBank(0x11, 0x9D40, Blob.FromHex("adee190a0a0a0a8dee19ad1319290f0dee198d13196b"));
			PutInBank(0x11, 0x9D60, Blob.FromHex("a90122769700f00ea90222769700f003a9186ba9176ba91a6b"));

			// instruction 2500 moved
			PutInBank(0x11, 0x9D80, Blob.FromHex("e2308ba91148abc230a20000dabdb09d48bcb69dbdbc9dfa8b547f06abfae8e8e00600d0e7ab6b000000000000000000809560a5c0b274d054d134d27f001f003f00"));

			// water tile routine
			PutInBank(0x11, 0x9DE0, Blob.FromHex("08e230a200bf209e119f4ccf7f9f64cf7f9fd0cf7f9fd0cf7fe8e00490e7c2308ba2249ea090d0a90300547f11a9020022769700d00320009bab286b00000000797a7a7931327a79"));
			
			// We manually cover each tile individually, a bit silly but it works
			PutInBank(0x11, 0x9B00, Blob.FromHex("a2509ca090d0a90300547f11a2549ca064cfa90300547f11a25c9ca024f3a90300547f11a2589ca0d0cfa90300547f1108e220a91c8f30887f8f34887f8f25897f8f208b7f8f268b7f8f678b7f8f6d8b7f8f718b7f8f778b7fa9378f32887f8f36887f8f62887f8fa1887f8f5e897f8fdd897f8fac8a7f8f9b8a7f8fd78a7f8ff58a7f8f168b7f8f228b7f8f2a8b7f8f698b7f8f6f8b7f8f748b7fa90e8f2bd17fa9168f70887f8f72887f8f74887f8f76887f8fa2887f8fe1887f8f65897f8f9e897f8f1d8a7f8fdb8a7f8fec8a7f8f178b7f8f358b7f8f568b7f8f608b7f8f628b7f8f668b7f8f6a8b7f8fa78b7f8fa98b7f8fad8b7f8faf8b7f8fb18b7f8fb48b7f2860"));

			PutInBank(0x11, 0x9C50, Blob.FromHex("b1b27a79b08a7ab08ab0b07905050505"));

			// On the fly palette switcher
			PutInBank(0x01, 0x8F6F, Blob.FromHex("22809911ea"));
			PutInBank(0x11, 0x9980, Blob.FromHex("ad910ed00a08e23020d09920009a28a9d08d21216b"));
			PutInBank(0x11, 0x99D0, Blob.FromHex("a90122769700f013ad8a0ec913b007a20f20509A8005a21420509A60")); // Mountain Tiles
			PutInBank(0x11, 0x9A00, Blob.FromHex("a90422769700f02aa90222769700d022ad8a0ec918b007a20020509A8014a90122769700f007a20520509A8005a20a20509A60")); // Corner/Lake Tiles
			PutInBank(0x11, 0x9A50, Blob.FromHex("8ba91148abbd709a8d2121e8a004bd709a8d2221e888d0f6ab60")); // Palette Updater
			PutInBank(0x11, 0x9A70, Blob.FromHex("66de7b7b6f666802e40166b02d6d2571b02d6d25716802e401")); // Palettes Data

			// Do palette routine on map load too
			PutInBank(0x01, 0x9221, Blob.FromHex("22a09a11"));
			PutInBank(0x11, 0x9AA0, Blob.FromHex("22809911224cb2016b"));
		}
	}
}
