using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using WesleyMoonScripts;

namespace WesleysMoonsHQModule.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatcher
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        internal static void Start_Postfix(MenuManager __instance)
        {
            string invalidSessionReason = "";

            int verNum = GameNetworkManager.Instance.gameVersionNum;

            // Global required mods
            invalidSessionReason += CheckModValidity(PackDefinition.commonRequiredMods, true);

            // Version specific required mods
            switch (verNum)
            {
                case 69:
                    invalidSessionReason += CheckModValidity(PackDefinition.v69Mods, true);
                    break;

                case 72:
                    invalidSessionReason += CheckModValidity(PackDefinition.v72Mods, true);
                    break;

                case 73:
                    invalidSessionReason += CheckModValidity(PackDefinition.v73Mods, true);
                    break;

                default:
                    invalidSessionReason += "Unsupported game version";
                    break;
            }

            // Global optional mods
            invalidSessionReason += CheckModValidity(PackDefinition.commonOptionalMods, false);

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
                __instance.DisplayMenuNotification($"WARNING! Modpack misconfiguration: {invalidSessionReason}", "[ OK ]");
                WesleysMoonsHQModule.Logger.LogWarning($"WARNING! Modpack misconfiguration: {invalidSessionReason}");
            }
        }

        internal static string CheckModValidity(Dictionary<string, string> dict, bool required)
        {
            string invalidSessionReason = "";
            foreach (KeyValuePair<string, string> entry in dict)
            {
                if (!WesleysMoonsHQModule.pluginInfos.ContainsKey(entry.Key))
                {
                    if (required) invalidSessionReason += $"{entry.Key} v{entry.Value} is misssing, ";
                    continue;
                }
                else if (WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.Version.ToString() != entry.Value)
                {
                    invalidSessionReason += $"{WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.GUID} v{WesleysMoonsHQModule.pluginInfos[entry.Key].Metadata.Version.ToString()} didnt match required version v{entry.Value}, ";
                }
            }
            return invalidSessionReason;
        }
    }
}
