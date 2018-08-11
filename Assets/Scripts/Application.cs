using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Application : MonoBehaviour
{
    private static Application _instance;
    public static Application Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Application>();
                if (_instance == null)
                {
                    var go = new GameObject("___Application");
                    _instance = go.AddComponent<Application>();
                }
            }

            return _instance;
        }
    }

    private InputManager _input;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        var inputs = FindObjectsOfType<InputManager>();
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i].gameObject != gameObject)
                Destroy(inputs[i].gameObject);
            else
                _input = inputs[i];
        }

        if (_input == null)
            _input = gameObject.AddComponent<InputManager>();


    }

    private void Update()
    {
    }

    public void MoveMainCharacter(Vector2 movement)
    {

    }
}
