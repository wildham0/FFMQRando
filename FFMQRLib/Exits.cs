using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.EventEmitters;

namespace FFMQLib
{	
	public class Entrance
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int Area { get; set; }
		[YamlIgnore]
		public (int x, int y) Coordinates { get; set; }
		[YamlIgnore]
		public (int id, int type) Teleporter { get; set; }
		public List<int> coordinates
		{
			get => new List<int> { Coordinates.x, Coordinates.y };
			set => Coordinates = (value[0], value[1]);
		}
		public List<int> teleporter
		{
			get => new List<int> { Teleporter.id, Teleporter.type };
			set => Teleporter = (value[0], value[1]);
		}

		public Entrance()
		{
			Name = "void";
			Id = 0;
			Coordinates = (0, 0);
			Teleporter = (0, 0);
			Area = 0;
		}
		public Entrance(string name, int id, int x, int y, int teleid, int teletype)
		{
			Name = name;
			Id = id;
			Coordinates = (x, y);
			Teleporter = (teleid, teletype);
			Area = 0;
		}

		public Entrance(string name, int id, int area, byte[] data)
		{
			Name = name;
			Id = id;
			Coordinates = (data[0], data[1]);
			Teleporter = (data[2], 0);
			Area = area;

		}
		public Entrance(int id, Entrance entrance)
		{
			Name = entrance.Name;
			Id = id;
			Coordinates = entrance.Coordinates;
			Teleporter = entrance.Teleporter;
			Area = entrance.Area;
		}

