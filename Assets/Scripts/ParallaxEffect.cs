using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float startposY, startposX;
    //camara en escena
    [SerializeField] GameObject cam;
    [Range(0,1)][SerializeField]float parallaxEffectX;
    [Range(0,1)][SerializeField] float parallaxEffectY;



    void Start()
    {
        startposX = transform.position.x;
        startposY = transform.position.y;
    }
    void FixedUpdate()
    {
        float distanceX = (cam.transform.position.x * parallaxEffectX);
        float distanceY = (cam.transform.position.y * parallaxEffectY);
        transform.position = new Vector2(startposX + distanceX, startposY + distanceY);
    }
}
