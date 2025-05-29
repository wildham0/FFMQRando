using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using static System.Math;
using System.Threading.Tasks.Dataflow;
using System.Drawing;
using System.Text.Json;

namespace FFMQLib
{

	public class SnesColor
	{ 
		public int Red { get; set; }
		public int Green { get; set; }
		public int Blue { get; set; }
		public SnesColor(int red, int green, int blue)
		{
			Red = Min(red, 31);
			Green = Min(green, 31);
			Blue = Min(blue, 31);
		}
		public SnesColor(byte[] color)
		{
			Red = color[0] & 0x1F;
			Green = ((color[0] & 0xE0) / 32) + ((color[1] & 0x03) * 8);
			Blue = (color[1] & 0x7C) / 4;
		}
		public SnesColor(ushort color)
		{
			Red = color & 0x001F;
			Green = (color & 0x03E0) / 32;
			Blue = (color & 0x7C00) / 32 / 32;
		}
		public byte[] GetBytes()
		{
			Red = Min(Red, 31);
			Green = Min(Green, 31);
			Blue = Min(Blue, 31);

			return new byte[] {
				(byte)(((Green * 32) & 0xE0) + Red),
				(byte)(Blue * 4 + Green / 8),
			};
		}
		public ushort GetUshort()
		{
			Red = Min(Red, 31);
			Green = Min(Green, 31);
			Blue = Min(Blue, 31);

			return (ushort)(Red + (Green * 32) + (Blue * 32 * 32));
		}
		public void Copy(SnesColor color)
		{
			Red = color.Red;
			Green = color.Green;
			Blue = color.Blue;
		}
	}
	public class Palette
	{ 
		public List<SnesColor> Colors { get; set; }
		//public List<Color> RgbColors => Colors.Select(c => Color.FromArgb(c.Red * 8, c.Green * 8, c.Blue * 8)).ToList();
		public Palette(byte[] palette)
		{
			Colors = palette.Chunk(2).Select(x => new SnesColor(x)).ToList();
		}
		public byte[] GetBytes()
		{
			return Colors.SelectMany(x => x.GetBytes()).ToArray();
		}
	}
	public class MapPalettes
	{
		public List<Palette> Palettes { get; set; }
		public const int MapPalettesBank = 0x05;
		public const int MapPalettesOffset = 0x8000;
		public const int MapPalettesQty = 0x8000;
		public const int NewMapPalettesBank = 0x12;
		public const int NewMapPalettesOffset = 0xD000;
		public const int HillOfDestinyPaletteBank = 0x07;
		public const int HillOfDestinyPaletteOffset = 0xD984;

		public MapPalettes(FFMQRom rom)
		{
			Palettes = rom.GetFromBank(MapPalettesBank, MapPalettesOffset, 0x80 * 0x19).Chunk(0x80).Select(x => new Palette(x)).ToList();
			Palettes.Add(new Palette(rom.GetFromBank(HillOfDestinyPaletteBank, HillOfDestinyPaletteOffset, 0x80)));
		}
		public string ExportToJson()
		{
			List<List<SnesColor>> rgbPalettes = Palettes.Select(p => p.Colors).ToList();

			return JsonSerializer.Serialize(rgbPalettes);
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(NewMapPalettesBank, NewMapPalettesOffset, Palettes.SelectMany(p => p.GetBytes()).ToArray());

			// Don't branch on palette $19
			rom.PutInBank(0x0B, 0x83BC, Blob.FromHex("FF"));

			// Redirect to load from new bank
			rom.PutInBank(0x0B, 0x83CA, Blob.FromUShorts(new ushort[] { NewMapPalettesOffset }));
			rom.PutInBank(0x0B, 0x83DA, Blob.FromSBytes(new sbyte[] { NewMapPalettesBank }));
		}
	}



}
