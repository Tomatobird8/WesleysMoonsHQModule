using HarmonyLib;
using UnityEngine;
using WesleyMoonScripts;

// BALANCING PATCHES

namespace WesleysMoonsHQModule.Patches;

/// <summary>
/// Balancing patches for v81
/// </summary>
[HarmonyPatch(typeof(StartOfRound))]
internal class BalancePatches_v2
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

            // -- COSMOCOS --
            if (s.name == "CosmocosLevel")
            {
                s.DaySpeedMultiplier = 0.959f; // Re-ajust daytime speed - Landing cutscene
            }

            // -- EMPRA --
            else if (s.name == "EmpraLevel")
            {
                s.DaySpeedMultiplier = 0.875f; // Re-adjust daytime speed - Cart ride
            }

            // -- HYX --
            else if (s.name == "HyxLevel")
            {
                s.maxOutsideEnemyPowerCount = 14;
                s.maxEnemyPowerCount = 20;
                s.spawnProbabilityRange = 3.04f;
                s.enemySpawnChanceThroughoutDay = new AnimationCurve([
                    new Keyframe(0.0f, -3.0f, 2f, 2f),
                    new Keyframe(0.2f, -1.0f, 15f, 15f),
                    new Keyframe(0.4f, 6.0f, 0f, 0f),
                    new Keyframe(0.6f, 4.0f, 4f, 4f),
                    new Keyframe(0.8f, 9.0f, 15f, 15f),
                    new Keyframe(1.0f, 50.0f, 70f, 70f)
                    ]);
                s.outsideEnemySpawnChanceThroughDay = AddToKeyFramesOverTime(s.outsideEnemySpawnChanceThroughDay, 2f);
                s.indoorMapHazards[0].numberToSpawn = AddToKeyFramesOverTime(s.indoorMapHazards[0].numberToSpawn, 20f); // landmines
                s.indoorMapHazards[1].numberToSpawn = AddToKeyFramesOverTime(s.indoorMapHazards[1].numberToSpawn, 12f); // turrets
                s.indoorMapHazards[2].numberToSpawn = AddToKeyFramesOverTime(s.indoorMapHazards[2].numberToSpawn, 6f); // spiketraps
                s.randomWeathers[1].weatherVariable = 4; // Eclipsed
            }
        }
    }

    internal static AnimationCurve AddToKeyFramesOverTime(AnimationCurve curve, float value)
    {
        Keyframe[] keyFrames = curve.keys;
        for (int i = 0; i < keyFrames.Length; i++)
        {
            if (keyFrames[i].value > 0)
            {
                keyFrames[i].value += keyFrames[i].time * value;
            }
        }
        return new AnimationCurve(keyFrames);
    }
}
