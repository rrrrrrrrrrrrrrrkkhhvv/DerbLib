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


namespace DerbLib
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)] 
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.derb.derbLib";
        public const string ModName = "Derb Lib";
        public const string ModVersion = "1.0.1";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;
        public void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            _harmony.PatchAll();
            Logger.LogInfo($"{ModName} loaded :)");
            Logger.LogInfo("This is my first mod so sorry for bugs");
        }
    }

    /*[HarmonyPatch(typeof(PlayerCamera), "Awake")]
    public static class TestPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            var testSlot = DerbItemLib.ItemSlot.Make(PlayerCamera.main.mainCanvas.transform.Find("MainView/RadialMenu"));
            var rectTrans = (RectTransform)testSlot.transform;
            rectTrans.anchoredPosition = new Vector2(700, -800); 
        }
    }*/
}
