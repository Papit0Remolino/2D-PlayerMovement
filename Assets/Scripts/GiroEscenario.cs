using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiroEscenario : MonoBehaviour
{
    float initialGravity;
    private void Start()
    {
        initialGravity = GetComponent<Rigidbody2D>().gravityScale;
    }
    void Update()
    {
        if (!PlayerMovement.Singleton.isReversed)
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
        }
        else if (PlayerMovement.Singleton.isReversed)
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity * -1;
            
        }
    }
}
