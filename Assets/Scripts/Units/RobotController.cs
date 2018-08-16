using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Model;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

public class RobotController : UnitControllerBase
{
    public float Force = 10f;
    private Transform _target;
    private NavMeshAgent _navAgent;

    public Model.Robot RobotModel;
    public Model.Game Game;

    private ProgramType[] _possiblePrograms;
    private Dictionary<ProgramType, Coroutine> _programCou = new Dictionary<ProgramType, Coroutine>();
    public Animator Animator;

    private string _walkStateName = "walk";
    private string _dragStateName = "drag";
    private string _cutStateName = "cut";

    public override void Init()
    {
        base.Init();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;
        _target = null;
        _possiblePrograms = (ProgramType[])System.Enum.GetValues(typeof(ProgramType));
        
        RobotModel.Programs.ObserveRemove().Subscribe(removeEvent =>
        {
            if (!_currentProgram.HasValue)
                return;

            Coroutine cou;
            if (_inProgress && _programCou.TryGetValue(_currentProgram.Value, out cou) && cou != null)
            {

                StopCoroutine(cou);
                EndCoProgram();
            }
            _currentProgram = null;
        });
    }

    private bool _inProgress;
    private ProgramType? _currentProgram;
    private ProgramType? _nextProgram;

    private float _executeTime;
    private int _nextStep;

    private void Update()
    {
        if (RobotModel.Broken.Value)
        {
            Debug.Log("Broken");
            Coroutine cou;
            if (_inProgress && _currentProgram.HasValue && _programCou.TryGetValue(_currentProgram.Value, out cou) && cou != null)
            {
                StopCoroutine(cou);
                EndCoProgram();
            }

            _currentProgram = null;
            return;
        }

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
                var cou = StartCoroutine(Co_Walk());
                _programCou[ProgramType.Walk] = cou;
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
                    else
                    {
                        StartCoroutine(Co_Wait(0.5f));
                        _inProgress = true;
                    }

                    return;
                }

                if (Vector3.Distance(_target.position, transform.position) > 5f)
                {
                    if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Walk))
                    {
                        _currentProgram = ProgramType.Walk;
                        _nextProgram = ProgramType.Cut;
                        return;
                    }
                    else
                    {
                        StartCoroutine(Co_Wait(0.5f));
                        _inProgress = true;
                    }
                }
                else
                {
                    var couCut = StartCoroutine(Co_Cut(_target.GetComponent<Tree>()));
                    _programCou[ProgramType.Cut] = couCut;
                    _inProgress = true;
                }
                break;
            case ProgramType.Gather:
                if (_target?.GetComponent<TreeTrunk>() == null)
                {
                    var obj = WorldObjects.Instance.GetClosestObject<TreeTrunk>(transform.position);
                    if (obj != null)
                    {
                        _target = obj.transform;
                    }
                    else
                    {
                        StartCoroutine(Co_Wait(0.5f));
                        _inProgress = true;
                    }

                    return;
                }

                if (Vector3.Distance(_target.position, transform.position) > 2f)
                {
                    if (RobotModel.Programs.Any(x => x.Template.Type == ProgramType.Walk))
                    {
                        _target = _target.GetComponent<TreeTrunk>().InteractionCollider.transform;
                        _currentProgram = ProgramType.Walk;
                        _nextProgram = ProgramType.Gather;
                        return;
                    }
                    else
                    {
                        
                        StartCoroutine(Co_Wait(0.5f));
                        _inProgress = true;
                    }
                }
                else
                {
                    var couGather = StartCoroutine(Co_Gather(_target.GetComponent<TreeTrunk>()));
                    _programCou[ProgramType.Cut] = couGather;
                    _inProgress = true;
                }

                break;
            case ProgramType.Protect:
                break;
        }
    }

    private IEnumerator Co_Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        EndCoProgram();
    }

    private IEnumerator Co_Walk()
    {
        Animator.SetBool(_walkStateName, _currentProgram == ProgramType.Walk);
        if (!RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Walk))
        {
            EndCoProgram();
            yield break;
        }

        Vector3 targetPosition;
        if (_target == null)
        {
            targetPosition = GetPointOnGround();
        }
        else
        {
            targetPosition = _target.position;
        }

        _navAgent.nextPosition = transform.position;
        _navAgent.ResetPath();
        _navAgent.SetDestination(targetPosition);

        yield return null;
        ResetTime();

        while ((_navAgent.pathStatus != NavMeshPathStatus.PathComplete || _navAgent.remainingDistance > _navAgent.stoppingDistance)
            && _navAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            var direction = _navAgent.desiredVelocity.normalized;
            var m = new Vector2(direction.x, direction.z);
            Move(new Vector2(direction.x, direction.z));
            transform.LookAt(transform.position + new Vector3(direction.x, 0, direction.z));
            _navAgent.velocity = _movable.Velocity;

            ComputeTime(Time.deltaTime, ProgramType.Walk);

            yield return null;
        }

        var carryPoint = _target?.GetComponent<Ark>();
        if (carryPoint != null)
        {
            //carryPoint.AcceptTrunk();
            // TODO:
        }

        EndCoProgram();
    }

    private Vector3 GetPointOnGround()
    {
        var targetPosition = transform.position;
        targetPosition.x += Random.Range(-50, 50);
        targetPosition.z += Random.Range(-50, 50);
        targetPosition.y += 2;

        RaycastHit hit;
        if (Physics.Raycast(targetPosition, Vector3.down, out hit, 5f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }

        return transform.position;
    }

    private IEnumerator Co_Cut(Tree tree)
    {
        Animator.SetBool(_cutStateName, _currentProgram == ProgramType.Cut);

        if (!RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Cut))
        {
            EndCoProgram();
            yield break;
        }

        if (tree == null || !tree.IsAlive)
        {
            EndCoProgram();
            yield break;
        }

        var direction = tree.transform.position - transform.position;
        direction.y = 0;
        ResetTime();
        while (tree != null && tree.IsAlive)
        {
            tree.Cut(10, direction);
            ComputeTime(Time.deltaTime, ProgramType.Walk);
            yield return new WaitForSeconds(0.1f);
        }

        EndCoProgram();
    }

    private IEnumerator Co_Gather(TreeTrunk trunk)
    {
        Animator.SetBool(_dragStateName, _currentProgram == ProgramType.Gather);

        if (!RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Gather))
        {
            EndCoProgram();
            yield break;
        }

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
                if (_possiblePrograms[currentIndex] == ProgramType.Walk && RobotModel.Programs.Any(_ => _.Template.Type != ProgramType.Walk))
                    continue;
                _currentProgram = _possiblePrograms[currentIndex];
                return;
            }
        }

        if (!_currentProgram.HasValue)
            _currentProgram = oldProgram;
    }

    private void ResetTime()
    {
        _executeTime = 0;
        _nextStep = 1;
    }

    private void ComputeTime(float deltaTime, ProgramType program)
    {
        _executeTime += deltaTime;
        int intTime = (int)_executeTime;
        intTime -= _nextStep;
        if (intTime > 0)
        {
            var currProgram = RobotModel.Programs.FirstOrDefault(_ => _.Template.Type == program);
            while (intTime-- > 0)
            {
                currProgram?.ExecuteOneSecond();
                _nextStep++;
            }
        }
    }
}
