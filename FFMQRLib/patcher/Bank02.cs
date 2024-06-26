﻿using System.Collections.Generic;

namespace FFMQLib
{
	public partial class Patcher
	{
		private static Dictionary<int, PatchInstruction> bank02instructions = new()
		{
			{ 0x0097, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x014D, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0158, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x01FA, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x020F, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x02E6, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x02EF, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0430, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x043D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x049D, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x04A3, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0599, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x05EE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x076F, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0839, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x09AC, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x09B8, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x09D7, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x09F6, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0A13, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0A29, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0A3C, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0A91, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0A97, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0AC5, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0ACB, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0FA9, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x0FC1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x0FCC, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 3, Qty = 3 } },
			{ 0x0FE7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x0FEA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x0FF3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x0FF6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1028, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x105F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1071, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1074, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x109E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x10CE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x10D1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x10DA, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 3, Qty = 3 } },
			{ 0x10EF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x10F2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x10F8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1101, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 3, Qty = 3 } },
			{ 0x1116, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1119, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1125, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x112E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1131, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1134, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x115C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1161, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1167, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x116D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1170, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x117F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1198, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x119E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x11A1, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x11A7, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x11DE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x126B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1277, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x128F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x12C1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x12C4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1300, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x130B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1319, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1327, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x136A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x136D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1376, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1388, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x138E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1394, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1397, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13B0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13B9, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13BC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13DC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13E1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13E6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13EB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13EE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x13F4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x143F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1442, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1464, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1467, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x147F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1485, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x14BC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x14E0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x14E5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x150D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1510, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1522, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x152A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1531, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1556, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x155C, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1562, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1585, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1618, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x16AF, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x171F, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 3, Qty = 3 } },
			{ 0x1762, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1768, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x176E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1774, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x177A, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1780, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x178C, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1792, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1798, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x179E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17A4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x17A7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x17AA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x17AD, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17B3, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17B9, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17BF, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17C5, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17CB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x17CE, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x17D4, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x19A7, new PatchInstruction() { Action = PatchAction.Insert, Chunk = new byte[] { 0x20, 0x2B, 0x89, 0x20, 0xF0, 0x16, 0x29, 0x10, 0x4A, 0x4A, 0x4A, 0x4A, 0xC5, 0x8D, 0xD0, 0x0C, 0xA5, 0x4E, 0x06, 0x4E, 0xC5, 0x4E, 0x90, 0x04, 0xA9, 0xFA, 0x85 } } },
			{ 0x19C2, new PatchInstruction() { Action = PatchAction.Offset, Value = 0x07 } },
			{ 0x1A2E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1AA6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1ABE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1AC5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1AEA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1AF0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1B29, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1B35, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1B65, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1B6B, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1B71, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1B79, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1B7F, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1B99, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1BA0, new PatchInstruction() { Action = PatchAction.DecreaseRange, Value = 0x51, Step = 2, Qty = 8 } },
			{ 0x1BC5, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1BE2, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1BE8, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1BEE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1BF5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1BFE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C01, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C0B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C16, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C28, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C2B, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C33, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C44, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C52, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C60, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C6E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C82, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C90, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1C96, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1C99, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CBF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CC2, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1CC8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CD7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CE0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CE9, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CEE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1CF1, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1CF7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D00, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D0B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D10, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D13, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1D19, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D22, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D2D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D32, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D35, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1D3B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D44, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D4F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D54, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D57, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1D5D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D66, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D71, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D76, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D79, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1D7F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D88, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D93, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D98, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1D9B, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1DA1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1DAB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1DEA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1DED, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1DF3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1DF6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1E1F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1E91, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1EA2, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1EB6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x1EE3, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F03, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F08, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F0A, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F0C, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F0E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1F3E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1FBE, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 5 } },
			{ 0x1FC9, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1FD3, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1FDD, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1FE7, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x1FFB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x20F4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2124, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x219F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x21B5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2249, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x224F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2255, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x225B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2261, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2267, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x22A4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x22B2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x22DE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x22E6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2316, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x232C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2343, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2395, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x23BF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x23C2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x23D0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x23DF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x23E2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2447, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2514, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2553, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x25A3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x25A8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x25CD, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2601, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x2634, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2645, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2652, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2680, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x26C5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2748, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2795, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x279C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x27CE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x27D3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2806, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x280D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2816, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x282C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x288D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x289A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x28EC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x28EF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x292D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x297D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x29AE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x29D0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2A93, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2AA8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2AD4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2AF3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2AF8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B00, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B05, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B24, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B29, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B31, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B3A, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 5 } },
			{ 0x2B77, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B7C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2B95, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2BD2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2BEA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2BED, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2C06, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x2C0C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2C10, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 5 } },
			{ 0x2C4B, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 3, Qty = 4 } },
			{ 0x2C6D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2C7C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2C89, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2CBC, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 12 } },
			{ 0x2CE1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2CE8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2CEA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2D63, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2D6F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2DA7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2DB4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2DBB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2DC7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2E50, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2E57, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 5 } },
			{ 0x2FED, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2FF4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x2FF6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x30CE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x31B4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x31C0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3276, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x327E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3288, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x32F9, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3300, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 4 } },
			{ 0x33C2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x33D8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x33DD, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x33E2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x33E7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x350F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3556, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3599, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x362C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3682, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3687, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36CA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36D2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36DA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36E2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36EF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x36FC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3709, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3716, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3755, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3774, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x384B, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 5 } },
			{ 0x38D0, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x38DA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x38E4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x38EF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x390B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x394D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3962, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3977, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x398E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x399A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3A05, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x3BEC, new PatchInstruction() { Action = PatchAction.Offset, Value = 0x14 } },
			{ 0x51CA, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x51D1, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x51 } },
			{ 0x5DB1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x14 } },
			{ 0x7642, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x14, Step = 2, Qty = 16 } },
		};
	}
}
