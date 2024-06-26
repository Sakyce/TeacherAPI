﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeacherAPI.patches;
using TeacherAPI.utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BepInEx.BepInDependency;

namespace TeacherAPI
{
    [BepInPlugin("sakyce.baldiplus.teacherapi", "Teacher API", PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.endlessfloors", DependencyFlags.SoftDependency)]
    public class TeacherPlugin : BaseUnityPlugin
    {
        public static TeacherPlugin Instance { get; private set; }
        public bool SpoopModeEnabled { get; internal set; }

        internal Dictionary<Character, NPC> whoAreTeachers = new Dictionary<Character, NPC>(); // Mostly used to differenciate who are teachers and who are not.
        internal Dictionary<LevelObject, Baldi> originalBaldiPerFloor = new Dictionary<LevelObject, Baldi>();
        public List<Teacher> spawnedTeachers = new List<Teacher>();
        public Baldi currentBaldi;

        public static ManualLogSource Log { get => Instance.Logger; }

        internal void Awake()
        {
            Instance = this;
            TeacherAPIConfiguration.Setup();
            //            foreach (var plugin in Chainloader.PluginInfos)
            //            {
            //                if (plugin.Value.Metadata.GUID == "mtm101.rulerp.bbplus.baldidevapi" && plugin.Value.Metadata.Version <= new Version("3.6.0.0"))
            //                {
            //                    ShowWarningScreen($@"
            //The mod <color=blue>TeacherAPI</color> requires a more recent version of <color=red>Baldi Dev API</color>!</color>

            //The current version you have is <color=yellow>{plugin.Value.Metadata.Version}</color> and the required version is <color=green>4.0.0.0</color>


            //<alpha=#AA>PRESS ALT + F4 TO CLOSE THIS GAME
            //");
            //                    break;
            //                }
            //            }
            new Harmony("sakyce.baldiplus.teacherapi").PatchAllConditionals();
            GeneratorManagement.Register(this, GenerationModType.Base, EditGenerator);

            // Remove this when a more recent version of Dev API release
            if (TeacherAPIConfiguration.DebugMode.Value)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        internal static Baldi ConvertTeacherToBaldi(Baldi teacher)
        {
            return teacher;
        }
        private void EditGenerator(string floorName, int floorNumber, LevelObject floorObject)
        {
            if (floorObject.potentialBaldis.Length < 1)
            {
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception("No potential baldi found. Possttttibly because of another mod that edit the teacher without using More Teachers API."));
            }
            else if (floorObject.potentialBaldis.Length > 1)
            {
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception("More than one potential baldi found. Possibly because of another mod."));
            }

            if (!TeacherAPIConfiguration.EnableBaldi.Value)
            {
                foreach (var baldi in floorObject.potentialBaldis)
                {
                    baldi.weight = 0;
                }
                Logger.LogInfo("Set Baldi weight to 0 for this floor");
            }

            if (floorName == "INF")
            {
                // MTM, do you eat clowns at breakfast ? 
                foreach (var baldi in floorObject.potentialBaldis)
                {
                    baldi.weight = TeacherAPIConfiguration.EnableBaldi.Value ? 100 : 0;
                }
            }

            try
            {
                originalBaldiPerFloor.Add(floorObject, GetPotentialBaldi(floorObject));
            }
            catch (ArgumentException) { }
        }
        internal Baldi GetPotentialBaldi(LevelObject floorObject)
        {
            var baldis = (from x in floorObject.potentialBaldis
                          where x.selection.GetType().Equals(typeof(Baldi))
                          select (Baldi)x.selection).ToArray();

            if (baldis.Length > 1)
            {
                (from baldi in baldis select baldi.name).Print("Baldis", TeacherPlugin.Log);
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception("Multiple Baldis found in the level!"));
            }
            return baldis.First();
        }

        /// <summary>
        /// Will show a warning screen telling the user to install the mod correctly
        /// if the folder for the specified plugin is not found in Modded.
        /// </summary>
        public static void RequiresAssetsFolder(BaseUnityPlugin plug)
        {
            string assetsPath = AssetLoader.GetModPath(plug);
            if (!Directory.Exists(assetsPath))
            {
                WarningScreenCustomText.ShowWarningScreen(String.Format(@"
The mod <color=blue>{0}</color> must have the assets file in <color=red>StreamingAssets/Modded</color>!</color>

The name of the assets folder must be <color=red>{1}</color>.


<alpha=#AA>PRESS ALT + F4 TO CLOSE THIS GAME
", Path.GetFileName(plug.Info.Location), plug.Info.Metadata.GUID));
            }
        }

        public static void ShowWarningScreen(string text)
        {
            WarningScreenCustomText.ShowWarningScreen(text);
        }

        public static T[] GetTeachersOfType<T>() where T : Teacher
        {
            return (from teacher in Instance.spawnedTeachers where teacher.GetType().Equals(typeof(T)) select (T)teacher).ToArray();
        }

        /// <summary>
        /// Returns true if Infinite Floors/Endless Floors is loaded.
        /// </summary>
        /// <returns></returns>
        public static bool IsEndlessFloorsLoaded()
        {
            return (
                from x in Chainloader.PluginInfos
                where x.Key.Equals("mtm101.rulerp.baldiplus.endlessfloors")
                select x.Key
            ).Count() > 0;
        }

        /// <summary>
        /// Make your teacher known to TeacherAPI
        /// </summary>
        /// <param name="teacher"></param>
        public static void RegisterTeacher(Teacher teacher)
        {
            teacher.ignorePlayerOnSpawn = true; // Or else, the teacher won't spawn instantly.
            Instance.whoAreTeachers.Add(teacher.Character, teacher);
        }

        /// <summary>
        /// Load textures from a pattern, used to easily load animations.
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="pattern">A pattern that will go through String.Format(pattern, i)</param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Texture2D[] TexturesFromMod(BaseUnityPlugin mod, string pattern, (int, int) range)
        {
            var textures = new List<Texture2D>();
            for (int i = range.Item1; i <= range.Item2; i++)
            {
                textures.Add(AssetLoader.TextureFromMod(mod, String.Format(pattern, i)));
            }
            return textures.ToArray();
        }
    }
}
