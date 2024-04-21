using BepInEx;
using HarmonyLib;
using TeacherAPI.utils;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BepInEx.BepInDependency;
using BepInEx.Bootstrap;

namespace TeacherAPI
{
    [BepInPlugin("sakyce.baldiplus.teacherapi", "Teacher API", "1.0.0.0")]
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

        public static AssetManager assetManager = new AssetManager();

        internal void Awake()
        {
            new Harmony("sakyce.baldiplus.teacherapi").PatchAllConditionals();
            Instance = this;

            GeneratorManagement.Register(this, GenerationModType.Base, EditGenerator);
        }
        public static Baldi ConvertTeacherToBaldi(Baldi teacher)
        {
            return teacher;
        }
        private void EditGenerator(string floorName, int floorNumber, LevelObject floorObject)
        {
            if (floorObject.potentialBaldis.Length < 1)
            {
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception("No potential baldi found. Possibly because of another mod that edit the teacher without using More Teachers API."));
            }

            if (floorName == "INF")
            {
                // MTM, do you eat clowns at breakfast ? 
                foreach (var baldi in floorObject.potentialBaldis)
                {
                    baldi.weight = 100;
                }
            }

            try
            {
                originalBaldiPerFloor.Add(floorObject, GetPotentialBaldi(floorObject));
            } catch (ArgumentException) {}
        }
        public Baldi GetPotentialBaldi(LevelObject floorObject)
        {
            var baldis = (from x in floorObject.potentialBaldis
                          where x.selection.GetType().Equals(typeof(Baldi))
                          select (Baldi)x.selection).ToArray();

            if (baldis.Length > 1)
            {
                (from baldi in baldis select baldi.name).Print("Baldis");
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception("Multiple Baldis found in the level!"));
            }
            return baldis.First();
        }

        /// <summary>
        /// Returns true if Infinite Floors/Endless Floors is loaded.
        /// </summary>
        /// <returns></returns>
        public bool IsEndlessFloorsLoaded()
        {
            return (
                from x in Chainloader.PluginInfos 
                where x.Key.Equals("mtm101.rulerp.baldiplus.endlessfloors") select x.Key
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
