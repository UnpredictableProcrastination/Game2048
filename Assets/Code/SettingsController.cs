using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SettingsController : MonoBehaviour
{
    public readonly String SceneName = "Settings";

    public void Exit()
    {
        SceneManager.UnloadSceneAsync(SceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }
    }
}
