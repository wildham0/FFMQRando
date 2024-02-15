using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;

namespace FFMQLib
{

	public enum EnemiesScaling : int
	{
		[Description("25%")]
		Quarter = 0,
		[Description("50%")]
		Half,
		[Description("75%")]
		ThreeQuarter,
		[Description("100%")]
		Normal,
		[Description("125%")]
		OneAndQuarter,
		[Description("150%")]
		OneAndHalf,
		[Description("200%")]
		Double,
		[Description("250%")]
		DoubleAndHalf,
		[Description("300%")]
		Triple,
	}
	public enum EnemiesScalingSpread : int
	{
		[Description("0%")]
		None = 0,
		[Description("25%")]
		Quarter,
		[Description("50%")]
		Half,
		[Description("100%")]
		Full,
	}
	public enum EnemizerAttacks : int
	{
		[Description("Disabled")]
		Normal = 0,
        [Description("Script Shuffle")]
        SimpleShuffle,
        [Description("Safe Randomization")]
		Safe,
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


	public enum ElementsType
	{ 
		Silence = 0x0001,
		Blind = 0x0002,
		Poison = 0x0004,
		Confusion = 0x0008,
		Sleep = 0x0010,
		Paralysis = 0x0020,
		Stone = 0x0040,
		Doom = 0x0080,
		Projectile = 0x0100,
		Bomb = 0x0200,
		Axe = 0x0400,
		Zombie = 0x0800,
		Air = 0x1000,
		Fire = 0x2000,
		Water = 0x4000,
		Earth = 0x8000,
	}
	public class Typing
	{ 
		public List<(int, string)> Elements = new() {
            (0x01, "Projectile"),
            (0x02, "Bomb"),
            (0x04, "Axe"),
            (0x08, "Zombie"),
            (0x10, "Air"),
            (0x20, "Fire"),
            (0x40, "Water"),
            (0x80, "Earth")
        };

		public List<(int, string)> Status = new() {
            (0x01, "Silence"),
            (0x02, "Blind"),
            (0x04, "Poison"),
            (0x08, "Confusion"),
            (0x10, "Sleep"),
            (0x20, "Paralysis"),
            (0x40, "Stone"),
            (0x80, "Doom")
        };
    }

    public class EnemiesStats
	{
		private List<Enemy> _enemies;
		
		private const int EnemiesStatsQty = 0x53;

