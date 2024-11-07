using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.UI;
using PerfectRandom.Sulfur.Core.Units;
using SulfurTweaks.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SulfurTweaks.Tweaks.CurrencyDisplay;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[HarmonyPatch]
public class Patches
{
    [HarmonyPatch(typeof(ResourceDisplayEntry), nameof(ResourceDisplayEntry.SetAmount))]
    [HarmonyPrefix]
    // Patches methods that updates the Sulfur Text in the UI
    public static bool SetAmount(ResourceDisplayEntry __instance, StoragePlaceType ___storagePlace)
    {
        if (___storagePlace != StoragePlaceType.Player) return true;
        if (__instance.GetTrackingResource().id != WorldResources.Resource_SulfCoin) return true;

        UpdateSulfurDisplay(__instance);
        return false;
    }

    [HarmonyPatch(typeof(ResourceDisplay), nameof(ResourceDisplay.OnInventoryOpen))]
    [HarmonyPrefix]
    // UI updates do not happen unless an change triggers it
    // so we need to force update when inventory opens
    public static void OnInventoryOpen(ResourceDisplay __instance)
    {
        if (__instance.GetStoragePlace() != StoragePlaceType.Player) return;

        AttributeManager attributeManager = StaticInstance<AttributeManager>.Instance;
        WorldResource sulfurResource = attributeManager.GetWorldResource(WorldResources.Resource_SulfCoin);
        ResourceDisplayEntry entry = __instance.GetResourceDisplayItems()[sulfurResource];

        UpdateSulfurDisplay(entry);
    }

    internal static void UpdateSulfurDisplay(bool forceNoStash = false)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        var inventoryDisplay = StaticInstance<UIManager>.Instance.InventoryUI.GetResourceDisplays();

        AttributeManager attributeManager = StaticInstance<AttributeManager>.Instance;
        WorldResource sulfurResource = attributeManager.GetWorldResource(WorldResources.Resource_SulfCoin);

        foreach (ResourceDisplay resourceDisplay in inventoryDisplay)
        {
            if (resourceDisplay.GetStoragePlace() != StoragePlaceType.Player) continue;

            UpdateSulfurDisplay(resourceDisplay.GetResourceDisplayItems()[sulfurResource], forceNoStash);
        }
    }

    private static void UpdateSulfurDisplay(ResourceDisplayEntry instance, bool forceNoStash = false)
    {
        WorldResource trackingResource = instance.GetTrackingResource();
        TextMeshProUGUI amountText = instance.GetAmountText();
        Unit playerUnit = instance.GetPlayerUnit();

        bool isPlayerAtChurch = StaticInstance<GameManager>.Instance.IsPlayerAtChurch;
        bool inStash = StaticInstance<UIManager>.Instance.InventoryUI.InStashUI;
        int inventorySulfur = playerUnit.Stats.GetWorldResource(trackingResource);

        if (isPlayerAtChurch && !inStash && !forceNoStash)
        {
            int stashSulfur = playerUnit.Stats.GetWorldResourceStash(trackingResource);

            amountText.text = $"{inventorySulfur} ({stashSulfur})";
            amountText.autoSizeTextContainer = true;

            float newWidth = amountText.preferredWidth;
            RectTransform rectTransform = amountText.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
        }
        else
        {
            amountText.text = inventorySulfur.ToString();
        }
    }
}