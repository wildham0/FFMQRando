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
	public class FloorLink
	{
		public bool OneWay { get; set; }
		public bool ForceDeadEnd { get; set; }
		public bool ForceLinkDestination { get; set; }
		public bool ForceLinkOrigin { get; set; }
		public int ValidSiblings{ get; set; }
		public int Room { get; set; }
		public RoomLink Current { get; set; }
		public RoomLink Origins { get; set; }

		public FloorLink(int room, RoomLink link, RoomLink origins)
		{
			Room = room;
			Current = link;
			Origins = origins;
			OneWay = false;
			ForceLinkDestination = false;
			ForceLinkOrigin = false;
			ForceDeadEnd = false;
			ValidSiblings = 0;

		}
	}
	public class BigRoom
	{ 
		public List<int> Rooms { get; set; }
		public List<FloorLink> Links { get; set; }

		public BigRoom(List<int> rooms)
		{
			Rooms = rooms;
			Links = new();
		}
		public BigRoom(List<int> rooms, List<FloorLink> links)
		{
			Rooms = rooms;
			Links = links.ToList();
		}

		public void Merge(BigRoom room)
		{
			Rooms = Rooms.Concat(room.Rooms).ToList();
			Links = Links.Concat(room.Links).ToList();
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
		public List<int> OneWays { get; set; }
		public List<int> ForcedDeadends { get; set; }
		public List<ForcedLink> ForcedLinks { get; set; }

		public ShufflingData()
		{
			FixedEntrances = new();
			OneWays = new();
			ForcedDeadends = new();
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
			OneWays = result.OneWays;
			ForcedDeadends = result.ForcedDeadends;
			ForcedLinks = result.ForcedLinks;

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

			List<AccessReqs> crestAcess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };

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

				newLinkToProcess.Add((crest1room.Id, new RoomLink(crest2room.Id, crest1link.Entrance, crest2.Origins, crest1link.Access.Except(crestAcess).Concat(AccessReferences.ItemAccessReq[crest1.Crest]).ToList())));
				newLinkToProcess.Add((crest2room.Id, new RoomLink(crest1room.Id, crest2link.Entrance, crest1.Origins, crest2link.Access.Except(crestAcess).Concat(AccessReferences.ItemAccessReq[crest2.Crest]).ToList())));

				CrestRoomLinks.Add((crest1link.Teleporter, crest2room.Id));
				CrestRoomLinks.Add((crest2link.Teleporter, crest1room.Id));
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
		}
		public void FloorShuffle(bool shufflefloors, MT19337 rng)
		{
		
			if (!shufflefloors)
			{
				return;
			}
			
			ReadPairs();
			ShufflingData shufflingData = new();
			shufflingData.ReadData(rng);

			// Get links that can be shuffled
			var flatLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var linkSet = entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[0]).Id, flatLinks.Find(l => l.l.Entrance == e[0]).l, flatLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			linkSet.AddRange(entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[1]).Id, flatLinks.Find(l => l.l.Entrance == e[1]).l, flatLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());

			// Create list of big rooms
			List<BigRoom> bigRooms = new();

			int maxId = 0;

			// Get rooms
			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0 || shufflingData.FixedEntrances.Contains(x.Entrance)).Select(x => x.TargetRoom).Append(room.Id).ToList();

				foreach (var link in internalLinks)
				{
					maxId = (link > maxId) ? link : maxId;
				}

				bigRooms.Add(new BigRoom(internalLinks, linkSet.Where(l => l.Room == room.Id).ToList()));
			}

			for (int i = 0; i <= maxId; i++)
			{ 
				var commonBigRooms = bigRooms.Where(x => x.Rooms.Contains(i)).ToList();

				if (commonBigRooms.Count > 1)
				{
					var bigRoomsToMerge = commonBigRooms.GetRange(1, commonBigRooms.Count - 1);
					foreach (var bigroom in bigRoomsToMerge)
					{
						bigRooms.Remove(bigroom);
						commonBigRooms[0].Merge(bigroom);
					}
				}
			}

			linkSet.ForEach(x => x.OneWay = shufflingData.OneWays.Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForceDeadEnd = shufflingData.ForcedDeadends.Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForceLinkOrigin = shufflingData.ForcedLinks.Select(x => x.Origin).ToList().Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForceLinkDestination = shufflingData.ForcedLinks.Select(x => x.Destination).ToList().Contains(x.Current.Entrance));
			foreach (var link in linkSet.Where(x => x.ForceLinkOrigin).ToList())
            {
				link.ValidSiblings = shufflingData.ForcedLinks.Find(x => x.Origin == link.Current.Entrance).Destination;
			}

			List<AccessReqs> crestAccess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			var crestRooms = Rooms.Where(x => x.Links.Where(l => l.Access.Intersect(crestAccess).Any()).Any()).Select(x => x.Id).ToList();
			var macShipBarredRooms = crestRooms.Append(157);
			int macShipDeck = 187;

			// Sort initial rooms
			List<int> seedRooms = Rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();
			List<BigRoom> seedBigRooms = bigRooms.Where(x => x.Rooms.Intersect(seedRooms).Any()).ToList();
			bigRooms = bigRooms.Except(seedBigRooms).ToList();
			
			var progressBigRooms = bigRooms.Where(x => x.Links.Count > 1).ToList();
			var deadendBigRooms = bigRooms.Where(x => x.Links.Count == 1).ToList();

			newLinkToProcess = new();

			seedBigRooms.Shuffle(rng);
			seedBigRooms = seedBigRooms.Where(x => x.Links.Count > 0).ToList();

			// Distribute non deadends room; a big dungeon is 15-20 rooms
			while (progressBigRooms.Any())
			{
				var receivingRoom = rng.PickFrom(seedBigRooms);

				var originLinks = receivingRoom.Links.Where(x => !x.ForceLinkDestination).ToList();
				var originLink = rng.PickFrom(originLinks);
				//Console.WriteLine("Origin Link: " + originLink.Current.Entrance);
				receivingRoom.Links.Remove(originLink);

				List<BigRoom> givingRooms;

				if (originLink.ForceLinkOrigin)
				{
					givingRooms = progressBigRooms.Where(x => !x.Links.Where(l => l.ForceLinkDestination).Any()).ToList();

					if (!givingRooms.Any())
					{
						continue;
					}
				}
				else if (originLink.ForceDeadEnd)
				{
					givingRooms = progressBigRooms.Where(x => !x.Rooms.Intersect(crestRooms).Any()).ToList();
				}
				else
				{
					givingRooms = progressBigRooms;
				}

				// Mac Ship Failsafe
				if (receivingRoom.Rooms.Contains(macShipDeck))
				{
					givingRooms = progressBigRooms.Where(x => !x.Rooms.Intersect(macShipBarredRooms).Any()).ToList();

					if (!givingRooms.Any())
					{
						continue;
					}
				}

				var givingRoom = rng.PickFrom(givingRooms);
				progressBigRooms.Remove(givingRoom);
				//Console.WriteLine("Giving Room: " + givingRoom.Rooms[0]);
				var destinationLinks = givingRoom.Links.Where(x => !x.OneWay && !x.ForceDeadEnd && !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
				var destinationLink = rng.PickFrom(destinationLinks);
				//Console.WriteLine("Destination Link: " + destinationLink.Current.Entrance);
				givingRoom.Links.Remove(destinationLink);

				ConnectLink(originLink, destinationLink);

				if (originLink.ForceLinkOrigin)
				{
					//Console.WriteLine("Is a forced");
					var validNewHeads = givingRoom.Links.Where(x => !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
					if (!validNewHeads.Any())
					{
						throw new Exception("Floor Shuffle: One way Orientation Error\n\n" + "Origin Link: " + originLink.Current.Entrance + "\n\n" + GenerateDumpFile());
					}

					var newLinkHead = rng.PickFrom(validNewHeads);
					//Console.WriteLine("LinkHead: " + newLinkHead.Current.Entrance);
					newLinkHead.ForceLinkOrigin = true;
					newLinkHead.ValidSiblings = originLink.ValidSiblings;
				}

				if (originLink.ForceDeadEnd)
				{
					//Console.WriteLine("Is a forced");
					var validNewHeads = givingRoom.Links.Where(x => !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
					validNewHeads.ForEach(x => x.ForceDeadEnd = true);
				}

				receivingRoom.Merge(givingRoom);
			}

			// Connect forced Links
			foreach (var room in seedBigRooms)
			{
				var logicaldeadends = room.Links.Where(x => x.ForceLinkOrigin).ToList();
				
				foreach (var deadend in logicaldeadends)
				{
					if (!room.Links.Contains(deadend))
					{
						continue;
					}

					var validLinks = room.Links.Where(x => deadend.ValidSiblings == x.Current.Entrance).ToList();
					if (!validLinks.Any())
					{
						validLinks = room.Links.Where(x => !x.ForceLinkDestination).ToList();
					}
					var seedLink = rng.PickFrom(validLinks);
					room.Links.Remove(seedLink);
					room.Links.Remove(deadend);

					ConnectLink(seedLink, deadend);
				}
			}

			// Forced Deadends and equalization
			foreach (var room in seedBigRooms)
			{
				var deadendLinks = room.Links.Where(x => x.ForceDeadEnd).ToList();

				foreach (var link in deadendLinks)
				{
					var validDeadEnds = deadendBigRooms.Where(x => !x.Rooms.Intersect(crestRooms).Any()).ToList();

					// MacShip protect
					if (room.Rooms.Contains(macShipDeck))
					{
						validDeadEnds = deadendBigRooms.Where(x => !x.Rooms.Intersect(macShipBarredRooms).Any()).ToList();
					}

					var connectRoom = rng.PickFrom(validDeadEnds);
					deadendBigRooms.Remove(connectRoom);
					var seedLink = rng.PickFrom(room.Links);
					room.Links.Remove(seedLink);
					var addonLink = rng.TakeFrom(connectRoom.Links);
					ConnectLink(seedLink, addonLink);

					room.Merge(connectRoom);
				}
				
				if ((room.Links.Count % 2) == 1)
				{
					var validDeadEnds = deadendBigRooms;

					// MacShip protect
					if (room.Rooms.Contains(macShipDeck))
					{
						validDeadEnds = deadendBigRooms.Where(x => !x.Rooms.Intersect(macShipBarredRooms).Any()).ToList();
					}

					var connectRoom = rng.PickFrom(validDeadEnds);
					deadendBigRooms.Remove(connectRoom);
					var seedLink = rng.PickFrom(room.Links);
					room.Links.Remove(seedLink);
					var addonLink = rng.TakeFrom(connectRoom.Links);
					ConnectLink(seedLink, addonLink);

					room.Merge(connectRoom);
				}
			}

			if ((deadendBigRooms.Count % 2) == 1)
			{
				throw new Exception("Floor Shuffle: Deadends Count Error\n" + GenerateDumpFile());
			}

			while (deadendBigRooms.Any())
			{
				var unfilledBigRooms = seedBigRooms.Where(x => x.Links.Count > 0).ToList();

				var deadends = new List<BigRoom>() { rng.TakeFrom(deadendBigRooms), rng.TakeFrom(deadendBigRooms) };
				
				if (deadends[0].Rooms.Intersect(macShipBarredRooms).Any() || deadends[1].Rooms.Intersect(macShipBarredRooms).Any())
				{
					unfilledBigRooms = unfilledBigRooms.Where(x => !x.Rooms.Contains(macShipDeck)).ToList();
					if (!unfilledBigRooms.Any())
					{
						throw new Exception("Floor Shuffle: Mac Ship Crest Error\n" + GenerateDumpFile());
					}
				}

				var pickedRoom = rng.PickFrom(unfilledBigRooms);

				foreach (var connectRoom in deadends)
				{
					var seedLink = rng.PickFrom(pickedRoom.Links);
					pickedRoom.Links.Remove(seedLink);

					var addonLink = rng.TakeFrom(connectRoom.Links);
					ConnectLink(seedLink, addonLink);

					pickedRoom.Merge(connectRoom);
				}
			}

			// Tie loose ends
			foreach (var room in seedBigRooms)
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

		private void ConnectLink(FloorLink link1, FloorLink link2)
		{
			Rooms.Find(r => r.Id == link1.Room).Links.Remove(link1.Current);
			Rooms.Find(r => r.Id == link2.Room).Links.Remove(link2.Current);

			newLinkToProcess.Add((link1.Room, new RoomLink(link2.Room, link1.Current.Entrance, link2.Origins.Teleporter, link1.Current.Access)));
			newLinkToProcess.Add((link2.Room, new RoomLink(link1.Room, link2.Current.Entrance, link1.Origins.Teleporter, link2.Current.Access)));
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
