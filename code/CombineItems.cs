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

namespace DerbInvLib
{
    public class Combination
    {
        public static List<Combination> Combinations = [];
        public Item item1;
        public Item item2;
        public Action<Item, Item> OnCombine;
    }
}