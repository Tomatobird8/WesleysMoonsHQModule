using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using WesleyMoonScripts;

// BALANCING PATCHES

namespace WesleysMoonsHQModule.Patches;

/// <summary>
/// Balancing patches for v81
/// </summary>
[HarmonyPatch]
internal class BalancePatches_v2
{
    internal static bool Patched = false;

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    internal static void Start_Postfix(StartOfRound __instance)
    {
        if (Patched) return; // Only patch once.

        if (WesleyScripts.LockMoons.Value) // Only apply when moons are locked - HQ Mode
        {
            LungProp[] lungProps = Resources.FindObjectsOfTypeAll<LungProp>();

            foreach (LungProp lp in lungProps)
            {
                if (lp.name == "CosmicLungApparatus" || lp.name == "CosmicLungApparatusDisabled")
                {
                    lp.scrapValue = 214; // Change Cosmocos outside apparatus value
                }
            }

            List<Item> items = StartOfRound.Instance.allItemsList.itemsList;

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                // -- HYX -- item value rebalance
                switch (item.itemName)
                {
                    case "The seed of assimilation":
                        item.maxValue = 580;
                        item.minValue = 500;
                        break;
                }
            }

            TerminalKeyword[] keywords = Resources.FindObjectsOfTypeAll<TerminalKeyword>();

            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].name == "Route")
                {
                    CompatibleNoun[] nouns = keywords[i].compatibleNouns;
                    for (int j = 0; j < nouns.Length; j++)
                    {
                        switch (nouns[j].result.name)
                        {
                            case "HyxRoute":
                                nouns[j].result.itemCost = 3500;
                                break;
                            case "CarnateRoute":
                                nouns[j].result.itemCost = 2200;
                                break;
                        }
                    }
                    break;
                }
            }

        }

        foreach (SelectableLevel s in __instance.levels)
        {
            s.DaySpeedMultiplier = Mathf.Max(s.DaySpeedMultiplier, 1f); // Make default daytime speed the minimum

            // -- COSMOCOS --
            if (s.name == "CosmocosLevel")
            {
                s.DaySpeedMultiplier = 0.959f; // Re-ajust daytime speed - Landing cutscene
            }

            // -- EMPRA --
            else if (s.name == "EmpraLevel")
            {
                s.DaySpeedMultiplier = 0.875f; // Re-adjust daytime speed - Cart ride

                if (!WesleyScripts.LockMoons.Value) continue; // patch only in HQ Mode

                s.maxEnemyPowerCount = 16;
                EnemyType? butlerType = null;
                foreach (var eType in Resources.FindObjectsOfTypeAll<ButlerEnemyAI>())
                {
                    if (eType.name == "ButlerEnemy")
                        butlerType = eType.enemyType;
                }
                if (butlerType != null)
                    s.Enemies.Add(new SpawnableEnemyWithRarity(butlerType, 56));
            }

            // -- HYX --
            else if (s.name == "HyxLevel")
            {
                if (!WesleyScripts.LockMoons.Value) continue; // patch only in HQ Mode

                s.maxScrap = 60;
                s.minScrap = 55;
                s.maxOutsideEnemyPowerCount = 14;
                s.maxEnemyPowerCount = 20;
                s.spawnProbabilityRange = 3.04f;
                s.enemySpawnChanceThroughoutDay = new AnimationCurve([
                    new Keyframe(0.0f, -3.0f, 2f, 2f),
                    new Keyframe(0.2f, -1.0f, 15f, 15f),
                    new Keyframe(0.4f, 6.0f, 0f, 0f),
                    new Keyframe(0.6f, 4.0f, 4f, 4f),
                    new Keyframe(0.8f, 9.0f, 15f, 15f),
                    new Keyframe(1.0f, 50.0f, 120f, 120f)
                    ]);
                s.outsideEnemySpawnChanceThroughDay = AddToKeyFramesOverTime(s.outsideEnemySpawnChanceThroughDay, 2f);
                s.indoorMapHazards[0].numberToSpawn = ZeroOrValueCurve(5, 0.2f); // Landmines
                s.indoorMapHazards[1].numberToSpawn = ZeroOrValueCurve(1, 0.5f); // Turrets
                s.indoorMapHazards[2].numberToSpawn = ZeroOrValueCurve(1, 0.5f); // Spiketraps
                s.randomWeathers[1].weatherVariable = 4; // Eclipsed
            }

            else if (s.name == "CarnateLevel")
            {
                if (!WesleyScripts.LockMoons.Value) continue; // patch only in HQ mode

                s.maxEnemyPowerCount = 16;
                s.Enemies[7].rarity = 9; // Lootbug
                s.enemySpawnChanceThroughoutDay.keys[1].time -= 0.1f;
                s.enemySpawnChanceThroughoutDay.keys[1].value += 1f;
                s.enemySpawnChanceThroughoutDay.keys[2].value += 3f;
                s.outsideEnemySpawnChanceThroughDay.keys[1].time -= 0.3f;
                s.outsideEnemySpawnChanceThroughDay.keys[2].value = 15f;
            }

            else if (s.name == "SelectorLevel")
            {
                if (!WesleyScripts.LockMoons.Value) continue; // patch only in HQ mode

                s.maxScrap = 35;
                s.DaytimeEnemies[2].rarity = 1; // Bees
                s.DaytimeEnemies[1].rarity = 20; // Locust
            }

            else if (s.name == "OldredLevel")
            {
                if (!WesleyScripts.LockMoons.Value) continue; // patch only in HQ mode

                s.maxEnemyPowerCount = 20;
                s.maxOutsideEnemyPowerCount = 8;
                s.Enemies[10].rarity = 3; // Oilfish
                EnemyType? nutcrackerType = null;
                foreach (var eType in Resources.FindObjectsOfTypeAll<NutcrackerEnemyAI>())
                {
                    if (eType.name == "NutcrackerEnemy")
                        nutcrackerType = eType.enemyType;
                }
                if (nutcrackerType != null)
                    s.Enemies.Add(new SpawnableEnemyWithRarity(nutcrackerType, 19));
                Keyframe[] v73indoor = new Keyframe[]
                {
                    new Keyframe(0f, -3f, 19.557f, 19.557f, 0f, 0.123f),
                    new Keyframe(0.3161f, 5.4704f, 24.4795f, 24.4795f, 0.3961f, 0.3547f),
                    new Keyframe(0.7594f, 4.973f, 2.6163f, 2.6163f, 0.2901f, 0.5361f),
                    new Keyframe(1f, 15f, 35.604f, 35.604f, 0.0491f, 0f),
                };
                s.enemySpawnChanceThroughoutDay = new AnimationCurve(v73indoor);
                Keyframe[] v73outdoor = new Keyframe[]
                {
                    new Keyframe(0f, -3f, -0.1126f, -0.1126f, 0f, 1f),
                    new Keyframe(0.1717f, 0.5331f, -8.0549f, -8.0549f, 0.3272f, 0.4063f),
                    new Keyframe(0.7145f, 3.2309f, 64.2007f, 64.2007f, 0.1701f, 0.0855f),
                    new Keyframe(1f, 5.3594f, -1.112f, -1.112f, 0.548f, 0.4832f),
                };
                s.outsideEnemySpawnChanceThroughDay = new AnimationCurve(v73outdoor);
            }
        }

        Patched = true;
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

    internal static AnimationCurve ZeroOrValueCurve(float value, float normalChance)
    {
        Keyframe[] keyFrames = new Keyframe[]
        {
            new Keyframe(0f, 0f, 0f, float.PositiveInfinity),
            new Keyframe(1 - normalChance, value, 0f, float.PositiveInfinity),
            new Keyframe(1f, value, 0f, float.PositiveInfinity),
        };

        return new AnimationCurve(keyFrames);
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyAfter("mrov.WeatherRegistry")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    internal static void SpawnScrap_Pre(RoundManager __instance)
    {
        if (!WesleyScripts.LockMoons.Value || __instance.currentLevel.name != "CarnateLevel")
            return;

        RoundManager.Instance.scrapValueMultiplier *= 1.88f;
    }
}
