using FFMQLib;
using Microsoft.VisualBasic;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static System.Math;

namespace FFMQLib
{
	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLink : ICloneable
	{
		private const int EnemiesAttackLinksAddress = 0xC6FF; // Bank 02
		private const int EnemiesAttackLinksBank = 0x02;
		private const int EnemiesAttackLinksLength = 0x09;

		public byte AttackPattern { get; set; }
		public List<EnemyAttackIds> Attacks { get; set; }
		public bool CastHeal { get; set; }
		public bool CastCure { get; set; }
		public EnemyIds Id { get; set; }
		public int AttackCount => Attacks.Count(a => a != EnemyAttackIds.Nothing);
		public List<int> NeedsSlotsFilled { get; set; }

		public EnemyAttackLink(int id, FFMQRom rom)
		{
			GetFromBytes(id, rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress + (id * EnemiesAttackLinksLength), EnemiesAttackLinksLength));
		}
		public EnemyAttackLink(int id, byte[] rawBytes)
		{
			GetFromBytes(id, rawBytes);
		}
		public EnemyAttackLink(EnemyIds id, EnemyAttackLink link)
		{
			GetFromBytes((int)id, link.GetBytes());
		}
		public void GetFromBytes(int id, byte[] rawBytes)
		{
			Id = (EnemyIds)id;
			AttackPattern = rawBytes[0];
			Attacks = rawBytes[1..7].Select(a => (EnemyAttackIds)a).ToList();
			CastHeal = rawBytes[7] == (byte)EnemyAttackIds.HealSelf;
			CastCure = rawBytes[8] == (byte)EnemyAttackIds.CureSelf;
			NeedsSlotsFilled = new();
		}
		private EnemyAttackLink(EnemyIds id, byte attackPattern, List<EnemyAttackIds> attacks, bool castHeal, bool castCure)
		{
			Id = id;
			AttackPattern = attackPattern;
			Attacks = attacks;
			CastHeal = castHeal;
			CastCure = castCure;
		}

