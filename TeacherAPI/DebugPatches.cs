using HarmonyLib;
using UnityEngine;
using System.Collections;
using TeacherAPI.utils;
using System;
using UnityEngine.SceneManagement;

namespace TeacherAPI.debug
{
#if DEBUG

    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.NPCSpawner))]
    public static class InstantMap
    {
        static void Postfix(EnvironmentController __instance, ref IEnumerator __result)
        {
            void postfixAction()
            {
                foreach (var npc in __instance.npcs)
                {
                    __instance.map.AddArrow(npc.transform, Color.red);
                }
            }

            void prefixAction()
            {
                __instance.map.CompleteMap();
            }

            __result = new SimpleEnumerator() { enumerator = __result, postfixAction = postfixAction, prefixAction = prefixAction }.GetEnumerator();
        }
    }

    public static class FastPlayer
    {
        public static void Play(MonoBehaviour behaviour)
        {
            behaviour.StartCoroutine(DelayGoToMainMenu(behaviour));
        }
        private static IEnumerator DelayGoToMainMenu(MonoBehaviour behaviour)
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadSceneAsync("MainMenu").completed += (AsyncOperation _) => behaviour.StartCoroutine(DelayGoToGame());

        }
        private static IEnumerator DelayGoToGame()
        {
            yield return new WaitForSeconds(0.1f);
            GameLoader loader = null;
            while (loader == null)
            {
                loader = GameObject.FindObjectOfType<GameLoader>(includeInactive: true);
                yield return null;
            }
            loader.gameObject.SetActive(true);
            loader.Initialize(2);
            loader.LoadLevel(loader.list.scenes[0]);
        }
    }
    [HarmonyPatch(typeof(GameInitializer), nameof(GameInitializer.WaitForGenerator))]
    class FastBeginPlayPatch
    {

        static void Postfix(ref IEnumerator __result)
        {
            void prefixAction() { }
            void postfixAction()
            {
                Singleton<BaseGameManager>.Instance.BeginPlay();
            }
            void preItemAction(object item) { }
            void postItemAction(object item) { }
            object itemAction(object item) { return item; }
            var myEnumerator = new SimpleEnumerator()
            {
                enumerator = __result,
                prefixAction = prefixAction,
                postfixAction = postfixAction,
                preItemAction = preItemAction,
                postItemAction = postItemAction,
                itemAction = itemAction
            };
            __result = myEnumerator.GetEnumerator();
        }
    }
#endif
}
