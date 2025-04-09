﻿using System;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public enum EnemyIds
	{
		Brownie = 0x00,
		Mintmint,
		RedCap,

		MadPlant,
		PlantMan,
		LiveOak,

		Slime,
		Jelly,
		Ooze,

		PoisonToad,
		GiantToad,
		MadToad,

		Basilisk,
		Flazzard,
		Salamand,

		SandWorm,
		LandWorm,
		Leech,

		Skeleton,
		RedBone,
		Skuldier,

		Roc,
		Sparna,
		Garuda,

		Zombie,
		Mummy,

		DesertHag,
		WaterHag,

		Ninja,
		Shadow,

		Sphinx,
		Manticor,

		Centaur,
		Nitemare,

		StoneyRoost,
		HotWings,

		Ghost,
		Spector,

		Gather,
		Beholder,

		Fangpire,
		Vampire,

		Mage,
		Sorcerer,

		LandTurtle,
		AdamantTurtle,

		Scorpion,
		Snipion,

		Werewolf,
		Cerberus,

		Edgehog,
		StingRat,

		Lamia,
		Naga,

		Avizzard,
		Gargoyle,

		Gorgon,
		MinotaurZombie,
		Phanquid,
		FreezerCrab,
		Iflyte,
		Stheno,
		Chimera,
		Thanatos,

		SkullrusRex = 0x40,
		StoneGolem = 0x41,
		Behemoth = 0x42,
		Minotaur = 0x43,
		Squidite = 0x44,
		SnowCrab = 0x45,
		Jinn = 0x46,
		Medusa = 0x47,
		Gidrah = 0x48,
		Dullahan = 0x49,
		FlamerusRex = 0x4A,
		IceGolem = 0x4B,
		DualheadHydra = 0x4C,
		TwinheadWyvern = 0x4D,
		Pazuzu = 0x4E,
		Zuh = 0x4F,
		DarkKing = 0x50,
		DarkKingWeapons = 0x51, //
		DarkKingSpider = 0x52,
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

	public enum EnemyFormationIds
	{
		Brownie01 = 0x00,
		Brownie02 = 0x01,
		Brownie03,
		Slime01,
		Slime02,
		Slime03,
		MadPlant01,
		MadPlant02,
		MadPlant03,
		PoisonToad01,
		PoisonToad02,
		PoisonToad03,
		Basilisk01,
		Basilisk02,
		Basilisk03,
		Roc01,
		Roc02,
		Roc03,
		SandWorm01,
		SandWorm02,
		SandWorm03,
		Skeleton01,
		Skeleton02,
		Skeleton03,
		Mintmint01,
		Mintmint02,
		Mintmint03,
		GiantToad01,
		GiantToad02,
		GiantToad03,
		Scorpion01,
		Scorpion02,
		Scorpion03,
		Edgehog01,
		Edgehog02,
		Edgehog03,
		LandWorm01,
		LandWorm02,
		LandWorm03,
		Centaur01,
		Centaur02,
		Centaur03,
		LandTurtle01,
		LandTurtle02,
		LandTurtle03,
		Sparna01,
		Sparna02,
		Sparna03,
		DesertHag01,
		DesertHag02,
		DesertHag03,
		Lamia01,
		Lamia02,
		Lamia03,
		Mage01,
		Mage02,
		Mage03,
		StoneyRoost01,
		StoneyRoost02,
		StoneyRoost03,
		Gather01,
		Gather02,
		Gather03,
		Sphinx01,
		Sphinx02,
		Sphinx03,
		Jelly01,
		Jelly02,
		Jelly03,
		StingRat01,
		StingRat02,
		PlantMan01,
		PlantMan02,
		PlantMan03,
		PlantMan04,
		Flazzard01,
		Flazzard02,
		Flazzard03,
		RedCap01,
		RedCap02,
		RedCap03,
		Zombie01,
		Zombie02,
		Zombie03,
		RedBone01,
		RedBone02,
		RedBone03,
		Ghost01,
		Ghost02,
		Ghost03,
		Nitemare01,
		Nitemare02,
		Nitemare03,
		WereWolf01,
		WereWolf02,
		WereWolf03,
		HotWings01,
		HotWings02,
		HotWings03,
		Ninja01,
		Ninja02,
		Ninja03,
		Avizzard01,
		Avizzard02,
		Avizzard03,
		Fangpire01,
		Fangpire02,
		Fangpire03,
		AdamantTurtle01,
		AdamantTurtle02,
		AdamantTurtle03,
		Salamand01,
		Salamand02,
		Salamand03,
		Mummy01,
		Mummy02,
		Mummy03,
		Spector01,
		Spector02,
		Spector03,
		LiveOak01,
		LiveOak02,
		LiveOak03,
		Snipion01,
		Snipion02,
		Snipion03,
		MadToad01,
		MadToad02,
		MadToad03,
		Leech01,
		Leech02,
		Leech03,
		Ooze01,
		Ooze02,
		Ooze03,
		Skuldier01,
		Skuldier02,
		Skuldier03,
		WaterHag01,
		WaterHag02,
		WaterHag03,
		Vampire01,
		Vampire02,
		Vampire03,
		Garuda01,
		Garuda02,
		Garuda03,
		Beholder01,
		Beholder02,
		Beholder03,
		Manticore01,
		Manticore02,
		Manticore03,
		Sorcerer01,
		Sorcerer02,
		Sorcerer03,
		Naga01,
		Naga02,
		Naga03,
		Gargoyle01,
		Gargoyle02,
		Gargoyle03,
		Shadow01,
		Shadow02,
		Shadow03,
		Cerberus01,
		Cerberus02,
		Cerberus03,
		Behemoth,
		Minotaur,
		Gorgon01,
		Gorgon02,
		Gorgon03,
		Gorgon04,
		Gorgon05,
		Gorgon06,
		MinotaurZombie01,
		MinotaurZombie02,
		MinotaurZombie03,
		MinotaurZombie04,
		MinotaurZombie05,
		MinotaurZombie06,
		Squidite,
		SnowCrab,
		Phanquid01,
		Phanquid02,
		Phanquid03,
		Phanquid04,
		Phanquid05,
		Phanquid06,
		FreezerCrab01,
		FreezerCrab02,
		FreezerCrab03,
		FreezerCrab04,
		FreezerCrab05,
		FreezerCrab06,
		Jinn,
		Medusa,
		Iflyte01,
		Iflyte02,
		Iflyte03,
		Iflyte04,
		Iflyte05,
		Iflyte06,
		Stheno01,
		Stheno02,
		Stheno03,
		Stheno04,
		Stheno05,
		Stheno06,
		Gidrah,
		Dullahan,
		Chimera01,
		Chimera02,
		Chimera03,
		Chimera04,
		Chimera05,
		Chimera06,
		Thanatos01,
		Thanatos02,
		Thanatos03,
		Thanatos04,
		Thanatos05,
		Thanatos06,
		FlamerusRex,
		IceGolem,
		DualheadHydra,
		Pazuzu,
		SkullrusRex,
		StoneGolem,
		TwinheadWyvern,
		Zuh,
		DarkKing,
		ReubenMummy
	}
}
