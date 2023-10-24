using RomUtilities;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks.Dataflow;
using System.Runtime.Intrinsics.Arm;

namespace FFMQLib
{
    public class TileScriptsManager : GameScriptManager
    {
		private const int TileScriptsBank = 0x03;
        private const int TileScriptsPointers = 0xbb81;
        private const int TileScriptOffset = 0xbc21;
        private const int TileScriptEndOffset = 0xd280;
        private const int TileScriptQty = 0x50;

        private const int ExpansionBank = 0x14;
        private const int ExpansionOffset = 0x8300;
        private const int newTileScriptQty = 0x100;

        public TileScriptsManager(FFMQRom rom) : base (rom, TileScriptsBank, TileScriptsPointers, TileScriptQty, TileScriptOffset, TileScriptEndOffset)
		{
            ExpandQuantity(ExpansionBank, ExpansionOffset, newTileScriptQty);
        }
        public override void Write(FFMQRom rom)
        {
            ExpansionCode(rom);
            InternalWrite(rom);
        }
        private void ExpansionCode(FFMQRom rom)
		{
			rom.PutInBank(0x01, 0xB36C, Blob.FromHex("2280821460eaeaeaeaeaeaeaeaeaea"));
            rom.PutInBank(0x14, 0x8280, Blob.FromHex("adee1938e900088d2000c95000b0045ca49b00080bc230a900005be220a9148519c230a52038e950000aaabf00831485175cbc9b006b"));
        }
    }

    public class TalkScriptsManager : GameScriptManager
    {
        private const int TalkScriptsBank = 0x03;
        private const int TalkScriptsPointers = 0xd5e5;
        private const int TalkScriptOffset = 0xd6dd;
        private const int TalkScriptEndOffset = 0xf811;
        private const int TalkScriptQty = 0x7C;

        private const int ExpansionBank = 0x14;
        private const int ExpansionOffset = 0xA000;
        private const int newTileScriptQty = 0x100;

        public TalkScriptsManager(FFMQRom rom) : base(rom, TalkScriptsBank, TalkScriptsPointers, TalkScriptQty, TalkScriptOffset, TalkScriptEndOffset)
        {
 
        }
        public override void Write(FFMQRom rom)
        {
             InternalWrite(rom);
        }
    }

    public class GameScriptManager
	{
		private List<ScriptEntry> _scripts;
        //private List<(int, ScriptEntry)> _scripts2;
        private Dictionary<int, ScriptBuilder> _newScripts;
		private List<int> _enableMoveScripts;
		private List<(int callid, int scriptid)> _callIds;
		private List<int> forceToExpansion;

		private int Bank;
        private int PointersPosition;
		private int PointersQty;
		private int ScriptsStart;
		private int ScriptsEnd;

		private int expansionBank;
		private int expansionOffset;
		private int expansionQty;
		
		public class ScriptEntry
		{
			public int Id { get; set; }
			public byte[] Script { get; set; }
			public int Pointer { get; set; }
			public List<int> Positions { get; set; }
            public ushort Position { get; set; }

            public ScriptEntry(byte[] script, int pointer, int position)
			{
				Script = script;
				Pointer = pointer;
				Positions = new();
				Positions.Add(position);
			}
            public ScriptEntry(int id, byte[] script, ushort position)
            {
				Id = id;
				Script = script;
				Position = position;
            }
        }

		public ScriptEntry Scripts(int index) => _scripts.Find(x => x.Positions.Contains(index));

		public GameScriptManager(FFMQRom rom, int bank, int pointersPosition, int pointersQty, int scriptsStart, int scriptsEnd)
		{
			LoadData(rom, bank, pointersPosition, pointersQty, scriptsStart, scriptsEnd);
		}