		public EnemiesStats(FFMQRom rom)
		{
			_enemies = new List<Enemy>();

			for (int i = 0; i < EnemiesStatsQty; i++)
			{
				_enemies.Add(new Enemy(i, rom));
			}
		}
		public Enemy this[int id]
		{
			get => _enemies[id];
			set => _enemies[id] = value;
		}
		public IList<Enemy> Enemies()
		{
			return _enemies.AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{
			foreach (Enemy e in _enemies)
			{
				e.Write(rom);
			}
		}
		private byte ScaleStat(byte value, int scaling, int spread, MT19337 rng)
		{
			int randomizedScaling = scaling;
			if (spread != 0)
			{
				int max = scaling + spread;
				int min = Max(25, scaling - spread);

				randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
			}
			return (byte)Min(0xFF, Max(0x01, value * randomizedScaling / 100));
		}
		private ushort ScaleHP(ushort value, int scaling, int spread, MT19337 rng)
		{
			int randomizedScaling = scaling;
			if (spread != 0)
			{
				int max = scaling + spread;
				int min = Max(25, scaling - spread);

				randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
			}
			return (ushort)Min(0xFFFF, Max(0x01, value * randomizedScaling / 100));
		}
		public void ShuffleResistWeakness(bool shuffle, GameInfoScreen info, MT19337 rng)
		{
			if (!shuffle)
			{
				return;
			}
			
			var allList = Enum.GetValues<ElementsType>().ToList();
			var elementsMainList = allList.Where(x => (int)x > 0x00FF).ToList();
			var statusMainList = allList.Where(x => (int)x < 0x0100).ToList();
			var elementsShuffledList = elementsMainList.ToList();
			var statusShuffledList = statusMainList.ToList();

			elementsShuffledList.Shuffle(rng);
			statusShuffledList.Shuffle(rng);

			var elementsPairList = elementsMainList.Select((e, i) => (e, elementsShuffledList[i])).ToList();
			var statusPairList = statusMainList.Select((e, i) => (e, statusShuffledList[i])).ToList();

			var allPairList = statusPairList.Concat(elementsPairList).ToList();

			foreach (var enemy in _enemies)
			{
				List<ElementsType> newweaks = allPairList.Where(w => enemy.Weaknesses.Contains(w.Item1)).Select(w => w.Item2).ToList();
				List<ElementsType> newresists = allPairList.Where(w => enemy.Resistances.Contains(w.Item1)).Select(w => w.Item2).ToList();

				enemy.Weaknesses = newweaks.ToList();
				enemy.Resistances = newresists.ToList();
			}

			info.ShuffledElementsType = allPairList;
		}
		public void ScaleEnemies(Flags flags, MT19337 rng)
		{
			List<int> enemiesId = Enumerable.Range(0, 0x40).ToList();
			List<int> bossesId  = Enumerable.Range(0x40, EnemiesStatsQty - enemiesId.Count).ToList();

			ScaleStats(flags.EnemiesScalingLower, flags.EnemiesScalingUpper, enemiesId, rng);
			ScaleStats(flags.BossesScalingLower, flags.BossesScalingUpper, bossesId, rng);
		}
		public void ScaleStats(EnemiesScaling lowerboundscaling, EnemiesScaling upperboundscaling, List<int> validEnemies, MT19337 rng)
		{
			int lowerbound = 100;
			int upperbound = 100;

			switch (lowerboundscaling)
			{
				case EnemiesScaling.Quarter: lowerbound = 25; break;
				case EnemiesScaling.Half: lowerbound = 50; break;
				case EnemiesScaling.ThreeQuarter: lowerbound = 75; break;
				case EnemiesScaling.Normal: lowerbound = 100; break;
				case EnemiesScaling.OneAndQuarter: lowerbound = 125; break;
				case EnemiesScaling.OneAndHalf: lowerbound = 150; break;
				case EnemiesScaling.Double: lowerbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: lowerbound = 250; break;
				case EnemiesScaling.Triple: lowerbound = 300; break;
			}

			switch (upperboundscaling)
			{
				case EnemiesScaling.Quarter: upperbound = 25; break;
				case EnemiesScaling.Half: upperbound = 50; break;
				case EnemiesScaling.ThreeQuarter: upperbound = 75; break;
				case EnemiesScaling.Normal: upperbound = 100; break;
				case EnemiesScaling.OneAndQuarter: upperbound = 125; break;
				case EnemiesScaling.OneAndHalf: upperbound = 150; break;
				case EnemiesScaling.Double: upperbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: upperbound = 250; break;
				case EnemiesScaling.Triple: upperbound = 300; break;
			}

			if (upperbound < lowerbound)
			{
				upperbound = lowerbound;
			}

			int spread = (upperbound - lowerbound) / 2;
			int scaling = lowerbound + spread;

			List<Enemy> selectedEnemies = _enemies.Where((x, i) => validEnemies.Contains(i)).ToList();

			foreach (Enemy e in selectedEnemies)
			{
				e.HP = ScaleHP(e.HP, scaling, spread, rng);
				e.AttackPower = ScaleStat(e.AttackPower, scaling, spread, rng);
				e.DamageReduction = ScaleStat(e.DamageReduction, scaling, spread, rng);
				e.Speed = Max((byte)0x03, ScaleStat(e.Speed, scaling, spread, rng));
				e.MagicPower = ScaleStat(e.MagicPower, scaling, spread, rng);
				e.Accuracy = ScaleStat(e.Accuracy, scaling, spread, rng);
				e.Evasion = ScaleStat(e.Evasion, scaling, spread, rng);
			}
		}
	}
	public class Enemy
	{
		private Blob _rawBytes;
		
