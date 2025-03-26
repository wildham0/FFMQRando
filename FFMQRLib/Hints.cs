using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Data;

namespace FFMQLib
{
	public enum HintModes
	{
		[Description("None")]
		None,
		[Description("Free")]
		Free,
		[Description("250 GP Fixed")]
		Fixed250,
		[Description("500 GP Fixed")]
		Fixed500,
		[Description("1000 GP Fixed")]
		Fixed1000,
		[Description("125 GP Progressive")]
		Prog125,
		[Description("250 GP Progressive")]
		Prog250,
		[Description("500 GP Progressive")]
		Prog500,
	}

	public partial class FFMQRom : SnesRom
	{
		public enum HintPriceMode
		{ 
			Free,
			Fixed,
			Progressive
		}
		public enum HintCheckType : byte
		{
			Item = 0x00,
			Weapon = 0x01,
			Armor = 0x02,
			Spell = 0x03,
			WeaponX3G1 = 0x04,
			WeaponX3G2 = 0x05,
			WeaponX3G3 = 0x06,
			ArmorX3G1 = 0x07,
			ArmorX3G2 = 0x08,
			ArmorX3G3 = 0x09,
		}

		public void HintRobots(Flags flags, MapSprites mapsprites,  ObjectList gameobjects, ItemsPlacement itemplacement, GameLogic gamelogic, MT19337 rng)
		{
			if(flags.HintMode == HintModes.None)
			{
				return;
			}

			Dictionary<HintModes, (int price, bool prog)> prices = new()
			{
				{ HintModes.Free, (0, false) },
				{ HintModes.Fixed250, (250, false) },
				{ HintModes.Fixed500, (500, false) },
				{ HintModes.Fixed1000, (1000, false) },
				{ HintModes.Prog125, (125, true) },
				{ HintModes.Prog250, (250, true) },
				{ HintModes.Prog500, (500, true) },
			};

			var priceMode = prices[flags.HintMode];

			// Debt Hack
			TemporalDebtHack();

			// Robot Sprite
			PutInBank(0x16, 0x8020, Blob.FromHex("600057002c001b0017007f00df40df4000201304080062620600ea003400d800e800fe00fd04fd040004c82010004646ff60f300f0009000f860ff607f050f00620c0f6f67600000ff06cf000f060f001900ff06ff66f6004630f6f0e606060000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028f019900560636003d103f00067e71662f090200f890f8b0f870b820f000e820f8b0f80090b070600010000000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028302830243003e003e143f00067e7e7e3c010100f890f8b0f8b0f8a03000d8c0d800b80090b0b0a0c0e0204037002c0017001b007d009e00950097000013080402616a68ec001400e800d800be007900a900e90000e81020408656169703ff6bff0cff0fff67ff607f050f006b6b0c0f67600000e9c0ffd6ff30fff6ffe0ff06ff66f600d6d630f6e0060600"));

			// Sprite loading hack, if sprite == 44, load at bank 16 instead, should extend tho 
			PutInBank(0x16, 0x9000, Blob.FromHex("ad2a19c90044f00ba602a404a90f00547f046be220c210a9168506c23018a502690080aaa404a90f00547f166b")); // 0x30
			PutInBank(0x01, 0xA90C, Blob.FromHex("22009016eaeaeaeaeaea"));

			// Hinter Routines
			// Set price part
			string progressiveskip = (priceMode.prog) ? "eaeaea" : "4c3091";
			string priceroutine = $"08c2309c03159c0615a9{(priceMode.price % 0x100):X2}{(priceMode.price / 0x100):X2}8d0115{progressiveskip}a901004820c090900aad01151869{(priceMode.price % 0x100):X2}{(priceMode.price / 0x100):X2}8d0115680ac9100090e94c3091";
				
			PutInBank(0x16, 0x9080, Blob.FromHex(priceroutine));

			// Flags Routine
			PutInBank(0x16, 0x90C0, Blob.FromHex("08e2202fe21f70f003283860281860")); // Check Flag
			PutInBank(0x16, 0x90D0, Blob.FromHex("e220ad9e000fe21f708fe21f706b")); // Set Flag from script
			PutInBank(0x16, 0x90E0, Blob.FromHex("e220ad9e0020c0909006a9008d9e006ba9ff8d9e006b")); // Check Flag from script

			// can afford routine
			string densityfloor = ((flags.EnemiesDensity == EnemiesDensity.None) && (priceMode.price != 0)) ?
				"eaea" :
				"8039";

			PutInBank(0x16, 0x9130, Blob.FromHex($"a900008d0415{densityfloor}a9d00022769700d00aad04151869c8008d0415a9d10022769700d00aad04151869f4018d0415a9d20022769700d00aad041518692c018d0415ad0115186d04158d0415e220c21022008f16ad1215d019c230ad1015cd0415b00fcd0115b005a9ff00800aa901008005c230a900008d9e00286b")); // 0x80

			// Take Money
			PutInBank(0x16, 0x91C0, Blob.FromHex("08c230ad01156fe01f708fe01f70286b")); // 0x20

			// Move hint address
			PutInBank(0x16, 0x91E0, Blob.FromHex("08e220c210ad07158d9e00ad08158d9f00ad09158da000286b")); // 0x20

			// Hint seeker
			PutInBank(0x16, 0x9200, Blob.FromHex("0bf4a60e2b225a97002b1a3a60")); // Items
			PutInBank(0x16, 0x9210, Blob.FromHex("0bf432102b38e920225a97002b1a3a60")); // Weapons
			PutInBank(0x16, 0x9220, Blob.FromHex("0bf435102b38e92f225a97002b1a3a60")); // Armors
			PutInBank(0x16, 0x9230, Blob.FromHex("0bf438102b38e914225a97002b1a3a60")); // Spells
			PutInBank(0x16, 0x9240, Blob.FromHex("3a3aaa201092d00d8a1aaa201092d0058a1a20109260")); // 3 weapons
			PutInBank(0x16, 0x9260, Blob.FromHex("3a3aaa202092d00d8a1aaa202092d0058a1a20209260")); // 3 armors

			PutInBank(0x16, 0x9280, Blob.FromHex("0092109220923092429241924092629261926092")); // check routines pointers

			// Main checker loop
			PutInBank(0x16, 0x9310, Blob.FromHex("c8c8c8c8c8b98193c9fff00ec9fef0250aaab98093fc8092d0e6b980938d0015b982938d0715b983938d0815b984938d0915ab286b08c230b98293a82880c7")); // 0x40

			// generate hints first , 1 item > 1 entry
			var keyitems = itemplacement.ItemsLocations.Where(l => (l.Content >= Items.Elixir && l.Content < Items.CurePotion) || (l.Content >= Items.ExitBook && l.Content <= Items.CupidLocket)).Select(l => (l.Content, l.Name, l.Type, l.Location)).ToList();
			List<(Items item, ushort address)> hintaddresses = new();

			byte[] endstring = Blob.FromHex("3600");
			byte[] itemname = Blob.FromHex("077DFE03");
			int pointeraddress = 0x9800;

			List<byte[]> allHints = new();

			Dictionary<LocationIds, byte> locationCode = new()
			{
				{ LocationIds.Foresta, 0x04 },
				{ LocationIds.AliveForest, 0x18 },
				{ LocationIds.Aquaria, 0x09 },
				{ LocationIds.BoneDungeon, 0x07 },
				{ LocationIds.DoomCastle, 0x24 },
				{ LocationIds.FallsBasin, 0x0D },
				{ LocationIds.Fireburg, 0x11 },
				{ LocationIds.FocusTowerAquaria, 0x01 },
				{ LocationIds.FocusTowerFireburg, 0x01 },
				{ LocationIds.FocusTowerForesta, 0x01 },
				{ LocationIds.FocusTowerFrozen, 0x01 },
				{ LocationIds.FocusTowerWindia, 0x01 },
				{ LocationIds.GiantTree, 0x18 },
				{ LocationIds.IcePyramid, 0x0E },
				{ LocationIds.KaidgeTemple, 0x1A },
				{ LocationIds.LavaDome, 0x16 },
				{ LocationIds.LevelForest, 0x03 },
				{ LocationIds.LibraTemple, 0x08 },
				{ LocationIds.LifeTemple, 0x0C },
				{ LocationIds.LightTemple, 0x20 },
				{ LocationIds.MacsShip, 0x23 },
				{ LocationIds.MacsShipDoom, 0x23 },
				{ LocationIds.Mine, 0x13 },
				{ LocationIds.MountGale, 0x1C },
				{ LocationIds.PazuzusTower, 0x1F },
				{ LocationIds.RopeBridge, 0x17 },
				{ LocationIds.SandTemple, 0x06 },
				{ LocationIds.SealedTemple, 0x14 },
				{ LocationIds.ShipDock, 0x21 },
				{ LocationIds.SpencersPlace, 0x0F },
				{ LocationIds.Volcano, 0x15 },
				{ LocationIds.WindholeTemple, 0x1B },
				{ LocationIds.Windia, 0x1D },
				{ LocationIds.WintryCave, 0x0B },
				{ LocationIds.WintryTemple, 0x10 },
			};

			foreach (var item in keyitems)
			{
				byte[] hintstring;
				byte[] hintpart;

				if (item.Type == GameObjectType.NPC)
				{
					hintpart = TextToByte(" is with " + item.Name + " at ", true).Concat(new byte[] { 0x1F, locationCode[item.Location] }).Concat(TextToByte(".", true)).ToArray();
				}
				else if (item.Type == GameObjectType.BattlefieldItem)
				{
					hintpart = TextToByte(" is at " + item.Name + ".", true);
				}
				else
				{
					hintpart = TextToByte(" is in " + item.Name + " at ", true).Concat(new byte[] { 0x1F, locationCode[item.Location] }).Concat(TextToByte(".", true)).ToArray();
				}

				hintaddresses.Add((item.Content, (ushort)pointeraddress));
				hintstring = TextToByte("The ", true).Concat(itemname).Concat(hintpart).Concat(endstring).ToArray();
				pointeraddress += hintstring.Length;

				allHints.Add(hintstring.ToArray());
			}

			if (pointeraddress > 0xFFFF)
			{
				throw new Exception("Hints text to large.");
			}

			PutInBank(0x16, 0x9800, allHints.SelectMany(h => h).ToArray());

			// generate logic lists
			var hintListAddress = 0x0000;
			byte[] endOfList = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

			// flat list, since we always return to this
			var flatListAddress = hintListAddress;
			var flatList = hintaddresses.SelectMany(h => new byte[] { (byte)h.item, (byte)GetHintType(h.item, false), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).ToArray().Concat(endOfList).ToArray();

			// progression list
			List<Items> swords = new() { Items.SteelSword, Items.KnightSword, Items.Excalibur };
			List<Items> axes = new() { Items.Axe, Items.BattleAxe, Items.GiantsAxe };
			List<Items> claws = new() { Items.CatClaw, Items.CharmClaw };
			List<Items> bombs = new() { Items.Bomb, Items.JumboBomb };

			List<Items> progressionItems = new() { Items.DragonClaw, Items.MegaGrenade, Items.SandCoin, Items.RiverCoin, Items.SunCoin, Items.SunCoin, Items.MultiKey, Items.LibraCrest, Items.GeminiCrest, Items.MobiusCrest, Items.Wakewater, Items.CaptainsCap, Items.ThunderRock };

			List<Items> progmodeItems = new();

			if (flags.NpcsShuffle != ItemShuffleNPCsBattlefields.Exclude)
			{
				progressionItems.AddRange(new List<Items>() { Items.TreeWither, Items.Elixir, Items.VenusKey });
			}

			if (flags.ProgressiveGear)
			{
				progressionItems.AddRange(swords);
				progressionItems.AddRange(axes);
				progressionItems.AddRange(claws);
				progressionItems.AddRange(bombs);
			}
			else
			{
				if (hintaddresses.TryFind(h => swords.Contains(h.item), out var logicSword))
				{
					progressionItems.Add(logicSword.item);
					progmodeItems.Add(logicSword.item);
				}

				if (hintaddresses.TryFind(h => swords.Contains(h.item), out var logicAxe))
				{
					progressionItems.Add(logicAxe.item);
					progmodeItems.Add(logicAxe.item);
				}

				if (hintaddresses.TryFind(h => claws.Contains(h.item), out var logicClaw))
				{
					progressionItems.Add(logicClaw.item);
					progmodeItems.Add(logicClaw.item);
				}

				if (hintaddresses.TryFind(h => bombs.Contains(h.item), out var logicBomb))
				{
					progressionItems.Add(logicBomb.item);
					progmodeItems.Add(logicBomb.item);
				}
			}

			var logicProgression = hintaddresses.Where(h => progressionItems.Contains(h.item)).ToList();

			byte[] jumpToFlatList = new byte[] { 0xFE, 0xFE, (byte)(flatListAddress % 0x100), (byte)(flatListAddress / 0x100), 0x16 };
			var logicProgressionAddress = hintListAddress + flatList.Length;
			var logicProgressionList = logicProgression.SelectMany(h => new byte[] { (byte)h.item, (byte)GetHintType(h.item, progmodeItems.Contains(h.item)), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// spells + progression list
			List<Items> spellsGood = new() { Items.ExitBook, Items.LifeBook, Items.AeroBook, Items.MeteorSeal, Items.WhiteSeal, Items.FlareSeal };
			List<Items> spellProgressionItems = spellsGood.Concat(progressionItems).ToList();

			var spellProgression = hintaddresses.Where(h => spellProgressionItems.Contains(h.item)).ToList();
			spellProgression.Shuffle(rng);

			var spellProgressionAddress = logicProgressionAddress + logicProgressionList.Length;
			var spellProgressionList = spellProgression.SelectMany(h => new byte[] { (byte)h.item, (byte)GetHintType(h.item, progmodeItems.Contains(h.item)), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// go mode list
			var shipaccess = gamelogic.GameObjects.Find(o => o.OnTrigger.Contains(AccessReqs.ShipDockAccess)).AccessRequirements.SelectMany(a => a).ToList();
			var shipaccessItems = shipaccess.SelectMany(a => AccessReferences.AccessReqItem.TryGetValue(a, out var itemlist) ? itemlist : new()).Distinct().ToList();

			List<Items> goModeItems = new();
			bool includeSkyCoin = flags.SkyCoinMode == SkyCoinModes.Standard;
			bool progressiveGear = flags.ProgressiveGear;
			bool doomCastleShortcut = flags.DoomCastleShortcut;
			bool doomCastleMaze = flags.DoomCastleMode == DoomCastleModes.Standard;

			if (includeSkyCoin)
			{
				goModeItems.Add(Items.SkyCoin);
			}

			if (!doomCastleShortcut)
			{
				goModeItems.AddRange(new List<Items>() { Items.MegaGrenade, Items.DragonClaw, Items.SunCoin, Items.ThunderRock, Items.CaptainsCap });
				goModeItems.AddRange(shipaccessItems);

				if (progressiveGear)
				{
					goModeItems.AddRange(new List<Items>() { Items.Bomb, Items.JumboBomb, Items.CatClaw, Items.CharmClaw });
				}
			}

			if (doomCastleMaze)
			{
				if (hintaddresses.TryFind(h => swords.Contains(h.item), out var logicSword))
				{
					goModeItems.Add(logicSword.item);
				}
			}

			goModeItems = goModeItems.Distinct().ToList();
			var goMode = hintaddresses.Where(h => goModeItems.Contains(h.item)).ToList();
			goMode.Shuffle(rng);
			byte[] jumpToProgList = new byte[] { 0xFE, 0xFE, (byte)(logicProgressionAddress % 0x100), (byte)(logicProgressionAddress / 0x100), 0x16 };

			var goModeAddress = spellProgressionAddress + spellProgressionList.Length;
			var goModeList = goMode.SelectMany(h => new byte[] { (byte)h.item, (byte)GetHintType(h.item, progmodeItems.Contains(h.item)), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// random list
			hintaddresses.Shuffle(rng);
			var randomAdress = goModeAddress + goModeList.Length;
			var randomList = hintaddresses.SelectMany(h => new byte[] { (byte)h.item, (byte)GetHintType(h.item, false), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(endOfList).ToArray();

			PutInBank(0x16, 0x9380 + flatListAddress, flatList);
			PutInBank(0x16, 0x9380 + logicProgressionAddress, logicProgressionList);
			PutInBank(0x16, 0x9380 + spellProgressionAddress, spellProgressionList);
			PutInBank(0x16, 0x9380 + goModeAddress, goModeList);
			PutInBank(0x16, 0x9380 + randomAdress, randomList);

			if (0x9380 + randomAdress + randomList.Length > pointeraddress)
			{
				throw new Exception("Hint Logic Lists are too large.");
			}

			// Entrance point for each hinter
			PutInBank(0x16, 0x9290, Blob.FromHex($"088be220c210a91648aba900eba0{(logicProgressionAddress % 0x100):X2}{(logicProgressionAddress / 0x100):X2}4c1593"));
			PutInBank(0x16, 0x92B0, Blob.FromHex($"088be220c210a91648aba900eba0{(spellProgressionAddress % 0x100):X2}{(spellProgressionAddress / 0x100):X2}4c1593"));
			PutInBank(0x16, 0x92D0, Blob.FromHex($"088be220c210a91648aba900eba0{(randomAdress % 0x100):X2}{(randomAdress / 0x100):X2}4c1593"));
			PutInBank(0x16, 0x92F0, Blob.FromHex($"088be220c210a91648aba900eba0{(goModeAddress % 0x100):X2}{(goModeAddress / 0x100):X2}4c1593"));

			// Generate scripts
			List<string> nohintsleft = new()
			{
				"DATABASE ERROR MQ100592: NO HINTS LEFT.",
				"Seems you have found everything that could be found...",
				"You get no hints! You lose! Good day, sir!",
				"I... I have nothing left to teach you...",
				"You found every treasures in this world, time to explore the Dark World!",
				"I could tell you where the Mega Buster is hiding, but I'm keeping that one for myself.",
				"Let me think... Maybe... No, no... I have no idea what I could tell you!",
				"Your rate for collecting items is 100%.",
			};

			List<string> hintgiven = new()
			{
				"Hope you're happy with your hint!",
				"Was my hint useful?",
				"Use that knowledge judiciously.",
				"Now you know, and knowledge is 49.29% of the battle.",
			};
			
			bool freehints = priceMode.price == 0;

			List<string> robotFlags = new() { "01", "02", "04", "08" };

			for (int i = 0; i < 4; i++)
			{
				TalkScripts.AddScript((int)TalkScriptsList.ForestaHinter + i,
					new ScriptBuilder(new List<string>()
					{
						$"053B{robotFlags[i]}09E090160B00[16]",
						//$"2E{((int)NewGameFlagsList.ForestaHintGiven + i):X2}[16]",
						// Check Money
						"09809016",
						// Read $9e for money result
						"0BFF[14]", // FF = Not Enough Money Message
						"0B01[15]", // 01 = Vendor Gate
						// We have enough money, ask if they want it
						$"09{(0x90+(i*0x20)):X2}9216", // compute hint, address at $1507
						"0F00150BFF[17]",
						freehints ?
							TextToHex("Want to know where the ") + "077DFE03" + TextToHex(" is?") :
							TextToHex("Want to know where the ") + "077DFE03" + TextToHex(" is for ") + "072CFF03" + TextToHex("?"), // todo
						"07D0FD03", // yes/no
						"050BFB[10]",
						"00", // no, we done
						// take money
						freehints ? "" : $"09C09116",
						// set flag
						$"053B{robotFlags[i]}09D09016",
						//$"23{((int)NewGameFlagsList.ForestaHintGiven + i):X2}",
						// fetch hint address
						"09E09116",
						// jump to hint text
						$"050300",
						// Not Enough Money
						TextToHex("Come back when you have enough GPs. I'll reveal the location of an item.") + "3600",
						// Vendor Gate
						TextToHex("You have enough GPs for a hint, but you might need that money for a vendor's item. Come back when you have more GPs!") + "3600",
						// Hint already given
						TextToHex(rng.TakeFrom(hintgiven)) + "3600",
						// Nothing left to hint for
						TextToHex(rng.TakeFrom(nohintsleft)) + "3600",
					}));
			}

			// Create NPCs
			mapsprites[0x31].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			var forestaRobot = new MapObject(gameobjects[0x11][0x01]);
			forestaRobot.Sprite = 0x4C;
			forestaRobot.Facing = FacingOrientation.Right;
			forestaRobot.Behavior = 0x08;
			forestaRobot.Value = 0x88;
			forestaRobot.X = 0x1F;
			forestaRobot.Y = 0x10;

			MapObjects[0x11].Add(forestaRobot);

			var aquariaRobot = new MapObject(forestaRobot);
			aquariaRobot.Facing = FacingOrientation.Down;
			aquariaRobot.X = 0x36;
			aquariaRobot.Y = 0x29;
			aquariaRobot.Value = 0x89;
			mapsprites[0x09].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x1B].Add(aquariaRobot);

			List<(FacingOrientation orient, byte x, byte y)> fireburgRobotPlacements = new() { (FacingOrientation.Left, 0x0A, 0x2B), (0x00, 0x11, 0x2E), (FacingOrientation.Down, 0x0E, 0x2D), (FacingOrientation.Left, 0x15, 0x2A), (FacingOrientation.Up, 0x10, 0x2C) };

			var fireburgRobotPlacement = rng.PickFrom(fireburgRobotPlacements);
			var fireburgRobot = new MapObject(forestaRobot);
			fireburgRobot.Facing = fireburgRobotPlacement.orient;
			fireburgRobot.X = fireburgRobotPlacement.x;
			fireburgRobot.Y = fireburgRobotPlacement.y;
			fireburgRobot.Value = 0x8A;
			mapsprites[0x13].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x31].Add(fireburgRobot);

			var windiaRobot = new MapObject(forestaRobot);
			windiaRobot.Facing = FacingOrientation.Left;
			windiaRobot.X = 0x35;
			windiaRobot.Y = 0x16;
			windiaRobot.Value = 0x8B;
			windiaRobot.Sprite = 0x44;
			windiaRobot.Palette = 0x03;
			// this one gets clobbered by phoebe a bit, but it should be fine since you can't talk from the north
			mapsprites[0x23].AdressorList.Add(new SpriteAddressor(7, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x52].Add(windiaRobot);
		}
		private static HintCheckType GetHintType(Items item, bool progmode)
		{
			if (progmode && item >= Items.SteelSword && item < Items.SteelHelm)
			{
				List<Items> baseWeapons = new() { Items.SteelSword, Items.Axe, Items.CatClaw, Items.Bomb };

				if (baseWeapons.Contains(item))
				{
					return HintCheckType.WeaponX3G1;
				}
				else if (baseWeapons.Contains((Items)(item - 1)))
				{
					return HintCheckType.WeaponX3G2;
				}
				else if (baseWeapons.Contains((Items)(item - 2)))
				{
					return HintCheckType.WeaponX3G3;
				}
				else
				{
					// should never happens
					return HintCheckType.Weapon;
				}
			}
			else if (item >= Items.Elixir && item < Items.CurePotion)
			{
				return HintCheckType.Item;
			}
			else if (item >= Items.ExitBook && item < Items.SteelSword)
			{
				return HintCheckType.Spell;
			}
			else if (item >= Items.SteelSword && item < Items.SteelHelm)
			{
				return HintCheckType.Weapon;
			}
			else
			{
				return HintCheckType.Armor;
			}
		}

		private void TemporalDebtHack()
		{
			// we add a permanent debt when you use the hint robots so players can't savescum
			// this only enabled with the hint feature
			
			// Compute debt and store actual GPs at $1510
			PutInBank(0x16, 0x8F00, Blob.FromHex("08c230ad840e38efe01f708d1015ad860ee900008d1215b009a900008d10158d1215286b"));

			var vendorGpScript = new ScriptBuilder(new List<string>()
			{
				"09008f16",
				"05aa1015[05]",
				"2bfd",
				"053bff",
				"00",
				"053b00",
				"00"
			});

			var vendorMaxGpScript = new ScriptBuilder(new List<string>()
			{
				"05401015",
			});

			var vendorJump = new ScriptBuilder(new List<string>()
			{
				"07408f16",
				"0b000cfe",
				"00",
			});

			var innGpScript = new ScriptBuilder(new List<string>()
			{
				"09008f16",
				"05401015",
				"05c45f0000[05]",
				"053bff",
				"00",
				"053b00",
				"00"
			});

			var innJump = new ScriptBuilder(new List<string>()
			{
				"07608f16",
				"0b00effb",
				"0a97fb",
			});

			var statusScreenGpScript = new ScriptBuilder(new List<string>()
			{
				"09008f16",
				"05401015",
				"00"
			});

			var statusJump = new ScriptBuilder(new List<string>()
			{
				"07808f16",
			});


			vendorGpScript.WriteAt(0x16, 0x8F40, this);
			innGpScript.WriteAt(0x16, 0x8F60, this);
			statusScreenGpScript.WriteAt(0x16, 0x8F80, this);

			vendorMaxGpScript.WriteAt(0x03, 0x8BC6, this);
			vendorMaxGpScript.WriteAt(0x03, 0x8C2D, this);
			vendorJump.WriteAt(0x03, 0xFE03, this);
			innJump.WriteAt(0x03, 0xFB8C, this);
			statusJump.WriteAt(0x03, 0x9BD5, this);
		}
	}
}
