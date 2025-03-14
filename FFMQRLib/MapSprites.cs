using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;
using static System.Math;

namespace FFMQLib
{
	public enum SpriteSize
	{ 
		Tiles8,
		Tiles16
	}
	
	public class SpriteAddressor
	{ 
		public byte SpriteGraphic { get; set; }
		public int Position => GetPosition();
		public int Row { get; set; }
		public int RowOffset { get; set; }
		public SpriteSize Size { get; set; }
		private List<(int index, byte bit)> rowToIndex = new() { (0, 0xF0), (0, 0x08), (0, 0x04), (1, 0xF0), (1, 0x0F), (2, 0xF0), (0, 0x02), (2, 0x0F), (3, 0xF0), (3, 0x0F), (0, 0x01), (4, 0xF0), (4, 0x0F), (5, 0xF0) };
        private List<byte> offsetToIndex = new() { 0x88, 0x44, 0x22, 0x11 };
		private List<int> fullRows = new() { 0x01, 0x02, 0x06, 0x0A };
        public SpriteAddressor(int _position, byte _graphic, SpriteSize _size)
		{
			//Position = _position;
			Size = _size;
			SpriteGraphic = _graphic;
		}
        public SpriteAddressor(int _row, int _offset, byte _graphic, SpriteSize _size)
        {
            Row = _row;
			RowOffset = _offset;
            Size = _size;
            SpriteGraphic = _graphic;
        }
        public SpriteAddressor(int _index, int _bit, byte _graphic)
        {
			byte bitIndex = (byte)(0x80 / Math.Pow(2, _bit));
            Row = rowToIndex.FindIndex(x => x.index == _index && (x.bit & bitIndex) > 0);
            RowOffset = (_bit > 3) ? (_bit - 4) : _bit;
            Size = ((_graphic & 0x80) > 0) ? SpriteSize.Tiles8 : SpriteSize.Tiles16;
            SpriteGraphic = (byte)(_graphic & 0x7F);
        }
		/*
        public (int index, byte bit) GetPositionBytes()
		{
			return (Position / 8, (byte)(0x80 / Math.Pow(2,Position % 8)));
		}*/
        public (int index, byte bit) GetPositionBytes()
        {
			if (fullRows.Contains(Row))
			{
				var targetIndex = rowToIndex[Row];
				return (targetIndex.index, targetIndex.bit);
			}
			else
			{
                var targetIndex = rowToIndex[Row];
				return (targetIndex.index, (byte)(offsetToIndex[RowOffset] & targetIndex.bit));
            }
        }
		public int GetPosition()
		{
			var positionByte = GetPositionBytes();
			return (positionByte.index * 8) + (int)Math.Log2(0x80 / positionByte.bit);
        }
        public byte GetSprite()
		{
			return (byte)(SpriteGraphic | ((Size == SpriteSize.Tiles8) ? 0x80 : 0x00));
		}
	}
	public class MapSpriteSet
	{
		public ushort Pointer { get; set; }
		public List<SpriteAddressor> AdressorList { get; }
		public List<byte> Palette { get; set; }
		public bool LoadMonsterSprites { get; set; }
		public MapSpriteSet(ushort _pointer, int _address, FFMQRom rom)
		{
			AdressorList = new();
			Palette = rom.Get(_address + _pointer, 6).ToBytes().ToList();

			List<byte> spriteList = rom.Get(_address + _pointer + 6, 6).ToBytes().ToList();

			int spritecount = 0;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < (i < 5 ? 8 : 4); j++)
				{
					byte bitIndex = (byte)(spriteList[i] & (byte)(0x80 / Math.Pow(2, j)));

					if (bitIndex > 0)
					{
						byte targetByte = rom.Get(_address + _pointer + 12 + spritecount, 1).ToBytes()[0];
                        //AdressorList.Add(new SpriteAddressor(i * 8 + j, targetByte, (targetByte & 0x80) > 0 ? SpriteSize.Tiles8 : SpriteSize.Tiles16));
                        AdressorList.Add(new SpriteAddressor(i, j, targetByte));
                        spritecount++;
					}
				}
			}

