using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private LayerMask _flyUpLayers;
    [SerializeField] private float _startHeight = 20;
    [SerializeField] private float _minFlyHeight = 10;
    [SerializeField] private float _maxFlyHeight = 55;
    
    private float _height;

    private float _deltaHeight;
    private Vector3 _deltaPosition;
    private bool _ignoreMouse;
    private float _deltaRotation;

    public Camera MainCamera;

    private void Awake()
    {
        var cameraFollow = GetComponent<SceneCameraFollow>();
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        MainCamera = GetComponent<Camera>();
        _height = _startHeight;
        if (MainCamera.orthographic)
            MainCamera.orthographicSize = _height;
        else
        {
            var position = transform.position;
            ScalePerspectiveCamera(ref position, 1f);
            transform.position = position;
        }
    }

    private void LateUpdate()
    {
        if (Mathf.Abs(_deltaRotation) > Mathf.Epsilon)
        {
            RaycastHit hit;
            var center = transform.position;
            if (Physics.Raycast(MainCamera.ScreenPointToRay(new Vector3(MainCamera.pixelWidth / 2, MainCamera.pixelHeight / 2, 0)), out hit, transform.position.y * 3, _flyUpLayers))
            {
                center = hit.point;
            }
            transform.RotateAround(center, Vector3.up, _deltaRotation);
            _deltaRotation = 0;
        }

        var euler = transform.rotation.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        var position = transform.position + (Quaternion.Euler(euler) * _deltaPosition);
        _deltaPosition = Vector3.zero;
        _ignoreMouse = false;

        if (MainCamera.orthographic)
        {
            MainCamera.orthographicSize = Mathf.Lerp(MainCamera.orthographicSize, _height, 0.1f);
        }
        else
        {
            ScalePerspectiveCamera(ref position, 0.3f);
        }

        transform.position = position;
    }

    private void ScalePerspectiveCamera(ref Vector3 position, float smoothmeth)
    {
        _deltaHeight = _height - transform.position.y;

        if (Mathf.Abs(_deltaHeight) > Mathf.Epsilon)
        {
            var yMagn = Mathf.Lerp(0, _deltaHeight, smoothmeth);
            var angle = Vector3.Angle(MainCamera.transform.forward, Vector3.down);
            yMagn = yMagn / Mathf.Cos(angle * Mathf.Deg2Rad);
            position -= MainCamera.transform.forward * yMagn;
            position.y = Mathf.Clamp(position.y, _minFlyHeight, _maxFlyHeight);
        }
    }

    public void GoToPoint(Vector3 point)
    {
        var position = MainCamera.transform.position;
        position.y -= point.y;
        var angle = Vector3.Angle(MainCamera.transform.forward, Vector3.down);
        var resultPosition = point - (MainCamera.transform.forward * (position.y / Mathf.Cos(angle * Mathf.Deg2Rad)));
        MainCamera.transform.position = resultPosition;
        _ignoreMouse = true;
    }

    public void Move(Vector2 shift)
    {
        if (_ignoreMouse)
            return;

        Move(shift.x, shift.y);
    }

    public void Move(float x, float y)
    {
        if (_ignoreMouse)
            return;

        _deltaPosition.x += x;
        _deltaPosition.z += y;
    }

    public void Scale(float value)
    {
        _height = Mathf.Clamp(_height + value, _minFlyHeight, _maxFlyHeight);
    }

    public void Rotate(float value)
    {
        _deltaRotation = value;
    }

    public Transform RotateTarget;
}