		public ushort HP { get; set; }
		public byte AttackPower { get; set; }
		public byte DamageReduction { get; set; }
		public byte Speed { get; set; }
		public byte MagicPower { get; set; }
		public byte Accuracy { get; set; }
		public byte Evasion { get; set; }
		public List<ElementsType> Resistances { get; set; }
		public List<ElementsType> Weaknesses { get; set; }

		private int _Id;

		private const int EnemiesStatsAddress = 0xC275; // Bank 02
		private const int EnemiesStatsBank = 0x02;
		private const int EnemiesStatsLength = 0x0e;

        public int Id()
		{
			return _Id;
		}

		public Enemy(int id, FFMQRom rom)
		{
			_rawBytes = rom.GetFromBank(EnemiesStatsBank, EnemiesStatsAddress + (id * EnemiesStatsLength), EnemiesStatsLength);

			_Id = id;
			HP = (ushort)(_rawBytes[1] * 0x100 + _rawBytes[0]);
			AttackPower = _rawBytes[2];
			DamageReduction = _rawBytes[3];
			Speed = _rawBytes[4];
			MagicPower = _rawBytes[5];
			Accuracy = _rawBytes[0x0a];
			Evasion = _rawBytes[0x0b];
			Resistances = new();
			Weaknesses = new();

			int resist = _rawBytes[0x06] * 0x100 + _rawBytes[0x07];
			int weak = _rawBytes[0x0c];
			var elementTypeList = Enum.GetValues<ElementsType>().ToList();
			foreach (var element in elementTypeList)
			{
				if ((resist & (int)element) > 0)
				{
					Resistances.Add(element);
				}
			}

			foreach (var element in elementTypeList)
			{
				if ((weak & ((int)element / 0x100)) > 0)
				{
					Weaknesses.Add(element);
				}
			}
		}
		public void Write(FFMQRom rom)
		{
			_rawBytes[0] = (byte)(HP % 0x100);
			_rawBytes[1] = (byte)(HP / 0x100);
			_rawBytes[2] = AttackPower;
			_rawBytes[3] = DamageReduction;
			_rawBytes[4] = Speed;
			_rawBytes[5] = MagicPower;
			_rawBytes[0x0a] = Accuracy;
			_rawBytes[0x0b] = Evasion;

			int tempresist = 0;
			int tempweak = 0;

			foreach (var resist in Resistances)
			{
				tempresist |= (int)resist;
			}

			foreach (var weak in Weaknesses)
			{
				tempweak |= ((int)weak / 0x100);
			}
			_rawBytes[0x06] = (byte)(tempresist / 0x100);
			_rawBytes[0x07] = (byte)(tempresist % 0x100);
			_rawBytes[0x0c] = (byte)(tempweak);

			rom.PutInBank(EnemiesStatsBank, EnemiesStatsAddress + (_Id * EnemiesStatsLength), _rawBytes);
		}
	}
	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLink : ICloneable
	{
		private Blob _rawBytes;

		private const int EnemiesAttackLinksAddress = 0xC6FF; // Bank 02
		private const int EnemiesAttackLinksBank = 0x02;
		private const int EnemiesAttackLinksLength = 0x09;

		public byte AttackPattern { get; set; }
		public byte[] Attacks { get; set; }
		public byte CastHeal { get; set; }
		public byte CastCure { get; set; }
		public int Id { get; set; }
		public int AttackCount => Attacks.Count(a => a != 0xFF);
		public List<int> NeedsSlotsFilled { get; }

		public EnemyAttackLink(int id, FFMQRom rom)
		{
			_rawBytes = rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress + (id * EnemiesAttackLinksLength), EnemiesAttackLinksLength);

			Id = id;
			AttackPattern = _rawBytes[0];
			Attacks = new byte[6];
			Attacks[0] = _rawBytes[1];
			Attacks[1] = _rawBytes[2];
			Attacks[2] = _rawBytes[3];
			Attacks[3] = _rawBytes[4];
			Attacks[4] = _rawBytes[5];
			Attacks[5] = _rawBytes[6];
			CastHeal = _rawBytes[7];
			CastCure = _rawBytes[8];
			NeedsSlotsFilled = new();
        }

