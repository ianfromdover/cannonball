using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.LightshipHub.Tools
{
	public class Switch : MonoBehaviour
	{
		public bool IsOn;
		[SerializeField]
		private Image circle, background;
		[SerializeField]
		private Color offColor, onColor;
		private RectTransform circleRT;

		void Awake()
		{
			circleRT = circle.gameObject.GetComponent<RectTransform>();
			UpdateButton();
		}
		public void OnTap() 
		{
			IsOn = !IsOn;
			UpdateButton();
		}

		private void UpdateButton()
		{
			circleRT.pivot = IsOn ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
			circleRT.anchorMin = IsOn ? new Vector2(1, 0) : new Vector2(0, 0);
			circleRT.anchorMax = IsOn ? new Vector2(1, 1) : new Vector2(0, 1);
			background.color = IsOn ? onColor : offColor;
		}
			
	}
}
