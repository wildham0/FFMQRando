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
		public bool Deadend { get; set; }
		public int Room { get; set; }
		public RoomLink Current { get; set; }
		public RoomLink Origins { get; set; }
		//public (int id, int type) Origins { get; set; }

		public FloorLink(int room, RoomLink link, RoomLink origins)
		{
			//Entrance = entrance;
			Room = room;
			Current = link;
			Deadend = false;
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
		List<int> Rooms { get; set; }
		List<RoomLink> Links { get; set; }
		bool Deadend { get; set; }

	
	
	}
	public partial class GameLogic
    {
		private List<List<int>> entrancesPairs;
		
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
			List<List<int>> bigRooms = new();

			foreach (var room in Rooms)
			{
				var internalLinks = room.Links.Where(x => x.Entrance < 0).Select(x => x.TargetRoom).Append(room.Id).ToList();

				bool commonroom = false;

				foreach (var bigroom in bigRooms)
				{
					if (bigroom.Intersect(internalLinks).Any())
					{
						bigroom.AddRange(internalLinks);
						commonroom = true;
						break;
					}
				}

				if (!commonroom)
				{ 
					bigRooms.Add(internalLinks);
				}
			}

			var flatLinks = Rooms.SelectMany(r => r.Links.Where(l => l.Entrance >= 0).Select(l => (r.Id, l)).ToList()).ToList();
			var linkSet = entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[0]).Id, flatLinks.Find(l => l.l.Entrance == e[0]).l, flatLinks.Find(l => l.l.Entrance == e[1]).l)).ToList();
			linkSet.AddRange(entrancesPairs.Select(e => new FloorLink(flatLinks.Find(l => l.l.Entrance == e[1]).Id, flatLinks.Find(l => l.l.Entrance == e[1]).l, flatLinks.Find(l => l.l.Entrance == e[0]).l)).ToList());


			foreach (var link in linkSet)
			{
				var targetbigroom = bigRooms.Find(r => r.Contains(link.Room));

				var targetIsDeadend = Rooms.Where(r => targetbigroom.Contains(r.Id)).SelectMany(r => r.Links.Where(l => l.Entrance >= 0).ToList()).Count() > 1;

				link.Deadend = targetIsDeadend;
			}


			List<int> seedRooms = Rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();
			//List<>


			foreach (var room in seedRooms)
			{
				//var location = locationsByEntrances.Find(x => x.Item2 == Rooms.Find(x => x.Id == room).Links.Find(l => l.TargetRoom == 0).Entrance).Item1;
				//ProcessRoom(room, new List<int>(), new List<AccessReqs>(), (location, 0));
			}

			linkSet.Shuffle(rng);

			List<(int roomid, RoomLink link)> newLinkToProcess = new();

			while (linkSet.Any())
			{
				FloorLink link1;
				FloorLink link2;

				link1 = rng.TakeFrom(linkSet);
				link2 = rng.TakeFrom(linkSet);

				Rooms.Find(r => r.Id == link1.Room).Links.Remove(link1.Current);
				Rooms.Find(r => r.Id == link2.Room).Links.Remove(link2.Current);

				newLinkToProcess.Add((link1.Room, new RoomLink(link2.Room, link1.Current.Entrance, link2.Origins.Teleporter, link1.Current.Access)));
				newLinkToProcess.Add((link2.Room, new RoomLink(link1.Room, link2.Current.Entrance, link1.Origins.Teleporter, link2.Current.Access)));
			}

			foreach (var newlink in newLinkToProcess)
			{
				Rooms.Find(x => x.Id == newlink.roomid).Links.Add(newlink.link);
			}
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
