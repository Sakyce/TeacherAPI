using HarmonyLib;
using System;
using System.Linq;
using TeacherAPI.utils;
using UnityEngine;

namespace TeacherAPI
{
    public static class Extensions
    {
        /// <summary>
        /// Get every teachers in this EnvironmentController.
        /// </summary>
        /// <param name="ec"></param>
        /// <returns></returns>
        public static Teacher[] GetTeachers(this EnvironmentController ec)
        {
            return (from npc in ec.npcs where npc.IsTeacher() select (Teacher)npc).ToArray();
        }

        /// <summary>
        /// Check if the NPC is registered as a Teacher in TeacherAPI
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static bool IsTeacher(this NPC npc)
        {
            return TeacherPlugin.Instance.whoAreTeachers.ContainsKey(npc.Character);
        }

        /// <summary>
        /// Am I crazy ?
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static PromiseLike<TeacherState> AsTeacherState(this NpcState state)
        {
            var promise = new PromiseLike<TeacherState>();
            try
            {
                var teacherstate = (TeacherState)state;
                promise.Resolve(teacherstate);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Handled InvalidCastException : Tried to cast {state} to Teacher State");
                promise.Fail(ex);
            }
            return promise;
        }

        /// <summary>
        /// Adds your teacher into the pool of potentialBaldis in the LevelObject.
        /// </summary>
        /// <param name="levelObject"></param>
        /// <param name="teacher">The teacher to be added</param>
        /// <param name="weight">The weight of the teacher for the selection (as a reference, MoreTeachers default teachers have a weight of 100)</param>
        public static void AddPotentialTeacher(this LevelObject levelObject, NPC teacher, int weight)
        {
            levelObject.potentialBaldis = levelObject.potentialBaldis.AddItem(new WeightedNPC() { selection = teacher, weight = weight }).ToArray();
        }
    }
}
