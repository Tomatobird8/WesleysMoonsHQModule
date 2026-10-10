using HarmonyLib;
using System.Reflection;
using Unity.Netcode;
using WesleyMoonScripts;
using WesleysInteriorTools.Features;

namespace WesleysMoonsHQModule.Patches;

/// <summary>
/// Open FabricationDoor for SMHQ.
/// </summary>
[HarmonyPatch]
internal class FabricationDoorOpener
{
    [HarmonyTargetMethod]
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            AccessTools.Inner(typeof(FabricationDoor), "<WaitForPlayersCoroutine>d__9"),
            "MoveNext" // Targeting compiler generated coroutine stuff
        );
    }
    [HarmonyPostfix]
    internal static void OpenDoor(object __instance, bool __result)
    {
        if (__result) return; // last MoveNext will return falses

        var thisField = AccessTools.Field(__instance.GetType(), "<>4__this");
        FabricationDoor? door = thisField.GetValue(__instance) as FabricationDoor;

        if (door != null && !WesleyScripts.LockMoons.Value && NetworkManager.Singleton.IsServer && door.DoorState == 0)
        {
            WesleysMoonsHQModule.Logger.LogDebug("Opening the door to the crafting machine...");
            door.OpenDoorClientRpc();
        }
    }
}
