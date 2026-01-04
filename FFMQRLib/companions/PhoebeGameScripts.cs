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
		public void UpdatePhoebeScripts(Flags flags, CompanionLocation phoebedata, Dictionary<ItemGivingNPCs, Items> itemsPlacement, MT19337 rng)
		{
            if (Companions.Available[CompanionsId.Phoebe])
            {
                // Location 1
                TalkScripts.AddScript((int)TalkScriptsList.PhoebeLibraTemple,
                    new ScriptBuilder(new List<string>
                    {
						MQText.TextToHex("Sure, you can be my sidekick, just don't do anything stupid. I'm the heroine here!") + "36",
                        "0F8B0E",       // get facing position
						"057C00[11]",   // looking up
						"057C01[12]",   // looking right
						"057C02[13]",   // looking down
						"057C03[14]",   // looking left
						$"2C4{(int)phoebedata.GameObject:X1}46",       // hide
						CompanionSwitchRoutine,       // update current companion flags
						$"05E6{(int)CompanionsId.Phoebe:X2}085B85", // join
						$"2B{(int)NewGameFlagsList.ShowLibraTemplePhoebe:X2}", // update  flag
						"00",
                        $"2C1{(int)phoebedata.GameObject:X1}4200",
                        $"2C1{(int)phoebedata.GameObject:X1}4300",
                        $"2C1{(int)phoebedata.GameObject:X1}4000",
                        $"2C1{(int)phoebedata.GameObject:X1}4100",
                   }));

                // Wintry Cave
                MapObjects[0x1C][0x00].Y = 0x25; // Move Phoebe

                TileScripts.AddScript((int)TileScriptsList.WintryCavePhoebeClaw,
                    new ScriptBuilder(new List<string>
                    {
                        $"2e{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[07]",
                        $"050f{(int)CompanionsId.Phoebe:X2}[07]",
                        "2a3046104310443054ffff",
                        "1a8a" + MQText.TextToHex("Good job not being a clutz and falling down like an idiot! I guess that calls for a reward..."),
                        $"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeWintryCave]:X2}0162",
                        "2a10414046ffff", // 24ff > d3fe
						$"23{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}",
                        "00"
                    }));

                // Windia INN
                var phoebequest = Companions.GetQuestFlag(QuestsId.VisitWintryCave, CompanionsId.Phoebe);

                MapObjects[0x52][0x02].Value = (byte)TalkScriptsList.PhoebeInAquaria;
                TalkScripts.AddScript((int)TalkScriptsList.PhoebeInAquaria,
                    new ScriptBuilder(new List<string>
                    {
                        "04",
                        (phoebequest != NewGameFlagsList.None) ? $"2E{(int)phoebequest:X2}[05]" : "0A[05]",
                        $"1A{(int)TalkScriptsList.PhoebeInAquaria:X2}" + MQText.TextToHex("Well, you did survive the Wintry Cave, go rest a bit before we go to Doom Castle.") + "36",
                        Companions.GetQuestString(QuestsId.VisitWintryCave),
                        "00",
                        $"1A{(int)TalkScriptsList.PhoebeInAquaria:X2}" + MQText.TextToHex("Finally, my quest to slay the Dark King is coming to an end! Come, my assistant.") + "36",
                        "2C1243",
                        "2C4246",
                        CompanionSwitchRoutine,
                        $"05E6{(int)CompanionsId.Phoebe:X2}085B85",
                        $"2B{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}",
                        "00"
                    }));

                // Create extra Phoebe object
                MapObjects[0x52][0x03].CopyFrom(MapObjects[0x52][0x02]);
                var phoebeobject = MapObjects[0x52][0x03];

                phoebeobject.Gameflag = 0xFE;
                phoebeobject.Value = 0x50;
                phoebeobject.X = 0x35;
                phoebeobject.Y = 0x1B;

                // Windia Inn Entrance
                TileScripts.AddScript((int)TileScriptsList.EnterWindiaInn,
                    new ScriptBuilder(new List<string>
                    {
                        "2C1F02",
                        (phoebequest != NewGameFlagsList.None) ? $"2E{(int)phoebequest:X2}[08]" : "00",
                        $"050f{(int)CompanionsId.Phoebe:X2}[08]",
                        $"050B{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[08]",
                        "2A3346134313443054FFFF",
                        "1A50" + MQText.TextToHex("Well, you did survive the Wintry Cave, you can rest a bit before we go to Doom Castle.") + "36",
                        Companions.GetQuestString(QuestsId.VisitWintryCave),
                        "2A13414346FFFF",
                        "00",
                    }));
            }
            else
            {
                TalkScripts.AddScript((int)TalkScriptsList.PhoebeLibraTemple,
                    new ScriptBuilder(new List<string>
                    {
						MQText.TextToHex("Sure, you can be my sidekick, just don't fall behind. Come, to the Wintry Cave!") + "36",
                        phoebedata.GetWalkOutScript(),
                        $"23{(int)NewGameFlagsList.ShowWintryCavePhoebe:X2}2B{(int)NewGameFlagsList.ShowLibraTemplePhoebe:X2}", // update tristam flag
						"00",
                   }));

                // Wintry Cave
                MapObjects[0x1C][0x00].Gameflag = (byte)NewGameFlagsList.ShowWintryCavePhoebe;
                MapObjects[0x1C][0x00].Value = (byte)TalkScriptsList.PhoebeWintryCave;
                MapObjects[0x1C][0x00].X = 0x30;
                MapObjects[0x1C][0x00].Y = 0x22;
                MapObjects[0x1C][0x00].Behavior = 0x0A;
                MapObjects[0x1C][0x00].UnknownIndex = 0x02;
                MapObjects[0x1C][0x00].Facing = FacingOrientation.Down;
                GameFlags[(int)NewGameFlagsList.ShowWintryCavePhoebe] = false;


                TileScripts.AddScript((int)TileScriptsList.WintryCavePhoebeClaw,
                    new ScriptBuilder(new List<string>
                    {
                        "00"
                    }));

                TalkScripts.AddScript((int)TalkScriptsList.PhoebeWintryCave,
                    new ScriptBuilder(new List<string>
                    {
                        "04",
                        $"2E{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[06]",
                        $"1a{(int)TalkScriptsList.PhoebeWintryCave:X2}" + MQText.TextToHex("Can you be slower? Here, I already found this while you were dallying around."),
                        $"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeWintryCave]:X2}0162",
                        $"23{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}23{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}2B{(int)NewGameFlagsList.ShowWintryCavePhoebe:X2}",
                        "00",
                        $"1a{(int)TalkScriptsList.PhoebeWintryCave:X2}" + MQText.TextToHex("I'm ready to face Dark King, I'm going back to my base of operations in Windia.") + "3600",
                    }));

                // Windia INN
                MapObjects[0x52][0x02].Value = (byte)TalkScriptsList.PhoebeInAquaria;
                TalkScripts.AddScript((int)TalkScriptsList.PhoebeInAquaria,
                    new ScriptBuilder(new List<string>
                    {
                        MQText.TextToHex("Finally, my quest to slay the Dark King is coming to an end!") + "36",
                        "00"
                    }));

                TileScripts.AddScript((int)TileScriptsList.EnterWindiaInn,
                    new ScriptBuilder(new List<string>
                    {
                        "2C1F02",
                        "00",
                    }));
            }
        }
	}
}
