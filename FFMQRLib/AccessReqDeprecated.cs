using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public class TreasureObject
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public LocationIds Location { get; set; }
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }
		public Items Content { get; set; }
		public TreasureType Type { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public bool Accessible { get; set; }
		public string Name { get; set; }

		public TreasureObject(int mapobjid, int mapid, LocationIds location, TreasureType type, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = "";
			SubRegion = AccessReferences.MapSubRegions.Find(x => x.Item2 == location).Item1;
			Region = AccessReferences.ReturnRegion(location);
		}

		public TreasureObject(int mapobjid, int mapid, LocationIds location, TreasureType type, string name, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = name;
			SubRegion = AccessReferences.MapSubRegions.Find(x => x.Item2 == location).Item1;
			Region = AccessReferences.ReturnRegion(location);
		}

		public TreasureObject(TreasureObject treasure)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible = treasure.Accessible;
			Name = treasure.Name;
			Region = treasure.Region;
			SubRegion = treasure.SubRegion;
		}

		public TreasureObject(TreasureObject treasure, string name)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible = treasure.Accessible;
			Region = treasure.Region;
			SubRegion = treasure.SubRegion;
			Name = treasure.Name == "" ? name : treasure.Name;
		}
	}
	public partial class ItemLocations
	{
		public static TreasureObject Foresta01 = new TreasureObject(0x2D, (int)MapList.Foresta, LocationIds.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs>{ AccessReqs.Axe } });
		public static TreasureObject Fireburg01 = new TreasureObject(0x74, (int)MapList.Fireburg, LocationIds.Fireburg, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LevelForest01 = new TreasureObject(0x28, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest02 = new TreasureObject(0x29, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest03 = new TreasureObject(0x2A, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LevelForest04 = new TreasureObject(0x2B, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject LevelForest05 = new TreasureObject(0x2C, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest01 = new TreasureObject(0x15, (int)MapList.LevelAliveForest, LocationIds.AliveForest, TreasureType.Chest, "Alive Forest Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest02 = new TreasureObject(0xA5, (int)MapList.LevelAliveForest, LocationIds.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest03 = new TreasureObject(0xA6, (int)MapList.LevelAliveForest, LocationIds.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject AliveForest04 = new TreasureObject(0xA7, (int)MapList.LevelAliveForest, LocationIds.AliveForest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject WintryCave01 = new TreasureObject(0x09, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Chest, "Squid Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave02 = new TreasureObject(0x43, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject WintryCave03 = new TreasureObject(0x44, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject WintryCave04 = new TreasureObject(0x45, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject WintryCave05 = new TreasureObject(0x46, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject WintryCave06 = new TreasureObject(0x47, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave07 = new TreasureObject(0x48, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave08 = new TreasureObject(0x49, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave09 = new TreasureObject(0x4A, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject WintryCave10 = new TreasureObject(0x4B, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject WintryCave11 = new TreasureObject(0x4C, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject WintryCave12 = new TreasureObject(0x4D, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineCliff01 = new TreasureObject(0x79, (int)MapList.VolcanoTop, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff02 = new TreasureObject(0x7A, (int)MapList.VolcanoTop, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff03 = new TreasureObject(0x7B, (int)MapList.VolcanoTop, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject MineCliff04 = new TreasureObject(0x7C, (int)MapList.VolcanoTop, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject VolcanoTop01 = new TreasureObject(0x12, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Chest, "Medusa Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop02 = new TreasureObject(0x82, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop03 = new TreasureObject(0x83, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop04 = new TreasureObject(0x84, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop05 = new TreasureObject(0x85, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop06 = new TreasureObject(0x86, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoTop07 = new TreasureObject(0x87, (int)MapList.VolcanoTop, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase01 = new TreasureObject(0x11, (int)MapList.VolcanoBase, LocationIds.Volcano, TreasureType.Chest, "Volcano Base Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase02 = new TreasureObject(0x7F, (int)MapList.VolcanoBase, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase03 = new TreasureObject(0x80, (int)MapList.VolcanoBase, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject VolcanoBase04 = new TreasureObject(0x81, (int)MapList.VolcanoBase, LocationIds.Volcano, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject RopeBridge01 = new TreasureObject(0xA3, (int)MapList.RopeBridge, LocationIds.RopeBridge, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject RopeBridge02 = new TreasureObject(0xA4, (int)MapList.RopeBridge, LocationIds.RopeBridge, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree01 = new TreasureObject(0xA8, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree02 = new TreasureObject(0xA9, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree03 = new TreasureObject(0xAA, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree04 = new TreasureObject(0xAB, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject GiantTree05 = new TreasureObject(0x16, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Chest, "Gidrah Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Gidrah's chest from getting Sand/River coins
		public static TreasureObject GiantTree06 = new TreasureObject(0xAC, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject GiantTree07 = new TreasureObject(0xAD, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject GiantTree08 = new TreasureObject(0xAE, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree09 = new TreasureObject(0xB5, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree10 = new TreasureObject(0xB6, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree11 = new TreasureObject(0xBB, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree12 = new TreasureObject(0xBC, (int)MapList.GiantTreeA, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree13 = new TreasureObject(0xAF, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe } });
		public static TreasureObject GiantTree14 = new TreasureObject(0xB0, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe } });
		public static TreasureObject GiantTree15 = new TreasureObject(0xB1, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree16 = new TreasureObject(0xB2, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree17 = new TreasureObject(0xB3, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Sword } });
		public static TreasureObject GiantTree18 = new TreasureObject(0xB4, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree19 = new TreasureObject(0xB7, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree20 = new TreasureObject(0xB8, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree21 = new TreasureObject(0xB9, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree22 = new TreasureObject(0xBA, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree23 = new TreasureObject(0xBD, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree24 = new TreasureObject(0xBE, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree25 = new TreasureObject(0xBF, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject GiantTree26 = new TreasureObject(0xC0, (int)MapList.GiantTreeB, LocationIds.GiantTree, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Axe, AccessReqs.Sword } });
		public static TreasureObject MountGale01 = new TreasureObject(0x17, (int)MapList.MountGale, LocationIds.MountGale, TreasureType.Chest, "Dullahan Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Dulahan's chest from getting Sand/River coins
		public static TreasureObject MountGale02 = new TreasureObject(0xC3, (int)MapList.MountGale, LocationIds.MountGale, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MountGale03 = new TreasureObject(0xC4, (int)MapList.MountGale, LocationIds.MountGale, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip01 = new TreasureObject(0xD9, (int)MapList.MacShipDeck, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip02 = new TreasureObject(0xDA, (int)MapList.MacShipDeck, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip03 = new TreasureObject(0xDB, (int)MapList.MacShipDeck, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip04 = new TreasureObject(0x1B, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Chest, "MacShip Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip05 = new TreasureObject(0xDF, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip06 = new TreasureObject(0xE0, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip07 = new TreasureObject(0xE1, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip08 = new TreasureObject(0xE2, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject MacShip09 = new TreasureObject(0xE3, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MacShip10 = new TreasureObject(0xE4, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MacShip11 = new TreasureObject(0xE5, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject MacShip12 = new TreasureObject(0xE6, (int)MapList.MacShipInterior, LocationIds.MacsShip, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon01 = new TreasureObject(0x06, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Chest, "Skull Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon02 = new TreasureObject(0x07, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Chest, "Last Room Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon03 = new TreasureObject(0x08, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Chest, "Rex Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon04 = new TreasureObject(0x35, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ElixirChest = new TreasureObject(0x04, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Chest, "Tristam's Treasure", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon05 = new TreasureObject(0x36, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon06 = new TreasureObject(0x37, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject BoneDungeon07 = new TreasureObject(0x38, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon08 = new TreasureObject(0x39, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon09 = new TreasureObject(0x3A, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon10 = new TreasureObject(0x3B, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon11 = new TreasureObject(0x3C, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon12 = new TreasureObject(0x3D, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon13 = new TreasureObject(0x3E, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject BoneDungeon14 = new TreasureObject(0x3F, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject IcePyramid01 = new TreasureObject(0x0C, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Chest, "Drop Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid02 = new TreasureObject(0x0D, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Chest, "Maze Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid03 = new TreasureObject(0x0E, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Chest, "Golem Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid04 = new TreasureObject(0x53, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid05 = new TreasureObject(0x54, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid06 = new TreasureObject(0x55, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid07 = new TreasureObject(0x5C, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid08 = new TreasureObject(0x5D, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid09 = new TreasureObject(0x5E, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid10 = new TreasureObject(0x5F, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid11 = new TreasureObject(0x60, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid12 = new TreasureObject(0x61, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid13 = new TreasureObject(0x62, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid14 = new TreasureObject(0x63, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid15 = new TreasureObject(0x64, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid16 = new TreasureObject(0x65, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid17 = new TreasureObject(0x66, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid18 = new TreasureObject(0x67, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid19 = new TreasureObject(0x68, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid20 = new TreasureObject(0x69, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject IcePyramid21 = new TreasureObject(0x6A, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid22 = new TreasureObject(0x6B, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid23 = new TreasureObject(0x6C, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid24 = new TreasureObject(0x6D, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject IcePyramid25 = new TreasureObject(0x6E, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid28 = new TreasureObject(0x0B, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Chest, "Diamond Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid29 = new TreasureObject(0x50, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid30 = new TreasureObject(0x51, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid31 = new TreasureObject(0x52, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject IcePyramid32 = new TreasureObject(0x56, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid33 = new TreasureObject(0x57, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid34 = new TreasureObject(0x58, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid35 = new TreasureObject(0x59, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid36 = new TreasureObject(0x5A, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject IcePyramid37 = new TreasureObject(0x5B, (int)MapList.IcePyramidB, LocationIds.IcePyramid, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword } });
		public static TreasureObject LavaDome01 = new TreasureObject(0x88, (int)MapList.LavaDomeExterior, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome02 = new TreasureObject(0x89, (int)MapList.LavaDomeExterior, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome03 = new TreasureObject(0x8A, (int)MapList.LavaDomeExterior, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome04 = new TreasureObject(0x8B, (int)MapList.LavaDomeExterior, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome05 = new TreasureObject(0x13, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Chest, "Left Path Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome06 = new TreasureObject(0x14, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Chest, "Hydra Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome07 = new TreasureObject(0x1C, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Chest, "Right Path Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome08 = new TreasureObject(0x91, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MegaGrenade } });
		public static TreasureObject LavaDome09 = new TreasureObject(0x92, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome10 = new TreasureObject(0x93, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome11 = new TreasureObject(0x94, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome12 = new TreasureObject(0x95, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject LavaDome13 = new TreasureObject(0x96, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome14 = new TreasureObject(0x97, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome15 = new TreasureObject(0x98, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome16 = new TreasureObject(0x99, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome17 = new TreasureObject(0x9A, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome18 = new TreasureObject(0x9B, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome19 = new TreasureObject(0x9C, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome20 = new TreasureObject(0x9D, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome21 = new TreasureObject(0x9E, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome22 = new TreasureObject(0x9F, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome23 = new TreasureObject(0xA0, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject LavaDome24 = new TreasureObject(0xA1, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Unacessible boxes? See mapbojects 0x41
		//public static TreasureObject LavaDome25 = new TreasureObject(0xA2, (int)MapList.LavaDomeInteriorA, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome26 = new TreasureObject(0x8C, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome27 = new TreasureObject(0x8D, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome28 = new TreasureObject(0x8E, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome29 = new TreasureObject(0x8F, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome30 = new TreasureObject(0x90, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome31 = new TreasureObject(0xF6, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome32 = new TreasureObject(0xF7, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome33 = new TreasureObject(0xF8, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome34 = new TreasureObject(0xF9, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LavaDome35 = new TreasureObject(0xFA, (int)MapList.LavaDomeInteriorB, LocationIds.LavaDome, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject PazuzuTower01 = new TreasureObject(0x18, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Chest, "Square Room Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower02 = new TreasureObject(0x19, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Chest, "Corridor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower03 = new TreasureObject(0xD0, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PazuzuTower04 = new TreasureObject(0xD1, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PazuzuTower05 = new TreasureObject(0xD2, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower06 = new TreasureObject(0xD3, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower07 = new TreasureObject(0xD4, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower08 = new TreasureObject(0xD5, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower09 = new TreasureObject(0xD6, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower10 = new TreasureObject(0xD7, (int)MapList.PazuzuTowerA, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower11 = new TreasureObject(0x1A, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Chest, "Pazuzu Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb, AccessReqs.Axe, AccessReqs.SandCoin, AccessReqs.RiverCoin } }); // Prevent Pazuzu's chest from getting Sand/River coins
		public static TreasureObject PazuzuTower12 = new TreasureObject(0xCA, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower13 = new TreasureObject(0xCB, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower14 = new TreasureObject(0xCC, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject PazuzuTower15 = new TreasureObject(0xCD, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower16 = new TreasureObject(0xCE, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject PazuzuTower17 = new TreasureObject(0xCF, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject WindholeTemple01 = new TreasureObject(0xC2, (int)MapList.SpencerCave, LocationIds.WindholeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); 
		public static TreasureObject SpencerCave01 = new TreasureObject(0x6F, (int)MapList.SpencerCave, LocationIds.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.WakeWater } });
		public static TreasureObject FallBasin01 = new TreasureObject(0x0A, (int)MapList.FallBasin, LocationIds.FallsBasin, TreasureType.Chest, "Crab Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FallBasin02 = new TreasureObject(0x4F, (int)MapList.FallBasin, LocationIds.FallsBasin, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Houses01 = new TreasureObject(0x41, (int)MapList.HouseInterior, LocationIds.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Aquaria
		public static TreasureObject Houses02 = new TreasureObject(0x42, (int)MapList.HouseInterior, LocationIds.Aquaria, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Aquaria
		public static TreasureObject Houses03 = new TreasureObject(0x75, (int)MapList.HouseInterior, LocationIds.Fireburg, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Reuben House (Fireburg)
		public static TreasureObject Houses04 = new TreasureObject(0xC5, (int)MapList.HouseInterior, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia
		public static TreasureObject Houses05 = new TreasureObject(0xC6, (int)MapList.HouseInterior, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia Inn
		public static TreasureObject Houses06 = new TreasureObject(0xC7, (int)MapList.HouseInterior, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia Inn
		public static TreasureObject Houses07 = new TreasureObject(0xC8, (int)MapList.HouseInterior, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia 
		public static TreasureObject Houses08 = new TreasureObject(0xC9, (int)MapList.HouseInterior, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Windia
		public static TreasureObject Caves01 = new TreasureObject(0x0F, (int)MapList.Caves, LocationIds.SpencersPlace, TreasureType.Chest, "Mobius Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MegaGrenade, AccessReqs.GeminiCrest, AccessReqs.RainbowBridge } });
		//public static TreasureObject Caves02 = new TreasureObject(0x34, (int)MapList.Caves, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Unaccessible?
		public static TreasureObject Caves03 = new TreasureObject(0x40, (int)MapList.Caves, LocationIds.LibraTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves04 = new TreasureObject(0x4E, (int)MapList.Caves, LocationIds.LifeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // Libra Crest
		public static TreasureObject Caves05 = new TreasureObject(0x70, (int)MapList.Caves, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WintryTempleCrestTile } }); // Wintry Temple Entrance
		public static TreasureObject Caves06 = new TreasureObject(0x71, (int)MapList.Caves, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WintryTempleCrestTile } }); // Wintry Temple Entrance
		//public static TreasureObject Caves07 = new TreasureObject(0x72, (int)MapList.Caves, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } }); // in Wintry Temple, but not there
		//public static TreasureObject Caves08 = new TreasureObject(0x73, (int)MapList.Caves, LocationIds.Windia, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves09 = new TreasureObject(0x7E, (int)MapList.Caves, LocationIds.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves10 = new TreasureObject(0x7D, (int)MapList.Caves, LocationIds.SealedTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Caves11 = new TreasureObject(0xC1, (int)MapList.Caves, LocationIds.KaidgeTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } });
		public static TreasureObject Caves12 = new TreasureObject(0xD8, (int)MapList.Caves, LocationIds.LightTemple, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw } }); // Light Temple single chest
		public static TreasureObject ForestaHouse01 = new TreasureObject(0x05, (int)MapList.ForestaInterior, LocationIds.Foresta, TreasureType.Chest, "House Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ForestaHouse02 = new TreasureObject(0x2E, (int)MapList.ForestaInterior, LocationIds.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ForestaHouse03 = new TreasureObject(0x2F, (int)MapList.ForestaInterior, LocationIds.Foresta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject Tree01 = new TreasureObject(0x30, (int)MapList.ForestaInterior, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WoodHouseMobiusCrestTile } });
		public static TreasureObject Tree02 = new TreasureObject(0x31, (int)MapList.ForestaInterior, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WoodHouseMobiusCrestTile } });
		public static TreasureObject Tree03 = new TreasureObject(0x32, (int)MapList.ForestaInterior, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WoodHouseLibraCrestTile } });
		public static TreasureObject Tree04 = new TreasureObject(0x33, (int)MapList.ForestaInterior, LocationIds.None, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WoodHouseGeminiCrestTile } });
		public static TreasureObject MineInterior01 = new TreasureObject(0x10, (int)MapList.FocusTowerBase, LocationIds.Mine, TreasureType.Chest, "Mine Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior02 = new TreasureObject(0x76, (int)MapList.FocusTowerBase, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior03 = new TreasureObject(0x77, (int)MapList.FocusTowerBase, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		public static TreasureObject MineInterior04 = new TreasureObject(0x78, (int)MapList.FocusTowerBase, LocationIds.Mine, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.Bomb } });
		//public static TreasureObject FocusTower01 = new TreasureObject(0x20, (int)MapList.FocusTowerBase, LocationIds.FocusTowerWest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle01 = new TreasureObject(0x01, (int)MapList.FocusTowerBase, LocationIds.DoomCastle, TreasureType.Chest, "Sand Floor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject DoomCastle02 = new TreasureObject(0x1E, (int)MapList.FocusTowerBase, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } });
		public static TreasureObject DoomCastle03 = new TreasureObject(0x1F, (int)MapList.FocusTowerBase, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		// public static TreasureObject FocusTower03 = new TreasureObject(0x1F, (int)MapList.FocusTowerBase, LocationIds.FocusTowerWest, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } }); Wind Chest??
		public static TreasureObject FocusTower05 = new TreasureObject(0x00, (int)MapList.FocusTowerBase, LocationIds.FocusTowerForesta, TreasureType.Chest, "SunCoin Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SunCoin } });
		public static TreasureObject FocusTower01 = new TreasureObject(0x21, (int)MapList.FocusTower, LocationIds.FocusTowerForesta, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower02 = new TreasureObject(0x22, (int)MapList.FocusTower, LocationIds.FocusTowerFireburg, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower03 = new TreasureObject(0x02, (int)MapList.FocusTower, LocationIds.FocusTowerFrozen, TreasureType.Chest, "Backdoor Chest", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FocusTower04 = new TreasureObject(0x03, (int)MapList.FocusTower, LocationIds.FocusTowerForesta, TreasureType.Chest, "SandCoin Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SandCoin } });
		public static TreasureObject DoomCastle04 = new TreasureObject(0xE7, (int)MapList.DoomCastleIce, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle06 = new TreasureObject(0xE8, (int)MapList.DoomCastleIce, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle07 = new TreasureObject(0xE9, (int)MapList.DoomCastleIce, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle08 = new TreasureObject(0xEA, (int)MapList.DoomCastleIce, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.DragonClaw } });
		public static TreasureObject DoomCastle09 = new TreasureObject(0xEB, (int)MapList.DoomCastleLava, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle10 = new TreasureObject(0xEC, (int)MapList.DoomCastleLava, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle11 = new TreasureObject(0xED, (int)MapList.DoomCastleLava, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle12 = new TreasureObject(0xEE, (int)MapList.DoomCastleLava, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle13 = new TreasureObject(0xEF, (int)MapList.DoomCastleSky, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle14 = new TreasureObject(0xF0, (int)MapList.DoomCastleSky, LocationIds.DoomCastle, TreasureType.Box, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle15 = new TreasureObject(0xF2, (int)MapList.DoomCastleHero, LocationIds.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle16 = new TreasureObject(0xF3, (int)MapList.DoomCastleHero, LocationIds.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle17 = new TreasureObject(0xF4, (int)MapList.DoomCastleHero, LocationIds.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DoomCastle18 = new TreasureObject(0xF5, (int)MapList.DoomCastleHero, LocationIds.DoomCastle, TreasureType.Chest, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject ForestaBattlefieldReward = new TreasureObject(0x01, (int)MapList.Overworld, LocationIds.ForestaWestBattlefield, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject AquariaBattlefieldReward = new TreasureObject(0x02, (int)MapList.Overworld, LocationIds.AquariaBattlefield03, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject LibraBattlefieldReward = new TreasureObject(0x03, (int)MapList.Overworld, LocationIds.LibraBattlefield01, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject FireburgBattlefieldReward = new TreasureObject(0x04, (int)MapList.Overworld, LocationIds.FireburgBattlefield02, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject MineBattlefieldReward = new TreasureObject(0x05, (int)MapList.Overworld, LocationIds.MineBattlefield02, TreasureType.Battlefield, new List<List<AccessReqs>> { new List<AccessReqs> { } });

		public static TreasureObject OldManForesta = new TreasureObject((int)ItemGivingNPCs.BoulderOldMan, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject KaeliForesta = new TreasureObject((int)ItemGivingNPCs.KaeliForesta, (int)MapList.LevelAliveForest, LocationIds.LevelForest, TreasureType.NPC, "Kaeli", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.TreeWither, AccessReqs.Kaeli1 } });
		public static TreasureObject TristamBoneDungeon = new TreasureObject((int)ItemGivingNPCs.TristamBoneDungeonBomb, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.NPC, "Tristam", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Tristam } });
		//public static TreasureObject TristamBoneDungeonExlixir = new TreasureObject((int)ItemGivingNPCs.TristamBoneDungeonElixir, (int)MapList.BoneDungeon, LocationIds.BoneDungeon, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Bomb } });
		public static TreasureObject SellerAquaria = new TreasureObject((int)ItemGivingNPCs.WomanAquaria, (int)MapList.Aquaria, LocationIds.Aquaria, TreasureType.NPC, "Aquaria Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject PhoebeWintryCave = new TreasureObject((int)ItemGivingNPCs.PhoebeWintryCave, (int)MapList.WintryCave, LocationIds.WintryCave, TreasureType.NPC, "Phoebe", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Phoebe1 } });
		public static TreasureObject MysteriousManLifeTemple = new TreasureObject((int)ItemGivingNPCs.MysteriousManLifeTemple, (int)MapList.Caves, LocationIds.LifeTemple, TreasureType.NPC, "Mysterious Man", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject PhoebeFallBasin = new TreasureObject((int)ItemGivingNPCs.PhoebeFallBasin, (int)MapList.FallBasin, LocationIds.FallsBasin, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject IceGolem = new TreasureObject(0x07, (int)MapList.IcePyramidA, LocationIds.IcePyramid, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Sword, AccessReqs.Bomb, AccessReqs.Claw } });
		public static TreasureObject Spencer = new TreasureObject((int)ItemGivingNPCs.Spencer, (int)MapList.SpencerCave, LocationIds.Aquaria, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.WakeWater, AccessReqs.AquariaPlaza } });
		public static TreasureObject VenusChest = new TreasureObject((int)ItemGivingNPCs.VenusChest, (int)MapList.FocusTowerBase, LocationIds.FocusTowerFrozen, TreasureType.NPC, "Venus Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.VenusKey, AccessReqs.Bomb } });
		public static TreasureObject TristamFireburg = new TreasureObject((int)ItemGivingNPCs.TristamFireburg, (int)MapList.Fireburg, LocationIds.Fireburg, TreasureType.NPC, "Tristam", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject SellerFireburg = new TreasureObject((int)ItemGivingNPCs.WomanFireburg, (int)MapList.Fireburg, LocationIds.Fireburg, TreasureType.NPC, "Fireburg Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject ReubenMine = new TreasureObject((int)ItemGivingNPCs.PhoebeFallBasin, (int)MapList.VolcanoTop, LocationIds.Mine, TreasureType.NPC, "Reuben", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MineCliff, AccessReqs.Reuben1 } });
		public static TreasureObject MegaGrenadeDude = new TreasureObject((int)ItemGivingNPCs.MegaGrenadeDude, (int)MapList.Fireburg, LocationIds.Fireburg, TreasureType.NPC, "MegaGrenade Dude", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.MultiKey, AccessReqs.Claw } });
		//public static TreasureObject GiantTree = new TreasureObject(0x0C, (int)MapList.LevelAliveForest, LocationIds.AliveForest, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Axe } });
		public static TreasureObject TristamSpencerPlace = new TreasureObject((int)ItemGivingNPCs.TristamSpencersPlace, (int)MapList.SpencerCave, LocationIds.Aquaria, TreasureType.NPC, "Tristam's Chest", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.VenusKey, AccessReqs.WakeWater } });
		//public static TreasureObject Pazuzu = new TreasureObject(0x0E, (int)MapList.PazuzuTowerB, LocationIds.PazuzusTower, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.DragonClaw } });
		public static TreasureObject Arion = new TreasureObject((int)ItemGivingNPCs.ArionFireburg, (int)MapList.Fireburg, LocationIds.Fireburg, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MegaGrenade, AccessReqs.MineCliff, AccessReqs.ReubenDadSaved } });
		public static TreasureObject KaeliWindia = new TreasureObject((int)ItemGivingNPCs.KaeliWindia, (int)MapList.Windia, LocationIds.Windia, TreasureType.NPC, "Kaeli", new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.Kaeli1, AccessReqs.Elixir, AccessReqs.TreeWither } });
		public static TreasureObject SellerWindia = new TreasureObject((int)ItemGivingNPCs.GirlWindia, (int)MapList.Windia, LocationIds.Windia, TreasureType.NPC, "Windia Vendor Girl", new List<List<AccessReqs>> { new List<AccessReqs> { } });
		//public static TreasureObject Kaeli02 = new TreasureObject(0x04, (int)MapList.Windia, LocationIds.Windia, TreasureType.NPC, new List<List<AccessReqs>> { new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.ThunderRock, AccessReqs.MegaGrenade } });


		public static TreasureObject DummySandTemple = new TreasureObject(0x00, (int)MapList.Caves, LocationIds.SandTemple, TreasureType.Dummy, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DummyShipDock = new TreasureObject(0x00, (int)MapList.Caves, LocationIds.ShipDock, TreasureType.Dummy, new List<List<AccessReqs>> { new List<AccessReqs> { } });
		public static TreasureObject DummyFocusTowerSouth2 = new TreasureObject(0x00, (int)MapList.Caves, LocationIds.FocusTowerWindia, TreasureType.Dummy, new List<List<AccessReqs>> { new List<AccessReqs> { } });

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
		public static List<TreasureObject> Generate(Flags flags, Battlefields battlefields, EntrancesData locations, Overworld overworld)
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
					//battlefieldsLocation[i].Location = battlefields.BattlefieldsWithItem[i];
				}
			}
			
			// Add Friendly logic extra requirements
			if (flags.LogicOptions == LogicOptions.Friendly)
			{
				foreach (var location in FriendAccessReqs)
				{
					//overworld.Locations.Find(x => x.LocationId == location.Key).AccessRequirements.ForEach(x => x.AddRange(location.Value));
				}
			}

			// Update teleporters logic
			foreach (var teleporterlink in locations.CrestPairs)
			{
				var teleporter1subregion = overworld.Locations.Find(x => x.LocationId == TeleporterLocations.Find(t => t.Item1 == teleporterlink.Item2).Item2).SubRegion;
				var teleporter2subregion = overworld.Locations.Find(x => x.LocationId == TeleporterLocations.Find(t => t.Item1 == teleporterlink.Item3).Item2).SubRegion;

				if (teleporter1subregion != teleporter2subregion)
				{
					var teleporter1access = SubRegionsAccess.Find(x => x.Item1 == teleporter1subregion).Item2;
					var teleporter2access = SubRegionsAccess.Find(x => x.Item1 == teleporter2subregion).Item2;

					var commonReqs = TeleporterAccess.Where(t => (t.Item1 == teleporterlink.Item2) || (t.Item1 == teleporterlink.Item3)).SelectMany(x => x.Item2).ToList();

					teleporter1access.ForEach(x => x.AddRange(commonReqs.Concat(new List<AccessReqs> { teleporterlink.Item1, teleporterlink.Item2 })));
					teleporter2access.ForEach(x => x.AddRange(commonReqs.Concat(new List<AccessReqs> { teleporterlink.Item1, teleporterlink.Item3 })));

					//overworld.Locations.Where(x => x.SubRegion == teleporter1subregion).ToList().ForEach(x => x.AccessRequirements.AddRange(teleporter2access));
					//overworld.Locations.Where(x => x.SubRegion == teleporter2subregion).ToList().ForEach(x => x.AccessRequirements.AddRange(teleporter1access));
				}
			}

			var voidLocations = properties.Where(x => x.Location == LocationIds.None).ToList();

			foreach (var loc in voidLocations)
			{
				var access = loc.AccessRequirements[0][0];

				var actualLocation = locations.CrestPairs.Find(x => x.loc1 == access);
				var actualAccess = (actualLocation.crest, actualLocation.loc1 == access ? actualLocation.loc2 : actualLocation.loc1);
				var extraReq = TeleporterAccess.Where(x => x.Item1 == actualAccess.Item2).SelectMany(x => x.Item2).ToList();

				loc.Location = TeleporterLocations.Find(x => x.Item1 == actualAccess.Item2).Item2;
				loc.AccessRequirements.ForEach(x => x.Remove(access));
				loc.AccessRequirements.ForEach(x => x.AddRange(extraReq.Concat(new List<AccessReqs> { actualAccess.crest })));
			}

			// Add Sealed Temple/Exit book trick
			if (flags.LogicOptions == LogicOptions.Expert)
			{
				List<AccessReqs> sealedTempleExit = new() { AccessReqs.RiverCoin, AccessReqs.SealedTempleCrestTile, AccessReqs.ExitBook, AccessReqs.GeminiCrest };

				//overworld.Locations.Where(x => x.SubRegion == SubRegions.Aquaria || x.SubRegion == SubRegions.AquariaFrozenField).ToList().ForEach(x => x.AccessRequirements.Add(sealedTempleExit));
			}

			// Collapse Access Requirements
			for (int i = 0; i < properties.Count; i++)
			{
				var locRequirements = properties[i].AccessRequirements[0].ToList();
				properties[i].AccessRequirements.Clear();

				/*
				foreach (var accessreqlist in overworld.Locations.Find(x => x.LocationId == properties[i].Location).AccessRequirements)
				{
					properties[i].AccessRequirements.Add(locRequirements.Concat(accessreqlist).ToList());
				}*/

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

			/*
			if (flags.BoxesShuffle == ItemShuffleBoxes.Exclude)
			{
				properties.Where(x => x.Type == TreasureType.Box).ToList().ForEach(x => x.Exclude = true);
			}*/

			// Exclude dummy locations
			properties.Where(x => x.Type == TreasureType.Dummy).ToList().ForEach(x => x.Exclude = true);

			// Exclude Hero Statue room's chests
			properties.Where(x => x.Type == TreasureType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList().ForEach(x => x.Exclude = true);

			return properties.ToList();
		}
		/*public List<TreasureObject> Clone(List<TreasureObject> initialTreasureList)
		{ 
			return 
			
			foreach(var  )
			
		
		}*/
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
			properties[5].Content = Items.Wakewater;
			properties[6].Content = Items.JumboBomb;
			properties[7].Content = Items.VenusKey;
			properties[8].Content = Items.VenusShield;
			properties[9].Content = Items.BlizzardBook;
			properties[10].Content = Items.MultiKey;
			properties[11].Content = Items.MegaGrenade;
			properties[12].Content = Items.DragonClaw;
			properties[13].Content = Items.ThunderRock;
			properties[14].Content = Items.CaptainsCap;

			return properties;
		}
		public static Dictionary<LocationIds, List<AccessReqs>> FriendAccessReqs => new Dictionary<LocationIds, List<AccessReqs>>
		{
			{ LocationIds.BoneDungeon, new List<AccessReqs> { AccessReqs.Bomb} },
			{ LocationIds.WintryCave, new List<AccessReqs> { AccessReqs.Bomb, AccessReqs.Claw  } },
			{ LocationIds.IcePyramid, new List<AccessReqs> { AccessReqs.Bomb, AccessReqs.Claw } },
			{ LocationIds.Mine, new List<AccessReqs> { AccessReqs.MegaGrenade, AccessReqs.Claw, AccessReqs.Reuben1 } },
			{ LocationIds.LavaDome, new List<AccessReqs> { AccessReqs.MegaGrenade } },
			{ LocationIds.GiantTree, new List<AccessReqs> { AccessReqs.Axe, AccessReqs.DragonClaw } },
			{ LocationIds.MountGale, new List<AccessReqs> { AccessReqs.DragonClaw } },
			{ LocationIds.PazuzusTower, new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.Bomb } },
			{ LocationIds.MacsShip, new List<AccessReqs> { AccessReqs.DragonClaw, AccessReqs.CaptainCap } },
		};
		public static MapRegions ReturnRegion(LocationIds location)
		{
			return Regions.Find(x => x.Item2 == location).Item1;
		}
		public static List<(MapRegions, LocationIds)> Regions => new()
		{
			(MapRegions.Foresta, LocationIds.ForestaSouthBattlefield),
			(MapRegions.Foresta, LocationIds.ForestaWestBattlefield),
			(MapRegions.Foresta, LocationIds.ForestaEastBattlefield),
			(MapRegions.Aquaria, LocationIds.AquariaBattlefield01),
			(MapRegions.Aquaria, LocationIds.AquariaBattlefield02),
			(MapRegions.Aquaria, LocationIds.AquariaBattlefield03),
			(MapRegions.Aquaria, LocationIds.WintryBattlefield01),
			(MapRegions.Aquaria, LocationIds.WintryBattlefield02),
			(MapRegions.Aquaria, LocationIds.PyramidBattlefield01),
			(MapRegions.Aquaria, LocationIds.LibraBattlefield01),
			(MapRegions.Aquaria, LocationIds.LibraBattlefield02),
			(MapRegions.Fireburg, LocationIds.FireburgBattlefield01),
			(MapRegions.Fireburg, LocationIds.FireburgBattlefield02),
			(MapRegions.Fireburg, LocationIds.FireburgBattlefield03),
			(MapRegions.Fireburg, LocationIds.MineBattlefield01),
			(MapRegions.Fireburg, LocationIds.MineBattlefield02),
			(MapRegions.Fireburg, LocationIds.MineBattlefield03),
			(MapRegions.Fireburg, LocationIds.VolcanoBattlefield01),
			(MapRegions.Windia, LocationIds.WindiaBattlefield01),
			(MapRegions.Windia, LocationIds.WindiaBattlefield02),
			(MapRegions.Foresta, LocationIds.HillOfDestiny),
			(MapRegions.Foresta, LocationIds.LevelForest),
			(MapRegions.Foresta, LocationIds.Foresta),
			(MapRegions.Foresta, LocationIds.SandTemple),
			(MapRegions.Foresta, LocationIds.BoneDungeon),
			(MapRegions.Foresta, LocationIds.FocusTowerForesta),
			(MapRegions.Aquaria, LocationIds.FocusTowerAquaria),
			(MapRegions.Aquaria, LocationIds.LibraTemple),
			(MapRegions.Aquaria, LocationIds.Aquaria),
			(MapRegions.Aquaria, LocationIds.WintryCave),
			(MapRegions.Aquaria, LocationIds.LifeTemple),
			(MapRegions.Aquaria, LocationIds.FallsBasin),
			(MapRegions.Aquaria, LocationIds.IcePyramid),
			(MapRegions.Aquaria, LocationIds.WintryTemple),
			(MapRegions.Aquaria, LocationIds.FocusTowerFrozen),
			(MapRegions.Fireburg, LocationIds.FocusTowerFireburg),
			(MapRegions.Fireburg, LocationIds.Fireburg),
			(MapRegions.Fireburg, LocationIds.Mine),
			(MapRegions.Fireburg, LocationIds.SealedTemple),
			(MapRegions.Fireburg, LocationIds.Volcano),
			(MapRegions.Fireburg, LocationIds.LavaDome),
			(MapRegions.Windia, LocationIds.FocusTowerWindia),
			(MapRegions.Windia, LocationIds.RopeBridge),
			(MapRegions.Windia, LocationIds.AliveForest),
			(MapRegions.Windia, LocationIds.GiantTree),
			(MapRegions.Windia, LocationIds.KaidgeTemple),
			(MapRegions.Windia, LocationIds.Windia),
			(MapRegions.Windia, LocationIds.WindholeTemple),
			(MapRegions.Windia, LocationIds.MountGale),
			(MapRegions.Windia, LocationIds.PazuzusTower),
			(MapRegions.Windia, LocationIds.SpencersPlace),
			(MapRegions.Windia, LocationIds.ShipDock),
			(MapRegions.Windia, LocationIds.DoomCastle),
			(MapRegions.Windia, LocationIds.LightTemple),
			(MapRegions.Windia, LocationIds.MacsShip)
		};
		public static List<(SubRegions, LocationIds)> MapSubRegions => new()
		{
			(SubRegions.Foresta, LocationIds.ForestaSouthBattlefield),
			(SubRegions.Foresta, LocationIds.ForestaWestBattlefield),
			(SubRegions.Foresta, LocationIds.ForestaEastBattlefield),
			(SubRegions.Aquaria, LocationIds.AquariaBattlefield01),
			(SubRegions.Aquaria, LocationIds.AquariaBattlefield02),
			(SubRegions.Aquaria, LocationIds.AquariaBattlefield03),
			(SubRegions.Aquaria, LocationIds.WintryBattlefield01),
			(SubRegions.Aquaria, LocationIds.WintryBattlefield02),
			(SubRegions.Aquaria, LocationIds.PyramidBattlefield01),
			(SubRegions.AquariaFrozenField, LocationIds.LibraBattlefield01),
			(SubRegions.AquariaFrozenField, LocationIds.LibraBattlefield02),
			(SubRegions.Fireburg, LocationIds.FireburgBattlefield01),
			(SubRegions.Fireburg, LocationIds.FireburgBattlefield02),
			(SubRegions.Fireburg, LocationIds.FireburgBattlefield03),
			(SubRegions.Fireburg, LocationIds.MineBattlefield01),
			(SubRegions.Fireburg, LocationIds.MineBattlefield02),
			(SubRegions.Fireburg, LocationIds.MineBattlefield03),
			(SubRegions.VolcanoBattlefield, LocationIds.VolcanoBattlefield01),
			(SubRegions.Windia, LocationIds.WindiaBattlefield01),
			(SubRegions.Windia, LocationIds.WindiaBattlefield02),
			(SubRegions.Foresta, LocationIds.HillOfDestiny),
			(SubRegions.Foresta, LocationIds.LevelForest),
			(SubRegions.Foresta, LocationIds.Foresta),
			(SubRegions.Foresta, LocationIds.SandTemple),
			(SubRegions.Foresta, LocationIds.BoneDungeon),
			(SubRegions.Foresta, LocationIds.FocusTowerForesta),
			(SubRegions.Aquaria, LocationIds.FocusTowerAquaria),
			(SubRegions.Aquaria, LocationIds.LibraTemple),
			(SubRegions.Aquaria, LocationIds.Aquaria),
			(SubRegions.Aquaria, LocationIds.WintryCave),
			(SubRegions.LifeTemple, LocationIds.LifeTemple),
			(SubRegions.Aquaria, LocationIds.FallsBasin),
			(SubRegions.Aquaria, LocationIds.IcePyramid),
			(SubRegions.AquariaFrozenField, LocationIds.WintryTemple),
			(SubRegions.AquariaFrozenField, LocationIds.FocusTowerFrozen),
			(SubRegions.Fireburg, LocationIds.FocusTowerFireburg),
			(SubRegions.Fireburg, LocationIds.Fireburg),
			(SubRegions.Fireburg, LocationIds.Mine),
			(SubRegions.Fireburg, LocationIds.SealedTemple),
			(SubRegions.Fireburg, LocationIds.Volcano),
			(SubRegions.Fireburg, LocationIds.LavaDome),
			(SubRegions.Windia, LocationIds.FocusTowerWindia),
			(SubRegions.Windia, LocationIds.RopeBridge),
			(SubRegions.Windia, LocationIds.AliveForest),
			(SubRegions.Windia, LocationIds.GiantTree),
			(SubRegions.Windia, LocationIds.KaidgeTemple),
			(SubRegions.Windia, LocationIds.Windia),
			(SubRegions.Windia, LocationIds.WindholeTemple),
			(SubRegions.Windia, LocationIds.MountGale),
			(SubRegions.Windia, LocationIds.PazuzusTower),
			(SubRegions.SpencerCave, LocationIds.SpencersPlace),
			(SubRegions.ShipDock, LocationIds.ShipDock),
			(SubRegions.DoomCastle, LocationIds.DoomCastle),
			(SubRegions.LightTemple, LocationIds.LightTemple),
			(SubRegions.MacShip, LocationIds.MacsShip)
		};
		public static List<(AccessReqs, LocationIds)> TeleporterLocations => new()
		{
			(AccessReqs.LibraTempleCrestTile, LocationIds.LibraTemple),
			(AccessReqs.LifeTempleCrestTile, LocationIds.LifeTemple),
			(AccessReqs.AquariaVendorCrestTile, LocationIds.Aquaria),
			(AccessReqs.FireburgVendorCrestTile, LocationIds.Fireburg),
			(AccessReqs.FireburgGrenademanCrestTile, LocationIds.Fireburg),
			(AccessReqs.SealedTempleCrestTile, LocationIds.SealedTemple),
			(AccessReqs.WintryTempleCrestTile, LocationIds.WintryTemple),
			(AccessReqs.KaidgeTempleCrestTile, LocationIds.KaidgeTemple),
			(AccessReqs.LightTempleCrestTile, LocationIds.LightTemple),
			(AccessReqs.WindiaKidsCrestTile, LocationIds.Windia),
			(AccessReqs.WindiaDockCrestTile, LocationIds.Windia),
			(AccessReqs.ShipDockCrestTile, LocationIds.ShipDock),
			(AccessReqs.AliveForestLibraCrestTile, LocationIds.AliveForest),
			(AccessReqs.AliveForestGeminiCrestTile, LocationIds.AliveForest),
			(AccessReqs.AliveForestMobiusCrestTile, LocationIds.AliveForest),
			(AccessReqs.WoodHouseLibraCrestTile, LocationIds.AliveForest),
			(AccessReqs.WoodHouseGeminiCrestTile, LocationIds.AliveForest),
			(AccessReqs.WoodHouseMobiusCrestTile, LocationIds.AliveForest),
		};
		public static List<(AccessReqs, List<AccessReqs>)> TeleporterAccess => new()
		{
			(AccessReqs.FireburgGrenademanCrestTile, new List<AccessReqs> { AccessReqs.Claw, AccessReqs.MultiKey }),
			(AccessReqs.WintryTempleCrestTile, new List<AccessReqs> { AccessReqs.Barred }),
			(AccessReqs.KaidgeTempleCrestTile, new List<AccessReqs> { AccessReqs.Claw }),
			(AccessReqs.AliveForestLibraCrestTile, new List<AccessReqs> { AccessReqs.Axe }),
			(AccessReqs.AliveForestGeminiCrestTile, new List<AccessReqs> { AccessReqs.Axe }),
			(AccessReqs.AliveForestMobiusCrestTile, new List<AccessReqs> { AccessReqs.Axe }),
			(AccessReqs.WoodHouseLibraCrestTile, new List<AccessReqs> { AccessReqs.Barred }),
			(AccessReqs.WoodHouseGeminiCrestTile, new List<AccessReqs> { AccessReqs.Barred }),
			(AccessReqs.WoodHouseMobiusCrestTile, new List<AccessReqs> { AccessReqs.Barred }),
		};
		public static List<(SubRegions, List<List<AccessReqs>>)> SubRegionsAccess => new()
		{
			(SubRegions.Foresta, new List<List<AccessReqs>> { new List<AccessReqs> { } }),
			(SubRegions.Aquaria, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.SummerAquaria },
				//new List<AccessReqs> { AccessReqs.LifeTempleLibraTeleporter, AccessReqs.LibraCrest, AccessReqs.LibraTempleLibraTeleporter }
			}),
			(SubRegions.LifeTemple, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
				//new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.LibraCrest, AccessReqs.LibraTempleLibraTeleporter, AccessReqs.LifeTempleLibraTeleporter },
			}),
			(SubRegions.AquariaFrozenField, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.SummerAquaria },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.SummerAquaria },
			}),
			(SubRegions.Fireburg, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.SummerAquaria },
			}),
			(SubRegions.VolcanoBattlefield, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.DualheadHydra, AccessReqs.SummerAquaria },
			}),
			(SubRegions.Windia, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			}),
			(SubRegions.SpencerCave, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.RainbowBridge },
			}),
			(SubRegions.LightTemple, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
			}),
			(SubRegions.ShipDock, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
			}),
			(SubRegions.MacShip, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.ShipDockAccess, AccessReqs.ShipLiberated },
			}),
			(SubRegions.DoomCastle, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.ShipDockAccess, AccessReqs.ShipSteeringWheel, AccessReqs.ShipLoaned },
			}),
		};
		public static Dictionary<Items, List<AccessReqs>> ItemAccessReq => new Dictionary<Items, List<AccessReqs>>
		{
			{ Items.Elixir, new List<AccessReqs> { AccessReqs.Elixir } },
			{ Items.TreeWither, new List<AccessReqs> { AccessReqs.TreeWither } },
			{ Items.Wakewater, new List<AccessReqs> { AccessReqs.WakeWater } },
			{ Items.VenusKey, new List<AccessReqs> { AccessReqs.VenusKey } },
			{ Items.MultiKey, new List<AccessReqs> { AccessReqs.MultiKey } },
			{ Items.ThunderRock, new List<AccessReqs> { AccessReqs.ThunderRock } },
			{ Items.CaptainsCap, new List<AccessReqs> { AccessReqs.CaptainCap } },
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

		public static List<(LocationIds, int)> LocationsByEntrances = new()
		{
			(LocationIds.LevelForest, 28),
			(LocationIds.Foresta, 38),
			(LocationIds.SandTemple, 54),
			(LocationIds.BoneDungeon, 55),
			(LocationIds.FocusTowerForesta, 3),
			(LocationIds.FocusTowerAquaria, 16),
			(LocationIds.LibraTemple, 75),
			(LocationIds.LifeTemple, 107),
			(LocationIds.Aquaria, 77),
			(LocationIds.WintryCave, 99),
			(LocationIds.FallsBasin, 111),
			(LocationIds.IcePyramid, 114),
			(LocationIds.SpencersPlace, 151),
			(LocationIds.WintryTemple, 157),
			(LocationIds.FocusTowerFrozen, 24),
			(LocationIds.FocusTowerFireburg, 22),
			(LocationIds.Fireburg, 159),
			(LocationIds.SealedTemple, 190),
			(LocationIds.Mine, 179),
			(LocationIds.Volcano, 192),
			(LocationIds.LavaDome, 209),
			(LocationIds.FocusTowerWindia, 9),
			(LocationIds.RopeBridge, 255),
			(LocationIds.AliveForest, 272),
			(LocationIds.KaidgeTemple, 307),
			(LocationIds.WindholeTemple, 309),
			(LocationIds.MountGale, 310),
			(LocationIds.Windia, 312),
			(LocationIds.PazuzusTower, 341),
			(LocationIds.LightTemple, 395),
			(LocationIds.ShipDock, 399),
			(LocationIds.MacsShip, 400),
			(LocationIds.DoomCastle, 1),
		};
	}

}
