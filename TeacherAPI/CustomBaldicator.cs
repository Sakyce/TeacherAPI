using MTM101BaldAPI.Components;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeacherAPI
{
	internal enum BaldicatorAnim { }

	/// <summary>
	/// Note sure if it triggers issues with wide screens
	/// </summary>
	public class CustomBaldicator : MonoBehaviour
	{
		private CustomImageAnimator animator;

		private Vector3 StartingPosition = new Vector3(9, -5, 1);
		private Vector3 EndingPosition = new Vector3(12, -5, 1);

		private Image image;

		internal void Awake()
		{
			image = GetComponent<Image>();
			animator = gameObject.AddComponent<CustomImageAnimator>();
			animator.affectedObject = image;
		}

		public void ActivateBaldicator(string animationToPlay)
		{
			StartCoroutine(BaldicatorActivateAnimation(animationToPlay));
		}

		public void AddAnimation(string key, CustomAnimation<Sprite> sprites)
		{
			animator.animations.Add(key, sprites);
		}
		public void SetHearingAnimation(CustomAnimation<Sprite> sprites)
		{
			animator.animations.Add("Hearing", sprites);
		}

		public static CustomBaldicator CreateBaldicator()
		{
			var hudManager = Singleton<CoreGameManager>.Instance.GetHud(0);
			var baldiclone = Instantiate(hudManager.gameObject.transform.Find("Baldi").gameObject);
			baldiclone.name = "Custom Baldicator";
			baldiclone.transform.SetParent(hudManager.transform);
			return baldiclone.AddComponent<CustomBaldicator>();
		}

		private IEnumerator BaldicatorActivateAnimation(string animationToPlay)
		{
			animator.SetDefaultAnimation("Hearing", 1);
			for (float i = 0; i < 1; i++)
			{
				transform.position = Vector3.Lerp(StartingPosition, EndingPosition, i);
			}
			yield return new WaitForSeconds(0.5f);
			animator.SetDefaultAnimation(animationToPlay, 1);
			yield return new WaitForSeconds(0.75f);
			for (float i = 0; i < 1; i++)
			{
				transform.position = Vector3.Lerp(EndingPosition, StartingPosition, i);
			}
		}
	}
}
