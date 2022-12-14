using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemiystandard : MonoBehaviour
{
    //PARAMETROS VIDA
    float BasehealthValue = 20; //cambiar el valor según el tag enemigo
    [SerializeField]float MaxhealthValue; //serializefield para poder verlo en el editor
    [SerializeField] float CurrenthealthValue; //serializefield para poder verlo en el editor
    //PARAMETROS MOVIMIENTO
    float moveSpeed = 8f; 
    //PARAMETROS ESTADISTICAS ESCALABLES
    int level; //el nivel será según la ronda en teoría
    float HealthMultiplier; // el multiplicador irá segun el nivel, cuanto mas dificil mas alto
    //REFERENCIAS
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rb;
    //PLACEHOLDER MOVIEMIENTO ALEATORIO
    Vector3 randomMovement;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        CurrenthealthValue = MaxhealthValue;
        if (HealthMultiplier == 0) { HealthMultiplier = 1.25f; }
    }

    // Update is called once per frame
    void Update()
    {
        die();
        PlaceHolder_MoveAround();
        LevelUp();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "weapon")
        {
            //healthValue -= other.WeaponStandar.DmgValue;
            animator.SetTrigger("FuisteGolpiado");
        }
    }

    void die()
    {
        if (CurrenthealthValue <= 0)
        {
            animator.SetTrigger("die");
        }
    }

    void PlaceHolder_MoveAround()
    {
        randomMovement = new Vector3(Random.Range(1, 100), 0, Random.Range(1, 100));
        rb.velocity = randomMovement;

        switch (rb.velocity.z)
        {
            case > 0.1f:
                animator.SetBool("isMovingForward", true);

                animator.SetBool("isMovingBack", false);
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
                break;
            case < 0.1f:
                animator.SetBool("isMovingBack", true);

                animator.SetBool("isMovingForward", false);
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
                break;
        }
        switch (rb.velocity.x)
        {
            case > 0.1f:
                animator.SetBool("isMovingRight", true);

                animator.SetBool("isMovingForward", false);
                animator.SetBool("isMovingBack", false);
                animator.SetBool("isMovingLeft", false);
                break;
            case < 0.1f:
                animator.SetBool("isMovingLeft", true);

                animator.SetBool("isMovingForward", false);
                animator.SetBool("isMovingBack", false);
                animator.SetBool("isMovingRight", false);
                break;
        }
    }
    void LevelUp()
    {
        //calcular la vida con el escalado de estadiscticas
        MaxhealthValue = (BasehealthValue + level) * HealthMultiplier;
        // subir de nivel como tal
        //level = round * 2;
    }
    private void OnEnable()
    {
        //al reactivarlo ponerle la vida maxima que le toca segun la ronda actual
        CurrenthealthValue = MaxhealthValue;
    }
}
