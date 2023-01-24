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
			TeleportersB.Add(new Teleporter(86, 0x07, 0x16, FacingOrientation.Down, 0x0F, 0x17)); // Kaeli's House
			TeleportersB.Add(new Teleporter(87, 0x17, 0x18, FacingOrientation.Down, 0x0F, 0x17)); // Rest House
			TeleportersB.Add(new Teleporter(88, 0x0D, 0x27, FacingOrientation.Down, 0x13, 0x19)); // Bone Dungeon 1F
			TeleportersB.Add(new Teleporter(89, 0x1B, 0x27, FacingOrientation.Down, 0x14, 0x19)); // Bone Dungeon B1 - Waterway
			TeleportersB.Add(new Teleporter(90, 0x17, 0x28, FacingOrientation.Up, 0x14, 0x19)); // Bone Dungeon B1 - Checker Room
			TeleportersB.Add(new Teleporter(91, 0x13, 0x0D, FacingOrientation.Left, 0x15, 0x19)); // Bone Dungeon B2 - From Hidden Room
			TeleportersB.Add(new Teleporter(92, 0x1D, 0x0F, FacingOrientation.Up, 0x15, 0x19)); // Bone Dungeon B2 - From Two Skulls
			TeleportersB.Add(new Teleporter(93, 0x35, 0x07, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Box Room
			TeleportersB.Add(new Teleporter(94, 0x29, 0x03, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Quake Room
			TeleportersB.Add(new Teleporter(95, 0x2F, 0x39, FacingOrientation.Down, 0x15, 0x19)); // Bone Dungeon B2 - From Flamerex Room
			TeleportersB.Add(new Teleporter(96, 0x28, 0x19, FacingOrientation.Down, 0x1C, 0x1E)); // Wintry Cave - From 3F
			TeleportersB.Add(new Teleporter(97, 0x0A, 0x2B, FacingOrientation.Down, 0x1C, 0x1E)); // Wintry Cave - From 2F
			TeleportersB.Add(new Teleporter(98, 0x0E, 0x06, FacingOrientation.Down, 0x2F, 0x26)); // Fireburg - From Reuben's House
			TeleportersB.Add(new Teleporter(99, 0x14, 0x08, FacingOrientation.Down, 0x2F, 0x26)); // Fireburg - From Hotel
			TeleportersB.Add(new Teleporter(100, 0x08, 0x07, FacingOrientation.Down, 0x32, 0x27)); // Mine - From Parallel Room
			TeleportersB.Add(new Teleporter(101, 0x1A, 0x0F, FacingOrientation.Down, 0x32, 0x27)); // Mine - From Crescent Room
			TeleportersB.Add(new Teleporter(102, 0x15, 0x23, FacingOrientation.Down, 0x32, 0x27)); // Mine - From Climbing Room
			TeleportersB.Add(new Teleporter(103, 0x28, 0x1F, FacingOrientation.Down, 0x38, 0x29)); // Volcano - From Cross Left-Right
			TeleportersB.Add(new Teleporter(104, 0x34, 0x1D, FacingOrientation.Down, 0x38, 0x29)); // Volcano - From Cross Right-Left
			TeleportersB.Add(new Teleporter(105, 0x3F, 0x15, FacingOrientation.Down, 0x39, 0x2A)); // Lava Dome - From Hydra Room
			TeleportersB.Add(new Teleporter(106, 0x15, 0x28, FacingOrientation.Down, 0x50, 0x30)); // Windia - Otto's House
			TeleportersB.Add(new Teleporter(107, 0x02, 0x13, FacingOrientation.Down, 0x51, 0x30)); // Windia - Otto's Attic
			TeleportersB.Add(new Teleporter(108, 0x08, 0x25, FacingOrientation.Down, 0x50, 0x30)); // Windia - Vendor House
			TeleportersB.Add(new Teleporter(135, 0x12, 0x23, FacingOrientation.Down, 0x50, 0x30)); // Windia - INN
			TeleportersB.Add(new Teleporter(109, 0x3B, 0x15, FacingOrientation.Down, 0x65, 0x35)); // Doom Castle - Ice Floor
			TeleportersB.Add(new Teleporter(110, 0x3B, 0x3D, FacingOrientation.Down, 0x65, 0x35)); // Doom Castle - Hero Room
			TeleportersB.Add(new Teleporter(111, 0x08, 0x13, FacingOrientation.Down, 0x18, 0x1D)); // Aquaria Winter - From Phoebe's House
			TeleportersB.Add(new Teleporter(112, 0x28, 0x14, FacingOrientation.Down, 0x19, 0x1D)); // Aquaria Summer - From Phoebe's House
			TeleportersB.Add(new Teleporter(113, 0x1A, 0x12, FacingOrientation.Down, 0x18, 0x1D)); // Aquaria Winter - From INN
			TeleportersB.Add(new Teleporter(114, 0x3A, 0x12, FacingOrientation.Down, 0x19, 0x1D)); // Aquaria Summer - From INN
			
			TeleportersB.Add(new Teleporter(136, 0x20, 0x06, FacingOrientation.Down, 0x36, 0x29)); // Volcano - From Right Path
			TeleportersB.Add(new Teleporter(137, 0x14, 0x06, FacingOrientation.Down, 0x36, 0x29)); // Volcano - From Left Path
			TeleportersB.Add(new Teleporter(138, 0x10, 0x19, FacingOrientation.Down, 0x38, 0x29)); // Volcano - From Top Right
			TeleportersB.Add(new Teleporter(139, 0x08, 0x17, FacingOrientation.Down, 0x38, 0x29)); // Volcano - From Top Left

			// Battlefields teleporters
			TeleportersB.Add(new Teleporter(115, 0x0E, 0x23, FacingOrientation.Down, 0x00, 0x01)); // Foresta South
			TeleportersB.Add(new Teleporter(116, 0x08, 0x1F, FacingOrientation.Down, 0x00, 0x02)); // Foresta West
			TeleportersB.Add(new Teleporter(117, 0x16, 0x1F, FacingOrientation.Down, 0x00, 0x03)); // Foresta East
			TeleportersB.Add(new Teleporter(118, 0x22, 0x19, FacingOrientation.Down, 0x00, 0x04)); // Aquaria 01
			TeleportersB.Add(new Teleporter(119, 0x29, 0x13, FacingOrientation.Down, 0x00, 0x05)); // Aquaria 02
			TeleportersB.Add(new Teleporter(120, 0x2E, 0x15, FacingOrientation.Down, 0x00, 0x06)); // Aquaria 03
			TeleportersB.Add(new Teleporter(121, 0x37, 0x0F, FacingOrientation.Down, 0x00, 0x07)); // Wintry 01
			TeleportersB.Add(new Teleporter(122, 0x33, 0x0D, FacingOrientation.Down, 0x00, 0x08)); // Wintry 02
			TeleportersB.Add(new Teleporter(123, 0x2D, 0x09, FacingOrientation.Down, 0x00, 0x09)); // Ice Pyramid
			TeleportersB.Add(new Teleporter(124, 0x24, 0x10, FacingOrientation.Down, 0x00, 0x0A)); // Libra 01
			TeleportersB.Add(new Teleporter(125, 0x1F, 0x10, FacingOrientation.Down, 0x00, 0x0B)); // Libra 02
			TeleportersB.Add(new Teleporter(126, 0x1A, 0x17, FacingOrientation.Down, 0x00, 0x0C)); // Fireburg 01
			TeleportersB.Add(new Teleporter(127, 0x1A, 0x13, FacingOrientation.Down, 0x00, 0x0D)); // Fireburg 02
			TeleportersB.Add(new Teleporter(128, 0x1A, 0x0E, FacingOrientation.Down, 0x00, 0x0E)); // Fireburg 03
			TeleportersB.Add(new Teleporter(129, 0x0F, 0x0C, FacingOrientation.Down, 0x00, 0x0F)); // Mine 01
			TeleportersB.Add(new Teleporter(130, 0x09, 0x0C, FacingOrientation.Down, 0x00, 0x10)); // Mine 02
			TeleportersB.Add(new Teleporter(131, 0x10, 0x08, FacingOrientation.Down, 0x00, 0x11)); // Mine 03
			TeleportersB.Add(new Teleporter(132, 0x1F, 0x0A, FacingOrientation.Down, 0x00, 0x12)); // Volcano
			TeleportersB.Add(new Teleporter(133, 0x2E, 0x29, FacingOrientation.Down, 0x00, 0x13)); // Windia 01
			TeleportersB.Add(new Teleporter(134, 0x33, 0x28, FacingOrientation.Down, 0x00, 0x14)); // Windia 02

			
		}
		public void Write(FFMQRom rom)
		{
			ushort teleportersOffsetA = NewTeleportersOffset;
			ushort teleportersOffsetB = (ushort)(teleportersOffsetA + (TeleportersA.Count * 3));
			ushort teleportersOffsetWarp = (ushort)(teleportersOffsetB + (TeleportersB.Count * 3));
			ushort teleportersOffsetLong = (ushort)(teleportersOffsetWarp + (TeleportersWarp.Count * 3));

			TeleportersB = TeleportersB.OrderBy(x => x.Id).ToList();

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

		public void ShuffleCrestTiles(GameScriptManager tileScripts, MT19337 rng)
		{ 
			List<((int id, int type), (int id, int type))> crestTileTeleporterList = new()
			{
				((0x27, 1), (67, 8)),
				((0x28, 1), (68, 8)),
				((0x29, 1), (69, 8)),
				((0x1D, 6), (72, 8)),
				((0x15, 6), (59, 8)),
				((0x16, 6), (60, 8)),
				((0x2D, 1), (33, 8)),
				((0x2E, 1), (34, 8)),
				((0x2F, 1), (35, 8)),
				((0x30, 1), (36, 8)),
				((0x35, 1), (64, 8)),
				((0x36, 1), (65, 8)),
				((0x37, 1), (66, 8)),
				((0x19, 6), (62, 8)),
				((0x1A, 6), (63, 8)),
				((0x1F, 6), (45, 8)),
				((0x1E, 6), (54, 8)),
				((0x1C, 6), (71, 8)),
				((0x1B, 6), (70, 8)),
				((0x18, 6), (44, 8)),
				((0x17, 6), (43, 8)),
				((0x20, 6), (61, 8)),
			};

			List<Items> crestTiles = new()
			{
				Items.LibraCrest,
				Items.LibraCrest,
				Items.LibraCrest,
				Items.GeminiCrest,
				Items.GeminiCrest,
				Items.GeminiCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
				Items.MobiusCrest,
			};

			List<(int id, int type)> crestTileList = crestTileTeleporterList.Select(x => x.Item2).ToList();


			var crestEntrances = EntrancesLinks.Where(x => crestTileList.Contains(x.EntranceA) || crestTileList.Contains(x.EntranceB)).ToList();

			var crestEntrancesB = crestEntrances.Select(x => x.EntranceB).ToList();
			var crestEntrancesA = crestEntrances.Select(x => x.EntranceA).ToList();
			crestEntrancesB.Shuffle(rng);
			crestTiles.Shuffle(rng);

			crestEntrances = crestEntrancesA.Select((x, i) => new EntrancesLink(x, (crestEntrancesB[i].id, crestEntrancesB[i].type))).ToList();


			while (crestTileList.Any())
			{
				var crestLink1 = rng.TakeFrom(crestEntrances);
				var crestLink2 = rng.TakeFrom(crestEntrances);






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
