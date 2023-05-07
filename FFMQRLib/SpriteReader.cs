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

            byte emptyPixel = pixelcolors.Find(p => p.Item2 == 0).Item1;

            // Get Software Bop Down flag
            byte softbopdownbyte = data[dataOffset + (2 * infoWidth) - 1];
            bool softbopdownenabled = (softbopdownbyte != emptyPixel);

            // Get Software Bop Up flag
            byte softbopupbyte = data[dataOffset + (2 * infoWidth) - 2];
            bool softbopupenabled = (softbopupbyte != emptyPixel);


            // Encode Sprites
            List<byte[]> walkseriesEncoded = EncodeSeries((0,0), 8);
            List<byte[]> pushseriesEncoded = EncodeSeries((16, 0), 8);
            List<byte[]> jumpseriesEncoded = EncodeSeries((32, 0), 6);
            List<byte[]> victoryseriesEncoded = EncodeSeries((48, 0), 4);
            List<byte[]> throwdeathEncoded = EncodeSeries((64, 0), 8);
            List<byte[]> bombshrugEncoded = EncodeSeries((80, 0), 4);
            byte[] shrughandEncoded = EncodeTile((96, 24));
            List<byte[]> climbEncoded = EncodeSeries((80, 32), 3);

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
            if (softbopdownenabled || softbopupenabled)
            {
                string bopdirection = "ff";
                /*
                if (softbopupenabled)
                {
                    bopdirection = "01";
                }*/
                
                rom.PutInBank(0x01, 0x94B6, Blob.FromHex("22008511eaeaeaea"));
                rom.PutInBank(0x11, 0x8500, Blob.FromHex($"ad26192904ea4a4a48ad8b0e2901f0096848f002a9{bopdirection}8d9919686b"));
            }
        }

        private void ReadSpriteSheet(string spritename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(spritename + ".bmp"));
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

        public void LoadCustomSprites(Preferences pref, FFMQRom rom)
        {
            if (pref.PlayerSprite == "default")
            {
                return;
            }
            else if (pref.PlayerSprite == "custom")
            {
                data = pref.CustomSprites;
                Encode(rom);
            }
            else
            {
                ReadSpriteSheet(pref.PlayerSprite);
                Encode(rom);
            }
        
        }
    }
}
