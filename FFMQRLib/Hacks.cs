using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void FastMovement()
		{
			// walking
			// double scrolling rate
			Put(0x008CAC, Blob.FromHex("FE0202FE")); // 8CAB > 8CAC
			// halve number of frames
			Put(0x00939C, Blob.FromHex("07")); // 939b > 939C
			// speed up animation
			Put(0x0094BA, Blob.FromHex("04EA")); // 94B9> 94BA


			// jumping
			// halve number of frames
			Put(0x009DC1, Blob.FromHex("07")); // 9DC0 > 9DC1
			// move routine to skip one position in the jump position table
			Put(0x009DE1, Blob.FromHex("22E0FF11EAEA")); // 9DE0 > 9DE1
			PutInBank(0x11, 0xFFE0, Blob.FromHex("E8E8E8E8E8E88E4F196B"));
			// move starting position in the jump pos table by one so we end on the last intended position
			PutInBank(0x00,0xF24C, Blob.FromHex("03006300C3002301")); // F251 > F24C

			// Working hack for inmap room transition, except it slow down walking speed back at 16 frames vs 8, but it works!
			PutInBank(0x11, 0x8200, Blob.FromHex("A9028D461AA00A008C9119A90F8D26196B"));
			Put(0x0093DD, Blob.FromHex("22008211")); // 93DC > 93DD
			Put(0x0093DD+4, Blob.FromHex("4A900C8007")); // 93DC > 93DD+4
			Put(0x009462, Blob.FromHex("4CE193")); // 9461 > 9462

			// Fix the dragonclaw, normal speed
			Put(0x009BCD, Blob.FromHex("02")); // 9BCC > 9BCD ? 98CD
			Put(0x009C7F, Blob.FromHex("07")); // 9C7E > 9C7F

			// Fix mine elevator animation
			PutInBank(0x01, 0xCD8C, Blob.FromHex("01"));

			// Move CODE_018AE4 for space
			//var CODE_018AE4 = Get(0x008AE4, 0x2F);
			//CODE_018AE4 += Blob.FromHex("6B");

			//PutInBank(0x11, 0x8300, CODE_018AE4);
			//Put(0x008AE4, Blob.FromHex("220083114C138B"));

			// Put duplicate of CODE_0182D8, without wait for vsync
			//Put(0x008492 + 5, Blob.FromHex("08DA5AE220C210205CAB2080A07AFA2860"));
			//Put(0x008AEB, Blob.FromHex("08DA5AE220C210205CAB2080A07AFA284CF293"));


			//Put(0x0093FB, Blob.FromHex("58"));
			//Put(0x00944C, Blob.FromHex("984AB0A2EAEAEAEAE220C21022008111"));
			//Put(0x00944C, Blob.FromHex("984A90044CEB8AEAE220C21022008111"));
			//PutInBank(0x11, 0x8100, Blob.FromHex("A9028D461AAD261938ED28198D26196B"));




			//Put(0x0093FB, Blob.FromHex("4C"));
			//Put(0x0093FC, Blob.FromHex("AE890EDA22008011"));
			//Put(0x0093FC + 0x08, Blob.FromHex("2077F9FA8E890EAC9119888C911920D882"));
			//Put(0x0093FC + 0x19, Blob.FromHex("AC9119888C911920D882EA4C4894"));
			//Put(0x009448, Blob.FromHex("22008111"));
			//Put(0x0093FC + 0x19, Blob.FromHex("AE890EDA22008011"));
			//Put(0x0093FC + 0x21, Blob.FromHex("2077F9FA8E890EAC9119888C91194C4C94"));



			//PutInBank(0x11, 0x8000, Blob.FromHex("E220AEC1198EBD19AEC3198EBF19AE890E8E2D19BBAD2E1938E905187FE194018D2E19BFE1940148AD2D1938E9088D2D19A900EB68C230186DBF19290F008DBF196B"));

			///PutInBank(0x11, 0x8100, Blob.FromHex("E220AD2619C907B007AD021938ED0A196B"));
		}
		public void DefaultSettings()
		{
			// Show Figure by default instead of Scale for HP
			GameFlags[(int)GameFlagsList.ShowFigureForHP] = true;

			// Default Text speed to 1
			Data[0x65397] = 0x00;
		}
		public void RemoveClouds()
		{
			// Change conditional branching for erasing clouds to always branch
			PutInBank(0x0B, 0x858E, Blob.FromHex("80"));
			PutInBank(0x0B, 0x8599, Blob.FromHex("80"));
			PutInBank(0x0B, 0x85A4, Blob.FromHex("80"));
		}
		public void RemoveStrobing()
		{
			// Crystal flash, simply skip the flash routine
			PutInBank(0x01, 0xD4C9, Blob.FromHex("EAEAEA"));
			PutInBank(0x01, 0xD4DB, Blob.FromHex("EAEAEA"));
			PutInBank(0x01, 0xD55A, Blob.FromHex("EAEAEA"));

			// Spencer's cave bombing
			PutInBank(0x01, 0xDA4E, Blob.FromHex("EAEAEA"));
			PutInBank(0x01, 0xDA62, Blob.FromHex("EAEAEA"));

			// Hero status, same, then rts early to avoid the big flash at the end
			PutInBank(0x01, 0xDC2A, Blob.FromHex("EAEAEA"));
			PutInBank(0x01, 0xDC37, Blob.FromHex("EAEAEA"));
			PutInBank(0x01, 0xDDBA, Blob.FromHex("60"));
		}
		public void SmallFixes()
		{
			// Fix Bomb and JumboBomb to work everywhere
			PutInBank(0x01, 0xF453, Blob.FromHex("3030"));

			// Allow shattered tile to intercept MegaGrenade
			GameMaps.TilesProperties[0x06][0x15].Byte1 = 0x07;

			// Stop CatClaws from giving Bow&Arrows to companion
			PutInBank(0x00, 0xdb9d, Blob.FromHex("EAEAEAEA"));

			// Phoebe1 start with Bow&Arrows
			PutInBank(0x0C, 0xd1d1, Blob.FromHex("2D0004"));

			// Start with 50 bombs so we don't need to update when acquiring them
			PutInBank(0x0C, 0xd0e0, Blob.FromHex("32"));

			// Fix armor downgrading
			PutInBank(0x00, 0xDBCE, Blob.FromHex("201490"));

			// Fix Giant Tree Axe-less softlock by blocking access from outside the forest
			PutInBank(0x03, 0xA4A3, Blob.FromHex("2E"));
			PutInBank(0x03, 0xA625, Blob.FromHex("7B"));

			// Reorder end of battle sequence to check player first
			PutInBank(0x02, 0x8107, Blob.FromHex("a594d04ca595d030"));

			// Add Wait 1 second routine
			PutInBank(0x11, 0x94A0, Blob.FromHex("da08a20000e220c210a93c4822a09600683ad0f728fa6b"));

			// Fix vendor text to sell books & seals
			var fullbookscript = new ScriptBuilder(new List<string>{
					"054D0C",			// Get item names
					"054320C10C",
					"05EA0C",
					"0F0015",			// Load item ID
					"05041F[10]",
					"050614[10]",
					"05041B[09]",
					TextToHex(" Book"),
					"00",
					TextToHex(" Seal"),
					"00"
				});

			var jumpScript = new ScriptBuilder(new List<string>{
					"07609411",
					"00"
				});

			fullbookscript.WriteAt(0x11, 0x9460, this);
			jumpScript.WriteAt(0x03, 0xFFD0, this);
			PutInBank(0x03, 0xFE80, Blob.FromHex("08D0FF"));
		}
		public void ExitHack(LocationIds startingLocation)
		{
			// Using exit on overworld send you back to home location
			PutInBank(0x00, 0xC064, Blob.FromHex("22d08711eaeaeaeaea"));
			PutInBank(0x11, 0x87D0, Blob.FromHex($"ad910e297f00f004c907006b08e220a9{(int)startingLocation:X2}8d880e28386b"));
		}
		public void ChestsHacks(Flags flags, ItemsPlacement itemsPlacement)
		{
			// Include chests when loading graphics for empty boxes
			PutInBank(0x01, 0xB095, Blob.FromHex("00"));

			// New routine to get the quantity of items, use a lut instead of comparing chest id
			PutInBank(0x11, 0x9000, Blob.FromHex("08c230ad9e00aabf0091118d6601e230c901f004a9808002a9008d6501286b"));

			// Generate lut of boxes & chests quantity
			byte[] lutResetBox = new byte[0x20];

			var test2 = itemsPlacement.ItemsLocations.Where(x => x.ObjectId < 0x20).ToList();

			foreach(var location in itemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.Chest || x.Type == GameObjectType.Box).ToList())
            {
				byte quantity = 1;

				if (location.Content >= Items.Potion && location.Content <= Items.Refresher)
				{
					quantity = 3;
				}
				else if (location.Content == Items.BombRefill || location.Content == Items.ProjectileRefill)
				{
					quantity = 10;
				}

				if (location.ObjectId >= 0xF2 && location.ObjectId <= 0xF5)
				{
					quantity = 25;
				}


				if (location.Type == GameObjectType.Chest)
				{
					GameFlags.CustomFlagToHex(lutResetBox, location.ObjectId, true);
				}

				
				PutInBank(0x11, 0x9100 + location.ObjectId, new byte[] { quantity });
            }

			// Part 1 of chest script, we gut the native quantity scripts
			PutInBank(0x03, 0x86BF, Blob.FromHex("051d2e000205fc4d6a8714ff115f012dc80e058e6a870d65010001090090110543008001052c9e0014ff0a3287"));

			// Part 2 of chest script
			PutInBank(0x03, 0x8732, Blob.FromHex("500f5f01093f800b5114ff08bd870bff828710660105050153870c0505250a57870c0505260f5f0105d80a6487"));

			// New routine to resets chests when leaving dungeon, instead of clearing everything we use a lut to spare red chests from the reset
			PutInBank(0x11, 0x8F90, Blob.FromHex("5aa20000a02000bfc08f113dc80e9dc80ee888d0f2a20000a920eaeaea9e280fe83ad0f67a6b"));
			PutInBank(0x0B, 0x818D, Blob.FromHex("22908f11eaeaeaeaeaeaeaeaeaeaea"));

			// Insert lut of resetable boxes, 0x20 bytes
			PutInBank(0x11, 0x8FC0, lutResetBox);

			// Put the Mirror/Mask effect with the give item routine instead, just outright cancel if they're outside their respective dungeons
			string maskBranch = "ff";
			string mirrorBranch = "ff";
			string skyCoinBranch = "ff";

			if (itemsPlacement.ItemsLocations.Find(x => x.Content == Items.Mask).Location == LocationIds.Volcano)
			{
				maskBranch = "05";
			}

			if (itemsPlacement.ItemsLocations.Find(x => x.Content == Items.MagicMirror).Location == LocationIds.IcePyramid)
			{
				mirrorBranch = "06";
			}

			if (flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				skyCoinBranch = "0F";
			}

			// see 11_9200_ChestHacks.asm
			PutInBank(0x11, 0x9200, Blob.FromHex($"C9{maskBranch}F013C9{mirrorBranch}F020C9{skyCoinBranch}F02D0BF4A60E2B224E97002B6B0BF4D0002BA992224E97002BAD9E0080E40BF4D0002BA992224E97002BAD9E0080D3EE930E6B"));
			PutInBank(0x00, 0xDB82, Blob.FromHex("22009211EAEAEAEAEAEA"));
		}
		public void NonSpoilerDemoplay(bool shortenedLoop)
		{
			// Don't cycle through the 3 demoplays, just do the first one
			PutInBank(0x00, 0x8184, Blob.FromHex("eaeaeaeaeaeaeaeaeaeaeaeaeaa900"));

			// Small routine to load the extra byte we add to the header
			PutInBank(0x00, 0x81A0, Blob.FromHex("22808711eaea"));
			PutInBank(0x11, 0x8780, Blob.FromHex("bfd581008d880ebfdd81008d910e6b"));

			// New header to load in fireburg, there's an extra byte to start in the actual city which clober the next demoplay, but we don't care about that
			var header = shortenedLoop ? "262834060caa2ea831" : "263335060caa2ea831";
			PutInBank(0x00, 0x81D5, Blob.FromHex(header));

			// Halve the input timer because of speedhack
			PutInBank(0x00, 0x934E, Blob.FromHex("07"));

			// Remove Tristam from the demoplay gameflags so he don't show up
			PutInBank(0x0C, 0xAA16, Blob.FromHex("A3"));

			// First input series to wake up, climb out, got to inn and trigger band, then go wild, while also removing the end of the first serie to bleed into the second series
			var inputseries = shortenedLoop ?
				"33338aa8888aaaaa3333aaa8888888888888bbbbbbbb99b9bbb73373373338aaabbb88838383" :
				"33338aaa888aaaa33333553a8888888b8bbb2bbbbb2b8bbb33bbbbbbbb99b9bbb73373373338";
			PutInBank(0x0C, 0xA82E, Blob.FromHex(inputseries));
			PutInBank(0x0C, 0xA8C0, Blob.FromHex("33"));
		}
		public void RestoreHillOfDestiny()
		{
			// Maybe one day, tilesets is linked to bone dungeons'
			//  this map is a mess
			const int hoDtileData = 0xAD00;
			const int tileDataBank = 0x06;

			List<byte> tileToBlock = new() { 0x41, 0x51, 0x57, 0x58, 0x61, 0x63, 0x64, 0x74 };
			GameMaps[(int)MapList.HillOfDestiny].ModifyMap(0x13, 0x0A, new List<List<byte>>() {
				new List<byte>() { 0x51 },
				new List<byte>() { 0x51 },
				new List<byte>() { 0x51 },
			});

			foreach (var tile in tileToBlock)
			{
				var tilevalue = GetFromBank(tileDataBank, hoDtileData + (tile * 2), 1);
				tilevalue[0] = (byte)(tilevalue[0] | 0x07);
				PutInBank(tileDataBank, hoDtileData + (tile * 2), tilevalue);
			}

		}
		public void DummyRoom()
		{
			// Add a small dummy room for Floor shuffle
			var dummyroomTiny = new List<List<byte>>() {
				new List<byte>() { 0x20, 0x21, 0x20 },
				new List<byte>() { 0x21, 0x22, 0x21 },
				new List<byte>() { 0x22, 0x05, 0x22 },
				new List<byte>() { 0x27, 0x7F, 0x02 },
				new List<byte>() { 0x37, 0x7F, 0x03 },
				new List<byte>() { 0x20, 0x7F, 0x20 },
				new List<byte>() { 0x20, 0x33, 0x20 },
			};

			GameMaps[(int)MapList.ForestaInterior].ModifyMap(0x28, 0x35, dummyroomTiny);
		}
		public void PazuzuFixedFloorRng(MT19337 rng)
		{
			PutInBank(0x11, 0x8800, Blob.FromHex("08e230ad940ec91ff0031ad002a9008d940eaabf2088118d9e00286b"));

			List<byte> randomJumps = new();
			for (int i = 0; i < 0x20; i++)
			{
				randomJumps.Add((byte)rng.Between(1, 6));
			}

			PutInBank(0x11, 0x8820, randomJumps.ToArray());

			var newPazuzuRng = new ScriptBuilder(new List<string> {
						"090088112F",
					});

			newPazuzuRng.WriteAt(0x03, 0xFC7E, this);
		}
		public void KeyItemWindow()
		{
			// Timer Hack
			PutInBank(0x00, 0x8968, Blob.FromHex("22008911eaeaeaeaeaeaeaea"));
			PutInBank(0x11, 0x8900, Blob.FromHex("a900005bee970ed003ee990ead610ed0049c600e6bce610e6b"));

			// Give Item Routine
			PutInBank(0x03, 0xB4F5, Blob.FromHex("07408911"));
			PutInBank(0x11, 0x8940, Blob.FromHex("0928db0005061053890506146b860504406b8611600e0d610e2c0100"));

			// Keep Weapon Sprite
			PutInBank(0x03, 0x822E, Blob.FromHex("00"));

			// Draw Empty Companion Stat Window
			PutInBank(0x03, 0xB787, Blob.FromHex("2f2f07708911"));
			PutInBank(0x11, 0x8970, Blob.FromHex("0530eaed089089280000"));

			// Draw Complete Companion Stat Window
			PutInBank(0x03, 0x8264, Blob.FromHex("2f07808911"));
			PutInBank(0x11, 0x8980, Blob.FromHex("08908924012e1e0700"));

			// Box drawing script
			PutInBank(0x11, 0x8990, Blob.FromHex("10610e05ba0000a789241b300405151c3118fefe01fefe09298d"));

			// Companion Weapon Drawing Routine
			PutInBank(0x00, 0x8D33, Blob.FromHex("EA22C08911"));
			PutInBank(0x00, 0x8D6C, Blob.FromHex("22e08911eaeaeaeaeaeaeaeaeaeaeaeaea"));
			PutInBank(0x11, 0x89C0, Blob.FromHex("08aef001e000f00cc230ae610ef005ae600e8003aeb11028e0ff6b"));
			PutInBank(0x11, 0x89E0, Blob.FromHex("22c08911dabf0098040a0a8df700c210686b"));
		}
		public void BugFixes()
		{
			// Fix vendor buy 0 bug
			PutInBank(0x00, 0xB75B, Blob.FromHex("D0")); // Instead of BPL, BNE to skip 0
			PutInBank(0x00, 0xB783, Blob.FromHex("8D")); // Instead of STZ, STA (A will always be #$01 if reached)

			PutInBank(0x00, 0xB799, Blob.FromHex("22108711")); // Long so we can compare to #01 instead of #00
			PutInBank(0x11, 0x8710, Blob.FromHex("38AD6201C9016B")); // Compare to #01
			
			PutInBank(0x00, 0xB7A4, Blob.FromHex("01")); // Set minimum quantity to 1
			PutInBank(0x00, 0xB7D8, Blob.FromHex("5C00871100")); // Long jump to set to 1 instead of 0
			PutInBank(0x11, 0x8700, Blob.FromHex("A9018D62015C8DB700")); // Jump back to hard coded adress because we're replacing a BRA

			// Fix Companion Armor Bug
			PutInBank(0x00, 0x9E7E, Blob.FromHex("1490")); // Add copying armor stats to working memory as a command (05 08)
			PutInBank(0x03, 0x8606, Blob.FromHex("08C0FF00")); // Insert jump for space in add companion routine
			PutInBank(0x03, 0xFFC0, Blob.FromHex("0508051ED0000100")); // Copy armor command + moved original command

			// Fix Life Insta Kill Bug
			PutInBank(0x02, 0x9238, Blob.FromHex("A5562B2908F0E5")); // Load weakness instead of resistance, and beq insteand of bne
			PutInBank(0x02, 0x9CAA, Blob.FromHex("EAEA")); // Don't branch if resistant to fatal

			// Fix Cure Overflow Bug
			// see 11_8600_CureOverflow.asm
			PutInBank(0x02, 0x95CA, Blob.FromHex("22008611eaea"));
			PutInBank(0x11, 0x8600, Blob.FromHex("a514186d77049008a51638e5148d7704c900809006a9fe7f8d77046b")); // Check for overflow and cap healing to positive value

			// Fix Dark King's crit loop
			// see 11_87A0_CritCheck.asm
			PutInBank(0x02, 0x9F24, Blob.FromHex("22A08711900AEAEAEAEA"));
			PutInBank(0x11, 0x87A0, Blob.FromHex("A53AC949900CC950900AC9CA9004C9D79002386B186B"));

			// Fix Skullrus Rex and Stone Golem not counting as boss for hp based attacks
			PutInBank(0x02, 0x9B07, Blob.FromHex("22508811"));
			PutInBank(0x11, 0x8850, Blob.FromHex("a53bc940f008c941f004c9449001386b"));

			// Fix crashing when transitioning from door and switching weapon at the same time (experimental)
			// We skip a PHA/PLP in an interrupt routine that seems to use vertical scanline location (OPVCT) to compute the status register ???
			//  vertscanline x3 + $0f (or + $9a)
			PutInBank(0x00, 0xB8C0, Blob.FromHex("EAEA"));
			PutInBank(0x00, 0xB852, Blob.FromHex("EAEA"));
		}
		public void Msu1SupportRandom(bool randomizesong, MT19337 rng)
		{
			var rngback = rng;
			
			// see 10_8000_MSUSupport.asm
			PutInBank(0x0D, 0x8186, Blob.FromHex("5C008010EAEA"));
			PutInBank(0x0D, 0x81F4, Blob.FromHex("22768010"));
			PutInBank(0x0D, 0x81FA, Blob.FromHex("229F8010EAEAEAEAEAEA"));
			PutInBank(0x0D, 0x85D2, Blob.FromHex("5C648010EA"));

			string loadrandomtrack = "EAEAEA";
			string saverandomtrack = "EAEAEA";

			if (randomizesong)
			{
				loadrandomtrack = "20F080";
				saverandomtrack = "200281";

				List<byte> tracks = Enumerable.Range(0, 0x1A).Select(x => (byte)x).ToList();
				List<byte> goodordertracks = Enumerable.Range(0, 0x1B).Select(x => (byte)x).ToList();
				tracks.Remove(0x00);
				tracks.Remove(0x15);

				tracks.Shuffle(rng);
				tracks.Insert(0, 0x00);
				tracks.Insert(0x15, 0x15);
				tracks.Add(0x1A);
				List<(byte, byte)> completetracks = goodordertracks.Select(x => (x, tracks[x])).ToList();

				PutInBank(0x10, 0x8120, completetracks.OrderBy(x => x.Item1).Select(x => x.Item2).ToArray());
				PutInBank(0x10, 0x8140, completetracks.OrderBy(x => x.Item2).Select(x => x.Item1).ToArray());
			}

			rng = rngback;
			rng.Next();

			PutInBank(0x10, 0x8000, Blob.FromHex($"{loadrandomtrack}A501C505D0045C8A810D20C2809044AFF0FF7FC501D00664015C8A810D9C0620A5018FF0FF7F8D04209C0520A9012C002070F9AD00202908D01DA9FF8D0620A501C915D004A9018005201081A9038D072064015CED810DA9008FF0FF7F5CED810D8D4021C9F0D0079C07205CD9850D5CED850DA6064820C2809009AD00202908D002686BAFF0FF7FF00D68A501201081A9008FF0FF7F6B682012816BA501D01620C280900FAFF0FF7FF0099C41219C024285056BA5018D412185058D02426BAD0220C953D025AD0320C92DD01EAD0420C94DD017AD0520C953D010AD0620C955D009AD0720C931D00238601860DA08E230A501AABF208110291F850128FA60DA08E230AABF408110291F28FA60A501{saverandomtrack}850960"));
		}
	}
}
