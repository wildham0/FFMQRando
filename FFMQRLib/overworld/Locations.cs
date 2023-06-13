using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public partial class Overworld
	{
		public List<Location> Locations { get; set; }

		private List<byte> ShipWestSteps = new();
		private List<byte> ShipEastSteps = new();

		private const int OWMovementBank = 0x07;
		private const int OWMovementArrows = 0xEE84;
		private const int OWWalkStepsPointers = 0xF011;
		private const int OwNodesQty = 0x38;
		private const int NewOWWalkStepsBank = 0x12;
		private const int NewOWWalkStepsPointers = 0x9000;

		private const int OWLocationActionsOffset = 0xEFA1;
		private const int OWLocationQty = 0x38;

		private const int OWExitCoordBank = 0x07;
		private const int OWExitCoordOffset = 0xF7C3;

		private void CreateLocations(FFMQRom rom)
		{
			var movementArrows = rom.GetFromBank(OWMovementBank, OWMovementArrows, (OwNodesQty + 1) * 0x05).Chunk(5);
			ShipWestSteps = rom.GetFromBank(OWMovementBank, 0xF234, 6).ToBytes().ToList();
			ShipEastSteps = rom.GetFromBank(OWMovementBank, 0xF23A, 6).ToBytes().ToList();
			var exitCoords = rom.GetFromBank(OWExitCoordBank, OWExitCoordOffset, (OwNodesQty + 1) * 2).Chunk(2);

			Locations = new();
			foreach (var arrow in movementArrows)
			{
				Locations.Add(new Location(arrow));
			}

			var stepspointers = rom.GetFromBank(OWMovementBank, OWWalkStepsPointers, OwNodesQty * 2).Chunk(2);

			for (int i = 0; i < OwNodesQty; i++)
			{
				int currentposition = (int)stepspointers[i].ToUShorts()[0];
				for (int j = 0; j < 4; j++)
				{
					var destination = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];

					if (destination != 0)
					{
						currentposition++;
						var currentstep = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];
						List<byte> stepList = new();

						while (currentstep > 0x7F)
						{
							stepList.Add(currentstep);
							currentposition++;
							currentstep = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];
						}

						Locations[i + 1].AddDestination((LocationIds)destination, stepList);
					}
					else
					{
						Locations[i + 1].AddDestination((LocationIds)destination, new List<byte>());
						currentposition++;
					}
				}
			}

			for (int i = 0; i < Locations.Count; i++)
			{
				Locations[i].Position = (exitCoords[i][0], exitCoords[i][1]);
				Locations[i].LocationId = (LocationIds)i;
			}

			AssignOwObjects();
		}
		private void CreateLocations()
		{
			Locations = new();
			for(int i = 0; i < (OwNodesQty + 1); i++)
			{
				Locations.Add(new Location());
			}

			for (int i = 0; i < Locations.Count; i++)
			{
				Locations[i].LocationId = (LocationIds)i;
			}

			AssignOwObjects();
		}
		public void OpenNodes(Flags flags)
		{
			for (int i = 0; i <= (int)LocationIds.PazuzusTower; i++)
			{
				for (int y = 0; y < 4; y++)
				{
					if (Locations[i].DirectionFlags[y] != 0)
					{
						Locations[i].DirectionFlags[y] = (int)GameFlagsList.HillCollapsed;
					}
				}
			}

			Locations[(int)LocationIds.LibraBattlefield02].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.WakeWaterUsed;
			if (flags.MapShuffling != MapShufflingMode.None || flags.CrestShuffle)
			{
				Locations[(int)LocationIds.LibraBattlefield01].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.WakeWaterUsed;
			}
			Locations[(int)LocationIds.LibraTemple].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.WakeWaterUsed;
			Locations[(int)LocationIds.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.WakeWaterUsed;
			Locations[(int)LocationIds.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.West] = (int)GameFlagsList.VolcanoErupted;
			Locations[(int)LocationIds.Volcano].DirectionFlags[(int)NodeDirections.East] = (int)GameFlagsList.VolcanoErupted;

			Locations[(int)LocationIds.PazuzusTower].DirectionFlags[(int)NodeDirections.North] = 0xDC;
			Locations[(int)LocationIds.SpencersPlace].DirectionFlags[(int)NodeDirections.South] = 0xDC;
			Locations[(int)LocationIds.ShipDock].DirectionFlags[(int)NodeDirections.North] = 0x1A;
			Locations[(int)LocationIds.MacsShip].DirectionFlags[(int)NodeDirections.South] = 0x1A;
		}
		public void DoomCastleShortcut()
		{
			Locations[(int)LocationIds.FocusTowerForesta].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.HillCollapsed;
			Locations[(int)LocationIds.FocusTowerForesta].Destinations[(int)NodeDirections.South] = LocationIds.DoomCastle;
			Locations[(int)LocationIds.FocusTowerForesta].Steps[(int)NodeDirections.South] = new List<byte> { 0xC6 };

			Locations[(int)LocationIds.DoomCastle].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.HillCollapsed;
			Locations[(int)LocationIds.DoomCastle].Destinations[(int)NodeDirections.North] = LocationIds.FocusTowerForesta;
			Locations[(int)LocationIds.DoomCastle].Steps[(int)NodeDirections.North] = new List<byte> { 0x86 };
		}
		public void UpdateOverworld(Flags flags, GameLogic gamelogic, Battlefields battlefields)
		{
			List<Location> newLocations = new();

			var battlefieldLocations = gamelogic.Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(x => x.GameObjects).ToList();
			var dungeonLocations = gamelogic.Rooms.Where(r => r.Type == RoomType.Subregion).SelectMany(x => x.Links.Where(x => x.Entrance >= 0)).ToList();

			foreach (var battlefield in battlefieldLocations)
			{
				newLocations.Add(new Location(Locations.Find(x => x.LocationId == battlefield.Location), Locations.Find(x => x.LocationId == battlefield.LocationSlot)));
			}

			foreach (var dungeon in dungeonLocations)
			{
				newLocations.Add(new Location(Locations.Find(x => x.LocationId == dungeon.Location), Locations.Find(x => x.LocationId == dungeon.LocationSlot)));
			}

			newLocations.Add(new Location(Locations.Find(x => x.LocationId == LocationIds.HillOfDestiny)));
			newLocations.Add(new Location(Locations.Find(x => x.LocationId == LocationIds.None)));

			List<(LocationIds, LocationIds)> nodesToUpdate = battlefieldLocations.Select(x => (x.Location, x.LocationSlot)).ToList();
			nodesToUpdate.AddRange(dungeonLocations.Select(x => (x.Location, x.LocationSlot)).ToList());
			nodesToUpdate.Add((LocationIds.None, LocationIds.None));
			nodesToUpdate.Add((LocationIds.HillOfDestiny, LocationIds.HillOfDestiny));

			Locations = newLocations.OrderBy(x => x.LocationId).ToList();

			foreach (var node in Locations)
			{
				for (int i = 0; i < node.Destinations.Count; i++)
				{
					node.Destinations[i] = nodesToUpdate.Find(x => x.Item2 == node.Destinations[i]).Item1;
				}
			}

			StartingLocation = Locations.Find(x => x.Position == (0x0E, 0x28)).LocationId;
			UpdateBattlefieldsColor(flags, battlefields);
		}
		public void UpdateOwObjects()
		{
			foreach (var location in Locations)
			{
				foreach (var owobject in location.OwMapObjects)
				{
					owObjects[(int)owobject].Position = location.Position;
				}
			}
		}
		public void WriteLocations(FFMQRom rom)
		{
			// classData.SelectMany(x => x.MagicPermissions()).ToArray()
			rom.PutInBank(OWMovementBank, OWMovementArrows, Locations.SelectMany(x => x.DirectionFlagsArray()).ToArray());

			var currentpointer = 0x9000 + ((Locations.Count - 1) * 2);
			List<byte> pointertable = new();
			List<byte> stepvalues = new();

			for (int i = 1; i < Locations.Count; i++)
			{
				pointertable.AddRange(new List<byte>() { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) });
				stepvalues.AddRange(Locations[i].StepsArray());
				currentpointer += Locations[i].StepsArray().Length;
			}

			var shipwestpointer = new List<byte>() { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) };
			var shipeastpointer = new List<byte>() { (byte)((currentpointer + ShipWestSteps.Count) % 0x100), (byte)((currentpointer + ShipWestSteps.Count) / 0x100) };

			pointertable.AddRange(stepvalues);
			pointertable.AddRange(ShipWestSteps);
			pointertable.AddRange(ShipEastSteps);

			rom.PutInBank(NewOWWalkStepsBank, NewOWWalkStepsPointers, pointertable.ToArray());

			// new location for walk steps
			rom.PutInBank(0x01, 0xF148, Blob.FromHex("bf009012"));
			rom.PutInBank(0x01, 0xF150, Blob.FromHex("bf000012"));
			rom.PutInBank(0x01, 0xF532, Blob.FromHex("bf000012"));

			// new pointers for ship
			rom.PutInBank(0x01, 0xF5B4, shipwestpointer.ToArray());
			rom.PutInBank(0x01, 0xF5F4, shipeastpointer.ToArray());

			rom.PutInBank(OWExitCoordBank, OWExitCoordOffset, Locations.SelectMany(x => new List<byte> { (byte)x.Position.x, (byte)x.Position.y }).ToArray());
		}
	}
}
