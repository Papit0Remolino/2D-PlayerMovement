using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class UI : MonoBehaviour
{
    //singleton
    public static UI UIsingleton;
    //referencias a ventanas
    [SerializeField] GameObject loginScreen;
    [SerializeField] GameObject settingsScreen;
    //referencias Options window
    [SerializeField] GameObject OptionsWindowSize; //el viewport para cambiar su tamaño
    [SerializeField] GameObject brightnessSlider;
    [SerializeField] Toggle borderlessWindow;
    [SerializeField] Toggle fullScreen;
    [SerializeField] Scrollbar mainVolume;
    [SerializeField] Scrollbar musicVolume;
    [SerializeField] Scrollbar efectsVolume;
    [SerializeField] Toggle invertX;
    public float UIbrightnessValue;
    int LastScenePlayed = 1; // aqui se guardará la ultima escena en la que estuvimos antes de desconectarnos.
    //parametros adicionales
    float sizeIterator = 1; //guarda el cambio de escala de la UI. Se usa en ChangeUISize();
    public bool invertXisActivated { get; private set; }
    //VOLVER ATRAS CON EL ESCAPE
    private void Start()
    {
        if (UIsingleton == null)
        {
            UIsingleton = this;
        }
        //SETTINGS STARTING VALUES
        brightnessSlider.GetComponent<Scrollbar>().value = 0.5f;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToLoginScreen();
        }
        //actualizamos el brillo(que luego le pasamos a game manager para que lo meta ingame) segun el valor que le demos en el menú de opciones;
        UIbrightnessValue = brightnessSlider.GetComponent<Scrollbar>().value;

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
    public void ChangeUIsize()
    {
        if (sizeIterator > 2)
        {
            sizeIterator = 0;
            OptionsWindowSize.GetComponent<RectTransform>().localScale = new Vector3(1 , 1, 0);
        }
        else
        {
            sizeIterator++;
            OptionsWindowSize.GetComponent<RectTransform>().localScale = new Vector3(1 + (sizeIterator / 4), 1 + (sizeIterator / 4), 0);
        }
    }

    public void InvertX()
    {
        invertXisActivated = !invertXisActivated;
        //invertimos los controles en si en el script de player, según el valor de este booleano;
    }


    //PREGUNTAR COMO VA ESTO EN CLASE
    //public void FullScreen()
    //{
    //    Screen.fullScreen = !Screen.fullScreen;
    //}
    //public void FUllScreenWindow()
    //{
    //    Screen.fullScreen = Screen.fullScreenMode.Windowed;
    //}

    //PREGUNTAR EL DONT DESTROY ON LOAD como hacer que guarde estos valores

    // investigar PLAYER PREFS
}
