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
	public class GameObject : GameObjectData
	{
		//public GameObjectData Data { get; set; }
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
			ObjectId = data.ObjectId;
			Type = data.Type;
			OnTrigger = data.OnTrigger.ToList();
			Access = data.Access.ToList();
			Name = data.Name;
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
			ObjectId = data.ObjectId;
			Type = data.Type;
			OnTrigger = data.OnTrigger.ToList();
			Access = data.Access.ToList();
			Name = data.Name;
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
		public GameObject(GameObject gameobject)
		{
			Location = gameobject.Location;
			ObjectId = gameobject.ObjectId;
			Type = gameobject.Type;
			OnTrigger = gameobject.OnTrigger.ToList();
			Access = gameobject.Access.ToList();
			Name = gameobject.Name;
			Region = gameobject.Region;
			SubRegion = gameobject.SubRegion;
			Content = gameobject.Content;
			IsPlaced = gameobject.IsPlaced;
			Prioritize = gameobject.Prioritize;
			Exclude = gameobject.Exclude;
			AccessRequirements = gameobject.AccessRequirements.ToList();
			Accessible = gameobject.Accessible;
		}
	}

	public class RoomLink
	{
		public int TargetRoom { get; set; }
		public int Entrance { get; set; }
		[YamlIgnore]
		public (int id, int type) Teleporter { get; set; }
		public List<AccessReqs> Access { get; set; }
		public List<int> teleporter {
			get => new() { Teleporter.id, Teleporter.type };
			set => Teleporter = (value[0], value[1]);
		}
		public RoomLink()
		{
			TargetRoom = 0;
			Entrance = -1;
			Access = new();
			Teleporter = (0, 0);
		}
		public RoomLink(int target, int entrance, (int, int) _teleporter, List<AccessReqs> access)
		{
			TargetRoom = target;
			Entrance = entrance;
			Teleporter = _teleporter;
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

	public partial class GameLogic
	{
		public List<Room> Rooms { get; set; }
		public List<GameObject> GameObjects { get; set; }
		private List<(int, LocationIds, List<AccessReqs>)> accessQueue;
		private List<(LocationIds, LocationIds, List<AccessReqs>)> bridgeQueue;
		private List<(int, int, LocationIds)> locationQueue;

		public GameLogic()
		{
			ReadRooms();
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

		public void CrawlRooms(Flags flags, Overworld overworld, Battlefields battlefields)
		{

			var locationsByEntrances = AccessReferences.LocationsByEntrances;

			//gameObjects = _rooms.SelectMany(x => x.GameObjects.Select(o => new GameObject(o)).ToList()).ToList();

			accessQueue = new();
			bridgeQueue = new();
			locationQueue = new();

			List<int> seedRooms = Rooms.Where(x => x.Links.Where(l => l.TargetRoom == 0).Any()).Select(x => x.Id).ToList();

			foreach (var room in seedRooms)
			{
				var location = locationsByEntrances.Find(x => x.Item2 == Rooms.Find(x => x.Id == room).Links.Find(l => l.TargetRoom == 0).Entrance).Item1;
				ProcessRoom(room, new List<int>(), new List<AccessReqs>(), (location, 0));
			}

			var finalQueue = accessQueue.Select(x => (x.Item1, x.Item2, x.Item3.Distinct().ToList())).Distinct().ToList();

			GameObjects = new();
			List<(SubRegions, List<AccessReqs>)> subRegionsAccess = new();


			foreach (var bridge in bridgeQueue)
			{
				var subRegionA = AccessReferences.MapSubRegions.Find(x => x.Item2 == bridge.Item1).Item1;
				var subRegionB = AccessReferences.MapSubRegions.Find(x => x.Item2 == bridge.Item2).Item1;

				if (subRegionA != subRegionB)
				{
					var originSubRegionAccess = AccessReferences.SubRegionsAccess.Find(x => x.Item1 == subRegionA).Item2;

					foreach (var access in originSubRegionAccess)
					{
						subRegionsAccess.Add((subRegionB, bridge.Item3.Concat(access).ToList()));
					}
				}
			}
			var hardsubaccess = AccessReferences.SubRegionsAccess.SelectMany(x => x.Item2.Select(s => (x.Item1, s))).ToList();
			subRegionsAccess = subRegionsAccess.Concat(hardsubaccess).ToList();

			subRegionsAccess = subRegionsAccess.Where(x => !x.Item2.Contains(AccessReqs.Barred)).ToList();

			// Add Sealed Temple/Exit book trick
			if (flags.LogicOptions == LogicOptions.Expert && !(flags.OverworldShuffle || flags.CrestShuffle))
			{
				List<AccessReqs> sealedTempleExit = new() { AccessReqs.RiverCoin, AccessReqs.ExitBook, AccessReqs.GeminiCrest };
				subRegionsAccess.Add((SubRegions.Aquaria, sealedTempleExit));
				subRegionsAccess.Add((SubRegions.AquariaFrozenField, sealedTempleExit));
			}

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

			foreach (var room in Rooms)
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

						GameObjects.Add(new GameObject(gamedata, bflocation, finalAccess));
						battlefieldCount++;
					}
				}
				else
				{

					actualLocation = locationQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item2).ToList().First().Item3;

					Location targetLocation = overworld.Locations.Find(x => x.LocationId == actualLocation);

					foreach (var gamedata in room.GameObjects)
					{
						var targetaccess = finalQueue.Where(x => x.Item1 == room.Id).ToList();
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

						GameObjects.Add(new GameObject(gamedata, targetLocation, finalAccess));
					}
				}
			}
			/*
						foreach (var gamedata in gameObjects)
						{ 
							Console.WriteLine(gamedata.Data.Name)
						}
			*/
			
			// Add Friendly logic extra requirements
			if (flags.LogicOptions == LogicOptions.Friendly)
			{
				foreach (var location in AccessReferences.FriendlyAccessReqs)
				{
					GameObjects.Where(x => x.Location == location.Key).ToList().ForEach(x => x.AccessRequirements.ForEach(a => a.AddRange(location.Value)));
				}
			}

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

			// Clean Up requirements
			List<AccessReqs> crestsList = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };

			foreach (var gameobject in GameObjects)
			{
				gameobject.AccessRequirements = gameobject.AccessRequirements.Select(x => x.Distinct().ToList()).ToList();
				gameobject.AccessRequirements = gameobject.AccessRequirements.OrderBy(x => x.Count + (x.Intersect(crestsList).ToList().Count * 2)).ToList(); // Add crest tax
			}

			// Expert Mode check
			if (flags.LogicOptions != LogicOptions.Expert)
			{
				foreach (var gameobject in GameObjects)
				{
					gameobject.AccessRequirements = gameobject.AccessRequirements.Where((x, i) => i == 0).ToList();
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
				GameObjects.Where(x => x.Type == GameObjectType.Battlefield).ToList().ForEach(x => x.Prioritize = true);
			}
			else if (flags.BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Exclude)
			{
				GameObjects.Where(x => x.Type == GameObjectType.Battlefield).ToList().ForEach(x => x.Exclude = true);
			}

			if (flags.BoxesShuffle == ItemShuffleBoxes.Exclude)
			{
				GameObjects.Where(x => x.Type == GameObjectType.Box).ToList().ForEach(x => x.Exclude = true);
			}

			// Exclude Hero Statue room's chests
			GameObjects.Where(x => x.Type == GameObjectType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList().ForEach(x => x.Exclude = true);
		}

		private void ProcessRoom(int roomid, List<int> origins, List<AccessReqs> access, (LocationIds, int) locPriority)
		{ 
			var targetroom = Rooms.Find(x => x.Id == roomid);
			bool traverseCrest = false;

			foreach (var children in targetroom.Links)
			{
				if (children.TargetRoom == 0)
				{
					if (origins.Count > 0)
					{
						bridgeQueue.Add((locPriority.Item1, AccessReferences.LocationsByEntrances.Find(x => x.Item2 == children.Entrance).Item1, access));
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
