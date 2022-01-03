using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace FFMQLib
{

	public partial class FFMQRom : SnesRom
	{
		/*
		 * 
		 * 		public const int ExitTilesCoordPointers = 0x05F920;
		public const int ExitTilesCoord = 0x05FB3C;
		public const int ExitsCoord = 0x05F4A0;
		
		 */

		public class ExitList
		{
			private List<List<MapObject>> _collections = new List<List<MapObject>>();
			private List<Exit> _exits = new();
			private List<int> _exitcoordqty = new();
			private List<ExitTile> _exittiles = new();
			private Dictionary<int, int> _exitpairs = new();
			public ExitList(FFMQRom rom)
			{
				for (int i = 0; i < RomOffsets.ExitsCoordQty; i++)
				{
					_exits.Add(new Exit(i, rom));
				}

				for (int i = 0; i < RomOffsets.ExitTilesCoordPointersQty - 1; i++)
				{
					var rawlowervalue = rom.Get(RomOffsets.ExitTilesCoordPointers + i * 2, 2);
					var lowervalue = rawlowervalue[1] * 0x100 + rawlowervalue[0];
					var rawuppervalue = rom.Get(RomOffsets.ExitTilesCoordPointers + (i+1) * 2, 2);
					var uppervalue = rawuppervalue[1] * 0x100 + rawuppervalue[0];

					_exitcoordqty.Add((int)((uppervalue - lowervalue) / 3));

					for (int j = 0; j < _exitcoordqty.Last(); j++)
					{
						_exittiles.Add(new ExitTile(i, j, rom));
					}
				}

				_exitcoordqty.Add(0);
			}

			public void ExitDataDump()
			{
				foreach (var exit in _exits)
				{ 
					string myStringOutput = "Target: " + exit.TargetArea.ToString("X2") + "; X: " + exit.TargetX.ToString("X2") + "; Y: " + exit.TargetY.ToString("X2");
					Console.WriteLine(myStringOutput);
				}

				Console.WriteLine("-----------------");

				foreach (var exittile in _exittiles)
				{
					string myStringOutput = "ExitID: " + exittile.TargetExit.ToString("X2") + "; X: " + exittile.X.ToString("X2") + "; Y: " + exittile.Y.ToString("X2");
					Console.WriteLine(myStringOutput);
				}

			}

			public List<ExitTile> GetAreaExitTiles(int mapid, MapUtilities maputilities)
			{
				return _exittiles.Where(x => maputilities.AreaIdToMapId(x.Area) == mapid).ToList();
			}

			public void TileDataDump()
			{
				foreach (var exittile in _exittiles)
				{
					string myStringOutput = "Map: " + exittile.TargetExit.ToString("X2") + "; X: " + exittile.X.ToString("X2") + "; Y: " + exittile.Y.ToString("X2");
					Console.WriteLine(myStringOutput);
				}
			}
			public void WriteAll(FFMQRom rom)
			{
				foreach (var collection in _collections)
				{
					foreach (var mapobject in collection)
					{
						mapobject.Write(rom);
					}
				}
			}
			public List<MapObject> this[int floorid]
			{
				get => _collections[floorid];
				set => _collections[floorid] = value;
			}
		}

		public class Exit
		{
			private List<byte> _array = new();
			private int _id;

			public byte TargetX { get; set; }
			public byte TargetY { get; set; }
			public byte TargetArea { get; set; }

			public Exit(int id, FFMQRom rom)
			{
				_id = id;
				_array = rom.Get(RomOffsets.ExitsCoord + id * 3, 3).ToBytes().ToList();

				TargetArea = _array[0];
				TargetX = _array[1];
				TargetY = _array[2];
			}

			public void Write(FFMQRom rom)
			{
				_array[0] = TargetArea;
				_array[1] = TargetX;
				_array[2] = TargetY;

				rom.Put(RomOffsets.ExitsCoord + _id * 3, _array.ToArray());
			}
		}
		public class ExitTile
		{
			private List<byte> _array = new();
			private int _index;
			private int _floor;
			private int _pointer;

			public byte X { get; set; }
			public byte Y { get; set; }
			public byte TargetExit { get; set; }
			public byte Area => (byte)_floor;

			public ExitTile(int floor, int index, FFMQRom rom)
			{
				_index = index;
				_floor = floor;
				var rawpointer = rom.Get(RomOffsets.ExitTilesCoordPointers + _floor * 2, 2);
				_pointer = rawpointer[1] * 0x100 + rawpointer[0];

				_array = rom.Get(RomOffsets.ExitTilesCoord + _pointer + _index * 3, 3).ToBytes().ToList();

				X = _array[0];
				Y = _array[1];
				TargetExit = _array[2];
			}

			public void Write(FFMQRom rom)
			{
				_array[0] = X;
				_array[1] = Y;
				_array[2] = TargetExit;

				rom.Put(RomOffsets.ExitTilesCoord + _pointer + _index * 3, _array.ToArray());
			}
		}

	}
}
