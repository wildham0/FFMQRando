﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;



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


	public class Entity
	{
		public int Strength;
		public int Agility;
		public int Speed; 
		public int Magic;
		public int Accuracy;
		public int Evade;
		public int Hp;
		public int MaxHp;
		public int Defense;
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

		public Entity(MT19337 rng)
		{
			Actions = new();
			Actions.Add(new WpSteelSword());

			Rng = rng;

			foreach (var action in Actions)
			{
				action.Execute(this, new() { this }, rng);
			}
		}
		private void ProcessGears()
		{
			foreach (var gear in Gears)
			{
				Strength += geardata[gear].Strength;
				Agility += geardata[gear].Agility;
				Speed += geardata[gear].Speed;
				Magic += geardata[gear].Magic;
				Defense += geardata[gear].Defense;
				Evade += geardata[gear].Evade;
				Resistances.AddRange(geardata[gear].Resistances);
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

			{ Items.SteelHelm, new GearStats() { Defense = 4, Evade = 5, Strength = 5 } },
			{ Items.MoonHelm, new GearStats() { Defense = 9, Evade = 9, Strength = 5, Resistances = new() { ElementsType.Fire } } },
			{ Items.ApolloHelm, new GearStats() { Defense = 15, Evade = 15, Strength = 5, Resistances = new() { ElementsType.Fire } } },

			{ Items.SteelArmor, new GearStats() { Defense = 6, Evade = 4 } },
			{ Items.NobleArmor, new GearStats() { Defense = 12, Evade = 10, Resistances = new() { ElementsType.Water, ElementsType.Poison } } },
			{ Items.GaiasArmor, new GearStats() { Defense = 15, Evade = 11, Resistances = new() { ElementsType.Water, ElementsType.Poison, ElementsType.Sleep, ElementsType.Air } } },

			{ Items.SteelShield, new GearStats() { Defense = 5, Evade = 6, Speed = 5 } },
			{ Items.VenusShield, new GearStats() { Defense = 10, Evade = 12, Speed = 5, Resistances = new() { ElementsType.Paralysis } } },
			{ Items.AegisShield, new GearStats() { Defense = 14, Evade = 14, Speed = 5, Resistances = new() { ElementsType.Paralysis, ElementsType.Stone } } },

			{ Items.Charm, new GearStats() { Defense = 1, Evade = 1, Magic = 5 } },
			{ Items.MagicRing, new GearStats() { Defense = 3, Evade = 3, Magic = 5, Resistances = new() { ElementsType.Silence } } },
			{ Items.CupidLocket, new GearStats() { Defense = 6, Evade = 6, Magic = 5, Resistances = new() { ElementsType.Silence, ElementsType.Blind, ElementsType.Confusion } } },
		};
	}
	public class BattleAction
	{
		protected int Power;
		protected int CritRate;
		protected int Accuracy;
		protected List<ElementsType> Ailments;
		protected List<ElementsType> TypeDamage;
		public BattleAction() { }
		//public virtual void Execute(Entity user, List<Entity> targets, MT19337 rng) { }

		public void Execute(Entity user, List<Entity> targets, MT19337 rng)
		{
			var hitrate = CalcHitRate(user);
			if (user.Ailments.Contains(ElementsType.Blind))
			{
				hitrate /= 2;
			}
			var damage = CalcDamage(user);
			var targetcount = targets.Count;

			foreach (var target in targets)
			{
				if (target.IsPlayer)
				{
					hitrate = 100 - target.Evade;
				}
				
				if (rng.Between(0, 100) < hitrate)
				{
					if (rng.Between(0, 100) < CritRate)
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
		public virtual int CalcHitRate(Entity user) { return 0; }
		public virtual int CalcDamage(Entity user) { return 0; }
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

}
