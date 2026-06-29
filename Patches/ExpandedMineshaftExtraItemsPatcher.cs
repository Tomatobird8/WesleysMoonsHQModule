using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class ExpandedMineshaftExtraItemsPatcher
    {
        public static int GetLevel3ButCoolID()
        {
            if (RoundManager.Instance == null) return -1;
            for (int i = 0; i < RoundManager.Instance.dungeonFlowTypes.Length; i++)
            {
                if (RoundManager.Instance.dungeonFlowTypes[i].dungeonFlow.name == "Level3ButCoolFlow")
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsMineshaftType(int dungeonType)
        {
            return dungeonType == 4 || dungeonType == GetLevel3ButCoolID();
        }

        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SpawnScrapInLevel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new(instructions);

            var targetPattern = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(RoundManager), nameof(RoundManager.currentDungeonType))),
                new CodeMatch(OpCodes.Ldc_I4_4)
            };

            matcher.MatchForward(false, targetPattern)
                .ThrowIfNotMatch("Failed to find targetPattern for patching ExpandedMineshaft bonus")
                .Advance(1)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMineshaftExtraItemsPatcher), nameof(IsMineshaftType))))
                .SetOpcodeAndAdvance(OpCodes.Brfalse_S);

            return matcher.InstructionEnumeration();
        }
    }
}
