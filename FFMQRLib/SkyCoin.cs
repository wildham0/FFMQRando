using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.ComponentModel;

namespace FFMQLib
{
	public enum SkyCoinModes : int
	{
		[Description("Standard")]
		Standard = 0,
		[Description("Start With")]
		StartWith,
		[Description("Save the Crystals")]
		SaveCrystals,
		[Description("Shattered SkyCoin")]
		SaveCrystals,
	}
	
public partial class FFMQRom : SnesRom
	{
		public void SkyCoin(Flags flags)
		{


		}
	}
}
			
