using RomUtilities;
using System.Collections.Generic;
using System.Linq;
using System;


namespace FFMQLib
{

	public partial class FFMQRom : SnesRom
	{

		public List<(string, int)> TextDTE = new() {

			("\n", 0x01),
			("#", 0x36), // end of box
			("Crystal", 0x3d),
			("Rainbow Road", 0x3e), // DTE in DTE...
			("th", 0x3f),
			("e ", 0x40),
			("the ", 0x41),
			("t ", 0x42),
			("ou", 0x43),
			("you", 0x44),
			("s ", 0x45),
			("to ", 0x46),
			("in", 0x47),
			("ing ", 0x48),
			("l ", 0x49),
			("ll ", 0x4a),
			("er", 0x4b),
			("d ", 0x4c),
			(", ", 0x4d),
			("'s ", 0x4e),
			("an", 0x4f),
			// 0x50 05 1d 9e 00 04
			// 0x51 05 1e 9e 00 04 0x1bb25
			("ight", 0x52),
			("...", 0x53),
			("on", 0x54),
			("you ", 0x55), // dte
			("en", 0x56),
			("ha", 0x57),
			("ow", 0x58),
			("y ", 0x59),
			("of ", 0x5a),
			("Th", 0x5b),
			("or", 0x5c),
			("I'll ", 0x5d), //dte
			("ea", 0x5e),
			("is ", 0x5f), //dte
			("es", 0x60),
			// 0x61 08 62 81 ?? 
			// 0x62 08 8a 87 ??
			("wa", 0x63),
			("again", 0x64), // dte
			("st", 0x65),
			("I ", 0x66),
			("ve ", 0x67), //dte
			("ed ", 0x68), //dte
			("om", 0x69),
			("er ", 0x6a), //dte
			("p ", 0x6b),
			("ack", 0x6c),
			("ust ", 0x6d), //dte
			("!#", 0x6e), 
			("!\n", 0x6f),
			("that ", 0x70), //dte
			("prophecy", 0x71),
			("o ", 0x72),
			(".\n", 0x73),
			(".#", 0x74),
			("I'm ", 0x75),
			("el", 0x76),
			("with ", 0x77), //dte
			("a ", 0x78),
			("Spencer", 0x79), //dte
			("ma", 0x7a),
			("in ", 0x7b), //dte
			("monst", 0x7c), //dte
			("k ", 0x7d),
			("'t ", 0x7e), //dte

			("0", 0x90),
			("1", 0x91),
			("2", 0x92),
			("3", 0x93),
			("4", 0x94),
			("5", 0x95),
			("6", 0x96),
			("7", 0x97),
			("8", 0x98),
			("9", 0x99),

			("A", 0x9a),
			("B", 0x9b),
			("C", 0x9c),
			("D", 0x9d),
			("E", 0x9e),
			("F", 0x9f),
			("G", 0xa0),
			("H", 0xa1),
			("I", 0xa2),
			("J", 0xa3),
			("K", 0xa4),
			("L", 0xa5),
			("M", 0xa6),
			("N", 0xa7),
			("O", 0xa8),
			("P", 0xa9),
			("Q", 0xaa),
			("R", 0xab),
			("S", 0xac),
			("T", 0xad),
			("U", 0xae),
			("V", 0xaf),
			("W", 0xb0),
			("X", 0xb1),
			("Y", 0xb2),
			("Z", 0xb3),

			("a", 0xb4),
			("b", 0xb5),
			("c", 0xb6),
			("d", 0xb7),
			("e", 0xb8),
			("f", 0xb9),
			("g", 0xba),
			("h", 0xbb),
			("i", 0xbc),
			("j", 0xbd),
			("k", 0xbe),
			("l", 0xbf),
			("m", 0xc0),
			("n", 0xc1),
			("o", 0xc2),
			("p", 0xc3),
			("q", 0xc4),
			("r", 0xc5),
			("s", 0xc6),
			("t", 0xc7),
			("u", 0xc8),
			("v", 0xc9),
			("w", 0xca),
			("x", 0xcb),
			("y", 0xcc),
			("z", 0xcd),

			("!", 0xce),
			("?", 0xcf),
			(",", 0xd0),
			("'", 0xd1),
			(".", 0xd2),
			("<\"", 0xd3), // opening apostroph
			("\">", 0xd4), // closing apostroph
			(".\">", 0xd5),
			(";", 0xd6),
			(":", 0xd7),
			("[...]", 0xd8), // small 3 dots
			("/", 0xd9), 
			("-", 0xda),
			("&", 0xdb),
			(">", 0xdc),
			("%", 0xdd),

			(" ", 0xff),

		};


