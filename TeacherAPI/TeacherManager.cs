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
		private EnvironmentController ec;
		private List<Teacher> assistingTeachers = new List<Teacher>();
		internal List<Teacher> spawnedTeachers = new List<Teacher>();
		internal System.Random controlledRng;

		public Teacher MainTeacher { get; private set; }
		internal Teacher MainTeacherPrefab { get; set; } // This will be used to SpawnNPC to HappyBaldi position
		public static TeacherManager Instance { get; private set; }
		public bool SpoopModeActivated { get; internal set; }
		public bool IsBaldiMainTeacher { get; internal set; }
		public void Initialize(EnvironmentController ec, int seed)
		{
			Instance = this;
			this.ec = ec;
            controlledRng = new System.Random(seed);
		}
		internal void SpawnHappyTeacher(Teacher teacher)
		{
			if (MainTeacher == null)
			{
				MainTeacher = teacher;
			}
		}

		public T[] GetTeachersOfType<T>() where T : Teacher
		{
			return (from teacher in spawnedTeachers
							where teacher.GetType().Equals(typeof(T))
							select (T)teacher).ToArray();
		}

		internal void CollectNotebook(Character character)
		{
			CurrentTeachersNotebooks.TryGetValue(character, out int val);
			CurrentTeachersNotebooks.Remove(character);
			CurrentTeachersNotebooks.Add(character, val + 1);

		}

		internal void DoIfMainTeacher(Action<Teacher> action)
		{
			if (MainTeacher != null)
			{
				action.Invoke(MainTeacher);
			}
		}
	}
}