		public object Clone()
		{
			return new EnemyAttackLink(Id, AttackPattern, Attacks, CastHeal, CastCure);
		}
		public byte[] GetBytes()
		{
			return new[] { AttackPattern, (byte)Attacks[0], (byte)Attacks[1], (byte)Attacks[2], (byte)Attacks[3], (byte)Attacks[4], (byte)Attacks[5], CastHeal ? (byte)EnemyAttackIds.HealSelf : (byte)EnemyAttackIds.Nothing, CastCure ? (byte)EnemyAttackIds.CureSelf : (byte)EnemyAttackIds.Nothing };
		}
		public byte[] GetAttackBytes()
		{
			return Attacks.Select(a => (byte)a).ToArray();
		}
	}
	// link from link table, from the SQL world, describing a many-to-many relationship
	public class EnemyAttackLinks
	{
		public Dictionary<EnemyIds, EnemyAttackLink> Data { get; }
		//private Blob _darkKingAttackLinkBytes;
		public EnemyAttackIds IceGolemDesperateAttack { get; set; }

		private const int EnemiesAttackLinksAddress = 0xC6FF; // Bank 02
		private const int EnemiesAttackLinksBank = 0x02;
		private const int EnemiesAttackLinksLength = 0x09;
		private const int EnemiesAttackLinksQty = 0x53;

		// Dark King Attack Links, separate from EnemiesAttacks
		// Dark King has its own byte range on top of attack links ids 79 through 82
		private const int DarkKingAttackLinkAddress = 0xD09E; // Bank 02
		private const int DarkKingAttackLinkBank = 0x02;
		private const int DarkKingAttackLinkQty = 0x0C;

		private const int IceGolemDesperateAttackBank = 0x02;
		private const int IceGolemDesperateAttackOffset = 0xAAC9;
		public EnemyAttackLinks(FFMQRom rom)
		{
			Data = rom.GetFromBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress, EnemiesAttackLinksQty * EnemiesAttackLinksLength).ToBytes().Chunk(EnemiesAttackLinksLength).Select((l, i) => new EnemyAttackLink(i, l)).ToDictionary(l => l.Id, l => l);

			IceGolemDesperateAttack = (EnemyAttackIds)rom.GetFromBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, 1)[0];

			// Wyvern, Hydra, DK2 and Dk3 need to have a specific slot filled to avoid softlock
			Data[EnemyIds.DualheadHydra].NeedsSlotsFilled.Add(3);
			Data[EnemyIds.TwinheadWyvern].NeedsSlotsFilled.Add(3);
			Data[EnemyIds.DarkKingWeapons].NeedsSlotsFilled.Add(3);
			Data[EnemyIds.DarkKingSpider].NeedsSlotsFilled.Add(2);
		}
		/*
		public EnemyAttackLink this[int attackid]
		{
			get => _EnemyAttackLinks[attackid];
			set => _EnemyAttackLinks[attackid] = value;
		}*/
		public IList<EnemyAttackLink> GetList()
		{
			return Data.Values.ToList().AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{

			rom.PutInBank(EnemiesAttackLinksBank, EnemiesAttackLinksAddress, Data.Values.SelectMany(l => l.GetBytes()).ToArray());

			rom.PutInBank(DarkKingAttackLinkBank, DarkKingAttackLinkAddress, Data.Values.Where(l => l.Id == EnemyIds.DarkKingWeapons || l.Id == EnemyIds.DarkKingSpider).OrderBy(l => l.Id).SelectMany(l => l.GetAttackBytes()).ToArray());
			rom.PutInBank(IceGolemDesperateAttackBank, IceGolemDesperateAttackOffset, new byte[] { (byte)IceGolemDesperateAttack });
		}
	}
	public class Attacks
	{
		private List<Attack> _attacks;

		private const int AttacksQty = 0xA9;

		public Attacks(FFMQRom rom)
		{
			_attacks = new List<Attack>();

			for (int i = 0; i < AttacksQty; i++)
			{
				_attacks.Add(new Attack(i, rom));
			}
		}
		public Attack this[int attackid]
		{
			get => _attacks[attackid];
			set => _attacks[attackid] = value;
		}
		public IList<Attack> AllAttacks()
		{
			return _attacks.AsReadOnly();
		}
		public void Write(FFMQRom rom)
		{
			foreach (Attack e in _attacks)
			{
				e.Write(rom);
			}
		}
	}
	public class Attack
	{
		private Blob _rawBytes;
		public byte Unknown1 { get; set; }
		public byte Unknown2 { get; set; }
		public byte Power { get; set; }
		public byte AttackType { get; set; }
		public byte AttackSound { get; set; }
		// my suspicion is that this unknown (or one of the other two) are responsible for targeting self, one PC or both PC.
		public byte Unknown3 { get; set; }
		public byte AttackTargetAnimation { get; set; }
		private int _Id;

		private const int AttacksAddress = 0xBC78; // Bank 02
		private const int AttacksBank = 0x02;
		private const int AttacksLength = 0x07;

		public Attack(int id, FFMQRom rom)
		{
			_rawBytes = rom.GetFromBank(AttacksBank, AttacksAddress + (id * AttacksLength), AttacksLength);

			_Id = id;
			Unknown1 = _rawBytes[0];
			Unknown2 = _rawBytes[1];
			Power = _rawBytes[2];
			AttackType = _rawBytes[3];
			AttackSound = _rawBytes[4];
			Unknown3 = _rawBytes[5];
			AttackTargetAnimation = _rawBytes[6];
		}
		public int Id()
		{
			return _Id;
		}

		public void Write(FFMQRom rom)
		{
			_rawBytes[0] = Unknown1;
			_rawBytes[1] = Unknown2;
			_rawBytes[2] = Power;
			_rawBytes[3] = AttackType;
			_rawBytes[4] = AttackSound;
			_rawBytes[5] = Unknown3;
			_rawBytes[6] = AttackTargetAnimation;
			rom.PutInBank(AttacksBank, AttacksAddress + (_Id * AttacksLength), _rawBytes);
		}
	}

	public class AttackPattern
	{
		public int Id { get; set; }
		public int Common { get; set; }
		public int Uncommon { get; set; }
		public int Rare { get; set; }
		public int Count { get => Common + Uncommon + Rare; }
		public int Opener { get; set; }

		public AttackPattern(int id, int common, int uncommon, int rare, int opener)
		{
			Id = id;
			Common = common;
			Uncommon = uncommon;
			Rare = rare;
			Opener = opener;
		}
	}
}
