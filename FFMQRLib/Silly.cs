using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void RandomBenjaminPalette(Preferences preferences, MT19337 rng)
		{
			if (!preferences.RandomBenjaminPalette)
			{
				return;
			}

			List<(int red, int green, int blue)> skinTones = new()
			{
				(17, 10, 4),
				(24, 16, 8),
				(28, 21, 13),
				(30, 24, 15),
				(31, 27, 21),
			};

			(int red, int green, int blue) skinColor = rng.PickFrom(skinTones);
			(int red, int green, int blue) hairColor = (rng.Between(0, 31), rng.Between(0, 31), rng.Between(0, 31));

			while (hairColor == skinColor)
			{
				hairColor = (rng.Between(0, 31), rng.Between(0, 31), rng.Between(0, 31));
			}

			(int red, int green, int blue) highlightColor = (Math.Min(hairColor.red + 3, 31), Math.Min(hairColor.green + 3, 31), Math.Min(hairColor.blue + 3, 31));

			(int red, int green, int blue) armorMidColor = (rng.Between(0, 31), rng.Between(0, 31), rng.Between(0, 31));

			while (armorMidColor == skinColor || armorMidColor == hairColor)
			{
				armorMidColor = (rng.Between(0, 31), rng.Between(0, 31), rng.Between(0, 31));
			}

			(int red, int green, int blue) armorLightColor = (Math.Min(armorMidColor.red + 3, 31), Math.Min(armorMidColor.green + 3, 31), Math.Min(armorMidColor.blue + 3, 31));
			(int red, int green, int blue) armorDarkColor = (Math.Max(armorMidColor.red - 7, 0), Math.Max(armorMidColor.green - 7, 0), Math.Max(armorMidColor.blue - 7, 0));


			List<byte> palette = new() { 
				(byte)(((skinColor.green * 32) & 0xE0) + skinColor.red),
				(byte)(skinColor.blue * 4 + skinColor.green / 8),
				
				(byte)(((highlightColor.green * 32) & 0xE0) + highlightColor.red),
				(byte)(highlightColor.blue * 4 + highlightColor.green / 8),
				
				(byte)(((hairColor.green * 32) & 0xE0) + hairColor.blue),
				(byte)(hairColor.blue * 4 + hairColor.green / 8),
				
				(byte)(((armorLightColor.green * 32) & 0xE0) + armorLightColor.red),
				(byte)(armorLightColor.blue * 4 + armorLightColor.green / 8),
				
				(byte)(((armorMidColor.green * 32) & 0xE0) + armorMidColor.red),
				(byte)(armorMidColor.blue * 4 + armorMidColor.green / 8),
				
				(byte)(((armorDarkColor.green * 32) & 0xE0) + armorDarkColor.red),
				(byte)(armorDarkColor.blue * 4 + armorDarkColor.green / 8),
			};

			PutInBank(0x07, 0xD828, palette.ToArray());
		}
	}
}
