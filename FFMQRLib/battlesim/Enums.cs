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
		Companion,
		Enemy,
		IceGolem,
		Hydra,
		Pazuzu,
		DarkKing
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
		Godly,
		Plain10,
		Plain20,
		Plain30,
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
		DeactivateShield,
	}

	public enum DkModes
	{ 
		Phase1,
		Phase2,
		Phase34
	}


	public enum RollType
	{ 
		HitRoll,
		CritRoll,
		AfflictRoll
	
	}

	public enum EnemyAttackIds
	{ 
		Sword = 0x40,
		Scimitar,
		DragonCut,
		Rapier,
		Axe,
		Beam,
		BoneMissile,
		BowArrow,
		BlowDart,
		CureSelf,
		HealSelf,
		QuakeSpell,
		Blizzard,
		Fire,
		ThunderSpell,
		Reflectant,
		Electrapulse,
		PowerDrain,
		Spark,
		IronNail,
		Scream,
		QuickSand,
		DoomGaze,
		DoomPowder,
		Cure,
		FireBreathFixed,
		Punch,
		Kick,
		Uppercut,
		Stab,
		HeadButt,
		BodySlam,
		Scrunch,
		FullNelson,
		NeckChoke,
		Dash,
		Roundhouse,
		ChokeUp,
		StompStomp,
		MegaPunch,
		Bearhug,
		AxeBomber,
		PiledriverFixed,
		SkyAttackFixed,
		Wraparound,
		Dive,
		Attatch,
		Mucus,
		Claw,
		Fang,
		Beak,
		Sting,
		Tail,
		Psudopod,
		Bite,
		HydroAcid,
		Branch,
		Fin,
		Scissor,
		WhipTongue,
		Horn,
		GiantBlade,
		Headboomerang,
		ChewOff,
		Quake,
		Flame,
		FlameSweep,
		FireBall,
		FlamePillar,
		Heatwave,
		Watergun,
		Coldness,
		IcyFoam,
		IceBlock,
		Snowstorm,
		Whirlwater,
		IceBreath,
		Tornado,
		Typhoon,
		Hurricane,
		Thunder,
		ThunderBeam,
		CorrodeGas,
		DoomDance,
		SonicBoom,
		Bark,
		Screechvoice,
		ParaNeedle,
		ParaClaw,
		ParaSnake,
		ParaBreath,
		PoisonSting,
		PoisonThorn,
		RottonMucus,
		PoisonSnake,
		Poisonbreath,
		Blinder,
		Blackness,
		StoneBeak,
		Gaze,
		Stare,
		SpookyLaugh,
		Riddle,
		BadBreath,
		BodyOdor,
		ParaStare,
		PoisonFluid,
		PoisonFlour,
		HypnoSleep,
		Lullaby,
		SleepLure,
		SleepPowder,
		BlindFlash,
		Smokescreen,
		Muffle,
		SilenceSong,
		StoneGas,
		StoneGaze,
		DoubleSword,
		DoubleHit,
		TripleFang,
		DoubleKick,
		TwinShears,
		ThreeHeads,
		SixPsudopods,
		SnakeHead,
		Drain,
		Dissolve,
		SuckerStick,
		Selfdestruct,
		Multiply,
		ParaGas,
		RipEarth,
		StoneBlock,
		Windstorm,
		TwinFang,
		PsychshieldBug,
		Psychshield,
		DarkCane,
		DarkSaber,
		IceSword,
		FireSword,
		MirrorSword,
		QuakeAxe,
		CureArrow,
		Lazer,
		SpiderKids,
		SilverWeb,
		GoldenWeb,
		MegaFlare,
		MegaWhite,
		FireBreath,
		SkyAttack,
		Piledriver,
		HurricanePlus,
		HeatwaveUnused,
		Nothing = 0xFF
	}
}
