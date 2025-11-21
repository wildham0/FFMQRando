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
using static System.Runtime.InteropServices.JavaScript.JSType;
using BigGustave;
using System.Drawing;

namespace FFMQLib
{
	public class SpriteReader
	{
		private byte[] data;

		private const int infoDataOffset = 0x0A; // 4 bytes
		private const int infoColorsUsed = 0x2E; // 4 bytes
		private const int infoColortable = 0x36;

		private int infoWidth;
		private int infoHeight;

		private int dataOffset;
		private int colorCount;

		private List<(byte pixelid, byte[] snesrgb)> palette;
		private List<List<(byte pixelid, byte position)>> pixelcolors;

		private List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };

		private byte[] GetTile((int x, int y) tilePosition)
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
		private List<List<byte>> ReadPalettes(int palettecount)
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
		private byte[] GetSnesPalette(byte[] rgbvalues)
		{
			return new byte[] {
				(byte)((((rgbvalues[1] / 8) * 32) & 0xE0) + (rgbvalues[2] / 8)),
				(byte)(((rgbvalues[0] / 8) * 4) + ((rgbvalues[1] / 8) / 8)) };
		}
		private byte[] EncodeLine(byte[] pixelline, int paletteid)
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
		private byte[] EncodeLine2bpp(byte[] pixelline)
		{
			List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

			byte[] encodedline = new byte[2];
			for (int i = 0; i < pixelline.Length; i++)
			{

				if (pixelline[i] == 0x01 || pixelline[i] == 0x03)
				{
					encodedline[0] |= bitmask[i];
				}

				if (pixelline[i] == 0x02 || pixelline[i] == 0x03)
				{
					encodedline[1] |= bitmask[i];
				}
			}

			return encodedline;
		}
		private byte[] EncodeRow2bpp(byte[] row)
		{
			var sections = row.Chunk(8).ToList();

			List<byte[]> encodedRow = new();

			foreach (var section in sections)
			{
				encodedRow.Add(EncodeLine2bpp(section));
			}

			return encodedRow.SelectMany(s => s).ToArray();
		}
		private byte[] EncodeImage2bpp(byte[] image)
		{
			var rows = image.Chunk(infoWidth).ToList();

			List<byte[]> encodedImage = new();

			foreach (var row in rows)
			{
				encodedImage.Add(EncodeRow2bpp(row));
			}

			return encodedImage.SelectMany(r => r).ToArray();
		}
		private void ReadPNG(Stream rawdata)
		{
			Png image = Png.Open(rawdata);

			palette = new();
			pixelcolors = new();
			infoWidth = image.Width;
			infoHeight = image.Height - 1;

			Dictionary<Pixel, byte> palettes = new();
			//List<(Pixel, Pixel)> palettes = new();

			for (int i = 0; i < 4; i++)
			{
				var pixel = image.GetPixel(infoWidth - 4 + i, infoHeight);
				palettes.Add(pixel, (byte)i);
			}

			byte[] rawimage = new byte[infoWidth * infoHeight];

			for (int y = 0; y < infoHeight; y++)
			{
				for (int x = 0; x < infoWidth; x++)
				{
					var pixel = image.GetPixel(x, y);
					if (palettes.TryGetValue(pixel, out var rawbyte))
					{
						rawimage[y * infoWidth + x] = rawbyte;
					}
				}
			}

			data = EncodeImage2bpp(rawimage);
		}
		public void ReadPNGFile()
		{
			//string metadatayaml = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("noobimage.png"));
			using (Stream imagefile = assembly.GetManifestResourceStream(filepath))
			{
				ReadPNG(imagefile);
			}
		}
		public void WriteAt(int bank, int offset, FFMQRom rom)
		{
			rom.PutInBank(bank, offset, data);
		}
		private byte[] EncodeTile((int x, int y) tilePosition, int paletteid)
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
		private byte[] EncodeTile(byte[] tile, int paletteid)
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

		private List<byte[]> EncodeSeries((int x, int y) startTile, int height, int paletteid)
		{
			List<byte[]> encodedSeries = new();

			for (int i = 0; i < height; i++)
			{
				encodedSeries.Add(EncodeTile((startTile.x, startTile.y + (i * 8)), paletteid));
				encodedSeries.Add(EncodeTile((startTile.x + 8, startTile.y + (i * 8)), paletteid));
			}

			return encodedSeries;
		}
		public DarkKingSpriteDataPack EncodeDarkKing(byte[] dksprite)
		{
			data = dksprite;
			
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
		public PlayerSpriteDataPack EncodePlayerSprite(PlayerSprite playersprite)
		{
			LoadCustomSprites(playersprite);

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

			// Get Full Horizontal Flip flag
			byte fullhorizontalflipbyte = data[dataOffset + (2 * infoWidth) - 2];


			PlayerSpriteDataPack playerSpriteDataPack = new()
			{
				SoftBopEnabled = (softbopbyte != emptyPixel),
				FullHorizontalFlipEnabled = (fullhorizontalflipbyte != emptyPixel),
				WalkingSeriesEncoded = EncodeSeries((0, 0), 8, 0),
				PushSeriesEncoded = EncodeSeries((16, 0), 8, 0),
				JumpSeriesEncoded = EncodeSeries((32, 0), 6, 0),
				VictorySeriesEncoded = EncodeSeries((48, 0), 4, 0),
				ThrowDeathSeriesEncoded = EncodeSeries((64, 0), 8, 0),
				BombShrugSeriesEncoded = EncodeSeries((80, 0), 4, 0),
				ShrugHandEncoded = EncodeTile((96, 24), 0),
				ClimbSeriesEncoded = EncodeSeries((80, 32), 3, 0),
				Palette = finalPalette
			};

			return playerSpriteDataPack;
		}
		private void LoadCustomSprites(PlayerSprite sprite)
		{
			if (sprite.filename == "default")
			{
				return;
			}
			else
			{
				data = sprite.spritesheet;
			}
		}
	}
}
