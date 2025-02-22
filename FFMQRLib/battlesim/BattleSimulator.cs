using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Net.WebSockets;
using Microsoft.VisualBasic;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Collections.Specialized.BitVector32;
using YamlDotNet.Serialization;



namespace FFMQLib
{
	public struct GearStats
	{
		public int AttackBonus;
		public int DefenseBonus;
		public int SpeedBonus;
		public int MagicBonus;
		public int Armor;
		public int Evade;
		public int MagicArmor;
		public int MagicEvade;
		public int Acccuracy;
		public List<ElementsType> Resistances;

		public GearStats()
		{
			AttackBonus = 0;
			DefenseBonus = 0;
			SpeedBonus = 0;
			MagicBonus = 0;
			Armor = 0;
			Evade = 0;
			Acccuracy = 0;
			Resistances = new();
		}
	}
	public struct ActionResult
	{
		public int MaxDamage;
		public int Probability;
		public int Disabled;
	}
	public class Logger
	{
		private List<string> logs;

		public Logger()
		{
			logs = new();
		}
		public void Add(string eventlog)
		{
			if (logs.Count > 10000)
			{
				logs.RemoveAt(0);
			}

			logs.Add(eventlog);
		}
		public void Add(List<string> eventlog)
		{
			foreach (var eveventstring in eventlog)
			{
				Add(eveventstring);
			}
		}
	}

	public class Battle
	{
		List<Entity> Battlers;
		private Logger Log;
		private Logger SimLog;
		private MT19337 Rng;
		private List<int> winsA;
		private List<int> winsB;
		private List<int> protoWinsA;
		private List<int> protoWinsB;
		private CompanionsId currentCompanion;
		private EnemiesStats enemies;
		private EnemyAttackLinks scripts;
		//private Dictionary<string, (int half, int quarter, int zero)> resultsTracker;
		private List<(List<string> attacks, int percent)> resultsTracker;

