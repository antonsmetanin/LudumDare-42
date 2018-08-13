using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkPart : MonoBehaviour
{
    public float Duration;
    private bool _isUsing;

    public void Use()
    {
        _isUsing = true;
    }

    private IEnumerator Co_Use()
    {
        var position = transform.position;
        var duration = Duration;
        while (Mathf.Abs(position.y - transform.position.y) < 1)
        {
            position.y = Mathf.Clamp(position.y, position.y - 1, 1 - duration / Duration);
            transform.position = position;

            yield return null;
        }
    }
}
