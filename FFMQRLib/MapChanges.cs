using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using System.Diagnostics;
using static System.Math;
using System.Xml.Linq;
using System.Text.Json;
using System.IO;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using System.Drawing;

namespace FFMQLib
{
	public class MapChangeAction
	{
		public byte Area {get; set;}
		private byte gameflag;
		private byte action;
		private byte changeid;

		public MapChangeAction(int id, byte[] initialarray)
		{
			Area = (byte)id;
			gameflag = initialarray[0];
			changeid = initialarray[1];
			action = initialarray[2];
		}
		public MapChangeAction(int id, byte _gameflag, byte _changeid, byte _action)
		{
			Area = (byte)id;
			gameflag = _gameflag;
			changeid = _changeid;
			action = _action;
		}
		public byte[] GetBytes()
		{
			return new byte[] { gameflag, changeid, action };
		}
	}

	public class MapChange
	{
		public byte X { get; set; }
		public byte Y { get; set; }
		public (int x, int y) Size { get; set; }
		public byte[,] Change { get; set; }

		public MapChange(int bank, int offset, FFMQRom rom)
		{
			var parameters = rom.GetFromBank(bank, offset, 3);
			int length = (parameters[2] & 0x0F) * (parameters[2] / 0x10); ;
			var data = rom.GetFromBank(bank, offset, length + 3);
			ReadData(data);
		}
		public MapChange(byte[] data)
		{
			ReadData(data);
		}
		public MapChange(byte posx, byte posy, int sizex, int sizey, byte[] mapdata)
		{
			X = posx;
			Y = posy;
			Size = (sizex, sizey);
			Change = ConvertMapData(mapdata);
		}
		public MapChange(byte posx, byte posy, int sizex, int sizey, byte[,] mapdata)
		{
			X = posx;
			Y = posy;
			Size = (sizex, sizey);
			Change = mapdata;
		}
		private void ReadData(byte[] data)
		{
			X = data[0];
			Y = data[1];
			Size = (data[2] / 0x10, data[2] & 0x0F);

			Change = new byte[Size.x, Size.y];

			for (int y = 0; y < Size.y; y++)
			{
				for (int x = 0; x < Size.x; x++)
				{
					Change[x, y] = data[(y * Size.x) + x + 3];
				}
			}
		}
		private byte[,] ConvertMapData(byte[] mapdata)
		{
			var tempmap = new byte[Size.x, Size.y];
			
			for (int y = 0; y < Size.y; y++)
			{
				for (int x = 0; x < Size.x; x++)
				{
					tempmap[x, y] = mapdata[(y * Size.x) + x];
				}
			}

			return tempmap;
		}
		private byte[] ConvertMapData(byte[,] mapdata)
		{
			var tempmap = new byte[Size.x * Size.y];

			for (int y = 0; y < Size.y; y++)
			{
				for (int x = 0; x < Size.x; x++)
				{
					tempmap[(y * Size.x) + x] = mapdata[x, y];
				}
			}

			return tempmap;
		}
		public byte[] ToBytes()
		{
			byte sizebyte = (byte)((Size.x * 0x0F) * 0x10 + Size.y & 0x0F);
			byte[] changedata = new byte[(Size.y * Size.x)];

			for (int y = 0; y < Size.y; y++)
			{
				for (int x = 0; x < Size.x; x++)
				{
					changedata[(y * Size.x) + x] =  Change[x, y];
				}
			}

			return (new byte[] { X, Y, sizebyte }).Concat(changedata).ToArray();
		}
	}

	public class MapChanges
	{
		private List<Blob> _pointers;
		private List<Blob> _mapchanges;
		private List<MapChangeAction> MapChangeActions;
		private List<MapChange> mapChanges;

		private const int MapChangesPointersOld = 0xB93A;
		private const int MapChangesEntriesOld = 0xBA0E;
		private const int MapChangesBankOld = 0x06;
		private const int MapChangesQtyOld = 0x6A;
		private const int MapChangesPointersNew = 0x8000;
		private const int MapChangesEntriesNew = 0x8100;
		private const int MapChangesBankNew = 0x12;
		private const int MapChangesQtyNew = 0x80;

