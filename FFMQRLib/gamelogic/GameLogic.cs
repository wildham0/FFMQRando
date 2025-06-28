using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using System.Xml.Linq;

namespace FFMQLib
{
	public partial class GameLogic
	{
		public List<Room> Rooms { get; set; }
		public List<GameObject> GameObjects { get; set; }
		private List<(int, (LocationIds, int), List<AccessReqs>, int)> accessQueue;
		private List<(int, LocationIds)> locationQueue;
		private int locationCount;
		private List<int> regionRoomIds;
		public GameLogic(ApConfigs apconfigs)
		{
			if (apconfigs.ApEnabled)
			{
				ReadFromAp(apconfigs.RoomsYaml);
			}
			else
			{
				ReadRooms();
			}
		}
		public GameLogic()
		{
			ReadRooms();
		}
		private void ReadFromAp(string aprooms)
		{
			if (aprooms == null || aprooms == "")
			{
				ReadRooms();
			}
			else
			{
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(UnderscoredNamingConvention.Instance)
					.Build();

				try
				{
					Rooms = deserializer.Deserialize<List<Room>>(aprooms);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}

		public void ReadRooms()
		{
			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("rooms.yaml"));
			using (Stream logicfile = assembly.GetManifestResourceStream(filepath))
			{
				using (StreamReader reader = new StreamReader(logicfile))
				{
					yamlfile = reader.ReadToEnd();
				}
			}

			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();

			var input = new StringReader(yamlfile);

			var yaml = new YamlStream();

			List<Room> result = new();

			try
			{
				result = deserializer.Deserialize<List<Room>>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			Rooms = result;
			yamlfile = "";
		}
		public string OutputRooms()
		{
			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();

			var yaml = serializer.Serialize(Rooms);
			return yaml;
		}

		public void CrawlRooms(Flags flags, Overworld overworld, EnemiesStats enemies, Companions companions, Battlefields battlefields)
		{
			// Initialization
			accessQueue = new();
			locationQueue = new();
			locationCount = 0;
			regionRoomIds = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();

			var locationLinks = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList();

			// Add Quests to Logic
			companions.AddQuestsToLogic(Rooms);

			// Process Logic Access
			if (flags.LogicOptions != LogicOptions.Expert)
			{
				// Remove path between fireburg and the frozen field from logic if not expert
				var volcanoBattlefieldRoom = Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.VolcanoBattlefield);
				volcanoBattlefieldRoom.Links.RemoveAll(l => l.Access.Contains(AccessReqs.SummerAquaria));

				var frozenFieldRoom = Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.AquariaFrozenField);
				frozenFieldRoom.Links.RemoveAll(l => l.Access.Contains(AccessReqs.DualheadHydra));
			}
			else if((flags.MapShuffling != MapShufflingMode.Everything) && !flags.OverworldShuffle && !flags.CrestShuffle)
			{
                // Add Sealed Temple Exit trick to logic in Expert mode
                var exitTrickRoom = Rooms.Find(x => x.Id == 75);
				exitTrickRoom.Links.Add(new RoomLink(74, new() { AccessReqs.ExitBook }));
			}

			// If map is shuffled, we block the one way access from Frozen Fields to Aquaria without wakewater
			if (flags.MapShuffling != MapShufflingMode.None || flags.CrestShuffle || flags.OverworldShuffle)
			{
				var frozenFieldsRoom = Rooms.Find(x => x.Id == 223);
				var aquariaAccess = frozenFieldsRoom.Links.Find(x => x.TargetRoom == 221);
				aquariaAccess.Access.Add(AccessReqs.SummerAquaria);
			}

			// Giant Tree
			var giantTreeLink = locationLinks.Find(l => l.Location == LocationIds.GiantTree);
			Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.Windia).Links.Remove(giantTreeLink);

			// Process Rooms
			ProcessRoom2(0, new() { 0 }, new(), (LocationIds.None, 0), (0, LocationIds.None));

			GameObjects = new();

			// Clean Up Access
			List<(int, (LocationIds, int), List<AccessReqs>, int)> accessToKeep = new();
			
			foreach (var room in Rooms)
			{
				var accessToCompare = accessQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item3.Count).ToList();
				List<(int, (LocationIds, int), List<AccessReqs>, int)> accessToRemove = new();

