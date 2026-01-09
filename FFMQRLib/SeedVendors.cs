using FFMQLib;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMQLib
{
	public enum SeedVendorSettings
	{
		[Description("Standard")]
		Standard = 0,
		[Description("No Vendors")]
		NoVendors,
		[Description("Defeat Jinn")]
		DefeatJinn,
		[Description("Random Bosses")]
		RandomBosses,
	}

	public class SeedVendors
	{
		private Dictionary<EnemyIds, TalkScriptsList> bossesSeedVendors;
		private Dictionary<TalkScriptsList, EnemyIds> seedVendorsBosses => bossesSeedVendors.ToDictionary(s => s.Value, s => s.Key);
		private Dictionary<EnemyIds, string> bossesNames = new()
		{
			{ EnemyIds.Minotaur, "Minotaur"},
			{ EnemyIds.Squidite, "Squidite"},
			{ EnemyIds.SnowCrab, "Snow Crab"},
			{ EnemyIds.Jinn, "Jinn"},
			{ EnemyIds.Medusa, "Medusa"},
			{ EnemyIds.Gidrah, "Gidrah"},
			{ EnemyIds.Dullahan, "Dullahan"},
			{ EnemyIds.FlamerusRex, "Flamerus Rex"},
			{ EnemyIds.IceGolem, "Ice Golem"},
			{ EnemyIds.DualheadHydra, "Dualhead Hydra"},
			{ EnemyIds.Pazuzu, "Pazuzu"},
		};

		private Dictionary<TalkScriptsList, string> scripts = new()
		{
			{ TalkScriptsList.FireburgSeedVendor, $"23{(int)GameFlagIds.FireburgSeedQuest:X2}"},
			{ TalkScriptsList.WindiaSeedVendor, $"23{(int)GameFlagIds.WindiaSeedQuest:X2}2B{(int)GameFlagIds.WindiaSeedPending:X2}"},
		};

		public SeedVendors()
		{
			bossesSeedVendors = new();
			// set vendor flag X
			// generate dialogue X
			// update vendor script X
			// update lady map object flag X
			// infoscreen?

			// fireburg dude 0x42
			// boat seed vendor 0x45
			// lady talk script selling 0x57 
			// lady talk script bed 0x63

			// just update lady script

			// fireburg
			// map31 > o08 > change talk script, jump to 0x42

			// windia
			// map 50 > o02 > flag 
			// map 52 > o06 > flag =/ , change talk script

			// rename duallahan gameflag
		}
		public void SetSeedVendors(SeedVendorSettings settings, MT19337 rng)
		{
			if (settings == SeedVendorSettings.Standard || settings == SeedVendorSettings.NoVendors)
			{
				return;
			}

			if (settings == SeedVendorSettings.DefeatJinn)
			{
				bossesSeedVendors.Add(EnemyIds.Jinn, TalkScriptsList.FireburgSeedVendor);
			}
			else if (settings == SeedVendorSettings.RandomBosses)
			{
				var bosses = Enemizer.Bosses.Where(b => b != EnemyIds.Behemoth).ToList();
				bossesSeedVendors.Add(rng.TakeFrom(bosses), TalkScriptsList.FireburgSeedVendor);
				bossesSeedVendors.Add(rng.TakeFrom(bosses), TalkScriptsList.WindiaSeedVendor);
			}
		}

		public string GetFlagScript(EnemyIds id)
		{
			if (bossesSeedVendors.TryGetValue(id, out var talkscript))
			{
				return scripts[talkscript];
			}
			else
			{
				return "";
			}
		}
		public string GetBossString(TalkScriptsList id)
		{
			if (seedVendorsBosses.TryGetValue(id, out var enemyid))
			{
				return bossesNames[enemyid];
			}
			else
			{
				return "";
			}
		}
	}

	public partial class FFMQRom : SnesRom
	{
		public void UpdateSeedVendorScripts(SeedVendorSettings settings, MT19337 rng)
		{
			if (settings == SeedVendorSettings.Standard)
			{
				return;
			}

			string seedShopAddress = "0DFC03";

			if (settings == SeedVendorSettings.NoVendors)
			{
				List<TalkScriptsList> otherShops = new()
				{
					TalkScriptsList.AquariaExplosiveVendor,
					TalkScriptsList.PotionVendor,
				};

				MapObjects[0x31][0x08].Value = (byte)rng.PickFrom(otherShops);
				MapObjects[0x50][0x02].Value = (byte)rng.PickFrom(otherShops);
				MapObjects[0x64][0x04].Value = (byte)TalkScriptsList.AquariaExplosiveVendor; // Mac Ship Vendor
			}
			else if (settings == SeedVendorSettings.DefeatJinn)
			{
				GameFlags[GameFlagIds.FireburgSeedQuest] = false;
				
				// Update only the Fireburg Seed Vendor
				TalkScripts.AddScript((int)TalkScriptsList.FireburgSeedVendor,
					new ScriptBuilder(new List<string>{
						$"2E{(int)GameFlagIds.FireburgSeedQuest:X2}[02]",
						MQText.TextToHex("The Mines are a treacherous place, my Seed supplier won't go back there until Jinn isn't a menace anymore.") + "00",
						"0502" + seedShopAddress + "00"
					}));

				MapObjects[0x31][0x08].Value = (byte)TalkScriptsList.FireburgSeedVendor;
			}
			else if (settings == SeedVendorSettings.RandomBosses)
			{
				GameFlags[GameFlagIds.FireburgSeedQuest] = false;
				GameFlags[GameFlagIds.WindiaSeedQuest] = false;
				GameFlags[GameFlagIds.WindiaSeedPending] = true;

				// Update the Fireburg Seed Vendor
				TalkScripts.AddScript((int)TalkScriptsList.FireburgSeedVendor,
					new ScriptBuilder(new List<string>{
						$"2E{(int)GameFlagIds.FireburgSeedQuest:X2}[02]",
						MQText.TextToHex($"My Seed supplier is blocked by {SeedVendors.GetBossString(TalkScriptsList.FireburgSeedVendor)}, someone should put them back in their place!") + "00",
						"0502" + seedShopAddress + "00"
					}));

				MapObjects[0x31][0x08].Value = (byte)TalkScriptsList.FireburgSeedVendor;

				// Update the Windia Seed Vendor
				TalkScripts.AddScript((int)TalkScriptsList.WindiaSeedVendor,
					new ScriptBuilder(new List<string>{
						MQText.TextToHex($"If only {SeedVendors.GetBossString(TalkScriptsList.WindiaSeedVendor)} wouldn't be blocking the way.\nThen I could gather seeds again.") + "00",
					}));

				MapObjects[0x52][0x06].Value = (byte)TalkScriptsList.WindiaSeedVendor;
				MapObjects[0x52][0x06].Gameflag = (byte)GameFlagIds.WindiaSeedPending;
				MapObjects[0x50][0x02].Gameflag = (byte)GameFlagIds.WindiaSeedQuest;
			}
		}
	}
}
