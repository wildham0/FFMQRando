﻿using System;
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
	public class BattleRating
	{
		public static Dictionary<EnemyAttackIds, int> AttackRating = new()
		{
			{ EnemyAttackIds.Sword, 24 },
			{ EnemyAttackIds.Scimitar, 25 },
			{ EnemyAttackIds.DragonCut, 25 },
			{ EnemyAttackIds.Rapier, 30 },
			{ EnemyAttackIds.Axe, 24 },
			{ EnemyAttackIds.Beam, 26 },
			{ EnemyAttackIds.BoneMissile, 7 },
			{ EnemyAttackIds.BowArrow, 28 },
			{ EnemyAttackIds.BlowDart, 28 },
			{ EnemyAttackIds.CureSelf, 10 },
			{ EnemyAttackIds.HealSelf, 100 },
			{ EnemyAttackIds.QuakeSpell, 51 },
			{ EnemyAttackIds.Blizzard, 15 },
			{ EnemyAttackIds.Fire, 16 },
			{ EnemyAttackIds.ThunderSpell, 17 },
			{ EnemyAttackIds.Reflectant, 0 },
			{ EnemyAttackIds.Electrapulse, 2 },
			{ EnemyAttackIds.PowerDrain, 2 },
			{ EnemyAttackIds.Spark, 4 },
			{ EnemyAttackIds.IronNail, 4 },
			{ EnemyAttackIds.Scream, 2 },
			{ EnemyAttackIds.QuickSand, 4 },
			{ EnemyAttackIds.DoomGaze, 50 },
			{ EnemyAttackIds.DoomPowder, 50 },
			{ EnemyAttackIds.Cure, 10 },
			{ EnemyAttackIds.FireBreathFixed, 49 },
			{ EnemyAttackIds.Punch, 22 },
			{ EnemyAttackIds.Kick, 22 },
			{ EnemyAttackIds.Uppercut, 23 },
			{ EnemyAttackIds.Stab, 22 },
			{ EnemyAttackIds.HeadButt, 23 },
			{ EnemyAttackIds.BodySlam, 23 },
			{ EnemyAttackIds.Scrunch, 23 },
			{ EnemyAttackIds.FullNelson, 23 },
			{ EnemyAttackIds.NeckChoke, 24 },
			{ EnemyAttackIds.Dash, 24 },
			{ EnemyAttackIds.Roundhouse, 23 },
			{ EnemyAttackIds.ChokeUp, 28 },
			{ EnemyAttackIds.StompStomp, 28 },
			{ EnemyAttackIds.MegaPunch, 22 },
			{ EnemyAttackIds.Bearhug, 22 },
			{ EnemyAttackIds.AxeBomber, 23 },
			{ EnemyAttackIds.PiledriverFixed, 42 },
			{ EnemyAttackIds.SkyAttackFixed, 37 },
			{ EnemyAttackIds.Wraparound, 23 },
			{ EnemyAttackIds.Dive, 23 },
			{ EnemyAttackIds.Attatch, 23 },
			{ EnemyAttackIds.Mucus, 23 },
			{ EnemyAttackIds.Claw, 23 },
			{ EnemyAttackIds.Fang, 23 },
			{ EnemyAttackIds.Beak, 23 },
			{ EnemyAttackIds.Sting, 24 },
			{ EnemyAttackIds.Tail, 23 },
			{ EnemyAttackIds.Psudopod, 23 },
			{ EnemyAttackIds.Bite, 23 },
			{ EnemyAttackIds.HydroAcid, 23 },
			{ EnemyAttackIds.Branch, 23 },
			{ EnemyAttackIds.Fin, 23 },
			{ EnemyAttackIds.Scissor, 23 },
			{ EnemyAttackIds.WhipTongue, 23 },
			{ EnemyAttackIds.Horn, 22 },
			{ EnemyAttackIds.GiantBlade, 25 },
			{ EnemyAttackIds.Headboomerang, 47 },
			{ EnemyAttackIds.ChewOff, 22 },
			{ EnemyAttackIds.Quake, 37 },
			{ EnemyAttackIds.Flame, 30 },
			{ EnemyAttackIds.FlameSweep, 42 },
			{ EnemyAttackIds.FireBall, 36 },
			{ EnemyAttackIds.FlamePillar, 47 },
			{ EnemyAttackIds.Heatwave, 44 },
			{ EnemyAttackIds.Watergun, 28 },
			{ EnemyAttackIds.Coldness, 29 },
			{ EnemyAttackIds.IcyFoam, 30 },
			{ EnemyAttackIds.IceBlock, 34 },
			{ EnemyAttackIds.Snowstorm, 55 },
			{ EnemyAttackIds.Whirlwater, 42 },
			{ EnemyAttackIds.IceBreath, 29 },
			{ EnemyAttackIds.Tornado, 30 },
			{ EnemyAttackIds.Typhoon, 47 },
			{ EnemyAttackIds.Hurricane, 23 },
			{ EnemyAttackIds.Thunder, 32 },
			{ EnemyAttackIds.ThunderBeam, 30 },
			{ EnemyAttackIds.CorrodeGas, 63 },
			{ EnemyAttackIds.DoomDance, 71 },
			{ EnemyAttackIds.SonicBoom, 63 },
			{ EnemyAttackIds.Bark, 64 },
			{ EnemyAttackIds.Screechvoice, 73 },
			{ EnemyAttackIds.ParaNeedle, 64 },
			{ EnemyAttackIds.ParaClaw, 64 },
			{ EnemyAttackIds.ParaSnake, 72 },
			{ EnemyAttackIds.ParaBreath, 70 },
			{ EnemyAttackIds.PoisonSting, 14 },
			{ EnemyAttackIds.PoisonThorn, 14 },
			{ EnemyAttackIds.RottonMucus, 14 },
			{ EnemyAttackIds.PoisonSnake, 15 },
			{ EnemyAttackIds.Poisonbreath, 23 },
			{ EnemyAttackIds.Blinder, 15 },
			{ EnemyAttackIds.Blackness, 15 },
			{ EnemyAttackIds.StoneBeak, 63 },
			{ EnemyAttackIds.Gaze, 50 },
			{ EnemyAttackIds.Stare, 50 },
			{ EnemyAttackIds.SpookyLaugh, 50 },
			{ EnemyAttackIds.Riddle, 50 },
			{ EnemyAttackIds.BadBreath, 50 },
			{ EnemyAttackIds.BodyOdor, 50 },
			{ EnemyAttackIds.ParaStare, 50 },
			{ EnemyAttackIds.PoisonFluid, 0 },
			{ EnemyAttackIds.PoisonFlour, 0 },
			{ EnemyAttackIds.HypnoSleep, 10 },
			{ EnemyAttackIds.Lullaby, 10 },
			{ EnemyAttackIds.SleepLure, 10 },
			{ EnemyAttackIds.SleepPowder, 10 },
			{ EnemyAttackIds.BlindFlash, 0 },
			{ EnemyAttackIds.Smokescreen, 0 },
			{ EnemyAttackIds.Muffle, 0 },
			{ EnemyAttackIds.SilenceSong, 0 },
			{ EnemyAttackIds.StoneGas, 50 },
			{ EnemyAttackIds.StoneGaze, 50 },
			{ EnemyAttackIds.DoubleSword, 25 },
			{ EnemyAttackIds.DoubleHit, 25 },
			{ EnemyAttackIds.TripleFang, 22 },
			{ EnemyAttackIds.DoubleKick, 14 },
			{ EnemyAttackIds.TwinShears, 13 },
			{ EnemyAttackIds.ThreeHeads, 31 },
			{ EnemyAttackIds.SixPsudopods, 41 },
			{ EnemyAttackIds.SnakeHead, 46 },
			{ EnemyAttackIds.Drain, 16 },
			{ EnemyAttackIds.Dissolve, 16 },
			{ EnemyAttackIds.SuckerStick, 14 },
			{ EnemyAttackIds.Selfdestruct, 100 },
			{ EnemyAttackIds.Multiply, 100 },
			{ EnemyAttackIds.ParaGas, 50 },
			{ EnemyAttackIds.RipEarth, 27 },
			{ EnemyAttackIds.StoneBlock, 11 },
			{ EnemyAttackIds.Windstorm, 23 },
			{ EnemyAttackIds.TwinFang, 14 },
			{ EnemyAttackIds.PsychshieldBug, 0 },
			{ EnemyAttackIds.Psychshield, 100 },
			{ EnemyAttackIds.DarkCane, 42 },
			{ EnemyAttackIds.DarkSaber, 61 },
			{ EnemyAttackIds.IceSword, 93 },
			{ EnemyAttackIds.FireSword, 53 },
			{ EnemyAttackIds.MirrorSword, 40 },
			{ EnemyAttackIds.QuakeAxe, 37 },
			{ EnemyAttackIds.CureArrow, 62 },
			{ EnemyAttackIds.Lazer, 64 },
			{ EnemyAttackIds.SpiderKids, 57 },
			{ EnemyAttackIds.SilverWeb, 92 },
			{ EnemyAttackIds.GoldenWeb, 87 },
			{ EnemyAttackIds.MegaFlare, 66 },
			{ EnemyAttackIds.MegaWhite, 46 },
			{ EnemyAttackIds.FireBreath, 34 },
			{ EnemyAttackIds.SkyAttack, 47 },
			{ EnemyAttackIds.Piledriver, 23 },
			{ EnemyAttackIds.HurricanePlus, 24 },
			{ EnemyAttackIds.HeatwaveUnused, 44 },
		};
	}
}
