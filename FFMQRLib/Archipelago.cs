using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void ArchipelagoSupport()
		{
			ItemFetcher();
            APItem();
        }
		public void ItemFetcher()
		{
			PutInBank(0x01, 0x82A9, Blob.FromHex("22008015eaea"));
            PutInBank(0x15, 0x8000, Blob.FromHex("eef7199cf81908e220add00ff00fa9018db019a9508dee19a9088def19286b"));

			TileScripts.AddScript(0x50, new ScriptBuilder(new List<string>
			{
				"0FD00F",
				"057F",
				"115F01",
                "0C600101",
                "62",
                "0588D10F",
                "0CD00F00",
				"00"
            }));
        }

        public void APItem()
        {
            // Set sprite bank
            PutInBank(0x00, 0x8525, Blob.FromHex("5c508015eaea"));
            PutInBank(0x15, 0x8050, Blob.FromHex("8d1621ad700029ff00c9f000f007f404005c2b8500f415009c70005c2b85006b"));

            // Set sprite adresss
            PutInBank(0x00, 0xB6B5, Blob.FromHex("5c808015"));
            PutInBank(0x15, 0x8080, Blob.FromHex("9c6500a59e8d7000c9de00b0045cbab600f011a900818562a990038d6400e2305ccbb600a901008562a920008d6400e2305ccbb600"));

            // Set item name
            PutInBank(0x03, 0xB50B, Blob.FromHex("07D080150A13B5"));
            PutInBank(0x15, 0x80D0, Blob.FromHex("0bf08d83054d0c054320c10c00053df08015"));
            PutInBank(0x15, 0x80F0, Blob.FromHex("03039aa9ffa2c7b8c0030303")); // AP Item

            // New sprite
            PutInBank(0x15, 0x8100, Blob.FromHex("0000010102031c1f3e3f7f7f7f7f7f7f0001031f2341415d")); // First tile
            PutInBank(0x15, 0x8118, Blob.FromHex("0000c0c020e01cfc3ee27fc17fc1ffdd00c0e0fce2c1c1dd")); // Second tile
            PutInBank(0x15, 0x8280, Blob.FromHex("3e227f417e437c473c271c1f020301013e7f7e7c3c1c0201")); // Third tile + 0x180 from first tile
            PutInBank(0x15, 0x8298, Blob.FromHex("2323c1c121e111f112f21cfc20e0c0c03fff3f1f1e1c20c0")); // Fourth tile

            // New Palette
            PutInBank(0x07, 0xDC74, Blob.FromHex("00000f3f9b429e4b79620f62f9410821")); // Palette 0x390

            // Palette hacks
            PutInBank(0x00, 0x8482, Blob.FromHex("22008515ea")); // zero out b part of x register when loading palette selector
            PutInBank(0x00, 0x848A, Blob.FromHex("22108515ea")); // zero out b part of x register when loading palette selector
            PutInBank(0x00, 0x850f, Blob.FromHex("eaeaea")); // don't clear the b part of x register
            PutInBank(0x15, 0x8500, Blob.FromHex("08e230a988aef400286b"));
            PutInBank(0x15, 0x8510, Blob.FromHex("08e230a998aef700286b"));


        }
    }
}
