using RomUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using System.Reflection.Metadata.Ecma335;

namespace FFMQLib
{
	public class LogicLink
	{
		public bool Exit { get; set; }
		public bool PriorityExit { get; set; }
		public int Room { get; set; }
		public List<int> ForbiddenDestinations { get; set; }
		public RoomLink Current { get; set; }
		public RoomLink Origin { get; set; }

		public LogicLink(int room, RoomLink link, RoomLink origin)
		{
			Room = room;
			Current = link;
			Origin = origin;
			ForbiddenDestinations = new();
			Exit = true;
			PriorityExit = false;
		}
		public void UpdateCurrent(RoomLink link)
		{ 
			Current.Entrance = link.Entrance;
			Current.TargetRoom = link.TargetRoom;
			Current.Teleporter = link.Teleporter;
		}
	}
	public class ClusterLocation
	{
		public List<ClusterRoom> Rooms { get; set; }
		public LocationIds Location { get; set; }
		public List<LogicLink> Links => Rooms.SelectMany(r => r.Links).ToList();
		public int Id { get; set; }
		//public int InitialRoom { get; set; }
		public List<int> InitialRooms { get; set; }
		public bool OddLinks => Rooms.Where(r => r.Links.Count % 2 == 1).Any();
		public bool DeadEndRequired => Rooms.Where(r => r.Links.Where(l => !l.Exit).Any()).Any();
		private List<ClusterRoom> BackupRooms;
		public ClusterLocation()
		{
			Rooms = new();
			BackupRooms = new();
			Id = 0;
			InitialRooms = new();
			Location = LocationIds.None;
		}
		public ClusterLocation(ClusterRoom initialRoom)
		{
			initialRoom = new ClusterRoom(initialRoom);

			Rooms = new() { initialRoom };
			BackupRooms = new();
			Id = 0;
			Location = initialRoom.Location;
			InitialRooms = initialRoom.Rooms.ToList();
		}
		public void BackUpState()
		{
			BackupRooms = Rooms.Select(r => new ClusterRoom(r)).ToList();
		}
		public void RestoreBackup()
		{
			Rooms = BackupRooms.Select(r => new ClusterRoom(r)).ToList();
		}
		public List<int> ForbiddenDestinations(LogicLink link)
		{
			if (!Rooms.TryFind(r => r.Links.Contains(link), out var originRoom))
			{
				throw new Exception("Couldn't find appropriate room.");
			}

			return originRoom.ForbiddenDestinations.Concat(link.ForbiddenDestinations).ToList();
		}
		public void Merge(ClusterRoom targetRoom, LogicLink originLink, LogicLink targetLink)
		{
			ClusterRoom originRoom;

			if (!Rooms.TryFind(r => r.Links.Contains(originLink), out originRoom))
			{
				throw new Exception("Couldn't find appropriate room.");
			}

			targetRoom = new ClusterRoom(targetRoom);
			
			if (!originLink.Exit)
			{
				targetRoom.ForbiddenDestinations.AddRange(ForbiddenDestinations(originLink));
				Rooms.Add(targetRoom);
				targetRoom.Links.Remove(targetLink);
				originRoom.Links.Remove(originLink);
			}
			else
			{
				originRoom.Merge(targetRoom);
				originRoom.Links.Remove(targetLink);
				originRoom.Links.Remove(originLink);
			}
		}
	}
	public class ClusterRoom
	{ 
		public List<int> Rooms { get; set; }
		public List<LogicLink> Links { get; set; }
		public LocationIds Location { get; set; }
		public int Id { get; set; }
		public List<int> ForbiddenDestinations { get; set; }

		public ClusterRoom(List<int> rooms)
		{
			Rooms = rooms;
			Links = new();
			Id = 0;
			Location = LocationIds.None;
			ForbiddenDestinations = new();
		}
		public ClusterRoom(int id, List<int> rooms, List<LogicLink> links, LocationIds loc)
		{
			Rooms = rooms;
			Links = links.ToList();
			Id = id;
			Location = loc;
			ForbiddenDestinations = new();
		}
		public ClusterRoom(ClusterRoom copyroom)
		{
			Rooms = copyroom.Rooms.ToList();
			Links = copyroom.Links.ToList();
			Id = copyroom.Id;
			Location = copyroom.Location;
			ForbiddenDestinations = copyroom.ForbiddenDestinations.ToList();
		}
		public void Merge(ClusterRoom room)
		{
			Rooms = Rooms.Concat(room.Rooms).ToList();
			Links = Links.Concat(room.Links).ToList();
			ForbiddenDestinations = ForbiddenDestinations.Concat(room.ForbiddenDestinations).ToList();
		}
		public void Merge(ClusterRoom targetRoom, LogicLink originLink, LogicLink targetLink, MT19337 rng)
		{
			Links.Remove(originLink);
			targetRoom.Links.Remove(targetLink);
			if (targetRoom.Id != Id)
			{
				Links = Links.Concat(targetRoom.Links).ToList();
				Rooms = Rooms.Concat(targetRoom.Rooms).ToList();
				ForbiddenDestinations = ForbiddenDestinations.Concat(targetRoom.ForbiddenDestinations).ToList();
			}
		}
	}
	public class ShufflingData
	{ 
		public List<int> TownsTemples { get; set; }
		public List<int> PriorityExits { get; set; }
		public List<int> NoExits { get; set; }
		public List<List<int>> ForcedLinks { get; set; }
		public List<List<int>> BlockedOneways { get; set; }
		public List<List<int>> AddedLinks { get; set; }
		public List<int> MacShipExclusions { get; set; }

