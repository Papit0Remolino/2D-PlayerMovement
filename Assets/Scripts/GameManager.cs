using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    //actualizar brillo
    [SerializeField] Light2D globalLight;
    float baseGameBrightness = 0.3f;

    void FixedUpdate()
    {
        updateBrightness();
    }

    void updateBrightness()
    {
        //cambiar a que se ejecute esta linea solo caundo el menú este abierto, para mejorar el rendimiento
        globalLight.intensity = baseGameBrightness + UI.UIsingleton.UIbrightnessValue;
    }
}
