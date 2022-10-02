using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;

namespace FFMQLib
{
	public partial class ObjectList
	{
		public void UpdateChests(Flags flags, ItemsPlacement itemsPlacement)
		{
			// Update Tristam Elixir Chest so it's taken into account
			_collections[_pointerCollectionPairs[0x16]][0x05].Type = MapObjectType.Chest;
			_collections[_pointerCollectionPairs[0x16]][0x05].Value = 0x04;
			_collections[_pointerCollectionPairs[0x16]][0x05].Gameflag = 0xAD;

			ChestList.Add((0x04, _pointerCollectionPairs[0x16], 0x05));
			
			List<Items> boxItems = new() { Items.Potion, Items.HealPotion, Items.Refresher, Items.Seed, Items.BombRefill, Items.ProjectileRefill };

			var test = itemsPlacement.ItemsLocations.Where(x => x.Type == TreasureType.Chest).ToList();

			foreach (var item in itemsPlacement.ItemsLocations.Where(x => x.Type == TreasureType.Chest || x.Type == TreasureType.Box).OrderBy(x => x.ObjectId))
			{
				byte sprite = 0x26;
					
				if (item.Type == TreasureType.Chest)
				{
					sprite = 0x24;
				}
					
				List<(int,int,int)> targetChest = ChestList.Where(x => x.Item1 == item.ObjectId).ToList();
					
				if (!targetChest.Any()) continue;

				_collections[targetChest.First().Item2][targetChest.First().Item3].Sprite = sprite;
				_collections[targetChest.First().Item2][targetChest.First().Item3].Gameflag = 0x00;
			}

			// Copy box+chest from Level Forest 2nd map to 1st map
			for (int i = 0; i < 5; i++)
			{
			_collections[0x09][0x0C + i].CopyFrom(_collections[0x0A][0x0C + i]);
			}
		}
		public void SetEnemiesDensity(Flags flags, MT19337 rng)
		{
			int density = 100;

			switch (flags.EnemiesDensity)
			{
				case EnemiesDensity.All: return;
				case EnemiesDensity.ThreeQuarter: density = 75; break;
				case EnemiesDensity.Half: density = 50; break;
				case EnemiesDensity.Quarter: density = 25; break;
				case EnemiesDensity.None: density = 0; break;
			}

			for (int i = 0; i < _collections.Count; i++)
			{
				var enemiescollection = _collections[i].Where(x => x.Type == MapObjectType.Battle).ToList();
				int totalcount = enemiescollection.Count;
				int toremove = ((100 - density) * totalcount) / 100;

				for (int j = 0; j < toremove; j++)
				{
					rng.TakeFrom(enemiescollection).Gameflag = 0xFE;
				}
			}
		}
		public void ShuffleEnemiesPosition(GameMaps maps, Flags flags, MT19337 rng)
		{
			Dictionary<MapList, List<(int, int)>> excludedCoordinates = new Dictionary<MapList, List<(int, int)>> {
				{ MapList.LevelAliveForest, new List<(int, int)> { (0x34, 0x10), (0x34, 0x0E) } },
				{ MapList.MineExterior, new List<(int, int)> { (0x0B, 0x026), (0x23, 0x2A), (0x21, 0x0F), (0x3B, 0x30) } },
				{ MapList.VolcanoBase, new List<(int, int)> { (0x0F, 0x08) } },
				{ MapList.MountGale, new List<(int, int)> { (0x18, 0x16), (0x17, 0x16), (0x1C, 0x14), (0x32, 0x23), (0x1F, 0x12), (0x27, 0x0F), (0x27, 0x0E), (0x22, 0x0A) } },
				{ MapList.MacShipDeck, new List<(int, int)> { (0x13, 0x1D) } },
				{ MapList.MacShipInterior, new List<(int, int)> { (0x12, 0x11), (0x08, 0x08), (0x11, 0x21) } },
				{ MapList.IcePyramidA, new List<(int, int)> { (0x21, 0x3D), (0x2A, 0x3C), (0x25, 0x34) } },
				{ MapList.LavaDomeInteriorA, new List<(int, int)> { (0x35, 0x14), (0x33, 0x14), (0x36, 0x0A), (0x36, 0x08), (0x34, 0x08), (0x32, 0x08), (0x29, 0x1E), (0x27, 0x1E), (0x29, 0x20), (0x29, 0x22), (0x2B, 0x22), (0x29, 0x24), (0x2B, 0x24), (0x29, 0x26), (0x19, 0x36), (0x03, 0x34), (0x05, 0x2C), (0x02, 0x23), (0x0E, 0x24), (0x28, 0x04) } },
				{ MapList.LavaDomeInteriorB, new List<(int, int)> { (0x04, 0x07), (0x0B, 0x12), (0x07, 0x14), (0x09, 0x19), (0x13, 0x14), (0x11, 0x14), (0x39, 0x05), (0x25, 0x0C), (0x23, 0x12), (0x25, 0x12), (0x2A, 0x0C), (0x2C, 0x0C), (0x38, 0x11), (0x36, 0x11), (0x2E, 0x16), (0x30, 0x16) } },
				{ MapList.PazuzuTowerA, new List<(int, int)> { (0x14, 0x0A), (0x10, 0x12), (0x10, 0x13), (0x0F, 0x16), (0x0F, 0x17), (0x12, 0x31), (0x14, 0x31), (0x2C, 0x33), (0x2A, 0x33), (0x32, 0x33), (0x34, 0x33) } },
				{ MapList.PazuzuTowerB, new List<(int, int)> { (0x2D, 0x19), (0x10, 0x2D), (0x06, 0x2E), (0x07, 0x32) } },
				{ MapList.FocusTowerBase, new List<(int, int)> { (0x2C, 0x1A), (0x25, 0x13), (0x1F, 0x13), (0x1E, 0x13), (0x1C, 0x1E), (0x1B, 0x1E), (0x1A, 0x1E), (0x19, 0x1E), (0x18, 0x1E), (0x0D, 0x12), (0x0E, 0x12), (0x0F, 0x12), (0x13, 0x13), (0x13, 0x15), (0x13, 0x17) } },
				{ MapList.DoomCastleIce, new List<(int, int)> { (0x1C, 0x24), (0x20, 0x24), (0x1C, 0x20), (0x25, 0x1A), (0x24, 0x19), (0x0F, 0x26), (0x1B, 0x0F), (0x1E, 0x16), (0x11, 0x1A), (0x0A, 0x17), (0x0B, 0x16), (0x18, 0x0B), (0x07, 0x0B) } },
				{ MapList.DoomCastleLava, new List<(int, int)> { (0x25, 0x1C), (0x25, 0x1F), (0x25, 0x21), (0x22, 0x17), (0x1B, 0x16), (0x1A, 0x1F), (0x13, 0x1F), (0x11, 0x1F), (0x10, 0x09), (0x11, 0x09), (0x0E, 0x09), (0x12, 0x11), (0x14, 0x18), (0x16, 0x18), (0x18, 0x0B), (0x0B, 0x20), (0x0B, 0x22) } },
				{ MapList.DoomCastleSky, new List<(int, int)> { (0x19, 0x22), (0x17, 0x22) } },
			};

			Dictionary<MapList, List<byte>> excludedTiles = new Dictionary<MapList, List<byte>> {
				{ MapList.GiantTreeA, new List<byte> { 0x06, 0x16 } },
				{ MapList.GiantTreeB, new List<byte> { 0x06, 0x16 } },
				{ MapList.LavaDomeExterior, new List<byte> { 0x7E, 0xFE, 0x37, 0x41, 0x19, 0x1A, 0x1B } },
				{ MapList.LavaDomeInteriorA, new List<byte> { 0x7E, 0xFE, 0x37, 0x41 } },
				{ MapList.LavaDomeInteriorB, new List<byte> { 0x49, 0x4B, 0x7D } },
				{ MapList.DoomCastleLava, new List<byte> { 0x57, 0x41 } },
				{ MapList.DoomCastleSky, new List<byte> { 0x0F } },
			};

			List<int> hookMaps = new() { (int)MapList.GiantTreeA, (int)MapList.GiantTreeB, (int)MapList.MountGale, (int)MapList.MacShipInterior, (int)MapList.PazuzuTowerA, (int)MapList.PazuzuTowerB, (int)MapList.FocusTowerBase, (int)MapList.DoomCastleIce, (int)MapList.DoomCastleLava };

			if (flags.ShuffleEnemiesPosition == false)
			{
				return;
			}

			for (int i = 0; i < _collections.Count; i++)
			{
					
				var enemiescollection = _collections[i].Where(x => x.Type == MapObjectType.Battle).ToList();
				if (!enemiescollection.Any())
				{
					continue;
				}
				int minx = Max(0, enemiescollection.Select(x => x.X).Min() - 1);
				int maxx = enemiescollection.Select(x => x.X).Max() + 1;
				int miny = Max(0, enemiescollection.Select(x => x.Y).Min() - 1);
				int maxy = enemiescollection.Select(x => x.Y).Max() + 1;

				var validLayers = enemiescollection.Select(x => x.Layer).Distinct().ToList();
				var targetmap = GetAreaMapId(i);

				var currentExcludedTiles = excludedTiles.ContainsKey((MapList)targetmap) ? excludedTiles[(MapList)targetmap] : new List<byte>();
				var currentExcludedCoordinates = excludedCoordinates.ContainsKey((MapList)targetmap) ? excludedCoordinates[(MapList)targetmap] : new List<(int,int)>();

				List <(byte, byte)> selectedPositions = _collections[i].Where(x => x.Type != MapObjectType.Battle).Select(x => (x.X, x.Y)).ToList();

				if (hookMaps.Contains(targetmap)) // Creat an exclusion zone around hooks
				{
					var hookList = _collections[i].Where(x => x.Sprite == 0x28).ToList();
					foreach (var hook in hookList)
					{
						for (int j = Max(hook.X - 5, miny); j <= Min(hook.X + 5, maxx); j++)
						{
							selectedPositions.Add(((byte)j, (byte)hook.Y));
						}

						for (int j = Max(hook.Y - 5, miny); j <= Min(hook.Y + 5, maxy); j++)
						{
							selectedPositions.Add(((byte)hook.X, (byte)j));
						}
					}
				}

				// Worm Party
				if (i == _pointerCollectionPairs[0x48] && rng.Between(1,20) == 10)
				{
					validLayers = new() { 0x02 };
				}

				foreach (var enemy in enemiescollection)
				{
					bool placed = false;
					while (!placed)
					{
						byte newx = (byte)rng.Between(minx, maxx);
						byte newy = (byte)rng.Between(miny, maxy);
						if (!selectedPositions.Contains((newx, newy))
							&& !currentExcludedCoordinates.Contains((newx, newy))
							&& validLayers.Contains(maps[targetmap].WalkableByte((int)newx, (int)newy))
							&& !maps[targetmap].IsScriptTile((int)newx, (int)newy)
							&& !currentExcludedTiles.Contains(maps[targetmap].TileValue((int)newx, (int)newy)))
						{
							selectedPositions.Add((newx, newy));
							enemy.X = newx;
							enemy.Y = newy;
							enemy.Layer = maps[targetmap].WalkableByte((int)newx, (int)newy);
							placed = true;
						}
					}
				}
			}
		}
	}
}

