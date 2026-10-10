using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using JLL.Components;
using JLL.Components.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WesleyMoonScripts;
using WesleyMoonScripts.Components;
using WesleysMoonsHQModule.Patches;
using OPI = WesleysMoonsHQModule.OtherPluginInfos;

namespace WesleysMoonsHQModule;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(OPI.JLL_WMS_GUID)]
[BepInDependency(OPI.LLL_GUID)]
[BepInDependency(OPI.WEATHERREGISTRY_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(OPI.INTERIORTOOLS_GUID, BepInDependency.DependencyFlags.SoftDependency)]

public class WesleysMoonsHQModule : BaseUnityPlugin
{
    public static WesleysMoonsHQModule Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    internal static List<PackDefinition> packDefinitions =
    [
        new PackDefinition(
            version: Versions.v69,
            requiredMods: new Dictionary<string, Version>{
                {OPI.LLL_GUID, new Version("1.4.11") },
                {OPI.PATHFINDINGLAGFIX_GUID, new Version("2.2.4") },
                {OPI.PATHFINDINGLIB_GUID, new Version("2.3.2") },
                {OPI.STARLANCERAIFIX_GUID, new Version("3.9.0") }, // 3.9.1 on TS
                {OPI.LETHALLIB_GUID, new Version("1.0.1") },
                {OPI.LOADSTONE_GUID, new Version("0.1.23") } },
            optionalMods: new Dictionary<string, Version>{ {OPI.CULLFACTORY_GUID, new Version("2.0.4") } },
            disallowedMods : [],
            patches: [
                typeof(BalancePatches_v1), 
                typeof(SoundManagerPatcher), 
                typeof(LLLConfigLoaderPatcher_v1)
                ]
            ),
        new PackDefinition(
            version: Versions.v72,
            requiredMods: new Dictionary<string, Version>{
                {OPI.LLL_GUID, new Version("1.4.11") },
                {OPI.PATHFINDINGLAGFIX_GUID, new Version("2.2.4") },
                {OPI.PATHFINDINGLIB_GUID, new Version("2.3.2") },
                {OPI.STARLANCERAIFIX_GUID, new Version("3.11.1") },
                {OPI.LETHALLIB_GUID, new Version("1.1.1") },
                {OPI.LOADSTONE_GUID, new Version("0.1.23") } },
            optionalMods: new Dictionary<string, Version>{ {OPI.CULLFACTORY_GUID, new Version("2.0.4") } },
            disallowedMods : [],
            patches: [
                typeof(BalancePatches_v1), 
                typeof(SoundManagerPatcher), 
                typeof(LLLConfigLoaderPatcher_v1)
                ]
            ),
        new PackDefinition(
            version: Versions.v73,
            requiredMods: new Dictionary<string, Version>{
                {OPI.LLL_GUID, new Version("1.6.8") },
                {OPI.WEATHERREGISTRY_GUID, new Version("0.7.5") },
                {OPI.MROVLIB_GUID, new Version("0.4.2") },
                {OPI.PATHFINDINGLAGFIX_GUID, new Version("2.2.5") },
                {OPI.PATHFINDINGLIB_GUID, new Version("2.4.1") },
                {OPI.STARLANCERAIFIX_GUID, new Version("3.11.1") },
                {OPI.LETHALLIB_GUID, new Version("1.1.1") },
                {OPI.LOADSTONE_GUID, new Version("0.1.23") } },
            optionalMods: new Dictionary<string, Version>{ {OPI.CULLFACTORY_GUID, new Version("2.0.4") } },
            disallowedMods : [],
            patches: [
                typeof(BalancePatches_v1), 
                typeof(SoundManagerPatcher), 
                typeof(LLLConfigLoaderPatcher_v2), 
                typeof(WesleysWeatherStuffPatcher), 
                typeof(ExpandedMineshaftExtraItemsPatcher),
                typeof(WeatherRegistryConfigPatcher_v1)
                ]
            ),
        new PackDefinition(
            version: Versions.v81,
            requiredMods: new Dictionary<string, Version>{
                {OPI.LLL_GUID, new Version("1.7.13") },
                {OPI.WEATHERREGISTRY_GUID, new Version("0.8.8") },
                {OPI.MROVLIB_GUID, new Version("0.4.15") },
                {OPI.PATHFINDINGLAGFIX_GUID, new Version("2.4.2") },
                {OPI.PATHFINDINGLIB_GUID, new Version("2.4.1") },
                {OPI.STARLANCERAIFIX_GUID, new Version("3.13.2") },
                {OPI.LETHALLIB_GUID, new Version("1.2.0") } },
            optionalMods: new Dictionary<string, Version>{ {OPI.CULLFACTORY_GUID, new Version("2.0.11") } },
            disallowedMods : [OPI.LOADSTONE_GUID, OPI.WATERASSETRESTORER_GUID, OPI.V73DCFIX_GUID],
            patches: [
                typeof(LLLConfigLoaderPatcher_v2),
                typeof(WeatherRegistryConfigPatcher_v2),
                typeof(FabricationDoorOpener)
                ]
            )
    ];
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

        Harmony.PatchAll(typeof(MenuManagerPatcher));

        if (Chainloader.PluginInfos.TryGetValue(OPI.LLL_GUID, out PluginInfo lllInfo))
        {
            foreach (PackDefinition pack in packDefinitions)
            {
                if (lllInfo.Metadata.Version == pack.RequiredMods[OPI.LLL_GUID])
                {
                    PatchType(pack.Patches);
                    break;
                }
            }
        }
        Logger.LogDebug("Finished patching!");
    }

    internal static void PatchType(Type[] typeArray)
    {
        foreach (Type type in typeArray)
        {
            PatchType(type);
        }
    }

    internal static void PatchType(Type type)
    {
        Logger.LogDebug($"Patching {type.Name}");
        Harmony?.PatchAll(type);
    }

    internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scenesToSkip.Contains(scene.name))
            return;
        RemoveAprilFools(scene);
        if (scene.name == "MusemaScene") 
        {
            EditGiftShop(scene);
            if (!WesleyScripts.LockMoons.Value && GameNetworkManager.Instance?.gameVersionNum >= 81) 
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
    internal static void EditGiftShop(Scene scene)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        Logger.LogInfo("Editing Gift Shops in Musema/Galetry scene.");

        GameObject environment = GetRootGameObject(scene, "Environment");

        foreach (ItemShop shop in environment.transform.Find("Giftshop/Itemshop").GetComponentsInChildren<ItemShop>())
        {
            if (shop == null) continue;
            // Only affect the shop script with valuable scrap in it
            if (shop.Catalogue[0].ItemName == "Mortar hammer")
            {
                // Make scrap not have value
                shop.setScrapValue = false;
                if (!WesleyScripts.LockMoons.Value && GameNetworkManager.Instance?.gameVersionNum >= 81) AddCraftingItemsToGiftShop(shop);
                break;
            }
        }
    }

    internal static void AddCraftingItemsToGiftShop(ItemShop shop)
    {
        List<ItemSpawner.WeightedItemRefrence> newItems = [
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 100,
                ItemName = "Plastic stock",
                ScrapValue = 20,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 200,
                ItemName = "Large plastic stock",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 80,
                ItemName = "Steel ingot",
                ScrapValue = 20,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 160,
                ItemName = "Large steel bars",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Circuit board",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 80,
                ItemName = "Circuit board container",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Copper wiring",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 160,
                ItemName = "Large wire roll",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Lithium container",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 120,
                ItemName = "Large lithium container",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Rubber roll",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 160,
                ItemName = "Rubber chunk",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Cables",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 160,
                ItemName = "Large cable rolls",
                ScrapValue = 70,
            },
            /*new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 40,
                ItemName = "Chemical container",
                ScrapValue = 70,
            },*/
            new ItemSpawner.WeightedItemRefrence()
            {
                Weight = 160,
                ItemName = "Large chemical container",
                ScrapValue = 70,
            }
            ];

        List<ItemSpawner.WeightedItemRefrence> catalogue = [.. shop.Catalogue];
        catalogue.AddRange(newItems);
        shop.Catalogue = [.. catalogue];
    }

    // GALETRY PROGRESSION OBJECT CHANGES
    internal static void EditMusemaScene(Scene scene)
    {
        Logger.LogInfo("Editing progression objects in Musema/Galetry scene.");

        Transform environment = GetRootGameObject(scene, "Environment").transform;

        Transform[] dontDestroy = [
            environment.Find("SideBuilding/HangingLight (6)"), 
            environment.Find("SideBuilding/fireexit"), 
            environment.Find("SideBuilding/CraftingMachine"), 
            environment.Find("SideBuilding/FireExitInteractTrigger")
            ];

        foreach (Transform t in dontDestroy) 
        {
            t.GetComponent<ProggressionObject>().enabled = false;
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

    // HYVE BALANCE CHANGES
    // Replace big hive spawn table with a null enemy
    internal static void EditHyveScene(Scene scene)
    {
        Logger.LogInfo("Editing Hyve Scene.");

        GameObject environment = GetRootGameObject(scene, "Environment");

        EnemySpawner.WeightedEnemyRefrence nullEnemy = new() { rarity = 99 };

        foreach (EnemySpawner spawner in environment.GetComponentsInChildren<EnemySpawner>())
        {
            if (spawner.name != "Spawner") continue;

            spawner.randomPool = [nullEnemy];
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
    internal enum Versions
    {
        v69 = 69,
        v72 = 72,
        v73 = 73,
        v81 = 81
    }
}
