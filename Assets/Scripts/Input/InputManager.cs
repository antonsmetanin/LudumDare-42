using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public LayerMask SelectableObjects;

    private CameraController _cameraController;

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(_cameraController.MainCamera.ScreenPointToRay(Input.mousePosition), out hit, SelectableObjects))
            {
                var selectable = hit.transform.GetComponent<Selectable>();
                if (selectable != null)
                {
                    selectable.Select();
                }
            }
        }
    }
}
