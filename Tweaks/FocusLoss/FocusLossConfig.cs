using BepInEx.Configuration;
using SulfurTweaks.Interfaces;

namespace SulfurTweaks.Tweaks.FocusLoss;

public class FocusLossConfig : ITweakConfig
{
    internal static ConfigEntry<bool> PauseOnFocusLoss;
    internal static ConfigEntry<bool> MuteOnFocusLoss;
    public void Initialize()
    {
        PauseOnFocusLoss = SulfurPlugin.ConfigFile.Bind(FocusLoss.TweakName,
            nameof(PauseOnFocusLoss),
            true,
            "Enabled/Disable pausing game on focus loss.");
        MuteOnFocusLoss = SulfurPlugin.ConfigFile.Bind(FocusLoss.TweakName,
            nameof(MuteOnFocusLoss),
            true,
            "Enabled/Disable muting game on focus loss.");
    }
}