		public ShufflingData()
		{
			TownsTemples = new();
			MacShipExclusions = new();
			ForcedLinks = new();
			PriorityExits = new();
			NoExits = new();
			BlockedOneways = new();
			AddedLinks = new();
		}
		public void ReadData(MT19337 rng)
		{
			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("shufflingdata.yaml"));
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

			ShufflingData result = new();

			try
			{
				result = deserializer.Deserialize<ShufflingData>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Debug.WriteLine(ex.ToString());
			}

			ForcedLinks = result.ForcedLinks;
			TownsTemples = result.TownsTemples;
			MacShipExclusions = result.MacShipExclusions;
			PriorityExits = result.PriorityExits;
			NoExits = result.NoExits;
			BlockedOneways = result.BlockedOneways;
			AddedLinks = result.AddedLinks;
		}
	}
	public partial class GameLogic
	{
		private List<List<int>> entrancesPairs;
		private List<(int roomid, RoomLink link)> newLinkToProcess;
		private List<(LogicLink origin, LogicLink target)> forcedLinks;

		public void FloorShuffle(MapShufflingMode mapshuffling, bool apenabled, MT19337 rng)
		{

			// Set parameters
			bool shuffleFloors = mapshuffling != MapShufflingMode.None;
			bool includeTemplesTowns = mapshuffling == MapShufflingMode.Everything;
			bool intradungeon = mapshuffling == MapShufflingMode.DungeonsInternal;

			if (!shuffleFloors || apenabled)
			{
				return;
			}

			// Init globals
			newLinkToProcess = new();

			// Read shuffling data
			ReadPairs();
			ShufflingData shufflingData = new();
			shufflingData.ReadData(rng);

			// Clear one ways and add new links
			foreach (var oneway in shufflingData.BlockedOneways)
			{
				Rooms.Find(r => r.Id == oneway[0]).Links.RemoveAll(l => l.TargetRoom == oneway[1]);
			}

			foreach (var newlink in shufflingData.AddedLinks)
			{
				Rooms.Find(r => r.Id == newlink[0]).Links.Add(new RoomLink(newlink[1], new() { (AccessReqs)newlink[2] }));
			}

			// Get links that can be shuffled
			var roomLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var logicLinks = entrancesPairs.Select(e => new LogicLink(roomLinks.Find(l => l.l.Entrance == e[0]).Id, roomLinks.Find(l => l.l.Entrance == e[0]).l, roomLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			logicLinks.AddRange(entrancesPairs.Select(e => new LogicLink(roomLinks.Find(l => l.l.Entrance == e[1]).Id, roomLinks.Find(l => l.l.Entrance == e[1]).l, roomLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());

			var seedLinksLocations = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (l.Entrance, l.Location))).ToList();

			// Find rooms that have requirements in other rooms to populate forbidden destinations
			List<int> doomCastleRooms = new() { 195, 196, 197, 198, 199, 200, 201 };
			var roomTriggers = Rooms.Where(r => !doomCastleRooms.Contains(r.Id)).SelectMany(r => r.GameObjects.Where(o => o.Type == GameObjectType.Trigger).Select(o => (r.Id, o.OnTrigger)).ToList()).ToList();
			var roomsReq = Rooms.Where(r => !doomCastleRooms.Contains(r.Id)).SelectMany(r => r.Links.Where(l => l.Access.Any()).Select(l => (r.Id, l)).ToList()).ToList();
			
			List<(int entrance, int room)> forbiddenDestinations = new();

			foreach (var trigger in roomTriggers)
			{
				var affectedRooms = roomsReq.Where(x => x.l.Access.Intersect(trigger.OnTrigger).Any() && x.Id != trigger.Id && x.l.Entrance != -1).ToList();
				forbiddenDestinations.AddRange(affectedRooms.Select(x => (x.l.Entrance, trigger.Id)).ToList());
			}
			
			// Check if temples and towns need to be added
			if (!includeTemplesTowns)
			{
				logicLinks = logicLinks.Where(x => !shufflingData.TownsTemples.Contains(x.Current.Entrance)).ToList();
			}

			// Update Logic Links with the shuffling data
			forbiddenDestinations.ForEach(x => logicLinks.Find(l => l.Current.Entrance == x.entrance).ForbiddenDestinations = new() { x.room });

			logicLinks.ForEach(x => x.PriorityExit = shufflingData.PriorityExits.Contains(x.Current.Entrance));
			logicLinks.ForEach(x => x.Exit = !shufflingData.NoExits.Contains(x.Current.Entrance));

			// Create the initial list of cluster rooms which will be the basis for how everything is connected logically
			List<ClusterRoom> clusterRooms = new();

			int maxId = 0;
			int roomId = 0;

			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0).Select(x => x.TargetRoom).Append(room.Id).ToList();

				foreach (var link in internalLinks)
				{
					maxId = (link > maxId) ? link : maxId;
				}

				clusterRooms.Add(new ClusterRoom(roomId++, internalLinks, logicLinks.Where(l => l.Room == room.Id).ToList(), LocationIds.None));
			}

			for (int i = 0; i <= maxId; i++)
			{ 
				var commonClusterRooms = clusterRooms.Where(x => x.Rooms.Contains(i)).ToList();

				if (commonClusterRooms.Count > 1)
				{
					var bigRoomsToMerge = commonClusterRooms.GetRange(1, commonClusterRooms.Count - 1);
					foreach (var bigroom in bigRoomsToMerge)
					{
						clusterRooms.Remove(bigroom);
						commonClusterRooms[0].Merge(bigroom);
					}
				}
			}

			// Crawl rooms to get their locations
			List<int> visitedRooms = new() { 0 };
			List<RoomLink> locationLinks = new();

			foreach (var regionRoom in Rooms.Where(r => r.Type == RoomType.Subregion))
			{
				locationLinks.AddRange(regionRoom.Links.Where(l => l.Location != LocationIds.None && l.Location != LocationIds.GiantTree && l.Location != LocationIds.MacsShipDoom).ToList());
			}

			foreach (var locationLink in locationLinks)
			{
				ProcessClusterRoom(locationLink.TargetRoom, clusterRooms, visitedRooms, locationLink.Location);
			}

			// Connect Forced Link
			forcedLinks = new();

			foreach (var link in shufflingData.ForcedLinks)
			{
				var originLink = logicLinks.Find(l => l.Current.Entrance == link[0]);
				var targetLink = logicLinks.Find(l => l.Current.Entrance == link[1]);
				var originRoom = clusterRooms.Find(r => r.Rooms.Contains(originLink.Room));
				var targetRoom = clusterRooms.Find(r => r.Rooms.Contains(targetLink.Room));

				ConnectLink(originLink, targetLink);
				if (originRoom != targetRoom)
				{
					clusterRooms.Remove(targetRoom);
				}
				originRoom.Merge(targetRoom, originLink, targetLink, rng);
				forcedLinks.Add((originLink, targetLink));
			}

			// Special restrictions
			var crestRooms = Rooms.Where(x => x.Links.Where(l => l.Access.Intersect(AccessReferences.CrestsAccess).Any()).Any()).Select(x => x.Id).ToList();
			var macShipBarredRooms = crestRooms.Concat(shufflingData.MacShipExclusions).Concat(forbiddenDestinations.Select(x => x.room)).ToList();
			int macShipDeck = 187;
			int macShipMaxSize = 4;

			List<(LocationIds location, int targetroom, int baseroom)> crystalRooms = new() { (LocationIds.BoneDungeon, 38, 25), (LocationIds.IcePyramid, 70, 54), (LocationIds.LavaDome, 121, 100), (LocationIds.PazuzusTower, 179, 166) };

			// Shuffle our core locations
			var seedRooms = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).Where(l => l.Entrance >= 0).Select(l => l.TargetRoom).Except(new List<int> { 125 }).ToList();
			var subRegionRooms = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();
			var seedClusterRooms = clusterRooms.Where(x => x.Rooms.Intersect(seedRooms).Any()).ToList();
			var seedClusterRoomsToShuffle = seedClusterRooms.Where(x => x.Links.Where(l => subRegionRooms.Contains(l.Current.TargetRoom)).Any()).ToList();
			var seedClusterRoomsFixed = seedClusterRooms.Except(seedClusterRoomsToShuffle).ToList();
			var seedClusterRoomsToShuffleProgress = seedClusterRoomsToShuffle.Where(x => x.Links.Count > 1).ToList();
			var seedClusterRoomsToShuffleDeadends = seedClusterRoomsToShuffle.Where(x => x.Links.Count == 1).ToList();

			var initialProgressClusterRooms = clusterRooms.Where(x => x.Links.Count > 1 && !x.Rooms.Intersect(seedRooms).Any() && !x.Rooms.Contains(0)).Concat(seedClusterRoomsToShuffleProgress).ToList();
			var initialDeadendClusterRooms = clusterRooms.Where(x => x.Links.Count == 1 && !x.Rooms.Intersect(seedRooms).Any() && !x.Rooms.Contains(0)).Concat(seedClusterRoomsToShuffleDeadends).ToList();

			initialProgressClusterRooms.Shuffle(rng);
			initialDeadendClusterRooms.Shuffle(rng);

			// Select seed rooms
			List<ClusterRoom> coreClusterRooms = new();

			if (intradungeon)
			{
				foreach (var progressRoom in seedClusterRoomsToShuffleProgress)
				{
					// new room is a room from the same location and that doesn't block the whole dungeon
					var validRooms = initialProgressClusterRooms.Where(r => r.Location == progressRoom.Location && (r.Links.Where(l => !l.ForbiddenDestinations.Any()).Count() > 1)).ToList();
					var newLocation = rng.PickFrom(validRooms);
					coreClusterRooms.Add(newLocation);
				}

				coreClusterRooms.AddRange(seedClusterRoomsToShuffleDeadends);
			}
			else
			{
				coreClusterRooms.AddRange(initialProgressClusterRooms.Where(r => !r.Rooms.Intersect(crystalRooms.Select(c => c.targetroom)).Any()).ToList().GetRange(0, seedClusterRoomsToShuffleProgress.Count));
				coreClusterRooms.AddRange(initialDeadendClusterRooms.Where(r => !r.Rooms.Intersect(crystalRooms.Select(c => c.targetroom)).Any()).ToList().GetRange(0, seedClusterRoomsToShuffleDeadends.Count));
			}

			var validSeecClusterRoomsToSwitch = coreClusterRooms.Except(seedClusterRoomsToShuffle).ToList();
			var validSeedClusterRoomsFixed = coreClusterRooms.Intersect(seedClusterRoomsToShuffle).ToList();

			// Connnect to overworld rooms that were originally connected to overworld
			foreach (var location in validSeedClusterRoomsFixed)
			{
				LogicLink linkToOverworld = location.Links.Find(x => subRegionRooms.Contains(x.Current.TargetRoom));
				LogicLink linkFromOverworld = clusterRooms
					.Where(r => r.Rooms.Intersect(subRegionRooms).Any())
					.ToList()
					.SelectMany(r => r.Links)
					.ToList()
					.Find(l => l.Origin.Entrance == linkToOverworld.Current.Entrance);

				location.Links.Remove(linkToOverworld);
				clusterRooms.Find(r => r.Links.Contains(linkFromOverworld)).Links.Remove(linkFromOverworld);

				ConnectOverworldLink(location.Location, linkFromOverworld, linkToOverworld);
			}

			var validCrystalSource = validSeecClusterRoomsToSwitch.Where(r => r.Links.Count > 1).ToList();

			// Force connect to progress room for each crystal room location
			for(int i = 0; i < crystalRooms.Count; i++)
			{
				List<LogicLink> linksFromOverworld = clusterRooms
					.Where(r => r.Rooms.Intersect(subRegionRooms).Any())
					.ToList()
					.SelectMany(r => r.Links)
					.ToList();

				LogicLink linkFromOverworld;

				// if we don't find a location, then it was a vanilla base room which is fine, we don't need to do anything
				if (linksFromOverworld.TryFind(l => l.Current.Location == crystalRooms[i].location, out linkFromOverworld))
				{
					ClusterRoom progressRoom;
					
					if (!validCrystalSource.TryFind(r => r.Location == crystalRooms[i].location, out progressRoom))
					{
						progressRoom = rng.PickFrom(validCrystalSource);
					}

					var validLinks = progressRoom.Links.Where(x => x.Exit).ToList();
					LogicLink linkToOverworld = validLinks.TryFind(l => l.PriorityExit, out var priorityLink) ? priorityLink : rng.PickFrom(validLinks);

					// Update reference
					crystalRooms[i] = (crystalRooms[i].location, crystalRooms[i].targetroom, progressRoom.Rooms.First());

					progressRoom.Links.Remove(linkToOverworld);
					clusterRooms.Find(r => r.Links.Contains(linkFromOverworld)).Links.Remove(linkFromOverworld);

					ConnectOverworldLink(seedLinksLocations.Find(x => x.Entrance == linkFromOverworld.Current.Entrance).Location, linkFromOverworld, linkToOverworld);
					validCrystalSource.Remove(progressRoom);
					validSeecClusterRoomsToSwitch.Remove(progressRoom);
				}
			}

			// Now connect the rest
			foreach (var location in validSeecClusterRoomsToSwitch)
			{
				var validLinks = location.Links.Where(x => x.Exit).ToList();
				LogicLink linkToOverworld = validLinks.TryFind(l => l.PriorityExit, out var priorityLink) ? priorityLink : rng.PickFrom(validLinks);
				LogicLink linkFromOverworld;
				List<LogicLink> linksFromOverworld = clusterRooms
					.Where(r => r.Rooms.Intersect(subRegionRooms).Any())
					.ToList()
					.SelectMany(r => r.Links)
					.ToList();

				if (!linksFromOverworld.TryFind(l => l.Current.Location == location.Location, out linkFromOverworld))
				{
					linkFromOverworld = rng.PickFrom(linksFromOverworld);
				}

				location.Links.Remove(linkToOverworld);
				clusterRooms.Find(r => r.Links.Contains(linkFromOverworld)).Links.Remove(linkFromOverworld);

				ConnectOverworldLink(seedLinksLocations.Find(x => x.Entrance == linkFromOverworld.Current.Entrance).Location, linkFromOverworld, linkToOverworld);
			}

			coreClusterRooms.AddRange(seedClusterRoomsFixed);

			// Prep to assign floors
			var coreClusterRoomsIds = coreClusterRooms.SelectMany(r => r.Rooms).ToList().Append(0).ToList();
			var progressClusterRooms = clusterRooms.Where(x => x.Links.Count > 1 && !x.Rooms.Intersect(coreClusterRoomsIds).Any()).ToList();
			var deadendClusterRooms = clusterRooms.Where(x => x.Links.Count == 1 && !x.Rooms.Intersect(coreClusterRoomsIds).Any()).ToList();

			coreClusterRooms.Shuffle(rng);
			coreClusterRooms = coreClusterRooms.Where(x => x.Links.Count > 0).ToList();

			// Build the maps
			if (intradungeon)
			{
				foreach (var originRoom in coreClusterRooms)
				{
					List<ClusterRoom> locationProgressClusters = progressClusterRooms.Where(x =>
						(x.Location == originRoom.Location)
						).ToList();

					List<ClusterRoom> deadendClusters = deadendClusterRooms.Where(x =>
						(x.Location == originRoom.Location)
						).ToList();

					List<(LogicLink origin, LogicLink target)> newLinkPairs = new();
					bool validconfig = false;
					
					int loopcount = 0;

					while (!validconfig)
					{
						loopcount++;
						newLinkPairs = new();
						var originCluster = new ClusterLocation(originRoom);
						var tempAllLinks = originCluster.Links.ToList();
						validconfig = true;

						locationProgressClusters.Shuffle(rng);

						// Progress
						foreach (var progressCluster in locationProgressClusters)
						{

							var originLinks = originCluster.Links.ToList();
							var originLink = rng.PickFrom(originLinks);

							if (progressCluster.Rooms.Intersect(originCluster.ForbiddenDestinations(originLink)).Any())
							{
								validconfig = false;
								break;
							}

							var targetLink = progressCluster.Links.TryFind(l => l.PriorityExit, out var priorityLink) ? priorityLink : rng.PickFrom(progressCluster.Links.Where(l => l.Exit).ToList());

							originCluster.Merge(progressCluster, originLink, targetLink);
							newLinkPairs.Add((originLink, targetLink));
						}

						if (!validconfig)
						{
							continue;
						}

						deadendClusters.Shuffle(rng);

						// Connect Deadend
						foreach (var deadend in deadendClusters)
						{
							var availableLocations = originCluster.Rooms.Where(r => r.Links.Any()).ToList();
							var oddLinksLocations = originCluster.Rooms.Where(r => (r.Links.Count % 2) == 1).ToList();
							var noExitLocations = originCluster.Rooms.Where(r => r.Links.Where(l => !l.Exit).Any()).ToList();
							bool noExit = false;

							if (noExitLocations.Any())
							{
								availableLocations = noExitLocations;
								noExit = true;
							}
							else if (oddLinksLocations.Any())
							{
								availableLocations = oddLinksLocations;
							}

							var originLocation = rng.PickFrom(availableLocations);
							var originLinks = noExit ? originLocation.Links.Where(l => !l.Exit).ToList() : originLocation.Links;

							var originLink = rng.PickFrom(originLinks);

							if (deadend.Rooms.Intersect(originCluster.ForbiddenDestinations(originLink)).Any())
							{
								validconfig = false;
								break;
							}

							var destinationLink = rng.PickFrom(deadend.Links);
							originCluster.Merge(deadend, originLink, destinationLink);
							newLinkPairs.Add((originLink, destinationLink));
						}

						// Tie loose ends
						if (!validconfig || originCluster.Rooms.Where(r => (r.Links.Count % 2) == 1).Any() || originCluster.Rooms.Where(r => r.Links.Where(l => !l.Exit).Any()).Any())
						{
							validconfig = false;
							continue;
						}

						foreach (var room in originCluster.Rooms)
						{
							while (room.Links.Any())
							{
								newLinkPairs.Add((rng.TakeFrom(room.Links), rng.TakeFrom(room.Links)));
							}
						}

						var allroomids = originCluster.Rooms.SelectMany(r => r.Rooms).ToList();
						List<AccessReqs> triggers = Rooms.Where(r => allroomids.Contains(r.Id)).SelectMany(r => r.GameObjects.Where(o => o.Type == GameObjectType.Trigger).SelectMany(o => o.OnTrigger)).ToList();
						//validconfig = ValidRoomCrawl(newLinkPairs, allroomids, originCluster.InitialRooms, triggers);
					}

					foreach (var linkpair in newLinkPairs)
					{
						ConnectLink(linkpair.origin, linkpair.target);
					}
				}
			}
			else
			{
				// Create origin locations
				var originLocations = coreClusterRooms.Select(r => new ClusterLocation(r)).ToList();

				// Process Mac Ship
				int macShipMergingCount = 0;
				ClusterLocation macShip = originLocations.Find(l => l.Rooms.SelectMany(r => r.Rooms).ToList().Contains(macShipDeck));
				macShip.Rooms.ForEach(r => r.ForbiddenDestinations.AddRange(macShipBarredRooms));

				// Remove crystal Room
				bool skyCrystalRoomPlaced = false;
				if (progressClusterRooms.TryFind(r => r.Rooms.Contains(crystalRooms[3].targetroom), out var skyCrystalRoom))
				{
					progressClusterRooms.Remove(skyCrystalRoom);
				}
				else
				{
					skyCrystalRoomPlaced = true;
				}

				// Place Progress Rooms
				while (progressClusterRooms.Any())
				{
					var validOrigins = (macShipMergingCount >= (macShipMaxSize - 2)) ? originLocations.Where(l => l != macShip).ToList() : originLocations;
					var originRoom = rng.PickFrom(validOrigins);
					var originLinks = originRoom.Links;
					var originLink = rng.PickFrom(originLinks);
					
					List<ClusterRoom> destinationRooms = progressClusterRooms.Where(x =>
						!x.Rooms.Intersect(originRoom.ForbiddenDestinations(originLink)).Any()
					).ToList();

					if (!destinationRooms.Any())
					{
						continue;
					}

					var destinationRoom = rng.PickFrom(destinationRooms);
					progressClusterRooms.Remove(destinationRoom);

					var destinationLink = destinationRoom.Links.TryFind(l => l.PriorityExit, out var priorityLink) ? priorityLink : rng.PickFrom(destinationRoom.Links.Where(l => l.Exit).ToList());

					ConnectLink(originLink, destinationLink);
					originRoom.Merge(destinationRoom, originLink, destinationLink);
					if (originRoom == macShip) macShipMergingCount++;
				}

				// Place Sky Crystal Room
				if (!skyCrystalRoomPlaced)
				{
					var skyLocation = originLocations.Find(o => o.Rooms.Where(r => r.Rooms.Contains(crystalRooms[3].baseroom)).Any());
					var destinationLink = rng.PickFrom(skyCrystalRoom.Links);
					var originLink = rng.PickFrom(skyLocation.Links);

					ConnectLink(originLink, destinationLink);
					skyLocation.Merge(skyCrystalRoom, originLink, destinationLink);
				}

				// Place the other crystal rooms
				var crystalClusters = deadendClusterRooms.Where(r => r.Rooms.Intersect(crystalRooms.Select(c => c.targetroom).ToList()).Any()).ToList();
				deadendClusterRooms = deadendClusterRooms.Except(crystalClusters).ToList();

				foreach (var crystalCluster in crystalClusters)
				{
					var crystalRoom = crystalRooms.Find(c => crystalCluster.Rooms.Contains(c.targetroom));
					var originRoom = originLocations.Find(o => o.Rooms.Where(r => r.Rooms.Contains(crystalRoom.baseroom)).Any());

					var originLink = rng.PickFrom(originRoom.Links);

					var destinationLink = rng.PickFrom(crystalCluster.Links);

					ConnectLink(originLink, destinationLink);
					originRoom.Merge(crystalCluster, originLink, destinationLink);
				}

				// Place Crest tile deadends first (because they're more restricted)
				var crestClusters = deadendClusterRooms.Where(r => r.Rooms.Intersect(crestRooms).Any()).ToList();
				deadendClusterRooms = deadendClusterRooms.Except(crestClusters).ToList();

				while(crestClusters.Any())
				{
					var validOrigins = originLocations.Where(r => r.Rooms.First().Links.Where(l => l.Exit).Any() && r != macShip).ToList();

					var originRoom = rng.PickFrom(validOrigins);
					var originLinks = originRoom.Rooms.First().Links.Where(l => l.Exit).ToList();
					var originLink = rng.PickFrom(originLinks);

					List<ClusterRoom> destinationRooms = crestClusters.Where(x =>
						!x.Rooms.Intersect(originRoom.ForbiddenDestinations(originLink)).Any()
						).ToList();

					if (!destinationRooms.Any())
					{
						continue;
					}

					var destinationRoom = rng.PickFrom(destinationRooms);
					crestClusters.Remove(destinationRoom);

					var destinationLink = rng.PickFrom(destinationRoom.Links);

					ConnectLink(originLink, destinationLink);
					originRoom.Merge(destinationRoom, originLink, destinationLink);
				}

				// Placed dead ends
				List<(LogicLink origin, LogicLink target)> deadEndLinkPairs = new();
				bool validDeadends = false;

				originLocations.ForEach(l => l.BackUpState());

				while (!validDeadends)
				{
					// We create a state backup to reset if we hit some softlocked scenarios
					originLocations.ForEach(l => l.RestoreBackup());
					var deadendRoomsToProcess = deadendClusterRooms.ToList();
					deadEndLinkPairs = new();

					int deadendInsanity = 0;
					bool abortRun = false;

					while (deadendRoomsToProcess.Any())
					{
						bool removeMacShip = (macShipMergingCount >= (macShipMaxSize)) && !macShip.OddLinks && !macShip.DeadEndRequired;

						var availableLocations = originLocations.Where(l => l.Rooms.Where(r => r.Links.Any()).ToList().Any() &&
							(removeMacShip ? l != macShip : true)).ToList();
						var oddLinksLocations = availableLocations.Where(l => l.OddLinks).ToList();
						var noExitLocations = availableLocations.Where(l => l.DeadEndRequired).ToList();
						bool noExit = false;
						bool oddLinks = false;

						if (noExitLocations.Any())
						{
							availableLocations = noExitLocations;
							noExit = true;
						}
						else if (oddLinksLocations.Any())
						{
							availableLocations = oddLinksLocations;
							oddLinks = true;
						}

						var originRoom = rng.PickFrom(availableLocations);
						var originLinks = originRoom.Links;
						if (noExit)
						{
							originLinks = originRoom.Links.Where(l => !l.Exit).ToList();
						}
						else if (oddLinks)
						{
							originLinks = originRoom.Rooms.Where(r => r.Links.Count % 2 == 1).SelectMany(r => r.Links).ToList();
						}

						var originLink = rng.PickFrom(originLinks);

						List<ClusterRoom> destinationRooms = deadendRoomsToProcess.Where(x =>
							!x.Rooms.Intersect(originRoom.ForbiddenDestinations(originLink)).Any()
							).ToList();

						List<ClusterRoom> machsipForbiddenRooms = destinationRooms.Where(x =>
							!x.Rooms.Intersect(macShipBarredRooms).Any()
							).ToList();

						destinationRooms = machsipForbiddenRooms.Any() ? machsipForbiddenRooms : destinationRooms;

						if (!destinationRooms.Any())
						{
							deadendInsanity++;
							if (deadendInsanity > 20)
							{
								abortRun = true;
								// There's a remote situation where mac ship is the only location left and there's still dead ends to place, this is it
								//throw new Exception("Map Shuffling Loop Error, try another seed.");
							}
							else
							{
								continue;
							}
						}

						if (abortRun)
						{
							break;
						}

						var destinationRoom = rng.PickFrom(destinationRooms);
						deadendRoomsToProcess.Remove(destinationRoom);

						var destinationLink = rng.PickFrom(destinationRoom.Links);
						deadEndLinkPairs.Add((originLink, destinationLink));

						originRoom.Merge(destinationRoom, originLink, destinationLink);
						if (originRoom == macShip) macShipMergingCount++;
					}

					if (!deadendRoomsToProcess.Any())
					{
						validDeadends = true;
					}
				}

				foreach (var pair in deadEndLinkPairs)
				{
					ConnectLink(pair.origin, pair.target);
				}

				// Check loose ends
				var orphanedRooms = originLocations.SelectMany(o => o.Rooms.Where(r => r.Links.Count % 2 == 1)).ToList();

				if (orphanedRooms.Count == 1)
				{
					// Add Dummy room as last ditch effort to salvage the seed
					var originLink = new RoomLink(500, 0, (141, 1), new List<AccessReqs>());
					var destinationLink = new RoomLink(0, 481, (0, 10), new List<AccessReqs>());

					var dummyRoom = new ClusterRoom(500,
						new List<int>() { 500 },
						new List<LogicLink>() { new LogicLink(500, destinationLink, originLink) },
						LocationIds.None);

					var orphanedRoom = orphanedRooms.First();
					var oprhanedLocation = originLocations.Find(o => o.Rooms.Contains(orphanedRoom));
					var orphanedLink = rng.PickFrom(orphanedRoom.Links);
					var dummyRoomLink = dummyRoom.Links.First();

					ConnectLink(orphanedLink, dummyRoomLink);
					oprhanedLocation.Merge(dummyRoom, orphanedLink, dummyRoomLink);

					Rooms.Add(new Room("Dummy Room", 500, 0x11, new List<GameObjectData>() { }, new List<RoomLink> { }));
				}
				else if (orphanedRooms.Count > 1)
				{
					// We're cooked
					throw new Exception("There's invalid loops left");
				}

				// Tie Loose ends
				foreach (var location in originLocations)
				{
					foreach (var room in location.Rooms)
					{
						while (room.Links.Any())
						{
							if ((room.Links.Count % 2) == 1)
							{
								throw new Exception("Floor Shuffle: Gap Connection Error\n" + GenerateDumpFile());
							}

							var firstLink = rng.TakeFrom(room.Links);
							var secondLink = rng.TakeFrom(room.Links);

							ConnectLink(firstLink, secondLink);
						}
					}
				}
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
		}
		private bool ValidRoomCrawl(List<(LogicLink origin, LogicLink target)> linkpairs, List<int> roomsToReach, List<int> initialrooms, List<AccessReqs> triggers)
		{
			List<int> visitedRoom = new();
			List<AccessReqs> accessFound = triggers;
			List<int> roomQueue = new();
			List<(List<AccessReqs> access, int room)> blockedRooms = new();

			roomQueue.AddRange(initialrooms);

			while (roomQueue.Any())
			{
				var currentroomid = roomQueue.First();
				roomQueue.RemoveAt(0);
				
				var currentroom = Rooms.Find(r => r.Id == currentroomid);
				
				// Process triggers
				var triggersFound = currentroom.GameObjects.Where(o => o.Type == GameObjectType.Trigger && !o.Access.Intersect(accessFound).Any()).SelectMany(o => o.OnTrigger).ToList();
				accessFound = accessFound.Except(triggersFound).ToList();
				
				blockedRooms.ForEach(r => r.access = r.access.Intersect(accessFound).ToList());
				roomQueue.AddRange(blockedRooms.Where(r => !r.access.Any()).Select(r => r.room).ToList());
				blockedRooms.RemoveAll(r => !r.access.Any());

				var softLinks = currentroom.Links.Where(l => l.Entrance < 0 && !l.Access.Intersect(accessFound).Any()).ToList();
				var hardlinks = linkpairs.Where(l => l.origin.Room == currentroomid || l.target.Room == currentroomid).Concat(forcedLinks.Where(l => l.origin.Room == currentroomid || l.target.Room == currentroomid)).ToList();

				var blockedlinks = currentroom.Links.Where(l => l.Access.Intersect(accessFound).Any()).ToList();

				if (blockedlinks.Any())
				{
					blockedRooms.Add((blockedlinks.SelectMany(l => l.Access).Intersect(accessFound).ToList(), currentroomid));
				}

				roomQueue.AddRange(softLinks.Select(l => l.TargetRoom).Except(visitedRoom));

				foreach (var link in hardlinks)
				{
					if (link.origin.Room == currentroomid)
					{
						var targetroom = link.target.Origin.TargetRoom;
						if (!visitedRoom.Contains(targetroom))
						{
							roomQueue.Add(targetroom);
						}
					}
					else if (link.target.Room == currentroomid)
					{
						var targetroom = link.origin.Origin.TargetRoom;
						if (!visitedRoom.Contains(targetroom))
						{
							roomQueue.Add(targetroom);
						}
					}
				}
				visitedRoom.Add(currentroomid);
			}

			roomsToReach = roomsToReach.Distinct().ToList();
			visitedRoom = visitedRoom.Distinct().ToList();
			var missingRoom = roomsToReach.Except(visitedRoom).ToList();
			return !roomsToReach.Except(visitedRoom).Any();
		}
		private void ConnectLink(LogicLink link1, LogicLink link2)
		{
			Rooms.Find(r => r.Id == link1.Room).Links.Remove(link1.Current);
			Rooms.Find(r => r.Id == link2.Room).Links.Remove(link2.Current);

			newLinkToProcess.Add((link1.Room, new RoomLink(link2.Room, link1.Current.Entrance, link2.Origin.Teleporter, link1.Current.Access)));
			newLinkToProcess.Add((link2.Room, new RoomLink(link1.Room, link2.Current.Entrance, link1.Origin.Teleporter, link2.Current.Access)));
		}
		private void ConnectOverworldLink(LocationIds location, LogicLink link1, LogicLink link2)
		{
			Rooms.Find(r => r.Id == link1.Room).Links.Remove(link1.Current);
			Rooms.Find(r => r.Id == link2.Room).Links.Remove(link2.Current);

			newLinkToProcess.Add((link1.Room, new RoomLink(link2.Room, link1.Current.Entrance, link2.Origin.Teleporter, location, link1.Current.Access)));
			newLinkToProcess.Add((link2.Room, new RoomLink(link1.Room, link2.Current.Entrance, link1.Origin.Teleporter, link2.Current.Access)));
		}

		public void ReadPairs()
		{
			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("entrancespairs.yaml"));
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

			List<List<int>> result = new();

			try
			{
				result = deserializer.Deserialize<List<List<int>>>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			entrancesPairs = result;
			yamlfile = "";
		}
		private void ProcessClusterRoom(int roomid, List<ClusterRoom> clusterRooms, List<int> processedRooms, LocationIds currentLocation)
		{
			List<ClusterRoom> currentClusters = clusterRooms.Where(r => r.Rooms.Contains(roomid)).ToList();
			var currentRoom = Rooms.Find(r => r.Id == roomid);

			foreach (var cluster in currentClusters)
			{
				cluster.Location = currentLocation;
			}

			processedRooms.Add(roomid);

			foreach (var children in currentRoom.Links.OrderBy(l => l.Entrance))
			{
				if (!processedRooms.Contains(children.TargetRoom))
				{
					if (children.Access.Contains(AccessReqs.LibraCrest) || children.Access.Contains(AccessReqs.GeminiCrest) || children.Access.Contains(AccessReqs.MobiusCrest) || Rooms.Find(x => x.Id == children.TargetRoom).Type == RoomType.Subregion)
					{
						continue;
						//stop at crests
					}
					else
					{
						ProcessClusterRoom(children.TargetRoom, clusterRooms, processedRooms, currentLocation);
					}
				}
			}
		}
		private string GenerateDumpFile()
		{
			return "";
			/*
			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.WithEventEmitter(next => new FlowStyleIntegerSequences(next))
				.Build();
			var yaml = serializer.Serialize(Rooms);
			yaml += "\n\n" + serializer.Serialize(this.newLinkToProcess);

			#if DEBUG
				return yaml;
			#else
				return "";
			#endif*/
		}
	}
}
