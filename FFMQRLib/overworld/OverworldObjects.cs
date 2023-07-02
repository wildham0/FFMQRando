using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public partial class Overworld
	{
		public List<OverworldObject> owObjects { get; set; }
		public void ConstructOwObjects()
		{
			owObjects.Add(new OverworldObject((0, 0),
				new List<List<int>>
				{
					new List<int> { 0, 1 },
					new List<int> { 2, 3 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0, 0),
				new List<List<int>>
				{
					new List<int> { 4, 5 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x2C, 0x23), // Giant Tree
				new List<List<int>>
				{
					new List<int> { 6, 8 },
					new List<int> { 7, 9 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x28, 0x26),
				new List<List<int>>
				{
					new List<int> { 10, 12 },
					new List<int> { 11, 13 },
				},
				(0, 1)));
			owObjects.Add(new OverworldObject((0x2E, 0x1A), // Ships
				new List<List<int>>
				{
					new List<int> { 14 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x31, 0x1C), // Ships
				new List<List<int>>
				{
					new List<int> { 15 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x1C, 0x28), // Ships
				new List<List<int>>
				{
					new List<int> { 16 },
				},
				(0, 0)));

			for (int i = (int)OverworldMapSprites.ForestaSouthBattlefield; i <= (int)OverworldMapSprites.ShipDockCave; i++)
			{
				owObjects.Add(new OverworldObject((owSprites[i].X, owSprites[i].Y),
					new List<List<int>>
					{
						new List<int> { i },
					},
					(0, 0)));
			}

			for (int i = (int)OverworldMapSprites.ForestaVillage1; i <= (int)OverworldMapSprites.WindiaVillage2; i+=2)
			{
				owObjects.Add(new OverworldObject((owSprites[i].X, owSprites[i].Y),
					new List<List<int>>
					{
						new List<int> { i, i + 1 },
					},
					((i == (int)OverworldMapSprites.FireburgVillage1) ? 1 : 0, 0)));
			}

			owObjects.Add(new OverworldObject((0x19, 0x2A), // Hill of Destiny
				new List<List<int>>
				{
					new List<int> { -1, 83, -1 },
					new List<int> { 84, 85, 86 },
					new List<int> { 87, 88, 89 },
				},
				(1, 2)));

			owObjects.Add(new OverworldObject((0x19, 0x05), // Lava Dome
				new List<List<int>>
				{
					new List<int> { -1, 90, -1 },
					new List<int> { 91, 92, 93 },
					new List<int> { 94, 95, 96 },
				},
				(1, 0)));
			owObjects.Add(new OverworldObject((0x3C, 0x22), // Mount Gale
				new List<List<int>>
				{
					new List<int> {  -1,  97,  -1 },
					new List<int> {  98,  99, 100 },
					new List<int> { 101, 102, 103 },
				},
				(1, 2)));
			owObjects.Add(new OverworldObject((0x0E, 0x1A), // Bone Dungeon
				new List<List<int>>
				{
					new List<int> {  -1, 104, 106,  -1 },
					new List<int> {  -1, 105, 107,  -1 },
					new List<int> { 108,  -1,  -1, 110 },
					new List<int> {  -1,  -1, 109,  -1 },
				},
				(1, 1)));
			owObjects.Add(new OverworldObject((0x37, 0x0B), // Wintry Cave
				new List<List<int>>
				{
					new List<int> { 111, 113 },
					new List<int> { 112, 114 },
				},
				(1, 1)));
			owObjects.Add(new OverworldObject((0x2D, 0x06), // Ice Pyramid
				new List<List<int>>
				{
					new List<int> { 115, 117 },
					new List<int> { 116, 118 },
				},
				(1, 1)));
			owObjects.Add(new OverworldObject((0x09, 0x08), // Mine
				new List<List<int>>
				{
					new List<int> { 119, 121 },
					new List<int> { 120, 122 },
				},
				(1, 1)));
			owObjects.Add(new OverworldObject((0x35, 0x1E), // Pazuzu's Tower
				new List<List<int>>
				{
					new List<int> { 123 },
					new List<int> { 124 },
					new List<int> { 125 },
				},
				(0, 2)));
			owObjects.Add(new OverworldObject((0x35, 0x20), // Rainbow Bridge A
				new List<List<int>>
				{
					new List<int> { 126 },
					new List<int> { 127 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x34, 0x19), // Rainbow Bridge B
				new List<List<int>>
				{
					new List<int> { 128 },
					new List<int> { 129 },
					new List<int> { 130 },
					new List<int> { 131 },
					new List<int> { 132 },
				},
				(0, 0)));
			owObjects.Add(new OverworldObject((0x1D, 0x19), // Focus Tower
				new List<List<int>>
				{
					new List<int> { 133, 134 },
					new List<int> { 135, 136 },
					new List<int> { 137, 138 },
					new List<int> { 139, 140 },
					new List<int> { 141, 142 },
				},
				(0, 0)));
		}
        private void AssignOwObjects()
        {
            Locations[(int)LocationIds.ForestaSouthBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaSouthBattlefield, OverworldMapObjects.ForestaSouthBattlefieldCleared };
            Locations[(int)LocationIds.ForestaSouthBattlefield].TargetTeleporter = (115, 1);

            Locations[(int)LocationIds.ForestaWestBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaWestBattlefield, OverworldMapObjects.ForestaWestBattlefieldCleared };
            Locations[(int)LocationIds.ForestaWestBattlefield].TargetTeleporter = (116, 1);

            Locations[(int)LocationIds.ForestaEastBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaEastBattlefield, OverworldMapObjects.ForestaEastBattlefieldCleared };
            Locations[(int)LocationIds.ForestaEastBattlefield].TargetTeleporter = (117, 1);

            Locations[(int)LocationIds.AquariaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield01, OverworldMapObjects.AquariaBattlefield01Cleared };
            Locations[(int)LocationIds.AquariaBattlefield01].TargetTeleporter = (118, 1);

            Locations[(int)LocationIds.AquariaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield02, OverworldMapObjects.AquariaBattlefield02Cleared };
            Locations[(int)LocationIds.AquariaBattlefield02].TargetTeleporter = (119, 1);

            Locations[(int)LocationIds.AquariaBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield03, OverworldMapObjects.AquariaBattlefield03Cleared };
            Locations[(int)LocationIds.AquariaBattlefield03].TargetTeleporter = (120, 1);

            Locations[(int)LocationIds.WintryBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield01, OverworldMapObjects.WintryBattlefield01Cleared };
            Locations[(int)LocationIds.WintryBattlefield01].TargetTeleporter = (121, 1);

            Locations[(int)LocationIds.WintryBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield02, OverworldMapObjects.WintryBattlefield02Cleared };
            Locations[(int)LocationIds.WintryBattlefield02].TargetTeleporter = (122, 1);

            Locations[(int)LocationIds.PyramidBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PyramidBattlefield01, OverworldMapObjects.PyramidBattlefield01Cleared };
            Locations[(int)LocationIds.PyramidBattlefield01].TargetTeleporter = (123, 1);

            Locations[(int)LocationIds.LibraBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield01, OverworldMapObjects.LibraBattlefield01Cleared };
            Locations[(int)LocationIds.LibraBattlefield01].TargetTeleporter = (124, 1);

            Locations[(int)LocationIds.LibraBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield02, OverworldMapObjects.LibraBattlefield02Cleared };
            Locations[(int)LocationIds.LibraBattlefield02].TargetTeleporter = (125, 1);

            Locations[(int)LocationIds.FireburgBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield01, OverworldMapObjects.FireburgBattlefield01Cleared };
            Locations[(int)LocationIds.FireburgBattlefield01].TargetTeleporter = (126, 1);

            Locations[(int)LocationIds.FireburgBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield02, OverworldMapObjects.FireburgBattlefield02Cleared };
            Locations[(int)LocationIds.FireburgBattlefield02].TargetTeleporter = (127, 1);

            Locations[(int)LocationIds.FireburgBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield03, OverworldMapObjects.FireburgBattlefield03Cleared };
            Locations[(int)LocationIds.FireburgBattlefield03].TargetTeleporter = (128, 1);

            Locations[(int)LocationIds.MineBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield01, OverworldMapObjects.MineBattlefield01Cleared };
            Locations[(int)LocationIds.MineBattlefield01].TargetTeleporter = (129, 1);

            Locations[(int)LocationIds.MineBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield02, OverworldMapObjects.MineBattlefield02Cleared };
            Locations[(int)LocationIds.MineBattlefield02].TargetTeleporter = (130, 1);

            Locations[(int)LocationIds.MineBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield03, OverworldMapObjects.MineBattlefield03Cleared };
            Locations[(int)LocationIds.MineBattlefield03].TargetTeleporter = (131, 1);

            Locations[(int)LocationIds.VolcanoBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoBattlefield01, OverworldMapObjects.VolcanoBattlefield01Cleared };
            Locations[(int)LocationIds.VolcanoBattlefield01].TargetTeleporter = (132, 1);

            Locations[(int)LocationIds.WindiaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield01, OverworldMapObjects.WindiaBattlefield01Cleared };
            Locations[(int)LocationIds.WindiaBattlefield01].TargetTeleporter = (133, 1);

            Locations[(int)LocationIds.WindiaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield02, OverworldMapObjects.WindiaBattlefield02Cleared };
            Locations[(int)LocationIds.WindiaBattlefield02].TargetTeleporter = (134, 1);

            Locations[(int)LocationIds.HillOfDestiny].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.HillOfDestiny };

            Locations[(int)LocationIds.LevelForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LevelForestMarker };
            Locations[(int)LocationIds.LevelForest].TargetTeleporter = (25, 0);

            Locations[(int)LocationIds.Foresta].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaVillage };
            Locations[(int)LocationIds.Foresta].TargetTeleporter = (31, 0);

            Locations[(int)LocationIds.SandTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SandTempleCave };
            Locations[(int)LocationIds.SandTemple].TargetTeleporter = (36, 0);

            Locations[(int)LocationIds.BoneDungeon].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.BoneDungeon };
            Locations[(int)LocationIds.BoneDungeon].TargetTeleporter = (37, 0);

            Locations[(int)LocationIds.FocusTowerForesta].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.FocusTowerForesta].TargetTeleporter = (2, 6);

            Locations[(int)LocationIds.FocusTowerAquaria].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.FocusTowerAquaria].TargetTeleporter = (4, 6);

            Locations[(int)LocationIds.LibraTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraTempleCave };
            Locations[(int)LocationIds.LibraTemple].TargetTeleporter = (13, 6);

            Locations[(int)LocationIds.Aquaria].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaVillage };
            Locations[(int)LocationIds.Aquaria].TargetTeleporter = (8, 6);

            Locations[(int)LocationIds.WintryCave].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryCave };
            Locations[(int)LocationIds.WintryCave].TargetTeleporter = (49, 0);

            Locations[(int)LocationIds.LifeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LifeTempleCave };
            Locations[(int)LocationIds.LifeTemple].TargetTeleporter = (14, 6);

            Locations[(int)LocationIds.FallsBasin].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FallBasinMarker };
            Locations[(int)LocationIds.FallsBasin].TargetTeleporter = (53, 0);

            Locations[(int)LocationIds.IcePyramid].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.IcePyramid };
            Locations[(int)LocationIds.IcePyramid].TargetTeleporter = (56, 0);

            Locations[(int)LocationIds.SpencersPlace].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SpencerCave };
            Locations[(int)LocationIds.SpencersPlace].TargetTeleporter = (7, 6);

            Locations[(int)LocationIds.WintryTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryTempleCave };
            Locations[(int)LocationIds.WintryTemple].TargetTeleporter = (15, 6);

            Locations[(int)LocationIds.FocusTowerFrozen].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.FocusTowerFrozen].TargetTeleporter = (5, 6);

            Locations[(int)LocationIds.FocusTowerFireburg].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.FocusTowerFireburg].TargetTeleporter = (6, 6);

            Locations[(int)LocationIds.Fireburg].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgVillage };
            Locations[(int)LocationIds.Fireburg].TargetTeleporter = (9, 6);

            Locations[(int)LocationIds.Mine].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.Mine };
            Locations[(int)LocationIds.Mine].TargetTeleporter = (98, 0);

            Locations[(int)LocationIds.SealedTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SealedTempleCave };
            Locations[(int)LocationIds.SealedTemple].TargetTeleporter = (16, 6);

            Locations[(int)LocationIds.Volcano].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoMarker };
            Locations[(int)LocationIds.Volcano].TargetTeleporter = (103, 0);

            Locations[(int)LocationIds.LavaDome].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LavaDome };
            Locations[(int)LocationIds.LavaDome].TargetTeleporter = (104, 0);

            Locations[(int)LocationIds.FocusTowerWindia].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.FocusTowerWindia].TargetTeleporter = (3, 6);

            Locations[(int)LocationIds.RopeBridge].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.RopeBridgeMarker };
            Locations[(int)LocationIds.RopeBridge].TargetTeleporter = (140, 0);

            Locations[(int)LocationIds.AliveForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AliveForestMarker };
            Locations[(int)LocationIds.AliveForest].TargetTeleporter = (142, 0);

            Locations[(int)LocationIds.GiantTree].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MovedGiantTree };
            Locations[(int)LocationIds.GiantTree].TargetTeleporter = (49, 8);

            Locations[(int)LocationIds.KaidgeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.KaidgeTempleCave };
            Locations[(int)LocationIds.KaidgeTemple].TargetTeleporter = (18, 6);

            Locations[(int)LocationIds.Windia].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaVillage };
            Locations[(int)LocationIds.Windia].TargetTeleporter = (10, 6);

            Locations[(int)LocationIds.WindholeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindholeTempleCave };
            Locations[(int)LocationIds.WindholeTemple].TargetTeleporter = (173, 0);

            Locations[(int)LocationIds.MountGale].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MountGale, OverworldMapObjects.MountGaleMarker };
            Locations[(int)LocationIds.MountGale].TargetTeleporter = (174, 0);

            Locations[(int)LocationIds.PazuzusTower].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PazuzuTower };
            Locations[(int)LocationIds.PazuzusTower].TargetTeleporter = (184, 0);

            Locations[(int)LocationIds.ShipDock].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipDockCave };
            Locations[(int)LocationIds.ShipDock].TargetTeleporter = (17, 6);

            Locations[(int)LocationIds.DoomCastle].OwMapObjects = new List<OverworldMapObjects> { };
            Locations[(int)LocationIds.DoomCastle].TargetTeleporter = (1, 6);

            Locations[(int)LocationIds.LightTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LightTempleCave };
            Locations[(int)LocationIds.LightTemple].TargetTeleporter = (19, 6);

            Locations[(int)LocationIds.MacsShip].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDock };
            Locations[(int)LocationIds.MacsShip].TargetTeleporter = (37, 8);

            Locations[(int)LocationIds.MacsShipDoom].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDoom };
            Locations[(int)LocationIds.MacsShipDoom].TargetTeleporter = (37, 8);

            Locations.ForEach(n => n.Region = AccessReferences.Regions.Find(r => r.Item2 == n.LocationId).Item1);
            Locations.ForEach(n => n.SubRegion = AccessReferences.MapSubRegions.Find(r => r.Item2 == n.LocationId).Item1);
        }
    }
}
