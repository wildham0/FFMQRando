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
		ClearSpecificBattlefield,
		DefeatMinotaur,
		DefeatSquidite,
		DefeatSnowCrab,
		DefeatJinn,
		DefeatMedusa,
		DefeatGidrah,
		DefeatDullahan,
		DefeatQtyMinibosses,
		SaveArion,
		BuildRainbowBridge,
		ThawAquaria,
		VisitPointlessLedge,
		VisitLightTemple,
		VisitTreeHouses,
		VisitMountGale,
		VisitChocobo,
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
		CrystalsQty,
		BossesQty,
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
		private List<CompanionsId> companionslist = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };
		private Dictionary<CompanionsId, string> CompanionRoutines = new()
		{
			{ CompanionsId.None, "00000000" },
			{ CompanionsId.Kaeli, "0700A710" },
			{ CompanionsId.Tristam, "0720A710" },
			{ CompanionsId.Phoebe, "0740A710" },
			{ CompanionsId.Reuben, "0760A710" },
		};
		private Dictionary<CompanionsId, List<NewGameFlagsList>> gameflagsList = new()
		{
			{ CompanionsId.Kaeli, new List<NewGameFlagsList> { NewGameFlagsList.KaeliQuest1, NewGameFlagsList.KaeliQuest2, NewGameFlagsList.KaeliQuest3, NewGameFlagsList.KaeliQuest4, } },
			{ CompanionsId.Tristam, new List<NewGameFlagsList> { NewGameFlagsList.TristamQuest1, NewGameFlagsList.TristamQuest2, NewGameFlagsList.TristamQuest3, NewGameFlagsList.TristamQuest4, } },
			{ CompanionsId.Phoebe, new List<NewGameFlagsList> { NewGameFlagsList.PhoebeQuest1, NewGameFlagsList.PhoebeQuest2, NewGameFlagsList.PhoebeQuest3, NewGameFlagsList.PhoebeQuest4, } },
			{ CompanionsId.Reuben, new List<NewGameFlagsList> { NewGameFlagsList.ReubenQuest1, NewGameFlagsList.ReubenQuest2, NewGameFlagsList.ReubenQuest3, NewGameFlagsList.ReubenQuest4, } },
		};
		public void SetQuests(Flags flags, GameInfoScreen screen, MT19337 rng)
		{
			KaeliEnabled = true;
			TristamEnabled = true;
			PhoebeEnabled = true;
			ReubenEnabled = true;
			QuestQuantity = 1;
			Quests = new();
			questsScripts = new(0x10, 0xA700);

			if (levelingType == LevelingType.Quests)
			{
				CreateStandardQuests();
			}
			else if (levelingType == LevelingType.SaveCrystals)
			{
				CreateCrystalQuests();
			}
			else if (levelingType == LevelingType.QuestsExtended)
			{
				CreateExtendedQuests(flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin, flags.DoomCastleShortcut, rng);
			}

			GenerateQuestsScripts();
			AddQuestsToGameInfoScreen(screen);
		}
		private void CreateStandardQuests()
		{
			Quests.Add(new Quest()
			{
				Name = QuestsId.CureKaeli,
				Gameflag = NewGameFlagsList.KaeliQuest1,
				Quantity = 0,
				Companion = CompanionsId.Kaeli,
				Description = "Give Elixir to\n  poisoned Kaeli."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitBoneDungeon,
				Gameflag = NewGameFlagsList.TristamQuest1,
				Quantity = 0,
				Companion = CompanionsId.Tristam,
				Description = "Visit Bone Dungeon with\n  Tristam and go to Fireburg."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitWintryCave,
				Gameflag = NewGameFlagsList.PhoebeQuest1,
				Quantity = 0,
				Companion = CompanionsId.Phoebe,
				Description = "Visit Wintry Cave with\n  Phoebe and go to Windia."
			});

			Quests.Add(new Quest()
			{
				Name = QuestsId.VisitMine,
				Gameflag = NewGameFlagsList.ReubenQuest1,
				Quantity = 0,
				Companion = CompanionsId.Reuben,
				Description = "Visit Mine with Reuben and\n  return to Fireburg."
			});
		}
		private void CreateExtendedQuests(bool skycoinfragment, bool darkkingshorcut, MT19337 rng)
		{
			int easyminibossesqty = rng.Between(2, 3);
			int mediumminibossesqty = rng.Between(4, 5);
			int hardminibossesqty = rng.Between(6, 7);

			int easycollectqty = rng.Between(15, 25);
			int mediumcollectqty = rng.Between(26, 35);
			int hardcollectqty = rng.Between(40, 50);

			int easyskycoinqty = rng.Between(15, 20);
			int mediumskycoinqty = rng.Between(25, 30);
			int hardskycoinqty = rng.Between(35, 38);

			int easybattlefieldsqty = rng.Between(5, 9);
			int mediumbattlefieldsqty = rng.Between(10, 14);
			int hardbattlefieldsqty = rng.Between(14, 18);

			List<Quest> availableQuests = new()
			{
				new Quest(QuestsId.CureKaeli, 0, QuestRating.Hard, CompanionsId.Kaeli, "Give Elixir to\n  poisoned Kaeli."),
				new Quest(QuestsId.VisitBoneDungeon, 0, QuestRating.Medium, CompanionsId.Tristam, "Visit Bone Dungeon with\n  Tristam and go to Fireburg."),
				new Quest(QuestsId.VisitWintryCave, 0, QuestRating.Medium, CompanionsId.Phoebe, "Visit Wintry Cave with\n  Phoebe and go to Windia."),
				new Quest(QuestsId.VisitMine, 0, QuestRating.Easy, CompanionsId.Reuben, "Visit Mine with Reuben and\n  return to Fireburg."),
				new Quest(QuestsId.SaveCrystalofEarth, 0, QuestRating.Easy, "Save the Crystal\n  of Earth."),
				new Quest(QuestsId.SaveCrystalofWater, 0, QuestRating.Medium, "Save the Crystal\n  of Water."),
				new Quest(QuestsId.SaveCrystalofFire, 0, QuestRating.Medium, "Save the Crystal of Fire.\n"),
				new Quest(QuestsId.SaveCrystalofWind, 0, QuestRating.Hard, "Save the Crystal of Wind.\n"),
				new Quest(QuestsId.SaveQtyCrystals, 2, QuestRating.Easy, "Save 2 Crystals.\n"),
				new Quest(QuestsId.SaveQtyCrystals, 3, QuestRating.Medium, "Save 3 Crystals.\n"),
				new Quest(QuestsId.SaveQtyCrystals, 4, QuestRating.Hard, "Save All 4 Crystals.\n"),
				new Quest(QuestsId.CollectQtyItems, easycollectqty, QuestRating.Easy, $"Collect {easycollectqty} Refreshers.\n"),
				new Quest(QuestsId.CollectQtyItems, mediumcollectqty, QuestRating.Medium, $"Collect {mediumcollectqty} Refreshers.\n"),
				new Quest(QuestsId.CollectQtyItems, hardcollectqty, QuestRating.Hard, $"Collect {hardcollectqty} Refreshers.\n"),
				new Quest(QuestsId.ClearQtyBattlefields, easybattlefieldsqty, QuestRating.Easy, $"Clear {easybattlefieldsqty} Battlefields.\n"),
				new Quest(QuestsId.ClearQtyBattlefields, mediumbattlefieldsqty, QuestRating.Medium, $"Clear {mediumbattlefieldsqty} Battlefields.\n"),
				new Quest(QuestsId.ClearQtyBattlefields, hardbattlefieldsqty, QuestRating.Hard, $"Clear {hardbattlefieldsqty} Battlefields.\n"),
				new Quest(QuestsId.ClearSpecificBattlefield, (int)LocationIds.VolcanoBattlefield01, QuestRating.Medium, $"Clear Lava Dome\n  Battlefield."),
				new Quest(QuestsId.ClearSpecificBattlefield, (int)LocationIds.WindiaBattlefield01, QuestRating.Medium, $"Clear Windia Small\n  Area Battlefield."),
				new Quest(QuestsId.ClearSpecificBattlefield, (int)LocationIds.WindiaBattlefield02, QuestRating.Medium, $"Clear Windia Large\n  Area Battlefield."),
				new Quest(QuestsId.DefeatMinotaur, 0, QuestRating.Easy, "Defeat Minotaur.\n"),
				new Quest(QuestsId.DefeatSquidite, 0, QuestRating.Easy, "Defeat Squidite.\n"),
				new Quest(QuestsId.DefeatSnowCrab, 0, QuestRating.Medium, "Defeat Snow Crab.\n"),
				new Quest(QuestsId.DefeatJinn, 0, QuestRating.Medium, "Defeat Jinn.\n"),
				new Quest(QuestsId.DefeatMedusa, 0, QuestRating.Medium, "Defeat Medusa.\n"),
				new Quest(QuestsId.DefeatGidrah, 0, QuestRating.Hard, "Defeat Gidrah.\n"),
				new Quest(QuestsId.DefeatDullahan, 0, QuestRating.Hard, "Defeat Dullahan.\n"),
				new Quest(QuestsId.DefeatQtyMinibosses, easyminibossesqty, QuestRating.Easy, $"Defeat {easyminibossesqty} Minibosses.\n"),
				new Quest(QuestsId.DefeatQtyMinibosses, mediumminibossesqty, QuestRating.Medium, $"Defeat {mediumminibossesqty} Minibosses.\n"),
				new Quest(QuestsId.DefeatQtyMinibosses, hardminibossesqty, QuestRating.Hard, $"Defeat {hardminibossesqty} Minibosses.\n"),
				new Quest(QuestsId.SaveArion, 0, QuestRating.Medium, $"Save Arion.\n"),
				new Quest(QuestsId.ThawAquaria, 0, QuestRating.Medium, $"Thaw Aquaria.\n"),
				new Quest(QuestsId.VisitChocobo, 0, QuestRating.Medium, $"Visit the Chocobo\n  in Winda."),
				new Quest(QuestsId.VisitLightTemple, 0, QuestRating.Medium, $"Visit the Light Temple.\n"),
				new Quest(QuestsId.VisitPointlessLedge, 0, QuestRating.Medium, $"Visit the Pointless Ledge\n  in Lava Dome."),
				new Quest(QuestsId.VisitTreeHouses, 0, QuestRating.Medium, $"Someone is hiding in\n  the Alive Forest treehouses."),
				new Quest(QuestsId.VisitMountGale, 0, QuestRating.Medium, $"Visit the upper right\n  ledge on Mount Gale."),
			};

			if (skycoinfragment)
			{
				availableQuests.AddRange(new List<Quest>()
				{
					new Quest(QuestsId.CollectQtySkyCoins, easyskycoinqty, QuestRating.Easy, $"Collect {easyskycoinqty} Sky Fragments.\n"),
					new Quest(QuestsId.CollectQtySkyCoins, mediumskycoinqty, QuestRating.Medium, $"Collect {mediumskycoinqty} Sky Fragments.\n"),
					new Quest(QuestsId.CollectQtySkyCoins, hardskycoinqty, QuestRating.Hard, $"Collect {hardskycoinqty} Sky Fragments.\n"),
				});
			}

			if (darkkingshorcut)
			{
				availableQuests.AddRange(new List<Quest>()
				{
					new Quest(QuestsId.BuildRainbowBridge, 0, QuestRating.Medium, $"Build the Rainbow Bridge."),
				});
			}

			List<QuestRating> questratings = new() { QuestRating.Easy, QuestRating.Medium, QuestRating.Medium, QuestRating.Hard };
			int currentflag = 0;
			foreach (var rating in questratings)
			{
				foreach (var companion in companionslist)
				{
					var validquests = availableQuests.Where(x => x.Rating == rating && (x.Companion == companion || x.Companion == CompanionsId.None)).ToList();
					if (!validquests.Any())
					{
						validquests = availableQuests.Where(x => x.Rating == QuestRating.Medium && (x.Companion == companion || x.Companion == CompanionsId.None)).ToList();
					}
					var selectedquest = rng.PickFrom(validquests);
					selectedquest.Companion = companion;
					selectedquest.Gameflag = gameflagsList[companion][currentflag];
					Quests.Add(selectedquest);

					availableQuests = availableQuests.Where(x => x.Name != selectedquest.Name).ToList();
				}
				currentflag++;
			}
		}
		private void AddQuestsToGameInfoScreen(GameInfoScreen screen)
		{
			screen.Quests = Quests.Select(q => (q.Companion, q.Gameflag, q.Description)).ToList();
		}
		private void CreateCrystalQuests()
		{
			Quests = new();
			QuestQuantity = 4;

			foreach (var companion in companionslist)
			{
				Quests.Add(new Quest()
				{ 
					Name = QuestsId.SaveCrystalofEarth,
					Gameflag = gameflagsList[companion][0],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Earth"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofWater,
					Gameflag = gameflagsList[companion][1],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Water"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofFire,
					Gameflag = gameflagsList[companion][2],
					Quantity = 0,
					Companion = companion,
					Description = "Save the Crystal of Fire"
				});

				Quests.Add(new Quest()
				{
					Name = QuestsId.SaveCrystalofWind,
					Gameflag = gameflagsList[companion][3],
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
			int bfqtycompanion = (int)CompanionsId.None;

			if (bfqtyquests.Any())
			{
				bfqtyflag = (int)bfqtyquests[0].Gameflag;
				bfqtyqty = bfqtyquests[0].Quantity;
				bfqtycompanion = (int)bfqtyquests[0].Companion;
			}

			var bfclearquests = Quests.Where(q => q.Name == QuestsId.ClearSpecificBattlefield).ToList();
			int bfclearqty = 0xFF;
			int bfclearflag = 0x00;
			int bfclearcompanion = (int)CompanionsId.None;

			int targetbf = 0xFF;

			if (bfclearquests.Any())
			{
				bfclearflag = (int)bfclearquests[0].Gameflag;
				bfclearqty = bfclearquests[0].Quantity;
				bfclearcompanion = (int)bfclearquests[0].Companion;
				targetbf = bfclearquests[0].Quantity;
			}

			rom.PutInBank(0x10, 0xA3A0, Blob.FromHex($"08e230a9{bfqtyflag:X2}22769700d027a000a201bdd30fd001c8e8e01590f5c0{bfqtyqty:X2}9014a9{bfqtyflag:X2}22609700ad920ec9{bfqtycompanion:X2}d00422a0a31028386b286b08e230a9{bfclearflag:X2}22769700d01ba2{targetbf:X2}bdd30fd014a9{bfclearflag:X2}22609700ad920ec9{bfclearcompanion:X2}d00422a0a31028386b286b"));

		}
		private void GenerateQuestsScripts()
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

			// Update individual companion scripts 
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
				(skyfragmentsflag != 0x00) ? $"2E{skyfragmentsflag:X2}[13]" : "00",   // skip all if already done
				"0F930E",                                                             // get sky fragment qty
				$"0506{skyfragmentsqty:X2}[13]",                                      // if smaller, jump
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

			// Defeat Qty Minibosses
			var minibossesquests = Quests.Where(q => q.Name == QuestsId.DefeatQtyMinibosses).ToList();
			int minibossesqty = 0xFF;
			int minibossesflag = 0x00;
			CompanionsId minibossescompanion = CompanionsId.None;

			if (minibossesquests.Any())
			{
				minibossesflag = (int)minibossesquests[0].Gameflag;
				minibossesqty = minibossesquests[0].Quantity;
				minibossescompanion = minibossesquests[0].Companion;
			}

			questsScripts.AddScript(new ScriptBuilder(new List<string>()
			{
				$"2E{minibossesflag:X2}[19]", // if quest already fufilled, skip all
				"053B00",
				"2E15[04]", // Minotaur
				"1301",
				"2E24[06]", // Squidite
				"1301",
				"2E25[08]", // Snow Crab
				"1301",
				"2E26[10]", // Jinn
				"1301",
				"2E27[12]", // Medusa
				"1301",
				"2E28[14]", // Gidrah
				"1301",
				"2E2A[16]", // Dullahan
				"1301",
				$"0506{minibossesqty:X2}[19]", // if not enough yet, jump
				$"23{minibossesflag:X2}",
				CompanionRoutines[minibossescompanion],
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

			if (levelingType == LevelingType.SaveCrystals)
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
				else if (selectedQuest.Name == QuestsId.DefeatQtyMinibosses)
				{
					return "07" + questsScripts.GetAddress(QuestScriptId.BossesQty) + "10";
				}
				else
				{
					return $"23{(int)selectedQuest.Gameflag:X2}" + CompanionRoutines[selectedQuest.Companion];
				}
			}
		}
	}

}