		public string TextToHex(string text)
		{
			return String.Join("", TextToByte(text).SelectMany(x => String.Join("", x.ToString("X2"))));
		}
		public byte[] TextToByte(string text)
		{
			byte[] byteText = new byte[text.Length];

			var orderedDTE = TextDTE.OrderByDescending(x => x.Item1.Length);

			string blackoutString = "************";
			
			foreach (var dte in orderedDTE)
			{
				for (int index = 0; ; index += dte.Item1.Length)
				{
					index = text.IndexOf(dte.Item1, index);
					if (index == -1)
						break;
					text = text.Remove(index, dte.Item1.Length).Insert(index, blackoutString.Substring(0, dte.Item1.Length));
					byteText[index] = (byte)dte.Item2;
				}
			}
			return byteText.Where(x => x != 0x00).ToArray();
		}
	}

	public class Credits
	{
		private byte[] originalCredits;
		private byte[] additionalCredits;
		private byte[] header;
		private int length;

		public Credits(FFMQRom rom)
		{
			header = rom.GetFromBank(RomOffsets.CreditsBankOld, RomOffsets.CreditsHeader, 8);
			length = header[7] * 0x100 + header[6];
			originalCredits = rom.GetFromBank(RomOffsets.CreditsBankOld, RomOffsets.CreditsAddressOld, length);
			
		}

		public void Update()
        {
			FFMQRom text = new();

			additionalCredits = text.TextToByte(
				"FFMQ Randomizer\n\n" +
				"Main Developer\n" +
				"wildham\n\n" +
				"Playtester\n" +
				"spellzapp\n\n" +
				"Special Thanks\n" +
				"Entroper\n" +
				"nitz\n" +
				"Septimus\n" +
				"rabite\n" +
				"DarkmoonEX\n" +
				"abyssonym\n" +
				"The FFR Dev Team\n" +
				"&\n" +
				"The FFR Community\n\n" +
				"Original FFMQ Credits\n\n"
				);
		}

		public void Write(FFMQRom rom)
		{ 
			var newCredits = additionalCredits.Concat(originalCredits).ToArray();
			length = newCredits.Length;

			var lengthBytes = new byte[] { (byte)(length % 0x100), (byte)(length / 0x100) };

			header[0] = (byte)(RomOffsets.CreditsAddressNew % 0x100);
			header[1] = (byte)(RomOffsets.CreditsAddressNew / 0x100);
			header[2] = (byte)RomOffsets.CreditsBankNew;
			header[6] = lengthBytes[0];
			header[7] = lengthBytes[1];

			rom.PutInBank(0x00, RomOffsets.CreditsLengthReadTo, lengthBytes);
			rom.PutInBank(0x03, RomOffsets.CreditsLengthTheEnd, lengthBytes);
			rom.PutInBank(0x0C, RomOffsets.CreditsLengthConvert, lengthBytes);

			rom.PutInBank(RomOffsets.CreditsBankOld, RomOffsets.CreditsHeader, header);
			rom.PutInBank(RomOffsets.CreditsBankNew, RomOffsets.CreditsAddressNew, newCredits);
		}
	}
}
