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
    //referencias a eol iterador de movimiento de la camara para cambiar entre pantallas
    [SerializeField] GameObject ScreenIterator;
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
    //parametros adicionales
    public float UIbrightnessValue;
    int LastScenePlayed = 1; // aqui se guardará la ultima escena en la que estuvimos antes de desconectarnos.
    float sizeIterator = 1; //guarda el cambio de escala de la UI. Se usa en ChangeUISize();
    public bool invertXisActivated { get; private set; }
    
    private void Start()
    {
        if (UIsingleton == null)
        {
            UIsingleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        //SETTINGS STARTING VALUES
        brightnessSlider.GetComponent<Scrollbar>().value = 0.5f;
    }
    //VOLVER ATRAS CON EL ESCAPE
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToLoginScreen();
        }
        //actualizamos el brillo(que luego le pasamos a game manager para que lo meta ingame) segun el valor que le demos en el menú de opciones;
        //quitar de aqui y meterlo en el onvaluechange del slider
        UIbrightnessValue = brightnessSlider.GetComponent<Scrollbar>().value;
        
        //activar la ui segun la poscion del iterador
        switch (ScreenIterator.transform.position.x)
        {
            case 18.75f:
                StartCoroutine(ActivateSettingsScreen());
                break;
            case 0:
                StartCoroutine(ActivateLoginScreen());
                break;
            case -18.75f:
                StartCoroutine(ActivateNewGameScreen());
                break;
            default:
                loginScreen.SetActive(false);
                settingsScreen.SetActive(false);
                break;
        }
    }

    //MOVIMIENTO ENTRE ESCENAS

    //volver a la pantalla principal
    public void GoToLoginScreen()
    {
        //ScreenIterator.GetComponent<Animator>().SetTrigger("GoBack");
        ScreenIterator.GetComponent<Animator>().SetInteger("Screen", 0);

    }
    IEnumerator ActivateLoginScreen()
    {
        yield return new WaitForSeconds(0.3f);
        loginScreen.SetActive(true);
    }

    //ir a la pantalla ajustes
    public void GoToSettingsScreen()
    {
        //ScreenIterator.GetComponent<Animator>().SetTrigger("GoRight");
        ScreenIterator.GetComponent<Animator>().SetInteger("Screen",1);
    }
    IEnumerator ActivateSettingsScreen()
    {
        yield return new WaitForSeconds(0.3f);
        settingsScreen.SetActive(true);
    }

    //ir a la pantalla nueva partida
    public void GoToNewGame()
    {
        //ScreenIterator.GetComponent<Animator>().SetTrigger("GoLeft");
        ScreenIterator.GetComponent<Animator>().SetInteger("Screen", -1);
        //activar cinematica

    }
    IEnumerator ActivateNewGameScreen()
    {
        yield return new WaitForSeconds(0.3f);
        //newGameScreen.SetActive(true);
    }


    public void GoToGame()
    {
        SceneManager.LoadScene(LastScenePlayed);
    }

    public void Exit()
    {
        Application.Quit();
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


    public void FullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    //PREGUNTAR EL DONT DESTROY ON LOAD como hacer que guarde estos valores

    // investigar PLAYER PREFS
}