		private EnemyAttackLink(int id, byte attackPattern, byte[] attacks, byte castHeal, byte castCure)
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

		public void Write(FFMQRom rom)
		{
			_rawBytes[0] = AttackPattern;
			_rawBytes[1] = Attacks[0];
			_rawBytes[2] = Attacks[1];
			_rawBytes[3] = Attacks[2];
			_rawBytes[4] = Attacks[3];
			_rawBytes[5] = Attacks[4];
			_rawBytes[6] = Attacks[5];
			_rawBytes[7] = CastHeal;
			_rawBytes[8] = CastCure;
			rom.PutInBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress + (Id * EnemiesAttackLinksLength), _rawBytes);
		}
	}
	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLinks
	{
		private List<EnemyAttackLink> _EnemyAttackLinks;
		private Blob _darkKingAttackLinkBytes;
		private byte iceGolemDesperateAttack;

		private List<int> Mobs;
		private List<int> Bosses;
		private List<int> DarkCastleBosses;
		private List<int> DarkKing;

        private const int EnemiesAttackLinksQty = 0x53;

        // Dark King Attack Links, separate from EnemiesAttacks
        // Dark King has its own byte range on top of attack links ids 79 through 82
        private const int DarkKingAttackLinkAddress = 0xD09E; // Bank 02
		private const int DarkKingAttackLinkBank = 0x02;
		private const int DarkKingAttackLinkQty = 0x0C;

		private const int IceGolemDesperateAttackBank = 0x02;
		private const int IceGolemDesperateAttackOffset = 0xAAC9;

        public EnemyAttackLinks(FFMQRom rom)
		{
			_EnemyAttackLinks = new List<EnemyAttackLink>();

			for (int i = 0; i < EnemiesAttackLinksQty; i++)
			{
				_EnemyAttackLinks.Add(new EnemyAttackLink(i, rom));
			}

			iceGolemDesperateAttack = rom.GetFromBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, 1)[0];
			_darkKingAttackLinkBytes = rom.GetFromBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, DarkKingAttackLinkQty);

            Mobs = Enumerable.Range(0, 0x40).ToList();
            Bosses = Enumerable.Range(0x42, 8).ToList().Concat(new List<int>() { 0x4A, 0x4B, 0x4C, 0x4E }).ToList();
			DarkCastleBosses = new List<int>() { 0x40, 0x41, 0x4D, 0x4F };
			DarkKing = new List<int> { 0x50, 0x51, 0x52 };

			// Wyvern, Hydra, DK2 and Dk3 need to have a specific slot filled to avoid softlock
			_EnemyAttackLinks[0x4C].NeedsSlotsFilled.Add(3);
            _EnemyAttackLinks[0x4D].NeedsSlotsFilled.Add(3);
            _EnemyAttackLinks[0x51].NeedsSlotsFilled.Add(3);
			_EnemyAttackLinks[0x52].NeedsSlotsFilled.Add(2);
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
			foreach (EnemyAttackLink e in _EnemyAttackLinks)
			{
				e.Write(rom);
			}

