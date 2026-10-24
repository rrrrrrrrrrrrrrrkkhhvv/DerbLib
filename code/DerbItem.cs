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
using DerbLib;

namespace DerbItemLib
{
    public class DerbItemDef
    {
        public static List<DerbItemDef> Defs;
        public string targetItem;
        public Action<Body, Item> OnUse;
        public Action<Limb, Item> OnLimbUse;
        public Action<Item> OnMade;

        public static DerbItemDef MakeFromItemInfo(ItemInfo itemInfo, string ID, Sprite icon)
        {
            var Def = new DerbItemDef();
            Def.targetItem = ID;

            if (itemInfo.useAction != null)
            {
                Def.OnUse += (body, item) => itemInfo.useAction(body, item);
            }
            itemInfo.useAction = Def.OnUseFunc;

            if (itemInfo.useLimbAction != null)
            {
                Def.OnLimbUse += (limb, item) => itemInfo.useLimbAction(limb, item);
            }
            itemInfo.useLimbAction = Def.OnLimbUseFunc;

            ItemRegistry.Register(ID, itemInfo, icon);
            Defs.Add(Def);
            return Def;
        }

        public void OnUseFunc(Body body, Item item)
        {
            OnUse.Invoke(body, item);
        }

        public void OnLimbUseFunc(Limb limb, Item item)
        {
            OnLimbUse.Invoke(limb, item);
        }
    }
    public class DerbItem : MonoBehaviour
    {
        public static List<DerbItem> Items;
        public Item item;
        public DerbItemDef Def;
        public Dictionary<string, DerbMeta> Data;

        public void Awake()
        {
            if (!TryGetComponent(out Item item)) {Destroy(this); return;}
            var def = DerbItemDef.Defs.FirstOrDefault(def => def.targetItem == item.id);
            if (def == null){Destroy(this); return;}
            Def = def;
            Items.Add(this);
            Def.OnMade.Invoke(item);
        }
    }
}