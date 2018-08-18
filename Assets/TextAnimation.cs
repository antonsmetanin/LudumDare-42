using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimation : MonoBehaviour
{
	[SerializeField] private RectTransform rect;
	[SerializeField] private Text text;

	private float animationTime;

	private float endtime;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (text.enabled)
		{
			float normalTime = 1 - (endtime - Time.time) / animationTime;
			rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, normalTime);
		
			if (normalTime > 1)
				text.CrossFadeAlpha(0, 1, false);

			if (normalTime > 1.4f)
				text.enabled = false;
		}
	}

	public void Play(float t)
	{
		animationTime = t;
		
		rect.localScale = Vector3.one;
		endtime = Time.time + t;
		text.color = Color.white;
		text.enabled = true;
		text.CrossFadeAlpha(1, 1, false);

	}
}
