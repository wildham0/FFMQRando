using FFMQLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFMQRWin
{
	public class ExportTools
	{
	/*	public void exportColorJson(FFMQRom rom, string savepath)
		{
			MapPalettes palettesfromrom = new MapPalettes(rom);
			//var json = palettesfromrom.ExportToJson();

			using (StreamWriter outputFile = new StreamWriter(savepath + "FFMQR_MapPalettes.json"))
			{
				outputFile.Write(json);
			}
		}

		public void exportTilesJson(FFMQRom rom, string savepath)
		{
			GraphicRows tilesfromrom = new GraphicRows(rom);
			var json = tilesfromrom.ExportToJson();

			using (StreamWriter outputFile = new StreamWriter(savepath + "FFMQR_GraphicRows.json"))
			{
				outputFile.Write(json);
			}
		}

		public void exportTilesPropJson(FFMQRom rom, string savepath)
		{
			TilesProperties tilesfromrom = new TilesProperties(rom);
			var json = tilesfromrom.ExportToJson();

			using (StreamWriter outputFile = new StreamWriter(savepath + "FFMQR_TilesProperties.json"))
			{
				outputFile.Write(json);
			}
		}

		public void exportMapJson(FFMQRom rom, string savepath)
		{
			List<JsonMap> maps = new();
			GameMaps mapsfromrom = new GameMaps(rom);
			for (int i = 0; i < 0x2C; i++)
			{
				//maps.Add(mapsfromrom[i].ConvertToJson());

				var json = JsonSerializer.Serialize(mapsfromrom[i].ConvertToJson());

				using (StreamWriter outputFile = new StreamWriter(savepath + $"FFMQR_map{i:X2}.json"))
				{
					outputFile.Write(json);
				}
			}


		}
	*/

	}

}
