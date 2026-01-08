using RomUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFMQLib
{
	public partial class Enemizer
	{
		public void SetElementalResistWeakness()
		{
			foreach (var enemy in ElementalEnemies)
			{
				var enemydata = enemies.Data[enemy.Key];
				var resist = resistances[enemy.Value].resistances;
				var weak = resistances[enemy.Value].weaknesses;

				enemies.Data[enemy.Key].Resistances = enemies.Data[enemy.Key].Resistances.Except(weak).Concat(resist).Distinct().ToList();
				enemies.Data[enemy.Key].Weaknesses = enemies.Data[enemy.Key].Weaknesses.Except(resist).Concat(weak).Distinct().ToList();
			}
		}

		public void ShuffleResistWeakness(bool shuffle, GameInfoScreen info, MT19337 rng)
		{
			if (!shuffle)
			{
				return;
			}

			var allList = Enum.GetValues<ElementsType>().ToList();
			var elementsMainList = allList.Where(x => (int)x > 0x00FF).ToList();
			var statusMainList = allList.Where(x => (int)x < 0x0100).ToList();
			var elementsShuffledList = elementsMainList.ToList();
			var statusShuffledList = statusMainList.ToList();

			elementsShuffledList.Shuffle(rng);
			statusShuffledList.Shuffle(rng);

			var elementsPairList = elementsMainList.Select((e, i) => (e, elementsShuffledList[i])).ToList();
			var statusPairList = statusMainList.Select((e, i) => (e, statusShuffledList[i])).ToList();

			var allPairList = statusPairList.Concat(elementsPairList).ToList();

			foreach (var enemy in enemies.Data)
			{
				List<ElementsType> newweaks = allPairList.Where(w => enemy.Value.Weaknesses.Contains(w.Item1)).Select(w => w.Item2).ToList();
				List<ElementsType> newresists = allPairList.Where(w => enemy.Value.Resistances.Contains(w.Item1)).Select(w => w.Item2).ToList();

				enemy.Value.Weaknesses = newweaks.ToList();
				enemy.Value.Resistances = newresists.ToList();
			}

			info.ShuffledElementsType = allPairList;
		}

	}
}
