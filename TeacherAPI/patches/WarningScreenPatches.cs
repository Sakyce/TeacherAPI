using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(WarningScreen), "Start")]
    [HarmonyPriority(Priority.LowerThanNormal)] // Low priority, because MTMModdingAPI is supposed to be the first.
    internal class WarningScreenCustomText
    {
        internal static bool preventAdvance = false;
        private static string forceText = null;

        internal static bool Prefix(WarningScreen __instance)
        {
            if (forceText != null)
            {
                __instance.textBox.SetText(forceText);
            }
            if (preventAdvance)
            {
                return false; // will prevent Baldi from formating the text to "Click button to continue"
            }

            preventAdvance = true;
            var text = @"<color=red>Here be dragons!!!</color> 
<color=blue>TeacherAPI</color> is still a <color=yellow>prototype</color> and you will see unexpected things!</color>

If you encounter an error, send me the Logs!
You can find the logs in <color=yellow>Baldi's Basics Plus/BepInEx/LogOutput.log</color>
";
            void Format(string txt)
            {
                if (InputManager.Instance.SteamInputActive)
                {
                    __instance.textBox.SetText(string.Format(txt, InputManager.Instance.GetInputButtonName("MouseSubmit")));
                }
                __instance.textBox.SetText(string.Format(txt, "ANY BUTTON"));
            }

            IEnumerator Coro()
            {
                __instance.textBox.SetText(text);
                yield return new WaitForSeconds(4f);
                preventAdvance = false;
                Format(__instance.textBox.text + @"
<alpha=#AA>PRESS {0} TO CONTINUE");
                yield break;
            }

            if (TeacherPlugin.DebugMode)
            {
                preventAdvance = false;
                GlobalStateManager.Instance.skipNameEntry = true; // Doesn't works with Dev API 3.6.0.0
                SceneManager.LoadScene("MainMenu");
                return false;
            }
            __instance.StartCoroutine(Coro());
            return false;
        }
        internal static void ShowWarningScreen(string text)
        {
            forceText = text;
            preventAdvance = true;
            SceneManager.LoadScene("Warnings");
        }
    }
    [HarmonyPatch(typeof(WarningScreen), "Advance")]
    [HarmonyPriority(Priority.Low)]
    internal class WarningScreenPreventAdvance
    {
        internal static bool Prefix() => !WarningScreenCustomText.preventAdvance;
    }
}
