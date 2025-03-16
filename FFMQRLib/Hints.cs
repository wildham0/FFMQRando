using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;
using System.Linq.Expressions;

namespace FFMQLib
{
	
	public partial class FFMQRom : SnesRom
	{
		public enum HintPriceMode
		{ 
			Free,
			Fixed,
			Progressive
		}
		public enum HintCheckType : byte
		{
			Item = 0x00,
			Weapon = 0x01,
			Armor = 0x02,
			Spell = 0x03,
			WeaponX3 = 0x04,
			ArmorX3 = 0x05
		}
		public void HintsHacks(Flags flags, MapSprites mapsprites,  ObjectList gameobjects, MT19337 rng)
		{
			var priceMode = HintPriceMode.Progressive;
			int hintprice = 250;


			// Robot Sprite
			PutInBank(0x16, 0x8020, Blob.FromHex("600057002c001b0017007f00df40df4000201304080062620600ea003400d800e800fe00fd04fd040004c82010004646ff60f300f0009000f860ff607f050f00620c0f6f67600000ff06cf000f060f001900ff06ff66f6004630f6f0e606060000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028f019900560636003d103f00067e71662f090200f890f8b0f870b820f000e820f8b0f80090b070600010000000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028302830243003e003e143f00067e7e7e3c010100f890f8b0f8b0f8b03000d8c0d800b80090b0b0b0c0e0204037002c0017001b007d009e00950097000013080402616a68ec001400e800d800be007900a900e90000e81020408656169703ff6bff0cff0fff67ff607f050f006b6b0c0f67600000e9c0ffd6ff30fff6ffe0ff06ff66f600d6d630f6e0060600"));

			// Sprite loading hack, if sprite == 44, load at bank 16 instead, should extend tho 
			PutInBank(0x16, 0x9000, Blob.FromHex("ad2a19c90044f00ba602a404a90f00547f046be220c210a9168506c23018a502690080aaa404a90f00547f166b")); // 0x30
			PutInBank(0x01, 0xA90C, Blob.FromHex("22009016eaeaeaeaeaea"));

			// Hinter Routines
			// Set price part
			string priceroutine = priceMode == HintPriceMode.Progressive ? 
				$"08c230a9c1004822769700f00aad00041869{(hintprice % 0x100):X2}{(hintprice / 0x100):X2}8d0004681ac9c40090e84c3091" : // 0x30
				$"08c230a9{(hintprice % 0x100):X2}{(hintprice / 0x100):X2}8d00044c3091"; // 0x10
			PutInBank(0x16, 0x9100, Blob.FromHex(priceroutine));

			// can afford routine
			string densityfloor = (flags.EnemiesDensity == EnemiesDensity.None) ?
				"eaea" :
				"8039";

			PutInBank(0x16, 0x9130, Blob.FromHex($"a900008d0204{densityfloor}a9d00022769700d00aad02041869c8008d0204a9d10022769700d00aad02041869f4018d0204a9d20022769700d00aad020418692c018d0204ad0004186d02048d0204e220c210ad860ed019c230ad840ecd0204b00fcd0004b005a9ff00800aa901008005c230a900008d9e00286b")); // 0x80

			// Take Money
			PutInBank(0x16, 0x91D0, Blob.FromHex("08c230ad840e38ed00048d840ee220c210ad860ee9008d860e286b")); // 0x20

			// Hint seeker
			PutInBank(0x16, 0x9200, Blob.FromHex("0bf4a60e2b225a97002b1a3a60")); // Items
			PutInBank(0x16, 0x9210, Blob.FromHex("0bf432102b225a97002b1a3a60")); // Weapons
			PutInBank(0x16, 0x9220, Blob.FromHex("0bf435102b225a97002b1a3a60")); // Armors
			PutInBank(0x16, 0x9230, Blob.FromHex("0bf438102b225a97002b1a3a60")); // Spells
			PutInBank(0x16, 0x9240, Blob.FromHex("aa201092d00d8a1aaa201092d0058a1a20109260")); // 3 weapons
			PutInBank(0x16, 0x9260, Blob.FromHex("aa202092d00d8a1aaa202092d0058a1a20209260")); // 3 armors

			PutInBank(0x16, 0x9280, Blob.FromHex("009210922092309240926092")); // check routines pointers

			ushort forestaHinterCheckArray = 0x0000;
			ushort aquariaHinterCheckArray = 0x0000;
			ushort fireburgHinterCheckArray = 0x0000;
			ushort widniawHinterCheckArray = 0x0000;

			// Entrance point for each hinter
			PutInBank(0x16, 0x9290, Blob.FromHex($"08e220c210a900eba{forestaHinterCheckArray:X4}04cd092"));
			PutInBank(0x16, 0x92A0, Blob.FromHex($"08e220c210a900eba{aquariaHinterCheckArray:X4}04cd092"));
			PutInBank(0x16, 0x92B0, Blob.FromHex($"08e220c210a900eba{fireburgHinterCheckArray:X4}04cd092"));
			PutInBank(0x16, 0x92C0, Blob.FromHex($"08e220c210a900eba{widniawHinterCheckArray:X4}04cd092"));
			// Main checker loop
			PutInBank(0x16, 0x92D0, Blob.FromHex("c8c8c8c8c8b90193c9fff00a0aaab90093fc8092d0ebb902938d9e00b903938d9f00b904938da000286b")); // 0x30

			// data_lut

			byte[] steelswordHint = new byte[] { (byte)WeaponFlags.SteelSword, (byte)HintCheckType.Weapon, 0x00, 0x94, 0x16 };
			byte[] sandcoinHint = new byte[] { (byte)ItemFlags.SandCoin, (byte)HintCheckType.Item, 0x20, 0x94, 0x16 };
			byte[] uselessHint = new byte[] { 0xFF, 0xFF, 0x40, 0x94, 0x16 };


			PutInBank(0x16, 0x9300, steelswordHint.Concat(sandcoinHint).Concat(uselessHint).ToArray()); // 0x30

			PutInBank(0x16, 0x9400, TextToByte("Steel Sword is over there.", true).Concat(new byte[] { 0x36, 0x00 }).ToArray()); // 0x30
			PutInBank(0x16, 0x9420, TextToByte("Sand Coin is over there.", true).Concat(new byte[] { 0x36, 0x00 }).ToArray()); // 0x30
			PutInBank(0x16, 0x9440, TextToByte("Lol joke hints.", true).Concat(new byte[] { 0x36, 0x00 }).ToArray()); // 0x30


			//
			//PutInBank(0x16, 0x9300, Blob.FromHex("c8c8c8c8c8b9ffffc9fff0ff0aaab9fffffcffffd0ebb9ffff8d9e00b9ffff8d9f00b9ffff8da000286b")); // 0x30







			mapsprites[0x31].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			var robotObject = new MapObject(gameobjects[0x11][0x01]);
			robotObject.Sprite = 0x4C;
			robotObject.Facing = FacingOrientation.Right;
			robotObject.Behavior = 0x08;
			robotObject.Value = 0x88;
			robotObject.X = 0x1F;
			robotObject.Y = 0x10;

			MapObjects[0x11].Add(robotObject);

			var aquariaRobot = new MapObject(robotObject);
			aquariaRobot.Facing = FacingOrientation.Down;
			aquariaRobot.X = 0x36;
			aquariaRobot.Y = 0x29;
			mapsprites[0x09].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x1B].Add(aquariaRobot);

