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
			Chest,
			NPCOn,
			NPCOff,
			Battlefield
		}

		Dictionary<ItemGivingNPCs, (byte flag, bool oncheck)> ItemNPCflags = new()
		{
			{ ItemGivingNPCs.BoulderOldMan, (0x13, true) },
			{ ItemGivingNPCs.VenusChest, ((byte)NewGameFlagsList.VenusChestUnopened, false) },
			{ ItemGivingNPCs.WomanAquaria, ((byte)NewGameFlagsList.AquariaSellerItemBought, true) },
			{ ItemGivingNPCs.MysteriousManLifeTemple, (0xCE, false) },
			{ ItemGivingNPCs.Spencer, ((byte)NewGameFlagsList.SpencerItemGiven, true) },
			{ ItemGivingNPCs.TristamSpencersPlace, ((byte)NewGameFlagsList.TristamChestUnopened, false) },
			{ ItemGivingNPCs.ArionFireburg, ((byte)NewGameFlagsList.ArionItemGiven, true) },
			{ ItemGivingNPCs.WomanFireburg, ((byte)NewGameFlagsList.FireburgSellerItemBought, true) },
			{ ItemGivingNPCs.MegaGrenadeDude, (0x74, true) },
			{ ItemGivingNPCs.PhoebeFallBasin, ((byte)NewGameFlagsList.ReubenMineItemGiven, true) },
			{ ItemGivingNPCs.GirlWindia, ((byte)NewGameFlagsList.WindiaSellerItemBought, true) },
			{ ItemGivingNPCs.KaeliForesta, ((byte)GameFlagsList.MinotaurDefeated, true) },
			{ ItemGivingNPCs.KaeliWindia, ((byte)NewGameFlagsList.KaeliSecondItemGiven, true) },
			{ ItemGivingNPCs.PhoebeWintryCave, ((byte)NewGameFlagsList.PhoebeWintryItemGiven, true) },
			{ ItemGivingNPCs.TristamBoneDungeonBomb, ((byte)NewGameFlagsList.TristamBoneDungeonItemGiven, true) },
			{ ItemGivingNPCs.TristamFireburg, ((byte)NewGameFlagsList.TristamFireburgItemGiven, true) },
		};

		public void HintRobots(Flags flags, MapSprites mapsprites,  ObjectList gameobjects, ItemsPlacement itemplacement, GameLogic gamelogic, ApConfigs apconfigs,MT19337 rng)
		{
			if(flags.HintMode == HintModes.None)
			{
				return;
			}

			bool includeSkyCoin = flags.SkyCoinMode == SkyCoinModes.Standard;
			bool progGear = flags.ProgressiveGear;

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
			PutInBank(0x16, 0x8020, Blob.FromHex("600057002c041b0817007f00df40df400020170c080062620600ea003420d810e800fe00fd04fd040004e83010004646f760fb08fc049202f860ff607f050f006a0c0f6f67600000ef06df103f264f401900ff06ff66f6005630f6f0e606060000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb028302b7018900560636003d103f00067e69762f090200f890f8b0f870b820f000e820f8b0f80090b070600010000000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb028302b302830243003e003e143f00067e6e7e3c010100f890f8b0f8b0f8a03000d8c0d800b80090b0b0a0c0e0204037002c0017001b007d009e00950097000013080402616a68ec001400e800d800be007900a900eb0200e8102040865616d743ff6bff0cff0fff67ff607f050f006b6b0c0f67600000ebc2ffd6ff30fff6ffe0ff06ff66f600d6d630f6e0060600"));

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

			PutInBank(0x16, 0x9130, Blob.FromHex($"a900008d0415{densityfloor}a9d00022769700d00aad04151869c8008d0415a9d10022769700d00aad04151869f4018d0415a9d20022769700d00aad041518692c018d0415ad0115186d04158d0415e220c21022008f16ad1215d019c230ad1015cd0415b00fcd0115b005a9ff00800aa901008005c230a900008d9e00286b")); // 0x70

			// Take Money
			PutInBank(0x16, 0x91B0, Blob.FromHex("08c230ad01156fe01f708fe01f70286b")); // 0x20

			// Move hint address
			PutInBank(0x16, 0x91D0, Blob.FromHex("08e220c210ad9e000a6d9e00aaad00159fe31f70ad07158d9e009fe41f70ad08158d9f009fe51f70ad09158da000286b")); // 0x30
			PutInBank(0x16, 0x9100, Blob.FromHex("08e220c210ad9e000a6d9e00aabfe31f708d0015bfe41f708d9e00bfe51f708d9f00a9168da000286b")); // 0x30

			// Hint seeker
			PutInBank(0x16, 0x9200, Blob.FromHex("0bf4a60e2b225a97002b1a3a60")); // Items
			PutInBank(0x16, 0x9210, Blob.FromHex("0bf432102b38e920225a97002b1a3a60")); // Weapons
			PutInBank(0x16, 0x9220, Blob.FromHex("0bf435102b38e92f225a97002b1a3a60")); // Armors
			PutInBank(0x16, 0x9230, Blob.FromHex("0bf438102b38e914225a97002b1a3a60")); // Spells
			PutInBank(0x16, 0x9240, Blob.FromHex("3a3aaa201092d00d8a1aaa201092d0058a1a20109260")); // 3 weapons
			PutInBank(0x16, 0x9260, Blob.FromHex("3a3aaa202092d00d8a1aaa202092d0058a1a20209260")); // 3 armors
			PutInBank(0x16, 0x9280, Blob.FromHex("0bf4c80e2baabdc092225a97002b1a3a60")); // Chests
			PutInBank(0x16, 0x9291, Blob.FromHex("0bf4a80e2baabdc092225a97002b1a3a60")); // NPC On
			PutInBank(0x16, 0x92A2, Blob.FromHex("0bf4a80e2baabdc092225a97002b1a3a90060a9ff60")); // NPC Off
			PutInBank(0x16, 0x92B8, Blob.FromHex("aabdc092aabdd40ff003a90060a9ff60")); // Battlefield

			PutInBank(0x16, 0x9300, Blob.FromHex("009210922092309242924192409262926192609280929192A292B892")); // check routines pointers

			// Main checker loop
			PutInBank(0x16, 0x93A0, Blob.FromHex("c8c8c8c8c8b90194c9fff00ec9fef0250aaab90094fc0093d0e6b900948d0015b902948d0715b903948d0815b904948d0915ab286b08c230b90294a82880c6")); // 0x40

			// generate hints first , 1 item > 1 entry
			var keyitems = itemplacement.ItemsLocations.Where(l => (l.Content >= Items.Elixir && l.Content < Items.CurePotion) || (l.Content >= Items.ExitBook && l.Content <= Items.CupidLocket)).Select(l => (l.Content, l.Name, l.Type, l.Location)).ToList();
			if (!includeSkyCoin)
			{
				keyitems = keyitems.Where(k => k.Content != Items.SkyCoin).ToList();
			}

			List<byte> progGearFlagsList = new();

			for (int i = (int)Items.SteelSword; i <= (int)Items.CupidLocket; i++)
			{
				GameObject gameobject;
				if (itemplacement.ItemsLocations.TryFind(l => l.Content == (Items)i, out gameobject))
				{
					if (gameobject.Type == GameObjectType.BattlefieldItem)
					{
						progGearFlagsList.Add((byte)(gameobject.Location - 1));
					}
					else if (gameobject.Type == GameObjectType.Chest || gameobject.Type == GameObjectType.Box)
					{
						progGearFlagsList.Add((byte)gameobject.ObjectId);
					}
					else if (gameobject.Type == GameObjectType.NPC)
					{
						progGearFlagsList.Add(ItemNPCflags[(ItemGivingNPCs)gameobject.ObjectId].flag);
					}
				}
				else
				{
					progGearFlagsList.Add(0xFF);
				}
			}

			PutInBank(0x16, 0x92E0, progGearFlagsList.ToArray());

			List<(Items item, HintCheckType type, ushort address)> hintaddresses = new();

			byte[] endstring = Blob.FromHex("3600");
			byte[] itemname = progGear ? Blob.FromHex("07008E16") : Blob.FromHex("077DFE03");
			int pointeraddress = 0x9900;

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

			if (apconfigs.ApEnabled)
			{
				foreach (var item in apconfigs.ExternalPlacement)
				{
					// Exclude progressive gear in AP
					if (!progGear || item.Content < Items.SteelSword || item.Content > Items.CupidLocket)
					{
						keyitems.Add((item.Content, SanitizeString(item.LocationName + " in " + item.Player + "'s World"), GameObjectType.ApLocation, LocationIds.None));

					}
				}
			}

			foreach (var item in keyitems)
			{
				byte[] hintstring;
				byte[] hintpart;

				bool isProgGear = progGear && item.Content >= Items.SteelSword && item.Content <= Items.CupidLocket;
				HintCheckType defaulType = GetHintType(item.Content, false);

				if (item.Type == GameObjectType.NPC)
				{
					hintpart = TextToByte(" is with " + item.Name + " at ", true).Concat(new byte[] { 0x1F, locationCode[item.Location] }).Concat(TextToByte(".", true)).ToArray();

					if (isProgGear)
					{
						var npcMode = ItemNPCflags[(ItemGivingNPCs)itemplacement.ItemsLocations.Find(i => i.Content == item.Content).ObjectId];
						defaulType = npcMode.oncheck ? HintCheckType.NPCOn : HintCheckType.NPCOff;
					}
				}
				else if (item.Type == GameObjectType.BattlefieldItem)
				{
					hintpart = TextToByte(" is at " + item.Name + ".", true);

					if (isProgGear)
					{
						defaulType = HintCheckType.Battlefield;
					}
				}
				else if (item.Type == GameObjectType.ApLocation)
				{
					hintpart = TextToByte(" is at " + item.Name + ".", true);
				}
				else
				{
					hintpart = TextToByte(" is in " + item.Name + " at ", true).Concat(new byte[] { 0x1F, locationCode[item.Location] }).Concat(TextToByte(".", true)).ToArray();

					if (isProgGear)
					{
						defaulType = HintCheckType.Chest;
					}
				}

				hintaddresses.Add((item.Content, defaulType, (ushort)pointeraddress));
				hintstring = TextToByte("The ", true).Concat(itemname).Concat(hintpart).Concat(endstring).ToArray();
				pointeraddress += hintstring.Length;

				allHints.Add(hintstring.ToArray());
			}

			if (pointeraddress > 0xFFFF)
			{
				throw new Exception("Hints text to large.");
			}

			PutInBank(0x16, 0x9900, allHints.SelectMany(h => h).ToArray());

			// generate logic lists
			var hintListAddress = 0x0000;
			byte[] endOfList = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

			// flat list, since we always return to this
			var flatListAddress = hintListAddress;
			var flatList = hintaddresses.SelectMany(h => new byte[] { (byte)h.item, (byte)h.type, (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).ToArray().Concat(endOfList).ToArray();

			// progression list
			List<Items> swords = new() { Items.SteelSword, Items.KnightSword, Items.Excalibur };
			List<Items> axes = new() { Items.Axe, Items.BattleAxe, Items.GiantsAxe };
			List<Items> claws = new() { Items.CatClaw, Items.CharmClaw };
			List<Items> bombs = new() { Items.Bomb, Items.JumboBomb };

			List<Items> progressionItems = new() { Items.DragonClaw, Items.MegaGrenade, Items.SandCoin, Items.RiverCoin, Items.SunCoin, Items.MultiKey, Items.LibraCrest, Items.GeminiCrest, Items.MobiusCrest, Items.Wakewater, Items.CaptainsCap, Items.ThunderRock };

			List<Items> progmodeItems = new();

			if (includeSkyCoin)
			{
				progressionItems.Add(Items.SkyCoin);
			}

			if (flags.NpcsShuffle != ItemShuffleNPCsBattlefields.Exclude)
			{
				progressionItems.AddRange(new List<Items>() { Items.TreeWither, Items.Elixir, Items.VenusKey });
			}

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

			if (progGear)
			{
				progressionItems.AddRange(new List<Items>() { Items.CharmClaw, Items.CatClaw, Items.Bomb, Items.JumboBomb });
			}
			else
			{
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
			var logicProgressionList = logicProgression.SelectMany(h => new byte[] { (byte)h.item, (byte)ConvertToProg(h.item, h.type, progmodeItems), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// spells + progression list
			List<Items> spellsGood = new() { Items.ExitBook, Items.LifeBook, Items.AeroBook, Items.MeteorSeal, Items.WhiteSeal, Items.FlareSeal };
			List<Items> spellProgressionItems = spellsGood.Concat(progressionItems).ToList();

			var spellProgression = hintaddresses.Where(h => spellProgressionItems.Contains(h.item)).ToList();
			spellProgression.Shuffle(rng);

			var spellProgressionAddress = logicProgressionAddress + logicProgressionList.Length;
			var spellProgressionList = spellProgression.SelectMany(h => new byte[] { (byte)h.item, (byte)ConvertToProg(h.item, h.type, progmodeItems), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// go mode list
			var shipaccess = gamelogic.GameObjects.Find(o => o.OnTrigger.Contains(AccessReqs.ShipDockAccess)).AccessRequirements.SelectMany(a => a).ToList();
			var shipaccessItems = shipaccess.SelectMany(a => AccessReferences.AccessReqItem.TryGetValue(a, out var itemlist) ? itemlist : new()).Distinct().ToList();

			List<Items> goModeItems = new();
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
				if (hintaddresses.TryFind(h => swords.Contains(h.item), out var logicSwordDoom))
				{
					goModeItems.Add(logicSwordDoom.item);
					progmodeItems.Add(logicSwordDoom.item);
				}
			}

			goModeItems = goModeItems.Distinct().ToList();
			var goMode = hintaddresses.Where(h => goModeItems.Contains(h.item)).ToList();
			goMode.Shuffle(rng);
			byte[] jumpToProgList = new byte[] { 0xFE, 0xFE, (byte)(logicProgressionAddress % 0x100), (byte)(logicProgressionAddress / 0x100), 0x16 };

			var goModeAddress = spellProgressionAddress + spellProgressionList.Length;
			var goModeList = goMode.SelectMany(h => new byte[] { (byte)h.item, (byte)ConvertToProg(h.item, h.type, progmodeItems), (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(jumpToFlatList).ToArray();

			// random list
			hintaddresses.Shuffle(rng);
			var randomAdress = goModeAddress + goModeList.Length;
			var randomList = hintaddresses.SelectMany(h => new byte[] { (byte)h.item, (byte)h.type, (byte)(h.address % 0x100), (byte)(h.address / 0x100), 0x16 }).Concat(endOfList).ToArray();

			PutInBank(0x16, 0x9400 + flatListAddress, flatList);
			PutInBank(0x16, 0x9400 + logicProgressionAddress, logicProgressionList);
			PutInBank(0x16, 0x9400 + spellProgressionAddress, spellProgressionList);
			PutInBank(0x16, 0x9400 + goModeAddress, goModeList);
			PutInBank(0x16, 0x9400 + randomAdress, randomList);

			if (0x9400 + randomAdress + randomList.Length > pointeraddress)
			{
				throw new Exception("Hint Logic Lists are too large.");
			}

			// Entrance point for each hinter
			PutInBank(0x16, 0x9320, Blob.FromHex($"088be220c210a91648aba900eba0{(logicProgressionAddress % 0x100):X2}{(logicProgressionAddress / 0x100):X2}4ca593"));
			PutInBank(0x16, 0x9340, Blob.FromHex($"088be220c210a91648aba900eba0{(spellProgressionAddress % 0x100):X2}{(spellProgressionAddress / 0x100):X2}4ca593"));
			PutInBank(0x16, 0x9360, Blob.FromHex($"088be220c210a91648aba900eba0{(randomAdress % 0x100):X2}{(randomAdress / 0x100):X2}4ca593"));
			PutInBank(0x16, 0x9380, Blob.FromHex($"088be220c210a91648aba900eba0{(goModeAddress % 0x100):X2}{(goModeAddress / 0x100):X2}4ca593"));

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
						// Check Money
						"09809016",
						// Read $9e for money result
						"0BFF[14]", // FF = Not Enough Money Message
						"0B01[15]", // 01 = Vendor Gate
						// We have enough money, ask if they want it
						$"09{(0x20+(i*0x20)):X2}9316", // compute hint, address at $1507
						"0F00150BFF[17]",
						freehints ?
							TextToHex("Want to know where the ") + (flags.ProgressiveGear ? "07008E16" : "077DFE03") + TextToHex(" is?") :
							TextToHex("Want to know where the ") + (flags.ProgressiveGear ? "07008E16" : "077DFE03") + TextToHex(" is for ") + "072CFF03" + TextToHex("?"), // todo
						"07D0FD03", // yes/no
						"050BFB[10]",
						"00", // no, we done
						// take money
						freehints ? "" : $"09B09116",
						// set flag
						$"053B{robotFlags[i]}09D09016",
						// fetch hint address
						$"053B{i:X2}09D09116",
						// jump to hint text
						$"050300",
						// Not Enough Money
						TextToHex("Come back when you have enough GPs. I'll reveal the location of an item.") + "3600",
						// Vendor Gate
						TextToHex("You have enough GPs for a hint, but you might need that money for a vendor's item. Come back when you have more GPs!") + "3600",
						// Hint already given, repeat hint
						$"053B{i:X2}09009116050300",
						// Nothing left to hint for
						TextToHex(rng.TakeFrom(nohintsleft)) + "3600",
					}));
			}

			// Progressive items script
			var progitemname = new ScriptBuilder(new List<string>()
			{
				"0F0015",
				"050620[11]",
				"05043F[11]",
				"050623[12]",
				"050626[13]",
				"050629[14]",
				"05062C[15]",
				"050632[16]",
				"050639[17]",
				"05063D[18]",
				"050640[19]",
				"05027DFE03",
				TextToHex("Progressive Sword") + "00",
				TextToHex("Progressive Axe") + "00",
				TextToHex("Progressive Claw") + "00",
				TextToHex("Progressive Bomb") + "00",
				TextToHex("Progressive Helmet") + "00",
				TextToHex("Progressive Armor") + "00",
				TextToHex("Progressive Shield") + "00",
				TextToHex("Progressive Accessory") + "00",
			});

			progitemname.WriteAt(0x16, 0x8E00, this);

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
		private static HintCheckType ConvertToProg(Items item, HintCheckType defaultype, List<Items> progModeItems)
		{
			if (progModeItems.Contains(item))
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
					return defaultype;
				}
			}
			else
			{
				return defaultype;
			}
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
