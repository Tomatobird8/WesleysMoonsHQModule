using HarmonyLib;
using UnityEngine;
using WesleysWeatherStuff.Stuff;

namespace WesleysMoonsHQModule.Patches;
/// <summary>
/// Fix for Wesley's Weathers objects in v73 not being destroyed when lobby is closed
/// </summary>
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
