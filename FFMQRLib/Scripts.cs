using RomUtilities;
using System.Collections.Generic;
using System.Linq;
using System;


namespace FFMQLib
{
	public class GameScriptManager
	{
		private List<ScriptEntry> _scripts;
		private Dictionary<int, ScriptBuilder> _newScripts;
		private List<int> _enableMoveScripts;
		private int PointersPosition;
		private int PointersQty;
		private int ScriptsStart;
		private int ScriptsEnd;
		public class ScriptEntry
		{
			public byte[] Script { get; set; }
			public int Pointer { get; set; }
			public List<int> Positions { get; set; }

			public ScriptEntry(byte[] script, int pointer, int position)
			{
				Script = script;
				Pointer = pointer;
				Positions = new();
				Positions.Add(position);
			}
		}

		public ScriptEntry Scripts(int index) => _scripts.Find(x => x.Positions.Contains(index));

		public GameScriptManager(FFMQRom rom, int pointersPosition, int pointersQty, int scriptsStart, int scriptsEnd)
		{
			PointersPosition = pointersPosition;
			PointersQty = pointersQty;
			ScriptsStart = scriptsStart;
			ScriptsEnd = scriptsEnd;
			_scripts = new();
			_newScripts = new Dictionary<int, ScriptBuilder>();
			_enableMoveScripts = new();

			List<int> ScriptsPointers = rom.Get(PointersPosition, PointersQty * 2).Chunk(2).Select(x => x[1] * 0x100 + x[0]).ToList();

			int previousScriptPointer = ScriptsPointers[ScriptsPointers.Count - 1];
			int length = ScriptsEnd - previousScriptPointer;
			_scripts.Add(new ScriptEntry(rom.GetFromBank(0x03, ScriptsPointers[ScriptsPointers.Count - 1], length), previousScriptPointer, ScriptsPointers.Count - 1));

			for (int i = (ScriptsPointers.Count-2); i >= 0; i--)
			{
				if (ScriptsPointers[i] == previousScriptPointer)
				{
					_scripts.First().Positions.Add(i);
				}
				else
				{
					length = previousScriptPointer - ScriptsPointers[i];
					_scripts.Insert(0, new ScriptEntry(rom.GetFromBank(0x03, ScriptsPointers[i], length), ScriptsPointers[i], i));
					previousScriptPointer = ScriptsPointers[i];
				}
			}
		}

		public void AddScript(int index, ScriptBuilder newscript)
		{
			_newScripts.Add(index, newscript);
		}

		public void AddMobileScript(int index)
		{
			_enableMoveScripts.Add(index);
		}
		public void Dump()
		{
			for (int i = 0; i < _scripts.Count; i++)
			{
				Console.WriteLine("---------------");
				string rawScriptString = String.Join("", _scripts[i].Script.Select(p => p.ToString("X2")).ToArray());
				Console.WriteLine(rawScriptString);
			}
		}
		public void Write(FFMQRom rom)
		{
			int offset = ScriptsStart;
			byte[] scripts = new byte[ScriptsEnd - ScriptsStart];
			byte[] pointers = new byte[0];

			for (int i = 0; i < _scripts.Count; i++)
			{
				var changedscripts = _newScripts.Where(x => _scripts[i].Positions.Contains(x.Key)).Select(y => y.Value);

				if (changedscripts.Any())
				{
					//Console.WriteLine(offset);
					_scripts[i].Script = changedscripts.First().Update(offset);
					_scripts[i].Pointer = offset;
					offset += _scripts[i].Script.Length;
					//Console.WriteLine(String.Concat(Array.ConvertAll(_scripts[i].Script, x => x.ToString("X2"))));
				}
				else if (_scripts[i].Positions.Where(x => _enableMoveScripts.Contains(x)).Any())
				{
					_scripts[i].Pointer = offset;
					offset += _scripts[i].Script.Length;
				}
				else
				{
					offset = _scripts[i].Pointer;
					offset += _scripts[i].Script.Length;
				}

				for (int j = 0; j < _scripts[i].Script.Length; j++)
				{
					scripts[_scripts[i].Pointer - ScriptsStart + j] = _scripts[i].Script[j];
				}
			}

			for (int i = 0; i < PointersQty; i++)
			{
				var targetScript = _scripts.Find(x => x.Positions.Contains(i));
				pointers = pointers.Concat(new byte[] { (byte)(targetScript.Pointer % 0x100), (byte)(targetScript.Pointer / 0x100) }).ToArray();
			}

			rom.Put(PointersPosition, pointers);
			rom.PutInBank(0x03, ScriptsStart, scripts);
		}
	}


