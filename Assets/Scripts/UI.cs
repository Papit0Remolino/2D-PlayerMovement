using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject loginScreen;
    [SerializeField] GameObject settingsScreen;
    [SerializeField] GameObject brightnessSlider;
    public float UbrightnessValue;
    int LastScenePlayed = 1; // aqui se guardará la ultima escena en la que estuvimos antes de desconectarnos.

    //VOLVER ATRAS CON EL ESCAPE
    private void Start()
    {
        UbrightnessValue = brightnessSlider.GetComponent<Scrollbar>().value;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToLoginScreen();
        }
    }

    //MOVIMIENTO ENTRE ESCENAS
    public void GoToLoginScreen()
    {
        loginScreen.SetActive(true);
        settingsScreen.SetActive(false);
    }
    public void GoToSettingsScreen()
    {
        loginScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }
    public void GoToGame()
    {
        SceneManager.LoadScene(LastScenePlayed);
    }

    // SETTINGS

    public static float Brightness()
    {
        //usar singleton porque esto no tira
        return brightnessValue / 5;
        //dividido entre 5 para que salga un valor de 0 a 0,2, que se le añadira a la intensity del global light
    }

}
