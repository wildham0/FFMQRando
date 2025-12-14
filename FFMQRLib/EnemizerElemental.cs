using FFMQLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMQRLib
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


	public class EnemizerElemental
	{
		private List<EnemizerElements> validElements = new() { EnemizerElements.Fire, EnemizerElements.Water, EnemizerElements.Earth, EnemizerElements.Air, EnemizerElements.Thunder };

		//public 

	}
}
