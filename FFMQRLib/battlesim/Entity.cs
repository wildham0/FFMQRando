using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;



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
		public int Strength;
		public int Agility;
		public int Speed;
		public int Magic;
		public int Accuracy => Level / 2 + 0x4B;
		public int Evade;
		public int MagicDef;
		public int MagicEvasion;
		public int Hp;
		public int MaxHp;
		public int Defense;
		public string Name;
		private int bonusStrength;
		private int bonusAgility;
		private int bonusSpeed;
		private int bonusMagic;
		private int coreStrength;
		private int coreAgility;
		private int coreSpeed;
		private int coreMagic;
		public List<ElementsType> Resistances;
		public List<ElementsType> Weaknesses;
		public List<ElementsType> Ailments;
		public List<BattleAction> Actions;
		public List<Items> Gears;
		private List<int> weightedScript;
		private MT19337 Rng;
		public bool IsPlayer;
		public bool IsDefending;
		public bool IsUndead;
		public bool IsBoss;
		public bool HydraMode;
		public PazuzuModes PazuzuMode;
		public int PazuzuRound;
		public DkModes DkMode;
		public Teams Team;
		public int Initiative;
		public AiProfiles AiProfile;
		public Logger Log;

		public Entity(Logger log, MT19337 rng)
		{
			Rng = rng;
			Log = log;
		}
		public void InitBen(PowerLevels power, Teams team)
		{
			Actions = new();
			Name = "Benjamin";
			Items startingWeapon = Items.SteelSword;

			if (power == PowerLevels.Initial)
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
				Actions.AddRange(new List<BattleAction>() { Battle.BattleActions[(int)Items.CureBook], Battle.BattleActions[(int)Items.LifeBook], Battle.BattleActions[(int)Items.FlareSeal], Battle.BattleActions[(int)Items.AeroBook] });
			}

			Actions.Add(Battle.BattleActions[(int)startingWeapon]);
			
			MaxHp = 40 * Level;
			Hp = MaxHp;

			coreStrength = 4 + 3 * Level;
			coreAgility = 4 + 2 * Level;
			coreSpeed = 6 + 2 * Level;
			coreMagic = 9 + Level;

			Resistances = new();
			Weaknesses = new();
			Ailments = new();

			ProcessGears();
			RestoreStats();

			IsDefending = false;
			IsPlayer = true;
			IsUndead = false;
			IsBoss = false;
			Team = team;
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
		public void InitCompanion(PowerLevels power, Teams team)
		{
			Actions = new();
			Companions companions = new(LevelingType.QuestsExtended);

			List<CompanionsId> validcompanions = new() { CompanionsId.Kaeli, CompanionsId.Reuben, CompanionsId.Tristam, CompanionsId.Phoebe };
			var currentcompanion = Rng.PickFrom(validcompanions);
			var companion = companions[currentcompanion];


			Name = Enum.GetName(typeof(CompanionsId), currentcompanion);
			if (power == PowerLevels.Initial)
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
				Level = 31;
				Gears = companion.ArmorSet2.Select(a => armorFlagsToItems[a]).Append(companion.Weapon).ToList();
			}

			Actions.Add(Battle.BattleActions[(int)companion.Weapon]);
			//Actions.AddRange(companion.LoadOut)

			MaxHp = companion.HPBase + 40 * Level;
			Hp = MaxHp;

			coreStrength = companion.StrBase + companion.StrMultiplier * Level;
			coreAgility = companion.ConBase + companion.ConMultiplier * Level;
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
			Team = team;
			weightedScript = new();
			AiProfile = AiProfiles.Player;
		}
		public void InitBehemoth(Teams team)
		{
			Actions = new() { Battle.BattleActions[0x7C] };

			Name = "Behemoth";

			MaxHp = 0x50;
			Hp = 0x50;
			Level = 1;
			coreStrength = 0x01;
			coreAgility = 0x19;
			coreSpeed = 0x0C;
			coreMagic = 0x01;
			MagicDef = 0x01;
			MagicEvasion = 0x01;
			Evade = 0x01;

			Resistances = new();
			Weaknesses = new();
			Ailments = new();

			RestoreStats();

			IsDefending = false;
			IsPlayer = false;
			IsUndead = false;
			IsBoss = false;
			weightedScript = new() { 0x2D, 0x23, 0x0A, 0x0A, 0x00, 0x00 };
			AiProfile = AiProfiles.Enemy;

			Team = team;
		}

		public void RollInitiative(MT19337 rng)
		{
			Initiative = Speed + rng.Between(0, Speed / 2);
		}
		public void DoRound(List<Entity> battlers, MT19337 rng)
		{
			switch (AiProfile)
			{
				case AiProfiles.Player:
					PlayerAi(battlers, rng);
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
			}
		}
		private void EnemyAi(List<Entity> battlers, MT19337 rng)
		{
			bool foundaction = false;
			int actionpos = 0;

			while (!foundaction)
			{
				int roll = rng.Between(0, 100);
				for (int i = 0; i < 6; i++)
				{
					if (roll < weightedScript[i])
					{
						if (i < Actions.Count)
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

			List<Entity> validTargets = battlers.Where(b => b.Team != Team).ToList();
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

				if (PazuzuMode > PazuzuModes.DeactivteShield)
				{
					PazuzuMode = PazuzuModes.AttackingShieldOff;
				}
			}

			EnemyAi(battlers, rng);
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
				var criticalAllies = validAllyTargets.Where(t => t.Hp < t.MaxHp / 2).ToList();

				BattleAction lifeaction;
				BattleAction healaction;
				BattleAction cureaction;

				bool haslife = Actions.TryFind(a => a.Id == 0x17, out lifeaction);
				bool hasheal = Actions.TryFind(a => a.Id == 0x16, out healaction);
				bool hascure = Actions.TryFind(a => a.Id == 0x15, out cureaction);

				if (!tookaction && deadAllies.Any() && haslife)
				{
					lifeaction.Execute(this, deadAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
					tookaction = true;
				}
				else if (!tookaction && statusAllies.Any() && (haslife || hasheal))
				{
					if (haslife)
					{
						lifeaction.Execute(this, statusAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
						tookaction = true;
					}
					else
					{
						healaction.Execute(this, statusAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
						tookaction = true;
					}
				}
				else if (!tookaction && criticalAllies.Any() && (haslife || hascure))
				{
					if (criticalAllies.Count == 1)
					{
						if (haslife)
						{
							lifeaction.Execute(this, criticalAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
						else
						{
							cureaction.Execute(this, criticalAllies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
					}
					else
					{
						if (hascure)
						{
							cureaction.Execute(this, criticalAllies, TargetSelections.PrioritizeMultipleTarget, Log, rng);
							tookaction = true;

						}
						else
						{
							lifeaction.Execute(this, criticalAllies.OrderBy(a => a.Hp).ToList().GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, rng);
							tookaction = true;
						}
					}
				}

				List<int> validAttackId = Enumerable.Range(0x18, 0x18).ToList();

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
						rng.PickFrom(validActions).Execute(this, validEnemyTargets, TargetSelections.RandomTargeting, Log, rng);
					}
					else
					{
						List<ElementsType> weaknesses = validEnemyTargets.SelectMany(e => e.Weaknesses).ToList();
						List<BattleAction> validActions = Actions.Where(a => validAttackId.Contains(a.Id)).ToList();
						validActions = validActions.OrderByDescending(a => a.Power).ToList();
						validActions = validActions.OrderByDescending(a => a.TypeDamage.Intersect(weaknesses).Any()).ToList();

						validActions.First().Execute(this, validEnemyTargets, TargetSelections.PrioritizeMultipleTarget, Log, rng);
					}
				}
			}









		}

		private void ProcessGears()
		{
			foreach (var gear in Gears)
			{
				bonusStrength += geardata[gear].Strength;
				bonusAgility += geardata[gear].Agility;
				bonusSpeed += geardata[gear].Speed;
				bonusMagic += geardata[gear].Magic;
				Defense += geardata[gear].Defense;
				Evade += geardata[gear].Evade;
				MagicDef += geardata[gear].MagicDef;
				MagicEvasion += geardata[gear].MagicEvasion;
				Resistances.AddRange(geardata[gear].Resistances);
			}
		}
		public void RestoreStats()
		{
			Agility = coreAgility + bonusAgility + Defense;
			Strength = coreStrength + bonusStrength;
			Speed = coreSpeed + bonusSpeed;
			Magic = coreMagic + bonusMagic;
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
			{ Items.KnightSword, new GearStats() { Speed = 5 } },
			{ Items.Excalibur, new GearStats() { Speed = 5 } },
			{ Items.Axe, new GearStats() { } },
			{ Items.BattleAxe, new GearStats() { } },
			{ Items.GiantsAxe, new GearStats() { } },
			{ Items.CatClaw, new GearStats() { Magic = 5 } },
			{ Items.CharmClaw, new GearStats() { Magic = 5 } },
			{ Items.DragonClaw, new GearStats() { Magic = 5 } },
			{ Items.Bomb, new GearStats() { } },
			{ Items.JumboBomb, new GearStats() { } },
			{ Items.MegaGrenade, new GearStats() { } },

			{ Items.SteelHelm, new GearStats() { Defense = 4, MagicDef = 4, Evade = 5, MagicEvasion = 5, Strength = 5 } },
			{ Items.MoonHelm, new GearStats() { Defense = 9, MagicDef = 9, Evade = 9, MagicEvasion = 9, Strength = 5, Resistances = new() { ElementsType.Fire } } },
			{ Items.ApolloHelm, new GearStats() { Defense = 15, MagicDef = 14, Evade = 15, MagicEvasion = 14, Strength = 5, Resistances = new() { ElementsType.Fire } } },

			{ Items.SteelArmor, new GearStats() { Defense = 6, MagicDef = 6, Evade = 4, MagicEvasion = 5 } },
			{ Items.NobleArmor, new GearStats() { Defense = 12, MagicDef = 10, Evade = 10, MagicEvasion = 10, Resistances = new() { ElementsType.Water, ElementsType.Poison } } },
			{ Items.GaiasArmor, new GearStats() { Defense = 15, MagicDef = 12, Evade = 11, MagicEvasion = 11, Resistances = new() { ElementsType.Water, ElementsType.Poison, ElementsType.Sleep, ElementsType.Air } } },

			{ Items.SteelShield, new GearStats() { Defense = 5, MagicDef = 4, Evade = 6, MagicEvasion = 5, Speed = 5 } },
			{ Items.VenusShield, new GearStats() { Defense = 10, MagicDef = 11, Evade = 12, MagicEvasion = 11, Speed = 5, Resistances = new() { ElementsType.Paralysis } } },
			{ Items.AegisShield, new GearStats() { Defense = 14,  MagicDef = 15,Evade = 14, MagicEvasion = 15, Speed = 5, Resistances = new() { ElementsType.Paralysis, ElementsType.Stone } } },

			{ Items.Charm, new GearStats() { Defense = 1, MagicDef = 2, Evade = 1, MagicEvasion = 1, Magic = 5 } },
			{ Items.MagicRing, new GearStats() { Defense = 3,  MagicDef = 4, Evade = 3, MagicEvasion = 4, Magic = 5, Resistances = new() { ElementsType.Silence } } },
			{ Items.CupidLocket, new GearStats() { Defense = 6,  MagicDef = 7, Evade = 6, MagicEvasion = 6, Magic = 5, Resistances = new() { ElementsType.Silence, ElementsType.Blind, ElementsType.Confusion } } },

			{ Items.BowOfGrace, new GearStats() { Speed = 5 } },
			{ Items.NinjaStar, new GearStats() { Speed = 5 } },

			{ Items.ReplicaArmor, new GearStats() { Defense = 15, MagicDef = 14, Evade = 15, MagicEvasion = 15, Resistances = new() { ElementsType.Water, ElementsType.Stone } } },
			{ Items.MysticRobes, new GearStats() { Defense = 13,  MagicDef = 15, Evade = 12, MagicEvasion = 15, Resistances = new() { ElementsType.Water, ElementsType.Air } } },
			{ Items.FlameArmor, new GearStats() { Defense = 14,  MagicDef = 12, Evade = 14, MagicEvasion = 12, Resistances = new() { ElementsType.Fire } } },
			{ Items.BlackRobe, new GearStats() { Defense = 13,  MagicDef = 12, Evade = 15, MagicEvasion = 14, Speed = 5, Resistances = new() { ElementsType.Doom } } },

			{ Items.EtherShield, new GearStats() { Defense = 12,  MagicDef = 12, Evade = 12, MagicEvasion = 13, Speed = 5, Resistances = new() { ElementsType.Paralysis, ElementsType.Sleep, ElementsType.Zombie } } },
		};
	}
}
