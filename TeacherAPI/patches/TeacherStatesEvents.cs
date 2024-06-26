﻿using HarmonyLib;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(HappyBaldi), nameof(HappyBaldi.OnTriggerExit))]
    internal class OnTriggerExitPatch
    {
        internal static bool Prefix()
        {
            if (TeacherPlugin.Instance.spawnedTeachers.Count > 0)
            {
                foreach (var teacher in TeacherPlugin.Instance.spawnedTeachers)
                {
                    teacher.behaviorStateMachine.currentState.AsTeacherState().IfSuccess(state => state.PlayerExitedSpawn());
                }
                // Don't want baldi to mess up with the spawn
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebooks))]
    internal class OnNotebookCollectedPatch
    {
        internal static void Postfix()
        {
            foreach (var teacher in TeacherPlugin.Instance.spawnedTeachers)
            {
                teacher.behaviorStateMachine.currentState.AsTeacherState().IfSuccess(state => state.NotebookCollected());
            }
        }
    }

    [HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.AllNotebooks))]
    internal class OnAllNotebooksCollected
    {
        internal static void Postfix()
        {
            Singleton<CoreGameManager>.Instance.audMan.FlushQueue(true);
            if (TeacherPlugin.Instance.spawnedTeachers.Count > 0)
            {
                foreach (var teacher in TeacherPlugin.Instance.spawnedTeachers)
                {
                    teacher.OnAllNotebooksCollected();
                }
            }
        }
    }
    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.PleaseBaldi))]
    internal class OnGoodMathMachineAnswer
    {
        internal static void Prefix()
        {
            foreach (var teacher in TeacherPlugin.Instance.spawnedTeachers)
            {
                teacher.behaviorStateMachine.currentState.AsTeacherState().IfSuccess(state => state.GoodMathMachineAnswer());
            }
        }
    }
}
