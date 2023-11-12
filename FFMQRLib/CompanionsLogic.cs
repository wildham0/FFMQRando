using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Collections;


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

			LevelingType = type;

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

			// Acquire X items + treasure hook
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
			var scriptkaeli = new ScriptBuilder(new List<string>()
			{
				"0880A7",
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050901[07]",
				"2C2A27",
				"0940A310",
				"00"
			});

			var scripttristam = new ScriptBuilder(new List<string>()
			{
				"0880A7",
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050902[07]",
				"2C2A27",
				"0940A310",
				"00"
			});

			var scriptphoebe = new ScriptBuilder(new List<string>()
			{
				"0880A7",
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050903[07]",
				"2C2A27",
				"0940A310",
				"00"
			});

			var scriptreuben = new ScriptBuilder(new List<string>()
			{
				"0880A7",
				"0F9010",
				"0BFF[07]",
				"0F920E",
				"050904[07]",
				"2C2A27",
				"0940A310",
				"00"
			});

			var scriptbox = new ScriptBuilder(new List<string>()
			{
				"2a20542527a054ffff",
				"1A00" + rom.TextToHex("\n          Quest Completed!") + "36",
				"00"
			});

			var scriptanycompanion = new ScriptBuilder(new List<string>()
			{
				"0880A7",
				"0F9010",
				"0BFF[05]",
				"2C2A27",
				"0940A310",
				"00"
			});



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

			var scriptcrystalqty = new ScriptBuilder(new List<string>()
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
				$"0506{crystalqty}[13]", // if not enough yet, jump
				$"23{crystalflag:X2}",
				CompanionRoutines[crystalcompanion],
				"00",
			});

			scriptkaeli.WriteAt(0x10, 0xA700, rom);
			scripttristam.WriteAt(0x10, 0xA720, rom);
			scriptphoebe.WriteAt(0x10, 0xA740, rom);
			scriptreuben.WriteAt(0x10, 0xA760, rom);
			scriptbox.WriteAt(0x10, 0xA780, rom);
			scriptanycompanion.WriteAt(0x10, 0xA7B0, rom);
			scriptcrystalqty.WriteAt(0x10, 0xA7D0, rom);

			// Main routine, check for no companion then level
			rom.PutInBank(0x10, 0xA340, Blob.FromHex("08e220ad9010c9fff00c2000a0a9288d1e0022029b00286b"));

			// Companion routine, check for companion, then go to main routine
			rom.PutInBank(0x10, 0xA360, Blob.FromHex("08e220ad9e0ec901d0032240a310286b"));
			rom.PutInBank(0x10, 0xA370, Blob.FromHex("08e220ad9e0ec902d0032240a310286b"));
			rom.PutInBank(0x10, 0xA380, Blob.FromHex("08e220ad9e0ec903d0032240a310286b"));
			rom.PutInBank(0x10, 0xA390, Blob.FromHex("08e220ad9e0ec904d0032240a310286b"));
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

				script += "07B0A710";
				return script;
			}
			else
			{
				var selectedQuest = matchedquests[0];

				if (selectedQuest.Name == QuestsId.SaveQtyCrystals)
				{
					return "07D0A710";
				}
				else
				{
					return $"23{(int)selectedQuest.Gameflag:X2}" + CompanionRoutines[selectedQuest.Companion];
				}


				
			}
		}
	}

}
