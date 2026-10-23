using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;
using CUCoreLib.ContentReload;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using UnityEngine.EventSystems;
using System.Xml.Serialization;
using UnityEngine.UIElements;
using System.Linq;
using DerbInvLib;

namespace DerbInvLibPatch
{
    [HarmonyPatch(typeof(PlayerCamera))]
    static class DropOnItemSlot
    {
        [HarmonyPatch(nameof(PlayerCamera.TryPerformInventoryAction))]
        [HarmonyPrefix]
        static bool Prefix(PlayerCamera __instance, ref RaycastResult hit, List<RaycastResult> uiCasts, ref bool __result)
        {
            if (!hit.gameObject.TryGetComponent(out ItemSlot slot)) return true;
            if (!slot.Button.Overlaps(uiCasts)) return true;

            var drag = __instance.dragItem;
            if (slot.GetItem() == drag) {
                __result = true; return false;
             }

            if (!slot.TryGetItem(out _))
            {
                if (drag.container) drag.container.UnloadItem(drag);
                if (__instance.body.HoldingItem(drag)) __instance.body.DropItem(drag);
                slot.PickupItem(drag);
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(PlayerCamera.HandleStartDragging))]
        [HarmonyPostfix]
        static void ReleasePostfix(PlayerCamera __instance)
        {
            var drag = __instance.dragItem;
            if (!drag) return;
            ItemSlot.Slots.FirstOrDefault(slot => slot.GetItem() == drag)?.Release();
        }
    }

    [HarmonyPatch(typeof(Body))]
    public class CombinePatch
    {
        [HarmonyPatch(nameof(Body.CanCombine))]
        [HarmonyPrefix]
        public static bool CombineCheck(ref Item it1, ref Item it2, ref bool __result)
        {
            foreach (Combination combo in Combination.Combinations)
            {
                if ((combo.item1 == it1 & combo.item2 == it2) || (combo.item2 == it1 & combo.item1 == it2))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Body.CombineItems))]
        [HarmonyPrefix]
        public static bool Combine(ref Item it1, ref Item it2)
        {
            foreach (Combination combo in Combination.Combinations)
            {
                if ((combo.item1 == it1 & combo.item2 == it2) || (combo.item2 == it1 & combo.item1 == it2))
                {
                    combo.OnCombine.Invoke(combo.item1 == it1 ? it1 : it2, combo.item1 == it1 ? it2 : it1);
                    return false;
                }
            }
            return true;
        }
    }
}