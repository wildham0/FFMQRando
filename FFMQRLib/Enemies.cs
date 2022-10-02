using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;

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
        [Description("150%")]
        OneAndHalf,
        [Description("200%")]
        Double,
        [Description("250%")]
        DoubleAndHalf,
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
    public enum EnemizerAttacks : int
    {
        [Description("Standard")]
        Normal = 0,
        [Description("Safe")]
        Safe,
        [Description("Chaos")]
        Chaos,
        [Description("Self-Destruct")]
        SelfDestruct,
    }
    public class EnemiesStats
    {
        private List<Enemy> _enemies;

        public EnemiesStats(FFMQRom rom)
        {
            _enemies = new List<Enemy>();

            for (int i = 0; i < RomOffsets.EnemiesStatsQty; i++)
            {
                _enemies.Add(new Enemy(i, rom));
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
                e.Write(rom);
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
        public void ScaleEnemies(Flags flags, MT19337 rng)
        {
            int scaling = 100;
            int spread = 0;

            switch (flags.EnemiesScaling)
            {
                case EnemiesScaling.Quarter: scaling = 25; break;
                case EnemiesScaling.Half: scaling = 50; break;
                case EnemiesScaling.ThreeQuarter: scaling = 75; break;
                case EnemiesScaling.Normal: scaling = 100; break;
                case EnemiesScaling.OneAndHalf: scaling = 150; break;
                case EnemiesScaling.Double: scaling = 200; break;
                case EnemiesScaling.DoubleAndHalf: scaling = 250; break;
            }

            switch (flags.EnemiesScalingSpread)
            {
                case EnemiesScalingSpread.None: spread = 0; break;
                case EnemiesScalingSpread.Quarter: spread = 25; break;
                case EnemiesScalingSpread.Half: spread = 50; break;
                case EnemiesScalingSpread.Full: spread = 100; break;
            }

            foreach (Enemy e in _enemies)
            {
                e.HP = ScaleHP(e.HP, scaling, spread, rng);
                e.AttackPower = ScaleStat(e.AttackPower, scaling, spread, rng);
                e.DamageReduction = ScaleStat(e.DamageReduction, scaling, spread, rng);
                e.Speed = ScaleStat(e.Speed, scaling, spread, rng);
                e.MagicPower = ScaleStat(e.MagicPower, scaling, spread, rng);
                e.Accuracy = ScaleStat(e.Accuracy, scaling, spread, rng);
                e.Evasion = ScaleStat(e.Evasion, scaling, spread, rng);
            }
        }
    }
    public class Enemy
    {
        private Blob _rawBytes;
        
        public ushort HP { get; set; }
        public byte AttackPower { get; set; }
        public byte DamageReduction { get; set; }
        public byte Speed { get; set; }
        public byte MagicPower { get; set; }
        public byte Accuracy { get; set; }
        public byte Evasion { get; set; }
        private int _Id;
        public int Id()
        {
            return _Id;
        }

        public Enemy(int id, FFMQRom rom)
        {
            _rawBytes = rom.GetFromBank(RomOffsets.EnemiesStatsBank, RomOffsets.EnemiesStatsAddress + (id * RomOffsets.EnemiesStatsLength), RomOffsets.EnemiesStatsLength);

            _Id = id;
            HP = (ushort)(_rawBytes[1] * 0x100 + _rawBytes[0]);
            AttackPower = _rawBytes[2];
            DamageReduction = _rawBytes[3];
            Speed = _rawBytes[4];
            MagicPower = _rawBytes[5];
            Accuracy = _rawBytes[0x0a];
            Evasion = _rawBytes[0x0b];
        }
        public void Write(FFMQRom rom)
        {
            _rawBytes[0] = (byte)(HP % 0x100);
            _rawBytes[1] = (byte)(HP / 0x100);
            _rawBytes[2] = AttackPower;
            _rawBytes[3] = DamageReduction;
            _rawBytes[4] = Speed;
            _rawBytes[5] = MagicPower;
            _rawBytes[0x0a] = Accuracy;
            _rawBytes[0x0b] = Evasion;
            rom.PutInBank(RomOffsets.EnemiesStatsBank, RomOffsets.EnemiesStatsAddress + (_Id * RomOffsets.EnemiesStatsLength), _rawBytes);
        }
    }
    // link from link table, from the SQL world, describing a many-to-many relationship
    public class EnemyAttackLink
    {
        private Blob _rawBytes;
        
        public byte Unknown1 { get; set; }
        public byte[] Attacks { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        private int _Id;
        public int Id()
        {
            return _Id;
        }
        public List<int> NeedsSlotsFilled()
        {
            // Twinhead Wyvern and Twinhead Hydra need their 4th attack slot filled, or the game locks up.
            if(_Id == 76 || _Id == 77)
            {
                return new List<int>(new int[] {3});
            }
            return new List<int>();
        }

        public EnemyAttackLink(int id, FFMQRom rom)
        {
            _rawBytes = rom.GetFromBank(RomOffsets.EnemiesAttackLinksBank, RomOffsets.EnemiesAttackLinksAddress + (id * RomOffsets.EnemiesAttackLinksLength), RomOffsets.EnemiesAttackLinksLength);

            _Id = id;
            Unknown1 = _rawBytes[0];
            Attacks = new byte[6];
            Attacks[0] = _rawBytes[1];
            Attacks[1] = _rawBytes[2];
            Attacks[2] = _rawBytes[3];
            Attacks[3] = _rawBytes[4];
            Attacks[4] = _rawBytes[5];
            Attacks[5] = _rawBytes[6];
            Unknown2 = _rawBytes[7];
            Unknown3 = _rawBytes[8];
        }
        public void Write(FFMQRom rom)
        {
            _rawBytes[0] = Unknown1;
            _rawBytes[1] = Attacks[0];
            _rawBytes[2] = Attacks[1];
            _rawBytes[3] = Attacks[2];
            _rawBytes[4] = Attacks[3];
            _rawBytes[5] = Attacks[4];
            _rawBytes[6] = Attacks[5];
            _rawBytes[7] = Unknown2;
            _rawBytes[8] = Unknown3;
            rom.PutInBank(RomOffsets.EnemiesAttackLinksBank, RomOffsets.EnemiesAttackLinksAddress + (_Id * RomOffsets.EnemiesAttackLinksLength), _rawBytes);
        }
    }
    // link from link table, from the SQL world, describing a many-to-many relationship
    public class EnemyAttackLinks
    {
        private List<EnemyAttackLink> _EnemyAttackLinks;
        private Blob _darkKingAttackLinkBytes;

        public EnemyAttackLinks(FFMQRom rom)
        {
            _EnemyAttackLinks = new List<EnemyAttackLink>();

            for (int i = 0; i < RomOffsets.EnemiesAttackLinksQty; i++)
            {
                _EnemyAttackLinks.Add(new EnemyAttackLink(i, rom));
            }

            _darkKingAttackLinkBytes = rom.GetFromBank(RomOffsets.DarkKingAttackLinkBank, RomOffsets.DarkKingAttackLinkAddress, RomOffsets.DarkKingAttackLinkQty);
        }
        public EnemyAttackLink this[int attackid]
        {
            get => _EnemyAttackLinks[attackid];
            set => _EnemyAttackLinks[attackid] = value;
        }
        public IList<EnemyAttackLink> AllAttacks()
        {
            return _EnemyAttackLinks.AsReadOnly();
        }
        public void Write(FFMQRom rom, Flags flags)
        {
            foreach (EnemyAttackLink e in _EnemyAttackLinks)
            {
                e.Write(rom);
            }

            rom.PutInBank(RomOffsets.DarkKingAttackLinkBank, RomOffsets.DarkKingAttackLinkAddress, _darkKingAttackLinkBytes);
        }
        public void ShuffleAttacks(Flags flags, MT19337 rng)
        {
            var possibleAttacks = new List<byte>();
            possibleAttacks.Add(0x19);
            possibleAttacks.Add(0x1B);
            for(byte i = 0x40; i <= 0xDB; i++)
            {
                possibleAttacks.Add(i);
            }

            switch (flags.EnemizerAttacks) 
            {
                case EnemizerAttacks.Safe:
                    // Remove self destruct as it makes bosses super easy and some regular monster super hard
                    possibleAttacks.Remove(0xC1);
                    // Remove dark king attacks as they make regular monsters impossible early on
                    for(byte i = 0xCA; i <= 0xD6; i++)
                    {
                        possibleAttacks.Remove(i);
                    }
                    _ShuffleAttacks(possibleAttacks, rng);
                    break;
                case EnemizerAttacks.Chaos:
                    _ShuffleAttacks(possibleAttacks, rng);
                    break;
                case EnemizerAttacks.SelfDestruct:
                    foreach(var ea in _EnemyAttackLinks)
                    {
                        ea.Unknown1 = 0x01;
                        ea.Attacks[0] = 0xC1;
                        ea.Attacks[1] = 0xFF;
                        ea.Attacks[2] = 0xFF;
                        ea.Attacks[3] = 0xFF;
                        ea.Attacks[4] = 0xFF;
                        ea.Attacks[5] = 0xFF;

                        // Some enemies require certain slots to be filled, or the game locks up
                        foreach(var slot in ea.NeedsSlotsFilled())
                        {
                            ea.Attacks[slot] = 0xC1;
                        }
                    }

                    // See the comment in _ShuffleAttacks() for more info
                    for(int i = 0; i < RomOffsets.DarkKingAttackLinkQty; i++)
                    {
                        _darkKingAttackLinkBytes[i] = 0xC1;
                    }
                    break;
                default:
                    break;

            }
        }

        private void _ShuffleAttacks(List<byte> possibleAttacks, MT19337 rng)
        {
            foreach(var ea in _EnemyAttackLinks)
            {
                uint noOfAttacks = (rng.Next() % 5) + 1;

                for(uint i = 0; i < 6; i++)
                {
                    ea.Attacks[i] = 0xFF;
                }

                for(uint i = 0; i < noOfAttacks; i++)
                {
                    ea.Attacks[i] = possibleAttacks[(int)(rng.Next() % possibleAttacks.Count)]; 
                }

                // Some values of Unknown1 (e.g. 0x0B) result in the third (or other) attack slot being used
                // regardless of it being 0xFF (which is an ignored slot for most other Unknown1 values)
                if(noOfAttacks < 3)
                {
                    ea.Unknown1 = 0x01;
                }

                // Similarly, most Unknown1 values do not use attack slots 5 and 6, but 0x0D and 0x0C do.
                if(noOfAttacks > 4)
                {
                    ea.Unknown1 = 0x0D;
                }

                // Some enemies require certain slots to be filled, or the game locks up
                foreach(var slot in ea.NeedsSlotsFilled())
                {
                    if(ea.Attacks[slot] == 0xFF)
                    {
                        ea.Attacks[slot] = possibleAttacks[(int)(rng.Next() % possibleAttacks.Count)];
                    }
                }
            }

            // Dark King has its own byte range on top of attack links ids 79 through 82
            // TODO: The Ice Golem has a special healing blizzard it does when it has < 50% HP, probably want to randomise that as well? Maybe more enemies have a similar thing?
            for(int i = 0; i < RomOffsets.DarkKingAttackLinkQty; i++)
            {
                _darkKingAttackLinkBytes[i] = possibleAttacks[(int)(rng.Next() % possibleAttacks.Count)];
            }
        }
    }
    public class Attacks
    {
        private List<Attack> _attacks;

        public Attacks(FFMQRom rom)
        {
            _attacks = new List<Attack>();

            for (int i = 0; i < RomOffsets.AttacksQty; i++)
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
        public void Write(FFMQRom rom, Flags flags)
        {
            foreach (Attack e in _attacks)
            {
                e.Write(rom);
            }
        }
        public void ScaleAttacks(Flags flags, MT19337 rng)
        {
            int scaling = 100;
            int spread = 0;

            switch (flags.EnemiesScaling)
            {
                case EnemiesScaling.Quarter: scaling = 25; break;
                case EnemiesScaling.Half: scaling = 50; break;
                case EnemiesScaling.ThreeQuarter: scaling = 75; break;
                case EnemiesScaling.Normal: scaling = 100; break;
                case EnemiesScaling.OneAndHalf: scaling = 150; break;
                case EnemiesScaling.Double: scaling = 200; break;
                case EnemiesScaling.DoubleAndHalf: scaling = 250; break;
            }
           
            switch (flags.EnemiesScalingSpread)
            {
                case EnemiesScalingSpread.None: spread = 0; break;
                case EnemiesScalingSpread.Quarter: spread = 25; break;
                case EnemiesScalingSpread.Half: spread = 50; break;
                case EnemiesScalingSpread.Full: spread = 100; break;
            }

            foreach (Attack e in _attacks)
            {
                int randomizedScaling = scaling;
                if (spread != 0)
                {
                    int max = scaling + spread;
                    int min = Max(25, scaling - spread);

                    randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
                }
                e.Power = (byte)Min(0xFF, Max(0x01, e.Power * randomizedScaling / 100));
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

        public Attack(int id, FFMQRom rom)
        {
            _rawBytes = rom.GetFromBank(RomOffsets.AttacksBank, RomOffsets.AttacksAddress + (id * RomOffsets.AttacksLength), RomOffsets.AttacksLength);

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
            rom.PutInBank(RomOffsets.AttacksBank, RomOffsets.AttacksAddress + (_Id * RomOffsets.AttacksLength), _rawBytes);
        }
    }
}
