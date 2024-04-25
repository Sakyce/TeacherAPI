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
            if (TeacherAPIConfiguration.DisableAssetsWarningScreen.Value)
            {
                return true;
            }
            if (forceText != null)
            {
                __instance.textBox.SetText(forceText);
            }
            if (preventAdvance)
            {
                return false; // will prevent Baldi from formating the text to "Click button to continue"
            }

            // Custom Warning for Teacher API
            var text = @"<color=red>Here be dragons!!!</color> 
<color=blue>TeacherAPI</color> is still a <color=yellow>prototype</color> and you will see unexpected things!</color>

Please read the instructions to report any bugs in the mod page!
If you encounter an error, send me the Logs!
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
                preventAdvance = true;
                __instance.textBox.SetText(text);
                yield return new WaitForSeconds(4f);
                preventAdvance = false;
                Format(__instance.textBox.text + @"
<alpha=#AA>PRESS {0} TO CONTINUE");
                yield break;
            }

            if (TeacherAPIConfiguration.DebugMode.Value)
            {
                preventAdvance = false;
                GlobalStateManager.Instance.skipNameEntry = true; // Doesn't works with Dev API 3.6.0.0
                SceneManager.LoadScene("MainMenu");
                return false;
            }
            if (!TeacherAPIConfiguration.EnableCustomWarningScreen.Value)
            {
                return true;
            }
            __instance.StartCoroutine(Coro());
            return false;
        }
        internal static void ShowWarningScreen(string text)
        {
            if (TeacherAPIConfiguration.DisableAssetsWarningScreen.Value)
            {
                TeacherPlugin.Log.LogError(text);
                return;
            }

            forceText = text;
            preventAdvance = true;
            SceneManager.LoadScene("Warnings");
        }
    }
    [HarmonyPatch(typeof(WarningScreen), "Advance")]
    [HarmonyPriority(Priority.Low)]
    internal class WarningScreenPreventAdvance
    {
        internal static bool Prefix() => !WarningScreenCustomText.preventAdvance || TeacherAPIConfiguration.DisableAssetsWarningScreen.Value;
    }
}
