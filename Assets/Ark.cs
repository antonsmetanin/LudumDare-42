using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ark : MonoBehaviour
{

	public GameObject[] Stages;
	public BoxCollider LoadingArea;

	private float _perStage;
	// Use this for initialization
	void Start ()
	{
		_perStage = 1f / (Stages.Length - 1);
		UpdateStage(0);
	}

	public void UpdateStage(float f)
	{
		int stage = (int)(f / _perStage);

		stage = Mathf.Min(Stages.Length - 1, stage);

		for (int i = 0; i < Stages.Length; i++)
			Stages[i].SetActive(i == stage);
	}
}
