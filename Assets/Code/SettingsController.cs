using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    //private readonly String SceneName = "Settings";
    private readonly String mainSceneName = "MainWindow";
    private readonly String settingsFileName = "/settings.txt";

    private string settingsFilePath;

    private void Start()
    {
        settingsFilePath = Application.persistentDataPath + settingsFileName;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }
    }

    public void Exit()
    {
        SceneManager.LoadScene(mainSceneName);
        //SceneManager.UnloadSceneAsync(SceneName);
    }

    public void UpdateSensitivity()
    {
        var slider = GameObject.Find("SensSlider").GetComponentInChildren<Slider>();
        
        Debug.Log(slider.value);
    }
}
