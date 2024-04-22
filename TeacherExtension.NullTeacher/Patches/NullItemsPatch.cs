using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                BaseGameManager.Instance.AngerBaldi(50f);
                foreach (var teacher in TeacherPlugin.GetTeachersOfType<NullTeacher>())
                {
                    teacher.SpeechCheck(NullPhrase.Enough, 0.33f);
                }
            }
        }
    }
}
