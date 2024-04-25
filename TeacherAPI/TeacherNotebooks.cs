using HarmonyLib;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TeacherAPI
{
    public class WeightedTeacherNotebook
    {
        public int weight = 100;
        public Sprite sprite;
        public Teacher teacher;
        public WeightedTeacherNotebook(Teacher teacher)
        {
            this.teacher = teacher;
        }
        public WeightedTeacherNotebook Weight(int weight)
        {
            this.weight = weight;
            return this;
        }
        public WeightedTeacherNotebook Sprite(Sprite sprite)
        {
            this.sprite = sprite;
            return this;
        }
    }
    internal class TeacherNotebook : MonoBehaviour
    {
        public Character character;
        private EnvironmentController ec;
        private TeacherManager teacherMan;
        internal void Initialize(EnvironmentController ec)
        {
            teacherMan = ec.gameObject.GetComponent<TeacherManager>();
            this.ec = ec;

            var teacherPool = (from t in ec.npcsToSpawn
                               where t.IsTeacher()
                               select ((Teacher)t).GetTeacherNotebookWeight()).ToList();
            teacherPool.Add(teacherMan.MainTeacherPrefab.GetTeacherNotebookWeight());
            var s = WeightedSelection<Teacher>.ControlledRandomSelection(teacherPool.ToArray(), teacherMan.controlledRng);
            character = s.Character;

            teacherMan.MaxTeachersNotebooks.TryGetValue(character, out int maxNotebooks);
            teacherMan.MaxTeachersNotebooks[character] = maxNotebooks + 1;
        }
        internal void OnCollect()
        {
            teacherMan.CurrentTeachersNotebooks.TryGetValue(character, out int currentNotebooks);
            teacherMan.CurrentTeachersNotebooks[character] = currentNotebooks + 1;

            // Oh my god, am I drunk ?
            teacherMan.spawnedTeachers.Where(t => t.Character == character).ToList().ForEach(t => t.behaviorStateMachine.CurrentState.AsTeacherState(e => e.NotebookCollected()));
        }

        internal static void RefreshNotebookText()
        {
            if (TeacherManager.Instance.spawnedTeachers.Count > 0)
            {
                var notebookText = "";
                foreach (var teacher in TeacherManager.Instance.spawnedTeachers)
                {
                    TeacherManager.Instance.CurrentTeachersNotebooks.TryGetValue(teacher.Character, out int currentNotebooks);
                    TeacherManager.Instance.MaxTeachersNotebooks.TryGetValue(teacher.Character, out int maxNotebooks);
                    notebookText = notebookText + teacher.GetNotebooksText(
                            $"{currentNotebooks}/{maxNotebooks}"
                    ) + '\n';
                }
                CoreGameManager.Instance.GetHud(0).gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(300, 0);

                CoreGameManager.Instance.GetHud(0).UpdateText(0, notebookText);
            }
        }
    }

    [HarmonyPatch(typeof(Notebook), nameof(Notebook.Start))]
    internal static class AttachTeacherNotebook
    {
        internal static void Postfix(Notebook __instance)
        {
            var teacherNotebook = __instance.gameObject.AddComponent<TeacherNotebook>();
            teacherNotebook.Initialize(BaseGameManager.Instance.Ec);
            TeacherNotebook.RefreshNotebookText();
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