			rom.PutInBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, _EnemyAttackLinks.Where(l => l.Id == 0x51 || l.Id == 0x52).OrderBy(l => l.Id).SelectMany(l => l.Attacks.ToList()).ToArray());
            rom.PutInBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, new byte[] { iceGolemDesperateAttack });
        }
		public void ShuffleAttacks(EnemizerAttacks enemizerattacks, EnemizerGroups group, MT19337 rng)
		{
			switch (enemizerattacks) 
			{
				case EnemizerAttacks.Safe:
                    SafeRandom(group, rng);
					break;
				case EnemizerAttacks.Chaos:
					ChaosRandom(group, rng);
					break;
				case EnemizerAttacks.SelfDestruct:
					Selfdestruct(group);
					break;
				case EnemizerAttacks.SimpleShuffle:
					ScriptShuffle(group, rng);
					break;
				default:
					break;
			}
		}
		private List<int> GetValidEnemies(EnemizerGroups group)
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

			return validenemies;
        }
		private void ScriptShuffle(EnemizerGroups group, MT19337 rng)
		{
			var mobs = Mobs.ToList();
			Dictionary<int, int> switchList = new();

			while (mobs.Count > 1)
			{
				var tempmobA = rng.TakeFrom(mobs);
                var tempmobB = rng.TakeFrom(mobs);

				switchList.Add(tempmobA, tempmobB);
                switchList.Add(tempmobB, tempmobA);
            }

			if (group != EnemizerGroups.MobsOnly)
			{
				bool includedk = (group == EnemizerGroups.MobsBossesDK);
				
				var bosses = Bosses.Concat(DarkCastleBosses).ToList();
                List<int> tooShortBosses = new() { 0x42, 0x43 };

				if (includedk)
				{
					bosses = bosses.Concat(DarkKing).ToList();
                    tooShortBosses.Add(0x50);
				}

				bosses.RemoveAll(tooShortBosses.Contains);
				
				// Do hydra/wyvern first since it requires a minimum 4 attacks pattern
				var hydra = 0x4C;
				var wyvern = 0x4D;

				bosses.Remove(hydra);
                var hydracompanion = rng.TakeFrom(bosses);

				switchList.Add(hydra, hydracompanion);
				switchList.Add(hydracompanion, hydra);

				if (hydracompanion != wyvern)
				{
                    bosses.Remove(wyvern);
                    var wyverncompanion = rng.TakeFrom(bosses);

					switchList.Add(wyvern, wyverncompanion);
					switchList.Add(wyverncompanion, wyvern);
				}

				bosses.AddRange(tooShortBosses);

				while (bosses.Count > 1)
				{
					var tempbossA = rng.TakeFrom(bosses);
					var tempbossB = rng.TakeFrom(bosses);

					switchList.Add(tempbossA, tempbossB);
					switchList.Add(tempbossB, tempbossA);
				}
			}

			var pazuzuScript = _EnemyAttackLinks.Find(e => e.Id == 0x4F);
			pazuzuScript.Attacks[0x05] = 0xDA;

			foreach (var link in _EnemyAttackLinks)
			{
				if (switchList.TryGetValue(link.Id, out var newid))
				{
                    link.Id = newid;
                }
			}
		}
		private void ChaosRandom(EnemizerGroups group, MT19337 rng)
		{
            var possibleAttacks = new List<byte>();
            for (byte i = 0x40; i <= 0xDB; i++)
            {
                possibleAttacks.Add(i);
            }

			var validenemies = GetValidEnemies(group);

            foreach (var link in validenemies)
			{
				var ea = _EnemyAttackLinks[link];
				
				uint noOfAttacks = (rng.Next() % 5) + 1;

				for(uint i = 0; i < 6; i++)
				{
					ea.Attacks[i] = 0xFF;
				}

				for(uint i = 0; i < noOfAttacks; i++)
				{
					ea.Attacks[i] = possibleAttacks[(int)(rng.Next() % possibleAttacks.Count)]; 
				}

				// Some values of AttackPattern (e.g. 0x0B) result in the third (or other) attack slot being used
				// regardless of it being 0xFF (which is an ignored slot for most other AttackPattern values)
				if(noOfAttacks <= 3)
				{
					ea.AttackPattern = 0x01;
				}

				// Similarly, most AttackPattern values do not use attack slots 5 and 6, but 0x0D and 0x0C do.
				if(noOfAttacks >= 4)
				{
					ea.AttackPattern = 0x0D;
				}

				// Some enemies require certain slots to be filled, or the game locks up
				foreach(var slot in ea.NeedsSlotsFilled)
				{
					if(ea.Attacks[slot] == 0xFF)
					{
						ea.Attacks[slot] = possibleAttacks[(int)(rng.Next() % possibleAttacks.Count)];
					}
				}
			}

			if (group != EnemizerGroups.MobsOnly)
			{
				var icegolemattacks = possibleAttacks
					.Except(new List<byte> { 0x49, 0x4A, 0xC1, 0xC2 })
					.Except(Enumerable.Range(0xC8, 20).Select(x => (byte)x))
					.ToList();

                iceGolemDesperateAttack = rng.PickFrom(icegolemattacks);
            }
		}
		private void SafeRandom(EnemizerGroups group, MT19337 rng)
		{
            var validenemies = GetValidEnemies(group);
            var validattacks = _EnemyAttackLinks.Where(l => Mobs.Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList();
			List<byte> dkattacks = new();


            if (group != EnemizerGroups.MobsOnly)
			{
                validattacks.AddRange(_EnemyAttackLinks.Where(l => Bosses.Concat(DarkCastleBosses).Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList());
            }

			if (group == EnemizerGroups.MobsBossesDK)
			{
                dkattacks.AddRange(_EnemyAttackLinks.Where(l => DarkKing.Contains(l.Id)).SelectMany(l => l.Attacks).Distinct().ToList());
            }

			List<byte> invalidattacks = new() { 0x49, 0x4A, 0xC1, 0xC2, 0xC8, 0xC9, 0xFF };
			validattacks.RemoveAll(invalidattacks.Contains);
            validattacks = validattacks.Distinct().ToList();

			foreach (var link in validenemies)
			{
				List<byte> attackpool = validattacks;

                if (DarkCastleBosses.Concat(DarkKing).Contains(link))
				{
					attackpool.AddRange(dkattacks);
                }
				
				List<byte> newattacks = new();

				for (int i = 0; i < _EnemyAttackLinks[link].AttackCount; i++)
				{
					newattacks.Add(rng.PickFrom(validattacks));
                }

				newattacks.Sort();
                while (newattacks.Count < 6)
                {
                    newattacks.Add(0xFF);
                }

				_EnemyAttackLinks[link].Attacks = newattacks.ToArray();
            }

			iceGolemDesperateAttack = rng.PickFrom(validattacks);
        }
		private void Selfdestruct(EnemizerGroups group)
		{
			var validenemies = GetValidEnemies(group);

            foreach (var link in validenemies)
            {
                var ea = _EnemyAttackLinks[link];

                ea.AttackPattern = 0x01;
                ea.Attacks[0] = 0xC1;
                ea.Attacks[1] = 0xFF;
                ea.Attacks[2] = 0xFF;
                ea.Attacks[3] = 0xFF;
                ea.Attacks[4] = 0xFF;
                ea.Attacks[5] = 0xFF;

                // Some enemies require certain slots to be filled, or the game locks up
                foreach (var slot in ea.NeedsSlotsFilled)
                {
                    ea.Attacks[slot] = 0xC1;
                }
            }
        }
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
				_EnemyAttackLinks[enemy].Attacks = attackList.Select(x => (byte)x).ToArray();
				_EnemyAttackLinks[enemy].CastHeal = (byte)(healCaster ? healSpell : 0xFF);
				_EnemyAttackLinks[enemy].CastCure = (byte)(cureCaster ? cureSpell : 0xFF);
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
					_EnemyAttackLinks[boss].Attacks = attackList.Select(x => (byte)x).ToList().ToArray();
				}
				else
				{
					// special check hydras as their scripts is splitted
					var targetHydra = (boss == 0xFE ? 0x4C : 0x4D);

					_EnemyAttackLinks[targetHydra].Attacks[3] = (byte)attackList[0];
					_EnemyAttackLinks[targetHydra].Attacks[4] = (byte)attackList[1];
					_EnemyAttackLinks[targetHydra].Attacks[5] = (byte)attackList[2];
				}
			}


			var dkPhase2Attacks = _EnemyAttackLinks[0x51].Attacks.ToList();
			var dkPhase3Attacks = _EnemyAttackLinks[0x52].Attacks.ToList();

			_darkKingAttackLinkBytes = dkPhase2Attacks.Concat(dkPhase3Attacks).ToArray();
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
