using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeacherAPI
{
	public class TeacherManager : MonoBehaviour
	{
		internal Dictionary<Character, int> MaxTeachersNotebooks = new Dictionary<Character, int>();
		internal Dictionary<Character, int> CurrentTeachersNotebooks = new Dictionary<Character, int>();

		internal List<Teacher> spawnedTeachers = new List<Teacher>(); // Every teachers spawned
		public Teacher SpawnedMainTeacher { get; internal set; } // First teacher that has spawned
		internal Teacher MainTeacherPrefab { get; set; } // This will be used to SpawnNPC to HappyBaldi position
		
		internal List<Teacher> assistingTeachersPrefabs = new List<Teacher>(); // Used only for the notebooks rn

        internal System.Random controlledRng;
		
		public static bool DefaultBaldiEnabled { get; internal set; }
		public static TeacherManager Instance { get; private set; }
		public bool SpoopModeActivated { get; internal set; }
		public bool IsBaldiMainTeacher { get; internal set; }
		public EnvironmentController Ec { get; private set; }
		public void Initialize(EnvironmentController ec, int seed)
		{
			Instance = this;
			Ec = ec;
            controlledRng = new System.Random(seed);
		}

		public T[] GetTeachersOfType<T>() where T : Teacher
		{
			return (from teacher in spawnedTeachers
							where teacher.GetType().Equals(typeof(T))
							select (T)teacher).ToArray();
		}

		internal void DoIfMainTeacher(Action<Teacher> action)
		{
			if (SpawnedMainTeacher != null)
			{
				action.Invoke(SpawnedMainTeacher);
			}
		}

		public bool ContainsRealBaldi()
		{
            foreach (var baldi in Ec.npcs)
            {
                if (baldi.Character == Character.Baldi)
                {
                    return true;
                }
            }
            foreach (var baldi in Ec.npcsToSpawn)
            {
                if (baldi.Character == Character.Baldi)
                {
                    return true;
                }
            }
            return false;
        }
		public Baldi GetRealBaldi()
		{
            foreach (var baldi in Ec.npcs)
            {
                if (baldi.Character == Character.Baldi)
				{
					return (Baldi)baldi;
				}
            }
			return null;
        }
	}
}
