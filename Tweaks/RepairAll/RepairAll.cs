using HarmonyLib;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.RepairAll;

public class RepairAll : ITweak
{
    internal static string TweakName => "RepairAll";
    private readonly Harmony _harmonyIDBuy = new($"{MyPluginInfo.PLUGIN_NAME}-{TweakName}");

    public void Initialize()
    {
        RepairAllConfig.Enabled.SettingChanged += (_, _) => { UpdatePatchState(); };
        UpdatePatchState();
    }

    private void UpdatePatchState()
    {
        if (RepairAllConfig.Enabled.Value)
        {
            SulfurPlugin.Logger.LogInfo($"Applying Patches for Tweak: {TweakName}");
            _harmonyIDBuy.PatchAll(typeof(Patches));
        }
        else
        {
            SulfurPlugin.Logger.LogInfo($"Removing Patches for Tweak: {TweakName}");
            _harmonyIDBuy.UnpatchSelf();
        }
    }
}