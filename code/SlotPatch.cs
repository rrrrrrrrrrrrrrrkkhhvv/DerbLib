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
using System.Linq;
using AsmResolver.PE.DotNet.StrongName;
using TMPro;

namespace DerbInvLib
{
    public class ItemSlot : MonoBehaviour
    {
        public static readonly List<ItemSlot> Slots = new();
        public static int NextIndex = -1;
        public int Index;
        public bool Active;
        public InvButton Button;
        public event Action<Item, Item> ItemChanged;

        public Item GetItem()
        {
            return Button.refItem;
        }

        public bool TryGetItem(out Item item)
        {
            var itm = GetItem();
            item = itm;
            return itm != null;
        }

        public void DropItem(Vector2 position)
        {
            if (!TryGetItem(out var item)) return;
            Release();
            item.rb.position = position;
            item.rb.velocity = Vector2.zero;
            item.rb.angularVelocity = 0f;
        }

        void SetItem(Item newItem)
        {
            var old = Button.refItem;
            Button.refItem = newItem;
            ItemChanged?.Invoke(old, newItem);
        }
        
        public void PickupItem(Item item)
        {
            if (TryGetItem(out _)) return;
            item.rb.simulated = false;
            item.transform.SetParent(PlayerCamera.main.body.transform, false);
            item.transform.localPosition = Vector3.zero;
            item.GetComponent<SpriteRenderer>().enabled = false;
            SetItem(item);
        }
        
        public void Release()
        {
            if (!TryGetItem(out var item)) return;
            item.transform.SetParent(null, true);
            item.GetComponent<SpriteRenderer>().enabled = true;
            item.rb.simulated = true;
            SetItem(null);
        }
        
        public void DeleteItem()
        {
            if (TryGetItem(out var item))
            {
                SetItem(null);
                Destroy(item.gameObject);
            }
        }
        
        void OnDestroy()
        {
            Slots.Remove(this);
        }
        
        public void Setup()
        {
            Button = GetComponent<InvButton>();
            Button.slot = -1;
            Index = NextIndex--;
            Slots.Add(this);
        }

        public void Awake()
        {
            Setup();
        }

        public static ItemSlot Make(Transform parent)
        {
            return Instantiate(Resources.Load<GameObject>("ContainerItem"), parent).AddComponent<ItemSlot>();
        }
    }

    [HarmonyPatch(typeof(InvButton))]
    public static class ButtonPatch
    {
        [HarmonyPatch(nameof(InvButton.UpdateGraphic))]
        [HarmonyPrefix]
        public static bool UpdatePrefix(ref InvButton __instance)
        {
            if (!__instance.TryGetComponent<ItemSlot>(out _)){return true;}
            Item item = __instance.GetItem();
            float num = 1f;
            if ((bool)item)
            {
                __instance.itemImg.enabled = true;
                __instance.itemImg.sprite = item.GetComponent<SpriteRenderer>().sprite;
                if (__instance.woundView)
                {
                    __instance.itemImg.color = item.Stats.ActuallyUsableOnLimb(item) ? Color.white : new Color(1f, 1f, 1f, 0.1f);
                    num = item.Stats.ActuallyUsableOnLimb(item) ? 1f : 0.1f;
                    __instance.locked.enabled = !__instance.body.aboveMedicalCutoff && !item.Stats.ignoreDepression;
                }
                __instance.itemImg.rectTransform.sizeDelta = PlayerCamera.ImageSizeDelta(__instance.itemImg.sprite.texture, 3f, __instance.maxImageSize);
                Color color = PlayerCamera.ConditionToColor(item.condition);
                color.a = num;
                __instance.itemCond.color = color;
                if (!item.container)
                {
                    __instance.itemCond.fillAmount = item.condition;
                }
                else
                {
                    __instance.itemCond.fillAmount = item.container.GetHoldingWeight() / item.container.maxWeight;
                }
                (string, string) tuple = PlayerCamera.ItemHoverDescription(item);
                __instance.tooltip.tipName = tuple.Item1;
                __instance.tooltip.tipDesc = tuple.Item2;
                if (item.Stats is LiquidItemInfo liquidItemInfo && item.TryGetComponent<WaterContainerItem>(out var component) && component.hasSprite)
                {
                    __instance.waterFill.sprite = component.fillSprite;
                    __instance.waterFill.rectTransform.sizeDelta = PlayerCamera.ImageSizeDelta(component.fillSprite.texture, 3f, __instance.maxImageSize);
                    __instance.waterFill.color = component.AverageColor().WithAlpha(num);
                    __instance.waterFill.fillAmount = component.CurrentTotal / liquidItemInfo.capacity;
                    __instance.waterFill.enabled = true;
                }
                else
                {
                    __instance.waterFill.enabled = false;
                }
            }
            else
            {
                __instance.itemImg.enabled = false;
                num = __instance.woundView ? 0.1f : 0.5f;
                __instance.itemCond.fillAmount = 0f;
                __instance.tooltip.tipName = string.Empty;
                if (__instance.woundView)
                {
                    __instance.locked.enabled = !__instance.body.aboveMedicalCutoff;
                }
                __instance.waterFill.enabled = false;
            }
            Color color2 = Color.white;
            if ((bool)PlayerCamera.main.dragItem && (bool)item && item.TryGetComponent<Container>(out var component2) && !component2.CanHoldItem(PlayerCamera.main.dragItem))
            {
                color2 = Color.red;
            }
            color2.a = num;
            if (__instance.miniHovered)
            {
                color2.r *= 0.55f;
                color2.g *= 0.55f;
                color2.b *= 0.55f;
            }
            if ((bool)item && item.favourited)
            {
                color2.g *= 0.922f;
                color2.b *= 0.529f;
            }
            __instance.image.color = color2;
            return false;
        }
    }
}