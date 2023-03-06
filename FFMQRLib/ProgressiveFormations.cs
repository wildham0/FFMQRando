using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum ProgressiveFormationsModes : int
	{
		[Description("Disabled")]
		Disabled = 0,
		[Description("By Regions (Strict)")]
		RegionsStrict,
		[Description("By Regions (Keep Type)")]
		RegionsKeepType,
	}
	public enum EnemyCategory
	{ 
		Imp = 0x00,
		Tree,
		Slime,
		Frog,
		Reptile,
		Worm,
		Skull,
		Bird,
		Mummy,
		Hag,
		Ninja,
		Sphinx,
		Centaur,
		FatBird,
		Ghost,
		Eye,
		Bat,
		Mage,
		Turtle,
		Scorpion,
		Doggo,
		Edgehog,
		Lamia,
		Devil,
		Minotaur,
		Behemot,
		Squid,
		Crab,
		Jinn,
		Medusa,
		Knight,
		Chimera,
	}
	public partial class FFMQRom : SnesRom
	{
		public void ProgressiveFormation(ProgressiveFormationsModes formationmode, Overworld overworld, MT19337 rng)
		{

			const int formationsVariantsBank = 0x02;
			const int formationsVariantsOffset = 0xCE12;
			
			if (formationmode == ProgressiveFormationsModes.Disabled)
			{
				return;
			}
			
			List<EnemyCategory> level1TypesList = new() { EnemyCategory.Imp, EnemyCategory.Tree, EnemyCategory.Slime, EnemyCategory.Frog, EnemyCategory.Reptile, EnemyCategory.Worm, EnemyCategory.Skull, EnemyCategory.Bird };

			List<EnemyCategory> level2TypesList = new() { EnemyCategory.Imp, EnemyCategory.Frog, EnemyCategory.Worm, EnemyCategory.Bird, EnemyCategory.Hag, EnemyCategory.Sphinx, EnemyCategory.Centaur, EnemyCategory.FatBird, EnemyCategory.Eye, EnemyCategory.Mage, EnemyCategory.Turtle, EnemyCategory.Scorpion, EnemyCategory.Edgehog, EnemyCategory.Lamia };

			List<EnemyCategory> level3TypesList = new() { EnemyCategory.Imp, EnemyCategory.Tree, EnemyCategory.Slime, EnemyCategory.Reptile, EnemyCategory.Skull, EnemyCategory.Mummy, EnemyCategory.Ninja, EnemyCategory.Centaur, EnemyCategory.FatBird, EnemyCategory.Ghost, EnemyCategory.Bat, EnemyCategory.Turtle, EnemyCategory.Doggo, EnemyCategory.Edgehog, EnemyCategory.Devil };

			List<EnemyCategory> level4TypesList = new() { EnemyCategory.Tree, EnemyCategory.Slime, EnemyCategory.Frog, EnemyCategory.Worm, EnemyCategory.Skull, EnemyCategory.Bird, EnemyCategory.Mummy, EnemyCategory.Hag, EnemyCategory.Ninja, EnemyCategory.Sphinx, EnemyCategory.Ghost, EnemyCategory.Eye, EnemyCategory.Bat, EnemyCategory.Mage, EnemyCategory.Scorpion, EnemyCategory.Doggo, EnemyCategory.Lamia, EnemyCategory.Devil };

			List<(byte, Blob)> newFormationsVariants = new()
			{
				(0x24, Blob.FromHex("070608")), // Madplants
				(0x25, Blob.FromHex("191819")), // Mintmint
				(0x26, Blob.FromHex("424243")), // Jelly
				(0x27, Blob.FromHex("474748")), // Plantman
				(0x29, Blob.FromHex("1E1D20")), // Giant Toad
				(0x2B, Blob.FromHex("454544")), // Sting Rat
				//(0x2F, Blob.FromHex("")),
			};

			List<(EnemyCategory, List<byte>)> formationsList = new()
			{
				(EnemyCategory.Imp, new() { 0x15, 0x25, 0x54, 0x54 }),
				(EnemyCategory.Tree, new() { 0x24, 0x27, 0x27, 0x84 }),
				(EnemyCategory.Slime, new() { 0x16, 0x26, 0x26, 0x8F }),
				(EnemyCategory.Frog, new() { 0x18, 0x29, 0x86, 0x86 }),
				(EnemyCategory.Reptile, new() { 0x19, 0x53, 0x79, 0x79 }),
				(EnemyCategory.Worm, new() { 0x1C, 0x2A , 0x8B, 0x8B }),
				(EnemyCategory.Skull, new() { 0x19, 0x51, 0x51, 0x8E }),
				(EnemyCategory.Bird, new() { 0x1B, 0x2D, 0x9A, 0x9A }),
				(EnemyCategory.Mummy, new() { 0x50, 0x50, 0x50, 0x7F }),
				(EnemyCategory.Hag, new() { 0x32, 0x32, 0x93, 0x93 }),
				(EnemyCategory.Ninja, new() { 0x66, 0x66, 0x66, 0xBE }),
				(EnemyCategory.Sphinx, new() { 0x4B, 0x4B, 0x9C, 0x9C }),
				(EnemyCategory.Centaur, new() { 0x28, 0x28, 0x5C, 0x5C }),
				(EnemyCategory.FatBird, new() { 0x42, 0x42, 0x65, 0x65 }),
				(EnemyCategory.Ghost, new() { 0x58, 0x58, 0x58, 0x80 }),
				(EnemyCategory.Eye, new() { 0x46, 0x46, 0x9B, 0x9B }),
				(EnemyCategory.Bat, new() { 0x71, 0x71, 0x71, 0x94 }),
				(EnemyCategory.Mage, new() { 0x3E, 0x3E, 0xA5, 0xA5 }),
				(EnemyCategory.Turtle, new() { 0x2C, 0x2C, 0x75, 0x75 }),
				(EnemyCategory.Scorpion, new() { 0x22, 0x22, 0x85, 0x85 }),
				(EnemyCategory.Doggo, new() { 0x5D, 0x5D, 0x5D, 0xC6 }),
				(EnemyCategory.Edgehog, new() { 0x23, 0x23, 0x2B, 0x2B }),
				(EnemyCategory.Lamia, new() { 0x37, 0x37, 0xB1, 0xB1 }),
				(EnemyCategory.Devil, new() { 0x67, 0x67, 0x67, 0xB9 }),
				(EnemyCategory.Minotaur, new() { 0x20, 0x20, 0x20, 0x20 }),
				(EnemyCategory.Behemot, new() { 0x1E, 0x1E, 0x1E, 0x1E }),
				(EnemyCategory.Squid, new() { 0x40, 0x40, 0x40, 0x40 }),
				(EnemyCategory.Crab, new() { 0x44, 0x44, 0x44, 0x44 }),
				(EnemyCategory.Jinn, new() { 0x64, 0x64, 0x73, 0x73 }),
				(EnemyCategory.Medusa, new() { 0x68, 0x68, 0x77, 0x77 }),
				(EnemyCategory.Knight, new() { 0xA2, 0xA2, 0xA2, 0xA7 }),
				(EnemyCategory.Chimera, new() { 0x9D, 0x9D, 0x9D, 0xA6 }),

				/*
								(EnemyCategory.Minotaur, new() { 0x20, 0x20, 0xC0, 0xC0 }),
								(EnemyCategory.Behemot, new() { 0x1E, 0x1E, 0xBF, 0xBF }),
								(EnemyCategory.Squid, new() { 0x40, 0x40, 0x40, 0xC3 }),
								(EnemyCategory.Crab, new() { 0x44, 0x44, 0x44, 0xC4 }),
								(EnemyCategory.Jinn, new() { 0x44, 0x44, 0x44, 0xC7 }),
								(EnemyCategory.Medusa, new() { 0x44, 0x44, 0x44, 0xC8 }),
								(EnemyCategory.Knight, new() { 0x44, 0x44, 0x44, 0xCC }),
								(EnemyCategory.Chimera, new() { 0x44, 0x44, 0x44, 0xCB }),
				*/
			};

			List<(EnemyCategory, byte)> animationsList = new()
			{
				(EnemyCategory.Imp, 0x0B),
				(EnemyCategory.Tree, 0x0A),
				(EnemyCategory.Slime, 0x0C),
				(EnemyCategory.Frog, 0x0C),
				(EnemyCategory.Reptile, 0x0B),
				(EnemyCategory.Worm, 0x0B),
				(EnemyCategory.Skull, 0x0B),
				(EnemyCategory.Bird, 0x0C),
				(EnemyCategory.Mummy, 0x0B),
				(EnemyCategory.Hag, 0x0B),
				(EnemyCategory.Ninja, 0x0A),
				(EnemyCategory.Sphinx, 0x0B),
				(EnemyCategory.Centaur, 0x0A),
				(EnemyCategory.FatBird, 0x0C),
				(EnemyCategory.Ghost, 0x0C),
				(EnemyCategory.Eye, 0x0B),
				(EnemyCategory.Bat, 0x0C),
				(EnemyCategory.Mage, 0x0C),
				(EnemyCategory.Turtle, 0x0B),
				(EnemyCategory.Scorpion, 0x0B),
				(EnemyCategory.Doggo, 0x0B),
				(EnemyCategory.Edgehog, 0x0C),
				(EnemyCategory.Lamia, 0x0B),
				(EnemyCategory.Devil, 0x0B),
				(EnemyCategory.Minotaur, 0x0B),
				(EnemyCategory.Behemot, 0x0B),
				(EnemyCategory.Squid, 0x0B),
				(EnemyCategory.Crab, 0x0C),
				(EnemyCategory.Jinn, 0x0C),
				(EnemyCategory.Medusa, 0x0A),
				(EnemyCategory.Knight, 0x0B),
				(EnemyCategory.Chimera, 0x0D),
			};

			List<(EnemyCategory, List<EnemyCategory>)> minibossesCategories = new() {
				(EnemyCategory.Behemot, new() { EnemyCategory.Behemot, EnemyCategory.Squid, EnemyCategory.Jinn, EnemyCategory.Chimera }),
				(EnemyCategory.Minotaur, new() { EnemyCategory.Minotaur, EnemyCategory.Crab, EnemyCategory.Medusa, EnemyCategory.Knight }),
				(EnemyCategory.Squid, new() { EnemyCategory.Behemot, EnemyCategory.Squid, EnemyCategory.Jinn, EnemyCategory.Chimera }),
				(EnemyCategory.Crab, new() { EnemyCategory.Minotaur, EnemyCategory.Crab, EnemyCategory.Medusa, EnemyCategory.Knight }),
				(EnemyCategory.Jinn, new() { EnemyCategory.Behemot, EnemyCategory.Squid, EnemyCategory.Jinn, EnemyCategory.Chimera }),
				(EnemyCategory.Medusa, new() { EnemyCategory.Minotaur, EnemyCategory.Crab, EnemyCategory.Medusa, EnemyCategory.Knight }),
				(EnemyCategory.Chimera, new() { EnemyCategory.Behemot, EnemyCategory.Squid, EnemyCategory.Jinn, EnemyCategory.Chimera }),
				(EnemyCategory.Knight, new() { EnemyCategory.Minotaur, EnemyCategory.Crab, EnemyCategory.Medusa, EnemyCategory.Knight }),
			};


			var categories = Enum.GetValues<EnemyCategory>().ToList();

			List<(EnemyCategory, List<EnemyCategory>)> sprites = new();
			List<(EnemyCategory, List<byte>)> formations = new();
			List<(EnemyCategory, List<byte>)> animations = new();

			List<(MapRegions, int)> levelValues = new()
			{
				(MapRegions.Foresta, 0),
				(MapRegions.Aquaria, 1),
				(MapRegions.Fireburg, 2),
				(MapRegions.Windia, 3),
			};

			List<(LocationIds, int)> locationsLevel = new();

			foreach (var loc in overworld.Locations)
			{
				locationsLevel.Add((loc.LocationId, levelValues.Find(x => x.Item1 == loc.Region).Item2));
			}

			if (formationmode == ProgressiveFormationsModes.RegionsStrict)
			{
				// 
				foreach (var category in categories)
				{
					bool minibosses = category >= EnemyCategory.Minotaur;

					List<EnemyCategory> validLevel1 = minibosses ? new() { minibossesCategories.Find(x => x.Item1 == category).Item2[0] } : level1TypesList;
					List<EnemyCategory> validLevel2 = minibosses ? new() { minibossesCategories.Find(x => x.Item1 == category).Item2[1] } : level2TypesList;
					List<EnemyCategory> validLevel3 = minibosses ? new() { minibossesCategories.Find(x => x.Item1 == category).Item2[2] } : level3TypesList;
					List<EnemyCategory> validLevel4 = minibosses ? new() { minibossesCategories.Find(x => x.Item1 == category).Item2[3] } : level4TypesList;

					var level1Sprite = level1TypesList.Contains(category) ? category : rng.PickFrom(validLevel1);
					var level1battle = formationsList.Find(x => x.Item1 == level1Sprite).Item2[0];
					var level2Sprite = level2TypesList.Contains(category) ? category : rng.PickFrom(validLevel2);
					var level2battle = formationsList.Find(x => x.Item1 == level2Sprite).Item2[1];
					var level3Sprite = level3TypesList.Contains(category) ? category : rng.PickFrom(validLevel3);
					var level3battle = formationsList.Find(x => x.Item1 == level3Sprite).Item2[2];
					var level4Sprite = level4TypesList.Contains(category) ? category : rng.PickFrom(validLevel4);
					var level4battle = formationsList.Find(x => x.Item1 == level4Sprite).Item2[3];

					sprites.Add((category, new() { level1Sprite, level2Sprite, level3Sprite, level4Sprite }));
					formations.Add((category, new() { level1battle, level2battle, level3battle, level4battle }));
					animations.Add((category, new() { animationsList.Find(x => x.Item1 == level1Sprite).Item2, animationsList.Find(x => x.Item1 == level2Sprite).Item2, animationsList.Find(x => x.Item1 == level3Sprite).Item2, animationsList.Find(x => x.Item1 == level4Sprite).Item2 }));
				}
			}
			else if (formationmode == ProgressiveFormationsModes.RegionsKeepType)
			{
				foreach (var category in categories)
				{
					var level1Sprite = category;
					var level1battle = formationsList.Find(x => x.Item1 == level1Sprite).Item2[0];
					var level2Sprite = category;
					var level2battle = formationsList.Find(x => x.Item1 == level2Sprite).Item2[1];
					var level3Sprite = category;
					var level3battle = formationsList.Find(x => x.Item1 == level3Sprite).Item2[2];
					var level4Sprite = category;
					var level4battle = formationsList.Find(x => x.Item1 == level4Sprite).Item2[3];

					sprites.Add((category, new() { category, category, category, category }));
					formations.Add(formationsList.Find(x => x.Item1 == category));
					animations.Add((category, new() { animationsList.Find(x => x.Item1 == level1Sprite).Item2, animationsList.Find(x => x.Item1 == level2Sprite).Item2, animationsList.Find(x => x.Item1 == level3Sprite).Item2, animationsList.Find(x => x.Item1 == level4Sprite).Item2 }));
				}
			}

			foreach (var variant in newFormationsVariants)
			{
				PutInBank(formationsVariantsBank, formationsVariantsOffset + (3 * variant.Item1), variant.Item2);
			}

			// see 11_9600_ProgressiveFormations.asm
			PutInBank(0x11, 0x9780, locationsLevel.OrderBy(x => x.Item1).Select(x => (byte)x.Item2).ToArray());
			PutInBank(0x11, 0x9800, formations.OrderBy(x => x.Item1).SelectMany(x => x.Item2).ToArray());
			PutInBank(0x11, 0x9880, animations.OrderBy(x => x.Item1).SelectMany(x => x.Item2).ToArray());
			PutInBank(0x11, 0x9900, sprites.OrderBy(x => x.Item1).SelectMany(x => x.Item2.Select(x => (byte)x).ToArray()).ToArray());

			PutInBank(0x11, 0x9600, Blob.FromHex("da08c2304829ff00186db519aa68e220c210bf00b11128fa60"));
			PutInBank(0x11, 0x9620, Blob.FromHex("ee3519ad880ec935f00fad35191869042000962918c908f00ca9008d2005ad35192000966ba9018d200520a0966b"));
			PutInBank(0x11, 0x9650, Blob.FromHex("ee3519ad2005d007ad35192000966bad351920009629e08d220520d0960d22056b"));
			PutInBank(0x11, 0x9680, Blob.FromHex("ee3519ad2005d007ad35192000966b20f0966b"));
			PutInBank(0x11, 0x96A0, Blob.FromHex("da18ad35196905200096291f0a0a8d2105ad880eaabf8097116d2105aabf009811fa60"));
			PutInBank(0x11, 0x96D0, Blob.FromHex("daad880eaabf8097116d2105aabf809811fa60"));
			PutInBank(0x11, 0x96F0, Blob.FromHex("daad880eaabf8097116d2105aabf009911186960fa60"));

			PutInBank(0x01, 0xA70A, Blob.FromHex("22209611EAEAEAEAEA"));
			PutInBank(0x01, 0xA74D, Blob.FromHex("22509611EAEAEAEAEA"));
			PutInBank(0x01, 0xA7C0, Blob.FromHex("22809611EAEAEAEAEA"));
		}
	}
}
			
