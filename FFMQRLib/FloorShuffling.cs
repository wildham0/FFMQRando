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
		public int Entrance { get; set; }
		public (int id, int type) Teleporter { get; set; }
		public bool OneWay { get; set; }
		public bool ForcePassage { get; set; }
		public int Room { get; set; }
		public RoomLink Current { get; set; }
		public RoomLink Origins { get; set; }
		//public (int id, int type) Origins { get; set; }

		public FloorLink(int room, RoomLink link, RoomLink origins)
		{
			//Entrance = entrance;
			Room = room;
			Current = link;
			OneWay = false;
			ForcePassage = false;
			//Teleporter = link.Teleporter;
			Origins = origins;
		}
	}
	public class LinkPairs
	{
		[YamlIgnore]
		public (int entrancea, int entranceb) Pair { get; set; }
		public List<int> pair
		{
			get => new List<int> { Pair.entrancea, Pair.entranceb };
			set => Pair = (value[0], value[1]);
		}
		public LinkPairs()
		{
			Pair = (0, 0);
		}
	}
	public class BigRoom
	{ 
		public List<int> Rooms { get; set; }
		public List<FloorLink> Links { get; set; }
		public bool Deadend { get; set; }
		public int RoomValue { get; set; }
		
		public List<BigRoom> MergedRooms { get; set; }

		public BigRoom(List<int> rooms)
		{
			Rooms = rooms;
			Links = new();
			MergedRooms = new();
			Deadend = false;
			RoomValue = 0;
		}

		public void Merge(BigRoom room)
		{
			Rooms = Rooms.Concat(room.Rooms).ToList();
			Links = Links.Concat(room.Links).ToList();
			MergedRooms.Add(room);
			//Deadend = false;
			RoomValue += (room.RoomValue - 2);
		}



	}
	public partial class GameLogic
    {
		private List<List<int>> entrancesPairs;
		private List<(int roomid, RoomLink link)> newLinkToProcess;


		public void CrestShuffle(bool shufflecrests, MT19337 rng)
		{
			if (!shufflecrests)
			{
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
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
		}
		public void FloorShuffle(bool shufflecrests, MT19337 rng)
		{
		
			/*
			if (!shufflecrests)
			{
				return;
			}*/
			
			ReadPairs();

			// Create list of big rooms
			List<BigRoom> bigRooms = new();

			int maxId = 0;

			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0).Select(x => x.TargetRoom).Append(room.Id).ToList();

				foreach (var link in internalLinks)
				{
					maxId = (link > maxId) ? link : maxId;
				}

				bigRooms.Add(new BigRoom(internalLinks));
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

			/*
			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0).Select(x => x.TargetRoom).Append(room.Id).ToList();

				bool commonroom = false;

				foreach (var bigroom in bigRooms)
				{
					if (bigroom.Rooms.Intersect(internalLinks).Any())
					{
						bigroom.Rooms.AddRange(internalLinks);
						commonroom = true;
						break;
					}
				}

				if (!commonroom)
				{ 
					bigRooms.Add(new BigRoom(internalLinks));
				}
			}*/

			// Get links that can be shuffled
			var flatLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var linkSet = entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[0]).Id, flatLinks.Find(l => l.l.Entrance == e[0]).l, flatLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			linkSet.AddRange(entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[1]).Id, flatLinks.Find(l => l.l.Entrance == e[1]).l, flatLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());

			// Define oneways and deadends
			List<int> oneWayEntrances = new() { 47, 57, 63, 64, 68, 69, 70, 100, 101, 116, 117, 118, 119, 120, 131 };
			List<int> forcedEntrances = new() { 113 };


			linkSet.ForEach(x => x.OneWay = oneWayEntrances.Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForcePassage = forcedEntrances.Contains(x.Current.Entrance));

			foreach (var bigroom in bigRooms)
			{ 
				bigroom.Links.AddRange(linkSet.Where(l => bigroom.Rooms.Contains(l.Room)).ToList());
				bigroom.RoomValue = bigroom.Links.Count;
				if (bigroom.RoomValue <= 1)
				{
					bigroom.Deadend = true;
				}
			}

			// Sort initial rooms
			List<int> seedRooms = Rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();
			List<BigRoom> seedBigRooms = bigRooms.Where(x => x.Rooms.Intersect(seedRooms).Any()).ToList();
			bigRooms = bigRooms.Except(seedBigRooms).ToList();

			var icepyramid5f = bigRooms.Find(x => x.Rooms.Contains(66));
			icepyramid5f.RoomValue = 1;

			var progressBigRooms = bigRooms.Where(x => x.RoomValue > 1).ToList();
			var deadendBigRooms = bigRooms.Where(x => x.RoomValue == 1).ToList();

			
			newLinkToProcess = new();

			seedBigRooms.Shuffle(rng);
			seedBigRooms = seedBigRooms.Where(x => x.Links.Count > 0).ToList();

			// Distribute non deadends room; a big dungeon is 15-20 rooms
			while (progressBigRooms.Any())
			{
				var targetBigRoom = rng.PickFrom(seedBigRooms);
				var seedLink = rng.TakeFrom(targetBigRoom.Links);

				var connectRoom = rng.TakeFrom(progressBigRooms);
				var validConnectLink = connectRoom.Links.Where(x => !x.OneWay).ToList();
				var addonLink = rng.PickFrom(validConnectLink);
				connectRoom.Links.Remove(addonLink);
				
				ConnectLink(seedLink, addonLink);

				targetBigRoom.Merge(connectRoom);
			}
			
			// initial affectation
			foreach (var room in seedBigRooms)
			{
				if ((room.RoomValue % 2) == 1)
				{
					var connectRoom = rng.TakeFrom(deadendBigRooms);
					var seedLink = rng.PickFrom(room.Links.Where(x => !x.ForcePassage).ToList());
					room.Links.Remove(seedLink);
					var addonLink = rng.TakeFrom(connectRoom.Links);
					ConnectLink(seedLink, addonLink);

					room.Merge(connectRoom);
				}
			}

			if ((deadendBigRooms.Count % 2) == 1)
			{
				throw new Exception("We screwed up somewhere!");
			}

			// 
			while (deadendBigRooms.Any())
			{
				var unfilledBigRooms = seedBigRooms.Where(x => x.RoomValue > 0).ToList();

				var pickedRoom = rng.PickFrom(unfilledBigRooms);
				var deadends = new List<BigRoom>() { rng.TakeFrom(deadendBigRooms), rng.TakeFrom(deadendBigRooms) };

				foreach (var connectRoom in deadends)
				{
					var seedLink = rng.PickFrom(pickedRoom.Links.Where(x => !x.ForcePassage).ToList());
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
						throw new Exception("We screwed up somewhere again!");
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
	}
}
