using RomUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFMQLib
{
	public partial class Enemizer
	{
		static public List<EnemyIds> Mobs = Enumerable.Range((int)EnemyIds.Brownie, 0x40).Select(e => (EnemyIds)e).ToList();
		static public List<EnemyIds> Bosses = Enumerable.Range((int)EnemyIds.Behemoth, 8).Select(e => (EnemyIds)e).ToList().Concat(new List<EnemyIds>() { EnemyIds.FlamerusRex, EnemyIds.IceGolem, EnemyIds.DualheadHydra, EnemyIds.Pazuzu }).ToList();
		static public List<EnemyIds> DarkCastleBosses = new() { EnemyIds.SkullrusRex, EnemyIds.StoneGolem, EnemyIds.TwinheadWyvern, EnemyIds.Zuh };
		static public List<EnemyIds> DarkKing = new() { EnemyIds.DarkKing, EnemyIds.DarkKingWeapons, EnemyIds.DarkKingSpider };

		private Enemies enemies;
		private EnemyAttackLinks attacks;
		private FormationsData formations;
		private EnemyPalettes enemyPalettes;

		public Dictionary<AccessReqs, AccessReqs> BossesPower;
		public Dictionary<LocationIds, AccessReqs> BattlefieldsPower;

		public Enemizer(Enemies _enemies, EnemyAttackLinks _attacks, FormationsData _formations, EnemyPalettes _palettes)
		{ 
			enemies = _enemies;
			enemyPalettes = _palettes;
			attacks = _attacks;
			formations = _formations;
			ElementalEnemies = new();
			SkillResults = new();
			BossesPower = new();
			BattlefieldsPower = new();
		}
		public void Process(Flags flags, GameInfoScreen gameinfoscreen, MT19337 rng)
		{
			CreateElementalEnemies(flags.EnemizerAttacks == EnemizerAttacks.Elemental, flags.EnemizerGroups, false, rng);
			ScaleEnemies(flags, rng);
			ShuffleAttacks(flags, rng);
			SetElementalResistWeakness();
			ShuffleResistWeakness(flags.ShuffleResWeakType, gameinfoscreen, rng);
			UpdateNames(rng);
			UpdatePalettes(flags.EnemizerAttacks == EnemizerAttacks.Elemental);
		}

		static public List<(EnemyCategory group, List<EnemyIds> enemies)> GetValidEnemies(EnemizerGroups group, bool progressive)
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
	}
}
