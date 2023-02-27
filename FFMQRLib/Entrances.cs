using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FFMQLib
{
	public enum EntranceType
	{ 
		Standard = 0,
		Overworld
	}
	
	public class Entrance
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int Area { get; set; }
		public EntranceType Type { get; set; }
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
			Type = EntranceType.Standard;
		}
		public Entrance(string name, int id, int x, int y, int teleid, int teletype)
		{
			Name = name;
			Id = id;
			Coordinates = (x, y);
			Teleporter = (teleid, teletype);
			Area = 0;
			Type = EntranceType.Standard;
		}

		public Entrance(string name, int id, int area, byte[] data)
		{
			Name = name;
			Id = id;
			Coordinates = (data[0], data[1]);
			Teleporter = (data[2], 0);
			Area = area;
		}
		public Entrance(int id, byte[] data)
		{
			Name = "OwEntrance";
			Id = id;
			Coordinates = (0, 0);
			Teleporter = (data[0], data[1]);
			Area = 0;
		}
		public Entrance(int id, Entrance entrance)
		{
			Name = entrance.Name;
			Id = id;
			Coordinates = entrance.Coordinates;
			Teleporter = entrance.Teleporter;
			Area = entrance.Area;
			Type = entrance.Type;
		}
		public byte[] ToBytes()
		{
			if (Type == EntranceType.Standard)
			{
				return new byte[] { (byte)Coordinates.x, (byte)Coordinates.y, (byte)Teleporter.id, (byte)Teleporter.type };
			}
			else
			{
				return new byte[] { (byte)Teleporter.id, (byte)Teleporter.type };
			}
		}
		public byte[] OwToBytes()
		{
			return new byte[] { (byte)Teleporter.id, (byte)Teleporter.type };
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
		public List<Entrance> OwEntrances { get; set; }
		public List<(AccessReqs crest, AccessReqs loc1, AccessReqs loc2)> CrestPairs { get; set; }

		private const int EntrancesBank = 0x05;
		private const int EntrancesPointers = 0xF920;
		private const int EntrancesPointersQty = 108;
		private const int EntrancesOffset = 0xF9F8;

		private const int NewEntrancesBank = 0x12;
		private const int NewEntrancesPointers = 0xA000;
		private const int NewEntrancesOffset = 0xA0D8;

		private const int OwEntrancesBank = 0x07;
		private const int OwEntrancesOffset = 0xEFCB;
		private const int OwEntrancesQty = 0x22;

		public EntrancesData()
		{
			Entrances = new();
			CrestPairs = new();
		}
		public EntrancesData(FFMQRom rom)
		{
			Entrances = new();

			ReadDataFile();
			//ReadOwEntrances(rom);
		}
		private void ReadOwEntrances(FFMQRom rom)
		{
			OwEntrances = rom.GetFromBank(OwEntrancesBank, OwEntrancesOffset, OwEntrancesQty * 2).Chunk(2).Select((x, i) => new Entrance(i + 0x16, x)).ToList();
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
		public void UpdateCrests(Flags flags, GameScriptManager tileScripts, GameMaps gameMaps, GameLogic logic, List<Teleporter> teleportersLong, MT19337 rng)
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
				new CrestTile(397, (0x20, 6), (61, 8), MapList.ShipDock),
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

			var flatLinkList = logic.Rooms.SelectMany(x => x.Links).ToList();

			foreach (var crest in crestsList)
			{
				var entranceToUpdate = Entrances.Where(x => x.Id == crest.Entrance).ToList();
				var updatedLink = flatLinkList.Find(x => x.Entrance == crest.Entrance);
				var targetLocation = logic.Rooms.Find(x => x.Id == logic.CrestRoomLinks.Find(x => x.Teleporter == crest.Script).Room).Location;
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

				if (newTeleporter.type == 6)
				{
					teleportersLong.Find(x => x.Id == newTeleporter.id).TargetLocation = (int)targetLocation;
				}
			}
		}

		public void UpdatEntrance(Flags flags, List<Room> rooms, MT19337 rng)
		{
			var flatLinkList = rooms.SelectMany(x => x.Links.Where(l => l.Entrance > 0)).ToList();
			List<(int, (int, int))> entrancesProcessList = new();

			foreach (var link in flatLinkList)
			{
				var teleporterToUpdate = Entrances.Find(x => x.Id == link.Entrance).Teleporter;
				var entrancesToUpdate = Entrances.Where(x => x.Teleporter == teleporterToUpdate).ToList();

				entrancesProcessList.AddRange(entrancesToUpdate.Select(x => (x.Id, link.Teleporter)).ToList());
			}
			
			foreach (var entrance in entrancesProcessList)
			{
				Entrances.Find(x => x.Id == entrance.Item1).Teleporter = entrance.Item2;
			}

			if (flags.FloorShuffle)
			{
				UpdateVolcano();
			}
		}
		private void UpdateVolcano()
		{
			List<((int id, int type), (int id, int type))> volcanoRealTeleporters = new()
			{
				((15, 8), (0x88, 1)),
				((26, 8), (0x8B, 1)),
				((27, 8), (0x89, 1)),
				((28, 8), (0x1C, 2)),
				((29, 8), (0x1B, 2)),
				((30, 8), (0x18, 2)),
				((31, 8), (0x17, 2)),
				((79, 8), (0x8A, 1)),
			};

			foreach (var realTeleporter in volcanoRealTeleporters)
			{ 
				var entrancesToUpdate = Entrances.Where(x => x.Teleporter == realTeleporter.Item1).ToList();
				entrancesToUpdate.ForEach(x => x.Teleporter = realTeleporter.Item2);
			}

			Entrances.Find(x => x.Id == 464).Teleporter = (15, 8);
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
				byte[] joineddata = Entrances.Where(x => x.Area == i && x.Type == EntranceType.Standard).SelectMany(x => x.ToBytes()).ToArray();

				if (joineddata.Length > 0)
				{
					rom.PutInBank(NewEntrancesBank, NewEntrancesOffset + currentaddress, joineddata);
				}

				rom.PutInBank(NewEntrancesBank, NewEntrancesPointers + currentpointer, Blob.FromUShorts(new ushort[] { currentaddress }));

				currentaddress += (ushort)joineddata.Length;
				currentpointer += 2;
			}

			rom.PutInBank(OwEntrancesBank, OwEntrancesOffset, Entrances.Where(x => x.Type == EntranceType.Overworld).OrderBy(x => x.Id).SelectMany(x => x.ToBytes()).ToArray());
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

