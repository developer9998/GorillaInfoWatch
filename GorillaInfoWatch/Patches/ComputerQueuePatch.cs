using GorillaNetworking;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GorillaInfoWatch.Patches
{
    [HarmonyPatch(typeof(GorillaComputer), nameof(GorillaComputer.ProcessQueueState))]
    internal class ComputerQueuePatch
    {
        public static List<string> baseGameQueueNames = [];

        [HarmonyPriority(Priority.Low)]
        [HarmonyWrapSafe]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (baseGameQueueNames.Count > 0) return instructions;

            CodeInstruction[] codes = [.. instructions];

            for(int i = 0; i < codes.Length; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i + 2].opcode == OpCodes.Call)
                {
                    // Too lazy to check for specifics surrounding what is being called haha
                    baseGameQueueNames.Add(codes[i].operand.ToString());
                }
            }

            return codes;
        }
    }
}
