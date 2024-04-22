using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullTeacher.Patches
{
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.RemoveItem))]
    internal static class ExcitingRemoveItemPatch
    {
        internal static bool Prefix()
        {
            NullTeacher.timeSinceExcitingThing = 0f;
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebook))]
    internal static class ExcitingCollectNotebookPatch
    {
        internal static bool Prefix()
        {
            NullTeacher.timeSinceExcitingThing = 0f;
            return true;
        }
    }

}
