using BepInEx;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using System.Linq;
using TeacherAPI;
using UnityEngine;
using static BepInEx.BepInDependency;

namespace NullTeacher
{
    // There is a lot of dependencies ik
    [BepInPlugin("sakyce.baldiplus.teacherextension.null.nullfloor", "Null Floor", "1.0.0.0")]
    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    [BepInDependency("sakyce.baldiplus.teacherextension.null", DependencyFlags.HardDependency)]
    public class NullFloorPlugin : BaseUnityPlugin
    {
        private static void EditLevelToNull(LevelObject level)
        {
            // Edit npcs
            level.potentialNPCs = new List<WeightedNPC>();
            level.forcedNpcs = new NPC[] { };
            level.additionalNPCs = 0;
            level.name = "ModdedNullLevel";

            // Only cafeterias
            level.potentialSpecialRooms = (
                from x in RoomAssetMetaStorage.Instance.AllOfCategory(RoomCategory.Special)
                    //where x.value.name.Contains("Cafeteria")
                select new WeightedRoomAsset() { selection = x.value, weight = 100 }
            ).ToArray();
            foreach (var special in level.potentialSpecialRooms)
            {
                print(special.selection.name);
            }

            // Any activities
            level.potentialClassRooms = (
                from x in Resources.FindObjectsOfTypeAll<RoomAsset>()
                where x.category == RoomCategory.Class
                select new WeightedRoomAsset() { selection = x, weight = 100 }
            ).ToArray();

            level.classStickToHallChance = 0.5f;
            level.extraStickToHallChance = 0.5f;
            level.facultyStickToHallChance = 0.33f;
            level.extraDoorChance = 0.6f;
            level.windowChance = 0.8f;

            level.standardDarkLevel = Color.black;
            level.lightMode = LightMode.Greatest;

            // Very important to make sure that no other npcs will spawn
            level.previousLevels = new LevelObject[] { };
        }


        // Not related to TeacherAPI but that's a good example of how to insert a floor
        // Try not to edit F1,F2,F3,YAY or END as it might clash with other mods
        // Although, it's good to offset the levels to place a level inbetween like I do
        // YAY would be any level after Level 3
        private static void AddNullLevel()
        {
            var F3 = (from x in Resources.FindObjectsOfTypeAll<SceneObject>() where x.levelTitle == "F3" select x).First();
            var F1 = (from x in Resources.FindObjectsOfTypeAll<SceneObject>() where x.levelTitle == "F1" select x).First(); // debug, to remove
            var YAY = F3.nextLevel;
            var NULL = Instantiate(F3);
            var level = Instantiate(NULL.levelObject);

            // Create the level and its scene
            NULL.levelTitle = "NUL";
            NULL.levelNo = F3.levelNo + 1;
            NULL.nextLevel = YAY;
            NULL.name = "ModdedNullScene";
            NULL.levelObject = level;
            YAY.levelNo += 1;
            F3.nextLevel = NULL;

            EditLevelToNull(level);
        }

        private void ForceNull(LevelObject ld)
        {
            // Null only!!!
            foreach (var baldi in ld.potentialBaldis)
            {
                baldi.weight = 0;
            }
            ld.AddPotentialTeacher(NullTeacherPlugin.Instance.NullTeacher, 1000000);
        }

        private void RegisterGenerator(string floorName, int floorNumber, LevelObject ld)
        {
            // Every 10 floors in InfiniteFloors
            if (floorName.Equals("INF") && floorNumber % NullConfiguration.InfiniteFloorsFrequency.Value == 0)
            {
                print("Boss Level Null");
                EditLevelToNull(ld);
                ForceNull(ld);
                return;
            }

            if (floorName == "NUL")
            {
                ForceNull(ld);
            }
        }

        internal void Awake()
        {
            // For safety, only edit the SceneObjects when Infinite Floors is not loaded.
            if (!TeacherPlugin.IsEndlessFloorsLoaded())
            {
                LoadingEvents.RegisterOnAssetsLoaded(Info, AddNullLevel, false);
            }

            // Finalizer BECAUSE he is supposed to be a boss, must have the final say from all the other mods.
            GeneratorManagement.Register(this, GenerationModType.Finalizer, RegisterGenerator);
        }
    }
}
