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
	public class LocationStructure
	{
		public List<Room> Rooms {get; set;}
		public List<EntrancesLink> EntrancesLinks { get; set; }
		public List<EntrancesLinkList> EntrancesLinksList { get; set; }
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
			/*
			EntrancesLinksList.Add(new EntrancesLinkList(new List<int> { 1, 8 }, new List<int> { 1, 2 }));
			EntrancesLinksList.Add(new EntrancesLinkList(new List<int> { 2, 8 }, new List<int> { 5, 2 }));*/
			//EntrancesLinks.Add(new EntrancesLink((1, 8), (1, 2)));
			//EntrancesLinks.Add(new EntrancesLink((2, 8), (5, 2)));


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
