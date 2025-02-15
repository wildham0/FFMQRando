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
	public class StoredAction
	{
		public BattleAction Action;
		public Entity User;
		public List<Entity> Targets;
		public TargetSelections TargetSelection;
		public Logger Log;
		public MT19337 Rng;
		private bool Cancel;
		private bool PickNew;

		public StoredAction(BattleAction action, Entity user, List<Entity> targets, TargetSelections targetPreference, Logger log, MT19337 rng)
		{
			Action = action;
			User = user;
			Targets = targets;
			TargetSelection = targetPreference;
			Log = log;
			Rng = rng;
			Cancel = false;
			PickNew = false;
		}
		public void Execute(List<Entity> potentialTargets)
		{
			if (!Cancel)
			{
				if (PickNew)
				{
					Targets = new() { Rng.PickFrom(potentialTargets) };
				}

				Action.Execute(User, Targets, TargetSelection, Log, Rng);
			}
			else
			{
				Log.Add(User.Name + " do nothing.");
			}
		}
		private void UpdateTargets()
		{
			if (Targets.First().Team == User.Team)
			{
				if (Action.Id == (int)Items.HealBook)
				{
					Targets = Targets.Where(t => !t.Ailments.Contains(ElementsType.Doom)).ToList();
				}
				else if (Action.Id == (int)Items.CureBook)
				{
					Targets = Targets.Where(t => !t.Ailments.Contains(ElementsType.Stone) && !t.Ailments.Contains(ElementsType.Doom)).ToList();
				}

				if (!Targets.Any())
				{
					Cancel = true;
				}
			}
			else
			{
				Targets = Targets.Where(t => !t.Ailments.Contains(ElementsType.Stone) && !t.Ailments.Contains(ElementsType.Doom)).ToList();

				if (!Targets.Any())
				{
					PickNew = true;
				}
			}
		}
	}
	public class BattleAction
	{
		public int Power;
		protected int Accuracy;
		protected int NumberOfHits;
		public List<ElementsType> Ailments;
		public List<ElementsType> TypeDamage;
		protected HitRoutines HitRoutine;
		private ActionRoutines ActionRoutine;
		protected Action battleAction;
		public int Id;
		public string Name;
		protected bool CanCrit = true;
		public Targetings Targeting;
		private Logger Log;
		private bool simulationMode = false;

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
			{ ActionRoutines.MagicStatsDebuff, HitRoutines.Magic },
			{ ActionRoutines.MagicUnknown2, HitRoutines.Magic },
			{ ActionRoutines.Life, HitRoutines.Magic },
			{ ActionRoutines.Heal, HitRoutines.Magic },
			{ ActionRoutines.Cure, HitRoutines.Magic },
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
		public BattleAction(string name, int id, int power, int accuracy, ActionRoutines battleaction, List<ElementsType> type, List<ElementsType> ailments, Targetings targeting, int numberofhits = 1)
		{
			Name = name;
			Id = id;
			Power = power;
			Accuracy = accuracy;
			ActionRoutine = battleaction;
			Ailments = ailments;
			TypeDamage = type;
			NumberOfHits = numberofhits;
			Targeting = targeting;
		}

		public int RelativePower(List<ElementsType> resistances, List<ElementsType> weaknesses)
		{
			int relpower = 0;

			if (ActionRoutine == ActionRoutines.MagicDamage2)
			{
				relpower = Power * 3;
			}
			else if (ActionRoutine == ActionRoutines.MagicDamage3)
			{
				relpower = Power * 9;
			}
			else
			{
				relpower = Power * 3;
			}

			CalcResistance(resistances, relpower);
			CalcWeakness(weaknesses, relpower);

			return relpower;
		}
		public int Rating(Entity user)
		{
			int rating = 0;
			int damage = 0;
			bool applyaailment = false;
			switch (ActionRoutine)
			{
				case ActionRoutines.None:
					break;
				case ActionRoutines.Punch:
					rating = CalcDamageStrSpdx2(user);
					break;
				case ActionRoutines.Sword:
					damage = CalcDamageStrSpdPowx2(user);
					break;
				case ActionRoutines.Axe:
					damage = CalcDamageStrSpdPowx2(user);
					break;
				case ActionRoutines.Claw:
					damage = CalcDamageStrSpdPowx2(user);
					applyaailment = true;
					break;
				case ActionRoutines.Bomb:
					damage = CalcDamagePowx12();
					break;
				case ActionRoutines.Projectile:
					damage = CalcDamageSpd2Powx12(user);
					applyaailment = true;
					break;
				case ActionRoutines.MagicDamage1:
					damage = CalcDamageMagPowx3(user);
					break;
				case ActionRoutines.MagicDamage2:
					damage = CalcDamageMagPowx3(user);
					break;
				case ActionRoutines.MagicDamage3:
					damage = CalcDamageMagPowx9(user);
					break;
				case ActionRoutines.MagicStatsDebuff:
					damage = 0x1E * (Power & 0x0F);
					break;
				case ActionRoutines.MagicUnknown2:
					// Exit
					break;
				case ActionRoutines.Life:
					// Not used
					break;
				case ActionRoutines.Heal:
					rating = 100;
					break;
				case ActionRoutines.Cure:
					rating = 10;
					break;
				case ActionRoutines.PhysicalDamage1:
					damage = CalcDamageStrAttHp16x2(user);
					break;
				case ActionRoutines.PhysicalDamage2:
					damage = CalcDamageStrAttHp8x15(user);
					break;
				case ActionRoutines.PhysicalDamage3:
					damage = CalcDamageStrAttHp8x15(user);
					break;
				case ActionRoutines.PhysicalDamage4:
					damage = CalcDamageStrAttHp8x15(user);
					break;
				case ActionRoutines.Ailments1:
					damage = CalcDamageStrAttHp8x075(user);
					applyaailment = true;
					break;
				case ActionRoutines.PhysicalDamage5:
					damage = CalcDamageStrAttHp8x075(user);
					applyaailment = true;
					break;
				case ActionRoutines.PhysicalDamage6:
					damage = CalcDamageStrAttHp8x075(user);
					applyaailment = true;
					break;
				case ActionRoutines.PhysicalDamage7:
					damage = (CalcDamageStrSpdPow(user) * NumberOfHits) / 2;
					break;
				case ActionRoutines.PhysicalDamage8:
					damage = CalcDamageStrSpdPow(user) * NumberOfHits;
					break;
				case ActionRoutines.PhysicalDamage9:
					damage = CalcDamageStrAttHp8x075(user);
					break;
				case ActionRoutines.SelfDestruct:
					rating = 100;
					break;
				case ActionRoutines.Multiply:
					rating = 100;
					break;
				case ActionRoutines.Seed:
					//
					break;
				case ActionRoutines.PureDamage1:
					damage = Power * 8 * (Targeting == Targetings.MultipleEnemy ? 2 : 1);
					applyaailment = true;
					break;
				case ActionRoutines.PureDamage2:
					damage = Power * 8 * (Targeting == Targetings.MultipleEnemy ? 2 : 1);
					applyaailment = true;
					break;
				case ActionRoutines.Unknown1:
					//
					break;
				case ActionRoutines.Unknown2:
					//
					break;
			}

			if (damage != 0)
			{
				rating = (damage * 100) / 1500;
			}

			List<ElementsType> badAilments = new() { ElementsType.Doom, ElementsType.Paralysis, ElementsType.Stone, ElementsType.Confusion, ElementsType.Sleep };
			//List<ElementsType> okAilments = new() { ElementsType.Sleep };
			if (applyaailment)
			{
				if (Ailments.Intersect(badAilments).Any())
				{
					rating += 20;
				}
			}

			return Math.Min(100, rating);
		}
		public void Execute(Entity user, List<Entity> targets, TargetSelections targetPreference, Logger log, MT19337 rng)
		{
			Log = log;
			targets = ProcessTargeting(targets, targetPreference, rng);

			simulationMode = user.SimulationMode;

			foreach (var target in targets)
			{
				Log.Add(user.Name + " use " + Name + " on " + target.Name + ".");
				switch (ActionRoutine)
				{
					case ActionRoutines.None:
						ActionNone(user, targets, rng);
						break;
					case ActionRoutines.Punch:
						ActionPunch(user, target, rng);
						break;
					case ActionRoutines.Sword:
						WeaponAttack(user, target, rng);
						break;
					case ActionRoutines.Axe:
						WeaponAttack(user, target, rng);
						break;
					case ActionRoutines.Claw:
						WeaponAttack(user, target, rng);
						break;
					case ActionRoutines.Bomb:
						BombAttack(user, target, targets.Count, rng);
						break;
					case ActionRoutines.Projectile:
						ProjectileAttack(user, target, rng);
						break;
					case ActionRoutines.MagicDamage1:
						OffensiveMagic3Attack(user, target, targets.Count, rng);
						break;
					case ActionRoutines.MagicDamage2:
						OffensiveMagic3Attack(user, target, targets.Count, rng);
						break;
					case ActionRoutines.MagicDamage3:
						OffensiveMagic9Attack(user, target, targets.Count, rng);
						break;
					case ActionRoutines.MagicStatsDebuff:
						StatsDebuffAttack(user, target, rng);
						break;
					case ActionRoutines.MagicUnknown2:
						ActionNone(user, targets, rng);
						break;
					case ActionRoutines.Life:
						CastLife(user, target, rng);
						break;
					case ActionRoutines.Heal:
						CastHeal(user, target, rng);
						break;
					case ActionRoutines.Cure:
						CastCure(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage1:
						PhysicalAttack01(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage2:
						PhysicalAttack02(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage3:
						PhysicalAttack01(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage4:
						PhysicalAttack01(user, target, targets.Count, rng);
						break;
					case ActionRoutines.Ailments1:
						PhysicalAttack03(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage5:
						PhysicalAttack04(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage6:
						PhysicalAttack04(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage7:
						PhysicalAttackMultiHits(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage8:
						PhysicalAttackMultiHits(user, target, targets.Count, rng);
						break;
					case ActionRoutines.PhysicalDamage9:
						AttackDrain(user, target, rng);
						break;
					case ActionRoutines.SelfDestruct:
						AttackSelfDestruct(user, target, rng);
						break;
					case ActionRoutines.Multiply:
						AttackMultiply(user, target, rng);
						break;
					case ActionRoutines.Seed:
						UseSeed(user, target, rng);
						break;
					case ActionRoutines.PureDamage1:
						AttackFlatDamage(user, target, rng);
						break;
					case ActionRoutines.PureDamage2:
						AttackFlatDamage(user, target, rng);
						break;
					case ActionRoutines.Unknown1:
						ActionNone(user, targets, rng);
						break;
					case ActionRoutines.Unknown2:
						ActionNone(user, targets, rng);
						break;
				}
			}
		}
		private List<Entity> ProcessTargeting(List<Entity> targets, TargetSelections targetingPreference, MT19337 rng)
		{
			List<Targetings> multipleTargeting = new() { Targetings.MultipleAlly, Targetings.SelectionAlly, Targetings.MultipleEnemy, Targetings.SelectionEnemy, Targetings.MultipleAny, Targetings.SelectionAny };
			List<Targetings> singleTargeting = new() { Targetings.SingleAlly, Targetings.SelectionAlly, Targetings.SingleEnemy, Targetings.SelectionEnemy, Targetings.SingleAny, Targetings.SelectionAny };

			List<TargetSelections> randomSelection = new() { TargetSelections.PrioritizeMultipleTarget, TargetSelections.PrioritizeSingleTarget };
			if (targetingPreference == TargetSelections.RandomTargeting)
			{
				targetingPreference = rng.PickFrom(randomSelection);
			}

			if (targetingPreference == TargetSelections.OverrideMultiple)
			{
				return targets;
			}
			else if (targetingPreference == TargetSelections.PrioritizeMultipleTarget)
			{
				if (multipleTargeting.Contains(Targeting))
				{
					return targets;
				}
				else
				{
					return new() { rng.PickFrom(targets) };
				}
			}
			else
			{
				if (singleTargeting.Contains(Targeting))
				{
					return new() { rng.PickFrom(targets) };
				}
				else
				{
					return targets;
				}
			}
		}
		private (int, int) CalcHitRate(Entity user, Entity target)
		{
			int targetPlayerPhysicalHit = 100 - target.Evade;
			int targetPlayerMagicalHit = 100 - target.MagicEvade;
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
					hitrate = (((user.Attack + user.Speed) / 8) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
					critrate = 8;
					break;
				case HitRoutines.Axe:
					hitrate = ((user.Attack / 4) + 0x4B + Accuracy) / 3 + (user.Accuracy / 3);
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
					hitrate = ((user.Attack / 4) + 0x4B + Accuracy) / 2;
					critrate = 5;
					break;
				case HitRoutines.StrengthSpeed:
					hitrate = (((user.Attack + user.Speed) / 8) + 0x4B + Accuracy + user.Accuracy) / 2;
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
						hitrate = 100 - target.MagicEvade;
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
			return target.IsDefending ? Math.Max(target.Defense * 2, 0xFA) : target.Defense;
		}
		public int CalcMagicDefense(Entity target)
		{
			return target.MagicDefense;
		}
		public int CalcDamageStrSpdx2(Entity user)
		{
			return (user.Attack + user.Speed) * 2;
		}
		public int CalcDamageStrSpdPowx2(Entity user)
		{
			return (user.Attack + user.Speed + Power) * 2;
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
			return (int)((user.Attack + Power + (user.MaxHp / (user.IsBoss ? 80 : 8))) * 1.5);
		}
		public int CalcDamageStrAttHp16x2(Entity user)
		{
			return (int)((user.Attack + Power + (user.MaxHp / (user.IsBoss ? 160 : 16))) * 2);
		}
		public int CalcDamageStrAttHp8x075(Entity user)
		{
			return (int)((user.Attack + Power + (user.MaxHp / (user.IsBoss ? 80 : 8))) * 0.75);
		}
		public int CalcDamageStrSpdPow(Entity user)
		{
			return user.Attack + Power + user.Speed;
		}
		public int CalcResistance(List<ElementsType> resists, int damage)
		{
			if (ElementsMatch(resists))
			{
				if (TypeDamage.Contains(ElementsType.Zombie))
				{
					return -damage;
				}
				else
				{
					return damage / 2;
				}
			}

			return damage;
		}
		public int CalcWeakness(List<ElementsType> weaks, int damage)
		{
			if (ElementsMatch(weaks))
			{
				return damage * 2;
			}

			return damage;
		}
		public bool ElementsMatch(List<ElementsType> elements)
		{
			bool thunderElement = elements.Contains(ElementsType.Water) && elements.Contains(ElementsType.Air);
			bool thunderAttack = TypeDamage.Contains(ElementsType.Water) && TypeDamage.Contains(ElementsType.Water);

			if (thunderAttack)
			{
				if (thunderElement)
				{
					return true;
				}
			}
			else if (elements.Intersect(TypeDamage).Any())
			{
				return true;
			}

			return false;
		}
		public int CalcCrit(Entity target, int damage, int critrate, MT19337 rng)
		{
			if (RollD100(RollType.CritRoll, rng) < critrate)
			{
				damage *= 2;
				Log.Add("Critical Hit!");
			}

			return damage;
		}
		private int RollD100(RollType roll, MT19337 rng)
		{
			if (simulationMode)
			{
				if (roll == RollType.CritRoll)
				{
					return 100;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return rng.Between(0, 100);
			}
		}
		public void ApplyAilments(Entity user, Entity target, int hitrate, MT19337 rng)
		{
			if (Ailments.Any() && RollD100(RollType.AfflictRoll, rng) < hitrate)
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
							Log.Add(user.Name + " inflicted " + Enum.GetName(typeof(ElementsType), ailment) + " to " + target.Name + ".");
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
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
			}
			else
			{
				Log.Add("Miss!");
			}
		}
		public void WeaponAttack(Entity user, Entity target, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageStrSpdPowx2(user);
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcWeakness(target.Weaknesses, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
				ApplyAilments(user, target, hitrate.hit, rng);
			}
			else
			{
				Log.Add("Miss!");
			}
		}
		public void BombAttack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			// should check if we have ammo, we don't care
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamagePowx12();
			damage /= targetcount;
			damage = CalcWeakness(target.Weaknesses, damage);
			target.ProcessDamage(damage, defense);
			// also weird check if Reflectant
		}
		public void ProjectileAttack(Entity user, Entity target, MT19337 rng)
		{
			// should check if we have ammo, we don't care
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageSpd2Powx12(user);
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcWeakness(target.Weaknesses, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);

			if (rng.Between(0, 100) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);
				ApplyAilments(user, target, 100, rng);
			}
			else
			{
				Log.Add("Miss!");
			}

		}
		public void OffensiveMagic3Attack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcMagicDefense(target);
			int damage = CalcDamageMagPowx3(user);
			damage /= targetcount;
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);
			damage = CalcWeakness(target.Weaknesses, damage);
			target.ProcessDamage(damage, defense);
			// There's some weird conditional after, skip it
		}
		public void OffensiveMagic9Attack(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcMagicDefense(target);
			int damage = CalcDamageMagPowx9(user);
			damage /= targetcount;
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);
			damage = CalcWeakness(target.Weaknesses, damage);
			target.ProcessDamage(damage, defense);
			// There's some weird conditional after, skip it
		}
		public void StatsDebuffAttack(Entity user, Entity target, MT19337 rng)
		{
			// check if we're using refresher??
			if (Id == (int)Items.Refresher)
			{
				target.RestoreStats();
			}
			else
			{
				(int hit, int crit) hitrate = CalcHitRate(user, target);
				int defense = CalcPhysicalDefense(target);
				int damage = 0x1E * (Power & 0x0F);
				if (RollD100(RollType.AfflictRoll, rng) < hitrate.hit)
				{
					target.ProcessDamage(damage, defense);
					int statid = ((Power & 0xE0) / 32) & 0x03;
					int statdebuff = ((Power / 16) & 0x03) * 0x19;

					switch (statid)
					{
						case 0:
							target.ApplyDebuff(statdebuff, 0, 0, 0);
							break;
						case 1:
							target.ApplyDebuff(0, statdebuff, 0, 0);
							break;
						case 2:
							target.ApplyDebuff(0, 0, statdebuff, 0);
							break;
						case 3:
							target.ApplyDebuff(0, 0, 0, statdebuff);
							break;
					}
				}
				else
				{
					Log.Add("Miss!");
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

					if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
					{
						target.Hp = 0;
						target.Ailments.Add(ElementsType.Doom);
					}
				}
			}
			else
			{
				if (target.Ailments.Contains(ElementsType.Doom) || target.Ailments.Contains(ElementsType.Stone) || target.Ailments.Contains(ElementsType.Confusion) || target.Ailments.Contains(ElementsType.Paralysis) || target.Ailments.Contains(ElementsType.Sleep) || target.Hp <= 0)
				{
					target.Recovered = true;
				}

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

				if (RollD100(RollType.AfflictRoll, rng) < hitrate.hit)
				{
					ApplyAilments(user, target, 100, rng);
				}
			}
			else
			{
				if (target.Ailments.Contains(ElementsType.Stone) || target.Ailments.Contains(ElementsType.Confusion) || target.Ailments.Contains(ElementsType.Paralysis) || target.Ailments.Contains(ElementsType.Sleep))
				{
					target.Recovered = true;
				}

				target.Ailments = new();
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
				damage = CalcWeakness(target.Weaknesses, damage);

				if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
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
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);
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

			if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
			{
				target.ProcessDamage(damage, defense);

				hitrate = (hitrate.hit / 2, hitrate.crit);

				if (RollD100(RollType.AfflictRoll, rng) < hitrate.hit)
				{
					ApplyAilments(user, target, 100, rng);
				}
			}
			else
			{
				Log.Add("Miss!");
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
				ApplyAilments(user, target, hitrate.hit, rng);
			}
			else
			{
				hitrate = (hitrate.hit / 2, hitrate.crit);
				if (RollD100(RollType.AfflictRoll, rng) < hitrate.hit)
				{
					ApplyAilments(user, target, 100, rng);
				}
				else
				{
					Log.Add("Miss!");
				}

			}
		}
		public void PhysicalAttackMultiHits(Entity user, Entity target, int targetcount, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int successfulhits = 0;
			for (int i = 0; i < NumberOfHits; i++)
			{
				if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
				{
					successfulhits++;
				}
			}
			
			if(successfulhits == 0)
			{
				Log.Add("Miss!");
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

			CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);
			target.ProcessDamage(damage, defense);
		}
		public void AttackDrain(Entity user, Entity target, MT19337 rng)
		{
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			int defense = CalcPhysicalDefense(target);
			int damage = CalcDamageStrAttHp8x075(user);
			damage = CalcResistance(target.Resistances, damage);
			damage = CalcCrit(target, damage, hitrate.crit, rng);
			target.ProcessDamage(damage, defense);
			user.ProcessDamage(-damage, 0);
			
			// there's a level component here, but i'm not sure
		}
		public void AttackSelfDestruct(Entity user, Entity target, MT19337 rng)
		{
			int defense = CalcPhysicalDefense(target);
			int damage = user.MaxHp + Power;

			target.ProcessDamage(damage, defense);
			user.ProcessDamage(user.Hp, 0);
		}
		public void AttackMultiply(Entity user, Entity target, MT19337 rng)
		{
			// yeah...?
			// if there's space
			(int hit, int crit) hitrate = CalcHitRate(user, target);
			if (RollD100(RollType.HitRoll, rng) < hitrate.hit)
			{
				user.Multiply = true;
			}
			else
			{
				Log.Add("Do nothing.");
			}

		}
		public void UseSeed(Entity user, Entity target, MT19337 rng)
		{ 
			// Nothing!
		}

		public void AttackFlatDamage(Entity user, Entity target, MT19337 rng)
		{
			int damage = Power * 8;
			
			if (Id == 0xD0)
			{
				user.ProcessDamage(-1000, 0);
			}
			int defense = CalcPhysicalDefense(target);
			damage -= defense; // remove defense before check resist
			damage = CalcResistance(target.Resistances, damage);
			//damage = CalcCrit(target, damage, hitrate.crit, rng);
			ApplyAilments(user, target, 100, rng);
			target.ProcessDamage(damage, 0);

		}
	}
}
