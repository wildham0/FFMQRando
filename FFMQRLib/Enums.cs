using System;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public enum NodeDirections : int
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3,
	}
	public enum SubRegions
	{ 
		Foresta,
		Aquaria,
		LifeTemple,
		AquariaFrozenField,
		Fireburg,
		VolcanoBattlefield,
		Windia,
		SpencerCave,
		ShipDock,
		MacShip,
		LightTemple,
		DoomCastle
	}
	public enum GameFlagsList : int
	{
		FlamerusRexDefeated = 0x01,
		WakeWaterUsed = 0x02, // This is on IceGolemDefeated, but we transfer it to wakewater
		IceGolemDefeated = 0x12,
		OldManSaved = 0x13,
		TreeWitherAcquired = 0x14, // ??? is it? it's set by oldman, but the script don't check for it, nor Kaeli
		GiantTreeSet = 0x1D,
		MinotaurDefeated = 0x1E,
		CureKaeli = 0x1F,
		ShowColumnNotMoved = 0x20,
		ShowColumnMoved = 0x21,
		HillCollapsed = 0x22,
		SandCoinUsed = 0x23,
		ShowForestaBoulder = 0x34,
		WintryCaveCollapsed = 0x35,
		ShowFireburgBoulder = 0x36,
		GiantTreeUnset = 0x3A,
		ShowPazuzuBridge = 0x3D,
		PhoebeHouseVisited = 0x4D,
		UseWakeWater = 0x4F,
		UseRiverCoin = 0x5B,
		BoulderRolled = 0x5C,
		DefeatMedusa = 0x5D,
		ShowFirstKaeli = 0x62,
		EnableMinotaurFight = 0x63,
		VolcanoErupted = 0x64,
		ShowMysteriousManLevelForest = 0x6C,
		HideDiseasedTree = 0x6D,
		TalkToTristam = 0x6E,
		TalkToPhoebe = 0x70,
		ExitFallBasin = 0x73,
		TalkToGrenadeGuy = 0x74,
		ShowSickKaeli = 0x7D,
		ShowFallBasinChest = 0xB3,
		RainbowRoad = 0xCF,
		SquidDefeated = 0xE0,
		PathCuttedByKaeli = 0xE3,
		ShowFigureForHP = 0xF0,
	}
	public enum NewGameFlagsList : int
	{
		WakeWaterUsed = 0x02, 

		// Barrel Flag
		ShowBarrelMoved = 0x20,
		ShowBarrelNotMoved = 0x21,

		// Kaeli new flags
		ShowForestaKaeli = 0x62,
		ShowSickKaeli = 0x7D,
		ShowWindiaKaeli = 0x68,
		KaeliCured = 0x1F,
		KaeliSecondItemGiven = 0xE8,

		// Tristam new flags
		ShowSandTempleTristam = 0x5A,
		TristamBoneDungeonItemGiven = 0xC9,
		ShowFireburgTristam = 0x53,
		TristamFireburgItemGiven = 0xE9,

		// Phoebe new flags
		ShowLibraTemplePhoebe = 0x4E,
		PhoebeWintryItemGiven = 0xEA,
		ShowWindiaPhoebe = 0x7E,

		// Reuben new flags
		ShowFireburgReuben = 0x5F,
		ReubenMineItemGiven = 0xEB,

		// Arion new flags
		ArionItemGiven = 0xEC,

		// Chests flags
		TristamChestUnopened = 0xED,
		VenusChestUnopened = 0xEE,

		// Spencer
		SpencerItemGiven = 0xEF,

		// Sellers flags
		AquariaSellerItemBought = 0xD0,
		FireburgSellerItemBought = 0xD1,
		WindiaSellerItemBought = 0xD2,

		// Tentative removed enemy flag
		ShowEnemies = 0xF9,


	}
	public enum AccessReqs : int
	{
		Elixir,
		TreeWither,
		WakeWater,
		VenusKey,
		MultiKey,
		Mask,
		MagicMirror,
		ThunderRock,
		CaptainCap,
		LibraCrest,
		GeminiCrest,
		MobiusCrest,
		SandCoin,
		RiverCoin,
		SunCoin,
		SkyCoin,
		Sword,
		Axe,
		Claw,
		CatClaw,
		CharmClaw,
		DragonClaw,
		Bomb,
		SmallBomb,
		JumboBomb,
		MegaGrenade,
		
		Kaeli1,
		Kaeli2,
		Tristam,
		Phoebe1,
		Reuben1,
		ReubenDadSaved,
		Otto,
		CaptainMac,
		ShipSteeringWheel,

		Minotaur,
		FlamerusRex,
		Phanquid,
		FreezerCrab,
		IceGolem,
		Jinn,
		Medusa,
		DualheadHydra,
		Gidrah,
		Dullahan,
		Pazuzu,
		
		ExitBook,
		
		AquariaPlaza,
		SummerAquaria,
		MineCliff,
		AliveForest,
		
		RainbowBridge,
		SpencerCaveTrigger,
		ShipLiberated,
		ShipLoaned,
		ShipDockAccess,
		
		LibraTempleCrestTile,
		LifeTempleCrestTile,
		AquariaVendorCrestTile,
		FireburgVendorCrestTile,
		FireburgGrenademanCrestTile,
		SealedTempleCrestTile,
		WintryTempleCrestTile,
		KaidgeTempleCrestTile,
		LightTempleCrestTile,
		WindiaKidsCrestTile,
		WindiaDockCrestTile,
		ShipDockCrestTile,
		AliveForestLibraCrestTile,
		AliveForestGeminiCrestTile,
		AliveForestMobiusCrestTile,
		WoodHouseLibraCrestTile,
		WoodHouseGeminiCrestTile,
		WoodHouseMobiusCrestTile,
		
		BarrelPushed,
		
		LongSpineBombed, 
		ShortSpineBombed, 
		Skull1Bombed, 
		Skull2Bombed, 

		IcePyramid1FStatue, //
		IcePyramid3FStatue,
		IcePyramid4FStatue,
		IcePyramid5FStatue,

		SpencerCaveLibraBlockBombed,

		LavaDomePlate,

		Pazuzu2FLock,
		Pazuzu4FLock,
		Pazuzu6FLock,
		Pazuzu1F,
		Pazuzu2F,
		Pazuzu3F,
		Pazuzu4F,
		Pazuzu5F,
		Pazuzu6F,

		StoneGolem,
		TwinheadWyvern,
		Zuh,
		
		Barred,
	}
	public enum TreasureType : int
	{
		Chest = 1,
		Box,
		NPC,
		Battlefield,
		Dummy
			
	}
	public enum Companion : int
	{
		Benjamin = 0x00,
		Kaeli = 0x01,
		Tristam = 0x02,
		Phoebe = 0x03,
		Reuben = 0x04,
		KaeliPromo = 0x05,
		TristamPromo = 0x06,
		PhoebePromo = 0x07,
		ReubenPromo = 0x08
	}
	public enum ItemGivingNPCs
	{
		BoulderOldMan = 0,
		KaeliForesta,
		TristamBoneDungeonBomb,
		TristamBoneDungeonElixir,
		WomanAquaria,
		PhoebeWintryCave,
		MysteriousManLifeTemple,
		PhoebeFallBasin,
		Spencer,
		VenusChest,
		TristamFireburg,
		WomanFireburg,
		MegaGrenadeDude,
		TristamSpencersPlace,
		ArionFireburg,
		KaeliWindia,
		GirlWindia,

	}
	public enum FlagPositions
	{
		Weapons = 0x3210,
		Armors = 0x3510,
		Spells = 0x3810,
		Items = 0xA60E,
	}
	public enum WeaponFlags : byte
	{
		SteelSword = 0x00,
		KnightSword = 0x01,
		Excalibur = 0x02,
		Axe = 0x03,
		BattleAxe = 0x04,
		GiantsAxe = 0x05,
		CatClaw = 0x06,
		CharmClaw = 0x07,
		DragonClaw = 0x08,
		Bomb = 0x09,
		JumboBomb = 0x0A,
		MegaGrenade = 0x0B,
		MorningStar = 0x0C,
		BowOfGrace = 0x0D,
		NinjaStar = 0x0E,
	}
	public enum ArmorFlags : byte
	{
		SteelHelm = 0x00,
		MoonHelm = 0x01,
		ApolloHelm = 0x02,
		SteelArmor = 0x03,
		NobleArmor = 0x04,
		GaiasArmor = 0x05,
		ReplicaArmor = 0x06,
		MysticRobes = 0x07,
		FlameArmor = 0x08,
		BlackRobe = 0x09,
		SteelShield = 0x0A,
		VenusShield = 0x0C,
		AegisShield = 0x0C,
		EtherShield = 0x0D,
		Charm = 0x0E,
		MagicRing = 0x0F,
		CupidLocket = 0x10,
	}
	public enum SpellFlags : byte
	{
		ExitBook = 0x00,
		CureBook = 0x01,
		HealBook = 0x02,
		LifeBook = 0x03,
		QuakeBook = 0x04,
		BlizzardBook = 0x05,
		FireBook = 0x06,
		AeroBook = 0x07,
		ThunderSeal = 0x08,
		WhiteSeal = 0x09,
		MeteorSeal = 0x0A,
		FlareSeal = 0x0B,
	}
	public enum ItemFlags : byte
	{
		Elixir = 0x00,
		TreeWither = 0x01,
		WakeWater = 0x02,
		VenusKey = 0x03,
		MultiKey = 0x04,
		Mask = 0x05,
		MagicMirror = 0x06,
		ThunderRock = 0x07,
		CaptainsCap = 0x08,
		LibraCrest = 0x09,
		GeminiCrest = 0x0A,
		MobiusCrest = 0x0B,
		SandCoin = 0x0C,
		RiverCoin = 0x0D,
		SunCoin = 0x0E,
		SkyCoin = 0x0F,
	}
	public enum Items : byte
	{
		Elixir = 0x00,
		TreeWither = 0x01,
		Wakewater = 0x02,
		VenusKey = 0x03,
		MultiKey = 0x04,
		Mask = 0x05,
		MagicMirror = 0x06,
		ThunderRock = 0x07,
		CaptainsCap = 0x08,
		LibraCrest = 0x09,
		GeminiCrest = 0x0A,
		MobiusCrest = 0x0B,
		SandCoin = 0x0C,
		RiverCoin = 0x0D,
		SunCoin = 0x0E,
		SkyCoin = 0x0F,
		CurePotion = 0x10,
		HealPotion = 0x11,
		Seed = 0x12,
		Refresher = 0x13,
		ExitBook = 0x14,
		CureBook = 0x15,
		HealBook = 0x16,
		LifeBook = 0x17,
		QuakeBook = 0x18,
		BlizzardBook = 0x19,
		FireBook = 0x1A,
		AeroBook = 0x1B,
		ThunderSeal = 0x1C,
		WhiteSeal = 0x1D,
		MeteorSeal = 0x1E,
		FlareSeal = 0x1F,
		SteelSword = 0x20,
		KnightSword = 0x21,
		Excalibur = 0x22,
		Axe = 0x23,
		BattleAxe = 0x24,
		GiantsAxe = 0x25,
		CatClaw = 0x26,
		CharmClaw = 0x27,
		DragonClaw = 0x28,
		Bomb = 0x29,
		JumboBomb = 0x2A,
		MegaGrenade = 0x2B,
		MorningStar = 0x2C,
		BowOfGrace = 0x2D,
		NinjaStar = 0x2E,
		SteelHelm = 0x2F,
		MoonHelm = 0x30,
		ApolloHelm = 0x31,
		SteelArmor = 0x32,
		NobleArmor = 0x33,
		GaiasArmor = 0x34,
		ReplicaArmor = 0x35,
		MysticRobes = 0x36,
		FlameArmor = 0x37,
		BlackRobe = 0x38,
		SteelShield = 0x39,
		VenusShield = 0x3A,
		AegisShield = 0x3B,
		EtherShield = 0x3C,
		Charm = 0x3D,
		MagicRing = 0x3E,
		CupidLocket = 0x3F,
		Xp54 = 0x60,
		Xp99 = 0x61,
		Xp540 = 0x62,
		Xp744 = 0x63,
		Xp816 = 0x64,
		Xp1068 = 0x65,
		Xp1200 = 0x66,
		Xp2700 = 0x67,
		Xp2808 = 0x68,
		Gp150 = 0x69,
		Gp300 = 0x6A,
		Gp600 = 0x6B,
		Gp900 = 0x6C,
		Gp1200 = 0x6D,
		BombRefill = 0xDD,
		ProjectileRefill = 0xDE,
		APItem = 0xF0,
		APItemFiller = 0xF1,
		None = 0xFF,
	}

	public enum TileScriptsList : int
	{
		PushedBoulder = 0,
		RestInBed,
		FightMinotaur,
		BoneDungeonTristamBomb,
		FightFlamerusRex,
		EnterPhoebesHouse,
		WintryCavePhoebeClaw,
		DriedUpSpringOfLife,
		EnterAquaria,
		FallBasinGiveJumboBomb,
		FightIceGolem,
		LockedDoor, // Fireburg?
		BlowingOffMineBoulder,
		ColumnMoved,
		FightDualheadHydra,
		RopeBridgeFight,
		Unknown10, // I was waiting for you little beast!
		Unknown11, // etc...
		Unknown12, // Ah, pazuzu at each floor
		Unknown13,
		Unknown14,
		Unknown15,
		Unknown16,
		Unknown17, // Not beast one!
		HeroStatue,
		FightDarkKing,
		VolcanoTeleportFromTop, // Teleport out of dungeon?
		VolcanoTeleportToBase, // Teleport out of dungeon?
		VolcanoTeleportToMask, // Teleport to Volcano > Mask location
		VolcanoTeleportToMedusa, // Teleport to Volcano > Medusa location
		VolcanoTeleportTo2FLeft, // Teleport to Volcano 2nd level
		VolcanoTeleportTo2FRight, // Teleport to Volcano 2nd level
		EnterIcePyramid, // Teleport to Ice Pyramid Entrance
		Unknown21, // Nothing
		Unknown22, // Nothing
		Unknown23, // Nothing
		Unknown24, // Nothing
		Unknown25, // Teleport out of dungeon?
		Unknown26, // Teleport to Windia in front of Upper Left house
		Unknown27, // Teleport to Fireburg in front of Lower Left house
		Unknown28, // Teleport to Aquaria in front of Upper Left house
		Unknown29, // Nothing
		Unknown2a, // Teleport out of dungeon?
		Unknown2b, // Nothing
		Unknown2c, // Nothing
		Unknown2d, // Nothing
		EnterLevelForest, // Teleport to Level Forest
		Unknown2f, // Nothing
		EnterSpencersPlace, // Teleport to Spencer's Place, Southern Entrance
		Unknown31, // Teleport in front of Giant Tree
		Unknown32, // Teleport to MacShip B1
		Unknown33, // Teleport to MacShip B1 Left
		Unknown34, // Teleport to MacShip B1 Right
		Unknown35, // Teleport to MacShip B1 Right
		Unknown36, // Teleport to MacShip B1 Center Upper Stairs
		Unknown37, // Nothing
		Unknown38, // Teleport to MacShip B1 Corridor
		Unknown39, // Teleport to MacShip B1 Double Corridors
		Unknown3a, // Teleport to MacShip B1 Center Lower Stairs
		Unknown3b, // Teleport to MacShip B1 Upper Right Lower Stairs
		Unknown3c, // Nothing
		Unknown3d, // Nothing
		Unknown3e, // Nothing
		Unknown3f, // Nothing
		Unknown40, // Nothing
		Unknown41, // Nothing
		Unknown42, // Nothing
		Unknown43, // Nothing
		Unknown44, // Nothing
		Unknown45, // Nothing
		Unknown46, // Nothing
		Unknown47, // Nothing
		Unknown48, // Nothing
		Unknown49, // Everyone disappear?
		FightJinn,
		TristamQuitPartyBoneDungeon, // Nothing
		EnterFallBasin, // Teleport and run intro script
		IcePyramidCheckStatue,
		ReceiveWakeWater,
		EnterWindiaInn // Teleport to an Inn
	}

	public enum TalkScriptsList : int
	{
		Unknown00 = 0, // Fight Skullrus Rex, probably  default?
		Unknown01,
		MysteriousManFindPhoebe,
		Unknown03, // Doing nothing, etc.
		Unknown04,
		Unknown05,
		Unknown06,
		MysteriousManHurryToShip,
		MysteriousManSeekReuben,
		MysteriousManSaveEarthCrystal,
		Unknown0a, // Thank you, have you seen Kaeli yet? Probably Foest old man
		Unknown0b, // Nothing
		ForestaOldLady01, // Crystal of Earth is in Bone dungeon
		ForestaOldPeole01, // Strange trees
		ForestaOldLady02, // Monster drain
		ForestaOldMan02, // Turned into an old man
		ForestaOldPeople02, // Go save
		ForestaYoungPeople01, // Thanks
		ForestaYoungPeople02, // I'm a little girl
		ForestaYoungPeople03, // Mac's stories
		KaelisMother, 
		KaeliWitherTree,
		SickKaeli, // Take care... (Kaeli?)
		ForestaOldManInHouse,
		ForestaPersonInHouse01,
		ForestaPersonInHouse02,
		TristamChest,
		Unknown1b, // Empty Chest
		PhoebeLibraTemple,
		AquariaPeople01,
		AquariaPeople02,
		AquariaPeople03,
		AquariaPeople04,
		AquariaPeople05,
		AquariaPeople06,
		AquariaPeople07,
		AquariaPeople08,
		PhoebeInAquaria, // Phoebe in her house?
		KaeliInAquaria,
		AquariaPeople0a,
		AquariaPeople0b,
		AquariaSellerGirl,
		AquariaExplosiveVendor,
		AquariaPotionVendor,
		AquariaInnKeeper, // Selling something
		FightSquid, 
		Unknown2e, // crash
		FightCrab,
		IceGolemGame,
		Unknown31, // crash?
		SpencerFirstTime,
		MysteriousManSeekReuben2, // More complete
		Unknown34, // nothing
		FireburgPeople01,
		ExplosiveVendor02,
		FireburgPeople02,
		FireburgPeople03,
		FireburgPeople04,
		ReubenFireburg,
		Arion,
		MegaGrenadeMan,
		TristamInFireburg01,
		TristamInFireburg02,
		Unknown3f, // I guess I'll head back to Aquaria, prob Tristam
		FireburgSellerGirl,
		MineElevatorTop = 0x48,
		MineElevatorEntrance = 0x49,
		MineElevatorCenter = 0x4A,
		MineElevatorBottomRight = 0x4B,
		MysteriousManSealedTemple = 0x4C, 
		FightMedusa = 0x4D,
		GiantTree = 0x4E,
		FightHeadlessKnight = 0x54,
		KaeliWindia = 0x5B,
		Otto = 0x5C,
		CaptainMacWindia = 0x5F,
		WindiaExplosiveVendor = 0x66,
		WindiaSellerGirl = 0x67,
		CaptainMacOnShip = 0x75,
		VenusChest = 0x7B,
	}
	public enum BattlefieldRewardType : byte
	{
		Gold = 0x00,
		Item = 0x40,
		Experience = 0x80,
	}
	public enum OverworldSpritesId : byte
	{
		ForestaSouthBattlefield = 0x11,
		ForestaWestBattlefield = 0x12,
		ForestaEastBattlefield = 0x13,
		AquariaBattlefield01 = 0x14,
		AquariaBattlefield02 = 0x15,
		AquariaBattlefield03 = 0x16,
		WintryBattlefield01 = 0x17,
		WintryBattlefield02 = 0x18,
		PyramidBattlefield01 = 0x19,
		LibraBattlefield01 = 0x1A,
		LibraBattlefield02 = 0x1B,
		FireburgBattlefield01 = 0x1C,
		FireburgBattlefield02 = 0x1D,
		FireburgBattlefield03 = 0x1E,
		MineBattlefield01 = 0x1F,
		MineBattlefield02 = 0x20,
		MineBattlefield03 = 0x21,
		VolcanoBattlefield01 = 0x22,
		WindiaBattlefield01 = 0x23,
		WindiaBattlefield02 = 0x24,
	}
	public enum LocationIds : byte
	{
		None = 0x00,
		ForestaSouthBattlefield = 0x01,
		ForestaWestBattlefield = 0x02,
		ForestaEastBattlefield = 0x03,
		AquariaBattlefield01 = 0x04,
		AquariaBattlefield02 = 0x05,
		AquariaBattlefield03 = 0x06,
		WintryBattlefield01 = 0x07,
		WintryBattlefield02 = 0x08,
		PyramidBattlefield01 = 0x09,
		LibraBattlefield01 = 0x0A,
		LibraBattlefield02 = 0x0B,
		FireburgBattlefield01 = 0x0C,
		FireburgBattlefield02 = 0x0D,
		FireburgBattlefield03 = 0x0E,
		MineBattlefield01 = 0x0F,
		MineBattlefield02 = 0x10,
		MineBattlefield03 = 0x11,
		VolcanoBattlefield01 = 0x12,
		WindiaBattlefield01 = 0x13,
		WindiaBattlefield02 = 0x14,

		HillOfDestiny = 0x15,
		LevelForest = 0x16,
		Foresta = 0x17,
		SandTemple = 0x18,
		BoneDungeon = 0x19,
		FocusTowerForesta = 0x1A,
		FocusTowerAquaria = 0x1B,
		LibraTemple = 0x1C,
		Aquaria = 0x1D,
		WintryCave = 0x1E,
		LifeTemple = 0x1F,
		FallsBasin = 0x20,
		IcePyramid = 0x21,
		SpencersPlace = 0x22,
		WintryTemple = 0x23,
		FocusTowerFrozen = 0x24,
		FocusTowerFireburg = 0x25,
		Fireburg = 0x26,
		Mine = 0x27,
		SealedTemple = 0x28,
		Volcano = 0x29,
		LavaDome = 0x2A,
		FocusTowerWindia = 0x2B,
		RopeBridge = 0x2C,
		AliveForest = 0x2D,
		GiantTree = 0x2E,
		KaidgeTemple = 0x2F,
		Windia = 0x30,
		WindholeTemple = 0x31,
		MountGale = 0x32,
		PazuzusTower = 0x33,
		ShipDock = 0x34,
		DoomCastle = 0x35,
		LightTemple = 0x36,
		MacsShip = 0x37,
		MacsShipDoom = 0x38,
	}

	public enum MapList : byte
	{
		Overworld = 0x00,
		Unknown = 0x01,
		Foresta = 0x02,
		Aquaria = 0x03,
		Windia = 0x04,
		Fireburg = 0x05,
		HillOfDestiny = 0x06,
		LevelAliveForest = 0x07,
		WintryCave = 0x08,
		MineExterior = 0x09,
		VolcanoTop = 0x0A,
		VolcanoBase = 0x0B,
		RopeBridge = 0x0C,
		GiantTreeA = 0x0D,
		GiantTreeB = 0x0E,
		GiantTreeExterior = 0x0F,
		MountGale = 0x10,
		MacShipDeck = 0x11,
		MacShipInterior = 0x12,
		BoneDungeon = 0x13,
		IcePyramidA = 0x14,
		IcePyramidB = 0x15,
		LavaDomeExterior = 0x16,
		LavaDomeInteriorA = 0x17,
		LavaDomeInteriorB = 0x18,
		PazuzuTowerA = 0x19,
		PazuzuTowerB = 0x1A,
		SpencerCave = 0x1B,
		ShipDock = 0x1C,
		FallBasin = 0x1D,
		HouseInterior = 0x1E,
		Caves = 0x1F,
		ForestaInterior = 0x20,
		FocusTowerBase = 0x21,
		FocusTower = 0x22,
		DoomCastleIce = 0x23,
		DoomCastleLava = 0x24,
		DoomCastleSky = 0x25,
		DoomCastleHero = 0x26,
		DoomCastleEvilKing = 0x27,
		BackgroundA = 0x28,
		BackgroundB = 0x29,
		BackgroundC = 0x2A,
		BackgroundD = 0x2B,
	}

	public enum MapRegions : byte
	{
		Foresta = 0x00,
		Aquaria,
		Fireburg,
		Windia,
	}

	public enum OverworldMapSprites : byte
	{ 
		Boulder1 = 0x00,
		Boulder2,
		Boulder3,
		Boulder4,
		Boulder5,
		Boulder6,
		GiantTree1,
		GiantTree2,
		GiantTree3,
		GiantTree4,
		MovedGiantTree1,
		MovedGiantTree2,
		MovedGiantTree3,
		MovedGiantTree4,
		StrandedShip,
		ShipAtDock,
		ShipAtDoom,
		ForestaSouthBattlefield,
		ForestaWestBattlefield,
		ForestaEastBattlefield,
		AquariaBattlefield01,
		AquariaBattlefield02,
		AquariaBattlefield03,
		WintryBattlefield01,
		WintryBattlefield02,
		PyramidBattlefield01,
		LibraBattlefield01,
		LibraBattlefield02,
		FireburgBattlefield01,
		FireburgBattlefield02,
		FireburgBattlefield03,
		MineBattlefield01,
		MineBattlefield02,
		MineBattlefield03,
		VolcanoBattlefield01,
		WindiaBattlefield01,
		WindiaBattlefield02,
		ForestaSouthBattlefieldCleared,
		ForestaWestBattlefieldCleared,
		ForestaEastBattlefieldCleared,
		AquariaBattlefield01Cleared,
		AquariaBattlefield02Cleared,
		AquariaBattlefield03Cleared,
		WintryBattlefield01Cleared,
		WintryBattlefield02Cleared,
		PyramidBattlefield01Cleared,
		LibraBattlefield01Cleared,
		LibraBattlefield02Cleared,
		FireburgBattlefield01Cleared,
		FireburgBattlefield02Cleared,
		FireburgBattlefield03Cleared,
		MineBattlefield01Cleared,
		MineBattlefield02Cleared,
		MineBattlefield03Cleared,
		VolcanoBattlefield01Cleared,
		WindiaBattlefield01Cleared,
		WindiaBattlefield02Cleared,
		LevelForestMarker,
		FallBasinMarker,
		VolcanoMarker,
		RopeBridgeMarker,
		AliveForestMarker,
		MountGaleMarker,
		UnknownMarker1,
		UnknownMarker2,
		SandTempleCave,
		LibraTempleCave,
		LifeTempleCave,
		SpencerCave,
		WintryTempleCave,
		SealedTempleCave,
		KaidgeTempleCave,
		WindholeTempleCave,
		LightTempleCave,
		ShipDockCave,
		ForestaVillage1,
		ForestaVillage2,
		AquariaVillage1,
		AquariaVillage2,
		FireburgVillage1,
		FireburgVillage2,
		WindiaVillage1,
		WindiaVillage2,
		HillOfDestiny1,
		HillOfDestiny2,
		HillOfDestiny3,
		HillOfDestiny4,
		HillOfDestiny5,
		HillOfDestiny6,
		HillOfDestiny7,
		LavaDome1,
		LavaDome2,
		LavaDome3,
		LavaDome4,
		LavaDome5,
		LavaDome6,
		LavaDome7,
		MountGale1,
		MountGale2,
		MountGale3,
		MountGale4,
		MountGale5,
		MountGale6,
		MountGale7,
		BoneDungeon1,
		BoneDungeon2,
		BoneDungeon3,
		BoneDungeon4,
		BoneDungeon5,
		BoneDungeon6,
		BoneDungeon7,
		WintryCave1,
		WintryCave2,
		WintryCave3,
		WintryCave4,
		IcePyramid1,
		IcePyramid2,
		IcePyramid3,
		IcePyramid4,
		Mine1,
		Mine2,
		Mine3,
		Mine4,
		PazuzuTower1,
		PazuzuTower2,
		PazuzuTower3,
		RainbowBridgeToPazuzu1,
		RainbowBridgeToPazuzu2,
		RainbowBridgeToSpencer1,
		RainbowBridgeToSpencer2,
		RainbowBridgeToSpencer3,
		RainbowBridgeToSpencer4,
		RainbowBridgeToSpencer5,
		FocusTower1,
		FocusTower2,
		FocusTower3,
		FocusTower4,
		FocusTower5,
		FocusTower6,
		FocusTower7,
		FocusTower8,
		FocusTower9,
		FocusTower10,
	}
	public enum OverworldMapObjects : int
	{
		BoulderFull = 0,
		BoulderHalf,
		GiantTree,
		MovedGiantTree,
		StrandedShip,
		ShipAtDock,
		ShipAtDoom,
		ForestaSouthBattlefield,
		ForestaWestBattlefield,
		ForestaEastBattlefield,
		AquariaBattlefield01,
		AquariaBattlefield02,
		AquariaBattlefield03,
		WintryBattlefield01,
		WintryBattlefield02,
		PyramidBattlefield01,
		LibraBattlefield01,
		LibraBattlefield02,
		FireburgBattlefield01,
		FireburgBattlefield02,
		FireburgBattlefield03,
		MineBattlefield01,
		MineBattlefield02,
		MineBattlefield03,
		VolcanoBattlefield01,
		WindiaBattlefield01,
		WindiaBattlefield02,
		ForestaSouthBattlefieldCleared,
		ForestaWestBattlefieldCleared,
		ForestaEastBattlefieldCleared,
		AquariaBattlefield01Cleared,
		AquariaBattlefield02Cleared,
		AquariaBattlefield03Cleared,
		WintryBattlefield01Cleared,
		WintryBattlefield02Cleared,
		PyramidBattlefield01Cleared,
		LibraBattlefield01Cleared,
		LibraBattlefield02Cleared,
		FireburgBattlefield01Cleared,
		FireburgBattlefield02Cleared,
		FireburgBattlefield03Cleared,
		MineBattlefield01Cleared,
		MineBattlefield02Cleared,
		MineBattlefield03Cleared,
		VolcanoBattlefield01Cleared,
		WindiaBattlefield01Cleared,
		WindiaBattlefield02Cleared,
		LevelForestMarker,
		FallBasinMarker,
		UnknownMarker1,
		RopeBridgeMarker,
		AliveForestMarker,
		UnknownMarker2,
		VolcanoMarker,
		MountGaleMarker,
		SandTempleCave,
		LibraTempleCave,
		LifeTempleCave,
		SpencerCave,
		WintryTempleCave,
		SealedTempleCave,
		KaidgeTempleCave,
		WindholeTempleCave,
		LightTempleCave,
		ShipDockCave,
		ForestaVillage,
		AquariaVillage,
		FireburgVillage,
		WindiaVillage,
		HillOfDestiny,
		LavaDome,
		MountGale,
		BoneDungeon,
		WintryCave,
		IcePyramid,
		Mine,
		PazuzuTower,
		RainbowBridgeToPazuzu,
		RainbowBridgeToSpencer,
		FocusTower,
	}
}
