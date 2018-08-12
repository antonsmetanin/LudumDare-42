﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotController : UnitControllerBase
{
    public Transform Target;
    private NavMeshAgent _navAgent;
    private bool _reached;

    public override void Init()
    {
        base.Init();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;
        SetTarget(Target);
    }

    private bool _started;

    private void Update()
    {
        if (!_reached)
        {
            if (_navAgent.pathStatus == NavMeshPathStatus.PathComplete && _navAgent.remainingDistance <= _navAgent.stoppingDistance)
            {
                _reached = true;
                _started = false;
                return;
            }

            var direction = _navAgent.desiredVelocity.normalized;
            Move(new Vector2(direction.x, direction.z));
            _navAgent.velocity = _movable.Velocity;
        }
        else if (!_started)
        {
            StartCut(Target.gameObject, 10);
        }
    }

    public void SetTarget(Transform target)
    {
        _reached = false;
        _navAgent.SetDestination(target.position);

        // TODO: Animation
    }

    public void StartCut(GameObject target, float force)
    {
        _started = true;
        var treeTarget = target.GetComponent<Tree>();
        if (treeTarget != null)
            StartCoroutine(Co_Cut(treeTarget, force));
        else
            OnCut(target, force);

        // TODO: Animation, etc
    }

    private IEnumerator Co_Cut(Tree target, float force)
    {
        if (target == null || !target.IsAlive)
            yield break;

        var direction = target.transform.position - transform.position;
        direction.y = 0;
        while (target != null && target.IsAlive)
        {
            target.Cut(force, direction);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnCut(GameObject target, float force)
    {
        var treeTarget = target.GetComponent<Tree>();
        if (treeTarget != null)
        {
            //treeTarget.Cut(force);
        }
    }
}
