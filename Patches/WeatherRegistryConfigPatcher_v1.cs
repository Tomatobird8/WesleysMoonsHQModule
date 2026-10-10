using HarmonyLib;
using TMPro;
using UnityEngine;
using WeatherRegistry;
using System.Collections;
using System;
using System.Reflection;

namespace WesleysMoonsHQModule.Patches;
/// <summary>
/// WeatherRegistry config lock for v73
/// </summary>
[HarmonyPatch]
public class WeatherRegistryConfigPatcher_v1
{
    private static TextMeshProUGUI? infoDisplay;

    private static bool VailidityCheckFailed;

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
    [HarmonyPostfix]
    public static void Awake_Postfix()
    {
        Type weatherCalcType = AccessTools.TypeByName("WeatherRegistry.WeatherCalculation");

        IDictionary algorithmsObj = (IDictionary)AccessTools.Field(weatherCalcType, "WeatherAlgorithms").GetValue(null); // Dictionary<WeatherAlgorithm (enum), WeatherSelectionAlgorithm (property)>

        object? selectedAlgorithm = null;

        foreach (DictionaryEntry entry in algorithmsObj)
        {
            if (Convert.ToInt32(entry.Key) == 2) // 2 - Hybrid algorithm
            {
                selectedAlgorithm = entry.Value; // new HybridWeatherSelection();
                break;
            }
        }
        if (selectedAlgorithm == null)
        {
            WesleysMoonsHQModule.Logger.LogError("Error patching v73 WeatherRegistry.");
            return;
        }

        // this is just ConfigManager.WeatherAlgorithm.Value = WeatherAlgorithm.Hybrid but in reflection because the namespaces for this stuff changed later
        Type settingsType = AccessTools.TypeByName("WeatherRegistry.Settings");
        AccessTools.PropertySetter(settingsType, "WeatherSelectionAlgorithm").Invoke(null, [selectedAlgorithm]);

        Type configManagerType = AccessTools.TypeByName("WeatherRegistry.ConfigManager");
        object weatherAlgorithmConfig = AccessTools.Property(configManagerType, "WeatherAlgorithm").GetValue(null);
        PropertyInfo configValueProp = AccessTools.Property(weatherAlgorithmConfig.GetType(), "Value");
        object hybridEnumValue = Enum.Parse(configValueProp.PropertyType, "Hybrid");
        configValueProp.SetValue(weatherAlgorithmConfig, hybridEnumValue);

        // ConfigManager.WeatherAlgorithm.ConfigFile.Save() as reflection to avoid compiling with the wrong namespace
        PropertyInfo configFileProp = AccessTools.Property(weatherAlgorithmConfig.GetType(), "ConfigFile");
        object configFile = configFileProp.GetValue(weatherAlgorithmConfig);
        MethodInfo saveMethod = AccessTools.Method(configFile.GetType(), "Save");
        saveMethod.Invoke(configFile, null);

        ConfigManager.FirstDayClear.Value = true;
        ConfigManager.FirstDayClear.ConfigFile.Save();
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start))]
    [HarmonyPostfix]
    public static void Start_Postfix() {
        foreach (Weather weather in WeatherManager.RegisteredWeathers)
        {
            if (weather.Config.ScrapValueMultiplier.ConfigEntry.Value != (float)weather.Config.ScrapValueMultiplier.ConfigEntry.DefaultValue || weather.Config.ScrapAmountMultiplier.ConfigEntry.Value != (float)weather.Config.ScrapAmountMultiplier.ConfigEntry.DefaultValue || weather.Config.DefaultWeight.ConfigEntry.Value != (int)weather.Config.DefaultWeight.ConfigEntry.DefaultValue)
            {
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
