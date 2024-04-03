using System.Collections.Generic;

namespace FFMQLib
{
	public partial class Patcher
	{
		private static Dictionary<int, PatchInstruction> bank0binstructions = new()
		{
			{ 0x006A, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x0074, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0084, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x00CE, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x00D4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x00E5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
		};
	}
}
