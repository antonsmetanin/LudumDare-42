using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movable : MonoBehaviour
{
    public float LinearSpeed = 1;
    public Vector3 Velocity { get { return _controller.velocity; } }

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void Move(Vector3 movement)
    {
        var charMovement = Vector3.ClampMagnitude(movement * LinearSpeed, LinearSpeed);
        if (!_controller.isGrounded)
        {
            charMovement.y += Physics.gravity.y;
        }

        _controller.Move(charMovement);
    }
}
