﻿using System;
using System.Collections.Generic;
using System.Text;
using RomUtilities;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public void FastMovement()
		{
			// walking
			// double scrolling rate
			Put(0x008CAC, Blob.FromHex("FE0202FE")); // 8CAB > 8CAC
			// halve number of frames
			Put(0x00939C, Blob.FromHex("07")); // 939b > 939C
			// speed up animation
			Put(0x0094BA, Blob.FromHex("04EA")); // 94B9> 94BA


			// jumping
			// halve number of frames
			Put(0x009DC1, Blob.FromHex("07")); // 9DC0 > 9DC1
			// move routine to skip one position in the jump position table
			Put(0x009DE1, Blob.FromHex("22E0FF11EAEA")); // 9DE0 > 9DE1
			PutInBank(0x11, 0xFFE0, Blob.FromHex("E8E8E8E8E8E88E4F196B"));
			// move starting position in the jump pos table by one so we end on the last intended position
			PutInBank(0x00,0xF24C, Blob.FromHex("03006300C3002301")); // F251 > F24C

			// Working hack for inmap room transition, except it slow down walking speed back at 16 frames vs 8, but it works!
			PutInBank(0x11, 0x8200, Blob.FromHex("A9028D461AA00A008C9119A90F8D26196B"));
			Put(0x0093DD, Blob.FromHex("22008211")); // 93DC > 93DD
			Put(0x0093DD+4, Blob.FromHex("4A900C8007")); // 93DC > 93DD+4
			Put(0x009462, Blob.FromHex("4CE193")); // 9461 > 9462

			// Fix the dragonclaw, normal speed
			Put(0x009BCD, Blob.FromHex("02")); // 9BCC > 9BCD ? 98CD
			Put(0x009C7F, Blob.FromHex("07")); // 9C7E > 9C7F

			// Fix mine elevator animation
			PutInBank(0x01, 0xCD8C, Blob.FromHex("01"));

			// Move CODE_018AE4 for space
			//var CODE_018AE4 = Get(0x008AE4, 0x2F);
			//CODE_018AE4 += Blob.FromHex("6B");

			//PutInBank(0x11, 0x8300, CODE_018AE4);
			//Put(0x008AE4, Blob.FromHex("220083114C138B"));

			// Put duplicate of CODE_0182D8, without wait for vsync
			//Put(0x008492 + 5, Blob.FromHex("08DA5AE220C210205CAB2080A07AFA2860"));
			//Put(0x008AEB, Blob.FromHex("08DA5AE220C210205CAB2080A07AFA284CF293"));


			//Put(0x0093FB, Blob.FromHex("58"));
			//Put(0x00944C, Blob.FromHex("984AB0A2EAEAEAEAE220C21022008111"));
			//Put(0x00944C, Blob.FromHex("984A90044CEB8AEAE220C21022008111"));
			//PutInBank(0x11, 0x8100, Blob.FromHex("A9028D461AAD261938ED28198D26196B"));




			//Put(0x0093FB, Blob.FromHex("4C"));
			//Put(0x0093FC, Blob.FromHex("AE890EDA22008011"));
			//Put(0x0093FC + 0x08, Blob.FromHex("2077F9FA8E890EAC9119888C911920D882"));
			//Put(0x0093FC + 0x19, Blob.FromHex("AC9119888C911920D882EA4C4894"));
			//Put(0x009448, Blob.FromHex("22008111"));
			//Put(0x0093FC + 0x19, Blob.FromHex("AE890EDA22008011"));
			//Put(0x0093FC + 0x21, Blob.FromHex("2077F9FA8E890EAC9119888C91194C4C94"));



			//PutInBank(0x11, 0x8000, Blob.FromHex("E220AEC1198EBD19AEC3198EBF19AE890E8E2D19BBAD2E1938E905187FE194018D2E19BFE1940148AD2D1938E9088D2D19A900EB68C230186DBF19290F008DBF196B"));

			///PutInBank(0x11, 0x8100, Blob.FromHex("E220AD2619C907B007AD021938ED0A196B"));
		}
		public void DefaultSettings()
		{
			// Show Figure by default instead of Scale for HP
			GameFlags[(int)GameFlagsList.ShowFigureForHP] = true;

			// Default Text speed to 1
			Data[0x65397] = 0x00;
		}
		public void RemoveClouds()
		{
			// Change conditional branching for erasing clouds to always branch
			PutInBank(0x0B, 0x858E, Blob.FromHex("80"));
			PutInBank(0x0B, 0x8599, Blob.FromHex("80"));
			PutInBank(0x0B, 0x85A4, Blob.FromHex("80"));
		}
		public void SmallFixes()
		{
			// Fix Bomb and JumboBomb to work everywhere
			PutInBank(0x01, 0xF453, Blob.FromHex("3030"));

			// Allow shattered tile to intercept MegaGrenade
			TilesProperties[0x06][0x15].Byte1 = 0x07;
			//PutInBank(0x06, 0xAE2A, Blob.FromHex("07"));

			// Stop CatClaws from giving Bow&Arrows to companion
			PutInBank(0x00, 0xdb9d, Blob.FromHex("EAEAEAEA"));

			// Phoebe1 start with Bow&Arrows
			PutInBank(0x0C, 0xd1d1, Blob.FromHex("2D0004"));

			// Start with 50 bombs so we don't need to update when acquiring them
			PutInBank(0x0C, 0xd0e0, Blob.FromHex("32"));

			// Fix armor downgrading
			PutInBank(0x00, 0xDBCE, Blob.FromHex("201490"));

			// Fix Giant Tree Axe-less softlock by blocking access from outside the forest
			PutInBank(0x03, 0xA4A3, Blob.FromHex("2E"));
			PutInBank(0x03, 0xA625, Blob.FromHex("7B"));
		}

		public void BugFixes()
		{
			// Fix vendor buy 0 bug
			PutInBank(0x00, 0xB75B, Blob.FromHex("D0")); // Instead of BPL, BNE to skip 0
			PutInBank(0x00, 0xB783, Blob.FromHex("8D")); // Instead of STZ, STA (A will always be #$01 if reached)

			PutInBank(0x00, 0xB799, Blob.FromHex("22108711")); // Long so we can compare to #01 instead of #00
			PutInBank(0x11, 0x8710, Blob.FromHex("38AD6201C9016B")); // Compare to #01
			
			PutInBank(0x00, 0xB7A4, Blob.FromHex("01")); // Set minimum quantity to 1
			PutInBank(0x00, 0xB7D8, Blob.FromHex("5C00871100")); // Long jump to set to 1 instead of 0
			PutInBank(0x11, 0x8700, Blob.FromHex("A9018D62015C8DB700")); // Jump back to hard coded adress because we're replacing a BRA

			// Fix Companion Armor Bug
			PutInBank(0x00, 0x9E7E, Blob.FromHex("1490")); // Add copying armor stats to working memory as a command (05 08)
			PutInBank(0x03, 0x8606, Blob.FromHex("08C0FF00")); // Insert jump for space in add companion routine
			PutInBank(0x03, 0xFFC0, Blob.FromHex("0508051ED0000100")); // Copy armor command + moved original command

			// Fix Life Insta Kill Bug
			PutInBank(0x02, 0x9238, Blob.FromHex("A5562B2908F0E5")); // Load weakness instead of resistance, and beq insteand of bne
			PutInBank(0x02, 0x9CAA, Blob.FromHex("EAEA")); // Don't branch if resistant to fatal

			// Fix Cure Overflow Bug
			PutInBank(0x02, 0x95CA, Blob.FromHex("EAEAEAEA22008611"));
			PutInBank(0x11, 0x8600, Blob.FromHex("A514186D7704B002C5166B")); // Same as original except we skip comparing to max hp if carry is set
		}
	}
}
