using FFMQLib;
using Microsoft.VisualBasic;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static System.Math;

namespace FFMQLib
{

	public enum EnemizerAttacks : int
	{
		[Description("Disabled")]
		Normal = 0,
		[Description("Script Shuffle")]
		SimpleShuffle,
		//[Description("Safe Randomization")]
		//Safe,
		[Description("Balanced")]
		Balanced,
		[Description("Balanced Expert")]
		BalancedExpert,
		[Description("Elemental")]
		Elemental,
		[Description("Chaos Randomization")]
		Chaos,
		[Description("Self-Destruct")]
		SelfDestruct,
	}
	public enum EnemizerGroups
	{
		[Description("Mobs Only")]
		MobsOnly = 0,
		[Description("Mobs & Bosses")]
		MobsBosses,
		[Description("Mobs, Bosses & Dark King")]
		MobsBossesDK,
	}

	public enum BalancedMode
	{ 
		Safe,
		NoAilment,
		ForcedBad,
		All
	}

	public partial class Enemizer
	{
		private Dictionary<EnemyIds, (bool bad, int ailments)> SkillResults;
		public void ShuffleAttacks(Flags flags, MT19337 rng)
		{
			var enemizerattacks = flags.EnemizerAttacks;
			var enemizergroup = flags.EnemizerGroups;
			var progressive = false;

			switch (enemizerattacks)
			{
				case EnemizerAttacks.Balanced:
					Balanced(enemizergroup, false, progressive, new(), rng);
					break;
				case EnemizerAttacks.BalancedExpert:
					Balanced(enemizergroup, true, progressive, new(), rng);
					break;
				case EnemizerAttacks.Elemental:
					Balanced(enemizergroup, false, progressive, ElementalEnemies, rng);
					break;
				case EnemizerAttacks.Chaos:
					ChaosRandom(enemizergroup, progressive, rng);
					break;
				case EnemizerAttacks.SelfDestruct:
					Selfdestruct(enemizergroup);
					break;
				case EnemizerAttacks.SimpleShuffle:
					if (progressive)
					{
						ProgressiveScriptShuffle(enemizergroup, rng);
					}
					else
					{
						ScriptShuffle(enemizergroup, rng);
					}
					break;
				default:
					break;
			}

			AnalyzeAttackLevel();
			GeneratePowerLevels();
		}
		private void ProgressiveScriptShuffle(EnemizerGroups group, MT19337 rng)
		{
			PadScripts(rng);

			var mobsCategory = EnemyInfo.EnemyCategories.Where(c => c.Key <= EnemyCategory.Devil).Select(c => (c.Key, c.Value)).ToList();
			var bossesCategory = EnemyInfo.EnemyCategories.Where(c => c.Key > EnemyCategory.Devil && c.Key < EnemyCategory.DarkKing).Select(c => (c.Key, c.Value)).ToList();

			List<((EnemyCategory category, List<EnemyIds> enemies) groupa, (EnemyCategory category, List<EnemyIds> enemies) groupb)> switchList = new();

			while (mobsCategory.Count > 1)
			{
				var tempmobA = rng.TakeFrom(mobsCategory);
				var tempmobB = rng.TakeFrom(mobsCategory);

				switchList.Add((tempmobA, tempmobB));
				switchList.Add((tempmobB, tempmobA));
			}

			if (group != EnemizerGroups.MobsOnly)
			{
				while (bossesCategory.Count > 1)
				{
					var tempmobA = rng.TakeFrom(bossesCategory);
					var tempmobB = rng.TakeFrom(bossesCategory);

					switchList.Add((tempmobA, tempmobB));
					switchList.Add((tempmobB, tempmobA));
				}
			}

			// DK isn't switched because he can't really get matched, so he just gets 3 random script
			if (group == EnemizerGroups.MobsBossesDK)
			{
				var potentialScript = Bosses.Concat(DarkCastleBosses).Concat(DarkKing).ToList();

				attacks.Data[EnemyIds.DarkKing] = new EnemyAttackLink(EnemyIds.DarkKing, attacks.Data[rng.TakeFrom(potentialScript)]);
				attacks.Data[EnemyIds.DarkKingWeapons] = new EnemyAttackLink(EnemyIds.DarkKingWeapons, attacks.Data[rng.TakeFrom(potentialScript)]);
				attacks.Data[EnemyIds.DarkKingSpider] = new EnemyAttackLink(EnemyIds.DarkKingSpider, attacks.Data[rng.TakeFrom(potentialScript)]);
			}

			// Assign scripts
			foreach (var enemygroup in switchList)
			{
				var groupA = enemygroup.groupa.enemies;
				var groupB = enemygroup.groupb.enemies;

				var tempattack0 = attacks.Data[groupA[0]];
				var tempattack1 = attacks.Data[groupA[1]];
				var tempattack2 = (groupA.Count > 2) ? attacks.Data[groupA[2]] : attacks.Data[groupA[1]];

				attacks.Data[groupA[0]] = new EnemyAttackLink(groupA[0], attacks.Data[groupB[0]]);
				attacks.Data[groupA[1]] = new EnemyAttackLink(groupA[1], attacks.Data[groupB[1]]);

				if (groupA.Count > 2)
				{
					attacks.Data[groupA[2]] = (groupB.Count > 2) ? new EnemyAttackLink(groupA[2], attacks.Data[groupB[2]]) : new EnemyAttackLink(groupA[2], attacks.Data[groupB[1]]); ;
				}

				attacks.Data[groupB[0]] = new EnemyAttackLink(groupB[0], tempattack0);
				attacks.Data[groupB[1]] = new EnemyAttackLink(groupB[1], tempattack1);

				if (groupB.Count > 2)
				{
					attacks.Data[groupB[2]] = (groupA.Count > 2) ? new EnemyAttackLink(groupB[2], tempattack2) : new EnemyAttackLink(groupB[2], tempattack1); ;
				}
			}
		}
		private void ScriptShuffle(EnemizerGroups group, MT19337 rng)
		{
			PadScripts(rng);

			var mobs = Mobs.ToList();
			Dictionary<EnemyIds, EnemyIds> switchList = new();

			while (mobs.Count > 1)
			{
				var tempmobA = rng.TakeFrom(mobs);
				var tempmobB = rng.TakeFrom(mobs);

				switchList.Add(tempmobA, tempmobB);
				switchList.Add(tempmobB, tempmobA);
			}

			List<EnemyIds> dkbosses = new();
			List<EnemyIds> bosses = new();

			switch (group)
			{
				case EnemizerGroups.MobsBosses:
					bosses = Bosses.Concat(DarkCastleBosses).ToList();
					break;
				case EnemizerGroups.MobsBossesDK:
					dkbosses = DarkCastleBosses.Concat(DarkKing).ToList();
					bosses = Bosses.ToList();
					break;
			}

			while (dkbosses.Count > 1)
			{
				var tempbossA = rng.TakeFrom(dkbosses);
				var tempbossB = rng.TakeFrom(dkbosses);

				switchList.Add(tempbossA, tempbossB);
				switchList.Add(tempbossB, tempbossA);
			}

			while (bosses.Count > 1)
			{
				var tempbossA = rng.TakeFrom(bosses);
				var tempbossB = rng.TakeFrom(bosses);

				switchList.Add(tempbossA, tempbossB);
				switchList.Add(tempbossB, tempbossA);
			}

			var oldattacks = attacks.Data.ToDictionary();
			foreach (var link in oldattacks.Values)
			{
				if (switchList.TryGetValue(link.Id, out var newid))
				{
					attacks.Data[newid] = link;
				}
			}

			//_EnemyAttackLinks = _EnemyAttackLinks.OrderBy(l => l.Id).ToList();
		}
		private void PadScripts(MT19337 rng)
		{
			// Remove Strong Psychshield since it might get used
			var zuhScript = attacks.Data[EnemyIds.Zuh];
			zuhScript.Attacks[0x05] = EnemyAttackIds.HurricanePlus;

			// Extend Behemoth, Minotaur, StoneGolem and DK1 attack list to avoid softlock
			var behemothScript = attacks.Data[EnemyIds.Behemoth];
			behemothScript.Attacks = Enumerable.Repeat(EnemyAttackIds.Horn, 6).ToList();

			var minotaurScript = attacks.Data[EnemyIds.Minotaur];
			minotaurScript.Attacks = new() { EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream, EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream };

			var stoneGolemScript = attacks.Data[EnemyIds.StoneGolem];
			minotaurScript.Attacks = new() { EnemyAttackIds.ThunderSpell, EnemyAttackIds.StoneBlock, EnemyAttackIds.Stare, EnemyAttackIds.CorrodeGas, EnemyAttackIds.StoneBlock, EnemyAttackIds.ThunderSpell };

			var dk1Script = attacks.Data[EnemyIds.DarkKing];
			dk1Script.Attacks[0x04] = rng.PickFrom(new List<EnemyAttackIds>() { EnemyAttackIds.DarkCane, EnemyAttackIds.IronNail, EnemyAttackIds.Spark });
		}
		private void ChaosRandom(EnemizerGroups group, bool progressive, MT19337 rng)
		{
			var possibleAttacks = new List<EnemyAttackIds>();
			bool meandDK = rng.Between(1, 6) == 1;

			for (var i = EnemyAttackIds.Sword; i < EnemyAttackIds.HeatwaveUnused; i++)
			{
				possibleAttacks.Add(i);
			}

			var validenemies = Enemizer.GetValidEnemies(group, progressive);

			foreach (var catgroup in validenemies)
			{
				List<EnemyAttackIds> attacklist = new();
				List<EnemyAttackIds> nothinglist = new() { EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing };

				foreach (var enemy in catgroup.enemies)
				{
					int maxattack = attacks.Data[enemy].AttackCount;
					while (attacklist.Count < maxattack)
					{
						if (DarkKing.Contains(enemy) && !meandDK)
						{
							attacklist.Add(rng.PickFrom(possibleAttacks.Except(new List<EnemyAttackIds> { EnemyAttackIds.CureSelf, EnemyAttackIds.HealSelf, EnemyAttackIds.Selfdestruct, EnemyAttackIds.Multiply }).ToList()));
						}
						else
						{
							attacklist.Add(rng.PickFrom(possibleAttacks));
						}
					}

					attacks.Data[enemy].Attacks = attacklist.GetRange(0, maxattack).Concat(nothinglist).ToList().GetRange(0, 6).ToList();
				}
			}

			if (group != EnemizerGroups.MobsOnly)
			{
				var icegolemattacks = possibleAttacks
					.Except(new List<EnemyAttackIds> { EnemyAttackIds.CureSelf, EnemyAttackIds.HealSelf, EnemyAttackIds.Selfdestruct, EnemyAttackIds.Multiply })
					.Except(Enumerable.Range((int)EnemyAttackIds.PsychshieldBug, 20).Select(x => (EnemyAttackIds)x))
					.ToList();

				attacks.IceGolemDesperateAttack = rng.PickFrom(icegolemattacks);
			}
		}
		private void GeneratePowerLevels()
		{
			Dictionary<int, AccessReqs> powerlevels = new()
			{
				{ 0, AccessReqs.PowerLevel0 },
				{ 1, AccessReqs.PowerLevel1 },
				{ 2, AccessReqs.PowerLevel2 },
				{ 3, AccessReqs.PowerLevel3 },
			};

			foreach (var formation in EnemyInfo.BossesFormations)
			{
				var enemieslist = SkillResults.Where(r => formation.Value.Contains(r.Key)).Select(r => r.Key).ToList();
				var results = SkillResults.Where(r => formation.Value.Contains(r.Key)).Select(r => r.Value).ToList();

				int baselevel = 0;
				int enemycount = enemieslist.Count;
				int maxhp = enemies.ToList().Where(e => enemieslist.Contains(e.Id)).Max(e => e.HP);

				if (enemycount == 1)
				{
					if (maxhp < 4000)
					{
						baselevel = 0;
					}
					else if (maxhp < 10000)
					{
						baselevel = 1;
					}
					else
					{
						baselevel = 2;
					}

					if (results[0].bad)
					{
						baselevel++;
					}
				}
				else
				{
					if (maxhp < 10000)
					{
						baselevel = 1;
					}
					else if (maxhp < 20000)
					{
						baselevel = 2;
					}
					else
					{
						baselevel = 3;
					}

					if (results.Count(r => r.bad) > 1 || results.Count(r => r.ailments > 0) > 1)
					{
						baselevel++;
					}
				}

				BossesPower.Add(EnemyInfo.BossesAccess[formation.Key], powerlevels[Math.Min(baselevel, 3)]);
			}

			foreach (var battlefield in EnemyInfo.BattlefieldFormations)
			{
				int minlevel = 3;
				foreach (var formation in battlefield.Value)
				{
					var enemieslists = formations.Formations[formation].Enemies;
					var results = SkillResults.Where(r => enemieslists.Contains(r.Key)).Select(r => r.Value).ToList();

					int baselevel = 0;
					int enemycount = enemieslists.Count;
					int maxhp = enemies.ToList().Where(e => enemieslists.Contains(e.Id)).Max(e => e.HP);

					if (enemycount == 1)
					{
						baselevel = 0;
					}
					else if (enemycount == 2)
					{
						if (maxhp < 250)
						{
							baselevel = 0;
						}
						else
						{
							baselevel = 1;
						}
					}
					else
					{
						if (maxhp < 250)
						{
							baselevel = 1;
						}
						else if (maxhp < 500)
						{
							baselevel = 1;
						}
						else
						{
							baselevel = 2;
						}
					}

					if (results.Count(r => r.bad) > 1 || results.Count(r => r.ailments > 0) > 1)
					{
						baselevel++;
					}

					minlevel = Math.Min(minlevel, baselevel);
				}

				BattlefieldsPower.Add(battlefield.Key, powerlevels[Math.Min(minlevel, 3)]);
			}
		}
		public void Balanced(EnemizerGroups group, bool expert, bool progressive, Dictionary<EnemyIds, EnemizerElements> elementalEnemies, MT19337 rng)
		{
			var validenemies = Enemizer.GetValidEnemies(group, progressive);
			List<EnemyAttack> validAttacks = new();
			bool includedk = group == EnemizerGroups.MobsBossesDK;

			// Distribute attacks
			foreach (var catgroup in validenemies)
			{
				List<EnemyAttack> attacklist = new();
				List<EnemyAttackIds> nothinglist = new() { EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing };

				bool firstenemydone = false;
				int badPicked = 0;
				bool elementalPicked = false;
				
				foreach (var enemy in catgroup.enemies)
				{
					int maxattack = attacks.Data[enemy].AttackCount;
					bool darkcastle = Enemizer.DarkCastleBosses.Concat(Enemizer.DarkKing).Contains(enemy);
					EnemizerElements element = elementalEnemies.TryGetValue(enemy, out var elementfound) ? elementfound : EnemizerElements.None;

					while (attacklist.Count < maxattack)
					{
						bool maxedOutBadAttacks = (maxattack == 2) || (maxattack == 3 && badPicked > 0) || (maxattack > 3 && badPicked > 1);

						if (elementalPicked || element == EnemizerElements.None)
						{
							validAttacks = Enemizer.Attacks.Where(a =>
								(maxedOutBadAttacks ? a.Strength == AttackStrengths.Safe : (expert ? a.Strength != AttackStrengths.Safe : true)) &&
								((includedk && darkcastle) ? true : (!a.IsDarkKing || a.IsMobs || a.IsBosses || a.IsDarkCastleBosses)) &&
								(element != EnemizerElements.None ? (elementalPicked ? (a.Element == element || a.Element == EnemizerElements.None) : a.Element == element) : true) &&
								!a.IsForbidden &&
								!attacklist.Contains(a)
							).ToList();
						}
						else
						{
							validAttacks = Enemizer.Attacks.Where(a =>
								(maxedOutBadAttacks ? a.Strength == AttackStrengths.Safe : true) &&
								((includedk && darkcastle) ? true : (!a.IsDarkKing || a.IsMobs || a.IsBosses || a.IsDarkCastleBosses)) &&
								(a.Element == element) &&
								!a.IsForbidden &&
								!attacklist.Contains(a)
							).ToList();
						}

						var newAttack = rng.PickFrom(validAttacks);

						if (newAttack.Strength != AttackStrengths.Safe)
						{
							badPicked++;
						}

						if (newAttack.Element != EnemizerElements.None)
						{
							elementalPicked = true;
						}

						attacklist.Add(newAttack);
					}

					if (!firstenemydone)
					{
						firstenemydone = true;
						attacklist.Shuffle(rng);
					}

					else if ((enemy == EnemyIds.TwinheadWyvern || enemy == EnemyIds.DualheadHydra) && !firstenemydone)
					{
						firstenemydone = true;

						// Split hydra's and wyvern attacks to make them less backended
						List<EnemyAttack> badhydra = attacklist.Where(a => a.Strength != AttackStrengths.Safe).ToList();
						List<EnemyAttack> goodhydra = attacklist.Where(a => a.Strength == AttackStrengths.Safe).ToList();

						List<List<EnemyAttack>> hydraattacks = new() { new(), new() };

						int currentphase = rng.Between(0, 1);

						while (badhydra.Any())
						{
							hydraattacks[currentphase].Add(badhydra.First());
							badhydra.RemoveAt(0);

							currentphase = (currentphase == 0) ? 1 : 0;
						}

						currentphase = 0;

						while (currentphase < 2)
						{
							while (hydraattacks[currentphase].Count < 3 && goodhydra.Any())
							{
								hydraattacks[currentphase].Add(goodhydra.First());
								goodhydra.RemoveAt(0);
							}

							hydraattacks[currentphase].Shuffle(rng);
							currentphase++;
						}

						attacklist = hydraattacks.SelectMany(a => a).ToList();
					}

					attacks.Data[enemy].Attacks = attacklist.GetRange(0, maxattack).Select(a => a.Id).Concat(nothinglist).ToList().GetRange(0, 6).ToList();
				}
			}

			// Do Ice Golem Desesperation Attack
			if (group != EnemizerGroups.MobsOnly)
			{
				var icegolemElement = elementalEnemies.TryGetValue(EnemyIds.IceGolem, out var elementfound) ? elementfound : EnemizerElements.None;

				validAttacks = Enemizer.Attacks.Where(a =>
					((includedk) ? true : (!a.IsDarkKing || a.IsMobs || a.IsBosses || a.IsDarkCastleBosses)) &&
					(icegolemElement != EnemizerElements.None ? (a.Element == icegolemElement || a.Element == EnemizerElements.None) : true) &&
					(a.Strength == AttackStrengths.Safe) &&
					!a.IsForbidden).ToList();

				attacks.IceGolemDesperateAttack = rng.PickFrom(validAttacks).Id;
			}
		}
		private void AnalyzeAttackLevel()
		{
			foreach (var link in attacks.Data.Values)
			{
				var strongattacks = link.Attacks.Intersect(Enemizer.Attacks.Where(a => a.Strength == AttackStrengths.Strong).Select(a => a.Id)).Count();
				var ailmentattacks = link.Attacks.Intersect(Enemizer.Attacks.Where(a => a.Strength == AttackStrengths.Ailment).Select(a => a.Id)).Count();

				SkillResults.Add(link.Id, ((strongattacks + ailmentattacks) > 0, ailmentattacks));
			}
		}
		private void Selfdestruct(EnemizerGroups group)
		{
			// Never progressive
			var validenemies = Enemizer.GetValidEnemies(group, false);

			foreach (var enemy in validenemies.SelectMany(e => e.enemies))
			{
				var ea = attacks.Data[enemy];
				ea.AttackPattern = 0x01;
				ea.Attacks = Enumerable.Repeat(EnemyAttackIds.Selfdestruct, 6).ToList();
			}
		}
	}
	public enum AttackStrengths
	{ 
		Safe,
		Strong,
		Ailment,
	}

