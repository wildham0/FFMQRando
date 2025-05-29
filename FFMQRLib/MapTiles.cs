using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace FFMQLib
{
	public class TilesProperties
	{
		private List<List<SingleTile>> _tilesProperties;

		private const int MapTileBank = 0x06;
		private const int MapTileGraphicOffset = 0x8000;
		private const int MapTileFlipOffset = 0xA000;
		private const int MapTilePropertiesOffset = 0xA800;
		private const int MapTileGraphicSize = 0x0200;
		private const int MapTileFlipSize = 0x80;
		private const int MapTilePropertiesSize = 0x100;
		private const int MapTileQty = 0x10;

		public TilesProperties(FFMQRom rom)
		{
			var tilesProp = rom.GetFromBank(MapTileBank, MapTilePropertiesOffset, MapTileQty * MapTilePropertiesSize).Chunk(MapTilePropertiesSize);
			var tilesFlip = rom.GetFromBank(MapTileBank, MapTileFlipOffset, MapTileQty * MapTileFlipSize).Chunk(MapTileFlipSize);
			var tilesGraphic = rom.GetFromBank(MapTileBank, MapTileGraphicOffset, MapTileQty * MapTileGraphicSize).Chunk(MapTileGraphicSize);

			_tilesProperties = new();

			for (int i = 0; i < MapTileQty; i++)
			{
				List<SingleTile> tilePropGroup = new();

				for (int j = 0; j < 0x80; j++)
				{
					tilePropGroup.Add(new SingleTile(tilesProp[i].SubBlob((j * 2), 2), tilesGraphic[i].SubBlob(j * 4, 4), tilesFlip[i][j]));
				}

				_tilesProperties.Add(tilePropGroup);
			}
		}
		public List<SingleTile> this[int propTableID]
		{
			get => _tilesProperties[propTableID];
			set => _tilesProperties[propTableID] = value;
		}
		public string ExportToJson()
		{
			return JsonSerializer.Serialize(_tilesProperties, new JsonSerializerOptions { WriteIndented = true });
		}
		public void ImportJson(string filename)
		{
			var assembly = Assembly.GetExecutingAssembly();
			string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
			List<List<SingleTile>> tileprops;

			using (StreamReader tilepropfile = new StreamReader(assembly.GetManifestResourceStream(filepath), Encoding.UTF8))
			{
				tileprops = JsonSerializer.Deserialize<List<List<SingleTile>>>(tilepropfile.ReadToEnd());
			}

			_tilesProperties = tileprops;
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(MapTileBank, MapTilePropertiesOffset, _tilesProperties.SelectMany(x => x.SelectMany(y => y.GetPropBytes())).ToArray());
			rom.PutInBank(MapTileBank, MapTileFlipOffset, _tilesProperties.SelectMany(x => x.Select(y => y.GetFlipByte())).ToArray());
			rom.PutInBank(MapTileBank, MapTileGraphicOffset, _tilesProperties.SelectMany(x => x.SelectMany(y => y.GetGraphicBytes())).ToArray());
		}
	}

	public class GraphicTileProp
	{ 
		public byte Tile { get; set; }
		public bool HorizontalFlip { get; set; }
		public GraphicTileProp() { }
		public GraphicTileProp(byte tile, bool hflip)
		{
			Tile = tile;
			HorizontalFlip = hflip;
		}
	}

	public enum GraphicTiles
	{ 
		UpperLeft = 0,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	public class SingleTile
	{ 
		public byte PropertyByte1 { get; set; }
		public byte PropertyByte2 { get; set; }
		public List<GraphicTileProp> GraphicTiles { get; set; }
		private List<byte> flipmask = new() { 0x01, 0x02, 0x04, 0x08 };
		public SingleTile() { }
		public SingleTile(byte[] tileprop, byte[] graphic, byte hflip)
		{
			PropertyByte1 = tileprop[0];
			PropertyByte2 = tileprop[1];

			GraphicTiles = new();
			
			for (int i = 0; i < 4; i++)
			{
				GraphicTiles.Add(new GraphicTileProp(graphic[i], (hflip & flipmask[i]) > 0));
			}
		}

		public byte[] GetPropBytes()
		{
			return new byte[] { PropertyByte1, PropertyByte2 };
		}
		public byte[] GetGraphicBytes()
		{
			return GraphicTiles.Select(t => t.Tile).ToArray();
		}
		public byte GetFlipByte()
		{
			int flipbyte = 0x00;
			
			for(int i = 0; i < 4; i++)
			{
				flipbyte |= GraphicTiles[i].HorizontalFlip ? flipmask[i] : 0x00;
			}

			return (byte)flipbyte;
		}
	}
}
