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
		private List<(int, List<AccessReqs>)> accessQueue;
		private List<(int, int, LocationIds)> locationQueue;

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

		public void CrawlRooms(Overworld overworld)
		{

			List<(LocationIds, (int id, int type))> locations = new()
			{
				(LocationIds.LevelForest, (25, 0)),
				(LocationIds.Foresta, (31, 0)),
				(LocationIds.SandTemple, (36, 0)),
				(LocationIds.BoneDungeon, (37, 0)),
				(LocationIds.FocusTowerForesta, (2, 6)),
				(LocationIds.FocusTowerAquaria, (4, 6)),
				(LocationIds.LibraTemple, (13, 6)),
				(LocationIds.LifeTemple, (14, 6)),
				(LocationIds.Aquaria, (8, 6)),
				(LocationIds.WintryCave, (49, 0)),
				(LocationIds.FallsBasin, (53, 0)),
				(LocationIds.IcePyramid, (56, 0)),
				(LocationIds.SpencersPlace, (7, 6)),
				(LocationIds.WintryTemple, (15, 6)),
				(LocationIds.FocusTowerFrozen, (5, 6)),
				(LocationIds.FocusTowerFireburg, (6, 6)),
				(LocationIds.Fireburg, (9, 6)),
				(LocationIds.SealedTemple, (16, 6)),
				(LocationIds.Mine, (98, 0)),
				(LocationIds.Volcano, (103, 0)),
				(LocationIds.LavaDome, (104, 0)),
				(LocationIds.FocusTowerWindia, (3, 6)),
				(LocationIds.RopeBridge, (140, 0)),
				(LocationIds.AliveForest, (142, 0)),
				(LocationIds.KaidgeTemple, (18, 6)),
				(LocationIds.WindholeTemple, (173, 0)),
				(LocationIds.MountGale, (174, 0)),
				(LocationIds.Windia, (10, 6)),
				(LocationIds.PazuzusTower, (184, 0)),
				(LocationIds.LightTemple, (19, 6)),
				(LocationIds.ShipDock, (17, 6)),
				(LocationIds.MacsShip, (37, 8)),
			};
			
			
			//gameObjects = _rooms.SelectMany(x => x.GameObjects.Select(o => new GameObject(o)).ToList()).ToList();

			accessQueue = new();
			locationQueue = new();

			List<int> seedRooms = _rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();

			foreach (var room in seedRooms)
			{
				var location = locations.Find(x => x.Item2 == _rooms.Find(x => x.Id == room).Links.Find(l => l.TargetRoom == 0).Entrance).Item1;
				ProcessRoom(room, new List<int>(), new List<AccessReqs>(), (location, 0));
			}

			var finalQueue = accessQueue.Select(x => (x.Item1, x.Item2.Distinct().ToList())).Distinct().ToList();

			gameObjects = new();

			foreach (var room in _rooms)
			{
				var actualLocation = LocationIds.None;

				if (room.Id != 0)
				{
					actualLocation = locationQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item2).ToList().First().Item3;
				}

				Location targetLocation = overworld.Locations.Find(x => x.LocationId == actualLocation);

				foreach (var gamedata in room.GameObjects)
				{
					gameObjects.Add(new GameObject(gamedata, targetLocation, finalQueue.Where(x => x.Item1 == room.Id).Select(x => x.Item2).ToList()));
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
						// update subregion
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
			accessQueue.Add((roomid, access));
		}

	}
}
