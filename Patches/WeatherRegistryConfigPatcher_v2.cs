using HarmonyLib;
using TMPro;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Modules;
using WeatherRegistry.Enums;

namespace WesleysMoonsHQModule.Patches;
/// <summary>
/// WeatherRegistry config lock for v81
/// </summary>
[HarmonyPatch]
public class WeatherRegistryConfigPatcher_v2
{
    private static TextMeshProUGUI? infoDisplay;

    private static bool VailidityCheckFailed;

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
    [HarmonyPostfix]
    public static void Awake_Postfix()
    {
        var algorithmValue = WeatherCalculation.WeatherAlgorithms[WeatherAlgorithm.Hybrid];
        AccessTools.PropertySetter(typeof(WeatherCalculation), nameof(WeatherCalculation.WeatherSelectionAlgorithm)).Invoke(null, [algorithmValue]);
        ConfigManager.WeatherAlgorithm.Value = WeatherAlgorithm.Hybrid;
        ConfigManager.FirstDayClear.Value = true;
        ConfigManager.WeatherAlgorithm.ConfigFile.Save();
        ConfigManager.FirstDayClear.ConfigFile.Save();
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix() {
        foreach (Weather weather in WeatherManager.RegisteredWeathers)
        {
            if (weather.Config.ScrapValueMultiplier.ConfigEntry.Value != (float)weather.Config.ScrapValueMultiplier.ConfigEntry.DefaultValue || weather.Config.ScrapAmountMultiplier.ConfigEntry.Value != (float)weather.Config.ScrapAmountMultiplier.ConfigEntry.DefaultValue || weather.Config.DefaultWeight.ConfigEntry.Value != (int)weather.Config.DefaultWeight.ConfigEntry.DefaultValue)
            {
                WesleysMoonsHQModule.Logger.LogWarning($"Invalid configuration in WeatherRegistry Weather definition: {weather.Name}");
                VailidityCheckFailed = true;
            }
        }
    }

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
    [HarmonyPostfix]
    private static void HUDManager_Start_Postfix()
    {
        if (!VailidityCheckFailed)
        {
            return;
        }
        if (infoDisplay == null)
        {
            WesleysMoonsHQModule.Logger.LogInfo("infoDisplay is null. Creating a new infodisplay.");
            GameObject infoDisplayObject = new("WesleysMoonsHQModule_infoDisplay");
            infoDisplayObject.transform.parent = HUDManager.Instance.weightCounter.transform.parent;
            TextMeshProUGUI weightCounter = HUDManager.Instance.weightCounter;
            infoDisplay = infoDisplayObject.AddComponent<TextMeshProUGUI>();
            infoDisplay.textStyle = weightCounter.textStyle;
            infoDisplay.tag = weightCounter.tag;
            infoDisplay.alignment = weightCounter.alignment;
            infoDisplay.color = weightCounter.color;
            infoDisplay.font = weightCounter.font;
            infoDisplay.fontSize = weightCounter.fontSize;
            infoDisplay.fontStyle = weightCounter.fontStyle;
            infoDisplay.fontWeight = weightCounter.fontWeight;
            infoDisplay.enableAutoSizing = weightCounter.enableAutoSizing;
            infoDisplay.fontSizeMin = weightCounter.fontSizeMin;
            infoDisplay.fontSizeMax = weightCounter.fontSizeMax;
            infoDisplay.isOverlay = weightCounter.isOverlay;
            infoDisplay.transform.position = weightCounter.transform.position;
            infoDisplay.text = "text";
            RectTransform infoDisplayTransform = infoDisplay.GetComponent<RectTransform>();
            if (infoDisplayTransform == null)
            {
                WesleysMoonsHQModule.Logger.LogError("Transform not found");
                return;
            }
            infoDisplayTransform.offsetMin = weightCounter.GetComponent<RectTransform>().offsetMin;
            infoDisplayTransform.offsetMax = weightCounter.GetComponent<RectTransform>().offsetMax;
            infoDisplayTransform.anchoredPosition = new Vector2(67, -32);
            infoDisplayTransform.localScale = Vector3.one;
            infoDisplayTransform.localRotation = Quaternion.identity;
        }

        if (infoDisplay == null)
        {
            return;
        }
        infoDisplay.text = "Invalid Pack";

    }
}
