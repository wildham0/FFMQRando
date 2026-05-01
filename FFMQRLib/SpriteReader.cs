using BigGustave;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFMQLib
{
	public enum EncodingModes
	{ 
		m2bpp,
		m3bpp
	}
	public class CommonImage
	{
		private const int infoDataOffset = 0x0A; // 4 bytes
		private const int infoColorsUsed = 0x2E; // 4 bytes
		private const int infoColortable = 0x36;
		private const int infoWidth = 0x12;
		private const int infoHeight = 0x16;

		private List<Pixel> image;
		private byte[] header;
		public int Width { get; set; }
		public int Height { get; set; }
		public List<(byte position, Pixel pixel)> Palette { get; set;  }
		public CommonImage(Png data)
		{
			ProcessPng(data);
		}

		// From BMP
		public CommonImage(byte[] data)
		{
			byte[] pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
			byte[] bmpSignature = [0x42, 0x4D];

			bool isBmp = true;
			bool isPng = true;

			for (int i = 0; i < 8; i++)
			{
				if (data[i] != pngSignature[i])
				{
					isPng = false;
				}

				if (i < 2 && data[i] != bmpSignature[i])
				{
					isBmp = false;
				}
			}

			if (isPng)
			{
				Stream pngStream = new MemoryStream(data);
				ProcessPng(Png.Open(pngStream));
			}
			else if (isBmp)
			{
				ProcessBmp(data);
			}
			else
			{
				throw new Exception("CommonImage: Trying to decode invalid image file.");
			}
		}
		private void ProcessPng(Png data)
		{
			Palette = new();
			Width = data.Width;
			Height = data.Height;

			header = Array.Empty<byte>();
			image = new();

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					image.Add(data.GetPixel(x, y));
				}
			}
		}
		private void ProcessBmp(byte[] data)
		{
			var dataOffset = data[infoDataOffset] + (data[infoDataOffset + 1] * 0x100) + (data[infoDataOffset + 2] * 0x100 * 0x100) + (data[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			var colorCount = data[infoColorsUsed] + (data[infoColorsUsed + 1] * 0x100) + (data[infoColorsUsed + 2] * 0x100 * 0x100) + (data[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);
			Width = data[infoWidth] + (data[infoWidth + 1] * 0x100) + (data[infoWidth + 2] * 0x100 * 0x100) + (data[infoWidth + 3] * 0x100 * 0x100 * 0x100);
			Height = data[infoHeight] + (data[infoHeight + 1] * 0x100) + (data[infoHeight + 2] * 0x100 * 0x100) + (data[infoHeight + 3] * 0x100 * 0x100 * 0x100);

			header = data[0..dataOffset];
			var rawimage = data[dataOffset..];

			// Read all the BMP colors
			if (colorCount == 0)
			{
				colorCount = 0x100;
			}

			Palette = new();
			image = new();

			for (int i = 0; i < colorCount; i++)
			{
				int lowerrange = infoColortable + i * 4;
				int upperrange = lowerrange + 4;

				byte[] color = header[lowerrange..upperrange];

				//BGR
				//Palette.Add(((byte)i, new Pixel(color[0], color[1], color[2])));
				Palette.Add(((byte)i, new Pixel(color[2], color[1], color[0])));
			}

			for (int y = (Height - 1); y >= 0; y--)
			{
				for (int x = 0; x < Width; x++)
				{
					if (Palette.TryFind(p => p.position == rawimage[y * Width + x], out var pixel))
					{
						image.Add(pixel.pixel);
					}
					else
					{
						image.Add(new Pixel(0, 0, 0));
					}
				}
			}
		}
		public Pixel GetPixel(int x, int y)
		{
			return image[y * Width + x];
		}
	}

	public class SpriteReader
	{
		private byte[] rawImage;
		private CommonImage image;
		private int width => image.Width;
		private int height => image.Height;

		private byte[] drawingArray;
		private byte[] paletteArray;

		private List<List<(Pixel pixel, byte position)>> pixelPalettes;
		private List<List<byte>> palettes;

		private List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };

		private void ReadPalettes(CommonImage image, int palettecount, EncodingModes mode)
		{
			palettes = new();
			pixelPalettes = new();

			int length = mode == EncodingModes.m2bpp ? 4 : 8;

			for (int p = 0; p < palettecount; p++)
			{
				List<byte> temppalette = new List<byte> { 0x00, 0x00 };
				List<(byte, byte)> temppixels = new();
				List<(Pixel pixel, byte position)> currentPixelPalette = new();

				for (int i = 0; i < length; i++)
				{
					var pixel = image.GetPixel(image.Width - length - (p * length) + i, image.Height - 1);
					currentPixelPalette.Add((pixel, (byte)i));
					
					if (i > 0)
					{
						temppalette.AddRange(GetSnesPalette(pixel));
					}
				}
				pixelPalettes.Add(currentPixelPalette);
				palettes.Add(temppalette);
			}
		}
		private void ConvertDkToBytes(CommonImage image)
		{
			int width = image.Width / 8;
			int height = image.Height / 8;

			List<Pixel> emptyPixels = pixelPalettes.Select(pal => pal.Find(p => p.position == 0).pixel).ToList();

			drawingArray = new byte[(width * height) / 8];
			paletteArray = new byte[(width * height) / 8];
			rawImage = new byte[image.Width * image.Height];

			int bitmaskposition = 0;
			int arrayposition = 0;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Get a tile
					List<Pixel> currentTile = new();
					for (int i = 0; i < 8; i++)
					{
						for (int j = 0; j < 8; j++)
						{
							currentTile.Add(image.GetPixel(x * 8 + i, y * 8 + j));
						}
					}

					// Empty tile?
					if (currentTile.Where(emptyPixels.Contains).ToList().Count >= 64)
					{
						drawingArray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						paletteArray[arrayposition] |= bitmask[bitmaskposition];
					}
					else // Not empty
					{
						// Find most appropriate palette
						int palettecandidate = 0;
						int matchcount = 0;

						for (int i = 0; i < pixelPalettes.Count; i++)
						{
							var currenmatchcount = pixelPalettes[i].Select(p => p.pixel).Intersect(currentTile).ToList().Count;

							if (currenmatchcount > matchcount)
							{
								matchcount = currenmatchcount;
								palettecandidate = i;
							}
						}

						// Update drawing arrays
						drawingArray[arrayposition] |= bitmask[bitmaskposition];
						if (palettecandidate == 0)
						{
							paletteArray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						}
						else
						{
							paletteArray[arrayposition] |= bitmask[bitmaskposition];
						}

						// Convert image
						for (int i = 0; i < 8; i++)
						{
							for (int j = 0; j < 8; j++)
							{
								if (pixelPalettes[palettecandidate].TryFind(p => p.pixel.Equals(image.GetPixel(x * 8 + j, y * 8 + i)), out var pixel))
								{
									rawImage[(y * 8 + i) * (width * 8) + (x * 8 + j)] = pixel.position;
								}
							}
						}
					}

					bitmaskposition++;

					if (bitmaskposition >= 8)
					{
						arrayposition++;
						bitmaskposition = 0;
					}
				}
			}
		}
		private void ConvertPlayerToBytes(CommonImage image)
		{
			List<Pixel> emptyPixels = pixelPalettes.Select(pal => pal.Find(p => p.position == 0).pixel).ToList();

			drawingArray = new byte[0];
			paletteArray = new byte[0];
			rawImage = new byte[image.Width * image.Height];

			// Convert image
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					if (pixelPalettes[0].TryFind(p => p.pixel.Equals(image.GetPixel(x, y)), out var pixel))
					{
						rawImage[y * image.Width + x] = pixel.position;
					}
				}
			}
		}
		private byte[] GetTile((int x, int y) tilePosition)
		{
			byte[] tilepixels = new byte[8*8];

			for (int y = 0; y < 8; y++)
			{
				int linestart = ((tilePosition.y + y) * width) + tilePosition.x;
				for (int x = 0; x < 8; x++)
				{
					tilepixels[(y * 8) + x] = rawImage[linestart + x];
				}
			}

			return tilepixels;
		}
		private byte[] GetSnesPalette(Pixel pixel)
		{
			return new byte[] {
				(byte)((((pixel.G / 8) * 32) & 0xE0) + (pixel.R / 8)),
				(byte)(((pixel.B / 8) * 4) + ((pixel.G / 8) / 8)) };
		}

		// 2bpp images, to fix
		/*
		private byte[] EncodeLine2bpp(byte[] pixelline)
		{
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
			Png pngimage = Png.Open(rawdata);


			Png.
			palette = new();
			pixelcolors = new();
			infoWidth = pngimage.Width;
			infoHeight = pngimage.Height - 1;

			Dictionary<Pixel, byte> palettes = new();
			//List<(Pixel, Pixel)> palettes = new();

			for (int i = 0; i < 4; i++)
			{
				var pixel = pngimage.GetPixel(infoWidth - 4 + i, infoHeight);
				palettes.Add(pixel, (byte)i);
			}

			byte[] rawimage = new byte[infoWidth * infoHeight];

			for (int y = 0; y < infoHeight; y++)
			{
				for (int x = 0; x < infoWidth; x++)
				{
					var pixel = pngimage.GetPixel(x, y);
					if (palettes.TryGetValue(pixel, out var rawbyte))
					{
						rawimage[y * infoWidth + x] = rawbyte;
					}
				}
			}

			image = EncodeImage2bpp(rawimage);
		}
		public void ReadPNGFile()
		{
			//string metadatayaml = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("noobimage.png"));
			using (Stream imagefile = assembly.GetManifestResourceStream(filepath))
			{
				//ReadPNG(imagefile);
			}
		}
		public void WriteAt(int bank, int offset, FFMQRom rom)
		{
			//rom.PutInBank(bank, offset, image);
		}*/

		private List<byte[]> EncodeSeries((int x, int y) startTile, int height)
		{
			List<byte[]> encodedSeries = new();

			for (int i = 0; i < height; i++)
			{
				encodedSeries.Add(EncodeTile((startTile.x, startTile.y + (i * 8))));
				encodedSeries.Add(EncodeTile((startTile.x + 8, startTile.y + (i * 8))));
			}

			return encodedSeries;
		}
		private byte[] EncodeTile((int x, int y) tilePosition)
		{
			byte[] encodedTile = new byte[0x18];

			for (int i = 0; i < 8; i++)
			{
				int linestart = ((tilePosition.y + i) * width) + tilePosition.x;
				var targetLine = rawImage[linestart..(linestart + 8)];
				var encodedline = EncodeLine(targetLine);
				encodedTile[(i * 2)] = encodedline[0];
				encodedTile[(i * 2) + 1] = encodedline[1];
				encodedTile[i + 0x10] = encodedline[2];
			}

			return encodedTile;
		}
		private byte[] EncodeTile(byte[] tile)
		{
			byte[] encodedTile = new byte[0x18];

			for (int i = 0; i < 8; i++)
			{
				int linestart = i * 8;
				var targetLine = tile[linestart..(linestart + 8)];
				var encodedline = EncodeLine(targetLine);
				encodedTile[(i * 2)] = encodedline[0];
				encodedTile[(i * 2) + 1] = encodedline[1];
				encodedTile[i + 0x10] = encodedline[2];
			}

			return encodedTile;
		}
		private byte[] EncodeLine(byte[] pixelline)
		{
			List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

			byte[] encodedline = new byte[3];
			for (int i = 0; i < pixelline.Length; i++)
			{
				byte pixelpalette = pixelline[i];

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
		public DarkKingSpriteDataPack EncodeDarkKing(CommonImage dksprite)
		{
			image = dksprite;
			ReadPalettes(dksprite, 2, EncodingModes.m3bpp);
			ConvertDkToBytes(dksprite);

			List<byte[]> encodedTiles = new();

			for (int y = 0; y < 10; y++)
			{

				for (int x = 0; x < 28; x++)
				{
					var currentile = GetTile((x * 8, y * 8));

					// Empty tile?
					if (currentile.Where(t => t == 0).ToList().Count < 64)
					{
						// Encode tile and add it to list
						encodedTiles.Add(EncodeTile(currentile));
					}
				}
			}

			DarkKingSpriteDataPack result = new()
			{
				DrawingArray = drawingArray,
				PaletteArray = paletteArray,
				Palette1 = palettes[1],
				Palette2 = palettes[0],
				EncodedTiles = encodedTiles
			};

			return result;
		}
		public PlayerSpriteDataPack EncodePlayerSprite(PlayerSprite playersprite)
		{
			image = playersprite.imagedata;
			ReadPalettes(playersprite.imagedata, 1, EncodingModes.m3bpp);
			ConvertPlayerToBytes(playersprite.imagedata);
			
			// Get Empty Pixel
			byte emptyPixel = 0x00;

			// Get Software Bop flag
			byte softbopbyte = rawImage[(height - 1) * width - 1];

			// Get Full Horizontal Flip flag
			byte fullhorizontalflipbyte = rawImage[(height - 1) * width - 2];

			PlayerSpriteDataPack playerSpriteDataPack = new()
			{
				SoftBopEnabled = (softbopbyte != emptyPixel),
				FullHorizontalFlipEnabled = (fullhorizontalflipbyte != emptyPixel),
				WalkingSeriesEncoded = EncodeSeries((0, 0), 8),
				PushSeriesEncoded = EncodeSeries((16, 0), 8),
				JumpSeriesEncoded = EncodeSeries((32, 0), 6),
				VictorySeriesEncoded = EncodeSeries((48, 0), 4),
				ThrowDeathSeriesEncoded = EncodeSeries((64, 0), 8),
				BombShrugSeriesEncoded = EncodeSeries((80, 0), 4),
				ShrugHandEncoded = EncodeTile((96, 24)),
				ClimbSeriesEncoded = EncodeSeries((80, 32), 3),
				Palette = palettes[0]
			};

			return playerSpriteDataPack;
		}
	}
}
