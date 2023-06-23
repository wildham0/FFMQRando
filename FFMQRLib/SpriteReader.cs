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

            if (colorCount == 0)
            {
                colorCount = 0x100;
            }

            for (int i = 0; i < colorCount; i++)
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

            // Get Software Bop flag
            byte softbopbyte = data[dataOffset + (2 * infoWidth) - 1];
            bool softbopenabled = (softbopbyte != emptyPixel);

            // Get Full Horizontal Flip flag
            byte fullhorizontalflipbyte = data[dataOffset + (2 * infoWidth) - 2];
            bool fullhorizontalflibenabled = (fullhorizontalflipbyte != emptyPixel);


            // Encode Sprites
            List<byte[]> walkseriesEncoded = EncodeSeries((0, 0), 8);
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

        private void ReadSpriteSheet(string spritename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("customsprites.zip"));
            using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
            {
                using (ZipArchive spriteContainer = new ZipArchive(zipfile))
                {
                    if (spritename == "random")
                    {
                        Random rand = new Random(Guid.NewGuid().GetHashCode());
                        int choosenSprite = rand.Next(1, (spriteContainer.Entries.Count - 1));

                        using (BinaryReader reader = new BinaryReader(spriteContainer.Entries[choosenSprite].Open()))
                        {
                            data = reader.ReadBytes((int)spriteContainer.Entries[choosenSprite].Length);
                        }
                    }
                    else
                    {
                        var entry = spriteContainer.GetEntry("spritesheets/" + spritename + ".bmp");
                        using (BinaryReader reader = new BinaryReader(entry.Open()))
                        {
                            data = reader.ReadBytes((int)entry.Length);
                        }
                    }
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
            for (int i = 0; i < pixelline.Length; i++)
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
