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
	public enum GameObjectType : int
	{
		Chest = 1,
		Box,
		NPC,
		Battlefield,
		Trigger,
		Companion,
		Dummy

	}
	public class GameObjectData
	{
		public int ObjectId { get; set; }
		public GameObjectType Type { get; set; }
		public List<AccessReqs> OnTrigger { get; set; }
		public List<AccessReqs> Access { get; set; }
		public string Name { get; set; }

		public GameObjectData()
		{
			ObjectId = 0;
			Type = GameObjectType.Dummy;
			OnTrigger = new();
			Access = new();
			Name = "None";
		}
	}
	public class GameObject
	{
		public GameObjectData Data { get; set; }
		public LocationIds Location { get; set; }
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }
		public Items Content { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public bool Accessible { get; set; }
		public GameObject()
		{
			Location = LocationIds.None;
			//MapId = 0;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
			Content = (Items)0xFF;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = new();
			Accessible = false;
		}

		public GameObject(GameObjectData data)
		{
			Location = LocationIds.None;
			Data = data;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
			Content = (Items)0xFF;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = new();
			Accessible = false;
		}
		public GameObject(GameObjectData data, Location location, List<List<AccessReqs>> roomAccess)
		{
			Location = location.LocationId;
			Data = data;
			Region = location.Region;
			SubRegion = location.SubRegion;
			Content = (Items)0xFF;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			Accessible = false;

			AccessRequirements = new();

			foreach (var access in roomAccess)
			{
				AccessRequirements.Add(access.Concat(data.Access).Distinct().ToList());
			}
		}
	}

	public class RoomLink
	{
		public int TargetRoom { get; set; }
		public List<int> EntranceId { get; set; }
		public List<AccessReqs> Access { get; set; }
		public (int id, int type) Entrance {
			get => (EntranceId[0], EntranceId[1]);
			set => EntranceId = new() { value.id, value.type };
		}
		public RoomLink()
		{
			TargetRoom = 0;
			EntranceId = new() { 255, 255 };
			Access = new();
		}
		public RoomLink(int target, (int, int) _entrance, List<AccessReqs> access)
		{
			TargetRoom = target;
			EntranceId = new() { _entrance.Item1, _entrance.Item2 };
			Access = access.ToList();
		}
	}

	public class Room
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public List<GameObjectData> GameObjects { get; set; }
		public List<RoomLink> Links { get; set; }
		public Room(string name, int id, int area, List<GameObjectData> objects, List<RoomLink> entrances)
		{
			Name = name;
			Id = id;
			GameObjects = objects; // shallowcopy?
			Links = entrances;
		}
		public Room()
		{
			Name = "void";
			Id = 0;
			GameObjects = new();
			Links = new();
		}
	}

	public class Rooms
	{
		private List<Room> _rooms { get; set; }
		private List<GameObject> gameObjects { get; set; }
		private List<(int, LocationIds, List<AccessReqs>)> accessQueue;
		private List<(LocationIds, LocationIds, List<AccessReqs>)> bridgeQueue;
		private List<(int, int, LocationIds)> locationQueue;

		public List<Room> List { get => _rooms; set => _rooms = value; }

		public Room this[int index]
		{
			get => _rooms[index];
			set => _rooms[index] = value;
		}


		public void ReadRooms()
		{

			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			//string filepath = "logic.yaml";
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("rooms.yaml"));
			using (Stream logicfile = assembly.GetManifestResourceStream(filepath))
			{
				using (StreamReader reader = new StreamReader(logicfile))
				{
					yamlfile = reader.ReadToEnd();
				}
			}


			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
				.Build();

			var input = new StringReader(yamlfile);

			var yaml = new YamlStream();

			/*
			try
			{
				yaml.Load(input);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}*/
			List<Room> result = new();

			try
			{
				result = deserializer.Deserialize<List<Room>>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			_rooms = result;
			yamlfile = "";
		}

		public void CrawlRooms(Overworld overworld, Battlefields battlefields)
		{

			var locationsByEntrances = ItemLocations.LocationsByEntrances;

			//gameObjects = _rooms.SelectMany(x => x.GameObjects.Select(o => new GameObject(o)).ToList()).ToList();

			accessQueue = new();
			bridgeQueue = new();
			locationQueue = new();

			List<int> seedRooms = _rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();

			foreach (var room in seedRooms)
			{
				var location = locationsByEntrances.Find(x => x.Item2 == _rooms.Find(x => x.Id == room).Links.Find(l => l.TargetRoom == 0).Entrance).Item1;
				ProcessRoom(room, new List<int>(), new List<AccessReqs>(), (location, 0));
			}

			var finalQueue = accessQueue.Select(x => (x.Item1, x.Item2, x.Item3.Distinct().ToList())).Distinct().ToList();

			gameObjects = new();
			List<(SubRegions, List<AccessReqs>)> subRegionsAccess = new();


			foreach (var bridge in bridgeQueue)
			{
				var subRegionA = ItemLocations.MapSubRegions.Find(x => x.Item2 == bridge.Item1).Item1;
				var subRegionB = ItemLocations.MapSubRegions.Find(x => x.Item2 == bridge.Item2).Item1;

				if (subRegionA != subRegionB)
				{
					var originSubRegionAccess = ItemLocations.SubRegionsAccess.Find(x => x.Item1 == subRegionA).Item2;

					foreach (var access in originSubRegionAccess)
					{
						subRegionsAccess.Add((subRegionB, bridge.Item3.Concat(access).ToList()));
					}
				}
			}
			var hardsubaccess = ItemLocations.SubRegionsAccess.SelectMany(x => x.Item2.Select(s => (x.Item1, s))).ToList();
			subRegionsAccess = subRegionsAccess.Concat(hardsubaccess).ToList();

			subRegionsAccess = subRegionsAccess.Where(x => !x.Item2.Contains(AccessReqs.Barred)).ToList();

			List<int> subRegionAccessToRemove = new();

			List<SubRegions> subRegions = Enum.GetValues<SubRegions>().ToList();
			List<(SubRegions, List<AccessReqs>)> sbgRegionAccessKeep = new();

			foreach (var subregion in subRegions)
			{
				//var targetAccesses = subRegionsAccess.Where(x => x.Item1 == subregion).ToList();

				for (int i = 0; i < subRegionsAccess.Count; i++)
				{
					if (subRegionAccessToRemove.Contains(i))
					{
						continue;
					}

					for (int j = 0; j < subRegionsAccess.Count; j++)
					{
						if (i == j || subRegionsAccess[i].Item1 != subRegionsAccess[j].Item1)
						{
							continue;
						}

						if (!subRegionsAccess[i].Item2.Except(subRegionsAccess[j].Item2).Any())
						{
							subRegionAccessToRemove.Add(j);
						}
					}
				}
			}

			subRegionsAccess = subRegionsAccess.Where((x, i) => !subRegionAccessToRemove.Contains(i)).ToList();

			foreach (var room in _rooms)
			{
				var actualLocation = LocationIds.None;

				if (room.Id == 0)
				{
					int battlefieldCount = 0;

					foreach (var gamedata in room.GameObjects)
					{
						var bflocation = overworld.Locations.Find(l => l.LocationId == battlefields.BattlefieldsWithItem[battlefieldCount]);

						//var targetaccess = finalQueue.Where(x => x.Item1 == room.Id);
						List<List<AccessReqs>> finalAccess = new();
						var locReq = subRegionsAccess.Where(x => x.Item1 == overworld.Locations.Find(l => l.LocationId == bflocation.LocationId).SubRegion).Select(x => x.Item2).ToList();
						foreach (var locAccess in locReq)
						{
							finalAccess.Add(locAccess);
						}

						gameObjects.Add(new GameObject(gamedata, bflocation, finalAccess));
						battlefieldCount++;
					}
				}
				else
				{

					actualLocation = locationQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item2).ToList().First().Item3;

					Location targetLocation = overworld.Locations.Find(x => x.LocationId == actualLocation);

					foreach (var gamedata in room.GameObjects)
					{
						var targetaccess = finalQueue.Where(x => x.Item1 == room.Id);
						List<List<AccessReqs>> finalAccess = new();
						foreach (var access in targetaccess)
						{
							var tsubregion = overworld.Locations.Find(l => l.LocationId == access.Item2).SubRegion;
							var tsubAccess = subRegionsAccess.Where(x => x.Item1 == tsubregion).ToList();
							if (tsubAccess.Any())
							{
								var locReq = tsubAccess.Select(x => x.Item2).ToList();
								foreach (var locAccess in locReq)
								{
									finalAccess.Add(locAccess.Concat(access.Item3).ToList());
								}
							}
							else
							{

								var test2 = 0;

							}

							/*
							var locReq = subRegionsAccess.Where(x => x.Item1 == overworld.Locations.Find(l => l.LocationId == access.Item2).SubRegion).Select(x => x.Item2).ToList();
							foreach (var locAccess in locReq)
							{
								finalAccess.Add(locAccess.Concat(access.Item3).ToList());
							}*/
						}

						gameObjects.Add(new GameObject(gamedata, targetLocation, finalAccess));
					}
				}
			}
/*
			foreach (var gamedata in gameObjects)
			{ 
				Console.WriteLine(gamedata.Data.Name)
			}
*/
			var test = 0;
		}

		private void ProcessRoom(int roomid, List<int> origins, List<AccessReqs> access, (LocationIds, int) locPriority)
		{ 
			var targetroom = _rooms.Find(x => x.Id == roomid);
			bool traverseCrest = false;

			foreach (var children in targetroom.Links)
			{
				if (children.TargetRoom == 0)
				{
					if (origins.Count > 0)
					{
						bridgeQueue.Add((locPriority.Item1, ItemLocations.LocationsByEntrances.Find(x => x.Item2 == children.Entrance).Item1, access));
					}
				}
				else if (!origins.Contains(children.TargetRoom))
				{
					if (children.Access.Contains(AccessReqs.LibraCrest) || children.Access.Contains(AccessReqs.GeminiCrest) || children.Access.Contains(AccessReqs.MobiusCrest))
					{
						traverseCrest = true;
					}
					
					ProcessRoom(children.TargetRoom, origins.Concat(new List<int> { roomid }).ToList(), access.Concat(children.Access).ToList(), (locPriority.Item1, traverseCrest ? locPriority.Item2 + 1 : locPriority.Item2));
				}
			}

			locationQueue.Add((roomid, locPriority.Item2, locPriority.Item1));
			accessQueue.Add((roomid, locPriority.Item1, access));
		}

	}
}
