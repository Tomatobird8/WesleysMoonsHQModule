using HarmonyLib;
using Unity.Netcode;
using WesleyMoonScripts;
using WesleysInteriorTools.Features;

namespace WesleysMoonsHQModule.Patches;

/// <summary>
/// Open FabricationDoor for SMHQ.
/// </summary>
[HarmonyPatch(typeof(FabricationDoor))]
internal class FabricationDoorOpener
{
    [HarmonyPatch(nameof(FabricationDoor.WaitForPlayersCoroutine), methodType: MethodType.Enumerator)]
    [HarmonyPostfix]
    internal static void OpenDoor(FabricationDoor __instance)
    {
        if (!WesleyScripts.LockMoons.Value && NetworkManager.Singleton.IsServer)
        {
            __instance.OpenDoorClientRpc();
        }
    }
}
