using RomUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using static System.Math;
using System.Buffers.Binary;
using System.Reflection.Emit;

namespace FFMQLib
{
	public enum SpellbookType : int
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Extended")]
		Extended,
		[Description("Random Balanced")]
		RandomBalanced,
		[Description("Random Chaos")]
		RandomChaos,
	}
	public class WeightedSpell
	{ 
		public SpellFlags Spell { get; set; }
		public (int min, int max) LevelRange { get; set; }
		public List<(CompanionsId companion, int weight)> Learners { get; set; }
		public WeightedSpell(SpellFlags spell, (int min, int max) range, List<(CompanionsId companion, int weight)> learners)
		{
			Spell = spell;
			LevelRange = range;
			Learners = learners;
		}
		public (CompanionsId companion, int level) GiveSpellRandom(List<CompanionsId> excluded, MT19337 rng)
		{
			var validcompanions = Learners.Where(l => !excluded.Contains(l.companion)).ToList();

			if (!validcompanions.Any())
			{
				return (CompanionsId.None, 0);
			}

			List<CompanionsId> weightedcompanionslist = new();
			foreach (var companion in validcompanions)
			{
				weightedcompanionslist.AddRange(Enumerable.Repeat(companion.companion, companion.weight).ToList());
			}

			return (rng.PickFrom(weightedcompanionslist), rng.Between(LevelRange.min, LevelRange.max));
		}
		public (CompanionsId companion, int level) GiveSpell(CompanionsId companion, MT19337 rng)
		{
			return (companion, rng.Between(LevelRange.min, LevelRange.max));
		}

	}
	public partial class Companions
	{
		public void SetSpellbooks(SpellbookType spellbooktype, GameInfoScreen infoscreen, MT19337 rng)
		{
            var availablecompanions = Available.Where(c => c.Value).Select(c => c.Key).ToList();
            int companioncount = availablecompanions.Count;

			if (companioncount == 0)
			{
				return;
			}

            List<(SpellFlags spell, List<(CompanionsId companion, int weight)> weights)> weightlist = new()
			{
				(SpellFlags.ExitBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 7), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 1),}),
				(SpellFlags.CureBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.HealBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 2), (CompanionsId.Phoebe, 3),}),
				(SpellFlags.QuakeBook, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 2), (CompanionsId.Reuben, 3), (CompanionsId.Phoebe, 2),}),
				(SpellFlags.BlizzardBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.FireBook, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 4), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.AeroBook, new() { (CompanionsId.Kaeli, 4), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.ThunderSeal, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 1), (CompanionsId.Phoebe, 4),}),
				(SpellFlags.WhiteSeal, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 1), (CompanionsId.Reuben, 3), (CompanionsId.Phoebe, 3),}),
				(SpellFlags.MeteorSeal, new() { (CompanionsId.Kaeli, 3), (CompanionsId.Tristam, 3), (CompanionsId.Reuben, 2), (CompanionsId.Phoebe, 2),}),
				(SpellFlags.FlareSeal, new() { (CompanionsId.Kaeli, 1), (CompanionsId.Tristam, 4), (CompanionsId.Reuben, 4), (CompanionsId.Phoebe, 1),}),
			};

			weightlist.ForEach(s => s.weights.RemoveAll(w => !availablecompanions.Contains(w.companion)));

			List<(CompanionsId companion, SpellFlags spell, int level)> spelllist = new();

            if (spellbooktype == SpellbookType.Standard || spellbooktype == SpellbookType.Extended)
			{
				spelllist = new()
				{
					(CompanionsId.Kaeli, SpellFlags.CureBook, rng.Between(8, 15)),
					(CompanionsId.Kaeli, SpellFlags.HealBook, rng.Between(8, 15)),
					(CompanionsId.Kaeli, SpellFlags.AeroBook, rng.Between(8, 15)),
					(CompanionsId.Reuben, SpellFlags.WhiteSeal, rng.Between(24, 31)),
					(CompanionsId.Phoebe, SpellFlags.CureBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.HealBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.FireBook, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.ThunderSeal, rng.Between(1, 15)),
					(CompanionsId.Phoebe, SpellFlags.BlizzardBook, rng.Between(16, 31)),
					(CompanionsId.Phoebe, SpellFlags.WhiteSeal, rng.Between(16, 31)),
				};

				if (spellbooktype == SpellbookType.Extended)
				{
					spelllist.Add((CompanionsId.Tristam, SpellFlags.ExitBook, rng.Between(16, 23)));
					spelllist.Add((CompanionsId.Tristam, SpellFlags.QuakeBook, rng.Between(8, 15)));
					spelllist.Add((CompanionsId.Reuben, SpellFlags.BlizzardBook, rng.Between(8, 15)));
					if (levelingType != LevelingType.Quests && levelingType != LevelingType.SaveCrystalsIndividual)
					{
						spelllist.Add((CompanionsId.Tristam, SpellFlags.FlareSeal, rng.Between(35, 41)));
						spelllist.Add((CompanionsId.Kaeli, SpellFlags.MeteorSeal, rng.Between(35, 41)));
					}
				}

				// Remove spells for unavaiable companions
				spelllist = spelllist.Where(s => availablecompanions.Contains(s.companion)).ToList();
            }
			else if(spellbooktype == SpellbookType.RandomBalanced || spellbooktype == SpellbookType.RandomChaos)
			{
				List<List<int>> levelrange = new()
				{
					Enumerable.Range(1,15).ToList(),
					Enumerable.Range(5,15).ToList(),
					Enumerable.Range(15,15).ToList(),
					Enumerable.Range(1,25).ToList(),
				};

				List<List<SpellFlags>> spells = new()
				{
					new() { SpellFlags.CureBook, SpellFlags.HealBook },
					new() { SpellFlags.QuakeBook, SpellFlags.BlizzardBook, SpellFlags.FireBook, SpellFlags.AeroBook, SpellFlags.ThunderSeal },
					new() { SpellFlags.WhiteSeal, SpellFlags.MeteorSeal, SpellFlags.FlareSeal },
					new() { SpellFlags.ExitBook },
				};

                List<int> counts = new()
				{
					rng.Between(1 * companioncount, 2 * companioncount),
                    rng.Between(1 * companioncount, 2 * companioncount),
                    rng.Between(Max(1, (int)(0.5 * companioncount)), companioncount),
					rng.Between(0, 1)
				};
				
				List<(SpellFlags spell, List<CompanionsId> weight)> collapsedlist = weightlist.Select(x => (x.spell, x.Item2.SelectMany(c => Enumerable.Repeat(c.companion, c.weight)).ToList())).ToList();

                if (spellbooktype == SpellbookType.RandomChaos)
				{
					levelrange = new() { Enumerable.Range(1, 41).Concat(Enumerable.Range(1, 35)).ToList() };
					spells = new() { spells.SelectMany(s => s).ToList() };
					counts = new() { rng.Between((int)(2.5 * companioncount), 6 * companioncount) };
					collapsedlist = weightlist.Select(x => (x.spell, x.Item2.Select(c => c.companion).ToList())).ToList();
				}

				for (int category = 0; category < counts.Count; category++)
				{
					for (int i = 0; i < counts[category]; i++)
					{
						var currentspell = rng.PickFrom(spells[category]);
						var spellweight = collapsedlist.Find(s => s.spell == currentspell);
						var learner = rng.PickFrom(spellweight.weight);
						spellweight.weight.RemoveAll(c => c == learner);
						if (!spellweight.weight.Any())
						{
							spells[category].Remove(currentspell);
						}

						spelllist.Add((learner, currentspell, rng.PickFrom(levelrange[category])));
					}
				}
			}

			// update to appropriate level
			List<LevelingType> cappedLevelingTypes = new() { LevelingType.Quests, LevelingType.QuestsExtended, LevelingType.SaveCrystalsAll, LevelingType.SaveCrystalsIndividual };

			if (cappedLevelingTypes.Contains(levelingType))
			{
				List<(CompanionsId companion, List<int> levels)> levelsbycompanion = new()
				{
					(CompanionsId.Kaeli, new() { 7, 31 } ),
					(CompanionsId.Tristam, new() { 7, 23 } ),
					(CompanionsId.Reuben, new() { 23, 31 } ),
					(CompanionsId.Phoebe, new() { 15, 34 } ),
				};

				if (levelingType == LevelingType.QuestsExtended || levelingType == LevelingType.SaveCrystalsAll)
				{
					levelsbycompanion = new()
					{
						(CompanionsId.Kaeli, new() { 7, 15, 23, 34, 41 } ),
						(CompanionsId.Tristam, new() { 7, 15, 23, 34, 41 } ),
						(CompanionsId.Reuben, new() { 7, 15, 23, 34, 41 } ),
						(CompanionsId.Phoebe, new() { 7, 15, 23, 34, 41 } ),
					};
				}

				for (int i = 0; i < spelllist.Count; i++)
				{ 
					var selectedlevels = levelsbycompanion.Find(x => x.companion == spelllist[i].companion).levels;
					bool nolevelfound = true;
					foreach(var level in selectedlevels)
					{
						if (level >= spelllist[i].level)
						{
							spelllist[i] = (spelllist[i].companion, spelllist[i].spell, level);
							nolevelfound = false;
							break;
						}
					}

					if (nolevelfound)
					{
						spelllist[i] = (spelllist[i].companion, spelllist[i].spell, selectedlevels.Last());
					}
				}
			}

			// Spell are distributed, so update InfoScreen
			infoscreen.SpellLearning = spelllist.GroupBy(s => s.companion).Select(g => (g.Key, g.Select(s => (s.level, s.spell)).ToList())).ToList();
			
			// Regroup spells by level and create new LoadOut
			List<CompanionsId> companions = new() { CompanionsId.Kaeli, CompanionsId.Tristam, CompanionsId.Phoebe, CompanionsId.Reuben };
			
			foreach (var companion in companions)
			{
				spelllist.Add((companion, SpellFlags.LifeBook, 1));

				var companionspells = spelllist.Where(s => s.companion == companion)
										.OrderBy(s => s.level)
										.GroupBy(s => s.level)
										.Select(g => (g.Key, g.Select(s => s.spell).ToList()))
										.ToList();
				
				List<LevelThreshold> newloadout = new();
				List<SpellFlags> cumulativespells = new();
				bool level23set = false;

				foreach (var spellsgroup in companionspells)
				{
					if (spellsgroup.Key == 23)
					{
						level23set = true;
					}
					else if (spellsgroup.Key > 23 && !level23set)
					{
						newloadout.Add(new(23, this[companion].ArmorSet2, new(cumulativespells)));
						level23set = true;
					}

					cumulativespells.AddRange(spellsgroup.Item2);
					newloadout.Add(new(spellsgroup.Key, spellsgroup.Key < 23 ? this[companion].ArmorSet1 : this[companion].ArmorSet2, new(cumulativespells)));
				}

				if (!level23set)
				{
					newloadout.Add(new(23, this[companion].ArmorSet2, new(cumulativespells)));
				}
				
				this[companion].LoadOut = newloadout;
			}
		}
	}
}
