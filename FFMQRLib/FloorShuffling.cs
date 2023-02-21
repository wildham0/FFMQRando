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
		public bool LogicalDeadEnd { get; set; }
		public bool ForceLinkDestination { get; set; }
		public bool ForceLinkOrigin { get; set; }
		public List<int> ValidSiblings{ get; set; }
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
			ForceLinkDestination = false;
			ForceLinkOrigin = false;
			LogicalDeadEnd = false;
			ValidSiblings = new();
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
		public BigRoom(List<int> rooms, List<FloorLink> links)
		{
			Rooms = rooms;
			Links = links.ToList();
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

			// Get links that can be shuffled
			var flatLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var linkSet = entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[0]).Id, flatLinks.Find(l => l.l.Entrance == e[0]).l, flatLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			linkSet.AddRange(entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[1]).Id, flatLinks.Find(l => l.l.Entrance == e[1]).l, flatLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());

			// Create list of big rooms
			List<BigRoom> bigRooms = new();
			List<int> nonShuffledEntrances = new() { 89, 90, 145, 148, 166, 168, 274, 278, 289, 290, 293, 294 };

			int maxId = 0;

			// Get rooms
			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0 || nonShuffledEntrances.Contains(x.Entrance)).Select(x => x.TargetRoom).Append(room.Id).ToList();

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
						/*
						if (!commonBigRooms[0].Rooms.Except(new List<int> { i }).ToList().Intersect(bigroom.Rooms).Any())
						{
							if (bigroom.Links.Any() && commonBigRooms[0].Links.Any())
							{
								var baseLink = rng.PickFrom(bigroom.Links);
								baseLink.LogicalDeadEnd = true;
								baseLink.ValidSiblings = commonBigRooms[0].Links.Select(x => x.Current.Entrance).ToList();
							}
						}
						else if (!bigroom.Rooms.Except(new List<int> { i }).ToList().Intersect(commonBigRooms[0].Rooms).Any())
						{
							if (bigroom.Links.Any())
							{
								var baseLink = rng.PickFrom(bigroom.Links);
								baseLink.LogicalDeadEnd = true;
							}
						}*/

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



			// Define oneways and deadends
			List<int> oneWayEntrances = new() { 47, 57, 63, 64, 68, 69, 70, 100, 101, 116, 117, 118, 119, 120, 131, 181, 182, 183, 215, 222, 251, 282, 284, 287, 289, 292, 293, 298, 300 };
			/*
			List<(int, List<int>)> forcedLinks = new()
			{
				//(47, new() { }), // Old man house
				(134, new() { }), // Ice Pyramid Loop
				(133, new() { 134 }),
				(141, new() { 113 }),
				(113, new() { }),
				(215, new() { 211, 212, 213, 214 }),
				(219, new() { 211, 212, 213, 214 }),
				(305, new() { 296, 297, 298 })

			};*/
			List<int> lavadomeLinks = new() { 211, 212, 213, 214 };
			List<int> giantree2fLinks = new() { 281, 283 };
			List<int> giantree3fLinks = new() { 281, 283 };
			List<int> giantree4fLinks = new() { 296, 297, 298 };
			List<int> pazuzu3fLinks = new() { 360, 361 };
			List<int> macShipB1Links = new() { 405, 406, 407 };
			List<int> macShipCorridorB1Links = new() { 412, 413 };
			List<(int, int)> forcedLinks = new()
			{
				//(47, new() { }), // Old man house
				(133, 134), // Ice Pyramid Loop
				(113, 137),
				(136, 141),
				(rng.TakeFrom(lavadomeLinks), 215),
				(rng.TakeFrom(lavadomeLinks), 219),
				(280, rng.TakeFrom(giantree2fLinks)),
				(282, 288),
				(289, 290),
				(284, 291),
				(rng.TakeFrom(giantree4fLinks), 305),
				(345, 346), // Pazuzu 1F shortcut
				(357, rng.TakeFrom(pazuzu3fLinks)), // Pazuzu 3F island
				(408, 418),
				(rng.TakeFrom(macShipB1Links), rng.TakeFrom(macShipCorridorB1Links)),

			};
			//List<int> forcedEntrances = new() { 113, 215 };
			//List<int> logicalDeadends = new() { 113, 134, 141, 215, 218 };


			linkSet.ForEach(x => x.OneWay = oneWayEntrances.Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForceLinkOrigin = forcedLinks.Select(x => x.Item1).ToList().Contains(x.Current.Entrance));
			linkSet.ForEach(x => x.ForceLinkDestination = forcedLinks.Select(x => x.Item2).ToList().Contains(x.Current.Entrance));
			foreach (var link in linkSet.Where(x => x.ForceLinkOrigin).ToList())
            {
				link.ValidSiblings = new List<int> { forcedLinks.Find(x => x.Item1 == link.Current.Entrance).Item2 };
			}
			//linkSet.ForEach(x => x.ForcePassage = forcedEntrances.Contains(x.Current.Entrance));
			//linkSet.ForEach(x => x.LogicalDeadEnd = logicalDeadends.Contains(x.Current.Entrance));

			/*
			foreach (var bigroom in bigRooms)
			{ 
				bigroom.Links.AddRange(linkSet.Where(l => bigroom.Rooms.Contains(l.Room)).ToList());
				bigroom.RoomValue = bigroom.Links.Count;
				
				if (bigroom.Links.Where(x => forcedEntrances.Contains(x.Current.Entrance)).Any())
				{
					bigroom.RoomValue -= 2;
				}

				if (bigroom.RoomValue <= 1)
				{
					bigroom.Deadend = true;
				}
			}*/

			// Sort initial rooms
			List<int> seedRooms = Rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();
			List<BigRoom> seedBigRooms = bigRooms.Where(x => x.Rooms.Intersect(seedRooms).Any()).ToList();
			bigRooms = bigRooms.Except(seedBigRooms).ToList();
			
			//bigRooms.ForEach()

			//var icepyramid5f = bigRooms.Find(x => x.Rooms.Contains(66));
			//icepyramid5f.RoomValue = 1;

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
				Console.WriteLine("Origin Link: " + originLink.Current.Entrance);
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
				else
				{
					givingRooms = progressBigRooms;
				}
				
				var givingRoom = rng.PickFrom(givingRooms);
				progressBigRooms.Remove(givingRoom);
				Console.WriteLine("Giving Room: " + givingRoom.Rooms[0]);
				var destinationLinks = givingRoom.Links.Where(x => !x.OneWay && !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList();
				var destinationLink = rng.PickFrom(destinationLinks);
				Console.WriteLine("Destination Link: " + destinationLink.Current.Entrance);
				givingRoom.Links.Remove(destinationLink);

				ConnectLink(originLink, destinationLink);

				if (originLink.ForceLinkOrigin)
				{
					Console.WriteLine("Is a forced");
					var newLinkHead = rng.PickFrom(givingRoom.Links.Where(x => !x.ForceLinkOrigin && !x.ForceLinkDestination).ToList());
					Console.WriteLine("LinkHead: " + newLinkHead.Current.Entrance);
					newLinkHead.ForceLinkOrigin = true;
					newLinkHead.ValidSiblings = originLink.ValidSiblings.ToList();
				}

				receivingRoom.Merge(givingRoom);
			}

			// logical dead ends
			foreach (var room in seedBigRooms)
			{
				var logicaldeadends = room.Links.Where(x => x.ForceLinkOrigin).ToList();
				logicaldeadends = logicaldeadends.OrderByDescending(x => x.ValidSiblings.Count).ToList();
				
				foreach (var deadend in logicaldeadends)
				{
					if (!room.Links.Contains(deadend))
					{
						continue;
					}

					var validLinks = room.Links.Where(x => deadend.ValidSiblings.Contains(x.Current.Entrance)).ToList();
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

			List<AccessReqs> crestAccess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			var crestRooms = Rooms.Where(x => x.Links.Where(l => l.Access.Intersect(crestAccess).Any()).Any()).Select(x => x.Id).ToList();

			// initial affectation
			foreach (var room in seedBigRooms)
			{
				if ((room.Links.Count % 2) == 1)
				{
					var validDeadEnds = deadendBigRooms;
					
					// MacShip protect
					if (room.Rooms.Contains(187))
					{
						validDeadEnds = deadendBigRooms.Where(x => !x.Rooms.Intersect(crestRooms).Any()).ToList();
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
				throw new Exception("Floor Shuffle: Deadends Count Error");
			}

			while (deadendBigRooms.Any())
			{
				var unfilledBigRooms = seedBigRooms.Where(x => x.Links.Count > 0).ToList();

				
				var deadends = new List<BigRoom>() { rng.TakeFrom(deadendBigRooms), rng.TakeFrom(deadendBigRooms) };
				
				if (deadends[0].Rooms.Intersect(crestRooms).Any() || deadends[1].Rooms.Intersect(crestRooms).Any())
				{
					unfilledBigRooms = unfilledBigRooms.Where(x => !x.Rooms.Contains(187)).ToList();
					if (!unfilledBigRooms.Any())
					{
						throw new Exception("Floor Shuffle: Mac Ship Crest Error");
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
						throw new Exception("Floor Shuffle: Gap Connection Error");
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
