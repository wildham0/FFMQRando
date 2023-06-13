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
}
