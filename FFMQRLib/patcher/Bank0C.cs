using System.Collections.Generic;

namespace FFMQLib
{
	public partial class Patcher
	{
		private static Dictionary<int, PatchInstruction> bank0cinstructions = new()
		{
			{ 0x00C7, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x00F8, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x143B, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x1464, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x149E, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x20 } },
			{ 0x2038, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x2063, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
			{ 0x207D, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x05 } },
		};
	}
}
