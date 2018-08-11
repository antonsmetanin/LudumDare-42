using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAlive
{
    float Health { get; }
    bool IsAlive { get; }
    bool IsDead { get; }
}
