using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void Improve(bool enablebugfix, Preferences preferences)
		{
			// Convert 1.0 rom to 1.1 for compatibility
			if (ConvertTo11)
			{
				Data = Patcher.PatchRom(this).DataReadOnly;
			}

			var seed = Blob.FromHex("FFFFFFFF");
			
			MT19337 rng;				// Fixed RNG so the same seed with the same flagset generate the same results
			MT19337 sillyrng;			// Fixed RNG so non impactful rng (preferences) matches for the same seed and the same flagset
			MT19337 asyncrng;			// Free RNG so non impactful rng varies for the same seed and flagset
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed);
				hashString = TitleScreen.EncodeTo32(hash).Substring(0, 8);
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}
            sillyrng = new MT19337((uint)Guid.NewGuid().GetHashCode());
            asyncrng = new MT19337((uint)Guid.NewGuid().GetHashCode());


			GameMaps = new(this);
			MapObjects = new(this);
			Battlefields = new(this);
			GameFlags = new(this);
			Enemies = new(this);

			Credits credits = new(this);
			TitleScreen titleScreen = new(this);

			// Sprites
			PlayerSprites playerSprites = new(PlayerSpriteMode.Spritesheets); // Merge by updating Credits at the end
			PlayerSprite playerSprite = playerSprites.GetSprite(preferences, asyncrng);
			DarkKingTrueForm darkKingTrueForm = new();

            // General modifications
            ImprovedModifications(enablebugfix, preferences.ReduceBattleFlash, preferences.MusicMode, rng);

			// Enemies
			MapObjects.SetEnemiesDensity(EnemiesDensity.Half, rng);
            MapObjects.GuidedDensity();
            Battlefields.SetBattlesQty(BattlesQty.Five, rng);

			// Various
			SetLevelingCurve(LevelingCurve.Double);

			// Preferences			
			SetMusicMode(preferences.MusicMode, sillyrng);
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);
			WindowPalette(preferences.WindowPalette);
			playerSprites.SetPlayerSprite(playerSprite, this);
			darkKingTrueForm.RandomizeDarkKingTrueForm(preferences, Enemies, Enemizer, false, sillyrng, this);

			// Credits
			credits.Update(playerSprite, darkKingTrueForm.DarkKingSprite);

			// Write everything back			
			GameMaps.Write(this);
			Battlefields.WriteWithoutSprites(this);
			MapObjects.Write(this);
			GameFlags.Write(this);
			Enemies.Write(this);

			credits.Write(this);
			titleScreen.Write(this, Metadata.Version, hashString, new Flags());
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}
        public void ImprovedModifications(bool enablebugfixes, bool reducebattleflash, MusicMode music, MT19337 rng)
        {
            ExpandRom();
            FastMovement();
            DefaultSettings();
            RemoveStrobing(reducebattleflash);
			//SmallFixes();
			if (enablebugfixes)
			{
                BugFixes();
            }
			SystemBugFixes();
            GameStateIndicator(hashString);
            //PazuzuFixedFloorRng(rng);
            Msu1Support(music == MusicMode.Mute);
			SaveFileReduction();
		}
    }
    public partial class Areas
    {
        public void GuidedDensity()
        {

			List<(int map, List<int> objects)> mapObjectsToRemove = new()
			{
				(0x0D, new() { 0x04, 0x05, 0x07, 0x0A } ),
                (0x0E, new() { 0x04, 0x05, 0x07, 0x0A } ),
                (0x13, new() { 0x02, 0x03, 0x06, 0x07, 0x09, 0x0A, 0x0D } ),
                (0x14, new() { 0x02, 0x03, 0x04, 0x05, 0x08 } ),
                (0x15, new() { 0x00, 0x02, 0x07, 0x09, 0x05, 0x06, 0x0B } ),
                (0x1C, new() { 0x02, 0x04, 0x05, 0x06, 0x09, 0x0C, 0x0D, 0x10 } ),
                (0x1D, new() { 0x01, 0x02, 0x03, 0x06, 0x09, 0x0C, 0x0D } ),
                (0x1E, new() { 0x01, 0x02, 0x04, 0x07 } ),
                (0x1F, new() { 0x02, 0x03, 0x04, 0x09 } ),
                (0x21, new() { 0x0D, 0x0E, 0x0B } ), // Fall Basin
				(0x22, new() { 0x03, 0x04, 0x06, 0x07, 0x09, 0x0A, 0x0E, 0x0F } ),
                (0x23, new() { 0x00, 0x01, 0x04, 0x05, 0x06, 0x07, 0x0C, 0x0E } ),
                (0x24, new() { 0x00, 0x01, 0x04, 0x05, 0x06, 0x07, 0x0C, 0x0E } ),
            };

			// Force Fall Basin Puzzle
			var fallbasin = Entries[areaPointers[0x21]].Objects;
			fallbasin[0x0A].Gameflag = 0x00;
            fallbasin[0x0B].Gameflag = (byte)GameFlagIds.ShowEnemies;
            fallbasin[0x0C].Gameflag = 0x00;
            fallbasin[0x0D].Gameflag = (byte)GameFlagIds.ShowEnemies;
            fallbasin[0x0E].Gameflag = (byte)GameFlagIds.ShowEnemies;
            fallbasin[0x0F].Gameflag = 0x00;

            // Clear Pazuzu stairs
            for (int i = 0x5A; i <= 0x5E; i++)
			{
				Entries[areaPointers[i]].Objects[0].Gameflag = (byte)GameFlagIds.ShowEnemies;
				Entries[areaPointers[i]].Objects[1].Gameflag = 0x00;
				Entries[areaPointers[i]].Objects[2].Gameflag = (byte)GameFlagIds.ShowEnemies;
            }
        }
    }
}
