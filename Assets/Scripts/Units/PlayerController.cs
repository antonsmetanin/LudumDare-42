using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitControllerBase
{
    public Animator Animator;

    [Header("Movement")]
    public float RotationSpeed;
    public float SpeedDrag = .04f;
    public float SpeedNormal = .1f;

    [Header("Axe")]
    public float CutRadius = 1f;
    public float CutTime = .1f;
    public float CutCooldown = .3f;
    public float CutForce = 30f;

    [Header("Interaction")]
    public Joint Joint;
    public SphereCollider InteractionCollider;
    public LayerMask InteractionLayer;
    public bool InteractionAvailable =  false;

    private Coroutine cut;
    private Collider[] _interactive = new Collider[3];
    private bool _drag;

    private bool Drag
    {
        get { return _drag; }
        set
        {
            _drag = value;
            _movable.LinearSpeed = _drag ? SpeedDrag : SpeedNormal;
        }
    }

    public override void Move(Vector2 movement)
    {
        var charMove = InputManager.GetCurrentAngle() * new Vector3(movement.x, 0, movement.y);
        _movable.Move(charMove);


        Animator.SetBool("walk", charMove.sqrMagnitude > 0);
        Animator.SetBool("drag", Joint.connectedBody != null);

        if (charMove.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Joint.connectedBody != null ? - charMove : charMove, Vector3.up), Time.deltaTime * RotationSpeed);
        }
    }

    public void Spin()
    {
        if (cut == null)
        {
            Animator.SetBool("cut", true);
            cut = StartCoroutine(Co_cut());
        }
    }

    public IEnumerator Co_cut()
    {
        yield return new WaitForSeconds(CutTime);
        Animator.SetBool("cut", false);

        var trees = WorldObjects.Instance.GetTreesInRadius(transform.position, CutRadius);
        foreach (var tree in trees)
            tree.Cut(CutForce, (tree.transform.position - transform.position).normalized );
        yield return new WaitForSeconds(CutCooldown);
        cut = null;
    }

    public void FindDragTarget()
    {
        if (Drag)
        {
            Joint.connectedBody = null;
        }
        else
        {
            if (_interactive[0] != null)
            {
                Debug.Log(_interactive[0]);
                Joint.connectedBody = _interactive[0].GetComponentInParent<Rigidbody>();
            }
        }

        Drag = Joint.connectedBody != null;
    }

    private void Update()
    {
        var count = Physics.OverlapSphereNonAlloc(InteractionCollider.transform.position, InteractionCollider.radius, _interactive, InteractionLayer);
        InteractionAvailable = count > 0;
    }
}
