using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FFMQLib
{
	public static class Metadata
	{
		public static string Version = "1.4.36";
	}
	public partial class FFMQRom : SnesRom
	{
		public ObjectList MapObjects;
		public MapChanges MapChanges;
		public GameFlags GameFlags;
		public TileScriptsManager TileScripts;
		public TalkScriptsManager TalkScripts;
		public Battlefields Battlefields;
		public Overworld Overworld;
		public GameMaps GameMaps;
		public Teleporters Teleporters;
		public MapSprites MapSpriteSets;
		public EnemyAttackLinks EnemyAttackLinks;
		public Attacks Attacks;
		public EnemiesStats enemiesStats;
		public GameLogic GameLogic;
		public EntrancesData EntrancesData;
		private byte[] originalData;
		public bool beta = false;
		public bool spoilers = false;
		public string spoilersText;
		public void Randomize(Blob seed, Flags flags, Preferences preferences)
		{
			MT19337 rng;
			MT19337 sillyrng;
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
				sillyrng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			EnemyAttackLinks = new(this);
			Attacks = new(this);
			enemiesStats = new(this);
			GameMaps = new(this);
			MapObjects = new(this);
			Credits credits = new(this);
			GameFlags = new(this);
			TalkScripts = new(this);
			TileScripts = new(this);
			Battlefields = new(this);
			MapChanges = new(this);
			Overworld = new(this);
			Teleporters = new(this);
			MapSpriteSets = new(this);
			GameLogic = new();
			EntrancesData = new(this);
			TitleScreen titleScreen = new(this);

			// General modifications
			GeneralModifications(flags, rng);

			// Maps Changes
			GameMaps.RandomGiantTreeMessage(rng);
			GameMaps.LessObnoxiousMaps(flags.TweakedDungeons, MapObjects, rng);

			// Enemies
			MapObjects.SetEnemiesDensity(flags.EnemiesDensity, rng);
			MapObjects.ShuffleEnemiesPosition(flags.ShuffleEnemiesPosition, GameMaps, rng);
			EnemyAttackLinks.ShuffleAttacks(flags.EnemizerAttacks, flags.BossesScalingUpper, rng);
			enemiesStats.ScaleEnemies(flags, rng);

			// Overworld
			Overworld.OpenNodes(flags);
			Battlefields.SetBattlesQty(flags.BattlesQuantity, rng);
			Battlefields.ShuffleBattelfieldRewards(flags.ShuffleBattlefieldRewards, GameLogic, rng);

			// Locations & Logic
			GameLogic.CrestShuffle(flags.CrestShuffle, rng);
			GameLogic.FloorShuffle(flags.MapShuffling, rng);
			Overworld.ShuffleOverworld(flags.MapShuffling, GameLogic, Battlefields, rng);

			Overworld.UpdateOverworld(flags, GameLogic, Battlefields);

			GameLogic.CrawlRooms(flags, Overworld, Battlefields);
			
			EntrancesData.UpdateCrests(flags, TileScripts, GameMaps, GameLogic, Teleporters.TeleportersLong, this);
			EntrancesData.UpdateEntrances(flags, GameLogic.Rooms, rng);
			
			// Items
			ItemsPlacement itemsPlacement = new(flags, GameLogic.GameObjects, this, rng);

			SetStartingItems(itemsPlacement);
			MapObjects.UpdateChests(itemsPlacement);
			UpdateScripts(flags, itemsPlacement, Overworld.StartingLocation, rng);
			ChestsHacks(flags, itemsPlacement);
			Battlefields.PlaceItems(itemsPlacement);

			// Doom Castle
			SetDoomCastleMode(flags.DoomCastleMode);
			DoomCastleShortcut(flags.DoomCastleShortcut);

			// Various
			SetLevelingCurve(flags.LevelingCurve);
			ProgressiveGears(flags.ProgressiveGear);
			SkyCoinMode(flags, rng);
			ExitHack(Overworld.StartingLocation);
			ProgressiveFormation(flags.ProgressiveFormations, Overworld, rng);
			credits.Update();

			// Preferences			
			RandomizeTracks(preferences.RandomMusic, sillyrng);
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);
			WindowPalette(preferences.WindowPalette);
			
			SpriteReader spriteReader = new SpriteReader();
			spriteReader.LoadCustomSprites(preferences, this);

			// Write everything back			
			itemsPlacement.WriteChests(this);
			credits.Write(this);
			EnemyAttackLinks.Write(this);
			Attacks.Write(this);
			enemiesStats.Write(this);
			GameMaps.Write(this);
			MapChanges.Write(this);
			Teleporters.Write(this);
			TileScripts.Write(this);
			TalkScripts.Write(this);
			GameFlags.Write(this);
			EntrancesData.Write(this);
			Battlefields.Write(this);
			Overworld.Write(this);
			MapObjects.Write(this);
			MapSpriteSets.Write(this);
			titleScreen.Write(this, Metadata.Version, seed, flags);

			// Spoilers
			spoilersText = itemsPlacement.GenerateSpoilers(flags, titleScreen.versionText, titleScreen.hashText, seed.ToHex());
			spoilers = flags.EnableSpoilers;
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}
	}
}
