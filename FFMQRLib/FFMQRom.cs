using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace FFMQLib
{
	public static class Metadata
	{
		public static string Version = "1.2.16";
	}
	public partial class FFMQRom : SnesRom
	{
		public ObjectList MapObjects;
		public MapChanges MapChanges;
		public GameFlags GameFlags;
		public GameScriptManager TileScripts;
		public GameScriptManager TalkScripts;
		public Battlefields Battlefields;
		public Overworld Overworld;
		public GameMaps GameMaps;
		public MapSprites MapSpriteSets;
		public EnemyAttackLinks EnemyAttackLinks;
		public Attacks Attacks;
		public EnemiesStats enemiesStats;
		public NodeLocations NodeLocations;
		private byte[] originalData;
		public bool beta = false;
		public bool spoilers = false;
		public string spoilersText;

		public override bool Validate()
		{
			using (SHA256 hasher = SHA256.Create())
			{
				byte[] dataToHash = new byte[0x80000];
				Array.Copy(Data, dataToHash, 0x80000);

				// zero out benjamin's sprites
				for (int i = 0x21A20; i < (0x24A20 + 0x180 * 1); i++)
				{
					dataToHash[i] = 0;
				}

				for (int i = 0x24A20; i < (0x24A20 + 0x180 * 5); i++)
				{
					dataToHash[i] = 0;
				}

				// benjamin's palette
				for (int i = 0x3D826; i < (0x3D826 + 0x0E); i++)
				{
					dataToHash[i] = 0;
				}

				Blob hash = hasher.ComputeHash(dataToHash);

				//Console.WriteLine(BitConverter.ToString(hash).Replace("-", ""));
				// if (hash == Blob.FromHex("F71817F55FEBD32FD1DCE617A326A77B6B062DD0D4058ECD289F64AF1B7A1D05")) unadultered hash

				if (hash == Blob.FromHex("92F625478568B1BE262E3F9D62347977CE7EE345E9FF353B4778E8560E16C7CA"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public bool IsEmpty()
		{
			if (Data == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public Stream DataStream()
		{
			return new MemoryStream(Data);
		}

		public void Load(byte[] _data)
		{
			Data = new byte[0x80000];
			Array.Copy(_data, Data, 0x80000);
		}
		public byte[] DataReadOnly { get => Data; }
		public Stream SpoilerStream()
		{
			if (spoilers)
			{
				var stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				
				writer.Write(spoilersText);
				writer.Flush();
				stream.Position = 0;
				return stream;
			}
			else
			{
				return null;
			}
		}

		public void PutInBank(int bank, int address, Blob data)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			Put(offset, data);
		}
		public Blob GetFromBank(int bank, int address, int length)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			return Get(offset, length);
		}
		public int GetOffset(int bank, int address)
		{
			return (bank * 0x8000) + (address - 0x8000);
		}
		public void ExpandRom()
		{
			Blob newData = new byte[0x100000];
			Array.Copy(Data, newData, 0x80000);
			Data = newData;
		}
		public void BackupOriginalData()
		{
			originalData = new byte[0x80000];
			Array.Copy(Data, originalData, 0x80000);
		}
		public void RestoreOriginalData()
		{
			Data = new byte[0x80000];
			Array.Copy(originalData, Data, 0x80000);
		}
		public void Randomize(Blob seed, Flags flags, Preferences preferences)
		{
			flags.FlagSanityCheck();

			MT19337 rng;
			MT19337 sillyrng;
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
				sillyrng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			NodeLocations = new(this);
			EnemyAttackLinks = new(this);
			Attacks = new(this);
			enemiesStats = new(this);
			GameMaps = new(this);
			MapObjects = new(this);
			Credits credits = new(this);
			GameFlags = new(this);
			TalkScripts = new(this, RomOffsets.TalkScriptsPointers, RomOffsets.TalkScriptPointerQty, RomOffsets.TalkScriptOffset, RomOffsets.TalkScriptEndOffset);
			TileScripts = new(this, RomOffsets.TileScriptsPointers, RomOffsets.TileScriptPointerQty, RomOffsets.TileScriptOffset, RomOffsets.TileScriptEndOffset);
			Battlefields = new(this);
			MapChanges = new(this);
			Overworld = new(this);
			MapSpriteSets = new(this);
			TitleScreen titleScreen = new(this);

			// General modifications
			ExpandRom();
			FastMovement();
			DefaultSettings();
			RemoveClouds();
			RemoveStrobing();
			SmallFixes();
			BugFixes();
			NonSpoilerDemoplay();
			CompanionRoutines();

			// Maps Changes
			GameMaps.RandomGiantTreeMessage(rng);
			GameMaps.LessObnoxiousMaps(flags.TweakedDungeons, MapObjects, rng);

			// Enemies
			MapObjects.SetEnemiesDensity(flags.EnemiesDensity, rng);
			MapObjects.ShuffleEnemiesPosition(flags.ShuffleEnemiesPosition, GameMaps, rng);
			EnemyAttackLinks.ShuffleAttacks(flags.EnemizerAttacks, rng);
			enemiesStats.ScaleEnemies(flags, rng);

			// Overworld
			NodeLocations.OpenNodes();
			Battlefields.SetBattlesQty(flags.BattlesQuantity, rng);
			Battlefields.ShuffleBattelfieldRewards(flags.ShuffleBattlefieldRewards, Overworld, rng);

			// Items
			ItemsPlacement itemsPlacement = new(flags, Battlefields, this, rng);

			SetStartingWeapons(itemsPlacement);
			MapObjects.UpdateChests(itemsPlacement);
			UpdateScripts(flags, itemsPlacement, rng);
			ChestsHacks(flags, itemsPlacement);
			Battlefields.PlaceItems(itemsPlacement);

			// Doom Castle
			SetDoomCastleMode(flags.DoomCastleMode);
			DoomCastleShortcut(flags.DoomCastleShortcut);

			// Various
			SetLevelingCurve(flags.LevelingCurve);
			ProgressiveGears(flags.ProgressiveGear);
			SkyCoinMode(flags, rng);
			credits.Update();

			// Preferences
			Msu1SupportRandom(preferences.RandomMusic, sillyrng);
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);

			// Write everything back			
			itemsPlacement.WriteChests(this);
			credits.Write(this);
			EnemyAttackLinks.Write(this);
			Attacks.Write(this);
			enemiesStats.Write(this);
			GameMaps.Write(this);
			MapChanges.Write(this);
			TileScripts.Write(this);
			TalkScripts.Write(this);
			BadShipHack(flags.DoomCastleShortcut);
			GameFlags.Write(this);
			NodeLocations.Write(this);
			Battlefields.Write(this);
			Overworld.Write(this);
			MapObjects.WriteAll(this);
			MapSpriteSets.Write(this);
			titleScreen.Write(this, Metadata.Version, seed, flags);

			// Spoilers
			spoilersText = itemsPlacement.GenerateSpoilers(this, titleScreen.versionText, titleScreen.hashText, flags.GenerateFlagString(), seed.ToHex());
			spoilers = flags.EnableSpoilers;
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}
	}
}
