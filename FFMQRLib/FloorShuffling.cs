using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;

namespace FFMQLib
{
	public class CrestLink
	{ 
		public (int id, int type) Entrance { get; set; }
		public bool Deadend { get; set; }
		public int Priority { get; set; }
		public (int id, int type) Origins { get; set; }
		public Items Crest { get; set; }

		public CrestLink((int, int) id, (int, int) origins, bool deadend, int priority)
		{
			Entrance = id;
			Deadend = deadend;
			Priority = priority;
			Origins = origins;
			Crest = Items.LibraCrest;
		}
	}
	public class LogicLink
	{
		public bool EntranceOnly { get; set; }
		public bool ForceDeadEnd { get; set; }
		public bool ForceLinkDestination { get; set; }
		public bool ForceLinkOrigin { get; set; }
		public int ForcedDestination { get; set; }
		public int Room { get; set; }
		public List<int> ForbiddenDestinations { get; set; }
		public RoomLink Current { get; set; }
		public RoomLink Origin { get; set; }

		public LogicLink(int room, RoomLink link, RoomLink origin)
		{
			Room = room;
			Current = link;
			Origin = origin;
			EntranceOnly = false;
			ForceLinkDestination = false;
			ForceLinkOrigin = false;
			ForceDeadEnd = false;
			ForcedDestination = 0;
			ForbiddenDestinations = new();
		}

		public void UpdateCurrent(RoomLink link)
		{ 
			Current.Entrance = link.Entrance;
			Current.TargetRoom = link.TargetRoom;
			Current.Teleporter = link.Teleporter;
		}
	}
	public class ClusterRoom
	{ 
		public List<int> Rooms { get; set; }
		public List<LogicLink> Links { get; set; }
		public int Size { get; set; }

		public ClusterRoom(List<int> rooms)
		{
			Rooms = rooms;
			Links = new();
			Size = 0;
		}
		public ClusterRoom(List<int> rooms, List<LogicLink> links)
		{
			Rooms = rooms;
			Links = links.ToList();
			Size = 0;
		}