			List<(FacingOrientation orient, byte x, byte y)> fireburgRobotPlacements = new() { (FacingOrientation.Left, 0x0A, 0x2B), (0x00, 0x11, 0x2E), (FacingOrientation.Down, 0x0E, 0x2D), (FacingOrientation.Left, 0x15, 0x2A), (FacingOrientation.Up, 0x10, 0x2C) };

			var fireburgRobotPlacement = rng.PickFrom(fireburgRobotPlacements);
			var fireburgRobot = new MapObject(robotObject);
			fireburgRobot.Facing = fireburgRobotPlacement.orient;
			fireburgRobot.X = fireburgRobotPlacement.x;
			fireburgRobot.Y = fireburgRobotPlacement.y;
			mapsprites[0x13].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x31].Add(fireburgRobot);

			var windiaRobot = new MapObject(robotObject);
			windiaRobot.Facing = FacingOrientation.Left;
			windiaRobot.X = 0x35;
			windiaRobot.Y = 0x16;
			windiaRobot.Sprite = 0x44;
			windiaRobot.Palette = 0x03;
			// this one gets clobbered by phoebe a bit, but it should be fine since you can't talk from the north
			mapsprites[0x23].AdressorList.Add(new SpriteAddressor(7, 0, 0x44, SpriteSize.Tiles16));
			MapObjects[0x52].Add(windiaRobot);

			// Scripts
			// Check flag, if set go to no more hints message
			// Check money
			// Check sellers flag, compute min money requirements,
			// If enough money
			// Text+choice
			// No, finish
			// Yes, find best hint, show it
			// take money, flip flag


			ScriptBuilder hinterBaseScript;

			// Seller in Aquaria
			if (priceMode == HintPriceMode.Free)
			{
				hinterBaseScript = new ScriptBuilder(new List<string>()
				{
					$"2E{(int)NewGameFlagsList.ForestaHintGiven:X2}[00]",
					TextToHex("Want a hint?") + "3600", // todo
					"08D0FD", // yes/no
					"050BFB[00]",
					"00", // no, we done
					$"23{(int)NewGameFlagsList.ForestaHintGiven:X2}", // set flag
					// compute hint, address in $9e
					"09909216",
					// jump to text
					"050300",
				});
			}
			else
			{
				hinterBaseScript = new ScriptBuilder(new List<string>()
				{
					$"2E{(int)NewGameFlagsList.ForestaHintGiven:X2}[00]",
					// Check Money
					"09009116",
					// Read $9e for money result
					"0BFF[00]", // FF = Not Enough Money Message
					"0B01[00]", // 01 = Vendor Gate
					// We have enough money, ask if they want it
					"100004120115", // move price
					TextToHex("Want a hint for ") + "3600", // todo
					"08D0FD", // yes/no
					"050BFB[00]",
					"00", // no, we done
					// take money
					$"09D09119",
					// set flag
					$"23{(int)NewGameFlagsList.ForestaHintGiven:X2}",
					// compute hint, address in $9e
					"09909216",
					// jump to text
					"050300",
					// Not Enough Money
					TextToHex("Need More Money Kid!") + "3600",
					// Vendor Gate
					TextToHex("Buy more from vendor kid!") + "3600",
					// Hint already given
					TextToHex("Hope you're happy with your hint!") + "3600",
				});
			}

			TalkScripts.AddScript((int)TalkScriptsList.ForestaHinter, hinterBaseScript);
		}
	}
}