			LoadMonsterSprites = (spriteList[5] & 0x01) > 0;
		}
		public MapSpriteSet(List<byte> palette, List<SpriteAddressor> adressors, bool loadmonsters)
		{
			LoadMonsterSprites = loadmonsters;
			Palette = palette;
			AdressorList = adressors;
		}
        public MapSpriteSet(MapSpriteSet settocopyfrom)
        {
            LoadMonsterSprites = settocopyfrom.LoadMonsterSprites;
            Palette = settocopyfrom.Palette.ToList();
            AdressorList = settocopyfrom.AdressorList.Select(a => new SpriteAddressor(a.Row, a.RowOffset, a.SpriteGraphic, a.Size)).ToList();
        }
        public List<byte> GetDataArray()
		{
			List<byte> positionList = new() { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			
			var orderedAddressors = AdressorList.OrderBy(x => x.Position).Where(x => x.Position < 44).ToList();

			foreach (var addressor in orderedAddressors)
			{
				var positionByte = addressor.GetPositionBytes();

                positionList[positionByte.index] |= positionByte.bit;
			}

			positionList[5] |= (byte)(LoadMonsterSprites ? 0x01 : 0x00);

			return Palette.Concat(positionList).Concat(orderedAddressors.Select(x => x.GetSprite()).ToList()).ToList();
		}
		public void DeleteAddressor(int position)
		{
			AdressorList.RemoveAll(x => x.Row == position);
		}
		public void MoveAddressor(int from, int to)
		{
			var targetAdressors = AdressorList.Where(x => x.Row == from).ToList();

			if (targetAdressors.Any())
			{
				targetAdressors.First().Row = to;
			}
        }
		public void AddAddressor(int row, int offset, byte sprite, SpriteSize size)
		{
			if (AdressorList.Where(x => x.Row == row).Any())
			{
				throw new Exception("Position " + row + " is already taken.");
			}

			AdressorList.Add(new SpriteAddressor(row, offset, sprite, size));
		}
	}
	public class MapSprites
	{ 
		private List<MapSpriteSet> MapSpriteSets { get; set; }
		
		private int MapSpriteSetPointersAddress = 0x8892;
		private int MapSpriteSetBaseAddress = 0x88FC;
		private int MapSpriteSetBank = 0x0B;

		private int MapSpriteSetQty = 0x35;

		private int NewMapSpriteSetPointersAddress = 0xA000;
		private int NewMapSpriteSetBaseAddress = 0xA100;
		private int NewMapSpriteSetBank = 0x11;

		public MapSprites(FFMQRom rom)
		{
			MapSpriteSets = new();
			
			int MapSpriteSetLongBaseAddress = (MapSpriteSetBank * 0x8000) + (MapSpriteSetBaseAddress - 0x8000);

			for (int i = 0; i < MapSpriteSetQty; i++)
			{
				MapSpriteSets.Add(new MapSpriteSet(rom.GetFromBank(MapSpriteSetBank, MapSpriteSetPointersAddress + (i * 2), 2).ToUShorts()[0], MapSpriteSetLongBaseAddress, rom));
			}

			var highersets = MapSpriteSets.Where(s => s.AdressorList.Where(a => a.SpriteGraphic > 0x42).Any()).ToList();
			MapSpriteSetLongBaseAddress = MapSpriteSetLongBaseAddress;
		}
        public MapSpriteSet this[int id]
        {
            get => MapSpriteSets[id];
            set => MapSpriteSets[id] = value;
        }
		public int Add(MapSpriteSet newmapspriteset)
		{
			MapSpriteSets.Add(newmapspriteset);
			return (MapSpriteSets.Count - 1);
        }
        public void Write(FFMQRom rom)
		{
			ushort currentPosition = 0;
			List<ushort> pointers = new();
			List<byte> dataSet = new();

			foreach (var mapspriteset in MapSpriteSets)
			{
				var spriteSetData = mapspriteset.GetDataArray();
				pointers.Add(currentPosition);
				dataSet.AddRange(spriteSetData);
				currentPosition += (ushort)spriteSetData.Count;
			}

			rom.PutInBank(NewMapSpriteSetBank, NewMapSpriteSetPointersAddress, Blob.FromUShorts(pointers.ToArray()));
			rom.PutInBank(NewMapSpriteSetBank, NewMapSpriteSetBaseAddress, dataSet.ToArray());

			// Update Addresses
			rom.PutInBank(0x0B, 0x8201, new byte[] { (byte)(NewMapSpriteSetPointersAddress % 0x100), (byte)(NewMapSpriteSetPointersAddress / 0x100), (byte)NewMapSpriteSetBank });

			byte[] baseAddress = new byte[] { (byte)(NewMapSpriteSetBaseAddress % 0x100), (byte)(NewMapSpriteSetBaseAddress / 0x100), (byte)NewMapSpriteSetBank };


			rom.PutInBank(0x01, 0xA45B, baseAddress);
			rom.PutInBank(0x01, 0xA507, baseAddress);
			rom.PutInBank(0x01, 0xA526, baseAddress);
			rom.PutInBank(0x01, 0xA55F, baseAddress);
		}

	}
}