	public class ScriptBuilder
	{
		private List<string> _scriptSeries;
		private List<int> _scriptAddresses;
		private int _address;
		private const int ScriptsBank = 0x03;

		public ScriptBuilder(int address = 0x0000)
		{

			_scriptSeries = new List<string>();
			_scriptAddresses = new List<int>();
			_address = address;
		}

		public ScriptBuilder(List<string> scripts)
		{
			_address = 0x0000;
			_scriptSeries = new List<string>();
			_scriptAddresses = new List<int>();

			foreach (var script in scripts)
			{
				Add(script);
			}
		}

		public void Add(string script)
		{
			if (_scriptSeries.Count == 0)
			{
				_scriptAddresses.Add(_address);
			}
			else
			{
				_scriptAddresses.Add(_scriptAddresses.Last() + (_scriptSeries.Last().Length / 2));
			}

			_scriptSeries.Add(script);
		}
		public byte[] Update(int newaddress)
		{
			for (int i = 0; i < _scriptAddresses.Count; i++)
			{
				_scriptAddresses[i] += (newaddress - _address);
			}

			_address = newaddress;

			return OutputBlob();
		}
		private byte[] OutputBlob()
		{
			for (int i = 0; i < _scriptSeries.Count; i++)
			{
				for (int j = 0; j < _scriptSeries[i].Length; j++)
				{
					if (_scriptSeries[i][j] == '[')
					{
						int targetScript = int.Parse(_scriptSeries[i].Substring(j + 1, 2), System.Globalization.NumberStyles.Integer);
						string targetJump = _scriptAddresses[targetScript].ToString("X4");
						string targetJumpInverse = targetJump.Substring(2, 2) + targetJump.Substring(0, 2);
						_scriptSeries[i] = _scriptSeries[i].Remove(j, 4).Insert(j, targetJumpInverse);
					}
				}
			}

			string blobToPut = String.Join("", _scriptSeries.SelectMany(x => String.Join("", x)));

			return Blob.FromHex(blobToPut);
		}
		public void Write(FFMQRom rom)
		{
			rom.PutInBank(ScriptsBank, _address, OutputBlob());
		}
		public void WriteAt(int bank, int address, FFMQRom rom)
		{
			rom.PutInBank(bank, address, Update(address));
		}
	}

	public partial class FFMQRom : SnesRom
	{





