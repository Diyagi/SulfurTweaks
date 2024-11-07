using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Audio;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.UI;
using PerfectRandom.Sulfur.Core.UI.Inventory;
using PerfectRandom.Sulfur.Core.Units;
using PerfectRandom.Sulfur.Gameplay;
using SulfurTweaks.Tweaks.StashCurrency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SulfurTweaks.Tweaks.RepairAll;

[HarmonyPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Patches
{
    private static bool isOpen;
    
    private static RectTransform repairAllButtonContainer;
    private static Button repairAllButton;
    private static RectTransform repairAllPriceContainer;
    private static TextMeshProUGUI repairAllPrice;

    private static List<InventoryItem> itemsToRepair = [];
    private static List<InventoryItem> itemsToConsume = [];
    private static int repairCost;

    private static GameObject baseGameObject;
    private static Unit unit;

    [HarmonyPatch(typeof(RepairStation), "SetupRepairStation")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SetupRepairStation(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i =>
                    i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "UpdateCombination"))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RepairStation), "buttonContainer")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(SetupRepairAllComponent))))
            .InstructionEnumeration();
    
    [HarmonyPatch(typeof(PaperdollSlot), nameof(PaperdollSlot.SetToItem))]
    [HarmonyPostfix]
    private static void SetToItem()
    {
        if (!isOpen) return;
        
        UpdateRepairAllCombination();
    }

    [HarmonyPatch(typeof(RepairStation), "PositionUI")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> PositionUI(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .End()
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Patches), nameof(PositionRepairAllComponent))))
            .InstructionEnumeration();
    
    [HarmonyPatch(typeof(RepairStation), "UIOnEnter")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> UIOnEnter(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .End()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RepairStation), "playerUnit")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Property(typeof(GameObject), nameof(GameObject.gameObject)).GetGetMethod()),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(RepairAllOnEnter))))
            .InstructionEnumeration();

    [HarmonyPatch(typeof(RepairStation), "UIOnExit")]
    [HarmonyPostfix]
    private static void UIOnExit()
    {
        InventoryUI inventoryUI = StaticInstance<UIManager>.Instance.InventoryUI;
        
        inventoryUI.PlayerBackpackGrid.onContentsChanged -= UpdateRepairAllCombination;
        repairAllButtonContainer.gameObject.SetActive(false);
        repairAllButton.onClick.RemoveListener(DoRepairAll);

        isOpen = false;
    }
    
    public static void RepairAllOnEnter(Unit playerUnit, GameObject gameObject)
    {
        InventoryUI inventoryUI = StaticInstance<UIManager>.Instance.InventoryUI;
        baseGameObject = gameObject;
        unit = playerUnit;

        UpdateRepairAllCombination();
        repairAllButton.onClick.AddListener(DoRepairAll);
        inventoryUI.PlayerBackpackGrid.onContentsChanged += UpdateRepairAllCombination;
        repairAllButtonContainer.gameObject.SetActive(true);

        isOpen = true;
    }

    private static void SetupRepairAllComponent(RectTransform buttonContainer)
    {
        repairAllButtonContainer = Object.Instantiate(buttonContainer, buttonContainer.parent, true);

        repairAllButton = repairAllButtonContainer.GetComponentInChildren<Button>();
        repairAllPriceContainer = repairAllButtonContainer.Find("PriceContainer").GetComponent<RectTransform>();
        repairAllPrice = repairAllPriceContainer.GetComponentInChildren<TextMeshProUGUI>();

        repairAllButton.GetComponentInChildren<TextMeshProUGUI>().text = "Repair All";

        for (int i = 0; i < repairAllButtonContainer.childCount; i++)
        {
            repairAllButtonContainer.GetChild(i).gameObject.SetActive(true);
        }

        repairAllButtonContainer.gameObject.SetActive(false);
    }

    private static void PositionRepairAllComponent(float y)
    {
        float newY = y - 60f;
        repairAllButtonContainer.anchoredPosition = new Vector2(0f, newY);
    }

    private static void UpdateRepairAllCombination()
    {
        InventoryUI inventoryUI = StaticInstance<UIManager>.Instance.InventoryUI;

        List<InventoryItem> list = inventoryUI.PlayerBackpackGrid.AllItems();

        itemsToRepair.Clear();
        itemsToConsume.Clear();
        int num = 0;
        foreach (InventoryItem inventoryItem in list)
        {
            if (inventoryItem.IsRepairable)
            {
                itemsToRepair.Add(inventoryItem);
                num += (int)(inventoryItem.PriceBase * 0.2f * (1f - inventoryItem.DurabilityNormalized));
            }

            if (inventoryItem.itemDefinition.repairReductionCost > 0)
            {
                itemsToConsume.Add(inventoryItem);
                num -= inventoryItem.itemDefinition.repairReductionCost;
            }
        }

        repairCost = num;
        if (itemsToRepair.Count > 0 && PlayerCanAfford(num))
        {
            repairAllButton.interactable = true;
        }
        else
        {
            repairAllButton.interactable = false;
        }

        repairAllPrice.color = PlayerCanAfford(num)
            ? SulfurConstants.COLOR_SulfurYellow
            : SulfurConstants.COLOR_RedText;
        repairAllPrice.text = repairCost.ToString();
    }

    private static void DoRepairAll()
    {
        if (itemsToRepair.Count == 0)
        {
            return;
        }

        if (!PlayerCanAfford(repairCost))
        {
            return;
        }

        if (StashCurrencyConfig.BuyWithStash.Value)
        {
            StashCurrency.Patches.ModifyCoins(unit.Stats, -repairCost);
        }
        else
        {
            unit.Stats.ModifyCoins(-repairCost);
        }

        StaticInstance<SoundBank>.Instance.PlayClip(Sfx.UI_Repair, 1f);
        foreach (InventoryItem inventoryItem in itemsToConsume)
        {
            inventoryItem.GetConsumedByExternalFactor(baseGameObject);
        }

        foreach (InventoryItem inventoryItem2 in itemsToRepair)
        {
            inventoryItem2.ModifyDurability(inventoryItem2.DurabilityMax);
            StaticInstance<AnalyticsManager>.Instance.TrackItemRepair(inventoryItem2);
        }

        UpdateRepairAllCombination();
    }

    private static bool PlayerCanAfford(int value)
    {
        bool isPlayerAtChurch = StaticInstance<GameManager>.Instance.IsPlayerAtChurch;

        if (StashCurrencyConfig.BuyWithStash.Value && isPlayerAtChurch)
        {
            return StashCurrency.Patches.BuyPatch.UnitCanAffordCombined(unit, value);
        }

        return unit.Stats.GetCoins() >= repairCost;
    }
}