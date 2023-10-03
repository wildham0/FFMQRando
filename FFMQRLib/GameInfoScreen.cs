using System.Collections.Generic;

using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public class GameInfoScreen
	{ 
		public int FragmentsCount { get; set; }
		public List<(ElementsType, ElementsType)> ShuffledElementsType { get; set; }
		private int pagecount;
		private Dictionary<ElementsType, string> elementsbytes = new()
		{
			{ ElementsType.Earth, "160001" },
			{ ElementsType.Water, "160101" },
			{ ElementsType.Fire, "160201" },
			{ ElementsType.Air, "160301" },
			{ ElementsType.Zombie, "160401" },
			{ ElementsType.Axe, "160505" },
			{ ElementsType.Bomb, "160601" },
			{ ElementsType.Projectile, "160705" },
			{ ElementsType.Doom, "160801" },
			{ ElementsType.Stone, "160905" },
			{ ElementsType.Silence, "160A05" },
			{ ElementsType.Blind, "160B05" },
			{ ElementsType.Poison, "160C01" },
			{ ElementsType.Paralysis, "160D01" },
			{ ElementsType.Sleep, "160E01" },
			{ ElementsType.Confusion, "160F01" },
		};
		public GameInfoScreen()
		{
			FragmentsCount = 0;
			ShuffledElementsType = new();
			pagecount = 1;
		}

		public void Write(FFMQRom rom)
		{
			// Extend menu size
			rom.PutInBank(0x03, 0xAB49, Blob.FromHex("88"));
			rom.PutInBank(0x03, 0xAB07, Blob.FromHex("11"));

			// Fix Item Scroll
			rom.PutInBank(0x00, 0xC8D2, Blob.FromHex("0E"));
			rom.PutInBank(0x00, 0xCE17, Blob.FromHex("88"));

			// Fix Spell Scroll
			rom.PutInBank(0x00, 0xCE0A, Blob.FromHex("22009010"));
			rom.PutInBank(0x00, 0xCE45, Blob.FromHex("22209010"));
			rom.PutInBank(0x10, 0x9000, Blob.FromHex("48adab00c921900668186d64006b686b"));
			rom.PutInBank(0x10, 0x9020, Blob.FromHex("48adab00c980b0066838ed64006b686b"));

			// Fix Armor/Weapon/Status Scroll
			rom.PutInBank(0x00, 0xCD23, Blob.FromHex("09"));
			rom.PutInBank(0x00, 0xCD29, Blob.FromHex("BF499010"));
			rom.PutInBank(0x10, 0x904A, Blob.FromHex("efefefeceae8e6e4e3"));

			rom.PutInBank(0x00, 0xCD43, Blob.FromHex("07"));
			rom.PutInBank(0x00, 0xCD49, Blob.FromHex("BF539010"));
			rom.PutInBank(0x10, 0x9054, Blob.FromHex("e3e5e7e9ececec"));

			// Fix Customize Scroll
			rom.PutInBank(0x00, 0xCCC3, Blob.FromHex("09"));
			rom.PutInBank(0x00, 0xCD10, Blob.FromHex("7F3F9010"));
			rom.PutInBank(0x10, 0x9040, Blob.FromHex("030303020202020103"));

			// Fix Save Scroll and Direct Access
			rom.PutInBank(0x03, 0x94FA, Blob.FromHex("0901")); // Title Offset
			rom.PutInBank(0x00, 0xC60B, Blob.FromHex("70"));
			rom.PutInBank(0x00, 0xBE22, Blob.FromHex("75BE"));
			rom.PutInBank(0x00, 0xBE26, Blob.FromHex("73BE"));

			// Add Game Info option
			rom.PutInBank(0x03, 0xAB36, Blob.FromHex("07009110"));
			rom.PutInBank(0x10, 0x9100, Blob.FromHex(rom.TextToHex("GAMEINFO") + "02" + rom.TextToHex("SAVE")));
			rom.PutInBank(0x00, 0xBC3F, Blob.FromHex("08"));
			rom.PutInBank(0x00, 0xBE35, Blob.FromHex("22809010f4e4bdbd4bbe48bd49be487c47be"));
			rom.PutInBank(0x10, 0x9080, Blob.FromHex("a50229ff000a85640a6564aa640164056b"));
			rom.PutInBank(0x00, 0xBE47, Blob.FromHex("bcc84abfb6c8c6c837c0c0c865c8dac359c86ac846c459c86fc8a1c459c87ac8d8c14bc810ff2fff59c885c847c353c8000000000000"));

			// Game Info Screen Routine
			rom.PutInBank(0x00, 0xFF10, Blob.FromHex("a980001cd900a220ff4c9dc8ffffffff")); // Setup Routine
			rom.PutInBank(0x00, 0xFF20, Blob.FromHex("809110749403709110749403609010")); // Jumping Addresses
			rom.PutInBank(0x00, 0xFF30, Blob.FromHex($"a9f0bf858ea9{pagecount:X2}018d0300a930cf2030b9d00b890080f0f3201cb9648e60648ead01008d0500a226ff20c9c84c30ff")); // Screen Input Loop
			rom.PutInBank(0x00, 0xFF60, Blob.FromHex("a22cff20a5c8a919008d19006b")); // Flip page

			var pageflipscript = new ScriptBuilder(
				new List<string> {
						"088091",
						"0960ff00",
						"00",
					});

			pageflipscript.WriteAt(0x10, 0x9170, rom);

			var maindrawscript = new ScriptBuilder(
				new List<string> {
						"0d42002040",
						"0eac00000800",
						"3a",
						"250c",
						"150301",
						"19",
						"3b",
						"2401041e13",
						"18",
						"0f0100",         // check current page
						"057C000092",
						"057C010093",
						"057C020094",
						"057C030095",
						"057C040096",
						"058d",
						"00"
				});
			
			maindrawscript.WriteAt(0x10, 0x9180, rom);

			string page1script = "";
			int lineoffset = 0x06;

			page1script += $"250C1503{lineoffset:X2}19";
			if (FragmentsCount == 0 && !ShuffledElementsType.Any())
			{
				page1script += rom.TextToHex("No info available.");
			}
			else
			{
				if (FragmentsCount > 0)
				{
					page1script += rom.TextToHex("Sky Fragments Required: " + FragmentsCount.ToString());
					lineoffset += 3;
				}

				if (ShuffledElementsType.Any())
				{
					page1script += $"250C1503{lineoffset:X2}19";
					page1script += rom.TextToHex("Elements Shuffling");
					lineoffset ++;
					page1script += $"25102404{lineoffset:X2}110618";
					lineoffset++;
					page1script += $"1505{lineoffset:X2}19";

					ShuffledElementsType.Reverse();

					int xcount = 0;
					foreach (var elementgroup in ShuffledElementsType)
					{
						page1script += elementsbytes[elementgroup.Item1] + "DC" + elementsbytes[elementgroup.Item2];
						xcount++;
						if (xcount >= 4)
						{
							page1script += "01";
							xcount = 0;
						}
						else
						{
							page1script += "FF";
						}
					}
					lineoffset += 5;
				}
			}

			var page1 = new ScriptBuilder(
				new List<string> {
					page1script,
					"00"
				});
			
			page1.WriteAt(0x10, 0x9200, rom);

		}
	}
}
