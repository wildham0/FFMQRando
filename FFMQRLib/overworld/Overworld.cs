using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public partial class Overworld
	{
		public LocationIds StartingLocation { get; set; }
		
		private const int OWObjectBank = 0x07;
		private const int OWObjectOffset = 0xEB44;
		private const int OWObjectQty = 0x8F;

		//private const int OWLocationActionsOffset = 0xEFA1;
		//private const int OWLocationQty = 0x38;

		private List<OverworldSprite> owSprites;
		public Overworld(FFMQRom rom)
		{
			owSprites = rom.GetFromBank(OWObjectBank, OWObjectOffset, 5 * OWObjectQty).Chunk(5).Select(x => new OverworldSprite(x)).ToList();
			owObjects = new();

			StartingLocation = LocationIds.LevelForest;

			ConstructOwObjects();
			CreateLocations(rom);
		}
		public Overworld()
		{
			StartingLocation = LocationIds.LevelForest;

			CreateLocations();
		}
		public void UpdateBattlefieldsColor(Flags flags, Battlefields battlefields)
		{
			if (!flags.ShuffleBattlefieldRewards && (flags.MapShuffling == MapShufflingMode.None || flags.MapShuffling == MapShufflingMode.Dungeons))
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
						owSprites[i + 0x11].Sprite = 0x60;
                        owSprites[i + 0x11].Palette = gpColor;
						owSprites[i + 0x25].Palette = gpColor;
						break;
					case BattlefieldRewardType.Item:
                        owSprites[i + 0x11].Sprite = 0x62;
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
}
