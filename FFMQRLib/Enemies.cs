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
        [Description("100%")]
        Normal,
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
        public Enemy this[int enemyid]
        {
            get => _enemies[enemyid];
            set => _enemies[enemyid] = value;
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
        private short ScaleHP(short value, int scaling, int spread, MT19337 rng)
        {
            int randomizedScaling = scaling;
            if (spread != 0)
            {
                int max = scaling + spread;
                int min = Max(25, scaling - spread);

                randomizedScaling = (int)Exp(((double)rng.Next() / uint.MaxValue) * (Log(max) - Log(min)) + Log(min));
            }
            return (short)Min(0xFFFF, Max(0x01, value * randomizedScaling / 100));
        }
        public void ScaleEnemies(Flags flags, MT19337 rng)
        {
            int scaling = 100;
            int spread = 0;

            switch (flags.EnemiesScaling)
            {
                case EnemiesScaling.Quarter: scaling = 25; break;
                case EnemiesScaling.Half: scaling = 50; break;
                case EnemiesScaling.Normal: scaling = 100; break;
                case EnemiesScaling.OneAndHalf: scaling = 150; break;
                case EnemiesScaling.Double: scaling = 200; break;
                case EnemiesScaling.DoubleAndHalf: scaling = 250; break;
                case EnemiesScaling.Triple: scaling = 300; break;
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
        
        public short HP { get; set; }
        public byte AttackPower { get; set; }
        public byte DamageReduction { get; set; }
        public byte Speed { get; set; }
        public byte MagicPower { get; set; }
        public byte Accuracy { get; set; }
        public byte Evasion { get; set; }
        private int _Id;

        public Enemy(int enemyId, FFMQRom rom)
        {
            _rawBytes = rom.GetFromBank(RomOffsets.EnemiesStatsBank, RomOffsets.EnemiesStatsAddress + (enemyId * RomOffsets.EnemiesStatsLength), RomOffsets.EnemiesStatsLength);

            _Id = enemyId;
            HP = (short)(_rawBytes[1] * 0x100 + _rawBytes[0]);
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
    public class EnemiesAttacks
    {
        private List<EnemyAttack> _enemyAttacks;

        public EnemiesAttacks(FFMQRom rom)
        {
            _enemyAttacks = new List<EnemyAttack>();

            for (int i = 0; i < RomOffsets.EnemiesAttacksQty; i++)
            {
                _enemyAttacks.Add(new EnemyAttack(i, rom));
            }
        }
        public EnemyAttack this[int attackid]
        {
            get => _enemyAttacks[attackid];
            set => _enemyAttacks[attackid] = value;
        }
        public void Write(FFMQRom rom)
        {
            foreach (EnemyAttack e in _enemyAttacks)
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
                case EnemiesScaling.Normal: scaling = 100; break;
                case EnemiesScaling.OneAndHalf: scaling = 150; break;
                case EnemiesScaling.Double: scaling = 200; break;
                case EnemiesScaling.DoubleAndHalf: scaling = 250; break;
                case EnemiesScaling.Triple: scaling = 300; break;
            }
           
            switch (flags.EnemiesScalingSpread)
            {
                case EnemiesScalingSpread.None: spread = 0; break;
                case EnemiesScalingSpread.Quarter: spread = 25; break;
                case EnemiesScalingSpread.Half: spread = 50; break;
                case EnemiesScalingSpread.Full: spread = 100; break;
            }

            foreach (EnemyAttack e in _enemyAttacks)
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
    public class EnemyAttack
    {
        private Blob _rawBytes;
        public byte Power { get; set; }
        private int _Id;

        public EnemyAttack(int attackId, FFMQRom rom)
        {
            _rawBytes = rom.GetFromBank(RomOffsets.EnemiesAttacksBank, RomOffsets.EnemiesAttacksAddress + (attackId * RomOffsets.EnemiesAttacksLength), RomOffsets.EnemiesAttacksLength);
            
            _Id = attackId;
            Power = _rawBytes[2];
        }
        public void Write(FFMQRom rom)
        { 
            _rawBytes[2] = Power;
            rom.PutInBank(RomOffsets.EnemiesAttacksBank, RomOffsets.EnemiesAttacksAddress + (_Id * RomOffsets.EnemiesAttacksLength), _rawBytes);
        }
    }
}