		public byte[] ToBytes()
		{
			return new byte[] { (byte)Coordinates.x, (byte)Coordinates.y, (byte)Teleporter.id, (byte)Teleporter.type };
		}
	}

	public class EntrancesLinkList
	{ 
		public List<int> EntranceA { get; set; }
		public List<int> EntranceB { get; set; }

		public EntrancesLinkList()
		{
			EntranceA = new() { 0, 0 };
			EntranceB = new() { 0, 0 };
		}

		public EntrancesLinkList(List<int> entrancea, List<int> entranceb)
		{
			EntranceA = entrancea.GetRange(0,2);
			EntranceB = entranceb.GetRange(0,2);
		}
	}
	
	public class EntrancesLink
	{
		public (int id, int type) EntranceA { get; set; }
		public (int id, int type) EntranceB { get; set; }

		public EntrancesLink()
		{
			EntranceA = (0,0);
			EntranceB = (0,0);
		}

		public EntrancesLink((int, int) entrancea, (int, int) entranceb)
		{
			EntranceA = entrancea;
			EntranceB = entranceb;
		}

		public EntrancesLink(List<int> entrancea, List<int> entranceb)
		{
			EntranceA = (entrancea[0], entrancea[1]);
			EntranceB = (entranceb[0], entranceb[1]);
		}

		public EntrancesLink(EntrancesLinkList entrancelist)
		{
			EntranceA = (entrancelist.EntranceA[0], entrancelist.EntranceA[1]);
			EntranceB = (entrancelist.EntranceB[0], entrancelist.EntranceB[1]);
		}
	}
	public class CrestTile
	{
		public (int id, int type) OriginTeleporter { get; set; }
		public (int id, int type) TargetTeleporter { get; set; }
		public int EntranceId { get; set; }
		public bool Deadend { get; set; }
		public int Priority { get; set; }
		public int Area { get; set; }
		public LocationIds Location { get; set; }
		public MapList Map { get; set; }
		public (int x, int y) Position {get; set;}
		public Items Crest { get; set; }
		public AccessReqs LocationRequirement { get; set; }
		public CrestTile(Entrance entrance)
		{ 
			EntranceId = entrance.Id;
			Position = entrance.Coordinates;
			Deadend = false;
			Priority = 0;
			OriginTeleporter = (0, 0);
			TargetTeleporter = (0, 0);
			Area = 0;
			Map = MapList.Overworld;
			Crest = Items.LibraCrest;
			Location = LocationIds.None;
			LocationRequirement = AccessReqs.Barred;
		}
	}

	public class CrestTile2
	{
		public int Entrance { get; set; }
		public (int id, int type) TargetTeleporter { get; set; }
		//public (int x, int y) Position { get; set; }
		public (int id, int type) Script { get; set; }
		public Items Crest { get; set; }
		public MapList Map { get; set; }

		public CrestTile2(int entrance, (int, int) target, (int, int) script, MapList map)
		{
			Entrance = entrance;
			TargetTeleporter = target;
			Script = script;
			Map = map;
			Crest = Items.LibraCrest;
		}
	}

	public class LocationStructure
	{
		//public List<RoomLegacy> Entrances {get; set;}
		public List<Entrance> Entrances { get; set; }
		public List<EntrancesLink> EntrancesLinks { get; set; }
		public List<EntrancesLinkList> EntrancesLinksList { get; set; }
		public List<CrestTile> CrestTiles { get; set; }
		public List<(AccessReqs crest, AccessReqs loc1, AccessReqs loc2)> CrestPairs { get; set; }

		public const int EntrancesBank = 0x05;
		public const int EntrancesPointers = 0xF920;
		public const int EntrancesPointersQty = 108;
		public const int EntrancesOffset = 0xF9F8;

		public const int NewEntrancesBank = 0x12;
		public const int NewEntrancesPointers = 0xA000;
		public const int NewEntrancesOffset = 0xA0D8;

		public LocationStructure()
		{
			Entrances = new();
			EntrancesLinks = new();
			EntrancesLinksList = new();
			CrestTiles = new();
			CrestPairs = new();
		}
		public LocationStructure(FFMQRom rom)
		{
			Entrances = new();
			
			for (int i = 0; i < 0x6C; i++)
			{
				//int currentAddress = EntrancesOffset;
				int currentEntrance = 0;
				List<Entrance> tempEntrances = new();

				//LocationIds.Add(new Location("void", i, new List<TreasureObject>(), new List<Entrance>()));

				int currentAddress = (EntrancesOffset + rom.GetFromBank(EntrancesBank, EntrancesPointers + (i * 2), 2).ToUShorts()[0]);
				int maxAddress = (EntrancesOffset + rom.GetFromBank(EntrancesBank, EntrancesPointers + ((i + 1) * 2), 2).ToUShorts()[0]);


				if (i == (EntrancesPointersQty - 1))
				{
					maxAddress = 0xFFFF;
				}

				while (currentAddress < maxAddress)
				{
					byte[] entrance = rom.GetFromBank(EntrancesBank, currentAddress, 3);

					if (entrance[0] == 0xFF)
					{
						break;
					}

					tempEntrances.Add(new Entrance("void", currentEntrance, i, entrance));

					currentAddress += 3;
					currentEntrance++;
				}

				//Entrances.Add(new RoomLegacy("void", i, i, new List<TreasureObject>(), tempEntrances));
			}

			EntrancesLinks = new();
			EntrancesLinksList = new();
			CrestTiles = new();
			CrestPairs = ItemLocations.LinkedTeleporters.ToList();
			/*
			EntrancesLinksList.Add(new EntrancesLinkList(new List<int> { 1, 8 }, new List<int> { 1, 2 }));
			EntrancesLinksList.Add(new EntrancesLinkList(new List<int> { 2, 8 }, new List<int> { 5, 2 }));*/
			//EntrancesLinks.Add(new EntrancesLink((1, 8), (1, 2)));
			//EntrancesLinks.Add(new EntrancesLink((2, 8), (5, 2)));


		}
		private void CrestShuffle(MT19337 rng)
		{
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

			var crestList = CrestTiles.ToList();
			CrestTiles = new();
			CrestPairs = new();

			crestList.Shuffle(rng);
			crestList = crestList.OrderByDescending(x => x.Priority).ToList();

			List<(int priority, Items crest)> crestPriority = new();
			List<CrestTile> crestToUpdate = new();

			int deadendCount = 0;
			int passableCount = 0;

			while (crestList.Any())
			{
				CrestTile crest1;
				CrestTile crest2;

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

				// Switch teleporters
				crest1.TargetTeleporter = crest2.OriginTeleporter;
				crest2.TargetTeleporter = crest1.OriginTeleporter;

				//var crest1room = rooms.Find(x => x.Links.Where(l => l.Entrance == crest1.EntranceId).Any());
				//var crest1link = crest1room.Links.Find(l => l.Entrance == crest1.EntranceId);
				
				//var crest2room = rooms.Find(x => x.Links.Where(l => l.Entrance == crest2.EntranceId).Any());
				//var crest2link = crest2room.Links.Find(l => l.Entrance == crest2.EntranceId);


				//crest1room.Links.Remove(crest1link);
				//crest1room.Links.Add(new RoomLink(crest2room.Id, crest1.EntranceId, crest1link.Access.Except(crestAcess).Concat(ItemLocations.ItemAccessReq[crest1.Crest]).ToList()));

				//crest2room.Links.Remove(crest2link);
				//crest2room.Links.Add(new RoomLink(crest1room.Id, crest2.EntranceId, crest2link.Access.Except(crestAcess).Concat(ItemLocations.ItemAccessReq[crest2.Crest]).ToList()));

				CrestTiles.Add(crest1);
				CrestTiles.Add(crest2);
				CrestPairs.Add((ItemLocations.ItemAccessReq[crest1.Crest][0], crest1.LocationRequirement, crest2.LocationRequirement));
			}
		}
		public void UpdateCrests(Flags flags, GameScriptManager tileScripts, GameMaps gameMaps, List<Room> rooms, MT19337 rng)
		{
			bool keepWintryTemple = !(flags.OverworldShuffle || flags.CrestShuffle);

			List<CrestTile2> crestsList = new()
			{
				new CrestTile2(51, (0x21, 6), (67, 8), MapList.ForestaInterior),
				new CrestTile2(52, (0x22, 6), (68, 8), MapList.ForestaInterior),
				new CrestTile2(53, (0x23, 6), (69, 8), MapList.ForestaInterior),
				new CrestTile2(96, (0x1D, 6), (72, 8), MapList.HouseInterior),
				new CrestTile2(76, (0x15, 6), (59, 8), MapList.Caves),
				new CrestTile2(108, (0x16, 6), (60, 8), MapList.Caves),
				new CrestTile2(275, (0x35, 1), (64, 8), MapList.LevelAliveForest),
				new CrestTile2(276, (0x36, 1), (65, 8), MapList.LevelAliveForest),
				new CrestTile2(277, (0x37, 1), (66, 8), MapList.LevelAliveForest),
				new CrestTile2(158, (0x19, 6), (62, 8), MapList.Caves),
				new CrestTile2(191, keepWintryTemple ? (0x1A, 6) : (0x8C, 1), (63, 8), MapList.Caves),
				new CrestTile2(175, (0x1F, 6), (45, 8), MapList.HouseInterior),
				new CrestTile2(171, (0x1E, 6), (54, 8), MapList.HouseInterior),
				new CrestTile2(308, (0x1C, 6), (71, 8), MapList.Caves),
				new CrestTile2(396, (0x1B, 6), (70, 8), MapList.Caves),
				new CrestTile2(334, (0x18, 6), (44, 8), MapList.HouseInterior),
				new CrestTile2(336, (0x17, 6), (43, 8), MapList.HouseInterior),
				new CrestTile2(397, (0x20, 6), (61, 8), MapList.ForestaInterior),
			};

			List<AccessReqs> crestAccess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			List<(Items, AccessReqs)> crestItemAccess = new()
			{
				(Items.LibraCrest, AccessReqs.LibraCrest),
				(Items.GeminiCrest, AccessReqs.GeminiCrest),
				(Items.MobiusCrest, AccessReqs.MobiusCrest)
			};


			List<((int id, int type), (int id, int type), LocationIds location, AccessReqs access, bool deadend, int priority)> crestTileTeleporterList = new()
			{
				((0x21, 6), (67, 8), LocationIds.AliveForest, AccessReqs.WoodHouseLibraCrestTile, true, 0), // (0x27, 1)
				((0x22, 6), (68, 8), LocationIds.AliveForest, AccessReqs.WoodHouseGeminiCrestTile, true, 0), // (0x28, 1)
				((0x23, 6), (69, 8), LocationIds.AliveForest, AccessReqs.WoodHouseMobiusCrestTile, true, 0), // (0x29, 1)
				((0x1D, 6), (72, 8), LocationIds.Aquaria, AccessReqs.AquariaVendorCrestTile, false, 1), // Aquaria Vendor House
				((0x15, 6), (59, 8), LocationIds.LibraTemple, AccessReqs.LibraTempleCrestTile, false, 0),
				((0x16, 6), (60, 8), LocationIds.LifeTemple, AccessReqs.LifeTempleCrestTile, true, 0),
				//((0x2D, 1), (33, 8)), Exclude spencer's cave teleporter
				//((0x2E, 1), (34, 8)),
				//((0x2F, 1), (35, 8)),
				//((0x30, 1), (36, 8)),
				((0x35, 1), (64, 8), LocationIds.AliveForest, AccessReqs.AliveForestLibraCrestTile, false, 0), // always short
				((0x36, 1), (65, 8), LocationIds.AliveForest, AccessReqs.AliveForestGeminiCrestTile, false, 0),
				((0x37, 1), (66, 8), LocationIds.AliveForest, AccessReqs.AliveForestMobiusCrestTile, false, 0),
				((0x19, 6), (62, 8), LocationIds.WintryTemple, AccessReqs.WintryTempleCrestTile, true, 0),
				(keepWintryTemple ? (0x1A, 6) : (0x8C, 1), (63, 8), LocationIds.SealedTemple, AccessReqs.SealedTempleCrestTile, false, 0), // to short
				((0x1F, 6), (45, 8), LocationIds.Fireburg, AccessReqs.FireburgVendorCrestTile, false, 1), // Fireburg Vendor House
				((0x1E, 6), (54, 8), LocationIds.Fireburg, AccessReqs.FireburgGrenademanCrestTile, false, 2), // Fireburg Grenade Man
				((0x1C, 6), (71, 8), LocationIds.KaidgeTemple, AccessReqs.KaidgeTempleCrestTile, false, 0),
				((0x1B, 6), (70, 8), LocationIds.LightTemple, AccessReqs.LightTempleCrestTile, true, 0),
				((0x18, 6), (44, 8), LocationIds.Windia, AccessReqs.WindiaKidsCrestTile, false, 0),
				((0x17, 6), (43, 8), LocationIds.Windia, AccessReqs.WindiaDockCrestTile, false, 2), // Windia Mobius Old
				((0x20, 6), (61, 8), LocationIds.ShipDock, AccessReqs.ShipDockCrestTile, true, 0),
			};

			List<(AccessReqs, Items)> vanillaCrests = new()
			{
				(AccessReqs.WoodHouseLibraCrestTile, Items.LibraCrest),
				(AccessReqs.WoodHouseGeminiCrestTile, Items.GeminiCrest),
				(AccessReqs.WoodHouseMobiusCrestTile, Items.MobiusCrest),
				(AccessReqs.AquariaVendorCrestTile, Items.GeminiCrest),
				(AccessReqs.LibraTempleCrestTile, Items.LibraCrest),
				(AccessReqs.LifeTempleCrestTile, Items.LibraCrest),
				(AccessReqs.AliveForestLibraCrestTile, Items.LibraCrest),
				(AccessReqs.AliveForestGeminiCrestTile, Items.GeminiCrest),
				(AccessReqs.AliveForestMobiusCrestTile, Items.MobiusCrest),
				(AccessReqs.WintryTempleCrestTile, Items.GeminiCrest),
				(AccessReqs.SealedTempleCrestTile, Items.GeminiCrest),
				(AccessReqs.FireburgVendorCrestTile, Items.GeminiCrest),
				(AccessReqs.FireburgGrenademanCrestTile, Items.MobiusCrest),
				(AccessReqs.KaidgeTempleCrestTile, Items.MobiusCrest),
				(AccessReqs.LightTempleCrestTile, Items.MobiusCrest),
				(AccessReqs.WindiaKidsCrestTile, Items.MobiusCrest),
				(AccessReqs.WindiaDockCrestTile, Items.MobiusCrest),
				(AccessReqs.ShipDockCrestTile, Items.MobiusCrest),
			};

			List<(MapList map, Items crest, byte tile)> crestMapTiles = new()
			{
				(MapList.LevelAliveForest, Items.LibraCrest, 0x52), // +1 for actual tile
				(MapList.LevelAliveForest, Items.GeminiCrest, 0x19),
				(MapList.LevelAliveForest, Items.MobiusCrest, 0x3D),
				(MapList.ShipDock, Items.LibraCrest, 0x53),
				(MapList.ShipDock, Items.GeminiCrest, 0x1A),
				(MapList.ShipDock, Items.MobiusCrest, 0x3E),
				(MapList.HouseInterior, Items.LibraCrest, 0x12),
				(MapList.HouseInterior, Items.GeminiCrest, 0x13),
				(MapList.HouseInterior, Items.MobiusCrest, 0x14),
				(MapList.Caves, Items.LibraCrest, 0x10),
				(MapList.Caves, Items.GeminiCrest, 0x11),
				(MapList.Caves, Items.MobiusCrest, 0x12),
				(MapList.ForestaInterior, Items.LibraCrest, 0x12),
				(MapList.ForestaInterior, Items.GeminiCrest, 0x13),
				(MapList.ForestaInterior, Items.MobiusCrest, 0x14),
			};


			List<(int area, MapList map)> areaToMap = new()
			{
				(17, MapList.ForestaInterior),
				(23, MapList.Caves),
				(27, MapList.HouseInterior),
				(32, MapList.Caves),
				(45, MapList.Caves),
				(46, MapList.Caves),
				(49, MapList.HouseInterior),
				(53, MapList.Caves),
				(67, MapList.LevelAliveForest),
				(77, MapList.Caves),
				(82, MapList.HouseInterior),
				(95, MapList.Caves),
				(96, MapList.ShipDock),
				(17, MapList.ForestaInterior),
			};
			/*
			List<(int id, int type)> crestTileList = crestTileTeleporterList.Select(x => x.Item2).ToList();
			var tempentrances = Entrances.ToList();
			var tempccrests = tempentrances.Where(x => crestTileList.Contains((x.Teleporter))).Distinct().ToList();
			var temptiles = tempccrests.Select(x => new CrestTile(x)).ToList();


			CrestTiles = Entrances.Where(x => crestTileList.Contains((x.Teleporter))).Distinct().Select(x => new CrestTile(x)).ToList();
			*/
			/*
			foreach (var crest in CrestTiles)
			{
				var teleporterValue = crestTileTeleporterList.Find(x => x.Item2 == crest.EntranceId);
				crest.TargetTeleporter = teleporterValue.Item1;
				crest.Deadend = teleporterValue.deadend;
				var originEntrance = EntrancesLinks.Where(x => (x.EntranceA == crest.EntranceId)).Any() ? EntrancesLinks.Where(x => (x.EntranceA == crest.EntranceId)).First().EntranceB : EntrancesLinks.Where(x => (x.EntranceB == crest.EntranceId)).First().EntranceA;
				crest.OriginTeleporter = crestTileTeleporterList.Find(x => x.Item2 == originEntrance).Item1;
				crest.Priority = teleporterValue.priority;
				crest.Location = teleporterValue.location;
				crest.LocationRequirement = teleporterValue.access;
				crest.Crest = vanillaCrests.Find(x => x.Item1 == teleporterValue.access).Item2;
				crest.Area = Entrances.Find(x => x.Entrances.Where(e => (e.TeleportId == crest.EntranceId.id) && (e.TeleportType == crest.EntranceId.type)).Any()).AreaId;
				crest.Map = areaToMap.Find(x => x.area == crest.Area).map;
			}*/
			/*
			if (flags.CrestShuffle)
			{
				CrestShuffle(rng);
			}*/

			var flatLinkList = rooms.SelectMany(x => x.Links).ToList();
			foreach (var crest in crestsList)
			{
				var entranceToUpdate = Entrances.Where(x => x.Id == crest.Entrance).ToList();
				var updatedLink = flatLinkList.Find(x => x.Entrance == crest.Entrance);
				var access = crestAccess.Intersect(updatedLink.Access).ToList().First();
				var crestItem = crestItemAccess.Find(c => c.Item2 == access).Item1;
				var newTeleporter = crestsList.Find(x => x.Script == updatedLink.Teleporter).TargetTeleporter;

				foreach (var entrance in entranceToUpdate)
				{
					var targetTile = crestMapTiles.Find(x => (x.crest == crestItem) && (x.map == crest.Map)).tile;
					gameMaps[(int)crest.Map].ModifyMap(entrance.Coordinates.x, entrance.Coordinates.y, targetTile, true);
					entrance.Teleporter = updatedLink.Teleporter;
				}

				tileScripts.AddScript(updatedLink.Teleporter.id, new ScriptBuilder(new List<string> {
						"2F",
						$"050D{(int)crestItem:X2}[03]",
						$"2A1227{newTeleporter.id:X2}{newTeleporter.type:X2}FFFF",
						"00",
						}));


			}

			//List<(int, RoomLink)> linksToProcess = new();
			//List<AccessReqs> crestAcess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			/*
			foreach (var crest in CrestTiles)
			{
				var entranceToUpdate = Entrances.Where(x => x.Id == crest.EntranceId).ToList();

				foreach (var entrance in entranceToUpdate)
				{
					var targetTile = crestMapTiles.Find(x => (x.crest == crest.Crest) && (x.map == crest.Map)).tile;
					gameMaps[(int)crest.Map].ModifyMap(entrance.Coordinates.x, entrance.Coordinates.y, targetTile, true);
				}

				tileScripts.AddScript(crest.EntranceId, new ScriptBuilder(new List<string> {
						"2F",
						$"050D{(int)crest.Crest:X2}[03]",
						$"2A1227{crest.TargetTeleporter.id:X2}{crest.TargetTeleporter.type:X2}FFFF",
						"00",
						}));
			}*/
		}
		/*
		public void UpdateCrests2(Flags flags, GameScriptManager tileScripts, GameMaps gameMaps, List<Room> rooms, MT19337 rng)
		{
			bool keepWintryTemple = !(flags.OverworldShuffle || flags.CrestShuffle);

			List<((int id, int type), (int id, int type), LocationIds location, AccessReqs access, bool deadend, int priority)> crestTileTeleporterList = new()
			{
				((0x21, 6), (67, 8), LocationIds.AliveForest, AccessReqs.WoodHouseLibraCrestTile, true, 0), // (0x27, 1)
				((0x22, 6), (68, 8), LocationIds.AliveForest, AccessReqs.WoodHouseGeminiCrestTile, true, 0), // (0x28, 1)
				((0x23, 6), (69, 8), LocationIds.AliveForest, AccessReqs.WoodHouseMobiusCrestTile, true, 0), // (0x29, 1)
				((0x1D, 6), (72, 8), LocationIds.Aquaria, AccessReqs.AquariaVendorCrestTile, false, 1), // Aquaria Vendor House
				((0x15, 6), (59, 8), LocationIds.LibraTemple, AccessReqs.LibraTempleCrestTile, false, 0),
				((0x16, 6), (60, 8), LocationIds.LifeTemple, AccessReqs.LifeTempleCrestTile, true, 0),
				//((0x2D, 1), (33, 8)), Exclude spencer's cave teleporter
				//((0x2E, 1), (34, 8)),
				//((0x2F, 1), (35, 8)),
				//((0x30, 1), (36, 8)),
				((0x35, 1), (64, 8), LocationIds.AliveForest, AccessReqs.AliveForestLibraCrestTile, false, 0), // always short
				((0x36, 1), (65, 8), LocationIds.AliveForest, AccessReqs.AliveForestGeminiCrestTile, false, 0),
				((0x37, 1), (66, 8), LocationIds.AliveForest, AccessReqs.AliveForestMobiusCrestTile, false, 0),
				((0x19, 6), (62, 8), LocationIds.WintryTemple, AccessReqs.WintryTempleCrestTile, true, 0),
				(keepWintryTemple ? (0x1A, 6) : (0x8C, 1), (63, 8), LocationIds.SealedTemple, AccessReqs.SealedTempleCrestTile, false, 0), // to short
				((0x1F, 6), (45, 8), LocationIds.Fireburg, AccessReqs.FireburgVendorCrestTile, false, 1), // Fireburg Vendor House
				((0x1E, 6), (54, 8), LocationIds.Fireburg, AccessReqs.FireburgGrenademanCrestTile, false, 2), // Fireburg Grenade Man
				((0x1C, 6), (71, 8), LocationIds.KaidgeTemple, AccessReqs.KaidgeTempleCrestTile, false, 0),
				((0x1B, 6), (70, 8), LocationIds.LightTemple, AccessReqs.LightTempleCrestTile, true, 0),
				((0x18, 6), (44, 8), LocationIds.Windia, AccessReqs.WindiaKidsCrestTile, false, 0),
				((0x17, 6), (43, 8), LocationIds.Windia, AccessReqs.WindiaDockCrestTile, false, 2), // Windia Mobius Old
				((0x20, 6), (61, 8), LocationIds.ShipDock, AccessReqs.ShipDockCrestTile, true, 0),
			};

			List<(AccessReqs, Items)> vanillaCrests = new()
			{
				(AccessReqs.WoodHouseLibraCrestTile, Items.LibraCrest),
				(AccessReqs.WoodHouseGeminiCrestTile, Items.GeminiCrest),
				(AccessReqs.WoodHouseMobiusCrestTile, Items.MobiusCrest),
				(AccessReqs.AquariaVendorCrestTile, Items.GeminiCrest),
				(AccessReqs.LibraTempleCrestTile, Items.LibraCrest),
				(AccessReqs.LifeTempleCrestTile, Items.LibraCrest),
				(AccessReqs.AliveForestLibraCrestTile, Items.LibraCrest),
				(AccessReqs.AliveForestGeminiCrestTile, Items.GeminiCrest),
				(AccessReqs.AliveForestMobiusCrestTile, Items.MobiusCrest),
				(AccessReqs.WintryTempleCrestTile, Items.GeminiCrest),
				(AccessReqs.SealedTempleCrestTile, Items.GeminiCrest),
				(AccessReqs.FireburgVendorCrestTile, Items.GeminiCrest),
				(AccessReqs.FireburgGrenademanCrestTile, Items.MobiusCrest),
				(AccessReqs.KaidgeTempleCrestTile, Items.MobiusCrest),
				(AccessReqs.LightTempleCrestTile, Items.MobiusCrest),
				(AccessReqs.WindiaKidsCrestTile, Items.MobiusCrest),
				(AccessReqs.WindiaDockCrestTile, Items.MobiusCrest),
				(AccessReqs.ShipDockCrestTile, Items.MobiusCrest),
			};

			List<(MapList map, Items crest, byte tile)> crestMapTiles = new()
			{
				(MapList.LevelAliveForest, Items.LibraCrest, 0x52), // +1 for actual tile
				(MapList.LevelAliveForest, Items.GeminiCrest, 0x19),
				(MapList.LevelAliveForest, Items.MobiusCrest, 0x3D),
				(MapList.ShipDock, Items.LibraCrest, 0x53),
				(MapList.ShipDock, Items.GeminiCrest, 0x1A),
				(MapList.ShipDock, Items.MobiusCrest, 0x3E),
				(MapList.HouseInterior, Items.LibraCrest, 0x12),
				(MapList.HouseInterior, Items.GeminiCrest, 0x13),
				(MapList.HouseInterior, Items.MobiusCrest, 0x14),
				(MapList.Caves, Items.LibraCrest, 0x10),
				(MapList.Caves, Items.GeminiCrest, 0x11),
				(MapList.Caves, Items.MobiusCrest, 0x12),
				(MapList.ForestaInterior, Items.LibraCrest, 0x12),
				(MapList.ForestaInterior, Items.GeminiCrest, 0x13),
				(MapList.ForestaInterior, Items.MobiusCrest, 0x14),
			};


			List<(int area, MapList map)> areaToMap = new()
			{
				(17, MapList.ForestaInterior),
				(23, MapList.Caves),
				(27, MapList.HouseInterior),
				(32, MapList.Caves),
				(45, MapList.Caves),
				(46, MapList.Caves),
				(49, MapList.HouseInterior),
				(53, MapList.Caves),
				(67, MapList.LevelAliveForest),
				(77, MapList.Caves),
				(82, MapList.HouseInterior),
				(95, MapList.Caves),
				(96, MapList.ShipDock),
				(17, MapList.ForestaInterior),
			};

			List<(int id, int type)> crestTileList = crestTileTeleporterList.Select(x => x.Item2).ToList();
			var tempentrances = Entrances.SelectMany(x => x.Entrances).ToList();
			var tempccrests = tempentrances.Where(x => crestTileList.Contains((x.TeleportId, x.TeleportType))).Distinct().ToList();
			var temptiles = tempccrests.Select(x => new CrestTile(x)).ToList();



			CrestTiles = Entrances.SelectMany(x => x.Entrances).Where(x => crestTileList.Contains((x.TeleportId, x.TeleportType))).Distinct().Select(x => new CrestTile(x)).ToList();

			foreach (var crest in CrestTiles)
			{
				var teleporterValue = crestTileTeleporterList.Find(x => x.Item2 == crest.EntranceId);
				crest.TargetTeleporter = teleporterValue.Item1;
				crest.Deadend = teleporterValue.deadend;
				var originEntrance = EntrancesLinks.Where(x => (x.EntranceA == crest.EntranceId)).Any() ? EntrancesLinks.Where(x => (x.EntranceA == crest.EntranceId)).First().EntranceB : EntrancesLinks.Where(x => (x.EntranceB == crest.EntranceId)).First().EntranceA;
				crest.OriginTeleporter = crestTileTeleporterList.Find(x => x.Item2 == originEntrance).Item1;
				crest.Priority = teleporterValue.priority;
				crest.Location = teleporterValue.location;
				crest.LocationRequirement = teleporterValue.access;
				crest.Crest = vanillaCrests.Find(x => x.Item1 == teleporterValue.access).Item2;
				crest.Area = Entrances.Find(x => x.Entrances.Where(e => (e.TeleportId == crest.EntranceId.id) && (e.TeleportType == crest.EntranceId.type)).Any()).AreaId;
				crest.Map = areaToMap.Find(x => x.area == crest.Area).map;
			}

			if (flags.CrestShuffle)
			{
				CrestShuffle(rng);
			}

			//List<(int, RoomLink)> linksToProcess = new();
			//List<AccessReqs> crestAcess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };

			foreach (var crest in CrestTiles)
			{
				var entranceToUpdate = Entrances.SelectMany(x => x.Entrances).Where(x => x.TeleportId == crest.EntranceId.id && x.TeleportType == crest.EntranceId.type).ToList();

				foreach (var entrance in entranceToUpdate)
				{
					foreach (var req in entrance.Access)
					{
						req.AddRange(ItemLocations.ItemAccessReq[crest.Crest]);
					}

					var targetTile = crestMapTiles.Find(x => (x.crest == crest.Crest) && (x.map == crest.Map)).tile;
					gameMaps[(int)crest.Map].ModifyMap(entrance.X, entrance.Y, targetTile, true);
				}

				tileScripts.AddScript(crest.EntranceId.id, new ScriptBuilder(new List<string> {
						"2F",
						$"050D{(int)crest.Crest:X2}[03]",
						$"2A1227{crest.TargetTeleporter.id:X2}{crest.TargetTeleporter.type:X2}FFFF",
						"00",
						}));
			}
		}*/
		public void SwapEntrances((int id, int type) entranceA, (int id, int type) entranceB)
		{ 
			/*
			var entrancesListA = Entrances.SelectMany(x => x.Entrances).Where(x => x.TeleportId == entranceA.id && x.TeleportType == entranceA.type).ToList();
			var entrancesListB = Entrances.SelectMany(x => x.Entrances).Where(x => x.TeleportId == entranceB.id && x.TeleportType == entranceB.type).ToList();

			entrancesListA.ForEach(x => x.TeleportId = entranceB.id);
			entrancesListA.ForEach(x => x.TeleportType = entranceB.type);

			entrancesListB.ForEach(x => x.TeleportId = entranceA.id);
			entrancesListB.ForEach(x => x.TeleportType = entranceA.type);
			*/
		}
		public void Write(FFMQRom rom)
		{
			/*
			var resthouseentrance = Rooms.Find(x => x.Name == "Foresta").Entrances.Find(x => x.Id == 7);
			var resthouseexit = Rooms.Find(x => x.Name == "Foresta Houses - Rest House").Entrances.Find(x => x.Id == 6);

			var oldmanhouseentrance = Rooms.Find(x => x.Name == "Foresta").Entrances.Find(x => x.Id == 4);
			var oldmanhouseexit = Rooms.Find(x => x.Name == "Foresta Houses - Old Man's House").Entrances.Find(x => x.Id == 0);

			var temptpid = resthouseentrance.TeleportId;
			var temptype = resthouseentrance.TeleportType;
			resthouseentrance.TeleportId = oldmanhouseentrance.TeleportId;
			resthouseentrance.TeleportType = oldmanhouseentrance.TeleportType;
			oldmanhouseentrance.TeleportId = temptpid;
			oldmanhouseentrance.TeleportType = temptype;

			temptpid = resthouseexit.TeleportId;
			temptype = resthouseexit.TeleportType;
			resthouseexit.TeleportId = oldmanhouseexit.TeleportId;
			resthouseexit.TeleportType = oldmanhouseexit.TeleportType;
			oldmanhouseexit.TeleportId = temptpid;
			oldmanhouseexit.TeleportType = temptype;
			*/

//			var orderedentrances = Entrances.OrderBy(e => e.Area).GroupBy(x => x.Area).Select(x => x.ToList()).ToList();

			ushort currentaddress = 0;
			int currentpointer = 0;
			for (int i = 0; i < EntrancesPointersQty; i++)
			{
				byte[] joineddata = Entrances.Where(x => x.Area == i).SelectMany(x => x.ToBytes()).ToArray();

				if (joineddata.Length > 0)
				{
					rom.PutInBank(NewEntrancesBank, NewEntrancesOffset + currentaddress, joineddata);
				}

				rom.PutInBank(NewEntrancesBank, NewEntrancesPointers + currentpointer, Blob.FromUShorts(new ushort[] { currentaddress }));

				currentaddress += (ushort)joineddata.Length;
				currentpointer += 2;
			}
			/*
			foreach (var area in orderedentrances)
			{ 
				byte[] joineddata = area.SelectMany(x => x.ToBytes()).ToArray();
				rom.PutInBank(NewEntrancesBank, NewEntrancesOffset + currentaddress, joineddata);

				rom.PutInBank(NewEntrancesBank, NewEntrancesPointers + currentpointer, Blob.FromUShorts(new ushort[] { currentaddress }));


				currentaddress += (ushort)joineddata.Length;
				currentpointer += 2;
			}*/
		}

		public void EntranceHack(FFMQRom rom)
		{
			rom.PutInBank(0x01, 0xF37D, Blob.FromHex("eaeaeaea")); // Don't branch on stack teleport
			rom.PutInBank(0x01, 0xF38A, Blob.FromHex("12")); // New Bank
			rom.PutInBank(0x01, 0xF398, Blob.FromHex("00A012")); // New Pointers address
			
			// Hijack instruction
			rom.PutInBank(0x01, 0xF39E, Blob.FromHex("22809F12ABeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaea"));
			rom.PutInBank(0x12, 0x9F80, Blob.FromHex("BCD8A0CC2B19D00EBDDBA08DEF19BDDAA08DEE198007E8E8E8E89810E36B"));

			// set teleport routine 3 (warp) to routine 1 
			rom.PutInBank(0x01, 0xC3AC, Blob.FromHex("E6B2"));
		}

		public void ReadRooms()
		{

			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			//string filepath = "logic.yaml";
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("entrances.yaml"));
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
			List<Entrance> result = new();

			try
			{
				result = deserializer.Deserialize<List<Entrance>>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			Entrances = result;
			//EntrancesLinks = result.EntrancesLinksList.Select(x => new EntrancesLink(x)).ToList();
			yamlfile = "";


		}
		/*
		public string GenerateYaml()
		{

			
			List<Entrance> entrances = Entrances.SelectMany(x => x.Entrances.Select(e => new Entrance(x.AreaId, x.Name, e))).ToList();
			entrances = entrances.Select((x, i) => new Entrance(i, x)).ToList();

			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.WithEventEmitter(next => new FlowStyleIntegerSequences(next))
//				.WithAttributeOverride<Entrance>(e => e.Coordinates,
//				new YamlMemberAttribute
//				{
//					//ScalarStyle = YamlDotNet.Core.ScalarStyle.Folded,
//					SerializeAs = typeof(int[])
//				}
//				)
				.Build();
			var yaml = serializer.Serialize(entrances);
			//var lineyaml = yaml.Split('\n');

			return yaml;
		}*/
	}

	class FlowStyleIntegerSequences : ChainedEventEmitter
	{
		public FlowStyleIntegerSequences(IEventEmitter nextEmitter)
			: base(nextEmitter) { }

		public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
		{
			if (typeof(IEnumerable<int>).IsAssignableFrom(eventInfo.Source.Type))
			{
				eventInfo = new SequenceStartEventInfo(eventInfo.Source)
				{
					Style = SequenceStyle.Flow
				};
			}

			nextEmitter.Emit(eventInfo, emitter);
		}
	}
	public class TupleArrayConverter : IYamlTypeConverter
	{
		public bool Accepts(Type type)
		{
			return type == typeof((int, int));
		}

		public object ReadYaml(IParser parser, Type type)
		{
			var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
			var bytes = (scalar.Value);
			parser.MoveNext();
			return bytes;
		}

		public void WriteYaml(IEmitter emitter, object value, Type type)
		{
			var bytes = (byte[])value;
			emitter.Emit(new YamlDotNet.Core.Events.Scalar(
				null,
				"tag:yaml.org,2002:binary",
				Convert.ToBase64String(bytes),
				ScalarStyle.Plain,
				false,
				false
			));
		}
	}

	public partial class FFMQRom : SnesRom
	{
		/*
		 * 
		 * 		public const int ExitTilesCoordPointers = 0x05F920;
		public const int ExitTilesCoord = 0x05FB3C;
		public const int ExitsCoord = 0x05F4A0;
		
		 */

		public class ExitList
		{
			private List<List<MapObject>> _collections = new List<List<MapObject>>();
			private List<Exit> _exits = new();
			private List<int> _exitcoordqty = new();
			private List<ExitTile> _exittiles = new();
			private Dictionary<int, int> _exitpairs = new();
			public ExitList(FFMQRom rom)
			{
				for (int i = 0; i < RomOffsets.ExitsCoordQty; i++)
				{
					_exits.Add(new Exit(i, rom));
				}

				for (int i = 0; i < RomOffsets.ExitTilesCoordPointersQty - 1; i++)
				{
					var rawlowervalue = rom.Get(RomOffsets.ExitTilesCoordPointers + i * 2, 2);
					var lowervalue = rawlowervalue[1] * 0x100 + rawlowervalue[0];
					var rawuppervalue = rom.Get(RomOffsets.ExitTilesCoordPointers + (i+1) * 2, 2);
					var uppervalue = rawuppervalue[1] * 0x100 + rawuppervalue[0];

					_exitcoordqty.Add((int)((uppervalue - lowervalue) / 3));

					for (int j = 0; j < _exitcoordqty.Last(); j++)
					{
						_exittiles.Add(new ExitTile(i, j, rom));
					}
				}

				_exitcoordqty.Add(0);
			}

			public void ExitDataDump()
			{
				foreach (var exit in _exits)
				{ 
					string myStringOutput = "Target: " + exit.TargetArea.ToString("X2") + "; X: " + exit.TargetX.ToString("X2") + "; Y: " + exit.TargetY.ToString("X2");
					Console.WriteLine(myStringOutput);
				}

				Console.WriteLine("-----------------");

				foreach (var exittile in _exittiles)
				{
					string myStringOutput = "ExitID: " + exittile.TargetExit.ToString("X2") + "; X: " + exittile.X.ToString("X2") + "; Y: " + exittile.Y.ToString("X2");
					Console.WriteLine(myStringOutput);
				}

			}

			public List<ExitTile> GetAreaExitTiles(int mapid, MapUtilities maputilities)
			{
				return _exittiles.Where(x => maputilities.AreaIdToMapId(x.Area) == mapid).ToList();
			}

			public void TileDataDump()
			{
				foreach (var exittile in _exittiles)
				{
					string myStringOutput = "Map: " + exittile.TargetExit.ToString("X2") + "; X: " + exittile.X.ToString("X2") + "; Y: " + exittile.Y.ToString("X2");
					Console.WriteLine(myStringOutput);
				}
			}
			public void WriteAll(FFMQRom rom)
			{
				foreach (var collection in _collections)
				{
					foreach (var mapobject in collection)
					{
						mapobject.Write(rom);
					}
				}
			}
			public List<MapObject> this[int floorid]
			{
				get => _collections[floorid];
				set => _collections[floorid] = value;
			}
		}

		public class Exit
		{
			private List<byte> _array = new();
			private int _id;

			public byte TargetX { get; set; }
			public byte TargetY { get; set; }
			public byte TargetArea { get; set; }

			public Exit(int id, FFMQRom rom)
			{
				_id = id;
				_array = rom.Get(RomOffsets.ExitsCoord + id * 3, 3).ToBytes().ToList();

				TargetArea = _array[0];
				TargetX = _array[1];
				TargetY = _array[2];
			}

			public void Write(FFMQRom rom)
			{
				_array[0] = TargetArea;
				_array[1] = TargetX;
				_array[2] = TargetY;

				rom.Put(RomOffsets.ExitsCoord + _id * 3, _array.ToArray());
			}
		}
		public class ExitTile
		{
			private List<byte> _array = new();
			private int _index;
			private int _floor;
			private int _pointer;

			public byte X { get; set; }
			public byte Y { get; set; }
			public byte TargetExit { get; set; }
			public byte Area => (byte)_floor;

			public ExitTile(int floor, int index, FFMQRom rom)
			{
				_index = index;
				_floor = floor;
				var rawpointer = rom.Get(RomOffsets.ExitTilesCoordPointers + _floor * 2, 2);
				_pointer = rawpointer[1] * 0x100 + rawpointer[0];

				_array = rom.Get(RomOffsets.ExitTilesCoord + _pointer + _index * 3, 3).ToBytes().ToList();

				X = _array[0];
				Y = _array[1];
				TargetExit = _array[2];
			}

			public void Write(FFMQRom rom)
			{
				_array[0] = X;
				_array[1] = Y;
				_array[2] = TargetExit;

				rom.Put(RomOffsets.ExitTilesCoord + _pointer + _index * 3, _array.ToArray());
			}
		}

	}
}
