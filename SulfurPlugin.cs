using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SulfurPlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    internal static ConfigFile ConfigFile;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");

        ConfigFile = Config;

        InitializeTweaksConfigs();
        InitializeTweaks();
    }

    private static void InitializeTweaks()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        var tweakTypes = assembly.GetTypes()
            .Where(type => typeof(ITweak).IsAssignableFrom(type) && !type.IsAbstract);

        foreach (Type tweakType in tweakTypes)
        {
            object tweakInstance = Activator.CreateInstance(tweakType);

            MethodInfo initializeMethod = tweakType.GetMethod("Initialize");
            if (initializeMethod == null) return;
            
            initializeMethod.Invoke(tweakInstance, null);
        }
    }

    private static void InitializeTweaksConfigs()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        var tweakConfigTypes = assembly.GetTypes()
            .Where(type => typeof(ITweakConfig).IsAssignableFrom(type) && !type.IsAbstract);

        foreach (Type tweakConfigType in tweakConfigTypes)
        {
            object tweakConfigInstance = Activator.CreateInstance(tweakConfigType);

            MethodInfo initializeMethod = tweakConfigType.GetMethod("Initialize");
            if (initializeMethod == null) return;

            initializeMethod.Invoke(tweakConfigInstance, null);
        }
    }
}