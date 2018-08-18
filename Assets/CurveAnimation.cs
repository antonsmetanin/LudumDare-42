using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveAnimation : MonoBehaviour
{


	public AnimationCurve[] rotations;
	
	void Update () {
		transform.localRotation = Quaternion.Euler(
			rotations[0].Evaluate(Time.time),
			rotations[1].Evaluate(Time.time),
			rotations[2].Evaluate(Time.time));
	}
}