		public Battle(EnemiesStats _enemies, EnemyAttackLinks _scripts, MT19337 rng)
		{
			Rng = rng;
			SimLog = new Logger();
			Log = new();

			int runningratio = 0;
			//int totalwinb = 0;

			resultsTracker = new();
			Dictionary<string, int> ratings = new();

			enemies = _enemies;
			scripts = _scripts;
			/*
			Flags flags = new();
			flags.BossesScalingLower = EnemiesScaling.OneAndHalf;
			flags.BossesScalingUpper = EnemiesScaling.OneAndHalf;
			flags.EnemiesScalingLower = EnemiesScaling.OneAndHalf;
			flags.EnemiesScalingUpper = EnemiesScaling.OneAndHalf;
			
			_enemies.ScaleEnemies(flags, rng);*/
			EnemyIds simformation = EnemyIds.Flazzard;

			List<(string attack1, string attack2)> attackpairs = new();

			//List<PowerLevels> powerlevels = new() { PowerLevels.Plain10 };
			//List<PowerLevels> powerlevels = new() { PowerLevels.Initial, PowerLevels.Intermediate, PowerLevels.Strong, PowerLevels.Godly };
			List<PowerLevels> powerlevels = new() { PowerLevels.Intermediate };
			//foreach (var power in powerlevels)
			List<CompanionsId> validcompanions = new() { CompanionsId.Kaeli, CompanionsId.Reuben, CompanionsId.Phoebe, CompanionsId.Tristam };
			currentCompanion = Rng.PickFrom(validcompanions);
			// 4,7 ; 8,15 ; 12,23 ; 19,31

			InitTeams(PowerLevels.Initial, simformation, 8, 15);

			for (int i = 0; i < 1000; i++)
			{
				winsA = new() { 0, 0, 0, 0 };
				winsB = new() { 0, 0, 0, 0 };

				InitEnemies(simformation);

				for (int j = 0; j < 100; j++)
				{
					Battlers.ForEach(b => b.HealUp());
					DoBattle(rng);
				}

				int attackRating = 0;

				foreach (var enemy in Battlers.Where(b => b.Team == Teams.TeamB))
				{
					SimLog.Add(Enum.GetName(typeof(EnemyIds), enemy.EnemyId) + ": " + String.Join(", ", scripts[(int)enemy.EnemyId].Attacks.Select(a => Battle.BattleActions[(int)a].Name)) + ".");

					scripts[(int)enemy.EnemyId].Attacks.Where(a => a != EnemyAttackIds.Nothing).Select(a => (Battle.BattleActions[(int)a].Name, Battle.BattleActions[(int)a].Rating(enemy))).ToList().ForEach(a => ratings[a.Name] = a.Item2);

					var namesList = scripts[(int)enemy.EnemyId].Attacks.Where(a => a != EnemyAttackIds.Nothing).Select(a => Battle.BattleActions[(int)a].Name).ToList();
					namesList.Sort();

					for (int z = 0; z < namesList.Count; z++)
					{
						for (int x = z + 1; x < namesList.Count; x++)
						{
							attackpairs.Add((namesList[z], namesList[x]));
						}
					}

					//var currentAttackRating = (int)scripts[(int)enemy.EnemyId].Attacks.Where(a => a != 0xFF).Select(a => Battle.BattleActions[a].Rating(enemy)).Average();
					//var attackCount = scripts[(int)enemy.EnemyId].Attacks.Where(a => a != 0xFF).Count();

					//currentAttackRating += (6 - attackCount) * 5;
					//attackRating += currentAttackRating;
				}

				//attackRating /= Battlers.Count(b => b.Team == Teams.TeamB);

				/*
				int attackRating = (int)Battlers.Where(b => b.Team == Teams.TeamB)
					.SelectMany(e => scripts[(int)e.EnemyId].Attacks)
					.Where(a => a != 0xFF)
					.Distinct()
					.ToList()
					.Select(a => Battle.BattleActions[a].Rating()
					.Average();*/

				SimLog.Add("Average Rating: " + attackRating + ".");

				SimLog.Add("Ben: " + String.Join(", ", Battlers.Find(b => b.IsBen).Gears.Select(g => Enum.GetName(typeof(Items), g))) + ".");

				if (winsA[0] + winsB[0] > 0) SimLog.Add("Kaeli: " + (winsA[0] * 100) / (winsA[0] + winsB[0]) + "%");
				if (winsA[1] + winsB[1] > 0) SimLog.Add("Tristam: " + (winsA[1] * 100) / (winsA[1] + winsB[1]) + "%");
				if (winsA[2] + winsB[2] > 0) SimLog.Add("Phoebe: " + (winsA[2] * 100) / (winsA[2] + winsB[2]) + "%");
				if (winsA[3] + winsB[3] > 0) SimLog.Add("Reuben: " + (winsA[3] * 100) / (winsA[3] + winsB[3]) + "%");

				var attackNames = Battlers.Where(b => b.Team == Teams.TeamB)
									.SelectMany(e => scripts[(int)e.EnemyId].Attacks)
									.Where(a => a != EnemyAttackIds.Nothing)
									.Distinct()
									.ToList()
									.Select(a => Battle.BattleActions[(int)a].Name)
									.ToList();


				var winRatio = (winsA[(int)currentCompanion - 1] * 100) / (winsA[(int)currentCompanion - 1] + winsB[(int)currentCompanion - 1]);
				runningratio += winRatio;

				resultsTracker.Add((new(attackNames), winRatio));
				/*
				(int half, int quarter, int zero) winIncrement = (0, 0, 0);

				if (winRatio <= 0)
				{
					winIncrement = (0, 0, 1);
				}
				else if (winRatio <= 25)
				{
					winIncrement = (0, 1, 0);
				}
				else if (winRatio <= 50)
				{
					winIncrement = (1, 0, 0);
				}

				if (winRatio <= 50)
				{
					foreach (var attack in attackNames)
					{
						if (resultsTracker.ContainsKey(attack))
						{
							resultsTracker[attack] = (resultsTracker[attack].half + winIncrement.half, resultsTracker[attack].quarter + winIncrement.quarter, resultsTracker[attack].zero + winIncrement.zero);
						}
						else
						{
							resultsTracker[attack] = winIncrement;
						}
					}
				}
				*/
				//SimLog.Add("Total: " + (winsA.Sum() * 100) / (winsA.Sum() + winsB.Sum()) + "%");
				/*
				if (!Battlers.Find(b => b.IsBen).ImproveGear(Battlers.Where(b => b.Team == Teams.TeamB).ToList(), rng))
				{
					break;
				}*/
			}

			List<string> nameList = resultsTracker.SelectMany(r => r.attacks).Distinct().ToList();
			attackpairs = attackpairs.Distinct().ToList();

			//var test = resultsTracker.GroupBy(r => r.attacks.GetRange(0, 2)).ToList();
			
			List<(string attack, int max, int min, int average, int rating)> finalResult = nameList
				.Select(n => (n,
					resultsTracker.Where(r => r.attacks.Contains(n)).Max(r => r.percent),
					resultsTracker.Where(r => r.attacks.Contains(n)).Min(r => r.percent),
					(int)resultsTracker.Where(r => r.attacks.Contains(n)).Average(r => r.percent),
					ratings[n])
					)
				.OrderByDescending(r => r.Item4)
				.ToList();

			//List<(string attack, string attack1, string attack2, int max, int min, int average, int count)> finalResult2 = attackpairs
			var finalResult2 = attackpairs
				.Select(n => (n.attack1 + ", " + n.attack2,
					n.attack1,
					n.attack2,
					resultsTracker.Where(r => r.attacks.Contains(n.attack1) && r.attacks.Contains(n.attack2)).Max(r => r.percent),
					resultsTracker.Where(r => r.attacks.Contains(n.attack1) && r.attacks.Contains(n.attack2)).Min(r => r.percent),
					(int)resultsTracker.Where(r => r.attacks.Contains(n.attack1) && r.attacks.Contains(n.attack2)).Average(r => r.percent),
					resultsTracker.Where(r => r.attacks.Contains(n.attack1) && r.attacks.Contains(n.attack2)).Count())
					)
				.OrderByDescending(r => r.Item6)
				.Select((r,i) => (r, i))
				.ToList();
			
			var finalResult3 = nameList.Select(n =>
				(n,
				finalResult2.Where(r => r.r.attack1 == n || r.r.attack2 == n).Average(r => r.i))).ToList();


			/*
			List<(string attack, int max, int min, int average)> finalResult = test
				.Select(g => (String.Join(", ", g.First().attacks.GetRange(0,2)),
					g.Max(r => r.percent),
					g.Min(r => r.percent),
					(int)g.Average(r => r.percent))
					)
				.OrderByDescending(r => r.Item4)
				.ToList();*/
			//resultsTracker = resultsTracker.OrderByDescending(r => r.Value.zero).ToDictionary(r => r.Key, r => r.Value);
			resultsTracker = resultsTracker.OrderByDescending(s => s.percent).ToList();
			SimLog.Add("Final %: " + runningratio / 100 + "%");
			SimLog.Add("Simulation finished.");
		}
		private void MatrixStart()
		{
			var enemies = Battlers.Where(b => b.Team == Teams.TeamB).ToList();
			var allies = Battlers.Where(b => b.Team == Teams.TeamA).ToList();

			List<List<(int slot, int prob)>> probabilities = new();
			foreach (var enemy in enemies)
			{
				enemy.SimulationMode = true;
				probabilities.Add(enemy.ScriptProbability2());
			}

			List<(int prob, int hpremaining, int incapacited)> results = new();
			ProcessEnemy(enemies, allies, results, 1, probabilities, 0, allies.Select(a => a.Hp).ToList(), allies.Select(a => a.Ailments.ToList()).ToList());

			int allprob = results.Sum(r => r.prob);

			var ok60plus = results.Where(r => r.hpremaining >= 60 && r.incapacited == 0).Sum(r => r.prob) * 100 / allprob;
			var one60plus = results.Where(r => r.hpremaining >= 60 && r.incapacited == 1).Sum(r => r.prob) * 100 / allprob;
			var two60plus = results.Where(r => r.hpremaining >= 60 && r.incapacited == 2).Sum(r => r.prob) * 100 / allprob;
			var okless = results.Where(r => r.hpremaining < 60 && r.incapacited == 0).Sum(r => r.prob) * 100 / allprob;
			var oneless = results.Where(r => r.hpremaining < 60 && r.incapacited == 1).Sum(r => r.prob) * 100 / allprob;
			var twoless = results.Where(r => r.hpremaining < 60 && r.incapacited == 2).Sum(r => r.prob) * 100 / allprob;

			var goodspot = ok60plus + one60plus;
			var badsport = two60plus + oneless + twoless + okless;

			SimLog.Add("Estimate survival rate: " + goodspot + "%");

			foreach (var enemy in enemies)
			{
				enemy.SimulationMode = false;
			}
		}
		private void ProcessEnemy(List<Entity> enemies, List<Entity> allies, List<(int prob, int hpremaining, int incapacited)> results, int runningprob, List<List<(int slot, int prob)>> probabilities, int currentenemy, List<int> hp, List<List<ElementsType>> ailments)
		{

			if (currentenemy >= enemies.Count)
			{
				List<ElementsType> incapicitingAilments = new() { ElementsType.Doom, ElementsType.Stone, ElementsType.Paralysis, ElementsType.Confusion, ElementsType.Sleep };
				results.Add((runningprob, allies.Sum(a => a.Hp) * 100 / allies.Sum(a => a.MaxHp), allies.Count(a => a.Ailments.Intersect(incapicitingAilments).Any())));
				Log.Add("Result added");
			}
			else
			{
				List<ElementsType> incapicitingAilments = new() { ElementsType.Doom, ElementsType.Stone };

				foreach (var action in probabilities[currentenemy])
				{
					for (int i = 0; i < allies.Count; i++)
					{
						allies[i].HealUp();
						allies[i].Hp = hp[i];
						allies[i].Ailments.AddRange(ailments[i]);
					}

					if (enemies[currentenemy].Actions[action.slot].Targeting == Targetings.MultipleEnemy)
					{
						enemies[currentenemy].Actions[action.slot].Execute(enemies[currentenemy], allies, TargetSelections.PrioritizeMultipleTarget, Log, Rng);
						ProcessEnemy(enemies, allies, results, runningprob * action.prob, probabilities, currentenemy + 1, allies.Select(a => a.Hp).ToList(), allies.Select(a => a.Ailments.ToList()).ToList());
					}
					else
					{
						if (!allies[0].Ailments.Intersect(incapicitingAilments).Any())
						{
							enemies[currentenemy].Actions[action.slot].Execute(enemies[currentenemy], allies.GetRange(0, 1), TargetSelections.PrioritizeSingleTarget, Log, Rng);
							ProcessEnemy(enemies, allies, results, runningprob * action.prob, probabilities, currentenemy + 1, allies.Select(a => a.Hp).ToList(), allies.Select(a => a.Ailments.ToList()).ToList());
						}

						for (int i = 0; i < allies.Count; i++)
						{
							allies[i].HealUp();
							allies[i].Hp = hp[i];
							allies[i].Ailments.AddRange(ailments[i]);
						}

						if (allies.Count > 1 && !allies[1].Ailments.Intersect(incapicitingAilments).Any())
						{
							enemies[currentenemy].Actions[action.slot].Execute(enemies[currentenemy], allies.GetRange(1, 1), TargetSelections.PrioritizeSingleTarget, Log, Rng);
							ProcessEnemy(enemies, allies, results, runningprob * action.prob, probabilities, currentenemy + 1, allies.Select(a => a.Hp).ToList(), allies.Select(a => a.Ailments.ToList()).ToList());
						}
					}
				}
			}
		}
		private void GenRoundMatrix()
		{ 
			var enemies = Battlers.Where(b => b.Team == Teams.TeamB).ToList();
			var allies = Battlers.Where(b => b.Team == Teams.TeamA).ToList();
			List<Dictionary<int, int>> trueOdds = new();

			List<ElementsType> incapicitingAilments = new() { ElementsType.Doom, ElementsType.Stone, ElementsType.Paralysis, ElementsType.Confusion, ElementsType.Sleep };
			List<(int prob, int hpremaining, int maxdamage, int incapacited)> results = new();

			foreach (var enemy in enemies)
			{
				enemy.SimulationMode = true;
				
				var probs = enemy.ScriptProbability2();

				foreach (var action in probs)
				{
					if (enemy.Actions[action.slot].Targeting == Targetings.MultipleEnemy)
					{
						allies.ForEach(a => a.HealUp());
						enemy.Actions[action.slot].Execute(enemy, allies, TargetSelections.PrioritizeMultipleTarget, Log, Rng);
						var morehp = allies.OrderByDescending(a => a.MaxHp).First();
						results.Add((action.prob, allies.Sum(a => a.Hp), morehp.MaxHp - morehp.Hp, allies.Count(a => a.Ailments.Intersect(incapicitingAilments).Any())));
					}
					else
					{
						allies.ForEach(a => a.HealUp());
						enemy.Actions[action.slot].Execute(enemy, allies.GetRange(0,1), TargetSelections.PrioritizeSingleTarget, Log, Rng);
						results.Add((action.prob / 2, allies.Sum(a => a.Hp), allies[0].MaxHp - allies[0].Hp, allies[0].Ailments.Intersect(incapicitingAilments).Any() ? 1 : 0));

						if (allies.Count > 1)
						{
							allies[0].HealUp();
							enemy.Actions[action.slot].Execute(enemy, allies.GetRange(1, 1), TargetSelections.PrioritizeSingleTarget, Log, Rng);
							results.Add((action.prob / 2, allies.Sum(a => a.Hp), allies[1].MaxHp - allies[1].Hp, allies[1].Ailments.Intersect(incapicitingAilments).Any() ? 1 : 0));
						}
					}
				}
			}

			enemies.ForEach(e => e.SimulationMode = false);
		}
		private void InitTeams(PowerLevels powerlevel, EnemyIds formation, int levelben, int levelcomp)
		{
			Battlers = new()
			{
				new Entity(Log, Rng, powerlevel, CompanionsId.Benjamin, levelben),
				new Entity(Log, Rng, powerlevel, currentCompanion, levelcomp),
			};

			InitEnemies(formation);
		}
		private void InitEnemies(EnemyIds formation)
		{

			//scripts.ShuffleAttacks(EnemizerAttacks.Chaos, EnemizerGroups.MobsBossesDK, Rng);
			scripts.Balanced(EnemizerGroups.MobsBossesDK, true, true, Rng);

			var formationentities = formations[formation];
			Battlers.RemoveAll(b => b.Team == Teams.TeamB);
			foreach (var entity in formationentities)
			{
				Battlers.Add(new Entity(Log, Rng, enemies[(int)entity], scripts));
			}
		}

