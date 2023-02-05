using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public class TreasureObject
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public LocationIds Location { get; set; }
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }
		public Items Content { get; set; }
		public TreasureType Type { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public bool Accessible { get; set; }
		public string Name { get; set; }

		public TreasureObject(int mapobjid, int mapid, LocationIds location, TreasureType type, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = "";
			SubRegion = ItemLocations.MapSubRegions.Find(x => x.Item2 == location).Item1;
			Region = ItemLocations.ReturnRegion(location);
		}

		public TreasureObject(int mapobjid, int mapid, LocationIds location, TreasureType type, string name, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = name;
			SubRegion = ItemLocations.MapSubRegions.Find(x => x.Item2 == location).Item1;
			Region = ItemLocations.ReturnRegion(location);
		}

		public TreasureObject(TreasureObject treasure)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible = treasure.Accessible;
			Name = treasure.Name;
			Region = treasure.Region;
			SubRegion = treasure.SubRegion;
		}

		public TreasureObject(TreasureObject treasure, string name)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible= treasure.Accessible;
			Region = treasure.Region;
			SubRegion = treasure.SubRegion;
			Name = treasure.Name == "" ? name : treasure.Name;
		}
	}
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
	public partial class Overworld
	{
		public List<Location> Locations = new();

		private List<byte> ShipWestSteps = new();
		private List<byte> ShipEastSteps = new();
		private List<RoomLegacy> Rooms;
	
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
			Locations[(int)LocationIds.AquariaBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.AquariaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield02, OverworldMapObjects.AquariaBattlefield02Cleared };
			Locations[(int)LocationIds.AquariaBattlefield02].TargetTeleporter = (119, 1);
			Locations[(int)LocationIds.AquariaBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.AquariaBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield03, OverworldMapObjects.AquariaBattlefield03Cleared };
			Locations[(int)LocationIds.AquariaBattlefield03].TargetTeleporter = (120, 1);
			Locations[(int)LocationIds.AquariaBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.WintryBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield01, OverworldMapObjects.WintryBattlefield01Cleared };
			Locations[(int)LocationIds.WintryBattlefield01].TargetTeleporter = (121, 1);
			Locations[(int)LocationIds.WintryBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.WintryBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield02, OverworldMapObjects.WintryBattlefield02Cleared };
			Locations[(int)LocationIds.WintryBattlefield02].TargetTeleporter = (122, 1);
			Locations[(int)LocationIds.WintryBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.PyramidBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PyramidBattlefield01, OverworldMapObjects.PyramidBattlefield01Cleared };
			Locations[(int)LocationIds.PyramidBattlefield01].TargetTeleporter = (123, 1);
			Locations[(int)LocationIds.PyramidBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.LibraBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield01, OverworldMapObjects.LibraBattlefield01Cleared };
			Locations[(int)LocationIds.LibraBattlefield01].TargetTeleporter = (124, 1);
			Locations[(int)LocationIds.LibraBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.LibraBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield02, OverworldMapObjects.LibraBattlefield02Cleared };
			Locations[(int)LocationIds.LibraBattlefield02].TargetTeleporter = (125, 1);
			Locations[(int)LocationIds.LibraBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.FireburgBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield01, OverworldMapObjects.FireburgBattlefield01Cleared };
			Locations[(int)LocationIds.FireburgBattlefield01].TargetTeleporter = (126, 1);
			Locations[(int)LocationIds.FireburgBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.FireburgBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield02, OverworldMapObjects.FireburgBattlefield02Cleared };
			Locations[(int)LocationIds.FireburgBattlefield02].TargetTeleporter = (127, 1);
			Locations[(int)LocationIds.FireburgBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.FireburgBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield03, OverworldMapObjects.FireburgBattlefield03Cleared };
			Locations[(int)LocationIds.FireburgBattlefield03].TargetTeleporter = (128, 1);
			Locations[(int)LocationIds.FireburgBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.MineBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield01, OverworldMapObjects.MineBattlefield01Cleared };
			Locations[(int)LocationIds.MineBattlefield01].TargetTeleporter = (129, 1);
			Locations[(int)LocationIds.MineBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.MineBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield02, OverworldMapObjects.MineBattlefield02Cleared };
			Locations[(int)LocationIds.MineBattlefield02].TargetTeleporter = (130, 1);
			Locations[(int)LocationIds.MineBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.MineBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield03, OverworldMapObjects.MineBattlefield03Cleared };
			Locations[(int)LocationIds.MineBattlefield03].TargetTeleporter = (131, 1);
			Locations[(int)LocationIds.MineBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.VolcanoBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoBattlefield01, OverworldMapObjects.VolcanoBattlefield01Cleared };
			Locations[(int)LocationIds.VolcanoBattlefield01].TargetTeleporter = (132, 1);
			Locations[(int)LocationIds.VolcanoBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.WindiaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield01, OverworldMapObjects.WindiaBattlefield01Cleared };
			Locations[(int)LocationIds.WindiaBattlefield01].TargetTeleporter = (133, 1);
			Locations[(int)LocationIds.WindiaBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.WindiaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield02, OverworldMapObjects.WindiaBattlefield02Cleared };
			Locations[(int)LocationIds.WindiaBattlefield02].TargetTeleporter = (134, 1);
			Locations[(int)LocationIds.WindiaBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

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
			Locations[(int)LocationIds.FocusTowerAquaria].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.LibraTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraTempleCave };
			Locations[(int)LocationIds.LibraTemple].TargetTeleporter = (13, 6);
			Locations[(int)LocationIds.LibraTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.Aquaria].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaVillage };
			Locations[(int)LocationIds.Aquaria].TargetTeleporter = (8, 6);
			Locations[(int)LocationIds.Aquaria].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.WintryCave].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryCave };
			Locations[(int)LocationIds.WintryCave].TargetTeleporter = (49, 0);
			Locations[(int)LocationIds.WintryCave].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.LifeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LifeTempleCave };
			Locations[(int)LocationIds.LifeTemple].TargetTeleporter = (14, 6);
			Locations[(int)LocationIds.LifeTemple].AccessRequirements = new List<List<AccessReqs>> { };

			Locations[(int)LocationIds.FallsBasin].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FallBasinMarker };
			Locations[(int)LocationIds.FallsBasin].TargetTeleporter = (53, 0);
			Locations[(int)LocationIds.FallsBasin].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.IcePyramid].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.IcePyramid };
			Locations[(int)LocationIds.IcePyramid].TargetTeleporter = (56, 0);
			Locations[(int)LocationIds.IcePyramid].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.SpencersPlace].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SpencerCave };
			Locations[(int)LocationIds.SpencersPlace].TargetTeleporter = (7, 6);
			Locations[(int)LocationIds.SpencersPlace].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.RainbowBridge },
			};

			Locations[(int)LocationIds.WintryTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryTempleCave };
			Locations[(int)LocationIds.WintryTemple].TargetTeleporter = (15, 6);
			Locations[(int)LocationIds.WintryTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.FocusTowerFrozen].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerFrozen].TargetTeleporter = (5, 6);
			Locations[(int)LocationIds.FocusTowerFrozen].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.FocusTowerFireburg].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerFireburg].TargetTeleporter = (6, 6);
			Locations[(int)LocationIds.FocusTowerFireburg].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.Fireburg].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgVillage };
			Locations[(int)LocationIds.Fireburg].TargetTeleporter = (9, 6);
			Locations[(int)LocationIds.Fireburg].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			};

			Locations[(int)LocationIds.Mine].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.Mine };
			Locations[(int)LocationIds.Mine].TargetTeleporter = (98, 0);
			Locations[(int)LocationIds.Mine].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.SealedTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SealedTempleCave };
			Locations[(int)LocationIds.SealedTemple].TargetTeleporter = (16, 6);
			Locations[(int)LocationIds.SealedTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.Volcano].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoMarker };
			Locations[(int)LocationIds.Volcano].TargetTeleporter = (103, 0);
			Locations[(int)LocationIds.Volcano].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.LavaDome].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LavaDome };
			Locations[(int)LocationIds.LavaDome].TargetTeleporter = (104, 0);
			Locations[(int)LocationIds.LavaDome].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Locations[(int)LocationIds.FocusTowerWindia].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.FocusTowerWindia].TargetTeleporter = (3, 6);
			Locations[(int)LocationIds.FocusTowerWindia].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.RopeBridge].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.RopeBridgeMarker };
			Locations[(int)LocationIds.RopeBridge].TargetTeleporter = (140, 0);
			Locations[(int)LocationIds.RopeBridge].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.AliveForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AliveForestMarker };
			Locations[(int)LocationIds.AliveForest].TargetTeleporter = (142, 0);
			Locations[(int)LocationIds.AliveForest].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.GiantTree].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MovedGiantTree };
			Locations[(int)LocationIds.GiantTree].TargetTeleporter = (49, 8);
			Locations[(int)LocationIds.GiantTree].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Axe, AccessReqs.AliveForest },
			};

			Locations[(int)LocationIds.KaidgeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.KaidgeTempleCave };
			Locations[(int)LocationIds.KaidgeTemple].TargetTeleporter = (18, 6);
			Locations[(int)LocationIds.KaidgeTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.Windia].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaVillage };
			Locations[(int)LocationIds.Windia].TargetTeleporter = (10, 6);
			Locations[(int)LocationIds.Windia].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.WindholeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindholeTempleCave };
			Locations[(int)LocationIds.WindholeTemple].TargetTeleporter = (173, 0);
			Locations[(int)LocationIds.WindholeTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.MountGale].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MountGale, OverworldMapObjects.MountGaleMarker };
			Locations[(int)LocationIds.MountGale].TargetTeleporter = (174, 0);
			Locations[(int)LocationIds.MountGale].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.PazuzusTower].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PazuzuTower };
			Locations[(int)LocationIds.PazuzusTower].TargetTeleporter = (184, 0);
			Locations[(int)LocationIds.PazuzusTower].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Locations[(int)LocationIds.ShipDock].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipDockCave };
			Locations[(int)LocationIds.ShipDock].TargetTeleporter = (17, 6);
			Locations[(int)LocationIds.ShipDock].AccessRequirements = new List<List<AccessReqs>> { };

			Locations[(int)LocationIds.DoomCastle].OwMapObjects = new List<OverworldMapObjects> { };
			Locations[(int)LocationIds.DoomCastle].TargetTeleporter = (1, 6);
			Locations[(int)LocationIds.DoomCastle].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SkyCoin, AccessReqs.ShipLoaned, AccessReqs.ShipSteeringWheel },
			};

			Locations[(int)LocationIds.LightTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LightTempleCave };
			Locations[(int)LocationIds.LightTemple].TargetTeleporter = (19, 6);
			Locations[(int)LocationIds.LightTemple].AccessRequirements = new List<List<AccessReqs>> {	};

			Locations[(int)LocationIds.MacsShip].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDock };
			Locations[(int)LocationIds.MacsShip].TargetTeleporter = (37, 8);
			Locations[(int)LocationIds.MacsShip].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.ShipDockAccess, AccessReqs.ShipLiberated },
			};
			Locations[(int)LocationIds.MacsShipDoom].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDoom };
			Locations[(int)LocationIds.MacsShipDoom].TargetTeleporter = (37, 8);
			Locations[(int)LocationIds.MacsShipDoom].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.ShipLoaned, AccessReqs.ShipSteeringWheel },
			};

			Locations.ForEach(n => n.Region = ItemLocations.Regions.Find(r => r.Item2 == n.LocationId).Item1);
			Locations.ForEach(n => n.SubRegion = ItemLocations.MapSubRegions.Find(r => r.Item2 == n.LocationId).Item1);
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
			if (flags.OverworldShuffle || flags.CrestShuffle)
			{
				Locations[(int)LocationIds.LibraBattlefield01].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.WakeWaterUsed;
			}
			Locations[(int)LocationIds.LibraTemple].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.WakeWaterUsed;
			Locations[(int)LocationIds.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.WakeWaterUsed;
			Locations[(int)LocationIds.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.West] = (int)GameFlagsList.VolcanoErupted;
			Locations[(int)LocationIds.Volcano].DirectionFlags[(int)NodeDirections.East] = (int)GameFlagsList.VolcanoErupted;

			Locations[(int)LocationIds.PazuzusTower].DirectionFlags[(int)NodeDirections.North] = 0xDC;
			Locations[(int)LocationIds.SpencersPlace].DirectionFlags[(int)NodeDirections.South] = 0xDC;
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
		public LocationIds ShuffleEntrances(Flags flags, Battlefields battlefields, MT19337 rng)
		{
			LocationIds startingLocation = LocationIds.LevelForest;

			if (!flags.OverworldShuffle)
			{
				return startingLocation;
			}

			var safeGoldBattlefield = (LocationIds)(battlefields.GetAllRewardType().Select((x, i) => (i, x)).ToList().Find(x => x.x == BattlefieldRewardType.Gold).i + 1);

			List<LocationIds> validLocations = Enum.GetValues<LocationIds>().ToList();
			List<LocationIds> invalidLocations = new() { LocationIds.DoomCastle, LocationIds.FocusTowerForesta, LocationIds.FocusTowerAquaria, LocationIds.FocusTowerFrozen, LocationIds.FocusTowerFireburg, LocationIds.FocusTowerWindia, LocationIds.GiantTree, LocationIds.HillOfDestiny, LocationIds.LifeTemple, LocationIds.LightTemple, LocationIds.MacsShip, LocationIds.MacsShipDoom, LocationIds.None, LocationIds.ShipDock, LocationIds.SpencersPlace };
			validLocations.RemoveAll(x => invalidLocations.Contains(x));

			List<LocationIds> companionLocation = new() { LocationIds.SandTemple, LocationIds.LibraTemple, LocationIds.Fireburg };
			//List<LocationIds> excludeFromStart = new() { LocationIds.IcePyramid, LocationIds.Mine, LocationIds.Volcano, LocationIds.LavaDome, LocationIds.AliveForest, LocationIds.MountGale, LocationIds.PazuzusTower };

			List<LocationIds> excludeFromStart = new();

			excludeFromStart.AddRange(Enum.GetValues<LocationIds>().ToList().GetRange(9, 12));

			var forestaLocations = validLocations.Where(x => ItemLocations.MapSubRegions.Find(r => r.Item2 == x).Item1 == SubRegions.Foresta).ToList();

			List<LocationIds> placedLocations = new();

			var loc1 = LocationIds.None;
			var loc2 = LocationIds.None;

			// Place guaranteed locations
			List<LocationIds> earlyLocations = new() { rng.PickFrom(companionLocation), safeGoldBattlefield };

			while (earlyLocations.Any())
			{
				loc1 = rng.TakeFrom(forestaLocations);
				loc2 = earlyLocations.First();

				SwapLocations(loc1, loc2);
				placedLocations.Add(loc1);
				placedLocations.Add(loc2);
				earlyLocations.RemoveAt(0);
			}
			
			forestaLocations = forestaLocations.Where(x => !placedLocations.Contains(x)).ToList();
			var validStartingLocations = validLocations.Where(x => !excludeFromStart.Contains(x) && !placedLocations.Contains(x)).ToList();

			// Place Foresta region
			while (forestaLocations.Count > 0)
			{
				loc1 = rng.PickFrom(forestaLocations);
				loc2 = rng.PickFrom(validStartingLocations);

				SwapLocations(loc1, loc2);
				placedLocations.Add(loc1);
				placedLocations.Add(loc2);

				forestaLocations = forestaLocations.Where(x => !placedLocations.Contains(x)).ToList();
				validStartingLocations = validStartingLocations.Where(x => !placedLocations.Contains(x)).ToList();
			}

			// Place Aquaria outside the frozen strip
			if (flags.LogicOptions != LogicOptions.Expert && !placedLocations.Contains(LocationIds.Aquaria))
			{ 
				var aquariaSafeLocactions = validLocations.Where(x => !placedLocations.Contains(x) && ItemLocations.MapSubRegions.Find(r => r.Item2 == x).Item1 != SubRegions.AquariaFrozenField).ToList();

				loc2 = rng.PickFrom(aquariaSafeLocactions);
				
				SwapLocations(LocationIds.Aquaria, loc2);
				placedLocations.Add(LocationIds.Aquaria);
				placedLocations.Add(loc2);
			}

			validLocations = validLocations.Where(x => !placedLocations.Contains(x)).ToList();

			// Place the rest
			while (validLocations.Count > 1)
			{
				loc1 = rng.TakeFrom(validLocations);
				loc2 = rng.TakeFrom(validLocations);

				SwapLocations(loc1, loc2);
			}

			UpdateBattlefieldsColor(battlefields);
			
			startingLocation = Locations.Find(x => x.Position == (0x0E, 0x28)).LocationId;
			
			return startingLocation;
		}
		public void SwapLocations(LocationIds locA, LocationIds locB)
		{
			var tempNode = new Location(Locations[(int)locA]);

			Locations[(int)locA].Destinations = Locations[(int)locB].Destinations.ToList();
			Locations[(int)locA].Steps = Locations[(int)locB].Steps.ToList();
			Locations[(int)locA].DirectionFlags = Locations[(int)locB].DirectionFlags.ToList();
			Locations[(int)locA].Position = Locations[(int)locB].Position;
			Locations[(int)locA].Region = Locations[(int)locB].Region;
			Locations[(int)locA].SubRegion = Locations[(int)locB].SubRegion;
			Locations[(int)locA].AccessRequirements = Locations[(int)locB].AccessRequirements.ToList();


			Locations[(int)locB].Destinations = tempNode.Destinations;
			Locations[(int)locB].Steps = tempNode.Steps;
			Locations[(int)locB].DirectionFlags = tempNode.DirectionFlags;
			Locations[(int)locB].Position = tempNode.Position;
			Locations[(int)locB].Region = tempNode.Region;
			Locations[(int)locB].SubRegion = tempNode.SubRegion;
			Locations[(int)locB].AccessRequirements = tempNode.AccessRequirements;

			foreach (var node in Locations)
			{
				for(int i = 0; i < node.Destinations.Count; i++)
				{
					if (node.Destinations[i] == locA)
					{
						node.Destinations[i] = locB;
					}
					else if (node.Destinations[i] == locB)
					{
						node.Destinations[i] = locA;
					}
				}
			}
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
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public LocationIds LocationId { get; set; }
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
			AccessRequirements = new() { new List<AccessReqs> { } };
			LocationId = LocationIds.None;
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
			AccessRequirements = node.AccessRequirements.ToList();
			Position = node.Position;
			LocationId = node.LocationId;
			Region = node.Region;
			SubRegion = node.SubRegion;
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
