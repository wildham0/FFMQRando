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

            PutInBank(0x0C, 0xD4CD, new byte[] { (byte)itemsPlacement.StartingItems.Count });
        }
	}
}
