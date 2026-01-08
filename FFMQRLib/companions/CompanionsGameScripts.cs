using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using static FFMQLib.FFMQRom;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public enum WalkDirection
		{ 
			Up = 0,
			Right = 1,
			Down = 2,
			Left = 3
		}
		public class CompanionLocation
		{ 
			public LocationIds Location { get; set; }
			public int GameObject { get; set; }
			public (int x, int y) Position { get; set; }
			public int CompanionPalette { get; set; }
			public int MapObjectsSet { get; set; }
			public int MapSpritesSet { get; set; }
			public bool AddMapObject { get; set; }
			public FacingOrientation Facing { get; set; }
			public List<(WalkDirection direction, int steps)> WalkOut { get; set; }
			
			public CompanionLocation(LocationIds loc, int gameobject, (int x, int y) pos, List<(WalkDirection, int)> walkout)
			{
				Location = loc;
				GameObject = gameobject;
				Position = pos;
				WalkOut = walkout;
			}
			public CompanionLocation(int gameobject, int mapset, int spriteset, (int x, int y) pos, List<(WalkDirection, int)> walkout, FacingOrientation facing, bool addobject)
			{
				GameObject = gameobject;
				MapObjectsSet = mapset;
				MapSpritesSet = spriteset;
				Position = pos;
				WalkOut = walkout;
				Facing = facing;
				AddMapObject = addobject;
			}
			public string GetWalkOutScript()
			{
				string walkscript = "2A";

				foreach (var direction in WalkOut)
				{
					walkscript += $"{direction.steps:X1}{GameObject:X1}4{(int)direction.direction:X1}";
				}

				walkscript += $"4{GameObject:X1}46FFFF";
				return walkscript;
			}
		}
		public class CompanionScriptData
		{ 
			public CompanionsId Name { get; set; }
			public byte Sprite { get; set; }
			public byte Palette { get; set; }
			public GameFlagIds Gameflag { get; set; }
			public byte Value { get; set; }
		}
		
		public void UpdateCompanionScripts(Flags flags, ItemsPlacement fullItemsPlacement, LocationIds startinglocation, bool apenabled, MT19337 rng)
		{
			var itemsPlacement = fullItemsPlacement.ItemsLocations.Where(x => x.Type == GameObjectType.NPC).ToDictionary(x => (ItemGivingNPCs)x.ObjectId, y => y.Content);

			int reubenhousenewset = MapSpriteSets.Add(new MapSpriteSet(MapSpriteSets[0x12]));
            int sandtemplenewset = MapSpriteSets.Add(new MapSpriteSet(MapSpriteSets[0x07]));
            int floatingtempleset = MapSpriteSets.Add(new MapSpriteSet(MapSpriteSets[0x02]));
            
			MapObjects.ModifyAreaAttribute(0x30, 2, (byte)reubenhousenewset);
            MapObjects.ModifyAreaAttribute(0x12, 2, (byte)sandtemplenewset);

            Dictionary<LocationIds, CompanionLocation> companionsLocation = new()
			{
				{ LocationIds.SandTemple, new CompanionLocation(0, 0x12, sandtemplenewset, (0x39, 0x06), new() { (WalkDirection.Down, 3), (WalkDirection.Left, 3), (WalkDirection.Down, 5) }, FacingOrientation.Down, false) },
				{ LocationIds.LibraTemple, new CompanionLocation(0, 0x17, 0x02, (0x0B, 0x04), new() { (WalkDirection.Down, 8) }, FacingOrientation.Down, false) },
				{ LocationIds.LifeTemple, new CompanionLocation(2, 0x20, 0x02, (0x09, 0x28), new() { (WalkDirection.Down, 6) }, FacingOrientation.Down, false) },
				{ LocationIds.Aquaria, new CompanionLocation(0, 0x1A, 0x09, (0x1C, 0x07), new() { (WalkDirection.Left, 1), (WalkDirection.Down, 3), (WalkDirection.Right, 2), (WalkDirection.Down, 2) }, FacingOrientation.Down, false) },
				{ LocationIds.SealedTemple, new CompanionLocation(4, 0x35, 0x02, (0x3B, 0x23), new() { (WalkDirection.Down, 7) }, FacingOrientation.Down, true) }, // add mapobject
				{ LocationIds.Fireburg, new CompanionLocation(4, 0x30, reubenhousenewset, (0x24, 0x28), new() { (WalkDirection.Down, 2), (WalkDirection.Left, 3), (WalkDirection.Down, 2) }, FacingOrientation.Left, true) }, // add mapobject, look left
				{ LocationIds.WintryTemple, new CompanionLocation(0, 0x2E, 0x02, (0x08, 0x16), new() { (WalkDirection.Down, 2), (WalkDirection.Right, 1) }, FacingOrientation.Down, false) },
				{ LocationIds.RopeBridge, new CompanionLocation(0, 0x42, 0x1C, (0x2A, 0x15), new() { (WalkDirection.Left, 3), (WalkDirection.Up, 6) }, FacingOrientation.Down, false) },
				{ LocationIds.KaidgeTemple, new CompanionLocation(3, 0x4D, 0x02, (0x36, 0x39), new() { (WalkDirection.Right, 2), (WalkDirection.Up, 2) }, FacingOrientation.Down, true) }, // add mapobject
				{ LocationIds.LightTemple, new CompanionLocation(3, 0x5F, 0x02, (0x13, 0x32), new() { (WalkDirection.Down, 1), (WalkDirection.Left, 1), (WalkDirection.Down, 1), (WalkDirection.Right, 7), (WalkDirection.Up, 3), (WalkDirection.Right, 3) }, FacingOrientation.Down, true) }, // add mapobject
				{ LocationIds.WindholeTemple, new CompanionLocation(1, 0x4E, 0x02, (0x0A, 0x0F), new() { (WalkDirection.Left, 1), (WalkDirection.Down, 3), (WalkDirection.Left, 7), (WalkDirection.Down, 4) }, FacingOrientation.Down, true) }, // add mapobject
				{ LocationIds.Foresta, new CompanionLocation(6, 0x10, 0x31, (0x0B, 0x0E), new() { (WalkDirection.Down, 6) }, FacingOrientation.Right, true) }, // add mapobject
			};

			List<CompanionScriptData> companionsData = new()
			{
				new CompanionScriptData
				{ 
					Name = CompanionsId.Kaeli,
					Sprite = 0x50,
					Palette = 0x05,
					Gameflag = GameFlagIds.ShowForestaKaeli,
					Value = (byte)TalkScriptsList.KaeliWitherTree
				},
				new CompanionScriptData
				{
					Name = CompanionsId.Tristam,
					Sprite = 0x54,
					Palette = 0x04,
					Gameflag = GameFlagIds.ShowSandTempleTristam,
					Value = (byte)TalkScriptsList.TristamChest
				},
				new CompanionScriptData
				{
					Name = CompanionsId.Phoebe,
					Sprite = 0x58,
					Palette = 0x03,
					Gameflag = GameFlagIds.ShowLibraTemplePhoebe,
					Value = (byte)TalkScriptsList.PhoebeLibraTemple
                },
				new CompanionScriptData
				{
					Name = CompanionsId.Reuben,
					Sprite = 0x5C,
					Palette = 0x02,
					Gameflag = GameFlagIds.ShowFireburgReuben1,
					Value = (byte)TalkScriptsList.ReubenFireburg
				}
			};

			if (Companions.Locations[CompanionsId.Reuben] == LocationIds.Fireburg)
			{
				companionsLocation.Remove(LocationIds.Fireburg);
				companionsLocation.Add(LocationIds.Fireburg, new CompanionLocation(1, 0x30, reubenhousenewset, (0x23, 0x27), new() { (WalkDirection.Left, 2), (WalkDirection.Down, 5) }, FacingOrientation.Down, false));
			}

			// Erase Companions
			MapObjects[0x12][0x00].Gameflag = 0xFE;
			MapObjects[0x10][0x01].Gameflag = 0xFE;
			MapObjects[0x17][0x00].Gameflag = 0xFE;
			MapObjects[0x30][0x01].Gameflag = 0xFE;

			// Update Sprites
			MapSpriteSets[0x31].MoveAddressor(10, 7);
			MapObjects[0x10][0x03].Sprite = 0x47;

			List<int> setsToUpdate = new() { 0x02, sandtemplenewset, 0x09, 0x1C, 0x31, reubenhousenewset, floatingtempleset };

			foreach (var set in setsToUpdate)
			{
				MapSpriteSets[set].DeleteAddressor(10);
				MapSpriteSets[set].DeleteAddressor(11);
				MapSpriteSets[set].DeleteAddressor(12);
				MapSpriteSets[set].DeleteAddressor(13);
				MapSpriteSets[set].AddAddressor(10, 0, 0x01, SpriteSize.Tiles16); // Kaeli / 0x50
				MapSpriteSets[set].AddAddressor(11, 0, 0x02, SpriteSize.Tiles16); // Tristam / 0x54
				MapSpriteSets[set].AddAddressor(12, 0, 0x03, SpriteSize.Tiles16); // Phoebe / 0x58
				MapSpriteSets[set].AddAddressor(13, 0, 0x04, SpriteSize.Tiles16); // Reuben / 0x5C
			}

			int templecount = 0;
			List<int> templepalette = new() { 0, 2, 3 };

			// Update companions
			foreach (var companion in companionsData)
			{
				var locationdata = companionsLocation[Companions.Locations[companion.Name]];
				
				if (locationdata.AddMapObject)
				{
					MapObjects[locationdata.MapObjectsSet].Add(new MapObject());
				}
				var companionobject = MapObjects[locationdata.MapObjectsSet][locationdata.GameObject];

                companionobject.Gameflag = (byte)companion.Gameflag;
                companionobject.X = (byte)locationdata.Position.x;
                companionobject.Y = (byte)locationdata.Position.y;
                companionobject.Value = companion.Value;
                companionobject.Behavior = 0x0A;
                companionobject.Facing = locationdata.Facing;
                companionobject.UnknownIndex = 0x02;
                companionobject.Sprite = companion.Sprite;
                companionobject.Palette = 0x00;

				// Update Palette/MapSet
				if (locationdata.MapSpritesSet == 0x02)
				{
					if (templecount >= 3)
					{
						MapObjects.ModifyAreaAttribute(locationdata.MapObjectsSet, 2, (byte)floatingtempleset);
						MapSpriteSets[locationdata.MapSpritesSet].Palette[0] = companion.Palette;
					}
					else
					{
						MapSpriteSets[locationdata.MapSpritesSet].Palette[templepalette[templecount]] = companion.Palette;
						companionobject.Palette = (byte)templepalette[templecount];
						templecount++;
					}
				}
				else if (Companions.Locations[companion.Name] == LocationIds.Fireburg && companion.Name != CompanionsId.Reuben)
				{
                    MapSpriteSets[locationdata.MapSpritesSet].Palette[3] = companion.Palette;
					companionobject.Palette = 0x03;
                }
				else
				{
					MapSpriteSets[locationdata.MapSpritesSet].Palette[0] = companion.Palette;
				}
			}

            UpdateKaeliScripts(flags, companionsLocation[Companions.Locations[CompanionsId.Kaeli]], itemsPlacement, rng);
            UpdateTristamScripts(flags, companionsLocation[Companions.Locations[CompanionsId.Tristam]], itemsPlacement, rng);
            UpdatePhoebeScripts(flags, companionsLocation[Companions.Locations[CompanionsId.Phoebe]], itemsPlacement, rng);
            UpdateReubenScripts(flags, companionsLocation[Companions.Locations[CompanionsId.Reuben]], itemsPlacement, rng);
		}
	}
}
