using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitControllerBase : MonoBehaviour
{
    [SerializeField] private Movable _movable;

    public virtual void Init()
    {
        _movable = GetComponent<Movable>();
        if (_movable == null)
        {
            _movable = gameObject.AddComponent<Movable>();
        }
    }

    public void Move(Vector2 movement)
    {
        _movable.Move(movement);
    }
}
