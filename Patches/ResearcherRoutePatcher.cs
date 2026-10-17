using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using WesleysInteriorTools;
using WesleysInteriorTools.Features;

namespace WesleysMoonsHQModule.Patches;

[HarmonyPatch(typeof(TriangulationSpot))]
internal class ResearcherRoutePatcher
{
    [HarmonyPatch(nameof(TriangulationSpot.TrySetTriangulatableMoons))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> TrySetTriangulatableMoons_Transpiler(IEnumerable<CodeInstruction> codes)
    {
        CodeMatcher matcher = new(codes);

        CodeInstruction[] initArray = new CodeInstruction[]
        {
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Ldc_I4_0), // Vindict
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Ldc_I4_8), // Dune
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_2),
            new CodeInstruction(OpCodes.Ldc_I4, 10), // Heap
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_3),
            new CodeInstruction(OpCodes.Ldc_I4_3), // Glimmer
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_4),
            new CodeInstruction(OpCodes.Ldc_I4_1), // Centus
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_5),
            new CodeInstruction(OpCodes.Ldc_I4_7), // Spine
            new CodeInstruction(OpCodes.Stelem_I4),

            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Ldc_I4_6),
            new CodeInstruction(OpCodes.Ldc_I4, 9), // Xenphir
            new CodeInstruction(OpCodes.Stelem_I4),
        };

        int end = matcher
            .End()
            .MatchBack(false,
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(
                        typeof(WesleysInteriorToolsPlugin),
                        nameof(WesleysInteriorToolsPlugin.CurrentSave)
                    )
                ),
                new CodeMatch(OpCodes.Ldloc_1)
            )
            .Advance(-1)
            .Pos;

        int start = matcher.
            Start().
            MatchForward(false, new CodeMatch(OpCodes.Newobj))
            .Pos;

        return matcher
            .Start()
            .Advance(start)
            .RemoveInstructionsInRange(start, end)
            .Insert(initArray)
            .InstructionEnumeration();
    }
}
