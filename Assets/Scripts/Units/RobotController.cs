﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Model;
using UnityEngine;
using UnityEngine.AI;

public class RobotController : UnitControllerBase
{
    public float Force = 10f;
    private Transform _target;
    private NavMeshAgent _navAgent;

    public Model.Robot RobotModel;
    public Model.Game Game;

    private ProgramType[] _possiblePrograms;

    public override void Init()
    {
        base.Init();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;
        _target = null;
        _possiblePrograms = (ProgramType[])System.Enum.GetValues(typeof(ProgramType));
    }

    private bool _inProgress;
    private ProgramType? _currentProgram;
    private ProgramType? _nextProgram;

    private void Update()
    {
        if (_inProgress)
            return;

        if (!_currentProgram.HasValue)
        {
            if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Cut))
            {
                _currentProgram = ProgramType.Cut;
            }
            else if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Gather))
            {
                _currentProgram = ProgramType.Gather;
            }
            else if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Protect))
            {
                _currentProgram = ProgramType.Protect;
            }
            else if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Walk))
            {
                _currentProgram = ProgramType.Walk;
            }
        }

        switch (_currentProgram)
        {
            case ProgramType.Walk:
                StartCoroutine(Co_Walk());
                _inProgress = true;

                break;
            case ProgramType.Cut:
                if (_target == null)
                {
                    var obj = WorldObjects.Instance.GetClosestObject<Tree>(transform.position);
                    if (obj != null)
                    {
                        _target = obj.transform;
                    }

                    return;
                }

                if (Vector3.Distance(_target.position, transform.position) > 2f)
                {
                    if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Walk))
                    {
                        _currentProgram = ProgramType.Walk;
                        _nextProgram = ProgramType.Cut;
                        return;
                    }
                }
                else
                {
                    StartCoroutine(Co_Cut(_target.GetComponent<Tree>()));
                    _inProgress = true;
                }
                break;
            case ProgramType.Gather:
                if (_target == null)
                {
                    var obj = WorldObjects.Instance.GetClosestObject<TrunkPart>(transform.position);
                    if (obj != null)
                    {
                        _target = obj.transform;
                    }

                    return;
                }

                //Target.GetComponent<TrunkPart>().

                break;
            case ProgramType.Protect:
                break;
        }
    }

    private IEnumerator Co_Walk()
    {
        Vector3 targetPosition;
        if (_target == null)
        {
            targetPosition = transform.position;
            targetPosition.x += Random.Range(-10, 10);
            targetPosition.z += Random.Range(-10, 10);
        }
        else
        {
            targetPosition = _target.position;
        }

        _navAgent.nextPosition = transform.position;
        _navAgent.ResetPath();
        _navAgent.SetDestination(targetPosition);

        while ((_navAgent.pathStatus != NavMeshPathStatus.PathComplete && _navAgent.remainingDistance <= _navAgent.stoppingDistance)
            && _navAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            var direction = _navAgent.desiredVelocity.normalized;
            Move(new Vector2(direction.x, direction.z));
            _navAgent.velocity = _movable.Velocity;

            yield return null;
        }

        var carryPoint = _target?.GetComponent<CarryPoint>();
        if (carryPoint != null)
        {
            carryPoint.AcceptTrunk();
            // TODO: 
        }

        EndCoProgram();
    }

    private IEnumerator Co_Cut(Tree tree)
    {
        if (tree == null || !tree.IsAlive)
        {
            EndCoProgram();
            yield break;
        }

        var direction = tree.transform.position - transform.position;
        direction.y = 0;
        while (tree != null && tree.IsAlive)
        {
            tree.Cut(Force, direction);
            yield return new WaitForSeconds(0.2f);
        }

        EndCoProgram();
    }

    private IEnumerator Co_Gather(TrunkPart trunk)
    {
        yield return null;

        EndCoProgram();
    }

    private void EndCoProgram()
    {
        _target = null;
        _inProgress = false;
        
        if (_nextProgram.HasValue)
            _currentProgram = _nextProgram;
        else
            SelectNextProgram();

        _nextProgram = null;
    }

    private void SelectNextProgram()
    {
        var oldProgram = _currentProgram.Value;
        _currentProgram = null;
        var currentIndex = System.Array.IndexOf(_possiblePrograms, oldProgram);
        for (int i = 0; i < _possiblePrograms.Length; i++)
        {
            currentIndex = ++currentIndex % _possiblePrograms.Length;
            if (RobotModel.Programs.Any(_ => _.Template.Type == _possiblePrograms[currentIndex]))
            {
                _currentProgram = _possiblePrograms[currentIndex];
                return;
            }
        }

        if (!_currentProgram.HasValue)
            _currentProgram = oldProgram;
    }
}