		public Dictionary<Items, (string, string)> ScriptItemFlags = new()
		{
			{ Items.Elixir, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.Elixir:X2}") },
			{ Items.TreeWither, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.TreeWither:X2}") },
			{ Items.WakeWater, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.WakeWater:X2}") },
			{ Items.VenusKey, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.VenusKey:X2}") },
			{ Items.MultiKey, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.MultiKey:X2}") },
			{ Items.Mask, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.Mask:X2}") },
			{ Items.MagicMirror, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.MagicMirror:X2}") },
			{ Items.ThunderRock, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.ThunderRock:X2}") },
			{ Items.CaptainCap, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.CaptainCap:X2}") },
			{ Items.LibraCrest, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.LibraCrest:X2}") },
			{ Items.GeminiCrest, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.GeminiCrest:X2}") },
			{ Items.MobiusCrest, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.MobiusCrest:X2}") },
			{ Items.SandCoin, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.SandCoin:X2}") },
			{ Items.RiverCoin, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.RiverCoin:X2}") },
			{ Items.SunCoin, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.SunCoin:X2}") },
			{ Items.SkyCoin, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.SkyCoin:X2}") },

			{ Items.ExitBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.ExitBook:X2}") },
			{ Items.CureBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.CureBook:X2}") },
			{ Items.HealBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.HealBook:X2}") },
			{ Items.LifeBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.LifeBook:X2}") },
			{ Items.QuakeBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.QuakeBook:X2}") },
			{ Items.BlizzardBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.BlizzardBook:X2}") },
			{ Items.FireBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.FireBook:X2}") },
			{ Items.AeroBook, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.AeroBook:X2}") },
			{ Items.ThunderSeal, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.ThunderSeal:X2}") },
			{ Items.WhiteSeal, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.WhiteSeal:X2}") },
			{ Items.MeteorSeal, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.MeteorSeal:X2}") },
			{ Items.FlareSeal, ($"{(int)FlagPositions.Spells:X4}", $"{(int)SpellFlags.FlareSeal:X2}") },

			{ Items.SteelSword, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.SteelSword:X2}") },
			{ Items.KnightSword, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.KnightSword:X2}") },
			{ Items.Excalibur, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.Excalibur:X2}") },
			{ Items.Axe, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.Axe:X2}") },
			{ Items.BattleAxe, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.BattleAxe:X2}") },
			{ Items.GiantsAxe, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.GiantsAxe:X2}") },
			{ Items.CatClaw, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.CatClaw:X2}") },
			{ Items.CharmClaw, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.CharmClaw:X2}") },
			{ Items.DragonClaw, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.DragonClaw:X2}") },
			{ Items.Bomb, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.Bomb:X2}") },
			{ Items.JumboBomb, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.JumboBomb:X2}") },
			{ Items.MegaGrenade, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.MegaGrenade:X2}") },
			{ Items.MorningStar, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.MorningStar:X2}") },
			{ Items.BowOfGrace, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.BowOfGrace:X2}") },
			{ Items.NinjaStar, ($"{(int)FlagPositions.Weapons:X4}", $"{(int)WeaponFlags.NinjaStar:X2}") },

			{ Items.SteelHelm, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.SteelHelm:X2}") },
			{ Items.MoonHelm, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.MoonHelm:X2}") },
			{ Items.ApolloHelm, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.ApolloHelm:X2}") },
			{ Items.SteelArmor, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.SteelArmor:X2}") },
			{ Items.NobleArmor, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.NobleArmor:X2}") },
			{ Items.GaiasArmor, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.GaiasArmor:X2}") },
			{ Items.ReplicaArmor, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.ReplicaArmor:X2}") },
			{ Items.MysticRobes, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.MysticRobes:X2}") },
			{ Items.FlameArmor, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.FlameArmor:X2}") },
			{ Items.BlackRobe, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.BlackRobe:X2}") },
			{ Items.SteelShield, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.SteelShield:X2}") },
			{ Items.VenusShield, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.VenusShield:X2}") },
			{ Items.AegisShield, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.AegisShield:X2}") },
			{ Items.EtherShield, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.EtherShield:X2}") },
			{ Items.Charm, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.Charm:X2}") },
			{ Items.MagicRing, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.MagicRing:X2}") },
			{ Items.CupidLock, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.CupidLock:X2}") },

		};
		public class ScriptCommands {

			public byte[] SetGameflagTypeWeapons = { 0x2d, 0x32, 0x10 };
			public byte[] CheckGameflagJumpIfTrue = { 0x05, 0x0c };
			public byte[] DoAnimation = { 0x2a };
			public byte[] TextBoxBenjamin = { 0x1a, 0x84 };
			public byte[] TextBoxCompanion = { 0x1a, 0x85 };
		}

	}
}
