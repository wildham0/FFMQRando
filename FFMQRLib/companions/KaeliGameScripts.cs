using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using static FFMQLib.FFMQRom;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public enum KaeliMode
		{
			Standard = 0,
			CompanionOnly,
			QuestOnly,
			Potato
		}
		public void UpdateKaeliScripts(Flags flags, CompanionLocation kaelidata, Dictionary<ItemGivingNPCs, Items> itemsPlacement, MT19337 rng)
		{
			bool kaelismom = flags.KaelisMomFightMinotaur;
			bool kaelienabled = Companions.Available[CompanionsId.Kaeli];

            KaeliMode mode = KaeliMode.Standard;
			if (kaelienabled && !kaelismom)
			{
				mode = KaeliMode.Standard;
			}
			else if (kaelienabled && kaelismom)
			{
				mode = KaeliMode.CompanionOnly;
			}
			else if (!kaelienabled && !kaelismom)
			{
				mode = KaeliMode.QuestOnly;
			}
			else if (!kaelienabled && kaelismom)
			{
				mode = KaeliMode.Potato;
			}

			GameFlags[(int)NewGameFlagsList.ShowLevelForestKaeli] = false;
			// Location 1 - Kaeli
			if (mode == KaeliMode.Standard)
			{
				TalkScripts.AddScript((int)TalkScriptsList.KaeliWitherTree,
					new ScriptBuilder(new List<string>
					{
						"04",
						"2d" + ScriptItemFlags[Items.TreeWither].Item1,
						"050c" + ScriptItemFlags[Items.TreeWither].Item2 + "[04]",
						$"1A{(int)TalkScriptsList.KaeliWitherTree:X2}" + TextToHex("Hey there, lubber. The forest is dying? Tell that to the &%?! marines! It's totally fine.") + "3600",
						$"1A{(int)TalkScriptsList.KaeliWitherTree:X2}" + TextToHex("!&%?! Look at that rotten hollard! Ok, let's go Johnny raw, let's chop some cursed tree!") + "36",
						"0F8B0E",       // get facing position
						"057C00[15]",   // looking up
						"057C01[16]",   // looking right
						"057C02[17]",   // looking down
						"057C03[18]",   // looking left
						$"2C4{(int)kaelidata.GameObject:X1}46",       // hide					"2C4146",
						CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
						$"2B{(int)NewGameFlagsList.ShowForestaKaeli:X2}2B6C",
						"00",
						$"2C1{(int)kaelidata.GameObject:X1}4200",
						$"2C1{(int)kaelidata.GameObject:X1}4300",
						$"2C1{(int)kaelidata.GameObject:X1}4000",
						$"2C1{(int)kaelidata.GameObject:X1}4100",
					}));
			}
			else if (mode == KaeliMode.CompanionOnly)
			{
				TalkScripts.AddScript((int)TalkScriptsList.KaeliWitherTree,
					new ScriptBuilder(new List<string>
					{
						TextToHex("Hey there, lubber. The forest is dying? Ok, let's go Johnny raw, let's hunt some &%?! ackmen!") + "36",
						"0F8B0E",       // get facing position
						"057C00[11]",   // looking up
						"057C01[12]",   // looking right
						"057C02[13]",   // looking down
						"057C03[14]",   // looking left
						$"2C4{(int)kaelidata.GameObject:X1}46",       // hide					"2C4146",
						CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
						$"2B{(int)NewGameFlagsList.ShowForestaKaeli:X2}2B{(int)NewGameFlagsList.ShowWindiaKaeli:X2}2B6C",
						"00",
						$"2C1{(int)kaelidata.GameObject:X1}4200",
						$"2C1{(int)kaelidata.GameObject:X1}4300",
						$"2C1{(int)kaelidata.GameObject:X1}4000",
						$"2C1{(int)kaelidata.GameObject:X1}4100",
					}));
			}
			else if (mode == KaeliMode.QuestOnly)
			{
				TalkScripts.AddScript((int)TalkScriptsList.KaeliWitherTree,
					new ScriptBuilder(new List<string>
					{
						"04",
						"2d" + ScriptItemFlags[Items.TreeWither].Item1,
						"050c" + ScriptItemFlags[Items.TreeWither].Item2 + "[04]",
						$"1A{(int)TalkScriptsList.KaeliWitherTree:X2}" + TextToHex("Hey there, lubber. The forest is dying? Tell that to the &%?! marines! It's totally fine.") + "3600",
						$"1A{(int)TalkScriptsList.KaeliWitherTree:X2}" + TextToHex("!&%?! Look at that rotten hollard! Ok, let's go Johnny raw, set sails to Level Forest!") + "36",
						kaelidata.GetWalkOutScript(),
						$"23{(int)NewGameFlagsList.ShowLevelForestKaeli:X2}2B{(int)NewGameFlagsList.ShowForestaKaeli:X2}2B6C",
						"00",
					}));
			}
			else if (mode == KaeliMode.Potato)
			{
                TalkScripts.AddScript((int)TalkScriptsList.KaeliWitherTree,
					new ScriptBuilder(new List<string>
					{
                        TextToHex("Hey there, lubber. The forest is dying? Tell that to the &%?! marines! It's totally fine.") + "3600",
					}));
            }

            // Foresta
            // Kaeli's Mom
			if (mode == KaeliMode.CompanionOnly || mode == KaeliMode.Potato)
            {
				MapObjects[0x10][0x00].Gameflag = (byte)NewGameFlagsList.ShowForestaKaelisMom;
                MapObjects[0x10][0x00].Value = (byte)TalkScriptsList.KaelisMomQuest;
				GameFlags[(int)NewGameFlagsList.ShowForestaKaelisMom] = true;

                TalkScripts.AddScript((int)TalkScriptsList.KaelisMomQuest,
                    new ScriptBuilder(new List<string>
                    {
                        "04",
                        "2d" + ScriptItemFlags[Items.TreeWither].Item1,
                        "050c" + ScriptItemFlags[Items.TreeWither].Item2 + "[04]",
                        $"1A{(int)TalkScriptsList.KaelisMomQuest:X2}" + TextToHex("The forest? Only The Void is my master, I await its wishes.") + "3600",
                        $"1A{(int)TalkScriptsList.KaelisMomQuest:X2}" + TextToHex("A sign from The Void! We'll meet in Level Forest to bring inexistence to its enemies.") + "36",
                        "2A4042204320424046FFFF",
                        $"23{(int)NewGameFlagsList.ShowLevelForestKaeli:X2}2B{(int)NewGameFlagsList.ShowForestaKaelisMom:X2}2B6C",
                        "00",
                    }));
            }

            // Foresta
            // Sick Kaeli/Mom
            if (mode == KaeliMode.Standard)
            {
                TalkScripts.AddScript((int)TalkScriptsList.SickKaeli,
                    new ScriptBuilder(new List<string>
					{
						"04",
	                    "2d" + ScriptItemFlags[Items.Elixir].Item1,
		                "050c" + ScriptItemFlags[Items.Elixir].Item2 + "[04]",
			            "1A16" + TextToHex("I'll be fine, mate. Nothing a grog and some rest can't fix. I'll be back on deck in no time.") + "3600",
				        "1A16" + TextToHex("Mate! Psha, this taste like &?!% kelt, but I'm ready to show a leg! Heave and rally!") + "36",
					    Companions.GetQuestString(QuestsId.CureKaeli),
	                    "0F890E",
		                "057C0C[14]",
			            "057C0E[15]",
				        "2C4246",
                        CompanionSwitchRoutine,
	                    $"05E6{(int)CompanionsId.Kaeli:X2}085B85",
		                $"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}",
			            "00",
				        "2C124300",
					    "2C124100"
                    }));
            }
            else if (mode == KaeliMode.CompanionOnly || mode == KaeliMode.Potato)
            {
				MapObjects[0x10][0x02].Sprite = 0x40;

                TalkScripts.AddScript((int)TalkScriptsList.SickKaeli,
                    new ScriptBuilder(new List<string>
                    {
                        "04",
                        "2d" + ScriptItemFlags[Items.Elixir].Item1,
                        "050c" + ScriptItemFlags[Items.Elixir].Item2 + "[04]",
                        "1A16" + TextToHex("No need to fear, we all join The Void in the End.") + "3600",
                        "1A16" + TextToHex("Medicine? I can still serve The Void in this Plane. My pilgrimage to Windia begins.") + "36",
						"2A72424246FFFF",
                        Companions.GetQuestString(QuestsId.CureKaeli),
                        $"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}23{(int)NewGameFlagsList.ShowWindiaKaelisMom:X2}",
                        $"050f{(int)CompanionsId.Kaeli:X2}[10]",
						"00",
                        $"23{(int)NewGameFlagsList.ShowWindiaKaeli:X2}2B{(int)NewGameFlagsList.ShowForestaKaeli:X2}",
                        "00",
                    }));
            }
            else if (mode == KaeliMode.QuestOnly)
            {
                TalkScripts.AddScript((int)TalkScriptsList.SickKaeli,
                    new ScriptBuilder(new List<string>
                    {
                        "04",
                        "2d" + ScriptItemFlags[Items.Elixir].Item1,
                        "050c" + ScriptItemFlags[Items.Elixir].Item2 + "[04]",
                        "1A16" + TextToHex("I'll be fine, mate. Nothing a grog and some rest can't fix. I'll be back on deck in no time.") + "3600",
                        "1A16" + TextToHex("Mate! Psha, this taste like &?!% kelt, but I'm ready to show a leg! Heave and rally!") + "36",
                        "2A72424246FFFF",
                        Companions.GetQuestString(QuestsId.CureKaeli),
                        $"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}23{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",
                    }));
            }

            // Alive Forest
            // Entrance

            string treecuttingdialogue = kaelismom ? "Tree, your death is a small sacrifice, but the path opened is great. Praise the Void!" : "There, griffin. Path is cleared. Let's find that decaying !&%? piece of lumber.";

			var kaeliquestcheck = (mode == KaeliMode.Standard) ? $"050f{(int)CompanionsId.Kaeli:X2}[11]" : $"050B{(int)NewGameFlagsList.ShowLevelForestKaeli:X2}[11]";

			TileScripts.AddScript((int)TileScriptsList.EnterLevelForest,
				new ScriptBuilder(new List<string>
				{
					"2C0101",
                    $"2e{(int)NewGameFlagsList.KaeliOpenedPath:X2}[11]",						    // path opened?, true jump
                    kaeliquestcheck,                    // is kaeli in party, false jump
					"2d" + ScriptItemFlags[Items.TreeWither].Item1,
                    "050D" + ScriptItemFlags[Items.TreeWither].Item2 + "[11]",                      // do we have tree wither?, false jump
					"2A334663401343FFFF",
                    "2A2344505010530054FFFF",
					"1A82" + TextToHex(treecuttingdialogue) + "36",
                    "2C0344" + "09209511093d8c00" + "2C0825" + "09309511093d8c00" + "2C6822",
                    "2A13424346FFFF",
                    $"23{(int)NewGameFlagsList.KaeliOpenedPath:X2}23{(int)NewGameFlagsList.EnableMinotaurFight:X2}",
					"00"
				}));

            string poisoneddialogue = kaelismom ? "Poison? A blessing in disguise, my voyage to The Void will only be the quicker." : "&%?!! That son of a harpooner just poisoned me! Let's do for this &?!% baracoota!";

            string giveitemdialogue = kaelismom ? "Don't cry for my imminent demise. Take this material object, I won't need it inside The Void." : "You're a &?%! agonist, mate! Here, you earned it. Split a few skulls for me!";

			// Minotaur
			var kaeliminotaurcheck = (mode == KaeliMode.Standard) ? $"050f{(int)CompanionsId.Kaeli:X2}[24]" : $"050B{(int)NewGameFlagsList.ShowLevelForestKaeli:X2}[24]";

            TileScripts.AddScript((int)TileScriptsList.FightMinotaur,
				new ScriptBuilder(new List<string>
				{
                    "050B63[24]",
                    $"050B{(int)NewGameFlagsList.KaeliOpenedPath:X2}[24]",						    // path opened?, false jump
                    kaeliminotaurcheck,
					//"2C0344" + "09209511093d8c00" + "2C0825" + "09309511093d8c00" + "2C6822",
                    //"2A3546 0054 105a 0e25 2727 5246 022A 05453055FFFF",
					"2A35460054105aFFFF" + "09209511093d8c00" + "2a63fb0e25ffff" + "09309511093d8c00" + "2A27275246022A05453055FFFF",
                    "1A0BAE63FF57FF57CE30ACC8C5C3C5BCC6B8CE36",
                    //"2A 1B27 8544 1054 3055 5054 FFFF",
					//"2A1B2785441054FFFF" + "09209511093d8c00" + "2C3055" + "09309511093d8c00" + "2C5054",
                    "2A1B278544105430555054FFFF",
                    "1A82" + TextToHex(poisoneddialogue),
                    "36",
                    "2A75448544754405440054FFFF",
                    "05E41714",
                    kaelismom ? "2A6246854410543544FFFF" : "2A62468544105436465640FFFF",
                    "1A82" + TextToHex(giveitemdialogue) + "36",
                    kaelismom ? "" : "2C7544",
                    kaelismom ? "" : "2C8544",
                    $"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliForesta]:X2}0162",
                    $"23{(int)NewGameFlagsList.ShowSickKaeli:X2}",
                    (mode == KaeliMode.Standard) ? CompanionSwitchRoutine + "61" : "",
                    kaelismom ? "2A65424546FFFF" : "2A1640454666424646FFFF",
                    "236D",
                    "231E",
                    "2B63",
                    "2B15",
                    Companions.GetQuestString(QuestsId.DefeatMinotaur),
                    Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
                    "00"
				}));

			// Windia
			MapSpriteSets[0x23].AddAddressor(5, 0, 0x06, SpriteSize.Tiles16); // Add Kaeli's Mom

			if (mode == KaeliMode.Standard)
			{
				// Enter Inn bedroom with Kaeli
				MapObjects[0x52][0x04].CopyFrom(MapObjects[0x52][0x00]);
				var kaeliobject = MapObjects[0x52][0x04];

				kaeliobject.Gameflag = 0xFE;
				kaeliobject.Value = 0x51;
				kaeliobject.X = 0x21;
				kaeliobject.Y = 0x3B;

				TileScripts.AddScript((int)TileScriptsList.EnterWindiaInnBedroom,
					new ScriptBuilder(new List<string>
					{
						"2CD700",
						$"050f{(int)CompanionsId.Kaeli:X2}[08]",
						$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[08]",
						"2A3446144314443054FFFF",
						"1A51" + TextToHex("Hearty, mate. This is straight from my ?%!& ditty-bag, but I want you to have it!") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
						$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
						"2A14414446FFFF",
						"00",
					}));

				// NPC Kaeli
				MapObjects[0x52][0x00].Gameflag = (byte)NewGameFlagsList.ShowWindiaKaeli;
				MapObjects[0x52][0x00].Value = 0x5B;

				TalkScripts.AddScript((int)TalkScriptsList.KaeliWindia,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[06]",
                        $"1A{(int)TalkScriptsList.KaeliWindia:X2}" + TextToHex("Hearty, mate. This is straight from my ?%!& ditty-bag, but I want you to have it!") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
						$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
						"00",
						$"1A{(int)TalkScriptsList.KaeliWindia:X2}" + TextToHex("I'm tired to play &?&% harbour-watch. Let's loose for sea, mate!") + "36",
						"0F8B0E",
						"057C02[16]",
						"057C03[17]",
						"057C01[18]",
						"2C4046",
                        CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
						$"2B{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",
						"00",
						"2C104000",
						"2C104100",
						"2C104300"
					}));
			}
			else if (mode == KaeliMode.QuestOnly)
			{
				TileScripts.AddScript((int)TileScriptsList.EnterWindiaInnBedroom,
					new ScriptBuilder(new List<string>
					{
						"2CD700",
						"00",
					}));

				// NPC Kaeli
				MapObjects[0x52][0x00].Gameflag = (byte)NewGameFlagsList.ShowWindiaKaeli;
				MapObjects[0x52][0x00].Value = 0x5B;

				TalkScripts.AddScript((int)TalkScriptsList.KaeliWindia,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[06]",
						"1A5B" + TextToHex("Hearty, mate. This is straight from my ?%!& ditty-bag, but I want you to have it!") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
						$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
						"00",
						"1A5B" + TextToHex("I'm tired to play &?&% harbour-watch. Maybe I should risk a run.") + "36",
						"00",
					}));
			}
			else if (mode == KaeliMode.CompanionOnly || mode == KaeliMode.Potato)
			{
				TileScripts.AddScript((int)TileScriptsList.EnterWindiaInnBedroom,
					new ScriptBuilder(new List<string>
					{
						"2CD700",
						"00",
					}));

				// NPC Kaeli
				MapObjects[0x52][0x00].Gameflag = (byte)NewGameFlagsList.ShowWindiaKaeli;
				MapObjects[0x52][0x00].Value = 0x5B;

				TalkScripts.AddScript((int)TalkScriptsList.KaeliWindia,
					new ScriptBuilder(new List<string>
					{
						(mode == KaeliMode.Potato) ? (TextToHex("I'm tired to play &?&% harbour-watch. Maybe I should risk a run.") + "3600") : (TextToHex("I'm tired to play &?&% harbour-watch. Let's loose for sea, mate!") + "36"),
						"0F8B0E",
						"057C02[10]",
						"057C03[11]",
						"057C01[12]",
						"2C4046",
                        CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
						$"2B{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",
						"00",
						"2C104000",
						"2C104100",
						"2C104300"
					}));

				// NPC Kaeli's Mom
				MapObjects[0x52][0x04].CopyFrom(MapObjects[0x52][0x00]);
				var kaelismomobject = MapObjects[0x52][0x04];

				kaelismomobject.Gameflag = (int)NewGameFlagsList.ShowWindiaKaelisMom;
				kaelismomobject.Sprite = 0x3C;
				kaelismomobject.Value = (int)TalkScriptsList.KaelisMomWindia;
				kaelismomobject.X = 0x1C;
				kaelismomobject.Y = 0x3A;
				kaelismomobject.Orientation = 0x01;

				TalkScripts.AddScript((int)TalkScriptsList.KaelisMomWindia,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[06]",
						$"1A{(int)TalkScriptsList.KaelisMomWindia:X2}" + TextToHex("The Void guided my hands to this item, use it to channel the Nothing into your soul.") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
						$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
						"00",
						$"1A{(int)TalkScriptsList.KaelisMomWindia:X2}" + TextToHex("I can sense the Dark Sun rising. Soon we will all unite into The Void!") + "36",
						"00",
					}));
			}
		}
	}
}
