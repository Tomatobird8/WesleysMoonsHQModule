using HarmonyLib;
using JLL.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using WesleyMoonScripts;

// BALANCING PATCHES

namespace WesleysMoonsHQModule.Patches;

/// <summary>
/// Balancing patches pre v81
/// </summary>
[HarmonyPatch(typeof(StartOfRound))]
internal class BalancePatches_v1
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    internal static void Start_Postfix(StartOfRound __instance)
    {
        if (WesleyScripts.LockMoons.Value) // Only apply when moons are locked
        {
            LungProp[] lungProps = Resources.FindObjectsOfTypeAll<LungProp>();

            foreach (LungProp lp in lungProps)
            {
                if (lp.name == "CosmicLungApparatus" || lp.name == "CosmicLungApparatusDisabled")
                {
                    lp.scrapValue = 214; // Change Cosmocos outside apparatus value
                }
            }
        }

        foreach (SelectableLevel s in __instance.levels)
        {
            s.DaySpeedMultiplier = Mathf.Max(s.DaySpeedMultiplier, 1f); // Set day speed multiplier to default on all moons

            if (s.name == "CosmocosLevel") // TODO: confirm scene name
            {
                s.DaySpeedMultiplier = 0.959f; // Re-ajust daytime speed - Landing cutscene

                if (WesleyScripts.LockMoons.Value)
                {
                    s.maxScrap = 55; // Change max scrap on Cosmocos, only apply when moons are locked
                }
            }

            if (s.name == "EmpraLevel") // TODO: confirm scene name
            {
                s.DaySpeedMultiplier = 0.875f; // Re-adjust daytime speed - Cart ride
            }
        }
    }

    // HYVE BALANCE CHANGES
    // Replace big hive spawn table with a null enemy
    internal static void EditHyveScene(Scene scene)
    {
        WesleysMoonsHQModule.Logger.LogInfo("Editing Hyve Scene.");

        GameObject environment = WesleysMoonsHQModule.GetRootGameObject(scene, "Environment");

        EnemySpawner.WeightedEnemyRefrence nullEnemy = new() { rarity = 99 };

        foreach (EnemySpawner spawner in environment.GetComponentsInChildren<EnemySpawner>())
        {
            if (spawner.name != "Spawner") continue;

            spawner.randomPool = [nullEnemy];
        }
    }
}
