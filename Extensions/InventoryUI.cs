using System.Reflection;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.UI.Inventory;

namespace SulfurTweaks.Extensions;

public static class InventoryUIExtensions
{
    private static readonly FieldInfo FiResourceDisplays =
        AccessTools.DeclaredField(typeof(InventoryUI), "resourceDisplays"); 
    
    public static ResourceDisplay[] GetResourceDisplays(this InventoryUI instance)
    {
        return (ResourceDisplay[])FiResourceDisplays.GetValue(instance);
    }
}