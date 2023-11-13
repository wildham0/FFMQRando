using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Collections;
using System.Runtime.Intrinsics.Arm;


namespace FFMQLib
{
	public enum QuestsId
	{ 
		None = 0,
		SaveCrystalofEarth,
		SaveCrystalofWater,
		SaveCrystalofFire,
		SaveCrystalofWind,
		SaveQtyCrystals,
		CollectQtyItems,
		CollectQtySkyCoins,
		ClearQtyBattlefields,
		ClearLavaDomeBattlefield,
		ClearWindia1Battlefield,
		ClearWindia2Battlefield,
		DefeatMinotaur,
		DefeatSquidite,
		DefeatSnowCrab,
		DefeatJinn,
		DefeatMedusa,
		DefeatGidrah,
		DefeatDullahan,
		DefeatQtyMinibosses,
		CureKaeli,
		VisitBoneDungeon,
		VisitWintryCave,
		VisitMine,
	}
	public enum QuestRating
	{ 
		Easy = 0,
		Medium,
		Hard
	}
	public enum QuestScriptId
	{ 
		QuestCompletedBox = 0x00,
		KaeliLevelUp,
		TristamLevelUp,
		PhoebeLevelUp,
		ReubenLevelUp,
		AnyCompanionLevelUp,
		CollectQty,
		CrystalsQty

	}

	public class Quest
	{ 
		public QuestsId Name { get; set; }
		public NewGameFlagsList Gameflag { get; set; }
		public int Quantity { get; set; }
		public string Description { get; set; }
		public CompanionsId Companion { get; set; }
		public QuestRating Rating { get; set; }

		public Quest(QuestsId name, int qty, QuestRating rating, string description)
		{
			Name = name;
			Gameflag = NewGameFlagsList.None;
			Quantity = qty;
			Rating = rating;
			Description = description;
			Companion = CompanionsId.None;
		}
		public Quest(QuestsId name, int qty, QuestRating rating, CompanionsId companion, string description)
		{
			Name = name;
			Gameflag = NewGameFlagsList.None;
			Quantity = qty;
			Rating = rating;
			Description = description;
			Companion = companion;
		}
		public Quest(QuestsId name, int qty, CompanionsId companion, NewGameFlagsList flag, string description)
		{
			Name = name;
			Gameflag = NewGameFlagsList.None;
			Quantity = qty;
			Gameflag = flag;
			Description = description;
			Companion = companion;
		}
		public Quest()
		{
			Rating = QuestRating.Easy;
		}
	}

	public partial class Companions
	{
		public bool KaeliEnabled { get; set; }
		public bool TristamEnabled { get; set; }
		public bool PhoebeEnabled { get; set; }
		public bool ReubenEnabled { get; set; }
		public int QuestQuantity { get; set; }
		public List<Quest> Quests { get; set; }
		private QuestScriptsManager questsScripts;
		private LevelingType LevelingType;
		private Dictionary<CompanionsId, string> CompanionRoutines = new()
		{
			{ CompanionsId.None, "00000000" },
			{ CompanionsId.Kaeli, "0700A710" },
			{ CompanionsId.Tristam, "0720A710" },
			{ CompanionsId.Phoebe, "0740A710" },
			{ CompanionsId.Reuben, "0760A710" },
		};
		private void InitializeQuests(LevelingType type)
		{
			KaeliEnabled = true;
			TristamEnabled = true;
			PhoebeEnabled = true;
			ReubenEnabled = true;
			QuestQuantity = 1;
			Quests = new();
			questsScripts = new(0x10, 0xA700);

			LevelingType = type;

			CreateTestQuests();
			GenerateQuestsScripts();

			if (type == LevelingType.Quests)
			{
				CreateStandardQuests();
			}
			else if(type == LevelingType.SaveCrystals)
			{
				CreateCrystalQuests();
			}
		}
		private void CreateStandardQuests()
		{
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };

