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
    public float WaterAvoidence = .5f;
    public float WaterThreshold = .5f;

    [Header("Axe")]
    public float CutRadius = 1f;
    public float CutTime = .1f;
    public float CutCooldown = .3f;
    public float CutForce = 30f;

    [Header("Interaction")]
    public Joint Joint;
    public SphereCollider InteractionCollider;
    public LayerMask InteractionLayer;
    public LayerMask GroundLayer;
    public LayerMask WaterLayer;
    public bool InteractionAvailable =  false;

    private Coroutine cut;
    private Collider[] _interactive = new Collider[1];
    private bool _drag;
    private TreeTrunk burden;
    public AudioSource Source;
    public AudioClip SwingClip;
    public AudioClip ChopCLip;
    
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


        var dir = charMove.normalized;
        
        /*var lowerpoint = transform.position + Vector3.down * 5f + dir * WaterAvoidence;
        var upperpoint = transform.position + Vector3.up * 5f + dir * WaterAvoidence;

        RaycastHit below;
        float waterDistance = 10;
        float groundDistance = 10;

        if (Physics.Linecast(upperpoint, lowerpoint, out below, WaterLayer))
            waterDistance = below.distance;

        if (Physics.Linecast(upperpoint, lowerpoint, out below, GroundLayer))
            groundDistance = below.distance;
        
        
        if(groundDistance  < waterDistance + WaterThreshold)   */     
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
            Source.PlayOneShot(SwingClip);
            Animator.SetBool("cut", true);
            cut = StartCoroutine(Co_cut());
        }
    }

    public IEnumerator Co_cut()
    {
        yield return new WaitForSeconds(CutTime);
        Animator.SetBool("cut", false);

        var trees = WorldObjects.Instance.GetTreesInRadius(transform.position, CutRadius);
        
        if(trees.Count > 0 )
            Source.PlayOneShot(ChopCLip);

        
        foreach (var tree in trees)
            tree.Cut(CutForce, (tree.transform.position - transform.position).normalized );
        yield return new WaitForSeconds(CutCooldown);
        cut = null;
    }

    public void FindDragTarget()
    {
        if (burden != null)
        {
            burden.Drop();
            burden.RecycleAction -= OnBurdenRecycled;
            burden = null;
        }
        else
        {
            if (_interactive[0] != null)
            {
                var treeTrunk = _interactive[0].GetComponentInParent<TreeTrunk>();
                burden = treeTrunk;
                burden.RecycleAction += OnBurdenRecycled;
                burden.Carry(Joint);
            }
        }

        Drag = burden != null;
    }

    private void OnBurdenRecycled(TreeTrunk obj)
    {
        obj.RecycleAction -= OnBurdenRecycled;
        burden = null;
        Drag = burden != null;
    }

    private void Update()
    {
        _interactive = new Collider[1];
        var count = Physics.OverlapSphereNonAlloc(InteractionCollider.transform.position, InteractionCollider.radius, _interactive, InteractionLayer);
        InteractionAvailable = count > 0;
    }

    public void Drowned()
    {
        Animator.SetTrigger("drown");
        _movable.LinearSpeed = 0.05f;
        RotationSpeed = 1f;
    }
}
