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
			EntranceA = entrancea.GetRange(0, 2);
			EntranceB = entranceb.GetRange(0, 2);
		}
	}

	public class EntrancesLink
	{
		public (int id, int type) EntranceA { get; set; }
		public (int id, int type) EntranceB { get; set; }

		public EntrancesLink()
		{
			EntranceA = (0, 0);
			EntranceB = (0, 0);
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
		public int Entrance { get; set; }
		public (int id, int type) TargetTeleporter { get; set; }
		public (int id, int type) Script { get; set; }
		public Items Crest { get; set; }
		public MapList Map { get; set; }

		public CrestTile(int entrance, (int, int) target, (int, int) script, MapList map)
		{
			Entrance = entrance;
			TargetTeleporter = target;
			Script = script;
			Map = map;
			Crest = Items.LibraCrest;
		}
	}

	public class EntrancesData
	{
		public List<Entrance> Entrances { get; set; }
		public List<EntrancesLink> EntrancesLinks { get; set; }
		public List<EntrancesLinkList> EntrancesLinksList { get; set; }
		public List<(AccessReqs crest, AccessReqs loc1, AccessReqs loc2)> CrestPairs { get; set; }

		public const int EntrancesBank = 0x05;
		public const int EntrancesPointers = 0xF920;
		public const int EntrancesPointersQty = 108;
		public const int EntrancesOffset = 0xF9F8;

		public const int NewEntrancesBank = 0x12;
		public const int NewEntrancesPointers = 0xA000;
		public const int NewEntrancesOffset = 0xA0D8;

		public EntrancesData()
		{
			Entrances = new();
			EntrancesLinks = new();
			EntrancesLinksList = new();
			CrestPairs = new();
		}
		public EntrancesData(FFMQRom rom)
		{
			Entrances = new();

			ReadDataFile();

			EntrancesLinks = new();
			EntrancesLinksList = new();
		}
		public void ReadFromRom(FFMQRom rom)
		{
			Entrances = new();

			for (int i = 0; i < 0x6C; i++)
			{
				int currentEntrance = 0;
				List<Entrance> tempEntrances = new();

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
			}
		}
		public void UpdateCrests(Flags flags, GameScriptManager tileScripts, GameMaps gameMaps, List<Room> rooms, MT19337 rng)
		{
			bool keepWintryTemple = !(flags.OverworldShuffle || flags.CrestShuffle);

			List<CrestTile> crestsList = new()
			{
				new CrestTile(51, (0x21, 6), (67, 8), MapList.ForestaInterior),
				new CrestTile(52, (0x22, 6), (68, 8), MapList.ForestaInterior),
				new CrestTile(53, (0x23, 6), (69, 8), MapList.ForestaInterior),
				new CrestTile(96, (0x1D, 6), (72, 8), MapList.HouseInterior),
				new CrestTile(76, (0x15, 6), (59, 8), MapList.Caves),
				new CrestTile(108, (0x16, 6), (60, 8), MapList.Caves),
				new CrestTile(275, (0x35, 1), (64, 8), MapList.LevelAliveForest),
				new CrestTile(276, (0x36, 1), (65, 8), MapList.LevelAliveForest),
				new CrestTile(277, (0x37, 1), (66, 8), MapList.LevelAliveForest),
				new CrestTile(158, (0x19, 6), (62, 8), MapList.Caves),
				new CrestTile(191, keepWintryTemple ? (0x1A, 6) : (0x8C, 1), (63, 8), MapList.Caves),
				new CrestTile(175, (0x1F, 6), (45, 8), MapList.HouseInterior),
				new CrestTile(171, (0x1E, 6), (54, 8), MapList.HouseInterior),
				new CrestTile(308, (0x1C, 6), (71, 8), MapList.Caves),
				new CrestTile(396, (0x1B, 6), (70, 8), MapList.Caves),
				new CrestTile(334, (0x18, 6), (44, 8), MapList.HouseInterior),
				new CrestTile(336, (0x17, 6), (43, 8), MapList.HouseInterior),
				new CrestTile(397, (0x20, 6), (61, 8), MapList.ForestaInterior),
			};

			List<AccessReqs> crestAccess = new() { AccessReqs.LibraCrest, AccessReqs.GeminiCrest, AccessReqs.MobiusCrest };
			List<(Items, AccessReqs)> crestItemAccess = new()
			{
				(Items.LibraCrest, AccessReqs.LibraCrest),
				(Items.GeminiCrest, AccessReqs.GeminiCrest),
				(Items.MobiusCrest, AccessReqs.MobiusCrest)
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
		}

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

			// Modify how entrances work
			EntranceHack(rom);

			// Write Entrances
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
		public void ReadDataFile()
		{
			string yamlfile = "";
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("entrances.yaml"));
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

		public string GenerateYaml()
		{
			//List<Entrance> entrances = Entrances.SelectMany(x => x.Entrances.Select(e => new Entrance(x.AreaId, x.Name, e))).ToList();
			var entrances = Entrances.Select((x, i) => new Entrance(i, x)).ToList();

			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.WithEventEmitter(next => new FlowStyleIntegerSequences(next))
				.Build();
			var yaml = serializer.Serialize(entrances);

			return yaml;
		}
	}
}

