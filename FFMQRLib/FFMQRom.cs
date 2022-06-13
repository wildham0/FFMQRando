using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace FFMQLib
{
	public static class Metadata
	{
		public static string VersionNumber = "1.1.04";
		public static string Version = VersionNumber + "-beta";
	}
	
	public partial class FFMQRom : SnesRom
	{

		public ObjectList MapObjects;
		public MapChanges MapChanges;
		public GameFlags GameFlags;
		public GameScriptManager TileScripts;
		public GameScriptManager TalkScripts;
		public Battlefields Battlefields;
		public Overworld Overworld;
		public GameMaps GameMaps;
		public MapSprites MapSpriteSets;
		private byte[] originalData;
		public bool beta = false;

		public override bool Validate()
		{
			using (SHA256 hasher = SHA256.Create())
			{
				byte[] dataToHash = new byte[0x80000];
				Array.Copy(Data, dataToHash, 0x80000);

				// zero out benjamin's sprites
				for (int i = 0x21A20; i < (0x24A20 + 0x180 * 1); i++)
				{
					dataToHash[i] = 0;
				}

				for (int i = 0x24A20; i < (0x24A20 + 0x180 * 5); i++)
				{
					dataToHash[i] = 0;
				}

				// benjamin's palette
				for (int i = 0x3D826; i < (0x3D826 + 0x0E); i++)
				{
					dataToHash[i] = 0;
				}

				Blob hash = hasher.ComputeHash(dataToHash);
				
				//Console.WriteLine(BitConverter.ToString(hash).Replace("-", ""));
				// if (hash == Blob.FromHex("F71817F55FEBD32FD1DCE617A326A77B6B062DD0D4058ECD289F64AF1B7A1D05")) unadultered hash
				
				if (hash == Blob.FromHex("92F625478568B1BE262E3F9D62347977CE7EE345E9FF353B4778E8560E16C7CA"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool IsEmpty()
		{
			if (Data == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public Stream DataStream()
		{
			return new MemoryStream(Data);
		}

		public void PutInBank(int bank, int address, Blob data)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			Put(offset, data);
		}
		public Blob GetFromBank(int bank, int address, int length)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			return Get(offset, length);
		}
		public void ExpandRom()
		{
			Blob newData = new byte[0x100000];
			Array.Copy(Data, newData, 0x80000);
			Data = newData;
		}
		public void BackupOriginalData()
		{
			originalData = new byte[0x80000];
			Array.Copy(Data, originalData, 0x80000);
		}
		public void RestoreOriginalData()
		{
			Data = new byte[0x80000];
			Array.Copy(originalData, Data, 0x80000);
		}
		public void Randomize(Blob seed, Flags flags, Preferences preferences)
		{
			flags.FlagSanityCheck();

			MT19337 rng;
			MT19337 sillyrng;
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
				sillyrng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			NodeLocations nodeLocations = new(this);
			EnemiesAttacks enemiesAttacks = new(this);
			EnemiesStats enemiesStats = new(this);
			GameMaps = new(this);
			MapObjects = new(this);
			Credits credits = new(this);
			//MapUtilities mapUtilities = new(this);
			GameFlags = new(this);
			//ExitList exits = new(this);
			TalkScripts = new(this, RomOffsets.TalkScriptsPointers, RomOffsets.TalkScriptPointerQty, RomOffsets.TalkScriptOffset, RomOffsets.TalkScriptEndOffset);
			TileScripts = new(this, RomOffsets.TileScriptsPointers, RomOffsets.TileScriptPointerQty, RomOffsets.TileScriptOffset, RomOffsets.TileScriptEndOffset);
			Battlefields = new(this);
			MapChanges = new(this);
			Overworld = new(this);
			MapSpriteSets = new(this);
			TitleScreen titleScreen = new(this);
			

			ExpandRom();
			FastMovement();
			DefaultSettings();
			RemoveClouds();
			RemoveStrobing();
			SmallFixes();
			BugFixes();
			NonSpoilerDemoplay();
			CompanionRoutines();
			SetLevelingCurve(flags);
			ProgressiveGears(flags);

			GameMaps.RandomGiantTreeMessage(rng);
			GameMaps.LessObnoxiousMaps(flags, rng, MapObjects);

			MapObjects.SetEnemiesDensity(flags, rng);
			MapObjects.ShuffleEnemiesPosition(GameMaps, flags, rng);
			//enemiesAttacks.ScaleAttacks(flags, rng);
			enemiesStats.ScaleEnemies(flags, rng);
			nodeLocations.OpenNodes();
			Battlefields.SetBattlesQty(flags, rng);
			Battlefields.ShuffleBattelfieldRewards(flags, rng);
			Overworld.UpdateBattlefieldsColor(flags, Battlefields);
			ItemsPlacement itemsPlacement = new(this, flags, rng);

			SetStartingWeapons(itemsPlacement);
			MapObjects.UpdateChests(flags, itemsPlacement);

			itemsPlacement.WriteChests(this);

			DoomCastle doomCastle = new(flags, GameMaps, MapChanges, MapObjects, MapSpriteSets, TalkScripts, this);

			UpdateScripts(flags, itemsPlacement, rng);
			ChestsHacks(itemsPlacement);
			Battlefields.PlaceItems(itemsPlacement);

			sillyrng.Next();
			RandomBenjaminPalette(preferences, sillyrng);

			credits.Update();
			
			credits.Write(this);
			enemiesAttacks.Write(this);
			enemiesStats.Write(this);
			GameMaps.Write(this);
			MapChanges.Write(this);
			TileScripts.Write(this);
			TalkScripts.Write(this);
			GameFlags.Write(this);
			nodeLocations.Write(this);
			Battlefields.Write(this);
			Overworld.Write(this);
			MapObjects.WriteAll(this);
			MapSpriteSets.Write(this);
			titleScreen.Write(this, Metadata.VersionNumber, seed, flags);

			this.Header = Array.Empty<byte>();
		}
		public void UpdateScripts(Flags flags, ItemsPlacement fullItemsPlacement, MT19337 rng)
		{
			var itemsPlacement = fullItemsPlacement.ItemsLocations.Where(x => x.Type == TreasureType.NPC).ToDictionary(x => (ItemGivingNPCs)x.ObjectId, y => y.Content);

			/*** Overworld ***/
			// GameStart - Skip Mountain collapse
			Put(RomOffsets.GameStartScript, Blob.FromHex("23222b7a2a0b2700200470002ab05320501629ffff00"));

			GameFlags[(int)GameFlagsList.ShowPazuzuBridge] = true;

			// Remove Mine Boulder
			for (int i = 0; i < 6; i++)
			{
				Overworld.RemoveObject(i);
			}

			// Put bridge to access temple
			GameMaps[(int)MapList.Overworld].ModifyMap(0x0F, 0x0E, 0x56);

			/*** Level Forest ***/
			// Enter Level Forest
			TileScripts.AddScript((int)TileScriptsList.EnterLevelForest,
				new ScriptBuilder(new List<string> {
					"2E13[03]",
					"2C0001",
					"00",
					"2C0101",
					$"2E{(int)NewGameFlagsList.ShowForestaKaeli:X2}[11]",
					"2EE3[11]",
					$"050f{(int)Companion.Kaeli:X2}[11]",
					"2A3346634013432344505010530054FFFF",
					"1A82" + TextToHex("There, griffin. Path is cleared. Let's find that decaying !&%? piece of lumber.") + "36",
					"2A03440825682213424346FFFF",
					"23E3",
					"00"
				}));

			// Boulder Man
			TileScripts.AddScript((int)TileScriptsList.PushedBoulder,
				new ScriptBuilder(new List<string> {
					"2E13[03]0F8B0E05090075BC2A12402054FFFF",
					"1A0A" + TextToHex("Finally, after all these years I can go back home.\nHere have this.") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.BoulderOldMan]:X2}0162231323142B34",
					"00"
				}));

			// Following script, reproduced for extra space
			TileScripts.AddScript(0x2F,
				new ScriptBuilder(new List<string> {
					"2E38[03]",
					"2338",
					"2A0527042AFFFF",
					"00"
				}));

			// Fight Minotaur
			TileScripts.AddScript((int)TileScriptsList.FightMinotaur,
				new ScriptBuilder(new List<string> {
					"050B63[21]",
					$"050f{(int)Companion.Kaeli:X2}[22]",
					"2A30460054105a0e2527275246022A00453055FFFF",
					"1A0BAE63FF57FF57CE30ACC8C5C3C5BCC6B8CE36",
					"2A1B278044105430555054FFFF",
					"1A82" + TextToHex("&%?!! That son of a harpooner just poisoned me! Let's do for this &?!% baracoota!"),
					"36",
					"2A70448044704400440054FFFF",
					"05E41714",
					"2A62468044105431465140FFFF",
					"1A82" + TextToHex("You're a &?%! agonist, mate! Here, you earned it. Split a few skulls for me!") + "36",
					"2C7044",
					"2C8044",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliForesta]:X2}0162",
					$"23{(int)NewGameFlagsList.ShowSickKaeli:X2}",
					"0880FF",
					"61",
					"2A1140404661424146FFFF",
					"236D",
					"231E",
					"2B63",
					"2B15",
					"00"
				}));

			/*** Foresta ***/
			// Kaeli TreeWither
			TalkScripts.AddScript((int)TalkScriptsList.KaeliWitherTree,
				new ScriptBuilder(new List<string>{
					"04",
					"2d" + ScriptItemFlags[Items.TreeWither].Item1,
					"050c" + ScriptItemFlags[Items.TreeWither].Item2 + "[04]",
					"1A15" + TextToHex("Hey there, lubber. The forest is dying? Tell that to the &%?! marines! It's totally fine.") + "3600",
					"1A15" + TextToHex("!&%?! Look at that rotten hollard! Ok, let's go Johnny raw, let's chop some cursed tree!") + "36",
					"0F8B0E",
					"057C01[13]",
					"057C00[14]",
					"2C4146",
					"0880FF",
					"05E601085B85",
					$"2B{(int)NewGameFlagsList.ShowForestaKaeli:X2}23632B6C",
					"00",
					"2C114300",
					"2C114200"
				}));

			// Sick Kaeli
			TalkScripts.AddScript((int)TalkScriptsList.SickKaeli,
				new ScriptBuilder(new List<string>{
					"04",
					"2d" + ScriptItemFlags[Items.Elixir].Item1,
					"050c" + ScriptItemFlags[Items.Elixir].Item2 + "[04]",
					"1A16" + TextToHex("I'll be fine, mate. Nothing a grog and some rest can't fix. I'll be back on deck in no time.") + "3600",
					"1A16" + TextToHex("Mate! Psha, this taste like &?!% kelt, but I'm ready to show a leg! Heave and rally!") + "36",
					"0F8B0E",
					"057C01[13]",
					"057C03[14]",
					"2C4246",
					"0880FF",
					$"05E6{(int)Companion.KaeliPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}",
					"00",
					"2C124300",
					"2C124100"
				}));

			/*** Sand Temple ***/
			// Tristam
			MapObjects[0x12][0x00].X = 0x39;
			MapObjects[0x12][0x00].Y = 0x06;
			MapObjects[0x12][0x00].Gameflag = 0x5A;
			MapObjects[0x12][0x00].Value = 0x1A;
			MapObjects[0x12][0x00].Behavior = 0x0A;
			MapObjects[0x12][0x00].Orientation = 0x02;
			MapObjects[0x12][0x00].UnknownIndex = 0x02;
			MapObjects[0x12][0x01].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.TristamChest,
				new ScriptBuilder(new List<string> {
					TextToHex("Defeat evil? Treasure hunting? Sounds like a great business opportunity here. I'm in!") + "36",
					"2C1042",
					"2C4046",
					"0880FF",
					$"05E6{(int)Companion.Tristam:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowSandTempleTristam:X2}",
					$"2B{(int)NewGameFlagsList.ShowFireburgTristam:X2}",
					"00"
				}));

			/*** Bone Dungeon ***/
			// Tristam Bomb
			GameFlags[0xC9] = false;
			TileScripts.AddScript((int)TileScriptsList.BoneDungeonTristamBomb,
				new ScriptBuilder(new List<string> {
					$"2e{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[15]",
					$"050f{(int)Companion.Tristam:X2}[15]",
					"2a3046104130441054ffff",
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}",
					flags.ProgressiveGear ? "09309411" : "",
					"2BFC",
					"1a85" + TextToHex("Care to invest in my ") + "087DFE" + TextToHex(" venture? I'll give you an early prototype!") + "36",
					"08D0FD",
					"050BFB[12]",
					"1a85" + TextToHex("That's fine, not everyone is cut out for massive profits and a lifetime of riches.") + "36",
					"2a10434046ffff",
					"00",
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}0162",
					"2a10434046ffff", // 24ff > d3fe
					$"23{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}",
					"00"
				}));

			// Fight Rex
			//MapObjects[0x16][0x05].Type = MapObjectType.Chest;
			//MapObjects[0x16][0x05].Value = 0x04;
			//MapObjects[0x16][0x05].Gameflag = 0xAD;
			//Data[0x8004] = (byte)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonElixir];

			TileScripts.AddScript((int)TileScriptsList.FightFlamerusRex,
				new ScriptBuilder(new List<string> {
					"2E01[06]",
					"1A1BA0C5C55301B243D14AC1B8C96AB55E42C0B8D23066576741C3586A5A413DFF5A9EB4C53FCE",
					"05E4210C",
					"2A42FF3054005500541D2529E6FFFF",
					"2301",
					"2B06",
					"00"
				}));

			// Tristam Quit Party Tile
			TileScripts.AddScript((int)TileScriptsList.TristamQuitPartyBoneDungeon,
				new ScriptBuilder(new List<string> { "00" }));

			/*** Focus Tower ***/
			GameFlags[(int)GameFlagsList.ShowColumnMoved] = true;
			GameFlags[(int)GameFlagsList.ShowColumnNotMoved] = false;
			GameFlags[0xCB] = false; // Hide MysteriousMan Find Phoebe

			// Venus Chest
			MapObjects[0x0A][0x08].Gameflag = (byte)NewGameFlagsList.VenusChestUnopened;
			GameFlags[(int)NewGameFlagsList.VenusChestUnopened] = true;

			TalkScripts.AddScript((int)TalkScriptsList.VenusChest,
				new ScriptBuilder(new List<string>{
					"04",
					"2F",
					"050C03[04]",
					"1A000A37FF", // Locked...
					$"2E{(int)NewGameFlagsList.VenusChestUnopened:X2}[06]",
					"08788600",
					"2A2827082C80FBFFFF", // need some adjusting
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.VenusChest]:X2}0062",
					$"2B{(int)NewGameFlagsList.VenusChestUnopened:X2}",
					"00"
				}));

			/*** Libra Temple ***/
			// Phoebe
			TalkScripts.AddScript((int)TalkScriptsList.PhoebeLibraTemple,
				new ScriptBuilder(new List<string>{
					TextToHex("Sure, you can be my sidekick, just don't do anything stupid. I'm the heroine here!") + "36",
					"2C1042",
					"2C4046",
					"0880FF",
					$"05E6{(int)Companion.Phoebe:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowLibraTemplePhoebe:X2}",
					"00"
				}));

			/*** Aquaria ***/
			// Entering Aquaria
			TileScripts.AddScript((int)TileScriptsList.EnterAquaria,
				new ScriptBuilder(new List<string> {
					"050B02[03]",
					"2C0901",
					"00",
					"2F",
					"050D02[10]",
					$"23{(int)NewGameFlagsList.WakeWaterUsed:X2}",
					"234F", //is it necessary?
					"2A5EFF080161FFFFFF",
					"2A20509053705015271225304506ff0900FFFF",
					"00",
					"2C0801",
					"00",
				}));

			TileScripts.AddScript((int)TileScriptsList.EnterPhoebesHouse,
				new ScriptBuilder(new List<string> { "2C0A0200" }));

			// Move girl that blocks Aquaria Seller's House
			MapObjects[0x18][0x02].X = 0x06;
			MapObjects[0x19][0x02].X = 0x26;

			// Seller in Aquaria
			TalkScripts.AddScript((int)TalkScriptsList.AquariaSellerGirl,
				new ScriptBuilder(new List<string>{
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.WomanAquaria]:X2}",
					flags.ProgressiveGear ? "09309411" : "",
					"2BFC",
					$"2E{(int)NewGameFlagsList.AquariaSellerItemBought:X2}BFFE",
					"0E0115C80000", // set price
					"0891FE",
					"2EFDA4D8",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.WomanAquaria]:X2}0462", // give item
					$"23{(int)NewGameFlagsList.AquariaSellerItemBought:X2}",
					"1A29",
					"0ABFFE"
				}));

			// Bomb vendor in Aquaria
			TalkScripts.AddMobileScript((int)TalkScriptsList.AquariaInnKeeper);
			TalkScripts.AddMobileScript((int)TalkScriptsList.AquariaPotionVendor);

			TalkScripts.AddScript((int)TalkScriptsList.AquariaExplosiveVendor,
				new ScriptBuilder(new List<string>{
					"2D" + ScriptItemFlags[Items.Bomb].Item1,
					$"050c" + ScriptItemFlags[Items.Bomb].Item2 + "[08]",
					"2D" + ScriptItemFlags[Items.JumboBomb].Item1,
					$"050c" + ScriptItemFlags[Items.JumboBomb].Item2 + "[08]",
					"2D" + ScriptItemFlags[Items.MegaGrenade].Item1,
					$"050c" + ScriptItemFlags[Items.MegaGrenade].Item2 + "[08]",
					TextToHex("Look, I'm a busy person. Figure out which kind of explosives you want first, then come back."),
					"00",
					"0C0015DD",
					"23FC",
					"0A2BFE"
				}));

			// Potion seller in Aquaria
			TalkScripts.AddScript((int)TalkScriptsList.AquariaPotionVendor,
				new ScriptBuilder(new List<string>{
					"2E02D4EA",
					"9A4A41BB5EBF48BB4BB545B4C540B9C5C2CD564DC67266B64F7E7ABE40C0591E1053",
					"00"
				}));

			// Update Fireburg and Windia Bomb vendors to same script
			MapObjects[0x2F][0x01].Value = (byte)TalkScriptsList.AquariaExplosiveVendor;
			MapObjects[0x52][0x09].Value = (byte)TalkScriptsList.AquariaExplosiveVendor;

			/*** Wintry Cave ***/
			// Wintry Cave
			GameFlags[(int)GameFlagsList.WintryCaveCollapsed] = true;
			MapObjects[0x1C][0x00].Y = 0x25; // Change to map change
			MapChanges.Replace(0x03, Blob.FromHex("2a2534393960404040404040404040")); // Put script tile in after collapse

			// Collaspe
			TileScripts.AddScript((int)TileScriptsList.WintryCavePhoebeClaw,
				new ScriptBuilder(new List<string> {
					$"2e{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[07]",
					$"050f{(int)Companion.Phoebe:X2}[07]",
					"2a3046104310443054ffff",
					"1a8a" + TextToHex("Good job not being a clutz and falling down like an idiot! I guess that calls for a reward..."),
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeWintryCave]:X2}0162",
					"2a10414046ffff", // 24ff > d3fe
					$"23{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}",
					"00"
				}));

			// Wintry Squid
			MapObjects[0x1F][0x0B].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.FightSquid,
				new ScriptBuilder(new List<string>{
					"04",
					"05E43110",
					"2B24",
					GameFlags[0xB2] ? "" : "23B2",
					"2A61463B46FFFF",
					"23E0",
					"00"
				}));

			// Reproduce script for space
			TalkScripts.AddScript(0x2E,
				new ScriptBuilder(new List<string>{
					"05E17800",
				}));

			/*** Life Temple ***/
			TileScripts.AddScript((int)TileScriptsList.DriedUpSpringOfLife,
				new ScriptBuilder(new List<string> { "00" }));

			TileScripts.AddScript((int)TileScriptsList.ReceiveWakeWater,
				new ScriptBuilder(new List<string> {
					"2ECE[02]",
					"00",
					"1A2E" + TextToHex("Need something?"),
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.MysteriousManLifeTemple]:X2}0162",
					"2BCE",
					"2A0821134110504346FFFF", // Fix animation
					"00"
				}));

			/*** Fall Basin ***/
			// Put Chest under crab
			MapObjects[0x21][0x0F].X = MapObjects[0x21][0x07].X;
			MapObjects[0x21][0x0F].Y = MapObjects[0x21][0x07].Y;
			MapObjects[0x21][0x0F].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.FightCrab,
				new ScriptBuilder(new List<string>{
					"04",
					"05E43403",
					"2B25",
					GameFlags[(int)GameFlagsList.ShowFallBasinChest] ? "" : "23B3",
					"2A67463F46FFFF",
					"00"
				}));

			// Enter Fall Basin
			TileScripts.AddScript((int)TileScriptsList.EnterFallBasin,
				new ScriptBuilder(new List<string> {
					"2C0C0100",
				}));

			// Exit Fall Basin
			GameMaps.TilesProperties[0x0A][0x22].Byte2 = 0x08;

			/*** Ice Pyramid ***/
			// Change entrance tile to disable script
			GameMaps[(int)MapList.IcePyramidA].ModifyMap(0x15, 0x20, 0x05);

			// Add teleport coordinates
			PutInBank(0x05, 0xFED5, Blob.FromHex("2F364D"));

			// Change tile properties from falling tile to script tile
			GameMaps.TilesProperties[0x06][0x1E].Byte2 = 0x88;

			TileScripts.AddScript((int)TileScriptsList.EnterIcePyramid,
				new ScriptBuilder(new List<string>
				{
					"2D" + ScriptItemFlags[Items.CatClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CatClaw].Item2 + "[17]", // goto teleport
					"2D" + ScriptItemFlags[Items.CharmClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CharmClaw].Item2 + "[17]",
					"2D" + ScriptItemFlags[Items.DragonClaw].Item1,
					$"050c" + ScriptItemFlags[Items.DragonClaw].Item2 + "[17]",
					"0F8B0E",
					"057C00[13]", // looking up
					"057C01[14]",// looking right
					"057C02[15]",  // lookingdown
					"057C03[16]",// looking left
					"1A48" + TextToHex("I should bring claws with me before going down there.") + "36",
					"00",
					"2C105900",
					"2C105A00",
					"2C105700",
					"2C105800",
					"0cee19000cef191a094cb20100" // Hack to excute the falling down routine
				}));


			// Fight IceGolem
			TileScripts.AddScript((int)TileScriptsList.FightIceGolem,
				new ScriptBuilder(new List<string> {
					"2E07[02]",
					"00",
					"2C2727",
					"1A31B243FF3F477D44D1C54078A4C1524DBBC8BBCF019F63FF57CE30ADB4BE403FBCC6CE",
					"05E44F13",
					"2A42FF1E2529E6FFFF",
					"2312",
					"2B50",
					"2B07",
					"23CD",
					"00"
				}));

			/*** Spencer's Cave Pre-bomb ***/

			// Create New Tristam Chest
			MapObjects[0x18][0x01].Value = 0x1E; // Change Talk Script of NPCs
			MapObjects[0x02C][0x01].RawOverwrite(Blob.FromHex("AD7B6435A60224")); //Copy Venus Chest settings
			MapObjects[0x02C][0x01].Gameflag = (byte)NewGameFlagsList.TristamChestUnopened;
			MapObjects[0x02C][0x01].Value = 0x1D; // Assign
			MapObjects[0x02C][0x01].X = 0x16;
			MapObjects[0x02C][0x01].Y = 0x30;

			GameFlags[(int)NewGameFlagsList.TristamChestUnopened] = true; // Tristam Chest

			// Block spencer's place exit
			GameMaps[(int)MapList.SpencerCave].ModifyMap(0x11, 0x28, new List<List<byte>> { new List<byte> { 0x5B }, new List<byte> { 0x5C } });

			// Change map objects
			MapObjects[0x2C][0x02].CopyFrom(MapObjects[0x2C][0x04]); // Copy box over Phoebe
			MapObjects[0x2C].RemoveAt(0x04); // Delete box
			MapObjects[0x2C][0x03].Sprite = 0x78; // Make Reuben invisible

			// Enter Tile
			TileScripts.AddScript((int)TileScriptsList.EnterSpencersPlace,
				new ScriptBuilder(new List<string> {
					"2E04[09]",
					"2C0E01",
					"2D" + ScriptItemFlags[Items.MegaGrenade].Item1,
					$"050c" + ScriptItemFlags[Items.MegaGrenade].Item2 + "[05]",
					"00",
					"2304",
					"231A",
					"2A10503346305418251825182521255EFF07062A250F0161FFFFFF", 
					"00",
					"2C0F01",
					"00"
			}));

			// Spencer
			List<string> spencerPostItemDialogue = new()
			{
				"A CANAL? Do I look like a Dwarf to you, kid? Then again, I guess with some TNT...",
				"Maybe I should dig a CANAL instead, but that would require some TNT...",
				"I should have kept that TNT and went with my original idea of diggin' a CANAL...",
			};

			TalkScripts.AddScript((int)TalkScriptsList.SpencerFirstTime,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.SpencerItemGiven:X2}[06]",
					"1A32" + TextToHex("This? It's some weird trash I found while diggin' to Captain Mac's ship, you can have it.") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.Spencer]:X2}0162",
					$"23{(int)NewGameFlagsList.SpencerItemGiven:X2}",
					"00",
					"1A32" + TextToHex(rng.PickFrom(spencerPostItemDialogue)) + "36",
					"00"
				}));

			// Tristam Chest
			TalkScripts.AddScript(0x1D,
				new ScriptBuilder(new List<string>{
					"04",
					"2F",
					"050C03[04]",
					"1A000A37FF", // Locked...
					$"2E{(int)NewGameFlagsList.TristamChestUnopened:X2}[06]",
					"08788600",
					"2A2827012C80FBFFFF", // it's fine now?
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamSpencersPlace]:X2}0062",
					$"2B{(int)NewGameFlagsList.TristamChestUnopened:X2}",
					"00"
				}));




			/*** Spencer's Cave Post-Bomb ***/

			// Reproduce spencer/tristam chest to avoid softlock
			var spencerObject = new MapObject(0, this);
			var tristamChestObject = new MapObject(0, this);

			spencerObject.CopyFrom(MapObjects[0x02C][0x00]);
			tristamChestObject.CopyFrom(MapObjects[0x02C][0x01]);

			tristamChestObject.X = 0x2E;
			tristamChestObject.Y = 0x12;
			tristamChestObject.Layer = 0x03;

			spencerObject.X = 0x2E;
			spencerObject.Y = 0x13;
			spencerObject.Layer = 0x03;

			MapObjects[0x2D].RemoveAt(0);
			MapObjects[0x2D].Insert(0, tristamChestObject);
			MapObjects[0x2D].Insert(0, spencerObject);

			/*** Fireburg ***/

			// Reuben
			List<string> reubenDiseaseList = new()
			{
				"Arthritis",
				"Boneitis",
				"Tumasyphilisitisosis",
				"Lupus",
				"Geostigma",
				"Amoria Phlebitis",
				"Cutie Pox",
				"Imminent Death Syndrome",
				"Kalavirus",
				"FoxDie Virus",
				"Phazon Madness",
				"Tiberium Poisoning"
			};

			List<string> reubenJoinDialogueList = new()
			{
				"They almost found a cure for my boneitis, but Tristam sold the company for a cool 100 GP.",
				"I think I have whiplash... I meant ass - whiplash.",
				"COUGH COUGH Desert Fever is no joke..."
			};

			TalkScripts.AddScript((int)TalkScriptsList.ReubenFireburg,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[08]",
					"1A3A" + TextToHex($"Help you? Oh! Uh... Oh no! My {rng.PickFrom(reubenDiseaseList)} is acting up! Arrgh, the pain... No? Alright...") + "36",
					"2A11434146FFFF",
					"0880FF",
					$"05E6{(int)Companion.Reuben:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowFireburgReuben:X2}",
					"00",
					"1A3A" + TextToHex(rng.PickFrom(reubenJoinDialogueList)) + "36",
					"2A11434146FFFF",
					"0880FF",
					$"05E6{(int)Companion.ReubenPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowFireburgReuben:X2}",
					"00",
				}));

			// Arion
			TalkScripts.AddScript((int)TalkScriptsList.Arion,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.ArionItemGiven:X2}[06]",
					"1A3B" + TextToHex("I was scared I would have to pass this on to my useless son, but now, I can give this to you!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.ArionFireburg]:X2}0162",
					$"23{(int)NewGameFlagsList.ArionItemGiven:X2}",
					"00",
					"1A3B" + TextToHex("Would you like to be adopted? We're a loving family for sons that aren't inept at life.") + "36",
					"00"
				}));

			// Seller in Fireburg
			TalkScripts.AddScript((int)TalkScriptsList.FireburgSellerGirl,
				new ScriptBuilder(new List<string>{
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.WomanFireburg]:X2}",
					flags.ProgressiveGear ? "09309411" : "",
					"2BFC",
					$"2E{(int)NewGameFlagsList.FireburgSellerItemBought:X2}BFFE",
					"0E0115F40100", // set price
					"0891FE",
					"2EFDA4D8",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.WomanFireburg]:X2}0462", // give item
					$"23{(int)NewGameFlagsList.FireburgSellerItemBought:X2}",
					"1A40",
					"0ABFFE"
				}));

			// Grenade Man
			TalkScripts.AddScript((int)TalkScriptsList.MegaGrenadeMan,
				new ScriptBuilder(new List<string>{
					"04",
					"2E74[06]",
					"1A3C" + TextToHex("Aaah! Burglars! Please, don't hurt me! Go ahead, take this, this is all I have!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.MegaGrenadeDude]:X2}0162",
					"2374",
					"00",
					"1A3C" + TextToHex("Threathening an old man in his own home. Who do you think you are? A milkdrinking psychopath?") + "36",
					"00"
				}));

			// Grenade Man's Door - Prevent softlock
			// Set door tile to one that jump to a script
			GameMaps[(int)MapList.HouseInterior].ModifyMap(0x37, 0x3C, 0xC9);

			// Hijack Phoebe's FallBasin script since we don't use it
			PutInBank(0x05, 0xFBC2, Blob.FromHex($"{(int)TileScriptsList.FallBasinGiveJumboBomb:X2}")); 

			TileScripts.AddScript((int)TileScriptsList.FallBasinGiveJumboBomb,
				new ScriptBuilder(new List<string> {
					"2D" + ScriptItemFlags[Items.MultiKey].Item1,
					$"050D" + ScriptItemFlags[Items.MultiKey].Item2 + "[04]",
					"2C5C00",
					"00",
					"2C3050",
					"1A00" + TextToHex("It's locked...") + "36",
					"2C2044",
					"1B3C" + TextToHex("I'm not locked up in here with you, kid. You're locked up in here with me!") + "36",
					"2C3044",
					"00",
				}));

			// Tristam Fireburg
			List<string> tristamJoinDialogueList = new()
			{
				"Hey! Call me a Treasure Hunter or I'll rip your lungs out!",
				"Death is always a step behind me..."
			};

			TalkScripts.AddScript((int)TalkScriptsList.TristamInFireburg01,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[06]",
					"1A3D" + TextToHex("Hey! You can get this, it's free! It will only report back some of your personal user data to me.") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
					$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
					"00",
					"1A3D" + TextToHex(rng.PickFrom(tristamJoinDialogueList)) + "36",
					"0F8B0E",
					"057C02[16]",
					"057C03[17]",
					"057C00[18]",
					"2C4146",
					"0880FF",
					$"05E6{(int)Companion.TristamPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowSandTempleTristam:X2}2B{(int)NewGameFlagsList.ShowFireburgTristam:X2}",
					"00",
					"2C114000",
					"2C114100",
					"2C114200"
				}));

			/*** Mine ***/
			// Prevent softlock by gating usage of the elevators with claws.
			TalkScripts.AddMobileScript((int)TalkScriptsList.MysteriousManSealedTemple);
			TalkScripts.AddMobileScript((int)TalkScriptsList.MineElevatorCenter);
			TalkScripts.AddMobileScript((int)TalkScriptsList.MineElevatorBottomRight);

			TalkScripts.AddScript((int)TalkScriptsList.MineElevatorTop,
				new ScriptBuilder(new List<string>
				{
					"04",
					"2D" + ScriptItemFlags[Items.CatClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CatClaw].Item2 + "[09]",
					"2D" + ScriptItemFlags[Items.CharmClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CharmClaw].Item2 + "[09]",
					"2D" + ScriptItemFlags[Items.DragonClaw].Item1,
					$"050c" + ScriptItemFlags[Items.DragonClaw].Item2 + "[09]",
					"1A48" + TextToHex("You'll need some kind of claw to operate this."),
					"00",
					"2C222500",
				}));

			TalkScripts.AddScript((int)TalkScriptsList.MineElevatorEntrance,
				new ScriptBuilder(new List<string>
				{
					"04",
					"2D" + ScriptItemFlags[Items.CatClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CatClaw].Item2 + "[09]",
					"2D" + ScriptItemFlags[Items.CharmClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CharmClaw].Item2 + "[09]",
					"2D" + ScriptItemFlags[Items.DragonClaw].Item1,
					$"050c" + ScriptItemFlags[Items.DragonClaw].Item2 + "[09]",
					"1A49" + TextToHex("You'll need some kind of claw to operate this."),
					"00",
					"2C232500",
				}));

			// Throw Mega Grenade
			TileScripts.AddScript((int)TileScriptsList.BlowingOffMineBoulder,
				new ScriptBuilder(new List<string> {
					$"2E{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[07]",
					$"050f{(int)Companion.Reuben:X2}[07]",
					"2a3046104310443054ffff",
					"1a91" + TextToHex("Ugh, my feet are killing me! Do me a favor and hold this on the way back. It's weighting a ton!"),
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeFallBasin]:X2}0162",
					"2a10414046ffff",
					$"23{(int)NewGameFlagsList.ReubenMineItemGiven:X2}",
					"2E37[10]",
					"2D" + ScriptItemFlags[Items.MegaGrenade].Item1,
					$"050c" + ScriptItemFlags[Items.MegaGrenade].Item2 + "[11]",
					"00",
					"2A105411411140214430461525404626252142214346464746ffff",
					"1a92" + TextToHex("Thanks! I would have died of old age waiting for my incompetent son to save me!") + "36",
					"2A314151424146FFFF",
					"2337",
					"2BDF",
					"00"
				}));

			/*** Volcano Base ***/
			// Medusa - Put medusa over chest
			MapObjects[0x37][0x00].X = MapObjects[0x37][0x0D].X;
			MapObjects[0x37][0x00].Y = MapObjects[0x37][0x0D].Y;
			MapObjects[0x37][0x0D].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.FightMedusa,
				new ScriptBuilder(new List<string>{
					"04",
					"05E45E04",
					"2B27",
					"2375",
					"235D",
					GameFlags[0xBB] ? "" : "23BB",
					"2A60463D46FFFF",
					"00"
				}));

			/*** Lava Dome ***/
			// Fight Hydra
			TileScripts.AddScript((int)TileScriptsList.FightDualheadHydra,
				new ScriptBuilder(new List<string> {
					"2E03[10]",
					"1A4EA2C74E41564C5A41BF4740B95CFF44CE",
					"05E47D06",
					"2364",
					"2B08",
					"2A42FF10501F2529E6FFFF",
					"2A082700200B275EFF2825052A61FFFFFF",
					"2BC7",
					"23CC",
					"2303",
					"00"
				}));

			/*** Rope Bridge ***/
			TileScripts.AddScript((int)TileScriptsList.RopeBridgeFight,
				new ScriptBuilder(new List<string> { "00" }));

			/*** Living Forest ***/
			GameFlags[(int)GameFlagsList.GiantTreeSet] = true;
			GameFlags[(int)GameFlagsList.GiantTreeUnset] = false;

			// Remove Giant Tree Script
			TalkScripts.AddScript((int)TalkScriptsList.GiantTree,
				new ScriptBuilder(new List<string>{
					"00",
				}));
			
			// Fix Alive Forest's Mobius teleporter disapearing after clearing Giant Tree
			MapChanges.Modify(0x0E, 0x17, 0x52);

			/*** Giant Tree ***/
			// Set door to chests in Giant Tree to open only once chimera is defeated
			var treeDoorChangeId = MapChanges.Add(Blob.FromHex("3806122F3F"));
			MapObjects[0x46][0x04].RawOverwrite(Blob.FromHex("0002073816002C")); // Put new map object to talk to
			GameMaps[(int)MapList.GiantTreeB].ModifyMap(0x38, 0x07, 0x3E); // Change map to block exit
			Data[0x02F65D] = 0x08; // Change exit coordinate



			TalkScripts.AddScript(0x02, new ScriptBuilder(new List<string>
				{
					"2e28[04]",
					TextToHex("You may enter.") + "36",
					$"2a{treeDoorChangeId:X2}2a10505eff9700ffff",
					"00",
					TextToHex("Defeat Gidrah and I'll let you pass.") + "36",
					"00"
				}));

			/*** Windia ***/
			TalkScripts.AddMobileScript(0x5D);
			TalkScripts.AddMobileScript(0x5E);
			//TalkScripts.AddMobileScript(0x5F);
			TalkScripts.AddMobileScript(0x60);
			TalkScripts.AddMobileScript(0x61);
			TalkScripts.AddMobileScript(0x62);
            TalkScripts.AddMobileScript(0x63);
            TalkScripts.AddMobileScript(0x64);
            TalkScripts.AddMobileScript(0x65);
			TalkScripts.AddMobileScript(0x66);

			// Captain Mac in Windia
			List<string> captainMacSilence = new()
			{
				"...",
				"I have nothing to say.",
				"I'm speechless...",
				"Silence is golden.",
				"I'm trying to sleep, kid."
			};

			TalkScripts.AddScript((int)TalkScriptsList.CaptainMacWindia,
				new ScriptBuilder(new List<string>{
					TextToHex(rng.PickFrom(captainMacSilence)),
					"36",
					"00"
				}));


			// Seller in Windia
			TalkScripts.AddScript((int)TalkScriptsList.WindiaSellerGirl,
				new ScriptBuilder(new List<string>{
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.GirlWindia]:X2}",
					flags.ProgressiveGear ? "09309411" : "",
					"2BFC",
					$"2E{(int)NewGameFlagsList.WindiaSellerItemBought:X2}BFFE",
					"0E01152C0100", // set price
					"0891FE",
					"2EFDA4D8",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.GirlWindia]:X2}0462", // give item
					$"23{(int)NewGameFlagsList.WindiaSellerItemBought:X2}",
					"1A67",
					"0ABFFE"
				}));

			// Kaeli Windia
			MapObjects[0x51][0x01].Gameflag = 0xFE;
			MapObjects[0x52][0x00].Gameflag = (byte)NewGameFlagsList.ShowWindiaKaeli;
			MapObjects[0x52][0x00].Value = 0x5B;

			TalkScripts.AddScript((int)TalkScriptsList.KaeliWindia,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[06]",
					"1A5B" + TextToHex("Hearty, mate. This is straight from my ?%!& ditty-bag, but I want you to have it!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
					$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
					"00",
					"1A5B" + TextToHex("I'm tired to play &?&% harbour-watch. Let's loose for sea, mate!") + "36",
					"0F8B0E",
					"057C02[16]",
					"057C03[17]",
					"057C01[18]",
					"2C4046",
					"0880FF",
					$"05E6{(int)Companion.KaeliPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",
					"00",
					"2C104000",
					"2C104100",
					"2C104300"
				}));

			// Phoebe Windia
			MapObjects[0x52][0x02].Value = (byte)TalkScriptsList.PhoebeInAquaria;
			TalkScripts.AddScript((int)TalkScriptsList.PhoebeInAquaria,
				new ScriptBuilder(new List<string>{
					TextToHex("Finally, my quest to slay the Dark King is coming to an end! Come, my assistant.") + "36",
					"2C1243",
					"2C4246",
					"0880FF",
					$"05E6{(int)Companion.PhoebePromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}",
					"00"
				}));

			TileScripts.AddScript((int)TileScriptsList.EnterWindiaInn,
				new ScriptBuilder(new List<string> {
					"2C1F0200",
				}));

			// Otto
			TalkScripts.AddScript((int)TalkScriptsList.Otto,
				new ScriptBuilder(new List<string> {
					"04",
					"2E78[11]",
					"2F",
					"050C07[06]",
					"1A5C" + TextToHex("I'm building a trebuchet to cross the chasm. A bridge? I guess... Need a Thunder Rock though.") + "36",
					"00",
					"1A5C" + TextToHex("Oh, you found a Thunder Rock. I'm almost done with my trebuchet though. No? You're sure? Alright...") + "36",
					"0816FC",
					"1A5C" + TextToHex("Good news everyone! The totally safe Rainbow Bridge is done! No tumbling to certain doom for you!") + "36",
					"237823DC",
					"00",
					"1A5C" + TextToHex("I could send you to the Moon with a Thunder Rock powered trebuchet. What, a whale?") + "36",
					"00"
				}));

			/*** Mount Gale ***/
			// Headless Knight
			MapObjects[0x4F][0x0C].X = MapObjects[0x4F][0x00].X;
			MapObjects[0x4F][0x0C].Y = MapObjects[0x4F][0x00].Y;
			MapObjects[0x4F][0x0C].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.FightHeadlessKnight,
				new ScriptBuilder(new List<string>{
					"04",
					"1A54" + TextToHex("The horseman comes! And tonight he comes for you!\nWatch your head!"),
					"05E49604",
					"2B2A",
					"232B",
					GameFlags[0xC0] ? "" : "23C0",
					"2A60463C46FFFF",
					"00",
				}));

			/*** Ship's Dock ***/
			GameFlags[0x1A] = false; // Mac Ship
			GameFlags[0x56] = false; // Mac Ship
			MapObjects[0x60][0x03].Gameflag = 0xFE; // Hide ship because cutescene enable it's flag anyway

			/*** Mac's Ship ***/
			// Put the same chests in pre-cap's mapobject list as post-cap list
			MapObjects[0x62][0x0E].CopyFrom(MapObjects[0x64][0x06]);
			MapObjects[0x62][0x0F].CopyFrom(MapObjects[0x64][0x07]);
			MapObjects[0x62][0x10].CopyFrom(MapObjects[0x64][0x08]);

			// Captain Mac on Ship
			TalkScripts.AddScript((int)TalkScriptsList.CaptainMacOnShip,
				new ScriptBuilder(new List<string> {
					"04",
					"2E59[09]",
					"2F",
					"050C08[06]",
					"1A75" + TextToHex("%&?! youngster think you can just take my craik like that? &?!%! Leave this old salt alone!") + "36",
					"00",
					"1A75" + TextToHex("My &%!? cap! Alright, you can have her, but you bring her back in one piece, ?!&% skip-jack!") + "36",
					"235923572B7F2B802B58",
					"00",
					"1A75" + TextToHex("Gonna hit the bunk now.") + "36",
					"00"
				}));
		}

		public void SetStartingWeapons(ItemsPlacement itemsPlacement)
		{
			if (itemsPlacement.StartingItems.Contains(Items.SteelSword))
			{
				return;
			}
			else if (itemsPlacement.StartingItems.Contains(Items.Axe))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("1000"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.Axe });
			}
			else if (itemsPlacement.StartingItems.Contains(Items.CatClaw))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("0200"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.CatClaw });
			}
			else if (itemsPlacement.StartingItems.Contains(Items.Bomb))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("0040"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.Bomb });
			}
		}
	}

}
