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
	public enum SkyCoinShardsQty : int
	{
		[Description("Low (16)")]
		Low = 0,
		[Description("Mid (24)")]
		Mid,
		[Description("High (32)")]
		High,
		[Description("Random")]
		Random,
	}
	public partial class FFMQRom : SnesRom
	{
		public void SkyCoin(Flags flags, MT19337 rng)
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
			}
			else if (flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				SkyCoinShardsQty skycoinqtyflag = SkyCoinShardsQty.Low;

				List<(int, int)> test = new()
				{
					(0, 16),
					(1, 24),
					(2, 36),
					(3, rng.PickFrom(new List<int>() { 16, 24, 32 }))
				};

				int skycointqty = test[(int)skycoinqtyflag].Item2;

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
			}
		}
	}
}
			
