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

	public partial class Companions
	{ 
		public bool KaeliEnabled { get;  }
		public bool TristamEnabled { get; }
		public bool PhoebeEnabled { get; }
		public bool ReubenEnabled { get; }
		public List<(Quests quest, NewGameFlagsList gameflag)> Quests { get; }



	}

}