	public struct EnemyAttack
	{ 
		public EnemyAttackIds Id;
		public EnemizerElements Element;
		public AttackStrengths Strength;
		public bool IsMobs;
		public bool IsBosses;
		public bool IsDarkCastleBosses;
		public bool IsDarkKing;
		public bool IsForbidden;

		public EnemyAttack() { }
		public EnemyAttack(EnemyAttackIds id, EnemizerElements element, AttackStrengths strength, bool ismobs, bool isboss, bool isdkboss, bool isdk, bool isforbid)
		{
			Id = id;
			Element = element;
			Strength = strength;
			IsMobs = ismobs;
			IsBosses = isboss;
			IsDarkCastleBosses = isdkboss;
			IsDarkKing = isdk;
			IsForbidden = isforbid;
		}
	}

	public partial class Enemizer
	{
		public static List<EnemyAttack> Attacks = new()
		{
			new(EnemyAttackIds.Sword, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Scimitar, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.DragonCut, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.Rapier, EnemizerElements.None, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.Axe, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.Beam, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.BoneMissile, EnemizerElements.None, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.BowArrow, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.BlowDart, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.CureSelf, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, true),
			new(EnemyAttackIds.HealSelf, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, true),
			new(EnemyAttackIds.QuakeSpell, EnemizerElements.Earth, AttackStrengths.Strong, true, false, true, false, false),
			new(EnemyAttackIds.Blizzard, EnemizerElements.Water, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Fire, EnemizerElements.Fire, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.ThunderSpell, EnemizerElements.Thunder, AttackStrengths.Safe, true, true, true, false, false),
			new(EnemyAttackIds.Reflectant, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, true),
			new(EnemyAttackIds.Electrapulse, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.PowerDrain, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Spark, EnemizerElements.None, AttackStrengths.Safe, true, false, false, true, false),
			new(EnemyAttackIds.IronNail, EnemizerElements.None, AttackStrengths.Safe, true, false, false, true, false),
			new(EnemyAttackIds.Scream, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.QuickSand, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.DoomGaze, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.DoomPowder, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.Cure, EnemizerElements.None, AttackStrengths.Safe, false, false, false, false, true),
			new(EnemyAttackIds.FireBreathFixed, EnemizerElements.Fire, AttackStrengths.Strong, false, true, false, false, false),
			new(EnemyAttackIds.Punch, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Kick, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Uppercut, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Stab, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.HeadButt, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.BodySlam, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Scrunch, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.FullNelson, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.NeckChoke, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Dash, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Roundhouse, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.ChokeUp, EnemizerElements.None, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.StompStomp, EnemizerElements.None, AttackStrengths.Strong, false, true, false, false, false),
			new(EnemyAttackIds.MegaPunch, EnemizerElements.None, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.Bearhug, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.AxeBomber, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.PiledriverFixed, EnemizerElements.None, AttackStrengths.Strong, false, true, false, false, false),
			new(EnemyAttackIds.SkyAttackFixed, EnemizerElements.None, AttackStrengths.Strong, false, true, false, false, false),
			new(EnemyAttackIds.Wraparound, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Mucus, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Claw, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Fang, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Beak, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Sting, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Tail, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Psudopod, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Bite, EnemizerElements.None, AttackStrengths.Safe, true, false, true, false, false),
			new(EnemyAttackIds.HydroAcid, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Branch, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Fin, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Scissor, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.WhipTongue, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Horn, EnemizerElements.None, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.GiantBlade, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.Headboomerang, EnemizerElements.None, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.ChewOff, EnemizerElements.None, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.Quake, EnemizerElements.Earth, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.Flame, EnemizerElements.Fire, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.FlameSweep, EnemizerElements.Fire, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.FireBall, EnemizerElements.Fire, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.FlamePillar, EnemizerElements.Fire, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.Heatwave, EnemizerElements.Fire, AttackStrengths.Strong, false, false, false, false, false),
			new(EnemyAttackIds.Watergun, EnemizerElements.Water, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Coldness, EnemizerElements.Water, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.IcyFoam, EnemizerElements.Water, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.IceBlock, EnemizerElements.Water, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.Snowstorm, EnemizerElements.Water, AttackStrengths.Strong, false, false, false, false, false),
			new(EnemyAttackIds.Whirlwater, EnemizerElements.Water, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.IceBreath, EnemizerElements.Water, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Tornado, EnemizerElements.Air, AttackStrengths.Safe, true, true, true, false, false),
			new(EnemyAttackIds.Typhoon, EnemizerElements.Air, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.Hurricane, EnemizerElements.Air, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.Thunder, EnemizerElements.Thunder, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.ThunderBeam, EnemizerElements.Thunder, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.CorrodeGas, EnemizerElements.None, AttackStrengths.Ailment, true, false, true, false, false),
			new(EnemyAttackIds.DoomDance, EnemizerElements.None, AttackStrengths.Ailment, true, true, true, false, false),
			new(EnemyAttackIds.SonicBoom, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.Bark, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.Screechvoice, EnemizerElements.None, AttackStrengths.Ailment, true, true, false, false, false),
			new(EnemyAttackIds.ParaNeedle, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.ParaClaw, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.ParaSnake, EnemizerElements.None, AttackStrengths.Ailment, false, true, false, false, false),
			new(EnemyAttackIds.ParaBreath, EnemizerElements.None, AttackStrengths.Ailment, false, true, true, false, false),
			new(EnemyAttackIds.PoisonSting, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.PoisonThorn, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.RottonMucus, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.PoisonSnake, EnemizerElements.None, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.Poisonbreath, EnemizerElements.None, AttackStrengths.Safe, true, true, true, false, false),
			new(EnemyAttackIds.Blinder, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Blackness, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.StoneBeak, EnemizerElements.None, AttackStrengths.Ailment, true, true, false, false, false),
			new(EnemyAttackIds.Gaze, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.Stare, EnemizerElements.None, AttackStrengths.Ailment, true, true, true, false, false),
			new(EnemyAttackIds.SpookyLaugh, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Riddle, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.BadBreath, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.BodyOdor, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.ParaStare, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, true, false),
			new(EnemyAttackIds.PoisonFluid, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.PoisonFlour, EnemizerElements.None, AttackStrengths.Safe, true, true, true, false, false),
			new(EnemyAttackIds.HypnoSleep, EnemizerElements.None, AttackStrengths.Ailment, true, true, false, false, false),
			new(EnemyAttackIds.Lullaby, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.SleepLure, EnemizerElements.None, AttackStrengths.Ailment, true, false, false, false, false),
			new(EnemyAttackIds.SleepPowder, EnemizerElements.None, AttackStrengths.Ailment, true, true, true, false, false),
			new(EnemyAttackIds.BlindFlash, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Smokescreen, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Muffle, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.SilenceSong, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.StoneGas, EnemizerElements.None, AttackStrengths.Ailment, true, true, true, false, false),
			new(EnemyAttackIds.StoneGaze, EnemizerElements.None, AttackStrengths.Ailment, true, true, false, false, false),
			new(EnemyAttackIds.DoubleSword, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.DoubleHit, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.TripleFang, EnemizerElements.None, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.DoubleKick, EnemizerElements.None, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.TwinShears, EnemizerElements.None, AttackStrengths.Strong, true, false, false, false, false),
			new(EnemyAttackIds.ThreeHeads, EnemizerElements.None, AttackStrengths.Strong, false, true, false, false, false),
			new(EnemyAttackIds.SixPsudopods, EnemizerElements.None, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.SnakeHead, EnemizerElements.None, AttackStrengths.Strong, true, true, false, false, false),
			new(EnemyAttackIds.Drain, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.Dissolve, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, false),
			new(EnemyAttackIds.SuckerStick, EnemizerElements.None, AttackStrengths.Safe, true, true, false, false, false),
			new(EnemyAttackIds.Selfdestruct, EnemizerElements.None, AttackStrengths.Safe, true, false, false, false, true),
			new(EnemyAttackIds.Multiply, EnemizerElements.None, AttackStrengths.Strong, true, false, false, false, true),
			new(EnemyAttackIds.ParaGas, EnemizerElements.None, AttackStrengths.Ailment, true, true, false, false, false),
			new(EnemyAttackIds.RipEarth, EnemizerElements.Earth, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.StoneBlock, EnemizerElements.Earth, AttackStrengths.Safe, false, false, true, false, false),
			new(EnemyAttackIds.Windstorm, EnemizerElements.Air, AttackStrengths.Safe, false, true, false, false, false),
			new(EnemyAttackIds.TwinFang, EnemizerElements.None, AttackStrengths.Safe, false, true, true, false, false),
			new(EnemyAttackIds.PsychshieldBug, EnemizerElements.None, AttackStrengths.Safe, false, true, true, false, true),
			new(EnemyAttackIds.Psychshield, EnemizerElements.None, AttackStrengths.Strong, false, false, true, false, true),
			new(EnemyAttackIds.DarkCane, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.DarkSaber, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.IceSword, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.FireSword, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.MirrorSword, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.QuakeAxe, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.CureArrow, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.Lazer, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.SpiderKids, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.SilverWeb, EnemizerElements.None, AttackStrengths.Ailment, false, false, false, true, false),
			new(EnemyAttackIds.GoldenWeb, EnemizerElements.None, AttackStrengths.Ailment, false, false, false, true, false),
			new(EnemyAttackIds.MegaFlare, EnemizerElements.Fire, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.MegaWhite, EnemizerElements.None, AttackStrengths.Strong, false, false, false, true, false),
			new(EnemyAttackIds.FireBreath, EnemizerElements.Fire, AttackStrengths.Strong, false, false, true, false, false),
			new(EnemyAttackIds.SkyAttack, EnemizerElements.None, AttackStrengths.Strong, false, false, true, false, false),
			new(EnemyAttackIds.Piledriver, EnemizerElements.None, AttackStrengths.Safe, false, false, true, false, false),
			new(EnemyAttackIds.HurricanePlus, EnemizerElements.Air, AttackStrengths.Safe, false, false, true, false, false),
			new(EnemyAttackIds.HeatwaveUnused, EnemizerElements.Fire, AttackStrengths.Strong, false, false, false, false, false),
		};
	
	}
}
