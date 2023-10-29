using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace FFMQLib
{
	public class SpriteReader
	{
		protected byte[] data;

		protected const int infoDataOffset = 0x0A; // 4 bytes
		protected const int infoColorsUsed = 0x2E; // 4 bytes
		protected const int infoColortable = 0x36;

		protected int infoWidth = 104;
		protected int infoHeight = 64;

		protected int dataOffset;
		protected int colorCount;

		protected List<(byte pixelid, byte[] snesrgb)> palette;
		protected List<List<(byte pixelid, byte position)>> pixelcolors;

		protected List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };

		protected byte[] GetTile((int x, int y) tilePosition)
		{
			byte[] tilepixels = new byte[8*8];

			for (int y = 0; y < 8; y++)
			{
				int linestart = dataOffset + ((infoHeight - tilePosition.y - 1 - y) * infoWidth) + tilePosition.x;
				for (int x = 0; x < 8; x++)
				{
					tilepixels[(y * 8) + x] = data[linestart + x];
				}
			}

			return tilepixels;
		}
		protected List<List<byte>> ReadPalettes(int palettecount)
		{
			palette = new();
			pixelcolors = new();

			// Read all the BMP colors
			if (colorCount == 0)
			{
				colorCount = 0x100;
			}

			for (int i = 0; i < colorCount; i++)
			{
				int lowerrange = infoColortable + i * 4;
				int upperrange = lowerrange + 4;

				palette.Add(((byte)i, GetSnesPalette(data[lowerrange..upperrange])));
			}

			List<List<byte>> finalpalettes = new();

			for (int p = 0; p < palettecount; p++)
			{
				List<byte> tempalette = new();
				tempalette.AddRange(new List<byte> { 0x00, 0x00 });
				List<(byte, byte)> temppixels = new();


				for (int i = 0; i < 8; i++)
				{
					byte pixelvalue = data[dataOffset + infoWidth - 8 - (p * 8) + i];

					temppixels.Add((pixelvalue, (byte)i));

					if (i > 0)
					{
						tempalette.AddRange(palette[pixelvalue].snesrgb);
					}
				}

				pixelcolors.Add(temppixels);
				finalpalettes.Add(tempalette);
			}

			//byte emptyPixel = pixelcolors.Find(p => p.position == 0).pixelid;

			return finalpalettes;
		}
		protected byte[] GetSnesPalette(byte[] rgbvalues)
		{
			return new byte[] {
				(byte)((((rgbvalues[1] / 8) * 32) & 0xE0) + (rgbvalues[2] / 8)),
				(byte)(((rgbvalues[0] / 8) * 4) + ((rgbvalues[1] / 8) / 8)) };
		}
		protected byte[] EncodeLine(byte[] pixelline, int paletteid)
		{

			List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

			byte[] encodedline = new byte[3];
			for (int i = 0; i < pixelline.Length; i++)
			{
				byte pixelpalette;

				if (pixelcolors[paletteid].Select(x => x.Item1).Contains(pixelline[i]))
				{
					pixelpalette = pixelcolors[paletteid].Find(x => x.Item1 == pixelline[i]).Item2;
				}
				else
				{
					pixelpalette = 0;
				}

				if ((pixelpalette & palettemask[0]) > 0)
				{
					encodedline[0] |= bitmask[i];
				}

				if ((pixelpalette & palettemask[1]) > 0)
				{
					encodedline[1] |= bitmask[i];
				}

				if ((pixelpalette & palettemask[2]) > 0)
				{
					encodedline[2] |= bitmask[i];
				}
			}

			return encodedline;
		}
		protected byte[] EncodeTile((int x, int y) tilePosition, int paletteid)
		{
			byte[] encodedTile = new byte[0x18];

			for (int i = 0; i < 8; i++)
			{
				int linestart = dataOffset + ((infoHeight - tilePosition.y - 1 - i) * infoWidth) + tilePosition.x;
				var targetLine = data[linestart..(linestart + 8)];
				var encodedline = EncodeLine(targetLine, paletteid);
				encodedTile[(i * 2)] = encodedline[0];
				encodedTile[(i * 2) + 1] = encodedline[1];
				encodedTile[i + 0x10] = encodedline[2];
			}

			return encodedTile;
		}
		protected byte[] EncodeTile(byte[] tile, int paletteid)
		{
			byte[] encodedTile = new byte[0x18];

			for (int i = 0; i < 8; i++)
			{
				int linestart = i * 8;
				var targetLine = tile[linestart..(linestart + 8)];
				var encodedline = EncodeLine(targetLine, paletteid);
				encodedTile[(i * 2)] = encodedline[0];
				encodedTile[(i * 2) + 1] = encodedline[1];
				encodedTile[i + 0x10] = encodedline[2];
			}

			return encodedTile;
		}

		protected List<byte[]> EncodeSeries((int x, int y) startTile, int height, int paletteid)
		{
			List<byte[]> encodedSeries = new();

			for (int i = 0; i < height; i++)
			{
				encodedSeries.Add(EncodeTile((startTile.x, startTile.y + (i * 8)), paletteid));
				encodedSeries.Add(EncodeTile((startTile.x + 8, startTile.y + (i * 8)), paletteid));
			}

			return encodedSeries;
		}

 
	}
	public class PlayerSpriteReader : SpriteReader
	{
		public void Encode(FFMQRom rom)
		{
			dataOffset = data[infoDataOffset] + (data[infoDataOffset + 1] * 0x100) + (data[infoDataOffset + 2] * 0x100 * 0x100) + (data[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			colorCount = data[infoColorsUsed] + (data[infoColorsUsed + 1] * 0x100) + (data[infoColorsUsed + 2] * 0x100 * 0x100) + (data[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

			palette = new();
			pixelcolors = new();
			infoWidth = 104;
			infoHeight = 64;

			if (colorCount == 0)
			{
				colorCount = 0x100;
			}

			for (int i = 0; i < colorCount; i++)
			{
				int lowerrange = infoColortable + i * 4;
				int upperrange = lowerrange + 4;

				palette.Add(((byte)i, GetSnesPalette(data[lowerrange..upperrange])));
			}

			List<byte> finalPalette = new();
			finalPalette.AddRange(new List<byte> { 0x00, 0x00 });
			pixelcolors.Add(new());


            for (int i = 0; i < 8; i++)
			{
				byte pixelvalue = data[dataOffset + infoWidth - 8 + i];

				pixelcolors[0].Add((pixelvalue, (byte)i));

				if (i > 0)
				{
					finalPalette.AddRange(palette[pixelvalue].Item2);
				}
			}

			byte emptyPixel = pixelcolors[0].Find(p => p.Item2 == 0).Item1;

			// Get Software Bop flag
			byte softbopbyte = data[dataOffset + (2 * infoWidth) - 1];
			bool softbopenabled = (softbopbyte != emptyPixel);

			// Get Full Horizontal Flip flag
			byte fullhorizontalflipbyte = data[dataOffset + (2 * infoWidth) - 2];
			bool fullhorizontalflibenabled = (fullhorizontalflipbyte != emptyPixel);

			// Encode Sprites
			List<byte[]> walkseriesEncoded = EncodeSeries((0, 0), 8, 0);
			List<byte[]> pushseriesEncoded = EncodeSeries((16, 0), 8, 0);
			List<byte[]> jumpseriesEncoded = EncodeSeries((32, 0), 6, 0);
			List<byte[]> victoryseriesEncoded = EncodeSeries((48, 0), 4, 0);
			List<byte[]> throwdeathEncoded = EncodeSeries((64, 0), 8, 0);
			List<byte[]> bombshrugEncoded = EncodeSeries((80, 0), 4, 0);
			byte[] shrughandEncoded = EncodeTile((96, 24), 0);
			List<byte[]> climbEncoded = EncodeSeries((80, 32), 3, 0);

			rom.PutInBank(0x04, 0x9A20, walkseriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCA20, pushseriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCBA0, jumpseriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCD20, victoryseriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCEA0, throwdeathEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xD020, bombshrugEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xD0E0, shrughandEncoded);
			rom.PutInBank(0x04, 0xD110, climbEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x07, 0xD824, finalPalette.ToArray());

			// Software Bop Hack
			if (softbopenabled)
			{
				rom.PutInBank(0x01, 0x94B6, Blob.FromHex("22008511eaeaeaea"));
				rom.PutInBank(0x11, 0x8500, Blob.FromHex("ad26192904ea4a4a48ad8b0e2901f0096848f002a9ff8d9919686b"));
			}

			// Full Horizontal Flip hack
			if (fullhorizontalflibenabled)
			{
				// Copy animation array
				rom.PutInBank(0x11, 0x82F0, rom.GetFromBank(0x00, 0xF13C, 0x110));

				rom.PutInBank(0x11, 0x82F0, Blob.FromHex("0d400c40"));
				rom.PutInBank(0x11, 0x8318, Blob.FromHex("01400040"));

				rom.PutInBank(0x01, 0x8D3E, Blob.FromHex("7ff08211"));
				rom.PutInBank(0x01, 0x8D45, Blob.FromHex("bff18211"));
				rom.PutInBank(0x01, 0x8D81, Blob.FromHex("7ff08211"));
				rom.PutInBank(0x01, 0x8D88, Blob.FromHex("bff18211"));

				// Horizontal flip in battle
				rom.PutInBank(0x02, 0xF35B, Blob.FromHex("2220851160"));
				rom.PutInBank(0x11, 0x8520, Blob.FromHex("8ad01eb9020c48b9060c99020c6899060cb9030c494099030cb9070c494099070cb90a0c48b90e0c990a0c68990e0cb90b0c4940990b0cb90f0c4940990f0c6b"));

				// Status sprites fix in battle
				rom.PutInBank(0x0B, 0xFFA0, Blob.FromHex("2270851120049360")); // reroute
				rom.PutInBank(0x11, 0x8570, Blob.FromHex("8ad005a9308d430c6b")); // Force initial position
				rom.PutInBank(0x0B, 0x8FFA, Blob.FromHex("20A0FF")); // Blind
				rom.PutInBank(0x0B, 0x9029, Blob.FromHex("20A0FF")); // Poison?
				rom.PutInBank(0x0B, 0x90A4, Blob.FromHex("20A0FF")); // Confusion
				rom.PutInBank(0x0B, 0x9178, Blob.FromHex("20A0FF")); // ???
				rom.PutInBank(0x0B, 0x9200, Blob.FromHex("20A0FF")); // Paralysis
				rom.PutInBank(0x0B, 0x9297, Blob.FromHex("20A0FF")); // Stone
				rom.PutInBank(0x0B, 0x92B3, Blob.FromHex("20A0FF")); // Death
				rom.PutInBank(0x0B, 0x8F39, Blob.FromHex("20A0FF")); // Back to normal

				// Action animation fix
				rom.PutInBank(0x11, 0x8580, Blob.FromHex("b50248b5069502689506b50349409503b50749409507b50a48b50e950a68950eb50b4940950bb50f4940950f6b"));
				rom.PutInBank(0x02, 0xF387, Blob.FromHex("22808511eaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaea"));
				rom.PutInBank(0x02, 0xF3D9, Blob.FromHex("22808511eaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaea"));
			}
		}
		public void LoadCustomSprites(PlayerSprite sprite, FFMQRom rom)
		{
			if (sprite.filename == "default")
			{
				return;
			}
			else
			{
				data = sprite.spritesheet;
				Encode(rom);
			}
		}
	}
	public class DarkKingSpriteDataPack
	{ 
		public byte[] DrawingArray { get; set; }
        public byte[] PaletteArray { get; set; }
        public List<byte[]> EncodedTiles { get; set; }
        public List<byte> Palette1 { get; set; }
        public List<byte> Palette2 { get; set; }
    }
	public class DarkKingSpriteReader : SpriteReader
	{
		private const int drawingArrayBank = 0x0A;
        private const int drawingArrayOffsetDK3 = 0x82C6;
        private const int drawingArrayOffsetDK4 = 0x82E9;
        private const int paletteArrayOffsetDK3 = 0x85D2;
        private const int paletteArrayOffsetDK4 = 0x85F5;

        private const int graphicsBank = 0x0B;
        private const int graphicsOffsetDK = 0xB33C;

        private const int paletteBank = 0x09;
        private const int paletteOffsetDarkKing = 0x8280;

		private List<string> dktrueformsname = new() { "dragonlord" };

        private DarkKingSpriteDataPack Encode()
		{
            dataOffset = data[infoDataOffset] + (data[infoDataOffset + 1] * 0x100) + (data[infoDataOffset + 2] * 0x100 * 0x100) + (data[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			colorCount = data[infoColorsUsed] + (data[infoColorsUsed + 1] * 0x100) + (data[infoColorsUsed + 2] * 0x100 * 0x100) + (data[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

			palette = new();
			pixelcolors = new();
			infoWidth = 28 * 8;
			infoHeight = 10 * 8 + 1;

			List<List<byte>> finalpalettes = ReadPalettes(2);

			List<byte> emptyPixels = new() { pixelcolors[0].Find(p => p.position == 0).pixelid, pixelcolors[1].Find(p => p.position == 0).pixelid };

			byte[] drawingarray = new byte[280 / 8];
			byte[] palettearray = new byte[280 / 8];

			List<byte[]> encodedTiles = new();

            int bitmaskposition = 0;
            int arrayposition = 0;

            for (int y = 0; y < 10; y++)
			{

				for (int x = 0; x < 28; x++)
				{
					var currentile = GetTile((x * 8, y * 8));

					// Empty tile?
					if (currentile.Where(emptyPixels.Contains).ToList().Count >= 64)
					{
						drawingarray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						palettearray[arrayposition] |= bitmask[bitmaskposition];
					}
					else // Not empty
					{
						int palettecandidate = 0;
						int matchcount = 0;

						for (int i = 0; i < pixelcolors.Count; i++)
						{
							var currenmatchcount = pixelcolors[i].Select(p => p.pixelid).Intersect(currentile).ToList().Count;

							if (currenmatchcount > matchcount)
							{
								matchcount = currenmatchcount;
                                palettecandidate = i;
                            }
						}
                        
						drawingarray[arrayposition] |= bitmask[bitmaskposition];
						if (palettecandidate == 0)
						{
							palettearray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						}
						else
						{
                            palettearray[arrayposition] |= bitmask[bitmaskposition];
                        }

						encodedTiles.Add(EncodeTile(currentile, palettecandidate));
                    }

					bitmaskposition++;

					if (bitmaskposition >= 8)
					{
						arrayposition++;
						bitmaskposition = 0;
					}
				}
			}

			DarkKingSpriteDataPack result = new()
			{
				DrawingArray = drawingarray,
				PaletteArray = palettearray,
				Palette1 = finalpalettes[1],
				Palette2 = finalpalettes[0],
				EncodedTiles = encodedTiles
            };

			return result;
        }
		public void RandomizeDarkKingTrueForm(bool enable, MT19337 rng, FFMQRom rom)
		{
			if (!enable)
			{
				rng.Next();
                return;
            }

            var rngback = rng;

            var filename = rng.PickFrom(dktrueformsname);

            LoadSpritesheetFromZip(filename + "1");
			DarkKingSpriteDataPack darkking3 = Encode();

            LoadSpritesheetFromZip(filename + "2");
            DarkKingSpriteDataPack darkking4 = Encode();

            rom.PutInBank(drawingArrayBank, drawingArrayOffsetDK3, darkking3.DrawingArray);
            rom.PutInBank(drawingArrayBank, paletteArrayOffsetDK3, darkking3.PaletteArray);

            rom.PutInBank(drawingArrayBank, drawingArrayOffsetDK4, darkking4.DrawingArray);
            rom.PutInBank(drawingArrayBank, paletteArrayOffsetDK4, darkking4.PaletteArray);

			var dk12sprites = rom.GetFromBank(graphicsBank, graphicsOffsetDK, 0x1890).ToBytes().ToList();

			// Move all DK sprites to bank 10
            rom.PutInBank(0x10, 0xB2F0, dk12sprites.Concat(darkking3.EncodedTiles.Concat(darkking4.EncodedTiles).SelectMany(x => x)).ToArray());
            rom.PutInBank(0x09, 0x85F0, Blob.FromHex("F0B210"));

            // Expand Dark King Palette Hack
            var originaldkpalettes = rom.GetFromBank(0x09, paletteOffsetDarkKing, 0x10 * 4).Chunk(0x10);
            List<byte[]> newdkpalettes = new() {
                originaldkpalettes[1].ToBytes(), originaldkpalettes[0].ToBytes(),
                originaldkpalettes[2].ToBytes(), originaldkpalettes[0].ToBytes(),
                darkking3.Palette1.ToArray(), darkking3.Palette2.ToArray(),
                darkking4.Palette1.ToArray(), darkking4.Palette2.ToArray(),
            };

            rom.PutInBank(0x10, 0xB100, newdkpalettes.SelectMany(x => x).ToArray());

            rom.PutInBank(0x02, 0xD89B, Blob.FromHex("2200b01080ef"));
            rom.PutInBank(0x10, 0xB000, Blob.FromHex("08e230a683b507c904d0013a0a4818690148c230a58329ff000a0a0a0a0a0a186940c0a8e220c21068c23029ff000a0a0a0a186900b1aaa90f00547e109818691000a8e220c21068c23029ff000a0a0a0a186900b1aaa90f00547e10286b"));

            rng = rngback;
            rng.Next();
        }
        private void LoadSpritesheet(string filename)
        {
            byte[] spritesheet;
            var assembly = Assembly.GetExecutingAssembly();
            string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
            using (Stream entry = assembly.GetManifestResourceStream(filepath))
            {
				using (BinaryReader reader = new BinaryReader(entry))
                {
                        spritesheet = reader.ReadBytes((int)entry.Length);
                }
            }

			data = spritesheet;
            //return spritesheet;
        }
        private void LoadSpritesheetFromZip(string spritename)
        {
            //byte[] spritesheet;
            var assembly = Assembly.GetExecutingAssembly();
            string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("dktrueforms.zip"));
            using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
            {
                using (ZipArchive spriteContainer = new ZipArchive(zipfile))
                {
                    var entry = spriteContainer.GetEntry(spritename + ".bmp");
                    using (BinaryReader reader = new BinaryReader(entry.Open()))
                    {
                        data = reader.ReadBytes((int)entry.Length);
                    }
                }
            }
        }
    }
	public enum PlayerSpriteMode
	{ 
		Spritesheets,
		Icons
	}

	public class PlayerSprite
	{
		public string filename { get; set; }
		public string author { get; set; }
		public string name { get; set; }
		[YamlIgnore]
		public byte[] spritesheet { get; set; }
		[YamlIgnore]
		public byte[] iconimg { get; set; }

		public PlayerSprite()
		{
			filename = "";
			author = "";
			name = "";
		}
		public PlayerSprite(string _name, byte[] _spritedata)
		{
			filename = _name;
			author = "";
			name = "";
			spritesheet = _spritedata;
		}
		public PlayerSprite(string _name)
		{
			filename = _name;
			author = "";
			name = "";
		}
		public PlayerSprite(PlayerSprite _sprite, byte[] _spritedata)
		{
			filename = _sprite.filename;
			author = _sprite.author;
			name = _sprite.name;
			spritesheet = _spritedata;
		}
	}
	public class PlayerSprites
	{
		public List<PlayerSprite> sprites { get; set; }
		public PlayerSprites(PlayerSpriteMode mode)
		{
			LoadMetadata();
			if (mode == PlayerSpriteMode.Icons)
			{
				LoadIcons();
			}
		}
		private void LoadMetadata()
		{
			string metadatayaml = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("customsprites.zip"));
			using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
			{
				using (ZipArchive spriteContainer = new ZipArchive(zipfile))
				{
					var entry = spriteContainer.GetEntry("metadata.yaml");
					using (StreamReader reader = new StreamReader(entry.Open()))
					{
						metadatayaml = reader.ReadToEnd();
					}
				}
			}

			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();

			try
			{
				sprites = deserializer.Deserialize<List<PlayerSprite>>(metadatayaml);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			sprites = sprites.OrderBy(s => s.name).ToList();
		}
		public PlayerSprite GetSprite(Preferences pref, MT19337 rng)
		{
			if (pref.PlayerSprite == "default")
			{ 
				return new PlayerSprite(pref.PlayerSprite);
			}
			if (pref.PlayerSprite == "random")
			{
				var selectedsprite = rng.PickFrom(sprites);

				return new PlayerSprite(selectedsprite, LoadSpritesheet(selectedsprite.filename));
			}
			else if (pref.PlayerSprite == "custom")
			{
				return new PlayerSprite(pref.PlayerSprite, pref.CustomSprites);
			}
			else
			{
				var selectedsprite = sprites.Where(s => s.filename == pref.PlayerSprite);

				if (selectedsprite.Any())
				{
					return new PlayerSprite(selectedsprite.First(), LoadSpritesheet(selectedsprite.First().filename));
				}
				else
				{
					return new PlayerSprite("default");
				}
			}
		}
		private void LoadIcons()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("customsprites.zip"));
			using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
			{
				using (ZipArchive spriteContainer = new ZipArchive(zipfile))
				{
					foreach (var sprite in sprites)
					{
						var entry = spriteContainer.GetEntry("icons/" + sprite.filename + ".png");
						using (BinaryReader reader = new BinaryReader(entry.Open()))
						{
							sprite.iconimg = reader.ReadBytes((int)entry.Length);
						}
					}
				}
			}
		}
		private byte[] LoadSpritesheet(string spritename)
		{
			byte[] spritesheet;
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("customsprites.zip"));
			using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
			{
				using (ZipArchive spriteContainer = new ZipArchive(zipfile))
				{
					var entry = spriteContainer.GetEntry("spritesheets/" + spritename + ".bmp");
					using (BinaryReader reader = new BinaryReader(entry.Open()))
					{
						spritesheet = reader.ReadBytes((int)entry.Length);
					}
				}
			}

			return spritesheet;
		}
	}
}
