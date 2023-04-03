﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public partial class AccessReferences
	{
		public static Dictionary<LocationIds, List<AccessReqs>> FriendlyAccessReqs => new Dictionary<LocationIds, List<AccessReqs>>
		{
			{ LocationIds.BoneDungeon, new List<AccessReqs> { AccessReqs.Bomb } },
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
			(MapRegions.Windia, LocationIds.MacsShip),
			(MapRegions.Windia, LocationIds.MacsShipDoom)
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
			(SubRegions.MacShip, LocationIds.MacsShip),
			(SubRegions.MacShip, LocationIds.MacsShipDoom)
		};
		public static List<(SubRegions, List<List<AccessReqs>>)> SubRegionsAccess => new()
		{
			(SubRegions.Foresta, new List<List<AccessReqs>> { new List<AccessReqs> { } }),
			(SubRegions.Aquaria, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra, AccessReqs.SummerAquaria },
			}),
			(SubRegions.LifeTemple, new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.Barred },
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
			{ Items.WakeWater, new List<AccessReqs> { AccessReqs.WakeWater } },
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
		public static List<AccessReqs> CrestsAccess = new()
		{
			AccessReqs.LibraCrest,
			AccessReqs.GeminiCrest,
			AccessReqs.MobiusCrest,
		};
		public static List<AccessReqs> FavoredCompanionsAccess = new()
		{
			AccessReqs.Tristam,
			AccessReqs.Phoebe1,
			AccessReqs.Reuben1,
		};
		public static List<LocationIds> StartingWeaponAccess = new()
		{
			LocationIds.LevelForest,
			LocationIds.Foresta,
			LocationIds.BoneDungeon,
			LocationIds.IcePyramid,
			LocationIds.Volcano,
			LocationIds.AliveForest,
			LocationIds.Aquaria,
			LocationIds.Fireburg,
			LocationIds.Windia
		};
		public static List<(LocationIds, int)> LocationsByEntrances = new()
		{
			(LocationIds.LevelForest, 445),
			(LocationIds.Foresta, 446),
			(LocationIds.SandTemple, 447),
			(LocationIds.BoneDungeon, 448),
			(LocationIds.FocusTowerForesta, 449),
			(LocationIds.FocusTowerAquaria, 450),
			(LocationIds.LibraTemple, 451),
			(LocationIds.Aquaria, 452),
			(LocationIds.WintryCave, 453),
			(LocationIds.LifeTemple, 454),
			(LocationIds.FallsBasin, 455),
			(LocationIds.IcePyramid, 456),
			(LocationIds.SpencersPlace, 457),
			(LocationIds.WintryTemple, 458),
			(LocationIds.FocusTowerFrozen, 459),
			(LocationIds.FocusTowerFireburg, 460),
			(LocationIds.Fireburg, 461),
			(LocationIds.Mine, 462),
			(LocationIds.SealedTemple, 463),
			(LocationIds.Volcano, 464),
			(LocationIds.LavaDome, 465),
			(LocationIds.FocusTowerWindia, 466),
			(LocationIds.RopeBridge, 467),
			(LocationIds.AliveForest, 468),
			(LocationIds.GiantTree, 469),
			(LocationIds.KaidgeTemple, 470),
			(LocationIds.Windia, 471),
			(LocationIds.WindholeTemple, 472),
			(LocationIds.MountGale, 473),
			(LocationIds.PazuzusTower, 474),
			(LocationIds.ShipDock, 475),
			(LocationIds.DoomCastle, 476),
			(LocationIds.LightTemple, 477),
			(LocationIds.MacsShip, 478),
			(LocationIds.MacsShipDoom, 479),
		};
	}
}
