using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{

	public class GameFlags
	{
		private const int StartingGameFlags = 0x0653A4;
		private byte[] _gameflags;
		private List<(int, int)> BitPos = new List<(int,int)> { (0,0x80), (1,0x40), (2,0x20), (3,0x10), (4,0x08), (5,0x04), (6,0x02), (7,0x01) };

		public GameFlags(FFMQRom rom)
		{
			_gameflags = rom.Get(StartingGameFlags, 0x20);

		}
		public void Write(FFMQRom rom)
		{
			rom.Put(StartingGameFlags, _gameflags);
		}
		public bool this[int flag]
		{
			get => HexToFlag(flag);
			set => FlagToHex(flag, value);
		}

		private bool HexToFlag(int flag)
		{
			var targetbyte = flag / 8;
			var targetbit = BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			return (_gameflags[targetbyte] & targetbit) == targetbit;
		}

		private void FlagToHex(int flag, bool value)
		{
			CustomFlagToHex(_gameflags, flag, value);
		}

		public void CustomFlagToHex(byte[] flagarray, int flag, bool value)
		{
			var targetbyte = flag / 8;
			var targetbit = (byte)BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			if (value)
			{
				flagarray[targetbyte] |= targetbit;
			}
			else
			{
				flagarray[targetbyte] &= (byte)~targetbit;
			}
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
        private void AssignOwObjects()
		{
			Locations[(int)LocationIds.ForestaSouthBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaSouthBattlefield, OverworldMapObjects.ForestaSouthBattlefieldCleared };
			Locations[(int)LocationIds.ForestaSouthBattlefield].TargetTeleporter = (115, 1);

			Locations[(int)LocationIds.ForestaWestBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaWestBattlefield, OverworldMapObjects.ForestaWestBattlefieldCleared };
			Locations[(int)LocationIds.ForestaWestBattlefield].TargetTeleporter = (116, 1);

			Locations[(int)LocationIds.ForestaEastBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaEastBattlefield, OverworldMapObjects.ForestaEastBattlefieldCleared };
			Locations[(int)LocationIds.ForestaEastBattlefield].TargetTeleporter = (117, 1);

			Locations[(int)LocationIds.AquariaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield01, OverworldMapObjects.AquariaBattlefield01Cleared };
			Locations[(int)LocationIds.AquariaBattlefield01].TargetTeleporter = (118, 1);

			Locations[(int)LocationIds.AquariaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield02, OverworldMapObjects.AquariaBattlefield02Cleared };
			Locations[(int)LocationIds.AquariaBattlefield02].TargetTeleporter = (119, 1);

			Locations[(int)LocationIds.AquariaBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield03, OverworldMapObjects.AquariaBattlefield03Cleared };
			Locations[(int)LocationIds.AquariaBattlefield03].TargetTeleporter = (120, 1);

			Locations[(int)LocationIds.WintryBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield01, OverworldMapObjects.WintryBattlefield01Cleared };
			Locations[(int)LocationIds.WintryBattlefield01].TargetTeleporter = (121, 1);

			Locations[(int)LocationIds.WintryBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield02, OverworldMapObjects.WintryBattlefield02Cleared };
			Locations[(int)LocationIds.WintryBattlefield02].TargetTeleporter = (122, 1);

			Locations[(int)LocationIds.PyramidBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PyramidBattlefield01, OverworldMapObjects.PyramidBattlefield01Cleared };
			Locations[(int)LocationIds.PyramidBattlefield01].TargetTeleporter = (123, 1);

			Locations[(int)LocationIds.LibraBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield01, OverworldMapObjects.LibraBattlefield01Cleared };
			Locations[(int)LocationIds.LibraBattlefield01].TargetTeleporter = (124, 1);

			Locations[(int)LocationIds.LibraBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield02, OverworldMapObjects.LibraBattlefield02Cleared };
			Locations[(int)LocationIds.LibraBattlefield02].TargetTeleporter = (125, 1);

			Locations[(int)LocationIds.FireburgBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield01, OverworldMapObjects.FireburgBattlefield01Cleared };
			Locations[(int)LocationIds.FireburgBattlefield01].TargetTeleporter = (126, 1);

			Locations[(int)LocationIds.FireburgBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield02, OverworldMapObjects.FireburgBattlefield02Cleared };
			Locations[(int)LocationIds.FireburgBattlefield02].TargetTeleporter = (127, 1);

			Locations[(int)LocationIds.FireburgBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield03, OverworldMapObjects.FireburgBattlefield03Cleared };
			Locations[(int)LocationIds.FireburgBattlefield03].TargetTeleporter = (128, 1);

			Locations[(int)LocationIds.MineBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield01, OverworldMapObjects.MineBattlefield01Cleared };
			Locations[(int)LocationIds.MineBattlefield01].TargetTeleporter = (129, 1);

			Locations[(int)LocationIds.MineBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield02, OverworldMapObjects.MineBattlefield02Cleared };
			Locations[(int)LocationIds.MineBattlefield02].TargetTeleporter = (130, 1);

			Locations[(int)LocationIds.MineBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield03, OverworldMapObjects.MineBattlefield03Cleared };
			Locations[(int)LocationIds.MineBattlefield03].TargetTeleporter = (131, 1);

			Locations[(int)LocationIds.VolcanoBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoBattlefield01, OverworldMapObjects.VolcanoBattlefield01Cleared };
			Locations[(int)LocationIds.VolcanoBattlefield01].TargetTeleporter = (132, 1);

			Locations[(int)LocationIds.WindiaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield01, OverworldMapObjects.WindiaBattlefield01Cleared };
			Locations[(int)LocationIds.WindiaBattlefield01].TargetTeleporter = (133, 1);

			Locations[(int)LocationIds.WindiaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield02, OverworldMapObjects.WindiaBattlefield02Cleared };
			Locations[(int)LocationIds.WindiaBattlefield02].TargetTeleporter = (134, 1);

			Locations[(int)LocationIds.HillOfDestiny].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.HillOfDestiny };

			Locations[(int)LocationIds.LevelForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LevelForestMarker };
			Locations[(int)LocationIds.LevelForest].TargetTeleporter = (25, 0);

			Locations[(int)LocationIds.Foresta].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaVillage };
			Locations[(int)LocationIds.Foresta].TargetTeleporter = (31, 0);

			Locations[(int)LocationIds.SandTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SandTempleCave };
			Locations[(int)LocationIds.SandTemple].TargetTeleporter = (36, 0);

			Locations[(int)LocationIds.BoneDungeon].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.BoneDungeon };
			Locations[(int)LocationIds.BoneDungeon].TargetTeleporter = (37, 0);

			Locations[(int)LocationIds.FocusTowerForesta].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerForesta].TargetTeleporter = (2, 6);

			Locations[(int)LocationIds.FocusTowerAquaria].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerAquaria].TargetTeleporter = (4, 6);

			Locations[(int)LocationIds.LibraTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraTempleCave };
			Locations[(int)LocationIds.LibraTemple].TargetTeleporter = (13, 6);

			Locations[(int)LocationIds.Aquaria].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaVillage };
			Locations[(int)LocationIds.Aquaria].TargetTeleporter = (8, 6);

			Locations[(int)LocationIds.WintryCave].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryCave };
			Locations[(int)LocationIds.WintryCave].TargetTeleporter = (49, 0);

			Locations[(int)LocationIds.LifeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LifeTempleCave };
			Locations[(int)LocationIds.LifeTemple].TargetTeleporter = (14, 6);

			Locations[(int)LocationIds.FallsBasin].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FallBasinMarker };
			Locations[(int)LocationIds.FallsBasin].TargetTeleporter = (53, 0);

			Locations[(int)LocationIds.IcePyramid].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.IcePyramid };
			Locations[(int)LocationIds.IcePyramid].TargetTeleporter = (56, 0);

			Locations[(int)LocationIds.SpencersPlace].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SpencerCave };
			Locations[(int)LocationIds.SpencersPlace].TargetTeleporter = (7, 6);

			Locations[(int)LocationIds.WintryTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryTempleCave };
			Locations[(int)LocationIds.WintryTemple].TargetTeleporter = (15, 6);

			Locations[(int)LocationIds.FocusTowerFrozen].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerFrozen].TargetTeleporter = (5, 6);

			Locations[(int)LocationIds.FocusTowerFireburg].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerFireburg].TargetTeleporter = (6, 6);

			Locations[(int)LocationIds.Fireburg].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgVillage };
			Locations[(int)LocationIds.Fireburg].TargetTeleporter = (9, 6);

			Locations[(int)LocationIds.Mine].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.Mine };
			Locations[(int)LocationIds.Mine].TargetTeleporter = (98, 0);

			Locations[(int)LocationIds.SealedTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SealedTempleCave };
			Locations[(int)LocationIds.SealedTemple].TargetTeleporter = (16, 6);

			Locations[(int)LocationIds.Volcano].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoMarker };
			Locations[(int)LocationIds.Volcano].TargetTeleporter = (103, 0);

			Locations[(int)LocationIds.LavaDome].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LavaDome };
			Locations[(int)LocationIds.LavaDome].TargetTeleporter = (104, 0);

			Locations[(int)LocationIds.FocusTowerWindia].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerWindia].TargetTeleporter = (3, 6);

			Locations[(int)LocationIds.RopeBridge].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.RopeBridgeMarker };
			Locations[(int)LocationIds.RopeBridge].TargetTeleporter = (140, 0);

			Locations[(int)LocationIds.AliveForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AliveForestMarker };
			Locations[(int)LocationIds.AliveForest].TargetTeleporter = (142, 0);

			Locations[(int)LocationIds.GiantTree].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MovedGiantTree };
			Locations[(int)LocationIds.GiantTree].TargetTeleporter = (49, 8);

			Locations[(int)LocationIds.KaidgeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.KaidgeTempleCave };
			Locations[(int)LocationIds.KaidgeTemple].TargetTeleporter = (18, 6);

			Locations[(int)LocationIds.Windia].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaVillage };
			Locations[(int)LocationIds.Windia].TargetTeleporter = (10, 6);

			Locations[(int)LocationIds.WindholeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindholeTempleCave };
			Locations[(int)LocationIds.WindholeTemple].TargetTeleporter = (173, 0);

			Locations[(int)LocationIds.MountGale].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MountGale, OverworldMapObjects.MountGaleMarker };
			Locations[(int)LocationIds.MountGale].TargetTeleporter = (174, 0);

			Locations[(int)LocationIds.PazuzusTower].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PazuzuTower };
			Locations[(int)LocationIds.PazuzusTower].TargetTeleporter = (184, 0);

			Locations[(int)LocationIds.ShipDock].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipDockCave };
			Locations[(int)LocationIds.ShipDock].TargetTeleporter = (17, 6);

			Locations[(int)LocationIds.DoomCastle].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.DoomCastle].TargetTeleporter = (1, 6);

			Locations[(int)LocationIds.LightTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LightTempleCave };
			Locations[(int)LocationIds.LightTemple].TargetTeleporter = (19, 6);

			Locations[(int)LocationIds.MacsShip].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDock };
			Locations[(int)LocationIds.MacsShip].TargetTeleporter = (37, 8);

			Locations[(int)LocationIds.MacsShipDoom].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDoom };
			Locations[(int)LocationIds.MacsShipDoom].TargetTeleporter = (37, 8);

			Locations.ForEach(n => n.Region = AccessReferences.Regions.Find(r => r.Item2 == n.LocationId).Item1);
			Locations.ForEach(n => n.SubRegion = AccessReferences.MapSubRegions.Find(r => r.Item2 == n.LocationId).Item1);
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
		public void ShuffleOverworld(MapShufflingMode mapshufflingmode, GameLogic gamelogic, Battlefields battlefields, MT19337 rng)
		{
			bool shuffleOverworld = mapshufflingmode != MapShufflingMode.None && mapshufflingmode != MapShufflingMode.Dungeons;
			
			if (!shuffleOverworld)
			{
				return;
			}

			List<MovableLocation> movableLocations = new();
            var regionRooms = gamelogic.Rooms.Where(r => r.Type == RoomType.Subregion).ToList();

            foreach (var room in regionRooms)
			{
				movableLocations.AddRange(room.GameObjects.Select(o => new MovableLocation(room.Region, o.Location, 0, o)));
                movableLocations.AddRange(room.Links.Where(l => l.Entrance >= 0).Select(l => new MovableLocation(room.Region, l.Location, l.TargetRoom, l)));
            }

            var regionLocationPairs = movableLocations.ToDictionary(x => x.Origins, x => x.Region);

            var safeGoldBattlefield = (LocationIds)(battlefields.GetAllRewardType().Select((x, i) => (i, x)).ToList().Find(x => x.x == BattlefieldRewardType.Gold).i + 1);

			List<LocationIds> shuffleLocations = Enum.GetValues<LocationIds>().ToList();
			List<LocationIds> destinationLocations = Enum.GetValues<LocationIds>().ToList();
			List<LocationIds> fixedLocations = new() { LocationIds.DoomCastle, LocationIds.FocusTowerForesta, LocationIds.FocusTowerAquaria, LocationIds.FocusTowerFrozen, LocationIds.FocusTowerFireburg, LocationIds.FocusTowerWindia, LocationIds.GiantTree, LocationIds.HillOfDestiny, LocationIds.LifeTemple, LocationIds.LightTemple, LocationIds.MacsShip, LocationIds.MacsShipDoom, LocationIds.None, LocationIds.ShipDock, LocationIds.SpencersPlace };
			shuffleLocations.RemoveAll(x => fixedLocations.Contains(x));
			destinationLocations.RemoveAll(x => fixedLocations.Contains(x));

			List<LocationIds> placedLocations = fixedLocations.ToList();
			List<LocationIds> takenLocations = fixedLocations.ToList();
			List<LocationIds> excludeFromStart = Enum.GetValues<LocationIds>().ToList().GetRange(9, 12);

			// Find best companion location
			List<(LocationIds, int)> companionsRating = new();

			foreach (var location in shuffleLocations.Where(x => x > LocationIds.HillOfDestiny))
			{
				companionsRating.Add(gamelogic.CrawlForCompanionRating(location));
			}
			
			companionsRating.Shuffle(rng);
			companionsRating = companionsRating.Where(x => x.Item2 > 0).OrderByDescending(x => x.Item2).ToList();
			LocationIds companionLocation = (mapshufflingmode == MapShufflingMode.Everything) ? companionsRating.First().Item1 : rng.PickFrom(companionsRating).Item1;

			// Find most accessible locations for item placement
			List<(LocationIds, int)> locationRating = new();

			foreach (var location in shuffleLocations.Where(x => x > LocationIds.HillOfDestiny))
			{
				locationRating.Add(gamelogic.CrawlForChestRating(location));
			}

			locationRating = locationRating.Where(x => x.Item1 != companionLocation).OrderByDescending(x => x.Item2).ToList();

			List<LocationIds> guaranteedChestLocations = new();

			
			guaranteedChestLocations = (mapshufflingmode == MapShufflingMode.Overworld) ?
				new() { rng.PickFrom(AccessReferences.StartingWeaponAccess) } :
				locationRating.GetRange(0, 2).Select(x => x.Item1).ToList();

			// Find Special Locations
			List<SpecialRegion> specialRegionsAccess = new()
			{
				new SpecialRegion(SubRegions.AquariaFrozenField, AccessReqs.SummerAquaria),
				new SpecialRegion(SubRegions.VolcanoBattlefield, AccessReqs.DualheadHydra),
			};

			List <AccessReqs> gatingLocationsAccess = new() { AccessReqs.SummerAquaria, AccessReqs.DualheadHydra, AccessReqs.LavaDomePlate, AccessReqs.Gidrah };

			List<(LocationIds, AccessReqs)> gatingLocations = new();

			foreach (var location in gatingLocationsAccess)
			{
				gatingLocations.Add((gamelogic.FindTriggerLocation(location), location));
			}

			foreach (var region in specialRegionsAccess)
			{
				var location = gatingLocations.Find(x => x.Item2 == region.Access).Item1;
				var accessreq = gamelogic.CrawlForRequirements(location);

				var commonaccess = accessreq.Intersect(gatingLocationsAccess).ToList();
				region.BarredLocations.AddRange(gatingLocations.Where(x => commonaccess.Contains(x.Item2)).Select(x => x.Item1).Append(location));
			}

			// Create the early locations, these are always placed in Foresta
			List<LocationIds> earlyLocations = new() { companionLocation, safeGoldBattlefield };
			earlyLocations.AddRange(guaranteedChestLocations);

			// Place guaranteed locations
			var loc1 = LocationIds.None;
			var loc2 = LocationIds.None;

			var forestaLocations = destinationLocations.Where(x => AccessReferences.MapSubRegions.Find(r => r.Item2 == x).Item1 == SubRegions.Foresta).ToList();

			while (earlyLocations.Any())
			{
				loc1 = earlyLocations.First();
				loc2 = rng.PickFrom(forestaLocations);

				movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
				placedLocations.Add(loc1);
				takenLocations.Add(loc2);

				earlyLocations = earlyLocations.Where(x => !placedLocations.Contains(x)).ToList();
				forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
			}
			
			forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
			var startingLocations = shuffleLocations.Where(x => !excludeFromStart.Contains(x) && !placedLocations.Contains(x)).ToList();

			// Place rest of Foresta region
			while (forestaLocations.Count > 0)
			{
				loc1 = rng.PickFrom(startingLocations);
				loc2 = rng.PickFrom(forestaLocations);

                movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
                placedLocations.Add(loc1);
				takenLocations.Add(loc2);

				forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
				startingLocations = startingLocations.Where(x => !placedLocations.Contains(x)).ToList();
			}

			// Place Special Regions
			bool gatingLocationPlaced = false;
			List<LocationIds> gatingLocationsList = gatingLocations.Select(l => l.Item1).ToList();

			foreach (var region in specialRegionsAccess)
			{
				var gatedRegionLocations = destinationLocations.Where(x => (AccessReferences.MapSubRegions.Find(r => r.Item2 == x).Item1 == region.SubRegion) && !takenLocations.Contains(x)).ToList();

				foreach (var location in gatedRegionLocations)
				{
					var regionSafeLocations = shuffleLocations.Where(x => !placedLocations.Contains(x) &&
						!region.BarredLocations.Contains(x) &&
						(gatingLocationPlaced ? !gatingLocationsList.Contains(x) : true)
						).ToList();

					loc1 = rng.PickFrom(regionSafeLocations);
					loc2 = location;

					if (gatingLocationsList.Contains(loc1))
					{
						gatingLocationPlaced = true;
					}

                    movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
                    placedLocations.Add(loc1);
					takenLocations.Add(loc2);
				}
			}

			shuffleLocations = shuffleLocations.Where(x => !placedLocations.Contains(x)).ToList();
			destinationLocations = destinationLocations.Where(x => !takenLocations.Contains(x)).ToList();

			// Place the rest
			while (shuffleLocations.Any())
			{
				loc1 = rng.TakeFrom(shuffleLocations);
				loc2 = rng.TakeFrom(destinationLocations);

                movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
			}

			// Clear rooms
			foreach (var room in regionRooms)
			{ 
				room.GameObjects.Clear();
				room.Links.RemoveAll(l => l.Entrance >= 0);
			}

			// Place the new locations
            foreach (var location in movableLocations)
            {
				if (location.Type == MovableLocationType.Battlefield)
				{
					var targetRegion = regionRooms.Find(x => x.Region == regionLocationPairs[location.Destination]);
					location.Object.LocationSlot = location.Destination;
					targetRegion.GameObjects.Add(location.Object);
				}
				else
				{
                    var targetRegion = regionRooms.Find(x => x.Region == regionLocationPairs[location.Destination]);
                    var originalRegion = regionRooms.Find(x => x.Region == location.Region);
                    location.Link.LocationSlot = location.Destination;
                    targetRegion.Links.Add(location.Link);

					var linkToUpdate = gamelogic.Rooms.Find(r => r.Id == location.Room).Links.Find(l => l.TargetRoom == originalRegion.Id);

					if (linkToUpdate != null)
					{
						linkToUpdate.TargetRoom = targetRegion.Id;
                    }
                }
            }
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
}
