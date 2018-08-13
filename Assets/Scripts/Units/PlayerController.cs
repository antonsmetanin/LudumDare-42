using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitControllerBase
{

    public float RotationSpeed;
    public Animator Animator;

    public override void Move(Vector2 movement)
    {
        var charMove = InputManager.GetCurrentAngle() * new Vector3(movement.x, 0, movement.y);
        _movable.Move(charMove);


        Animator.SetBool("walk", charMove.sqrMagnitude > 0);


        if (charMove.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(charMove, Vector3.up), Time.deltaTime * RotationSpeed);
        }
    }

    private void Update()
    {

    }

    private Coroutine cut;

    public void Spin()
    {
        if (cut == null)
        {
            Animator.SetBool("cut", true);
            cut = StartCoroutine(Co_cut());
        }


    }

    public float CutRadius = 1f;
    public float CutTime = .1f;
    public float CutCooldown = .3f;
    public float CutForce = 30f;

    public IEnumerator Co_cut()
    {
        yield return new WaitForSeconds(CutTime);
        Animator.SetBool("cut", false);

        var trees = WorldObjects.Instance.GetTreesInRadius(transform.position, CutRadius);
        foreach (var tree in trees)
            tree.Cut(CutForce, tree.transform.position - transform.position );
        yield return new WaitForSeconds(CutCooldown);
        cut = null;
    }
}
