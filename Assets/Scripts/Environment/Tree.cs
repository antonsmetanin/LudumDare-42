using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IAlive
{
    [SerializeField] private float _baseHealht = 100f;
    public float Health { get; private set; }
    public bool IsAlive { get { return Health > 0; } }
    public bool IsDead { get; private set; }

    private TreeDeadCondition _deadCondition;

    private void Start()
    {
        Health = _baseHealht;
        IsDead = false;
        _deadCondition = GetComponentInChildren<TreeDeadCondition>();
    }

    public void Cut(float force)
    {
        if (!IsAlive)
            return;

        Health -= force;
        if (Health <= 0)
            StartCoroutine(Co_Dieing());
    }

    private IEnumerator Co_Dieing()
    {
        var rigidbody = GetComponent<Rigidbody>();
        var pos = transform.position;
        pos.y += 5;
        rigidbody.AddForceAtPosition(new Vector3(2, 0, 0), pos);

        do
        {
            yield return null;

            if (rigidbody.angularVelocity == Vector3.zero)
                rigidbody.AddForceAtPosition(new Vector3(2, 0, 0), pos);
        } while (!_deadCondition.IsDead);

        Debug.Log("Dead");
        IsDead = true;
    }
}
