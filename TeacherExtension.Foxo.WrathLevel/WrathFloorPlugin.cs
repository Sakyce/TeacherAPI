using BepInEx;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using System.Linq;
using TeacherAPI;
using UnityEngine;
using static BepInEx.BepInDependency;

namespace TeacherExtension.Foxo
{
    // There is a lot of dependencies ik
    [BepInPlugin("sakyce.baldiplus.teacherextension.foxo.wrathfloor", "Foxo Wrath Floor", "1.0.0.0")]
    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    [BepInDependency("sakyce.baldiplus.teacherextension.foxo", DependencyFlags.HardDependency)]
    public class WrathFloorPlugin : BaseUnityPlugin
    {
        private static void EditLevelToWrath(LevelObject level)
        {
            // Edit npcs
            level.potentialNPCs = new List<WeightedNPC>();
            level.forcedNpcs = new NPC[] { };
            level.additionalNPCs = 0;
            level.name = "ModdedFoxoWrathLevel";

            // Only cafeterias
            level.potentialSpecialRooms = (
                from x in RoomAssetMetaStorage.Instance.AllOfCategory(RoomCategory.Special)
                where x.value.name.Contains("Cafeteria")
                select new WeightedRoomAsset() { selection = x.value, weight = 100 }
            ).ToArray();

            // No math machines
            level.potentialClassRooms = (
                from x in Resources.FindObjectsOfTypeAll<RoomAsset>()
                where (x.category == RoomCategory.Class && x.activity.prefab.GetType().Equals(typeof(NoActivity)))
                select new WeightedRoomAsset() { selection = x, weight = 100 }
            ).ToArray();

            // Tweaks. We ignore classrooms to let infinite floors decide.
            level.minSpecialRooms = 0;
            level.maxSpecialRooms = 5;
            level.minFacultyRooms = 0;
            level.maxFacultyRooms = 30;
            level.maxSize = new IntVector2(Mathf.Max(level.maxSize.x, 30), Mathf.Max(level.maxSize.z, 30));
            //level.classStickToHallChance = 0.5f;
            //level.extraStickToHallChance = 0.5f;
            level.extraDoorChance = 0.8f;
            //level.windowChance = 0.8f;
            //level.maxLightDistance *= (int)(1.25f);
            level.standardDarkLevel = Color.black;
            level.lightMode = LightMode.Greatest;

            // Very important to make sure that no other npcs will spawn
            level.previousLevels = new LevelObject[] { };
        }


        // Not related to TeacherAPI but that's a good example of how to insert a floor
        // Try not to edit F1,F2,F3,YAY or END as it might clash with other mods
        // Although, it's good to offset the levels to place a level inbetween like I do
        // YAY would be any level after Level 3
        private static void AddWrathLevel()
        {
            var F3 = (from x in Resources.FindObjectsOfTypeAll<SceneObject>() where x.levelTitle == "F3" select x).First();
            var F1 = (from x in Resources.FindObjectsOfTypeAll<SceneObject>() where x.levelTitle == "F1" select x).First(); // debug, to remove
            var YAY = F3.nextLevel;
            var WRATH = Instantiate(F3);
            var level = Instantiate(WRATH.levelObject);

            // Create the level and its scene
            WRATH.levelTitle = "FOX";
            WRATH.levelNo = F3.levelNo + 1;
            WRATH.nextLevel = YAY;
            WRATH.name = "ModdedFoxoWrathScene";
            WRATH.levelObject = level;
            YAY.levelNo += 1;
            F3.nextLevel = WRATH;

            EditLevelToWrath(level);
        }

        private void ForceWrathFoxo(LevelObject ld)
        {
            // Foxo only!!!
            foreach (var baldi in ld.potentialBaldis)
            {
                baldi.weight = 0;
            }
            ld.AddPotentialTeacher(FoxoPlugin.Instance.DarkFoxo, 1000000);
        }

        private void RegisterGenerator(string floorName, int floorNumber, LevelObject ld)
        {
            // Every 10 floors in InfiniteFloors
            if (floorName.Equals("INF") && floorNumber % FoxoConfiguration.FoxFloor.Value == 0)
            {
                print("Boss Level FOX");
                EditLevelToWrath(ld);
                ForceWrathFoxo(ld);
                return;
            }

            if (floorName == "FOX")
            {
                ForceWrathFoxo(ld);
            }
        }

        internal void Awake()
        {
            // For safety, only edit the SceneObjects when Infinite Floors is not loaded.
            if (!TeacherPlugin.IsEndlessFloorsLoaded())
            {
                LoadingEvents.RegisterOnAssetsLoaded(Info, AddWrathLevel, false);
            }

            // Finalizer BECAUSE he is supposed to be a boss, must have the final say from all the other mods.
            GeneratorManagement.Register(this, GenerationModType.Finalizer, RegisterGenerator);
        }
    }
}
