using System.Reflection;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Units;
using TMPro;

namespace SulfurTweaks.Extensions;

public static class ResourceDisplayEntryExtensions
{
    private static readonly FieldInfo FiTrackingResource =
        AccessTools.DeclaredField(typeof(ResourceDisplayEntry), "trackingResource");

    private static readonly FieldInfo FiPlayerUnit =
        AccessTools.DeclaredField(typeof(ResourceDisplayEntry), "playerUnit");

    private static readonly FieldInfo FiAmountText =
        AccessTools.DeclaredField(typeof(ResourceDisplayEntry), "amountText");

    public static WorldResource GetTrackingResource(this ResourceDisplayEntry instance)
    {
        return (WorldResource)FiTrackingResource.GetValue(instance);
    }

    public static Unit GetPlayerUnit(this ResourceDisplayEntry instance)
    {
        return (Unit)FiPlayerUnit.GetValue(instance);
    }

    public static TextMeshProUGUI GetAmountText(this ResourceDisplayEntry instance)
    {
        return (TextMeshProUGUI)FiAmountText.GetValue(instance);
    }
}