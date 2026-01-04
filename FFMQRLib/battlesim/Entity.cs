using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;



namespace FFMQLib
{
	public class ScriptProfile
	{ 
		public List<BattleAction> Actions;
		public List<int> ActionIds;
		public List<int> WeigthedScript;
	}
	public class Entity
	{
		public int Level;
		public int Attack => Math.Max(0, coreAttack + bonusAttack - debuffAttack);
		public int Defense => Math.Max(0, coreDefense + bonusDefense + Armor - debuffDefense);
		public int Speed => Math.Max(0, coreSpeed + bonusSpeed - debuffSpeed);
		public int Magic => Math.Max(0, coreMagic + bonusMagic - debuffMagic);
		public int Accuracy => Level / 2 + 0x4B;
		public int Evade;
		public int MagicDefense => MagicArmor + Magic;
		public int MagicEvade;
		public int Hp;
		public int MaxHp;
		public int Armor;
		public int MagicArmor;
		public string Name;
		private int bonusAttack;
		private int bonusDefense;
		private int bonusSpeed;
		private int bonusMagic;
		private int coreAttack;
		private int coreDefense;
		private int coreSpeed;
		private int coreMagic;
		private int debuffAttack;
		private int debuffDefense;
		private int debuffSpeed;
		private int debuffMagic;
		public List<ElementsType> Resistances;
		public List<ElementsType> Weaknesses;
		public List<ElementsType> Ailments;
		public List<BattleAction> Actions;
		public List<Items> Gears;
		private List<int> weightedScript;
		private MT19337 Rng;
		public bool IsPlayer;
		public bool IsBen = false;
		public bool IsDefending;
		public bool IsUndead;
		public bool IsBoss;
		public bool HydraMode;
		public bool SimulationMode = false;
		public PazuzuModes PazuzuMode = PazuzuModes.AttackingShieldOff;
		public int PazuzuRound;
		public DkModes DkMode;
		public Teams Team;
		public int Initiative;
		public AiProfiles AiProfile;
		public StoredAction NextAction;
		public Logger Log;
		public bool Recovered;
		public bool Multiply = false;
		public EnemyIds EnemyId;
		public bool Debuffed => debuffAttack > 0 || debuffDefense > 0 || debuffSpeed > 0 || debuffMagic > 0;
		public bool TechnicalDeath => Ailments.Intersect(tdAilments).Any();

		private EnemyAttackLinks Scripts;

		private List<ElementsType> tdAilments = new() { ElementsType.Doom, ElementsType.Stone };
		private List<List<int>> scriptsWeight = new()
		{
			new() { 0x2d, 0x23, 0x0a, 0x0a, 0x00, 0x00, 0x00, 0x00 },
			new() { 0x1e, 0x1e, 0x14, 0x14, 0x00, 0x00, 0x00, 0x00 },
			new() { 0x28, 0x1e, 0x14, 0x0a, 0x00, 0x00, 0x00, 0x00 },
			new() { 0x23, 0x19, 0x0a, 0x0a, 0x00, 0x00, 0x00, 0x00 },
			new() { 0x19, 0x19, 0x16, 0x0a, 0x0a, 0x0a, 0x00, 0x00 },
			new() { 0x14, 0x14, 0x14, 0x14, 0x0a, 0x08, 0x00, 0x00 },
			new() { 0x1c, 0x17, 0x12, 0x0d, 0x0a, 0x0a, 0x00, 0x00 },
			new() { 0x14, 0x14, 0x0a, 0x0a, 0x0a, 0x08, 0x00, 0x00 },
			new() { 0x14, 0x0f, 0x0f, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a },
			new() { 0x13, 0x0f, 0x0f, 0x0f, 0x0a, 0x0a, 0x0a, 0x0a },
			new() { 0x11, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x09, 0x09 },
			new() { 0x2d, 0x23, 0x8a, 0x0a, 0x00, 0x00, 0x00, 0x00 },
			new() { 0x23, 0x19, 0x8a, 0x0a, 0x0a, 0x0a, 0x00, 0x00 },
			new() { 0x19, 0x19, 0x16, 0x8a, 0x0a, 0x08, 0x00, 0x00 },
			new() { 0x14, 0x0f, 0x0f, 0x8a, 0x0a, 0x0a, 0x0a, 0x0a },
			new() { 0x13, 0x0f, 0x0f, 0x0f, 0x89, 0x09, 0x09, 0x09 }, // 0x0F

			// sps
			
			new() { 0x26, 0x13, 0x13, 0x05, 0x05, 0x0e, 0x00, 0x00 }, // Flamerus
			new() { 0x14, 0x14, 0x1e, 0x0e, 0x14, 0x00, 0x00, 0x00 }, // IceGolem
			new() { 0x2e, 0x1b, 0x1b, 0x00, 0x00, 0x00, 0x00, 0x00 }, // Hydra/Wyvern
			new() { 0x19, 0x14, 0x19, 0x14, 0x0a, 0x00, 0x00, 0x00 }, // Pazuzu
			new() { 0x14, 0x14, 0x14, 0x14, 0x14, 0x00, 0x00, 0x00 }, // Zuh
			new() { 0x28, 0x14, 0x14, 0x14, 0x00, 0x00, 0x00, 0x00 }, // Dk1

		};

