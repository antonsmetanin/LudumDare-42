using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{

	public Transform follow;
	public Transform height;

	public float LErp;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector3(follow.position.x,
			Mathf.Lerp(follow.position.y, height.position.y, LErp), follow.position.z);
		
	}
}
