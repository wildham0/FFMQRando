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

namespace FFMQLib
{
	public class PlayerSpriteDataPack
	{
		public bool SoftBopEnabled { get; set; }
		public bool FullHorizontalFlipEnabled { get; set; }
		public List<byte[]> WalkingSeriesEncoded { get; set; }
		public List<byte[]> PushSeriesEncoded { get; set; }
		public List<byte[]> JumpSeriesEncoded { get; set; }
		public List<byte[]> VictorySeriesEncoded { get; set; }
		public List<byte[]> ThrowDeathSeriesEncoded { get; set; }
		public List<byte[]> BombShrugSeriesEncoded { get; set; }
		public byte[] ShrugHandEncoded { get; set; }
		public List<byte[]> ClimbSeriesEncoded { get; set; }
		public List<byte> Palette { get; set; }
	}
	public class DarkKingSpriteDataPack
	{ 
		public byte[] DrawingArray { get; set; }
        public byte[] PaletteArray { get; set; }
        public List<byte[]> EncodedTiles { get; set; }
        public List<byte> Palette1 { get; set; }
        public List<byte> Palette2 { get; set; }
    }
	public enum PlayerSpriteMode
	{ 
		Spritesheets,
		Icons
	}
	public class DarkKingSprite
	{ 
		public string filename { get; set; }
		public string author { get; set; }
		public string name { get; set; }
		public DarkKingSprite()
		{
			filename = "";
			author = "";
			name = "";
		}
	}

	public class DarkKingTrueForm
	{
		private DarkKingSprite dksprite;
		private byte[] dk3bmpdata;
		private byte[] dk4bmpdata;

		private const int drawingArrayBank = 0x0A;
		private const int drawingArrayOffsetDK3 = 0x82C6;
		private const int drawingArrayOffsetDK4 = 0x82E9;
		private const int paletteArrayOffsetDK3 = 0x85D2;
		private const int paletteArrayOffsetDK4 = 0x85F5;

		private const int graphicsBank = 0x0B;
		private const int graphicsOffsetDK = 0xB33C;

		private const int paletteBank = 0x09;
		private const int paletteOffsetDarkKing = 0x8280;
		private void GetRandomDKfromMetadata(MT19337 rng)
		{
			string metadatayaml = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("dktrueforms.zip"));
			using (Stream zipfile = assembly.GetManifestResourceStream(filepath))
			{
				using (ZipArchive spriteContainer = new ZipArchive(zipfile))
				{
					var entry = spriteContainer.GetEntry("metadata.yaml");
					using (StreamReader reader = new StreamReader(entry.Open()))
					{
						metadatayaml = reader.ReadToEnd();
					}

					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(UnderscoredNamingConvention.Instance)
						.Build();

					List<DarkKingSprite> spritelist = new();
					try
					{
						spritelist = deserializer.Deserialize<List<DarkKingSprite>>(metadatayaml);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}

					dksprite = rng.PickFrom(spritelist);

					entry = spriteContainer.GetEntry(dksprite.filename + "1.bmp");
					using (BinaryReader reader = new BinaryReader(entry.Open()))
					{
						dk3bmpdata = reader.ReadBytes((int)entry.Length);
					}

					entry = spriteContainer.GetEntry(dksprite.filename + "2.bmp");
					using (BinaryReader reader = new BinaryReader(entry.Open()))
					{
						dk4bmpdata = reader.ReadBytes((int)entry.Length);
					}
				}
			}
		}
		public void RandomizeDarkKingTrueForm(Preferences pref, MT19337 rng, FFMQRom rom)
		{
			bool debugmode = pref.DarkKing3.Length > 0;

			if (!pref.DarkKingTrueForm && !debugmode)
			{
				rng.Next();
				return;
			}

			var rngback = rng;

			GetRandomDKfromMetadata(rng);

			DarkKingSpriteDataPack darkking3 = new();
			DarkKingSpriteDataPack darkking4 = new();

			SpriteReader darkkingspritereader = new();

			if (debugmode)
			{
				darkking3 = darkkingspritereader.EncodeDarkKing(pref.DarkKing3);
				darkking4 = darkkingspritereader.EncodeDarkKing(pref.DarkKing4);

				dksprite.name = "Dark\nKing";
				dksprite.author = "";
			}
			else
			{
				darkking3 = darkkingspritereader.EncodeDarkKing(dk3bmpdata);
				darkking4 = darkkingspritereader.EncodeDarkKing(dk4bmpdata);
			}

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

			// Name Hack
			rom.PutInBank(0x02, 0xD351, Blob.FromHex("2280b010eaea"));
			rom.PutInBank(0x10, 0xB080, Blob.FromHex("485a08c230afb8d0028d4a11a2a0b0a00011a9100054000f287a686b"));

			string finalname = rom.TextToHex(dksprite.name);
			while (finalname.Length < 0x20)
			{
				finalname += "03";
			}
			rom.PutInBank(0x10, 0xB0A0, Blob.FromHex(finalname));

			rng = rngback;
			rng.Next();
		}
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
		public void SetPlayerSprite(PlayerSprite playersprite, FFMQRom rom)
		{
			if (playersprite.name == "default")
			{
				return;
			}

			PlayerSpriteDataPack playerspritedatapack = new();
			SpriteReader playerspritereader = new();

			//playerspritereader.LoadCustomSprites(playersprite);
			playerspritedatapack = playerspritereader.EncodePlayerSprite(playersprite);

			rom.PutInBank(0x04, 0x9A20, playerspritedatapack.WalkingSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCA20, playerspritedatapack.PushSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCBA0, playerspritedatapack.JumpSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCD20, playerspritedatapack.VictorySeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xCEA0, playerspritedatapack.ThrowDeathSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xD020, playerspritedatapack.BombShrugSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x04, 0xD0E0, playerspritedatapack.ShrugHandEncoded);
			rom.PutInBank(0x04, 0xD110, playerspritedatapack.ClimbSeriesEncoded.SelectMany(x => x).ToArray());
			rom.PutInBank(0x07, 0xD824, playerspritedatapack.Palette.ToArray());

			// Software Bop Hack
			if (playerspritedatapack.SoftBopEnabled)
			{
				rom.PutInBank(0x01, 0x94B6, Blob.FromHex("22008511eaeaeaea"));
				rom.PutInBank(0x11, 0x8500, Blob.FromHex("ad26192904ea4a4a48ad8b0e2901f0096848f002a9ff8d9919686b"));
			}

			// Full Horizontal Flip hack
			if (playerspritedatapack.FullHorizontalFlipEnabled)
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
	}
}
