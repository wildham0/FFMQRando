using System;
using RomUtilities;

namespace FFMQLib
{
	public class RomOffsets
	{
		// MapObjects
		public const int AreaAttributesPointers = 0x3AF3B;
		public const int AreaAttributesPointersBase = 0x3B013;
		public const int AreaAttributesPointersQty = 108;
		public const int AreaAttributesPointersSize = 2;

		public const int MapObjectsAttributes = 0x3B013;
		public const int MapObjectsAttributesSize = 7;

		// Scripts
		public const int GameStartScript = 0x01f811;

		public const int TalkScriptsPointers = 0x01d5e5;
		public const int TileScriptsPointers = 0x01bb81;
		public const int TileScriptPointerQty = 0x50;
		public const int TalkScriptPointerQty = 0x7C;

		public const int TileScriptOffset = 0xbc21;
		public const int TalkScriptOffset = 0xd6dd;
		public const int TileScriptEndOffset = 0xd280;
		public const int TalkScriptEndOffset = 0xf811;

		// Exits
		public const int ExitTilesCoordPointers = 0x02F920;
		public const int ExitTilesCoordPointersQty = 108;
		public const int ExitTilesCoord = 0x02F9F8;
		public const int ExitsCoord = 0x02F4A0;
		public const int ExitsCoordQty = 216;

		// Maps
		public const int MapDataAddresses = 0x058735;
		public const int MapAttributes = 0x058CD9;
		public const int MapDimensionsTable = 0x058540;
		public const int MapTileData = 0x032800;
	}
}
