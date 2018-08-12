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

    public void SetTarget(Transform target)
    {
        _navAgent.SetDestination(target.position);

        // TODO: Animation
    }

    public void StartCut(GameObject target, float force)
    {
        // TODO: Animation, etc
    }

    public void OnCut(GameObject target, float force)
    {
        var treeTarget = target.GetComponent<Tree>();
        if (treeTarget != null)
        {
            treeTarget.Cut(force);
        }
    }
}
