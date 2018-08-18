using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{

    public Image Fill;
    private void Update()
    {
        if (Input.anyKeyDown && _async == null)
        {
            _async  = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);

        }

        if (_async != null)
        {
            Fill.fillAmount = _async.progress;
        }
    }

    private AsyncOperation _async;

}