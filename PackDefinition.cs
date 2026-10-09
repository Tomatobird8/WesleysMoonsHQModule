using System;
using System.Collections.Generic;
using OPI = WesleysMoonsHQModule.OtherPluginInfos;

namespace WesleysMoonsHQModule;
/// <summary>
/// Modpack definition container
/// </summary>
internal class PackDefinition(WesleysMoonsHQModule.Versions version, Dictionary<string, Version> requiredMods, Dictionary<string, Version> optionalMods, string[] disallowedMods, Type[] patches)
{
    public WesleysMoonsHQModule.Versions Version = version;
    public Dictionary<string, Version> RequiredMods = requiredMods;
    public Dictionary<string, Version> OptionalMods = optionalMods;
    public string[] DisallowedMods = disallowedMods;
    public Type[] Patches = patches;

    // --- MOD VERSION DEFINITIONS ---
    internal static Dictionary<string, Version> commonRequiredMods = [];
}
