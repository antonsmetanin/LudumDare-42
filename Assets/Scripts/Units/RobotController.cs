using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Data;
using Model;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

public class RobotController : UnitControllerBase
{
    private Transform _target;
    [SerializeField] private NavMeshAgent _navAgent;

    public Model.Robot RobotModel;
    public Model.Game Game;

    private ProgramType[] _possiblePrograms;
    private Dictionary<ProgramType, Coroutine> _programCou = new Dictionary<ProgramType, Coroutine>();
    public Animator Animator;

    private string _walkStateName = "walk";
    private string _dragStateName = "drag";
    private string _cutStateName = "cut";

    public Joint Joint;
    public float RotationSpeed;

    private bool _inProgress;
    private ProgramType? _currentProgram;
    private ProgramType? _nextProgram;

    private float _executeTime;
    private int _nextStep;

    public float SyncInterval = 1f; 
    private float _syncTime;
    private bool _hasSyncState;

    public BotSpeaker Speaker;

    public override void Init()
    {
        base.Init();

        _navAgent.updatePosition = false;
        _navAgent.updateRotation = false;
        _target = null;
        _possiblePrograms = (ProgramType[])System.Enum.GetValues(typeof(ProgramType));

        RobotModel.Programs.ObserveAdd().Subscribe(addEvent =>
        {
            var newType = addEvent.Value.Template.Type;
            if (newType == ProgramType.Sync)
            {
                if (!_hasSyncState)
                    _hasSyncState = true;
                return;
            }

            if (!_currentProgram.HasValue)
                return;
            
            switch (newType)
            {
                case ProgramType.Protect:
                case ProgramType.Gather:
                case ProgramType.Cut:

                    if (_currentProgram.Value == ProgramType.Walk)
                    {
                        Coroutine cou;
                        if (_inProgress && _programCou.TryGetValue(_currentProgram.Value, out cou) && cou != null)
                        {

                            StopCoroutine(cou);
                            EndCoProgram();
                        }

                        _currentProgram = null;
                    }
                    break;
            }
        });

        RobotModel.Programs.ObserveRemove().Subscribe(removeEvent =>
        {
            if (!_currentProgram.HasValue)
                return;

            if (removeEvent.Value.Template.Type == ProgramType.Sync)
            {
                if (_hasSyncState && !RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Sync))
                    _hasSyncState = false;
            }
            else
            {
                Coroutine cou;
                if (_inProgress && _programCou.TryGetValue(_currentProgram.Value, out cou) && cou != null)
                {

                    StopCoroutine(cou);
                    EndCoProgram();
                }
                _currentProgram = null;
            }
        });
    }

    private void Steer(Vector2 move, bool backwards = false)
    {
        Move(move);
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.LookRotation(backwards ? new Vector3(-move.x, 0, -move.y) : new Vector3(move.x, 0, move.y), Vector3.up), Time.deltaTime * RotationSpeed);
    }

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
                
                Animator.SetBool("off", true);
            }

            _currentProgram = null;
            return;
        }
        
        if (_hasSyncState && _syncTime + SyncInterval < Time.time)
        {
            // TODO Sync

            _syncTime = Time.time;
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
        
        Animator.SetBool("off", !_currentProgram.HasValue);

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

                if (Vector3.Distance(_target.position, transform.position) > 3f)
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

    public IEnumerator CO_Spawn(Vector3 targetPosition)
    {
        Speaker.Speak();

        Animator.SetBool(_walkStateName, true);
        Animator.SetBool("off", true);

        _navAgent.nextPosition = transform.position;
        _navAgent.ResetPath();
        _navAgent.SetDestination(targetPosition);
        
        yield return null;
        ResetTime();

        while ((_navAgent.pathStatus != NavMeshPathStatus.PathComplete || _navAgent.remainingDistance > _navAgent.stoppingDistance)
               && _navAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            var direction = _navAgent.desiredVelocity.normalized;
            var move = new Vector2(direction.x, direction.z);
            Steer(move);
            _navAgent.velocity = _movable.Velocity;

            ComputeTime(Time.deltaTime, ProgramType.Walk);

            yield return null;
        }
        
        Animator.SetBool(_walkStateName, false);
        Speaker.Speak();

    }


    private IEnumerator Co_Walk()
    {
        Animator.SetBool(_walkStateName, true);
        if (!RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Walk))
        {
            EndCoProgram();
            yield break;
        }

        Speaker.Speak();

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
            var move = new Vector2(direction.x, direction.z);
            Steer(move);
            _navAgent.velocity = _movable.Velocity;

            ComputeTime(Time.deltaTime, ProgramType.Walk);

            yield return null;
        }


        EndCoProgram();
    }

    private Vector3 GetPointOnGround()
    {
        var targetPosition = transform.position;
        targetPosition.x += Random.Range(-5, 5);
        targetPosition.z += Random.Range(-5, 5);
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

        Speaker.Speak();

        
        var direction = tree.transform.position - transform.position;
        direction.y = 0;
        ResetTime();
        while (tree != null && tree.IsAlive)
        {
            Animator.SetBool(_cutStateName, true);
            
            yield return new WaitForSeconds(CutHitTime);
            tree.Cut(CutStr, direction);
            ComputeTime(Time.deltaTime, ProgramType.Cut);
            
            Animator.SetBool(_cutStateName, false);
            yield return new WaitForSeconds(CutDelay);
            Speaker.Speak();
        }

        EndCoProgram();
    }
    
    [SerializeField] private float CutDelay = 1;
    [SerializeField] private float CutHitTime = .1f;
    [SerializeField] private float CutStr = 10;

    private IEnumerator Co_Gather(TreeTrunk trunk)
    {
        
        Speaker.Speak();

        
        _isLoadPointSet = false;
        Animator.SetBool(_dragStateName, true);

        if (!RobotModel.Programs.Any(_ => _.Template.Type == ProgramType.Gather))
        {
            EndCoProgram();
            yield break;
        }

        if (trunk == null || trunk.IsCarring)
        {
            EndCoProgram();
            yield break;
        }

        trunk.IsCarring = true;

        yield return null;

        trunk.Carry(Joint);

        var ark = WorldObjects.Instance.GetFirstItem<Ark>();
        _navAgent.nextPosition = transform.position;
        _navAgent.ResetPath();
        _navAgent.SetDestination(ark.transform.position);

        yield return null;
        ResetTime();

        Animator.SetBool(_walkStateName, true);
        
        while (!trunk.IsRecycling && _navAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            var direction = _navAgent.desiredVelocity.normalized;
            Steer(new Vector2(direction.x, direction.z), true);
            _navAgent.velocity = _movable.Velocity;

            ComputeTime(Time.deltaTime, ProgramType.Gather);

            yield return null;
        }

        Joint.connectedBody = null;

        EndCoProgram();
    }

    private bool _isLoadPointSet;

    private void OnTriggerEnter(Collider other)
    {
        if (_isLoadPointSet)
            return;
        var ark = other.GetComponentInParent<Ark>();
        if (ark == null)
            return;

        _isLoadPointSet = true;
        var center = other.bounds.center;
        center.y = transform.position.y;
        var resultPoint = transform.position - 1.5f * (transform.position - center);
        _navAgent.SetDestination(resultPoint);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_navAgent.destination, 0.1f);
    }

    private void EndCoProgram()
    {
        
        Speaker.Speak();

        
        Animator.SetBool(_walkStateName, false);
        Animator.SetBool(_cutStateName, false);
        Animator.SetBool(_dragStateName, false);
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
        
        Speaker.Speak();

        
        var oldProgram = _currentProgram;
        _currentProgram = null;
        var currentIndex = oldProgram.HasValue ? System.Array.IndexOf(_possiblePrograms, oldProgram) : 0;
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