				while (accessToCompare.Any())
				{
					for (int i = 1; i < accessToCompare.Count; i++)
					{
						if (!accessToCompare[0].Item3.Except(accessToCompare[i].Item3).Any())
						{
							accessToRemove.Add(accessToCompare[i]);
						}
					}

					accessToKeep.Add(accessToCompare[0]);
					accessToRemove.Add(accessToCompare[0]);
					accessToCompare = accessToCompare.Except(accessToRemove).ToList();
				}
			}
			
			accessQueue = accessToKeep;

			Dictionary<int, int> vendorCost = new()
			{
				{ 4, 200 },
				{ 11, 500 },
				{ 16, 300 },
			};

			// Process Game Objects
			foreach (var room in Rooms)
			{
				var targetaccess = accessQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item2.Item2).ToList();
				if (!targetaccess.Any())
				{
					throw new Exception("Game Logic: Unaccessible Location\n\n" + "Room: " + room.Id);
				}

				var lowestPriority = targetaccess.First().Item2.Item2;

				if (flags.LogicOptions != LogicOptions.Expert)
				{
					targetaccess = targetaccess.Where(x => x.Item2.Item2 == lowestPriority).ToList();
				}

				List<List<AccessReqs>> finalAccess = targetaccess.Select(x => x.Item3).ToList();

				Location targetLocation = overworld.Locations.Find(x => x.LocationId == targetaccess.First().Item2.Item1);
				room.Location = locationQueue.Find(l => l.Item1 == targetaccess.First().Item4).Item2;

