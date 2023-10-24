using System.Collections.Generic;

using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.Runtime.Intrinsics.Arm;

namespace FFMQLib
{
	public class GameInfoScreen
	{
		public int FragmentsCount { get; set; }
		public List<(ElementsType, ElementsType)> ShuffledElementsType { get; set; }
		public List<(CompanionsId, List<(int level, SpellFlags spell)>)> SpellLearning { get; set; }
		private int pagecount;
		private List<int> pageoffsets;
		private Dictionary<ElementsType, string> elementsbytes = new()
		{
			{ ElementsType.Earth, "160001" },
			{ ElementsType.Water, "160101" },
			{ ElementsType.Fire, "160201" },
			{ ElementsType.Air, "160301" },
			{ ElementsType.Zombie, "161001" }, // Original Icon 160401
			{ ElementsType.Axe, "161205" }, // Original Icon 160505
			{ ElementsType.Bomb, "161105" }, // Original Icon 160601
			{ ElementsType.Projectile, "160705" },
			{ ElementsType.Doom, "160805" }, // Color bugged?
			{ ElementsType.Stone, "160905" },
			{ ElementsType.Silence, "160A05" },
			{ ElementsType.Blind, "160B05" },
			{ ElementsType.Poison, "160C01" },
			{ ElementsType.Paralysis, "160D01" },
			{ ElementsType.Sleep, "160E01" },
			{ ElementsType.Confusion, "160F01" },
		};
		private Dictionary<SpellFlags, List<string>> spellbooksbytes = new()
		{
			{ SpellFlags.ExitBook, new() { "164816164916", "165816165916" } },
			{ SpellFlags.CureBook, new() { "164A16164B16", "165A16165B16" } },
			{ SpellFlags.HealBook, new() { "164C16164D16", "165C16165D16" } },
			{ SpellFlags.LifeBook, new() { "164E16164F16", "165E16165F16" } },
			{ SpellFlags.QuakeBook, new() { "166016166116", "167016167116" } },
			{ SpellFlags.BlizzardBook, new() { "166216166316", "167216167316" } },
			{ SpellFlags.FireBook, new() { "166416166516", "167416167516" } },
			{ SpellFlags.AeroBook, new() { "166616166716", "167616167716" } },
			{ SpellFlags.ThunderSeal, new() { "166816166916", "167816167916" } },
			{ SpellFlags.WhiteSeal, new() { "166A16166B16", "167A16167B16" } },
			{ SpellFlags.MeteorSeal, new() { "166C16166D16", "167C16167D16" } },
			{ SpellFlags.FlareSeal, new() { "166E16166F16", "167E16167F16" } },
		};
		private Dictionary<CompanionsId, string> companionnames = new()
		{
			{ CompanionsId.Kaeli, "Kaeli" },
			{ CompanionsId.Tristam, "Tristam" },
			{ CompanionsId.Phoebe, "Phoebe" },
			{ CompanionsId.Reuben, "Reuben" },

		};
		public GameInfoScreen()
		{
			FragmentsCount = 0;
			ShuffledElementsType = new();
			SpellLearning = new();
			pagecount = 1;
			pageoffsets = new();
		}
		private void NewElementsIcons(FFMQRom rom)
		{
			rom.PutInBank(0x10, 0x8F00, Blob.FromHex("4000E40040001C003E003E043E0C1C000000000000000000000000003E005738237C237C037C063804083E6F5F5F7F3E00002E2E8C94F088F0846040202000000000187874220100"));
			rom.PutInBank(0x10, 0x8F60, Blob.FromHex("f40410aba24098a0100022df8d00aba2008fa0030022df8d006b"));
			rom.PutInBank(0x00, 0xBB34, Blob.FromHex("22608f10eaeaeaeaeaeaeaeaeaeaea"));
		}
		public void Write(FFMQRom rom)
		{
			GeneratePages(rom);
            NewElementsIcons(rom);

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
						//"057C000092",
						//"057C010093",
						//"057C020094",
						//"057C030095",
						//"057C040096",
						//"058d",
						//"00"
				});

			for(int i = 0; i < pageoffsets.Count; i++)
			{
                maindrawscript.Add("057C" + i.ToString().PadLeft(2, '0') + $"{(pageoffsets[i] % 0x100):X2}" + $"{(pageoffsets[i] / 0x100):X2}");
            }

			maindrawscript.Add("058d");
            maindrawscript.Add("00");

