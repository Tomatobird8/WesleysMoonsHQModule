// from LethalLevelLoader 1.7.0

using HarmonyLib;
using System.Collections;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch(typeof(SoundManager))]
    internal class SoundManagerPatcher
    {
        [HarmonyPatch(nameof(SoundManager.ResetValues))]
        [HarmonyPostfix]
        internal static void ResetValues_Postfix(SoundManager __instance)
        {
            WesleysMoonsHQModule.Logger.LogInfo("Attempting to fix chorus effect bug in SoundManager.ResetsValues.");
            __instance.StartCoroutine(FixChorusEffect(__instance));
        }

        private static IEnumerator FixChorusEffect(SoundManager soundManager)
        {
            yield return null;
            soundManager.SetDiageticMixerSnapshot(4, 1000.0f);
            yield return null;
            soundManager.SetDiageticMixerSnapshot(0, 1.0f);
        }
    }
}
