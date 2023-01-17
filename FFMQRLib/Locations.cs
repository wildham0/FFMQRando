using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum BattlesQty : int
	{
		[Description("10")]
		Ten = 0,
		[Description("7")]
		Seven,
		[Description("5")]
		Five,
		[Description("3")]
		Three,
		[Description("1")]
		One,
		[Description("Random 1-10")]
		RandomHigh,
		[Description("Random 1-5")]
		RandomLow,

	}

	public class ChestLocation
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public byte AccessRequirements { get; set; }

		public ChestLocation(int mapobjid, int mapid, byte access)
		{
			ObjectId = mapobjid;
			MapId = mapid;
			AccessRequirements = access;
		}
	}

	public class TreasureObject
	{
		public int ObjectId { get; set; }
		public int MapId { get; set; }
		public Locations Location { get; set; }
		public MapRegions Region { get; set; }
		public Items Content { get; set; }
		public TreasureType Type { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public bool Accessible { get; set; }
		public string Name { get; set; }

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = "";
			Region = ItemLocations.ReturnRegion(location);
		}

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, string name, List<List<AccessReqs>> access)
		{
			ObjectId = mapobjid;
			Content = (Items)0xFF;
			Location = location;
			Type = type;
			MapId = mapid;
			IsPlaced = false;
			Prioritize = false;
			Exclude = false;
			AccessRequirements = access;
			Accessible = false;
			Name = name;
			Region = ItemLocations.ReturnRegion(location);
		}

		public TreasureObject(TreasureObject treasure)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible = treasure.Accessible;
			Name = treasure.Name;
			Region = treasure.Region;
		}

		public TreasureObject(TreasureObject treasure, string name)
		{
			ObjectId = treasure.ObjectId;
			Content = treasure.Content;
			Location = treasure.Location;
			Type = treasure.Type;
			MapId = treasure.MapId;
			IsPlaced = treasure.IsPlaced;
			Prioritize = treasure.Prioritize;
			Exclude = treasure.Exclude;
			AccessRequirements = treasure.AccessRequirements.ToList();
			Accessible= treasure.Accessible;
			Region = treasure.Region;
			Name = treasure.Name == "" ? name : treasure.Name;
		}
	}
	public class GameFlags
	{
		private const int StartingGameFlags = 0x0653A4;
		private byte[] _gameflags;
		private List<(int, int)> BitPos = new List<(int,int)> { (0,0x80), (1,0x40), (2,0x20), (3,0x10), (4,0x08), (5,0x04), (6,0x02), (7,0x01) };

		public GameFlags(FFMQRom rom)
		{
			_gameflags = rom.Get(StartingGameFlags, 0x20);

		}

		public void Write(FFMQRom rom)
		{
			rom.Put(StartingGameFlags, _gameflags);
		}
		public bool this[int flag]
		{
			get => HexToFlag(flag);
			set => FlagToHex(flag, value);
		}

		private bool HexToFlag(int flag)
		{
			var targetbyte = flag / 8;
			var targetbit = BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			return (_gameflags[targetbyte] & targetbit) == targetbit;
		}

		private void FlagToHex(int flag, bool value)
		{
			CustomFlagToHex(_gameflags, flag, value);
		}

		public void CustomFlagToHex(byte[] flagarray, int flag, bool value)
		{
			var targetbyte = flag / 8;
			var targetbit = (byte)BitPos.Find(x => x.Item1 == (flag & 0x07)).Item2;

			if (value)
			{
				flagarray[targetbyte] |= targetbit;
			}
			else
			{
				flagarray[targetbyte] &= (byte)~targetbit;
			}
		}
	}
	public class Battlefields
	{
		private const int BattlefieldsRewardsBank = 0x07;
		private const int BattlefieldsRewardsOffset = 0xEFA1;

		private const int BattlefieldsQty = 0x14;

		private List<byte> _battlesQty;
		private List<Blob> _rewards;

		public List<Locations> BattlefieldsWithItem;

		public Battlefields(FFMQRom rom)
		{
			_battlesQty = rom.GetFromBank(0x0C, 0xD4D0, BattlefieldsQty).Chunk(1).Select(x => x[0]).ToList();
			_rewards = rom.GetFromBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, BattlefieldsQty * 2).Chunk(2);

			BattlefieldsWithItem = new();

			for (int i = 0; i < _battlesQty.Count; i++)
			{
				if ((BattlefieldRewardType)(_rewards[i][1] & 0b1100_0000) == BattlefieldRewardType.Item)
				{
					BattlefieldsWithItem.Add((Locations)(i + 1));
				}
			}
		}
		public void PlaceItems(ItemsPlacement itemsPlacement)
		{
			var battlefieldsWithItem = itemsPlacement.ItemsLocations.Where(x => x.Type == TreasureType.Battlefield && x.Content != Items.None).ToList();

			foreach (var battlefield in battlefieldsWithItem)
			{
				_rewards[(int)(battlefield.Location - 1)][0] = (byte)battlefield.Content;
			}
		}
		public void ShuffleBattelfieldRewards(bool enable, Overworld overworld, MT19337 rng)
		{
			if (!enable)
			{
				return;
			}
			
			_rewards.Shuffle(rng);

			BattlefieldsWithItem.Clear();

			for (int i = 0; i < _battlesQty.Count; i++)
			{
				if ((BattlefieldRewardType)(_rewards[i][1] & 0b1100_0000) == BattlefieldRewardType.Item)
				{
					BattlefieldsWithItem.Add((Locations)(i + 1));
				}
			}

			overworld.UpdateBattlefieldsColor(this);
		}
		public BattlefieldRewardType GetRewardType(Locations targetBattlefield)
		{
			return (BattlefieldRewardType)(_rewards[(int)(targetBattlefield - 1)][1] & 0b1100_0000);
		}
		public List<BattlefieldRewardType> GetAllRewardType()
		{
			return _rewards.Select(x => (BattlefieldRewardType)(x[1] & 0b1100_0000)).ToList();
		}
		public void SetBattlesQty(BattlesQty battlesqty, MT19337 rng)
		{
			int battleQty = 10;
			bool randomQty = false;

			switch (battlesqty)
			{
				case BattlesQty.Ten: return;
				case BattlesQty.Seven: battleQty = 7; break;
				case BattlesQty.Five: battleQty = 5; break;
				case BattlesQty.Three: battleQty = 3; break;
				case BattlesQty.One: battleQty = 1; break;
				case BattlesQty.RandomHigh: battleQty = 10; randomQty = true; break;
				case BattlesQty.RandomLow: battleQty = 5; randomQty = true; break;
			}

			for (int i = 0; i < _battlesQty.Count; i++)
			{ 
				_battlesQty[i] = randomQty ? (byte)rng.Between(1, battleQty) : (byte)battleQty;
			}
		}

		public void Write(FFMQRom rom)
		{
			rom.PutInBank(0x0C, 0xD4D0, _battlesQty.ToArray());
			rom.PutInBank(BattlefieldsRewardsBank, BattlefieldsRewardsOffset, _rewards.SelectMany(x => x.ToBytes()).ToArray());
		}
	
	}
	public class NodeLocations
	{
		
		private List<Blob> _movementArrows = new();
		public List<Node> Nodes = new();
		private List<byte> ShipWestSteps = new();
		private List<byte> ShipEastSteps = new();
		private List<Room> Rooms;
	
		private const int OWMovementBank = 0x07;
		private const int OWMovementArrows = 0xEE84;
		private const int OWWalkStepsPointers = 0xF011;
		private const int OwNodesQty = 0x38;
		private const int NewOWWalkStepsBank = 0x12;
		private const int NewOWWalkStepsPointers = 0x9000;

		private const int OWLocationActionsOffset = 0xEFA1;
		private const int OWLocationQty = 0x38;

		private const int OWExitCoordBank = 0x07;
		private const int OWExitCoordOffset = 0xF7C3;
		

		public NodeLocations(FFMQRom rom, List<Room> rooms)
		{
			Rooms = rooms;

			_movementArrows = rom.GetFromBank(OWMovementBank, OWMovementArrows, (OwNodesQty + 1) * 0x05).Chunk(5);
			ShipWestSteps = rom.GetFromBank(OWMovementBank, 0xF234, 6).ToBytes().ToList();
			ShipEastSteps = rom.GetFromBank(OWMovementBank, 0xF23A, 6).ToBytes().ToList();
			var exitCoords = rom.GetFromBank(OWExitCoordBank, OWExitCoordOffset, (OwNodesQty + 1) * 2).Chunk(2);

			Nodes = new();
			foreach (var arrow in _movementArrows)
			{
				Nodes.Add(new Node(arrow));
			}

			var stepspointers = rom.GetFromBank(OWMovementBank, OWWalkStepsPointers, OwNodesQty * 2).Chunk(2);

			for (int i = 0; i < OwNodesQty; i++)
			{
				int currentposition = (int)stepspointers[i].ToUShorts()[0];
				for (int j = 0; j < 4; j++)
				{
					var destination = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];

					if (destination != 0)
					{
						currentposition++;
						var currentstep = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];
						List<byte> stepList = new();

						while (currentstep > 0x7F)
						{
							stepList.Add(currentstep);
							currentposition++;
							currentstep = rom.GetFromBank(OWMovementBank, currentposition, 1)[0];
						}

						Nodes[i+1].AddDestination((Locations)destination, stepList);
					}
					else
					{
						Nodes[i+1].AddDestination((Locations)destination, new List<byte>());
						currentposition++;
					}
				}
			}

			var locationactions = rom.GetFromBank(OWMovementBank, OWLocationActionsOffset, OWLocationQty * 2).Chunk(2);

			for (int i = 0; i < Nodes.Count; i++)
			{
				Nodes[i].Position = (exitCoords[i][0], exitCoords[i][1]);
				Nodes[i].Location = (Locations)i;
			}

			AssignOwObjects();
		}
		private void AssignOwObjects()
		{
			Nodes[(int)Locations.ForestaSouthBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaSouthBattlefield, OverworldMapObjects.ForestaSouthBattlefieldCleared };
			Nodes[(int)Locations.ForestaSouthBattlefield].TargetTeleporter = (115, 1);

			Nodes[(int)Locations.ForestaWestBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaWestBattlefield, OverworldMapObjects.ForestaWestBattlefieldCleared };
			Nodes[(int)Locations.ForestaWestBattlefield].TargetTeleporter = (116, 1);

			Nodes[(int)Locations.ForestaEastBattlefield].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaEastBattlefield, OverworldMapObjects.ForestaEastBattlefieldCleared };
			Nodes[(int)Locations.ForestaEastBattlefield].TargetTeleporter = (117, 1);

			Nodes[(int)Locations.AquariaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield01, OverworldMapObjects.AquariaBattlefield01Cleared };
			Nodes[(int)Locations.AquariaBattlefield01].TargetTeleporter = (118, 1);
			Nodes[(int)Locations.AquariaBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook }
			};

			Nodes[(int)Locations.AquariaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield02, OverworldMapObjects.AquariaBattlefield02Cleared };
			Nodes[(int)Locations.AquariaBattlefield02].TargetTeleporter = (119, 1);
			Nodes[(int)Locations.AquariaBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.AquariaBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaBattlefield03, OverworldMapObjects.AquariaBattlefield03Cleared };
			Nodes[(int)Locations.AquariaBattlefield03].TargetTeleporter = (120, 1);
			Nodes[(int)Locations.AquariaBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.WintryBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield01, OverworldMapObjects.WintryBattlefield01Cleared };
			Nodes[(int)Locations.WintryBattlefield01].TargetTeleporter = (121, 1);
			Nodes[(int)Locations.WintryBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.WintryBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryBattlefield02, OverworldMapObjects.WintryBattlefield02Cleared };
			Nodes[(int)Locations.WintryBattlefield02].TargetTeleporter = (122, 1);
			Nodes[(int)Locations.WintryBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.PyramidBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PyramidBattlefield01, OverworldMapObjects.PyramidBattlefield01Cleared };
			Nodes[(int)Locations.PyramidBattlefield01].TargetTeleporter = (123, 1);
			Nodes[(int)Locations.PyramidBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.LibraBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield01, OverworldMapObjects.LibraBattlefield01Cleared };
			Nodes[(int)Locations.LibraBattlefield01].TargetTeleporter = (124, 1);
			Nodes[(int)Locations.LibraBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.LibraBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraBattlefield02, OverworldMapObjects.LibraBattlefield02Cleared };
			Nodes[(int)Locations.LibraBattlefield02].TargetTeleporter = (125, 1);
			Nodes[(int)Locations.LibraBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.FireburgBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield01, OverworldMapObjects.FireburgBattlefield01Cleared };
			Nodes[(int)Locations.FireburgBattlefield01].TargetTeleporter = (126, 1);
			Nodes[(int)Locations.FireburgBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.FireburgBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield02, OverworldMapObjects.FireburgBattlefield02Cleared };
			Nodes[(int)Locations.FireburgBattlefield02].TargetTeleporter = (127, 1);
			Nodes[(int)Locations.FireburgBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.FireburgBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgBattlefield03, OverworldMapObjects.FireburgBattlefield03Cleared };
			Nodes[(int)Locations.FireburgBattlefield03].TargetTeleporter = (128, 1);
			Nodes[(int)Locations.FireburgBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.MineBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield01, OverworldMapObjects.MineBattlefield01Cleared };
			Nodes[(int)Locations.MineBattlefield01].TargetTeleporter = (129, 1);
			Nodes[(int)Locations.MineBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.MineBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield02, OverworldMapObjects.MineBattlefield02Cleared };
			Nodes[(int)Locations.MineBattlefield02].TargetTeleporter = (130, 1);
			Nodes[(int)Locations.MineBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.MineBattlefield03].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MineBattlefield03, OverworldMapObjects.MineBattlefield03Cleared };
			Nodes[(int)Locations.MineBattlefield03].TargetTeleporter = (131, 1);
			Nodes[(int)Locations.MineBattlefield03].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.VolcanoBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoBattlefield01, OverworldMapObjects.VolcanoBattlefield01Cleared };
			Nodes[(int)Locations.VolcanoBattlefield01].TargetTeleporter = (132, 1);
			Nodes[(int)Locations.VolcanoBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.DualheadHydra },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.WindiaBattlefield01].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield01, OverworldMapObjects.WindiaBattlefield01Cleared };
			Nodes[(int)Locations.WindiaBattlefield01].TargetTeleporter = (133, 1);
			Nodes[(int)Locations.WindiaBattlefield01].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.WindiaBattlefield02].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaBattlefield02, OverworldMapObjects.WindiaBattlefield02Cleared };
			Nodes[(int)Locations.WindiaBattlefield02].TargetTeleporter = (134, 1);
			Nodes[(int)Locations.WindiaBattlefield02].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.HillOfDestiny].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.HillOfDestiny };

			Nodes[(int)Locations.LevelForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LevelForestMarker };
			Nodes[(int)Locations.LevelForest].TargetTeleporter = (25, 0);

			Nodes[(int)Locations.Foresta].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ForestaVillage };
			Nodes[(int)Locations.Foresta].TargetTeleporter = (31, 0);

			Nodes[(int)Locations.SandTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SandTempleCave };
			Nodes[(int)Locations.SandTemple].TargetTeleporter = (36, 0);

			Nodes[(int)Locations.BoneDungeon].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.BoneDungeon };
			Nodes[(int)Locations.BoneDungeon].TargetTeleporter = (37, 0);

			Nodes[(int)Locations.FocusTowerSouth].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.FocusTowerSouth].TargetTeleporter = (2, 6);

			Nodes[(int)Locations.FocusTowerWest].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.FocusTowerWest].TargetTeleporter = (4, 6);
			Nodes[(int)Locations.FocusTowerWest].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.LibraTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LibraTempleCave };
			Nodes[(int)Locations.LibraTemple].TargetTeleporter = (13, 6);
			Nodes[(int)Locations.LibraTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest }
			};

			Nodes[(int)Locations.Aquaria].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AquariaVillage };
			Nodes[(int)Locations.Aquaria].TargetTeleporter = (8, 6);
			Nodes[(int)Locations.Aquaria].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.WintryCave].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryCave };
			Nodes[(int)Locations.WintryCave].TargetTeleporter = (49, 0);
			Nodes[(int)Locations.WintryCave].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.LifeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LifeTempleCave };
			Nodes[(int)Locations.LifeTemple].TargetTeleporter = (14, 6);
			Nodes[(int)Locations.LifeTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.LibraCrest, AccessReqs.LibraTemple },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple, AccessReqs.LibraCrest, AccessReqs.LibraTemple }
			};

			Nodes[(int)Locations.FallsBasin].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FallBasinMarker };
			Nodes[(int)Locations.FallsBasin].TargetTeleporter = (53, 0);
			Nodes[(int)Locations.FallsBasin].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.IcePyramid].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.IcePyramid };
			Nodes[(int)Locations.IcePyramid].TargetTeleporter = (56, 0);
			Nodes[(int)Locations.IcePyramid].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.SpencersPlace].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SpencerCave };
			Nodes[(int)Locations.SpencersPlace].TargetTeleporter = (7, 6);
			Nodes[(int)Locations.SpencersPlace].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.ThunderRock, AccessReqs.Otto },
			};

			Nodes[(int)Locations.WintryTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WintryTempleCave };
			Nodes[(int)Locations.WintryTemple].TargetTeleporter = (15, 6);
			Nodes[(int)Locations.WintryTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.FocusTowerNorth].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.FocusTowerNorth].TargetTeleporter = (5, 6);
			Nodes[(int)Locations.FocusTowerNorth].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.WakeWater, AccessReqs.AquariaPlaza },
				new List<AccessReqs> { AccessReqs.RiverCoin, AccessReqs.GeminiCrest, AccessReqs.ExitBook, AccessReqs.SealedTemple }
			};

			Nodes[(int)Locations.FocusTowerEast].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.FocusTowerEast].TargetTeleporter = (6, 6);
			Nodes[(int)Locations.FocusTowerEast].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.Fireburg].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.FireburgVillage };
			Nodes[(int)Locations.Fireburg].TargetTeleporter = (9, 6);
			Nodes[(int)Locations.Fireburg].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.Mine].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.Mine };
			Nodes[(int)Locations.Mine].TargetTeleporter = (98, 0);
			Nodes[(int)Locations.Mine].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.SealedTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.SealedTempleCave };
			Nodes[(int)Locations.SealedTemple].TargetTeleporter = (16, 6);
			Nodes[(int)Locations.SealedTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.Volcano].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.VolcanoMarker };
			Nodes[(int)Locations.Volcano].TargetTeleporter = (103, 0);
			Nodes[(int)Locations.Volcano].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.LavaDome].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LavaDome };
			Nodes[(int)Locations.LavaDome].TargetTeleporter = (104, 0);
			Nodes[(int)Locations.LavaDome].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.RiverCoin },
				new List<AccessReqs> { AccessReqs.SandCoin, AccessReqs.AquariaPlaza, AccessReqs.WakeWater, AccessReqs.DualheadHydra },
			};

			Nodes[(int)Locations.FocusTowerSouth2].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.FocusTowerSouth2].TargetTeleporter = (3, 6);
			Nodes[(int)Locations.FocusTowerSouth2].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.RopeBridge].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.RopeBridgeMarker };
			Nodes[(int)Locations.RopeBridge].TargetTeleporter = (140, 0);
			Nodes[(int)Locations.RopeBridge].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.AliveForest].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.AliveForestMarker };
			Nodes[(int)Locations.AliveForest].TargetTeleporter = (142, 0);
			Nodes[(int)Locations.AliveForest].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.GiantTree].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MovedGiantTree };
			Nodes[(int)Locations.GiantTree].TargetTeleporter = (49, 8);
			Nodes[(int)Locations.GiantTree].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.Axe },
			};

			Nodes[(int)Locations.KaidgeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.KaidgeTempleCave };
			Nodes[(int)Locations.KaidgeTemple].TargetTeleporter = (18, 6);
			Nodes[(int)Locations.KaidgeTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.Windia].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindiaVillage };
			Nodes[(int)Locations.Windia].TargetTeleporter = (10, 6);
			Nodes[(int)Locations.Windia].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.WindholeTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.WindholeTempleCave };
			Nodes[(int)Locations.WindholeTemple].TargetTeleporter = (173, 0);
			Nodes[(int)Locations.WindholeTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.MountGale].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.MountGale, OverworldMapObjects.MountGaleMarker };
			Nodes[(int)Locations.MountGale].TargetTeleporter = (174, 0);
			Nodes[(int)Locations.MountGale].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.PazuzusTower].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.PazuzuTower };
			Nodes[(int)Locations.PazuzusTower].TargetTeleporter = (184, 0);
			Nodes[(int)Locations.PazuzusTower].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin },
			};

			Nodes[(int)Locations.ShipDock].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipDockCave };
			Nodes[(int)Locations.ShipDock].TargetTeleporter = (17, 6);
			Nodes[(int)Locations.ShipDock].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.MobiusCrest },
			};

			Nodes[(int)Locations.DoomCastle].OwMapObjects = new List<OverworldMapObjects> { };
			Nodes[(int)Locations.DoomCastle].TargetTeleporter = (1, 6);
			Nodes[(int)Locations.DoomCastle].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.ShipLoaned, AccessReqs.ShipSteeringWheel },
			};

			Nodes[(int)Locations.LightTemple].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.LightTempleCave };
			Nodes[(int)Locations.LightTemple].TargetTeleporter = (19, 6);
			Nodes[(int)Locations.LightTemple].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.MobiusCrest, AccessReqs.KaidgeTemple },
			};

			Nodes[(int)Locations.MacsShip].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDock };
			Nodes[(int)Locations.MacsShip].TargetTeleporter = (37, 8);
			Nodes[(int)Locations.MacsShip].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.MobiusCrest, AccessReqs.ShipLiberated },
			};
			Nodes[(int)Locations.MacsShipDoom].OwMapObjects = new List<OverworldMapObjects> { OverworldMapObjects.ShipAtDoom };
			Nodes[(int)Locations.MacsShipDoom].TargetTeleporter = (37, 8);
			Nodes[(int)Locations.MacsShipDoom].AccessRequirements = new List<List<AccessReqs>> {
				new List<AccessReqs> { AccessReqs.SunCoin, AccessReqs.ShipLoaned, AccessReqs.ShipSteeringWheel },
			};

			Nodes.ForEach(n => n.Region = ItemLocations.Regions.Find(r => r.Item2 == n.Location).Item1);
			Nodes.ForEach(n => n.SubRegion = ItemLocations.MapSubRegions.Find(r => r.Item2 == n.Location).Item1);
		}
		public void OpenNodes()
		{
			for (int i = 0; i <= (int)Locations.PazuzusTower; i++)
			{
				for (int y = 0; y < 4; y++)
				{
					if (Nodes[i].DirectionFlags[y] != 0)
					{
						Nodes[i].DirectionFlags[y] = (int)GameFlagsList.HillCollapsed;
					}
				}
			}

			Nodes[(int)Locations.LibraBattlefield02].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.WakeWaterUsed;
			Nodes[(int)Locations.LibraTemple].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.WakeWaterUsed;
			Nodes[(int)Locations.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.WakeWaterUsed;
			Nodes[(int)Locations.VolcanoBattlefield01].DirectionFlags[(int)NodeDirections.West] = (int)GameFlagsList.VolcanoErupted;
			Nodes[(int)Locations.Volcano].DirectionFlags[(int)NodeDirections.East] = (int)GameFlagsList.VolcanoErupted;

			Nodes[(int)Locations.PazuzusTower].DirectionFlags[(int)NodeDirections.North] = 0xDC;
			Nodes[(int)Locations.SpencersPlace].DirectionFlags[(int)NodeDirections.South] = 0xDC;
		}
		public void DoomCastleShortcut()
		{
			Nodes[(int)Locations.FocusTowerSouth].DirectionFlags[(int)NodeDirections.South] = (int)GameFlagsList.HillCollapsed;
			Nodes[(int)Locations.FocusTowerSouth].Destinations[(int)NodeDirections.South] = Locations.DoomCastle;
			Nodes[(int)Locations.FocusTowerSouth].Steps[(int)NodeDirections.South] = new List<byte> { 0xC6 };

			Nodes[(int)Locations.DoomCastle].DirectionFlags[(int)NodeDirections.North] = (int)GameFlagsList.HillCollapsed;
			Nodes[(int)Locations.DoomCastle].Destinations[(int)NodeDirections.North] = Locations.FocusTowerSouth;
			Nodes[(int)Locations.DoomCastle].Steps[(int)NodeDirections.North] = new List<byte> { 0x86 };
		}
		public Locations ShuffleEntrances(MT19337 rng)
		{ 
			List<Locations> validLocations = Enum.GetValues<Locations>().ToList();
			List<Locations> invalidLocations = new() { Locations.DoomCastle, Locations.FocusTowerEast, Locations.FocusTowerNorth, Locations.FocusTowerSouth, Locations.FocusTowerSouth2, Locations.FocusTowerWest, Locations.GiantTree, Locations.HillOfDestiny, Locations.LifeTemple, Locations.LightTemple, Locations.MacsShip, Locations.MacsShipDoom, Locations.None, Locations.ShipDock, Locations.SpencersPlace };
			validLocations.RemoveAll(x => invalidLocations.Contains(x));

			Locations startingLocation = Locations.LevelForest;

			while (validLocations.Count > 1)
			{
				var loc1 = rng.TakeFrom(validLocations);
				var loc2 = rng.TakeFrom(validLocations);

				if (loc1 == Locations.LevelForest)
				{
					startingLocation = loc2;
				}
				else if(loc2 == Locations.LevelForest)
				{
					startingLocation = loc1;
				}

				SwapNodes(loc1, loc2);
			}

			return startingLocation;
		}
		public void SwapNodes(Locations locA, Locations locB)
		{
			var tempNode = new Node(Nodes[(int)locA]);
			/*
			var tempDestinations = Nodes[(int)locA].Destinations.ToList();
			var tempSteps = Nodes[(int)locA].Steps.ToList();
			var tempFlags = Nodes[(int)locA].DirectionFlags.ToList();
			var tempPosition = Nodes[(int)locA].Position;
			var tempRegion = Nodes[(int)locA].Region;
			var tempAccesReq = Nodes[(int)locA].AccessRequirements.ToList();
			*/
			Nodes[(int)locA].Destinations = Nodes[(int)locB].Destinations.ToList();
			Nodes[(int)locA].Steps = Nodes[(int)locB].Steps.ToList();
			Nodes[(int)locA].DirectionFlags = Nodes[(int)locB].DirectionFlags.ToList();
			Nodes[(int)locA].Position = Nodes[(int)locB].Position;
			Nodes[(int)locA].Region = Nodes[(int)locB].Region;
			Nodes[(int)locA].SubRegion = Nodes[(int)locB].SubRegion;
			Nodes[(int)locA].AccessRequirements = Nodes[(int)locB].AccessRequirements.ToList();


			Nodes[(int)locB].Destinations = tempNode.Destinations;
			Nodes[(int)locB].Steps = tempNode.Steps;
			Nodes[(int)locB].DirectionFlags = tempNode.DirectionFlags;
			Nodes[(int)locB].Position = tempNode.Position;
			Nodes[(int)locB].Region = tempNode.Region;
			Nodes[(int)locB].SubRegion = tempNode.SubRegion;
			Nodes[(int)locB].AccessRequirements = tempNode.AccessRequirements;

			foreach (var node in Nodes)
			{
				for(int i = 0; i < node.Destinations.Count; i++)
				{
					if (node.Destinations[i] == locA)
					{
						node.Destinations[i] = locB;
					}
					else if (node.Destinations[i] == locB)
					{
						node.Destinations[i] = locA;
					}
				}
			}
		}
		public void UpdateSprites(List<OverworldObject> owObjects)
		{
			foreach (var node in Nodes)
			{
				foreach (var owobject in node.OwMapObjects)
				{
					owObjects[(int)owobject].Position = node.Position;
				}
			}
		}
		public void Write(FFMQRom rom)
		{
			// classData.SelectMany(x => x.MagicPermissions()).ToArray()
			rom.PutInBank(OWMovementBank, OWMovementArrows, Nodes.SelectMany(x => x.DirectionFlagsArray()).ToArray());

			var currentpointer = 0x9000 + ((Nodes.Count - 1) * 2);
			List<byte> pointertable = new();
			List<byte> stepvalues = new();

			for (int i = 1; i < Nodes.Count; i++)
			{
				pointertable.AddRange(new List<byte>() { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) });
				stepvalues.AddRange(Nodes[i].StepsArray());
				currentpointer += Nodes[i].StepsArray().Length;
			}

			var shipwestpointer = new List<byte>() { (byte)(currentpointer % 0x100), (byte)(currentpointer / 0x100) };
			var shipeastpointer = new List<byte>() { (byte)((currentpointer + ShipWestSteps.Count) % 0x100), (byte)((currentpointer + ShipWestSteps.Count) / 0x100) };

			pointertable.AddRange(stepvalues);
			pointertable.AddRange(ShipWestSteps);
			pointertable.AddRange(ShipEastSteps);

			rom.PutInBank(NewOWWalkStepsBank, NewOWWalkStepsPointers, pointertable.ToArray());

			// new location for walk steps
			rom.PutInBank(0x01, 0xF148, Blob.FromHex("bf009012"));
			rom.PutInBank(0x01, 0xF150, Blob.FromHex("bf000012"));
			rom.PutInBank(0x01, 0xF532, Blob.FromHex("bf000012"));

			// new pointers for ship
			rom.PutInBank(0x01, 0xF5B4, shipwestpointer.ToArray());
			rom.PutInBank(0x01, 0xF5F4, shipeastpointer.ToArray());

			rom.PutInBank(OWExitCoordBank, OWExitCoordOffset, Nodes.SelectMany(x => new List<byte> { (byte)x.Position.x, (byte)x.Position.y }).ToArray());
		}
	}

	public enum NodeType
	{ 
		Battlefield = 0,
		Place 
	}

	public class Node
	{
		public List<byte> DirectionFlags { get; set; }
		public byte NodeName { get; set; }
		public List<List<byte>> Steps { get; set; }
		public List<Locations> Destinations { get; set; }
		public List<OverworldMapObjects> OwMapObjects { get; set; }
		public (int x, int y) Position { get; set; }
		public byte[] Action { get; set; }
		public NodeType Type { get; set; }
		public (int id, int type) TargetTeleporter { get; set; }
		public List<List<AccessReqs>> AccessRequirements { get; set; }
		public Locations Location { get; set; }
		public MapRegions Region { get; set; }
		public SubRegions SubRegion { get; set; }

		public Node(byte[] arrowvalues)
		{
			NodeName = arrowvalues[0];
			DirectionFlags = arrowvalues.ToList().GetRange(1, 4);
			Steps = new();
			Destinations = new();
			OwMapObjects = new();
			Action = new byte[] { 0x00, 0x00 };
			TargetTeleporter = (0, 0);
			Position = (0, 0);
			AccessRequirements = new() { new List<AccessReqs> { } };
			Location = Locations.None;
			Region = MapRegions.Foresta;
			SubRegion = SubRegions.Foresta;
		}
		public Node(Node node)
		{
			NodeName = node.NodeName;
			DirectionFlags = node.DirectionFlags.ToList();
			Steps = node.Steps.ToList();
			Destinations = node.Destinations.ToList();
			OwMapObjects = node.OwMapObjects.ToList();
			Action = node.Action.ToArray();
			TargetTeleporter = node.TargetTeleporter;
			AccessRequirements = node.AccessRequirements.ToList();
			Position = node.Position;
			Location = node.Location;
			Region = node.Region;
			SubRegion = node.SubRegion;
		}
		public void AddDestination(Locations destination, List<byte> steps)
		{
			Destinations.Add(destination);
			Steps.Add(steps);
		}
		public byte[] DirectionFlagsArray()
		{ 
			return DirectionFlags.Prepend(NodeName).ToArray();
		}
		public byte[] StepsArray()
		{
			List<byte> finalarray = new();

			if (Destinations.Any())
			{
				for (int i = 0; i < 4; i++)
				{
					finalarray.AddRange(Steps[i].Prepend((byte)Destinations[i]));
				}
			}

			return finalarray.ToArray();
		}
	}

	public class MapUtilities
	{
		private List<Blob> _areaattributes = new();

		public MapUtilities(FFMQRom rom)
		{
			var attributepointers = rom.Get(RomOffsets.AreaAttributesPointers, RomOffsets.AreaAttributesPointersQty * 2).Chunk(2);

			foreach (var pointer in attributepointers)
			{
				var address = RomOffsets.AreaAttributesPointersBase + pointer[1] * 0x100 + pointer[0];
				_areaattributes.Add(rom.Get(address, 8));
			}
		}

		public byte AreaIdToMapId(byte areaid)
		{
			return _areaattributes[(int)areaid][1];
		}
	}
	public partial class FFMQRom : SnesRom
	{


	
	}
}
