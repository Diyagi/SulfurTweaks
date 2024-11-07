using HarmonyLib;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.FocusLoss;

public class FocusLoss : ITweakConfig
{
    internal static string TweakName => "FocusLoss";
    private readonly Harmony _harmonyIDBuy = new($"{MyPluginInfo.PLUGIN_NAME}-{TweakName}");

    public void Initialize()
    {
        UpdatePatchState();
    }

    private void UpdatePatchState()
    {
        SulfurPlugin.Logger.LogInfo($"Applying Patches for Tweak: {TweakName}");
        _harmonyIDBuy.PatchAll(typeof(Patches));
    }
}