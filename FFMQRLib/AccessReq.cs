﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public partial class ItemLocations
	{
		public static TreasureObject Foresta01 = new TreasureObject(0x2D, (int)MapList.Foresta, Locations.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs>{ AccessReqs.Axe } });
		public static TreasureObject Fireburg01 = new TreasureObject(0x74, (int)MapList.Fireburg, Locations.Fireburg, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LevelForest01 = new TreasureObject(0x28, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest02 = new TreasureObject(0x29, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest03 = new TreasureObject(0x2A, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LevelForest04 = new TreasureObject(0x2B, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest05 = new TreasureObject(0x2C, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest01 = new TreasureObject(0x15, (int)MapList.LevelAliveForest, Locations.AliveForest, TreasureType.Chest, "Alive Forest Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest02 = new TreasureObject(0xA5, (int)MapList.LevelAliveForest, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest03 = new TreasureObject(0xA6, (int)MapList.LevelAliveForest, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest04 = new TreasureObject(0xA7, (int)MapList.LevelAliveForest, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject WintryCave01 = new TreasureObject(0x09, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Chest, "Squid Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave02 = new TreasureObject(0x43, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject WintryCave03 = new TreasureObject(0x44, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject WintryCave04 = new TreasureObject(0x45, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject WintryCave05 = new TreasureObject(0x46, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject WintryCave06 = new TreasureObject(0x47, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave07 = new TreasureObject(0x48, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave08 = new TreasureObject(0x49, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave09 = new TreasureObject(0x4A, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave10 = new TreasureObject(0x4B, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject WintryCave11 = new TreasureObject(0x4C, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject WintryCave12 = new TreasureObject(0x4D, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineCliff01 = new TreasureObject(0x79, (int)MapList.VolcanoTop, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff02 = new TreasureObject(0x7A, (int)MapList.VolcanoTop, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff03 = new TreasureObject(0x7B, (int)MapList.VolcanoTop, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff04 = new TreasureObject(0x7C, (int)MapList.VolcanoTop, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject VolcanoTop01 = new TreasureObject(0x12, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Chest, "Medusa Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop02 = new TreasureObject(0x82, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop03 = new TreasureObject(0x83, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop04 = new TreasureObject(0x84, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop05 = new TreasureObject(0x85, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop06 = new TreasureObject(0x86, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop07 = new TreasureObject(0x87, (int)MapList.VolcanoTop, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase01 = new TreasureObject(0x11, (int)MapList.VolcanoBase, Locations.Volcano, TreasureType.Chest, "Volcano Base Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase02 = new TreasureObject(0x7F, (int)MapList.VolcanoBase, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase03 = new TreasureObject(0x80, (int)MapList.VolcanoBase, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase04 = new TreasureObject(0x81, (int)MapList.VolcanoBase, Locations.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject RopeBridge01 = new TreasureObject(0xA3, (int)MapList.RopeBridge, Locations.RopeBridge, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject RopeBridge02 = new TreasureObject(0xA4, (int)MapList.RopeBridge, Locations.RopeBridge, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree01 = new TreasureObject(0xA8, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree02 = new TreasureObject(0xA9, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree03 = new TreasureObject(0xAA, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree04 = new TreasureObject(0xAB, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree05 = new TreasureObject(0x16, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Chest, "Gidrah Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Gidrah's chest from getting Sand/River coins
		public static TreasureObject GiantTree06 = new TreasureObject(0xAC, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject GiantTree07 = new TreasureObject(0xAD, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject GiantTree08 = new TreasureObject(0xAE, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree09 = new TreasureObject(0xB5, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree10 = new TreasureObject(0xB6, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree11 = new TreasureObject(0xBB, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree12 = new TreasureObject(0xBC, (int)MapList.GiantTreeA, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree13 = new TreasureObject(0xAF, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe } });
		public static TreasureObject GiantTree14 = new TreasureObject(0xB0, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe } });
		public static TreasureObject GiantTree15 = new TreasureObject(0xB1, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree16 = new TreasureObject(0xB2, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree17 = new TreasureObject(0xB3, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree18 = new TreasureObject(0xB4, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree19 = new TreasureObject(0xB7, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree20 = new TreasureObject(0xB8, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree21 = new TreasureObject(0xB9, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree22 = new TreasureObject(0xBA, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree23 = new TreasureObject(0xBD, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree24 = new TreasureObject(0xBE, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree25 = new TreasureObject(0xBF, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree26 = new TreasureObject(0xC0, (int)MapList.GiantTreeB, Locations.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject MountGale01 = new TreasureObject(0x17, (int)MapList.MountGale, Locations.MountGale, TreasureType.Chest, "Dullahan Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Dulahan's chest from getting Sand/River coins
		public static TreasureObject MountGale02 = new TreasureObject(0xC3, (int)MapList.MountGale, Locations.MountGale, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MountGale03 = new TreasureObject(0xC4, (int)MapList.MountGale, Locations.MountGale, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip01 = new TreasureObject(0xD9, (int)MapList.MacShipDeck, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip02 = new TreasureObject(0xDA, (int)MapList.MacShipDeck, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip03 = new TreasureObject(0xDB, (int)MapList.MacShipDeck, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip04 = new TreasureObject(0x1B, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Chest, "MacShip Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip05 = new TreasureObject(0xDF, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip06 = new TreasureObject(0xE0, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip07 = new TreasureObject(0xE1, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip08 = new TreasureObject(0xE2, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject MacShip09 = new TreasureObject(0xE3, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip10 = new TreasureObject(0xE4, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MacShip11 = new TreasureObject(0xE5, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MacShip12 = new TreasureObject(0xE6, (int)MapList.MacShipInterior, Locations.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon01 = new TreasureObject(0x06, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Chest, "Skull Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon02 = new TreasureObject(0x07, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Chest, "Last Room Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon03 = new TreasureObject(0x08, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Chest, "Rex Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon04 = new TreasureObject(0x35, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ElixirChest = new TreasureObject(0x04, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Chest, "Tristam's Treasure", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon05 = new TreasureObject(0x36, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon06 = new TreasureObject(0x37, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon07 = new TreasureObject(0x38, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon08 = new TreasureObject(0x39, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon09 = new TreasureObject(0x3A, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon10 = new TreasureObject(0x3B, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon11 = new TreasureObject(0x3C, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon12 = new TreasureObject(0x3D, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon13 = new TreasureObject(0x3E, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon14 = new TreasureObject(0x3F, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject IcePyramid01 = new TreasureObject(0x0C, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Chest, "Drop Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid02 = new TreasureObject(0x0D, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Chest, "Maze Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid03 = new TreasureObject(0x0E, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Chest, "Golem Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid04 = new TreasureObject(0x53, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid05 = new TreasureObject(0x54, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid06 = new TreasureObject(0x55, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid07 = new TreasureObject(0x5C, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid08 = new TreasureObject(0x5D, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid09 = new TreasureObject(0x5E, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid10 = new TreasureObject(0x5F, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid11 = new TreasureObject(0x60, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid12 = new TreasureObject(0x61, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid13 = new TreasureObject(0x62, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid14 = new TreasureObject(0x63, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid15 = new TreasureObject(0x64, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid16 = new TreasureObject(0x65, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid17 = new TreasureObject(0x66, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid18 = new TreasureObject(0x67, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid19 = new TreasureObject(0x68, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid20 = new TreasureObject(0x69, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject IcePyramid21 = new TreasureObject(0x6A, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid22 = new TreasureObject(0x6B, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid23 = new TreasureObject(0x6C, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid24 = new TreasureObject(0x6D, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject IcePyramid25 = new TreasureObject(0x6E, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid28 = new TreasureObject(0x0B, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Chest, "Diamond Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid29 = new TreasureObject(0x50, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid30 = new TreasureObject(0x51, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid31 = new TreasureObject(0x52, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid32 = new TreasureObject(0x56, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid33 = new TreasureObject(0x57, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid34 = new TreasureObject(0x58, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid35 = new TreasureObject(0x59, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid36 = new TreasureObject(0x5A, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid37 = new TreasureObject(0x5B, (int)MapList.IcePyramidB, Locations.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject LavaDome01 = new TreasureObject(0x88, (int)MapList.LavaDomeExterior, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome02 = new TreasureObject(0x89, (int)MapList.LavaDomeExterior, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome03 = new TreasureObject(0x8A, (int)MapList.LavaDomeExterior, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome04 = new TreasureObject(0x8B, (int)MapList.LavaDomeExterior, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome05 = new TreasureObject(0x13, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Chest, "Left Path Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome06 = new TreasureObject(0x14, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Chest, "Hydra Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome07 = new TreasureObject(0x1C, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Chest, "Right Path Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome08 = new TreasureObject(0x91, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome09 = new TreasureObject(0x92, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome10 = new TreasureObject(0x93, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome11 = new TreasureObject(0x94, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome12 = new TreasureObject(0x95, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome13 = new TreasureObject(0x96, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome14 = new TreasureObject(0x97, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome15 = new TreasureObject(0x98, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome16 = new TreasureObject(0x99, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome17 = new TreasureObject(0x9A, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome18 = new TreasureObject(0x9B, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome19 = new TreasureObject(0x9C, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome20 = new TreasureObject(0x9D, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome21 = new TreasureObject(0x9E, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome22 = new TreasureObject(0x9F, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome23 = new TreasureObject(0xA0, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject LavaDome24 = new TreasureObject(0xA1, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Unacessible boxes? See mapbojects 0x41
		//public static TreasureObject LavaDome25 = new TreasureObject(0xA2, (int)MapList.LavaDomeInteriorA, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome26 = new TreasureObject(0x8C, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome27 = new TreasureObject(0x8D, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome28 = new TreasureObject(0x8E, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome29 = new TreasureObject(0x8F, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome30 = new TreasureObject(0x90, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome31 = new TreasureObject(0xF6, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome32 = new TreasureObject(0xF7, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome33 = new TreasureObject(0xF8, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome34 = new TreasureObject(0xF9, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome35 = new TreasureObject(0xFA, (int)MapList.LavaDomeInteriorB, Locations.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject PazuzuTower01 = new TreasureObject(0x18, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Chest, "Square Room Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower02 = new TreasureObject(0x19, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Chest, "Corridor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower03 = new TreasureObject(0xD0, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PazuzuTower04 = new TreasureObject(0xD1, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PazuzuTower05 = new TreasureObject(0xD2, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower06 = new TreasureObject(0xD3, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower07 = new TreasureObject(0xD4, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower08 = new TreasureObject(0xD5, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower09 = new TreasureObject(0xD6, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower10 = new TreasureObject(0xD7, (int)MapList.PazuzuTowerA, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower11 = new TreasureObject(0x1A, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Chest, "Pazuzu Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb, AccessReqs.Axe, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Pazuzu's chest from getting Sand/River coins
		public static TreasureObject PazuzuTower12 = new TreasureObject(0xCA, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower13 = new TreasureObject(0xCB, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower14 = new TreasureObject(0xCC, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower15 = new TreasureObject(0xCD, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower16 = new TreasureObject(0xCE, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower17 = new TreasureObject(0xCF, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject WindholeTemple01 = new TreasureObject(0xC2, (int)MapList.SpencerCave, Locations.WindholeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); 
		public static TreasureObject SpencerCave01 = new TreasureObject(0x6F, (int)MapList.SpencerCave, Locations.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.WakeWater } });
		public static TreasureObject FallBasin01 = new TreasureObject(0x0A, (int)MapList.FallBasin, Locations.FallsBasin, TreasureType.Chest, "Crab Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FallBasin02 = new TreasureObject(0x4F, (int)MapList.FallBasin, Locations.FallsBasin, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Houses01 = new TreasureObject(0x41, (int)MapList.HouseInterior, Locations.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Aquaria
		public static TreasureObject Houses02 = new TreasureObject(0x42, (int)MapList.HouseInterior, Locations.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Aquaria
		public static TreasureObject Houses03 = new TreasureObject(0x75, (int)MapList.HouseInterior, Locations.Fireburg, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Reuben House (Fireburg)
		public static TreasureObject Houses04 = new TreasureObject(0xC5, (int)MapList.HouseInterior, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia
		public static TreasureObject Houses05 = new TreasureObject(0xC6, (int)MapList.HouseInterior, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia Inn
		public static TreasureObject Houses06 = new TreasureObject(0xC7, (int)MapList.HouseInterior, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia Inn
		public static TreasureObject Houses07 = new TreasureObject(0xC8, (int)MapList.HouseInterior, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia 
		public static TreasureObject Houses08 = new TreasureObject(0xC9, (int)MapList.HouseInterior, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia
		public static TreasureObject Caves01 = new TreasureObject(0x0F, (int)MapList.Caves, Locations.SpencersPlace, TreasureType.Chest, "Mobius Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MegaGrenade, AccessReqs.ThunderRock, AccessReqs.SunCoin, AccessReqs.LibraCrest } });
		//public static TreasureObject Caves02 = new TreasureObject(0x34, (int)MapList.Caves, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Unaccessible?
		public static TreasureObject Caves03 = new TreasureObject(0x40, (int)MapList.Caves, Locations.LibraTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves04 = new TreasureObject(0x4E, (int)MapList.Caves, Locations.LifeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Libra Crest
		public static TreasureObject Caves05 = new TreasureObject(0x70, (int)MapList.Caves, Locations.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.GeminiCrest } }); // Wintry Temple Entrance
		public static TreasureObject Caves06 = new TreasureObject(0x71, (int)MapList.Caves, Locations.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.GeminiCrest } }); // Wintry Temple Entrance
		//public static TreasureObject Caves07 = new TreasureObject(0x72, (int)MapList.Caves, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // in Wintry Temple, but not there
		//public static TreasureObject Caves08 = new TreasureObject(0x73, (int)MapList.Caves, Locations.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves09 = new TreasureObject(0x7E, (int)MapList.Caves, Locations.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves10 = new TreasureObject(0x7D, (int)MapList.Caves, Locations.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves11 = new TreasureObject(0xC1, (int)MapList.Caves, Locations.KaidgeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject Caves12 = new TreasureObject(0xD8, (int)MapList.Caves, Locations.KaidgeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MobiusCrest } }); // Light Temple single chest
		public static TreasureObject ForestaHouse01 = new TreasureObject(0x05, (int)MapList.ForestaInterior, Locations.Foresta, TreasureType.Chest, "House Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ForestaHouse02 = new TreasureObject(0x2E, (int)MapList.ForestaInterior, Locations.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ForestaHouse03 = new TreasureObject(0x2F, (int)MapList.ForestaInterior, Locations.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Tree01 = new TreasureObject(0x30, (int)MapList.ForestaInterior, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe, AccessReqs.MobiusCrest } });
		public static TreasureObject Tree02 = new TreasureObject(0x31, (int)MapList.ForestaInterior, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe, AccessReqs.MobiusCrest } });
		public static TreasureObject Tree03 = new TreasureObject(0x32, (int)MapList.ForestaInterior, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe, AccessReqs.LibraCrest } });
		public static TreasureObject Tree04 = new TreasureObject(0x33, (int)MapList.ForestaInterior, Locations.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe, AccessReqs.GeminiCrest } });
		public static TreasureObject MineInterior01 = new TreasureObject(0x10, (int)MapList.FocusTowerBase, Locations.Mine, TreasureType.Chest, "Mine Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior02 = new TreasureObject(0x76, (int)MapList.FocusTowerBase, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior03 = new TreasureObject(0x77, (int)MapList.FocusTowerBase, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior04 = new TreasureObject(0x78, (int)MapList.FocusTowerBase, Locations.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		//public static TreasureObject FocusTower01 = new TreasureObject(0x20, (int)MapList.FocusTowerBase, Locations.FocusTowerWest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle01 = new TreasureObject(0x01, (int)MapList.FocusTowerBase, Locations.DoomCastle, TreasureType.Chest, "Sand Floor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject DoomCastle02 = new TreasureObject(0x1E, (int)MapList.FocusTowerBase, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject DoomCastle03 = new TreasureObject(0x1F, (int)MapList.FocusTowerBase, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		// public static TreasureObject FocusTower03 = new TreasureObject(0x1F, (int)MapList.FocusTowerBase, Locations.FocusTowerWest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } }); Wind Chest??
		public static TreasureObject FocusTower05 = new TreasureObject(0x00, (int)MapList.FocusTowerBase, Locations.FocusTowerSouth, TreasureType.Chest, "SunCoin Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SunCoin } });
		public static TreasureObject FocusTower01 = new TreasureObject(0x21, (int)MapList.FocusTower, Locations.FocusTowerSouth, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower02 = new TreasureObject(0x22, (int)MapList.FocusTower, Locations.FocusTowerEast, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower03 = new TreasureObject(0x02, (int)MapList.FocusTower, Locations.FocusTowerNorth, TreasureType.Chest, "Backdoor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower04 = new TreasureObject(0x03, (int)MapList.FocusTower, Locations.FocusTowerSouth, TreasureType.Chest, "SandCoin Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SandCoin } });
		public static TreasureObject DoomCastle04 = new TreasureObject(0xE7, (int)MapList.DoomCastleIce, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle06 = new TreasureObject(0xE8, (int)MapList.DoomCastleIce, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle07 = new TreasureObject(0xE9, (int)MapList.DoomCastleIce, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle08 = new TreasureObject(0xEA, (int)MapList.DoomCastleIce, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle09 = new TreasureObject(0xEB, (int)MapList.DoomCastleLava, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle10 = new TreasureObject(0xEC, (int)MapList.DoomCastleLava, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle11 = new TreasureObject(0xED, (int)MapList.DoomCastleLava, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle12 = new TreasureObject(0xEE, (int)MapList.DoomCastleLava, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle13 = new TreasureObject(0xEF, (int)MapList.DoomCastleSky, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle14 = new TreasureObject(0xF0, (int)MapList.DoomCastleSky, Locations.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle15 = new TreasureObject(0xF2, (int)MapList.DoomCastleHero, Locations.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle16 = new TreasureObject(0xF3, (int)MapList.DoomCastleHero, Locations.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle17 = new TreasureObject(0xF4, (int)MapList.DoomCastleHero, Locations.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle18 = new TreasureObject(0xF5, (int)MapList.DoomCastleHero, Locations.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject ForestaBattlefieldReward = new TreasureObject(0x01, (int)MapList.Overworld, Locations.ForestaWestBattlefield, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject AquariaBattlefieldReward = new TreasureObject(0x02, (int)MapList.Overworld, Locations.AquariaBattlefield03, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LibraBattlefieldReward = new TreasureObject(0x03, (int)MapList.Overworld, Locations.LibraBattlefield01, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FireburgBattlefieldReward = new TreasureObject(0x04, (int)MapList.Overworld, Locations.FireburgBattlefield02, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MineBattlefieldReward = new TreasureObject(0x05, (int)MapList.Overworld, Locations.MineBattlefield02, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject OldManForesta = new TreasureObject((int)ItemGivingNPCs.BoulderOldMan, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject KaeliForesta = new TreasureObject((int)ItemGivingNPCs.KaeliForesta, (int)MapList.LevelAliveForest, Locations.LevelForest, TreasureType.NPC, "Kaeli", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.TreeWither } });
		public static TreasureObject TristamBoneDungeon = new TreasureObject((int)ItemGivingNPCs.TristamBoneDungeonBomb, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.NPC, "Tristam", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject TristamBoneDungeonExlixir = new TreasureObject((int)ItemGivingNPCs.TristamBoneDungeonElixir, (int)MapList.BoneDungeon, Locations.BoneDungeon, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject SellerAquaria = new TreasureObject((int)ItemGivingNPCs.WomanAquaria, (int)MapList.Aquaria, Locations.Aquaria, TreasureType.NPC, "Aquaria Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PhoebeWintryCave = new TreasureObject((int)ItemGivingNPCs.PhoebeWintryCave, (int)MapList.WintryCave, Locations.WintryCave, TreasureType.NPC, "Phoebe", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MysteriousManLifeTemple = new TreasureObject((int)ItemGivingNPCs.MysteriousManLifeTemple, (int)MapList.Caves, Locations.LibraTemple, TreasureType.NPC, "Mysterious Man", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.LibraCrest } });
		//public static TreasureObject PhoebeFallBasin = new TreasureObject((int)ItemGivingNPCs.PhoebeFallBasin, (int)MapList.FallBasin, Locations.FallsBasin, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject IceGolem = new TreasureObject(0x07, (int)MapList.IcePyramidA, Locations.IcePyramid, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject Spencer = new TreasureObject((int)ItemGivingNPCs.Spencer, (int)MapList.SpencerCave, Locations.Aquaria, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WakeWater } });
		public static TreasureObject VenusChest = new TreasureObject((int)ItemGivingNPCs.VenusChest, (int)MapList.FocusTowerBase, Locations.FocusTowerNorth, TreasureType.NPC, "Venus Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.VenusKey, AccessReqs.Bomb } });
		public static TreasureObject TristamFireburg = new TreasureObject((int)ItemGivingNPCs.TristamFireburg, (int)MapList.Fireburg, Locations.Fireburg, TreasureType.NPC, "Tristam", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject SellerFireburg = new TreasureObject((int)ItemGivingNPCs.WomanFireburg, (int)MapList.Fireburg, Locations.Fireburg, TreasureType.NPC, "Fireburg Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ReubenMine = new TreasureObject((int)ItemGivingNPCs.PhoebeFallBasin, (int)MapList.VolcanoTop, Locations.Mine, TreasureType.NPC, "Reuben", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MegaGrenadeDude = new TreasureObject((int)ItemGivingNPCs.MegaGrenadeDude, (int)MapList.Fireburg, Locations.Fireburg, TreasureType.NPC, "MegaGrenade Dude", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MultiKey, AccessReqs.Claw } });
		//public static TreasureObject GiantTree = new TreasureObject(0x0C, (int)MapList.LevelAliveForest, Locations.AliveForest, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject TristamSpencerPlace = new TreasureObject((int)ItemGivingNPCs.TristamSpencersPlace, (int)MapList.SpencerCave, Locations.Aquaria, TreasureType.NPC, "Tristam's Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.VenusKey, AccessReqs.WakeWater } });
		//public static TreasureObject Pazuzu = new TreasureObject(0x0E, (int)MapList.PazuzuTowerB, Locations.PazuzusTower, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject Arion = new TreasureObject((int)ItemGivingNPCs.ArionFireburg, (int)MapList.Fireburg, Locations.Fireburg, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MegaGrenade } });
		public static TreasureObject KaeliWindia = new TreasureObject((int)ItemGivingNPCs.KaeliWindia, (int)MapList.Windia, Locations.Windia, TreasureType.NPC, "Kaeli", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Elixir, AccessReqs.TreeWither } });
		public static TreasureObject SellerWindia = new TreasureObject((int)ItemGivingNPCs.GirlWindia, (int)MapList.Windia, Locations.Windia, TreasureType.NPC, "Windia Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject Kaeli02 = new TreasureObject(0x04, (int)MapList.Windia, Locations.Windia, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.ThunderRock, AccessReqs.MegaGrenade } });

		public static List<TreasureObject> AllChests()
		{
			List<TreasureObject> properties = new();
			ItemLocations instance = new();

			foreach (PropertyInfo prop in typeof(ItemLocations).GetProperties())
			{
				properties.Add(new TreasureObject((TreasureObject)prop.GetValue(instance)));
			}

			return properties.Where(x => x.Type == TreasureType.Chest || x.Type == TreasureType.Box).ToList();
		}
		public static List<TreasureObject> Generate(Flags flags, Battlefields battlefields, NodeLocations nodeslocations)
		{
			List<TreasureObject> properties = new();
			ItemLocations instance = new();

			foreach (FieldInfo prop in typeof(ItemLocations).GetFields())
			{
				if (prop.FieldType == typeof(TreasureObject))
				{
					properties.Add(new TreasureObject((TreasureObject)prop.GetValue(instance), prop.Name));
				}
			}

			// Update Battlefields Locations
			if (flags.ShuffleBattlefieldRewards)
			{
				var battlefieldsLocation = properties.Where(x => x.Type == TreasureType.Battlefield).ToList();

				for (int i = 0; i < battlefieldsLocation.Count(); i++)
				{
					battlefieldsLocation[i].Location = battlefields.BattlefieldsWithItem[i];
				}
			}

			List<(Locations location, List<List<AccessReqs>> accessReqs)> globalAccessReqs = nodeslocations.Nodes.Select(x => (x.Location, x.AccessRequirements)).ToList();
			//List<(Locations, List<List<AccessReqs>>)> friendlyAccessReqs = new();

			if (flags.LogicOptions == LogicOptions.Friendly)
			{

				for(int i = 0; i < globalAccessReqs.Count; i++)
				{
					for (int j = 0; j < globalAccessReqs[i].accessReqs.Count; j++)
					{
						globalAccessReqs[i].accessReqs[j].AddRange(properties.Where(x => x.Location == globalAccessReqs[i].location).SelectMany(x => x.AccessRequirements[0]).Distinct().ToList());
					}
				}
				
				
				/*
				foreach (var location in nodeslocations.Nodes.Select(x => x.Key).ToList())
				{
					List<List<AccessReqs>> globalAccessReqs = properties.Where(x => x.Location == location).SelectMany(x => x.AccessRequirements).Distinct().ToList();
					friendlyAccessReqs.Add((location, globalAccessReqs));
				}*/

				globalAccessReqs.Find(x => x.location == Locations.LevelForest).accessReqs.ForEach(x => x.Remove(AccessReqs.Axe));
				globalAccessReqs.Find(x => x.location == Locations.LevelForest).accessReqs.ForEach(x => x.Remove(AccessReqs.TreeWither));

				globalAccessReqs.Find(x => x.location == Locations.Foresta).accessReqs.ForEach(x => x.Remove(AccessReqs.Axe));
				globalAccessReqs.Find(x => x.location == Locations.Windia).accessReqs.ForEach(x => x.Remove(AccessReqs.Elixir));

				globalAccessReqs.Find(x => x.location == Locations.FocusTowerNorth).accessReqs.Clear();
				globalAccessReqs.Find(x => x.location == Locations.FocusTowerEast).accessReqs.Clear();
				globalAccessReqs.Find(x => x.location == Locations.FocusTowerWest).accessReqs.Clear();
				globalAccessReqs.Find(x => x.location == Locations.FocusTowerSouth).accessReqs.Clear();
				globalAccessReqs.Find(x => x.location == Locations.FocusTowerSouth2).accessReqs.Clear();

				globalAccessReqs.Find(x => x.location == Locations.Mine).accessReqs.ForEach(x => x.Add(AccessReqs.MegaGrenade));
				globalAccessReqs.Find(x => x.location == Locations.MacsShip).accessReqs.ForEach(x => x.Add(AccessReqs.CaptainCap));
				globalAccessReqs.Find(x => x.location == Locations.MacsShip).accessReqs.ForEach(x => x.Add(AccessReqs.CaptainCap));

				//friendlyAccessReqs.Find(x => x.Item1 == Locations.IcePyramid).Item2.Add(AccessReqs.MagicMirror);
				//friendlyAccessReqs.Find(x => x.Item1 == Locations.Volcano).Item2.Add(AccessReqs.Mask);
			}

			// Collapse Access Requirements
			for (int i = 0; i < properties.Count; i++)
			{
				var locRequirements = properties[i].AccessRequirements[0].ToList();
				properties[i].AccessRequirements.Clear();

				foreach (var accessreqlist in globalAccessReqs.Find(x => x.location == properties[i].Location).accessReqs)
				{
					properties[i].AccessRequirements.Add(locRequirements.Concat(accessreqlist).ToList());
				}

				if (flags.ProgressiveGear)
				{
					for (int j = 0; j < properties[i].AccessRequirements.Count; j++)
					{ 
						if (properties[i].AccessRequirements[j].Contains(AccessReqs.DragonClaw))
						{
							properties[i].AccessRequirements[j].AddRange(new List<AccessReqs> { AccessReqs.CatClaw, AccessReqs.CharmClaw });
						}

						if (properties[i].AccessRequirements[j].Contains(AccessReqs.MegaGrenade))
						{
							properties[i].AccessRequirements[j].AddRange(new List<AccessReqs> { AccessReqs.SmallBomb, AccessReqs.JumboBomb });
						}
					}
				}
			}

			// Update teleporters logic
			foreach (var teleporterlink in LinkedTeleporters)
			{
				var teleporter1subregion = nodeslocations.Nodes.Find(x => x.Location == TeleporterLocations.Find(t => t.Item1 == teleporterlink.Item1).Item2).SubRegion;
				var teleporter2subregion = nodeslocations.Nodes.Find(x => x.Location == TeleporterLocations.Find(t => t.Item1 == teleporterlink.Item2).Item2).SubRegion;

				if (teleporter1subregion != teleporter2subregion)
				{
					var teleporter1access = SubRegionsAccess.Find(x => x.Item1 == teleporter1subregion).Item2;
					var teleporter2access = SubRegionsAccess.Find(x => x.Item1 == teleporter2subregion).Item2;

					teleporter1access.ForEach(x => x.AddRange(TeleporterAccess.Find(t => t.Item1 == teleporterlink.Item1).Item2));
					teleporter2access.ForEach(x => x.AddRange(TeleporterAccess.Find(t => t.Item1 == teleporterlink.Item2).Item2));

					nodeslocations.Nodes.Where(x => x.SubRegion == teleporter1subregion).ToList().ForEach(x => x.AccessRequirements.AddRange(teleporter2access));
					nodeslocations.Nodes.Where(x => x.SubRegion == teleporter2subregion).ToList().ForEach(x => x.AccessRequirements.AddRange(teleporter1access));
				}
			}

			// Expert Mode check
			if (flags.LogicOptions != LogicOptions.Expert)
			{
				for (int i = 0; i < properties.Count; i++)
				{
					properties[i].AccessRequirements = properties[i].AccessRequirements.Where((x, i) => i == 0).ToList();
				}
			}

			// Set Priorization
			if (flags.ChestsShuffle == ItemShuffleChests.Prioritize)
			{
				properties.Where(x => x.Type == TreasureType.Chest && x.ObjectId < 0x20).ToList().ForEach(x => x.Prioritize = true);
			}

			if (flags.NpcsShuffle == ItemShuffleNPCsBattlefields.Prioritize)
			{
				properties.Where(x => x.Type == TreasureType.NPC).ToList().ForEach(x => x.Prioritize = true);
			}
			else if (flags.NpcsShuffle == ItemShuffleNPCsBattlefields.Exclude)
			{
				properties.Where(x => x.Type == TreasureType.NPC).ToList().ForEach(x => x.Exclude = true);
			}

			if (flags.BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Prioritize)
			{
				properties.Where(x => x.Type == TreasureType.Battlefield).ToList().ForEach(x => x.Prioritize = true);
			}
			else if (flags.BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Exclude)
			{
				properties.Where(x => x.Type == TreasureType.Battlefield).ToList().ForEach(x => x.Exclude = true);
			}

			if (flags.BoxesShuffle == ItemShuffleBoxes.Exclude)
			{
				properties.Where(x => x.Type == TreasureType.Box).ToList().ForEach(x => x.Exclude = true);
			}

			properties.Where(x => x.Type == TreasureType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList().ForEach(x => x.Exclude = true);

			return properties.ToList();
		}
		public static List<TreasureObject> FinalChests()
		{
			List<TreasureObject> properties = new();
			ItemLocations instance = new();

			foreach (FieldInfo prop in typeof(ItemLocations).GetFields())
			{
				if (prop.FieldType == typeof(TreasureObject))
				{
					properties.Add(new TreasureObject((TreasureObject)prop.GetValue(instance)));
				}

			}

			return properties.Where(x => x.Type == TreasureType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList();
		}

		public static List<TreasureObject> AllNPCsItems()
		{
			List<TreasureObject> properties = new();
			ItemLocations instance = new();

			foreach (PropertyInfo prop in typeof(ItemLocations).GetProperties())
			{
				properties.Add((TreasureObject)prop.GetValue(instance));
			}

			//List<(TreasureObject, Items)> npcitemList = properties.Where(x => x.Type == TreasureType.NPC).Select(x => (x, Items.None)).ToList();
			properties[0].Content = Items.TreeWither;
			properties[1].Content = Items.Axe;
			properties[2].Content = Items.Bomb;
			properties[3].Content = Items.Elixir;
			properties[4].Content = Items.CatClaw;
			properties[5].Content = Items.WakeWater;
			properties[6].Content = Items.JumboBomb;
			properties[7].Content = Items.VenusKey;
			properties[8].Content = Items.VenusShield;
			properties[9].Content = Items.BlizzardBook;
			properties[10].Content = Items.MultiKey;
			properties[11].Content = Items.MegaGrenade;
			properties[12].Content = Items.DragonClaw;
			properties[13].Content = Items.ThunderRock;
			properties[14].Content = Items.CaptainCap;

			return properties;
		}

		public static Dictionary<Locations, List<AccessReqs>> LocationAccessReq => new Dictionary<Locations, List<AccessReqs>>
		{
			{ Locations.ForestaSouthBattlefield, new List<AccessReqs> { } },
			{ Locations.ForestaWestBattlefield, new List<AccessReqs> { } },
			{ Locations.ForestaEastBattlefield, new List<AccessReqs> { } },
			{ Locations.AquariaBattlefield01, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.AquariaBattlefield02, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.AquariaBattlefield03, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.WintryBattlefield01, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.WintryBattlefield02, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.PyramidBattlefield01, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.LibraBattlefield01, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater } },
			{ Locations.LibraBattlefield02, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater } },
			{ Locations.FireburgBattlefield01, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.FireburgBattlefield02, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.FireburgBattlefield03, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.MineBattlefield01, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.MineBattlefield02, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.MineBattlefield03, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.VolcanoBattlefield01, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.WindiaBattlefield01, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.WindiaBattlefield02, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.HillOfDestiny, new List<AccessReqs> { } },
			{ Locations.LevelForest, new List<AccessReqs> { } },
			{ Locations.Foresta, new List<AccessReqs> { } },
			{ Locations.SandTemple, new List<AccessReqs> { } },
			{ Locations.BoneDungeon, new List<AccessReqs> { } },
			{ Locations.FocusTowerSouth, new List<AccessReqs> { } },
			{ Locations.FocusTowerWest, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.LibraTemple, new List<AccessReqs> { AccessReqs.SandCoin  } },
			{ Locations.Aquaria, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.WintryCave, new List<AccessReqs> { AccessReqs.SandCoin  } },
			{ Locations.LifeTemple, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.LibraCrest } },
			{ Locations.FallsBasin, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.IcePyramid, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Locations.SpencersPlace, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater } },
			{ Locations.WintryTemple, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater } },
			{ Locations.FocusTowerNorth, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater } },
			{ Locations.FocusTowerEast, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.Fireburg, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.Mine, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.SealedTemple, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.Volcano, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.LavaDome, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Locations.FocusTowerSouth2, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.RopeBridge, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.AliveForest, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.GiantTree, new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.Axe } },
			{ Locations.KaidgeTemple, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.Windia, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.WindholeTemple, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.MountGale, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.PazuzusTower, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Locations.ShipDock, new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.MobiusCrest } },
			{ Locations.DoomCastle, new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.RiverCoin, AccessReqs.SunCoin, AccessReqs.SkyCoin, AccessReqs.MobiusCrest, AccessReqs.CaptainCap, AccessReqs.ThunderRock, AccessReqs.Sword, AccessReqs.MegaGrenade, AccessReqs.DragonClaw } }, // Maybe put to ALL
			{ Locations.LightTemple, new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.Claw, AccessReqs.MobiusCrest } },
			{ Locations.MacsShip, new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.MobiusCrest, AccessReqs.ThunderRock, AccessReqs.MegaGrenade } },
		};
		public static List<(Locations, List<AccessReqs>)> LocationTriggers => new List<(Locations, List<AccessReqs>)>
		{
			( Locations.LevelForest, new List<AccessReqs> { AccessReqs.Minotaur } ),
			( Locations.Foresta, new List<AccessReqs> { AccessReqs.Kaeli1 } ),
			( Locations.LibraTemple, new List<AccessReqs> { AccessReqs.Phoebe1 } ),
			( Locations.Aquaria, new List<AccessReqs> { AccessReqs.AquariaPlaza } ),
			( Locations.Fireburg, new List<AccessReqs> { AccessReqs.Tristam2 } ),
			( Locations.Fireburg, new List<AccessReqs> { AccessReqs.Reuben1 } ),
			( Locations.Mine, new List<AccessReqs> { AccessReqs.MineCliff } ),
			( Locations.LavaDome, new List<AccessReqs> { AccessReqs.DualheadHydra } ),
			( Locations.GiantTree, new List<AccessReqs> { AccessReqs.Gidrah } ),
			( Locations.Windia, new List<AccessReqs> { AccessReqs.Otto } ),
			( Locations.SpencersPlace, new List<AccessReqs> { AccessReqs.SpencerCaveTrigger } ),
			( Locations.MacsShip, new List<AccessReqs> { AccessReqs.ShipSteeringWheel, AccessReqs.CaptainMac } ),
		};
		public static Dictionary<AccessReqs, List<AccessReqs>> AccessEvents => new Dictionary<AccessReqs, List<AccessReqs>>
		{
			{ AccessReqs.SummerAquaria, new List<AccessReqs> { AccessReqs.WakeWater, AccessReqs.AquariaPlaza } },
			{ AccessReqs.RainbowBridge, new List<AccessReqs> { AccessReqs.Otto, AccessReqs.ThunderRock } },
			{ AccessReqs.ShipLiberated, new List<AccessReqs> { AccessReqs.MegaGrenade, AccessReqs.SpencerCaveTrigger } },
			{ AccessReqs.ShipLoaned, new List<AccessReqs> { AccessReqs.CaptainCap, AccessReqs.CaptainMac } },
		};
		public static MapRegions ReturnRegion(Locations location)
		{
			return Regions.Find(x => x.Item2 == location).Item1;
		}
		public static List<(MapRegions, Locations)> Regions => new()
		{
			(MapRegions.Foresta, Locations.ForestaSouthBattlefield),
			(MapRegions.Foresta, Locations.ForestaWestBattlefield),
			(MapRegions.Foresta, Locations.ForestaEastBattlefield),
			(MapRegions.Aquaria, Locations.AquariaBattlefield01),
			(MapRegions.Aquaria, Locations.AquariaBattlefield02),
			(MapRegions.Aquaria, Locations.AquariaBattlefield03),
			(MapRegions.Aquaria, Locations.WintryBattlefield01),
			(MapRegions.Aquaria, Locations.WintryBattlefield02),
			(MapRegions.Aquaria, Locations.PyramidBattlefield01),
			(MapRegions.Aquaria, Locations.LibraBattlefield01),
			(MapRegions.Aquaria, Locations.LibraBattlefield02),
			(MapRegions.Fireburg, Locations.FireburgBattlefield01),
			(MapRegions.Fireburg, Locations.FireburgBattlefield02),
			(MapRegions.Fireburg, Locations.FireburgBattlefield03),
			(MapRegions.Fireburg, Locations.MineBattlefield01),
			(MapRegions.Fireburg, Locations.MineBattlefield02),
			(MapRegions.Fireburg, Locations.MineBattlefield03),
			(MapRegions.Fireburg, Locations.VolcanoBattlefield01),
			(MapRegions.Windia, Locations.WindiaBattlefield01),
			(MapRegions.Windia, Locations.WindiaBattlefield02),
			(MapRegions.Foresta, Locations.HillOfDestiny),
			(MapRegions.Foresta, Locations.LevelForest),
			(MapRegions.Foresta, Locations.Foresta),
			(MapRegions.Foresta, Locations.SandTemple),
			(MapRegions.Foresta, Locations.BoneDungeon),
			(MapRegions.Foresta, Locations.FocusTowerSouth),
			(MapRegions.Aquaria, Locations.FocusTowerWest),
			(MapRegions.Aquaria, Locations.LibraTemple),
			(MapRegions.Aquaria, Locations.Aquaria),
			(MapRegions.Aquaria, Locations.WintryCave),
			(MapRegions.Aquaria, Locations.LifeTemple),
			(MapRegions.Aquaria, Locations.FallsBasin),
			(MapRegions.Aquaria, Locations.IcePyramid),
			(MapRegions.Aquaria, Locations.WintryTemple),
			(MapRegions.Aquaria, Locations.FocusTowerNorth),
			(MapRegions.Fireburg, Locations.FocusTowerEast),
			(MapRegions.Fireburg, Locations.Fireburg),
			(MapRegions.Fireburg, Locations.Mine),
			(MapRegions.Fireburg, Locations.SealedTemple),
			(MapRegions.Fireburg, Locations.Volcano),
			(MapRegions.Fireburg, Locations.LavaDome),
			(MapRegions.Windia, Locations.FocusTowerSouth2),
			(MapRegions.Windia, Locations.RopeBridge),
			(MapRegions.Windia, Locations.AliveForest),
			(MapRegions.Windia, Locations.GiantTree),
			(MapRegions.Windia, Locations.KaidgeTemple),
			(MapRegions.Windia, Locations.Windia),
			(MapRegions.Windia, Locations.WindholeTemple),
			(MapRegions.Windia, Locations.MountGale),
			(MapRegions.Windia, Locations.PazuzusTower),
			(MapRegions.Windia, Locations.SpencersPlace),
			(MapRegions.Windia, Locations.ShipDock),
			(MapRegions.Windia, Locations.DoomCastle),
			(MapRegions.Windia, Locations.LightTemple),
			(MapRegions.Windia, Locations.MacsShip)
		};
		public static List<(SubRegions, Locations)> MapSubRegions => new()
		{
			(SubRegions.Foresta, Locations.ForestaSouthBattlefield),
			(SubRegions.Foresta, Locations.ForestaWestBattlefield),
			(SubRegions.Foresta, Locations.ForestaEastBattlefield),
			(SubRegions.Aquaria, Locations.AquariaBattlefield01),
			(SubRegions.Aquaria, Locations.AquariaBattlefield02),
			(SubRegions.Aquaria, Locations.AquariaBattlefield03),
			(SubRegions.Aquaria, Locations.WintryBattlefield01),
			(SubRegions.Aquaria, Locations.WintryBattlefield02),
			(SubRegions.Aquaria, Locations.PyramidBattlefield01),
			(SubRegions.AquariaFrozenField, Locations.LibraBattlefield01),
			(SubRegions.AquariaFrozenField, Locations.LibraBattlefield02),
			(SubRegions.Fireburg, Locations.FireburgBattlefield01),
			(SubRegions.Fireburg, Locations.FireburgBattlefield02),
			(SubRegions.Fireburg, Locations.FireburgBattlefield03),
			(SubRegions.Fireburg, Locations.MineBattlefield01),
			(SubRegions.Fireburg, Locations.MineBattlefield02),
			(SubRegions.Fireburg, Locations.MineBattlefield03),
			(SubRegions.VolcanoBattlefield, Locations.VolcanoBattlefield01),
			(SubRegions.Windia, Locations.WindiaBattlefield01),
			(SubRegions.Windia, Locations.WindiaBattlefield02),
			(SubRegions.Foresta, Locations.HillOfDestiny),
			(SubRegions.Foresta, Locations.LevelForest),
			(SubRegions.Foresta, Locations.Foresta),
			(SubRegions.Foresta, Locations.SandTemple),
			(SubRegions.Foresta, Locations.BoneDungeon),
			(SubRegions.Foresta, Locations.FocusTowerSouth),
			(SubRegions.Aquaria, Locations.FocusTowerWest),
			(SubRegions.Aquaria, Locations.LibraTemple),
			(SubRegions.Aquaria, Locations.Aquaria),
			(SubRegions.Aquaria, Locations.WintryCave),
			(SubRegions.LifeTemple, Locations.LifeTemple),
			(SubRegions.Aquaria, Locations.FallsBasin),
			(SubRegions.Aquaria, Locations.IcePyramid),
			(SubRegions.AquariaFrozenField, Locations.WintryTemple),
			(SubRegions.AquariaFrozenField, Locations.FocusTowerNorth),
			(SubRegions.Fireburg, Locations.FocusTowerEast),
			(SubRegions.Fireburg, Locations.Fireburg),
			(SubRegions.Fireburg, Locations.Mine),
			(SubRegions.Fireburg, Locations.SealedTemple),
			(SubRegions.Fireburg, Locations.Volcano),
			(SubRegions.Fireburg, Locations.LavaDome),
			(SubRegions.Windia, Locations.FocusTowerSouth2),
			(SubRegions.Windia, Locations.RopeBridge),
			(SubRegions.Windia, Locations.AliveForest),
			(SubRegions.Windia, Locations.GiantTree),
			(SubRegions.Windia, Locations.KaidgeTemple),
			(SubRegions.Windia, Locations.Windia),
			(SubRegions.Windia, Locations.WindholeTemple),
			(SubRegions.Windia, Locations.MountGale),
			(SubRegions.Windia, Locations.PazuzusTower),
			(SubRegions.Windia, Locations.SpencersPlace),
			(SubRegions.ShipDock, Locations.ShipDock),
			(SubRegions.DoomCastle, Locations.DoomCastle),
			(SubRegions.LightTemple, Locations.LightTemple),
			(SubRegions.ShipDock, Locations.MacsShip)
		};
		public static List<(AccessReqs, Locations)> TeleporterLocations => new()
		{
			(AccessReqs.LibraTempleLibraTeleporter, Locations.LibraTemple),
			(AccessReqs.LifeTempleLibraTeleporter, Locations.LifeTemple),
			(AccessReqs.AquariaGeminiTeleporter, Locations.Aquaria),
			(AccessReqs.FireburgGeminiTeleporter, Locations.Fireburg),
			(AccessReqs.FireburgMobiusTeleporter, Locations.Fireburg),
			(AccessReqs.SealedTempleGeminiTeleporter, Locations.SealedTemple),
			(AccessReqs.WintryTempleGeminiTeleporter, Locations.WintryTemple),
			(AccessReqs.WindiaMobiusTeleporter, Locations.Windia),
			(AccessReqs.WindiaDockTeleporter, Locations.Windia),
			(AccessReqs.ShipDockTeleporter, Locations.ShipDock),
		};
		public static List<(AccessReqs, List<AccessReqs>)> TeleporterAccess => new()
		{
			(AccessReqs.LibraTempleLibraTeleporter, new List<AccessReqs> { AccessReqs.LibraCrest }),
			(AccessReqs.LifeTempleLibraTeleporter, new List<AccessReqs> { AccessReqs.LibraCrest }),
			(AccessReqs.AquariaGeminiTeleporter, new List<AccessReqs> { AccessReqs.GeminiCrest  }),
			(AccessReqs.FireburgGeminiTeleporter, new List<AccessReqs> { AccessReqs.GeminiCrest }),
			(AccessReqs.FireburgMobiusTeleporter, new List<AccessReqs> { AccessReqs.MobiusCrest, AccessReqs.Claw, AccessReqs.MultiKey }),
			(AccessReqs.SealedTempleGeminiTeleporter, new List<AccessReqs> { AccessReqs.GeminiCrest }),
			(AccessReqs.WintryTempleGeminiTeleporter, new List<AccessReqs> { AccessReqs.GeminiCrest, AccessReqs.Barred }),
			(AccessReqs.WindiaMobiusTeleporter, new List<AccessReqs> { AccessReqs.MobiusCrest, AccessReqs.Claw, AccessReqs.MultiKey }),
			(AccessReqs.WindiaDockTeleporter, new List<AccessReqs> { AccessReqs.MobiusCrest }),
			(AccessReqs.ShipDockTeleporter, new List<AccessReqs> { AccessReqs.MobiusCrest }),
		};
		public static List<(AccessReqs, AccessReqs)> LinkedTeleporters => new()
		{
			(AccessReqs.LibraTempleLibraTeleporter, AccessReqs.LifeTempleLibraTeleporter),
			(AccessReqs.AquariaGeminiTeleporter, AccessReqs.FireburgGeminiTeleporter),
			(AccessReqs.FireburgMobiusTeleporter, AccessReqs.WindiaMobiusTeleporter),
			(AccessReqs.SealedTempleGeminiTeleporter, AccessReqs.WintryTempleGeminiTeleporter),
			(AccessReqs.WindiaDockTeleporter, AccessReqs.ShipDockTeleporter),
		};
		public static List<(SubRegions, List<List<AccessReqs>>)> SubRegionsAccess => new()
		{
			(SubRegions.Foresta, new List<List<AccessReqs>> { new List<AccessReqs> { } }),
			(SubRegions.Aquaria, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				//new List<AccessReqs> { AccessReqs.LifeTempleLibraTeleporter, AccessReqs.LibraCrest, AccessReqs.LibraTempleLibraTeleporter }
			}),
			(SubRegions.LifeTemple, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
				//new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.LibraCrest, AccessReqs.LibraTempleLibraTeleporter, AccessReqs.LifeTempleLibraTeleporter },
			}),
			(SubRegions.AquariaFrozenField, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			}),
			(SubRegions.Fireburg, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			}),
			(SubRegions.VolcanoBattlefield, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
			}),
			(SubRegions.Windia, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			}),
			(SubRegions.LightTemple, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
			}),
			(SubRegions.ShipDock, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
			}),
			(SubRegions.DoomCastle, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
			}),
		};
		public static Dictionary<Items, List<AccessReqs>> ItemAccessReq => new Dictionary<Items, List<AccessReqs>>
		{
			{ Items.Elixir, new List<AccessReqs> { AccessReqs.Elixir } },
			{ Items.TreeWither, new List<AccessReqs> { AccessReqs.TreeWither } },
			{ Items.WakeWater, new List<AccessReqs> { AccessReqs.WakeWater } },
			{ Items.VenusKey, new List<AccessReqs> { AccessReqs.VenusKey } },
			{ Items.MultiKey, new List<AccessReqs> { AccessReqs.MultiKey } },
			{ Items.ThunderRock, new List<AccessReqs> { AccessReqs.ThunderRock } },
			{ Items.CaptainCap, new List<AccessReqs> { AccessReqs.CaptainCap } },
			{ Items.LibraCrest, new List<AccessReqs> { AccessReqs.LibraCrest } },
			{ Items.GeminiCrest, new List<AccessReqs> { AccessReqs.GeminiCrest } },
			{ Items.MobiusCrest, new List<AccessReqs> { AccessReqs.MobiusCrest } },
			{ Items.SandCoin, new List<AccessReqs> { AccessReqs.SandCoin } },
			{ Items.RiverCoin, new List<AccessReqs> { AccessReqs.RiverCoin } },
			{ Items.SunCoin, new List<AccessReqs> { AccessReqs.SunCoin } },
			{ Items.SkyCoin, new List<AccessReqs> { AccessReqs.SkyCoin } },
			{ Items.SteelSword, new List<AccessReqs> { AccessReqs.Sword } },
			{ Items.KnightSword, new List<AccessReqs> { AccessReqs.Sword } },
			{ Items.Excalibur, new List<AccessReqs> { AccessReqs.Sword } },
			{ Items.Axe, new List<AccessReqs> { AccessReqs.Axe } },
			{ Items.BattleAxe, new List<AccessReqs> { AccessReqs.Axe } },
			{ Items.GiantsAxe, new List<AccessReqs> { AccessReqs.Axe } },
			{ Items.CatClaw, new List<AccessReqs> { AccessReqs.Claw, AccessReqs.CatClaw } },
			{ Items.CharmClaw, new List<AccessReqs> { AccessReqs.Claw, AccessReqs.CharmClaw } },
			{ Items.DragonClaw, new List<AccessReqs> { AccessReqs.Claw, AccessReqs.DragonClaw } },
			{ Items.Bomb, new List<AccessReqs> { AccessReqs.Bomb, AccessReqs.SmallBomb } },
			{ Items.JumboBomb, new List<AccessReqs> { AccessReqs.Bomb, AccessReqs.JumboBomb } },
			{ Items.MegaGrenade, new List<AccessReqs> { AccessReqs.Bomb, AccessReqs.MegaGrenade } },
			{ Items.ExitBook, new List<AccessReqs> { AccessReqs.ExitBook } },
		};
	}

}