		public Entity(Logger log, MT19337 rng)
		{
			Rng = rng;
			Log = log;
		}
		public Entity(Logger log, MT19337 rng, Enemy enemy, EnemyAttackLinks scripts)
		{
			Rng = rng;
			Log = log;
			Scripts = scripts;
			InitFromEnemy(enemy, scripts);
		}
		public Entity(Logger log, MT19337 rng, PowerLevels powerlevel, CompanionsId companion, int level = 0)
		{
			Rng = rng;
			Log = log;
			if (companion == CompanionsId.Benjamin)
			{
				InitBen(powerlevel, level);
			}
			else
			{
				InitCompanion(powerlevel, companion, level);
			}
		}
		public void InitBen(PowerLevels power, int level = 0)
		{
			Actions = new();
			Name = "Benjamin";
			Items startingWeapon = Items.SteelSword;

			List<(int level, Items gear)> gearThreshold = new()
			{
				(1, Items.SteelSword),
				(1, Items.Axe),
				(1, Items.CureBook),
				(4, Items.Bomb),
				(4, Items.SteelShield),
				(4, Items.QuakeBook),
				(4, Items.Charm),
				(7, Items.FireBook),
				(7, Items.SteelHelm),
				(7, Items.MagicRing),
				(7, Items.CatClaw),
				(7, Items.MagicRing),
				(8, Items.JumboBomb),
				(8, Items.HealBook),
				(11, Items.KnightSword),
				(11, Items.NobleArmor),
				(11, Items.BlizzardBook),
				(11, Items.VenusShield),
				//(11, Items.ExitBook),
				(12, Items.BattleAxe),
				(12, Items.ThunderSeal),
				(12, Items.CharmClaw),
				(12, Items.WhiteSeal),
				(16, Items.LifeBook),
				(16, Items.MoonHelm),
				(16, Items.AeroBook),
				(16, Items.MegaGrenade),
				(19, Items.GiantsAxe),
				(19, Items.MeteorSeal),
				(19, Items.CupidLocket),
				(19, Items.ApolloHelm),
				(19, Items.DragonClaw),
				(21, Items.Excalibur),
				(21, Items.FlareSeal),
				(22, Items.GaiasArmor),
				(22, Items.AegisShield),
			};


			if (level != 0)
			{
				Level = level;
				Gears = gearThreshold.Where(g => g.level <= Level).Select(g => g.gear).ToList();
			}
			else if (power == PowerLevels.Initial)
			{
				Level = 1;
				List<Items> startingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
				startingWeapon = Rng.PickFrom(startingWeapons);

				Gears = new() { Items.SteelArmor, startingWeapon };
			}
			else if (power == PowerLevels.Intermediate)
			{
				Level = 10;
				List<Items> startingWeapons = new() { Items.KnightSword, Items.BattleAxe, Items.CharmClaw, Items.JumboBomb };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.NobleArmor, Items.VenusShield, Items.MoonHelm, Items.MagicRing, startingWeapon };
				Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.BlizzardBook], Battle.BattleActions[(int)Items.ThunderSeal] });
			}
			else if (power == PowerLevels.Strong)
			{
				Level = 20;
				List<Items> startingWeapons = new() { Items.Excalibur, Items.GiantsAxe, Items.DragonClaw, Items.MegaGrenade };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.GaiasArmor, Items.AegisShield, Items.ApolloHelm, Items.CupidLocket, startingWeapon };
				Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.WhiteSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}
			else if (power == PowerLevels.Godly)
			{
				Level = 30;
				List<Items> startingWeapons = new() { Items.Excalibur, Items.GiantsAxe, Items.DragonClaw, Items.MegaGrenade };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.GaiasArmor, Items.AegisShield, Items.ApolloHelm, Items.CupidLocket, startingWeapon };
				Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.WhiteSeal], Battle.BattleActions[(int)Items.MeteorSeal], Battle.BattleActions[(int)Items.FlareSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}
			else if (power == PowerLevels.Plain10)
			{
				Level = 10;
				List<Items> startingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, startingWeapon };
				//Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.WhiteSeal], Battle.BattleActions[(int)Items.MeteorSeal], Battle.BattleActions[(int)Items.FlareSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}
			else if (power == PowerLevels.Plain20)
			{
				Level = 20;
				List<Items> startingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, startingWeapon };
				//Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.WhiteSeal], Battle.BattleActions[(int)Items.MeteorSeal], Battle.BattleActions[(int)Items.FlareSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}
			else if (power == PowerLevels.Plain30)
			{
				Level = 30;
				List<Items> startingWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };
				startingWeapon = Rng.PickFrom(startingWeapons);
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, startingWeapon };
				//Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.WhiteSeal], Battle.BattleActions[(int)Items.MeteorSeal], Battle.BattleActions[(int)Items.FlareSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}

			//Actions.Add(Battle.BattleActions[(int)startingWeapon]);
			var validOffenseGear = Enumerable.Range(0x15, 23).Select(g => (Items)g).ToList();
			Actions.AddRange(Gears.Intersect(validOffenseGear).Select(g => Battle.BattleActions[(int)g]).ToList());
			Actions.Add(Battle.BattleActions[(int)Items.Refresher]);

			MaxHp = 40 * Level;
			Hp = MaxHp;

			coreAttack = 4 + 3 * Level;
			coreDefense = 4 + 2 * Level;
			coreSpeed = 6 + 2 * Level;
			coreMagic = 9 + Level;

			Resistances = new();
			Weaknesses = new();
			Ailments = new();

			ProcessGears();
			RestoreStats();

			IsDefending = false;
			IsPlayer = true;
			IsBen = true;
			IsUndead = false;
			IsBoss = false;
			Recovered = false;
			Team = Teams.TeamA;
			weightedScript = new();
			AiProfile = AiProfiles.Player;
		}
		private Dictionary<ArmorFlags, Items> armorFlagsToItems = new()
		{
			{ ArmorFlags.SteelHelm, Items.SteelHelm },
			{ ArmorFlags.MoonHelm, Items.MoonHelm },
			{ ArmorFlags.ApolloHelm, Items.ApolloHelm },
			{ ArmorFlags.SteelArmor, Items.SteelArmor },
			{ ArmorFlags.NobleArmor, Items.NobleArmor },
			{ ArmorFlags.GaiasArmor, Items.GaiasArmor },
			{ ArmorFlags.ReplicaArmor, Items.ReplicaArmor },
			{ ArmorFlags.MysticRobes, Items.MysticRobes },
			{ ArmorFlags.FlameArmor, Items.FlameArmor },
			{ ArmorFlags.BlackRobe, Items.BlackRobe },
			{ ArmorFlags.SteelShield, Items.SteelShield },
			{ ArmorFlags.VenusShield, Items.VenusShield },
			{ ArmorFlags.AegisShield, Items.AegisShield },
			{ ArmorFlags.EtherShield, Items.EtherShield },
			{ ArmorFlags.Charm, Items.Charm },
			{ ArmorFlags.MagicRing, Items.MagicRing },
			{ ArmorFlags.CupidLocket, Items.CupidLocket },
		};
		private Dictionary<SpellFlags, Items> spellFlagsToItems = new()
		{
			{ SpellFlags.ExitBook, Items.ExitBook },
			{ SpellFlags.CureBook, Items.CureBook },
			{ SpellFlags.HealBook, Items.HealBook },
			{ SpellFlags.LifeBook, Items.LifeBook },
			{ SpellFlags.QuakeBook, Items.QuakeBook },
			{ SpellFlags.FireBook, Items.FireBook },
			{ SpellFlags.BlizzardBook, Items.BlizzardBook },
			{ SpellFlags.AeroBook, Items.AeroBook },
			{ SpellFlags.ThunderSeal, Items.ThunderSeal },
			{ SpellFlags.WhiteSeal, Items.WhiteSeal },
			{ SpellFlags.MeteorSeal, Items.MeteorSeal },
			{ SpellFlags.FlareSeal, Items.FlareSeal },
		};
		public CompanionsId InitCompanion(PowerLevels power, CompanionsId currentcompanion, int level = 0)
		{
			Actions = new();
			Companions companions = new(LevelingType.QuestsExtended);

			List<CompanionsId> validcompanions = new() { CompanionsId.Kaeli, CompanionsId.Reuben, CompanionsId.Phoebe, CompanionsId.Tristam };
			//var currentcompanion = Rng.PickFrom(validcompanions);
			var companion = companions[currentcompanion];

			Name = Enum.GetName(typeof(CompanionsId), currentcompanion);
			//power = PowerLevels.Strong;
			if (power == PowerLevels.Plain10)
			{
				level = 10;
			}
			else if (power == PowerLevels.Plain20)
			{
				level = 20;
			}
			else if (power == PowerLevels.Plain30)
			{
				level = 30;
			}



			if (level > 0)
			{
				//Level = (int)(level * 1.5) + 5;
				Level = level;
				if (Level < 23)
				{
					Gears = companion.ArmorSet1.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
				}
				else
				{
					Gears = companion.ArmorSet2.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
				}
			}
			else if (power == PowerLevels.Initial)
			{
				Level = 7;
				Gears = companion.ArmorSet1.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
			}
			else if (power == PowerLevels.Intermediate)
			{
				Level = 15;
				Gears = companion.ArmorSet1.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
			}
			else if (power == PowerLevels.Strong)
			{
				Level = 23;
				Gears = companion.ArmorSet2.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
			}
			else if (power == PowerLevels.Godly)
			{
				Level = 34;
				Gears = companion.ArmorSet2.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
			}
			/*
			else if (power == PowerLevels.Plain10)
			{
				Level = 15;
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, companion.Weapon };
			}
			else if (power == PowerLevels.Plain20)
			{
				Level = 25;
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, companion.Weapon };
			}
			else if (power == PowerLevels.Plain30)
			{
				Level = 35;
				Gears = new() { Items.SteelArmor, Items.SteelHelm, Items.SteelShield, companion.Weapon };
			}*/

			Actions.Add(Battle.BattleActions[(int)companion.Weapon]);
			Actions.Add(Battle.BattleActions[(int)Items.Refresher]);
			int loadoutselect = Level < 23 ? 0 : 1;

			var loadOut = companion.LoadOut[loadoutselect];

			if (loadOut.Spells.Any())
			{
				Actions.AddRange(loadOut.Spells.Select(s => Battle.BattleActions[(int)spellFlagsToItems[s]]).ToList());
			}

			MaxHp = companion.HPBase + 40 * Level;
			Hp = MaxHp;

			coreAttack = companion.AttBase + companion.AttMultiplier * Level;
			coreDefense = companion.DefBase + companion.DefMultiplier * Level;
			coreSpeed = companion.SpdBase + companion.SpdMultiplier * Level;
			coreMagic = companion.MagBase + companion.MagMultiplier * Level;

			Resistances = new();
			Weaknesses = new();
			Ailments = new();

			ProcessGears();
			RestoreStats();

			IsDefending = false;
			IsPlayer = true;
			IsUndead = false;
			IsBoss = false;
			Recovered = false;
			Team = Teams.TeamA;
			weightedScript = new();
			AiProfile = AiProfiles.Companion;

			return currentcompanion;
		}
		public void InitFromEnemy(Enemy enemy, EnemyAttackLinks scripts)
		{
			Name = Enum.GetName(typeof(EnemyIds), enemy.Id);

			EnemyId = enemy.Id;

			MaxHp = enemy.HP;
			Hp = enemy.HP;
			Level = enemy.Level;
			coreAttack = enemy.Attack;
			coreDefense = enemy.Defense;
			coreSpeed = enemy.Speed;
			coreMagic = enemy.Magic;
			MagicArmor = enemy.MagicDefense;
			MagicEvade = enemy.MagicEvade;
			Evade = enemy.Evade;

			Resistances = enemy.Resistances;
			Weaknesses = enemy.Weaknesses;
			Ailments = new();

			RestoreStats();

			IsDefending = false;
			IsPlayer = false;
			IsUndead = false;
			IsBoss = enemy.Id > EnemyIds.Minotaur || (enemy.Id == EnemyIds.StoneGolem) || (enemy.Id == EnemyIds.SkullrusRex);
			Recovered = false;
			Team = Teams.TeamB;
			weightedScript = scriptsWeight[scripts.Data[enemy.Id].AttackPattern].ToList();

			AiProfile =	AiProfiles.Enemy;

			switch (enemy.Id)
			{
				case EnemyIds.FlamerusRex:
					weightedScript = scriptsWeight[0x10].ToList();
					break;
				case EnemyIds.IceGolem:
					weightedScript = scriptsWeight[0x11].ToList();
					AiProfile = AiProfiles.IceGolem;
					break;
				case EnemyIds.DualheadHydra:
					weightedScript = scriptsWeight[0x12].ToList();
					AiProfile = AiProfiles.Hydra;
					break;
				case EnemyIds.TwinheadWyvern:
					weightedScript = scriptsWeight[0x12].ToList();
					AiProfile = AiProfiles.Hydra;
					break;
				case EnemyIds.Pazuzu:
					weightedScript = scriptsWeight[0x13].ToList();
					AiProfile = AiProfiles.Pazuzu;
					break;
				case EnemyIds.Zuh:
					weightedScript = scriptsWeight[0x14].ToList();
					AiProfile = AiProfiles.Pazuzu;
					break;
				case EnemyIds.DarkKing:
					weightedScript = scriptsWeight[0x15].ToList();
					AiProfile = AiProfiles.DarkKing;
					break;
			}


			var stongolemlist = new List<int> { 0xB2, 0x88, 0xB7, 0xFF, 0xFF, 0xFF };
			Actions = scripts.Data[enemy.Id].Attacks.Select(a => Battle.BattleActions[(int)a]).ToList();
			//Actions = stongolemlist.Select(a => Battle.BattleActions[a]).ToList();
			if (enemy.Id == EnemyIds.IceGolem)
			{
				Actions.Add(Battle.BattleActions[0x8A]);
			}

		}
		public void RollInitiative(MT19337 rng)
		{
			Initiative = Speed + rng.Between(0, Speed / 2);
		}
		public void PlayerPickAction(List<Entity> battlers, MT19337 rng)
		{
			PlayerAi(battlers, rng);
		}
		public void DoRound(List<Entity> battlers, MT19337 rng)
		{
			switch (AiProfile)
			{
				case AiProfiles.Player:
					PlayerActionAi(battlers, rng);
					break;
				case AiProfiles.Companion:
					CompanionAi(battlers, rng);
					break;
				case AiProfiles.Enemy:
					EnemyAi(battlers, rng);
					break;
				case AiProfiles.IceGolem:
					IceGolemAi(battlers, rng);
					break;
				case AiProfiles.Hydra:
					HydraAi(battlers, rng);
					break;
				case AiProfiles.Pazuzu:
					PazuzuAi(battlers, rng);
					break;
				case AiProfiles.DarkKing:
					DarkKingAi(battlers, rng);
					break;
			}
		}
		//public ()
		public List<(int slot, int prob)> ScriptProbability2()
		{
			List<(int slot, int prob)> truprobab = new();

			// standard
			int totalodds = 0;

			for (int i = 0; i < 6; i++)
			{
				if (i < Actions.Count && Actions[i].Id != 0xFF && weightedScript[i] != 0x00)
				{
					truprobab.Add((i, weightedScript[i] & 0x7F));
				}

				totalodds += weightedScript[i];
			}

			for (int i = 0; i < truprobab.Count; i++)
			{
				truprobab[i] = (truprobab[i].slot, (truprobab[i].prob * 100) / totalodds);
			}
			//
			// firstrike 
			/*
			for (int i = 0; i < 6; i++)
			{
				if ((weightedScript[i] & 0x80) > 0)
				{
					truprobab.Add((i, 0xFF));
				}
			}*/

			return truprobab;
		}
		public List<(int slot, Entity enemy, int prob)> ScriptProbability()
		{
			List<(int slot, Entity enemy, int prob)> truprobab = new();

			// standard
			int totalodds = 0;

			for (int i = 0; i < 6; i++)
			{
				if (i < Actions.Count && Actions[i].Id != 0xFF && weightedScript[i] != 0x00)
				{
					truprobab.Add((i, this, weightedScript[i] & 0x7F));
				}

				totalodds += weightedScript[i];
			}

			for (int i = 0; i < truprobab.Count; i++)
			{
				truprobab[i] = (truprobab[i].slot, this, (truprobab[i].prob * 100) / totalodds);
			}
			//
			// firstrike 
			/*
			for (int i = 0; i < 6; i++)
			{
				if ((weightedScript[i] & 0x80) > 0)
				{
					truprobab.Add((i, 0xFF));
				}
			}*/

			return truprobab;
		}
		private void EnemyAi(List<Entity> battlers, MT19337 rng)
		{
			bool foundaction = false;
			int actionpos = 0;

			for (int i = 0; i < 6; i++)
			{
				if ((weightedScript[i] & 0x80) > 0)
				{
					actionpos = i;
					foundaction = true;
					weightedScript[i] &= 0x7F;
				}
			}

			while (!foundaction)
			{
				int roll = rng.Between(0, 100);
				for (int i = 0; i < 6; i++)
				{
					if (roll < weightedScript[i])
					{
						if (i < Actions.Count && Actions[i].Id != 0xFF)
						{
							actionpos = i;
							foundaction = true;
							break;
						}
						else
						{
							foundaction = false;
							break;
						}
					}
					else
					{
						roll -= weightedScript[i];
					}
				}
			}

			List<Entity> validTargets = battlers.Where(b => b.Team != Team && !b.TechnicalDeath).ToList();
			Actions[actionpos].Execute(this, validTargets, TargetSelections.PrioritizeMultipleTarget, Log, rng);
		}

		private void IceGolemAi(List<Entity> battlers, MT19337 rng)
		{
			if (Hp < (MaxHp / 4) && rng.Between(0, 100) < 50)
			{
				List<Entity> validTargets = battlers.Where(b => b.Team != Team).ToList();
				Actions[6].Execute(this, validTargets, TargetSelections.OverrideMultiple, Log, rng);
			}
			else
			{
				EnemyAi(battlers, rng);
			}
		}
		private void HydraAi(List<Entity> battlers, MT19337 rng)
		{
			if (Hp < (MaxHp / 2) && !HydraMode)
			{
				HydraMode = true;
				weightedScript = new() { 0x00, 0x00, 0x00, 0x3C, 0x1E, 0x0A };
			}

			EnemyAi(battlers, rng);
		}
		private void PazuzuAi(List<Entity> battlers, MT19337 rng)
		{
			PazuzuRound++;

			if (PazuzuRound > 5)
			{
				PazuzuMode++;

				if (PazuzuMode > PazuzuModes.DeactivateShield)
				{
					PazuzuMode = PazuzuModes.AttackingShieldOff;
				}
			}

			EnemyAi(battlers, rng);
		}
		private void DarkKingAi(List<Entity> battlers, MT19337 rng)
		{
			if (DkMode == DkModes.Phase1 && Hp < (MaxHp * 0.75))
			{
				DkMode = DkModes.Phase2;
				Actions = Scripts.Data[EnemyIds.DarkKing + 1].Attacks.Select(a => Battle.BattleActions[(int)a]).ToList();
				//Actions = new() { Battle.BattleActions[0xCD], Battle.BattleActions[0xCC], Battle.BattleActions[0xCB], Battle.BattleActions[0xCF], Battle.BattleActions[0xCE], Battle.BattleActions[0xD0] };
				weightedScript = new() { 0x14, 0x14, 0x14, 0x94, 0x0A, 0x0A };
			}
			else if (DkMode == DkModes.Phase2 && Hp < (MaxHp * 0.5))
			{
				DkMode = DkModes.Phase34;
				Actions = Scripts.Data[EnemyIds.DarkKing + 2].Attacks.Select(a => Battle.BattleActions[(int)a]).ToList();
				//Actions = new() { Battle.BattleActions[0xD1], Battle.BattleActions[0xD2], Battle.BattleActions[0xD3], Battle.BattleActions[0xD4], Battle.BattleActions[0xD5], Battle.BattleActions[0xD6] };
				weightedScript = new() { 0x14, 0x14, 0x8C, 0x0C, 0x12, 0x12 };
			}

			EnemyAi(battlers, rng);
		}
		private void PlayerActionAi(List<Entity> battlers, MT19337 rng)
		{
			List<ElementsType> disablingAilments = new() { ElementsType.Doom, ElementsType.Stone };
			NextAction.Execute(battlers.Where(b => b.Team != Team && !b.Ailments.Intersect(disablingAilments).Any()).ToList());
		}

		private void CompanionAi(List<Entity> battlers, MT19337 rng)
		{
			List<ElementsType> disablingAilments = new() { ElementsType.Doom, ElementsType.Stone };
			PlayerAi(battlers, rng);
			NextAction.Execute(battlers.Where(b => b.Team != Team && !b.Ailments.Intersect(disablingAilments).Any()).ToList());
		}
		private void PlayerAi(List<Entity> battlers, MT19337 rng)
		{
			List<ElementsType> disablingAilments = new() { ElementsType.Doom, ElementsType.Stone };
			var validEnemyTargets = battlers.Where(b => b.Team != Team && !b.Ailments.Intersect(disablingAilments).Any()).ToList();
			var validAllyTargets = battlers.Where(b => b.Team == Team).ToList();

			bool tookaction = false;

			if (!Ailments.Contains(ElementsType.Confusion))
			{
				var deadAllies = validAllyTargets.Where(t => t.Ailments.Contains(ElementsType.Doom)).ToList();
				var stonedAllies = validAllyTargets.Where(t => t.Ailments.Contains(ElementsType.Stone)).ToList();
				var incapacitedAllies = validAllyTargets.Where(t => t.Ailments.Contains(ElementsType.Paralysis) || t.Ailments.Contains(ElementsType.Confusion) || t.Ailments.Contains(ElementsType.Sleep)).ToList();
				var poisonedBlindedAllies = validAllyTargets.Where(t => t.Ailments.Contains(ElementsType.Poison) || t.Ailments.Contains(ElementsType.Blind)).ToList();
				var statusAllies = validAllyTargets.Where(t => t.Ailments.Except(new List<ElementsType>() { ElementsType.Doom }).Any()).ToList();
				var criticalAllies = validAllyTargets.Where(t => t.Hp < t.MaxHp / 2 && !t.TechnicalDeath).ToList();
				var debuffedAllies = validAllyTargets.Where(t => t.Debuffed && !t.TechnicalDeath).ToList();

				BattleAction lifeaction;
				BattleAction healaction;
				BattleAction cureaction;
				BattleAction refresher;

				bool haslife = Actions.TryFind(a => a.Id == 0x17, out lifeaction);
				bool hasheal = Actions.TryFind(a => a.Id == 0x16, out healaction);
				bool hascure = Actions.TryFind(a => a.Id == 0x15, out cureaction);
				bool hasrefressher = Actions.TryFind(a => a.Id == 0x13, out refresher);

				if (!tookaction && deadAllies.Any() && haslife)
				{
					NextAction = new StoredAction(lifeaction, this, deadAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
					tookaction = true;
				}
				else if (!tookaction && statusAllies.Any() && (haslife || hasheal))
				{
					if (haslife)
					{
						NextAction = new StoredAction(lifeaction, this, statusAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
						tookaction = true;
					}
					else
					{
						NextAction = new StoredAction(healaction, this, statusAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
						tookaction = true;
					}
				}
				else if (!tookaction && criticalAllies.Any() && (haslife || hascure))
				{
					var enemies = battlers.Where(b => b.Team != Team).ToList();
					var ally = battlers.Where(b => b.Team == Team && b.Name != Name).ToList();
					/*
					int enemiesInit = (int)(enemies.OrderByDescending(e => e.Speed).First().Speed * 1.5);
					int myInit = (int)(Speed * 1.5);
					int allyInit = 0;
					if (ally.Any())
					{
						allyInit = (int)(ally.First().Speed * 1.5);
					}*/


					if (criticalAllies.Count == 1)
					{
						if (haslife)
						{
							NextAction = new StoredAction(lifeaction, this, criticalAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
						else
						{
							NextAction = new StoredAction(cureaction, this, criticalAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
					}
					else
					{
						if (hascure)
						{
							NextAction = new StoredAction(cureaction, this, criticalAllies, TargetSelections.PrioritizeMultipleTarget, Log, rng);
							tookaction = true;

						}
						else
						{
							NextAction = new StoredAction(lifeaction, this, criticalAllies.OrderBy(a => a.Hp).ToList().GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
					}
				}
				else if (debuffedAllies.Any())
				{
					NextAction = new StoredAction(refresher, this, debuffedAllies, TargetSelections.PrioritizeSingleTarget, Log, rng);
					tookaction = true;
				}
			}
			
			List<int> validAttackId = Enumerable.Range(0x18, 0x18).ToList();
			List<int> physicalAttackId = Enumerable.Range(0x20, 0x0F).ToList();

			if (!tookaction)
			{
				// if we can't heal or are confused, then just attack
				if (Ailments.Contains(ElementsType.Confusion))
				{
					// pick a side, then just pick a random attack
					if (rng.Between(0, 100) < 50) // confirm odds;
					{
						validEnemyTargets = validAllyTargets.Where(a => !a.Ailments.Contains(ElementsType.Doom) && !a.Ailments.Contains(ElementsType.Stone)).ToList();
					}

					List<BattleAction> validActions = Actions.Where(a => validAttackId.Contains(a.Id)).ToList();
					NextAction = new StoredAction(rng.PickFrom(validActions), this, validEnemyTargets, TargetSelections.RandomTargeting, Log, rng);
				}
				else
				{
					List<(int power, BattleAction action)> priorityCast = new();
						
					List<ElementsType> weaknesses = validEnemyTargets.SelectMany(e => e.Weaknesses).ToList();
					List<ElementsType> resistances = validEnemyTargets.SelectMany(e => e.Resistances).ToList();
					List<BattleAction> validActions = Actions.Where(a => validAttackId.Contains(a.Id)).ToList();
					priorityCast = validActions.Select(a => (a.RelativePower(resistances, weaknesses), a)).ToList();
					priorityCast = priorityCast.OrderByDescending(a => a.power).ToList();

					if (validEnemyTargets.Where(v => v.PazuzuMode == PazuzuModes.AttackingShieldOn || v.PazuzuMode == PazuzuModes.DeactivateShield).Any())
					{
						priorityCast = priorityCast.Where(c => physicalAttackId.Contains(c.action.Id)).ToList();
					}

					NextAction = new StoredAction(priorityCast.First().action, this, validEnemyTargets, TargetSelections.PrioritizeMultipleTarget, Log, rng);
				}
			}
		}
		public bool ImproveGear(List<Entity> battlers, MT19337 rng)
		{
			var validOffenseGear = Enumerable.Range(0x18, 20).Select(g => (Items)g).ToList();
			var validDefenseGear = new List<Items>() { Items.SteelArmor, Items.NobleArmor, Items.GaiasArmor, Items.SteelHelm, Items.MoonHelm, Items.ApolloHelm, Items.SteelShield, Items.VenusShield, Items.AegisShield, Items.Charm, Items.MagicRing, Items.CupidLocket };

			// Remove what we already have
			validOffenseGear = validOffenseGear.Except(Gears).ToList();
			validDefenseGear = validDefenseGear.Except(Gears).ToList();

			List<ElementsType> weaknesses = battlers.SelectMany(b => b.Weaknesses).ToList();
			List<ElementsType> resistances = battlers.SelectMany(b => b.Resistances).ToList();
			List<ElementsType> inflictedailments = battlers.SelectMany(b => b.Actions.SelectMany(a => a.Ailments)).ToList();
			List<ElementsType> typedamage = battlers.SelectMany(b => b.Actions.SelectMany(a => a.TypeDamage)).ToList();

			inflictedailments = inflictedailments.Except(Resistances).ToList();
			typedamage = typedamage.Except(Resistances).ToList();
			var allelements = typedamage.Concat(inflictedailments).ToList();

			var improvementArmor = validDefenseGear.Where(g => geardata[g].Resistances.Intersect(allelements).Any()).OrderBy(g => geardata[g].Armor).ToList();
			//improvementArmor = improvementArmor.OrderByDescending(g => geardata[g].Resistances.Intersect(allelements).Count()).ToList();

			var improvementAttack = validOffenseGear.OrderBy(g => Battle.BattleActions[(int)g].RelativePower(resistances, weaknesses)).ToList();

			if (rng.Between(0, 5) == 0 && improvementArmor.Any())
			{
				// improve defense
				Gears.Add(improvementArmor.First());
			}
			else if (improvementAttack.Any())
			{
				// improve offense
				var newaction = improvementAttack.First();
				Gears.Add(newaction);
				Actions.Add(Battle.BattleActions[(int)newaction]);
			}
			else
			{
				return false;
			}

			ProcessGears();
			return true;
		}
		public void ApplyDebuff(int str, int agi, int spd, int mag)
		{
			debuffAttack += str;
			debuffDefense += agi;
			debuffSpeed += spd;
			debuffMagic += mag;
		}

		public void HealUp()
		{
			Ailments = new();
			Hp = MaxHp;
			RestoreStats();
			Recovered = false;

			if (AiProfile != AiProfiles.Player || AiProfile != AiProfiles.Companion)
			{
				switch (EnemyId)
				{
					case EnemyIds.FlamerusRex:
						weightedScript = scriptsWeight[0x10].ToList();
						break;
					case EnemyIds.IceGolem:
						weightedScript = scriptsWeight[0x11].ToList();
						AiProfile = AiProfiles.IceGolem;
						break;
					case EnemyIds.DualheadHydra:
						weightedScript = scriptsWeight[0x12].ToList();
						AiProfile = AiProfiles.Hydra;
						break;
					case EnemyIds.TwinheadWyvern:
						weightedScript = scriptsWeight[0x12].ToList();
						AiProfile = AiProfiles.Hydra;
						break;
					case EnemyIds.Pazuzu:
						weightedScript = scriptsWeight[0x13].ToList();
						AiProfile = AiProfiles.Pazuzu;
						break;
					case EnemyIds.Zuh:
						weightedScript = scriptsWeight[0x14].ToList();
						AiProfile = AiProfiles.Pazuzu;
						break;
					case EnemyIds.DarkKing:
						weightedScript = scriptsWeight[0x15].ToList();
						AiProfile = AiProfiles.DarkKing;
						break;
				}
			}
		}

		private void ProcessGears()
		{
			bonusAttack = 0;
			bonusDefense = 0;
			bonusSpeed = 0;
			bonusMagic = 0;
			Armor = 0;
			Evade = 0;
			MagicArmor = 0;
			MagicEvade = 0;

			foreach (var gear in Gears)
			{
				if (gear > Items.FlareSeal)
				{
					bonusAttack += geardata[gear].AttackBonus;
					bonusDefense += geardata[gear].DefenseBonus;
					bonusSpeed += geardata[gear].SpeedBonus;
					bonusMagic += geardata[gear].MagicBonus;
					Armor += geardata[gear].Armor;
					Evade += geardata[gear].Evade;
					MagicArmor += geardata[gear].MagicArmor;
					MagicEvade += geardata[gear].MagicEvade;
					Resistances.AddRange(geardata[gear].Resistances);
				}
			}
		}
		public void RestoreStats()
		{
			debuffAttack = 0;
			debuffDefense = 0;
			debuffSpeed = 0;
			debuffMagic = 0;
		}

		public void ProcessDamage(int damage, int defense)
		{
			if (damage < 0)
			{
				Hp = Math.Min(Hp - damage, MaxHp);
				Log.Add(Name + " healed for " + damage + " HP.");
			}
			else
			{
				Hp = Math.Max(Hp - Math.Max(damage - Math.Min(0xFF, defense), 1), 0);
				Log.Add(Name + " was damaged for " + Math.Max(damage - Math.Min(0xFF, defense), 1) + " HP.");
			}

			if (Hp == 0)
			{
				Ailments.Add(ElementsType.Doom);
				Log.Add(Name + " was killed.");
			}
		}

		public Dictionary<Items, GearStats> geardata = new()
		{
			{ Items.SteelSword, new GearStats() {  } },
			{ Items.KnightSword, new GearStats() { SpeedBonus = 5 } },
			{ Items.Excalibur, new GearStats() { SpeedBonus = 5 } },
			{ Items.Axe, new GearStats() { } },
			{ Items.BattleAxe, new GearStats() { } },
			{ Items.GiantsAxe, new GearStats() { } },
			{ Items.CatClaw, new GearStats() { MagicBonus = 5 } },
			{ Items.CharmClaw, new GearStats() { MagicBonus = 5 } },
			{ Items.DragonClaw, new GearStats() { MagicBonus = 5 } },
			{ Items.Bomb, new GearStats() { } },
			{ Items.JumboBomb, new GearStats() { } },
			{ Items.MegaGrenade, new GearStats() { } },

			{ Items.SteelHelm, new GearStats() { Armor = 4, MagicArmor = 4, Evade = 5, MagicEvade = 5, AttackBonus = 5 } },
			{ Items.MoonHelm, new GearStats() { Armor = 9, MagicArmor = 9, Evade = 9, MagicEvade = 9, AttackBonus = 5, Resistances = new() { ElementsType.Fire } } },
			{ Items.ApolloHelm, new GearStats() { Armor = 15, MagicArmor = 14, Evade = 15, MagicEvade = 14, AttackBonus = 5, Resistances = new() { ElementsType.Fire } } },

			{ Items.SteelArmor, new GearStats() { Armor = 6, MagicArmor = 6, Evade = 4, MagicEvade = 5 } },
			{ Items.NobleArmor, new GearStats() { Armor = 12, MagicArmor = 10, Evade = 10, MagicEvade = 10, Resistances = new() { ElementsType.Water, ElementsType.Poison } } },
			{ Items.GaiasArmor, new GearStats() { Armor = 15, MagicArmor = 12, Evade = 11, MagicEvade = 11, Resistances = new() { ElementsType.Water, ElementsType.Poison, ElementsType.Sleep, ElementsType.Air } } },

			{ Items.SteelShield, new GearStats() { Armor = 5, MagicArmor = 4, Evade = 6, MagicEvade = 5, SpeedBonus = 5 } },
			{ Items.VenusShield, new GearStats() { Armor = 10, MagicArmor = 11, Evade = 12, MagicEvade = 11, SpeedBonus = 5, Resistances = new() { ElementsType.Paralysis } } },
			{ Items.AegisShield, new GearStats() { Armor = 14,  MagicArmor = 15,Evade = 14, MagicEvade = 15, SpeedBonus = 5, Resistances = new() { ElementsType.Paralysis, ElementsType.Stone } } },

			{ Items.Charm, new GearStats() { Armor = 1, MagicArmor = 2, Evade = 1, MagicEvade = 1, MagicBonus = 5 } },
			{ Items.MagicRing, new GearStats() { Armor = 3,  MagicArmor = 4, Evade = 3, MagicEvade = 4, MagicBonus = 5, Resistances = new() { ElementsType.Silence } } },
			{ Items.CupidLocket, new GearStats() { Armor = 6,  MagicArmor = 7, Evade = 6, MagicEvade = 6, MagicBonus = 5, Resistances = new() { ElementsType.Silence, ElementsType.Blind, ElementsType.Confusion } } },

			{ Items.BowOfGrace, new GearStats() { SpeedBonus = 5 } },
			{ Items.NinjaStar, new GearStats() { SpeedBonus = 5 } },
			{ Items.MorningStar, new GearStats() },

			{ Items.ReplicaArmor, new GearStats() { Armor = 15, MagicArmor = 14, Evade = 15, MagicEvade = 15, Resistances = new() { ElementsType.Water, ElementsType.Stone } } },
			{ Items.MysticRobes, new GearStats() { Armor = 13,  MagicArmor = 15, Evade = 12, MagicEvade = 15, Resistances = new() { ElementsType.Water, ElementsType.Air } } },
			{ Items.FlameArmor, new GearStats() { Armor = 14,  MagicArmor = 12, Evade = 14, MagicEvade = 12, Resistances = new() { ElementsType.Fire } } },
			{ Items.BlackRobe, new GearStats() { Armor = 13,  MagicArmor = 12, Evade = 15, MagicEvade = 14, SpeedBonus = 5, Resistances = new() { ElementsType.Doom } } },

			{ Items.EtherShield, new GearStats() { Armor = 12,  MagicArmor = 12, Evade = 12, MagicEvade = 13, SpeedBonus = 5, Resistances = new() { ElementsType.Paralysis, ElementsType.Sleep, ElementsType.Zombie } } },
		};
	}
}
