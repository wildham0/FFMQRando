using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public class Location
	{
		public List<byte> DirectionFlags { get; set; }
		public byte NodeName { get; set; }
		public List<List<byte>> Steps { get; set; }
		public List<LocationIds> Destinations { get; set; }
		public List<OverworldMapObjects> OwMapObjects { get; set; }
		public (int x, int y) Position { get; set; }
		public (int id, int type) TargetTeleporter { get; set; }
		public LocationIds LocationId { get; set; }
		public LocationIds PreviousLocation { get; set; }
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }
		public Location(byte[] arrowvalues)
		{
			NodeName = arrowvalues[0];
			DirectionFlags = arrowvalues.ToList().GetRange(1, 4);
			Steps = new();
			Destinations = new();
			OwMapObjects = new();
			TargetTeleporter = (0, 0);
			Position = (0, 0);
			LocationId = LocationIds.None;
			PreviousLocation = LocationId;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
		}
		public Location()
		{
			NodeName = 0x00;
			DirectionFlags = new();
			Steps = new();
			Destinations = new();
			OwMapObjects = new();
			TargetTeleporter = (0, 0);
			Position = (0, 0);
			LocationId = LocationIds.None;
			PreviousLocation = LocationId;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
		}
		public Location(Location node)
		{
			NodeName = node.NodeName;
			DirectionFlags = node.DirectionFlags.ToList();
			Steps = node.Steps.ToList();
			Destinations = node.Destinations.ToList();
			OwMapObjects = node.OwMapObjects.ToList();
			TargetTeleporter = node.TargetTeleporter;
			Position = node.Position;
			LocationId = node.LocationId;
			PreviousLocation = node.PreviousLocation;
			Region = node.Region;
			SubRegion = node.SubRegion;
		}
		public void UpdateFromLocation(Location node)
		{
			NodeName = node.NodeName;
			OwMapObjects = node.OwMapObjects.ToList();
			TargetTeleporter = node.TargetTeleporter;
			LocationId = node.LocationId;
		}
		public Location(Location newlocation, Location position)
		{
			NodeName = newlocation.NodeName;
			OwMapObjects = newlocation.OwMapObjects.ToList();
			TargetTeleporter = newlocation.TargetTeleporter;
			LocationId = newlocation.LocationId;

			DirectionFlags = position.DirectionFlags.ToList();
			Steps = position.Steps.ToList();
			Destinations = position.Destinations.ToList();
			Position = position.Position;
			Region = position.Region;
			SubRegion = position.SubRegion;
		}
		public void AddDestination(LocationIds destination, List<byte> steps)
		{
			Destinations.Add(destination);
			Steps.Add(steps);
		}
		public byte[] DirectionFlagsArray()
		{ 
			return DirectionFlags.Prepend(NodeName).ToArray();
		}
		public byte[] StepsArray()
		{
			List<byte> finalarray = new();

			if (Destinations.Any())
			{
				for (int i = 0; i < 4; i++)
				{
					finalarray.AddRange(Steps[i].Prepend((byte)Destinations[i]));
				}
			}

			return finalarray.ToArray();
		}
	}
	public class SpecialRegion
	{
		public SubRegions SubRegion { get; set; }
		public AccessReqs Access { get; set; }
		public List<LocationIds> BarredLocations { get; set; }

		public SpecialRegion(SubRegions subregion, AccessReqs access)
		{
			SubRegion = subregion;
			Access = access;
			BarredLocations = new();
		}
	}
	public enum MovableLocationType
	{
		Battlefield = 0,
		Dungeon
	}
	public class MovableLocation
	{
		public LocationIds Origins { get; set; }
		public LocationIds Destination { get; set; }
		public SubRegions Region { get; set; }
		public RoomLink Link { get; set; }
		public GameObjectData Object { get; set; }
		public MovableLocationType Type { get; set; }
		public int Room { get; set; }
		public MovableLocation()
		{
			Origins = LocationIds.None;
			Destination = LocationIds.None;
			Link = new();
			Object = new();
			Type = MovableLocationType.Battlefield;
		}
		public MovableLocation(SubRegions region, LocationIds location, int room, RoomLink link)
		{
			Origins = location;
			Destination = location;
			Region = region;
			if (link != null)
			{
				Link = new RoomLink(link);
			}
			else
			{
				Link = null;
			}
			Object = new();
			Room = room;
			Type = MovableLocationType.Dungeon;
		}
		public MovableLocation(SubRegions region, LocationIds location, int room, GameObjectData objectdata)
		{
			Origins = location;
			Destination = location;
			Region = region;
			Object = new GameObjectData(objectdata);
			Link = new();
			Room = room;
			Type = MovableLocationType.Battlefield;
		}
	}
	public partial class OverworldObject
	{
		public (int x, int y) Position { get; set; }
		public List<List<int>> SpriteLayout;
		public (int x, int y) AnchorPosition;
		public OverworldObject((int x, int y) position, List<List<int>> layout, (int x, int y) anchor)
		{
			SpriteLayout = layout.ToList();
			Position = position;
			AnchorPosition = anchor;
		}
		public void UpdateCoordinates(List<OverworldSprite> owSprites)
		{
			for (int y = 0; y < SpriteLayout.Count; y++)
			{
				for (int x = 0; x < SpriteLayout[y].Count; x++)
				{
					int currentsprite = SpriteLayout[y][x];
					if (currentsprite > -1)
					{
						owSprites[currentsprite].X = (byte)(Position.x + x - AnchorPosition.x);
						owSprites[currentsprite].Y = (byte)(Position.y + y - AnchorPosition.y);
					}
				}
			}
		}
	}
	public class OverworldSprite
	{
		private byte[] _data = new byte[5];

		public OverworldSprite(byte[] data)
		{
			_data[0] = data[0]; // Y
			_data[1] = data[1]; // X
			_data[2] = data[2]; // Gameflag
			_data[3] = data[3]; // Sprite
			_data[4] = data[4]; // palette & flip  
		}
		public byte Palette
		{

			get => (byte)((_data[4] & 0b0000_1110) / 2);
			set => _data[4] = (byte)((_data[4] & 0b1111_0001) | ((value * 2) & 0b0000_1110));
		}
		public byte Sprite
		{
			get => _data[3];
			set => _data[3] = value;
		}
		public byte X
		{
			get => _data[1];
			set => _data[1] = value;
		}
		public byte Y
		{
			get => _data[0];
			set => _data[0] = value;
		}
		public byte[] Data
		{
			get => _data;
		}
	}
	public class NodeObject
	{
		public ushort LocationAction { get; set; }
		public List<int> MapObjects { get; set; }
		public int Id { get; set; }

		public NodeObject(int id, ushort action, List<int> mapobjects)
		{
			Id = id;
			LocationAction = action;
			MapObjects = mapobjects.ToList();
		}
	}
}
