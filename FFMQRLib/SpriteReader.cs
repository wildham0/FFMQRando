using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;




namespace FFMQLib
{
    public class SpriteReader
    {
        private byte[] data;

        private const int infoWidth = 104;
        private const int infoHeight = 64;
        private const int infoDataOffset = 0x0A; // 4 bytes
        private const int infoColorsUsed = 0x2E; // 4 bytes
        private const int infColortable = 0x36;

        private int dataOffset;
        private int colorCount;

        private List<(byte, byte[])> palette;
        private List<(byte, byte)> pixelcolors;

        public void Encode(FFMQRom rom)
        {

            ReadData();

            dataOffset = data[infoDataOffset] + (data[infoDataOffset + 1] * 0x100) + (data[infoDataOffset + 2] * 0x100 * 0x100) + (data[infoDataOffset + 3] * 0x100 * 0x100 * 0x100);
            colorCount = data[infoColorsUsed] + (data[infoColorsUsed + 1] * 0x100) + (data[infoColorsUsed + 2] * 0x100 * 0x100) + (data[infoColorsUsed + 3] * 0x100 * 0x100 * 0x100);

            palette = new();
            pixelcolors = new();

            for (int i = 0; i< colorCount; i++)
            {
                int lowerrange = infColortable + i * 4;
                int upperrange = lowerrange + 4;

                palette.Add(((byte)i, GetSnesPalette(data[lowerrange..upperrange])));
            }

            List<byte> finalPalette = new();
            finalPalette.AddRange(new List<byte> { 0x00, 0x00 });

            for (int i = 0; i < 8; i++)
            {
                byte pixelvalue = data[dataOffset + infoWidth - 8 + i];

                pixelcolors.Add((pixelvalue, (byte)i));

                if (i > 0)
                {
                    finalPalette.AddRange(palette[pixelvalue].Item2);
                }
            }

            List<(int, int)> walkTiles = new() {
                (0, 0),
                (8, 0),
                (0, 8),
                (8, 8),
                (0, 16),
                (8, 16),
                (0, 24),
                (8, 24),
                (0, 32),
                (8, 32),
                (0, 40),
                (8, 40),
                (0, 48),
                (8, 48),
                (0, 56),
                (8, 56),
            };

            List<(int, int)> pushTiles = new() {
                (16, 0),
                (24, 0),
                (16, 8),
                (24, 8),
                (16, 16),
                (24, 16),
                (16, 24),
                (24, 24),
                (16, 32),
                (24, 32),
                (16, 40),
                (24, 40),
                (16, 48),
                (24, 48),
                (16, 56),
                (24, 56),
            };


            List<byte[]> walkseriesEncoded = new();
            foreach (var tile in walkTiles)
            {
                walkseriesEncoded.Add(EncodeTile(tile));
            }

            List<byte[]> pushseriesEncoded = new();
            foreach (var tile in pushTiles)
            {
                pushseriesEncoded.Add(EncodeTile(tile));
            }

            rom.PutInBank(0x04, 0x9A20, walkseriesEncoded.SelectMany(x => x).ToArray());
            rom.PutInBank(0x04, 0xCA20, pushseriesEncoded.SelectMany(x => x).ToArray());
            rom.PutInBank(0x07, 0xD824, finalPalette.ToArray());
        }

        private void ReadData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("fighter.bmp"));
            using (Stream imagefile = assembly.GetManifestResourceStream(filepath))
            {
                using (BinaryReader reader = new BinaryReader(imagefile))
                {
                    data = reader.ReadBytes((int)imagefile.Length);
                }
            }
        }
        private byte[] GetSnesPalette(byte[] rgbvalues)
        {
            return new byte[] {
                (byte)((((rgbvalues[1] / 8) * 32) & 0xE0) + (rgbvalues[2] / 8)),
                (byte)(((rgbvalues[0] / 8) * 4) + ((rgbvalues[1] / 8) / 8)) };
        }

        private byte[] EncodeLine(byte[] pixelline)
        {
            List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };
            List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

            byte[] encodedline = new byte[3];
            for(int i = 0; i < pixelline.Length; i++)
            {
                byte pixelpalette;

                if (pixelcolors.Select(x => x.Item1).Contains(pixelline[i]))
                {
                    pixelpalette = pixelcolors.Find(x => x.Item1 == pixelline[i]).Item2;
                }
                else
                {
                    pixelpalette = 0;
                }

                if((pixelpalette & palettemask[0]) > 0)
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

        private byte[] EncodeTile((int x, int y) tilePosition)
        {
            List<byte> bitmask = new() { 0x80, 0x40, 0x20, 0x10, 0x08, 0x4, 0x02, 0x01 };
            List<byte> palettemask = new() { 0x01, 0x02, 0x04 };

            byte[] encodedTile = new byte[0x18];

            for (int i = 0; i < 8; i++)
            {
                int linestart = dataOffset + ((infoHeight - tilePosition.y - 1 - i) * infoWidth) + tilePosition.x;
                var targetLine = data[linestart..(linestart + 8)];
                var encodedline = EncodeLine(targetLine);
                encodedTile[(i * 2)] = encodedline[0];
                encodedTile[(i * 2) + 1] = encodedline[1];
                encodedTile[i + 0x10] = encodedline[2];
            }

            return encodedTile;
        }
    }
}
