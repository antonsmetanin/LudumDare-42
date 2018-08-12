using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTrunk : MonoBehaviour
{

    public GameObject[] CollectObjects;
    public bool IsDead;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsDead = true;
        }
    }
}
