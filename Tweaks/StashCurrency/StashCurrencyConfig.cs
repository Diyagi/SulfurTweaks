using BepInEx.Configuration;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.StashCurrency;

public class StashCurrencyConfig : ITweakConfig
{
    internal static ConfigEntry<bool> BuyWithStash;
    internal static ConfigEntry<bool> SellToStash;
    
    public void Initialize()
    {
        BuyWithStash = SulfurPlugin.ConfigFile.Bind(StashCurrency.TweakName,
            nameof(BuyWithStash),
            true,
            "Enabled/Disable buying items with Sulfur from stash (only when in Church)");
        
        SellToStash = SulfurPlugin.ConfigFile.Bind(StashCurrency.TweakName,
            nameof(SellToStash),
            true,
            "Enabled/Disable depositing sulfur from items sell to stash (only when in Church)");
    }
}