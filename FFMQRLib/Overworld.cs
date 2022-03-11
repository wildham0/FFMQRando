using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{

    public class Overworld
    {
        private const int OWObjectBank = 0x07;
        private const int OWObjectOffset = 0xEB44;
        private const int OWObjectQty = 0x8F;

        private List<OverworldObject> owObjects;


        public Overworld(FFMQRom rom)
        {
            owObjects = rom.GetFromBank(OWObjectBank, OWObjectOffset, 5 * OWObjectQty).Chunk(5).Select(x => new OverworldObject(x)).ToList();
        }

        public void UpdateBattlefieldsColor(Flags flags, Battlefields battlefields)
        {
            if (!flags.ShuffleBattlefieldRewards)
            {
                return;
            }

            const byte gpColor = 3;
            const byte itemColor = 4;
            const byte xpColor = 6;

            var allBattlefields = battlefields.GetAllRewardType();

            for (int i = 0; i < allBattlefields.Count; i++)
            {
                switch (allBattlefields[i])
                {
                    case BattlefieldRewardType.Gold:
                        owObjects[i + 0x11].Palette = gpColor;
                        owObjects[i + 0x25].Palette = gpColor;
                        break;
                    case BattlefieldRewardType.Item:
                        owObjects[i + 0x11].Palette = itemColor;
                        owObjects[i + 0x25].Palette = itemColor;
                        break;
                    case BattlefieldRewardType.Experience:
                        owObjects[i + 0x11].Palette = xpColor;
                        owObjects[i + 0x25].Palette = xpColor;
                        break;
                }
            }
        }
        public void RemoveObject(int index)
        {
            owObjects[index].Data[0] = 0;
            owObjects[index].Data[1] = 0;
        }
        public void Write(FFMQRom rom)
        {
            rom.PutInBank(OWObjectBank, OWObjectOffset, owObjects.SelectMany(x => x.Data).ToArray());
        }
    }
    public class OverworldObject
    {
        private byte[] _data = new byte[5];

        public OverworldObject(byte[] data)
        {
            _data[0] = data[0];
            _data[1] = data[1];
            _data[2] = data[2];
            _data[3] = data[3];
            _data[4] = data[4];
        }

        public byte Palette {

            get => (byte)((_data[4] & 0b0000_1110) / 2);
            set => _data[4] = (byte)((_data[4] & 0b1111_0001) | ((value * 2) & 0b0000_1110));
        }
        public byte Sprite
        {

            get => _data[3];
            set => _data[3] = value;
        }
        public byte[] Data
        {
            get => _data;
        }
    }
}
