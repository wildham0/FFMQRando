using System;
using RomUtilities;

namespace FFMQLib
{
	public class RomOffsets
	{
		public const int TreasuresOffset = 0x8000;

		// MapObjects
		public const int AreaAttributesPointers = 0x3AF3B;
		public const int AreaAttributesPointersBase = 0x3B013;
		public const int AreaAttributesPointersQty = 108;
		public const int AreaAttributesPointersSize = 2;

		public const int MapObjectsAttributes = 0x3B013;
		public const int MapObjectsAttributesSize = 7;

		// MapChanges
		public const int MapChangesPointersOld = 0xB93A;
		public const int MapChangesEntriesOld = 0xBA0E;
		public const int MapChangesBankOld = 0x06;
		public const int MapChangesQtyOld = 0x6A;
		public const int MapChangesPointersNew = 0x8000;
		public const int MapChangesEntriesNew = 0x8100;
		public const int MapChangesBankNew = 0x12;
		public const int MapChangesQtyNew = 0x80;

		// Credits
		public const int CreditsHeader = 0x805F;
		public const int CreditsAddressOld = 0xB7BD;
		public const int CreditsBankOld = 0x03;
		public const int CreditsAddressNew = 0x8800;
		public const int CreditsBankNew = 0x12;
		public const int CreditsLengthConvert = 0xA57F; // Bank 0C
		public const int CreditsLengthReadTo = 0x9AD2; // Bank 00
		public const int CreditsLengthTheEnd = 0x80B8; // Bank 03

		// Enemies' Attacks
		public const int EnemiesAttacksAddress = 0xBC78; // Bank 02
		public const int EnemiesAttacksBank = 0x02;
		public const int EnemiesAttacksQty = 0xA9;
		public const int EnemiesAttacksLength = 0x07;

		// Enemies' Stats
		public const int EnemiesStatsAddress = 0xC275; // Bank 02
		public const int EnemiesStatsBank = 0x02;
		public const int EnemiesStatsQty = 0x53;
		public const int EnemiesStatsLength = 0x0e;

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

		// OW Movement Arrows
		public const int OWMovementArrows = 0x03EE84;


	}
}
