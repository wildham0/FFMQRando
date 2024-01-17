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
			var seed = Blob.FromHex("FFFFFFFF");
			
			MT19337 rng;				// Fixed RNG so the same seed with the same flagset generate the same results
			MT19337 sillyrng;			// Fixed RNG so non impactful rng (preferences) matches for the same seed and the same flagset
			MT19337 asyncrng;			// Free RNG so non impactful rng varies for the same seed and flagset
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed);
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}
            sillyrng = new MT19337((uint)Guid.NewGuid().GetHashCode());
            asyncrng = new MT19337((uint)Guid.NewGuid().GetHashCode());


			GameMaps = new(this);
			MapObjects = new(this);
			Battlefields = new(this);
			GameFlags = new(this);

			Credits credits = new(this);
			TitleScreen titleScreen = new(this);

			// Sprites
			PlayerSprites playerSprites = new(PlayerSpriteMode.Spritesheets); // Merge by updating Credits at the end
			PlayerSprite playerSprite = playerSprites.GetSprite(preferences, asyncrng);
			DarkKingTrueForm darkKingTrueForm = new();

            // General modifications
            ImprovedModifications(enablebugfix, rng);

			// Enemies
			MapObjects.SetEnemiesDensity(EnemiesDensity.Half, rng);
			Battlefields.SetBattlesQty(BattlesQty.Five, rng);

			// Various
			SetLevelingCurve(LevelingCurve.Double);

			// Preferences			
			RandomizeTracks(preferences.RandomMusic, sillyrng);
			RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);
			WindowPalette(preferences.WindowPalette);
			playerSprites.SetPlayerSprite(playerSprite, this);
			darkKingTrueForm.RandomizeDarkKingTrueForm(preferences, sillyrng, this);

			// Credits
			credits.Update(playerSprite, darkKingTrueForm.DarkKingSprite);

			// Write everything back			
			GameMaps.Write(this);
			Battlefields.Write(this);
			MapObjects.Write(this);
			GameFlags.Write(this);

			credits.Write(this);
			titleScreen.Write(this, Metadata.Version, seed, new Flags());
			
			// Remove header if any
			this.Header = Array.Empty<byte>();
		}

        public void ImprovedModifications(bool enablebugfixes, MT19337 rng)
        {
            ExpandRom();
            FastMovement();
            DefaultSettings();
            RemoveStrobing();
			//SmallFixes();
			if (enablebugfixes)
			{
                BugFixes();
            }
			SystemBugFixes();
            GameStateIndicator();
            PazuzuFixedFloorRng(rng);
            Msu1Support();
        }
    }
}
