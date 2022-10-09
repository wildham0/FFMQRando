using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum SkyCoinModes : int
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Start With")]
		StartWith,
		[Description("Save the Crystals")]
		SaveTheCrystals,
		[Description("Shattered Sky Coin")]
		ShatteredSkyCoin,
	}
	public enum SkyCoinFragmentsQty : int
	{
		[Description("Low (16)")]
		Low16 = 0,
		[Description("Mid (24)")]
		Mid24,
		[Description("High (32)")]
		High32,
		[Description("Random")]
		Random,
	}
	public partial class FFMQRom : SnesRom
	{
		public void SkyCoinMode(Flags flags, MT19337 rng)
		{
			if (flags.SkyCoinMode == SkyCoinModes.Standard)
			{
				return;
			}
			else if (flags.SkyCoinMode == SkyCoinModes.StartWith)
			{
				// Start With SkyCoin
				PutInBank(0x0C, 0xD3A3, Blob.FromHex("01"));
			}
			else if (flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals)
			{
				//change 4 crystals flags, give sky coin if all 4 sets(remove skycoin requirement from logic)
				var crystalSkyCoinScript = new ScriptBuilder(new List<string> {
						"2F",
						"050C0F[07]",
						"050B01[07]",
						"050B12[07]",
						"050B03[07]",
						"050B05[07]",
						$"0D5F01{(int)Items.SkyCoin:X2}0162",
						"00"
					});

				var pazuzuScript = new ScriptBuilder(new List<string> {
						"2B47",
						"2B48",
						"2B49",
						"2B40",
						"2B41",
						"2B42",
						"2B43",
						"2B44",
						"2B45",
						"2B46",
						"050260C11200",
						"00"
					});

				var pazuzuJumpScript = new ScriptBuilder(new List<string> {
						"0502A0C11200"
					});

				crystalSkyCoinScript.WriteAt(0x12, 0xC160, this);
				pazuzuScript.WriteAt(0x12, 0xC1A0, this);
				pazuzuJumpScript.WriteAt(0x03, 0xFDAD, this);
			}
			else if (flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				List<(SkyCoinFragmentsQty, int)> fragmentQtySelector = new()
				{
					(SkyCoinFragmentsQty.Low16, 16),
					(SkyCoinFragmentsQty.Mid24, 24),
					(SkyCoinFragmentsQty.High32, 36),
					(SkyCoinFragmentsQty.Random, rng.PickFrom(new List<int>() { 16, 24, 32 }))
				};

				int skycointqty = fragmentQtySelector[(int)flags.SkyCoinFragmentsQty].Item2;

				// show sky coin shards qty				
				PutInBank(0x03, 0x9811, Blob.FromHex("0700C11204"));
				PutInBank(0x12, 0xC100, Blob.FromHex("0b0f0ac1115e00058a00115e00058a0f930e052007ef970305210f5f0100"));

				// Increase shard count
				PutInBank(0x00, 0xDB82, Blob.FromHex("22409211EAEAEAEAEAEA"));
				PutInBank(0x11, 0x9240, Blob.FromHex("C90FD004EE930E6B0BF4A60E2B224E97002B6B"));

				// Start With SkyCoin
				PutInBank(0x0C, 0xD3A3, Blob.FromHex("01"));

				TalkScripts.AddScript(0x06,
					new ScriptBuilder(new List<string> {
						"04",
						"050220C112",
						"00"
					}));

				var newSkyDoorScript = new ScriptBuilder(new List<string> {
						"0F930E",
						$"0506{skycointqty:X2}[04]",
						"232F2B33",
						"050202FC03",
						"1A06" + TextToHex($"You need {skycointqty} Sky Coin pieces to open this door.") + "36",
						"00"
					});
				
				newSkyDoorScript.WriteAt(0x12, 0xC120, this);

				// Sky Coin new name
				PutInBank(0x0C, 0xC1D4, Blob.FromHex("acbeccff9fc5b4bac0b8c1c7"));

				// Sky Coin Fragment sprites
				PutInBank(0x04, 0x8450, Blob.FromHex("0D081F0037076B0FDF9FFC3E7F3FFF3F0A000F18BE35AC2C"));
				PutInBank(0x04, 0x8468, Blob.FromHex("7010F010E0E0E0E0C0C04040C0C080809010E02040C04080"));
				PutInBank(0x04, 0x85D0, Blob.FromHex("7D3FFD3FBE2EFC945808280810100000B43D6E9428181000"));
				PutInBank(0x04, 0x85E8, Blob.FromHex("808000000000000000000000000000008000000000000000"));
			}
		}
	}
}
			
