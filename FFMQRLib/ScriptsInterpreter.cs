using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
    public enum ScriptArgumentType
    { 
        Address = 0,
        AddressByte,
        AddressWord,
        AddressSWord,
        AddressDWord,
        RelativeAddressByte,
        RelativeAddressWord,
        RelativeAddressSWord,
        RelativeAddressDWord,
        Value
    }

    public class ScriptArgument
    { 
        public ScriptArgumentType Type { get; set; }
        public int Size { get; set; }
        public bool CreateLabel { get; set; }
        public ScriptArgument(ScriptArgumentType _type, int _size, bool _createlabel = false)
        {
            Type = _type;
            Size = _size;
            CreateLabel = _createlabel;
        }
    }
    public class ScriptCode
    { 
        public ushort Index { get; set; }
        public string Command { get; set; }
        public List<ScriptArgument> Arguments { get; set; }
        public ScriptCode(ushort _index, string _command, List<ScriptArgument> _arguments = null)
        {
            Index = _index;
            Command = _command;
            Arguments = _arguments ?? new List<ScriptArgument>();
        }
        public ScriptCode(ScriptCode copyscript)
        {
            Index = copyscript.Index;
            Command = copyscript.Command;
            Arguments = copyscript.Arguments.ToList();
        }
        public ScriptCode()
        {
            Index = 0x0000;
            Command = "";
            Arguments = new List<ScriptArgument>();
        }
        public byte PrimaryIndex
        {
            get => (byte)(Index / 0x100);
            set => Index = (ushort)((value * 0x100) + (Index & 0x00FF));
        }
        public byte SecondaryIndex
        {
            get => (byte)(Index & 0x00FF);
            set => Index = (ushort)((value) + (Index & 0xFF00));
        }
    }
    public static class ScriptCodesList
    {
        public static ScriptCode Code0000 = new(0x0000, "END");
        public static ScriptCode Code0001 = new(0x0100, "LINEFEED1");
        public static ScriptCode Code0002 = new(0x0200, "LINEFEED2");
        public static ScriptCode Code0003 = new(0x0300, "UNKNOWN_03");
        public static ScriptCode Code0004 = new(0x0400, "RETURN");
        public static ScriptCode Code0005 = new(0x0500, "SUB");
        public static ScriptCode Code0006 = new(0x0600, "UNKNOWN_06");
        public static ScriptCode Code0007 = new(0x0700, "GOSUB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 3, true) });
        public static ScriptCode Code0008 = new(0x0800, "GOSUB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0009 = new(0x0900, "RUNL arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 3, true) });
        public static ScriptCode Code000A = new(0x0A00, "GOTO arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code000B = new(0x0B00, "IF s[9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code000C = new(0x0C00, "arg0 = arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code000D = new(0x0D00, "arg0 = arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2), new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code000E = new(0x0E00, "arg0 = arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2), new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code000F = new(0x0F00, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0010 = new(0x1000, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0011 = new(0x1100, "arg0 = b[9E]", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2) });
        public static ScriptCode Code0012 = new(0x1200, "arg0 = w[9E]", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2) });
        public static ScriptCode Code0013 = new(0x1300, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0014 = new(0x1400, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0015 = new(0x1500, "[25] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0016 = new(0x1600, "UNKNOWN_16 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0017 = new(0x1700, "SYSFLAG arg0 = FALSE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0018 = new(0x1800, "UNKNOWN_18");
        public static ScriptCode Code0019 = new(0x1900, "UNKNOWN_19");
        public static ScriptCode Code001A = new(0x1A00, "PRIMARYTEXTBOX arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code001B = new(0x1B00, "SECONDARYTEXTBOX arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code001C = new(0x1C00, "UNKNOWN_1C");
        public static ScriptCode Code001D = new(0x1D00, "COMPANION arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code001E = new(0x1E00, "ITEM arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code001F = new(0x1F00, "LOCATION arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0020 = new(0x2000, "ENEMY arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0021 = new(0x2100, "SKIP");
        public static ScriptCode Code0022 = new(0x2200, "UNKNOWN_22");
        public static ScriptCode Code0023 = new(0x2300, "GAMEFLAG arg0 = TRUE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0024 = new(0x2400, "[28] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 4) });
        public static ScriptCode Code0025 = new(0x2500, "[1E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0026 = new(0x2600, "[3F] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code0027 = new(0x2700, "[27] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0028 = new(0x2800, "[1D] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0029 = new(0x2900, "SYSFLAG arg0 = TRUE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code002A = new(0x2A00, "ACTIONS");
        public static ScriptCode Code002B = new(0x2B00, "GAMEFLAG arg0 = FALSE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code002C = new(0x2C00, "ACTION arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code002D = new(0x2D00, "MEMOFFSET = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code002E = new(0x2E00, "IF GAMEFLAG arg0 = TRUE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code002F = new(0x2F00, "MEMOFFSET = ITEMS");

        public static ScriptCode Code0500 = new(0x0500, "UNKNOWN_0500");
        public static ScriptCode Code0501 = new(0x0501, "UNKNOWN_0501 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0502 = new(0x0502, "GOTOL arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 3, true) });
        public static ScriptCode Code0503 = new(0x0503, "GOTO [9E]");
        public static ScriptCode Code0504 = new(0x0504, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true), });
        public static ScriptCode Code0505 = new(0x0505, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true), });
        public static ScriptCode Code0506 = new(0x0506, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true), });
        public static ScriptCode Code0507 = new(0x0507, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true), });
        public static ScriptCode Code0508 = new(0x0508, "UNUSED_08");
        public static ScriptCode Code0509 = new(0x0509, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true), });
        public static ScriptCode Code050A = new(0x050A, "UNUSED_0A");
        public static ScriptCode Code050B = new(0x050B, "IF GAMEFLAG arg0 = FALSE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code050C = new(0x050C, "IF MEMFLAG arg0 = TRUE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code050D = new(0x050D, "IF MEMFLAG arg0 = FALSE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code050E = new(0x050E, "GOSUB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code050F = new(0x050F, "UNUSED_0F");
        public static ScriptCode Code0510 = new(0x0510, "UNUSED_10");
        public static ScriptCode Code0511 = new(0x0511, "GOSUB [9E]");
        public static ScriptCode Code0512 = new(0x0512, "IF GAMEFLAG arg0 = TRUE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0513 = new(0x0513, "IF GAMEFLAG arg0 = FALSE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0514 = new(0x0514, "IF MEMFLAG arg0 = TRUE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0515 = new(0x0515, "IF MEMFLAG arg0 = FALSE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0516 = new(0x0516, "UNUSED_16");
        public static ScriptCode Code0517 = new(0x0517, "GOSUBL [9E]");
        public static ScriptCode Code0518 = new(0x0518, "UNKNOWN_18 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code0519 = new(0x0519, "UNKNOWN_19");
        public static ScriptCode Code051A = new(0x051A, "UNUSED_1A");
        public static ScriptCode Code051B = new(0x051B, "UNUSED_1B");
        public static ScriptCode Code051C = new(0x051C, "UNUSED_1C");
        public static ScriptCode Code051D = new(0x051D, "UNKNOWN_1D arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code051E = new(0x051E, "UNKNOWN_1E arg0 arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code051F = new(0x051F, "UNUSED_1F");
        public static ScriptCode Code0520 = new(0x0520, "[1A] -= b40");
        public static ScriptCode Code0521 = new(0x0521, "[1A] += b40");
        public static ScriptCode Code0522 = new(0x0522, "[1A] -= b02");
        public static ScriptCode Code0523 = new(0x0523, "UNUSED_23");
        public static ScriptCode Code0524 = new(0x0524, "UNKNOWN_24 arg0 arg1 arg2", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0525 = new(0x0525, "UNKNOWN_25");
        public static ScriptCode Code0526 = new(0x0526, "UNUSED_26");
        public static ScriptCode Code0527 = new(0x0527, "<9E> = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0528 = new(0x0528, "UNUSED_28");
        public static ScriptCode Code0529 = new(0x0529, "<9E> = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code052A = new(0x052A, "UNUSED_2A");
        public static ScriptCode Code052B = new(0x052B, "<9E> = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code052C = new(0x052C, "arg0 = b<9E>", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2) });
        public static ScriptCode Code052D = new(0x052D, "arg0 = w<9E>", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2) });
        public static ScriptCode Code052E = new(0x052E, "UNKNOWN_2E arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code052F = new(0x052F, "UNKNOWN_2F");
        public static ScriptCode Code0530 = new(0x0530, "LOOP");
        public static ScriptCode Code0531 = new(0x0531, "UNKNOWN_31");
        public static ScriptCode Code0532 = new(0x0532, "UNKNOWN_32");
        public static ScriptCode Code0533 = new(0x0533, "UNUSED_33");
        public static ScriptCode Code0534 = new(0x0534, "UNUSED_34");
        public static ScriptCode Code0535 = new(0x0535, "UNKNOWN_35 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0536 = new(0x0536, "UNKNOWN_36 arg0 arg1 arg2 arg3", new List<ScriptArgument> { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0537 = new(0x0537, "BITINVERSE [9E]");
        public static ScriptCode Code0538 = new(0x0538, "UNUSED_38");
        public static ScriptCode Code0539 = new(0x0539, "UNUSED_39");
        public static ScriptCode Code053A = new(0x053A, "arg0 = s[9E]", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2) });
        public static ScriptCode Code053B = new(0x053B, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code053C = new(0x053C, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code053D = new(0x053D, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code053E = new(0x053E, "UNUSED_3E");
        public static ScriptCode Code053F = new(0x053F, "UNUSED_3F");
        public static ScriptCode Code0540 = new(0x0540, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code0541 = new(0x0541, "UNUSED_41");
        public static ScriptCode Code0542 = new(0x0542, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0543 = new(0x0543, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code0544 = new(0x0544, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0545 = new(0x0545, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0546 = new(0x0546, "[9E] += arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code0547 = new(0x0547, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0548 = new(0x0548, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0549 = new(0x0549, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code054A = new(0x054A, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code054B = new(0x054B, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code054C = new(0x054C, "[9E] -= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code054D = new(0x054D, "[9E] *= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code054E = new(0x054E, "[9E] *= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code054F = new(0x054E, "[9E] *= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0550 = new(0x0550, "[9E] *= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0551 = new(0x0551, "[9E] /= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0552 = new(0x0552, "[9E] /= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0553 = new(0x0553, "[9E] /= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0554 = new(0x0554, "[9E] /= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0555 = new(0x0555, "[9E] %= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0556 = new(0x0556, "[9E] %= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0557 = new(0x0557, "[9E] %= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0558 = new(0x0558, "[9E] %= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0559 = new(0x0559, "UNUSED_59");
        public static ScriptCode Code055A = new(0x055A, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code055B = new(0x055B, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code055C = new(0x055C, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code055D = new(0x055D, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code055E = new(0x055E, "[9E] &= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code055F = new(0x055F, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0560 = new(0x0560, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0561 = new(0x0561, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code0562 = new(0x0562, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0563 = new(0x0563, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0564 = new(0x0564, "[9E] |= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code0565 = new(0x0565, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0566 = new(0x0566, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0567 = new(0x0567, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code0568 = new(0x0568, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0569 = new(0x0569, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code056A = new(0x056A, "[9E] ^= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2) });
        public static ScriptCode Code056B = new(0x056B, "[9E] <<= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code056C = new(0x056C, "[9E] >>= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code056D = new(0x056D, "UNKNOWN_6D");
        public static ScriptCode Code056E = new(0x056E, "[9E] = COUNT AT arg0 QTY arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code056F = new(0x056F, "UNKNOWN_6F");
        public static ScriptCode Code0570 = new(0x0570, "UNUSED_70");
        public static ScriptCode Code0571 = new(0x0571, "UNUSED_71");
        public static ScriptCode Code0572 = new(0x0572, "MEMFLAG arg0 = TRUE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0573 = new(0x0573, "MEMFLAG arg0 = FALSE", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0574 = new(0x0574, "UNUSED_74");
        public static ScriptCode Code0575 = new(0x0575, "UNKNOWN_75", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0576 = new(0x0576, "UNKNOWN_76");
        public static ScriptCode Code0577 = new(0x0577, "[9E] = COUNT >> [9E]");
        public static ScriptCode Code0578 = new(0x0578, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0579 = new(0x0579, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code057A = new(0x057A, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code057B = new(0x057B, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code057C = new(0x057C, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code057D = new(0x057D, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code057E = new(0x057E, "[9E] ++");
        public static ScriptCode Code057F = new(0x057F, "[9E] --");
        public static ScriptCode Code0580 = new(0x0580, "arg0 &= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0581 = new(0x0581, "arg0 |= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0582 = new(0x0582, "arg0 ^= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code0583 = new(0x0583, "arg0 ++", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0584 = new(0x0584, "arg0 --", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code0585 = new(0x0585, "arg0 &= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0586 = new(0x0586, "arg0 |= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0587 = new(0x0587, "arg0 ^= arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code0588 = new(0x0588, "arg0 ++", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code0589 = new(0x0589, "arg0 --", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2) });
        public static ScriptCode Code058A = new(0x058A, "UKNOWN_8A");
        public static ScriptCode Code058B = new(0x058B, "IF [1A] < [44] THEN [44] = [1A]");
        public static ScriptCode Code058C = new(0x058C, "IF [1A] >= [46] THEN [46] = [1A]");
        public static ScriptCode Code058D = new(0x058D, "UKNOWN_8D");
        public static ScriptCode Code058E = new(0x058E, "IF MEMFLAG [9E] = TRUE GOTO arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code058F = new(0x058F, "IF MEMFLAG [9E] = FALSE GOTO arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0590 = new(0x0590, "IF MEMFLAG [9E] = TRUE GOSUB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0591 = new(0x0591, "IF MEMFLAG [9E] = FALSE GOSUB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0592 = new(0x0592, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0593 = new(0x0593, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0594 = new(0x0594, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0595 = new(0x0595, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0596 = new(0x0596, "IF [9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0597 = new(0x0597, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0598 = new(0x0598, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code0599 = new(0x0599, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059A = new(0x059A, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059B = new(0x059B, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059C = new(0x059C, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059D = new(0x059D, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059E = new(0x059E, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code059F = new(0x059F, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A0 = new(0x05A0, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A1 = new(0x05A1, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A2 = new(0x05A2, "IF [9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A3 = new(0x05A3, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A4 = new(0x05A4, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A5 = new(0x05A5, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A6 = new(0x05A6, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A7 = new(0x05A7, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A8 = new(0x05A8, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05A9 = new(0x05A9, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AA = new(0x05AA, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AB = new(0x05AB, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AC = new(0x05AC, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AD = new(0x05AD, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AE = new(0x05AE, "IF [9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05AF = new(0x05AF, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B0 = new(0x05B0, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B1 = new(0x05B1, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B2 = new(0x05B2, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B3 = new(0x05B3, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B4 = new(0x05B4, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B5 = new(0x05B5, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B6 = new(0x05B6, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B7 = new(0x05B7, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B8 = new(0x05B8, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05B9 = new(0x05B9, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BA = new(0x05BA, "IF [9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BB = new(0x05BB, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BC = new(0x05BC, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BD = new(0x05BD, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BE = new(0x05BE, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05BF = new(0x05BF, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C0 = new(0x05C0, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C1 = new(0x05C1, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C2 = new(0x05C2, "IF [9E] < arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C3 = new(0x05C3, "IF [9E] <= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C4 = new(0x05C4, "IF [9E] > arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C5 = new(0x05C5, "IF [9E] >= arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C6 = new(0x05C6, "IF [9E] = arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C7 = new(0x05C7, "IF [9E] != arg0 GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C8 = new(0x05C8, "IF [9E] < arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05C9 = new(0x05C9, "IF [9E] <= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05CA = new(0x05CA, "IF [9E] > arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05CB = new(0x05CB, "IF [9E] >= arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05CC = new(0x05CC, "IF [9E] = arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05CD = new(0x05CD, "IF [9E] != arg0 GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05CE = new(0x05CE, "[9E] <<= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code05CF = new(0x05CF, "[9E] >>= arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 2) });
        public static ScriptCode Code05D0 = new(0x05D0, "[9E] = INVERSE COUNT >> [9E]");
        public static ScriptCode Code05D1 = new(0x05D1, "UNKNOWN_D1 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05D2 = new(0x05D2, "UNKNOWN_D2 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05D3 = new(0x05D3, "<1A> = [9E]");
        public static ScriptCode Code05D4 = new(0x05D4, "UNKNOWN_D4");
        public static ScriptCode Code05D5 = new(0x05D5, "UNKNOWN_D5 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05D6 = new(0x05D6, "GAMEFLAG [9E] = TRUE");
        public static ScriptCode Code05D7 = new(0x05D7, "GAMEFLAG [9E] = FALSE");
        public static ScriptCode Code05D8 = new(0x05D8, "MEMFLAG [9E] = TRUE");
        public static ScriptCode Code05D9 = new(0x05D9, "MEMFLAG [9E] = FALSE");
        public static ScriptCode Code05DA = new(0x05DA, "UNKNOWN_DA arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05DB = new(0x05DB, "UNKNOWN_DB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05DC = new(0x05DC, "UNKNOWN_DC arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05DD = new(0x05DD, "UNKNOWN_DD");
        public static ScriptCode Code05DE = new(0x05DE, "UNKNOWN_DE arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05DF = new(0x05DF, "UNKNOWN_DF");
        public static ScriptCode Code05E0 = new(0x05E0, "UNUSED_E0");
        public static ScriptCode Code05E1 = new(0x05E1, "UNKNOWN_E1 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05E2 = new(0x05E2, "UNKNOWN_E2");
        public static ScriptCode Code05E3 = new(0x05E3, "UNKNOWN_E3");
        public static ScriptCode Code05E4 = new(0x05E4, "UNKNOWN_E4 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code05E5 = new(0x05E5, "UNKNOWN_E5");
        public static ScriptCode Code05E6 = new(0x05E6, "ADDCOMPANION arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05E7 = new(0x05E7, "UNKNOWN_E7");
        public static ScriptCode Code05E8 = new(0x05E8, "UNKNOWN_E8");
        public static ScriptCode Code05E9 = new(0x05E9, "UNKNOWN_E9 arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05EA = new(0x05EA, "UNKNOWN_EA arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05EB = new(0x05EB, "UNKNOWN_EB");
        public static ScriptCode Code05EC = new(0x05EC, "UNKNOWN_EA arg0 arg1 arg2", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05ED = new(0x05ED, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressByte, 3) });
        public static ScriptCode Code05EE = new(0x05EE, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressWord, 3) });
        public static ScriptCode Code05EF = new(0x05EF, "[9E] = arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.AddressSWord, 3) });
        public static ScriptCode Code05F0 = new(0x05F0, "UNKNOWN_EB arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05F1 = new(0x05F1, "arg0 = arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 3), new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code05F2 = new(0x05F2, "arg0 = arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Address, 3), new ScriptArgument(ScriptArgumentType.Value, 3) });
        public static ScriptCode Code05F3 = new(0x05F3, "UNKNOWN_F3 arg0 arg1 arg2", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Value, 3), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05F4 = new(0x05F4, "[9E] = arg0 , arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.RelativeAddressByte, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05F5 = new(0x05F5, "[9E] = arg0 , arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.RelativeAddressWord, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05F6 = new(0x05F6, "[9E] = arg0 , arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.RelativeAddressSWord, 2), new ScriptArgument(ScriptArgumentType.Value, 1) });
        public static ScriptCode Code05F7 = new(0x05F7, "UNKNOWN_F7");
        public static ScriptCode Code05F8 = new(0x05F8, "UNUSED_F8");
        public static ScriptCode Code05F9 = new(0x05F9, "[2C] = bFFFF - arg0", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 2) });
        public static ScriptCode Code05FA = new(0x05FA, "UNUSED_FA");
        public static ScriptCode Code05FB = new(0x05FB, "UNUSED_FB");
        public static ScriptCode Code05FC = new(0x05FC, "IF SYSFLAG arg0 = TRUE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05FD = new(0x05FD, "IF SYSFLAG arg0 = FALSE GOTO arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05FE = new(0x05FE, "IF SYSFLAG arg0 = TRUE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });
        public static ScriptCode Code05FF = new(0x05FF, "IF SYSFLAG arg0 = FALSE GOSUB arg1", new List<ScriptArgument>() { new ScriptArgument(ScriptArgumentType.Value, 1), new ScriptArgument(ScriptArgumentType.Address, 2, true) });

        public static ScriptCode Macro30 = new(0x3000, "MACRO_30");
        public static ScriptCode Macro31 = new(0x3100, "MACRO_31");
        public static ScriptCode Macro32 = new(0x3200, "MACRO_32");
        public static ScriptCode Macro33 = new(0x3300, "MACRO_33");
        public static ScriptCode Macro34 = new(0x3400, "MACRO_34");
        public static ScriptCode Macro35 = new(0x3500, "MACRO_35");
        //public static ScriptCode Macro36 = new(0x3600, "MACRO_36");
        public static ScriptCode Macro37 = new(0x3700, "MACRO_37");
        public static ScriptCode Macro38 = new(0x3800, "MACRO_38");
        public static ScriptCode Macro39 = new(0x3900, "MACRO_39");
        public static ScriptCode Macro3A = new(0x3A00, "MACRO_3A");
        public static ScriptCode Macro3B = new(0x3B00, "MACRO_3B");
        public static ScriptCode Macro3C = new(0x3C00, "MACRO_3C");
        public static ScriptCode Macro50 = new(0x5000, "MACRO_50");
        public static ScriptCode Macro51 = new(0x5100, "MACRO_51");
        public static ScriptCode Macro61 = new(0x6100, "MACRO_61");
        public static ScriptCode Macro62 = new(0x6200, "MACRO_62");
        public static ScriptCode Macro7F = new(0x7F00, "MACRO_7F");

        public static List<ScriptCode> ScriptCodes()
        {
            List<ScriptCode> properties = new();
            ScriptCode instance = new();

            foreach (FieldInfo prop in typeof(ScriptCodesList).GetFields())
            {
                if (prop.FieldType == typeof(ScriptCode))
                {
                    properties.Add(new ScriptCode((ScriptCode)prop.GetValue(instance)));
                }

            }

            return properties.ToList();
        }
    }

    public class ScriptsInterpreter
    {
        private List<(string, int)> Labels;
        private List<(int address, string line)> Lines;
        private int Bank;
        private int Address;
        private int Length;
        private FFMQRom Rom;
        public string TextualData;
        private List<byte> ByteList;

        public ScriptsInterpreter(int bank, int address, int length, FFMQRom rom)
        {
            Bank = bank;
            Address = address;
            Length = length;
            Rom = rom;
            ByteList = Rom.GetFromBank(Bank, Address, Length).ToBytes().ToList();
            Interpret();

            foreach (var label in Labels)
            {
                Console.WriteLine(label.Item1 + " = " + $"{label.Item2:X6}");
                TextualData += label.Item1 + " = " + $"{label.Item2:X6}" + "\n";
            }

            Console.WriteLine("-----");
            TextualData += "-----" + "\n";

            foreach (var line in Lines)
            {
                Console.WriteLine(line.line);
                TextualData += line.line + "\n";
            }
        }

        public ScriptsInterpreter(string address, List<byte> code)
        {
            ByteList = code;
            Bank = (int)Convert.FromHexString(address.Substring(0, 2))[0];
            var sub = address.Substring(2, 4);
            var array = Convert.FromHexString(sub).ToArray();
            var intval = Int32.Parse(sub, System.Globalization.NumberStyles.HexNumber);

            Address = intval;
            //Address = BitConverter.ToInt32(Convert.FromHexString(address.Substring(2, 4)));
            Length = code.Count;
            Rom = new FFMQRom();
            Interpret();

            foreach (var label in Labels)
            {
                Console.WriteLine(label.Item1 + " = " + $"{label.Item2:X6}");
                TextualData += label.Item1 + " = " + $"{label.Item2:X6}" + "\n";
            }

            Console.WriteLine("-----");
            TextualData += "-----" + "\n";

            foreach (var line in Lines)
            {
                Console.WriteLine(line.line);
                TextualData += line.line + "\n";
            }
        }
        public ScriptsInterpreter(List<string> code)
        {
            int orgIndex = code.FindIndex(x => x.Split(' ')[0] == "ORG");

            if (orgIndex == -1)
            {
                throw new Exception("No ORG statement");
            }

            Labels = code.GetRange(0, orgIndex).Select(x => (x.Split(' ')[0], Convert.ToInt32(x.Split(' ')[1], 16))).ToList();
            int orgAddress = Convert.ToInt32(code[orgIndex].Split(' ')[1], 16);

            Bank = orgAddress / 0x10000;
            Address = orgAddress % 0x10000;

            int baseAddress = orgAddress;
            int currentbyte = 0;

            //code.RemoveRange(0, orgIndex + 1);

            List<ScriptCode> codeList = ScriptCodesList.ScriptCodes();
            List<string> sizeIndices = new() { "z", "b", "w", "s", "d", "b", "w", "s", "d" };



            for(int currentLine = orgIndex + 1; currentLine < code.Count; currentLine++)
            {
                

                if (code[currentLine][0] != ' ')
                {
                    Labels.Add((code[currentLine].Substring(0, code[currentLine].Length - 1), baseAddress + currentbyte));
                }
                else
                {
                    List<ScriptCode> validCodeList = codeList;

                    var splitLine = code[currentLine].Substring(2).Split(' ');

                    int currentStep = 0;
                    int argCount = 0;

                    for(int currentSegment = 0; currentSegment < splitLine.Length; currentSegment++)
                    {
                        string workingSegment = splitLine[currentSegment];
                        
                        if (workingSegment[0] == '[' || workingSegment[0] == '<')
                        {
                            char openBracket = workingSegment[0];
                            char closeBracket = workingSegment[workingSegment.Length - 1];
                            string middleValue = workingSegment.Substring(1, workingSegment.Length - 2);

                            int labelIndex = Labels.FindIndex(x => x.Item1 == middleValue);

                            if (labelIndex == -1)
                            {

                            }
                            else
                            {
                                workingSegment = openBracket + $"{Labels[labelIndex].Item2:X2}" + closeBracket;
                            }
                        }

                        List<ScriptCode> tempCodeList = validCodeList.Where(x => (x.Command.Split(' ').Length > currentStep) &&
                            (x.Command.Split(' ')[currentStep] == workingSegment)).ToList();

                        if (!tempCodeList.Any())
                        {
                            workingSegment = "arg" + argCount;
                            argCount++;
                            tempCodeList = validCodeList.Where(x => (x.Command.Split(' ').Length > currentStep) && (x.Command.Split(' ')[currentStep] == workingSegment)).ToList();

                            if (!tempCodeList.Any())
                            {
                                throw new Exception("Invalid code: line " + currentLine);
                            }
                        }
                    }

                    if (validCodeList.Count == 1)
                    {

                    }
                    else
                    {
                        throw new Exception("Invalid code: line " + currentLine);
                    }

                }


            }
            //Length = length;
            //Rom = rom;

            Interpret();

            foreach (var label in Labels)
            {
                Console.WriteLine(label.Item1 + " = " + $"{label.Item2:X6}");
            }

            Console.WriteLine("-----");

            foreach (var line in Lines)
            {
                Console.WriteLine(line.line);
            }
        }

        private void Interpret()
        {
            Blob byteList = ByteList.ToArray();
            List<ScriptCode> codeList = ScriptCodesList.ScriptCodes();
            List<string> sizeIndices = new() { "z", "b", "w", "s", "d", "b", "w", "s", "d" };

            Labels = new();
            Lines = new();

            int currentbyte = 0;
            int baseAddress = Bank * 0x10000 + Address;
            int labelcount = 0;
            int templabel = 0;
            bool stringOfLetters = false;
            int stringOfLettersLength = 0;
            int addressOfLetters = 0x0000;

            while (currentbyte < Length)
            {
                Console.WriteLine(currentbyte);
                byte command = byteList[currentbyte];
                int lineAddress = baseAddress + currentbyte;

                if (command < 0x30 && stringOfLetters)
                {
                    stringOfLetters = false;
                    Lines.Add((addressOfLetters, $"  PRINT " + MQText.BytesToText(byteList.SubBlob(currentbyte - stringOfLettersLength, stringOfLettersLength))));
                }


                if (command == 0x2A)
                {
                    var selectedCommand = codeList.Find(x => x.PrimaryIndex == command);
                    Lines.Add((lineAddress, "  " + String.Join(' ', selectedCommand.Command)));
                    currentbyte++;


                    bool readactions = true;

                    while (readactions)
                    {
                        lineAddress = baseAddress + currentbyte;
                        ushort nextaction = byteList.SubBlob(currentbyte, 2).ToUShorts()[0];

                        if (nextaction == 0xFFFF)
                        {
                            readactions = false;
                        }
                        
                        Lines.Add((lineAddress, $"    .WORD {nextaction:X4}"));
                        currentbyte += 2;
                    }
                }
                else if (command < 0x30)
                {
                    ScriptCode selectedCommand;

                    if (command == 0x05)
                    {
                        currentbyte++;
                        var subByte = byteList.SubBlob(currentbyte, 1).ToBytes()[0];
                        selectedCommand = codeList.Find(x => x.SecondaryIndex == subByte);
                    }
                    else
                    {
                        selectedCommand = codeList.Find(x => x.PrimaryIndex == command);
                    }
                    
                    List<string> dividedString = selectedCommand.Command.Split(' ').ToList();
                    currentbyte++;

                    for (int i = 0; i < selectedCommand.Arguments.Count; i++)
                    {
                        int argumentIndex = dividedString.FindIndex(x => x == ("arg" + i));
                        if (argumentIndex < 0)
                        {
                            throw new Exception("Invalid Code Description");
                        }

                        Blob argumentBytes = byteList.SubBlob(currentbyte, selectedCommand.Arguments[i].Size);
                        int argumentInt = ConvertBytesToAddress(argumentBytes);

                        currentbyte += selectedCommand.Arguments[i].Size;

                        if (selectedCommand.Arguments[i].Type == ScriptArgumentType.Value)
                        {
                            string argumentFormat = "X" + (selectedCommand.Arguments[i].Size * 2);
                            dividedString[argumentIndex] = "#$" + argumentInt.ToString(argumentFormat);
                        }
                        else
                        {
                            bool useLabel = selectedCommand.Arguments[i].CreateLabel;
                            string sizeIndice = selectedCommand.Arguments[i].Type == ScriptArgumentType.Address ? "" : sizeIndices[(int)selectedCommand.Arguments[i].Type];

                            if (useLabel)
                            {
                                templabel = CreateLabel(ref labelcount, argumentInt, selectedCommand.Arguments[i].Size);
                            }

                            string openingChar = "[";
                            string closingChar = "]";

                            if (selectedCommand.Arguments[i].Type > ScriptArgumentType.AddressDWord)
                            {
                                openingChar = "<";
                                closingChar = ">";
                            }

                            string argumentFormat = "X" + (selectedCommand.Arguments[i].Size * 2);
                            dividedString[argumentIndex] = (useLabel ? (sizeIndice + Labels[templabel].Item1) : (sizeIndice + openingChar + argumentInt.ToString(argumentFormat) + closingChar));
                        }
                    }

                    Lines.Add((lineAddress, "  " + String.Join(' ', dividedString)));
                }
                else
                {/*
                    var selectedCommand = codeList.Where(x => x.PrimaryIndex == command);
                    if(selectedCommand.Any())
                    {
                        Lines.Add((lineAddress, "  " + String.Join(' ', selectedCommand.First().Command)));
                    }*/
                    if (stringOfLetters)
                    {
                        stringOfLettersLength++;
                    }
                    else
                    {
                        stringOfLetters = true;
                        stringOfLettersLength = 1;
                        addressOfLetters = lineAddress;
                    }
                    currentbyte++;
                }
            }

            // Insert labels
            foreach (var label in Labels)
            {
                var lineIndex = Lines.FindIndex(x => x.address == label.Item2);

                if (lineIndex > -1)
                {
                    Lines.Insert(lineIndex, (0x000000, label.Item1 + ":"));
                }
            }
        }

        public void ScriptToBinary()
        { 
        
        
        
        }
        private int CreateLabel(ref int labelcount, int address, int length)
        {
            if (length < 3)
            {
                address = Bank * 0x10000 + address;
            }
            
            int labelId = Labels.FindIndex(x => x.Item2 == address);

            if (labelId >= 0)
            {
                return labelId;
            }
            else
            {
                Labels.Add(("LABEL_" + $"{labelcount:X4}", address));
                labelcount++;
                return (labelcount-1);
            }
        }

        private int ConvertBytesToAddress(Blob byteSeries)
        {
            int multiplier = 1;
            int value = 0;

            for (int i = 0; i < byteSeries.Length; i++)
            { 
                value += (int)(byteSeries[i] * multiplier);
                multiplier *= 0x100;
            }

            return value;
        }
    }
}
