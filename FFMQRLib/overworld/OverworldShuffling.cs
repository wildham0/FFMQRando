using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public partial class Overworld
	{
		public void ShuffleOverworld(MapShufflingMode mapshufflingmode, GameLogic gamelogic, Battlefields battlefields, List<LocationIds> questEasyWins, bool apenabled, MT19337 rng)
		{
			bool shuffleOverworld = mapshufflingmode != MapShufflingMode.None && mapshufflingmode != MapShufflingMode.Dungeons;
			
			if (!shuffleOverworld || apenabled)
			{
				return;
			}

			List<MovableLocation> movableLocations = new();
			var regionRooms = gamelogic.Rooms.Where(r => r.Type == RoomType.Subregion).ToList();

			foreach (var room in regionRooms)
			{
				movableLocations.AddRange(room.GameObjects.Select(o => new MovableLocation(room.Region, o.Location, 0, o)));
				movableLocations.AddRange(room.Links.Where(l => l.Entrance >= 0).Select(l => new MovableLocation(room.Region, l.Location, l.TargetRoom, l)));
			}

			var regionLocationPairs = movableLocations.ToDictionary(x => x.Origins, x => x.Region);

			var safeGoldBattlefield = (LocationIds)(battlefields.GetAllRewardType().Select((x, i) => (i, x)).ToList().Find(x => x.x == BattlefieldRewardType.Gold).i + 1);

			List<LocationIds> shuffleLocations = Enum.GetValues<LocationIds>().ToList();
			List<LocationIds> destinationLocations = Enum.GetValues<LocationIds>().ToList();
			List<LocationIds> fixedLocations = new() { LocationIds.DoomCastle, LocationIds.FocusTowerForesta, LocationIds.FocusTowerAquaria, LocationIds.FocusTowerFrozen, LocationIds.FocusTowerFireburg, LocationIds.FocusTowerWindia, LocationIds.GiantTree, LocationIds.HillOfDestiny, LocationIds.LifeTemple, LocationIds.LightTemple, LocationIds.MacsShip, LocationIds.MacsShipDoom, LocationIds.None, LocationIds.ShipDock, LocationIds.SpencersPlace };
			shuffleLocations.RemoveAll(x => fixedLocations.Contains(x));
			destinationLocations.RemoveAll(x => fixedLocations.Contains(x));

			List<LocationIds> placedLocations = fixedLocations.ToList();
			List<LocationIds> takenLocations = fixedLocations.ToList();
			List<LocationIds> excludeFromStart = Enum.GetValues<LocationIds>().ToList().GetRange(9, 12);

			// Find best companion location
			List<(LocationIds, int)> companionsRating = new();

			foreach (var location in shuffleLocations.Where(x => x > LocationIds.HillOfDestiny))
			{
				companionsRating.Add(gamelogic.CrawlForCompanionRating(location));
			}
			
			companionsRating.Shuffle(rng);
			companionsRating = companionsRating.Where(x => x.Item2 > 0).OrderByDescending(x => x.Item2).ToList();
			LocationIds companionLocation = (mapshufflingmode == MapShufflingMode.Everything) ? companionsRating.First().Item1 : rng.PickFrom(companionsRating).Item1;

			// Find most accessible locations for item placement
			List<(LocationIds, int)> locationRating = new();

			foreach (var location in shuffleLocations.Where(x => x > LocationIds.HillOfDestiny))
			{
				locationRating.Add(gamelogic.CrawlForChestRating(location));
			}

			locationRating = locationRating.Where(x => x.Item1 != companionLocation).OrderByDescending(x => x.Item2).ToList();

			List<LocationIds> guaranteedChestLocations = new();

			
			guaranteedChestLocations = (mapshufflingmode == MapShufflingMode.Overworld) ?
				new() { rng.PickFrom(AccessReferences.StartingWeaponAccess) } :
				locationRating.GetRange(0, 2).Select(x => x.Item1).ToList();

			// Find Special Locations
			List<SpecialRegion> specialRegionsAccess = new()
			{
				new SpecialRegion(SubRegions.AquariaFrozenField, AccessReqs.SummerAquaria),
				new SpecialRegion(SubRegions.VolcanoBattlefield, AccessReqs.DualheadHydra),
			};

			List <AccessReqs> gatingLocationsAccess = new() { AccessReqs.SummerAquaria, AccessReqs.DualheadHydra, AccessReqs.LavaDomePlate, AccessReqs.Gidrah };

			List<(LocationIds, AccessReqs)> gatingLocations = new();

			foreach (var location in gatingLocationsAccess)
			{
				gatingLocations.Add((gamelogic.FindTriggerLocation(location), location));
			}

			foreach (var region in specialRegionsAccess)
			{
				var location = gatingLocations.Find(x => x.Item2 == region.Access).Item1;
				var accessreq = gamelogic.CrawlForRequirements(location);

				var commonaccess = accessreq.Intersect(gatingLocationsAccess).ToList();
				region.BarredLocations.AddRange(gatingLocations.Where(x => commonaccess.Contains(x.Item2)).Select(x => x.Item1).Append(location));
			}

			// Create the early locations, these are always placed in Foresta
			List<LocationIds> earlyLocations = new() { companionLocation, safeGoldBattlefield }; // 2 locations
			earlyLocations.AddRange(guaranteedChestLocations); // 2 extra locations
			questEasyWins = questEasyWins.Except(earlyLocations).ToList();

			if (questEasyWins.Count >= 1)
			{
				questEasyWins.Shuffle(rng);
				earlyLocations.Add(questEasyWins.First());
			}

			// Place guaranteed locations
			var loc1 = LocationIds.None;
			var loc2 = LocationIds.None;

			var forestaLocations = destinationLocations.Where(x => AccessReferences.MapSubRegions.Find(r => r.Item2 == x).Item1 == SubRegions.Foresta).ToList();

			while (earlyLocations.Any())
			{
				loc1 = earlyLocations.First();
				loc2 = rng.PickFrom(forestaLocations);

				movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
				placedLocations.Add(loc1);
				takenLocations.Add(loc2);

				earlyLocations = earlyLocations.Where(x => !placedLocations.Contains(x)).ToList();
				forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
			}
			
			forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
			var startingLocations = shuffleLocations.Where(x => !excludeFromStart.Contains(x) && !placedLocations.Contains(x)).ToList();

			// Place rest of Foresta region
			while (forestaLocations.Count > 0)
			{
				loc1 = rng.PickFrom(startingLocations);
				loc2 = rng.PickFrom(forestaLocations);

				movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
				placedLocations.Add(loc1);
				takenLocations.Add(loc2);

				forestaLocations = forestaLocations.Where(x => !takenLocations.Contains(x)).ToList();
				startingLocations = startingLocations.Where(x => !placedLocations.Contains(x)).ToList();
			}

			// Place Special Regions
			bool gatingLocationPlaced = false;
			List<LocationIds> gatingLocationsList = gatingLocations.Select(l => l.Item1).ToList();

			foreach (var region in specialRegionsAccess)
			{
				var gatedRegionLocations = destinationLocations.Where(x => (AccessReferences.MapSubRegions.Find(r => r.Item2 == x).Item1 == region.SubRegion) && !takenLocations.Contains(x)).ToList();

				foreach (var location in gatedRegionLocations)
				{
					var regionSafeLocations = shuffleLocations.Where(x => !placedLocations.Contains(x) &&
						!region.BarredLocations.Contains(x) &&
						(gatingLocationPlaced ? !gatingLocationsList.Contains(x) : true)
						).ToList();

					loc1 = rng.PickFrom(regionSafeLocations);
					loc2 = location;

					if (gatingLocationsList.Contains(loc1))
					{
						gatingLocationPlaced = true;
					}

					movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
					placedLocations.Add(loc1);
					takenLocations.Add(loc2);
				}
			}

			shuffleLocations = shuffleLocations.Where(x => !placedLocations.Contains(x)).ToList();
			destinationLocations = destinationLocations.Where(x => !takenLocations.Contains(x)).ToList();

			// Place the rest
			while (shuffleLocations.Any())
			{
				loc1 = rng.TakeFrom(shuffleLocations);
				loc2 = rng.TakeFrom(destinationLocations);

				movableLocations.Find(l => l.Origins == loc1).Destination = loc2;
			}

			// Clear rooms
			foreach (var room in regionRooms)
			{ 
				room.GameObjects.Clear();
				room.Links.RemoveAll(l => l.Entrance >= 0);
			}

			// Place the new locations
			foreach (var location in movableLocations)
			{
				if (location.Type == MovableLocationType.Battlefield)
				{
					var targetRegion = regionRooms.Find(x => x.Region == regionLocationPairs[location.Destination]);
					location.Object.LocationSlot = location.Destination;
					targetRegion.GameObjects.Add(location.Object);
				}
				else
				{
					var targetRegion = regionRooms.Find(x => x.Region == regionLocationPairs[location.Destination]);
					var originalRegion = regionRooms.Find(x => x.Region == location.Region);
					location.Link.LocationSlot = location.Destination;
					targetRegion.Links.Add(location.Link);

					var linkToUpdate = gamelogic.Rooms.Find(r => r.Id == location.Room).Links.Find(l => l.TargetRoom == originalRegion.Id);

					if (linkToUpdate != null)
					{
						linkToUpdate.TargetRoom = targetRegion.Id;
					}
				}
			}
		}
	}
}
