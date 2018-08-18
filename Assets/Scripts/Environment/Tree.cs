using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IAlive
{
    public event EventHandler OnDead;

    [SerializeField] private float _baseHealht = 100f;
    [SerializeField] private GameObject[] _disableOnDeath;
    [SerializeField] private Rigidbody _fallingRigidbody;
    [SerializeField] private Vector3 _forceApplyPoint = new Vector3(0, 0, 3);

    private float _health;
    public float Health
    {
        get { return _health; }
        private set
        {
            _health = value;
            if (_health <= 0f)
                IsAlive = false;
        }
    }
    public bool IsAlive { get; private set; } = true;
    public bool IsDead { get; private set; }

    private Vector3 _fallDirection;

    private TreeTrunk _deadCondition;

    private void Awake()
    {
        Health = _baseHealht * transform.lossyScale.x;
        IsDead = false;
    }

    private void Start()
    {
        _deadCondition = GetComponentInChildren<TreeTrunk>();

        if (_fallingRigidbody == null)
            _fallingRigidbody = GetComponentInChildren<Rigidbody>();

        if (_fallingRigidbody == null)
            return;

        // TODO: поиграться с центром масс для более реалистичного падения.
        //var centerOfMass = _fallingRigidbody.centerOfMass;
        //centerOfMass.z -= 1;
        //_fallingRigidbody.centerOfMass = centerOfMass;
    }

    public void Cut(float force, Vector3 direction)
    {
        if (!IsAlive)
            return;

        WorldObjects.Instance.CutEffectAt(_disableOnDeath[0].transform.position, -direction);

        Health -= force;
        var dir = direction.normalized;
        _fallDirection += dir * force;
        if (!IsAlive)
        {
            if (Mathf.Approximately(_fallDirection.sqrMagnitude, 0f))
                _fallDirection = dir;
            StartCoroutine(Co_Dieing(_fallDirection.normalized));
        }
    }

    private IEnumerator Co_Dieing(Vector3 direction)
    {
        if (_fallingRigidbody == null)
            _fallingRigidbody = GetComponentInChildren<Rigidbody>();
        _fallingRigidbody.useGravity = true;
        _fallingRigidbody.isKinematic = false;
        _fallingRigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;

        _fallingRigidbody.AddForceAtPosition(-direction.normalized * 60, transform.position + Vector3.up * 6, ForceMode.Impulse);


        for (int i = 0; _disableOnDeath != null && i < _disableOnDeath.Length; i++)
        {
            _disableOnDeath[i].SetActive(false);
        }

        do
        {
            yield return null;

            _fallingRigidbody.constraints = RigidbodyConstraints.None;

        } while (!_deadCondition.IsDead);

        IsDead = true;
        OnDead?.Invoke(this, EventArgs.Empty);
    }
}
