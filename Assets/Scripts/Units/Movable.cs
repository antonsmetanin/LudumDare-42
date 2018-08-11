using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movable : MonoBehaviour
{
    public float LinearSpeed;

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void Move(Vector2 movement)
    {
        movement = Vector2.ClampMagnitude(movement * LinearSpeed, LinearSpeed);
        Vector3 charMovement = new Vector3(movement.x, 0, movement.y);
        //if (!_controller.isGrounded)
        //{
        //    charMovement.y += Physics.gravity.y;
        //}

        _controller.Move(charMovement);
        //transform.position = new Vector3(transform.position.x + movement.x, transform.position.y, transform.position.z + movement.y);
    }
}
