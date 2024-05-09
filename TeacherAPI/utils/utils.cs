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
        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit)
        {
            return AssetLoader.SpriteFromTexture2D(texture, pixelsPerUnit);
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

        public static void PrintWeights<T>(this IEnumerable<WeightedSelection<T>> w, string label, ManualLogSource logger)
        {
            w.Select(x => $"{x.selection} {x.weight}").Print(label, logger);
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
