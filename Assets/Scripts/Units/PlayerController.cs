using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitControllerBase
{
    public override void Move(Vector2 movement)
    {
        var charMove = InputManager.GetCurrentAngle() * new Vector3(movement.x, 0, movement.y);
        _movable.Move(charMove);
    }
}
