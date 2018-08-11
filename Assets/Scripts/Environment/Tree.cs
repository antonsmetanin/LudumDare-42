using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IAlive
{
    [SerializeField] private float _baseHealht = 100f;
    [SerializeField] private GameObject[] _disableOnDeath;
    [SerializeField] private Rigidbody _fallingRigidbody;

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
        if (_fallingRigidbody == null)
            _fallingRigidbody = GetComponentInChildren<Rigidbody>();
        _fallingRigidbody.useGravity = true;
        _fallingRigidbody.isKinematic = false;
        var pos = transform.position;
        pos.y += 5;
        _fallingRigidbody.AddForceAtPosition(new Vector3(2, 0, 0), pos);

        for (int i = 0; _disableOnDeath != null && i < _disableOnDeath.Length; i++)
        {
            _disableOnDeath[i].SetActive(false);
        }

        do
        {
            yield return null;

            if (_fallingRigidbody.angularVelocity == Vector3.zero)
                _fallingRigidbody.AddForceAtPosition(new Vector3(2, 0, 0), pos);
        } while (!_deadCondition.IsDead);

        Debug.Log("Dead");
        IsDead = true;

        _fallingRigidbody.useGravity = false;
    }
}
