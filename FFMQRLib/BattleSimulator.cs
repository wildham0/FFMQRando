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
	enum BattleActionType
	{ 
		Damage,
		Status,
		DamageStats,
		Heal,
		Cure,
		Multiply,
	}

	public struct GearStats
	{
		public int Strength;
		public int Agility;
		public int Speed;
		public int Magic;
		public int Defense;
		public int Evade;
		public int MagicDef;
		public int MagicEvasion;
		public int Acccuracy;
		public List<ElementsType> Resistances;

		public GearStats()
		{
			Strength = 0;
			Agility = 0;
			Speed = 0;
			Magic = 0;
			Defense = 0;
			Evade = 0;
			Acccuracy = 0;
			Resistances = new();
		}
	}

	public class GearData
	{
		
	}

	public enum Teams
	{ 
		TeamA,
		TeamB,
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
		private MT19337 Rng;
		public bool IsPlayer;
		public bool IsDefending;
		public bool IsUndead;
		public bool IsBoss;
		public Teams Team;

		public Entity(MT19337 rng)
		{
			InitBen();
			Rng = rng;
		}
		private void InitBen()
		{
			Actions = new();
			Actions.Add(new WpSteelSword());


			MaxHp = 40;
			Hp = 40;
			Level = 1;
			coreStrength = 10;
			coreAgility = 12;
			coreSpeed = 8;
			coreMagic = 15;
			//Accuracy = Level / 2 + 0x4B;
			Gears = new() { Items.SteelArmor, Items.SteelSword };
			ProcessGears();
			RestoreStats();

			IsDefending = false;
			IsPlayer = true;
			IsUndead = false;
			IsBoss = false;
			Team = Teams.TeamA;
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
			}
			else
			{
				Hp = Math.Max(Hp - Math.Max(damage - Math.Min(0xFF, defense), 1), 0);
			}

			if (Hp == 0)
			{
				Ailments.Add(ElementsType.Doom);
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

	public enum HitRoutines
	{ 
		Plain,
		Punch,
		Sword,
		Axe,
		Claw,
		Bomb,
		Projectile,
		Magic,
		Speed,
		Strength,
		StrengthSpeed,
		Base,
		Hit100,
		Hit90,
	}
	public enum ActionRoutines
	{
		None = 0x00,
		Punch,
		Sword,
		Axe,
		Claw,
		Bomb,
		Projectile,
		MagicDamage1,
		MagicDamage2,
		MagicDamage3,
		MagicUnknown1,
		MagicUnknown2,
		Life,
		Heal,
		MagicUnknown3,
		PhysicalDamage1,
		PhysicalDamage2,
		PhysicalDamage3 = 0x10,
		PhysicalDamage4,
		PhysicalDamage5,
		Ailments1,
		PhysicalDamage6,
		PhysicalDamage7,
		PhysicalDamage8,
		PhysicalDamage9,
		PhysicalUnknown1,
		SelfDestruct,
		Multiply,
		Seed,
		PureDamage1,
		PureDamage2,
		Unknown1,
		Unknown2
	}
	public class BattleAction
	{
		protected int Power;
		//protected int CritRate;
		protected int Accuracy;
		protected int NumberOfHits;
		protected List<ElementsType> Ailments;
		protected List<ElementsType> TypeDamage;
		protected HitRoutines HitRoutine;
		protected ActionRoutines ActionRoutine;
		protected int Id;
		protected bool CanCrit = true;

		private static Dictionary<ActionRoutines, HitRoutines> actionToHitRoutines = new()
		{
			{ ActionRoutines.None, HitRoutines.Plain },
			{ ActionRoutines.Punch, HitRoutines.Punch },
			{ ActionRoutines.Sword, HitRoutines.Sword },
			{ ActionRoutines.Axe, HitRoutines.Axe },
			{ ActionRoutines.Claw, HitRoutines.Claw },
			{ ActionRoutines.Bomb, HitRoutines.Bomb },
			{ ActionRoutines.Projectile, HitRoutines.Projectile },
			{ ActionRoutines.MagicDamage1, HitRoutines.Magic },
			{ ActionRoutines.MagicDamage2, HitRoutines.Magic },
			{ ActionRoutines.MagicDamage3, HitRoutines.Magic },
			{ ActionRoutines.MagicUnknown1, HitRoutines.Magic },
			{ ActionRoutines.MagicUnknown2, HitRoutines.Magic },
			{ ActionRoutines.Life, HitRoutines.Magic },
			{ ActionRoutines.Heal, HitRoutines.Magic },
			{ ActionRoutines.MagicUnknown3, HitRoutines.Magic },
			{ ActionRoutines.PhysicalDamage1, HitRoutines.Speed },
			{ ActionRoutines.PhysicalDamage2, HitRoutines.Speed },
			{ ActionRoutines.PhysicalDamage3, HitRoutines.Strength },
			{ ActionRoutines.PhysicalDamage4, HitRoutines.Strength },
			{ ActionRoutines.Ailments1, HitRoutines.Strength },
			{ ActionRoutines.PhysicalDamage5, HitRoutines.Strength },
			{ ActionRoutines.PhysicalDamage6, HitRoutines.StrengthSpeed },
			{ ActionRoutines.PhysicalDamage7, HitRoutines.Base },
			{ ActionRoutines.PhysicalDamage8, HitRoutines.Magic },
			{ ActionRoutines.PhysicalDamage9, HitRoutines.Plain },
			{ ActionRoutines.SelfDestruct, HitRoutines.Hit100 },
			{ ActionRoutines.Multiply, HitRoutines.Hit90 },
			{ ActionRoutines.Seed, HitRoutines.Plain },
			{ ActionRoutines.PureDamage1, HitRoutines.Plain },
			{ ActionRoutines.PureDamage2, HitRoutines.Plain },
			{ ActionRoutines.Unknown1, HitRoutines.Plain },
			{ ActionRoutines.Unknown2, HitRoutines.Plain },
		};
		public BattleAction() { }
		//public virtual void Execute(Entity user, List<Entity> targets, MT19337 rng) { }

		public void Execute(Entity user, List<Entity> targets, MT19337 rng)
		{
			//(int, int) result = CalcHitRate(user);
			//int hitrate = result.Item1;
			//int critrate = result.Item2;

			var damage = CalcDamage(user);
			var targetcount = targets.Count;

			foreach (var target in targets)
			{
				(int, int) result = CalcHitRate(user, target);
				int hitrate = result.Item1;
				int critrate = result.Item2;


				for (int i = 0; i < NumberOfHits; i++)
				{
					if (rng.Between(0, 100) < hitrate)
					{
						if (rng.Between(0, 100) < critrate)
						{
							damage *= 2;
						}

						damage = Math.Min(target.Hp, Math.Max(1, damage - target.Defense));
						target.Hp = target.Hp - damage;
						if (target.Hp == 0)
						{
							target.Ailments.Add(ElementsType.Doom);
						}

						ApplyAilments(target);
					}
				}
			}
		}
		private (int, int) CalcHitRate(Entity user, Entity target)
		{
			int targetPlayerPhysicalHit = 100 - target.Evade;
			int targetPlayerMagicalHit = 100 - target.MagicEvasion;
			int hitrate = 0;
			int critrate = 0;

			var hitroutine = actionToHitRoutines[ActionRoutine];

			switch (hitroutine)
			{
				case HitRoutines.Punch:
					hitrate = user.Accuracy;
					critrate = 5;
					break;
				case HitRoutines.Sword:
					hitrate = (((user.Strength + user.Speed) / 8) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
					critrate = 8;
					break;
				case HitRoutines.Axe:
					hitrate = ((user.Strength / 4) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
					critrate = 2;
					break;
				case HitRoutines.Claw:
					hitrate = ((user.Speed / 4) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
					critrate = 10;
					break;
				case HitRoutines.Bomb:
					hitrate = Accuracy;
					critrate = 0;
					break;
				case HitRoutines.Projectile:
					hitrate = ((user.Speed / 4) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
					critrate = 14;
					break;
				case HitRoutines.Magic:
					hitrate = ((user.Magic / 4) + 0x4B + Accuracy) / 2;
					critrate = 0;
					break;
				case HitRoutines.Speed:
					hitrate = ((user.Speed / 4) + 0x4B + Accuracy) / 2;
					critrate = 5;
					break;
				case HitRoutines.Strength:
					hitrate = ((user.Strength / 4) + 0x4B + Accuracy) / 2;
					critrate = 5;
					break;
				case HitRoutines.StrengthSpeed:
					hitrate = (((user.Strength + user.Speed) / 8) + 0x4B + Accuracy + user.Accuracy) / 2;
					critrate = 0;
					break;
				case HitRoutines.Base:
					hitrate = user.Accuracy;
					critrate = 0;
					break;
				case HitRoutines.Hit90:
					hitrate = 90;
					critrate = 0;
					break;
				case HitRoutines.Hit100:
					hitrate = 100;
					critrate = 0;
					break;
			}

			// Cap at 100
			hitrate = Math.Min(100, hitrate);

			// Attacker is blind
			if (user.Ailments.Contains(ElementsType.Blind))
			{
				hitrate /= 2;
			}

			// If the target is a player character, then hit rate is based on evasion
			if (target.IsPlayer)
			{
				// there's a special status check here, not sure what it is

				// if attacker is higher level, we skip player evade
				if (user.Level < target.Level)
				{
					if (HitRoutine == HitRoutines.Magic)
					{
						hitrate = 100 - target.MagicEvasion;
					}
					else
					{
						hitrate = 100 - target.Evade;
					}
				}
			}

			return (hitrate, critrate);
		}
		public int CalcPhysicalDefense(Entity target)
		{
			return target.IsDefending ? Math.Max(target.Agility * 2, 0xFA) : target.Agility;
		}
		public int CalcMagicDefense(Entity target)
		{
			return target.Magic + target.MagicDef;
		}
		public int CalcDamageStrSpdx2(Entity user)
		{
			return (user.Strength + user.Speed) * 2;
		}
		public int CalcDamageStrSpdPowx2(Entity user)
		{
			return (user.Strength + user.Speed + Power) * 2;
		}
		public int CalcDamagePowx12()
		{
			return Power * 12;
		}
		public int CalcDamageSpd2Powx12(Entity user)
		{
			return (user.Speed * 2 + Power) * 2;
		}
		public int CalcDamageMagPowx3(Entity user)
		{
			return (user.Magic + Power) * 3;
		}
		public int CalcDamageMagPowx9(Entity user)
		{
			return (user.Magic + Power) * 9;
		}
		public int CalcDamageStrAttHp8x15(Entity user)
		{
			return (int)((user.Strength + Power + (user.MaxHp / (user.IsBoss ? 80 : 8))) * 1.5);
		}
		public int CalcDamageStrAttHp16x2(Entity user)
		{
			return (int)((user.Strength + Power + (user.MaxHp / (user.IsBoss ? 160 : 16))) * 2);
		}
		public int CalcDamageStrAttHp8x075(Entity user)
		{
			return (int)((user.Strength + Power + (user.MaxHp / (user.IsBoss ? 80 : 8))) * 0.75);
		}
		public int CalcDamageStrSpdPow(Entity user)
		{
			return user.Strength + Power + user.Speed;
		}
		public int CalcResistance(Entity target, int damage)
		{
			if (target.Resistances.Intersect(TypeDamage).Any())
			{
				if (TypeDamage.Contains(ElementsType.Zombie))
				{
					damage = -damage;
				}
				else
				{
					damage /= 2;
				}
			}

			return damage;
		}
		public int CalcWeakness(Entity target, int damage)
		{
			bool thunderWeakness = target.Weaknesses.Contains(ElementsType.Water) && target.Weaknesses.Contains(ElementsType.Air);
			bool thunderAttack = TypeDamage.Contains(ElementsType.Water) && TypeDamage.Contains(ElementsType.Water);

			if (target.Weaknesses.Intersect(TypeDamage).Count() == 1 || (thunderWeakness && thunderAttack))
			{
				damage *= 2;
			}

			return damage;
		}
		public int CalcCrit(Entity target, int damage, int critrate, MT19337 rng)
		{
			if (rng.Between(0, 100) < critrate)
			{
				damage *= 2;
			}

			return damage;
		}
		public void ApplyAilments(Entity user, Entity target, int hitrate, MT19337 rng)
		{
			if (Ailments.Any() && rng.Between(0, 100) < hitrate)
			{
				if (!user.Ailments.Contains(ElementsType.Confusion) && (user.Team == target.Team))
				{
					target.Ailments = target.Ailments.Except(Ailments).ToList();
				}
				else
				{
					foreach (var ailment in Ailments)
					{
						if (!target.Resistances.Contains(ailment))
						{
							target.Ailments.Add(ailment);
						}
					}
				}
				// There's a bug where applying stone here, if resisted, continue to remove all ailments, we're ignoring it here			
			}
		}


		public void ActionNone(Entity user, List<Entity> targets, MT19337 rng)
		{
			return;
		}
		public void ActionPunch(Entity user, Entity target, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);

			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageStrSpdx2(user);
			damage = CalcResistance(target, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
			}
		}
		public void WeaponAttack(Entity user, Entity target, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageStrSpdPowx2(user);
			damage = CalcResistance(target, damage);
			damage = CalcWeakness(target, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
				ApplyAilments(user, target, hitrate.hit, rng);
			}
		}
		public void BombAttack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			// should check if we have ammo, we don't care
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamagePowx12();
			damage /= targetcount;
			damage = CalcWeakness(target, damage);
			target.ProcessDamage(damage, defense);
			// also weird check if Reflectant
		}
		public void ProjectileAttack(Entity user, Entity target, MT19337 rng)
		{
			// should check if we have ammo, we don't care
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageSpd2Powx12(user);
			damage = CalcResistance(target, damage);
			damage = CalcWeakness(target, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
				ApplyAilments(user, target, hitrate.hit, rng);
			}
		}
		public void OffensiveMagic3Attack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcMagicDefense(target);
			int damage = CalcDamageMagPowx3(user);
			damage /= targetcount;
			damage = CalcResistance(target, damage);
			damage = CalcWeakness(target, damage);
			target.ProcessDamage(damage, defense);
			// There's some weird conditional after, skip it
		}
		public void OffensiveMagic9Attack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcMagicDefense(target);
			int damage = CalcDamageMagPowx9(user);
			damage /= targetcount;
			damage = CalcResistance(target, damage);
			damage = CalcWeakness(target, damage);
			target.ProcessDamage(damage, defense);
			// There's some weird conditional after, skip it
		}
		public void StatsDebuffAttack(Entity user, Entity target, MT19337 rng)
		{
			// check if we're using refresher??
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = 0x1E * (Power & 0x0F);
			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
				int statid = ((Power & 0xE0) / 32) & 0x03;
				int statdebuff = ((Power / 16) & 0x03) * 0x19;

				switch (statid)
				{
					case 0:
						target.Strength = Math.Max(0, target.Strength - statdebuff);
						break;
					case 1:
						target.Agility = Math.Max(0, target.Agility - statdebuff);
						break;
					case 2:
						target.Speed = Math.Max(0, target.Speed - statdebuff);
						break;
					case 3:
						target.Magic = Math.Max(0, target.Magic - statdebuff);
						break;
				}
			}
		}

		public void CastLife(Entity user, Entity target, MT19337 rng)
		{
			if (target.Team != user.Team)
			{
				if (target.IsUndead && target.Level < user.Level)
				{
					(int hit, int crit) hitrate = CalcHitRate(user, target);

					if (rng.Between(0, 100) < hitrate.hit)
					{
						target.Hp = 0;
						target.Ailments.Add(ElementsType.Doom);
					}
				}
			}
			else
			{
				target.Ailments = new();
				target.Hp = target.MaxHp;
			}
		}
		public void CastHeal(Entity user, Entity target, MT19337 rng)
		{
			if (target.Team != user.Team || user.Ailments.Contains(ElementsType.Confusion))
			{
				(int hit, int crit) hitrate = CalcHitRate(user, target);
				
				if (!user.IsPlayer)
				{
					hitrate = (hitrate.hit / 2, hitrate.crit);
				}

				if (rng.Between(0, 100) < hitrate.hit)
				{
					ApplyAilments(target);
				}
			}
			else
			{
				if (target.Ailments.Contains(ElementsType.Doom))
				{
					target.Ailments = new() { ElementsType.Doom };
				}
				else
				{
					target.Ailments = new();
				}
			}
		}
		public void CastCure(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			if (target.Team != user.Team || target.IsUndead)
			{
				(int hit, int crit) hitrate = CalcHitRate(user, target);
				int defense = CalcMagicDefense(target);
				int damage = CalcDamageMagPowx3(user);
				damage /= targetcount;
				damage = CalcWeakness(target, damage);

				if (rng.Between(0, 100) < hitrate.hit)
				{
					target.ProcessDamage(damage, defense);
				}
			}
			else
			{
				int healing =((((int)(user.Magic * 1.5) + Power) * target.MaxHp) / 100) / targetcount;
				target.ProcessDamage(-healing, 0);
			}
		}
		public void PhysicalAttack01(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = 1;
			if (ActionRoutine == ActionRoutines.PhysicalDamage1)
			{
				damage = CalcDamageStrAttHp16x2(user);
			}
			else
			{
				damage = CalcDamageStrAttHp8x15(user);
			}
			damage = CalcResistance(target, damage);
			damage /= targetcount;
			target.ProcessDamage(damage, defense);

			if(Id == 0x8A) //if snowstorm
			{
				user.ProcessDamage(-200, 0);
			}
		}
		public void PhysicalAttack02(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			PhysicalAttack01(user, target, targetcount, rng);
		}
		public void PhysicalAttack03(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageStrAttHp8x075(user);

			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);

				hitrate = (hitrate.hit / 2, hitrate.crit);

				if (rng.Between(0, 100) < hitrate.hit)
				{
					ApplyAilments(target);
				}
			}
		}
		public void PhysicalAttack04(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);

			if (ActionRoutine != ActionRoutines.PhysicalDamage7)
			{
				int defense = CalcPhysicalDefense(target);
				int damage = CalcDamageStrAttHp8x075(user);
				damage /= targetcount;
				target.ProcessDamage(damage, defense);
				ApplyAilments(target);
			}
			else
			{
				hitrate = (hitrate.hit / 2, hitrate.crit);
				if (rng.Between(0, 100) < hitrate.hit)
				{
					ApplyAilments(target);
				}
			}
		}
		public void PhysicalAttackMultiHits(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int successfulhits = 0;
			for (int i = 0; i < NumberOfHits; i++)
			{
				if (rng.Between(0, 100) < hitrate.hit)
				{
					successfulhits++;
				}
			}

			int defense = CalcPhysicalDefense(target);
			int damage = 0;

			if (ActionRoutine < ActionRoutines.PhysicalDamage9)
			{
				damage = CalcDamageStrSpdPow(user);
			}
			else
			{
				damage = CalcDamageStrAttHp8x075(user);
			}

			damage = damage * successfulhits;

			if (ActionRoutine != ActionRoutines.PhysicalDamage8)
			{
				damage /= 2;
			}

			CalcResistance(target, damage);
			target.ProcessDamage(damage, defense);
		}
		public virtual int CalcHitRate(Entity user) { return 0; }
		//public virtual int CalcDamage(Entity user) { return 0; }
		public void ApplyAilments(Entity target)
		{
			foreach (var ailment in Ailments)
			{
				if (!target.Resistances.Contains(ailment))
				{
					target.Ailments.Add(ailment);
				}
			}
		}
	}

	public class WpSteelSword : BattleAction
	{
		public WpSteelSword()
		{
			Power = 10;
			CritRate = 8;
			Accuracy = 10;
			Ailments = new();
			TypeDamage = new();
		}
		public override int CalcHitRate(Entity user)
		{ 
			return (((user.Strength + user.Speed) / 8) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
		}
		public override int CalcDamage(Entity user)
		{
			return (user.Strength + user.Speed + Power) * 2;
		}
	}

	public class Battle
	{
		List<Entity> Enemies;
		List<Entity> Heroes;

		private void InitTeams()
		{ 
			
		
		
		}
	
	
	}

}
