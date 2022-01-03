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

    public enum LevelingCurve : int
    {
        [Description("0.5x")]
        Half = 0,
        [Description("1x")]
        Normal,
        [Description("1.5x")]
        OneAndHalf,
        [Description("2x")]
        Double,
        [Description("3x")]
        Triple,
        [Description("4x")]
        Quadruple,
    }

    public partial class FFMQRom : SnesRom
    {

        public void SetLevelingCurve(Flags flags)
        {
            byte xpconst1 = 0x3d;
            byte xpconst2 = 0x0d;

            switch (flags.LevelingCurve)
            {
                case LevelingCurve.Half:
                    xpconst1 = 0x6E;
                    xpconst2 = 0x18;
                    break;
                case LevelingCurve.Normal:
                    return;
                case LevelingCurve.OneAndHalf:
                    xpconst1 = 0x26;
                    xpconst2 = 0x08;
                    break;
                case LevelingCurve.Double:
                    xpconst1 = 0x1C;
                    xpconst2 = 0x06;
                    break;
                case LevelingCurve.Triple:
                    xpconst1 = 0x14;
                    xpconst2 = 0x04;
                    break;
                case LevelingCurve.Quadruple:
                    xpconst1 = 0x0E;
                    xpconst2 = 0x03;
                    break;
            }

            // for level up check after battle
            PutInBank(0x03, 0xAE1F, new byte[] { xpconst1 });
            PutInBank(0x03, 0xAE2D, new byte[] { xpconst2 });

            // for next level in status screen
            PutInBank(0x03, 0x9C11, new byte[] { xpconst1 });
            PutInBank(0x03, 0x9C1F, new byte[] { xpconst2 });
        }

    }
}
