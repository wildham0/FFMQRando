using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void ProgressiveGears(bool enable)
		{
			if (!enable)
			{
				return;
			}
			
			// Replace cat claw check routine when giving weapons and jump to new routine to figure out what's the next weapon in line
			PutInBank(0x00, 0xDB9C, Blob.FromHex("22009311eaeaeaeaeaeaeaeaeaeaeaeaeaeaea"));
			PutInBank(0x11, 0x9300, Blob.FromHex("0b202093f432102b98224e9700981869208d60018d9e002b6b"));
			PutInBank(0x11, 0x9320, Blob.FromHex("c923900ac9269019c92990288039a900200094f043a901200094f03ca902a88037a903200094f030a904200094f029a905a88024a906200094f01da907200094f016a908a88011a909200094f00aa90a200094f003a90ba860"));
			PutInBank(0x11, 0x9400, Blob.FromHex("a8f432102b225a970060"));

			// New routine for armors and figure out what's the next armor in line
			PutInBank(0x00, 0xDBBE, Blob.FromHex("22809311eaeaeaeaeaeaeaeaeaea"));
			PutInBank(0x11, 0x9380, Blob.FromHex("0b20a093f435102b98224e97009818692f8d60018d9e002b6b"));
			PutInBank(0x11, 0x93A0, Blob.FromHex("c932900ac9399019c93d90288039a900201094f043a901201094f03ca902a88037a903201094f030a904201094f029a905a88024a90a201094f01da90b201094f016a90ca88011a90e201094f00aa90f201094f003a910a860"));
			PutInBank(0x11, 0x9410, Blob.FromHex("a8f435102b225a970060"));

			// Vendor Routine
			PutInBank(0x11, 0x9430, Blob.FromHex("e230ad0015c920901dc92f9005c940900c6b202093981869208d00156b20a09398692f8d00156b"));
		}
		public void SetStartingItems(ItemsPlacement itemsplacement)
		{
			byte[] itemsByte = new byte[] { 0x00, 0x00 };
			byte[] weaponsByte = new byte[] { 0x00, 0x00, 0x00 };
			byte[] armorsByte = new byte[] { 0x00, 0x00, 0x00 };
			byte[] spellsByte = new byte[] { 0x00, 0x00 };

			byte startingWeapon = 0x00;
			int startingWeaponLevel = -1;

			List<(Items item, int count)> consumables  = new() { (Items.CurePotion, 0), (Items.HealPotion, 0), (Items.Refresher, 0), (Items.Seed, 0) };

			foreach(var item in itemsplacement.StartingItems)
			{
				if (consumables.Select(x => x.item).Contains(item))
				{
					consumables = consumables.Select(x => (x.item == item) ? (x.item, x.count + 1) : x).ToList();
				}
				else if (item <= Items.SkyCoin)
				{
					int targetByte = ((int)item / 8);
					byte targetBit = (byte)(0x80 / (Math.Pow(2, (int)item % 8)));

					itemsByte[targetByte] |= targetBit;
				}
				else if (item >= Items.ExitBook && item <= Items.FlareSeal)
				{
					int targetByte = ((int)(item - Items.ExitBook) / 8);
					byte targetBit = (byte)(0x80 / (Math.Pow(2, (int)(item - Items.ExitBook) % 8)));

					spellsByte[targetByte] |= targetBit;
				}
				else if (item >= Items.SteelSword && item <= Items.NinjaStar)
				{
					int targetByte = ((int)(item - Items.SteelSword) / 8);
					byte targetBit = (byte)(0x80 / (Math.Pow(2, (int)(item - Items.SteelSword) % 8)));

					weaponsByte[targetByte] |= targetBit;

					var currentWeaponLevel = ((item - Items.SteelSword) % 3);

					if (currentWeaponLevel > startingWeaponLevel)
					{
						startingWeapon = (byte)item;
					}
				}
				else if (item >= Items.SteelHelm && item <= Items.CupidLocket)
				{
					int targetByte = ((int)(item - Items.SteelHelm) / 8);
					byte targetBit = (byte)(0x80 / (Math.Pow(2, (int)(item - Items.SteelHelm) % 8)));

					armorsByte[targetByte] |= targetBit;
				}
			}

			PutInBank(0x0C, 0xD0E1, new byte[] { startingWeapon });
			PutInBank(0x0C, 0xD0E2, weaponsByte);
			PutInBank(0x0C, 0xD0E5, armorsByte);
			PutInBank(0x0C, 0xD0E8, spellsByte);
			PutInBank(0x0C, 0xD3A2, itemsByte);

			int currentConsumableSlot = 0;

			foreach (var consumable in consumables)
			{
				if (consumable.count > 0)
				{
					PutInBank(0x0C, 0xD39A + (currentConsumableSlot * 2), new byte[] { (byte)consumable.item, (byte)consumable.count });
					currentConsumableSlot++;
				}
			}
		}
		public void SetStartingWeapons(ItemsPlacement itemsPlacement)
		{
			if (itemsPlacement.StartingItems.Contains(Items.SteelSword))
			{
				return;
			}
			else if (itemsPlacement.StartingItems.Contains(Items.Axe))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("1000"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.Axe });
			}
			else if (itemsPlacement.StartingItems.Contains(Items.CatClaw))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("0200"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.CatClaw });
			}
			else if (itemsPlacement.StartingItems.Contains(Items.Bomb))
			{
				PutInBank(0x0C, 0xD0E2, Blob.FromHex("0040"));
				PutInBank(0x0C, 0xD0E1, new byte[] { (byte)Items.Bomb });
			}
		}
	}
}
