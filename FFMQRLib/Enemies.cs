using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Core.Tokens;
using static System.Math;

namespace FFMQLib
{
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

	public class Enemies
	{
		public Dictionary<EnemyIds, Enemy> Data { get; }
		
		private const int EnemiesStatsQty = 0x53;

		private const int levelMultDataBank = 0x02;
		private const int levelMultDataOffset = 0xC17C;
		private const int levelMultDataSize = 0x03;

		private const int enemiesStatsOffset = 0xC275;
		private const int enemiesStatsBank = 0x02;
		private const int enemiesStatsLength = 0x0e;

		private const int graphicDataOffset = 0x8460;
		private const int graphicDataBank = 0x09;
		private const int graphicDataLength = 0x05;
		private const int graphicDataQty = 0x51;

		private const int namesOffset = 0xCBA0;
		private const int namesBank = 0x0C;
		private const int namesLength = 0x10;
		private const int namesQty = 0x51;

		public Enemies(FFMQRom rom)
		{
			Data = new();

			for (int i = 0; i < EnemiesStatsQty; i++)
			{
				Data.Add((EnemyIds)i,
					new Enemy(i,
						rom.GetFromBank(levelMultDataBank, levelMultDataOffset + (i * levelMultDataSize), levelMultDataSize),
						rom.GetFromBank(enemiesStatsBank, enemiesStatsOffset + (i * enemiesStatsLength), enemiesStatsLength),
						rom.GetFromBank(graphicDataBank, graphicDataOffset + (Min(i, (graphicDataQty - 1)) * graphicDataLength), graphicDataLength),
						rom.GetFromBank(namesBank, namesOffset + (Min(i, (namesQty - 1)) * namesLength), namesLength)
						)
					);
			}
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(enemiesStatsBank, enemiesStatsOffset, Data.SelectMany(e => e.Value.GetStatsBytes()).ToArray());
			rom.PutInBank(levelMultDataBank, levelMultDataOffset, Data.SelectMany(e => e.Value.GetLevelMultBytes()).ToArray());
			rom.PutInBank(graphicDataBank, graphicDataOffset, Data.Where(e => (int)e.Value.Id < graphicDataQty).SelectMany(e => e.Value.GetGraphicDataBytes()).ToArray());
			rom.PutInBank(namesBank, namesOffset, Data.Where(e => (int)e.Value.Id < namesQty).SelectMany(e => e.Value.GetNameBytes()).ToArray());
		}
		public IList<Enemy> ToList()
		{
			return Data.Select(e => e.Value).ToList().AsReadOnly();
		}
	}
	public class Enemy
	{
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
		public byte Palette1 { get; set; }
		public byte Palette2 { get; set; }
		public string Name { get; set; }
		public EnemizerElements Element { get; set; }

		private byte[] graphicData;

		private byte spByte;



		public EnemyIds Id;

		public Enemy(int id, byte[] levelmult, byte[] statsdata, byte[] graphicdata, byte[] name)
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
			graphicData = graphicdata[0..3];

			Palette1 = graphicdata[3];
			Palette2 = graphicdata[4];

			Name = MQText.BytesToText(name).TrimEnd('_');
			Element = EnemizerElements.None;
		}
		public byte[] GetLevelMultBytes()
		{
			return new byte[] { Level, XpMultiplier, GpMultiplier };
		}
		public byte[] GetGraphicDataBytes()
		{
			return graphicData.Concat(new byte[] { Palette1, Palette2 }).ToArray();
		}
		public byte[] GetNameBytes()
		{

			string nametrail = Name.PadRight(0x10, '_');
			if (nametrail.Length > 0x10)
			{
				nametrail = nametrail.Substring(0, 0x10);
			}

			return MQText.TextToByte(nametrail, false);
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
			_rawBytes[0x0D] = spByte;

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

	public class EnemyPalettes
	{
		private const int paletteBank = 0x09;
		private const int paletteOffset = 0x8000;
		private const int paletteSize = 0x10;
		private const int paletteQty = 0x40;

		public Dictionary<int, Palette> Data;

		public EnemyPalettes(FFMQRom rom)
		{ 
			Data = rom.GetFromBank(paletteBank, paletteOffset, paletteSize * paletteQty).Chunk(paletteSize).Select((p, i) => (i, new Palette(p))).ToDictionary(p => p.i, p => p.Item2);
		}

		public void Write(FFMQRom rom)
		{
			rom.PutInBank(paletteBank, paletteOffset, Data.SelectMany(f => f.Value.GetBytes()).ToArray());
		}
	}
}
