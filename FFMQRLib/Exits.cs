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
	public class Entrance
	{ 
		public string Name { get; set; }
		public int Id { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int TeleportId { get; set; }
		public int TeleportType { get; set; }
		public List<List<AccessReqs>> Access { get; set; }
		public Entrance()
		{
			Name = "void";
			Id = 0;
			X = 0;
			Y = 0;
			TeleportId = 0;
			TeleportType = 0;
			Access = new();
		}
		public Entrance(string name, int id, int x, int y, int teleid, int teletype)
		{
			Name = name;
			Id = id;
			X = x;
			Y = y;
			TeleportId = teleid;
			TeleportType = teletype;
			Access = new List<List<AccessReqs>>();
		}

		public Entrance(string name, int id, byte[] data)
		{
			Name = name;
			Id = id;
			X = data[0];
			Y = data[1];
			TeleportId = data[2];
			TeleportType = 0;
			Access = new List<List<AccessReqs>>();
		}

		public byte[] ToBytes()
		{
			return new byte[] { (byte)X, (byte)Y, (byte)TeleportId, (byte)TeleportType };
		}

	}
	public class Room
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int AreaId { get; set; }
		public List<TreasureObject> Treasures { get; set; }
		public List<Entrance> Entrances { get; set; }
		public Room(string name, int id, int area, List<TreasureObject> treasures, List<Entrance> entrances)
		{
			Name = name;
			Id = id;
			AreaId = area;
			Treasures = treasures; // shallowcopy?
			Entrances = entrances;
		}
		public Room()
		{
			Name = "void";
			Id = 0;
			AreaId = 0;
			Treasures = new();
			Entrances = new();
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
		public (int id, int type) EntranceId { get; set; }
		public Entrance Entrance { get; set; }
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
			Entrance = entrance;
			EntranceId = (entrance.TeleportId, entrance.TeleportType);
			Position = (entrance.X, entrance.Y);
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

	public class LocationStructure
	{
		public List<Room> Rooms {get; set;}
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
			Rooms = new();
			EntrancesLinks = new();
			EntrancesLinksList = new();
			CrestTiles = new();
			CrestPairs = new();
		}
		public LocationStructure(FFMQRom rom)
		{
			Rooms = new();
			
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

					//LocationIds.Last().Entrances.Add(new Entrance("void", currentEntrance, entrance));
					tempEntrances.Add(new Entrance("void", currentEntrance, entrance));

					currentAddress += 3;
					currentEntrance++;
				}

				Rooms.Add(new Room("void", i, i, new List<TreasureObject>(), tempEntrances));
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
				Items.LibraCrest,
				Items.GeminiCrest,
				Items.GeminiCrest,
				//Items.GeminiCrest, Spencer's cave crests
				//Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
			};

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

				CrestTiles.Add(crest1);
				CrestTiles.Add(crest2);
				CrestPairs.Add((ItemLocations.ItemAccessReq[crest1.Crest][0], crest1.LocationRequirement, crest2.LocationRequirement));
			}
		}
		public void UpdateCrests(Flags flags, GameScriptManager tileScripts, GameMaps gameMaps, MT19337 rng)
		{
			bool shuffle = true;
			bool keepWintryTemple = !(flags.ShuffleEntrances || shuffle);
			
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


			CrestTiles = Rooms.SelectMany(x => x.Entrances).Where(x => crestTileList.Contains((x.TeleportId, x.TeleportType))).Distinct().Select(x => new CrestTile(x)).ToList();

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
				crest.Area = Rooms.Find(x => x.Entrances.Where(e => (e.TeleportId == crest.EntranceId.id) && (e.TeleportType == crest.EntranceId.type)).Any()).AreaId;
				crest.Map = areaToMap.Find(x => x.area == crest.Area).map;
			}

			if (shuffle)
			{
				CrestShuffle(rng);
			}

			foreach (var crest in CrestTiles)
			{
				var entranceToUpdate = Rooms.SelectMany(x => x.Entrances).Where(x => x.TeleportId == crest.EntranceId.id && x.TeleportType == crest.EntranceId.type).ToList();

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
		}
		public void SwapEntrances((int id, int type) entranceA, (int id, int type) entranceB)
		{ 
			var entrancesListA = Rooms.SelectMany(x => x.Entrances).Where(x => x.TeleportId == entranceA.id && x.TeleportType == entranceA.type).ToList();
			var entrancesListB = Rooms.SelectMany(x => x.Entrances).Where(x => x.TeleportId == entranceB.id && x.TeleportType == entranceB.type).ToList();

			entrancesListA.ForEach(x => x.TeleportId = entranceB.id);
			entrancesListA.ForEach(x => x.TeleportType = entranceB.type);

			entrancesListB.ForEach(x => x.TeleportId = entranceA.id);
			entrancesListB.ForEach(x => x.TeleportType = entranceA.type);
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

			var idrooms = Rooms.Select(r => (r.AreaId, r.Entrances)).ToList();
			List<List<Entrance>> orderedentrances = new();

			for (int i = 0; i < EntrancesPointersQty; i++)
			{
				orderedentrances.Add(Rooms.Where(r => r.AreaId == i).SelectMany(r => r.Entrances).ToList());
			}

			ushort currentaddress = 0;
			int currentpointer = 0;
			foreach (var area in orderedentrances)
			{ 
				byte[] joineddata = area.SelectMany(x => x.ToBytes()).ToArray();
				rom.PutInBank(NewEntrancesBank, NewEntrancesOffset + currentaddress, joineddata);

				rom.PutInBank(NewEntrancesBank, NewEntrancesPointers + currentpointer, Blob.FromUShorts(new ushort[] { currentaddress }));


				currentaddress += (ushort)joineddata.Length;
				currentpointer += 2;
			}
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
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("locations.yaml"));
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
			LocationStructure result = new();

			try
			{
				result = deserializer.Deserialize<LocationStructure>(yamlfile);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			
			Rooms = result.Rooms;
			EntrancesLinks = result.EntrancesLinksList.Select(x => new EntrancesLink(x)).ToList();
			yamlfile = "";


		}

		public string GenerateYaml()
		{
			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();
			var yaml = serializer.Serialize(this);
			//var lineyaml = yaml.Split('\n');

			return yaml;
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
