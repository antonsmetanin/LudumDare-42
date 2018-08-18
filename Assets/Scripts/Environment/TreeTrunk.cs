using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeTrunk : MonoBehaviour
{
    [Serializable]
    public class CollectPoint
    {
        public Vector3 Point;
        public float Count;
    }

    public Joint Carrier;
    public Collider FallCollider;
    public Collider InteractionCollider;
    public Rigidbody Rigidbody;
    
    public bool IsDead;
    public bool IsCarring;
    public bool IsLoaded;
    public bool IsRecycling;

    public float Wood = 50;

    public Action<TreeTrunk> RecycleAction;


    private void Start()
    {
        Wood *= transform.lossyScale.x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsDead = true;
            InteractionCollider.enabled = true;
            FallCollider.enabled = false;
        }
    }

    public void Carry(Joint carrier)
    {
        Carrier = carrier;
        Carrier.connectedBody = Rigidbody;
        IsCarring = carrier != null;
        InteractionCollider.enabled = !IsCarring;
    }

    public void Drop()
    {
        if (Carrier != null)
        {
            Carrier.connectedBody = null;
            Carrier = null;
        }
        
        IsCarring = Carrier != null;
        InteractionCollider.enabled = !IsCarring;

    }

    public CollectPoint GetCollectPoint()
    {
        return null;
    }

    public Vector3 GetCollectPointPosition(CollectPoint cp)
    {
        return transform.position + cp.Point;
    }

    public float Collect(CollectPoint cp, float count)
    {
        var value = Mathf.Min(count, cp.Count);
        cp.Count -= value;
        return value;
    }

    public void Recycle(Ark ark)
    {
        IsRecycling = true;
        InteractionCollider.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("InvisibleTree");
        InteractionCollider.gameObject.layer = LayerMask.NameToLayer("InvisibleTree");
        Drop();
        if (RecycleAction != null)
            RecycleAction(this);

        StartCoroutine(Co_Recycling());

    }

    IEnumerator Co_Recycling()
    {
        var forceAnimation = Time.time + 2;
        while (true)
        {
            yield return null;

            if(Time.time > forceAnimation || Rigidbody.IsSleeping())
                break;
        }

        float animationLn = 3;
        Destroy(Rigidbody);

        if (RecyclingStarted != null)
            RecyclingStarted(this, animationLn);

        var animationEnd = Time.time + animationLn;
        while (Time.time < animationEnd)
        {
            transform.position += Vector3.down * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public Action<TreeTrunk, float> RecyclingStarted;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.magenta;
    //    foreach (var cp in CollectPoints)
    //    {
    //        Gizmos.DrawWireSphere(transform.position + cp.Point, 0.1f);
    //    }
    //}
}
