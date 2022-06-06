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
        public int Position { get; set; }
        public SpriteSize Size { get; set; }

        public SpriteAddressor(int _position, byte _graphic, SpriteSize _size)
        {
            Position = _position;
            Size = _size;
            SpriteGraphic = _graphic;
        }
        public (int index, byte bit) GetPositionBytes()
        {
            return (Position / 8, (byte)(0x80 / Math.Pow(2,Position % 8)));
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
                    if ((spriteList[i] & (byte)(0x80 / Math.Pow(2, j))) > 0)
                    {
                        byte targetByte = rom.Get(_address + _pointer + 12 + spritecount, 1).ToBytes()[0];
                        AdressorList.Add(new SpriteAddressor(i * 8 + j, targetByte, (targetByte & 0x80) > 0 ? SpriteSize.Tiles8 : SpriteSize.Tiles16));
                        spritecount++;
                    }
                }
            }

            LoadMonsterSprites = (spriteList[5] & 0x01) > 0;
        }
        public List<byte> GetDataArray()
        {
            List<byte> positionList = new() { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            
            var orderedAddressors = AdressorList.OrderBy(x => x.Position).Where(x => x.Position < 44).ToList();

            foreach (var addressor in orderedAddressors)
            {
                positionList[addressor.GetPositionBytes().index] |= addressor.GetPositionBytes().bit;
            }

            positionList[5] |= (byte)(LoadMonsterSprites ? 0x01 : 0x00);

            return Palette.Concat(positionList).Concat(orderedAddressors.Select(x => x.GetSprite()).ToList()).ToList();
        }
        public void DeleteAddressor(int position)
        {
            AdressorList.RemoveAll(x => x.Position == position);
        }
        public void AddAddressor(int position, byte sprite, SpriteSize size)
        {
            if (AdressorList.Where(x => x.Position == position).Any())
            {
                throw new Exception("Position " + position + " is already taken.");
            }

            AdressorList.Add(new SpriteAddressor(position, sprite, size));
        }
    }
    public class MapSprites
    { 
        public List<MapSpriteSet> MapSpriteSets { get; set; }
        
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
