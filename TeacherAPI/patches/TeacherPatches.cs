using HarmonyLib;
using System.Linq;
using TeacherAPI.utils;
using UnityEngine;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.GetBaldi))]
    [HarmonyPriority(Priority.First)]
    internal class GetBaldiPatch
    {
        public static void Postfix(EnvironmentController __instance, ref Baldi __result)
        {
            if (__result == null && TeacherManager.Instance.SpawnedMainTeacher != null)
            {
                __result = (Baldi)TeacherManager.Instance.SpawnedMainTeacher;
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.BeginSpoopMode))]
    internal class BeginSpoopmodePatch
    {
        internal static bool Prefix()
        {
            TeacherManager.Instance.SpoopModeActivated = true;
            return true;
        }
    }

    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.SpawnNPC))]
    internal class ChangeStateAfterTeacherSpawn
    {
        internal static void Postfix()
        {
            foreach (var teacher in TeacherManager.Instance.spawnedTeachers.Where(x => !x.HasInitialized))
            {
                var mainTeacherPrefab = TeacherManager.Instance.MainTeacherPrefab;
                if (mainTeacherPrefab != null && TeacherManager.Instance.SpawnedMainTeacher == null)
                {
                    if (mainTeacherPrefab.GetType().Equals(teacher.GetType()))
                    {
                        TeacherManager.Instance.SpawnedMainTeacher = teacher;
                    }
                }
                teacher.behaviorStateMachine.ChangeState(teacher.GetHappyState());
                teacher.HasInitialized = true;
            }
        }
    }

    internal class ReplaceHappyBaldiWithTeacherPatch
    {
        internal static void ReplaceHappyBaldi(BaseGameManager __instance)
        {
            var happyBaldi = __instance.Ec.gameObject.GetComponentInChildren<HappyBaldi>();
            var teacherManager = __instance.Ec.gameObject.GetComponent<TeacherManager>();

            // The main teacher
            if (teacherManager.MainTeacherPrefab)
            {
                var happyBaldiPos = __instance.Ec.CellFromPosition(happyBaldi.transform.position).position;
                __instance.Ec.SpawnNPC(teacherManager.MainTeacherPrefab, happyBaldiPos);
                TeacherNotebook.RefreshNotebookText();

                happyBaldi.sprite.enabled = false;
                happyBaldi.audMan.enabled = false;
                happyBaldi.audMan.disableSubtitles = true;
                Singleton<MusicManager>.Instance.StopMidi();
            }

            foreach (var prefab in teacherManager.assistingTeachersPrefabs)
            {
                var cells = __instance.Ec.notebooks
                    .Where(n => n.gameObject.GetComponent<TeacherNotebook>().character == prefab.Character)
                    .Select(n => n.activity.room.RandomEntitySafeCellNoGarbage())
                    .ToArray();
                var i = teacherManager.controlledRng.Next(cells.Count());
                __instance.Ec.SpawnNPC(prefab, cells[i].position);
            }

            foreach (var notebook in __instance.Ec.notebooks)
            {
                var teacherNotebook = notebook.gameObject.GetComponent<TeacherNotebook>();
                if (teacherNotebook.character != teacherManager.MainTeacherPrefab.Character) 
                    notebook.Hide(true);
            }
        }

        [HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.CreateHappyBaldi))]
        internal class InMainGameManager
        {
            internal static void Postfix(MainGameManager __instance)
            {
                ReplaceHappyBaldi(__instance);
            }
        }

        [HarmonyPatch(typeof(EndlessGameManager), nameof(EndlessGameManager.CreateHappyBaldi))]
        internal class InEndlessGameManager
        {
            internal static void Postfix(EndlessGameManager __instance)
            {
                ReplaceHappyBaldi(__instance);
            }
        }
    }


    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.AngerBaldi))]
    internal class AngerBaldiPatch
    {
        public static void Postfix(ref float val)
        {
            var val2 = val;
            TeacherManager.Instance.DoIfMainTeacher(t => t.GetAngry(val2));
        }
    }

    [HarmonyPatch(typeof(RulerEvent), nameof(RulerEvent.Begin))]
    internal class BreakRulerPatch
    {
        public static void Postfix()
        {
            TeacherManager.Instance.DoIfMainTeacher(t => t.BreakRuler());
        }
    }

    [HarmonyPatch(typeof(RulerEvent), nameof(RulerEvent.End))]
    internal class RestoreRulerPatch
    {
        public static void Postfix()
        {
            TeacherManager.Instance.DoIfMainTeacher(t => t.RestoreRuler());
        }
    }
}
