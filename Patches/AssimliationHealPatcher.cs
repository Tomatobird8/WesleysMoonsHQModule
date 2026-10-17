using HarmonyLib;
using WesleysInteriorTools.Items;

namespace WesleysMoonsHQModule.Patches;

[HarmonyPatch(typeof(StimItem))]
internal class AssimilationHealPatcher
{
    [HarmonyPatch(nameof(StimItem.UseStim))]
    [HarmonyPostfix]
    internal static void UseStim_Post(StimItem __instance)
    {
        AssimilatedGrowth superCavader = UnityEngine.Object.FindAnyObjectByType<AssimilatedGrowth>();
        int playerClientId = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
        if (!__instance.HealthStim ||
            superCavader == null ||
            !superCavader.playerInfections[playerClientId].infected ||
            superCavader.playerInfections[playerClientId].infectionMeter >= 1f
        )
            return;

        superCavader.HealInfection(playerClientId, 0.5f);
    }
}