			Quests.Add(new Quest()
			{
				Name = QuestsId.CureKaeli,
				Gameflag = NewGameFlagsList.KaeliQuest1,
				Quantity = 0,
				Companion = CompanionsId.Kaeli,
				Description = "Give Elixir to poisoned Kaeli."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitBoneDungeon,
				Gameflag = NewGameFlagsList.TristamQuest1,
				Quantity = 0,
				Companion = CompanionsId.Tristam,
				Description = "Visit Bone Dungeon with Tristam and return to Fireburg."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitWintryCave,
				Gameflag = NewGameFlagsList.PhoebeQuest1,
				Quantity = 0,
				Companion = CompanionsId.Phoebe,
				Description = "Visit Wintry Cave with Phoebe and return to Windia."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitMine,
				Gameflag = NewGameFlagsList.ReubenQuest1,
				Quantity = 0,
				Companion = CompanionsId.Reuben,
				Description = "Visit Mine with Reuben and return to Fireburg."
			});
		}
		private void CreateExtendedQuests(MT19337 rng)
		{
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };

			int easyminibossesqty = rng.Between(2, 3);
			int mediumminibossesqty = rng.Between(4, 5);
			int hardminibossesqty = rng.Between(6, 7);

			int easycollectqty = rng.Between(20, 30);
			int mediumcollectqty = rng.Between(40, 60);
			int hardcollectqty = rng.Between(70, 99);

			bool skycoinfragment = false;
			int easyskycoinqty = rng.Between(15, 20);
			int mediumskycoinqty = rng.Between(25, 30);
			int hardskycoinqty = rng.Between(35, 38);

			int easybattlefieldsqty = rng.Between(5, 9);
			int mediumbattlefieldsqty = rng.Between(10, 14);
			int hardbattlefieldsqty = rng.Between(14, 18);

			// Clear Battlefields + battlefield hook
			// Quest Quest (just add to script)
			// Visit Quests (we'll need some talk scripts in here =/ )

