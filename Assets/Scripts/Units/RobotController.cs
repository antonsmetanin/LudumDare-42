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
    }

    private void Update()
    {
        _navAgent.SetDestination(Target.position);
    }

    public override void Move(Vector2 movement)
    {
        //base.Move(movement);
    }
}
