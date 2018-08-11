using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public LayerMask SelectableObjects;

    private CameraController _cameraController;
    private Vector2 _mousePosition;

    private void Start()
    {
        _cameraController = FindObjectOfType<CameraController>();
        if (_cameraController == null)
            _cameraController = Camera.main.gameObject.AddComponent<CameraController>();
    }

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit;
        var mousePosition = Input.mousePosition;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(_cameraController.MainCamera.ScreenPointToRay(mousePosition), out hit, SelectableObjects))
            {
                var selectable = hit.transform.GetComponent<Selectable>();
                if (selectable != null)
                {
                    selectable.Select();
                }
            }
        }

        ControlPlayer();
        ControlCamera(mousePosition);

        _mousePosition = mousePosition;
    }

    public void ControlPlayer()
    {
        var movement = new Vector2();

        if (Input.GetKey(KeyCode.W))
        {
            movement.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement.y -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement.x += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement.x -= 1;
        }

        Application.Instance.MoveMainCharacter(movement);
    }

    private void ControlCamera(Vector2 mousePosition)
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _cameraController.Rotate((Input.mousePosition.x - _mousePosition.x) / 2);
            return;
        }

        if (mousePosition.x < 50)
        {
            _cameraController.Move(-5f, 0f);
        }
        else if (_cameraController.MainCamera.pixelWidth - mousePosition.x < 50)
        {
            _cameraController.Move(5f, 0f);
        }
        if (mousePosition.y < 50)
        {
            _cameraController.Move(0f, -5f);
        }
        else if (_cameraController.MainCamera.pixelHeight - mousePosition.y < 50)
        {
            _cameraController.Move(0f, 5f);
        }

        var scale = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scale) > Mathf.Epsilon)
        {
            _cameraController.Scale(scale * 100);
        }
    }
}
