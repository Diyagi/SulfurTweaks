using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.CurrencyDisplay;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CurrencyDisplay : ITweak
{
    internal static string TweakName => "CurrencyDisplay";
    private readonly Harmony _harmonyID = new($"{MyPluginInfo.PLUGIN_NAME}-{TweakName}");

    public CurrencyDisplay()
    {
        CurrencyDisplayConfig.Enabled.SettingChanged += SettingChanged;
    }

    public void Initialize()
    {
        UpdatePatchState(CurrencyDisplayConfig.Enabled.Value);
    }

    private void SettingChanged(object sender, EventArgs args)
    {
        bool isEnabled = CurrencyDisplayConfig.Enabled.Value;
        
        UpdatePatchState(isEnabled);
        Patches.UpdateSulfurDisplay(!isEnabled);
    }

    private void UpdatePatchState(bool state)
    {
        if (state)
        {
            SulfurPlugin.Logger.LogInfo($"Applying Patches for Tweak: {TweakName}");
            _harmonyID.PatchAll(typeof(Patches));
        }
        else
        {
            SulfurPlugin.Logger.LogInfo($"Removing Patches for Tweak: {TweakName}");
            _harmonyID?.UnpatchSelf();
        }
    }
}