		Dictionary<EnemyIds, List<EnemyIds>> formations = new()
		{
			{ EnemyIds.Behemoth, new() { EnemyIds.Behemoth } },
			{ EnemyIds.Minotaur, new() { EnemyIds.Minotaur } },
			{ EnemyIds.FlamerusRex, new() { EnemyIds.FlamerusRex } },
			{ EnemyIds.Squidite, new() { EnemyIds.Squidite, EnemyIds.Sparna, EnemyIds.Sparna } },
			{ EnemyIds.SnowCrab, new() { EnemyIds.SnowCrab, EnemyIds.DesertHag, EnemyIds.DesertHag } },
			{ EnemyIds.IceGolem, new() { EnemyIds.IceGolem } },
			{ EnemyIds.Jinn, new() { EnemyIds.Jinn, EnemyIds.RedBone, EnemyIds.RedBone } },
			{ EnemyIds.Medusa, new() { EnemyIds.Medusa, EnemyIds.Werewolf, EnemyIds.Werewolf } },
			{ EnemyIds.DualheadHydra, new() { EnemyIds.DualheadHydra } },
			{ EnemyIds.Gidrah, new() { EnemyIds.Gidrah, EnemyIds.Skuldier, EnemyIds.Skuldier } },
			{ EnemyIds.Dullahan, new() { EnemyIds.Dullahan, EnemyIds.Vampire, EnemyIds.Vampire } },
			{ EnemyIds.Thanatos, new() { EnemyIds.Dullahan } },
			{ EnemyIds.Pazuzu, new() { EnemyIds.Pazuzu } },
			{ EnemyIds.SkullrusRex, new() { EnemyIds.SkullrusRex, EnemyIds.Shadow, EnemyIds.Shadow } },
			{ EnemyIds.StoneGolem, new() { EnemyIds.StoneGolem, EnemyIds.Cerberus, EnemyIds.Cerberus } },
			{ EnemyIds.TwinheadWyvern, new() { EnemyIds.TwinheadWyvern, EnemyIds.Iflyte, EnemyIds.Stheno } },
			{ EnemyIds.Zuh, new() { EnemyIds.Zuh, EnemyIds.Chimera, EnemyIds.Thanatos } },
			{ EnemyIds.DarkKing, new() { EnemyIds.DarkKing } },
			{ EnemyIds.WaterHag, new() { EnemyIds.WaterHag, EnemyIds.WaterHag, EnemyIds.Vampire } },
			{ EnemyIds.Flazzard, new() { EnemyIds.Flazzard, EnemyIds.Flazzard } },
		};
		private void DoBattle(MT19337 rng)
		{
			List<ElementsType> deathAilments = new() { ElementsType.Doom, ElementsType.Stone };

			Log.Add("Battle started.");
			bool teamdefeated = false;
			int roundcount = 0;

			while (!teamdefeated)
			{

				
				roundcount++;
				Log.Add("Round " + roundcount + " started.");
				DoRound(rng);

				if (roundcount > 200)
				{
					Battlers.Where(b => b.Team == Teams.TeamA).ToList().ForEach(b => b.Ailments.Add(ElementsType.Doom));
				}	

				if (!Battlers.Where(b => b.Team == Teams.TeamA && !b.Ailments.Intersect(deathAilments).Any()).Any())
				{
					Log.Add("Team A is defeated.");
					winsB[(int)currentCompanion - 1]++;
					teamdefeated = true;
				}
				else if (!Battlers.Where(b => b.Team == Teams.TeamB && !b.Ailments.Intersect(deathAilments).Any()).Any())
				{
					Log.Add("Team B is defeated.");
					winsA[(int)currentCompanion - 1]++;
					teamdefeated = true;
				}
			}
		}
		private void DoRound(MT19337 rng)
		{
			// Ben choose is action before initiative
			Battlers.Find(b => b.IsBen).PlayerPickAction(Battlers, rng);

			// Roll initiative
			foreach (var battler in Battlers)
			{
				battler.RollInitiative(rng);
				battler.Recovered = false;
			}

			Battlers = Battlers.OrderByDescending(b => b.Initiative).ToList();

			List<ElementsType> deathAilments = new() { ElementsType.Doom, ElementsType.Stone };
			List<ElementsType> disabledAilments = new() { ElementsType.Paralysis, ElementsType.Sleep };

			foreach (var battler in Battlers)
			{
				if (!Battlers.Where(b => b.Team == Teams.TeamA && !b.TechnicalDeath).Any() || !Battlers.Where(b => b.Team == Teams.TeamB && !b.TechnicalDeath).Any())
				{
					break;
				}
				
				if (!battler.Ailments.Intersect(deathAilments).Any())
				{
					bool skipround = false;

					if (battler.Ailments.Contains(ElementsType.Paralysis))
					{
						skipround = true;
						
						if (rng.Between(0, 100) < 0x14)
						{
							battler.Ailments.Remove(ElementsType.Paralysis);
							Log.Add(battler.Name + " recovered from paralysis.");
						}
					}

					if (battler.Ailments.Contains(ElementsType.Sleep))
					{
						skipround = true;

						if (rng.Between(0, 100) < 0x1E)
						{
							battler.Ailments.Remove(ElementsType.Sleep);
							Log.Add(battler.Name + " recovered from sleep.");
						}
					}

					if (battler.Ailments.Contains(ElementsType.Confusion))
					{
						if (rng.Between(0, 100) < 0x28)
						{
							battler.Ailments.Remove(ElementsType.Confusion);
							Log.Add(battler.Name + " recovered from confusion.");
							skipround = true;
						}
					}

					if (battler.Recovered)
					{
						battler.Recovered = false;
						skipround = true;
					}

					if (!skipround)
					{
						Log.Add(battler.Name + " is acting.");
						battler.DoRound(Battlers, rng);
					}
				}
			}


			foreach (var battler in Battlers)
			{
				if (!battler.TechnicalDeath && battler.Ailments.Contains(ElementsType.Poison))
				{
					battler.ProcessDamage(battler.MaxHp / 16, 0);
					Log.Add(battler.Name + " took " + (battler.MaxHp / 16) + " poison damage.");
				}
			}


			var multiplyingBattlers = Battlers.Where(b => b.Multiply).ToList();

			foreach (var battler in multiplyingBattlers)
			{
				if (battler.Multiply)
				{
					battler.Multiply = false;
					var wholeteam = Battlers.Where(b => b.Team == battler.Team).ToList();
					var deadteammates = wholeteam.Where(b => b.TechnicalDeath).ToList();

					if (deadteammates.Any() && wholeteam.Count >= 3)
					{
						Battlers.Remove(deadteammates.First());
					}

					if (wholeteam.Count < 3)
					{
						Entity copy = new Entity(Log, Rng, enemies[(int)battler.EnemyId], scripts);
						copy.Hp = battler.Hp;
						Battlers.Add(copy);
					}
				}
			}
		}

