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
using DerbItemLib;

namespace DerbItemLibPatch
{
    [HarmonyPatch(typeof(Item))]
    public class ItemPatch
    {
        [HarmonyPatch(nameof(Item.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(Item __instance)
        {
            var def = DerbItemDef.Defs.FirstOrDefault(def => def.targetItem == __instance.id);
            if (def != null)
            {
                __instance.gameObject.AddComponent<DerbItem>();
            }
        }
    }
}