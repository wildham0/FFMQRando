using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	
	public partial class FFMQRom : SnesRom
	{
		public void HintsHacks(Flags flags, MapSprites mapsprites,  ObjectList gameobjects, MT19337 rng)
		{
			// Robot Sprite
			PutInBank(0x16, 0x8020, Blob.FromHex("600057002c001b0017007f00df40df4000201304080062620600ea003400d800e800fe00fd04fd040004c82010004646ff60f300f0009000f860ff607f050f00620c0f6f67600000ff06cf000f060f001900ff06ff66f6004630f6f0e606060000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028f019900560636003d103f00067e71662f090200f890f8b0f870b820f000e820f8b0f80090b070600010000000000f0033006f20df40bf01bf02be0000000c30605152511000be10fa90f4c0f8e0f8303810d800001094c8e030d020fb0283028302830243003e003e143f00067e7e7e3c010100f890f8b0f8b0f8b03000d8c0d800b80090b0b0b0c0e0204037002c0017001b007d009e00950097000013080402616a68ec001400e800d800be007900a900e90000e81020408656169703ff6bff0cff0fff67ff607f050f006b6b0c0f67600000e9c0ffd6ff30fff6ffe0ff06ff66f600d6d630f6e0060600"));

			// Sprite loading hack, if sprite == 44, load at bank 16 instead, should extend tho
			PutInBank(0x16, 0x8300, Blob.FromHex("ad2a19c90044f00ba602a404a90f00547f046be220c210a9168506c23018a502690080aaa404a90f00547f166b"));
			PutInBank(0x01, 0xA90C, Blob.FromHex("22008316eaeaeaeaeaea"));


			mapsprites[0x31].AdressorList.Add(new SpriteAddressor(9, 0, 0x44, SpriteSize.Tiles16));
			var robotObject = new MapObject(gameobjects[0x11][0x01]);
			robotObject.Sprite = 0x4C;
			robotObject.Facing = FacingOrientation.Right;
			robotObject.Behavior = 0x08;
			robotObject.Value = 0x2A;
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


			// Seller in Aquaria
			TalkScripts.AddScript((int)TalkScriptsList.ForestaHinter,
				new ScriptBuilder(new List<string>{
					$"2E{(int)NewGameFlagsList.ForestaHintGiven:X2}[00]",
					// Check Money
					$"09FFFFFF",
					// Read $9e for money result
					$"0BFF[00]", // FF = Not Enough Money Message
					$"0B01[00]", // 01 = Vendor Gate
					// We have enough money, ask if they want it
					$"100004120115", // move price
					TextToHex("Want a hint for ") + "3600",
					"08D0FD", // yes/no
					"050BFB[00]",
					"00", // no, we done
					// take money
					$"09FFFFFF",
					// set flag
					$"23{(int)NewGameFlagsList.ForestaHintGiven:X2}",
					// compute hint, address in $9e
					$"09FFFFFF",
					// jump to text
					"050300",
					// Not Enough Money
					TextToHex("Need More Money Kid!") + "3600",
					// Vendor Gate
					TextToHex("Buy more from vendor kid!") + "3600",
					// Hint already given
					TextToHex("Hope you're happy with your hint!") + "3600",




				}));
		}
	}
}
