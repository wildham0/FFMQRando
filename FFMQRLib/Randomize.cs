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
		// X.YY.ZZ
		// X = Global Version
		// YY = Release
		// ZZ = Build
		public static string Version = "1.06.36";
		
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
		public EnemiesStats EnemiesStats;
		public FormationsData FormationsData;
		public GameLogic GameLogic;
		public EntrancesData EntrancesData;
		public MapPalettes MapPalettes;
		public Companions Companions;
		public GameInfoScreen GameInfoScreen;


		private byte[] originalData;
		public bool beta = false;
		public bool spoilers = false;
		public string spoilersText;
		public void Randomize(Blob seed, Flags flags, Preferences preferences, ApConfigs apconfigs)
		{
			// Convert 1.0 rom to 1.1 for compatibility
			if (ConvertTo11)
			{
				Data = Patcher.PatchRom(this).DataReadOnly;
			}
			
			seed = apconfigs.ApEnabled ? apconfigs.GetSeed() : seed;
			
			MT19337 rng;                // Fixed RNG so the same seed with the same flagset generate the same results
			MT19337 sillyrng;			// Fixed RNG so non impactful rng (preferences) matches for the same seed and the same flagset
			MT19337 asyncrng;			// Free RNG so non impactful rng varies for the same seed and flagset
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
				sillyrng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}
			asyncrng = new MT19337((uint)Guid.NewGuid().GetHashCode());

			Attacks = new(this);
			EnemiesStats = new(this);
			FormationsData = new(this);
			EnemyAttackLinks = new(this);
			GameMaps = new(this);
			MapObjects = new(this);
			GameFlags = new(this);
			TalkScripts = new(this);
			TileScripts = new(this);
			Battlefields = new(this);
			MapChanges = new(this);
			Overworld = new(this);
			Teleporters = new(this);
			MapSpriteSets = new(this);
			GameLogic = new(apconfigs);
			EntrancesData = new(this);
			MapPalettes = new(this);
			Companions = new(flags.CompanionLevelingType);
			GameInfoScreen = new();

			//Battle battle = new(EnemiesStats, EnemyAttackLinks, rng);

			Credits credits = new(this);
			TitleScreen titleScreen = new(this);

			// Sprites
			PlayerSprites playerSprites = new(PlayerSpriteMode.Spritesheets); // Merge by updating Credits at the end
			PlayerSprite playerSprite = playerSprites.GetSprite(preferences, asyncrng);
			DarkKingTrueForm darkKingTrueForm = new();

			// General modifications
			GeneralModifications(flags, preferences, apconfigs.ApEnabled, rng);
			UnjankOverworld(GameMaps, MapChanges, MapPalettes);

			// Maps Changes
			GameMaps.RandomGiantTreeMessage(rng);
			GameMaps.LessObnoxiousMaps(flags.TweakedDungeons, MapObjects, rng);
			GameMaps.ShuffledMapChanges(flags.MapShuffling, MapObjects);

			// Enemies
			MapObjects.SetEnemiesDensity(flags.EnemiesDensity, rng);
			MapObjects.ShuffleEnemiesPosition(flags.ShuffleEnemiesPosition, GameMaps, rng);
			EnemiesStats.ScaleEnemies(flags, rng);
			EnemiesStats.ShuffleResistWeakness(flags.ShuffleResWeakType, GameInfoScreen, rng);
			EnemyAttackLinks.ShuffleAttacks(flags, EnemiesStats, FormationsData, rng);

			// Companions
			GameLogic.CompanionsShuffle(flags.CompanionsLocations, flags.KaelisMomFightMinotaur, apconfigs, rng);
			Companions.SetStartingCompanion(flags.StartingCompanion, rng);
			Companions.SetAvailableCompanions(flags.AvailableCompanions, rng);
			Companions.SetSpellbooks(flags.CompanionSpellbookType, GameInfoScreen, rng);
			Companions.SetQuests(flags, Battlefields, GameInfoScreen, rng);
			Companions.SetCompanionsLocation(GameLogic.Rooms);

			// Overworld
			Overworld.OpenNodes(flags);
			Battlefields.SetBattlesQty(flags.BattlesQuantity, rng);
			Battlefields.ShuffleBattlefieldRewards(flags.ShuffleBattlefieldRewards, GameLogic, apconfigs, rng);

			// Map Shuffling
			GameLogic.CrestShuffle(flags.CrestShuffle, apconfigs.ApEnabled, rng);
			GameLogic.FloorShuffle(flags.MapShuffling, apconfigs.ApEnabled, rng);
			Overworld.ShuffleOverworld(flags.OverworldShuffle, flags.MapShuffling, GameLogic, Battlefields, Companions.QuestEasyWinLocations, apconfigs.ApEnabled, rng);
			Overworld.UpdateOverworld(flags, GameLogic, Battlefields);

			// Logic
			GameLogic.CrawlRooms(flags, Overworld, EnemiesStats, Companions, Battlefields);
			EntrancesData.UpdateCrests(flags, TileScripts, GameMaps, GameLogic, Teleporters.TeleportersLong, this);
			EntrancesData.UpdateEntrances(flags, GameLogic.Rooms, rng);

			//var spoilers = new Spoilers();
			//var floors = spoilers.GenerateMapSpoiler(flags, GameLogic);

			// Items
			ItemsPlacement itemsPlacement = new(flags, GameLogic.GameObjects, Companions, apconfigs, this, rng);

			SetStartingItems(itemsPlacement);
			MapObjects.UpdateChests(itemsPlacement);
			UpdateScripts(flags, itemsPlacement, Overworld.StartingLocation, apconfigs.ApEnabled, preferences.MusicMode == MusicMode.Mute, rng);
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

			// Preferences			
			SetMusicMode(preferences.MusicMode, new MT19337(sillyrng.Next()));
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, new MT19337(sillyrng.Next()));
			WindowPalette(preferences.WindowPalette);
			playerSprites.SetPlayerSprite(playerSprite, this);
			darkKingTrueForm.RandomizeDarkKingTrueForm(preferences, new MT19337(sillyrng.Next()), this);

			// Credits
			credits.Update(playerSprite, darkKingTrueForm.DarkKingSprite);

			// Write everything back			
			itemsPlacement.WriteChests(this);
			EnemyAttackLinks.Write(this);
			Attacks.Write(this);
			EnemiesStats.Write(this);
			GameMaps.Write(this);
			MapChanges.Write(this);
			Teleporters.Write(this);
			TileScripts.Write(this);
			TalkScripts.Write(this);
			GameFlags.Write(this);
			EntrancesData.Write(this);
			Battlefields.Write(flags.ShuffleBattlefieldRewards, this);
			Overworld.Write(this);
			MapObjects.Write(this);
			MapSpriteSets.Write(this);
			MapPalettes.Write(this);
			Companions.Write(this);
			GameInfoScreen.Write(this);

			credits.Write(this);
			titleScreen.Write(this, Metadata.Version, seed, flags);

			// Spoilers
			Spoilers spoilersGenerator = new();
			spoilersText = spoilersGenerator.GenerateSpoilers(flags, titleScreen, seed.ToHex(), itemsPlacement, GameInfoScreen, GameLogic, Battlefields);

			if (apconfigs.ApEnabled)
			{
				PutInBank(0x00, 0xFFC0, apconfigs.GetRomName());
			}
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}
	}
}
