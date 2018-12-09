using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    //private readonly String SceneName = "Settings";
    private readonly String mainSceneName = "MainWindow";
    private readonly String settingsFileName = "/settings.txt";

    // Файл незавершенной сессии
    private string sessionFilePath;
    private readonly String sessionFileName = "/session";

    private string settingsFilePath;
    private float sensitivity = 3;

    private void Start()
    {
        settingsFilePath = Application.persistentDataPath + settingsFileName;
        sessionFilePath = Application.persistentDataPath + sessionFileName;
        LoadSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }
    }


    private void Exit()
    {
        SaveSettings();
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// Записывает измененные настройки в файл
    /// </summary>
    private void SaveSettings()
    {
        using (StreamWriter writer = new StreamWriter(settingsFilePath))
        {
            // Записываем чувствительность
            writer.WriteLine(sensitivity.ToString("F1"));
        }
    }

    /// <summary>
    /// Читает настройки из файла и отображает их в интерфейсе
    /// </summary>
    private void LoadSettings()
    {
        using (StreamReader reader = new StreamReader(settingsFilePath))
        {
            float sens;
            string rawSens = reader.ReadLine();
            if (float.TryParse(rawSens, out sens))
            {
                sensitivity = sens;
            }
            else
            {
                Debug.LogWarning("Can not parse sensitivity value!\n" + rawSens + " is not looks like a float.");
            }

            var slider = GameObject.Find("SensSlider").GetComponentInChildren<Slider>();
            slider.value = sensitivity;
        }
    }

    /// <summary>
    /// Обрабатывает событие движения ползунка чувствительности
    /// </summary>
    public void UpdateSensitivity()
    {
        var slider = GameObject.Find("SensSlider").GetComponentInChildren<Slider>();
        float value = slider.value;
        sensitivity = value;

        Debug.Log(value);
    }

    public void DeleteSession()
    {
        if(File.Exists(sessionFilePath))
        {
            File.Delete(sessionFilePath);
        }
    }
}
