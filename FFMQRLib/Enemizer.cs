using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;
using Microsoft.VisualBasic;

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

	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLink : ICloneable
	{
		private const int EnemiesAttackLinksAddress = 0xC6FF; // Bank 02
		private const int EnemiesAttackLinksBank = 0x02;
		private const int EnemiesAttackLinksLength = 0x09;

		public byte AttackPattern { get; set; }
		public List<EnemyAttackIds> Attacks { get; set; }
		public bool CastHeal { get; set; }
		public bool CastCure { get; set; }
		public EnemyIds Id { get; set; }
		public int AttackCount => Attacks.Count(a => a != EnemyAttackIds.Nothing);
		public List<int> NeedsSlotsFilled { get; set; }

		public EnemyAttackLink(int id, FFMQRom rom)
		{
			GetFromBytes(id, rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress + (id * EnemiesAttackLinksLength), EnemiesAttackLinksLength));
		}
		public EnemyAttackLink(int id, byte[] rawBytes)
		{
			GetFromBytes(id, rawBytes);
		}
		public EnemyAttackLink(EnemyIds id, EnemyAttackLink link)
		{
			GetFromBytes((int)id, link.GetBytes());
		}
		public void GetFromBytes(int id, byte[] rawBytes)
		{
			Id = (EnemyIds)id;
			AttackPattern = rawBytes[0];
			Attacks = rawBytes[1..7].Select(a => (EnemyAttackIds)a).ToList();
			CastHeal = rawBytes[7] == (byte)EnemyAttackIds.HealSelf;
			CastCure = rawBytes[8] == (byte)EnemyAttackIds.CureSelf;
			NeedsSlotsFilled = new();
		}
		private EnemyAttackLink(EnemyIds id, byte attackPattern, List<EnemyAttackIds> attacks, bool castHeal, bool castCure)
		{
			Id = id;
			AttackPattern = attackPattern;
			Attacks = attacks;
			CastHeal = castHeal;
			CastCure = castCure;
		}

		public object Clone()
		{
			return new EnemyAttackLink(Id, AttackPattern, Attacks, CastHeal, CastCure);
		}
		public byte[] GetBytes()
		{
			return new[] { AttackPattern, (byte)Attacks[0], (byte)Attacks[1], (byte)Attacks[2], (byte)Attacks[3], (byte)Attacks[4], (byte)Attacks[5], CastHeal ? (byte)EnemyAttackIds.HealSelf : (byte)EnemyAttackIds.Nothing, CastCure ? (byte)EnemyAttackIds.CureSelf : (byte)EnemyAttackIds.Nothing };
		}
		public byte[] GetAttackBytes()
		{
			return Attacks.Select(a => (byte)a).ToArray();
		}
	}
	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLinks
	{
		private List<EnemyAttackLink> _EnemyAttackLinks;
		private Blob _darkKingAttackLinkBytes;
		private EnemyAttackIds iceGolemDesperateAttack;

		private List<EnemyIds> Mobs;
		private List<EnemyIds> Bosses;
		private List<EnemyIds> DarkCastleBosses;
		private List<EnemyIds> DarkKing;

		private const int EnemiesAttackLinksAddress = 0xC6FF; // Bank 02
		private const int EnemiesAttackLinksBank = 0x02;
		private const int EnemiesAttackLinksLength = 0x09;
		private const int EnemiesAttackLinksQty = 0x53;

		// Dark King Attack Links, separate from EnemiesAttacks
		// Dark King has its own byte range on top of attack links ids 79 through 82
		private const int DarkKingAttackLinkAddress = 0xD09E; // Bank 02
		private const int DarkKingAttackLinkBank = 0x02;
		private const int DarkKingAttackLinkQty = 0x0C;

		private const int IceGolemDesperateAttackBank = 0x02;
		private const int IceGolemDesperateAttackOffset = 0xAAC9;

		private EnemiesStats enemiesStats;
		private FormationsData formationsData;
		private Dictionary<EnemyIds, (bool bad, int ailments)> skillResults;

		public EnemyAttackLinks(FFMQRom rom)
		{
			_EnemyAttackLinks = rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress, EnemiesAttackLinksQty * EnemiesAttackLinksLength).ToBytes().Chunk(EnemiesAttackLinksLength).Select((l, i) => new EnemyAttackLink(i, l)).ToList();

			iceGolemDesperateAttack = (EnemyAttackIds)rom.GetFromBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, 1)[0];
			_darkKingAttackLinkBytes = rom.GetFromBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, DarkKingAttackLinkQty);

			Mobs = Enumerable.Range((int)EnemyIds.Brownie, 0x40).Select(e => (EnemyIds)e).ToList();
			Bosses = Enumerable.Range((int)EnemyIds.Behemoth, 8).Select(e => (EnemyIds)e).ToList().Concat(new List<EnemyIds>() { EnemyIds.FlamerusRex, EnemyIds.IceGolem, EnemyIds.DualheadHydra, EnemyIds.Pazuzu }).ToList();
			DarkCastleBosses = new() { EnemyIds.SkullrusRex, EnemyIds.StoneGolem, EnemyIds.TwinheadWyvern, EnemyIds.Zuh };
			DarkKing = new() { EnemyIds.DarkKing, EnemyIds.DarkKingWeapons, EnemyIds.DarkKingSpider };

			skillResults = new();

			// Wyvern, Hydra, DK2 and Dk3 need to have a specific slot filled to avoid softlock
			_EnemyAttackLinks[(int)EnemyIds.DualheadHydra].NeedsSlotsFilled.Add(3);
			_EnemyAttackLinks[(int)EnemyIds.TwinheadWyvern].NeedsSlotsFilled.Add(3);
			_EnemyAttackLinks[(int)EnemyIds.DarkKingWeapons].NeedsSlotsFilled.Add(3);
			_EnemyAttackLinks[(int)EnemyIds.DarkKingSpider].NeedsSlotsFilled.Add(2);
		}
		public EnemyAttackLink this[int attackid]
		{
			get => _EnemyAttackLinks[attackid];
			set => _EnemyAttackLinks[attackid] = value;
		}
		public IList<EnemyAttackLink> AllAttacks()
		{
			return _EnemyAttackLinks.AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{

			rom.PutInBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress, _EnemyAttackLinks.SelectMany(l => l.GetBytes()).ToArray());

			rom.PutInBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, _EnemyAttackLinks.Where(l => l.Id == EnemyIds.DarkKingWeapons || l.Id == EnemyIds.DarkKingSpider).OrderBy(l => l.Id).SelectMany(l => l.GetAttackBytes()).ToArray());
			rom.PutInBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, new byte[] { (byte)iceGolemDesperateAttack });
		}
		public void ShuffleAttacks(Flags flags, EnemiesStats enemies, FormationsData formations, MT19337 rng)
		{
			enemiesStats = enemies;
			formationsData = formations;

			var enemizerattacks = flags.EnemizerAttacks;
			var enemizergroup = flags.EnemizerGroups;
			var progressive = flags.ProgressiveEnemizer;
			
			enemizerattacks = EnemizerAttacks.Balanced;
			enemizergroup = EnemizerGroups.MobsBosses;
			progressive = false;

			switch (enemizerattacks) 
			{
				case EnemizerAttacks.Balanced:
					Balanced(enemizergroup, false, progressive, rng);
					break;
				case EnemizerAttacks.BalancedExpert:
					Balanced(enemizergroup, true, progressive, rng);
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
			GeneratePowerLevels(skillResults);
		}
		private List<(EnemyCategory group, List<EnemyIds> enemies)> GetValidEnemies(EnemizerGroups group, bool progressive)
		{
			var validenemies = Mobs.ToList();
			if (group != EnemizerGroups.MobsOnly)
			{
				validenemies.AddRange(Bosses.Concat(DarkCastleBosses).ToList());
			}

			if (group == EnemizerGroups.MobsBossesDK)
			{
				validenemies.AddRange(DarkKing);
			}

			List<(EnemyCategory group, List<EnemyIds> enemies)> progenemies = new();
			if (progressive)
			{
				progenemies = EnemyInfo.EnemyCategories.Select(c => (c.Key, c.Value.Intersect(validenemies).ToList())).ToList();
				progenemies = progenemies.Where(g => g.enemies.Any()).ToList();

				// Don't group Dark Kings
				if (progenemies.TryFind(g => g.group == EnemyCategory.DarkKing, out var dkgroup))
				{
					progenemies.Remove(dkgroup);
					dkgroup.enemies.ForEach(dk => progenemies.Add((EnemyCategory.DarkKing, new List<EnemyIds>() { dk })));
				}
			}
			else
			{
				progenemies = validenemies.Select(e => (EnemyCategory.Imp, new List<EnemyIds>() { e })).ToList();
			}

			return progenemies;
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

				_EnemyAttackLinks[(int)EnemyIds.DarkKing] = new EnemyAttackLink(EnemyIds.DarkKing, _EnemyAttackLinks[(int)rng.TakeFrom(potentialScript)]);
				_EnemyAttackLinks[(int)EnemyIds.DarkKingWeapons] = new EnemyAttackLink(EnemyIds.DarkKingWeapons, _EnemyAttackLinks[(int)rng.TakeFrom(potentialScript)]);
				_EnemyAttackLinks[(int)EnemyIds.DarkKingSpider] = new EnemyAttackLink(EnemyIds.DarkKingSpider, _EnemyAttackLinks[(int)rng.TakeFrom(potentialScript)]);
			}

			// Assign scripts
			foreach (var enemygroup in switchList)
			{
				var groupA = enemygroup.groupa.enemies;
				var groupB = enemygroup.groupb.enemies;
				
				var tempattack0 = _EnemyAttackLinks[(int)groupA[0]];
				var tempattack1 = _EnemyAttackLinks[(int)groupA[1]];
				var tempattack2 = (groupA.Count > 2) ? _EnemyAttackLinks[(int)groupA[2]] : _EnemyAttackLinks[(int)groupA[1]];

				_EnemyAttackLinks[(int)groupA[0]] = new EnemyAttackLink(groupA[0], _EnemyAttackLinks[(int)groupB[0]]);
				_EnemyAttackLinks[(int)groupA[1]] = new EnemyAttackLink(groupA[1], _EnemyAttackLinks[(int)groupB[1]]);
				
				if (groupA.Count > 2)
				{
					_EnemyAttackLinks[(int)groupA[2]] = (groupB.Count > 2) ? new EnemyAttackLink(groupA[2], _EnemyAttackLinks[(int)groupB[2]]) : new EnemyAttackLink(groupA[2], _EnemyAttackLinks[(int)groupB[1]]); ;
				}

				_EnemyAttackLinks[(int)groupB[0]] = new EnemyAttackLink(groupB[0], tempattack0);
				_EnemyAttackLinks[(int)groupB[1]] = new EnemyAttackLink(groupB[1], tempattack1);

				if (groupB.Count > 2)
				{
					_EnemyAttackLinks[(int)groupB[2]] = (groupA.Count > 2) ? new EnemyAttackLink(groupB[2], tempattack2) : new EnemyAttackLink(groupB[2], tempattack1); ;
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

			List<EnemyIds> dkbosses = (group == EnemizerGroups.MobsBossesDK) ? DarkCastleBosses.Concat(DarkKing).ToList() : DarkCastleBosses;
			
			if (group == EnemizerGroups.MobsBossesDK)
			{
				while (dkbosses.Count > 1)
				{
					var tempbossA = rng.TakeFrom(dkbosses);
					var tempbossB = rng.TakeFrom(dkbosses);

					switchList.Add(tempbossA, tempbossB);
					switchList.Add(tempbossB, tempbossA);
				}
			}

			if (group == EnemizerGroups.MobsBosses)
			{
				var bosses = Bosses.Concat(dkbosses).ToList();

				while (bosses.Count > 1)
				{
					var tempbossA = rng.TakeFrom(bosses);
					var tempbossB = rng.TakeFrom(bosses);

					switchList.Add(tempbossA, tempbossB);
					switchList.Add(tempbossB, tempbossA);
				}
			}


			foreach (var link in _EnemyAttackLinks)
			{
				if (switchList.TryGetValue(link.Id, out var newid))
				{
					link.Id = newid;
				}
			}

			_EnemyAttackLinks = _EnemyAttackLinks.OrderBy(l => l.Id).ToList();
		}
		private void PadScripts(MT19337 rng)
		{
			// Remove Strong Psychshield since it might get used
			var zuhScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Zuh);
			zuhScript.Attacks[0x05] = EnemyAttackIds.HurricanePlus;

			// Extend Behemoth, Minotaur and DK1 attack list to avoid softlock
			var behemothScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Behemoth);
			behemothScript.Attacks = Enumerable.Repeat(EnemyAttackIds.Horn, 6).ToList();

			var minotaurScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Minotaur);
			minotaurScript.Attacks = new() { EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream, EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream };

			var dk1Script = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.DarkKing);
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

			var validenemies = GetValidEnemies(group, progressive);

			foreach (var catgroup in validenemies)
			{
				List<EnemyAttackIds> attacklist = new();
				List<EnemyAttackIds> nothinglist = new() { EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing };

				foreach (var enemy in catgroup.enemies)
				{
					int maxattack = _EnemyAttackLinks[(int)enemy].AttackCount;
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

					_EnemyAttackLinks[(int)enemy].Attacks = attacklist.GetRange(0, maxattack).Concat(nothinglist).ToList().GetRange(0, 6).ToList();
				}
			}

			if (group != EnemizerGroups.MobsOnly)
			{
				var icegolemattacks = possibleAttacks
					.Except(new List<EnemyAttackIds> { EnemyAttackIds.CureSelf, EnemyAttackIds.HealSelf, EnemyAttackIds.Selfdestruct, EnemyAttackIds.Multiply })
					.Except(Enumerable.Range((int)EnemyAttackIds.PsychshieldBug, 20).Select(x => (EnemyAttackIds)x))
					.ToList();

				iceGolemDesperateAttack = rng.PickFrom(icegolemattacks);
			}
		}
		private void GeneratePowerLevels(Dictionary<EnemyIds, (bool bad, int ailments)> skillResults)
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
				var enemies = skillResults.Where(r => formation.Value.Contains(r.Key)).Select(r => r.Key).ToList();
				var results = skillResults.Where(r => formation.Value.Contains(r.Key)).Select(r => r.Value).ToList();

				int baselevel = 0;
				int enemycount = enemies.Count;
				int maxhp = enemiesStats.Enemies().Where(e => enemies.Contains(e.Id)).Max(e => e.HP);

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

				enemiesStats.BossesPower.Add(EnemyInfo.BossesAccess[formation.Key], powerlevels[Math.Min(baselevel, 3)]);
			}

			foreach (var battlefield in EnemyInfo.BattlefieldFormations)
			{
				int minlevel = 3;
				foreach (var formation in battlefield.Value)
				{
					var enemies = formationsData.Formations[formation].Enemies;
					var results = skillResults.Where(r => enemies.Contains(r.Key)).Select(r => r.Value).ToList();

					int baselevel = 0;
					int enemycount = enemies.Count;
					int maxhp = enemiesStats.Enemies().Where(e => enemies.Contains(e.Id)).Max(e => e.HP);

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

				enemiesStats.BattlefieldsPower.Add(battlefield.Key, powerlevels[Math.Min(minlevel, 3)]);
			}
		}
		public void Balanced(EnemizerGroups group, bool expert, bool progressive, MT19337 rng)
		{
			var validenemies = GetValidEnemies(group, progressive);
			
			// Select attacks
			var validattacks = _EnemyAttackLinks.Where(l => Mobs.Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList();
			List<EnemyAttackIds> dkattacks = new();

			if (group != EnemizerGroups.MobsOnly)
			{
				validattacks.AddRange(_EnemyAttackLinks.Where(l => Bosses.Concat(DarkCastleBosses).Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList());
			}

			if (group == EnemizerGroups.MobsBossesDK)
			{
				dkattacks.AddRange(_EnemyAttackLinks.Where(l => DarkKing.Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList());
				dkattacks.RemoveAll(a => a == EnemyAttackIds.Nothing);
			}

			List<EnemyAttackIds> invalidattacks = new() { EnemyAttackIds.CureSelf, EnemyAttackIds.HealSelf, EnemyAttackIds.Selfdestruct, EnemyAttackIds.Multiply, EnemyAttackIds.Psychshield, EnemyAttackIds.PsychshieldBug, EnemyAttackIds.Nothing };
			validattacks.RemoveAll(invalidattacks.Contains);
			validattacks = validattacks.Distinct().ToList();

			var safeattacks = validattacks.Intersect(safeAttacks).ToList();
			var badattacks = validattacks.Intersect(ailmentAttacks.Concat(strongAttacks)).ToList();

			// Distribute attacks
			foreach (var catgroup in validenemies)
			{
				List<EnemyAttackIds> attacklist = new();
				List<EnemyAttackIds> nothinglist = new() { EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing, EnemyAttackIds.Nothing };
				
				List<EnemyAttackIds> attackpool = new(validattacks);

				bool firstenemydone = false;
				int ailmentPicked = 0;
				int badPicked = 0;

				foreach (var enemy in catgroup.enemies)
				{
					int maxattack = _EnemyAttackLinks[(int)enemy].AttackCount;

					if (DarkCastleBosses.Concat(DarkKing).Contains(enemy))
					{
						attackpool.AddRange(dkattacks);
					}

					while (attacklist.Count < maxattack)
					{
						List<EnemyAttackIds> validAttacks = new();
						
						if ((maxattack == 2) || (maxattack == 3 && badPicked > 0) || (maxattack > 3 && badPicked > 1))
						{
							validAttacks = attackpool.Intersect(safeattacks).ToList();
						}
						else if (expert)
						{
							validAttacks = attackpool.Intersect(badattacks).ToList();
						}
						else
						{
							validAttacks = attackpool.Intersect(safeattacks.Concat(badattacks)).ToList();
						}

						var newAttack = rng.PickFrom(validAttacks.Except(attacklist).ToList());

						if (badattacks.Contains(newAttack))
						{
							badPicked++;
						}
						
						if (ailmentAttacks.Contains(newAttack))
						{
							ailmentPicked++;
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
						List<EnemyAttackIds> badhydra = attacklist.Intersect(badattacks).ToList();
						List<EnemyAttackIds> goodhydra = attacklist.Intersect(safeattacks).ToList();

						List<List<EnemyAttackIds>> hydraattacks = new() { new(), new() };

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

					_EnemyAttackLinks[(int)enemy].Attacks = attacklist.GetRange(0, maxattack).Concat(nothinglist).ToList().GetRange(0, 6).ToList();
					//skillResults.Add(enemy, (badPicked > 0, ailmentPicked));
				}
			}

			iceGolemDesperateAttack = rng.PickFrom(safeattacks);
		}
		private void AnalyzeAttackLevel()
		{
			foreach (var link in _EnemyAttackLinks)
			{
				var strongattacks = link.Attacks.Intersect(strongAttacks).Count();
				var ailmentattacks = link.Attacks.Intersect(ailmentAttacks).Count();

				skillResults.Add(link.Id, ((strongattacks + ailmentattacks) > 0, ailmentattacks));
			}
		}
		private void Selfdestruct(EnemizerGroups group)
		{
			// Never progressive
			var validenemies = GetValidEnemies(group, false);

			foreach (var enemy in validenemies.SelectMany(e => e.enemies))
			{
				var ea = _EnemyAttackLinks[(int)enemy];
				ea.AttackPattern = 0x01;
				ea.Attacks = Enumerable.Repeat(EnemyAttackIds.Selfdestruct, 6).ToList();
			}

			AnalyzeAttackLevel();
		}

		private List<EnemyAttackIds> safeAttacks = new()
		{
			EnemyAttackIds.BoneMissile,
			EnemyAttackIds.DoubleHit,
			EnemyAttackIds.DoubleSword,
			EnemyAttackIds.Cure,
			EnemyAttackIds.DoubleSword,
			EnemyAttackIds.PsychshieldBug,
			EnemyAttackIds.Blizzard,
			EnemyAttackIds.Muffle,
			EnemyAttackIds.SilenceSong,
			EnemyAttackIds.Windstorm,
			EnemyAttackIds.StoneBlock,
			EnemyAttackIds.ThunderSpell,
			EnemyAttackIds.PowerDrain,
			EnemyAttackIds.Mucus,
			EnemyAttackIds.BlindFlash,
			EnemyAttackIds.Electrapulse,
			EnemyAttackIds.Punch,
			EnemyAttackIds.Spark,
			EnemyAttackIds.Scream,
			EnemyAttackIds.PoisonFlour,
			EnemyAttackIds.Fire,
			EnemyAttackIds.TwinFang,
			EnemyAttackIds.Claw,
			EnemyAttackIds.BowArrow,
			EnemyAttackIds.PoisonSting,
			EnemyAttackIds.Blinder,
			EnemyAttackIds.Psudopod,
			EnemyAttackIds.IronNail,
			EnemyAttackIds.QuickSand,
			EnemyAttackIds.Thunder,
			EnemyAttackIds.ThunderBeam,
			EnemyAttackIds.WhipTongue,
			EnemyAttackIds.Sting,
			EnemyAttackIds.Horn,
			EnemyAttackIds.MegaPunch,
			EnemyAttackIds.Branch,
			EnemyAttackIds.PoisonFluid,
			EnemyAttackIds.BlowDart,
			EnemyAttackIds.Fang,
			EnemyAttackIds.Scissor,
			EnemyAttackIds.Bearhug,
			EnemyAttackIds.Axe,
			EnemyAttackIds.Hurricane,
			EnemyAttackIds.Blackness,
			EnemyAttackIds.ChewOff,
			EnemyAttackIds.Coldness,
			EnemyAttackIds.Smokescreen,
			EnemyAttackIds.Tail,
			EnemyAttackIds.Stab,
			EnemyAttackIds.RottonMucus,
			EnemyAttackIds.Fin,
			EnemyAttackIds.AxeBomber,
			EnemyAttackIds.Piledriver,
			EnemyAttackIds.Wraparound,
			EnemyAttackIds.Beak,
			EnemyAttackIds.Dive,
			EnemyAttackIds.PoisonSnake,
			EnemyAttackIds.BodySlam,
			EnemyAttackIds.HurricanePlus,
			EnemyAttackIds.PoisonThorn,
			EnemyAttackIds.IceBlock,
			EnemyAttackIds.HydroAcid,
			EnemyAttackIds.Kick,
			EnemyAttackIds.Dissolve,
			EnemyAttackIds.Watergun,
			EnemyAttackIds.Tornado,
			EnemyAttackIds.Attatch,
			EnemyAttackIds.IcyFoam,
			EnemyAttackIds.Drain,
			EnemyAttackIds.Roundhouse,
			EnemyAttackIds.Bite,
			EnemyAttackIds.Scimitar,
			EnemyAttackIds.HeadButt,
			EnemyAttackIds.IceBreath,
			EnemyAttackIds.GiantBlade,
			EnemyAttackIds.Scrunch,
			EnemyAttackIds.RipEarth,
			EnemyAttackIds.Dash,
			EnemyAttackIds.FullNelson,
			EnemyAttackIds.Uppercut,
			EnemyAttackIds.DragonCut,
			EnemyAttackIds.Poisonbreath,
			EnemyAttackIds.NeckChoke,
			EnemyAttackIds.Sword,
			EnemyAttackIds.Beam,
			EnemyAttackIds.SuckerStick
		};

		private List<EnemyAttackIds> ailmentAttacks = new()
		{
			EnemyAttackIds.DoomPowder,
			EnemyAttackIds.DoomGaze,
			EnemyAttackIds.HealSelf,
			EnemyAttackIds.SpookyLaugh,
			EnemyAttackIds.HypnoSleep,
			EnemyAttackIds.Riddle,
			EnemyAttackIds.CorrodeGas,
			EnemyAttackIds.Gaze,
			EnemyAttackIds.SonicBoom,
			EnemyAttackIds.DoomGaze,
			EnemyAttackIds.SleepLure,
			EnemyAttackIds.SleepPowder,
			EnemyAttackIds.Stare,
			EnemyAttackIds.ParaGas,
			EnemyAttackIds.Bark,
			EnemyAttackIds.Lullaby,
			EnemyAttackIds.ParaNeedle,
			EnemyAttackIds.BadBreath,
			EnemyAttackIds.ParaClaw,
			EnemyAttackIds.BodyOdor,
			EnemyAttackIds.ParaSnake,
			EnemyAttackIds.ParaStare,
			EnemyAttackIds.ParaBreath,
			EnemyAttackIds.Screechvoice,
			EnemyAttackIds.SilverWeb,
			EnemyAttackIds.StoneGaze,
			EnemyAttackIds.GoldenWeb,
			EnemyAttackIds.StoneBeak
		};

		private List<EnemyAttackIds> strongAttacks = new()
		{
			EnemyAttackIds.DoubleKick,
			EnemyAttackIds.StompStomp,
			EnemyAttackIds.TwinShears,
			EnemyAttackIds.Flame,
			EnemyAttackIds.Rapier,
			EnemyAttackIds.ChokeUp,
			EnemyAttackIds.FireBreath,
			EnemyAttackIds.FireBall,
			EnemyAttackIds.HeatwaveUnused,
			EnemyAttackIds.Typhoon,
			EnemyAttackIds.Quake,
			EnemyAttackIds.Whirlwater,
			EnemyAttackIds.Heatwave,
			EnemyAttackIds.QuakeSpell,
			EnemyAttackIds.Psychshield,
			EnemyAttackIds.MirrorSword,
			EnemyAttackIds.PiledriverFixed,
			EnemyAttackIds.DarkCane,
			EnemyAttackIds.DarkSaber,
			EnemyAttackIds.FlameSweep,
			EnemyAttackIds.TripleFang,
			EnemyAttackIds.Lazer,
			EnemyAttackIds.FlamePillar,
			EnemyAttackIds.SixPsudopods,
			EnemyAttackIds.SnakeHead,
			EnemyAttackIds.Headboomerang,
			EnemyAttackIds.SpiderKids,
			EnemyAttackIds.MegaFlare,
			EnemyAttackIds.Snowstorm,
			EnemyAttackIds.FireSword,
			EnemyAttackIds.CureArrow,
			EnemyAttackIds.ThreeHeads,
			EnemyAttackIds.SkyAttack,
			EnemyAttackIds.IceSword,
			EnemyAttackIds.MegaWhite,
			EnemyAttackIds.QuakeAxe,
			EnemyAttackIds.SkyAttackFixed,
			EnemyAttackIds.FireBreathFixed,
			EnemyAttackIds.Multiply
		};
		// deprecated, harvest for reference data down the road
		private void SafeShuffleAttacks(bool highhpscaling, MT19337 rng)
		{
			List<AttackPattern> standardPatterns = new()
			{
				new AttackPattern(0x00, 2, 0, 2, 0xFF),
				new AttackPattern(0x01, 2, 2, 0, 0xFF),
				new AttackPattern(0x02, 2, 1, 1, 0xFF),
				new AttackPattern(0x03, 1, 1, 2, 0xFF),
				new AttackPattern(0x04, 0, 3, 3, 0xFF),
				new AttackPattern(0x05, 0, 4, 2, 0xFF),
				new AttackPattern(0x06, 1, 3, 2, 0xFF),
				new AttackPattern(0x07, 0, 2, 4, 0xFF),
				//new AttackPattern(0x08, 0, 3, 5, 0xFF),
				//new AttackPattern(0x09, 0, 4, 4, 0xFF),
				//new AttackPattern(0x0A, 0, 6, 2, 0xFF),
				new AttackPattern(0x0B, 2, 0, 2, 0x02),
				new AttackPattern(0x0C, 1, 1, 4, 0x02),
				new AttackPattern(0x0D, 0, 3, 3, 0x03),
				//new AttackPattern(0x0E, 0, 3, 5, 0x03),
				//new AttackPattern(0x0F, 0, 4, 4, 0x04),
			};

			List<AttackPattern> bossPatterns = new()
			{
				new AttackPattern(0x4A, 1, 2, 3, 0xFF), // Flamerus Rex
				new AttackPattern(0x4B, 0, 3, 2, 0xFF), // Ice Golem
				new AttackPattern(0x4C, 3, 0, 0, 0xFF), // Hydra phase 1
				new AttackPattern(0x4D, 3, 0, 0, 0xFF), // Twinhead phase 1
				new AttackPattern(0x4E, 0, 4, 1, 0xFF), // Pazuzu
				new AttackPattern(0x4F, 0, 5, 0, 0xFF), // Zuh
				new AttackPattern(0x50, 1, 3, 0, 0xFF), // DarkKing 1
				new AttackPattern(0x51, 0, 4, 2, 0x03), // DarkKing 2
				new AttackPattern(0x52, 0, 6, 0, 0x02), // DarkKing 3+4
				new AttackPattern(0x40, 0, 3, 3, 0x03), // Skullrus Rex
				new AttackPattern(0x41, 2, 2, 0, 0xFF), // Stone Golem
				new AttackPattern(0xFE, 2, 0, 1, 0xFF), // Hydra/Twinhead phase 3
				new AttackPattern(0xFF, 2, 0, 1, 0xFF), // Hydra/Twinhead phase 3
			};

			List<int> standardEnemies = Enumerable.Range(0, 0x40).ToList();
			List<int> miniBosses = Enumerable.Range(0x42, 8).ToList();
			List<int> crystalBosses = new List<int>() { 0x4A, 0x4B, 0x4C, 0x4E, 0xFE };
			List<int> endBosses = new List<int>() { 0x40, 0x41, 0x4D, 0x4F, 0x50, 0x51, 0x52, 0xFF };

			List<int> allAttacks = Enumerable.Range(0x40, 156).ToList();

			List<int> lowHpAttacks = Enumerable.Range(0x5A, 14).ToList();
			lowHpAttacks.AddRange(new List<int>() { 0xB9, 0xBA, 0xBE, 0xBF, 0xC0, 0xC5, 0xC7 });

			List<int> midHpAttacks = Enumerable.Range(0x6C, 38).ToList();
			midHpAttacks.AddRange(new List<int>() { 0x68, 0x69, 0xB8, 0xBB, 0xC4, 0xC6, 0xD9, 0xDA });
		   
			List<int> statusHpAttacks = Enumerable.Range(0x92, 17).ToList();
			
			List<int> highHpAttacks = Enumerable.Range(0x80, 18).ToList();
			highHpAttacks.AddRange(new List<int>() { 0xBC, 0xBD, 0xD7, 0xD8, 0xDB });

			List<int> dkAttacks = Enumerable.Range(0xCA, 13).ToList();
			List<int> psychshieldAttacks = Enumerable.Range(0xC8, 2).ToList();
			List<int> strongAttacks = new List<int>() { 0x45, 0x59, 0x6A, 0x6B, 0x7E, 0x80, 0x82, 0x84, 0x85, 0x8A, 0x8E, 0x9F, 0xBB };
			List<int> deathstoneAttacks = new List<int>() { 0x56, 0x57, 0x92, 0x93, 0xA2, 0xB4, 0xB5 };

			int multiply = 0xC2;
			int selfdestruct = 0xC1;
			byte healSpell = 0x4A;
			byte cureSpell = 0x49; 

			allAttacks.Remove(cureSpell);
			allAttacks.Remove(healSpell);

			int cureCasters = 5;
			int healCasters = 4;
			int selfdestructs = 3;
			int oneTrackMinds = rng.Between(1, 5);
			int multipliers = 4;

			List<int> commonAttacks = allAttacks.Except(dkAttacks).Except(psychshieldAttacks).Except(strongAttacks).Except(deathstoneAttacks).ToList();
			List<int> rareAttacks = strongAttacks.Concat(deathstoneAttacks).ToList();

			foreach (var enemy in standardEnemies.Concat(miniBosses))
			{
				var pattern = rng.PickFrom(standardPatterns);
				int minimum = (oneTrackMinds > 0) ? 1 : 2;
				int maximum = pattern.Count;
				bool healCaster = false;
				bool cureCaster = false;

				if (cureCasters > 0)
				{
					var randomvalue = rng.Between(0, 10);
					if (randomvalue == 0)
					{
						cureCaster = true;
						cureCasters--;
					}
				}

				if (healCasters > 0)
				{
					var randomvalue = rng.Between(0, 10);
					if (randomvalue == 0)
					{
						healCaster = true;
						healCasters--;
					}
				}

				if (pattern.Opener < 0xFF)
				{
					minimum = (pattern.Opener + 1);
				}

				int noOfAttacks = rng.Between(minimum, maximum);
				if (noOfAttacks == 1)
				{
					oneTrackMinds--;
				}

				int nastyAttackBudget = Min(noOfAttacks / 2, pattern.Rare);

				List<int> attackList = new();

				for (int i = 0; i < noOfAttacks; i++)
				{
					if (nastyAttackBudget > 0)
					{
						var selectAttack = rng.PickFrom(commonAttacks.Concat(rareAttacks).ToList());
						attackList.Add(selectAttack);
						if (rareAttacks.Contains(selectAttack))
						{
							nastyAttackBudget--;
						}

						if (selectAttack == selfdestruct)
						{
							selfdestructs--;
							if (selfdestructs <= 0)
							{
								commonAttacks.Remove(selfdestruct);
							}
						}

						if (selectAttack == multiply)
						{
							multipliers--;
							if (multipliers <= 0)
							{
								commonAttacks.Remove(multiply);
							}
						}
					}
					else
					{
						attackList.Add(rng.PickFrom(commonAttacks));
					}
				}

				attackList = attackList.OrderBy(a => rareAttacks.Contains(a)).ToList();

				while (attackList.Count < 6)
				{
					attackList.Add(0xFF);
				}

				_EnemyAttackLinks[enemy].AttackPattern = (byte)pattern.Id;
				//_EnemyAttackLinks[enemy].Attacks = attackList.Select(x => (byte)x).ToArray();
				//_EnemyAttackLinks[enemy].CastHeal = (byte)(healCaster ? healSpell : 0xFF);
				//_EnemyAttackLinks[enemy].CastCure = (byte)(cureCaster ? cureSpell : 0xFF);
			}

			commonAttacks = allAttacks.Except(dkAttacks).Except(psychshieldAttacks).Except(deathstoneAttacks).Except(highHpAttacks).Except(midHpAttacks).Except(lowHpAttacks).Except(statusHpAttacks).ToList();
			commonAttacks.Remove(multiply);
			commonAttacks.Remove(selfdestruct);
			//rareAttacks = stronHpAttacks.Concat(deathstoneAttacks).ToList();

			foreach (var boss in crystalBosses.Concat(endBosses))
			{
				var pattern = bossPatterns.Find(x => x.Id == boss);
				int noOfAttacks = pattern.Count;
				int nastyAttackBudget = pattern.Rare;

				List<int> attackList = new();

				List<int> validCommonAttacks;

				if (highhpscaling)
				{
					rareAttacks = deathstoneAttacks.Concat(lowHpAttacks).Concat(statusHpAttacks).ToList();
					validCommonAttacks = endBosses.Contains(boss) ? commonAttacks.Concat(dkAttacks).ToList() : commonAttacks;
				}
				else
				{
					rareAttacks = deathstoneAttacks.Concat(midHpAttacks).Concat(statusHpAttacks).ToList();
					validCommonAttacks = endBosses.Contains(boss) ? commonAttacks.Concat(dkAttacks).Concat(lowHpAttacks).ToList() : commonAttacks.Concat(lowHpAttacks).ToList();
				}

				for (int i = 0; i < noOfAttacks; i++)
				{

					if (nastyAttackBudget > 0)
					{ 
						var selectAttack = rng.PickFrom(validCommonAttacks.Concat(rareAttacks).ToList());
						attackList.Add(selectAttack);
						if (rareAttacks.Contains(selectAttack))
						{
							nastyAttackBudget--;
						}
					}
					else
					{
						attackList.Add(rng.PickFrom(validCommonAttacks));
					}
				}

				attackList = attackList.OrderBy(a => rareAttacks.Contains(a)).ToList();

				while (attackList.Count < 6)
				{
					attackList.Add(0xFF);
				}

				if (boss < 0xF0)
				{
					//_EnemyAttackLinks[boss].Attacks = attackList.Select(x => (byte)x).ToList().ToArray();
				}
				else
				{
					// special check hydras as their scripts is splitted
					var targetHydra = (boss == 0xFE ? 0x4C : 0x4D);

					//_EnemyAttackLinks[targetHydra].Attacks[3] = (byte)attackList[0];
					//_EnemyAttackLinks[targetHydra].Attacks[4] = (byte)attackList[1];
					//_EnemyAttackLinks[targetHydra].Attacks[5] = (byte)attackList[2];
				}
			}


			var dkPhase2Attacks = _EnemyAttackLinks[0x51].Attacks.ToList();
			var dkPhase3Attacks = _EnemyAttackLinks[0x52].Attacks.ToList();

			//_darkKingAttackLinkBytes = dkPhase2Attacks.Concat(dkPhase3Attacks).ToArray();
		}

	}
	public class Attacks
	{
		private List<Attack> _attacks;

		private const int AttacksQty = 0xA9;

		public Attacks(FFMQRom rom)
		{
			_attacks = new List<Attack>();

			for (int i = 0; i < AttacksQty; i++)
			{
				_attacks.Add(new Attack(i, rom));
			}
		}
		public Attack this[int attackid]
		{
			get => _attacks[attackid];
			set => _attacks[attackid] = value;
		}
		public IList<Attack> AllAttacks()
		{
			return _attacks.AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{
			foreach (Attack e in _attacks)
			{
				e.Write(rom);
			}
		}
		public void ScaleAttacks(Flags flags, MT19337 rng)
		{
			int lowerbound = 100;
			int upperbound = 100;

			switch (flags.EnemiesScalingLower)
			{
				case EnemiesScaling.Quarter: lowerbound = 25; break;
				case EnemiesScaling.Half: lowerbound = 50; break;
				case EnemiesScaling.ThreeQuarter: lowerbound = 75; break;
				case EnemiesScaling.Normal: lowerbound = 100; break;
				case EnemiesScaling.OneAndHalf: lowerbound = 150; break;
				case EnemiesScaling.Double: lowerbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: lowerbound = 250; break;
			}
		   
			switch (flags.EnemiesScalingUpper)
			{
				case EnemiesScaling.Quarter: upperbound = 25; break;
				case EnemiesScaling.Half: upperbound = 50; break;
				case EnemiesScaling.ThreeQuarter: upperbound = 75; break;
				case EnemiesScaling.Normal: upperbound = 100; break;
				case EnemiesScaling.OneAndHalf: upperbound = 150; break;
				case EnemiesScaling.Double: upperbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: upperbound = 250; break;
			}

			foreach (Attack e in _attacks)
			{
				if (upperbound < lowerbound)
				{
					upperbound = lowerbound;
				}

				int randomizedScaling = upperbound;
				int spread = upperbound - lowerbound;

				if (spread != 0)
				{
					int max = upperbound;
					int min = lowerbound;

					randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
				}
				e.Power = (byte)Min(0xFF, Max(0x01, e.Power * randomizedScaling / 100));
			}
		}
	}
	public class Attack
	{
		private Blob _rawBytes;
		public byte Unknown1 { get; set; }
		public byte Unknown2 { get; set; }
		public byte Power { get; set; }
		public byte AttackType { get; set; }
		public byte AttackSound { get; set; }
		// my suspicion is that this unknown (or one of the other two) are responsible for targeting self, one PC or both PC.
		public byte Unknown3 { get; set; }
		public byte AttackTargetAnimation { get; set; }
		private int _Id;

		private const int AttacksAddress = 0xBC78; // Bank 02
		private const int AttacksBank = 0x02;
		private const int AttacksLength = 0x07;

		public Attack(int id, FFMQRom rom)
		{
			_rawBytes = rom.GetFromBank(AttacksBank, AttacksAddress + (id * AttacksLength), AttacksLength);

			_Id = id;
			Unknown1 = _rawBytes[0];
			Unknown2 = _rawBytes[1];
			Power = _rawBytes[2];
			AttackType = _rawBytes[3];
			AttackSound = _rawBytes[4];
			Unknown3 = _rawBytes[5];
			AttackTargetAnimation = _rawBytes[6];
		}
		public int Id()
		{
			return _Id;
		}

		public void Write(FFMQRom rom)
		{
			_rawBytes[0] = Unknown1;
			_rawBytes[1] = Unknown2;
			_rawBytes[2] = Power;
			_rawBytes[3] = AttackType;
			_rawBytes[4] = AttackSound;
			_rawBytes[5] = Unknown3;
			_rawBytes[6] = AttackTargetAnimation;
			rom.PutInBank(AttacksBank, AttacksAddress + (_Id * AttacksLength), _rawBytes);
		}
	}

	public class AttackPattern
	{
		public int Id { get; set; }
		public int Common { get; set; }
		public int Uncommon { get; set; }
		public int Rare { get; set; }
		public int Count { get => Common + Uncommon + Rare; }
		public int Opener { get; set; }

		public AttackPattern(int id, int common, int uncommon, int rare, int opener)
		{
			Id = id;
			Common = common;
			Uncommon = uncommon;
			Rare = rare;
			Opener = opener;
		}
	}
}
