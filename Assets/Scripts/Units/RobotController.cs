using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotController : UnitControllerBase
{
    public Transform Target;
    private NavMeshAgent _navAgent;

    public override void Init()
    {
        base.Init();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;
        _navAgent.SetDestination(Target.position);
    }

    private void Update()
    {
        
        var direction = _navAgent.desiredVelocity.normalized;
        Move(new Vector2(direction.x, direction.z));
        _navAgent.velocity = _movable.Velocity;
    }

    //public override void Move(Vector2 movement)
    //{
    //    base.Move(movement);
    //}
}
