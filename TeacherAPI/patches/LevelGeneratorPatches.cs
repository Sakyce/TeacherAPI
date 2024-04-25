using HarmonyLib;
using System.Collections;
using System.Diagnostics;
using TeacherAPI.utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.Generate))]
    internal class LevelGeneratorPatches
    {
        internal static void Postfix(LevelGenerator __instance, ref IEnumerator __result)
        {
            var man = __instance.ec.gameObject.AddComponent<TeacherManager>();
            man.Initialize(__instance.ec, __instance.seed);
            TeacherPlugin.Instance.currentBaldi = TeacherPlugin.Instance.GetPotentialBaldi(__instance.ld);

            object itemAction(object i)
            {
                if (man.MainTeacherPrefab != null) return i;

                var controlledRng = new System.Random(__instance.seed);
                man.MainTeacherPrefab = WeightedSelection<Teacher>.ControlledRandomSelection(TeacherPlugin.Instance.potentialTeachersPerFloor[__instance.ld].ToArray(), controlledRng);
                __instance.ld.potentialBaldis = new WeightedNPC[] { }; // Don't put anything in EC.NPCS, only secondary teachers can be there.

                return i;
            }

            var routine = new SimpleEnumerator(__result) { itemAction = itemAction };
            __result = routine.GetEnumerator();
        }
    }
}
