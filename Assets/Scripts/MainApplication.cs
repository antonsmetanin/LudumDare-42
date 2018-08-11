using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainApplication : MonoBehaviour
{
    private static MainApplication _instance;
    public static MainApplication Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MainApplication>();
                if (_instance == null)
                {
                    var go = new GameObject("___Application");
                    _instance = go.AddComponent<MainApplication>();
                }
            }

            return _instance;
        }
    }

    public LayerMask SelectableObjects;

    private InputManager _input;
    private PlayerController _currentPlayer;

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

        _currentPlayer = FindObjectOfType<PlayerController>();
        if (_currentPlayer == null)
        {
            var playerObj = new GameObject();
            _currentPlayer = playerObj.AddComponent<PlayerController>();
            _currentPlayer.Init();
        }
    }

    private void Update()
    {
    }

    public void MoveMainCharacter(Vector2 movement)
    {
        _currentPlayer.Move(movement);
    }
}
