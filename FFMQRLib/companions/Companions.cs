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
	public enum LevelingType : int
	{
		[Description("Quests")]
		Quests = 0,
		[Description("Quests Extended")]
		QuestsExtended,
		[Description("Save the Crystals (Individual)")]
		SaveCrystalsIndividual,
		[Description("Save the Crystals (All)")]
		SaveCrystalsAll,
		[Description("Benjamin Level")]
		BenPlus0,
		[Description("Benjamin Level + 5")]
		BenPlus5,
		[Description("Benjamin Level + 10")]
		BenPlus10,
	}
	public enum StartingCompanionType : int
	{
		[Description("None")]
		None = 0,
		[Description("Kaeli")]
		Kaeli,
		[Description("Tristam")]
		Tristam,
		[Description("Phoebe")]
		Phoebe,
		[Description("Reuben")]
		Reuben,
		[Description("Random")]
		Random,
		[Description("Random+None")]
		RandomPlusNone,
	}
	public enum AvailableCompanionsType : int
	{
		[Description("4")]
		Four = 0,
		[Description("3")]
		Three,
		[Description("2")]
		Two,
		[Description("1")]
		One,
		[Description("0")]
		Zero,
		[Description("Random 1-4")]
		Random14,
		[Description("Random 0-4")]
		Random04,
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
	public class Companion
	{ 
		public Items Weapon { get; set; }
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

	public partial class Companions
	{
		private List<Companion> companions;
		protected LevelingType levelingType;
		public CompanionsId StartingCompanion { get; set; }
		public Dictionary<CompanionsId, LocationIds> Locations { get; set; }
		public Dictionary<CompanionsId, bool> Available { get; set; }

		public Companion this[CompanionsId companion]
		{
			get => companions[((int)companion) - 1];
		}
		public Companions(LevelingType type)
		{
			levelingType = type;
			companions = new();

			StartingCompanion = CompanionsId.None;

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

			Available = new()
			{
				{ CompanionsId.Kaeli, false },
				{ CompanionsId.Tristam, false },
				{ CompanionsId.Phoebe, false },
				{ CompanionsId.Reuben, false },
			};

		}
		public void SetStartingCompanion(StartingCompanionType companionoption, MT19337 rng)
		{
			List<CompanionsId> companions = new()
			{
				CompanionsId.Kaeli,
				CompanionsId.Tristam,
				CompanionsId.Phoebe,
				CompanionsId.Reuben
			};

			Dictionary<StartingCompanionType, CompanionsId> selectedcompanions = new()
			{
				{ StartingCompanionType.Random, rng.PickFrom(companions) },
				{ StartingCompanionType.RandomPlusNone, rng.PickFrom(companions.Append(CompanionsId.None).ToList()) },
				{ StartingCompanionType.None, CompanionsId.None },
				{ StartingCompanionType.Kaeli, CompanionsId.Kaeli },
				{ StartingCompanionType.Tristam, CompanionsId.Tristam },
				{ StartingCompanionType.Phoebe, CompanionsId.Phoebe },
				{ StartingCompanionType.Reuben, CompanionsId.Reuben },
			};

			StartingCompanion = selectedcompanions[companionoption];
		}
		public void SetAvailableCompanions(AvailableCompanionsType companionoption, MT19337 rng)
		{
			List<CompanionsId> companions = new()
			{
				CompanionsId.Kaeli,
				CompanionsId.Tristam,
				CompanionsId.Phoebe,
				CompanionsId.Reuben
			};

			Dictionary<AvailableCompanionsType, int> selectedcompanions = new()
			{
				{ AvailableCompanionsType.Zero, 0 },
				{ AvailableCompanionsType.One, 1 },
				{ AvailableCompanionsType.Two, 2 },
				{ AvailableCompanionsType.Three, 3 },
				{ AvailableCompanionsType.Four, 4 },
				{ AvailableCompanionsType.Random14, rng.Between(1,4) },
				{ AvailableCompanionsType.Random04, rng.Between(0,4) },
			};

			int companionqty = selectedcompanions[companionoption];

			List<CompanionsId> availablecompanions = new();

			if (StartingCompanion != CompanionsId.None)
			{
				availablecompanions.Add(StartingCompanion);
				companions.Remove(StartingCompanion);
				companionqty--;
			}

			for (int i = 0; i < companionqty; i++)
			{
				availablecompanions.Add(rng.TakeFrom(companions));
			}

			foreach (var companion in availablecompanions)
			{
				Available[companion] = true;
			}
		}
		public void SetCompanionsLocation(List<Room> rooms)
		{
			Locations = new();

			Dictionary<int, LocationIds> locationRooms = new()
			{
				{ 17, LocationIds.Foresta },
				{ 24, LocationIds.SandTemple },
				{ 39, LocationIds.LibraTemple },
				{ 77, LocationIds.Fireburg },	 // Reuben's House
				{ 51, LocationIds.LifeTemple },
				{ 41, LocationIds.Aquaria },
				{ 92, LocationIds.SealedTemple },
				{ 75, LocationIds.WintryTemple },
				{ 123, LocationIds.RopeBridge },
				{ 153, LocationIds.KaidgeTemple },
				{ 154, LocationIds.WindholeTemple },
				{ 185, LocationIds.LightTemple },
			};

			List<(string name, CompanionsId id)> companions = new()
			{
				("Kaeli Companion", CompanionsId.Kaeli),
				("Tristam Companion", CompanionsId.Tristam),
				("Phoebe Companion", CompanionsId.Phoebe),
				("Reuben Companion", CompanionsId.Reuben),
			};

			foreach (var companion in companions)
			{
				var targetroom = rooms.Find(r => r.GameObjects.Select(o => o.Name).ToList().Contains(companion.name));
				Locations.Add(companion.id, locationRooms[targetroom.Id]);
			}
		}
		public void Write(FFMQRom rom)
		{
			QuestRoutines(rom);
			LevelingRoutine(rom);

			// Leveling Routine Hooks
			// Companion Join
			rom.PutInBank(0x00, 0xA26B, Blob.FromHex("2200a310eaeaeaea"));
			rom.PutInBank(0x10, 0xA300, Blob.FromHex("d006a980001ca0102000a02080a26b"));

			if (levelingType >= LevelingType.BenPlus0 && levelingType <= LevelingType.BenPlus10)
			{
				// Ben Levelup
				rom.PutInBank(0x02, 0x8830, Blob.FromHex("2210a310"));
				rom.PutInBank(0x10, 0xA310, Blob.FromHex("8b08e230ad9010c9fff0062000a020a0a222029b0028ab6b"));
			}

			// lut_CompanionLevel
			List<byte> levelsset = new()
			{
				07, 31, 31, 31, 31,
				07, 23, 23, 23, 23,
				15, 34, 34, 34, 34,
				23, 31, 31, 31, 31,
			};

			if (levelingType == LevelingType.SaveCrystalsAll || levelingType == LevelingType.QuestsExtended)
			{
				levelsset = new()
				{
					07, 15, 23, 34, 41,
					07, 15, 23, 34, 41,
					07, 15, 23, 34, 41,
					07, 15, 23, 34, 41,
				};
			}

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

			if (levelingType >= LevelingType.BenPlus0 && levelingType <= LevelingType.BenPlus10)
			{
				levelingroutine = "A0A0";

				if (levelingType == LevelingType.BenPlus5)
				{
					benlevelbonus = "05";
				}
				else if (levelingType == LevelingType.BenPlus10)
				{
					benlevelbonus = "0A";
				}
			}

			// DoLeveling
			rom.PutInBank(0x10, 0xA000, Blob.FromHex($"08c2302080a020{levelingroutine}2020a12000a22040a02860"));

			// UpdateCompanionStats
			rom.PutInBank(0x10, 0xA040, Blob.FromHex($"08e220c210a20300bdcc109da610187daa109da210ca10f02860"));

			// GetCompanionOffset
			rom.PutInBank(0x10, 0xA080, Blob.FromHex("08e230a900ebad920e38e901c2300a0a0a0a0a0a0aaa2860"));

			// SetLevel_Ben
			rom.PutInBank(0x10, 0xA0A0, Blob.FromHex($"08e220ad10101869{benlevelbonus}8d90102860"));

			// SetLevel_Quest; flush to ComputeStats, maybe move one or the other
			rom.PutInBank(0x10, 0xA0B0, Blob.FromHex("08e220c210daa900ebad920e38e9010a0aa8186d920e38e901aa981869a94822769700f001e8681869014822769700f001e8681869014822769700f001e86818690122769700f001e8bf00a4108d9010fa2860"));

			// ComputeStats
			rom.PutInBank(0x010, 0xA120, Blob.FromHex("08c230ad90108d02422080a18d961020a0a1e2208dcc108da61020a0a1e2208dcd108da71020a0a1e2208dce108da81020a0a1e2208dcf108da910ad90104820b0a18d9b10684a4820b0a18d9c10684a20b0a18d9d102860"));

			// GetStats, GetStats99, GetMP
			rom.PutInBank(0x10, 0xA180, Blob.FromHex("08e220c210bf00a5108d0342c230ebebe818ad16427f00a510e8e82860000000c2302080a1c963009003a96300600000187f00a510e8c9639002a96360"));
			/*
			// GetStatsHP
			rom.PutInBank(0x10, 0xA1C0, Blob.FromHex("c2302080a1c968069003a9680660"));
			*/
			// SetEquipSpells, LoadEquipSpells
			rom.PutInBank(0x10, 0xA200, Blob.FromHex("08e220c210ad9010df00a510b009e8e8e8e8e8e8e880eec230e88a186900a5aaa90400a0b510540010282030a260"));

			// HealUp
			rom.PutInBank(0x10, 0xA280, Blob.FromHex("08e220c210ae96108e9410ae9b108e9810ad9d108d9a109ca1102860"));

			// HpUp
			rom.PutInBank(0x10, 0xA2A0, Blob.FromHex("08c230ad9410186928008d94102860"));

			// ComputeAI
			rom.PutInBank(0x10, 0xA230, Blob.FromHex("08c230dabf0000102903000aaabf30a4108dc210fae220c210a900ebbf000010aabf20a410a8a904aa0bf4b8102b225a9700f003988002a9009dc010e88ae00c0090e72b2860"));
			rom.PutInBank(0x10, 0xA420, Blob.FromHex("006432221914110f0d")); // lut_OddsValues
			rom.PutInBank(0x10, 0xA430, Blob.FromHex("64004123283c283c")); // lut_AiAttacks
		}
	}
}