		private const int MapActionsInitialPointers = 0xBE77;
		private const int MapActionsSecondaryPointers = 0xBEE3;
		private const int MapActionsOffset = 0xBF15;
		private const int MapActionsNewPointers = 0x9400;
		private const int MapActionsNewOffset = 0x94D8;
		private const int MapActionsQty = 0x6C;


		public MapChanges(FFMQRom rom)
		{
			_pointers = rom.GetFromBank(MapChangesBankOld, MapChangesPointersOld, MapChangesQtyOld * 2).Chunk(2);
			_mapchanges = new List<Blob>();
			mapChanges = new();

			foreach (var pointer in _pointers)
			{
				var test = pointer.ToUShorts()[0];
				mapChanges.Add(new MapChange(MapChangesBankOld, MapChangesEntriesOld + pointer.ToUShorts()[0], rom));
				/*
				var sizeByte = rom.GetFromBank(MapChangesBankOld, MapChangesEntriesOld + pointer.ToUShorts()[0] + 2, 1)[0];
				var size = (sizeByte & 0x0F) * (sizeByte / 0x10);
				_mapchanges.Add(rom.GetFromBank(MapChangesBankOld, MapChangesEntriesOld + pointer.ToUShorts()[0], size + 3));*/
			}

			MapChangeActions = new();
			var actionInitialPointers = rom.GetFromBank(MapChangesBankOld, MapActionsInitialPointers, MapActionsQty);

			for (int i = 0; i < actionInitialPointers.Length; i++)
			{
				byte individualPointer = actionInitialPointers[i];
				if (individualPointer != 0xFF)
				{
					var actualPointer = rom.GetFromBank(MapChangesBankOld, MapActionsSecondaryPointers + (individualPointer * 2), 2).ToUShorts()[0];

					var action = rom.GetFromBank(MapChangesBankOld, MapActionsOffset + actualPointer, 3);

					while (action[0] != 0xFF)
					{
						MapChangeActions.Add(new MapChangeAction(i, action));
						actualPointer += 3;
						action = rom.GetFromBank(MapChangesBankOld, MapActionsOffset + actualPointer, 3);
					}
				}
			}
		}
		/*
		public byte Add(Blob mapchange)
		{
			if (_mapchanges.Count() >= MapChangesQtyNew)
			{
				throw new Exception("Too many map changes.");
			}

			var newpointer = _pointers.Last().ToUShorts()[0] + _mapchanges.Last().Length;
			_pointers.Add(new byte[] { (byte)(newpointer % 0x100), (byte)(newpointer / 0x100) });
			_mapchanges.Add(mapchange);
			return (byte)(_mapchanges.Count() - 1);
		}*/
		public byte Add(MapChange mapchange)
		{
			if (mapChanges.Count() >= MapChangesQtyNew)
			{
				throw new Exception("Too many map changes.");
			}

			//var newpointer = _pointers.Last().ToUShorts()[0] + _mapchanges.Last().Length;
			//_pointers.Add(new byte[] { (byte)(newpointer % 0x100), (byte)(newpointer / 0x100) });
			mapChanges.Add(mapchange);
			return (byte)(mapChanges.Count() - 1);
		}
		/*
		public void Modify(int index, int address, byte modification)
		{
			_mapchanges[index][address] = modification;
		}*/
		public void Modify(int index, int x, int y, byte modification)
		{
			mapChanges[index].Change[x,y] = modification;
		}
		/*
		public void Modify(int index, int address, List<byte> modifications)
        {
			for (int i = 0; i < modifications.Count; i++)
			{
                _mapchanges[index][address + i] = modifications[i];
            }
        }*/
		public void Modify(int index, int posx, int posy, byte[,] modifications)
		{
			Array.Copy(modifications, 0, mapChanges[index].Change, posy * mapChanges[index].Size.x + posx, modifications.GetLength(0));
		}
		public void Replace(int index, Blob mapchange)
		{
			_mapchanges[index] = mapchange;
		}
		public void Replace(int index, MapChange mapchange)
		{
			mapChanges[index] = mapchange;
		}

