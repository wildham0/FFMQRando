using System.Collections.Generic;

namespace FFMQLib
{
	public partial class Patcher
	{
		private static Dictionary<int, PatchInstruction> bank0dinstructions = new()
		{
			{ 0x3DB4, new PatchInstruction() { Action = PatchAction.IncreaseRange, Value = 0x01, Step = 3, Qty = 9 } },
		};
	}
}
