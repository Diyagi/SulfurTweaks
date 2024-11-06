using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;

namespace SulfurTweaks.Extensions;

public static class ResourceDisplayExtensions
{
    private static readonly FieldInfo FiStoragePlace =
        AccessTools.DeclaredField(typeof(ResourceDisplay), "storagePlace");

    private static readonly FieldInfo FiResourceDisplayItems =
        AccessTools.DeclaredField(typeof(ResourceDisplay), "resourceDisplayItems");

    public static StoragePlaceType GetStoragePlace(this ResourceDisplay instance)
    {
        return (StoragePlaceType)FiStoragePlace.GetValue(instance);
    }

    public static Dictionary<WorldResource, ResourceDisplayEntry> GetResourceDisplayItems(this ResourceDisplay instance)
    {
        return (Dictionary<WorldResource, ResourceDisplayEntry>)FiResourceDisplayItems.GetValue(instance);
    }
}