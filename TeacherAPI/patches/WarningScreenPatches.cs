using HarmonyLib;
using UnityEngine.SceneManagement;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(WarningScreen), "Start")]
    [HarmonyPriority(Priority.Low)] // Low priority, because MTMModdingAPI is supposed to be the first.
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
            return true;
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
