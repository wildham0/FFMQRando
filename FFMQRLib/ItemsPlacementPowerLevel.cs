using FFMQLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMQLib
{
	public class PowerLevel
	{
		public AccessReqs Current;
		private List<AccessReqs> crystalSaved;
		private List<LocationIds> battlefieldDefeated;
		private List<AccessReqs> bossesDefeated;
		private List<AccessReqs> accessibleCompanions;

		private static List<Items> level1weapons = new() { Items.KnightSword, Items.BattleAxe, Items.JumboBomb, Items.CharmClaw };
		private static List<Items> level2weapons = new() { Items.Excalibur, Items.GiantsAxe, Items.MegaGrenade, Items.DragonClaw };
		private static List<Items> level0armors = new() { Items.SteelHelm, Items.SteelArmor, Items.SteelShield, Items.Charm };
		private static List<Items> level1armors = new() { Items.MoonHelm, Items.NobleArmor, Items.VenusShield, Items.MagicRing };
		private static List<Items> level2armors = new() { Items.ApolloHelm, Items.GaiasArmor, Items.AegisShield, Items.CupidLocket };
		private static List<Items> level0spells = new() { Items.CureBook, Items.FireBook };
		private static List<Items> level1spells = new() { Items.BlizzardBook, Items.ThunderSeal, Items.AeroBook };
		private static List<Items> level2spells = new() { Items.WhiteSeal, Items.MeteorSeal, Items.FlareSeal };
		private static List<Items> coins = new() { Items.SandCoin, Items.RiverCoin, Items.SunCoin };

		private static List<AccessReqs> bosses = new() { AccessReqs.Minotaur, AccessReqs.Squidite, AccessReqs.SnowCrab, AccessReqs.Jinn, AccessReqs.Medusa, AccessReqs.Gidrah, AccessReqs.Dullahan };
		private static List<AccessReqs> crystals = new() { AccessReqs.FlamerusRex, AccessReqs.IceGolem, AccessReqs.DualheadHydra, AccessReqs.Pazuzu };
		private static List<LocationIds> battlefields = new() { LocationIds.ForestaSouthBattlefield, LocationIds.ForestaWestBattlefield, LocationIds.ForestaEastBattlefield, LocationIds.AquariaBattlefield01, LocationIds.AquariaBattlefield02, LocationIds.AquariaBattlefield03, LocationIds.WintryBattlefield01, LocationIds.WintryBattlefield02, LocationIds.PyramidBattlefield01, LocationIds.LibraBattlefield01, LocationIds.LibraBattlefield02, LocationIds.FireburgBattlefield01, LocationIds.FireburgBattlefield02, LocationIds.FireburgBattlefield03, LocationIds.MineBattlefield01, LocationIds.MineBattlefield02, LocationIds.MineBattlefield03, LocationIds.VolcanoBattlefield01, LocationIds.WindiaBattlefield01, LocationIds.WindiaBattlefield02 };
		private LevelingType levelingMode;

		public PowerLevel(LevelingType _levelingMode)
		{
			Current = AccessReqs.PowerLevel0;
			levelingMode = _levelingMode;
		}
		private void ProcessQuests(List<AccessReqs> processedReqs, List<GameObject> locations)
		{ 
			
		
		}
		public List<AccessReqs> UpdatePowerLevel(List<Items> placedItems, List<AccessReqs> processedReqs, List<GameObject> locations)
		{

			//var accessiblecount = ItemsLocations.Count(l => l.Accessible);
			//var totlcount = ItemsLocations.Count();

			int openness = 1;
			if (placedItems.Intersect(coins).Count() >= 2)
			{
				openness = 3;
			}
			else if (placedItems.Intersect(coins).Count() >= 1)
			{
				openness = 2;
			}

			int companion = 0;
			if (accessibleCompanions.Any())
			{
				if (levelingMode == LevelingType.BenPlus0 || levelingMode == LevelingType.BenPlus5)
				{
					companion = openness;
				}
				else if (levelingMode == LevelingType.BenPlus10)
				{
					companion = Math.Min(openness + 1, 3);
				}
				else
				{ 
					// ProcessQest
				
				}
			}


			int weaponquality = 1;
			if (placedItems.Intersect(level2weapons).Any())
			{
				weaponquality = 3;
			}
			else if (placedItems.Intersect(level1weapons).Any())
			{
				weaponquality = 2;
			}

			int armorquality = 0;
			int level2armorscount = placedItems.Intersect(level2armors).Count();
			int level1armorscount = placedItems.Intersect(level1armors).Count();
			int level0armorscount = placedItems.Intersect(level0armors).Count();

			if (level2armorscount >= 2)
			{
				armorquality = 3;
			}
			else if (level2armorscount + level1armorscount >= 2)
			{
				armorquality = 2;
			}
			else if (level2armorscount + level1armorscount + level0armorscount >= 3)
			{
				armorquality = 1;
			}

			int spellquality = 0;

			if (placedItems.Intersect(level2spells).Any())
			{
				spellquality = 3;
			}
			else if (placedItems.Intersect(level1spells).Any())
			{
				spellquality = 2;
			}
			else if (placedItems.Intersect(level0spells).Any())
			{
				spellquality = 1;
			}

			List<int> qualityList = new() { openness, weaponquality, armorquality, spellquality };
			List<AccessReqs> accessReqToProcess = new();

			for (int i = 0; i <= qualityList.Min(); i++)
			{
				if ((int)AccessReqs.PowerLevel0 + i > (int)Current)
				{
					accessReqToProcess.Add((AccessReqs)((int)AccessReqs.PowerLevel0 + i));
					Current = (AccessReqs)((int)AccessReqs.PowerLevel0 + i);
				}
			}

			return accessReqToProcess;
		}

	}
}