			Quests = new()
			{
				new Quest(QuestsId.CureKaeli, 0, QuestRating.Hard, CompanionsId.Kaeli, "Give Elixir to poisoned Kaeli."),
				new Quest(QuestsId.VisitBoneDungeon, 0, QuestRating.Medium, CompanionsId.Tristam, "Visit Bone Dungeon with Tristam and return to Fireburg."),
				new Quest(QuestsId.VisitWintryCave, 0, QuestRating.Medium, CompanionsId.Phoebe, "Visit Wintry Cave with Phoebe and return to Windia."),
				new Quest(QuestsId.VisitMine, 0, QuestRating.Easy, CompanionsId.Reuben, "Visit Mine with Reuben and return to Fireburg."),
				new Quest(QuestsId.SaveCrystalofEarth, 0, QuestRating.Easy, "Save the Crystal of Earth."),
				new Quest(QuestsId.SaveCrystalofWater, 0, QuestRating.Medium, "Save the Crystal of Water."),
				new Quest(QuestsId.SaveCrystalofFire, 0, QuestRating.Medium, "Save the Crystal of Fire."),
				new Quest(QuestsId.SaveCrystalofWind, 0, QuestRating.Hard, "Save the Crystal of Wind."),
				new Quest(QuestsId.SaveQtyCrystals, 2, QuestRating.Easy, "Save 2 Crystals."),
				new Quest(QuestsId.SaveQtyCrystals, 3, QuestRating.Medium, "Save 3 Crystals."),
				new Quest(QuestsId.SaveQtyCrystals, 4, QuestRating.Hard, "Save All 4 Crystals."),
				new Quest(QuestsId.CollectQtyItems, easycollectqty, QuestRating.Easy, $"Collect {easycollectqty} Refreshers."),
				new Quest(QuestsId.CollectQtyItems, mediumcollectqty, QuestRating.Medium, $"Collect {mediumcollectqty} Refreshers."),
				new Quest(QuestsId.CollectQtyItems, hardcollectqty, QuestRating.Hard, $"Collect {hardcollectqty} Refreshers."),
				new Quest(QuestsId.ClearQtyBattlefields, easybattlefieldsqty, QuestRating.Easy, $"Clear {easybattlefieldsqty} Battlefields."),
				new Quest(QuestsId.ClearQtyBattlefields, mediumbattlefieldsqty, QuestRating.Medium, $"Clear {mediumbattlefieldsqty} Battlefields."),
				new Quest(QuestsId.ClearQtyBattlefields, hardbattlefieldsqty, QuestRating.Hard, $"Clear {hardbattlefieldsqty} Battlefields."),
				new Quest(QuestsId.ClearLavaDomeBattlefield, 0, QuestRating.Medium, $"Clear Lava Dome Battlefield."),
				new Quest(QuestsId.ClearWindia1Battlefield, 0, QuestRating.Medium, $"Clear Winda Small Area Battlefield."),
				new Quest(QuestsId.ClearWindia2Battlefield, 0, QuestRating.Medium, $"Clear Windia Large Area Battlefield."),
				new Quest(QuestsId.DefeatMinotaur, 0, QuestRating.Easy, "Defeat Minotaur."),
				new Quest(QuestsId.DefeatSquidite, 0, QuestRating.Easy, "Defeat Squidite."),
				new Quest(QuestsId.DefeatSnowCrab, 0, QuestRating.Medium, "Defeat Snow Crab."),
				new Quest(QuestsId.DefeatJinn, 0, QuestRating.Medium, "Defeat Jinn."),
				new Quest(QuestsId.DefeatMedusa, 0, QuestRating.Medium, "Defeat Medusa."),
				new Quest(QuestsId.DefeatGidrah, 0, QuestRating.Hard, "Defeat Gidrah."),
				new Quest(QuestsId.DefeatDullahan, 0, QuestRating.Hard, "Defeat Dullahan."),
				new Quest(QuestsId.DefeatQtyMinibosses, easyminibossesqty, QuestRating.Easy, $"Defeat {easyminibossesqty} Minibosses."),
				new Quest(QuestsId.DefeatQtyMinibosses, mediumminibossesqty, QuestRating.Medium, $"Defeat {mediumminibossesqty} Minibosses."),
				new Quest(QuestsId.DefeatQtyMinibosses, hardminibossesqty, QuestRating.Hard, $"Defeat {hardminibossesqty} Minibosses."),
			};

