﻿using HarmonyLib;
using System.Collections;
using System.Linq;
using TeacherAPI.utils;

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

            object itemAction(object obj)
            {
                if (man.MainTeacherPrefab != null) return obj;

                var rng = new System.Random(__instance.seed);
                var i = WeightedSelection<Teacher>.ControlledRandomIndexList(TeacherPlugin.Instance.potentialTeachers[__instance.ld], rng);
                man.MainTeacherPrefab = TeacherPlugin.Instance.potentialTeachers[__instance.ld][i].selection;

                var potentialAssistants = TeacherPlugin.Instance.potentialAssistants[__instance.ld]
                    .Where(t => t.selection != man.MainTeacherPrefab)
                    .ToArray();
                var assistant = WeightedSelection<Teacher>.ControlledRandomSelection(potentialAssistants, rng);
                man.assistingTeachersPrefabs.Add(assistant);

                __instance.ld.potentialBaldis = new WeightedNPC[] { }; // Don't put anything in EC.NPCS, only secondary teachers can be there.

                return obj;
            }

            void postfix()
            {
                var controlledRng = new System.Random(__instance.seed);
                __instance.Ec.offices
                    .ForEach(office => __instance.Ec.BuildPosterInRoom(office, man.MainTeacherPrefab.Poster, controlledRng));
                foreach (var assistant in man.assistingTeachersPrefabs)
                {
                    __instance.Ec.offices
                        .ForEach(office => __instance.Ec.BuildPosterInRoom(office, assistant.Poster, controlledRng));
                }
            }

            var routine = new SimpleEnumerator(__result) { itemAction = itemAction, postfixAction = postfix };
            __result = routine.GetEnumerator();
        }
    }
}
