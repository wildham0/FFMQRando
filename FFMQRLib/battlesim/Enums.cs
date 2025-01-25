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
	public enum Teams
	{ 
		TeamA,
		TeamB,
	}

	public enum AiProfiles
	{ 
		Player,
		Enemy,
		IceGolem,
		Hydra,
		Pazuzu
	}
	public enum TargetSelections
	{ 
		PrioritizeSingleTarget,
		PrioritizeMultipleTarget,
		RandomTargeting,
		OverrideMultiple
	}

	public enum PowerLevels
	{ 
		Initial,
		Intermediate,
		Strong,
		Godly
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
		MagicStatsDebuff,
		MagicUnknown2,
		Life,
		Heal,
		Cure,
		PhysicalDamage1,
		PhysicalDamage2 = 0x10,
		PhysicalDamage3,
		PhysicalDamage4,
		Ailments1,
		PhysicalDamage5,
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
	public enum Targetings
	{ 
		SingleEnemy,
		MultipleEnemy,
		SelectionEnemy,
		SingleAlly,
		MultipleAlly,
		SelectionAlly,
		SingleAny,
		MultipleAny,
		SelectionAny
	}

	public enum PazuzuModes
	{ 
		AttackingShieldOff,
		ActivateShield,
		AttackingShieldOn,
		DeactivteShield,
	}

	public enum DkModes
	{ 
		Phase1,
		Phase2,
		Phase34
	}
}