		public void AddAction(int area, byte _gameflag, byte _changeid, byte _action)
		{
			MapChangeActions.Add(new MapChangeAction(area, new byte[] { _gameflag, _changeid, _action }));
		}
		public void RemoveActionByFlag(int area, int flag)
		{
			MapChangeActions.RemoveAll(x => x.Area == area && x.GetBytes()[0] == flag);
		}
		private void UpdatePointers()
		{
			_pointers.Clear();
			ushort currentpointer = 0x0000;

			foreach (var change in mapChanges)
			{
				_pointers.Add(new byte[] { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) });
				currentpointer += (ushort)change.ToBytes().Length;
			}
		}
		public void ReorderOwMapchanges()
		{
			var inst25 = new MapChangeAction(0, 0x04, 0x00, 0x25);
			var inst22 = new MapChangeAction(0, 0x22, 0x00, 0x22);

			MapChangeActions[4] = inst22;
			MapChangeActions[5] = inst25;
		}
		private void UpdateMapChangeActions(FFMQRom rom)
		{
			ushort currentPointer = 0x0000;
			List<ushort> pointerList = new();
			List<byte> actionData = new();

			for (int i = 0; i < MapActionsQty; i++)
			{
				pointerList.Add(currentPointer);

				var currentChanges = MapChangeActions.Where(x => x.Area == i).ToList();
				foreach (var change in currentChanges)
				{
					actionData.AddRange(change.GetBytes());
					currentPointer += 3;
				}
				actionData.Add(0xFF);
				currentPointer++;
			}

			rom.PutInBank(MapChangesBankNew, MapActionsNewPointers, Blob.FromUShorts(pointerList.ToArray()));
			rom.PutInBank(MapChangesBankNew, MapActionsNewOffset, actionData.ToArray());
		}
		public void Write(FFMQRom rom)
		{
			UpdatePointers();
			UpdateMapChangeActions(rom);

			rom.PutInBank(MapChangesBankNew, MapChangesPointersNew, _pointers.SelectMany(x => x.ToBytes()).ToArray());
			rom.PutInBank(MapChangesBankNew, MapChangesEntriesNew, mapChanges.SelectMany(x => x.ToBytes()).ToArray());

			// Change LoadMapChange routine
			rom.PutInBank(0x01, 0xC593, Blob.FromHex("008012")); // Change pointers table address
			rom.PutInBank(0x01, 0xC5A0, Blob.FromHex("018112")); // Change Y base
			rom.PutInBank(0x01, 0xC5B6, Blob.FromHex("008112")); // Change X base
			rom.PutInBank(0x01, 0xC5CB, Blob.FromHex("028112")); // Change Size base
			rom.PutInBank(0x01, 0xC5EB, Blob.FromHex("008112")); // Change Entry base

			// Change MapAction routine
			rom.PutInBank(0x01, 0xC8B2, Blob.FromHex("EAEAEAEAEAEAEA")); // skip initial table check
			rom.PutInBank(0x01, 0xC8BF, new byte[] { (MapActionsNewPointers % 0x100), (MapActionsNewPointers / 0x100), MapChangesBankNew }); // new pointers address
			rom.PutInBank(0x01, 0xC8C5, new byte[] { (MapActionsNewOffset % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
			rom.PutInBank(0x01, 0xC8D3, new byte[] { ((MapActionsNewOffset + 1) % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
			rom.PutInBank(0x01, 0xC8DA, new byte[] { ((MapActionsNewOffset + 2) % 0x100), (MapActionsNewOffset / 0x100), MapChangesBankNew }); // new offsets
		}
	}
}