				foreach (var gamedata in room.GameObjects)
				{ 
					if (gamedata.Type == GameObjectType.BattlefieldXp)
					{
						var bflocation = overworld.Locations.Find(l => l.LocationId == gamedata.Location);
						var battlefieldTrigger = new GameObject(gamedata, bflocation, finalAccess);
						battlefieldTrigger.Type = GameObjectType.Trigger;
						GameObjects.Add(battlefieldTrigger);
					}
					else if (gamedata.Type == GameObjectType.BattlefieldItem)
					{
						var bflocation = overworld.Locations.Find(l => l.LocationId == gamedata.Location);
						GameObjects.Add(new GameObject(gamedata, bflocation, finalAccess));
					}
					else if (gamedata.Type == GameObjectType.BattlefieldGp)
					{
						var bflocation = overworld.Locations.Find(l => l.LocationId == gamedata.Location);
						var battlefieldTrigger = new GameObject(gamedata, bflocation, finalAccess);
						battlefieldTrigger.Type = GameObjectType.Trigger;
						GameObjects.Add(battlefieldTrigger);
					}
					else if (gamedata.Type == GameObjectType.NPC && vendorCost.ContainsKey(gamedata.ObjectId))
					{
						// Set Gp Value for Vendors
						var vendorObject = new GameObject(gamedata, targetLocation, finalAccess);
						vendorObject.Cost = vendorCost[vendorObject.ObjectId];
						GameObjects.Add(vendorObject);
					}
					else
					{
						GameObjects.Add(new GameObject(gamedata, targetLocation, finalAccess));
					}
				}
			}

			// Add Friendly logic extra requirements
			if (flags.LogicOptions == LogicOptions.Friendly && (flags.MapShuffling == MapShufflingMode.None))
			{
				foreach (var location in AccessReferences.FriendlyAccessReqs)
				{
					GameObjects.Where(x => x.Location == location.Key).ToList().ForEach(x => x.AccessRequirements.ForEach(a => a.AddRange(location.Value)));
				}
			}

			// Avoid Bosses early softlock
			List<AccessReqs> windiaBosses = new() { AccessReqs.Gidrah, AccessReqs.Dullahan, AccessReqs.Pazuzu1F };
			List<AccessReqs> otherBosses = new() { AccessReqs.FreezerCrab, AccessReqs.IceGolem, AccessReqs.Jinn, AccessReqs.Medusa, AccessReqs.DualheadHydra };
			otherBosses.AddRange(windiaBosses);
			List<AccessReqs> progressCoin = new() { AccessReqs.SandCoin, AccessReqs.RiverCoin };

			if (flags.MapShuffling != MapShufflingMode.None || flags.OverworldShuffle)
			{
				windiaBosses.Add(AccessReqs.DualheadHydra);
			}

			foreach (var boss in enemies.BossesPower)
			{
				if (GameObjects.TryFind(o => o.OnTrigger.Contains(boss.Key), out var foundboss))
				{
					foundboss.AccessRequirements.ForEach(r => r.Add(boss.Value));
				}
			}

			foreach (var battlefield in enemies.BattlefieldsPower)
			{
				if (GameObjects.TryFind(o => o.Location == battlefield.Key, out var foundbattlefield))
				{
					foundbattlefield.AccessRequirements.ForEach(r => r.Add(battlefield.Value));
				}
			}

			/*
			foreach (var gameobject in GameObjects)
			{
				foreach (var requirements in gameobject.AccessRequirements)
				{
					if (requirements.Intersect(otherBosses).Any() && gameobject.Region == MapRegions.Foresta)
					{
						requirements.AddRange(progressCoin);
					}
					else if (requirements.Intersect(windiaBosses).Any())
					{
						requirements.AddRange(progressCoin);
					}
				}
			}*/

			// Progressive Gear Logic
			if (flags.ProgressiveGear)
			{
				foreach (var gameobject in GameObjects)
				{
					foreach (var requirements in gameobject.AccessRequirements)
					{
						if (requirements.Contains(AccessReqs.DragonClaw))
						{
							requirements.AddRange(new List<AccessReqs> { AccessReqs.CatClaw, AccessReqs.CharmClaw });
						}
						
						if (requirements.Contains(AccessReqs.MegaGrenade))
						{
							requirements.AddRange(new List<AccessReqs> { AccessReqs.SmallBomb, AccessReqs.JumboBomb });
						}
					}
				}
			}

			// Set Priorization
			if (flags.ChestsShuffle == ItemShuffleChests.Prioritize)
			{
				GameObjects.Where(x => x.Type == GameObjectType.Chest && x.ObjectId < 0x20).ToList().ForEach(x => x.Prioritize = true);
			}

			if (flags.NpcsShuffle == ItemShuffleNPCsBattlefields.Prioritize)
			{
				GameObjects.Where(x => x.Type == GameObjectType.NPC).ToList().ForEach(x => x.Prioritize = true);
			}
			else if (flags.NpcsShuffle == ItemShuffleNPCsBattlefields.Exclude)
			{
				GameObjects.Where(x => x.Type == GameObjectType.NPC).ToList().ForEach(x => x.Exclude = true);
			}

			if (flags.BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Prioritize)
			{
				GameObjects.Where(x => x.Type == GameObjectType.BattlefieldItem).ToList().ForEach(x => x.Prioritize = true);
			}
			else if (flags.BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Exclude)
			{
				GameObjects.Where(x => x.Type == GameObjectType.BattlefieldItem).ToList().ForEach(x => x.Exclude = true);
			}

			GameObjects.Where(x => x.Type == GameObjectType.Box).ToList().ForEach(x => x.Exclude = false);

			// Exclude Hero Statue room's chests
			GameObjects.Where(x => x.Type == GameObjectType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList().ForEach(x => x.Exclude = true);
		}

		private void ProcessRoom(int roomid, List<int> origins, List<AccessReqs> access, (LocationIds, int) locPriority)
		{ 
			var targetroom = Rooms.Find(x => x.Id == roomid);

			LocationIds newLocation = locPriority.Item1;

			foreach (var children in targetroom.Links)
			{
				bool reachLocation = false;
				if (regionRoomIds.Contains(targetroom.Id))
				{
					if (children.Entrance >= 0)
					{
						newLocation = children.Location;
						reachLocation = true;
					}
				}

				if (!origins.Contains(children.TargetRoom))
				{
					bool traverseCrest = false;

					if (children.Access.Contains(AccessReqs.LibraCrest) || children.Access.Contains(AccessReqs.GeminiCrest) || children.Access.Contains(AccessReqs.MobiusCrest))
					{
						traverseCrest = true;
					}
					ProcessRoom(children.TargetRoom, origins.Concat(new List<int> { roomid }).ToList(), access.Concat(children.Access).ToList(), (reachLocation ? newLocation : locPriority.Item1, traverseCrest ? locPriority.Item2 + 1 : locPriority.Item2));
				}
			}

			accessQueue.Add((roomid, locPriority, access, 0));
		}
		private void ProcessRoom2(int roomid, List<int> origins, List<AccessReqs> access, (LocationIds location, int priority) locPriority, (int seekId, LocationIds locId) location)
		{
			// Get the room
			var targetroom = Rooms.Find(x => x.Id == roomid);

			// cycle each links
			foreach (var children in targetroom.Links)
			{
				// crawled location id, and location
				(int seekId, LocationIds locId) newLocation = (location.seekId, location.locId);

				bool reachLocation = false;

				// if we're in a region room, add a new location (do links have their locations set correctly?)
				// set that we reached a location
				if (regionRoomIds.Contains(targetroom.Id))
				{
					if (children.Entrance >= 0)
					{
						locationCount++;
						locationQueue.Add((locationCount, children.Location));
						newLocation = (locationCount, children.Location);

						reachLocation = true;
					}
				}

				// did we visit that room before?
				if (!origins.Contains(children.TargetRoom))
				{
					// if current room isn't a region and the link is targeting a region AND the current priority is higher than zero
					// Get that region location and update the current location
					if ((targetroom.Type != RoomType.Overworld && targetroom.Type != RoomType.Subregion) && regionRoomIds.Contains(children.TargetRoom) && locPriority.priority > 0)
					{
						var location2 = Rooms.Find(x => x.Id == children.TargetRoom).Links.Find(l => l.TargetRoom == targetroom.Id).Location;

						locationQueue.RemoveAll(l => l.Item1 == location.seekId);
						locationQueue.Add((location.seekId, location2));
						newLocation = (location.seekId, location2);
					}

					bool traverseCrest = false;
					
					// are we moving through a crest tile?
					// if so, create a new location
					if (children.Access.Contains(AccessReqs.LibraCrest) || children.Access.Contains(AccessReqs.GeminiCrest) || children.Access.Contains(AccessReqs.MobiusCrest))
					{
						traverseCrest = true;
						locationCount++;

						locationQueue.Add((locationCount, location.locId));
						newLocation = (locationCount, location.locId);
					}
					
					// Go down the link
					// if we reached a new location, use that
					// if we traversed a crest tile, increase priority
					ProcessRoom2(children.TargetRoom,
						origins.Concat(new List<int> { roomid }).ToList(),
						access.Concat(children.Access).ToList(),
						(reachLocation ? newLocation.locId : locPriority.location, traverseCrest ? locPriority.priority + 1 : locPriority.priority),
						newLocation);
				}
			}

			accessQueue.Add((roomid, locPriority, access, location.seekId));
		}
		public LocationIds FindTriggerLocation(AccessReqs trigger)
		{
			var initialRoom = Rooms.Find(x => x.GameObjects.Where(o => o.OnTrigger.Contains(trigger)).Any());
			List<AccessReqs> crestsList = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			List<int> roomToProcess = new() { initialRoom.Id };
			List<int> roomProcessed = new() { 0 };
			List<int> regionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();

			while (roomToProcess.Any())
			{
				var currentRoom = roomToProcess.First();
				var linksToProcess = Rooms.Find(x => x.Id == currentRoom).Links.Where(x => !x.Access.Intersect(crestsList).Any()).ToList();

				foreach (var link in linksToProcess)
				{
					if (regionRooms.Contains(link.TargetRoom))
					{
						return Rooms.Find(x => x.Id == link.TargetRoom).Links.Find(l => l.TargetRoom == currentRoom).Location;
						//var owLink = Rooms.Where(r => r.Type == RoomType.Location).SelectMany(r => r.Links).ToList().Find(l => l.TargetRoom == currentRoom && l.Entrance != 469);
						//return AccessReferences.LocationsByEntrances.Find(x => x.Item2 == owLink.Entrance).Item1;
					}
					else
					{
						if (!roomProcessed.Contains(link.TargetRoom))
						{
							roomToProcess.Add(link.TargetRoom);
						}
					}
				}

				roomProcessed.Add(currentRoom);
				roomToProcess.Remove(currentRoom);
			}

			return LocationIds.None;
		}

		public (LocationIds, int) CrawlForChestRating(LocationIds location)
		{
			var regionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();
			var initialRoom = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList().Find(l => l.Location == location).TargetRoom;

			List<int> chestsList = new();
			List<int> visitedRooms = regionRooms;

			ProcessRoomForChests(0, initialRoom, chestsList, visitedRooms);

			int rating = 0;

			foreach(var chest in chestsList)
			{
				if (chest == 0)
				{
					rating += 10;
				}
				else if (chest == 1)
				{
					rating += 3;
				}
				else if (chest > 1)
				{
					rating += 1;
				}
			}

			return (location, rating);
		}

		public void ProcessRoomForChests(int reqcount, int roomid, List<int> chestlist, List<int> visitedrooms)
		{
			var currentRoom = Rooms.Find(x => x.Id == roomid);

			visitedrooms.Add(roomid);

			foreach (var chest in currentRoom.GameObjects.Where(o => o.Type == GameObjectType.Chest))
			{
				chestlist.Add(reqcount + chest.Access.Count);
			}

			foreach (var link in currentRoom.Links.Where(l => !visitedrooms.Contains(l.TargetRoom) && !l.Access.Intersect(AccessReferences.CrestsAccess).Any()))
			{
				ProcessRoomForChests(reqcount + link.Access.Count, link.TargetRoom, chestlist, visitedrooms);
			}
		}

		public (LocationIds, int) CrawlForCompanionRating(LocationIds location)
		{
			var regionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();
			var initialRoom = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList().Find(l => l.Location == location).TargetRoom;

			List<int> companionsList = new();
			List<int> visitedRooms = regionRooms;

			ProcessRoomForCompanions(0, initialRoom, companionsList, visitedRooms);

			int rating = 0;

			foreach (var companion in companionsList)
			{
				if (companion == 0)
				{
					rating += 10;
				}
				else if (companion == 1)
				{
					rating += 3;
				}
				else if (companion > 1)
				{
					rating += 1;
				}
			}

			return (location, rating);
		}

		public void ProcessRoomForCompanions(int reqcount, int roomid, List<int> companionlist, List<int> visitedrooms)
		{
			var currentRoom = Rooms.Find(x => x.Id == roomid);

			visitedrooms.Add(roomid);

			foreach (var companion in currentRoom.GameObjects.Where(o => o.Type == GameObjectType.Trigger && o.OnTrigger.Intersect(AccessReferences.FavoredCompanionsAccess).Any()))
			{
				companionlist.Add(reqcount + companion.Access.Count);
			}

			foreach (var link in currentRoom.Links.Where(l => !visitedrooms.Contains(l.TargetRoom)))
			{
				ProcessRoomForCompanions(reqcount + link.Access.Count, link.TargetRoom, companionlist, visitedrooms);
			}
		}
		public List<(CompanionsId, LocationIds, List<string>)> CrawlForCompanionSpoiler()
		{
			List<(CompanionsId id, string name)> companionList = new() { (CompanionsId.Kaeli, "Kaeli Companion"), (CompanionsId.Tristam, "Tristam Companion"), (CompanionsId.Phoebe, "Phoebe Companion"), (CompanionsId.Reuben, "Reuben Companion") };
			List<(CompanionsId, LocationIds, List<string>)> resultingPaths = new();
			List<LocationIds> barredLocations = new() { LocationIds.LifeTemple, LocationIds.LightTemple, LocationIds.ShipDock };

			foreach (var companion in companionList)
			{
				if (Rooms.TryFind(r => r.GameObjects.Where(o => o.Name == companion.name).Any(), out var originRoom))
				{
					List<(LocationIds location, List<int> rooms)> validPaths = new();
					List<int> visitedRooms = new();

					ProcessCompanionSpoiler(originRoom.Id, validPaths, visitedRooms);

					validPaths = validPaths.Where(p => !barredLocations.Contains(p.location)).OrderBy(p => p.rooms.Count).ToList();
					resultingPaths.Add((companion.id, validPaths.First().location, validPaths.First().rooms.Select(r => Rooms.Find(t => t.Id == r).Name).Reverse().ToList()));
				}
			}

			return resultingPaths;
		}
		public void ProcessCompanionSpoiler(int roomid, List<(LocationIds, List<int>)> validpaths, List<int> visitedrooms)
		{
			var currentRoom = Rooms.Find(x => x.Id == roomid);

			if (currentRoom.Type == RoomType.Subregion)
			{
				if (currentRoom.Links.TryFind(l => l.TargetRoom == visitedrooms.Last(), out var locationLink))
				{
					validpaths.Add((locationLink.Location, new(visitedrooms)));
				}
			}
			else
			{
				foreach (var link in currentRoom.Links.Where(l => !visitedrooms.Contains(l.TargetRoom)))
				{
					ProcessCompanionSpoiler(link.TargetRoom, validpaths, visitedrooms.Append(roomid).ToList());
				}
			}
		}
		public List<AccessReqs> CrawlForRequirements(LocationIds location)
		{

			var regionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();
			var initialRoom = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList().Find(l => l.Location == location).TargetRoom;
			///var initialRoom = Rooms.Where(r => r.Type == RoomType.Location).ToList().Find(r => r.Id == ((int)location + 240)).Links.Find(l => l.Entrance >= 0).TargetRoom;

			List<AccessReqs> accessList = Rooms.Find(x => x.Id == initialRoom).Links.SelectMany(x => x.Access).ToList();
			List<int> visitedRooms = regionRooms;

			ProcessRoomForRequirements(0, initialRoom, accessList, visitedRooms);

			return accessList;
		}

		public void ProcessRoomForRequirements(int reqcount, int roomid, List<AccessReqs> accesslist, List<int> visitedrooms)
		{
			var currentRoom = Rooms.Find(x => x.Id == roomid);

			visitedrooms.Add(roomid);

			foreach (var link in currentRoom.Links.Where(l => !visitedrooms.Contains(l.TargetRoom) && !l.Access.Intersect(AccessReferences.CrestsAccess).Any()))
			{
				accesslist.AddRange(link.Access);
				ProcessRoomForRequirements(reqcount + link.Access.Count, link.TargetRoom, accesslist, visitedrooms);
			}
		}
		
		public List<SpoilerRoom> CrawlForSpoilers(LocationIds location)
		{
            var regionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();
            var initialRoom = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList().Find(l => l.Location == location).TargetRoom;

			//List<AccessReqs> accessList = Rooms.Find(x => x.Id == initialRoom).Links.SelectMany(x => x.Access).ToList();
			List<int> visitedRooms = new() { initialRoom };

            var initialspoilerroom = new SpoilerRoom()
			{
				RoomId = initialRoom,
				ParentId = 0,
				Access = new(),
				Depth = 0,
				DewyNumber = new() { 0 },
				Description = Rooms.Find(x => x.Id == initialRoom).Name
			};

			List<SpoilerRoom> spoilerRooms = new();
			spoilerRooms.Add(initialspoilerroom);

			ProcessRoomForSpoilers(0, initialspoilerroom, spoilerRooms, visitedRooms);

			return spoilerRooms;
		}

		
		public void ProcessRoomForSpoilers(int depth, SpoilerRoom parent, List<SpoilerRoom> spoilerrooms, List<int> visitedrooms)
		{
			var currentRoom = Rooms.Find(x => x.Id == parent.RoomId);
			int dewynumber = 0;
			List<SpoilerRoom> roomsToVisit = new();

			foreach (var link in currentRoom.Links.Where(l => l.TargetRoom != parent.ParentId))
			{
                var targetroom = Rooms.Find(x => x.Id == link.TargetRoom);
				SpoilerRoom currentspoilerroom;
				if (targetroom.Type != RoomType.Dungeon)
				{
					continue;
				}

				if (visitedrooms.Contains(link.TargetRoom))
				{
					currentspoilerroom = new SpoilerRoom()
					{
						RoomId = link.TargetRoom,
						ParentId = parent.RoomId,
						Access = link.Access,
						Depth = depth + 1,
						DewyNumber = parent.DewyNumber.Append(dewynumber++).ToList(),
						Description = targetroom.Name
                    };

                    spoilerrooms.Add(currentspoilerroom);
                }
				else
				{
                    currentspoilerroom = new SpoilerRoom()
                    {
                        RoomId = link.TargetRoom,
                        ParentId = parent.RoomId,
                        Access = link.Access,
                        Depth = depth + 1,
                        DewyNumber = parent.DewyNumber.Append(dewynumber++).ToList(),
                        Description = targetroom.Name
                    };

					visitedrooms.Add(targetroom.Id);

                    if (link.Access.Intersect(AccessReferences.CrestsAccess).Any() && (targetroom.Location != currentRoom.Location))
                    {
                        currentspoilerroom.Description = "To " + targetroom.Location.ToString();
                        spoilerrooms.Add(currentspoilerroom);
                    }
                    else
                    {
                        roomsToVisit.Add(currentspoilerroom);
                        spoilerrooms.Add(currentspoilerroom);
                    }
                }
            }

			foreach (var room in roomsToVisit)
			{
                ProcessRoomForSpoilers(depth + 1, room, spoilerrooms, visitedrooms);
            }
		}
	}

	public class SpoilerRoom
	{
		public int RoomId { get; set; }
		public int ParentId { get; set; }
		public List<int> DewyNumber { get; set; }
		public List<AccessReqs> Access { get; set; }
		public int Depth { get; set; }
		public string Description { get; set; }
		public double ComputedDewy => ComputeDewy();

		private double ComputeDewy()
		{
			double dewy = 0;
			for (int i = 0; i < 9; i++)
			{
				if (i >= DewyNumber.Count)
				{
					break;
				}
				dewy += DewyNumber[i] * Math.Pow(10, (9 - i));
			}

			return dewy;
		}
	}
}
