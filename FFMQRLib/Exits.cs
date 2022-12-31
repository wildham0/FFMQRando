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
	public enum FacingOrientation
	{ 
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3
	}
	
	
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

	public class Teleporter
	{
		public int Id { get; set; }
		public int TargetX { get; set; }
		public int TargetY { get; set; }
		public int TargetArea { get; set; }
		public int TargetLocation { get; set; }
		public FacingOrientation Orientation { get; set; }

		public Teleporter()
		{
			Id = 0;
			TargetX = 0;
			TargetY = 0;
			TargetArea = 0;
			TargetLocation = 0;
			Orientation = FacingOrientation.Down;
		}
		public Teleporter(int id, int x, int y, FacingOrientation orientation, int area, int loc)
		{
			Id = id;
			TargetX = x;
			TargetY = y;
			TargetArea = area;
			TargetLocation = loc;
			Orientation = orientation;
		}
		public Teleporter(int id, byte[] data)
		{
			Id = id;
			if (data.Length == 3)
			{
				TargetX = (data[2] & 0x3F);
				TargetY = data[1];
				TargetArea = data[0];
				TargetLocation = 0;
				Orientation = (FacingOrientation)((data[2] / 64) & 0x03);
			}
			else
			{
				TargetX = data[3];
				TargetY = data[2];
				TargetArea = data[0];
				TargetLocation = data[1];
				Orientation = (FacingOrientation)((data[3] / 64) & 0x03);
			}
		}

		public byte[] ToBytesShort()
		{
			return new byte[] { (byte)TargetArea, (byte)TargetY, (byte)(TargetX | ((int)Orientation * 64))};
		}
		public byte[] ToBytesLong()
		{
			return new byte[] { (byte)TargetArea, (byte)TargetLocation, (byte)TargetY, (byte)(TargetX | ((int)Orientation * 64)) };
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

	public class Teleporters
	{ 
		public List<Teleporter> TeleportersA { get; set; }
		public List<Teleporter> TeleportersB { get; set; }
		public List<Teleporter> TeleportersWarp { get; set; }
		public List<Teleporter> TeleportersLong { get; set; }

		public const int TeleportersBank = 0x05;
		public const int TeleportersQtyA = 217;
		public const int TeleportersQtyB = 86;
		public const int TeleportersQtyWarp = 37;
		public const int TeleportersQtyLong = 33;
		public const int TeleportersOffsetA = 0xF4A0;
		public const int TeleportersOffsetB = 0xF79A;
		public const int TeleportersOffsetWarp = 0xF72B;
		public const int TeleportersOffsetLong = 0xF89C;

		public const int NewTeleportersBank = 0x12;
		public const int NewTeleportersOffset = 0xAA00;


		public Teleporters(FFMQRom rom)
		{
			TeleportersA = rom.GetFromBank(TeleportersBank, TeleportersOffsetA, TeleportersQtyA * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersB = rom.GetFromBank(TeleportersBank, TeleportersOffsetB, TeleportersQtyB * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersWarp = rom.GetFromBank(TeleportersBank, TeleportersOffsetWarp, TeleportersQtyWarp * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersLong = rom.GetFromBank(TeleportersBank, TeleportersOffsetLong, TeleportersQtyLong * 4).Chunk(4).Select((x, i) => new Teleporter(i, x)).ToList();
		}
		public void ExtraTeleporters()
		{
			TeleportersWarp.Add(new Teleporter(37, 0x07, 0x16, FacingOrientation.Down, 0x0F, 0x17)); // Kaeli's House
			TeleportersWarp.Add(new Teleporter(38, 0x17, 0x18, FacingOrientation.Down, 0x0F, 0x17)); // Rest House
			TeleportersWarp.Add(new Teleporter(39, 0x0D, 0x27, FacingOrientation.Down, 0x13, 0x19)); // Bone Dungeon 1F
			TeleportersWarp.Add(new Teleporter(40, 0x1B, 0x27, FacingOrientation.Down, 0x14, 0x19)); // Bone Dungeon B1 - Waterway
			TeleportersWarp.Add(new Teleporter(41, 0x17, 0x28, FacingOrientation.Up, 0x14, 0x19)); // Bone Dungeon B1 - Checker Room
			TeleportersWarp.Add(new Teleporter(42, 0x13, 0x0D, FacingOrientation.Left, 0x15, 0x19)); // Bone Dungeon B2 - From Hidden Room
			TeleportersWarp.Add(new Teleporter(43, 0x1D, 0x0F, FacingOrientation.Up, 0x15, 0x19)); // Bone Dungeon B2 - From Two Skulls
			TeleportersWarp.Add(new Teleporter(44, 0x35, 0x07, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Box Room
			TeleportersWarp.Add(new Teleporter(45, 0x29, 0x03, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Quake Room
			TeleportersWarp.Add(new Teleporter(46, 0x2F, 0x39, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Flamerex Room
		}
		public void Write(FFMQRom rom)
		{
			ushort teleportersOffsetA = NewTeleportersOffset;
			ushort teleportersOffsetB = (ushort)(teleportersOffsetA + (TeleportersA.Count * 3));
			ushort teleportersOffsetWarp = (ushort)(teleportersOffsetB + (TeleportersB.Count * 3));
			ushort teleportersOffsetLong = (ushort)(teleportersOffsetWarp + (TeleportersWarp.Count * 3));

			rom.PutInBank(NewTeleportersBank, teleportersOffsetA, TeleportersA.SelectMany(x => x.ToBytesShort()).ToArray());
			rom.PutInBank(NewTeleportersBank, teleportersOffsetB, TeleportersB.SelectMany(x => x.ToBytesShort()).ToArray());
			rom.PutInBank(NewTeleportersBank, teleportersOffsetWarp, TeleportersWarp.SelectMany(x => x.ToBytesShort()).ToArray());
			rom.PutInBank(NewTeleportersBank, teleportersOffsetLong, TeleportersLong.SelectMany(x => x.ToBytesLong()).ToArray());

			rom.PutInBank(0x01, 0xB29E, Blob.FromUShorts(new ushort[] { teleportersOffsetA }));
			rom.PutInBank(0x01, 0xB2BD, Blob.FromHex("12"));
			rom.PutInBank(0x01, 0xB2E7, Blob.FromUShorts(new ushort[] { teleportersOffsetB }));

			rom.PutInBank(0x01, 0xB302, Blob.FromUShorts(new ushort[] { teleportersOffsetWarp }) + Blob.FromHex("12"));
			rom.PutInBank(0x01, 0xB309, Blob.FromUShorts(new ushort[] { (ushort)(teleportersOffsetWarp + 1) }) + Blob.FromHex("12"));
			rom.PutInBank(0x01, 0xB310, Blob.FromUShorts(new ushort[] { (ushort)(teleportersOffsetWarp + 2) }) + Blob.FromHex("12"));
			rom.PutInBank(0x01, 0xB319, Blob.FromUShorts(new ushort[] { (ushort)(teleportersOffsetWarp + 2) }) + Blob.FromHex("12"));

			rom.PutInBank(0x01, 0xB35E, Blob.FromHex("12"));
			rom.PutInBank(0x01, 0xB355, Blob.FromUShorts(new ushort[] { teleportersOffsetLong }));
		}
	}

	public class LocationStructure
	{
		public List<Room> Rooms {get; set;}
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
		}
		public LocationStructure(FFMQRom rom)
		{
			Rooms = new();
			
			for (int i = 0; i < 0x6C; i++)
			{
				//int currentAddress = EntrancesOffset;
				int currentEntrance = 0;
				List<Entrance> tempEntrances = new();

				//Locations.Add(new Location("void", i, new List<TreasureObject>(), new List<Entrance>()));

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

					//Locations.Last().Entrances.Add(new Entrance("void", currentEntrance, entrance));
					tempEntrances.Add(new Entrance("void", currentEntrance, entrance));

					currentAddress += 3;
					currentEntrance++;
				}

				Rooms.Add(new Room("void", i, i, new List<TreasureObject>(), tempEntrances));
			}
		}

		public void Write(FFMQRom rom)
		{

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
			
			//rom.PutInBank(0x01, 0xF398, Blob.FromHex("00A012")); // New Pointers address

			// Hijack instruction
			rom.PutInBank(0x01, 0xF39E, Blob.FromHex("22809F12ABeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaeaea"));
			rom.PutInBank(0x12, 0x9F80, Blob.FromHex("BCD8A0CC2B19D00EBDDBA08DEF19BDDAA08DEE198007E8E8E8E89810E36B"));

			// set teleport routine 3 (warp) to routine 2 (set warp+teleport)
			rom.PutInBank(0x01, 0xC3AC, Blob.FromHex("EBB2"));
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
			yaml.Load(input);
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
