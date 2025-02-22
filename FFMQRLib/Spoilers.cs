using FFMQLib;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FFMQLib
{
	class Spoilers
	{
		public string GenerateSpoilers(Flags flags, TitleScreen titlescreen, string seed, ItemsPlacement itemsplacement, GameInfoScreen gameinfo, GameLogic gamelogic, Battlefields battlefields)
		{
			string spoilers = "";
			spoilers += GenerateRomData(flags, titlescreen.versionText, titlescreen.hashText, seed) + "\n";
			if (!flags.DisableSpoilers)
			{
				spoilers += GenerateItemsPlacementSpoiler(flags, itemsplacement) + "\n";
				spoilers += GenerateInfoScreenSpoiler(gameinfo);
				spoilers += GenerateCompanionSpoilers(gameinfo, gamelogic, flags.CompanionsLocations != CompanionsLocationType.Standard || (flags.MapShuffling != MapShufflingMode.None && flags.MapShuffling != MapShufflingMode.Overworld));
				if (flags.MapShuffling != MapShufflingMode.None)
				{
					spoilers += GenerateMapSpoiler(flags, gamelogic) + "\n";
				}
				spoilers += GenerateAllItemsPlacementSpoiler(flags, itemsplacement, gamelogic, battlefields);
			}
			else
			{
				spoilers += GenerateInfoScreenSpoiler(gameinfo);
				spoilers += GenerateCompanionSpoilers(gameinfo, gamelogic, false);
			}

			return spoilers;
		}
		
		private string GenerateRomData(Flags flags, string version, string hash, string seed)
		{
			string spoilers = "";

			spoilers += "FFMQR " + version + "\n";
			spoilers += "Flags: " + flags.GenerateFlagString() + "\n";
			spoilers += "Seed: " + seed + "\n";
			spoilers += "Hash: " + hash + "\n";

			return spoilers;
		}
		private string GenerateItemsPlacementSpoiler(Flags flags, ItemsPlacement placement)
		{
			List<Items> invalidItems = new() { Items.CurePotion, Items.HealPotion, Items.Refresher, Items.Seed, Items.BombRefill, Items.ProjectileRefill };
			List<GameObjectType> validType = new() { GameObjectType.BattlefieldItem, GameObjectType.Box, GameObjectType.Chest, GameObjectType.NPC };
			List<(Items, string)> progressiveItems = new()
			{
				(Items.SteelSword, "Progressive Sword"),
				(Items.KnightSword, "Progressive Sword"),
				(Items.Excalibur, "Progressive Sword"),
				(Items.Axe, "Progressive Axe"),
				(Items.BattleAxe, "Progressive Axe"),
				(Items.GiantsAxe, "Progressive Axe"),
				(Items.CatClaw, "Progressive Claw"),
				(Items.CharmClaw, "Progressive Claw"),
				(Items.DragonClaw, "Progressive Claw"),
				(Items.Bomb, "Progressive Bomb"),
				(Items.JumboBomb, "Progressive Bomb"),
				(Items.MegaGrenade, "Progressive Bomb"),
				(Items.SteelHelm, "Progressive Helmet"),
				(Items.MoonHelm, "Progressive Helmet"),
				(Items.ApolloHelm, "Progressive Helmet"),
				(Items.SteelShield, "Progressive Shield"),
				(Items.VenusShield, "Progressive Shield"),
				(Items.AegisShield, "Progressive Shield"),
				(Items.SteelArmor, "Progressive Armor"),
				(Items.NobleArmor, "Progressive Armor"),
				(Items.GaiasArmor, "Progressive Armor"),
				(Items.Charm, "Progressive Accessory"),
				(Items.MagicRing, "Progressive Accessory"),
				(Items.CupidLocket, "Progressive Accessory"),
			};

			string spoilers = "";

			spoilers += "--- Starting Items ---\n";

			foreach (var item in placement.StartingItems)
			{
				spoilers += "  " + item + "\n";
			}

			spoilers += "\n--- Key Items Placement ---\n";
			var keyItems = placement.ItemsLocations.Where(x => !invalidItems.Contains(x.Content) && validType.Contains(x.Type)).ToList();
			var forestaKi = keyItems.Where(x => x.Region == MapRegions.Foresta).OrderBy(x => x.Location).ToList();
			var aquariaKi = keyItems.Where(x => x.Region == MapRegions.Aquaria).OrderBy(x => x.Location).ToList();
			var fireburgKi = keyItems.Where(x => x.Region == MapRegions.Fireburg).OrderBy(x => x.Location).ToList();
			var windiaKi = keyItems.Where(x => x.Region == MapRegions.Windia).OrderBy(x => x.Location).ToList();

			spoilers += "Foresta\n";
			foreach (var item in forestaKi)
			{
				string itemname = item.Content.ToString();
				if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == item.Content) > 0))
				{
					itemname = progressiveItems.Find(x => x.Item1 == item.Content).Item2;
				}

				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + itemname + "\n";
			}

			spoilers += "\nAquaria\n";
			foreach (var item in aquariaKi)
			{
				string itemname = item.Content.ToString();
				if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == item.Content) > 0))
				{
					itemname = progressiveItems.Find(x => x.Item1 == item.Content).Item2;
				}
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + itemname + "\n";
			}

			spoilers += "\nFireburg\n";
			foreach (var item in fireburgKi)
			{
				string itemname = item.Content.ToString();
				if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == item.Content) > 0))
				{
					itemname = progressiveItems.Find(x => x.Item1 == item.Content).Item2;
				}
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + itemname + "\n";
			}

			spoilers += "\nWindia\n";
			foreach (var item in windiaKi)
			{
				string itemname = item.Content.ToString();
				if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == item.Content) > 0))
				{
					itemname = progressiveItems.Find(x => x.Item1 == item.Content).Item2;
				}
				spoilers += "  " + item.Name + " (" + item.Location + ") " + " -> " + itemname + "\n";
			}

			return spoilers;
		}
		private string GenerateAllItemsPlacementSpoiler(Flags flags, ItemsPlacement placement, GameLogic gameLogic, Battlefields battlefields)
		{
			List<GameObjectType> validType = new() { GameObjectType.BattlefieldItem, GameObjectType.Box, GameObjectType.Chest, GameObjectType.NPC };
			List<SubRegions> subregions = new() { SubRegions.Foresta, SubRegions.Aquaria, SubRegions.AquariaFrozenField, SubRegions.SpencerCave, SubRegions.Fireburg, SubRegions.VolcanoBattlefield, SubRegions.Windia, SubRegions.MacShip, SubRegions.DoomCastle };
			List<(Items, string)> progressiveItems = new()
			{
				(Items.SteelSword, "Progressive Sword"),
				(Items.KnightSword, "Progressive Sword"),
				(Items.Excalibur, "Progressive Sword"),
				(Items.Axe, "Progressive Axe"),
				(Items.BattleAxe, "Progressive Axe"),
				(Items.GiantsAxe, "Progressive Axe"),
				(Items.CatClaw, "Progressive Claw"),
				(Items.CharmClaw, "Progressive Claw"),
				(Items.DragonClaw, "Progressive Claw"),
				(Items.Bomb, "Progressive Bomb"),
				(Items.JumboBomb, "Progressive Bomb"),
				(Items.MegaGrenade, "Progressive Bomb"),
				(Items.SteelHelm, "Progressive Helmet"),
				(Items.MoonHelm, "Progressive Helmet"),
				(Items.ApolloHelm, "Progressive Helmet"),
				(Items.SteelShield, "Progressive Shield"),
				(Items.VenusShield, "Progressive Shield"),
				(Items.AegisShield, "Progressive Shield"),
				(Items.SteelArmor, "Progressive Armor"),
				(Items.NobleArmor, "Progressive Armor"),
				(Items.GaiasArmor, "Progressive Armor"),
				(Items.Charm, "Progressive Accessory"),
				(Items.MagicRing, "Progressive Accessory"),
				(Items.CupidLocket, "Progressive Accessory"),
			};

			string spoilers = "";

			spoilers += "--- Full Item Placement ---\n";

			foreach (var subregion in subregions)
			{
				spoilers += "*** " + subregion.ToString() + " ***\n";
				var regionObjects = gameLogic.GameObjects.Where(o => o.SubRegion == subregion).ToList();

				var locationList = regionObjects.Select(o => o.Location).Distinct().ToList();

				foreach (var location in locationList)
				{
					spoilers += "[" + location.ToString() + "]\n";
					var locationObjects = regionObjects.Where(o => o.Location == location && o.Type != GameObjectType.Trigger).ToList();
					if (battlefields.RewardByLocation().TryGetValue(location, out var battlefieldReward))
					{
						string itemname = battlefieldReward.ToString();
						if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == battlefieldReward) > 0))
						{
							itemname = progressiveItems.Find(x => x.Item1 == battlefieldReward).Item2;
						}
						spoilers += "  " + regionObjects.Find(o => o.Location == location).Name + " -> " + itemname + "\n";
					}
					else if (locationObjects.Any())
					{
						foreach (var locObject in locationObjects)
						{
							if (placement.ItemsLocations.TryFind(l => l.Type == locObject.Type && l.ObjectId == locObject.ObjectId, out var foundObject))
							{
								string itemname = foundObject.Content.ToString();
								if (flags.ProgressiveGear && (progressiveItems.FindIndex(x => x.Item1 == foundObject.Content) > 0))
								{
									itemname = progressiveItems.Find(x => x.Item1 == foundObject.Content).Item2;
								}

								spoilers += "  " + foundObject.Name + " -> " + itemname + "\n";
							}
						}
					}
					else
					{
						spoilers += "  None\n";
					}

					spoilers += "\n";
				}
			}

			return spoilers;
		}
		private string GenerateCompanionSpoilers(GameInfoScreen screen, GameLogic gamelogic, bool spoilLocations)
		{
			string spoilers = "";

			List<(CompanionsId companion, LocationIds location, List<string> path)> locationSpoilers = new();

			if (spoilLocations)
			{
				locationSpoilers = gamelogic.CrawlForCompanionSpoiler();
			}
			
			spoilers += "--- Companions ---\n";
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };
			foreach (var companion in companions)
			{
				List<(int level, SpellFlags spell)> spellslist = new();

				var companionspells = screen.SpellLearning.Where(s => s.Item1 == companion).ToList();
				if (companionspells.Any())
				{
					spellslist = screen.SpellLearning.Find(s => s.Item1 == companion).Item2.OrderBy(s => s.level).ToList();
				}

				var companionquest = screen.Quests.Where(x => x.companion == companion).ToList();

				spoilers += "[" + companion.ToString() + "]\n";
				if (!spellslist.Any() && !companionquest.Any())
				{
					spoilers += " Nothing here.\n\n";
				}

				spoilers += "  Spells: ";
				if (companionspells.Any())
				{
					spoilers += string.Join(", ", spellslist.Select(s => s.spell.ToString() + " (level " + s.level + ")"));
				}
				else
				{
					spoilers += "None";
				}

				spoilers += "\n\n";

				if (companionquest.Any())
				{
					spoilers += "  Quests:\n";
					for (int i = 0; i < companionquest.Count; i++)
					{
						spoilers += "    " + (i + 1) + ". " + companionquest[i].description.Replace("\n  ", " ").Replace("\n", "") + "\n";
					}

					spoilers += "\n";
				}

				if (spoilLocations)
				{
					var currentLocation = locationSpoilers.Find(l => l.companion == companion);
					
					spoilers += "  Location:\n";
					spoilers += "    " + currentLocation.location.ToString() + "\n";
					
					string indent = "    ";

					foreach (var room in currentLocation.path)
					{
						spoilers += indent + "╚>" + room + "\n";
						indent += "  ";
					}
					spoilers += "\n";
				}
			}
			return spoilers;
		}
		private string GenerateInfoScreenSpoiler(GameInfoScreen screen)
		{
			string spoilers = "";

			if (screen.FragmentsCount > 0)
			{ 
				spoilers += "--- Sky Fragments ---\n";
				spoilers += "  Required Count: " + screen.FragmentsCount;
				spoilers += "\n\n";
			}

			if (screen.ShuffledElementsType.Any())
			{
				spoilers += "--- Shuffled Resists & Weaknesses ---\n";

				foreach (var elementgroup in screen.ShuffledElementsType)
				{
					spoilers += "  " + elementgroup.Item1.ToString() + " > " + elementgroup.Item2.ToString() + "\n";
				}

				spoilers = spoilers.Replace("Zombie", "Holy");
				spoilers += "\n";
			}

			return spoilers;
		}

		private string GenerateMapSpoiler(Flags flags, GameLogic gamelogic)
		{
			string spoilers = "--- Map Shuffling ---\n";

			List<SubRegions> subregions = new() { SubRegions.Foresta, SubRegions.Aquaria, SubRegions.LifeTemple, SubRegions.AquariaFrozenField, SubRegions.SpencerCave, SubRegions.Fireburg, SubRegions.VolcanoBattlefield, SubRegions.Windia, SubRegions.LightTemple, SubRegions.ShipDock, SubRegions.MacShip, SubRegions.DoomCastle };

			foreach (var subregion in subregions)
			{
				spoilers += "*** " + subregion.ToString() + " ***\n";
				var subregionroom = gamelogic.Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == subregion);
				var locationslist = subregionroom.Links.Where(l => l.Entrance > 0).Select(l => l.Location).ToList();
				var battlefieldslist = subregionroom.GameObjects.Select(o => o.Name).ToList();

				foreach (var battlefield in battlefieldslist)
				{
					spoilers += "[" + battlefield + "]\n";
				}

				if (flags.MapShuffling == MapShufflingMode.Overworld)
				{
					foreach (var location in locationslist)
					{
						spoilers += "[" + location.ToString() + "]\n";
					}
                    spoilers += "\n";
                }
				else
				{
					if (battlefieldslist.Any())
					{
                        spoilers += "\n";
                    }

					foreach (var location in locationslist)
					{
						var spoilerRooms = gamelogic.CrawlForSpoilers(location);
						//List<int> processedrooms = new();

						spoilers += "[" + location.ToString() + "]\n";

						//string baseindent = "";

						List<string> locationspoiler = new();

						spoilerRooms = spoilerRooms.OrderBy(x => x.ComputedDewy).ToList();
						spoilerRooms.Reverse();

						List<bool> touchedDepths = Enumerable.Repeat(false, 40).ToList();

						foreach (var room in spoilerRooms)
						{
							string indent = "";
							string tempspoiler = "";
							touchedDepths = touchedDepths.Select((x, i) => i <= room.Depth && x).ToList();

							if (room.Depth > 0)
							{
								indent += "  ";

								for (int i = 1; i < room.Depth; i++)
								{
									if (touchedDepths[i])
									{
										indent += "║ ";
									}
									else
									{
										indent += "  ";
									}
								}

								if (touchedDepths[room.Depth])
								{
									indent += "╟>";
								}
								else
								{
									indent += "╚>";
									touchedDepths[room.Depth] = true;
								}
							}
							else
							{
								indent += " >";
							}

							//string actualindent = string.Join("", Enumerable.Repeat("  ", (room.Depth - 1)).ToList());
							tempspoiler += indent;
							tempspoiler += room.Access.Any() ? "[" + string.Join(", ", room.Access.Select(a => a.ToString())) + "] " + room.Description : room.Description;
							tempspoiler += "\n";
							//processedrooms.Add(room.RoomId);

							locationspoiler.Add(tempspoiler);
						}

						locationspoiler.Reverse();
						spoilers += string.Join("", locationspoiler);
						spoilers += "\n";
					}
				}
			}
			
			return spoilers;
		}
	}
}
