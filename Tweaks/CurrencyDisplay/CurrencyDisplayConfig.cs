using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.CurrencyDisplay;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CurrencyDisplayConfig : ITweakConfig
{
    internal static ConfigEntry<bool> Enabled;

    public void Initialize()
    {
        Enabled = SulfurPlugin.ConfigFile.Bind(CurrencyDisplay.TweakName,
            "Enabled",
            true,
            "Enabled/Disable repair all feature.");
    }
}