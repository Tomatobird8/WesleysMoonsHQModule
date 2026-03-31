using HarmonyLib;
using System.Collections.Generic;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch]
    internal class LLLConfigLoaderPatcher_v1
    {
        static readonly List<string> vanillaLevels = ["Adamance", "Offense", "Assurance", "Experimentation", "Liquidation", "Embrion", "Vow", "March", "Artifice", "Dine", "Titan", "Rend"];

        [HarmonyPatch("LethalLevelLoader.Tools.ExtendedDungeonConfig, LethalLevelLoader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "BindConfigs")]
        [HarmonyPrefix]
        public static void PatchDungeon(object[] __args)
        {
            WesleysMoonsHQModule.Logger.LogDebug("PatchDungeon running");
            object extendedDungeonFlow = __args[0];
            var type = extendedDungeonFlow.GetType();
            WesleysMoonsHQModule.Logger.LogDebug(type.FullName);
            var genConfigs = type.GetProperty("GenerateAutomaticConfigurationOptions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            WesleysMoonsHQModule.Logger.LogDebug(genConfigs.Name);
            genConfigs.SetValue(extendedDungeonFlow, false);
        }

        [HarmonyPatch("LethalLevelLoader.Tools.ExtendedLevelConfig, LethalLevelLoader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "BindConfigs")]
        [HarmonyPrefix]
        public static void PatchMoon(object[] __args)
        {
            object extendedMoon = __args[0];
            var type = extendedMoon.GetType();
            var genConfigs = type.GetProperty("GenerateAutomaticConfigurationOptions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            genConfigs.SetValue(extendedMoon, false);

            var NumberlessPlanetName = type.GetProperty("NumberlessPlanetName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (vanillaLevels.Contains((string)NumberlessPlanetName.GetValue(extendedMoon)))
            {
                var IsRouteLocked = type.GetProperty("IsRouteLocked", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var IsRouteHidden = type.GetProperty("IsRouteHidden", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                IsRouteLocked.SetValue(extendedMoon, true);
                IsRouteHidden.SetValue(extendedMoon, true);
            }
        }
    }
}