            maindrawscript.WriteAt(0x10, 0x9180, rom);
		}
		private void GeneratePages(FFMQRom rom)
		{
            int lineoffset = 0x06;

            List<string> pages = new();

            if (FragmentsCount == 0 && !ShuffledElementsType.Any() && !SpellLearning.Any())
            {
                string pagescript = $"250C1503{lineoffset:X2}19";
                pagescript += rom.TextToHex("No info available.");
                pages.Add(pagescript);
            }

            if (FragmentsCount > 0 || ShuffledElementsType.Any())
            {
                string pagescript = $"250C1503{lineoffset:X2}19";
                if (FragmentsCount > 0)
                {
                    pagescript += rom.TextToHex("Sky Fragments Required: " + FragmentsCount.ToString());
                    lineoffset += 3;
                }

                if (ShuffledElementsType.Any())
                {
                    pagescript += $"250C1503{lineoffset:X2}19";
                    pagescript += rom.TextToHex("Resist/Weak Shuffling");
                    lineoffset++;
                    pagescript += $"25102404{lineoffset:X2}110618";
                    lineoffset++;
                    pagescript += $"1505{lineoffset:X2}19";

                    ShuffledElementsType.Reverse();

                    int xcount = 0;
                    foreach (var elementgroup in ShuffledElementsType)
                    {
                        pagescript += elementsbytes[elementgroup.Item1] + "DC" + elementsbytes[elementgroup.Item2];
                        xcount++;
                        if (xcount >= 4)
                        {
                            pagescript += "01";
                            xcount = 0;
                        }
                        else
                        {
                            pagescript += "FF";
                        }
                    }
                    lineoffset += 5;
                }
                pages.Add(pagescript);
            }

            if (SpellLearning.Any())
            {
                foreach (var spelllearner in SpellLearning.OrderBy(l => l.Item1))
                {
                    pages.Add(CompanionPage(spelllearner.Item1, rom));
                }
            }

            if (pages.Count > 1)
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    pages[i] += "250C15191519";
                    pages[i] += rom.TextToHex((i + 1).ToString() + "/" + pages.Count.ToString() + " ") + "DC";
                }
            }

			pagecount = pages.Count;

            int pageoffset = 0x9200;

            foreach (var page in pages)
            {
                var pagescript = new ScriptBuilder(
                    new List<string> {
                        page,
                        "00"
                    });

                pagescript.WriteAt(0x10, pageoffset, rom);
				pageoffsets.Add(pageoffset);
                pageoffset += pagescript.Size();
            }
        }
		private string CompanionPage(CompanionsId companion, FFMQRom rom)
		{
            string pagescript = "";
            int lineoffset = 0x06;

			// name box
			pagescript += $"240101{(companionnames[companion].Length+2):X2}0318";
            pagescript += $"250C15020219";
            pagescript += rom.TextToHex(companionnames[companion]);

            // Spell window
			pagescript += $"25102401041E0518";
            pagescript += $"15020419";
            pagescript += rom.TextToHex("Spells");

            // Position cursor
            pagescript += $"15020519";

            /*
            pagescript += $"250C1503{lineoffset:X2}19";
            pagescript += rom.TextToHex(companionnames[companion] + " - Spells");
            lineoffset++;
            pagescript += $"25102404{lineoffset:X2}1A0918";
            lineoffset++;
            pagescript += $"1505{lineoffset:X2}19";*/

            var spellist = SpellLearning.Find(s => s.Item1 == companion).Item2.OrderBy(s => s.level).ToList();
            
			int xcount = 0;
            string line1 = "";
            string line2 = "";
            string line3 = "";

            foreach (var spell in spellist)
            {
                var currentspellsprite = spellbooksbytes[spell.spell];

				line1 += currentspellsprite[0];
				line2 += currentspellsprite[1];
                line3 += rom.TextToHex(spell.level.ToString().PadLeft(2));


                /*
				line1 += "FF" + currentspellsprite[0] + "FF";
                line2 += "FF" + currentspellsprite[1] + "FF";
                line3 += rom.TextToHex("Lv" + spell.level.ToString().PadLeft(2));
				*/
                xcount++;
                if (xcount >= 10)
                {
                    pagescript += line1 + "01" + line2 + "01" + line3 + "01";
                    xcount = 0;
                    line1 = "";
                    line2 = "";
                    line3 = "";
                }
                else
                {
                    line1 += "FF";
                    line2 += "FF";
                    line3 += "FF";
                }
            }
            pagescript += line1 + "01" + line2 + "01" + line3 + "01";

			return pagescript;
        }
	}
}
