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

    [SerializeField] private Movable _targetTransform;

    public Camera MainCamera;


    [SerializeField] private Vector3 _cameraTarget = Vector3.zero;

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
        /*if (Mathf.Abs(_deltaRotation) > Mathf.Epsilon)
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

        _cameraTarget += Quaternion.Euler(euler) * _deltaPosition;

        _deltaPosition = Vector3.zero;
        _ignoreMouse = false;

        ScalePerspectiveCamera(ref position, 0.3f);

        if (_targetTransform != null)
        {

        }
        else
        {
            var frustumHeight = 2.0f * (_maxFlyHeight -_height) * Mathf.Tan(MainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumOnPlane = frustumHeight / 0.75f / 2f; //45 deg
            float frustumOnPlane2 = frustumHeight / 0.75f / 2f * MainCamera.aspect;


            _cameraTarget.x = Mathf.Clamp(_cameraTarget.x, _xLimits.x - frustumOnPlane, _xLimits.y + frustumOnPlane);
            _cameraTarget.z = Mathf.Clamp(_cameraTarget.z, _zLimits.x - frustumOnPlane2, _zLimits.y + frustumOnPlane2);


        }*/


        Vector3 newTarget = _targetTransform.transform.position +
                            _targetTransform.Velocity.normalized * MovementAnticipation;

        Vector3 p1, p2, p3, p4;
        var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Intersect2Planes(planes[2], new Plane(Vector3.down, LandDepth), out p1); //down;
        Intersect2Planes(planes[3], new Plane(Vector3.down, LandDepth), out p2); //up;
        
        Intersect2Planes(planes[0], new Plane(Vector3.down, LandDepth), out p3); //l;
        Intersect2Planes(planes[1], new Plane(Vector3.down, LandDepth), out p4); //r;

        float vertical = Vector3.Distance(p1, p2) / 2f;
        float horizontal = Vector3.Distance(p3, p4) / 2f;

        newTarget.x = Mathf.Clamp(newTarget.x, LowerLimit + vertical, UpperLimit - vertical);
        newTarget.z = Mathf.Clamp(newTarget.z, LeftLimit + horizontal, RightLimit - horizontal);
        _cameraTarget = Vector3.SmoothDamp(_cameraTarget, newTarget, ref _vel, CameraSmootTime);
        transform.position = _cameraTarget - transform.forward * _height;
    }

    public float LandDepth = 21;
    public float LowerLimit = -70;
    public float UpperLimit = 129;
    public float LeftLimit = -70;
    public float RightLimit = 129;
    
    public void Intersect2Planes(Plane Pn1, Plane Pn2, out Vector3 point1)
    {
        Vector3 u = Vector3.Cross(Pn1.normal, Pn2.normal); // cross product
        float ax = (u.x >= 0 ? u.x : -u.x);
        float ay = (u.y >= 0 ? u.y : -u.y);
        float az = (u.z >= 0 ? u.z : -u.z);

        // Pn1 and Pn2 intersect in a line
        // first determine max abs coordinate of cross product
        int maxc; // max coordinate
        if (ax > ay)
        {
            if (ax > az)
                maxc = 1;
            else maxc = 3;
        }
        else
        {
            if (ay > az)
                maxc = 2;
            else maxc = 3;
        }

        Vector3 iP = new Vector3(); // intersect point
        float d1, d2;
        d1 = Vector3.Dot(Pn1.normal, Pn1.normal * Pn1.distance);
        d2 = Vector3.Dot(Pn2.normal, Pn2.normal * Pn2.distance);

        switch (maxc)
        {
            // select max coordinate
            case 1: // intersect with x=0
                iP.x = 0;
                iP.y = (d2 * Pn1.normal.z - d1 * Pn2.normal.z) / u.x;
                iP.z = (d1 * Pn2.normal.y - d2 * Pn1.normal.y) / u.x;
                break;
            case 2: // intersect with y=0
                iP.x = (d1 * Pn2.normal.z - d2 * Pn1.normal.z) / u.y;
                iP.y = 0;
                iP.z = (d2 * Pn1.normal.x - d1 * Pn2.normal.x) / u.y;
                break;
            case 3: // intersect with z=0
                iP.x = (d2 * Pn1.normal.y - d1 * Pn2.normal.y) / u.z;
                iP.y = (d1 * Pn2.normal.x - d2 * Pn1.normal.x) / u.z;
                iP.z = 0;
                break;
        }

        point1 = iP;
        //point2 = iP + u;
    }

    public float MovementAnticipation = 2;
    public float CameraSmootTime = 0.1f;
    private Vector3 _vel;

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
