using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;
using System.Data.SqlTypes;

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

	public enum ElementsType
	{ 
		Silence = 0x0001,
		Blind = 0x0002,
		Poison = 0x0004,
		Confusion = 0x0008,
		Sleep = 0x0010,
		Paralysis = 0x0020,
		Stone = 0x0040,
		Doom = 0x0080,
		Projectile = 0x0100,
		Bomb = 0x0200,
		Axe = 0x0400,
		Zombie = 0x0800,
		Air = 0x1000,
		Fire = 0x2000,
		Water = 0x4000,
		Earth = 0x8000,
	}
	public class Typing
	{ 
		public List<(int, string)> Elements = new() {
			(0x01, "Projectile"),
			(0x02, "Bomb"),
			(0x04, "Axe"),
			(0x08, "Zombie"),
			(0x10, "Air"),
			(0x20, "Fire"),
			(0x40, "Water"),
			(0x80, "Earth")
		};

		public List<(int, string)> Status = new() {
			(0x01, "Silence"),
			(0x02, "Blind"),
			(0x04, "Poison"),
			(0x08, "Confusion"),
			(0x10, "Sleep"),
			(0x20, "Paralysis"),
			(0x40, "Stone"),
			(0x80, "Doom")
		};
	}

	public class EnemiesStats
	{
		private List<Enemy> _enemies;
		
		private const int EnemiesStatsQty = 0x53;

		private const int levelMultDataBank = 0x02;
		private const int levelMultDataOffset = 0xC17C;
		private const int levelMultDataSize = 0x03;


		private const int enemiesStatsOffset = 0xC275;
		private const int enemiesStatsBank = 0x02;
		private const int enemiesStatsLength = 0x0e;

		//public Dictionary<EnemyFormationIds, PowerLevels> FormationPowers { get; set; }
		public Dictionary<AccessReqs, AccessReqs> BossesPower;
		public Dictionary<LocationIds, AccessReqs> BattlefieldsPower;

		public EnemiesStats(FFMQRom rom)
		{

			BossesPower = new();
			BattlefieldsPower = new();

			_enemies = new List<Enemy>();

			for (int i = 0; i < EnemiesStatsQty; i++)
			{
				_enemies.Add(
					new Enemy(i,
						rom.GetFromBank(levelMultDataBank, levelMultDataOffset + (i * levelMultDataSize), levelMultDataSize),
						rom.GetFromBank(enemiesStatsBank, enemiesStatsOffset + (i * enemiesStatsLength), enemiesStatsLength)
						)
					);
			}
		}
		public Enemy this[int id]
		{
			get => _enemies[id];
			set => _enemies[id] = value;
		}
		public IList<Enemy> Enemies()
		{
			return _enemies.AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{
			foreach (Enemy e in _enemies)
			{
				rom.PutInBank(enemiesStatsBank, enemiesStatsOffset, _enemies.SelectMany(e => e.GetStatsBytes()).ToArray());
				rom.PutInBank(levelMultDataBank, levelMultDataOffset, _enemies.SelectMany(e => e.GetLevelMultBytes()).ToArray());
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
		public void ShuffleResistWeakness(bool shuffle, GameInfoScreen info, MT19337 rng)
		{
			if (!shuffle)
			{
				return;
			}
			
			var allList = Enum.GetValues<ElementsType>().ToList();
			var elementsMainList = allList.Where(x => (int)x > 0x00FF).ToList();
			var statusMainList = allList.Where(x => (int)x < 0x0100).ToList();
			var elementsShuffledList = elementsMainList.ToList();
			var statusShuffledList = statusMainList.ToList();

			elementsShuffledList.Shuffle(rng);
			statusShuffledList.Shuffle(rng);

			var elementsPairList = elementsMainList.Select((e, i) => (e, elementsShuffledList[i])).ToList();
			var statusPairList = statusMainList.Select((e, i) => (e, statusShuffledList[i])).ToList();

			var allPairList = statusPairList.Concat(elementsPairList).ToList();

			foreach (var enemy in _enemies)
			{
				List<ElementsType> newweaks = allPairList.Where(w => enemy.Weaknesses.Contains(w.Item1)).Select(w => w.Item2).ToList();
				List<ElementsType> newresists = allPairList.Where(w => enemy.Resistances.Contains(w.Item1)).Select(w => w.Item2).ToList();

				enemy.Weaknesses = newweaks.ToList();
				enemy.Resistances = newresists.ToList();
			}

			info.ShuffledElementsType = allPairList;
		}
		public void ScaleEnemies(Flags flags, MT19337 rng)
		{
			List<int> enemiesId = Enumerable.Range(0, 0x40).ToList();
			List<int> bossesId  = Enumerable.Range(0x40, EnemiesStatsQty - enemiesId.Count).ToList();

			ScaleStats(flags.EnemiesScalingLower, flags.EnemiesScalingUpper, enemiesId, rng);
			ScaleStats(flags.BossesScalingLower, flags.BossesScalingUpper, bossesId, rng);
		}
		public void ScaleStats(EnemiesScaling lowerboundscaling, EnemiesScaling upperboundscaling, List<int> validEnemies, MT19337 rng)
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

			List<Enemy> selectedEnemies = _enemies.Where((x, i) => validEnemies.Contains(i)).ToList();

			foreach (Enemy e in selectedEnemies)
			{
				e.HP = ScaleHP(e.HP, scaling, spread, rng);
				e.Attack = ScaleStat(e.Attack, scaling, spread, rng);
				e.Defense = ScaleStat(e.Defense, scaling, spread, rng);
				e.Speed = Max((byte)0x03, ScaleStat(e.Speed, scaling, spread, rng));
				e.Magic = ScaleStat(e.Magic, scaling, spread, rng);
				e.Accuracy = ScaleStat(e.Accuracy, scaling, spread, rng);
				e.Evade = ScaleStat(e.Evade, scaling, spread, rng);
			}
		}
	}
	public class Enemy
	{
		private Blob _rawBytes;

		public ushort HP { get; set; }
		public byte Attack { get; set; }
		public byte Defense { get; set; }
		public byte Speed { get; set; }
		public byte Magic { get; set; }
		public byte Accuracy { get; set; }
		public byte Evade { get; set; }
		public byte MagicDefense { get; set; }
		public byte MagicEvade { get; set; }
		public byte Level { get; set; }
		public byte XpMultiplier { get; set; }
		public byte GpMultiplier { get; set; }
		public List<ElementsType> Resistances { get; set; }
		public List<ElementsType> Weaknesses { get; set; }
		private byte spByte;

		public EnemyIds Id;

		private int _Id;

		private const int EnemiesStatsAddress = 0xC275; // Bank 02
		private const int EnemiesStatsBank = 0x02;
		private const int EnemiesStatsLength = 0x0e;

		public Enemy(int id, byte[] levelmult, byte[] statsdata)
		{
			Id = (EnemyIds)id;
			HP = (ushort)(statsdata[0x01] * 0x100 + statsdata[0x00]);
			Attack = statsdata[0x02];
			Defense = statsdata[0x03];
			Speed = statsdata[0x04];
			Magic = statsdata[0x05];
			// Resistances (2 bytes)
			MagicDefense = statsdata[0x08];
			MagicEvade = statsdata[0x09];
			Accuracy = statsdata[0x0A];
			Evade = statsdata[0x0B];
			// Weakneses (1 byte)
			spByte = statsdata[0x0D];

			Resistances = new();
			Weaknesses = new();

			Level = levelmult[0];
			XpMultiplier = levelmult[1];
			GpMultiplier = levelmult[2];

			int resist = statsdata[0x06] * 0x100 + statsdata[0x07];
			int weak = statsdata[0x0c];
			var elementTypeList = Enum.GetValues<ElementsType>().ToList();
			foreach (var element in elementTypeList)
			{
				if ((resist & (int)element) > 0)
				{
					Resistances.Add(element);
				}
			}

			foreach (var element in elementTypeList)
			{
				if ((weak & ((int)element / 0x100)) > 0)
				{
					Weaknesses.Add(element);
				}
			}
		}
		public byte[] GetLevelMultBytes()
		{
			return new byte[] { Level, XpMultiplier, GpMultiplier };
		}
		public byte[] GetStatsBytes()
		{
			byte[] _rawBytes = new byte[0x0E];
			
			_rawBytes[0x00] = (byte)(HP % 0x100);
			_rawBytes[0x01] = (byte)(HP / 0x100);
			_rawBytes[0x02] = Attack;
			_rawBytes[0x03] = Defense;
			_rawBytes[0x04] = Speed;
			_rawBytes[0x05] = Magic;
			// Resistances (2 bytes)
			_rawBytes[0x08] = MagicDefense;
			_rawBytes[0x09] = MagicEvade;
			_rawBytes[0x0A] = Accuracy;
			_rawBytes[0x0B] = Evade;
			// Weaknesses (1 byte)
			_rawBytes[0x0E] = spByte;

			int tempresist = 0;
			int tempweak = 0;

			foreach (var resist in Resistances)
			{
				tempresist |= (int)resist;
			}

			foreach (var weak in Weaknesses)
			{
				tempweak |= ((int)weak / 0x100);
			}
			_rawBytes[0x06] = (byte)(tempresist / 0x100);
			_rawBytes[0x07] = (byte)(tempresist % 0x100);
			_rawBytes[0x0c] = (byte)(tempweak);

			return _rawBytes;
		}
	}
	public enum BattleTracks
	{ 
		Normal = 0x00,
		Boss = 0x01,
		Final = 0x10,
		Unusued = 0x11,
	}

	public class Formation
	{ 
		public EnemyFormationIds Id { get; set; }
		public List<EnemyIds> Enemies { get; set; }
		public List<bool> Flying { get; set; }
		private byte settingByte;
		public bool CantRun { get; set; }
		public bool Floating { get; set; }
		public BattleTracks Track { get; set; }

		private byte unknownBitMask = 0x93;

		public Formation(EnemyFormationIds id, byte[] data)
		{
			Id = id;
			Enemies = new();
			Flying = new();

			for (int i = 0; i < 3; i++)
			{
				if (data[i] != 0xFF)
				{
					Enemies.Add((EnemyIds)(data[i] & 0x7F));
				}

				Flying.Add((data[i] & 0x80) > 0);
			}

			settingByte = data[3];

			CantRun = (settingByte & 0x40) > 0;
			Floating = (settingByte & 0x20) > 0;
			Track = (BattleTracks)((settingByte / 4) & 0x03);
		}

		public byte[] GetBytes()
		{
			var byteEnemies = Enemies.Select(e => (byte)e).Concat(new List<byte>() { 0xFF, 0xFF }).ToList().GetRange(0, 3);

			return new byte[]
				{
					(byte)(byteEnemies[0] | (Flying[0] ? 0x80 : 0x00)),
					(byte)(byteEnemies[1] | (Flying[0] ? 0x80 : 0x00)),
					(byte)(byteEnemies[2] | (Flying[0] ? 0x80 : 0x00)),
					(byte)((settingByte & unknownBitMask) | (CantRun ? 0x40 : 0x00) | (Floating ? 0x20 : 0x00) | (byte)((int)Track * 4))
				};
		}
	}
	public class FormationsData
	{
		private const int formationsBank = 0x02;
		private const int formationsOffset = 0xCA6A;
		private const int formationsSize = 0x04;
		private const int formationsQty = 0xEA;

		public Dictionary<EnemyFormationIds, Formation> Formations;

		public FormationsData(FFMQRom rom)
		{ 
			Formations = rom.GetFromBank(formationsBank, formationsOffset, formationsQty * formationsSize).Chunk(formationsSize).Select((f,i) => new Formation((EnemyFormationIds)i, f)).ToDictionary(f => f.Id, f => f);
		}

		public void Write(FFMQRom rom)
		{
			rom.PutInBank(formationsBank, formationsOffset, Formations.SelectMany(f => f.Value.GetBytes()).ToArray());
		}
	}
}
