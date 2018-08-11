using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private LayerMask _flyUpLayers;
    [SerializeField] private float _minFlyHeight = 1;
    [SerializeField] private float _maxFlyHeight = 5;
    
    private float _height;

    private float _deltaHeight;
    private Vector3 _deltaPosition;
    private float _deltaRotation;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _height = _minFlyHeight;
        transform.position = new Vector3(transform.position.x, _height, transform.position.z);
    }

    private void LateUpdate()
    {
        if (Mathf.Abs(_deltaRotation) > Mathf.Epsilon)
        {
            transform.RotateAround(transform.position, Vector3.up, _deltaRotation);
            _deltaRotation = 0;
        }

        var euler = transform.rotation.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        var position = transform.position + (Quaternion.Euler(euler) * _deltaPosition);
        _deltaPosition = Vector3.zero;

        RaycastHit hit;
        
        if (Physics.Raycast(new Ray(position, Vector3.down), out hit, _maxFlyHeight * 2, _flyUpLayers))
        {
            _deltaHeight = _height - hit.distance;
            if (Mathf.Abs(_deltaHeight) > Mathf.Epsilon)
                position.y = Mathf.Clamp(Mathf.Lerp(position.y, position.y + _deltaHeight, 0.1f), _minFlyHeight, _maxFlyHeight);
        }

        transform.position = position;
    }

    public void Move(Vector2 shift)
    {
        _deltaPosition = new Vector3(shift.x, 0, shift.y);
    }

    public void Scale(float value)
    {
        _height = Mathf.Clamp(_height + value, _minFlyHeight, _maxFlyHeight);
    }

    public void Rotate(float value)
    {
        _deltaRotation = value;
    }
}
