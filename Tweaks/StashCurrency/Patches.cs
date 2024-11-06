using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Stats;
using PerfectRandom.Sulfur.Core.Units;
using PerfectRandom.Sulfur.Gameplay;
using SulfurTweaks.Extensions;
using UnityEngine;

namespace SulfurTweaks.Tweaks.StashCurrency;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[HarmonyPatch]
public class Patches
{
    [HarmonyPatch]
    internal class SellPatch
    {
        [HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.TransferOwnership))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TransferOwnership_Sell(IEnumerable<CodeInstruction> instructions) =>
            new CodeMatcher(instructions)
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "owner"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_Stats"),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "ModifyCoins"))
                .SetInstruction(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Patches), nameof(ModifyCoins))))
                .InstructionEnumeration();
    }

    [HarmonyPatch]
    internal class BuyPatch
    {
        [HarmonyPatch(typeof(InventoryItem), nameof(InventoryItem.TransferOwnership))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TransferOwnership_Buy(IEnumerable<CodeInstruction> instructions) =>
            new CodeMatcher(instructions)
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_Stats"),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Neg),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "ModifyCoins"))
                .SetInstruction(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Patches), nameof(ModifyCoins))))
                .InstructionEnumeration();

        [HarmonyPatch(typeof(RepairStation), "playerCanAfford", MethodType.Getter)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PlayerCanAfford_Get(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator) =>
            new CodeMatcher(instructions, generator)
                .Start()
                .CreateLabel(out Label label)
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Property(typeof(StaticInstance<GameManager>), "Instance").GetGetMethod()),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Property(typeof(GameManager), "IsPlayerAtChurch").GetGetMethod()),
                    new CodeInstruction(OpCodes.Brfalse_S, label),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RepairStation), "playerUnit")),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RepairStation), "repairCost")),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(BuyPatch), nameof(UnitCanAffordCombined))),
                    new CodeInstruction(OpCodes.Ret))
                .InstructionEnumeration();

        [HarmonyPatch(typeof(RepairStation), "DoRepair")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DoRepair(IEnumerable<CodeInstruction> instructions) =>
            new CodeMatcher(instructions)
                .MatchEndForward(
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "repairCost"),
                    new CodeMatch(OpCodes.Neg),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "ModifyCoins"))
                .SetInstruction(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Patches), nameof(ModifyCoins))))
                .InstructionEnumeration();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryItem), "UnitCanAfford")]
        static bool UnitCanAfford(Unit unit, int price, ref bool __result)
        {
            bool isPlayerAtChurch = StaticInstance<GameManager>.Instance.IsPlayerAtChurch;
            if (!unit.isPlayer) return true;
            if (!isPlayerAtChurch) return true;
            
            __result = UnitCanAffordCombined(unit, price);

            return false;
        }

        static bool UnitCanAffordCombined(Unit unit, int price)
        {
            AttributeManager attributeManager = StaticInstance<AttributeManager>.Instance;
            WorldResource sulfurResource = attributeManager.GetWorldResource(WorldResources.Resource_SulfCoin);

            long combinedAmount = unit.Stats.worldResources[(int)sulfurResource.id] +
                                  unit.Stats.worldResourcesStash[(int)sulfurResource.id];

            return combinedAmount >= price;
        }
    }

    private static void ModifyCoins(EntityStats instance, int amount)
    {
        if (instance.GetCoinResource() == null)
        {
            instance.SetCoinResource(
                StaticInstance<AttributeManager>.Instance.GetWorldResource(WorldResources.Resource_SulfCoin));
        }

        bool isPlayerAtChurch = StaticInstance<GameManager>.Instance.IsPlayerAtChurch;

        if (instance.owner.isPlayer && isPlayerAtChurch)
        {
            SulfurTransaction(instance, instance.GetCoinResource(), amount);
            return;
        }

        instance.ModifyWorldResource(instance.GetCoinResource(), amount);
    }

    private static void SulfurTransaction(EntityStats instance, WorldResource worldResource, int amount)
    {
        int stashSulfur = instance.worldResourcesStash[(int)worldResource.id];
        int inventorySulfur = instance.worldResources[(int)worldResource.id];
        int stashAmount = 0;
        int inventoryAmount = 0;

        if (amount > 0)
        {
            // Prioritize stashing sulfur, if stash is full then store the rest in inventory
            stashAmount = Mathf.Min(amount, worldResource.maxAmountStored - stashSulfur);
            inventoryAmount = amount - stashAmount;
        }
        else if (amount < 0)
        {
            // Prioritize spending sulfur from inventory,
            // if inventory is empty or not enough then take the rest from stash
            inventoryAmount = Mathf.Max(amount, -inventorySulfur);
            stashAmount = amount - inventoryAmount;
        }

        if (stashAmount != 0)
        {
            instance.ModifyWorldResourceStash(worldResource, stashAmount);
            instance.onWorldResourceMod?.Invoke(StoragePlaceType.Player, worldResource,
                instance.worldResources[(int)worldResource.id]);
        }

        if (inventoryAmount != 0)
            instance.ModifyWorldResource(worldResource, inventoryAmount);
    }
}