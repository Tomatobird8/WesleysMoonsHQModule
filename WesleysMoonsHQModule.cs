using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using JLL.Components;
using JLL.Components.Filters;
using System;
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

        // Allowed items to spawn in giftshop (to avoid spawning scrap)
        internal static List<string> allowedItemNames = ["Bury the child videotape", "Teach the disloyal videotape"];

        // Skip these scenes
        internal static List<string> scenesToSkip = ["MainMenu", "InitScene", "InitSceneLaunchOptions"];

        // Loaded plugins
        internal static Dictionary<string, PluginInfo> pluginInfos = [];

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

            if (Chainloader.PluginInfos.TryGetValue(OPI.LLL_GUID, out PluginInfo pluginInfo))
            {
                if (pluginInfo.Metadata.Version >= new Version(PackDefinition.v73Mods[OPI.LLL_GUID]))
                {
                    Harmony.PatchAll(typeof(LLLConfigLoaderPatcher_v2));
                    Harmony.PatchAll(typeof(WesleysWeatherStuffPatcher));
                }
                else
                {
                    Harmony.PatchAll(typeof(LLLConfigLoaderPatcher_v1));
                }
            }
            Logger.LogDebug("Finished patching!");
        }

        internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scenesToSkip.Contains(scene.name))
                return;
            RemoveAprilFools(scene);
            if (scene.name == "MusemaScene") 
            {
                EditMusemaScene(scene);
            }
            if (scene.name == "Asteroid14Scene" && WesleyScripts.LockMoons.Value)
            {
                EditHyveScene(scene);
            }
            if (scene.name == "CalistScene")
            {
                EditCalistScene(scene);
            }
        }

        internal static GameObject GetRootGameObject(Scene scene, string name)
        {
            return scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
        }

        // GALETRY GIFT SHOP CHANGES
        internal static void EditMusemaScene(Scene scene)
        {
            Logger.LogInfo("Editing Galetry Scene.");

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
        // Replace big hive spawn table with a null enemy
        internal static void EditHyveScene(Scene scene)
        {
            Logger.LogInfo("Editing Galetry Scene.");

            GameObject environment = GetRootGameObject(scene, "Environment");

            EnemySpawner.WeightedEnemyRefrence nullEnemy = new() { rarity = 99 };

            foreach (EnemySpawner spawner in environment.GetComponentsInChildren<EnemySpawner>())
            {
                if (spawner.name != "Spawner") continue;

                spawner.randomPool = [nullEnemy];
            }
        }

        // CALIST ACCESSIBILITY CHANGES
        // parent sky objects to non-animated object
        internal static void EditCalistScene(Scene scene)
        {
            Logger.LogInfo("Editing Calist Scene.");

            Transform environment = GetRootGameObject(scene, "Environment").transform;
            Transform sun = environment.Find("Lighting/BrightDay/Sun");
            Transform[] skyObjects = 
                [
                sun.transform.Find("SunAnimContainer/GameObject"), 
                sun.transform.Find("SunAnimContainer/AsteroidsPAck"), 
                sun.transform.Find("SunAnimContainer/Sphere")
                ];
            foreach (Transform t in skyObjects)
            {
                t.SetParent(sun.transform);
            }
        }

        // REMOVE APRIL FOOLS
        internal static void RemoveAprilFools(Scene scene)
        {
            DateFilter[] dateFilters = FindObjectsOfType<DateFilter>();
            foreach (DateFilter d in dateFilters) 
            {
                if (d.gameObject.name != "randomAprilEffect")
                {
                    return;
                }
                Logger.LogInfo("Removing randomAprilEffect...");
                Destroy(d.gameObject);
            }
        }
    }
}
