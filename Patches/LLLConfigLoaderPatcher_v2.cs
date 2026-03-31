using LethalLevelLoader.Tools;
using HarmonyLib;
using System.Collections.Generic;
using LethalLevelLoader;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch]
    internal class LLLConfigLoaderPatcher_v2
    {
        static readonly List<string> vanillaLevels = ["Adamance","Offense","Assurance","Experimentation","Liquidation","Embrion","Vow","March","Artifice","Dine","Titan","Rend"];

        [HarmonyPatch(typeof(ExtendedDungeonConfig), "BindConfigs")]
        [HarmonyPrefix]
        public static void PatchDungeon(ref ExtendedDungeonFlow extendedDungeonFlow)
        {
            WesleysMoonsHQModule.Logger.LogDebug("Running LLLConfigLoaderPatcher.PatchDungeon()");
            extendedDungeonFlow.GenerateAutomaticConfigurationOptions = false;
        }

        [HarmonyPatch(typeof(ExtendedLevelConfig), "BindConfigs")]
        [HarmonyPrefix]
        public static void PatchMoon(ref ExtendedLevel extendedLevel)
        {
            WesleysMoonsHQModule.Logger.LogDebug("Running LLLConfigLoaderPatcher.PatchMoon()");
            extendedLevel.GenerateAutomaticConfigurationOptions = false;
            if (vanillaLevels.Contains(extendedLevel.NumberlessPlanetName))
            {
                extendedLevel.IsRouteLocked = true;
                extendedLevel.IsRouteHidden = true;
            }
        }
    }
}
