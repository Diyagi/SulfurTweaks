using System;
using BepInEx.Configuration;
using HarmonyLib;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.StashCurrency;

public class StashCurrency : ITweak
{
    internal static string TweakName => "StashCurrency";
    private readonly Harmony _harmonyIDBuy = new($"{MyPluginInfo.PLUGIN_NAME}-{TweakName}-Buy");
    private readonly Harmony _harmonyIDSell = new($"{MyPluginInfo.PLUGIN_NAME}-{TweakName}-Sell");

    public StashCurrency()
    {
        SulfurPlugin.ConfigFile.SettingChanged += SettingChanged;
    }

    public void Initialize()
    {
        UpdatePatchState();
    }

    private void SettingChanged(object sender, SettingChangedEventArgs args)
    {
        UpdatePatchState(args.ChangedSetting);
    }

    private void UpdatePatchState(ConfigEntryBase config = null)
    {
        if (config == StashCurrencyConfig.SellToStash || config == null)
        {
            if (StashCurrencyConfig.SellToStash!.Value)
            {
                SulfurPlugin.Logger.LogInfo($"Applying Patches for Tweak: {TweakName}-Sell");
                _harmonyIDSell.PatchAll(typeof(Patches.SellPatch));
            }
            else
            {
                SulfurPlugin.Logger.LogInfo($"Removing Patches for Tweak: {TweakName}-Sell");
                _harmonyIDSell.UnpatchSelf();
            }
        }
        
        if (config == StashCurrencyConfig.BuyWithStash || config == null)
        {
            if (StashCurrencyConfig.BuyWithStash!.Value)
            {
                SulfurPlugin.Logger.LogInfo($"Applying Patches for Tweak: {TweakName}-Buy");
                _harmonyIDBuy.PatchAll(typeof(Patches.BuyPatch));
            }
            else
            {
                SulfurPlugin.Logger.LogInfo($"Removing Patches for Tweak: {TweakName}-Buy");
                _harmonyIDBuy.UnpatchSelf();
            }
        }
        
    }
}