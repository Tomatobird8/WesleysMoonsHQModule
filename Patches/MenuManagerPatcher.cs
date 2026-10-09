using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using WesleyMoonScripts;

namespace WesleysMoonsHQModule.Patches;
/// <summary>
/// Pack validity checks to inform user whether the current pack configuration seems valid.
/// </summary>
[HarmonyPatch(typeof(MenuManager))]
internal class MenuManagerPatcher
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    [HarmonyBefore("OreoM.VLog")]
    internal static void Start_Postfix(MenuManager __instance)
    {
        if (__instance.isInitScene)
        {
            return;
        }
        string invalidSessionReason = "";

        int verNum = GameNetworkManager.Instance.gameVersionNum;

        WesleysMoonsHQModule.Logger.LogInfo("Game version: " + verNum);

        // Global required mods
        invalidSessionReason += CheckModValidity(PackDefinition.commonRequiredMods, true);

        bool packFound = false;

        // Version specific required mods
        foreach (PackDefinition pack in WesleysMoonsHQModule.packDefinitions)
        {
            if ((int)pack.Version != verNum) continue;
            invalidSessionReason += CheckModValidity(pack.RequiredMods, true);
            invalidSessionReason += CheckForBannedMods(pack.DisallowedMods);
            packFound = true;
        }

        if (!packFound) 
        {
            invalidSessionReason += "Unsupported game version, ";
        }

        // FreeMoons special check
        if (WesleysMoonsHQModule.pluginInfos.ContainsKey(OtherPluginInfos.FREEMOONS_GUID) && WesleyScripts.LockMoons.Value)
        {
            invalidSessionReason += "Freemoons installed in non-SMHQ mode, ";
        }
        else if (!WesleysMoonsHQModule.pluginInfos.ContainsKey(OtherPluginInfos.FREEMOONS_GUID) && !WesleyScripts.LockMoons.Value)
        {
            invalidSessionReason += "Freemoons missing in SMHQ mode, ";
        }

        // Vlog special check
        if (!WesleysMoonsHQModule.pluginInfos.ContainsKey(OtherPluginInfos.VLOG_GUID))
        {
            invalidSessionReason += "Vlog missing, ";
        }

        // Display warning
        if (!invalidSessionReason.IsNullOrWhiteSpace())
        {
            invalidSessionReason = invalidSessionReason.TrimEnd(',', ' ');
            WesleysMoonsHQModule.Logger.LogWarning($"WARNING! Modpack misconfiguration: {invalidSessionReason}");
            __instance.DisplayMenuNotification($"WARNING! Modpack misconfiguration: {invalidSessionReason}", "[ OK ]");
        }
    }

    internal static string CheckModValidity(Dictionary<string, Version> dict, bool required)
    {
        string invalidSessionReason = "";
        foreach (KeyValuePair<string, Version> entry in dict)
        {
            if (!WesleysMoonsHQModule.pluginInfos.ContainsKey(entry.Key))
            {
                if (required) invalidSessionReason += $"{entry.Key} v{entry.Value} is misssing, ";
                continue;
            }
            else if (WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.Version != entry.Value)
            {
                if (!required && WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.Version <= entry.Value)
                {
                    continue;
                }
                invalidSessionReason += $"{WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.GUID} v{WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.Version} didnt match required version v{entry.Value}, ";
            }
        }
        return invalidSessionReason;
    }

    internal static string CheckForBannedMods(string[] names)
    {
        foreach (string s in names)
        {
            if (WesleysMoonsHQModule.pluginInfos.ContainsKey(s)) return "Invalid mods found - please reinstall the pack, ";
        }
        return "";
    }
}
