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

    public Collider FallCollider;
    public Collider InteractionCollider;
    public CollectPoint[] CollectPoints;
    public GameObject[] Parts;
    public bool IsDead;
    public bool IsCarring;
    public bool IsLoaded;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsDead = true;
            InteractionCollider.enabled = true;
            FallCollider.enabled = false;
            /*for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].transform.SetParent(null);
                Parts[i].SetActive(true);
                Parts[i].GetComponent<Rigidbody>().AddForce(Vector3.up * 5, ForceMode.VelocityChange);
            }

            gameObject.SetActive(false);*/
        }
    }

    public CollectPoint GetCollectPoint()
    {
        return CollectPoints.FirstOrDefault(_ => _.Count > 0);
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

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.magenta;
    //    foreach (var cp in CollectPoints)
    //    {
    //        Gizmos.DrawWireSphere(transform.position + cp.Point, 0.1f);
    //    }
    //}
}
