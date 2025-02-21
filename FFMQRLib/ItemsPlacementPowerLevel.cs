using FFMQLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace FFMQLib
{
	public class PowerLevel
	{
		public AccessReqs Current => (AccessReqs)((int)AccessReqs.PowerLevel0 + currentPowerLevel);
		private List<AccessReqs> crystalSaved;
		private List<LocationIds> battlefieldDefeated;
		private List<AccessReqs> bossesDefeated;
		private List<AccessReqs> accessibleCompanions;
		private int battlefieldsNeeded;
		private int bossesNeeded;
		private int crystalNeeded;
		private int skyCoinNeeded;
		private LocationIds battlefieldToClear;

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
		private static Dictionary<CompanionsId, AccessReqs> companionAccess = new()
		{
			{ CompanionsId.Kaeli, AccessReqs.Kaeli },
			{ CompanionsId.Tristam, AccessReqs.Tristam },
			{ CompanionsId.Phoebe, AccessReqs.Phoebe },
			{ CompanionsId.Reuben, AccessReqs.Reuben },
		};
		private Dictionary<QuestsId, AccessReqs> questToAcces = new()
		{
			{ QuestsId.CureKaeli, AccessReqs.HealedPerson },
			{ QuestsId.VisitBoneDungeon, AccessReqs.TristamQuestCompleted },
			{ QuestsId.VisitWintryCave, AccessReqs.PhoebeQuestCompleted },
			{ QuestsId.VisitMine, AccessReqs.ReubenQuestCompleted },
			{ QuestsId.SaveCrystalofEarth, AccessReqs.FlamerusRex },
			{ QuestsId.SaveCrystalofWater, AccessReqs.IceGolem },
			{ QuestsId.SaveCrystalofFire, AccessReqs.DualheadHydra },
			{ QuestsId.SaveCrystalofWind, AccessReqs.Pazuzu },
			{ QuestsId.DefeatMinotaur, AccessReqs.Minotaur },
			{ QuestsId.DefeatSquidite, AccessReqs.Squidite },
			{ QuestsId.DefeatSnowCrab, AccessReqs.SnowCrab },
			{ QuestsId.DefeatJinn, AccessReqs.Jinn },
			{ QuestsId.DefeatMedusa, AccessReqs.Medusa },
			{ QuestsId.DefeatGidrah, AccessReqs.Gidrah },
			{ QuestsId.DefeatDullahan, AccessReqs.Dullahan },
			{ QuestsId.SaveArion, AccessReqs.ReubenDadSaved },
			{ QuestsId.ThawAquaria, AccessReqs.SummerAquaria },
			{ QuestsId.VisitTopOfVolcano, AccessReqs.TopOfVolcanoVisited },
			{ QuestsId.VisitChocobo, AccessReqs.ChocoboVisited },
			{ QuestsId.VisitLightTemple, AccessReqs.LightTempleVisited },
			{ QuestsId.VisitPointlessLedge, AccessReqs.PointlessLedgeVisited },
			{ QuestsId.VisitTreeHouses, AccessReqs.TreehouseVisited },
			{ QuestsId.VisitMountGale, AccessReqs.MountGaleVisited },
			{ QuestsId.VisitChocobo, AccessReqs.ChocoboVisited },
			{ QuestsId.BuildRainbowBridge, AccessReqs.RainbowBridge },
		};

		private LevelingType levelingMode;

		private int openness;
		private int weaponQuality;
		private int armorQuality;
		private int level0Armor;
		private int level1Armor;
		private int level2Armor;
		private int spellQuality;
		private int companionLevel;
		private int currentPowerLevel;
		private Dictionary<AccessReqs, int> questCompleted = new()
		{
			{ AccessReqs.Kaeli, 0 },
			{ AccessReqs.Tristam, 0 },
			{ AccessReqs.Phoebe, 0 },
			{ AccessReqs.Reuben, 0 },
		};
		private Dictionary<AccessReqs, List<int>> companionLevels = new();

		private List<(QuestsId quest, AccessReqs companion, AccessReqs req)> quests = new();
		private List<(QuestsId quest, AccessReqs companion, AccessReqs req)> currentQuests = new();


		public PowerLevel(LevelingType _levelingMode, Companions companions)
		{
			levelingMode = _levelingMode;
			ProcessQuests(companions);
			Initialize();
		}
		public void Initialize()
		{
			currentPowerLevel = 0;
			crystalSaved = new();
			battlefieldDefeated = new();
			bossesDefeated = new();
			accessibleCompanions = new();
			openness = 1;
			weaponQuality = 1;
			armorQuality = 0;
			level0Armor = 0;
			level1Armor = 0;
			level2Armor = 0;
			spellQuality = 0;
			companionLevel = 0;
			currentQuests = quests.ToList();
		}
		private void ProcessQuests(Companions companions)
		{
			// Remove unavailable companions
			companionAccess = companionAccess.Where(c => companions.Available[c.Key]).ToDictionary(c => c.Key, c => c.Value);
			
			crystalNeeded = 100;
			battlefieldsNeeded = 100;
			skyCoinNeeded = 100;
			bossesNeeded = 100;
			battlefieldToClear = LocationIds.None;

			if (levelingMode == LevelingType.Quests || levelingMode == LevelingType.SaveCrystalsIndividual)
			{
				companionLevels = new()
				{
					{ AccessReqs.Kaeli, new() { 7, 31 } },
					{ AccessReqs.Tristam, new() { 7, 23 } },
					{ AccessReqs.Phoebe, new() { 15, 34 } },
					{ AccessReqs.Reuben, new() { 23, 31 } },
				};
			}
			else if (levelingMode == LevelingType.QuestsExtended || levelingMode == LevelingType.SaveCrystalsAll)
			{
				companionLevels = new()
				{
					{ AccessReqs.Kaeli, new() { 7, 15, 23, 34, 41 } },
					{ AccessReqs.Tristam, new() { 7, 15, 23, 34, 41 } },
					{ AccessReqs.Phoebe, new() { 7, 15, 23, 34, 41 } },
					{ AccessReqs.Reuben, new() { 7, 15, 23, 34, 41 } },
				};
			}

			foreach (var quest in companions.Quests)
			{
				if (quest.Name == QuestsId.CollectQtyItems)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
				}
				else if (quest.Name == QuestsId.SaveQtyCrystals)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
					crystalNeeded = quest.Quantity;
				}
				else if (quest.Name == QuestsId.ClearQtyBattlefields)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
					battlefieldsNeeded = quest.Quantity;
				}
				else if (quest.Name == QuestsId.ClearSpecificBattlefield)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
					battlefieldToClear = (LocationIds)quest.Quantity;
				}
				else if (quest.Name == QuestsId.DefeatQtyMinibosses)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
					bossesNeeded = quest.Quantity;
				}
				else if (quest.Name == QuestsId.CollectQtySkyCoins)
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], AccessReqs.Barred));
					skyCoinNeeded = quest.Quantity;
				}
				else
				{
					quests.Add((quest.Name, companionAccess[quest.Companion], questToAcces[quest.Name]));
				}
			}
		}
		private void UpdateItem(Items placedItem)
		{
			if (coins.Contains(placedItem) && openness < 3)
			{
				openness++;
			}
			else if (level2weapons.Contains(placedItem) && weaponQuality < 3)
			{
				weaponQuality = 3;
			}
			else if (level1weapons.Contains(placedItem) && weaponQuality < 2)
			{
				weaponQuality = 2;
			}
			else if (level2armors.Contains(placedItem))
			{
				level2Armor++;
				ProcessArmor();
			}
			else if (level1armors.Contains(placedItem))
			{
				level1Armor++;
				ProcessArmor();
			}
			else if (level0armors.Contains(placedItem))
			{
				level0Armor++;
				ProcessArmor();
			}
			else if (level2spells.Contains(placedItem) && spellQuality < 3)
			{
				spellQuality = 3;
			}
			else if (level1spells.Contains(placedItem) && spellQuality < 2)
			{
				spellQuality = 2;
			}
			else if (level0spells.Contains(placedItem) && spellQuality < 1)
			{
				spellQuality = 1;
			}
		}
		private void UpdateCompanion(AccessReqs req, List<GameObject> locations)
		{
			// Update accessible companions
			if (companionAccess.Select(c => c.Value).Contains(req))
			{
				accessibleCompanions.Add(req);
			}
			
			if (accessibleCompanions.Any() && (levelingMode == LevelingType.BenPlus0 || levelingMode == LevelingType.BenPlus5))
			{
				companionLevel = openness;
			}
			else if (accessibleCompanions.Any() && levelingMode == LevelingType.BenPlus10)
			{
				companionLevel = Math.Min(openness + 1, 3);
			}
			else
			{
				// Update counts
				if (bosses.Contains(req))
				{
					bossesDefeated.Add(req);
				}
				else if (crystals.Contains(req))
				{
					crystalSaved.Add(req);
				}

				battlefieldDefeated = locations.Where(l => l.Accessible && battlefields.Contains(l.Location)).Select(l => l.Location).Distinct().ToList();

				(QuestsId quest, AccessReqs companion, AccessReqs req) quest;
				
				// ProcessQest
				if (currentQuests.TryFind(q => q.req == req, out quest))
				{
					questCompleted[quest.companion]++;
					currentQuests.Remove(quest);
				}

				// checkCount quest
				if (bossesDefeated.Distinct().Count() >= bossesNeeded)
				{
					if (currentQuests.TryFind(q => q.quest == QuestsId.DefeatQtyMinibosses, out quest))
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				if (crystalSaved.Distinct().Count() >= crystalNeeded)
				{
					if (currentQuests.TryFind(q => q.quest == QuestsId.SaveQtyCrystals, out quest))
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				// These are placed after logic, so we make some assumptions
				if (currentQuests.TryFind(q => q.quest == QuestsId.CollectQtyItems, out quest))
				{
					if (openness > 1)
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				if (currentQuests.TryFind(q => q.quest == QuestsId.CollectQtySkyCoins, out quest))
				{
					if ((skyCoinNeeded < 40 && openness >= 3) ||
						(skyCoinNeeded < 25 && openness >= 2) ||
						(skyCoinNeeded < 15 && openness >= 1))
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				// check battlefield quests
				if (battlefieldDefeated.Distinct().Count() >= battlefieldsNeeded)
				{
					if (currentQuests.TryFind(q => q.quest == QuestsId.ClearQtyBattlefields, out quest))
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				if (currentQuests.TryFind(q => q.quest == QuestsId.ClearSpecificBattlefield, out quest))
				{
					if (battlefieldDefeated.Contains(battlefieldToClear))
					{
						questCompleted[quest.companion]++;
						currentQuests.Remove(quest);
					}
				}

				// Compute companion level value
				var questResult = questCompleted.Where(c => accessibleCompanions.Contains(c.Key)).ToList();

				if (questResult.Any())
				{
					var bestlevel = questResult.Max(c => companionLevels[c.Key][c.Value]);

					if (bestlevel >= 31)
					{
						companionLevel = 3;
					}
					else if (bestlevel >= 23)
					{
						companionLevel = 2;
					}
					else if (bestlevel >= 15)
					{
						companionLevel = 1;
					}
				}
			}
		}
		private List<AccessReqs> GeneratePowerLevelReq()
		{
			List<int> qualityList = new() { openness, weaponQuality, armorQuality, spellQuality };
			List<AccessReqs> accessReqToProcess = new();

			if (qualityList.Min() > currentPowerLevel)
			{
				for (int i = currentPowerLevel + 1; i <= qualityList.Min(); i++)
				{
					accessReqToProcess.Add((AccessReqs)((int)AccessReqs.PowerLevel0 + i));
				}
			}

			currentPowerLevel = qualityList.Min();
			return accessReqToProcess;
		}
		private void ProcessArmor()
		{
			if (level2Armor >= 2)
			{
				armorQuality = 3;
			}
			else if (level2Armor + level1Armor >= 2)
			{
				armorQuality = 2;
			}
			else if (level2Armor + level1Armor + level0Armor >= 3)
			{
				armorQuality = 1;
			}
		}
		public List<AccessReqs> GetNewPowerLevel(Items placedItem, List<GameObject> locations)
		{
			UpdateItem(placedItem);
			UpdateCompanion(AccessReqs.None, locations);
			return GeneratePowerLevelReq();
		}
		public List<AccessReqs> GetNewPowerLevel(AccessReqs req, List<GameObject> locations)
		{
			UpdateCompanion(req, locations);
			return GeneratePowerLevelReq();
		}
		public List<AccessReqs> UpdatePowerLevel2(List<Items> placedItems, List<AccessReqs> processedReqs, List<GameObject> locations)
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
