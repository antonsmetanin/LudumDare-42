using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDeadCondition : MonoBehaviour
{
    public event EventHandler OnDead;
    public bool IsDead;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsDead = true;
            if (OnDead != null)
                OnDead(this, EventArgs.Empty);
        }
    }
}
