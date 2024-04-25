using HarmonyLib;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(HappyBaldi), nameof(HappyBaldi.OnTriggerExit))]
    internal class OnTriggerExitPatch
    {
        internal static bool Prefix()
        {
            if (TeacherManager.Instance.spawnedTeachers.Count > 0)
            {
                foreach (var teacher in TeacherManager.Instance.spawnedTeachers)
                {
                    teacher.behaviorStateMachine.currentState.AsTeacherState().IfSuccess(state => state.PlayerExitedSpawn());
                }
                // Don't want baldi to mess up with the spawn
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.PleaseBaldi))]
    internal class OnGoodMathMachineAnswer
    {
        internal static void Prefix()
        {
            foreach (var teacher in TeacherManager.Instance.spawnedTeachers)
            {
                teacher.behaviorStateMachine.currentState.AsTeacherState().IfSuccess(state => state.GoodMathMachineAnswer());
            }
        }
    }
}