			if (skycoinfragment)
			{
				Quests.AddRange(new List<Quest>()
				{
					new Quest(QuestsId.CollectQtySkyCoins, easyskycoinqty, QuestRating.Easy, $"Collect {easyskycoinqty} Sky Fragments."),
					new Quest(QuestsId.CollectQtySkyCoins, mediumskycoinqty, QuestRating.Medium, $"Collect {mediumskycoinqty} Sky Fragments."),
					new Quest(QuestsId.CollectQtySkyCoins, hardskycoinqty, QuestRating.Hard, $"Collect {hardskycoinqty} Sky Fragments."),
				});
			}
		}
		private void CreateTestQuests()
		{
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };

			Quests = new List<Quest>()
			{
				new Quest(QuestsId.CollectQtyItems, 10, CompanionsId.Tristam, NewGameFlagsList.TristamQuest1, "Collect 10 Refreshers"),
				new Quest(QuestsId.SaveQtyCrystals, 1, CompanionsId.Tristam, NewGameFlagsList.TristamQuest2, "Save 1 Crystal"),
			};
		}
		private void CreateCrystalQuests()
		{
			Quests = new();
			QuestQuantity = 4;
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };
			Dictionary<CompanionsId, List<NewGameFlagsList>> flagslist = new()
			{
				{ CompanionsId.Kaeli, new List<NewGameFlagsList> { NewGameFlagsList.KaeliQuest1, NewGameFlagsList.KaeliQuest2, NewGameFlagsList.KaeliQuest3, NewGameFlagsList.KaeliQuest4, } },
				{ CompanionsId.Tristam, new List<NewGameFlagsList> { NewGameFlagsList.TristamQuest1, NewGameFlagsList.TristamQuest2, NewGameFlagsList.TristamQuest3, NewGameFlagsList.TristamQuest4, } },
				{ CompanionsId.Phoebe, new List<NewGameFlagsList> { NewGameFlagsList.PhoebeQuest1, NewGameFlagsList.PhoebeQuest2, NewGameFlagsList.PhoebeQuest3, NewGameFlagsList.PhoebeQuest4, } },
				{ CompanionsId.Reuben, new List<NewGameFlagsList> { NewGameFlagsList.ReubenQuest1, NewGameFlagsList.ReubenQuest2, NewGameFlagsList.ReubenQuest3, NewGameFlagsList.ReubenQuest4, } },
			};

			foreach (var companion in companions)
			{
				Quests.Add(new Quest()
				{ 
					Name = QuestsId.SaveCrystalofEarth,
					Gameflag = flagslist[companion][0],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Earth"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofWater,
					Gameflag = flagslist[companion][1],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Water"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofFire,
					Gameflag = flagslist[companion][2],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Fire"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofWind,
					Gameflag = flagslist[companion][3],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Wind"
				});
			}
		}
		private void QuestRoutines(FFMQRom rom)
		{
			// Write Scripts
			questsScripts.Write(rom);

			// Main routine, check for no companion then level
			rom.PutInBank(0x10, 0xA340, Blob.FromHex("08e220ad9010c9fff00c2000a0a9288d1e0022029b00286b"));
			rom.PutInBank(0x10, 0xA360, Blob.FromHex("08e220ad9010c9fff00c2000a0286b")); // same but don't trigger screen refresh

			// Companion routine, check for companion, then go to main routine
			//rom.PutInBank(0x10, 0xA360, Blob.FromHex("08e220ad9e0ec901d0032240a310286b"));
			//rom.PutInBank(0x10, 0xA370, Blob.FromHex("08e220ad9e0ec902d0032240a310286b"));
			//rom.PutInBank(0x10, 0xA380, Blob.FromHex("08e220ad9e0ec903d0032240a310286b"));
			//rom.PutInBank(0x10, 0xA390, Blob.FromHex("08e220ad9e0ec904d0032240a310286b"));

			// Items Qty Script Handler
			rom.PutInBank(0x10, 0xA370, Blob.FromHex("08e220c210a20000bd9e0ec913f00ce8e8e0080090f29c9e00286be8bd9e0e8d9e00286b"));
			rom.PutInBank(0x03, 0x8A24, Blob.FromHex("07" + questsScripts.GetAddress(QuestScriptId.CollectQty) + "1000"));

			// Battlefield Script Handler
			rom.PutInBank(0x02, 0x89F5, Blob.FromHex("20e0ffeaeaea"));
			rom.PutInBank(0x02, 0xFFE0, Blob.FromHex("a25fd520358822a0a3109006a2e0ff20358822d4a3109006a2e0ff20358860")); // Messages + Check
			rom.PutInBank(0x03, 0xFFE0, Blob.FromHex("2c2527aac860429c69c3bfb8c7b8b7ce22")); // Jingle + Text

			var bfqtyquests = Quests.Where(q => q.Name == QuestsId.ClearQtyBattlefields).ToList();
			int bfqtyqty = 0xFF;
			int bfqtyflag = 0x00;
			CompanionsId bfqtycompanion= CompanionsId.None;

			if (bfqtyquests.Any())
			{
				bfqtyflag = (int)bfqtyquests[0].Gameflag;
				bfqtyqty = bfqtyquests[0].Quantity;
				bfqtycompanion = bfqtyquests[0].Companion;
			}

			var bfclearquests = Quests.Where(q => q.Name >= QuestsId.ClearLavaDomeBattlefield && q.Name <= QuestsId.ClearWindia2Battlefield).ToList();
			int bfclearqty = 0xFF;
			int bfclearflag = 0x00;
			CompanionsId bfclearcompanion = CompanionsId.None;

			int targetbf = 0xFF;

			if (bfclearquests.Any())
			{
				bfclearflag = (int)bfclearquests[0].Gameflag;
				bfclearqty = bfclearquests[0].Quantity;
				bfclearcompanion = bfclearquests[0].Companion;

				if (bfclearquests[0].Name == QuestsId.ClearLavaDomeBattlefield)
				{
					targetbf = (int)LocationIds.VolcanoBattlefield01;
				}
				else if (bfclearquests[0].Name == QuestsId.ClearWindia1Battlefield)
				{
					targetbf = (int)LocationIds.WindiaBattlefield01;
				}
				else if (bfclearquests[0].Name == QuestsId.ClearWindia2Battlefield)
				{
					targetbf = (int)LocationIds.WindiaBattlefield02;
				}
			}

			rom.PutInBank(0x10, 0xA3A0, Blob.FromHex($"08e230a9{bfqtyflag:X2}22769700d027a000a201bdd30fd001c8e8e01590f5c0{bfqtyqty:X2}9014a9{bfqtyflag:X2}22609700ad920ec9{bfqtycompanion:X2}d00422a0a31028386b286b08e230a9{bfclearflag:X2}22769700d01ba2{targetbf:X2}bdd30fd014a9{bfclearflag:X2}22609700ad920ec9{bfclearcompanion:X2}d00422a0a31028386b286b"));

		}
		public void GenerateQuestsScripts()
		{
			FFMQRom rom = new();

			// Quest Completed Box
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"2a20542527a054ffff",
				"1A00" + rom.TextToHex("\n          Quest Completed!") + "36",
				"00"
			}));

			// Script Kaeli 0x00
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"08" + questsScripts.GetAddress(QuestScriptId.QuestCompletedBox),
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050901[07]",
				"2C2A27",
				"0940A310",
				"00"
			}));

			// Script Tristam 0x01
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"08" + questsScripts.GetAddress(QuestScriptId.QuestCompletedBox),
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050902[07]",
				"2C2A27",
				"0940A310",
				"00"
			}));

			// Script Phoebe 0x02
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"08" + questsScripts.GetAddress(QuestScriptId.QuestCompletedBox),
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050903[07]",
				"2C2A27",
				"0940A310",
				"00"
			}));
			// Script Reuben 0x03
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"08" + questsScripts.GetAddress(QuestScriptId.QuestCompletedBox),
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050904[07]",
				"2C2A27",
				"0940A310",
				"00"
			}));

			// Uodate individual companion scripts 
			CompanionRoutines = new()
			{
				{ CompanionsId.None, "00000000" },
				{ CompanionsId.Kaeli, "07" + questsScripts.GetAddress(QuestScriptId.KaeliLevelUp) + "10" },
				{ CompanionsId.Tristam, "07" + questsScripts.GetAddress(QuestScriptId.TristamLevelUp) + "10" },
				{ CompanionsId.Phoebe, "07" + questsScripts.GetAddress(QuestScriptId.PhoebeLevelUp) + "10" },
				{ CompanionsId.Reuben, "07" + questsScripts.GetAddress(QuestScriptId.ReubenLevelUp) + "10" },
			};

			// Any companion
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"08" + questsScripts.GetAddress(QuestScriptId.QuestCompletedBox),
				"0F9010",
				"0BFF[05]",
				"2C2A27",
				"0940A310",
				"00"
			}));

			// Item Quantity Quest
			var itemsquests = Quests.Where(q => q.Name == QuestsId.CollectQtyItems).ToList();
			int itemsqty = 0xFF;
			int itemsflag = 0x00;
			CompanionsId itemscompanion = CompanionsId.None;

			if (itemsquests.Any())
			{
				itemsflag = (int)itemsquests[0].Gameflag;
				itemsqty = itemsquests[0].Quantity;
				itemscompanion = itemsquests[0].Companion;
			}

			var skyfragmentsquests = Quests.Where(q => q.Name == QuestsId.CollectQtySkyCoins).ToList();
			int skyfragmentsqty = 0xFF;
			int skyfragmentsflag = 0x00;
			CompanionsId skyfragmentscompanion = CompanionsId.None;

			if (skyfragmentsquests.Any())
			{
				skyfragmentsflag = (int)skyfragmentsquests[0].Gameflag;
				skyfragmentsqty = skyfragmentsquests[0].Quantity;
				skyfragmentscompanion = skyfragmentsquests[0].Companion;
			}

			// Collect Script
			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				"05FD81[02]",
				"09ED9B00",
				"51",
				(itemsflag != 0x00) ? $"2E{itemsflag:X2}[08]" : "0A[08]",             // skip all if already done
				"0970A310",                                                           // get refresher qty
				$"0506{itemsqty:X2}[08]",                                             // if smaller, jump
				$"23{itemsflag:X2}",                                                  // setflag
				CompanionRoutines[itemscompanion],
				(skyfragmentsflag != 0x00) ? $"2E{skyfragmentsflag:X2}[08]" : "00",   // skip all if already done
				"0F930E",                                                             // get sky fragment qty
				$"0506{skyfragmentsqty:X2}[08]",                                      // if smaller, jump
				$"23{skyfragmentsflag:X2}",                                           // setflag
				CompanionRoutines[skyfragmentscompanion],
				"00"
			}));

			// Save Qty Crystals Quest
			var matchedquests = Quests.Where(q => q.Name == QuestsId.SaveQtyCrystals).ToList();
			int crystalqty = 0xFF;
			int crystalflag = 0x00;
			CompanionsId crystalcompanion = CompanionsId.None;

			if (matchedquests.Any())
			{
				crystalflag = (int)matchedquests[0].Gameflag;
				crystalqty = matchedquests[0].Quantity;
				crystalcompanion = matchedquests[0].Companion;
			}

			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				$"2E{crystalflag:X2}[13]", // if quest already fufilled, skip all
				"053B00",
				"050B01[04]",
				"1301",
				"050B12[06]",
				"1301",
				"050B03[08]",
				"1301",
				"050B05[10]",
				"1301",
				$"0506{crystalqty:X2}[13]", // if not enough yet, jump
				$"23{crystalflag:X2}",
				CompanionRoutines[crystalcompanion],
				"00",
			}));

		}
		public NewGameFlagsList GetQuestFlag(QuestsId quest, CompanionsId companion = CompanionsId.None)
		{
			var matchedquests = Quests.Where(q => q.Name == quest && ((companion != CompanionsId.None) ? q.Companion == companion : true)).ToList();

			if (!matchedquests.Any())
			{
				return NewGameFlagsList.None;
			}
			else
			{
				return matchedquests[0].Gameflag;
			}
		}
		public string GetQuestString(QuestsId quest)
		{
			var matchedquests = Quests.Where(q => q.Name == quest).ToList();

			if (!matchedquests.Any())
			{
				return "";
			}

			if (LevelingType == LevelingType.SaveCrystals)
			{
				string script = "";

				foreach (var matchedquest in matchedquests)
				{
					script += $"23{matchedquest.Gameflag:X2}";
				}

				script += "07" + questsScripts.GetAddress(QuestScriptId.AnyCompanionLevelUp) + "10";
				return script;
			}
			else
			{
				var selectedQuest = matchedquests[0];

				if (selectedQuest.Name == QuestsId.SaveQtyCrystals)
				{
					return "07" + questsScripts.GetAddress(QuestScriptId.CrystalsQty) + "10";
				}
				else
				{
					return $"23{(int)selectedQuest.Gameflag:X2}" + CompanionRoutines[selectedQuest.Companion];
				}


				
			}
		}
	}

}
