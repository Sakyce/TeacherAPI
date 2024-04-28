using HarmonyLib;
using TeacherAPI;

namespace NullTeacher.Patches
{
    [HarmonyPatch(typeof(ITM_PrincipalWhistle), nameof(ITM_PrincipalWhistle.Use))]
    internal class WhistlePatch
    {
        internal static void Postfix(PlayerManager pm, ref bool __result)
        {
            if (__result)
            {
                pm.ec.MakeNoise(pm.transform.position, 127);
                foreach (var teacher in TeacherManager.Instance.GetTeachersOfType<NullTeacher>())
                {
                    teacher.GetAngry(50f);
                    teacher.SpeechCheck(NullPhrase.Enough, 0.33f);
                }
            }
        }
    }
}
