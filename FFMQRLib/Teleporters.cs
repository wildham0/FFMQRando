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
				TargetArea = data[1];
				TargetLocation = data[0];
				Orientation = (FacingOrientation)((data[3] / 64) & 0x03);
			}
		}

		public byte[] ToBytesShort()
		{
			return new byte[] { (byte)TargetArea, (byte)TargetY, (byte)(TargetX | ((int)Orientation * 64))};
		}
		public byte[] ToBytesLong()
		{
			return new byte[] { (byte)TargetLocation, (byte)TargetArea, (byte)TargetY, (byte)(TargetX | ((int)Orientation * 64)) };
		}
	}

	public class Teleporters
	{ 
		public List<Teleporter> TeleportersA { get; set; }
		public List<Teleporter> TeleportersB { get; set; }
		public List<Teleporter> TeleportersWarp { get; set; }
		public List<Teleporter> TeleportersLong { get; set; }

		private const int TeleportersBank = 0x05;
		private const int TeleportersQtyA = 217;
		private const int TeleportersQtyB = 86;
		private const int TeleportersQtyWarp = 37;
		private const int TeleportersQtyLong = 33;
		private const int TeleportersOffsetA = 0xF4A0;
		private const int TeleportersOffsetB = 0xF79A;
		private const int TeleportersOffsetWarp = 0xF72B;
		private const int TeleportersOffsetLong = 0xF89C;

		private const int NewTeleportersBank = 0x12;
		private const int NewTeleportersOffset = 0xAA00;

		public Teleporters(FFMQRom rom)
		{
			TeleportersA = rom.GetFromBank(TeleportersBank, TeleportersOffsetA, TeleportersQtyA * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersB = rom.GetFromBank(TeleportersBank, TeleportersOffsetB, TeleportersQtyB * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersWarp = rom.GetFromBank(TeleportersBank, TeleportersOffsetWarp, TeleportersQtyWarp * 3).Chunk(3).Select((x, i) => new Teleporter(i, x)).ToList();
			TeleportersLong = rom.GetFromBank(TeleportersBank, TeleportersOffsetLong, TeleportersQtyLong * 4).Chunk(4).Select((x, i) => new Teleporter(i, x)).ToList();

			ExtraTeleporters();
			PushTeleporters();
		}
		private void ExtraTeleporters()
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

			TeleportersB.Add(new Teleporter(141, 0x29, 0x3B, FacingOrientation.Up, 0x11, 0x17)); // To Dummy Room
			TeleportersB.Add(new Teleporter(142, 0x30, 0x0F, FacingOrientation.Up, 0x19, 0x1D)); // Spring Aquaria Leaf

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

			// Extra Crest tile Teleports
			TeleportersLong.Add(new Teleporter(33, 0x08, 0x34, FacingOrientation.Down, 0x43, 0x2D)); // Alive Forest - West teleporter
			TeleportersLong.Add(new Teleporter(34, 0x39, 0x31, FacingOrientation.Down, 0x43, 0x2D)); // Alive Forest - East teleporter
			TeleportersLong.Add(new Teleporter(35, 0x18, 0x0A, FacingOrientation.Down, 0x43, 0x2D)); // Alive Forest - North teleporter
			TeleportersB.Add(new Teleporter(140, 0x09, 0x18, FacingOrientation.Down, 0x2E, 0x23)); // Wintry Temple

		}
		private void PushTeleporters()
		{
			List<int> teleportersAtoPush = new() { 148, 152, 155, 159, 168, 169 };
			List<int> teleportersBtoPush = new() { 96, 97, 100, 101, 102, 105 };
			List<int> teleportersWarptoPush = new() { };
			List<int> teleportersLongtoPush = new() { };

			TeleportersA.Where(t => teleportersAtoPush.Contains(t.Id)).ToList().ForEach(t => t.TargetY++);
			TeleportersB.Where(t => teleportersBtoPush.Contains(t.Id)).ToList().ForEach(t => t.TargetY++);
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

}
