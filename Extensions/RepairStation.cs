using System.Reflection;
using HarmonyLib;
using PerfectRandom.Sulfur.Gameplay;
using UnityEngine;

namespace SulfurTweaks.Extensions;

public static class RepairStationExtensions
{
    private static readonly FieldInfo FiCraftInventory =
        AccessTools.DeclaredField(typeof(RepairStation), "craftInventory");
    
    private static readonly FieldInfo FiButtonContainer =
        AccessTools.DeclaredField(typeof(RepairStation), "buttonContainer");

    public static CraftInventory GetCraftInventory(this RepairStation instance)
    {
        return (CraftInventory)FiCraftInventory.GetValue(instance);
    }
    
    public static RectTransform GetButtonContainer(this RepairStation instance)
    {
        return (RectTransform)FiCraftInventory.GetValue(instance);
    }
}