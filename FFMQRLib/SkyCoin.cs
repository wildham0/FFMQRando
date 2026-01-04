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
		[Description("Random 16-32")]
		RandomNarrow,
		[Description("Random 10-38")]
		RandomWide,

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
				List<string> mysteriousmanline = new()
				{
					$"Why are you talking to me? You already have the Sky Coin!",
					$"Now take a look\nAt your inventory\nYou've got the Sky Coin\nAlready!",
					$"Starting with the Sky Coin? Daring today, aren't we?",
				};

				UpdateMysteriousMan(rng.PickFrom(mysteriousmanline));
			}
			else if (flags.SkyCoinMode == SkyCoinModes.SaveTheCrystals)
			{
				// extra hack in Hacks.cs in ChestsHacks()
				
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

				crystalSkyCoinScript.WriteAt(0x12, 0xC160, this);

				List<string> mysteriousmanline = new()
				{
					$"To enter Doom Castle, you shall defeat the four demons, no more, no less! Once the fourth evil one is defeated, then you shall receive the Sky Coin.",
					$"Defeat the four Fiends to light the four orbs... I mean, receive the Sky Coin.",
					$"Only the one strong enough to restore the four Crystals will be blessed by the Sky Coin!",
				};

				UpdateMysteriousMan(rng.PickFrom(mysteriousmanline));
			}
			else if (flags.SkyCoinMode == SkyCoinModes.ShatteredSkyCoin)
			{
				List<(SkyCoinFragmentsQty, int)> fragmentQtySelector = new()
				{
					(SkyCoinFragmentsQty.Low16, 16),
					(SkyCoinFragmentsQty.Mid24, 24),
					(SkyCoinFragmentsQty.High32, 32),
					(SkyCoinFragmentsQty.RandomNarrow, rng.Between(16,32)),
					(SkyCoinFragmentsQty.RandomWide, rng.Between(10, 38))
				};

				int skycointqty = fragmentQtySelector[(int)flags.SkyCoinFragmentsQty].Item2;

				// show sky coin shards qty				
				PutInBank(0x03, 0x9811, Blob.FromHex("0700C11204"));
				PutInBank(0x12, 0xC100, Blob.FromHex("0b0f0ac1115e00058a00115e00058a0f930e052007ef970305210f5f0100"));

				// Start With SkyCoin
				//PutInBank(0x0C, 0xD3A3, Blob.FromHex("01"));

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
						"1A06" + MQText.TextToHex($"You need {skycointqty} Sky Coin pieces to open this door.") + "36",
						"00"
					});
				
				newSkyDoorScript.WriteAt(0x12, 0xC120, this);

				List<string> mysteriousmanline = new()
				{
					$"Oh oh! Finding {skycointqty} Sky Coin fragments shouldn't be too difficult for the Hero!",
					$"{skycointqty} Sky Coin fragments for the Dark King on his dark throne,\nin the Doom Castle where the Fiends lie.",
					$"You're the Seeker of the {skycointqty} Sky Coin fragments.\nFind them before the demons and rescue mankind!",
					$"Phase 1: Collect {skycointqty} Sky Coin fragments.\nPhase 2: ...\nPhase 3: Profit.",
					$"Stop. Who would enter the Doom Castle must gather first these Sky Coin fragments {skycointqty}, ere the Dark King he see.",
					$"Legend says he told Phoebe to go forth and combat evil. To do that, she had to find {skycointqty} Sky Coin fragments."
				};

				GameInfoScreen.FragmentsCount = skycointqty;
				UpdateMysteriousMan(rng.PickFrom(mysteriousmanline));

				// Sky Coin new name
				PutInBank(0x0C, 0xC1D4, Blob.FromHex("acbeccff9fc5b4bac0b8c1c7"));

				// Sky Coin Fragment sprites
				PutInBank(0x04, 0x8450, Blob.FromHex("0D081F0037076B0FDF9FFC3E7F3FFF3F0A000F18BE35AC2C"));
				PutInBank(0x04, 0x8468, Blob.FromHex("7010F010E0E0E0E0C0C04040C0C080809010E02040C04080"));
				PutInBank(0x04, 0x85D0, Blob.FromHex("7D3FFD3FBE2EFC945808280810100000B43D6E9428181000"));
				PutInBank(0x04, 0x85E8, Blob.FromHex("808000000000000000000000000000008000000000000000"));
			}
		}
		public void UpdateMysteriousMan(string dialogue)
		{
			// Mysterious Man tell number of fragments
			TalkScripts.AddScript(0x09,
				new ScriptBuilder(new List<string> {
						"04",
						"0502E0C112",
						"00"
				}));

			var newMysteriousManScript = new ScriptBuilder(new List<string> {
						"1A09" + MQText.TextToHex(dialogue) + "36",
						"2B6C",
						"0502E9FE03"
					});

			newMysteriousManScript.WriteAt(0x12, 0xC1E0, this);
		}
	}
}
			
