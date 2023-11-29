using RomUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FFMQLib
{
	public enum CompanionsLocationType
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Shuffled")]
		Shuffled,
		[Description("Shuffled Extended")]
		ShuffledExtended,
	}

	public partial class GameLogic
	{
		public void CompanionsShuffle(CompanionsLocationType shuffletype, bool kaelismom, ApConfigs apconfigs, MT19337 rng)
		{
            // [AP1.4] 1.4 temporary fix until api/apworld are updated
            if ((shuffletype == CompanionsLocationType.Standard && !apconfigs.ApEnabled) || (apconfigs.ApEnabled && apconfigs.Version == "1.5"))
            {
                return;
            }

            List<GameObjectData> companions = new()
			{ 
				new GameObjectData()
				{ 
					OnTrigger = new() { AccessReqs.Tristam },
					Type = GameObjectType.Trigger,
					Name = "Tristam Companion"
				},
				new GameObjectData()
				{
					OnTrigger = new() { AccessReqs.Phoebe1 },
					Type = GameObjectType.Trigger,
					Name = "Phoebe Companion"
				},
				new GameObjectData()
				{
					OnTrigger = new() { AccessReqs.Reuben1 },
					Type = GameObjectType.Trigger,
					Name = "Reuben Companion"
				},
			 };

			if (!kaelismom)
			{
				companions.Add(new GameObjectData()
				{
					OnTrigger = new() { AccessReqs.Kaeli1 },
					Type = GameObjectType.Trigger,
					Name = "Kaeli Companion",
					Access = new() { AccessReqs.TreeWither }
				});
			}
			else
			{
				companions.Add(new GameObjectData()
				{
					Type = GameObjectType.Trigger,
					Name = "Kaeli Companion",
				});

				Rooms.Find(r => r.Id == 17).GameObjects.Find(o => o.Name == "Kaeli Companion").Name = "Kaeli's Mom";
			}

			Rooms.ForEach(x => x.GameObjects.RemoveAll(o => o.OnTrigger.Intersect(companions.SelectMany(c => c.OnTrigger).ToList()).Any()));

			List<(MapRegions region, int room)> validRooms = new()
			{
				(MapRegions.Foresta, 17),    // Kaeli's House
				(MapRegions.Foresta, 24),    // Sand Temple
				(MapRegions.Aquaria, 39),    // Libra Temple
				(MapRegions.Fireburg, 77)	 // Reuben's House
			};

            if (shuffletype == CompanionsLocationType.ShuffledExtended)
			{
				validRooms.AddRange(new List<(MapRegions, int)>()
				{
					(MapRegions.Aquaria, 51),	// Life Temple
					(MapRegions.Aquaria, 41),	// Phoebe's House
					(MapRegions.Fireburg, 92),	// Sealed Temple
					(MapRegions.Fireburg, 75),	// Wintry Temple
				});

				List<(MapRegions, int)> windiaRooms = new()
				{
					(MapRegions.Windia, 123),	// Rope Bridge
					(MapRegions.Windia, 153),	// Kaidge Temple
					(MapRegions.Windia, 154),	// Windhole Temple
					(MapRegions.Windia, 185),	// Light Temple
				};

				validRooms.Add(rng.TakeFrom(windiaRooms));
                validRooms.Add(rng.TakeFrom(windiaRooms));
            }
			
			var guaranteedforestalocation = rng.PickFrom(validRooms.Where(r => r.region == MapRegions.Foresta).ToList());
			validRooms.Remove(guaranteedforestalocation);

			Rooms.Find(r => r.Id == guaranteedforestalocation.room).GameObjects.Add(rng.TakeFrom(companions));

			foreach (var companion in companions)
			{
				var newroom = rng.TakeFrom(validRooms);
                Rooms.Find(r => r.Id == newroom.room).GameObjects.Add(companion);
            }
		}
	}
}
