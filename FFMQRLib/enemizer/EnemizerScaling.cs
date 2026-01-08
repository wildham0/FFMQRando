using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static System.Math;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFMQLib
{
	public enum EnemiesScaling : int
	{
		[Description("25%")]
		Quarter = 0,
		[Description("50%")]
		Half,
		[Description("75%")]
		ThreeQuarter,
		[Description("100%")]
		Normal,
		[Description("125%")]
		OneAndQuarter,
		[Description("150%")]
		OneAndHalf,
		[Description("200%")]
		Double,
		[Description("250%")]
		DoubleAndHalf,
		[Description("300%")]
		Triple,
	}
	public enum EnemiesScalingSpread : int
	{
		[Description("0%")]
		None = 0,
		[Description("25%")]
		Quarter,
		[Description("50%")]
		Half,
		[Description("100%")]
		Full,
	}
	public partial class Enemizer
	{

		public void ScaleEnemies(Flags flags, MT19337 rng)
		{
			NerfBosses((flags.EnemizerGroups != EnemizerGroups.MobsOnly) && (flags.EnemizerAttacks != EnemizerAttacks.Normal));

			ScaleStats(flags.EnemiesScalingLower, flags.EnemiesScalingUpper, Mobs, rng);
			ScaleStats(flags.BossesScalingLower, flags.BossesScalingUpper, Bosses.Concat(DarkCastleBosses).Concat(DarkKing).ToList(), rng);
		}
		public void NerfBosses(bool enemizer)
		{
			if (!enemizer)
			{
				return;
			}

			// Wyvern
			enemies.Data[EnemyIds.TwinheadWyvern].Attack -= 50;
			enemies.Data[EnemyIds.TwinheadWyvern].Magic -= 100;

			// Zuh
			enemies.Data[EnemyIds.Zuh].Attack -= 50;
		}
		public void ScaleStats(EnemiesScaling lowerboundscaling, EnemiesScaling upperboundscaling, List<EnemyIds> validEnemies, MT19337 rng)
		{
			int lowerbound = 100;
			int upperbound = 100;

			switch (lowerboundscaling)
			{
				case EnemiesScaling.Quarter: lowerbound = 25; break;
				case EnemiesScaling.Half: lowerbound = 50; break;
				case EnemiesScaling.ThreeQuarter: lowerbound = 75; break;
				case EnemiesScaling.Normal: lowerbound = 100; break;
				case EnemiesScaling.OneAndQuarter: lowerbound = 125; break;
				case EnemiesScaling.OneAndHalf: lowerbound = 150; break;
				case EnemiesScaling.Double: lowerbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: lowerbound = 250; break;
				case EnemiesScaling.Triple: lowerbound = 300; break;
			}

			switch (upperboundscaling)
			{
				case EnemiesScaling.Quarter: upperbound = 25; break;
				case EnemiesScaling.Half: upperbound = 50; break;
				case EnemiesScaling.ThreeQuarter: upperbound = 75; break;
				case EnemiesScaling.Normal: upperbound = 100; break;
				case EnemiesScaling.OneAndQuarter: upperbound = 125; break;
				case EnemiesScaling.OneAndHalf: upperbound = 150; break;
				case EnemiesScaling.Double: upperbound = 200; break;
				case EnemiesScaling.DoubleAndHalf: upperbound = 250; break;
				case EnemiesScaling.Triple: upperbound = 300; break;
			}

			if (upperbound < lowerbound)
			{
				upperbound = lowerbound;
			}

			int spread = (upperbound - lowerbound) / 2;
			int scaling = lowerbound + spread;

			var selectedEnemies = enemies.Data.Where((x, i) => validEnemies.Contains(x.Key)).ToList();

			foreach (var e in selectedEnemies)
			{
				e.Value.HP = ScaleHP(e.Value.HP, scaling, spread, rng);
				e.Value.Attack = ScaleStat(e.Value.Attack, scaling, spread, rng);
				e.Value.Defense = ScaleStat(e.Value.Defense, scaling, spread, rng);
				e.Value.Speed = Max((byte)0x03, ScaleStat(e.Value.Speed, scaling, spread, rng));
				e.Value.Magic = ScaleStat(e.Value.Magic, scaling, spread, rng);
				e.Value.Accuracy = ScaleStat(e.Value.Accuracy, scaling, spread, rng);
				e.Value.Evade = ScaleStat(e.Value.Evade, scaling, spread, rng);
			}
		}
		private byte ScaleStat(byte value, int scaling, int spread, MT19337 rng)
		{
			int randomizedScaling = scaling;
			if (spread != 0)
			{
				int max = scaling + spread;
				int min = Max(25, scaling - spread);

				randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
			}
			return (byte)Min(0xFF, Max(0x01, value * randomizedScaling / 100));
		}
		private ushort ScaleHP(ushort value, int scaling, int spread, MT19337 rng)
		{
			int randomizedScaling = scaling;
			if (spread != 0)
			{
				int max = scaling + spread;
				int min = Max(25, scaling - spread);

				randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
			}
			return (ushort)Min(0xFFFF, Max(0x01, value * randomizedScaling / 100));
		}

	}
}
