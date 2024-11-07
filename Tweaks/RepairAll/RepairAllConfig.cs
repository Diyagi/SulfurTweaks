using BepInEx.Configuration;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.RepairAll;

public class RepairAllConfig : ITweakConfig
{
    internal static ConfigEntry<bool> Enabled;
    
    public void Initialize()
    {
        Enabled = SulfurPlugin.ConfigFile.Bind(RepairAll.TweakName,
            "Enabled",
            true,
            "Enabled/Disable showing the amount of Sulfur in stash on inventory");
    }
}