using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFMQLib
{
	public enum PatchAction
	{
		Delete,
		Insert,
		Replace,
		Offset,
		IncreaseWord,
		DecreaseWord,
		IncreaseRange,
		DecreaseRange,
	}
	public struct PatchInstruction
	{
		public int Address { get; set; }
		public PatchAction Action { get; set; }
		public int Value { get; set; }
		public int Step { get; set; }
		public int Qty { get; set; }
		public byte[] Chunk { get; set; }
	}
	public static partial class Patcher
	{
		public static FFMQRom PatchRom(FFMQRom rom)
		{
			byte[] bank00 = PatchBank(rom.GetFromBank(0x00, 0x8000, 0x8000), bank00instructions);
			byte[] bank01 = PatchBank(rom.GetFromBank(0x01, 0x8000, 0x8000), bank01instructions);
			byte[] bank02 = PatchBank(rom.GetFromBank(0x02, 0x8000, 0x8000), bank02instructions);
			byte[] bank03 = PatchBank(rom.GetFromBank(0x03, 0x8000, 0x8000), bank03instructions);
			byte[] bank06 = PatchBank(rom.GetFromBank(0x06, 0x8000, 0x8000), bank06instructions);
			byte[] bank0B = PatchBank(rom.GetFromBank(0x0B, 0x8000, 0x8000), bank0binstructions);
			byte[] bank0C = PatchBank(rom.GetFromBank(0x0C, 0x8000, 0x8000), bank0cinstructions);
			byte[] bank0D = PatchBank(rom.GetFromBank(0x0D, 0x8000, 0x8000), bank0dinstructions);
			byte[] bank0F = PatchBank(rom.GetFromBank(0x0F, 0x8000, 0x8000), bank0finstructions);

			FFMQRom newrom = new();
			newrom.CopyData(rom.DataReadOnly);
			
			newrom.PutInBank(0x00, 0x8000, bank00);
			newrom.PutInBank(0x01, 0x8000, bank01);
			newrom.PutInBank(0x02, 0x8000, bank02);
			newrom.PutInBank(0x03, 0x8000, bank03);
			newrom.PutInBank(0x06, 0x8000, bank06);
			newrom.PutInBank(0x0B, 0x8000, bank0B);
			newrom.PutInBank(0x0C, 0x8000, bank0C);
			newrom.PutInBank(0x0D, 0x8000, bank0D);
			newrom.PutInBank(0x0F, 0x8000, bank0F);

			return newrom;
		}
		private static byte[] PatchBank(byte[] bankdata, Dictionary<int, PatchInstruction> bankinstructions)
		{
			int offset = 0;
			byte[] newbank = new byte[0x8000];
			int index = 0x00;

			foreach (var instruction in bankinstructions)
			{
				if (index < instruction.Key)
				{
					Array.Copy(bankdata, index + offset, newbank, index, (int)(instruction.Key - index));
					index = instruction.Key;
				}

				if (instruction.Value.Action == PatchAction.IncreaseWord)
				{
					int newvalue = bankdata[index + offset] + instruction.Value.Value;

					if (newvalue > 0xFF)
					{
						newbank[index] = (byte)(newvalue - 0x100);
						newbank[index + 1] = (byte)(bankdata[index + 1 + offset] + 1);
						index += 2;
					}
					else
					{
						newbank[index] = (byte)(newvalue);
						index++;
					}
				}
				else if (instruction.Value.Action == PatchAction.IncreaseRange)
				{
					for (int j = 0; j < instruction.Value.Qty; j++)
					{
						int step = instruction.Value.Step;
						int newvalue = bankdata[index + (j * step) + offset] + instruction.Value.Value;

						byte newbyte = (newvalue > 0xFF) ? (byte)(newvalue - 0x100) : (byte)(newvalue);
						byte increment = (newvalue > 0xFF) ? (byte)0x01 : (byte)0x00;

						newbank[index + (j * step)] = newbyte;
						newbank[index + (j * step) + 1] = (byte)(bankdata[index + (j * step) + 1 + offset] + increment);
						if (step == 3)
						{
							newbank[index + (j * step) + 2] = (byte)(bankdata[index + (j * step) + 2 + offset]);
						}
					}

					index += (instruction.Value.Step * instruction.Value.Qty);
				}
				else if (instruction.Value.Action == PatchAction.DecreaseRange)
				{
					for (int j = 0; j < instruction.Value.Qty; j++)
					{
						int step = instruction.Value.Step;
						int newvalue = bankdata[index + (j * step) + offset] - instruction.Value.Value;

						byte newbyte = (newvalue < 0) ? (byte)(newvalue + 0x100) : (byte)(newvalue);
						byte decrement = (newvalue < 0) ? (byte)0x01 : (byte)0x00;

						newbank[index + (j * step)] = newbyte;
						newbank[index + (j * step) + 1] = (byte)(bankdata[index + (j * step) + 1 + offset] - decrement);
						if (step == 3)
						{
							newbank[index + (j * step) + 2] = (byte)(bankdata[index + (j * step) + 2 + offset]);
						}
					}

					index += (instruction.Value.Step * instruction.Value.Qty);
				}
				else if (instruction.Value.Action == PatchAction.DecreaseWord)
				{
					int newvalue = bankdata[index + offset] - instruction.Value.Value;

					if (newvalue < 0)
					{
						newbank[index] = (byte)(newvalue + 0x100);
						newbank[index + 1] = (byte)(bankdata[index + 1 + offset] - 1);
						index +=2;
					}
					else
					{
						newbank[index] = (byte)(newvalue);
						index++;
					}
				}
				else if (instruction.Value.Action == PatchAction.Replace)
				{
					for (int j = 0; j < instruction.Value.Chunk.Length; j++)
					{
						newbank[index + j] = instruction.Value.Chunk[j];
					}

					index += instruction.Value.Chunk.Length;
				}
				else if (instruction.Value.Action == PatchAction.Insert)
				{
					for (int j = 0; j < instruction.Value.Chunk.Length; j++)
					{
						newbank[index + j] = instruction.Value.Chunk[j];
					}

					index += instruction.Value.Chunk.Length;
					offset -= instruction.Value.Chunk.Length;
				}
				else if (instruction.Value.Action == PatchAction.Offset)
				{
					offset += instruction.Value.Value;
					newbank[index] = bankdata[index + offset];
					index++;
				}
			}

			Array.Copy(bankdata, index + offset, newbank, index, (int)(0x8000 - index));

			return newbank;
		}
	}
}
