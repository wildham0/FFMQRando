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
		public Items Content { get; set; }
		public TreasureType Type { get; set; }
		public bool IsPlaced { get; set; }
		public bool Prioritize { get; set; }
		public bool Exclude { get; set; }
		public List<AccessReqs> AccessRequirements { get; set; }
		public string Name { get; set; }

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, List<AccessReqs> access)
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
			Name = "";
		}

		public TreasureObject(int mapobjid, int mapid, Locations location, TreasureType type, string name, List<AccessReqs> access)
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
			Name = name;
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
			Name = treasure.Name;
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
		public void ShuffleBattelfieldRewards(Flags flags, MT19337 rng)
		{
			if (!flags.ShuffleBattlefieldRewards)
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
		}
		public BattlefieldRewardType GetRewardType(Locations targetBattlefield)
		{
			return (BattlefieldRewardType)(_rewards[(int)(targetBattlefield - 1)][1] & 0b1100_0000);
		}
		public List<BattlefieldRewardType> GetAllRewardType()
		{
			return _rewards.Select(x => (BattlefieldRewardType)(x[1] & 0b1100_0000)).ToList();
		}
		public void SetBattlesQty(Flags flags, MT19337 rng)
		{
			int battleQty = 10;
			bool randomQty = false;

			switch (flags.BattlesQuantity)
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
		private List<Node> Nodes = new();
		private List<byte> ShipWestSteps = new();
		private List<byte> ShipEastSteps = new();
		private const int OWMovementBank = 0x07;
		private const int OWMovementArrows = 0xEE84;
		private const int OWWalkStepsPointers = 0xF011;
		private const int OwNodesQty = 0x38;
		private const int NewOWWalkStepsBank = 0x12;
		private const int NewOWWalkStepsPointers = 0x9000;

		public NodeLocations(FFMQRom rom)
		{
			_movementArrows = rom.GetFromBank(OWMovementBank, OWMovementArrows, (OwNodesQty + 1) * 0x05).Chunk(5);
			ShipWestSteps = rom.GetFromBank(OWMovementBank, 0xF234, 6).ToBytes().ToList();
			ShipEastSteps = rom.GetFromBank(OWMovementBank, 0xF23A, 6).ToBytes().ToList();

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
		}
	}

	public class Node
	{
		public List<byte> DirectionFlags { get; set; }
		public byte NodeName { get; set; }
		public List<List<byte>> Steps {get; set;}
		public List<Locations> Destinations { get; set; }

		public Node(byte[] arrowvalues)
		{
			NodeName = arrowvalues[0];
			DirectionFlags = arrowvalues.ToList().GetRange(1, 4);
			Steps = new();
			Destinations = new();
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
