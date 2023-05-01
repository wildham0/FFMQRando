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
	public enum GameObjectType : int
	{
		Chest = 1,
		Box,
		NPC,
		//Battlefield,
		BattlefieldItem,
		BattlefieldXp,
		BattlefieldGp,
		Trigger,
		Companion,
		Dummy
	}
    public enum RoomType : int
    {
		Overworld = 0,
		Subregion,
		Location,
		Dungeon
    }
    public enum MapShufflingMode
	{
		[Description("None")]
		None,
		[Description("Overworld")]
		Overworld,
		[Description("Dungeons")]
		Dungeons,
		[Description("Overworld+Dungeons")]
		OverworldDungeons,
		[Description("Everything")]
		Everything


	}
	public class GameObjectData
	{
		public int ObjectId { get; set; }
		public GameObjectType Type { get; set; }
		public List<AccessReqs> OnTrigger { get; set; }
		public List<AccessReqs> Access { get; set; }
		public LocationIds Location { get; set; }
        public LocationIds LocationSlot { get; set; }

        public string Name { get; set; }

		public GameObjectData()
		{
			ObjectId = 0;
			Type = GameObjectType.Dummy;
			OnTrigger = new();
			Access = new();
			Location = LocationIds.None;
            LocationSlot = LocationIds.None;
            Name = "None";
		}
        public GameObjectData(GameObjectData copyFrom)
        {
            ObjectId = copyFrom.ObjectId;
            Type = copyFrom.Type;
            OnTrigger = copyFrom.OnTrigger.ToList();
            Access = copyFrom.Access.ToList();
			Location = copyFrom.Location;
			LocationSlot = copyFrom.LocationSlot;
            Name = copyFrom.Name;
        }
    }
	public class GameObject : GameObjectData
	{
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }
		public Items Content { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public bool Accessible { get; set; }
		public bool Reset { get; set; }
		public GameObject()
		{
			Location = LocationIds.None;
			LocationSlot = LocationIds.None;
			//MapId = 0;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
			Content = (Items)0xFF;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = new();
			Accessible = false;
			Reset = false;
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
            Reset = false;
        }
		public GameObject(GameObjectData data, Location location, List<List<AccessReqs>> roomAccess)
		{
			Location = location.LocationId;
			//LocationSlot = location.LocationId;
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
            Reset = false;

            AccessRequirements = new();

			foreach (var access in roomAccess)
			{
				AccessRequirements.Add(access.Concat(data.Access).Distinct().ToList());
			}
		}
		public GameObject(GameObject gameobject)
		{
			Location = gameobject.Location;
			LocationSlot = gameobject.LocationSlot;
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
            Reset = gameobject.Reset;
        }
	}

	public class RoomLink
	{
		public int TargetRoom { get; set; }
		public int Entrance { get; set; }
		[YamlIgnore]
		public (int id, int type) Teleporter { get; set; }
		public List<AccessReqs> Access { get; set; }
        public LocationIds Location { get; set; }
        public LocationIds LocationSlot { get; set; }
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
			Location = LocationIds.None;
			LocationSlot = LocationIds.None;
		}
        public RoomLink(int target, List<AccessReqs> access)
        {
            TargetRoom = target;
            Entrance = -1;
			Access = access.ToList();
            Teleporter = (0, 0);
            Location = LocationIds.None;
            LocationSlot = LocationIds.None;
        }
        public RoomLink(int target, int entrance, (int, int) _teleporter, List<AccessReqs> access)
		{
			TargetRoom = target;
			Entrance = entrance;
			Teleporter = _teleporter;
			Access = access.ToList();
            Location = LocationIds.None;
            LocationSlot = LocationIds.None;
        }
        public RoomLink(int target, int entrance, (int, int) _teleporter, LocationIds location, List<AccessReqs> access)
        {
            TargetRoom = target;
            Entrance = entrance;
            Teleporter = _teleporter;
			Location = location;
            Access = access.ToList();
            LocationSlot = location;
        }
        public RoomLink(RoomLink newlink)
        {
            TargetRoom = newlink.TargetRoom;
            Entrance = newlink.Entrance;
            Teleporter = newlink.Teleporter;
            Location = newlink.Location;
			LocationSlot = newlink.LocationSlot;
            Access = newlink.Access.ToList();
        }
    }
	public class Room
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public List<GameObjectData> GameObjects { get; set; }
		public List<RoomLink> Links { get; set; }
		public RoomType Type { get; set; }
		public LocationIds Location { get; set; }
        public SubRegions Region { get; set; }
        public Room(string name, int id, int area, List<GameObjectData> objects, List<RoomLink> entrances)
		{
			Name = name;
			Id = id;
			GameObjects = objects; // shallowcopy?
			Links = entrances;
			Location = LocationIds.None;
			Region = SubRegions.Foresta;
            Type = RoomType.Dungeon;
		}
		public Room()
		{
			Name = "void";
			Id = 0;
			GameObjects = new();
			Links = new();
			Location = LocationIds.None;
            Region = SubRegions.Foresta;
            Type = RoomType.Dungeon;
		}
	}

	public partial class GameLogic
	{
		public List<Room> Rooms { get; set; }
		public List<GameObject> GameObjects { get; set; }
        private List<(int, (LocationIds, int), List<AccessReqs>)> accessQueue;
		private List<int> regionRoomIds;

		public GameLogic()
		{
			ReadRooms();
		}
        public GameLogic(string aprooms)
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

		public void CrawlRooms(Flags flags, Overworld overworld, Battlefields battlefields)
		{
			var locationsByEntrances = AccessReferences.LocationsByEntrances;

			// Initialization
			accessQueue = new();
			regionRoomIds = Rooms.Where(r => r.Type == RoomType.Subregion).Select(r => r.Id).ToList();

            var locationLinks = Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(r => r.Links).ToList();

			// Process Logic Access
			if (flags.LogicOptions != LogicOptions.Expert)
			{
				var volcanoBattlefieldRoom = Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.VolcanoBattlefield);
				volcanoBattlefieldRoom.Links.RemoveAll(l => l.Access.Contains(AccessReqs.SummerAquaria));

				var frozenFieldRoom = Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.AquariaFrozenField);
				frozenFieldRoom.Links.RemoveAll(l => l.Access.Contains(AccessReqs.DualheadHydra));
			}
			else
			{
                var exitTrickRoom = Rooms.Find(x => x.Id == 75);
                exitTrickRoom.Links.Add(new RoomLink(74, new() { AccessReqs.ExitBook }));
            }

			var giantTreeLink = locationLinks.Find(l => l.Location == LocationIds.GiantTree);
            Rooms.Find(x => x.Type == RoomType.Subregion && x.Region == SubRegions.Windia).Links.Remove(giantTreeLink);

			// Process Rooms
            ProcessRoom(0, new() { 0 }, new(), (LocationIds.None, 0));

			GameObjects = new();

			// Clean Up Access
            List<(int, (LocationIds, int), List<AccessReqs>)> accessToKeep = new();

            foreach (var room in Rooms)
			{
                var accessToCompare = accessQueue.Where(x => x.Item1 == room.Id).OrderBy(x => x.Item3.Count).ToList();
				List<(int, (LocationIds, int), List<AccessReqs>)> accessToRemove = new();

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

                foreach (var gamedata in room.GameObjects)
				{ 
					if (gamedata.Type == GameObjectType.BattlefieldXp || gamedata.Type == GameObjectType.BattlefieldGp)
					{
						continue;
					}
					else if (gamedata.Type == GameObjectType.BattlefieldItem)
					{
						var bflocation = overworld.Locations.Find(l => l.LocationId == gamedata.Location);
						GameObjects.Add(new GameObject(gamedata, bflocation, finalAccess));
					}
					else
					{
						GameObjects.Add(new GameObject(gamedata, targetLocation, finalAccess));
                    }
				}
			}
			
			// Add Friendly logic extra requirements
			if (flags.LogicOptions == LogicOptions.Friendly && (flags.MapShuffling == MapShufflingMode.None || flags.MapShuffling == MapShufflingMode.Overworld))
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

			// Prune duplicate reqs and update standard logic to excludwe Crest Teleporters
			foreach (var gameobject in GameObjects)
			{
				gameobject.AccessRequirements = gameobject.AccessRequirements.Select(x => x.Distinct().ToList()).ToList();
				var nonCrestAccesss = gameobject.AccessRequirements.Where(x => !x.Intersect(AccessReferences.CrestsAccess).Any()).ToList();

				if (flags.LogicOptions != LogicOptions.Expert && nonCrestAccesss.Any())
				{
					gameobject.AccessRequirements = nonCrestAccesss;
				}
			}

			// Special check for Ship Dock
			var shipdockobject = GameObjects.Find(x => x.OnTrigger.Contains(AccessReqs.ShipDockAccess));

			shipdockobject.AccessRequirements = shipdockobject.AccessRequirements.OrderBy(x => x.Count).Take(1).ToList();

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
			bool traverseCrest = false;

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
					if (children.Access.Contains(AccessReqs.LibraCrest) || children.Access.Contains(AccessReqs.GeminiCrest) || children.Access.Contains(AccessReqs.MobiusCrest))
					{
						traverseCrest = true;
					}
					
					ProcessRoom(children.TargetRoom, origins.Concat(new List<int> { roomid }).ToList(), access.Concat(children.Access).ToList(), (reachLocation ? newLocation : locPriority.Item1, traverseCrest ? locPriority.Item2 + 1 : locPriority.Item2));
				}
			}

            accessQueue.Add((roomid, locPriority, access));
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
						return Rooms.Find(x => x.Id == currentRoom).Location;
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

			foreach (var link in currentRoom.Links.Where(l => !visitedrooms.Contains(l.TargetRoom) && !l.Access.Intersect(AccessReferences.CrestsAccess).Any()))
			{
				ProcessRoomForCompanions(reqcount + link.Access.Count, link.TargetRoom, companionlist, visitedrooms);
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
	}
}
