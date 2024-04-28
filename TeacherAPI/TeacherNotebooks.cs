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
        public SoundObject sound;

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
        public WeightedTeacherNotebook CollectSound(SoundObject sound)
        {
            this.sound = sound;
            return this;
        }
        public static WeightedTeacherNotebook GetRandom(WeightedTeacherNotebook[] weighteds, System.Random rng)
        {
            var i = ControlledRandomIndex(weighteds, rng);
            return weighteds[i];
        }
    }
    internal class TeacherNotebook : MonoBehaviour
    {
        public Character character;
        private Sprite[] sprites;
        public EnvironmentController ec;
        public TeacherManager teacherMan;
        private SoundObject sound;
        private PropagatedAudioManager audMan;

        internal void Initialize(EnvironmentController ec)
        {
            teacherMan = ec.gameObject.GetComponent<TeacherManager>();
            this.ec = ec;

            var teacherPool = teacherMan.assistingTeachersPrefabs
                .OfType<Teacher>()
                .Select(t => t.GetTeacherNotebookWeight())
                .AddItem(teacherMan.MainTeacherPrefab.GetTeacherNotebookWeight());

            teacherMan.MaxTeachersNotebooks.TryGetValue(teacherMan.MainTeacherPrefab.Character, out int mainTeacherMaxNotebooks);
            
            // The first TeacherNotebook to initialize will always be for the main teacher (or else softlock)
            var randomTeacher = mainTeacherMaxNotebooks <= 0
                ? teacherMan.MainTeacherPrefab.GetTeacherNotebookWeight()
                : WeightedTeacherNotebook.GetRandom(teacherPool.ToArray(), teacherMan.controlledRng);
            character = randomTeacher.selection.Character;
            sprites = randomTeacher.sprites;

            var notebook = gameObject.GetComponent<Notebook>();
            if (randomTeacher.sprites != null)
            {
                var i = teacherMan.controlledRng.Next(randomTeacher.sprites.Count());
                notebook.sprite.sprite = randomTeacher.sprites[i];
            }
            if (randomTeacher.sound != null)
            {
                sound = randomTeacher.sound;
            }
            SetNotebookTexture();
            audMan = gameObject.AddComponent<PropagatedAudioManager>();

            teacherMan.MaxTeachersNotebooks.TryGetValue(character, out int maxNotebooks);
            teacherMan.MaxTeachersNotebooks[character] = maxNotebooks + 1;
        }
        internal void SetNotebookTexture()
        {
            if (sprites != null)
            {
                var notebook = gameObject.GetComponent<Notebook>();
                var i = teacherMan.controlledRng.Next(sprites.Count());
                notebook.sprite.sprite = sprites[i];
            }
        }
        internal void OnCollect()
        {
            teacherMan.CurrentTeachersNotebooks.TryGetValue(character, out int currentNotebooks);
            teacherMan.CurrentTeachersNotebooks[character] = currentNotebooks + 1;

            teacherMan.MaxTeachersNotebooks.TryGetValue(character, out int maxNotebooks);
            if (sound != null)
            {
                audMan.PlaySingle(sound);
            }

            // Oh my god, am I drunk ?
            teacherMan.spawnedTeachers
                .Where(t => t.Character == character)
                .ToList()
                .ForEach(t =>
                {
                    t.behaviorStateMachine.CurrentState.AsTeacherState(e => e.NotebookCollected(currentNotebooks + 1, maxNotebooks));
                    t.GetAngry(BaseGameManager.Instance.NotebookAngerVal);
                });
        }

        internal static void RefreshNotebookText()
        {
            if (TeacherManager.DefaultBaldiEnabled) return;
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
                    if (maxNotebooks == 0 && currentNotebooks == 0) continue;
                    notebookText = notebookText + teacher.GetNotebooksText(
                            $"{currentNotebooks}/{maxNotebooks}"
                    ) + '\n';
                }
                CoreGameManager.Instance.GetHud(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 200);
                CoreGameManager.Instance.GetHud(0).UpdateText(0, notebookText);
            }
        }
    }

    [HarmonyPatch(typeof(Notebook), nameof(Notebook.Start))]
    internal static class AttachTeacherNotebook
    {
        internal static void Postfix(Notebook __instance)
        {
            if (TeacherManager.DefaultBaldiEnabled) return;
            if (__instance.gameObject.GetComponent<TeacherNotebook>() != null) return;
            var teacherNotebook = __instance.gameObject.AddComponent<TeacherNotebook>();
            teacherNotebook.Initialize(BaseGameManager.Instance.Ec);
        }
    }

    [HarmonyPatch(typeof(Activity), nameof(Activity.SetNotebook))]
    internal static class AttachTeacherNotebookToActivity
    {
        internal static void Postfix(Activity __instance, Notebook val)
        {
            if (TeacherManager.DefaultBaldiEnabled) return;
            var teacherNotebook = val.gameObject.AddComponent<TeacherNotebook>();
            teacherNotebook.Initialize(BaseGameManager.Instance.Ec);
        }
    }

    [HarmonyPatch(typeof(MathMachine), nameof(MathMachine.NumberClicked))]
    internal static class PreventMathMachinesFromBeingSolved
    {
        internal static bool Prefix(MathMachine __instance)
        {
            if (TeacherManager.DefaultBaldiEnabled) return true;
            var teacherNotebook = __instance.notebook.gameObject.GetComponent<TeacherNotebook>();
            if (!teacherNotebook.teacherMan.SpoopModeActivated && teacherNotebook.character != teacherNotebook.teacherMan.MainTeacherPrefab.Character)
            {
                __instance.audMan.PlaySingle(__instance.audLose);
                return false;
            }
            return true;
        }
    }

    // Fixes the texture of the notebook being overwritten by defaults when spawning from math machine
    [HarmonyPatch(typeof(Notebook), nameof(Notebook.Start))]
    internal static class SetNotebookTextureOnStart 
    {
        internal static void Postfix(Notebook __instance)
        {
            if (TeacherManager.DefaultBaldiEnabled) return;
            var teacherNotebook = __instance.gameObject.GetComponent<TeacherNotebook>();
            teacherNotebook.SetNotebookTexture();
        }
    }


    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebook))]
    internal static class CollectTeacherNotebook
    {
        internal static void Postfix(BaseGameManager __instance, ref Notebook notebook)
        {
            if (TeacherManager.DefaultBaldiEnabled) return;
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