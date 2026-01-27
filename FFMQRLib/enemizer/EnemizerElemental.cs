using FFMQLib;
using RomUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMQLib
{
	public enum EnemizerElements
	{
		None,
		Fire,
		Water,
		Earth,
		Air,
		Thunder,
	}
	public partial class Enemizer
	{
		private List<EnemizerElements> validElements = new() { EnemizerElements.Fire, EnemizerElements.Water, EnemizerElements.Earth, EnemizerElements.Air, EnemizerElements.Thunder };

		private Dictionary<EnemizerElements, (List<ElementsType> resistances, List<ElementsType> weaknesses)> resistances = new()
		{
			{ EnemizerElements.Fire, (new List<ElementsType>() { ElementsType.Fire}, new List<ElementsType>() { ElementsType.Water }) },
			{ EnemizerElements.Water, (new List<ElementsType>() { ElementsType.Water}, new List<ElementsType>() { ElementsType.Earth }) },
			{ EnemizerElements.Earth, (new List<ElementsType>() { ElementsType.Earth}, new List<ElementsType>() { ElementsType.Air }) },
			{ EnemizerElements.Air, (new List<ElementsType>() { ElementsType.Air}, new List<ElementsType>() { ElementsType.Fire }) },
			{ EnemizerElements.Thunder, (new List<ElementsType>() { ElementsType.Air, ElementsType.Water }, new List<ElementsType>() { ElementsType.Fire, ElementsType.Earth }) },
		};

		public static Palette FirePalette = new(new List<SnesColor>()
			{
				new SnesColor(0,0,0),
				new SnesColor(31,31,31),
				new SnesColor(31,31,5),
				new SnesColor(31,17,0),
				new SnesColor(31,8,0),
				new SnesColor(26,0,0),
				new SnesColor(16,14,31),
				new SnesColor(0,0,0),
			});

		public static Palette WaterPalette = new(new List<SnesColor>()
			{
				new SnesColor(0,0,0),
				new SnesColor(31,31,31),
				new SnesColor(19,26,21),
				new SnesColor(14,18,26),
				new SnesColor(8,7,21),
				new SnesColor(9,0,13),
				new SnesColor(9,10,23),
				new SnesColor(0,0,0),
			});

		public static Palette EarthPalette = new(new List<SnesColor>()
			{
				new SnesColor(0,0,0),
				new SnesColor(31,31,31),
				new SnesColor(31,23,17),
				new SnesColor(26,12,5),
				new SnesColor(16,4,0),
				new SnesColor(8,2,0),
				new SnesColor(8,15,10),
				new SnesColor(0,0,0),
			});
		public static Palette AirPalette = new(new List<SnesColor>()
			{
				new SnesColor(0,0,0),
				new SnesColor(28,27,28),
				new SnesColor(21,19,20),
				new SnesColor(14,11,13),
				new SnesColor(9,5,8),
				new SnesColor(4,2,3),
				new SnesColor(5,15,12),
				new SnesColor(0,0,0),
			});
		public static Palette ThunderPalette = new(new List<SnesColor>()
			{
				new SnesColor(0,0,0),
				new SnesColor(31,31,31),
				new SnesColor(31,31,21),
				new SnesColor(27,19,0),
				new SnesColor(13,16,0),
				new SnesColor(6,6,0),
				new SnesColor(15,20,26),
				new SnesColor(0,0,0),
			});

		public static Dictionary<EnemizerElements, Palette> ElementalPalettes = new()
		{
			{ EnemizerElements.Fire, FirePalette },
			{ EnemizerElements.Water, WaterPalette },
			{ EnemizerElements.Earth, EarthPalette },
			{ EnemizerElements.Air, AirPalette },
			{ EnemizerElements.Thunder, ThunderPalette },
		};
		public Dictionary<EnemyIds, EnemizerElements> ElementalEnemies { get; }

		public void CreateElementalEnemies(bool enabled, EnemizerGroups group, bool prog, MT19337 rng)
		{
			if (!enabled)
			{
				return;
			}

			var validenemies = GetValidEnemies(group, prog);
			var darkkings = validenemies.Where(e => e.enemies.Intersect(Enemizer.DarkKing).Any());
			validenemies = validenemies.Where(e => !e.enemies.Intersect(Enemizer.DarkKing).Any()).ToList();

			foreach (var enemygroup in validenemies)
			{
				var commonelement = rng.PickFrom(validElements);

				foreach (var enemy in enemygroup.enemies)
				{
					ElementalEnemies[enemy] = commonelement;
				}
			}

			var darkkingelement = rng.PickFrom(validElements);

			foreach (var enemygroup in darkkings)
			{
				foreach (var enemy in enemygroup.enemies)
				{
					ElementalEnemies[enemy] = darkkingelement;
				}
			}
		}

		public void UpdatePalettes(bool modifypalettes)
		{

			if (!modifypalettes)
			{
				return;
			}

			Dictionary<EnemizerElements, byte> palettes = new()
			{
				{ EnemizerElements.Fire, 0x02 },
				{ EnemizerElements.Water, 0x03 },
				{ EnemizerElements.Earth, 0x04 },
				{ EnemizerElements.Air, 0x05 },
				{ EnemizerElements.Thunder, 0x06 },
			};

			foreach (var elementalEnemy in ElementalEnemies)
			{
				var enemy = enemies.Data[elementalEnemy.Key];
				enemy.Palette1 = palettes[elementalEnemy.Value];
				enemy.Palette2 = palettes[elementalEnemy.Value];
			}

			enemyPalettes.Data[0x02] = FirePalette;
			enemyPalettes.Data[0x03] = WaterPalette;
			enemyPalettes.Data[0x04] = EarthPalette;
			enemyPalettes.Data[0x05] = AirPalette;
			enemyPalettes.Data[0x06] = ThunderPalette;
		}
		public void UpdateNames(MT19337 rng)
		{
			Dictionary<EnemizerElements, List<string>> prefixes = new()
			{
				{ EnemizerElements.Fire, new() { "Red", "Fire", "Magma", "Hot", "Flaming", "Burning", "Lava" } },
				{ EnemizerElements.Water, new() { "Blue", "Water", "Liquid", "Aqua", "Sea", "River", "Hydro" } },
				{ EnemizerElements.Earth, new() { "Brown", "Earth", "Stone", "Ground", "Dust", "Sand", "Dirt" } },
				{ EnemizerElements.Air, new() { "Grey", "Air", "Wind", "Tempest", "Gale", "Gust" } },
				{ EnemizerElements.Thunder, new() { "Yellow", "Electric", "Spark", "Flash", "Magnetic", "Volt" } },
			};

			Dictionary<EnemizerElements, string> lazyPrefixes = new()
			{
				{ EnemizerElements.Fire, "F" },
				{ EnemizerElements.Water, "W" },
				{ EnemizerElements.Earth, "E" },
				{ EnemizerElements.Air, "A" },
				{ EnemizerElements.Thunder, "T" },
			};

			bool darkKingNamed = false;
			string darkKingName = "Dark|King";

			foreach (var elementalEnemy in ElementalEnemies)
			{
				var enemy = enemies.Data[elementalEnemy.Key];

				var currentname = enemy.Name.Split('|');
				int availablespace = 0;

				string newname = enemy.Name;
				string toppart = "";
				string lowerpart = "";

				bool useLazyPrefix = false;

				if (currentname.Length == 1)
				{
					availablespace = 8;
					lowerpart = currentname[0];
				}
				else
				{
					if ((currentname[0].Length + currentname[1].Length) < 8)
					{
						availablespace = 8;
						lowerpart = currentname[0] + " " + currentname[1];
					}
					else if ((currentname[0].Length + currentname[1].Length) < 9)
					{
						availablespace = 8;
						lowerpart = currentname[0] + currentname[1];
					}
					else
					{
						availablespace = 8 - currentname[0].Length;
						if (availablespace <= 4)
						{
							toppart = " " + String.Join("", currentname[0].Split('a', 'e', 'i', 'o', 'u', 'y'));
							availablespace = 8 - toppart.Length;

							if (availablespace < 4)
							{
								useLazyPrefix = true;
							}
						}
						lowerpart = currentname[1];
					}
				}

				// Select Prefix
				var prefix = "";

				if (useLazyPrefix)
				{
					if (availablespace >= 2)
					{
						prefix = lazyPrefixes[elementalEnemy.Value] + ".";
					}
					else if (availablespace >= 1)
					{
						prefix = lazyPrefixes[elementalEnemy.Value];
					}
				}
				else
				{
					var validPrefixes = prefixes[elementalEnemy.Value].Where(p => p.Length <= availablespace).ToList();
					prefix = rng.PickFrom(validPrefixes);
				}

				newname = prefix + toppart + "|" + lowerpart;

				if (Enemizer.DarkKing.Contains(enemy.Id))
				{
					if (!darkKingNamed)
					{
						darkKingName = newname;
						darkKingNamed = true;
					}
					else
					{
						newname = darkKingName;
					}
				}

				enemy.Name = newname;
			}
		}
	}
}
