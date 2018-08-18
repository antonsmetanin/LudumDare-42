using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class BotSpeaker : MonoBehaviour
{


	public AudioSource source;
	public Vector2[] Ranges;

	private float stopTime;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (source.isPlaying && Time.time > stopTime)
			source.Stop();

	}

	public void Speak()
	{

		if(source.isPlaying)
			return;
		
		Vector2 r = Ranges[Random.Range(0, Ranges.Length)];
		source.time = r.x;
		stopTime = Time.time + r.y - r.x;
		source.Play();
	}

	public void PlayCut()
	{
		
	}
}
