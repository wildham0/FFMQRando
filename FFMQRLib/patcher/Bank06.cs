using System.Collections.Generic;

namespace FFMQLib
{
	public partial class Patcher
	{
		private static Dictionary<int, PatchInstruction> bank06instructions = new()
		{
			{ 0x3C7B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x27 } },
		};
	}
}
