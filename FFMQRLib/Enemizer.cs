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

	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLink : ICloneable
	{
		//private Blob _rawBytes;

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

		public void Write(FFMQRom rom)
		{
			/*
			_rawBytes[0] = AttackPattern;
			_rawBytes[1] = (byte)Attacks[0];
			_rawBytes[2] = (byte)Attacks[1];
			_rawBytes[3] = (byte)Attacks[2];
			_rawBytes[4] = (byte)Attacks[3];
			_rawBytes[5] = (byte)Attacks[4];
			_rawBytes[6] = (byte)Attacks[5];
			_rawBytes[7] = CastHeal;
			_rawBytes[8] = CastCure;
			rom.PutInBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress + (Id * EnemiesAttackLinksLength), _rawBytes);*/
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

		public EnemyAttackLinks(FFMQRom rom)
		{

			_EnemyAttackLinks = rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress, EnemiesAttackLinksQty * EnemiesAttackLinksLength).ToBytes().Chunk(EnemiesAttackLinksLength).Select((l, i) => new EnemyAttackLink(i, l)).ToList();

			/*
			_EnemyAttackLinks = new List<EnemyAttackLink>();

			for (int i = 0; i < EnemiesAttackLinksQty; i++)
			{
				_EnemyAttackLinks.Add(new EnemyAttackLink(i, rom));
			}*/

			iceGolemDesperateAttack = (EnemyAttackIds)rom.GetFromBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, 1)[0];
			_darkKingAttackLinkBytes = rom.GetFromBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, DarkKingAttackLinkQty);

			Mobs = Enumerable.Range((int)EnemyIds.Brownie, 0x40).Select(e => (EnemyIds)e).ToList();
			Bosses = Enumerable.Range((int)EnemyIds.Behemoth, 8).Select(e => (EnemyIds)e).ToList().Concat(new List<EnemyIds>() { EnemyIds.FlamerusRex, EnemyIds.IceGolem, EnemyIds.DualheadHydra, EnemyIds.Pazuzu }).ToList();
			DarkCastleBosses = new() { EnemyIds.SkullrusRex, EnemyIds.StoneGolem, EnemyIds.TwinheadWyvern, EnemyIds.Zuh };
			DarkKing = new() { EnemyIds.DarkKing, EnemyIds.DarkKingWeapons, EnemyIds.DarkKingSpider };

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
			/*
			foreach (EnemyAttackLink e in _EnemyAttackLinks)
			{
				e.Write(rom);
			}*/

			rom.PutInBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, _EnemyAttackLinks.Where(l => l.Id == EnemyIds.DarkKingWeapons || l.Id == EnemyIds.DarkKingSpider).OrderBy(l => l.Id).SelectMany(l => l.GetAttackBytes()).ToArray());
			rom.PutInBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, new byte[] { (byte)iceGolemDesperateAttack });
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
		private List<EnemyIds> GetValidEnemies(EnemizerGroups group)
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
			Dictionary<EnemyIds, EnemyIds> switchList = new();

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
					//tooShortBosses.Add(0x50);
				}

				//bosses.RemoveAll(tooShortBosses.Contains);
				
				// Do hydra/wyvern first since it requires a minimum 4 attacks pattern
				/*var hydra = 0x4C;
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
				*/
				while (bosses.Count > 1)
				{
					var tempbossA = rng.TakeFrom(bosses);
					var tempbossB = rng.TakeFrom(bosses);

					switchList.Add(tempbossA, tempbossB);
					switchList.Add(tempbossB, tempbossA);
				}
			}

			// Remove Strong Psychshield since it might get used
			var zuhScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Zuh);
			zuhScript.Attacks[0x05] = EnemyAttackIds.HurricanePlus;

			// Extend Behemoth, Minotaur and DK1 attack list to avoid softlock
			var behemothScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Behemoth);
			behemothScript.Attacks = Enumerable.Repeat(EnemyAttackIds.Horn, 6).ToList();

			var minotaurScript = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.Minotaur);
			minotaurScript.Attacks = new() { EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream, EnemyAttackIds.Axe, EnemyAttackIds.Roundhouse, EnemyAttackIds.Scream };

			var dk1Script = _EnemyAttackLinks.Find(e => e.Id == EnemyIds.DarkKing);
			minotaurScript.Attacks[0x04] = rng.PickFrom(new List<EnemyAttackIds>() { EnemyAttackIds.DarkCane, EnemyAttackIds.IronNail, EnemyAttackIds.Spark });

			foreach (var link in _EnemyAttackLinks)
			{
				if (switchList.TryGetValue(link.Id, out var newid))
				{
					link.Id = newid;
				}
			}

			_EnemyAttackLinks = _EnemyAttackLinks.OrderBy(l => l.Id).ToList();
		}
		private void ChaosRandom(EnemizerGroups group, MT19337 rng)
		{
			var possibleAttacks = new List<EnemyAttackIds>();
			bool meandDK = rng.Between(1, 6) == 1;

			for (var i = EnemyAttackIds.Sword; i <= EnemyAttackIds.HeatwaveUnused; i++)
			{
				possibleAttacks.Add(i);
			}

			var validenemies = GetValidEnemies(group);

			foreach (var link in validenemies)
			{
				var ea = _EnemyAttackLinks[(int)link];
				
				int noOfAttacks = (int)rng.Between(2, 6);

				for(int i = 0; i < 6; i++)
				{
					ea.Attacks[i] = EnemyAttackIds.Nothing;
				}

				for(int i = 0; i < noOfAttacks; i++)
				{
					if (DarkKing.Contains(link) && !meandDK)
					{
						ea.Attacks[i] = rng.PickFrom(possibleAttacks.Except(new List<EnemyAttackIds> { EnemyAttackIds.CureSelf, EnemyAttackIds.HealSelf, EnemyAttackIds.Selfdestruct, EnemyAttackIds.Multiply }).ToList());
					}
					else
					{
						ea.Attacks[i] = rng.PickFrom(possibleAttacks);
					}
					
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
					ea.AttackPattern = 0x04;
				}

				// Some enemies require certain slots to be filled, or the game locks up
				foreach(var slot in ea.NeedsSlotsFilled)
				{
					if(ea.Attacks[slot] == EnemyAttackIds.Nothing)
					{
						ea.Attacks[slot] = rng.PickFrom(possibleAttacks);
					}
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
		private void SafeRandom(EnemizerGroups group, MT19337 rng)
		{
			var validenemies = GetValidEnemies(group);
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

			foreach (var link in validenemies)
			{
				List<EnemyAttackIds> attackpool = new(validattacks);

				if (DarkCastleBosses.Concat(DarkKing).Contains(link))
				{
					attackpool.AddRange(dkattacks);
				}
				
				List<EnemyAttackIds> newattacks = new();

				for (int i = 0; i < _EnemyAttackLinks[(int)link].AttackCount; i++)
				{
					newattacks.Add(rng.PickFrom(attackpool));
				}

				newattacks.Sort();

				// Split hydra's and wyvern attacks to make them less backended
				if (link == EnemyIds.TwinheadWyvern || link == EnemyIds.DualheadHydra)
				{
					newattacks = newattacks.Where((a, i) => i % 2 == 0).Concat(newattacks.Where((a, i) => i % 2 == 1)).ToList();
				}

				while (newattacks.Count < 6)
				{
					newattacks.Add(EnemyAttackIds.Nothing);
				}

				_EnemyAttackLinks[(int)link].Attacks = newattacks.ToList();
			}

			iceGolemDesperateAttack = rng.PickFrom(validattacks.Where(a => a < EnemyAttackIds.Selfdestruct).ToList());
		}
		public void BalancedTest(EnemizerGroups group, MT19337 rng)
		{
			var validenemies = GetValidEnemies(group);
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

			foreach (var link in validenemies)
			{
				List<EnemyAttackIds> attackpool = new(validattacks);

				if (DarkCastleBosses.Concat(DarkKing).Contains(link))
				{
					attackpool.AddRange(dkattacks);
				}

				List<EnemyAttackIds> newattacks = new();

				bool badAttackPicked = false;
				int badAttackPickedCount = 0;

				for (int i = 0; i < _EnemyAttackLinks[(int)link].AttackCount; i++)
				{
					var newAttack = badAttackPicked ? rng.PickFrom(safeattacks.Except(newattacks).ToList()) : rng.PickFrom(badattacks.Except(newattacks).ToList());
					if (!badAttackPicked && badattacks.Contains(newAttack))
					{
						badAttackPickedCount++;
						if (badAttackPickedCount >= 2)
						{
							badAttackPicked = true;
						}
						
					}

					newattacks.Add(newAttack);
				}

				newattacks.Shuffle(rng);

				//newattacks.Sort();

				// Split hydra's and wyvern attacks to make them less backended
				if (link == EnemyIds.TwinheadWyvern || link == EnemyIds.DualheadHydra)
				{
					newattacks = newattacks.Where((a, i) => i % 2 == 0).Concat(newattacks.Where((a, i) => i % 2 == 1)).ToList();
				}

				while (newattacks.Count < 6)
				{
					newattacks.Add(EnemyAttackIds.Nothing);
				}

				_EnemyAttackLinks[(int)link].Attacks = newattacks.ToList();
			}

			iceGolemDesperateAttack = rng.PickFrom(validattacks.Where(a => a < EnemyAttackIds.Selfdestruct).ToList());
		}
		private void Selfdestruct(EnemizerGroups group)
		{
			var validenemies = GetValidEnemies(group);

			foreach (var link in validenemies)
			{
				var ea = _EnemyAttackLinks[(int)link];

				ea.AttackPattern = 0x01;
				ea.Attacks = Enumerable.Repeat(EnemyAttackIds.Selfdestruct, 6).ToList();
			}
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
