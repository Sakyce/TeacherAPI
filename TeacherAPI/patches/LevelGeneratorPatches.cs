using HarmonyLib;
using MTM101BaldAPI;
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
            var seed = CoreGameManager.Instance.Seed();
            var man = __instance.ec.gameObject.AddComponent<TeacherManager>();
            man.Initialize(__instance.ec, seed);
            TeacherPlugin.Instance.currentBaldi = TeacherPlugin.Instance.GetPotentialBaldi(__instance.ld);

            object itemAction(object obj)
            {
                if (man.MainTeacherPrefab != null) return obj;
                if (TeacherPlugin.Instance.potentialTeachers[__instance.ld].Count <= 0)
                {
                    TeacherManager.DefaultBaldiEnabled = true;
                    return obj;
                };
                TeacherManager.DefaultBaldiEnabled = false;

                var rng = new System.Random(seed);

                var mainTeacher = WeightedSelection<Teacher>.ControlledRandomSelectionList(TeacherPlugin.Instance.potentialTeachers[__instance.ld], rng);
                man.MainTeacherPrefab = mainTeacher;
                TeacherPlugin.Log.LogInfo($"Selected Main Teacher {EnumExtensions.GetExtendedName<Character>((int)mainTeacher.character)}");

                var potentialAssistants = TeacherPlugin.Instance.potentialAssistants[__instance.ld]
                    .Where(t => t.selection != man.MainTeacherPrefab)
                    .ToArray();
                if (potentialAssistants.Length > 0 && !TeacherAPIConfiguration.DisableAssistingTeachers.Value)
                {
                    var assistant = WeightedSelection<Teacher>.ControlledRandomSelection(potentialAssistants, rng);
                    man.assistingTeachersPrefabs.Add(assistant);
                    TeacherPlugin.Log.LogInfo($"Selected Teacher {EnumExtensions.GetExtendedName<Character>((int)assistant.character)}");
                }

                __instance.ld.potentialBaldis = new WeightedNPC[] { }; // Don't put anything in EC.NPCS, only secondary teachers can be there.

                return obj;
            }

            void postfix()
            {
                if (TeacherManager.DefaultBaldiEnabled) return;
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
