﻿using RomUtilities;
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
        [Description("2.5x")]
        DoubleHalf,
        [Description("3x")]
        Triple,
        [Description("4x")]
        Quadruple,
    }

    public partial class FFMQRom : SnesRom
    {

        public void SetLevelingCurve(LevelingCurve levelingcurve)
        {
            byte xpconst1 = 0x3d;
            byte xpconst2 = 0x0d;

            switch (levelingcurve)
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
                case LevelingCurve.DoubleHalf:
                    xpconst1 = 0x18;
                    xpconst2 = 0x05;
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

        public void CompanionRoutines()
        {
            // Check char opcode
            PutInBank(0x11, 0x8400, Blob.FromHex("08E230A717E617AE9010E0FFF00DCD920EF00E186904CD920EF00628A71785176B28E617E6176B"));
            PutInBank(0x00, 0xff00, Blob.FromHex("2200841160"));

            // Switch companion code
            var companionSwitch = new ScriptBuilder(new List<string>{
                $"050f{(int)Companion.Kaeli:X2}[06]",
                $"2e{(int)NewGameFlagsList.KaeliCured:X2}[04]",                  // 01 is Elixir Quest done?
				$"2e{(int)NewGameFlagsList.ShowSickKaeli:X2}[05]",               // 02 No, is Kaeli Sick?
				$"23{(int)NewGameFlagsList.ShowForestaKaeli:X2}00",              // 03 No, show Foresta
				$"23{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",                 // 04 then available in Windia
				"00",															 // 05
				$"050f{(int)Companion.Tristam:X2}[11]",
                $"23{(int)NewGameFlagsList.ShowFireburgTristam:X2}",	         // 07 Tristam is at Fireburg
				$"2e{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[10]", // 08 Is bone quest done?
				$"23{(int)NewGameFlagsList.ShowSandTempleTristam:X2}",           // 09 No, show at Sand Temple
				"00",												             // 10
				$"050f{(int)Companion.Phoebe:X2}[16]",
                $"2e{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[14]",       // 12 is WintryCave Quest done?
				$"23{(int)NewGameFlagsList.ShowLibraTemplePhoebe:X2}00",         // 13 No, show in Libra Temple
				$"23{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}",		         // 14 Yes, show in Windia
				"00",												             // 15
				$"050f{(int)Companion.Reuben:X2}[18]",
                $"23{(int)NewGameFlagsList.ShowFireburgReuben:X2}00",            // 17 Reuben is always in Fireburg
				"00",
                });

            companionSwitch.Update(0xFF80);
            companionSwitch.Write(this);

            PutInBank(0x00, 0x9e8c, Blob.FromHex("00ff"));
        }
    }
}