		public void Merge(ClusterRoom room)
		{
			Rooms = Rooms.Concat(room.Rooms).ToList();
			Links = Links.Concat(room.Links).ToList();
			Size++;
		}
		public void UpdateLinks(LogicLink originLink, MT19337 rng)
		{
			if (originLink.ForceLinkOrigin)
			{
				var validOrigins = Links.Where(x => !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
				if (!validOrigins.Any())
				{
					throw new Exception("Floor Shuffle: One way Orientation Error\n\n" + "Origin Link: " + originLink.Current.Entrance);
				}

				var newOrigin = rng.PickFrom(validOrigins);
				newOrigin.ForceLinkOrigin = true;
				newOrigin.ForcedDestination = originLink.ForcedDestination;
			}

			if (originLink.ForceDeadEnd)
			{
				var validDeadEnds = Links.Where(x => !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
				validDeadEnds.ForEach(x => x.ForceDeadEnd = true);
			}

			Links.ForEach(x => x.ForbiddenDestinations.AddRange(originLink.ForbiddenDestinations));
		}
	}
	public class ForcedLink
	{
		public List<int> Origins { get; set; }
		public List<int> Destinations { get; set; }
		[YamlIgnore]
		public int Origin { get; set; }
		[YamlIgnore]
		public int Destination { get; set; }
		public ForcedLink()
		{
			Origins = new();
			Destinations = new();
			Origin = 0;
			Destination = 0;
		}
	}
	public class ShufflingData
	{ 
		public List<int> FixedEntrances { get; set; }
		public List<int> TownsTemples { get; set; }
		public List<int> EntranceOnly { get; set; }
		public List<int> ForcedDeadends { get; set; }
		public List<ForcedLink> ForcedLinks { get; set; }
		public List<int> MacShipExclusions { get; set; }

		public ShufflingData()
		{
			FixedEntrances = new();
			TownsTemples = new();
			EntranceOnly = new();
			ForcedDeadends = new();
			MacShipExclusions = new();
			ForcedLinks = new();
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

			FixedEntrances = result.FixedEntrances;
			EntranceOnly = result.EntranceOnly;
			ForcedDeadends = result.ForcedDeadends;
			ForcedLinks = result.ForcedLinks;
			TownsTemples = result.TownsTemples;
			MacShipExclusions = result.MacShipExclusions;

			foreach (var link in ForcedLinks)
			{
				link.Origin = rng.PickFrom(link.Origins);
				link.Destination = rng.PickFrom(link.Destinations);
			}
		}
	}
	public partial class GameLogic
    {
		private List<List<int>> entrancesPairs;
		private List<(int roomid, RoomLink link)> newLinkToProcess;
		public List<((int, int) Teleporter, int Room)> CrestRoomLinks { get; set; }


		public void CrestShuffle(bool shufflecrests, MT19337 rng)
		{
			List<CrestLink> crestList = new()
			{
				new CrestLink((67, 8), (64, 8), true, 0), // (0x27, 1)
				new CrestLink((68, 8), (65, 8), true, 0), // (0x28, 1)
				new CrestLink((69, 8), (66, 8), true, 0), // (0x29, 1)
				new CrestLink((72, 8), (45, 8), false, 1), // Aquaria Vendor House
				new CrestLink((59, 8), (60, 8), false, 0),
				new CrestLink((60, 8), (59, 8), true, 0),
				//((0x2D, 1), (33, 8)), Exclude spencer's cave teleporter
				//((0x2E, 1), (34, 8)),
				//((0x2F, 1), (35, 8)),
				//((0x30, 1), (36, 8)),
				new CrestLink((64, 8), (67, 8), false, 0), // always short
				new CrestLink((65, 8), (68, 8), false, 0),
				new CrestLink((66, 8), (69, 8), false, 0),
				new CrestLink((62, 8), (63, 8), true, 0),
				new CrestLink((63, 8), (62, 8), false, 0), // to short
				new CrestLink((45, 8), (72, 8), false, 1), // Fireburg Vendor House
				new CrestLink((54, 8), (44, 8), false, 2), // Fireburg Grenade Man
				new CrestLink((71, 8), (70, 8), false, 0),
				new CrestLink((70, 8), (71, 8), true, 0),
				new CrestLink((44, 8), (54, 8), false, 0),
				new CrestLink((43, 8), (61, 8), false, 2), // Windia Mobius Old
				new CrestLink((61, 8), (43, 8), true, 0),
			};

			CrestRoomLinks = new();

			if (!shufflecrests)
			{
				foreach (var crest in crestList)
				{
					var crestRoom = Rooms.Find(x => x.Links.Where(l => l.Teleporter == crest.Origins).Any());
					CrestRoomLinks.Add((crest.Entrance, crestRoom.Id));
				}
				
				return;
			}

			List<Items> crestTiles = new()
			{
				Items.LibraCrest,
				Items.LibraCrest,
				Items.GeminiCrest,
				Items.GeminiCrest,
				Items.GeminiCrest,
				//Items.GeminiCrest, Spencer's cave crests
				//Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
			};

			crestList.Shuffle(rng);
			crestList = crestList.OrderByDescending(x => x.Priority).ToList();

			List<(int priority, Items crest)> crestPriority = new();

			int deadendCount = 0;
			int passableCount = 0;

			List<(int roomid, RoomLink link)> newLinkToProcess = new();

			while (crestList.Any())
			{
				CrestLink crest1;
				CrestLink crest2;

				deadendCount = crestList.Where(x => x.Deadend).Count();
				passableCount = crestList.Where(x => !x.Deadend).Count();

				crest1 = crestList.First();
				crestList.Remove(crest1);

				// Don't match 2 deadends
				if (crest1.Deadend)
				{
					var nondeadend = crestList.Where(x => !x.Deadend).ToList();
					crest2 = rng.PickFrom(nondeadend);
					crestList.Remove(crest2);
				}
				else
				{
					if (deadendCount < passableCount)
					{
						crest2 = rng.TakeFrom(crestList);
					}
					else
					{
						crest2 = crestList.Where(x => x.Deadend).ToList().First();
						crestList.Remove(crest2);
					}
				}

				// Check for linked crests tiles
				if (crest1.Priority > 0)
				{
					if (crestPriority.Where(x => x.priority == crest1.Priority).Any())
					{
						crest1.Crest = crestPriority.Find(x => x.priority == crest1.Priority).crest;
						crestTiles.Remove(crest1.Crest);
					}
					else
					{
						var pickedCrest = rng.TakeFrom(crestTiles);
						crest1.Crest = pickedCrest;
						crestPriority.Add((crest1.Priority, pickedCrest));
					}
				}
				else
				{
					crest1.Crest = rng.TakeFrom(crestTiles);
				}

				crest2.Crest = crest1.Crest;

				if (crest2.Priority > 0 && !crestPriority.Where(x => x.priority == crest2.Priority).Any())
				{
					crestPriority.Add((crest2.Priority, crest1.Crest));
				}

				var crest1room = Rooms.Find(x => x.Links.Where(l => l.Teleporter == crest1.Entrance).Any());
				var crest1link = crest1room.Links.Find(l => l.Teleporter == crest1.Entrance);

				var crest2room = Rooms.Find(x => x.Links.Where(l => l.Teleporter == crest2.Entrance).Any());
				var crest2link = crest2room.Links.Find(l => l.Teleporter == crest2.Entrance);

				crest1room.Links.Remove(crest1link);
				crest2room.Links.Remove(crest2link);

				newLinkToProcess.Add((crest1room.Id, new RoomLink(crest2room.Id, crest1link.Entrance, crest2.Origins, crest1link.Access.Except(AccessReferences.CrestsAccess).Concat(AccessReferences.ItemAccessReq[crest1.Crest]).ToList())));
				newLinkToProcess.Add((crest2room.Id, new RoomLink(crest1room.Id, crest2link.Entrance, crest1.Origins, crest2link.Access.Except(AccessReferences.CrestsAccess).Concat(AccessReferences.ItemAccessReq[crest2.Crest]).ToList())));

				CrestRoomLinks.Add((crest1link.Teleporter, crest2room.Id));
				CrestRoomLinks.Add((crest2link.Teleporter, crest1room.Id));
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
		}
		public void FloorShuffle(MapShufflingMode mapshuffling, MT19337 rng)
		{

			bool shuffleFloors = mapshuffling == MapShufflingMode.Dungeons || mapshuffling == MapShufflingMode.OverworldDungeons || mapshuffling == MapShufflingMode.Everything;
			bool includeTemplesTowns = mapshuffling == MapShufflingMode.Everything;

			if (!shuffleFloors)
			{
				return;
			}

			newLinkToProcess = new();

			ReadPairs();
			ShufflingData shufflingData = new();
			shufflingData.ReadData(rng);

			// Get links that can be shuffled
			var roomLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var logicLinks = entrancesPairs.Select(e => new LogicLink(roomLinks.Find(l => l.l.Entrance == e[0]).Id, roomLinks.Find(l => l.l.Entrance == e[0]).l, roomLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			logicLinks.AddRange(entrancesPairs.Select(e => new LogicLink(roomLinks.Find(l => l.l.Entrance == e[1]).Id, roomLinks.Find(l => l.l.Entrance == e[1]).l, roomLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());

			// Find rooms that have requirements in other rooms to populate forbidden destinations
			List<int> doomCastleRooms = new() { 195, 196, 197, 198, 199, 200 };
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
			logicLinks.ForEach(x => x.EntranceOnly = shufflingData.EntranceOnly.Contains(x.Current.Entrance));
			logicLinks.ForEach(x => x.ForceDeadEnd = shufflingData.ForcedDeadends.Contains(x.Current.Entrance));
			logicLinks.ForEach(x => x.ForceLinkOrigin = shufflingData.ForcedLinks.Select(x => x.Origin).ToList().Contains(x.Current.Entrance));
			logicLinks.ForEach(x => x.ForceLinkDestination = shufflingData.ForcedLinks.Select(x => x.Destination).ToList().Contains(x.Current.Entrance));
			forbiddenDestinations.ForEach(x => logicLinks.Find(l => l.Current.Entrance == x.entrance).ForbiddenDestinations = new() { x.room });

			foreach (var link in logicLinks.Where(x => x.ForceLinkOrigin).ToList())
			{
				link.ForcedDestination = shufflingData.ForcedLinks.Find(x => x.Origin == link.Current.Entrance).Destination;
			}

			// Create the initial list of cluster rooms which will be the basis for how everything is connected logically
			List<ClusterRoom> clusterRooms = new();

			int maxId = 0;

			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0 || shufflingData.FixedEntrances.Contains(x.Entrance)).Select(x => x.TargetRoom).Append(room.Id).ToList();

				foreach (var link in internalLinks)
				{
					maxId = (link > maxId) ? link : maxId;
				}

				clusterRooms.Add(new ClusterRoom(internalLinks, logicLinks.Where(l => l.Room == room.Id).ToList()));
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

			// Special restrictions
			var crestRooms = Rooms.Where(x => x.Links.Where(l => l.Access.Intersect(AccessReferences.CrestsAccess).Any()).Any()).Select(x => x.Id).ToList();
			var macShipBarredRooms = crestRooms.Concat(shufflingData.MacShipExclusions).Concat(forbiddenDestinations.Select(x => x.room)).ToList();
			int macShipDeck = 187;
			int macShipMaxSize = 4;

			// Shuffle our core locations
			var seedRooms = Rooms.Find(x => x.Id == 0).Links.Select(l => l.TargetRoom).Except(new List<int> { 125 }).ToList();
			var seedClusterRooms = clusterRooms.Where(x => x.Rooms.Intersect(seedRooms).Any()).ToList();
			var seedClusterRoomsToShuffle = seedClusterRooms.Where(x => x.Links.Where(l => l.Current.TargetRoom == 0).Any()).ToList();
			var seedClusterRoomsFixed = seedClusterRooms.Except(seedClusterRoomsToShuffle).ToList();
			var seedClusterRoomsToShuffleProgress = seedClusterRoomsToShuffle.Where(x => x.Links.Count > 1).ToList();
			var seedClusterRoomsToShuffleDeadends = seedClusterRoomsToShuffle.Where(x => x.Links.Count == 1).ToList();


			var initialProgressClusterRooms = clusterRooms.Where(x => x.Links.Count > 1 && !x.Rooms.Intersect(seedRooms).Any() && !x.Rooms.Contains(0)).Concat(seedClusterRoomsToShuffleProgress).ToList();
			var initialDeadendClusterRooms = clusterRooms.Where(x => x.Links.Count == 1 && !x.Rooms.Intersect(seedRooms).Any() && !x.Rooms.Contains(0)).Concat(seedClusterRoomsToShuffleDeadends).ToList();


			initialProgressClusterRooms.Shuffle(rng);
			initialDeadendClusterRooms.Shuffle(rng);

			List<ClusterRoom> coreClusterRooms = new();
			coreClusterRooms.AddRange(initialProgressClusterRooms.GetRange(0, seedClusterRoomsToShuffleProgress.Count));
			coreClusterRooms.AddRange(initialDeadendClusterRooms.GetRange(0, seedClusterRoomsToShuffleDeadends.Count));

			var validSeecClusterRoomsToSwitch = seedClusterRoomsToShuffle.Except(coreClusterRooms).ToList();

			foreach (var room in coreClusterRooms)
			{
				ClusterRoom coreClusterRoom;
				LogicLink overworldLink;
				LogicLink coreClusterRoomLink;

				if (!room.Links.Where(l => l.Current.TargetRoom == 0).Any())
				{
					coreClusterRoom = rng.TakeFrom(validSeecClusterRoomsToSwitch);
					overworldLink = clusterRooms.Find(x => x.Rooms.Contains(0)).Links.Find(x => x.Origin.Entrance == coreClusterRoom.Links.Find(x => x.Current.TargetRoom == 0).Current.Entrance);

					var validLinks = room.Links.Where(x => !x.ForceDeadEnd && !x.EntranceOnly && !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
					coreClusterRoomLink = rng.PickFrom(validLinks);
					room.Links.Remove(coreClusterRoomLink);
				}
				else
				{
					coreClusterRoom = room;
					overworldLink = clusterRooms.Find(x => x.Rooms.Contains(0)).Links.Find(x => x.Origin.Entrance == coreClusterRoom.Links.Find(x => x.Current.TargetRoom == 0).Current.Entrance);

					coreClusterRoomLink = room.Links.Find(x => x.Current.TargetRoom == 0);
					room.Links.Remove(coreClusterRoomLink);
				}


				ConnectLink(overworldLink, coreClusterRoomLink);
			}

			coreClusterRooms = coreClusterRooms.Concat(seedClusterRoomsFixed).ToList();
			var coreClusterRoomsIds = coreClusterRooms.SelectMany(x => x.Rooms).Append(0).ToList();
			var progressClusterRooms = clusterRooms.Where(x => x.Links.Count > 1 && !x.Rooms.Intersect(coreClusterRoomsIds).Any()).ToList();
			var deadendClusterRooms = clusterRooms.Where(x => x.Links.Count == 1 && !x.Rooms.Intersect(coreClusterRoomsIds).Any()).ToList();

			coreClusterRooms.Shuffle(rng);
			coreClusterRooms = coreClusterRooms.Where(x => x.Links.Count > 0).ToList();

			// Distribute non deadends room
			while (progressClusterRooms.Any())
			{
				var originRoom = rng.PickFrom(coreClusterRooms);
				var originLinks = originRoom.Links.Where(x => !x.ForceLinkDestination).ToList();
				var originLink = rng.PickFrom(originLinks);
				
				List<ClusterRoom> destinationRooms = progressClusterRooms.Where(x => 
					!x.Rooms.Intersect(originLink.ForbiddenDestinations).Any() &&
					(originLink.ForceLinkOrigin ? !x.Links.Where(l => l.ForceLinkDestination).Any() : true) &&
					(originLink.ForceDeadEnd ? (!x.Rooms.Intersect(crestRooms).Any() && (x.Links.Count % 2 == 0)) : true) &&
					(originRoom.Rooms.Contains(macShipDeck) ? !x.Rooms.Intersect(macShipBarredRooms).Any() : true)
				).ToList();

				if (!destinationRooms.Any() || (originRoom.Rooms.Contains(macShipDeck) && originRoom.Size >= macShipMaxSize))
				{
					continue;
				}

				var destinationRoom = rng.PickFrom(destinationRooms);
				progressClusterRooms.Remove(destinationRoom);

				var destinationLinks = destinationRoom.Links.Where(x => !x.EntranceOnly && !x.ForceDeadEnd && !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
				var destinationLink = rng.PickFrom(destinationLinks);

				originRoom.Links.Remove(originLink);
				destinationRoom.Links.Remove(destinationLink);

				ConnectLink(originLink, destinationLink);
				destinationRoom.UpdateLinks(originLink, rng);
				originRoom.Merge(destinationRoom);
			}

			// Connect Forced Links
			foreach (var room in coreClusterRooms)
			{
				var forcedLinks = room.Links.Where(x => x.ForceLinkOrigin).ToList();
				
				foreach (var forcedLink in forcedLinks)
				{
					var forcedDestination = room.Links.Find(x => forcedLink.ForcedDestination == x.Current.Entrance);
					if (forcedDestination == null)
					{
						throw new Exception("Map Shuffling Error: Lost forced destination rooms");
					}

					room.Links.Remove(forcedLink);
					room.Links.Remove(forcedDestination);

					ConnectLink(forcedLink, forcedDestination);
				}
			}

			// Forced Deadends and equalization
			foreach (var room in coreClusterRooms)
			{
				var deadendLinks = room.Links.Where(x => x.ForceDeadEnd).ToList();

				while (deadendLinks.Any())
				{
					var originLink = deadendLinks.First();

					List<ClusterRoom> destinationRooms = deadendClusterRooms.Where(x =>
						!x.Rooms.Intersect(crestRooms).Any() &&
						!x.Rooms.Intersect(originLink.ForbiddenDestinations).Any() &&
						(room.Rooms.Contains(macShipDeck) ? !x.Rooms.Intersect(macShipBarredRooms).Any() : true)
					).ToList();

					if (!destinationRooms.Any())
					{
						continue;
					}

					room.Links.Remove(originLink);
					deadendLinks.Remove(originLink);

					var destinationRoom = rng.PickFrom(destinationRooms);
					var destinationLink = rng.TakeFrom(destinationRoom.Links);
					deadendClusterRooms.Remove(destinationRoom);

					ConnectLink(originLink, destinationLink);
					room.Merge(destinationRoom);
				}

				if ((room.Links.Count % 2) == 1)
				{
					var originLink = rng.PickFrom(room.Links);

					List<ClusterRoom> destinationRooms = deadendClusterRooms.Where(x =>
						!x.Rooms.Intersect(originLink.ForbiddenDestinations).Any() &&
						room.Rooms.Contains(macShipDeck) ? !x.Rooms.Intersect(macShipBarredRooms).Any() : true
					).ToList();

					var destinationRoom = rng.PickFrom(destinationRooms);
					var destinationLink = rng.TakeFrom(destinationRoom.Links);

					deadendClusterRooms.Remove(destinationRoom);
					room.Links.Remove(originLink);
					
					ConnectLink(originLink, destinationLink);
					room.Merge(destinationRoom);
				}
			}

			// Add Dummy room if there was too many locations to equalize
			if ((deadendClusterRooms.Count % 2) == 1)
			{
				var originLink = new RoomLink(500, 0, (141, 1), new List<AccessReqs>());
				var destinationLink = new RoomLink(0, 481, (0, 10), new List<AccessReqs>());

				deadendClusterRooms.Add(
					new ClusterRoom(new List<int>() { 500 },
					new List<LogicLink>() { new LogicLink(500, destinationLink, originLink) }));

				Rooms.Add(new Room("Dummy Room", 500, 0x11, new List<GameObjectData>() { }, new List<RoomLink> { }));
			}

			// Connect remaining dead ends
			var macshipexcepttioncount = 0;
			while (deadendClusterRooms.Any())
			{
				deadendClusterRooms.Shuffle(rng);
				var destinationRooms = new List<ClusterRoom>() { deadendClusterRooms[0], deadendClusterRooms[1] };

				var unfilledClusterRooms = coreClusterRooms.Where(x => 
					x.Links.Count > 0 &&
					!x.Links.SelectMany(l => l.ForbiddenDestinations).Intersect(destinationRooms.SelectMany(d => d.Rooms)).Any() &&
					!x.Links.SelectMany(l => l.ForbiddenDestinations).Intersect(destinationRooms.SelectMany(d => d.Rooms)).Any() &&
					(macShipBarredRooms.Intersect(destinationRooms.SelectMany(d => d.Rooms)).Any() ? !x.Rooms.Contains(macShipDeck) : true)
					).ToList();

				if (!unfilledClusterRooms.Any())
				{
					// Keep counts here in case we hit the unlikely scenario of mac ship behind the only location left and the deadends left are forbidden to it
					// We should just reroll in that unlikely scenario
					macshipexcepttioncount++;
					if (macshipexcepttioncount > 50)
					{
						throw new Exception("Floor Shuffle: Mac Ship Crest Error\n" + GenerateDumpFile());
					}

					continue;
				}

				var originRoom = rng.PickFrom(unfilledClusterRooms);

				deadendClusterRooms.Remove(destinationRooms[0]);
				deadendClusterRooms.Remove(destinationRooms[1]);

				foreach (var destinationRoom in destinationRooms)
				{
					var originLink = rng.PickFrom(originRoom.Links);
					originRoom.Links.Remove(originLink);

					var destinationLink = rng.TakeFrom(destinationRoom.Links);
					ConnectLink(originLink, destinationLink);

					originRoom.Merge(destinationRoom);
				}
			}

			// Tie loose ends
			foreach (var room in coreClusterRooms)
			{
				while (room.Links.Any())
				{
					if ((room.Links.Count % 2) == 1)
					{
						throw new Exception("Floor Shuffle: Gap Connection Error\n" + GenerateDumpFile());
					}

					ConnectLink(rng.TakeFrom(room.Links), rng.TakeFrom(room.Links));
				}
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
		}

		private void ConnectLink(LogicLink link1, LogicLink link2)
		{
			Rooms.Find(r => r.Id == link1.Room).Links.Remove(link1.Current);
			Rooms.Find(r => r.Id == link2.Room).Links.Remove(link2.Current);

			newLinkToProcess.Add((link1.Room, new RoomLink(link2.Room, link1.Current.Entrance, link2.Origin.Teleporter, link1.Current.Access)));
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
		private string GenerateDumpFile()
		{
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
			#endif
		}
	}
}
