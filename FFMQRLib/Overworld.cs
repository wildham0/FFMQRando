using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{

    public class NodeObject
    { 
        public ushort LocationAction { get; set; }
        public List<int> MapObjects { get; set; }
        public int Id { get; set; }

        public NodeObject(int id, ushort action, List<int> mapobjects)
        {
            Id = id;
            LocationAction = action;
            MapObjects = mapobjects.ToList();
        }
    }

    public partial class Overworld
    {
        private const int OWObjectBank = 0x07;
        private const int OWObjectOffset = 0xEB44;
        private const int OWObjectQty = 0x8F;

        //private const int OWLocationActionsOffset = 0xEFA1;
        //private const int OWLocationQty = 0x38;

        private List<OverworldSprite> owSprites;
        public List<OverworldObject> owObjects { get; set; }
        //private List<NodeObject> nodesObjects;


        public Overworld(FFMQRom rom, List<Room> rooms)
        {
            Rooms = rooms;
            
            owSprites = rom.GetFromBank(OWObjectBank, OWObjectOffset, 5 * OWObjectQty).Chunk(5).Select(x => new OverworldSprite(x)).ToList();
            owObjects = new();

            ConstructOwObjects();
            CreateLocations(rom);
        }
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

        public void UpdateBattlefieldsColor(Battlefields battlefields)
        {
            const byte gpColor = 3;
            const byte itemColor = 4;
            const byte xpColor = 6;

            var allBattlefields = battlefields.GetAllRewardType();

            for (int i = 0; i < allBattlefields.Count; i++)
            {
                switch (allBattlefields[i])
                {
                    case BattlefieldRewardType.Gold:
                        owSprites[i + 0x11].Palette = gpColor;
                        owSprites[i + 0x25].Palette = gpColor;
                        break;
                    case BattlefieldRewardType.Item:
                        owSprites[i + 0x11].Palette = itemColor;
                        owSprites[i + 0x25].Palette = itemColor;
                        break;
                    case BattlefieldRewardType.Experience:
                        owSprites[i + 0x11].Palette = xpColor;
                        owSprites[i + 0x25].Palette = xpColor;
                        break;
                }
            }
        }
        public void RemoveObject(int index)
        {
            owSprites[index].Data[0] = 0;
            owSprites[index].Data[1] = 0;
        }

        public void AlignObjects()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < OWObjectQty; i++)
            {
                owSprites[i].Data[0] = (byte)(15 + y);
                owSprites[i].Data[1] = (byte)(10 + x);
                owSprites[i].Data[2] = 0;
                x++;
                if (x > 20)
                {
                    x = 0;
                    y++;
                }
            }
        }
        private void UpdateSprites()
        {
            foreach (var item in owObjects)
            {
                item.UpdateCoordinates(owSprites);
            }
        }
        public void Write(FFMQRom rom)
        {
            UpdateOwObjects();
            UpdateSprites();
            WriteLocations(rom);

            rom.PutInBank(OWObjectBank, OWObjectOffset, owSprites.SelectMany(x => x.Data).ToArray());
        }
    }
    public partial class OverworldObject
    {
        public (int x, int y) Position { get; set; }
        public List<List<int>> SpriteLayout;
        public (int x, int y) AnchorPosition;
        public OverworldObject((int x, int y) position, List<List<int>> layout, (int x, int y) anchor)
        {
            SpriteLayout = layout.ToList();
            Position = position;
            AnchorPosition = anchor;
        }
        public void UpdateCoordinates(List<OverworldSprite> owSprites)
        {
            for (int y = 0; y < SpriteLayout.Count; y++)
            {
                for (int x = 0; x < SpriteLayout[y].Count; x++)
                {
                    int currentsprite = SpriteLayout[y][x];
                    if (currentsprite > -1)
                    {
                        owSprites[currentsprite].X = (byte)(Position.x + x - AnchorPosition.x);
                        owSprites[currentsprite].Y = (byte)(Position.y + y - AnchorPosition.y);
                    }
                }
            }
        }
    }

    public class OverworldSprite
    {
        private byte[] _data = new byte[5];

        public OverworldSprite(byte[] data)
        {
            _data[0] = data[0]; // Y
            _data[1] = data[1]; // X
            _data[2] = data[2]; // Gameflag
            _data[3] = data[3]; // Sprite
            _data[4] = data[4]; // palette & flip  
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
        public byte X
        {
            get => _data[1];
            set => _data[1] = value;
        }
        public byte Y
        {
            get => _data[0];
            set => _data[0] = value;
        }
        public byte[] Data
        {
            get => _data;
        }
    }
}
