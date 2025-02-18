using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;



namespace FFMQLib
{
	public static class EnemyInfo
	{
		public static Dictionary<EnemyFormationIds, List<EnemyIds>> BossesFormations = new()
		{
			//{ EnemyFormationIds.Behemoth, new() { EnemyIds.Behemoth } },
			//{ EnemyFormationIds.Minotaur, new() { EnemyIds.Minotaur } },
			{ EnemyFormationIds.FlamerusRex, new() { EnemyIds.FlamerusRex } },
			{ EnemyFormationIds.Squidite, new() { EnemyIds.Squidite, EnemyIds.Sparna, EnemyIds.Sparna } },
			{ EnemyFormationIds.SnowCrab, new() { EnemyIds.SnowCrab, EnemyIds.DesertHag, EnemyIds.DesertHag } },
			{ EnemyFormationIds.IceGolem, new() { EnemyIds.IceGolem } },
			{ EnemyFormationIds.Jinn, new() { EnemyIds.Jinn, EnemyIds.RedBone, EnemyIds.RedBone } },
			{ EnemyFormationIds.Medusa, new() { EnemyIds.Medusa, EnemyIds.Werewolf, EnemyIds.Werewolf } },
			{ EnemyFormationIds.DualheadHydra, new() { EnemyIds.DualheadHydra } },
			{ EnemyFormationIds.Gidrah, new() { EnemyIds.Gidrah, EnemyIds.Skuldier, EnemyIds.Skuldier } },
			{ EnemyFormationIds.Dullahan, new() { EnemyIds.Dullahan, EnemyIds.Vampire, EnemyIds.Vampire } },
			{ EnemyFormationIds.Pazuzu, new() { EnemyIds.Pazuzu } },
			{ EnemyFormationIds.SkullrusRex, new() { EnemyIds.SkullrusRex, EnemyIds.Shadow, EnemyIds.Shadow } },
			{ EnemyFormationIds.StoneGolem, new() { EnemyIds.StoneGolem, EnemyIds.Cerberus, EnemyIds.Cerberus } },
			{ EnemyFormationIds.TwinheadWyvern, new() { EnemyIds.TwinheadWyvern, EnemyIds.Iflyte, EnemyIds.Stheno } },
			{ EnemyFormationIds.Zuh, new() { EnemyIds.Zuh, EnemyIds.Chimera, EnemyIds.Thanatos } },
			{ EnemyFormationIds.DarkKing, new() { EnemyIds.DarkKing } },
		};

