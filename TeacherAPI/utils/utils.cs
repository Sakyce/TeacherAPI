using BepInEx.Logging;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeacherAPI.utils
{
    public static class Extensions
    {
        /// <summary>
        /// Convert the textures into an array of sprites
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite[] ToSprites(this Texture2D[] textures, float pixelsPerUnit)
        {
            return textures.Select(x => AssetLoader.SpriteFromTexture2D(x, pixelsPerUnit)).ToArray();
        }

        /// <summary>
        /// Used internally for debugging purposes, dont use it in production.
        /// </summary>
        /// <param></param>
        public static void Print<T>(this IEnumerable<T> array, string arrayName, ManualLogSource logger)
        {
            logger.LogInfo("Array " + arrayName + " {");
            foreach (var item in array)
            {
                logger.LogInfo("    " + item.ToString());
            }
            logger.LogInfo("}");
        }

        /// <summary>
        /// Used internally for debugging purposes, dont use it in production.
        /// </summary>
        /// <param></param>
        public static void Print<K, V>(this Dictionary<K, V> dict, string arrayName, ManualLogSource logger)
        {
            logger.LogInfo("Dictionary " + arrayName + " {");
            foreach (var item in dict)
            {
                logger.LogInfo("    " + item.Key.ToString() + " : " + item.Value.ToString());
            }
            logger.LogInfo("}");
        }

        /// <summary>
        /// Used internally for debugging purposes, dont use it in production.
        /// </summary>
        /// <param name="thing"></param>
        public static void Print(this object thing)
        {
            Debug.Log(thing ?? "null");
        }
    }

    public class SimpleEnumerator : IEnumerable
    {
        public IEnumerator enumerator;
        public Action prefixAction = () => { };
        public Action postfixAction = () => { };
        public Action<object> preItemAction = (a) => { };
        public Action<object> postItemAction = (a) => { };
        public Func<object, object> itemAction = (a) => { return a; };
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public SimpleEnumerator(IEnumerator enumerator) {
            this.enumerator = enumerator;
        }
        public IEnumerator GetEnumerator()
        {
            prefixAction();
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                preItemAction(item);
                yield return itemAction(item);
                postItemAction(item);
            }
            postfixAction();
        }
    }
}
