using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;
using System.Buffers.Binary;
using System.Reflection.Emit;

namespace FFMQLib
{
	public enum LevelingCurve : int
	{
		[Description("0.5x")]
		Half = 0,
		[Description("1x")]
		Normal,
		[Description("1.5x")]
		OneAndHalf,
		[Description("2x")]
		Double,
		[Description("2.5x")]
		DoubleAndHalf,
		[Description("3x")]
		Triple,
		[Description("4x")]
		Quadruple,
	}

	public enum LevelingType : int
	{
		[Description("Quests")]
		Quests = 0,
		[Description("Benjamin Level")]
		BenPlus0,
		[Description("Benjamin Level + 5")]
		BenPlus5,
		[Description("Benjamin Level + 10")]
		BenPlus10,
	}
	public enum SpellbookType : int
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Standard Extended")]
		StandardExtended,
		[Description("Random Balanced")]
		RandomBalanced,
		[Description("Random Chaos")]
		RandomChaos,
	}
	public partial class FFMQRom : SnesRom
	{

		public void SetLevelingCurve(LevelingCurve levelingcurve)
		{
			byte xpconst1 = 0x3d;
			byte xpconst2 = 0x0d;

			switch (levelingcurve)
			{
				case LevelingCurve.Half:
					xpconst1 = 0x6E;
					xpconst2 = 0x18;
					break;
				case LevelingCurve.Normal:
					return;
				case LevelingCurve.OneAndHalf:
					xpconst1 = 0x26;
					xpconst2 = 0x08;
					break;
				case LevelingCurve.Double:
					xpconst1 = 0x1C;
					xpconst2 = 0x06;
					break;
				case LevelingCurve.DoubleAndHalf:
					xpconst1 = 0x18;
					xpconst2 = 0x05;
					break;
				case LevelingCurve.Triple:
					xpconst1 = 0x14;
					xpconst2 = 0x04;
					break;
				case LevelingCurve.Quadruple:
					xpconst1 = 0x0E;
					xpconst2 = 0x03;
					break;
			}

			// for level up check after battle
			PutInBank(0x03, 0xAE1F, new byte[] { xpconst1 });
			PutInBank(0x03, 0xAE2D, new byte[] { xpconst2 });

			// for next level in status screen
			PutInBank(0x03, 0x9C11, new byte[] { xpconst1 });
			PutInBank(0x03, 0x9C1F, new byte[] { xpconst2 });
		}

		public void CompanionRoutines()
		{
			// Check char opcode
			PutInBank(0x11, 0x8400, Blob.FromHex("08E230A717E617AE9010E0FFF00DCD920EF00E186904CD920EF00628A71785176B28E617E6176B"));
			PutInBank(0x00, 0xff00, Blob.FromHex("2200841160"));

			// Switch companion code
			var companionSwitch = new ScriptBuilder(new List<string>{
				$"050f{(int)CompanionsId.Kaeli:X2}[06]",
				$"2e{(int)NewGameFlagsList.KaeliCured:X2}[04]",                  // 01 is Elixir Quest done?
				$"2e{(int)NewGameFlagsList.ShowSickKaeli:X2}[05]",               // 02 No, is Kaeli Sick?
				$"23{(int)NewGameFlagsList.ShowForestaKaeli:X2}00",              // 03 No, show Foresta
				$"23{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",                 // 04 then available in Windia
				"00",															 // 05
				$"050f{(int)CompanionsId.Tristam:X2}[11]",
				$"23{(int)NewGameFlagsList.ShowFireburgTristam:X2}",	         // 07 Tristam is at Fireburg
				$"2e{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[10]", // 08 Is bone quest done?
				$"23{(int)NewGameFlagsList.ShowSandTempleTristam:X2}",           // 09 No, show at Sand Temple
				"00",												             // 10
				$"050f{(int)CompanionsId.Phoebe:X2}[16]",
				$"2e{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[14]",       // 12 is WintryCave Quest done?
				$"23{(int)NewGameFlagsList.ShowLibraTemplePhoebe:X2}00",         // 13 No, show in Libra Temple
				$"23{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}",		         // 14 Yes, show in Windia
				"00",												             // 15
				$"050f{(int)CompanionsId.Reuben:X2}[18]",
				$"23{(int)NewGameFlagsList.ShowFireburgReuben:X2}00",            // 17 Reuben is always in Fireburg
				"00",
				});

			companionSwitch.Update(0xFF80);
			companionSwitch.Write(this);

			PutInBank(0x00, 0x9e8c, Blob.FromHex("00ff"));
		}
	}
	public class LevelThreshold
	{ 
		public int Level { get; set; }
		public int OffenseSpellsCount => Spells.Count(s => s >= SpellFlags.QuakeBook);
		public List<ArmorFlags> Armors { get; set; }
		public List<SpellFlags> Spells { get; set; }
		public LevelThreshold(int level, List<ArmorFlags> armors, List<SpellFlags> spells)
		{
			Level = level;
			Armors = armors;
			Spells = spells;
		}
		public byte[] ToArray()
		{
			GameFlags flagsmanager = new();
			int armorsflagsoffset = 8;
			int spellsflagsoffset = 8 * 4;

			byte[] thresholddata = new byte[7];
			thresholddata[0] = (byte)Level;
			
			foreach (var armor in Armors)
			{
				flagsmanager.CustomFlagToHex(thresholddata, ((int)armor + armorsflagsoffset), true);
			}

			foreach (var spell in Spells)
			{
				flagsmanager.CustomFlagToHex(thresholddata, ((int)spell + spellsflagsoffset), true);
			}

			thresholddata[6] = (byte)OffenseSpellsCount;


			return thresholddata;
		}
	}
	public class WeightedSpell
	{ 
		public SpellFlags Spell { get; set; }
		public (int min, int max) LevelRange { get; set; }
		public List<(CompanionsId companion, int weight)> Learners { get; set; }
		public WeightedSpell(SpellFlags spell, (int min, int max) range, List<(CompanionsId companion, int weight)> learners)
		{
			Spell = spell;
			LevelRange = range;
			Learners = learners;
		}
		public (CompanionsId companion, int level) GiveSpellRandom(List<CompanionsId> excluded, MT19337 rng)
		{
			var validcompanions = Learners.Where(l => !excluded.Contains(l.companion)).ToList();

			if (!validcompanions.Any())
			{
				return (CompanionsId.None, 0);
			}

			List<CompanionsId> weightedcompanionslist = new();
			foreach (var companion in validcompanions)
			{
				weightedcompanionslist.AddRange(Enumerable.Repeat(companion.companion, companion.weight).ToList());
			}

			return (rng.PickFrom(weightedcompanionslist), rng.Between(LevelRange.min, LevelRange.max));
		}
		public (CompanionsId companion, int level) GiveSpell(CompanionsId companion, MT19337 rng)
		{
			return (companion, rng.Between(LevelRange.min, LevelRange.max));
		}

	}

	public class Companion
	{ 
		public Items Weapon { get; set; }
		//public List<ArmorFlags> Armors { get; set; }
		//public List<SpellFlags> Spells { get; set; }
		public List<LevelThreshold> LoadOut { get; set; }
		public List<ArmorFlags> ArmorSet1 { get; set; }
		public List<ArmorFlags> ArmorSet2 { get; set; }
		public int HPBase { get; set; }
		public int StrBase { get; set; }
		public int StrMultiplier { get; set; }
		public int ConBase { get; set; }
		public int ConMultiplier { get; set; }
		public int MagBase { get; set; }
		public int MagMultiplier { get; set; }
		public int SpdBase { get; set; }
		public int SpdMultiplier { get; set; }
		public int WhiteMPBase { get; set; }
		public int BlackMPBase { get; set; }
		public int WizardMPBase { get; set; }
		private const int HPMultiplier = 40;

		public Companion()
		{
			LoadOut = new();
			ArmorSet1 = new();
			ArmorSet2 = new();
		}
		public byte[] ToArray()
		{
			List<byte> levelingdata = new();
			levelingdata.Add((byte)HPMultiplier);
			var hpbase = Blob.FromUShorts(new ushort[] { (ushort)HPBase });
			levelingdata.AddRange(hpbase.ToBytes());

			levelingdata.Add((byte)StrMultiplier);
			var strbase = Blob.FromUShorts(new ushort[] { (ushort)StrBase });
			levelingdata.AddRange(strbase.ToBytes());

			levelingdata.Add((byte)ConMultiplier);
			var conbase = Blob.FromUShorts(new ushort[] { (ushort)ConBase });
			levelingdata.AddRange(conbase.ToBytes());

			levelingdata.Add((byte)SpdMultiplier);
			var spdbase = Blob.FromUShorts(new ushort[] { (ushort)SpdBase });
			levelingdata.AddRange(spdbase.ToBytes());

			levelingdata.Add((byte)MagMultiplier);
			var magbase = Blob.FromUShorts(new ushort[] { (ushort)MagBase });
			levelingdata.AddRange(magbase.ToBytes());

			levelingdata.Add((byte)WhiteMPBase);
			levelingdata.Add((byte)BlackMPBase);
			levelingdata.Add((byte)WizardMPBase);


			foreach (var loadout in LoadOut.OrderByDescending(l => l.Level))
			{
				levelingdata.AddRange(loadout.ToArray());
			}

			if (levelingdata.Count > 0x80)
			{
				throw new Exception("Leveling Data is too long.");
			}

			return levelingdata.ToArray();
		}

		public byte[] GetLeveProgression()
		{
			var levellist = LoadOut.Select(l => (byte)l.Level).Order().ToList();

			byte trailing = levellist.Last();
			while (levellist.Count < 5)
			{
				levellist.Add(trailing);
			}

			return levellist.ToArray();
		}

		public byte[] GetWeaponData()
		{
			byte[] weaponarray = new byte[4];
			GameFlags flagsmanager = new();
			int weaponsflagsoffset = 8 * 2;

			List<Items> projectileweapons = new() { Items.Bomb, Items.JumboBomb, Items.MegaGrenade, Items.NinjaStar, Items.BowOfGrace };
			byte ammoqty = projectileweapons.Contains(Weapon) ? (byte)0x63 : (byte)0x00;

			weaponarray[0] = ammoqty;
			weaponarray[1] = (byte)Weapon;
			flagsmanager.CustomFlagToHex(weaponarray, ((int)Weapon - 0x20 + weaponsflagsoffset), true);

			return weaponarray;
		}
	}

	public class Companions
	{
		private List<Companion> companions;
		private LevelingType levelingtype;

		public Companion this[CompanionsId companion]
		{
			get => companions[((int)companion) - 1];
		}

		public Companions(LevelingType type)
		{
			levelingtype = type;
			companions = new();

			Companion kaeli = new Companion
			{
				Weapon = Items.GiantsAxe,
				HPBase = 40,
				StrBase = 3,
				StrMultiplier = 3,
				ConBase = 10,
				ConMultiplier = 2,
				MagBase = 0,
				MagMultiplier = 2,
				SpdBase = 15,
				SpdMultiplier = 2,
				WhiteMPBase = 4,
				BlackMPBase = 2,
				WizardMPBase = 0,
				ArmorSet1 = new() { ArmorFlags.MagicRing, ArmorFlags.ReplicaArmor },
				ArmorSet2 = new() { ArmorFlags.MagicRing, ArmorFlags.ReplicaArmor, ArmorFlags.SteelHelm },
				LoadOut = new()
				{
					new LevelThreshold(7, new() { ArmorFlags.MagicRing, ArmorFlags.ReplicaArmor }, new() { SpellFlags.LifeBook }),
					new LevelThreshold(31, new() { ArmorFlags.MagicRing, ArmorFlags.ReplicaArmor, ArmorFlags.SteelHelm }, new() { SpellFlags.LifeBook, SpellFlags.CureBook, SpellFlags.HealBook, SpellFlags.AeroBook }),
				}
			};

			Companion tristam = new Companion
			{
				Weapon = Items.NinjaStar,
				HPBase = 160,
				StrBase = 2,
				StrMultiplier = 3,
				ConBase = 8,
				ConMultiplier = 1,
				MagBase = 8,
				MagMultiplier = 1,
				SpdBase = 10,
				SpdMultiplier = 3,
				WhiteMPBase = 0,
				BlackMPBase = 0,
				WizardMPBase = 0,
				ArmorSet1 = new() { ArmorFlags.MoonHelm, ArmorFlags.BlackRobe },
				ArmorSet2 = new() { ArmorFlags.MoonHelm, ArmorFlags.BlackRobe, ArmorFlags.Charm },
				LoadOut = new()
				{
					new LevelThreshold(7, new() { ArmorFlags.MoonHelm, ArmorFlags.BlackRobe }, new() { SpellFlags.LifeBook }),
					new LevelThreshold(23, new() { ArmorFlags.MoonHelm, ArmorFlags.BlackRobe, ArmorFlags.Charm }, new() { SpellFlags.LifeBook }),
				}
			};

			Companion phoebe = new Companion
			{
				Weapon = Items.BowOfGrace,
				HPBase = 80,
				StrBase = 35,
				StrMultiplier = 1,
				ConBase = 5,
				ConMultiplier = 1,
				MagBase = 4,
				MagMultiplier = 3,
				SpdBase = 2,
				SpdMultiplier = 2,
				WhiteMPBase = 6,
				BlackMPBase = 3,
				WizardMPBase = 2,
				ArmorSet1 = new() { ArmorFlags.MysticRobes, ArmorFlags.MagicRing },
				ArmorSet2 = new() { ArmorFlags.MysticRobes, ArmorFlags.MagicRing, ArmorFlags.SteelHelm, ArmorFlags.EtherShield },
				LoadOut = new()
				{
					new LevelThreshold(15, new() { ArmorFlags.MysticRobes, ArmorFlags.MagicRing }, new() { SpellFlags.LifeBook, SpellFlags.CureBook, SpellFlags.HealBook, SpellFlags.FireBook, SpellFlags.ThunderSeal }),
					new LevelThreshold(34, new() { ArmorFlags.MysticRobes, ArmorFlags.MagicRing, ArmorFlags.SteelHelm, ArmorFlags.EtherShield }, new() { SpellFlags.LifeBook, SpellFlags.CureBook, SpellFlags.HealBook, SpellFlags.FireBook, SpellFlags.BlizzardBook, SpellFlags.ThunderSeal, SpellFlags.WhiteSeal }),
				}
			};

			Companion reuben = new Companion
			{
				Weapon = Items.MorningStar,
				HPBase = 80,
				StrBase = 38,
				StrMultiplier = 2,
				ConBase = 38,
				ConMultiplier = 1,
				MagBase = 10,
				MagMultiplier = 1,
				SpdBase = 38,
				SpdMultiplier = 1,
				WhiteMPBase = 0,
				BlackMPBase = 0,
				WizardMPBase = 0,
				ArmorSet1 = new() { ArmorFlags.FlameArmor, ArmorFlags.SteelHelm, ArmorFlags.Charm },
				ArmorSet2 = new() { ArmorFlags.FlameArmor, ArmorFlags.SteelHelm, ArmorFlags.Charm },
				LoadOut = new()
				{
					new LevelThreshold(23, new() { ArmorFlags.FlameArmor, ArmorFlags.SteelHelm, ArmorFlags.Charm }, new() { SpellFlags.LifeBook }),
					new LevelThreshold(31, new() { ArmorFlags.FlameArmor, ArmorFlags.SteelHelm, ArmorFlags.Charm }, new() { SpellFlags.LifeBook, SpellFlags.WhiteSeal }),
				}
			};

			companions = new() { kaeli, tristam, phoebe, reuben };
		}
		public void SetSpellbooks(SpellbookType spellbooktype, GameInfoScreen infoscreen, MT19337 rng)
		{
			List<(SpellFlags spell, List<(CompanionsId companion, int weight)>)> weightlist = new()
			{
				(SpellFlags.ExitBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 7), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 1),}),
				(SpellFlags.CureBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.HealBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 2), (CompanionsId.Phoebe, 3),}),
				(SpellFlags.QuakeBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 3), (CompanionsId.Phoebe, 2),}),
				(SpellFlags.BlizzardBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.FireBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 4), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.AeroBook, new() { (CompanionsId.Kaeli, 4), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.ThunderSeal, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.WhiteSeal, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 3), (CompanionsId.Phoebe, 3),}),
				(SpellFlags.MeteorSeal, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 3), (CompanionsId.Reuben, 2), (CompanionsId.Phoebe, 2),}),
				(SpellFlags.FlareSeal, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 4), (CompanionsId.Phoebe, 1),}),
			};

			//List<(SpellFlags spell, List<int> levelrange, CompanionsId companion)> spelllist = new();
			List<(CompanionsId companion, SpellFlags spell, int level)> spelllist = new();

			if (spellbooktype == SpellbookType.Standard || spellbooktype == SpellbookType.StandardExtended)
			{
				spelllist = new()
				{
					(CompanionsId.Kaeli, SpellFlags.CureBook, rng.Between(8, 15)),
					(CompanionsId.Kaeli, SpellFlags.HealBook, rng.Between(8, 15)),
					(CompanionsId.Kaeli, SpellFlags.AeroBook, rng.Between(8, 15)),
					(CompanionsId.Reuben, SpellFlags.WhiteSeal, rng.Between(24, 31)),
					(CompanionsId.Phoebe, SpellFlags.CureBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.HealBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.FireBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.ThunderSeal, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.BlizzardBook, rng.Between(16, 31)),
					(CompanionsId.Phoebe, SpellFlags.WhiteSeal, rng.Between(16, 31)),
				};

				if (spellbooktype == SpellbookType.StandardExtended)
				{
					spelllist.Add((CompanionsId.Tristam, SpellFlags.ExitBook, rng.Between(16, 23)));
					spelllist.Add((CompanionsId.Tristam, SpellFlags.QuakeBook, rng.Between(8, 15)));
					spelllist.Add((CompanionsId.Reuben, SpellFlags.BlizzardBook, rng.Between(8, 15)));
					/*
					if (levelingtype == LevelingType.Quests)
					{
						spelllist.Add((CompanionsId.Tristam, SpellFlags.FlareSeal, rng.Between(35, 41)));
						spelllist.Add((CompanionsId.Kaeli, SpellFlags.MeteorSeal, rng.Between(35, 41)));
					}*/
				}
			}
			else if(spellbooktype == SpellbookType.RandomBalanced || spellbooktype == SpellbookType.RandomChaos)
			{
				List<List<int>> levelrange = new()
				{
					Enumerable.Range(1,15).ToList(),
					Enumerable.Range(5,15).ToList(),
					Enumerable.Range(15,20).ToList(),
					Enumerable.Range(1,35).ToList(),
				};

				List<List<SpellFlags>> spells = new()
				{
					new() { SpellFlags.CureBook, SpellFlags.HealBook },
					new() { SpellFlags.QuakeBook, SpellFlags.BlizzardBook, SpellFlags.FireBook, SpellFlags.AeroBook, SpellFlags.ThunderSeal },
					new() { SpellFlags.WhiteSeal, SpellFlags.MeteorSeal, SpellFlags.FlareSeal },
					new() { SpellFlags.ExitBook },
				};

				List<int> counts = new()
				{
					rng.Between(2, 6),
					rng.Between(3, 8),
					rng.Between(1, 4),
					rng.Between(0, 1)
				};
				
				List<(SpellFlags spell, List<CompanionsId> weight)> collapsedlist = weightlist.Select(x => (x.spell, x.Item2.SelectMany(c => Enumerable.Repeat(c.companion, c.weight)).ToList())).ToList();

				if (spellbooktype == SpellbookType.RandomChaos)
				{
					levelrange = new() { Enumerable.Range(1, 41).Concat(Enumerable.Range(1, 35)).ToList() };
					spells = new() { spells.SelectMany(s => s).ToList() };
					counts = new() { rng.Between(6, 20)};
					collapsedlist = weightlist.Select(x => (x.spell, x.Item2.Select(c => c.companion).ToList())).ToList();
				}

				for (int category = 0; category < counts.Count; category++)
				{
					for (int i = 0; i < counts[category]; i++)
					{
						var currentspell = rng.PickFrom(spells[category]);
						var spellweight = collapsedlist.Find(s => s.spell == currentspell);
						var learner = rng.PickFrom(spellweight.weight);
						spellweight.weight.RemoveAll(c => c == learner);
						if (!spellweight.weight.Any())
						{
							spells[category].Remove(currentspell);
						}

						spelllist.Add((learner, currentspell, rng.PickFrom(levelrange[category])));
					}
				}
			}

			// update to appropriate level
			if (levelingtype == LevelingType.Quests)
			{
				List<(CompanionsId companion, List<int> levels)> levelsbycompanion = new()
				{
					(CompanionsId.Kaeli, new() { 7, 31 } ),
					(CompanionsId.Tristam, new() { 7, 23 } ),
					(CompanionsId.Reuben, new() { 23, 31 } ),
					(CompanionsId.Phoebe, new() { 15, 34 } ),
				};

				for (int i = 0; i < spelllist.Count; i++)
				{ 
					var selectedlevels = levelsbycompanion.Find(x => x.companion == spelllist[i].companion).levels;
					bool nolevelfound = true;
					foreach(var level in selectedlevels)
					{
						if (level >= spelllist[i].level)
						{
							spelllist[i] = (spelllist[i].companion, spelllist[i].spell, level);
							nolevelfound = false;
							break;
						}
					}

					if (nolevelfound)
					{
						spelllist[i] = (spelllist[i].companion, spelllist[i].spell, selectedlevels.Last());
					}
				}
			}

			// Spell are distributed, so update InfoScreen
			infoscreen.SpellLearning = spelllist.GroupBy(s => s.companion).Select(g => (g.Key, g.Select(s => (s.level, s.spell)).ToList())).ToList();
			
			// Regroup spells by level and create new LoadOut
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };
			
			foreach (var companion in companions)
			{
				spelllist.Add((companion, SpellFlags.LifeBook, 1));

				var companionspells = spelllist.Where(s => s.companion == companion)
										.OrderBy(s => s.level)
										.GroupBy(s => s.level)
										.Select(g => (g.Key, g.Select(s => s.spell).ToList()))
										.ToList();
				
				List<LevelThreshold> newloadout = new();
				List<SpellFlags> cumulativespells = new();
				bool level23set = false;

				foreach (var spellsgroup in companionspells)
				{
					if (spellsgroup.Key == 23)
					{
						level23set = true;
					}
					else if (spellsgroup.Key > 23 && !level23set)
					{
						newloadout.Add(new(23, this[companion].ArmorSet2, new(cumulativespells)));
						level23set = true;
					}

					cumulativespells.AddRange(spellsgroup.Item2);
					newloadout.Add(new(spellsgroup.Key, spellsgroup.Key < 23 ? this[companion].ArmorSet1 : this[companion].ArmorSet2, new(cumulativespells)));
				}

				this[companion].LoadOut = newloadout;
			}
		}
		public void Write(FFMQRom rom)
		{
			LevelingRoutine(rom);

			// Leveling Routine Hooks
			// Companion Join
			rom.PutInBank(0x00, 0xA26B, Blob.FromHex("2200a310eaeaeaea"));
			rom.PutInBank(0x10, 0xA300, Blob.FromHex("d006a980001ca0102000a02080a26b"));

			if (levelingtype != LevelingType.Quests)
			{
				// Ben Levelup
				rom.PutInBank(0x02, 0x8830, Blob.FromHex("2220a310"));
				rom.PutInBank(0x10, 0xA320, Blob.FromHex("08e230ad9010c9fff0062000a020a0a222029b00286b"));
			}

			// Quest fullfilled


			// lut_CompanionLevel
			List<byte> levelsset = new()
			{
				07, 31, 31, 31, 31,
				07, 23, 23, 23, 23,
				15, 34, 34, 34, 34,
				23, 31, 31, 31, 31,
			};
			/*
			if (levelingtype == LevelingType.Quests)
			{ 
			
			
			}*/
			rom.PutInBank(0x10, 0xA400, levelsset.ToArray());

			// lut_CompanionStats + Weapon Data
			int companionleveldataoffset = 0xA500;
			int companiondataoffset = 0xD100;

			foreach (var companion in companions)
			{
				rom.PutInBank(0x10, companionleveldataoffset, companion.ToArray());
				companionleveldataoffset += 0x80;
				rom.PutInBank(0x0C, companiondataoffset + 0x30, companion.GetWeaponData());
				companiondataoffset += 0x50;
			}
		}

		private void LevelingRoutine(FFMQRom rom)
		{
			string levelingroutine = "B0A0";
			string benlevelbonus = "00";

			if (levelingtype != LevelingType.Quests)
			{
				levelingroutine = "A0A0";

				if (levelingtype == LevelingType.BenPlus5)
				{
					benlevelbonus = "05";
				}
				else if (levelingtype == LevelingType.BenPlus10)
				{
					benlevelbonus = "0A";
				}
			}

			// DoLeveling
			rom.PutInBank(0x10, 0xA000, Blob.FromHex($"08c2302080a020{levelingroutine}2000a12000a22860"));
			
			// GetCompanionOffset
			rom.PutInBank(0x10, 0xA080, Blob.FromHex("08e230a900ebad920e38e901c2300a0a0a0a0a0a0aaa2860"));

			// SetLevel_Ben

			rom.PutInBank(0x10, 0xA0A0, Blob.FromHex($"08e220ad10101869{benlevelbonus}8d90102860"));

			// SetLevel_Quest; flush to ComputeStats, maybe move one or the other
			rom.PutInBank(0x10, 0xA0B0, Blob.FromHex("08e220c210daa900ebad920e38e9010a0aa8186d920e38e901aa981869a922769700f001e818690122769700f001e818690122769700f001e818690122769700f001e8186901bf00a4108d9010fa2860"));

			// ComputeStats
			rom.PutInBank(0x010, 0xA100, Blob.FromHex("08c230ad90108d02422080a18d961020a0a1e2208dcc108da61020a0a1e2208dcd108da71020a0a1e2208dce108da81020a0a1e2208dcf108da910ad90104820b0a18d9b10684a4820b0a18d9c10684a20b0a18d9d102860"));

			// GetStats, GetStats99, GetMP
			rom.PutInBank(0x10, 0xA180, Blob.FromHex("08e220c210bf00a5108d0342c230ebebe818ad16427f00a510e8e82860000000c2302080a1c963009003a96300600000187f00a510e8c9639002a96360"));

			// SetEquipSpells, LoadEquipSpells
			rom.PutInBank(0x10, 0xA200, Blob.FromHex("08e220c210ad9010df00a510b009e8e8e8e8e8e8e880eec230e88a186900a5aaa90400a0b510540010282030a260"));

			// HealUp
			rom.PutInBank(0x10, 0xA280, Blob.FromHex("08e220c210ae96108e9410ae9b108e9810ad9d108d9a109ca1102860"));

			// HpUp
			rom.PutInBank(0x10, 0xA2A0, Blob.FromHex("08e230ad94101869288d94102860"));

			// ComputeAI
			rom.PutInBank(0x10, 0xA230, Blob.FromHex("08c230dabf0000102903000aaabf30a4108dc210fae220c210a900ebbf000010aabf20a410a8a904aa0bf4b8102b225a9700f003988002a9009dc010e88ae00c0090e72b2860"));
			rom.PutInBank(0x10, 0xA420, Blob.FromHex("006432221914110f0d")); // lut_OddsValues
			rom.PutInBank(0x10, 0xA430, Blob.FromHex("64004123283c283c")); // lut_AiAttacks
		}
	}
}
