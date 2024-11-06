using System.Reflection;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.UI.Inventory;

namespace SulfurTweaks.Extensions;

public static class EntityStatsExtension
{
    private static readonly FieldInfo FiCoinResource =
        AccessTools.DeclaredField(typeof(EntityStats), "coinResource"); 
    
    public static WorldResource GetCoinResource(this EntityStats instance)
    {
        return (WorldResource)FiCoinResource.GetValue(instance);
    }
    
    public static void SetCoinResource(this EntityStats instance, WorldResource resource)
    {
        FiCoinResource.SetValue(instance, resource);
    }
}