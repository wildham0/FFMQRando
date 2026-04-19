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
using System.Net.Http.Headers;

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
		public int Width { get; }
		public int Height { get; }
		public List<(byte position, Pixel pixel)> Palette { get; }
		public CommonImage(Png data)
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
					image.Add(data.GetPixel(x,y));
				}
			}
		}

		// From BMP
		public CommonImage(byte[] data)
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

			for (int i = 0; i < colorCount; i++)
			{
				int lowerrange = infoColortable + i * 4;
				int upperrange = lowerrange + 4;

				byte[] color = header[lowerrange..upperrange];

				Palette.Add(((byte)i, new Pixel(color[0], color[1], color[2])));
			}

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (Palette.TryFind(p => p.position == rawimage[y * Width + x], out var pixel))
					{
						image.Add(pixel.pixel);
					}
					else
					{
						image.Add(new Pixel(0,0,0));
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
		private byte[] dataz;
		private byte[] metadata;
		private byte[] image;

		private const int infoDataOffset = 0x0A; // 4 bytes
		private const int infoColorsUsed = 0x2E; // 4 bytes
		private const int infoColortable = 0x36;

		private int infoWidth;
		private int infoHeight;
		private int paletteCount;

		private int dataOffset;
		private int colorCount;

		private EncodingModes encodingMode;

		private List<(byte pixelid, byte[] snesrgb)> palette;
		private List<List<(byte pixelid, byte position)>> pixelcolors;
		private List<List<(Pixel pixel, byte position)>> pixelPalettes;
		private List<List<byte>> palettes;

		private List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };

		private List<List<byte>> ReadPalettes(CommonImage image, int palettecount, EncodingModes mode)
		{
			List<List<byte>> palettes = new();
			pixelPalettes = new();

			int length = mode == EncodingModes.m2bpp ? 4 : 8;

			for (int p = 0; p < palettecount; p++)
			{
				List<byte> temppalette = new List<byte> { 0x00, 0x00 };
				List<(byte, byte)> temppixels = new();
				List<(Pixel pixel, byte position)> currentPixelPalette = new();

				for (int i = 0; i < length; i++)
				{
					var pixel = image.GetPixel(image.Width - length - (p * length) + i, image.Height);
					currentPixelPalette.Add((pixel, (byte)i));
					
					if (i > 0)
					{
						temppalette.AddRange(GetSnesPalette(pixel));
					}
				}
				pixelPalettes.Add(currentPixelPalette);
				palettes.Add(temppalette);
			}

			return palettes;
		}

		private void ConvertToBytes(CommonImage image)
		{

			int width = image.Width / 8;
			int height = image.Height / 8;

			List<Pixel> emptyPixels = pixelPalettes.Select(pal => pal.Find(p => p.position == 0).pixel).ToList();

			byte[] drawingarray = new byte[(width * height) / 8];
			byte[] palettearray = new byte[(width * height) / 8];
			byte[] rawimage = new byte[width * height];

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
						drawingarray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						palettearray[arrayposition] |= bitmask[bitmaskposition];
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
						drawingarray[arrayposition] |= bitmask[bitmaskposition];
						if (palettecandidate == 0)
						{
							palettearray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						}
						else
						{
							palettearray[arrayposition] |= bitmask[bitmaskposition];
						}

						// Convert image
						for (int i = 0; i < 8; i++)
						{
							for (int j = 0; j < 8; j++)
							{
								if (pixelPalettes[palettecandidate].TryFind(p => p.pixel.Equals(image.GetPixel(x * 8 + i, y * 8 + j)), out var pixel))
								{
									rawimage[(y * 8 + j) * (height * 8) + (x * 8 + i)] = pixel.position;
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


		private void ProcessBmp(byte[] bmpdata)
		{
			dataOffset = bmpdata[infoDataOffset] + (bmpdata[infoDataOffset + 1] * 0x100) + (bmpdata[infoDataOffset + 2] * 0x100 * 0x100) + (bmpdata[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			colorCount = bmpdata[infoColorsUsed] + (bmpdata[infoColorsUsed + 1] * 0x100) + (bmpdata[infoColorsUsed + 2] * 0x100 * 0x100) + (bmpdata[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

			metadata = bmpdata[0..dataOffset];
			image = bmpdata[dataOffset..];

			palette = new();
			pixelcolors = new();

			palettes = ReadBmpPalettes(paletteCount);
		}



		private void ProcessPng(Png pngdata)
		{
			//palette = new();
			pixelcolors = new();


			//List<(Pixel, Pixel)> palettes = new();

			//
			Dictionary<Pixel, byte> palette = new();
			int paletteLength = encodingMode == EncodingModes.m2bpp ? 4 : 8;

			for (int i = 0; i < paletteLength; i++)
			{
				var pixel = pngdata.GetPixel(infoWidth - paletteLength + i, infoHeight);
				palette.Add(pixel, (byte)i);
			}

			byte[] rawimage = new byte[infoWidth * infoHeight];

			for (int y = 0; y < infoHeight; y++)
			{
				for (int x = 0; x < infoWidth; x++)
				{
					pngdata.Header.
					var pixel = pngdata.GetPixel(x, y);
					pixel.
					if (palette.TryGetValue(pixel, out var rawbyte))
					{
						rawimage[y * infoWidth + x] = rawbyte;
					}
				}
			}

			image = EncodeImage2bpp(rawimage);


			//bmpdata
			dataOffset = bmpdata[infoDataOffset] + (bmpdata[infoDataOffset + 1] * 0x100) + (bmpdata[infoDataOffset + 2] * 0x100 * 0x100) + (bmpdata[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			colorCount = bmpdata[infoColorsUsed] + (bmpdata[infoColorsUsed + 1] * 0x100) + (bmpdata[infoColorsUsed + 2] * 0x100 * 0x100) + (bmpdata[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

			metadata = bmpdata[0..dataOffset];
			image = bmpdata[dataOffset..];

			palette = new();
			pixelcolors = new();

			palettes = ReadBmpPalettes(paletteCount);
		}


		private byte[] GetTile((int x, int y) tilePosition)
		{
			byte[] tilepixels = new byte[8*8];

			for (int y = 0; y < 8; y++)
			{
				//int linestart = dataOffset + ((infoHeight - tilePosition.y - 1 - y) * infoWidth) + tilePosition.x;
				int linestart = ((infoHeight - tilePosition.y - 1 - y) * infoWidth) + tilePosition.x;
				for (int x = 0; x < 8; x++)
				{
					tilepixels[(y * 8) + x] = image[linestart + x];
				}
			}

			return tilepixels;
		}


		private List<List<byte>> ReadBmpPalettes(int palettecount)
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

				palette.Add(((byte)i, GetSnesPalette(metadata[lowerrange..upperrange])));
			}

			List<List<byte>> finalpalettes = new();

			for (int p = 0; p < palettecount; p++)
			{
				List<byte> tempalette = new();
				tempalette.AddRange(new List<byte> { 0x00, 0x00 });
				List<(byte, byte)> temppixels = new();


				for (int i = 0; i < 8; i++)
				{
					//byte pixelvalue = data[dataOffset + infoWidth - 8 - (p * 8) + i];
					byte pixelvalue = image[infoWidth - 8 - (p * 8) + i];

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
		private List<List<byte>> ReadPngPalettes(int palettecount)
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

				palette.Add(((byte)i, GetSnesPalette(metadata[lowerrange..upperrange])));
			}

			List<List<byte>> finalpalettes = new();

			for (int p = 0; p < palettecount; p++)
			{
				List<byte> tempalette = new();
				tempalette.AddRange(new List<byte> { 0x00, 0x00 });
				List<(byte, byte)> temppixels = new();


				for (int i = 0; i < 8; i++)
				{
					//byte pixelvalue = data[dataOffset + infoWidth - 8 - (p * 8) + i];
					byte pixelvalue = image[infoWidth - 8 - (p * 8) + i];

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
		private byte[] GetSnesPalette(Pixel pixel)
		{
			return new byte[] {
				(byte)((((pixel.G / 8) * 32) & 0xE0) + (pixel.B / 8)),
				(byte)(((pixel.R / 8) * 4) + ((pixel.G / 8) / 8)) };
		}
		private byte[] EncodeLine(byte[] pixelline, int paletteid)
		{
			List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

			byte[] encodedline = new byte[3];
			for (int i = 0; i < pixelline.Length; i++)
			{
				byte pixelpalette;

				if (pixelcolors[paletteid].Select(x => x.pixelid).Contains(pixelline[i]))
				{
					pixelpalette = pixelcolors[paletteid].Find(x => x.pixelid == pixelline[i]).position;
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
				ReadPNG(imagefile);
			}
		}
		public void WriteAt(int bank, int offset, FFMQRom rom)
		{
			rom.PutInBank(bank, offset, image);
		}
		private byte[] EncodeTile((int x, int y) tilePosition, int paletteid)
		{
			byte[] encodedTile = new byte[0x18];

			for (int i = 0; i < 8; i++)
			{
				int linestart = ((infoHeight - tilePosition.y - 1 - i) * infoWidth) + tilePosition.x;
				//int linestart = dataOffset + ((infoHeight - tilePosition.y - 1 - i) * infoWidth) + tilePosition.x;
				var targetLine = image[linestart..(linestart + 8)];
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
		private void LoadDarkKingBmpData(byte[] dksprite)
		{ 
		
		}

		public DarkKingSpriteDataPack EncodeDarkKing(byte[] dksprite)
		{

			infoWidth = 28 * 8;
			infoHeight = 10 * 8 + 1;
			paletteCount = 2;

			ProcessBmp(dksprite);
			palettes = ReadBmpPalettes(2);

			//data = dksprite;
			/*
			dataOffset = dksprite[infoDataOffset] + (dksprite[infoDataOffset + 1] * 0x100) + (dksprite[infoDataOffset + 2] * 0x100 * 0x100) + (dksprite[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
			colorCount = dksprite[infoColorsUsed] + (dksprite[infoColorsUsed + 1] * 0x100) + (dksprite[infoColorsUsed + 2] * 0x100 * 0x100) + (dksprite[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

			metadata = dksprite[0..dataOffset];
			image = dksprite[dataOffset..];

			palette = new();
			pixelcolors = new();
			infoWidth = 28 * 8;
			infoHeight = 10 * 8 + 1;

			List<List<byte>> finalpalettes = ReadBmpPalettes(2);*/

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
						// Find most appropriate palette
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

						// Update drawing arrays
						drawingarray[arrayposition] |= bitmask[bitmaskposition];
						if (palettecandidate == 0)
						{
							palettearray[arrayposition] &= (byte)~bitmask[bitmaskposition];
						}
						else
						{
							palettearray[arrayposition] |= bitmask[bitmaskposition];
						}

						// Encode tile and add it to list
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
				Palette1 = palettes[1],
				Palette2 = palettes[0],
				EncodedTiles = encodedTiles
			};

			return result;
		}
		public PlayerSpriteDataPack EncodePlayerSprite(PlayerSprite playersprite)
		{
			LoadCustomSprites(playersprite);

			infoWidth = 104;
			infoHeight = 64;
			paletteCount = 1;

			ProcessBmp(playersprite.spritesheet);

			/*
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
			*/

			byte emptyPixel = pixelcolors[0].Find(p => p.position == 0).pixelid;

			// Get Software Bop flag
			byte softbopbyte = image[(2 * infoWidth) - 1];

			// Get Full Horizontal Flip flag
			byte fullhorizontalflipbyte = image[(2 * infoWidth) - 2];


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
				Palette = palettes[0]
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