		public static Dictionary<EnemyFormationIds, AccessReqs> BossesAccess = new()
		{
			//{ EnemyFormationIds.Behemoth, AccessReqs.Be },
			{ EnemyFormationIds.Minotaur, AccessReqs.Minotaur },
			{ EnemyFormationIds.FlamerusRex, AccessReqs.FlamerusRex },
			{ EnemyFormationIds.Squidite, AccessReqs.Squidite },
			{ EnemyFormationIds.SnowCrab, AccessReqs.SnowCrab },
			{ EnemyFormationIds.IceGolem, AccessReqs.IceGolem },
			{ EnemyFormationIds.Jinn, AccessReqs.Jinn },
			{ EnemyFormationIds.Medusa, AccessReqs.Medusa },
			{ EnemyFormationIds.DualheadHydra, AccessReqs.DualheadHydra },
			{ EnemyFormationIds.Gidrah, AccessReqs.Gidrah },
			{ EnemyFormationIds.Dullahan, AccessReqs.Dullahan },
			{ EnemyFormationIds.Pazuzu, AccessReqs.Pazuzu },
			{ EnemyFormationIds.SkullrusRex, AccessReqs.SkullrusRex },
			{ EnemyFormationIds.StoneGolem, AccessReqs.StoneGolem },
			{ EnemyFormationIds.TwinheadWyvern, AccessReqs.TwinheadWyvern },
			{ EnemyFormationIds.Zuh, AccessReqs.Zuh },
			{ EnemyFormationIds.DarkKing, AccessReqs.DarkKing },
		};
		/*
		public static Dictionary<EnemyFormationIds, List<EnemyIds>> BattlefieldFormations = new()
		{
			{ EnemyFormationIds.MadPlant01, new() { EnemyIds.MadPlant, EnemyIds.MadPlant } },
			{ EnemyFormationIds.MadPlant02, new() { EnemyIds.MadPlant, EnemyIds.PoisonToad } },
			{ EnemyFormationIds.MadPlant03, new() { EnemyIds.MadPlant, EnemyIds.MadPlant, EnemyIds.MadPlant } },
			{ EnemyFormationIds.PoisonToad01, new() { EnemyIds.PoisonToad, EnemyIds.PoisonToad, EnemyIds.Basilisk } },
			{ EnemyFormationIds.PoisonToad02, new() { EnemyIds.PoisonToad, EnemyIds.PoisonToad, EnemyIds.MadPlant } },
			{ EnemyFormationIds.PoisonToad03, new() { EnemyIds.PoisonToad, EnemyIds.PoisonToad, EnemyIds.PoisonToad } },
			{ EnemyFormationIds.SandWorm02, new() { EnemyIds.SandWorm, EnemyIds.SandWorm } },
			{ EnemyFormationIds.MinotaurZombie01, new() { EnemyIds.MinotaurZombie } },
			{ EnemyFormationIds.MinotaurZombie02, new() { EnemyIds.SandWorm, EnemyIds.MinotaurZombie } },
			{ EnemyFormationIds.MinotaurZombie02, new() { EnemyIds.SandWorm, EnemyIds.MinotaurZombie } },

		};*/
		public static Dictionary<LocationIds, List<EnemyFormationIds>> BattlefieldFormations = new()
		{
			{ LocationIds.ForestaSouthBattlefield, new() { EnemyFormationIds.MadPlant02, EnemyFormationIds.MadPlant01, EnemyFormationIds.MadPlant03 } },
			{ LocationIds.ForestaWestBattlefield, new() { EnemyFormationIds.PoisonToad01, EnemyFormationIds.PoisonToad02, EnemyFormationIds.PoisonToad03 } },
			{ LocationIds.ForestaEastBattlefield, new() { EnemyFormationIds.SandWorm02, EnemyFormationIds.MinotaurZombie01, EnemyFormationIds.MinotaurZombie02 } },
			{ LocationIds.AquariaBattlefield01, new() { EnemyFormationIds.Mintmint02, EnemyFormationIds.Mintmint01, EnemyFormationIds.Mintmint02 } },
			{ LocationIds.AquariaBattlefield02, new() { EnemyFormationIds.GiantToad01, EnemyFormationIds.Mintmint02, EnemyFormationIds.GiantToad03 } },
			{ LocationIds.AquariaBattlefield03, new() { EnemyFormationIds.Scorpion01, EnemyFormationIds.GiantToad01, EnemyFormationIds.Scorpion03 } },
			{ LocationIds.WintryBattlefield01, new() { EnemyFormationIds.Edgehog01, EnemyFormationIds.Scorpion03, EnemyFormationIds.Edgehog02 } },
			{ LocationIds.WintryBattlefield02, new() { EnemyFormationIds.DesertHag02, EnemyFormationIds.DesertHag01, EnemyFormationIds.DesertHag03 } },
			{ LocationIds.PyramidBattlefield01, new() { EnemyFormationIds.Mage01, EnemyFormationIds.Lamia02, EnemyFormationIds.Lamia03 } },
			{ LocationIds.LibraBattlefield01, new() { EnemyFormationIds.Phanquid01, EnemyFormationIds.Phanquid01, EnemyFormationIds.Phanquid02 } },
			{ LocationIds.LibraBattlefield02, new() { EnemyFormationIds.FreezerCrab01, EnemyFormationIds.FreezerCrab01, EnemyFormationIds.FreezerCrab02 } },
			{ LocationIds.FireburgBattlefield01, new() { EnemyFormationIds.Jelly01, EnemyFormationIds.Jelly01, EnemyFormationIds.Jelly02 } },
			{ LocationIds.FireburgBattlefield02, new() { EnemyFormationIds.StingRat01, EnemyFormationIds.StingRat01, EnemyFormationIds.Jelly03 } },
			{ LocationIds.FireburgBattlefield03, new() { EnemyFormationIds.PlantMan01, EnemyFormationIds.PlantMan01, EnemyFormationIds.PlantMan02 } },
			{ LocationIds.MineBattlefield01, new() { EnemyFormationIds.Flazzard01, EnemyFormationIds.PlantMan03, EnemyFormationIds.PlantMan04 } },
			{ LocationIds.MineBattlefield02, new() { EnemyFormationIds.RedCap01, EnemyFormationIds.Flazzard02, EnemyFormationIds.RedCap03 } },
			{ LocationIds.MineBattlefield03, new() { EnemyFormationIds.Ghost01, EnemyFormationIds.RedBone03, EnemyFormationIds.Ghost03 } },
			{ LocationIds.VolcanoBattlefield01, new() { EnemyFormationIds.Stheno04, EnemyFormationIds.Iflyte04, EnemyFormationIds.Stheno04 } },
			{ LocationIds.WindiaBattlefield01, new() { EnemyFormationIds.Skuldier02, EnemyFormationIds.Ooze02, EnemyFormationIds.Skuldier03 } },
			{ LocationIds.WindiaBattlefield02, new() { EnemyFormationIds.WaterHag03, EnemyFormationIds.WaterHag01, EnemyFormationIds.WaterHag01 } },
		};
		public static Dictionary<EnemyCategory, List<EnemyIds>> EnemyCategories = new()
		{
			{ EnemyCategory.Imp, new() { EnemyIds.Brownie, EnemyIds.Mintmint, EnemyIds.RedCap } },
			{ EnemyCategory.Tree, new() { EnemyIds.MadPlant, EnemyIds.PlantMan, EnemyIds.LiveOak } },
			{ EnemyCategory.Slime, new() { EnemyIds.Slime, EnemyIds.Jelly, EnemyIds.Ooze } },
			{ EnemyCategory.Frog, new() { EnemyIds.PoisonToad, EnemyIds.GiantToad, EnemyIds.MadToad } },
			{ EnemyCategory.Reptile, new() { EnemyIds.Basilisk, EnemyIds.Flazzard, EnemyIds.Salamand } },
			{ EnemyCategory.Worm, new() { EnemyIds.SandWorm, EnemyIds.LandWorm, EnemyIds.Leech } },
			{ EnemyCategory.Skull, new() { EnemyIds.Skeleton, EnemyIds.RedBone, EnemyIds.Skuldier } },
			{ EnemyCategory.Bird, new() { EnemyIds.Roc, EnemyIds.Sparna, EnemyIds.Garuda } },
			{ EnemyCategory.Mummy, new() { EnemyIds.Zombie, EnemyIds.Mummy } },
			{ EnemyCategory.Hag, new() { EnemyIds.DesertHag, EnemyIds.WaterHag } },
			{ EnemyCategory.Ninja, new() { EnemyIds.Ninja, EnemyIds.Shadow } },
			{ EnemyCategory.Sphinx, new() { EnemyIds.Sphinx, EnemyIds.Manticor } },
			{ EnemyCategory.Centaur, new() { EnemyIds.Centaur, EnemyIds.Nitemare } },
			{ EnemyCategory.FatBird, new() { EnemyIds.StoneyRoost, EnemyIds.HotWings } },
			{ EnemyCategory.Ghost, new() { EnemyIds.Ghost, EnemyIds.Spector } },
			{ EnemyCategory.Eye, new() { EnemyIds.Gather, EnemyIds.Beholder } },
			{ EnemyCategory.Bat, new() { EnemyIds.Fangpire, EnemyIds.Vampire } },
			{ EnemyCategory.Mage, new() { EnemyIds.Mage, EnemyIds.Sorcerer } },
			{ EnemyCategory.Turtle, new() { EnemyIds.LandTurtle, EnemyIds.AdamantTurtle } },
			{ EnemyCategory.Scorpion, new() { EnemyIds.Scorpion, EnemyIds.Snipion } },
			{ EnemyCategory.Doggo, new() { EnemyIds.Werewolf, EnemyIds.Cerberus } },
			{ EnemyCategory.Edgehog, new() { EnemyIds.Edgehog, EnemyIds.StingRat } },
			{ EnemyCategory.Lamia, new() { EnemyIds.Lamia, EnemyIds.Naga } },
			{ EnemyCategory.Devil, new() { EnemyIds.Avizzard, EnemyIds.Gargoyle } },
			{ EnemyCategory.Minotaur, new() { EnemyIds.Minotaur, EnemyIds.MinotaurZombie } },
			{ EnemyCategory.Behemot, new() { EnemyIds.Behemoth, EnemyIds.Gorgon } },
			{ EnemyCategory.Squid, new() { EnemyIds.Phanquid, EnemyIds.Squidite } },
			{ EnemyCategory.Crab, new() { EnemyIds.FreezerCrab, EnemyIds.SnowCrab } },
			{ EnemyCategory.Jinn, new() { EnemyIds.Iflyte, EnemyIds.Jinn } },
			{ EnemyCategory.Medusa, new() { EnemyIds.Stheno, EnemyIds.Medusa } },
			{ EnemyCategory.Chimera, new() { EnemyIds.Chimera, EnemyIds.Gidrah } },
			{ EnemyCategory.Knight, new() { EnemyIds.Thanatos, EnemyIds.Dullahan } },
			{ EnemyCategory.Rex, new() { EnemyIds.FlamerusRex, EnemyIds.SkullrusRex } },
			{ EnemyCategory.Golem, new() { EnemyIds.IceGolem, EnemyIds.StoneGolem } },
			{ EnemyCategory.Hydra, new() { EnemyIds.DualheadHydra, EnemyIds.TwinheadWyvern } },
			{ EnemyCategory.Zu, new() { EnemyIds.Pazuzu, EnemyIds.Zuh } },
			{ EnemyCategory.DarkKing, new() { EnemyIds.DarkKing, EnemyIds.DarkKingWeapons, EnemyIds.DarkKingSpider } },
		};

	}
}
