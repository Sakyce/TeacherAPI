using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TeacherAPI.utils;
using UnityEngine;

namespace TeacherAPI
{
    public class WeightedTeacherNotebook : WeightedSelection<Teacher>
    {
        public Sprite[] sprites;
        public WeightedTeacherNotebook(Teacher teacher)
        {
            selection = teacher;
            weight = 1;
        }
        public WeightedTeacherNotebook Weight(int weight)
        {
            this.weight = weight;
            return this;
        }
        public WeightedTeacherNotebook Sprite(params Sprite[] sprites)
        {
            this.sprites = sprites;
            return this;
        }
        public static WeightedTeacherNotebook GetRandom(WeightedTeacherNotebook[] weighteds, System.Random rng)
        {
            // doesn't supports weights at the moment, BB+ WeightedSelection is probably broken
            var next = rng.Next(weighteds.Count());
            return weighteds[next];
        }
    }
    internal class TeacherNotebook : MonoBehaviour
    {
        public Character character;
        public EnvironmentController ec;
        public TeacherManager teacherMan;
        internal void Initialize(EnvironmentController ec)
        {
            teacherMan = ec.gameObject.GetComponent<TeacherManager>();
            this.ec = ec;

            var teacherPool = teacherMan.assistingTeachersPrefabs
                .OfType<Teacher>()
                .Select(t => t.GetTeacherNotebookWeight())
                .AddItem(teacherMan.MainTeacherPrefab.GetTeacherNotebookWeight());

            teacherPool
                .Select(t => $"{t.selection} {t.weight}")
                .Print("Teacher weight Notebooks", TeacherPlugin.Log);

            var randomTeacher = WeightedTeacherNotebook.GetRandom(teacherPool.ToArray(), teacherMan.controlledRng);
            character = randomTeacher.selection.Character;
            print($"Selected {randomTeacher.selection.name} for {EnumExtensions.GetExtendedName<Character>((int)character)}");

            var notebook = gameObject.GetComponent<Notebook>();
            if (randomTeacher.sprites != null)
            {
                var i = teacherMan.controlledRng.Next(randomTeacher.sprites.Count());
                notebook.sprite.sprite = randomTeacher.sprites[i];
            }

            teacherMan.MaxTeachersNotebooks.TryGetValue(character, out int maxNotebooks);
            teacherMan.MaxTeachersNotebooks[character] = maxNotebooks + 1;
        }
        internal void OnCollect()
        {
            teacherMan.CurrentTeachersNotebooks.TryGetValue(character, out int currentNotebooks);
            teacherMan.CurrentTeachersNotebooks[character] = currentNotebooks + 1;

            // Oh my god, am I drunk ?
            teacherMan.spawnedTeachers
                .Where(t => t.Character == character)
                .ToList()
                .ForEach(t => t.behaviorStateMachine.CurrentState.AsTeacherState(e => e.NotebookCollected()));
        }

        internal static void RefreshNotebookText()
        {
            var teacherMan = TeacherManager.Instance;
            var teachers = new Teacher[] {}
                .AddItem(teacherMan.MainTeacherPrefab)
                .Concat(teacherMan.assistingTeachersPrefabs);

            if (teachers.Count() > 0)
            {
                var notebookText = "";
                foreach (var teacher in teachers)
                {
                    teacherMan.CurrentTeachersNotebooks.TryGetValue(teacher.Character, out int currentNotebooks);
                    teacherMan.MaxTeachersNotebooks.TryGetValue(teacher.Character, out int maxNotebooks);
                    notebookText = notebookText + teacher.GetNotebooksText(
                            $"{currentNotebooks}/{maxNotebooks}"
                    ) + '\n';
                }
                CoreGameManager.Instance.GetHud(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 200);
                CoreGameManager.Instance.GetHud(0).UpdateText(0, notebookText);
            }
        }
    }

    [HarmonyPatch(typeof(Notebook), nameof(Notebook.Start))]
    internal static class AttachTeacherNotebook
    {
        internal static void Postfix(Notebook __instance)
        {
            if (__instance.gameObject.GetComponent<TeacherNotebook>() != null)
            {
                return;
            }
            var teacherNotebook = __instance.gameObject.AddComponent<TeacherNotebook>();
            teacherNotebook.Initialize(BaseGameManager.Instance.Ec);
        }
    }

    [HarmonyPatch(typeof(Activity), nameof(Activity.SetNotebook))]
    internal static class AttachTeacherNotebookToActivity
    {
        internal static void Postfix(Activity __instance, Notebook val)
        {
            var teacherNotebook = val.gameObject.AddComponent<TeacherNotebook>();
            teacherNotebook.Initialize(BaseGameManager.Instance.Ec);
        }
    }
    [HarmonyPatch(typeof(MathMachine), nameof(MathMachine.NumberClicked))]
    internal static class PreventMathMachinesFromBeingSolved
    {
        internal static bool Prefix(MathMachine __instance)
        {
            var teacherNotebook = __instance.notebook.gameObject.GetComponent<TeacherNotebook>();
            if (!teacherNotebook.teacherMan.SpoopModeActivated && teacherNotebook.character != teacherNotebook.teacherMan.MainTeacherPrefab.Character)
            {
                __instance.audMan.PlaySingle(__instance.audLose);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebook))]
    internal static class CollectTeacherNotebook
    {
        internal static void Postfix(BaseGameManager __instance, ref Notebook notebook)
        {
            var teacherNotebook = notebook.gameObject.GetComponent<TeacherNotebook>();
            teacherNotebook.OnCollect();
            TeacherNotebook.RefreshNotebookText(); // Maybe possible to remove that
        }
    }

    [HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.CollectNotebooks))]
    [HarmonyPriority(Priority.Last)] // Let mods that adds "#/n Elevators" overwrite this.
    internal static class MainNotebooksText
    {
        internal static void Postfix(MainGameManager __instance)
        {
            TeacherNotebook.RefreshNotebookText();
        }
    }

}