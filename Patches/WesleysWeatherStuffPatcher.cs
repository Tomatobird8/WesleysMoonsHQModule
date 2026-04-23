using HarmonyLib;
using UnityEngine;
using WesleysWeatherStuff.Stuff;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch(typeof(WeatherObjectContainer))]
    internal class WesleysWeatherStuffPatcher
    {
        [HarmonyPatch("DestroyObjects")]
        [HarmonyPrefix]
        private static void DestroyObjectsPatch(WeatherObjectContainer __instance)
        {
            __instance.weatherObjects = [.. Object.FindObjectsOfType<WeatherObject>()];
        }
    }
}
