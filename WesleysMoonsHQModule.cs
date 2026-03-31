using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using JLL.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WesleyMoonScripts;
using WesleyMoonScripts.Components;
using WesleysMoonsHQModule.Patches;
using OPI = WesleysMoonsHQModule.OtherPluginInfos;

namespace WesleysMoonsHQModule
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(OPI.JLL_WMS_GUID)]
    [BepInDependency(OPI.LLL_GUID)]
    public class WesleysMoonsHQModule : BaseUnityPlugin
    {
        public static WesleysMoonsHQModule Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static List<string> allowedItemNames = ["Bury the child videotape", "Teach the disloyal videotape"];

        internal static Dictionary<string, PluginInfo> pluginInfos = [];

        internal static Dictionary<string, string> commonRequiredMods = new Dictionary<string, string>
        {
            {OPI.LOADSTONE_GUID, "0.1.23" }
        };

        internal static Dictionary<string, string> commonOptionalMods = new Dictionary<string, string>
        {
            {OPI.FREEMOONS_GUID, "1.0.2" }, // 1.2.0 on TS
            {OPI.CULLFACTORY_GUID, "2.0.4" }
        };

        internal static Dictionary<string, string> v69Mods = new Dictionary<string, string>
        {
            {OPI.LLL_GUID, "1.4.11" },
            {OPI.PATHFINDINGLAGFIX_GUID, "2.2.4" },
            {OPI.PATHFINDINGLIB_GUID, "2.3.2" },
            {OPI.STARLANCERAIFIX_GUID, "3.9.1" },
            {OPI.LETHALLIB_GUID, "1.0.1" }
        };

        internal static Dictionary<string, string> v72Mods = new Dictionary<string, string>
        {
            {OPI.LLL_GUID, "1.4.11" },
            {OPI.PATHFINDINGLAGFIX_GUID, "2.2.4" },
            {OPI.PATHFINDINGLIB_GUID, "2.3.2" },
            {OPI.STARLANCERAIFIX_GUID, "3.11.1" },
            {OPI.LETHALLIB_GUID, "1.1.1" }
        };

        internal static Dictionary<string, string> v73Mods = new Dictionary<string, string>
        {
            {OPI.LLL_GUID, "1.6.8" },
            {OPI.WEATHERREGISTRY_GUID, "0.7.5"},
            {OPI.MROVLIB_GUID, "0.4.2" },
            {OPI.PATHFINDINGLAGFIX_GUID, "2.2.5" },
            {OPI.PATHFINDINGLIB_GUID, "2.4.1" },
            {OPI.STARLANCERAIFIX_GUID, "3.11.1" },
            {OPI.LETHALLIB_GUID, "1.1.1" }
        };

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            pluginInfos = Chainloader.PluginInfos;

            Patch();

            SceneManager.sceneLoaded += OnSceneLoad;

            Logger.LogInfo($"Pack launched in {(WesleyScripts.LockMoons.Value ? "High Quota" : "Single Moon High Quota")} mode!");

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(StartOfRoundPatcher)); // BALANCING PATCHES

            Harmony.PatchAll(typeof(MenuManagerPatcher));

            Logger.LogDebug("Finished patching!");
        }

        internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MusemaScene") 
            {
                EditMusemaScene(scene);
            }
            if (scene.name == "Asteroid14Scene" && WesleyScripts.LockMoons.Value)
            {
                EditHyveScene(scene);
            }
        }

        internal static GameObject GetRootGameObject(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
        }

        // GALETRY GIFT SHOP CHANGES
        internal static void EditMusemaScene(Scene scene)
        {
            GameObject environment = GetRootGameObject(scene, "Environment");

            foreach (ItemShop shop in environment.transform.Find("Giftshop/Itemshop").GetComponentsInChildren<ItemShop>())
            {
                if (shop == null) continue;
                // Only affect the shop script with valuable scrap in it
                if (shop.Catalogue[0].ItemName == "Mortar hammer")
                {
                    // Remove all but included items in allowedItemNames from shop catalogue.
                    shop.Catalogue = [.. shop.Catalogue.Where(item => allowedItemNames.Contains(item.ItemName))];
                    break;
                }
            }
        }

        // HYVE BALANCE CHANGES
        // beehive chance from large beehive 89.2% -> 30.6%
        internal static void EditHyveScene(Scene scene)
        {
            GameObject environment = GetRootGameObject(scene, "Environment");

            EnemySpawner.WeightedEnemyRefrence nullEnemy = new() { rarity = 99 };

            foreach (EnemySpawner spawner in environment.GetComponentsInChildren<EnemySpawner>())
            {
                if (spawner.name != "Spawner") continue;

                foreach (EnemySpawner.WeightedEnemyRefrence reference in spawner.randomPool)
                {
                    if (reference.enemyType.name == "RedLocustBees")
                    {
                        reference.rarity = 49; // Decrease beehive spawn rarity from large hives
                    }
                }

                spawner.randomPool.Add(nullEnemy); // Add high chance to not spawn an enemy
            }
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