		private void LoadData(FFMQRom rom, int bank, int pointersPosition, int pointersQty, int scriptsStart, int scriptsEnd)
		{

            Bank = bank;
            PointersPosition = pointersPosition;
            PointersQty = pointersQty;
            ScriptsStart = scriptsStart;
            ScriptsEnd = scriptsEnd;
            _scripts = new();
            _newScripts = new Dictionary<int, ScriptBuilder>();
            _enableMoveScripts = new();
            forceToExpansion = new();
			_callIds = new();

            ushort currentScriptPointer = (ushort)ScriptsStart;
            ushort nextScriptPointer;
            int scriptCount = 0;

            List<ushort> scriptPointers = rom.GetFromBank(Bank, PointersPosition, PointersQty * 2).ToUShorts().ToList();

            for (int i = 0; i < scriptPointers.Count; i++)
            {
                if (i == scriptPointers.Count - 1)
                {
                    nextScriptPointer = (ushort)ScriptsEnd;
                }
                else
                {
                    nextScriptPointer = scriptPointers[i + 1];
                }

                _callIds.Add((i, scriptCount));

                if (currentScriptPointer != nextScriptPointer)
                {
                    _scripts.Add(new ScriptEntry(scriptCount, rom.GetFromBank(Bank, currentScriptPointer, nextScriptPointer - currentScriptPointer), currentScriptPointer));
                    scriptCount++;
                }

                currentScriptPointer = nextScriptPointer;
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
        public void ForceScriptToExpansion(int index)
        {
            forceToExpansion.Add(index);
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
		public void ExpandQuantity(int newBank, int newOffset, int newQty)
		{
			expansionBank = newBank;
			expansionOffset = newOffset;
			expansionQty = newQty;
		}

		public virtual void Write(FFMQRom rom)
		{
			InternalWrite(rom);
		}
        public void InternalWrite(FFMQRom rom)
		{
			ushort offset = (ushort)(PointersPosition + (PointersQty * 2));
			ushort expansionoffset = (ushort)(expansionOffset + ((expansionQty - PointersQty) * 2));

			List<byte> scripts = new();
            List<ushort> pointers = new();
            List<ushort> expansionPointers = new();
            List<byte> expansionScripts = new();

            foreach (var script in _scripts)
			{
                var changedscripts = _newScripts.Where(x => _callIds.Where(x => x.scriptid == script.Id).Select(x => x.callid).Contains(x.Key)).Select(y => y.Value);
				int padTo = 0;

				if (changedscripts.Any())
				{
					if (forceToExpansion.Intersect(_callIds.Where(x => x.scriptid == script.Id).Select(x => x.callid)).Any())
					{
						var newScript = changedscripts.First().Update(expansionoffset);
						script.Script = new byte[] { 0x07, (byte)(expansionoffset % 0x100), (byte)(expansionoffset / 0x100), (byte)expansionBank, 0x00 };
                        
						expansionScripts.AddRange(newScript);
						expansionoffset += (ushort)newScript.Length;

                        pointers.Add(offset);
                        offset += (ushort)script.Script.Length;
                    }
					else
					{
                        script.Script = changedscripts.First().Update(offset);
                        pointers.Add(offset);
                        offset += (ushort)script.Script.Length;
                    }
				}
				else if (_enableMoveScripts.Intersect(_callIds.Where(x => x.scriptid == script.Id).Select(x => x.callid)).Any())
                {
                    pointers.Add(offset);
                    offset += (ushort)script.Script.Length;
                }
                else
                {
					padTo = script.Position - offset;
                    offset = (ushort)script.Position;
                    pointers.Add(offset);
                    offset += (ushort)script.Script.Length;
                }

				scripts.AddRange(Enumerable.Repeat((byte)0x00, padTo));
                scripts.AddRange(script.Script);
            }

			var expansionScript = _newScripts.Where(x => x.Key >= PointersQty).OrderBy(x => x.Key).ToList();

			for (int i = PointersQty; i < expansionQty; i++)
			{
				var validscripts = expansionScript.Where(x => x.Key == i).ToList();

				if (validscripts.Any())
				{
                    var newScript = validscripts.First().Value.Update(expansionoffset);

                    expansionScripts.AddRange(newScript);
                    expansionPointers.Add(expansionoffset);
                    expansionoffset += (ushort)newScript.Length;
                }
				else
				{
                    expansionPointers.Add(expansionoffset);
                }
			}


			List<ushort> actualPointers = new();
			
			foreach (var callid in _callIds)
			{
				actualPointers.Add((ushort)pointers[callid.scriptid]);
            }

			rom.PutInBank(Bank, PointersPosition, Blob.FromUShorts(actualPointers.ToArray()));
            rom.PutInBank(Bank, ScriptsStart, scripts.ToArray());
			if (expansionPointers.Any())
			{
                rom.PutInBank(expansionBank, expansionOffset, Blob.FromUShorts(expansionPointers.ToArray()));
                rom.PutInBank(expansionBank, (expansionOffset + ((expansionQty - PointersQty) * 2)), expansionScripts.ToArray());
            }
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
		public int Size()
		{
			return (_scriptSeries.SelectMany(s => s).Count() / 2);
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
			{ Items.Wakewater, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.WakeWater:X2}") },
			{ Items.VenusKey, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.VenusKey:X2}") },
			{ Items.MultiKey, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.MultiKey:X2}") },
			{ Items.Mask, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.Mask:X2}") },
			{ Items.MagicMirror, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.MagicMirror:X2}") },
			{ Items.ThunderRock, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.ThunderRock:X2}") },
			{ Items.CaptainsCap, ($"{(int)FlagPositions.Items:X4}", $"{(int)ItemFlags.CaptainsCap:X2}") },
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
			{ Items.CupidLocket, ($"{(int)FlagPositions.Armors:X4}", $"{(int)ArmorFlags.CupidLocket:X2}") },

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
