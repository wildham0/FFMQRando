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
		public void UpdateTristamScripts(Flags flags, CompanionLocation tristamdata, Dictionary<ItemGivingNPCs, Items> itemsPlacement, MT19337 rng)
		{
			if (Companions.Available[CompanionsId.Tristam])
			{
				// Location 1				
				TalkScripts.AddScript((int)TalkScriptsList.TristamChest,
					new ScriptBuilder(new List<string>
					{
						TextToHex("Defeat evil? Treasure hunting? Sounds like a great business opportunity here. I'm in!") + "36",
						"0F8B0E",       // get facing position
						"057C00[11]",   // looking up
						"057C01[12]",   // looking right
						"057C02[13]",   // looking down
						"057C03[14]",   // looking left
						$"2C4{(int)tristamdata.GameObject:X1}46",       // hide
						CompanionSwitchRoutine,       // update current companion flags
						$"05E6{(int)CompanionsId.Tristam:X2}085B85", // join
						$"2B{(int)NewGameFlagsList.ShowSandTempleTristam:X2}2B{(int)NewGameFlagsList.ShowFireburgTristam:X2}", // update tristam flag
						"00",
						$"2C1{(int)tristamdata.GameObject:X1}4200",
						$"2C1{(int)tristamdata.GameObject:X1}4300",
						$"2C1{(int)tristamdata.GameObject:X1}4000",
						$"2C1{(int)tristamdata.GameObject:X1}4100",
				   }));

				// Bone Dungeon
				//GameFlags[0xC9] = false;
				TileScripts.AddScript((int)TileScriptsList.BoneDungeonTristamBomb,
					new ScriptBuilder(new List<string>
					{
						$"2e{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[15]",
						$"050f{(int)CompanionsId.Tristam:X2}[15]",
						"2a3046104130441054ffff",
						$"0C0015{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}",
						flags.ProgressiveGear ? "09309411" : "",
						"2BFC",
						"1a85" + TextToHex("Care to invest in my ") + "087DFE" + TextToHex(" venture? I'll give you an early prototype!") + "36",
						"08D0FD",
						"050BFB[12]",
						"1a85" + TextToHex("That's fine, not everyone is cut out for massive profits and a lifetime of riches.") + "36",
						"2a10434046ffff",
						"00",
						$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}0162",
						"2a10434046ffff", // 24ff > d3fe
						$"23{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}",
						"00"
					}));

				// Fireburg Hotel
				// On Entrance
				// Create extra Tristam object
				MapObjects[0x31].Add(new MapObject(MapObjects[0x31][0x0B]));
				MapObjects[0x31][0x0B].CopyFrom(MapObjects[0x31][0x01]);
				var tristamobject = MapObjects[0x31][0x0B];

				tristamobject.Gameflag = 0xFE;
				tristamobject.Value = 0x50;
				tristamobject.X = 0x11;
				tristamobject.Y = 0x35;

				// Process Quest
				var tristamquest = Companions.GetQuestFlag(QuestsId.VisitBoneDungeon, CompanionsId.Tristam);

				TileScripts.AddScript((int)TileScriptsList.EnterFireburgHotel,
					new ScriptBuilder(new List<string>
					{
						"2C1102",
						$"050f{(int)CompanionsId.Tristam:X2}[19]",
						 (tristamquest != NewGameFlagsList.None) ? $"2E{(int)tristamquest:X2}[10]" : "0A[10]",
						$"050B{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[09]",
						"2A3B461B431B443054FFFF",
						"1A50" + TextToHex("That was some good dungeon pillaging back there. Let's get some drinks!") + "36",
						Companions.GetQuestString(QuestsId.VisitBoneDungeon),
						"08[16]",
						"2A1B414B46FFFF",
						"00",
						$"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[19]",
                        $"050B{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[19]",
                        "2A3B461B431B443054FFFF",
						"08[16]",
						"2A1B414B46FFFF",
						"00",
						"1A50" + TextToHex("Tell you what, I'll give you this and you cover my tab, deal?") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
						$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
						"00"
					}));

				// Tristam Fireburg
				List<string> tristamJoinDialogueList = new()
				{
					"Hey! Call me a Treasure Hunter or I'll rip your lungs out!",
					"Death is always a step behind me..."
				};

				TalkScripts.AddScript((int)TalkScriptsList.TristamInFireburg01,
					new ScriptBuilder(new List<string>
					{
						"04",
						(tristamquest != NewGameFlagsList.None) ? $"2E{(int)tristamquest:X2}[05]" : "0A[05]",
                        $"050B{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[05]",
                        $"1a{(int)TalkScriptsList.TristamInFireburg01:X2}" + TextToHex("That was some good dungeon pillaging back there.") + "36",
                        Companions.GetQuestString(QuestsId.VisitBoneDungeon),
                        $"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[10]",
                        $"1a{(int)TalkScriptsList.TristamInFireburg01:X2}" + TextToHex("Hey! You can get this, it's free! It will only report back some of your personal user data to me.") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
						$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
						"00",
                        $"1a{(int)TalkScriptsList.TristamInFireburg01:X2}" + TextToHex(rng.PickFrom(tristamJoinDialogueList)) + "36",
						"0F8B0E",
						"057C02[20]",
						"057C03[21]",
						"057C00[22]",
						"2C4146",
                        CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Tristam:X2}085B85",
						$"2B{(int)NewGameFlagsList.ShowFireburgTristam:X2}",
						"00",
						"2C114000",
						"2C114100",
						"2C114200"
					}));
			}
			else
			{
				// Location 1
				TalkScripts.AddScript((int)TalkScriptsList.TristamChest,
					new ScriptBuilder(new List<string>
					{
						TextToHex("Defeat evil? Treasure hunting? Smells like profits. I'll see you in Bone Dungeon!") + "36",
						tristamdata.GetWalkOutScript(),
						$"23{(int)NewGameFlagsList.ShowBoneDungeonTristam:X2}2B{(int)NewGameFlagsList.ShowSandTempleTristam:X2}2B{(int)NewGameFlagsList.ShowFireburgTristam:X2}", // update tristam flag
						"00",
				   }));

				// Bone Dungeon
				MapObjects[0x14][0x00].Gameflag = (byte)NewGameFlagsList.ShowBoneDungeonTristam;
				MapObjects[0x14][0x00].Value = (byte)TalkScriptsList.TristamBoneDungeon;
				MapObjects[0x14][0x00].X++;
				MapObjects[0x14][0x00].Behavior = 0x0A;
				MapObjects[0x14][0x00].Orientation = 0x02;
                MapObjects[0x14][0x00].UnknownIndex = 0x02;
                GameFlags[(int)NewGameFlagsList.ShowBoneDungeonTristam] = false;

                TileScripts.AddScript((int)TileScriptsList.BoneDungeonTristamBomb,
					new ScriptBuilder(new List<string>
					{
                        "00"
					}));

                TalkScripts.AddScript((int)TalkScriptsList.TristamBoneDungeon,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[13]",
						$"0C0015{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}",
						flags.ProgressiveGear ? "09309411" : "",
						"2BFC",
                        $"1a{(int)TalkScriptsList.TristamBoneDungeon:X2}" + TextToHex("Care to invest in my ") + "077DFE03" + TextToHex(" venture? I'll give you an early prototype!") + "36",
						"07D0FD03",
						"050BFB[10]",
                        $"1a{(int)TalkScriptsList.TristamBoneDungeon:X2}" + TextToHex("That's fine, not everyone is cut out for massive profits and a lifetime of riches.") + "36",
						"00",
						$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}0162",
						$"23{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}2B{(int)NewGameFlagsList.ShowBoneDungeonTristam:X2}23{(int)NewGameFlagsList.ShowFireburgTristam:X2}",
						"00",
                        $"1a{(int)TalkScriptsList.TristamBoneDungeon:X2}" + TextToHex("Let's go to Fireburg, have a nice cold pint, and wait for all of this to blow over.") + "36",
						"00"
					}));

				// Fireburg Hotel
				// On Entrance
				TileScripts.AddScript((int)TileScriptsList.EnterFireburgHotel,
					new ScriptBuilder(new List<string>
					{
						"2C1102",
						"00"
					}));

				// Tristam Fireburg
				List<string> tristambarDialogueList = new()
				{
					"Nothing like 'soda pop' to quench your thirst!",
					"My job here's over. I've earned my fee! Ta-ta......!"
				};

				TalkScripts.AddScript((int)TalkScriptsList.TristamInFireburg01,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[06]",
                        $"1a{(int)TalkScriptsList.TristamInFireburg01:X2}" + TextToHex("Hey! You can get this, it's free! It will only report back some of your personal user data to me.") + "36",
						$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
						$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
						"00",
                        $"1a{(int)TalkScriptsList.TristamInFireburg01:X2}" + TextToHex(rng.PickFrom(tristambarDialogueList)) + "36",
						"00",
					}));
			}
		}
	}
}
