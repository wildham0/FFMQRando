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
		public static string Version = "1.5.29";
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
			seed = apconfigs.ApEnabled ? apconfigs.GetSeed() : seed;
			
			MT19337 rng;				// Fixed RNG so the same seed with the same flagset generate the same results
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
			EnemyAttackLinks = new(this);
			EnemiesStats = new(this);
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

			Credits credits = new(this);
			TitleScreen titleScreen = new(this);

			// Sprites
			PlayerSprites playerSprites = new(PlayerSpriteMode.Spritesheets); // Merge by updating Credits at the end
			PlayerSprite playerSprite = playerSprites.GetSprite(preferences, asyncrng);
			DarkKingTrueForm darkKingTrueForm = new();

			// General modifications
			GeneralModifications(flags, apconfigs.ApEnabled, rng);
			UnjankOverworld(GameMaps, MapChanges, MapPalettes);

			// Maps Changes
			GameMaps.RandomGiantTreeMessage(rng);
			GameMaps.LessObnoxiousMaps(flags.TweakedDungeons, MapObjects, rng);

			// Enemies
			MapObjects.SetEnemiesDensity(flags.EnemiesDensity, rng);
			MapObjects.ShuffleEnemiesPosition(flags.ShuffleEnemiesPosition, GameMaps, rng);
			EnemyAttackLinks.ShuffleAttacks(flags.EnemizerAttacks, flags.EnemizerGroups, rng);
			EnemiesStats.ScaleEnemies(flags, rng);
			EnemiesStats.ShuffleResistWeakness(flags.ShuffleResWeakType, GameInfoScreen, rng);

			// Overworld
			Overworld.OpenNodes(flags);
			Battlefields.SetBattlesQty(flags.BattlesQuantity, rng);
			Battlefields.ShuffleBattlefieldRewards(flags.ShuffleBattlefieldRewards, GameLogic, apconfigs, rng);

			// Map Shuffling
			GameLogic.CrestShuffle(flags.CrestShuffle, apconfigs.ApEnabled, rng);
			GameLogic.FloorShuffle(flags.MapShuffling, apconfigs.ApEnabled, rng);
			Overworld.ShuffleOverworld(flags.MapShuffling, GameLogic, Battlefields, apconfigs.ApEnabled, rng);
			Overworld.UpdateOverworld(flags, GameLogic, Battlefields);

			// Logic
			GameLogic.CrawlRooms(flags, Overworld, Battlefields);
			EntrancesData.UpdateCrests(flags, TileScripts, GameMaps, GameLogic, Teleporters.TeleportersLong, this);
			EntrancesData.UpdateEntrances(flags, GameLogic.Rooms, rng);
			
			// Items
			ItemsPlacement itemsPlacement = new(flags, GameLogic.GameObjects, apconfigs, this, rng);

			SetStartingItems(itemsPlacement);
			MapObjects.UpdateChests(itemsPlacement);
			UpdateScripts(flags, itemsPlacement, Overworld.StartingLocation, rng);
			ChestsHacks(flags, itemsPlacement);
			Battlefields.PlaceItems(itemsPlacement);

			// Doom Castle
			SetDoomCastleMode(flags.DoomCastleMode);
			DoomCastleShortcut(flags.DoomCastleShortcut);

			// Companion
			Companions.SetSpellbooks(flags.CompanionSpellbookType, GameInfoScreen, rng);

			// Various
			SetLevelingCurve(flags.LevelingCurve);
			ProgressiveGears(flags.ProgressiveGear);
			SkyCoinMode(flags, rng);
			ExitHack(Overworld.StartingLocation);
			ProgressiveFormation(flags.ProgressiveFormations, Overworld, rng);

			// Preferences			
			RandomizeTracks(preferences.RandomMusic, sillyrng);
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);
			WindowPalette(preferences.WindowPalette);
			playerSprites.SetPlayerSprite(playerSprite, this);
			darkKingTrueForm.RandomizeDarkKingTrueForm(preferences, sillyrng, this);

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
			Battlefields.Write(this);
			Overworld.Write(this);
			MapObjects.Write(this);
			MapSpriteSets.Write(this);
			MapPalettes.Write(this);
			Companions.Write(this);
			GameInfoScreen.Write(this);

			credits.Write(this);
			titleScreen.Write(this, Metadata.Version, seed, flags);

			// Spoilers
			spoilersText = itemsPlacement.GenerateSpoilers(flags, titleScreen.versionText, titleScreen.hashText, seed.ToHex());
			spoilers = flags.EnableSpoilers;

			if (apconfigs.ApEnabled)
			{
				PutInBank(0x00, 0xFFC0, apconfigs.GetRomName());
			}
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}
	}
}
