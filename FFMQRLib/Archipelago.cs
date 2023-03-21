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
		
		}
		public void ItemFetcher()
		{
			PutInBank(0x01, 0x82A9, Blob.FromHex("22008015eaea"));
            PutInBank(0x01, 0x82A9, Blob.FromHex("08e220add00ff00fa9018db019a9508dee19a9088def19286b"));

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
	}
}
