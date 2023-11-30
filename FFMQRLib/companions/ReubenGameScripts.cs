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
		public void UpdateReubenScripts(Flags flags, CompanionLocation reubendata, Dictionary<ItemGivingNPCs, Items> itemsPlacement, MT19337 rng)
		{
			// Reuben
			List<string> reubenDiseaseList = new()
			{
				"Arthritis",
				"Boneitis",
				"Tumasyphilisitisosis",
				"Lupus",
				"Geostigma",
				"Amoria Phlebitis",
				"Cutie Pox",
				"Imminent Death Syndrome",
				"Kalavirus",
				"FoxDie Virus",
				"Phazon Madness",
				"Tiberium Poisoning"
			};

			List<string> reubenJoinDialogueList = new()
			{
				"They almost found a cure for my boneitis, but Tristam sold the company for a cool 100 GP.",
				"I think I have whiplash... I meant ass - whiplash.",
				"COUGH COUGH Desert Fever is no joke..."
			};

			if (Companions.Available[CompanionsId.Reuben])
			{
				// Location 1
				TalkScripts.AddScript((int)TalkScriptsList.ReubenFireburg,
					new ScriptBuilder(new List<string>
					{
						TextToHex($"Help you? Oh! Uh... Oh no! My {rng.PickFrom(reubenDiseaseList)} is acting up! Arrgh, the pain... No? Alright...") + "36",
						"0F8B0E",       // get facing position
						"057C00[11]",   // looking up
						"057C01[12]",   // looking right
						"057C02[13]",   // looking down
						"057C03[14]",   // looking left
						$"2C4{(int)reubendata.GameObject:X1}46",       // hide
						CompanionSwitchRoutine,       // update current companion flags
						$"05E6{(int)CompanionsId.Reuben:X2}085B85", // join
						$"2B{(int)NewGameFlagsList.ShowFireburgReuben1:X2}2B{(int)NewGameFlagsList.ShowFireburgReuben2:X2}", // update tristam flag
						"00",
						$"2C1{(int)reubendata.GameObject:X1}4200",
						$"2C1{(int)reubendata.GameObject:X1}4300",
						$"2C1{(int)reubendata.GameObject:X1}4000",
						$"2C1{(int)reubendata.GameObject:X1}4100",
				   }));

				// Mine - Handled by Megagrenade Script

				// Fireburg 2
				// Create extra Reuben object
				int reuben2id = MapObjects[0x30].Count;
				int reubensceneid = reuben2id + 1;
				MapObjects[0x30].Add(new MapObject(MapObjects[0x30][0x01]));
				MapObjects[0x30].Add(new MapObject(MapObjects[0x30][0x01]));

				var reuben2 = MapObjects[0x30][reuben2id];
				reuben2.Gameflag = (int)NewGameFlagsList.ShowFireburgReuben2;
				reuben2.Value = (int)TalkScriptsList.ReubenFireburg2;
				GameFlags[(int)NewGameFlagsList.ShowFireburgReuben2] = false;

				var reubenobject = MapObjects[0x30][reubensceneid];
				reubenobject.Gameflag = 0xFE;
				reubenobject.Value = 0x50;
				reubenobject.X = 0x21;
				reubenobject.Y = 0x2B;

				// Process Quest
				var reubenquest = Companions.GetQuestFlag(QuestsId.VisitMine, CompanionsId.Reuben);
				TileScripts.AddScript((int)TileScriptsList.EnterReubenHouse,
					new ScriptBuilder(new List<string>
					{
						"2C1002",
						(reubenquest != NewGameFlagsList.None) ? $"2E{(int)reubenquest:X2}[08]" : "00",
						$"050f{(int)CompanionsId.Reuben:X2}[08]",
						$"050B{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[08]",
						$"2A3{reubensceneid:X1}461{reubensceneid:X1}431{reubensceneid:X1}443054FFFF",
						"1A50" + TextToHex("Whew. I'm exhausted. Let's never go back to the Mine again.") + "36",
						Companions.GetQuestString(QuestsId.VisitMine),
						$"2A1{reubensceneid:X1}414{reubensceneid:X1}46FFFF",
						"00",
					}));

				TalkScripts.AddScript((int)TalkScriptsList.ReubenFireburg2,
					new ScriptBuilder(new List<string>
					{
						"04",
						(reubenquest != NewGameFlagsList.None) ? $"2E{(int)reubenquest:X2}[05]" : "0A[05]",
						$"1A{(int)TalkScriptsList.ReubenFireburg2:X2}" + TextToHex("Whew. That was exhausting. Let's never go back to the Mine again.") + "36",
						Companions.GetQuestString(QuestsId.VisitMine),
						"00",
                        $"1A{(int)TalkScriptsList.ReubenFireburg2:X2}" + TextToHex(rng.PickFrom(reubenJoinDialogueList)) + "36",
						$"2A1{reuben2id:X1}434{reuben2id:X1}46FFFF",
						CompanionSwitchRoutine,
						$"05E6{(int)CompanionsId.Reuben:X2}075B8503",
						$"2B{(int)NewGameFlagsList.ShowFireburgReuben2:X2}",
						"00"
					}));
			}
			else
			{
				// Location 1
				TalkScripts.AddScript((int)TalkScriptsList.ReubenFireburg,
					new ScriptBuilder(new List<string>
					{
						TextToHex($"Help you? Oh! Uh... I guess hiking a bit will help with my {rng.PickFrom(reubenDiseaseList)}. On to the Mine.") + "36",
						reubendata.GetWalkOutScript(),
						$"23{(int)NewGameFlagsList.ShowMineReuben:X2}2B{(int)NewGameFlagsList.ShowFireburgReuben1:X2}", // update tristam flag
						"00",
				   }));

				// Mine
				MapObjects[0x34][0x00].Gameflag = (byte)NewGameFlagsList.ShowMineReuben;
				MapObjects[0x34][0x00].Value = (byte)TalkScriptsList.ReubenMine;
				MapObjects[0x34][0x00].X = 0x0D;
				MapObjects[0x34][0x00].Y = 0x05;
				MapObjects[0x34][0x00].Behavior = 0x0A;
				MapObjects[0x34][0x00].Orientation = 0x02;

				TalkScripts.AddScript((int)TalkScriptsList.ReubenMine,
					new ScriptBuilder(new List<string>
					{
						"04",
						$"2E{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[06]",
						$"1a{(int)TalkScriptsList.ReubenMine:X2}" + TextToHex("Ugh, my feet are killing me! Do me a favor and hold this on the way back. It's weighting a ton!"),
						$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeFallBasin]:X2}0162",
						$"23{(int)NewGameFlagsList.ReubenMineItemGiven:X2}23{(int)NewGameFlagsList.ShowFireburgReuben2:X2}2B{(int)NewGameFlagsList.ShowMineReuben:X2}",
						"00",
						$"1a{(int)TalkScriptsList.ReubenMine:X2}" + TextToHex("You wouldn't give me a lift back to Fireburg, eh? Right, right..."),
						"00"
					}));

				// Fireburg 2
				// Create extra Reuben object
				int reuben2id = MapObjects[0x30].Count;
				MapObjects[0x30].Add(new MapObject(MapObjects[0x30][0x01]));

				var reuben2 = MapObjects[0x30][reuben2id];
				reuben2.Gameflag = (int)NewGameFlagsList.ShowFireburgReuben2;
				reuben2.Value = (int)TalkScriptsList.ReubenFireburg2;
				GameFlags[(int)NewGameFlagsList.ShowFireburgReuben2] = false;

				TileScripts.AddScript((int)TileScriptsList.EnterReubenHouse,
					new ScriptBuilder(new List<string>
					{
						"2C1002",
						 "00",
					}));

				TalkScripts.AddScript((int)TalkScriptsList.ReubenFireburg2,
					new ScriptBuilder(new List<string>
					{
						TextToHex(rng.PickFrom(reubenJoinDialogueList)) + "36",
						"00"
					}));
			}
		}
	}
}
