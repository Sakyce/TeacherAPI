using HarmonyLib;
using System.Linq;
using TeacherAPI.utils;

namespace TeacherAPI.patches
{
    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.GetBaldi))]
    [HarmonyPriority(Priority.First)]
    internal class GetBaldiPatch
    {
        public static void Postfix(EnvironmentController __instance, ref Baldi __result)
        {
            if (__result == null)
            {
                foreach (NPC npc in __instance.npcs)
                {
                    if (npc.IsTeacher())
                    {
                        var fakeBaldi = TeacherPlugin.ConvertTeacherToBaldi((Baldi)npc);
                        __result = fakeBaldi;
                        break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.Initialize))]
    internal class CleanupSpawnedTeachersPatch
    {
        internal static bool Prefix()
        {
            TeacherPlugin.Instance.spawnedTeachers.Clear();
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.BeginSpoopMode))]
    internal class BeginSpoopmodePatch
    {
        internal static bool Prefix()
        {
            TeacherPlugin.Instance.SpoopModeEnabled = true;
            return true;
        }
    }

    internal class ReplaceHappyBaldiWithTeacherPatch
    {
        internal static void ReplaceHappyBaldi(BaseGameManager __instance)
        {
            var customTeacherFound = false;
            var happyBaldi = __instance.Ec.gameObject.GetComponentInChildren<HappyBaldi>();

            var level = Singleton<BaseGameManager>.Instance.levelObject;
            (from x in level.potentialBaldis select $"{x.selection.name} (weight: {x.weight})").Print($"Potential Baldis of {level.name}", TeacherPlugin.Log);
            TeacherPlugin.Instance.currentBaldi = TeacherPlugin.Instance.GetPotentialBaldi(level);
            TeacherPlugin.Instance.spawnedTeachers.Clear();
            TeacherPlugin.Instance.SpoopModeEnabled = false;

            // Spawn all teachers
            for (int i = 0; i < __instance.Ec.npcsToSpawn.Count; i++)
            {
                var npc = __instance.Ec.npcsToSpawn[i];
                if (npc.IsTeacher())
                {
                    customTeacherFound = true;
                    __instance.Ec.SpawnNPC(npc, __instance.Ec.CellFromPosition(happyBaldi.transform.position).position);
                    __instance.Ec.npcsToSpawn.RemoveAt(i);
                    __instance.Ec.npcSpawnTile = __instance.Ec.npcSpawnTile.Where((c, ix) => ix != i).ToArray();
                };
            }
            TeacherPlugin.Instance.spawnedTeachers.Print("Spawned Teachers", TeacherPlugin.Log);

            if (customTeacherFound)
            {
                happyBaldi.sprite.enabled = false;
                happyBaldi.audMan.enabled = false;
                Singleton<MusicManager>.Instance.StopMidi();
            }
        }

        [HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.CreateHappyBaldi))]
        internal class InMainGameManager
        {
            internal static void Postfix(MainGameManager __instance)
            {
                ReplaceHappyBaldi(__instance);
            }
        }

        [HarmonyPatch(typeof(EndlessGameManager), nameof(EndlessGameManager.CreateHappyBaldi))]
        internal class InEndlessGameManager
        {
            internal static void Postfix(EndlessGameManager __instance)
            {
                ReplaceHappyBaldi(__instance);
            }
        }
    }


    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.AngerBaldi))]
    internal class AngerBaldiPatch
    {
        public static bool Prefix(ref float val)
        {
            foreach (Teacher teacher in TeacherPlugin.Instance.spawnedTeachers)
            {
                teacher.GetAngry(val);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RulerEvent), nameof(RulerEvent.Begin))]
    internal class BreakRulerPatch
    {
        public static void Postfix()
        {
            foreach (Teacher teacher in TeacherPlugin.Instance.spawnedTeachers)
            {
                teacher.BreakRuler();
            }
        }
    }

    [HarmonyPatch(typeof(RulerEvent), nameof(RulerEvent.End))]
    internal class RestoreRulerPatch
    {
        public static void Postfix()
        {
            foreach (Teacher teacher in TeacherPlugin.Instance.spawnedTeachers)
            {
                teacher.RestoreRuler();
            }
        }
    }
}
