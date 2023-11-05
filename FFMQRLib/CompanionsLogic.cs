using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using RomUtilities;


namespace FFMQLib
{
	public enum Quests
	{ 
		
	
	
	}

	public class Quest
	{ 
		public Quests Name { get; set; }
		public NewGameFlagsList Gameflag { get; set; }
		public int Quantity { get; set; }
		public string Description { get; set; }
	}

	public partial class Companions
	{ 
		public bool KaeliEnabled { get; set; }
		public bool TristamEnabled { get; set; }
		public bool PhoebeEnabled { get; set; }
		public bool ReubenEnabled { get; set; }
		public int QuestQuantity { get; set; }
		public List<Quest> Quests { get; set; }

		private void InitializeQuests()
		{
			KaeliEnabled = true;
			TristamEnabled = true;
			PhoebeEnabled = true;
			ReubenEnabled = true;
			QuestQuantity = 1;
			Quests = new();
		}



	}

}