		public static Dictionary<int, BattleAction> BattleActions = new()
		{

			//new BattleAction("Steel Sword", 0x20, 10, 100, ActionRoutines.Sword, new(), new(), Targetings.SingleEnemy) },

			// Items
			{ 0x13, new BattleAction("Refresher", 0x13, 00, 100, ActionRoutines.MagicStatsDebuff, new(), new(), Targetings.SingleAlly) },
			// Spells
			{ 0x14, new BattleAction("Exit Book", 0x14, 00, 100, ActionRoutines.MagicUnknown2, new(), new(), Targetings.SingleEnemy) },
			{ 0x15, new BattleAction("Cure Book", 0x15, 0x32, 100, ActionRoutines.Cure, new() { ElementsType.Zombie }, new(), Targetings.SelectionAny) },
			{ 0x16, new BattleAction("Heal Book", 0x16, 0, 100, ActionRoutines.Heal, new(), new() { ElementsType.Stone }, Targetings.SingleAny) },
			{ 0x17, new BattleAction("Life Book", 0x17, 0, 100, ActionRoutines.Life, new() { ElementsType.Zombie }, new() { ElementsType.Doom }, Targetings.SingleAny) },
			{ 0x18, new BattleAction("Earthquake Book", 0x18, 0x19, 100, ActionRoutines.MagicDamage3, new() { ElementsType.Earth }, new(), Targetings.MultipleEnemy) },
			{ 0x19, new BattleAction("Blizzard Book", 0x19, 0x87, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Water }, new(), Targetings.SelectionEnemy) },
			{ 0x1A, new BattleAction("Fire Book", 0x1A, 0x55, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Fire }, new(), Targetings.SelectionEnemy) },
			{ 0x1B, new BattleAction("Aero Book", 0x1B, 0xEB, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Air }, new(), Targetings.SelectionEnemy) },
			{ 0x1C, new BattleAction("Thunder Seal", 0x1C, 0xB4, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Air, ElementsType.Water }, new(), Targetings.SelectionEnemy) },
			{ 0x1D, new BattleAction("White Seal", 0x1D, 0x8C, 100, ActionRoutines.MagicDamage3, new(), new(), Targetings.MultipleEnemy) },
			{ 0x1E, new BattleAction("Meteor Seal", 0x1E, 0xAA, 100, ActionRoutines.MagicDamage3, new() { ElementsType.Earth }, new(), Targetings.MultipleEnemy) },
			{ 0x1F, new BattleAction("Flare Seal", 0x1F, 0xC8, 100, ActionRoutines.MagicDamage3, new() { ElementsType.Fire }, new(), Targetings.MultipleEnemy) },

			// Weapon
			{ 0x20, new BattleAction("Steel Sword", 0x20, 10, 100, ActionRoutines.Sword, new(), new(), Targetings.SingleEnemy) },
			{ 0x21, new BattleAction("Knight Sword", 0x21, 0x5A, 100, ActionRoutines.Sword, new(), new(), Targetings.SingleEnemy) },
			{ 0x22, new BattleAction("Excalibur", 0x22, 0xFA, 100, ActionRoutines.Sword, new(), new(), Targetings.SingleEnemy) },
			{ 0x23, new BattleAction("Axe", 0x23, 0x12, 91, ActionRoutines.Axe, new() { ElementsType.Axe }, new(), Targetings.SingleEnemy) },
			{ 0x24, new BattleAction("Battle Axe", 0x24, 0x7D, 91, ActionRoutines.Axe, new() { ElementsType.Axe }, new(), Targetings.SingleEnemy) },
			{ 0x25, new BattleAction("Giant Axe", 0x25, 0xD2, 91, ActionRoutines.Axe, new() { ElementsType.Axe }, new(), Targetings.SingleEnemy) },
			{ 0x26, new BattleAction("Cat Claw", 0x26, 0x05, 82, ActionRoutines.Claw, new(), new() { ElementsType.Paralysis, ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0x27, new BattleAction("Charm Claw", 0x27, 0x4B, 82, ActionRoutines.Claw, new(), new() { ElementsType.Paralysis, ElementsType.Poison, ElementsType.Sleep, ElementsType.Blind, ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0x28, new BattleAction("Dragon Claw", 0x28, 0xA0, 82, ActionRoutines.Claw, new(), new() { ElementsType.Paralysis, ElementsType.Poison, ElementsType.Sleep, ElementsType.Blind, ElementsType.Confusion, ElementsType.Stone }, Targetings.SingleEnemy) },
			{ 0x29, new BattleAction("Bomb", 0x29, 0x0E, 100, ActionRoutines.Bomb, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0x2A, new BattleAction("Jumbo Bomb", 0x2A, 0x19, 100, ActionRoutines.Bomb, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0x2B, new BattleAction("Mega Grenade", 0x2B, 0x24, 100, ActionRoutines.Bomb, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0x2C, new BattleAction("Morning Star", 0x2C, 0xC8, 100, ActionRoutines.Axe, new() { ElementsType.Axe }, new(), Targetings.SingleEnemy) },
			{ 0x2D, new BattleAction("Bow of Grace", 0x2D, 0x50, 100, ActionRoutines.Projectile, new() { ElementsType.Projectile }, new() { ElementsType.Blind }, Targetings.SingleEnemy) },
			{ 0x2E, new BattleAction("Shuriken", 0x2E, 0x28, 100, ActionRoutines.Projectile, new() { ElementsType.Projectile }, new() { ElementsType.Paralysis, ElementsType.Poison }, Targetings.SingleEnemy) },

			// Enemy Attack
			{ 0x40, new BattleAction("Sword", 0x40, 0x05, 100, ActionRoutines.Sword, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x41, new BattleAction("Scimitar", 0x41, 0x0A, 100, ActionRoutines.Sword, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x42, new BattleAction("Dragon Cut", 0x42, 0x0A, 100, ActionRoutines.Sword, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x43, new BattleAction("Rapier", 0x43, 0x30, 100, ActionRoutines.Sword, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x44, new BattleAction("Axe", 0x44, 0x05, 100, ActionRoutines.Axe, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x45, new BattleAction("Beam", 0x45, 0x16, 100, ActionRoutines.Bomb, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x46, new BattleAction("Bone Missile", 0x46, 0x06, 100, ActionRoutines.Bomb, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x47, new BattleAction("Bow & Arrow", 0x47, 0x0F, 100, ActionRoutines.Projectile, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x48, new BattleAction("Blow Dart", 0x48, 0x0F, 100, ActionRoutines.Projectile, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x49, new BattleAction("CureSelf", 0x49, 0x19, 100, ActionRoutines.Cure, new(), new(), Targetings.SingleAlly) },
			{ 0x4A, new BattleAction("HealSelf", 0x4A, 0x00, 100, ActionRoutines.Heal, new(), new() { ElementsType.Stone }, Targetings.SingleAlly) },
			{ 0x4B, new BattleAction("QuakeSpell", 0x4B, 0x14, 100, ActionRoutines.MagicDamage3, new() { ElementsType.Earth }, new(), Targetings.MultipleEnemy) },
			{ 0x4C, new BattleAction("Blizzard", 0x4C, 0x0A, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Water }, new(), Targetings.SingleEnemy) },
			{ 0x4D, new BattleAction("Fire", 0x4D, 0x0F, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Fire }, new(), Targetings.SingleEnemy) },
			{ 0x4E, new BattleAction("ThunderSpell", 0x4E, 0x14, 100, ActionRoutines.MagicDamage2, new() { ElementsType.Water, ElementsType.Air }, new(), Targetings.SingleEnemy) },
			// weird one
			{ 0x4F, new BattleAction("Reflectant", 0x4F, 0x00, 55, ActionRoutines.Seed, new(), new(), Targetings.SingleAlly) },
			{ 0x50, new BattleAction("Electrapulse", 0x50, 0xD1, 100, ActionRoutines.MagicStatsDebuff, new(), new(), Targetings.SingleEnemy) },
			{ 0x51, new BattleAction("Power Drain", 0x51, 0x11, 100, ActionRoutines.MagicStatsDebuff, new(), new(), Targetings.SingleEnemy) },
			{ 0x52, new BattleAction("Spark", 0x52, 0x12, 100, ActionRoutines.MagicStatsDebuff, new(), new(), Targetings.SingleEnemy) },
			{ 0x53, new BattleAction("Iron Nail", 0x53, 0x52, 100, ActionRoutines.MagicStatsDebuff, new(), new(), Targetings.SingleEnemy) },
			{ 0x54, new BattleAction("Scream", 0x54, 0x51, 100, ActionRoutines.MagicStatsDebuff, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x55, new BattleAction("Quicksand", 0x55, 0x92, 100, ActionRoutines.MagicStatsDebuff, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x56, new BattleAction("Doom Gaze", 0x56, 0x00, 94, ActionRoutines.Ailments1, new() { ElementsType.Projectile }, new() { ElementsType.Doom }, Targetings.SingleEnemy) },
			{ 0x57, new BattleAction("Doom Powder", 0x57, 0x00, 94, ActionRoutines.Ailments1, new() { ElementsType.Projectile }, new() { ElementsType.Doom }, Targetings.SingleEnemy) },
			{ 0x58, new BattleAction("Cure", 0x58, 0x00, 55, ActionRoutines.Cure, new(), new(), Targetings.SingleAlly) },
			{ 0x59, new BattleAction("Fire Breath Fixed", 0x59, 0x5C, 100, ActionRoutines.PureDamage1, new() { ElementsType.Fire }, new(), Targetings.MultipleEnemy) },
			{ 0x5A, new BattleAction("Punch", 0x5A, 0x04, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x5B, new BattleAction("Kick", 0x5B, 0x05, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x5C, new BattleAction("Uppercut", 0x5C, 0x0F, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x5D, new BattleAction("Stab", 0x5D, 0x05, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x5E, new BattleAction("Head Butt", 0x5E, 0x0F, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x5F, new BattleAction("Body Slam", 0x5F, 0x0A, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x60, new BattleAction("Scrunch", 0x60, 0x0F, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x61, new BattleAction("Full Nelson", 0x61, 0x0A, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x62, new BattleAction("Neck Choke", 0x62, 0x14, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x63, new BattleAction("Dash", 0x63, 0x14, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x64, new BattleAction("Roundhouse", 0x64, 0x0B, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x65, new BattleAction("Choke Up", 0x65, 0x32, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x66, new BattleAction("Stomp Stomp", 0x66, 0x32, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x67, new BattleAction("Mega Punch", 0x67, 0x05, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x68, new BattleAction("Bearhug", 0x68, 0x02, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x69, new BattleAction("Axe Bomber", 0x69, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x6A, new BattleAction("Piledriver Fixed", 0x6A, 0x50, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x6B, new BattleAction("Sky Attack Fixed", 0x6B, 0x46, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0x6C, new BattleAction("Wraparound", 0x6C, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x6D, new BattleAction("Dive", 0x6D, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x6E, new BattleAction("Attach", 0x6E, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x6F, new BattleAction("Mucus", 0x6F, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x70, new BattleAction("Claw", 0x70, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x71, new BattleAction("Fang", 0x71, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x72, new BattleAction("Beak", 0x72, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x73, new BattleAction("Sting", 0x73, 0x0F, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x74, new BattleAction("Tail", 0x74, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x75, new BattleAction("Pseudopod", 0x75, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x76, new BattleAction("Bite", 0x76, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x77, new BattleAction("Hydro Acid", 0x77, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x78, new BattleAction("Branch", 0x78, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x79, new BattleAction("Fin", 0x79, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x7A, new BattleAction("Scissor", 0x7A, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x7B, new BattleAction("Whip Tongue", 0x7B, 0x05, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x7C, new BattleAction("Horn", 0x7C, 0x01, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x7D, new BattleAction("Giant Blade", 0x7D, 0x19, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x7E, new BattleAction("Headboomerang", 0x7E, 0xFA, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0x7F, new BattleAction("Chew Off", 0x7F, 0x01, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0x80, new BattleAction("Quake", 0x80, 0x96, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Earth, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x81, new BattleAction("Flame", 0x81, 0x50, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x82, new BattleAction("Flame Sweep", 0x82, 0xC8, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x83, new BattleAction("Fire Ball", 0x83, 0x8C, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x84, new BattleAction("Flame Pillar", 0x84, 0xFA, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x85, new BattleAction("Heatwave", 0x85, 0xDC, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Air, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x86, new BattleAction("Watergun", 0x86, 0x3C, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x87, new BattleAction("Coldness", 0x87, 0x41, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x88, new BattleAction("Icy Foam", 0x88, 0x50, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x89, new BattleAction("Ice Block", 0x89, 0x78, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x8A, new BattleAction("Snowstorm", 0x8A, 0xFA, 100, ActionRoutines.PhysicalDamage1, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x8B, new BattleAction("Whirlwater", 0x8B, 0xC8, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x8C, new BattleAction("Ice Breath", 0x8C, 0x46, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x8D, new BattleAction("Tornado", 0x8D, 0x50, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x8E, new BattleAction("Typhoon", 0x8E, 0xFA, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Projectile }, new(), Targetings.MultipleEnemy) },
			{ 0x8F, new BattleAction("Hurricane", 0x8F, 0x0A, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x90, new BattleAction("Thunder", 0x90, 0x64, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x91, new BattleAction("Thunder Bean", 0x91, 0x50, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Water, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0x92, new BattleAction("Corrode Gas", 0x92, 0x32, 100, ActionRoutines.Ailments1, new(), new() { ElementsType.Doom }, Targetings.SingleEnemy) },
			{ 0x93, new BattleAction("Doom Dance", 0x93, 0xC8, 94, ActionRoutines.Ailments1, new(), new() { ElementsType.Doom }, Targetings.SingleEnemy) },
			{ 0x94, new BattleAction("Sonic Boom", 0x94, 0x32, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0x95, new BattleAction("Bark", 0x95, 0x46, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0x96, new BattleAction("Screechvoice", 0x96, 0xFA, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0x97, new BattleAction("Para-needle", 0x97, 0x46, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0x98, new BattleAction("Para-claw", 0x98, 0x46, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0x99, new BattleAction("Para-snake", 0x99, 0xE6, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0x9A, new BattleAction("Para-breath", 0x9A, 0xB4, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0x9B, new BattleAction("Poison Sting", 0x9B, 0x46, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0x9C, new BattleAction("Poison Thorn", 0x9C, 0x46, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0x9D, new BattleAction("Rotton Mucus", 0x9D, 0x46, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0x9E, new BattleAction("Poison Snake", 0x9E, 0x50, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0x9F, new BattleAction("Poisonbreath", 0x9F, 0xFA, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Poison }, Targetings.MultipleEnemy) },
			{ 0xA0, new BattleAction("Blinder", 0xA0, 0x4B, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Blind }, Targetings.SingleEnemy) },
			{ 0xA1, new BattleAction("Blackness", 0xA1, 0x4B, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Projectile }, new() { ElementsType.Blind }, Targetings.SingleEnemy) },
			{ 0xA2, new BattleAction("Stone Beak", 0xA2, 0x2D, 100, ActionRoutines.PhysicalDamage5, new(), new() { ElementsType.Stone }, Targetings.SingleEnemy) },
			{ 0xA3, new BattleAction("Gaze", 0xA3, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0xA4, new BattleAction("Stare", 0xA4, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0xA5, new BattleAction("Spooky Laugh", 0xA5, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0xA6, new BattleAction("Riddle", 0xA6, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Confusion }, Targetings.SingleEnemy) },
			{ 0xA7, new BattleAction("Bad Breath", 0xA7, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0xA8, new BattleAction("Body Odor", 0xA8, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0xA9, new BattleAction("Para-stare", 0xA9, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0xAA, new BattleAction("Poison Fluid", 0xAA, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0xAB, new BattleAction("Poison Flour", 0xAB, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0xAC, new BattleAction("Hypno-sleep", 0xAC, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Sleep }, Targetings.SingleEnemy) },
			{ 0xAD, new BattleAction("Lullaby", 0xAD, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Sleep }, Targetings.SingleEnemy) },
			{ 0xAE, new BattleAction("Sleep Lure", 0xAE, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Sleep }, Targetings.SingleEnemy) },
			{ 0xAF, new BattleAction("Sleep Powder", 0xAF, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Sleep }, Targetings.SingleEnemy) },
			{ 0xB0, new BattleAction("Blind Flash", 0xB0, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Blind }, Targetings.SingleEnemy) },
			{ 0xB1, new BattleAction("Smokescreen", 0xB1, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Blind }, Targetings.SingleEnemy) },
			{ 0xB2, new BattleAction("Muffle", 0xB2, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Silence }, Targetings.SingleEnemy) },
			{ 0xB3, new BattleAction("Silence Song", 0xB3, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Silence }, Targetings.SingleEnemy) },
			{ 0xB4, new BattleAction("Stone Gas", 0xB4, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Stone }, Targetings.SingleEnemy) },
			{ 0xB5, new BattleAction("Stone Gaze", 0xB5, 0x00, 100, ActionRoutines.PhysicalDamage6, new() { ElementsType.Projectile }, new() { ElementsType.Stone }, Targetings.SingleEnemy) },
			{ 0xB6, new BattleAction("Double Sword", 0xB6, 0x0A, 100, ActionRoutines.PhysicalDamage7, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xB7, new BattleAction("Double Hit", 0xB7, 0x0A, 100, ActionRoutines.PhysicalDamage7, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xB8, new BattleAction("Triple Fang", 0xB8, 0x46, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 3) },
			{ 0xB9, new BattleAction("Double Kick", 0xB9, 0x3C, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 2) },
			{ 0xBA, new BattleAction("Twin Shears", 0xBA, 0x32, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 2) },
			{ 0xBB, new BattleAction("Three Heads", 0xBB, 0xC8, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 3) },
			{ 0xBC, new BattleAction("6 Psudopods", 0xBC, 0x32, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 6) },
			{ 0xBD, new BattleAction("Snake Head", 0xBD, 0x05, 100, ActionRoutines.PhysicalDamage8, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy, 6) },
			{ 0xBE, new BattleAction("Drain", 0xBE, 0x64, 100, ActionRoutines.PhysicalDamage9, new() { ElementsType.Zombie }, new(), Targetings.SingleEnemy) },
			{ 0xBF, new BattleAction("Dissolve", 0xBF, 0x64, 100, ActionRoutines.PhysicalDamage9, new() { ElementsType.Zombie }, new(), Targetings.SingleEnemy) },
			{ 0xC0, new BattleAction("Sucker Stick", 0xC0, 0x3C, 100, ActionRoutines.PhysicalDamage9, new() { ElementsType.Zombie }, new(), Targetings.MultipleEnemy) },
			{ 0xC1, new BattleAction("Selfdestruct", 0xC1, 0x32, 100, ActionRoutines.SelfDestruct, new(), new(), Targetings.SingleEnemy) },
			{ 0xC2, new BattleAction("Multiply", 0xC2, 0x00, 100, ActionRoutines.Multiply, new(), new(), Targetings.SingleEnemy) },
			{ 0xC3, new BattleAction("Para Gas", 0xC3, 0x00, 100, ActionRoutines.PhysicalDamage6, new(), new() { ElementsType.Paralysis }, Targetings.SingleEnemy) },
			{ 0xC4, new BattleAction("Rip Earth", 0xC4, 0x2D, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Earth }, new(), Targetings.MultipleEnemy) },
			{ 0xC5, new BattleAction("Stone Block", 0xC5, 0x01, 100, ActionRoutines.PhysicalDamage5, new() { ElementsType.Earth }, new(), Targetings.SingleEnemy) },
			{ 0xC6, new BattleAction("Windstorm", 0xC6, 0x05, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air }, new(), Targetings.SingleEnemy) },
			{ 0xC7, new BattleAction("Twin Fang", 0xC7, 0x37, 100, ActionRoutines.PhysicalDamage8, new(), new(), Targetings.SingleEnemy) },
			{ 0xC8, new BattleAction("Psychshield (bug)", 0xC8, 0x00, 100, ActionRoutines.PureDamage1, new(), new(), Targetings.SingleEnemy) },
			{ 0xC9, new BattleAction("Psychshield", 0xC9, 0xC8, 100, ActionRoutines.PureDamage1, new(), new(), Targetings.SingleEnemy) },
			{ 0xCA, new BattleAction("Dark Cane", 0xCA, 0x50, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xCB, new BattleAction("Dark Saber", 0xCB, 0x73, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xCC, new BattleAction("Ice Sword", 0xCC, 0x58, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xCD, new BattleAction("Fire Sword", 0xCD, 0x64, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xCE, new BattleAction("Mirror Sword", 0xCE, 0x4B, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xCF, new BattleAction("Quake Axe", 0xCF, 0x46, 100, ActionRoutines.PureDamage1, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0xD0, new BattleAction("Cure Arrow", 0xD0, 0x46, 100, ActionRoutines.PureDamage1, new() { ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0xD1, new BattleAction("Lazer", 0xD1, 0x78, 100, ActionRoutines.PureDamage1, new(), new(), Targetings.SingleEnemy) },
			{ 0xD2, new BattleAction("Spider Kids", 0xD2, 0x6C, 100, ActionRoutines.PureDamage1, new(), new(), Targetings.SingleEnemy) },
			{ 0xD3, new BattleAction("Silver Web", 0xD3, 0x50, 100, ActionRoutines.PureDamage1, new(), new() { ElementsType.Confusion, ElementsType.Poison }, Targetings.SingleEnemy) },
			{ 0xD4, new BattleAction("Golden Web", 0xD4, 0x46, 100, ActionRoutines.PureDamage1, new(), new() { ElementsType.Stone }, Targetings.SingleEnemy) },
			{ 0xD5, new BattleAction("Mega Flare", 0xD5, 0x7D, 100, ActionRoutines.PureDamage1, new() { ElementsType.Fire }, new(), Targetings.SingleEnemy) },
			{ 0xD6, new BattleAction("Mega White", 0xD6, 0x58, 100, ActionRoutines.PureDamage1, new(), new(), Targetings.MultipleEnemy) },
			{ 0xD7, new BattleAction("Fire Breath", 0xD7, 0x78, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire }, new(), Targetings.SingleEnemy) },
			{ 0xD8, new BattleAction("Sky Attack", 0xD8, 0xFA, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.MultipleEnemy) },
			{ 0xD9, new BattleAction("Piledriver", 0xD9, 0x0A, 100, ActionRoutines.PhysicalDamage3, new() { ElementsType.Bomb }, new(), Targetings.SingleEnemy) },
			{ 0xDA, new BattleAction("Hurricane Plus", 0xDA, 0x0F, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Air, ElementsType.Projectile }, new(), Targetings.SingleEnemy) },
			{ 0xDB, new BattleAction("Heatwave Unused", 0xDB, 0xDC, 100, ActionRoutines.PhysicalDamage4, new() { ElementsType.Fire, ElementsType.Air, ElementsType.Projectile }, new() { ElementsType.Paralysis, ElementsType.Sleep, ElementsType.Silence }, Targetings.MultipleEnemy) },

			{ 0xFF, new BattleAction("Nothing", 0xFF, 0x00, 0, ActionRoutines.None, new() , new(), Targetings.SelectionEnemy) },
		};
	}
}
