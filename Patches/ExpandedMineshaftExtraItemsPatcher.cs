using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace WesleysMoonsHQModule.Patches;
/// <summary>
/// Patch to add extra items to mineshaft to match vanilla mineshaft itemcounts in v73
/// </summary>
[HarmonyPatch(typeof(RoundManager))]
internal class ExpandedMineshaftExtraItemsPatcher
{
    public static int GetLevel3ButCoolID()
    {
        if (RoundManager.Instance == null) return -1;

        var flowTypes = RoundManager.Instance.dungeonFlowTypes;

        for (int i = 0; i < flowTypes.Length; i++)
        {
            var item = flowTypes[i];
            var flowObj = AccessTools.Field(item.GetType(), "dungeonFlow").GetValue(item);
            if (flowObj is UnityEngine.Object obj && obj.name == "Level3ButCoolFlow")
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

        CodeMatch[] targetPattern =
        {
            new(OpCodes.Ldfld, AccessTools.Field(typeof(RoundManager), nameof(RoundManager.currentDungeonType))),
            new(OpCodes.Ldc_I4_4)
        };

        matcher.MatchForward(false, targetPattern)
            .ThrowIfNotMatch("Failed to find targetPattern for patching ExpandedMineshaft bonus")
            .Advance(1)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMineshaftExtraItemsPatcher), nameof(IsMineshaftType))))
            .SetOpcodeAndAdvance(OpCodes.Brfalse_S);

        return matcher.InstructionEnumeration();
    }
}
