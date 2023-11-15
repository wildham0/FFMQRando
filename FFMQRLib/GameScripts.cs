﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void UpdateScripts(Flags flags, ItemsPlacement fullItemsPlacement, LocationIds startinglocation, MT19337 rng)
		{
			const int GameStartScript = 0x01f811;

			var itemsPlacement = fullItemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.NPC).ToDictionary(x => (ItemGivingNPCs)x.ObjectId, y => y.Content);

			/*** Overworld ***/
			// GameStart - Skip Mountain collapse
			Put(GameStartScript, Blob.FromHex($"23222b7a2a0b2700200470002ab0532050{((byte)startinglocation):X2}29ffff00"));

			GameFlags[(int)GameFlagsList.ShowPazuzuBridge] = true;
			
			// Set Pazuzu Initial Floor
			GameFlags[(int)GameFlagsList.ShowPazuzu7F] = false;
			int pazuzuFloor = rng.Between(0, 5);
			for (int i = 0; i < 6; i++)
			{
				GameFlags[(int)GameFlagsList.ShowPazuzu1F + i] = (pazuzuFloor == i);
			}

			for (int i = 0; i < 16; i++)
			{
				GameFlags[(int)NewGameFlagsList.KaeliQuest1 + i] = false;
			}

			// Remove Mine Boulder
			for (int i = 0; i < 6; i++)
			{
				Overworld.RemoveObject(i);
			}

			// Put bridge to access temple
			GameMaps[(int)MapList.Overworld].ModifyMap(0x0F, 0x0E, 0x56);

			/*** Level Forest ***/
			// Copy over Cloudman+Oldman
			for (int i = 0; i < 3; i++)
			{
				MapObjects[0x0E].Add(new MapObject(MapObjects[0x0D][0x00 + i]));
			}

			MapObjects[0x0E][0x12].Sprite = 0x4C;
			MapObjects[0x0E][0x13].Sprite = 0x50;
			MapObjects[0x0E][0x14].Gameflag = 0x34;
			MapObjects[0x0E][0x12].Palette = 0x00; // Cloudman
			MapObjects[0x0E][0x13].Palette = 0x00; // Cloud
			MapObjects[0x0E][0x14].Palette = 0x00; // Old man

			// Put Cloud man first for script and switch oldman too for access
			MapObjects.SwapMapObjects(0x0E, 0x12, 0x00);
			MapObjects.SwapMapObjects(0x0E, 0x13, 0x01);
			MapObjects.SwapMapObjects(0x0E, 0x14, 0x04);

			MapObjects.SwapMapObjects(0x0E, 0x12, 0x05); // Switch back Kaeli at Tree
			MapObjects.SwapMapObjects(0x0E, 0x13, 0x06); // And Kaeli's mom

			// Final Order
			// 0x00 Cloudman
			// 0x01 Cloud
			// 0x02 Minotaur
			// 0x03 Kaeli Entrance
			// 0x04 Oldman
			// 0x05 Kaeli at Tree
			// 0x06 Kaeli's mom

			bool enablekaelismom = false;

			PutInBank(0x01, 0xD6E5, Blob.FromHex("a905ea8de219")); // Set axe swing script to new Kaeli at tree map object

			if (enablekaelismom)
			{
				MapSpriteSets.MapSpriteSets[0x05] = new MapSpriteSet(
					new List<byte> { 0x06, 0x05, 0x47, 0x4a, 0x2a, 0x1e },
					new List<SpriteAddressor>
					{
						new SpriteAddressor(0, 0, 0x39, SpriteSize.Tiles8),  // Rock
						new SpriteAddressor(2, 0, 0x06, SpriteSize.Tiles16), // Kaeli's Mom
						new SpriteAddressor(3, 0, 0x06, SpriteSize.Tiles16), // Kaeli's Mom
						new SpriteAddressor(6, 0, 0x06, SpriteSize.Tiles16), // Kaeli's Mom
						new SpriteAddressor(8, 0, 0x0D, SpriteSize.Tiles16), // Old Man
						new SpriteAddressor(9, 0, 0x05, SpriteSize.Tiles16), // Cloud Man
						new SpriteAddressor(10, 0, 0x14, SpriteSize.Tiles8), // Cloud
					},
					true
					);
			}
			else
			{
				MapSpriteSets.MapSpriteSets[0x05] = new MapSpriteSet(
					new List<byte> { 0x06, 0x05, 0x47, 0x4a, 0x2a, 0x1e },
					new List<SpriteAddressor>
					{
						new SpriteAddressor(0, 0, 0x39, SpriteSize.Tiles8),  // Rock
						new SpriteAddressor(2, 0, 0x01, SpriteSize.Tiles16), // Kaeli Base
						new SpriteAddressor(3, 0, 0x29, SpriteSize.Tiles16), // Kaeli Swing
						new SpriteAddressor(6, 0, 0x06, SpriteSize.Tiles16), // Kaeli's Mom
						new SpriteAddressor(8, 0, 0x0D, SpriteSize.Tiles16), // Old Man
						new SpriteAddressor(9, 0, 0x05, SpriteSize.Tiles16), // Cloud Man
						new SpriteAddressor(10, 0, 0x14, SpriteSize.Tiles8), // Cloud
					},
					true
					);
			}

			// Update Kaeli Tree Map Object
			MapObjects[0x0E][0x03].Gameflag = 0x00;
			MapObjects[0x0E][0x03].Value = (int)TalkScriptsList.KaeliCuttingTree;
			MapObjects[0x0E][0x03].Orientation = 0x00;
			MapObjects[0x0E][0x03].UnknownIndex = 0x00;
			MapObjects[0x0E][0x03].X = 0x2D;
			MapObjects[0x0E][0x03].Y = 0x12;
			MapObjects[0x0E][0x03].Behavior = 0x0A;

			string treecuttingdialogue = enablekaelismom ? "Tree, your death is a small sacrifice, but the path opened is great. Praise the Void!" : "There, griffin. Path is cleared. Let's find that decaying !&%? piece of lumber.";

			TalkScripts.AddScript((int)TalkScriptsList.KaeliCuttingTree,
				new ScriptBuilder(new List<string>
				{
					"04",
					"0F8B0E",
					"057C01[09]",
					"057C03[10]",
					"1A80" + TextToHex(treecuttingdialogue) + "36",
					"2C0344" + "09209511093d8c00" + "2C0825" + "09309511093d8c00" + "2C6822",
					"2A13424346FFFF",
					"23E3",
					"00",
					"2A105210510054FFFF00",
					"2A105210530054FFFF00",
				}
				));

			// Enter Level Forest
			if (enablekaelismom)
			{
				TileScripts.AddScript((int)TileScriptsList.EnterLevelForest,
				new ScriptBuilder(new List<string> {
					"2C0101",
					$"2E{(int)NewGameFlagsList.ShowForestaKaeli:X2}[04]",
					"2EE3[04]",
					"00",
					"2C434600"
				}));
			}
			else
			{
				TileScripts.AddScript((int)TileScriptsList.EnterLevelForest,
				new ScriptBuilder(new List<string> {
					"2C0101",
					$"2E{(int)NewGameFlagsList.ShowForestaKaeli:X2}[05]",
					"2EE3[05]",
					$"050f{(int)CompanionsId.Kaeli:X2}[05]",
					"00",
					"2C434600"
				}));
			}

			// Boulder Man
			TileScripts.AddScript((int)TileScriptsList.PushedBoulder,
				new ScriptBuilder(new List<string> {
					"2E13[03]0F8B0E05090075BC2A14402054FFFF",
					"1A0A" + TextToHex("Finally, after all these years I can go back home.\nHere have this.") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.BoulderOldMan]:X2}0162231323142B34",
					"00"
				}));

			// Boulder Man Talking Script
			TalkScripts.AddScript((int)0x0A,
				new ScriptBuilder(new List<string> {
					"2E13[03]",
					"5BB442B543BFB76A5FB5BFC2B6BE48C0596359B56CFF46C758C173B043BF4C55C6BBC267BC42B4C6BCB7B8CF",
					"00",
					"5B4F7D4473A1B46755C6B856FF1D01FFCCB8C7CF",
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
			if (enablekaelismom)
			{
				// WIP				
				TileScripts.AddScript((int)TileScriptsList.FightMinotaur,
				new ScriptBuilder(new List<string> {
					"050B63[20]",
					"050BE3[20]",
					"2A35460054105a0e2527275246022A05453055FFFF",
					"1A0BAE63FF57FF57CE30ACC8C5C3C5BCC6B8CE36",
					"2A1B278544105430555054FFFF",
					"1A82" + TextToHex("&%?!! That son of a harpooner just poisoned me! Let's do for this &?!% baracoota!"),
					"36",
					"2A754405440054FFFF",
					"05E41714",
					"2A624675441054FFFF",
					"1A82" + TextToHex("You're a &?%! agonist, mate! Here, you earned it. Split a few skulls for me!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliForesta]:X2}0162",
					$"23{(int)NewGameFlagsList.ShowSickKaeli:X2}",
					"2A205465424546FFFF",
					"236D",
					"231E",
					"2B63",
					"2B15",
					"00"
				}));
			}
			else
			{
				TileScripts.AddScript((int)TileScriptsList.FightMinotaur,
				new ScriptBuilder(new List<string> {
					"050B63[21]",
					$"050f{(int)CompanionsId.Kaeli:X2}[22]",
					"2A35460054105a0e2527275246022A05453055FFFF",
					"1A0BAE63FF57FF57CE30ACC8C5C3C5BCC6B8CE36",
					"2A1B278544105430555054FFFF",
					"1A82" + TextToHex("&%?!! That son of a harpooner just poisoned me! Let's do for this &?!% baracoota!"),
					"36",
					"2A75448544754405440054FFFF",
					"05E41714",
					"2A62468544105436465640FFFF",
					"1A82" + TextToHex("You're a &?%! agonist, mate! Here, you earned it. Split a few skulls for me!") + "36",
					"2C7544",
					"2C8544",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliForesta]:X2}0162",
					$"23{(int)NewGameFlagsList.ShowSickKaeli:X2}",
					"0880FF",
					"61",
					"2A1640454666424646FFFF",
					"236D",
					"231E",
					"2B63",
					"2B15",
					Companions.GetQuestString(QuestsId.DefeatMinotaur),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00"
				}));
			}

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
					Companions.GetQuestString(QuestsId.CureKaeli),
					"0F890E",
					"057C0C[14]",
					"057C0E[15]",
					"2C4246",
					"0880FF",
					$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowSickKaeli:X2}23{(int)NewGameFlagsList.KaeliCured:X2}",
					"00",
					"2C124300",
					"2C124100"
				}));

			// Barrel in Oldman's house
			GameFlags[(int)NewGameFlagsList.ShowBarrelMoved] = false;
			GameFlags[(int)NewGameFlagsList.ShowBarrelNotMoved] = true;

			MapObjects[0x11][0x09].Gameflag = (byte)NewGameFlagsList.ShowBarrelNotMoved;
			MapObjects[0x11].Add(new MapObject(MapObjects[0x11][0x09]));
			MapObjects[0x11][0x0B].Gameflag = (byte)NewGameFlagsList.ShowBarrelMoved;
			MapObjects[0x11][0x0B].X--;

			GameMaps[(int)MapList.ForestaInterior].ModifyMap(0x23, 0x1E, 0x36);

			TileScripts.AddScript((int)TileScriptsList.ColumnMoved, new ScriptBuilder(new List<string> {
					"2320",
					"2B21",
					"00"
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
					$"05E6{(int)CompanionsId.Tristam:X2}085B85",
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
					$"050f{(int)CompanionsId.Tristam:X2}[15]",
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
			TileScripts.AddScript((int)TileScriptsList.FightFlamerusRex,
				new ScriptBuilder(new List<string> {
					"2E01[09]",
					"1A1BA0C5C55301B243D14AC1B8C96AB55E42C0B8D23066576741C3586A5A413DFF5A9EB4C53FCE",
					"05E4210C",
					"2A42FF1D25FFFF",
					Companions.GetQuestString(QuestsId.SaveCrystalofEarth),
					Companions.GetQuestString(QuestsId.SaveQtyCrystals),
					"2C29E6",
					"2301",
					"2B06",
					(flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals) ? "050260C11200" : "",
					"00"
				}));

			// Tristam Quit Party Tile
			GameMaps[(int)MapList.BoneDungeon].ModifyMap(0x1D, 0x17, 0x84);

			/*** Focus Tower ***/
			MapObjects[0x0A][0x03].Gameflag = 0x00;
			MapObjects[0x0A][0x05].Gameflag = 0x00;

			GameMaps[(int)MapList.FocusTower].ModifyMap(48, 53, 0x49);

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
					"2A2827062C80FBFFFF", // need some adjusting
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.VenusChest]:X2}0062",
					$"2B{(int)NewGameFlagsList.VenusChestUnopened:X2}",
					"00"
				}));

			MapObjects[0x0A].RemoveAt(4);
			MapObjects[0x0A].RemoveAt(2);

			/*** Libra Temple ***/
			// Phoebe
			TalkScripts.AddScript((int)TalkScriptsList.PhoebeLibraTemple,
				new ScriptBuilder(new List<string>{
					TextToHex("Sure, you can be my sidekick, just don't do anything stupid. I'm the heroine here!") + "36",
					"2C1042",
					"2C4046",
					"0880FF",
					$"05E6{(int)CompanionsId.Phoebe:X2}085B85",
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
					"2C0801",
					"00",
				}));


			//MapObjects[0x18].Add(new MapObject(Blob.FromHex("003F073816002C"))); // Put new map object to talk to
			//MapObjects[0x18][0x06].Coord = (0x10, 0x0E);
			MapObjects[0x18][0x04].Value = 0x3F;
			MapObjects[0x18][0x04].Type = MapObjectType.Talk;

			TalkScripts.AddScript((int)TalkScriptsList.Unknown3f, new ScriptBuilder(new List<string>
				{
					"04",
					"2F",
					"050D02[08]",
					$"23{(int)NewGameFlagsList.WakeWaterUsed:X2}",
					"2A15271225304506ff8E01FFFF",
					"234F",
					Companions.GetQuestString(QuestsId.ThawAquaria),
					"00",
					"1A00" + TextToHex("Maybe the WakeWater can save this poor plant.") + "36",
					"00"
				}));

			// Change Phoebe's script for the house exit to account for aquaria winter/summer 
			TileScripts.AddScript((int)TileScriptsList.EnterPhoebesHouse,
				new ScriptBuilder(new List<string> {
					"2E02" + "[03]",
					"2C6F01",
					"00",
					"2C7001",
					"00"
				}));

			// Take Tristam's script for the INN exit to account for aquaria winter/summer 
			TileScripts.AddScript((int)TileScriptsList.TristamQuitPartyBoneDungeon,
				new ScriptBuilder(new List<string> {
					"2E02" + "[03]",
					"2C7101",
					"00",
					"2C7201",
					"00"
				}));

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
					$"050f{(int)CompanionsId.Phoebe:X2}[07]",
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
					Companions.GetQuestString(QuestsId.DefeatSquidite),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
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
			MapObjects[0x21][0x07].X--;

			MapObjects[0x21][0x0F].X = MapObjects[0x21][0x07].X;
			MapObjects[0x21][0x0F].Y = MapObjects[0x21][0x07].Y;
			MapObjects[0x21][0x0F].Gameflag = 0xFE;

			TalkScripts.AddScript((int)TalkScriptsList.FightCrab,
				new ScriptBuilder(new List<string>{
					"04",
					"05E43403",
					"2B25",
					//GameFlags[(int)GameFlagsList.ShowFallBasinChest] ? "" : "23B3",
					"2A67463F46FFFF",
					Companions.GetQuestString(QuestsId.DefeatSnowCrab),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00"
				}));

			// Remove Phoebe Script Tile
			GameMaps[(int)MapList.FallBasin].ModifyMap(0x11, 0x06, 0x1D, true);

			// Fix pillar softlock
			GameMaps[(int)MapList.FallBasin].ModifyMap(0x03, 0x17, 
				new()
				{ 
					new() { 0x1D, 0x0D, 0x0E },
					new() { 0x1D, 0x1D, 0x1E },
				});

			// Exit Fall Basin
			GameMaps.TilesProperties[0x0A][0x22].Byte2 = 0x08;

			/*** Ice Pyramid ***/
			// Ice Pyramid Entrance
			var pyramidTeleporter = EntrancesData.Entrances.Find(x => x.Id == 456).Teleporter;
			TileScripts.AddScript((int)TileScriptsList.EnterIcePyramid,
				new ScriptBuilder(new List<string> {
					"2F",
					"050C06[03]",
					"23F2",
					$"2C{pyramidTeleporter.id:X2}{pyramidTeleporter.type:X2}",
					"00",
				}));

			EntrancesData.Entrances.Find(x => x.Id == 456).Teleporter = (32, 8);

			// Change entrance tile to disable script
			GameMaps[(int)MapList.IcePyramidA].ModifyMap(0x15, 0x20, 0x05);

			// Open 4F door to avoid softlock in floor shuffle
			if (flags.MapShuffling != MapShufflingMode.None && flags.MapShuffling != MapShufflingMode.Overworld)
			{ 
				GameMaps[(int)MapList.IcePyramidA].ModifyMap(0x37, 0x06,
					new()
					{
						new() { 0xBF },
						new() { 0xCF },
					});
			}

			// Change tile properties from falling tile to script tile
			GameMaps.TilesProperties[0x06][0x1E].Byte2 = 0x88;

			TileScripts.AddScript((int)TileScriptsList.IcePyramidCheckStatue,
				new ScriptBuilder(new List<string>
				{
					"2D" + ScriptItemFlags[Items.CatClaw].Item1,
					$"050c" + ScriptItemFlags[Items.CatClaw].Item2 + "[17]", // check sword
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
					"2D" + ScriptItemFlags[Items.SteelSword].Item1,
					$"050c" + ScriptItemFlags[Items.SteelSword].Item2 + "[30]", // goto teleport
					"2D" + ScriptItemFlags[Items.KnightSword].Item1,
					$"050c" + ScriptItemFlags[Items.KnightSword].Item2 + "[30]",
					"2D" + ScriptItemFlags[Items.Excalibur].Item1,
					$"050c" + ScriptItemFlags[Items.Excalibur].Item2 + "[30]",
					"0F8B0E",
					"057C00[13]", // looking up
					"057C01[14]",// looking right
					"057C02[15]",  // lookingdown
					"057C03[16]",// looking left
					"1A48" + TextToHex("I should bring a sword with me before going down there.") + "36",
					"00",
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
					"2A42FF1E25FFFF",
					Companions.GetQuestString(QuestsId.SaveCrystalofWater),
					Companions.GetQuestString(QuestsId.SaveQtyCrystals),
					"2C29E6",
					"2312",
					"2B50",
					"2B07",
					"23CD",
					(flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals) ? "050260C11200" : "00"
				}));

			// Update crystal to not show up after wakewater use
			MapObjects[0x02A][0x06].Gameflag = 0x12;
			MapObjects[0x02A][0x07].Gameflag = 0x12;

			/*** Spencer's Cave Pre-bomb ***/
			// Create New Tristam Chest
			MapObjects[0x18][0x01].Value = 0x1E; // Change Talk Script of NPCs
			MapObjects[0x02C][0x01].RawOverwrite(Blob.FromHex("AD7B6435A60224")); //Copy Venus Chest settings
			MapObjects[0x02C][0x01].Gameflag = (byte)NewGameFlagsList.TristamChestUnopened;
			MapObjects[0x02C][0x01].Value = 0x1D; // Assign
			MapObjects[0x02C][0x01].X = 0x16;
			MapObjects[0x02C][0x01].Y = 0x30;
			MapObjects[0x02C][0x01].Palette = 0x01;

			GameFlags[(int)NewGameFlagsList.TristamChestUnopened] = true; // Tristam Chest

			// Block spencer's place exit
			GameMaps[(int)MapList.SpencerCave].ModifyMap(0x10, 0x28, new List<List<byte>> { new List<byte> { 0x3D, 0x3D, 0x3D }, new List<byte> { 0x3E, 0x3E, 0x3E } });

			// Change map objects
			MapObjects[0x2C][0x02].CopyFrom(MapObjects[0x2C][0x04]); // Copy box over Phoebe
			MapObjects[0x2C].RemoveAt(0x04); // Delete box
			MapObjects[0x2C][0x03].Sprite = 0x78; // Make Reuben invisible
			MapObjects[0x2C][0x02].Palette = 0x01;

			// Update palette so that chests/boxes don't reflect water
			MapSpriteSets.MapSpriteSets[0x10].Palette.RemoveAt(0x01);
			MapSpriteSets.MapSpriteSets[0x10].Palette.Insert(0x01, 0x1E);

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
					"2A105033463054182521255EFF07062A250F0161FFFFFF",
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
			var spencerObject = new MapObject();
			var tristamChestObject = new MapObject();
			var boxObject = new MapObject();

			spencerObject.CopyFrom(MapObjects[0x02C][0x00]);
			tristamChestObject.CopyFrom(MapObjects[0x02C][0x01]);
			boxObject.CopyFrom(MapObjects[0x02C][0x02]);

			tristamChestObject.X = 0x2E;
			tristamChestObject.Y = 0x12;
			tristamChestObject.Layer = 0x03;

			spencerObject.X = 0x2E;
			spencerObject.Y = 0x13;
			spencerObject.Layer = 0x03;

			boxObject.X = 0x2C;
			boxObject.Y = 0x0F;
			boxObject.Layer = 0x03;

			MapObjects[0x2D].RemoveAt(0);
			MapObjects[0x2D].Insert(0, boxObject);
			MapObjects[0x2D].Insert(0, tristamChestObject);
			MapObjects[0x2D].Insert(0, spencerObject);

			/*** Fireburg ***/
			// Create extra Reuben object
			MapObjects[0x30].Add(new MapObject(MapObjects[0x30][0x01]));
			var reubenobject = MapObjects[0x30][0x04];

			reubenobject.Gameflag = 0xFE;
			reubenobject.Value = 0x50;
			reubenobject.X = 0x21;
			reubenobject.Y = 0x2B;

			// Process Quest
			var reubenquest = Companions.GetQuestFlag(QuestsId.VisitMine, CompanionsId.Reuben);
			TileScripts.AddScript((int)TileScriptsList.EnterReubenHouse,
				new ScriptBuilder(new List<string>
				{
					"2C1002",
					(reubenquest != NewGameFlagsList.None) ? $"2E{(int)reubenquest:X2}[08]" : "00",
					$"050f{(int)CompanionsId.Reuben:X2}[08]",
					$"050B{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[08]",
					"2A3446144314443054FFFF",
					"1A50" + TextToHex("Whew. I'm exhausted. Let's never go back to the Mine again.") + "36",
					Companions.GetQuestString(QuestsId.VisitMine),
					"2A14414446FFFF",
					"00",
				}));

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
					$"05E6{(int)CompanionsId.Reuben:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowFireburgReuben:X2}",
					"00",
					"1A3A" + TextToHex(rng.PickFrom(reubenJoinDialogueList)) + "36",
					"2A11434146FFFF",
					"0880FF",
					$"05E6{(int)CompanionsId.Reuben:X2}085B85",
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

			// Enter Fireburg Hotel
			// Create extra Tristam object
			MapObjects[0x31].Add(new MapObject(MapObjects[0x31][0x0B]));
			MapObjects[0x31][0x0B].CopyFrom(MapObjects[0x31][0x01]);
			var tristamobject = MapObjects[0x31][0x0B];

			tristamobject.Gameflag = 0xFE;
			tristamobject.Value = 0x50;
			tristamobject.X = 0x11;
			tristamobject.Y = 0x35;

			// Process Quest
			var tristamquest = Companions.GetQuestFlag(QuestsId.VisitBoneDungeon, CompanionsId.Tristam);

			TileScripts.AddScript((int)TileScriptsList.EnterFireburgHotel,
				new ScriptBuilder(new List<string>
				{
					"2C1102",
					$"050f{(int)CompanionsId.Tristam:X2}[18]",
					 (tristamquest != NewGameFlagsList.None) ? $"2E{(int)tristamquest:X2}[10]" : "0A[10]",
					$"050B{(int)NewGameFlagsList.TristamBoneDungeonItemGiven:X2}[10]",
					"2A3B461B431B443054FFFF",
					"1A50" + TextToHex("That was some good dungeon pillaging back there. Let's get some drinks!") + "36",
					Companions.GetQuestString(QuestsId.VisitBoneDungeon),
					"08[15]",
					"2A1B414B46FFFF",
					"00",
					$"2E{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}[18]",
					"2A3B461B431B443054FFFF",
					"08[15]",
					"2A1B414B46FFFF",
					"00",
					"1A50" + TextToHex("Tell you what, I'll give you this and you cover my tab, deal?") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.TristamFireburg]:X2}0162",
					$"23{(int)NewGameFlagsList.TristamFireburgItemGiven:X2}",
					"00"
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
					$"05E6{(int)CompanionsId.Tristam:X2}085B85",
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

			// Jinn Fight
			TileScripts.AddScript((int)TileScriptsList.FightJinn,
				new ScriptBuilder(new List<string>
				{
					"050B26[07]",
					"2C2727",
					"05E4520D",
					"2C6846",
					"2B26",
					Companions.GetQuestString(QuestsId.DefeatJinn),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00",
				}));

			// Throw Mega Grenade
			TileScripts.AddScript((int)TileScriptsList.BlowingOffMineBoulder,
				new ScriptBuilder(new List<string> {
					$"2E{(int)NewGameFlagsList.ReubenMineItemGiven:X2}[07]",
					$"050f{(int)CompanionsId.Reuben:X2}[07]",
					"2a3046104310443054ffff",
					"1a91" + TextToHex("Ugh, my feet are killing me! Do me a favor and hold this on the way back. It's weighting a ton!"),
					$"0d5f01{(int)itemsPlacement[ItemGivingNPCs.PhoebeFallBasin]:X2}0162",
					"2a10414046ffff",
					$"23{(int)NewGameFlagsList.ReubenMineItemGiven:X2}",
					"2E37[10]",
					"2D" + ScriptItemFlags[Items.MegaGrenade].Item1,
					$"050c" + ScriptItemFlags[Items.MegaGrenade].Item2 + "[11]",
					"00",
					"2A105411411140214410541525105426252142214346464746ffff",
					"1a92" + TextToHex("Thanks! I would have died of old age waiting for my incompetent son to save me!") + "36",
					Companions.GetQuestString(QuestsId.SaveArion),
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
					Companions.GetQuestString(QuestsId.DefeatMedusa),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00"
				}));

			if (flags.MapShuffling != MapShufflingMode.None && flags.MapShuffling != MapShufflingMode.Overworld)
			{
				var volcanoTeleporter = EntrancesData.Entrances.Find(x => x.Id == 464).Teleporter;

				TileScripts.AddScript((int)TileScriptsList.RopeBridgeFight,
					new ScriptBuilder(new List<string> {
						"2F",
						"050C05[03]",
						"23F2",
						$"2C{volcanoTeleporter.id:X2}{volcanoTeleporter.type:X2}",
						"00"
					}));

				EntrancesData.Entrances.Find(x => x.Id == 464).Teleporter = (15, 8);
			}
			else
			{
				TileScripts.AddScript((int)TileScriptsList.RopeBridgeFight,
					new ScriptBuilder(new List<string> {
						"2BF2",
						"2C8801",
						"00"
					}));
			}

			TileScripts.AddScript((int)TileScriptsList.VolcanoTeleportToBase,
				new ScriptBuilder(new List<string> {
					"2BF2",
					"2C8901",
					"00"
				}));
			TileScripts.AddScript((int)TileScriptsList.VolcanoTeleportFromTop,
				new ScriptBuilder(new List<string> {
					"2F",
					"050C05[03]",
					"23F2",
					"2C8B01",
					"00"
				}));
			TileScripts.AddScript((int)TileScriptsList.VolcanoExtraScript,
				new ScriptBuilder(new List<string> {
					"2F",
					"050C05[03]",
					"23F2",
					"2C8A01",
					"00"
				}));

			/*** Lava Dome ***/
			// Pointless Ledge Quest
			GameMaps[(int)MapList.LavaDomeInteriorA].ModifyMap(0x04, 0x04, new List<List<byte>>() { new() { 0x59 }, new() { 0x59 } });

			var pointlessledgeflag = Companions.GetQuestFlag(QuestsId.VisitPointlessLedge);

			TileScripts.AddScript((int)TileScriptsList.PointlessLedgeQuest,
				new ScriptBuilder(new List<string> {
					(pointlessledgeflag != NewGameFlagsList.None) ? $"2E{(int)pointlessledgeflag:X2}[03]" : "00",
					"1A00" + TextToHex("Well, that was pointless.\n...Or was it?") + "36",
					Companions.GetQuestString(QuestsId.VisitPointlessLedge),
					"00"
				}));


			// Fight Hydra
			TileScripts.AddScript((int)TileScriptsList.FightDualheadHydra,
				new ScriptBuilder(new List<string> {
					"2E03[13]",
					"1A4EA2C74E41564C5A41BF4740B95CFF44CE",
					"05E47D06",
					"2364",
					"2B08",
					"2A42FF10501F25FFFF",
					Companions.GetQuestString(QuestsId.SaveCrystalofFire),
					Companions.GetQuestString(QuestsId.SaveQtyCrystals),
					"2A29E608270020052AFFFF",
					"2BC7",
					"23CC",
					"2303",
					(flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals) ? "050260C11200" : "00",
					"00"
				}));

			/*** Rope Bridge ***/
			GameMaps[(int)MapList.RopeBridge].ModifyMap(0xD, 0x0C, 0x38);
			/*
			TileScripts.AddScript((int)TileScriptsList.RopeBridgeFight,
				new ScriptBuilder(new List<string> { "00" }));*/

			/*** Living Forest ***/
			GameFlags[(int)GameFlagsList.GiantTreeSet] = true;
			GameFlags[(int)GameFlagsList.GiantTreeUnset] = false;
			bool exitToGiantTree = flags.MapShuffling == MapShufflingMode.None;

			// Remove Giant Tree Script
			TalkScripts.AddScript((int)TalkScriptsList.GiantTree,
				new ScriptBuilder(new List<string>{
					"00",
				}));

			// Fix Alive Forest's Mobius teleporter disapearing after clearing Giant Tree
			var crestTile = GameMaps[(int)MapList.LevelAliveForest].TileValue(8, 52);
			MapChanges.Modify(0x0E, 0x17, crestTile);

			if (!exitToGiantTree)
			{
				MapChanges.RemoveActionByFlag(0x43, 0x3C);
			}

			// Tree Houses Quest
			var treehousesflag = Companions.GetQuestFlag(QuestsId.VisitTreeHouses);

			if (treehousesflag != NewGameFlagsList.None)
			{
				List<(byte x, byte y)> treehouseslocations = new() { (0x1D, 0x37), (0x05, 0x31), (0x12, 0x22) };
				List<byte> treehousesnpclook = new() { 0x60, 0x64, 0x68, 0x6C, 0x70, 0x74 };

				MapObjects[0x11].Add(new MapObject(MapObjects[0x11][0x01]));
				MapObjects[0x11][0x0C].Coord = rng.PickFrom(treehouseslocations);
				MapObjects[0x11][0x0C].Sprite = rng.PickFrom(treehousesnpclook);
				MapObjects[0x11][0x0C].Value = (byte)TalkScriptsList.TreeHouseQuestNPC;
			}

			TalkScripts.AddScript((int)TalkScriptsList.TreeHouseQuestNPC,
				new ScriptBuilder(new List<string> {
					$"2E{(int)treehousesflag:X2}[04]",
					TextToHex("Even behind magical teleporting crests I cannot find quietude!") + "36",
					Companions.GetQuestString(QuestsId.VisitTreeHouses),
					"00",
					TextToHex("Maybe no one will find me behind that giant boulder in the Mine.") + "36",
					"00"
				}));

			/*** Giant Tree ***/
			// Set door to chests in Giant Tree to open only once chimera is defeated
			var treeDoorChangeClosed = MapChanges.Add(Blob.FromHex("3806122F3E"));
			MapObjects[0x46].Add(new MapObject(Blob.FromHex("2802073816002C"))); // Put new map object to talk to
			MapChanges.AddAction(0x46, 0x28, treeDoorChangeClosed, 0x22);

			TalkScripts.AddScript(0x02, new ScriptBuilder(new List<string>
				{
					"2e28[02]",
					"00",
					TextToHex("Defeat Gidrah and I'll let you pass.") + "36",
					"00"
				}));

			// Add check for Dragon Claw on 2F to avoid softlock
			PutInBank(0x06, 0x93F4, Blob.FromHex("4c4d5c5d")); // Add new tile+properties to execute script
			GameMaps.TilesProperties[0x09][0x7D].Byte1 = 0x00;
			GameMaps.TilesProperties[0x09][0x7D].Byte2 = 0x88;
			GameMaps[(int)MapList.GiantTreeA].ModifyMap(0x2E, 0x33, 0x7D);

			TileScripts.AddScript((int)TileScriptsList.EnterFallBasin,
				new ScriptBuilder(new List<string> {
					"2D" + ScriptItemFlags[Items.DragonClaw].Item1,
					$"050c" + ScriptItemFlags[Items.DragonClaw].Item2 + "[04]",
					"1A48" + TextToHex("Wait, I'll need the Dragon Claw to come back up!") + "36",
					"2C105000",
					"0cee19000cef1916094cb20100" // Hack to excute the falling down routine
				}));

			// Add hook on 3F to avoid softlock
			MapObjects[0x47].Add(new MapObject(MapObjects[0x47][0x15]));
			MapObjects[0x47][0x16].Coord = (0x2D, 0x36);

			// Fight Gidrah
			TalkScripts.AddScript((int)TalkScriptsList.FightGidrah,
				new ScriptBuilder(new List<string> {
					"04",
					"05E49107",
					"2C6146",
					"2C0321",
					"2B28",
					"2329",
					Companions.GetQuestString(QuestsId.DefeatGidrah),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00"
				}));

			// Giant Tree Walking Script
			var newGidrahLocation = GameLogic.FindTriggerLocation(AccessReqs.Gidrah);
			TalkScripts.AddScript(0x50, new ScriptBuilder(new List<string>
				{
					"2E28[10]",
					"66B4C0FFBAC5B4C7B8B9C8494655B95CFFB55EC7483FC2C6407C4BC6CE309ABFBF58FFC04046B6B4C5C559446F",
					"36",
					"2A142A10505EFFAA00072B10511B25FFFF",
					"23FE",
					"2A1C2510531053062BAB0061FF" + (exitToGiantTree ? "2E" : $"{(int)newGidrahLocation:X2}") + "29" + "FFFF",
					"2BFE",
					"233C",
					"236A",
					"00",
					"A2B9FF55B64FFFB55E4220484D417C4B45CABC4AB7BCC6B4C3C35EC5CE",
					"00"
				}));

			// Remove Kaeli
			MapObjects[0x4C].RemoveAt(0);

			// Change entrance to not teleport to giant tree ow location
			if (exitToGiantTree)
			{
				TileScripts.AddScript(0x31, new ScriptBuilder(new List<string>
				{
					"2E3C[03]",
					"2C3801",
					"00",
					"2C0900",
					"00"
				}));

				EntrancesData.Entrances.Find(x => x.Id == 278).Teleporter = (0x31, 8);
				EntrancesData.Entrances.Find(x => x.Id == 279).Teleporter = (0x31, 8);
			}
			else
			{
				var disableGiantTreeEntrance = new ScriptBuilder(new List<string> {
						"0A06A5",
					});

				disableGiantTreeEntrance.WriteAt(0x03, 0xA624, this);
			}

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
					$"05E6{(int)CompanionsId.Kaeli:X2}085B85",
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
					$"05E6{(int)CompanionsId.Phoebe:X2}085B85",
					$"2B{(int)NewGameFlagsList.ShowWindiaPhoebe:X2}",
					"00"
				}));

			// Otto
			TalkScripts.AddScript((int)TalkScriptsList.Otto,
				new ScriptBuilder(new List<string> {
					"04",
					"2E78[12]",
					"2F",
					"050C07[06]",
					"1A5C" + TextToHex("I'm building a trebuchet to cross the chasm. A bridge? I guess... Need a Thunder Rock though.") + "36",
					"00",
					"1A5C" + TextToHex("Oh, you found a Thunder Rock. I'm almost done with my trebuchet though. No? You're sure? Alright...") + "36",
					"0816FC",
					"1A5C" + TextToHex("Good news everyone! The totally safe Rainbow Bridge is done! No tumbling to certain doom for you!") + "36",
					"237823DC",
					Companions.GetQuestString(QuestsId.BuildRainbowBridge),
					"00",
					"1A5C" + TextToHex("I could send you to the Moon with a Thunder Rock powered trebuchet. What, a whale?") + "36",
					"00"
				}));

			// Windia INN
			// Create extra Phoebe object
			MapObjects[0x52][0x03].CopyFrom(MapObjects[0x52][0x02]);
			var phoebeobject = MapObjects[0x52][0x03];

			phoebeobject.Gameflag = 0xFE;
			phoebeobject.Value = 0x50;
			phoebeobject.X = 0x35;
			phoebeobject.Y = 0x1B;

			// Process Quest
			var phoebequest = Companions.GetQuestFlag(QuestsId.VisitWintryCave, CompanionsId.Phoebe);
			TileScripts.AddScript((int)TileScriptsList.EnterWindiaInn,
				new ScriptBuilder(new List<string>
				{
					"2C1F02",
					(phoebequest != NewGameFlagsList.None) ? $"2E{(int)phoebequest:X2}[08]" : "00",
					$"050f{(int)CompanionsId.Phoebe:X2}[08]",
					$"050B{(int)NewGameFlagsList.PhoebeWintryItemGiven:X2}[08]",
					"2A3346134313443054FFFF",
					"1A50" + TextToHex("Well, you did survive the Wintry Cave, you can rest a bit before we go to Doom Castle.") + "36",
					Companions.GetQuestString(QuestsId.VisitWintryCave),
					"2A13414346FFFF",
					"00",
				}));

			// Enter Inn bedroom with Kaeli
			MapObjects[0x52][0x04].CopyFrom(MapObjects[0x52][0x00]);
			var kaeliobject = MapObjects[0x52][0x04];

			kaeliobject.Gameflag = 0xFE;
			kaeliobject.Value = 0x51;
			kaeliobject.X = 0x21;
			kaeliobject.Y = 0x3B;

			TileScripts.AddScript((int)TileScriptsList.EnterWindiaInnBedroom,
				new ScriptBuilder(new List<string>
				{
					"2CD700",
					$"2E{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}[07]",
					"2A3446144314443054FFFF",
					"1A50" + TextToHex("Hearty, mate. This is straight from my ?%!& ditty-bag, but I want you to have it!") + "36",
					$"0D5F01{(int)itemsPlacement[ItemGivingNPCs.KaeliWindia]:X2}0162",
					$"23{(int)NewGameFlagsList.KaeliSecondItemGiven:X2}",
					"2A14414446FFFF",
					"00",
				}));

			// Chocobo Quest
			var chocoboquestflag = Companions.GetQuestFlag(QuestsId.VisitChocobo);

			TalkScripts.AddScript((int)TalkScriptsList.WindiaChocobo,
				new ScriptBuilder(new List<string> {
					"04",
					"1A00B057423FB853CFFFFF9AFF9CBBC2B6C2B5C2CECE36",
					(chocoboquestflag != NewGameFlagsList.None) ? $"2E{(int)chocoboquestflag:X2}[04]" : "00",
					Companions.GetQuestString(QuestsId.VisitChocobo),
					"00"
				}));

			/*** Light Temple ***/
			// Light Temple Quest
			GameMaps[(int)MapList.Caves].ModifyMap(0x22, 0x27, new List<List<byte>>() { new() { 0xCF, 0xCF } });

			var ligthtempleflag = Companions.GetQuestFlag(QuestsId.VisitLightTemple);

			TileScripts.AddScript((int)TileScriptsList.LightTempleQuest,
				new ScriptBuilder(new List<string> {
					(ligthtempleflag != NewGameFlagsList.None) ? $"2E{(int)ligthtempleflag:X2}[03]" : "00",
					"1A00" + TextToHex("Finally! Hidden corridors should be shaded like in FF2...") + "36",
					Companions.GetQuestString(QuestsId.VisitLightTemple),
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
					Companions.GetQuestString(QuestsId.DefeatDullahan),
					Companions.GetQuestString(QuestsId.DefeatQtyMinibosses),
					"00",
				}));

			// Mount Gale Quest
			GameMaps[(int)MapList.MountGale].ModifyMap(0x2C, 0x07, 0x48);

			var mountgalequestflag = Companions.GetQuestFlag(QuestsId.VisitMountGale);

			TileScripts.AddScript((int)TileScriptsList.MountGaleQuest,
				new ScriptBuilder(new List<string> {
					(mountgalequestflag != NewGameFlagsList.None) ? $"2E{(int)mountgalequestflag:X2}[03]" : "00",
					"1A00" + TextToHex("Feels like something should be here...") + "36",
					Companions.GetQuestString(QuestsId.VisitMountGale),
					"00"
				}));

			/*** Pazuzu's Tower ***/
			bool skip7fteleport = flags.MapShuffling != MapShufflingMode.None && flags.MapShuffling != MapShufflingMode.Overworld;

			var newResetflags = new ScriptBuilder(new List<string> {
						"2B47",
						"2B48",
						"2B49",
						"2B40",
						"2B41",
						"2B42",
						"2B43",
						"2B44",
						"2B45",
						"2B46",
						"00"
					});

			var newPazuzuScript = new ScriptBuilder(new List<string> {
						"2C50FF",
						"07A0C112",
						skip7fteleport ? "00" : "0898FD",
						"00"
					});

			var jumpToResetFlagsShort = new ScriptBuilder(new List<string> {
						"07A6C11200",
					});
			var newJumpInRoutineToReset = new ScriptBuilder(new List<string> {
						"08D5FF",
					});

			var standardCrystalScript = new ScriptBuilder(new List<string>
				{
					"2A20500527205410575EFF4E01A057260161FF105300542025FFFF",
					Companions.GetQuestString(QuestsId.SaveCrystalofWind),
					Companions.GetQuestString(QuestsId.SaveQtyCrystals),
					"2C29E6",
					"2305",
					flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals ? "050260C11200" : "00",
					"00"
				});

			var floorCrystalScript = new ScriptBuilder(new List<string>
				{
					"2E05[07]",
					"2A61FF105310502025FFFF",
					Companions.GetQuestString(QuestsId.SaveCrystalofWind),
					Companions.GetQuestString(QuestsId.SaveQtyCrystals),
					"2C29E6",
					"2305",
					flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals ? "050260C11200" : "00",
					"00"
				});

			newResetflags.WriteAt(0x12, 0xC1A0, this);
			newJumpInRoutineToReset.WriteAt(0x03, 0xFC6B, this);
			jumpToResetFlagsShort.WriteAt(0x03, 0xFFD5, this);
			newPazuzuScript.WriteAt(0x03, 0xFD8C, this);

			if (skip7fteleport)
			{
				floorCrystalScript.WriteAt(0x03, 0xFD98, this);

				TileScripts.AddScript(0x26,
					new ScriptBuilder(new List<string>{
						"0A98FD"
					}));

				//GameMaps[(int)MapList.PazuzuTowerB].ModifyMap(0x10, 0x28, 0x78);
				MapChanges.Replace(0x12, Blob.FromHex("0F26334e4e4e4e2020794e20"));
				MapObjects[0x59][0x06].Coord = (0x10, 0x28);
			}
			else
			{
				standardCrystalScript.WriteAt(0x03, 0xFD98, this);
			}

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

			/*** Doom Castle ***/
			TileScripts.AddScript((int)TileScriptsList.HeroStatue,
				new ScriptBuilder(new List<string> {
					"2E39[07]",
					"2A14250C27B05520501925FFFF",
					"1AA7" + TextToHex("... Knights of the Light...\nWe Crystals now entrust you with our power.") + "36",
					"0829E6",
					"2339",
					"2B19",
					"2A0C260527052B0221FFFF",
					"00"
				}));


			/*** Ending ***/
			MapObjects[0x06][0x01].X = 0x10;
			MapObjects[0x06][0x01].Y = 0x0F;
			MapObjects[0x06][0x01].Gameflag = 0xF4;

			string darkKingFight = "29950C0106030800801A0020504DCA40C0B8B842B442BFB465531B7AA2C1B7B8B8B74D4F4CC158FF55CABC4AC64B67C0B8533166B7547E3F477DC6C26FB040CA547EB4BFBF58FF5546C5C87B43C5FFB05CBFB7D2329ABFB4C64DC6BCBFBF59BBC87AC1C64D66C06DC657C54078C74BC5BCB5BF40C6B8B6C5B8C7CE305BB442A9C5C2C3BBB8B6CCCF019ABA60FFB4BA726665B4C5C76870C5C8C05CCE30B076B669404641C3586A5A9DB4C5BEC160C6CE23E405E4CE000C010600080080";
			string flagSwitch = "6123022304235923572B7F2B802B582B542355";

			TileScripts.AddScript((int)TileScriptsList.FightDarkKing,
				new ScriptBuilder(new List<string> {
					darkKingFight,
					flagSwitch,
					"2A0121FFFF",
					"0C010618080080",
					"2A3027005402443055024507251427FFFF",
					"05E8",
					"08EE85",
					"09A09411",
					"09A09411",
					"23F4",
					"23F3",
					"2A2E251D0140511052305110501E051051605010531050FFFF",
					"09A09411",
					"2362",
					"2A10537052105130520201105030532050000550502144E1453344FFFF",
					"09A09411",
					"2AB0541054FFFF",
					"09A09411",
					"2A314561440054FFFF",
					"09A09411",
					"2AB0541054FFFF",
					"09A09411",
					"2A104230441344FFFF",
					"09A09411",
					"2A5052090150502053105230531050805310503053520130520144FFFF",
					"09A09411",
					"2A6050140120536050505310501105FFFF",
					"0C010614",
					"080080",
					"2AC050205300541442FFFF",
					"2B80",
					"09A09411",
					"2AB05220514052FFFF",
					"0C010618",
					"080080",
					"2A110606FF530106FF1054FFFF",
					"09A09411",
					"2C3054",
					"09A09411",
					"2A1052164437443054FFFF",
					"09A09411",
					"2C1054",
					"09A09411",
					"2A2050154439443054FFFF",
					"09A09411",
					"2C1054",
					"09A09411",
					"2A1050124434443054FFFF",
					"09A09411",
					"2C1054",
					"09A09411",
					"2A21440145FFFF",
					"09A09411",
					"2A1050104431441054FFFF",
					"09A09411",
					"2C3054",
					"09A09411",
					"2A50442050205403440844054409440244044400440144A054FFFF",
					"09A09411",
					"2A4050205440404140A0543050B0541050510106FFE055A054B3E15001FFFF",
					"0920940C",
					"0C010613",
					"080080",
					"2A51013055105406FF305406FF1054B3E13146814300541142FFFF",
					"09A09411",
					"2CE055",
					"2C5001",
					"0C010619",
					"080080",
					"0994940C",
				}));
		}
	}
}
