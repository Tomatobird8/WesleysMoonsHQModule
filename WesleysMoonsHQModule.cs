using BepInEx;
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

namespace WesleysMoonsHQModule
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(OtherPluginInfos.JLL_WMS_GUID)]
    public class WesleysMoonsHQModule : BaseUnityPlugin
    {
        public static WesleysMoonsHQModule Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static List<string> allowedItemNames = ["Bury the child videotape", "Teach the disloyal videotape"];

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            SceneManager.sceneLoaded += OnSceneLoad;

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(StartOfRoundPatcher)); // BALANCING PATCHES

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
        internal static void EditHyveScene(Scene scene)
        {
            GameObject environment = GetRootGameObject(scene, "Environment");

            foreach (EnemySpawner spawner in environment.GetComponentsInChildren<EnemySpawner>())
            {
                if (spawner.name != "Spawner") continue;

                foreach (EnemySpawner.WeightedEnemyRefrence reference in spawner.randomPool)
                {
                    if (reference.enemyType.name == "RedLocustBees")
                    {
                        reference.rarity = 50; // Decrease beehive spawn rarity from large hives
                    }
                }
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
