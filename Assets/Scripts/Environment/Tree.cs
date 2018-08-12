using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IAlive
{
    [SerializeField] private float _baseHealht = 100f;
    [SerializeField] private GameObject[] _disableOnDeath;
    [SerializeField] private Rigidbody _fallingRigidbody;
    [SerializeField] private Vector3 _forceApplyPoint = new Vector3(0, 0, 3);

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

    public void Cut(float force, Vector3 direction)
    {
        if (!IsAlive)
            return;

        Health -= force;
        if (Health <= 0)
            StartCoroutine(Co_Dieing(direction));
    }

    private IEnumerator Co_Dieing(Vector3 direction)
    {
        if (_fallingRigidbody == null)
            _fallingRigidbody = GetComponentInChildren<Rigidbody>();
        _fallingRigidbody.useGravity = true;
        _fallingRigidbody.isKinematic = false;
        _fallingRigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;

        Debug.Log(direction.normalized);
        _fallingRigidbody.AddForceAtPosition(direction.normalized, transform.position + transform.rotation * _forceApplyPoint, ForceMode.VelocityChange);

        for (int i = 0; _disableOnDeath != null && i < _disableOnDeath.Length; i++)
        {
            _disableOnDeath[i].SetActive(false);
        }

        do
        {
            yield return null;

            _fallingRigidbody.constraints = RigidbodyConstraints.None;

        } while (!_deadCondition.IsDead);

        Debug.Log("Dead");
        IsDead = true;
    }
}
