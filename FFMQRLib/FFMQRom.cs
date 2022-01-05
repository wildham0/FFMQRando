using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{

		public ObjectList MapObjects;
		public MapChanges MapChanges;
		public GameFlags GameFlags;
		public GameScriptManager TileScripts;
		public GameScriptManager TalkScripts;

		public override bool Validate()
		{
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(Data);
				if (hash == Blob.FromHex("F71817F55FEBD32FD1DCE617A326A77B6B062DD0D4058ECD289F64AF1B7A1D05"))
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
		public void Randomize(Blob seed, Flags flags)
		{
			MT19337 rng;
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			NodeLocations nodeLocations = new(this);
			EnemiesAttacks enemiesAttacks = new(this);
			EnemiesStats enemiesStats = new(this);
			MapObjects = new(this);
			Credits credits = new(this);
			//MapUtilities mapUtilities = new(this);
			GameFlags = new(this);
			//ExitList exits = new(this);
			TalkScripts = new(this, RomOffsets.TalkScriptsPointers, RomOffsets.TalkScriptPointerQty, RomOffsets.TalkScriptOffset, RomOffsets.TalkScriptEndOffset);
			TileScripts = new(this, RomOffsets.TileScriptsPointers, RomOffsets.TileScriptPointerQty, RomOffsets.TileScriptOffset, RomOffsets.TileScriptEndOffset);
			MapChanges = new(this);

			List<Map> mapList = new();
			for (int i = 0; i < 0x2C; i++)
			{
				mapList.Add(new Map(i, this));
			}

			ExpandRom();
			FastMovement();
			DefaultSettings();
			RemoveClouds();
			SmallFixes();
			CompanionRoutines();
			SetLevelingCurve(flags);

			MapObjects.SetEnemiesDensity(flags, rng);
			MapObjects.ShuffleEnemiesPosition(mapList, flags, rng);
			enemiesAttacks.ScaleAttacks(flags, rng);
			enemiesStats.ScaleEnemies(flags, rng);
			nodeLocations.OpenNodes();



			ItemsPlacement itemsPlacement = new(rng);

			foreach (var item in itemsPlacement.ItemsLocations)
			{
				if (item.Type == TreasureType.Chest)
				{
					Data[RomOffsets.TreasuresOffset + item.ObjectId] = (byte)item.Content;
				}
			}


			UpdateScripts(itemsPlacement, rng);

			credits.Update();

			credits.Write(this);
			enemiesAttacks.Write(this);
			enemiesStats.Write(this);
			MapChanges.Write(this);
			TileScripts.Write(this);
			TalkScripts.Write(this);
			GameFlags.Write(this);
			nodeLocations.Write(this);
			MapObjects.WriteAll(this);
		}
		public void UpdateScripts(ItemsPlacement fullItemsPlacement, MT19337 rng)
		{
			var itemsPlacement = fullItemsPlacement.ItemsLocations.Where(x => x.Type == TreasureType.NPC).ToDictionary(x => (ItemGivingNPCs)x.ObjectId, y => y.Content);
		
			/*** Overworld ***/
			// GameStart - Skip Mountain collapse
			Put(RomOffsets.GameStartScript, Blob.FromHex("23222b7a2a0b2700200470002ab05320501629ffff00"));
			
			GameFlags[(int)GameFlagsList.ShowPazuzuBridge] = true;

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
					"1A15" + TextToHex("!&%?! Look at that rotten hollard! Ok, let's go Johnny raw, we have some cursed tree to chop!") + "36",
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
					"1A16" + TextToHex("Hey, Elixir, let's go!") + "36",
					"0F8B0E",
					"057C01[13]",
					"057C03[14]",
					"2C4146",
					"0880FF",
					$"05E6{(int)Companion.KaeliPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}",
					"00",
					"2C114300",
					"2C114100"
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
					$"2e{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[11]",
					$"050f{(int)Companion.Tristam:X2}[11]",
					"2a3046104130441054ffff",
					"1a85" + TextToHex("Care to invest in my ") + $"1E{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}" + TextToHex(" venture? I'll give you an early prototype!") + "36",
					"08D0FD",
					"050BFB[09]",
					"1a85" + TextToHex("That's fine, not everyone is cut out for massive profits and a lifetime of riches.") + "36",
					"2a10434046ffff",
					"00",
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonBomb]:X2}0162",
					"2a10434046ffff", // 24ff > d3fe
					$"23{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}",
					"00"
				}));

			// Fight Rex
			MapObjects[0x16][0x05].Type = MapObjectType.Chest;
			MapObjects[0x16][0x05].Value = 0x04;
			MapObjects[0x16][0x05].Gameflag = 0xAD;
			Data[0x8004] = (byte)itemsPlacement[ItemGivingNPCs.TristamBoneDungeonElixir];

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
					"0D08788600",
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
					"2A5EFF080161FFFFFF",
					"2A20509053705015271225304506ff0900FFFF",
					$"23{(int)NewGameFlagsList.WakeWaterUsed:X2}",
					"234F", //is it necessary?
					"00",
					"2C0801",
					"00",
				}));

			TileScripts.AddScript((int)TileScriptsList.EnterPhoebesHouse,
				new ScriptBuilder(new List<string> { "2C0A0200" }));

			// Seller in Aquaria
			TalkScripts.AddScript((int)TalkScriptsList.AquariaSellerGirl,
				new ScriptBuilder(new List<string>{
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.WomanAquaria]:X2}",
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
			TalkScripts.AddScript((int)TalkScriptsList.FightSquid,
				new ScriptBuilder(new List<string>{
					"04",
					"05E43110",
					"2B24",
					"23B2",
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
			
			GameFlags[(int)GameFlagsList.ShowFallBasinChest] = false;

			TalkScripts.AddScript((int)TalkScriptsList.FightCrab,
				new ScriptBuilder(new List<string>{
					"04",
					"05E43403",
					"2B25",
					"23B3",
					"2A67463F46FFFF",
					"00"
				}));

			// Enter Fall Basin
			TileScripts.AddScript((int)TileScriptsList.EnterFallBasin,
				new ScriptBuilder(new List<string> {
					"2C0C0100",
				}));

			// Exit Fall Basin
			TileScripts.AddScript((int)TileScriptsList.FallBasinGiveJumboBomb,
				new ScriptBuilder(new List<string> {
					"00",
				}));

			/*** Ice Pyramid ***/
			TileScripts.AddScript((int)TileScriptsList.EnterIcePyramid,
				new ScriptBuilder(new List<string> { "00" }));

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
					"235E",
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
			Data[0x046E3D] = 0x5B;
			Data[0x046E4E] = 0x5C;

			// Change map objects
			MapObjects[0x2C][0x02].RawOverwrite(MapObjects[0x2C][0x04].RawArray()); // Copy box over Phoebe
			MapObjects[0x2C].RemoveAt(0x04); // Delete box
			MapObjects[0x2C][0x03].Sprite = 0x78; // Make Reuben invisible

			// Enter Tile
			TileScripts.AddScript((int)TileScriptsList.EnterSpencersPlace,
				new ScriptBuilder(new List<string> {
					"2E04[08]",
					"2C0E01",
					"2D" + ScriptItemFlags[Items.MegaGrenade].Item1,
					$"050c" + ScriptItemFlags[Items.MegaGrenade].Item2 + "[05]",
					"00",
					"2304",
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
					"2A2827082C80FBFFFF", // need some adjusting
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamSpencersPlace]:X2}0062",
					$"2B{(int)NewGameFlagsList.TristamChestUnopened:X2}",
					"00"
				}));




			/*** Spencer's Cave Post-Bomb ***/

			// Reproduce spencer/tristam chest to avoid softlock
			var spencerObject = new MapObject(0, this);
			var tristamChestObject = new MapObject(0, this);

			spencerObject.RawOverwrite(MapObjects[0x02C][0x00].RawArray());
			tristamChestObject.RawOverwrite(MapObjects[0x02C][0x01].RawArray());

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

			// Tristam Fireburg
			TalkScripts.AddScript((int)TalkScriptsList.TristamInFireburg01,
				new ScriptBuilder(new List<string>{
					"04",
					$"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[06]",
					"1A3D" + TextToHex("Hey! You can get this, it's free! It will only report back some of your personal user data to me.") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
					$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
					"00",
					"1A3D" + TextToHex("Let's go!") + "36",
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
			GameFlags[0xBB] = false;

			TalkScripts.AddScript((int)TalkScriptsList.FightMedusa,
				new ScriptBuilder(new List<string>{
					"04",
					"05E45E04",
					"2B27",
					"2375",
					"235D",
					"23BB",
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

			/*** Giant Tree ***/
			// Set door to chests in Giant Tree to open only once chimera is defeated
			var treeDoorChangeId = MapChanges.Add(Blob.FromHex("3806122F3F"));
			MapObjects[0x46][0x04].RawOverwrite(Blob.FromHex("0002073816002C")); // Put new map object to talk to
			Data[0x0437F4] = 0x3E; // Change map to block exit
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
			// Seller in Windia
			TalkScripts.AddScript((int)TalkScriptsList.WindiaSellerGirl,
				new ScriptBuilder(new List<string>{
					$"0C0015{(int)itemsPlacement[ItemGivingNPCs.GirlWindia]:X2}",
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
					"1A5B" + TextToHex("Take this mate!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
					$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
					"00",
					"1A5B" + TextToHex("Let's go!") + "36",
					"0F8B0E",
					"057C02[16]",
					"057C03[17]",
					"057C01[18]",
					"2C4146",
					"0880FF",
					$"05E6{(int)Companion.KaeliPromo:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowWindiaKaeli:X2}",
					"00",
					"2C114000",
					"2C114100",
					"2C114300"
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

			GameFlags[0xC0] = false;

			TalkScripts.AddScript((int)TalkScriptsList.FightHeadlessKnight,
				new ScriptBuilder(new List<string>{
					"04",
					"1A54" + TextToHex("The horseman comes! And tonight he comes for you!\nWatch your head!"),
					"05E49604",
					"2B2A",
					"232B",
					"23C0",
					"2A60463C46FFFF",
					"00",
				}));

			/*** Ship's Dock ***/
			GameFlags[0x1A] = true; // Mac Ship
			GameFlags[0x56] = false; // Mac Ship
			MapObjects[0x60][0x03].Gameflag = 0xFE; // Hide ship because cutescene enable it's flag anyway

			/*** Mac's Ship ***/
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
	}

}
