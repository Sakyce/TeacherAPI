using HarmonyLib;
using MTM101BaldAPI;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using TeacherAPI.utils;
using UnityEngine.Assertions;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.Generate))]
    internal class LevelGeneratorPatches
    {
        internal static void Postfix(LevelGenerator __instance, ref IEnumerator __result)
        {
            var seed = CoreGameManager.Instance.Seed();
            var man = __instance.ec.gameObject.AddComponent<TeacherManager>();
            man.Initialize(__instance.ec, seed);
            TeacherPlugin.Instance.CurrentBaldi = TeacherPlugin.Instance.GetPotentialBaldi(__instance.ld);

            object itemAction(object obj)
            {
                if (man.MainTeacherPrefab != null) return obj;
                if (TeacherPlugin.Instance.potentialTeachers[__instance.ld].Count <= 0 || TeacherPlugin.Instance.originalBaldiPerFloor[__instance.ld] == null)
                {
                    TeacherManager.DefaultBaldiEnabled = true;
                    return obj;
                };
                TeacherManager.DefaultBaldiEnabled = false;

                var rng = new System.Random(seed + TeacherPlugin.Instance.floorNumbers[__instance.ld]);

                var mainTeacher = WeightedSelection<Teacher>.ControlledRandomSelectionList(TeacherPlugin.Instance.potentialTeachers[__instance.ld], rng);
                man.MainTeacherPrefab = mainTeacher;
                TeacherPlugin.Instance.potentialTeachers[__instance.ld].PrintWeights("Potential Teachers", TeacherPlugin.Log);
                TeacherPlugin.Log.LogInfo($"Selected Main Teacher {EnumExtensions.GetExtendedName<Character>((int)mainTeacher.character)}");

                // Assistants setup
                var policy = mainTeacher.GetAssistantPolicy();
                var potentialAssistants = TeacherPlugin.Instance.potentialAssistants[__instance.ld]
                    .Where(t => t.selection != man.MainTeacherPrefab)
                    .Where(t => policy.CheckAssistant(t.selection))
                    .ToList();

                potentialAssistants.PrintWeights("Potential Assistants", TeacherPlugin.Log);

                for (var x = 0; x < policy.maxAssistants; x++)
                {
                    if (potentialAssistants.Count > 0 && rng.NextDouble() <= policy.probability && !TeacherAPIConfiguration.DisableAssistingTeachers.Value)
                    {
                        var i = WeightedSelection<Teacher>.ControlledRandomIndex(potentialAssistants.ToArray(), rng);
                        TeacherPlugin.Log.LogInfo($"Selected Teacher {EnumExtensions.GetExtendedName<Character>((int)potentialAssistants[i].selection.character)}");
                        man.assistingTeachersPrefabs.Add(potentialAssistants[i].selection);
                        potentialAssistants.Remove(potentialAssistants[i]);
                    }
                }

                __instance.ld.potentialBaldis = new WeightedNPC[] { }; // Don't put anything in EC.NPCS, only secondary teachers can be there.

                return obj;
            }

            void postfix()
            {
                if (TeacherManager.DefaultBaldiEnabled || TeacherPlugin.Instance.originalBaldiPerFloor[__instance.ld] == null) return;
                var controlledRng = new System.Random(seed);
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
