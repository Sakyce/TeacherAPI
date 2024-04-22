using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TeacherAPI.utils;
using UnityEngine;

namespace TeacherAPI.patches
{
    //[HarmonyPatch(typeof(CharacterPostersRoomFunction), nameof(CharacterPostersRoomFunction.Build))]
    // Patch disabled because it misses the Poster of the Teachers
    internal class CharacterPostersRoomPatch
    {
        internal static bool Prefix(LevelBuilder builder)
        {
            var charactersThatDisablesSpawn = (
                from x in builder.Ec.npcsToSpawn
                where x.IsTeacher() && ((Teacher)x).disableNpcs
                select (Teacher)x
            );
            charactersThatDisablesSpawn.Print("Characters that disable NPC spawn");
            if (charactersThatDisablesSpawn.Count() > 0)
            {
                return false;
            }
            return true;
        }
    }
}
