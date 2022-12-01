using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Singleton = null;
    //GROUNDCHECK
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform ceilingCheck;
    [Range(0, 0.3f)][SerializeField] float groundedRadius = 1f;
    
    //PARÁMETROS MOVIMIENTO
    public float moveInput { get; private set; }
    public float moveSpeed { get; private set; }
    public float acceleration { get; private set; }
    public float decceleration { get; private set; }
    public float velPower { get; private set; }
    [Range(0, 1)][SerializeField] float movementSmoothing;
    Vector3 referenceVector = Vector3.zero;
    //PARÁMETROS DIRRECCIÓN SPRITE JUGADOR
    public bool isFacingRight { get; private set; }
    //PARÁMETROS SALTO
    public bool isGrounded { get; private set; }
    public float initialGravity { get; private set; }
    [SerializeField] PhysicsMaterial2D friction;
    [Range(0,3)][SerializeField] float gravityMultiplier;
    public float maxFallSpeed { get; private set; }
    //NOTA MENTAL:AÑADIR COYOTE TIME Y JUMP BUFFERING
    public float fallAcceleration { get; private set; }
    public bool canDoubleJump { get; private set; }
    //PARÁMETROS DASH
    public bool canDash { get; private set; }
    public bool CooldownRanOut { get; private set; }
    public bool isDashing { get; private set; }
    [SerializeField] float dashingTime;
    public float dashingCooldown { get; private set; }
    [Range(0, 500f)][SerializeField] float dashingPower;
    public bool jumpAfterDashing { get; private set; }
    //PARAMETROS WALLJUMP
    [SerializeField] Transform wallCheckLeft;
    [SerializeField] Transform wallCheckRight;
    [Range(0,25)][SerializeField] float xWallJumpForce;
    public float wallHodingdRadius { get; private set; }
    public bool isWallJumping { get; private set; }
    public bool isWallHolding { get; private set; }
    public bool isWallHoldingReversed { get; private set; }
    public float maxFallSpeedWhenWallholding { get; private set; }
    //subseccion (rotar cabeza en walljump)
    [SerializeField] GameObject cabeziglia;
    public bool Rotate { get; private set; }
    public bool RotateBack { get; private set; }
    //CAMBIO GRAVEDAD
    public bool isReversed;
    //CAMARA
    [SerializeField] CinemachineVirtualCamera cam;
    [SerializeField] Animator camAnimator;
    //UI
    [SerializeField] Animator uiAnimator;
    //CHECKPOINTS
    GameObject lastCheckpoint;
    private void Awake()
    {
        friction = GetComponent<Rigidbody2D>().sharedMaterial;
    }
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        CooldownRanOut = true;
        dashingTime = 0.2f;
        maxFallSpeed = 30;
        isReversed = false;
        isWallJumping = false;
        wallHodingdRadius = 0.05f;
        canDash = true;
        initialGravity = GetComponent<Rigidbody2D>().gravityScale;
        moveSpeed = 17f;
        acceleration = 17f;
        decceleration = 20f;
        velPower = 0.96f;
        canDoubleJump = true;
        dashingCooldown = 0.2f;
        maxFallSpeedWhenWallholding = 6;
    }

    void Update()
    {
        Debug.Log(isReversed);
        if (canDash && CooldownRanOut && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Dash());
        }
        if (GetComponent<Rigidbody2D>().gravityScale > 0)
        {
            jump();
            Fall();
            LimitFallSpeed();
            WallJump();

        }
        if (GetComponent<Rigidbody2D>().gravityScale < 0)
        {
            jumpWhenReversed();
            FallWhenReversed();
            LimitFallSpeedReversed();
            WallJumpWhenReversed();
        }
        Rotation();
        CheckIfGrounded();
        HoldOnCeeling();
        reverseGravity();

    }
    private void FixedUpdate()
    {
            
        Movement();
        
    }
    public void Rotation()
    {
        //ROTACIÓN A LA R
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetComponent<Animator>().SetTrigger("Rotation");
        }
    }
    //MOVIMIENTO DEL JUGADOR
    void Movement()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        Vector3 targetSpeed = new Vector2(moveInput * moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
        //float targetSpeed = moveInput * moveSpeed;
        //lo de abajo es movimiento mas complejo pero va un poco a trompicones, menos fluido
        //float speedDif = targetSpeed - GetComponent<Rigidbody2D>().velocity.x;
        //float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration; // if targetSpeed > 0 (estoy andando) entonces (targetSpeed * acceleration) else (tagetSpeed * decceleration)
        //float FinalMovement = Mathf.Pow(Mathf.Abs(speedDif) * accelerationRate, velPower) * Mathf.Sign(speedDif);
        //GetComponent<Rigidbody2D>().AddForce(FinalMovement * Vector2.right);

        //sprite direction
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        GetComponent<Rigidbody2D>().velocity = Vector3.SmoothDamp(GetComponent<Rigidbody2D>().velocity, targetSpeed, ref referenceVector, movementSmoothing);


    }
    void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 NormalScale = transform.localScale;
        NormalScale.x *= -1;
        transform.localScale =NormalScale;
    }
    void Fall()
    {
        //Aumentar velocidad de caida cuando el salto llega a su punto mas alto (y cuando caes en general)
        if (!isGrounded && GetComponent<Rigidbody2D>().velocity.y < 0 && !isWallJumping && !isDashing)
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity * gravityMultiplier;
        }
        else if(!isDashing)
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
        }
    }
    void FallWhenReversed()
    {
        //lo mismopero para cuando la gravedad esta invertida
        if (!isGrounded && GetComponent<Rigidbody2D>().velocity.y > 0 && !isWallJumping)
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity * gravityMultiplier;
        }
        else
        {
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
        }
    }
    void jump()
    {
        // SALTO ESTÁNDAR
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float Jumpforce = 25f; // se crea aqui para que vuelva a este valor cada vez que llamas al método salto
            if (GetComponent<Rigidbody2D>().velocity.y < 0) // if estas cayendo
            {
                //le añades a la fuerza del salto, la fuerza con la que estas cayendo para que se contrareste 
                //(si estas a velocidad.y = -10 y le sumas 25 saltas solo 10)
                //en cambio, si estas v.y = -10 y antes de saltar haces fuerza 25 - (-10) = 35 de fuerza de salto
                //cuando vayas a saltar a v.y = -10 saltarás 35 -10 =  25, ak el valor de salto base
                Jumpforce -= GetComponent<Rigidbody2D>().velocity.y;

            }
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * Jumpforce, ForceMode2D.Impulse);
        }

        //DOBLE SALTO
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isWallHolding && canDoubleJump && !isDashing) //!isDashing porque si le das al espacio mientras dashea no hace nada porque la velocidad es la del dash, y se pone double jump en false por lo que te cancela el doble salto
        {
            float jumpForce = 25f;  
            //reseta su velocidad en y para que no se acumulen las velocidades de ambos saltos
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
            //por si queires hacer el doble salto justo despues del dash
            

        }
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isWallHolding && canDoubleJump && isDashing)
        {
            Debug.Log("salatar en dash");
            jumpAfterDashing = true;
        }
            



        //si sueltas la tecla dejar de saltar
        if (Input.GetKeyUp(KeyCode.Space) && GetComponent<Rigidbody2D>().velocity.y > 0)
        {
            float jumpForce = GetComponent<Rigidbody2D>().velocity.y;

            GetComponent<Rigidbody2D>().AddForce(Vector2.down * jumpForce, ForceMode2D.Impulse);
        }

    }
    void jumpWhenReversed()
    {
        // SALTO ESTÁNDAR
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float Jumpforce = -25f; // se crea aqui para que vuelva a este valor cada vez que llamas al método salto
            if (GetComponent<Rigidbody2D>().velocity.y < 0) // if estas cayendo
            {
                //le añades a la fuerza del salto, la fuerza con la que estas cayendo para que se contrareste 
                //(si estas a velocidad.y = -10 y le sumas 25 saltas solo 10)
                //en cambio, si estas v.y = -10 y antes de saltar haces fuerza 25 - (-10) = 35 de fuerza de salto
                //cuando vayas a saltar a v.y = -10 saltarás 35 -10 =  25, ak el valor de salto base
                Jumpforce -= GetComponent<Rigidbody2D>().velocity.y;

            }
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * Jumpforce, ForceMode2D.Impulse);
        }

        //DOBLE SALTO
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && !isWallHolding && canDoubleJump) 
        {
            float jumpForce = -25f;
            //reseta su velocidad en y para que no se acumulen las velocidades de ambos saltos
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;

        }


        //si sueltas la tecla dejar de saltar
        if (Input.GetKeyUp(KeyCode.Space) && GetComponent<Rigidbody2D>().velocity.y < 0)
        {
            float jumpForce = GetComponent<Rigidbody2D>().velocity.y;

            GetComponent<Rigidbody2D>().AddForce(Vector2.down * jumpForce, ForceMode2D.Impulse);
        }
    }
    //WALLJUMP
    void WallJump()
    {
        if (Physics2D.OverlapCircle(wallCheckRight.position, wallHodingdRadius, whatIsGround) || Physics2D.OverlapCircle(wallCheckLeft.position, wallHodingdRadius, whatIsGround))
        {
            isWallHolding = true;
            GetComponent<Animator>().SetBool("WallHoldingReversed", false);
            GetComponent<Animator>().SetBool("WallHolding", true);
            canDash = true;
            canDoubleJump = true;
            friction.friction = 0.2f;
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
            if (GetComponent<Rigidbody2D>().velocity.y < -6)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, -6);
            }
            //roatacion de cabeza
            if (Rotate == false)
            {
                Vector3 NormalScale = cabeziglia.transform.localScale;
                NormalScale.x *= -1;
                cabeziglia.transform.localScale = NormalScale;
                Rotate = true;
            }


        }
        if (!Physics2D.OverlapCircle(wallCheckRight.position, wallHodingdRadius, whatIsGround) && !Physics2D.OverlapCircle(wallCheckLeft.position, wallHodingdRadius, whatIsGround))
        {
            GetComponent<Animator>().SetBool("WallHolding", false);
            isWallHolding = false;
            isWallHoldingReversed = false;
            friction.friction = 0.1f;
            //cabeziglia.transform.localScale *= -1;
            if (Rotate == true)
            {
                Vector3 NormalScale = cabeziglia.transform.localScale;
                NormalScale.x *= -1;
                cabeziglia.transform.localScale = NormalScale;
                Rotate = false;
            } 
        }

        //según si te agarras a la izq o la drch que la fuerza en x del salto sea positiva o negativa
        if (isFacingRight && Input.GetKeyDown(KeyCode.Space) && isWallHolding)
        {
            float yWallJumpForce = 25f;
            //StartCoroutine(WallJumpGravity());
            GetComponent<Rigidbody2D>().velocity = new Vector2(xWallJumpForce * -2, yWallJumpForce);
        }
        if (!isFacingRight && Input.GetKeyDown(KeyCode.Space) && isWallHolding)
        {
            float yWallJumpForce = 25f;
            //StartCoroutine(WallJumpGravity());
            GetComponent<Rigidbody2D>().velocity = new Vector2(xWallJumpForce * 2, yWallJumpForce);
        }

    }
    void WallJumpWhenReversed()
    {
        if (Physics2D.OverlapCircle(wallCheckRight.position, wallHodingdRadius, whatIsGround) || Physics2D.OverlapCircle(wallCheckLeft.position, wallHodingdRadius, whatIsGround))
        {
            isWallHolding = true;
            GetComponent<Animator>().SetBool("WallHolding", false);
            GetComponent<Animator>().SetBool("WallHoldingReversed", true);
            canDash = true;
            canDoubleJump = true;
            friction.friction = 0.2f;
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
            //limitar la velocidad de caida cuando te agarras
            if (GetComponent<Rigidbody2D>().velocity.y > 6)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 6);
            }
            //roatacion de cabeza
            if (Rotate == false)
            {
                Vector3 NormalScale = cabeziglia.transform.localScale;
                NormalScale.x *= -1;
                cabeziglia.transform.localScale = NormalScale;
                Rotate = true;
            }


        }
        if (!Physics2D.OverlapCircle(wallCheckRight.position, wallHodingdRadius, whatIsGround) && !Physics2D.OverlapCircle(wallCheckLeft.position, wallHodingdRadius, whatIsGround))
        {
            GetComponent<Animator>().SetBool("WallHoldingReversed", false);
            isWallHolding = false;
            friction.friction = 0.1f;
            //cabeziglia.transform.localScale *= -1;
            if (Rotate == true)
            {
                Vector3 NormalScale = cabeziglia.transform.localScale;
                NormalScale.x *= -1;
                cabeziglia.transform.localScale = NormalScale;
                Rotate = false;
            }
        }

        //según si te agarras a la izq o la drch que la fuerza en x del salto sea positiva o negativa
        if (isFacingRight && Input.GetKeyDown(KeyCode.Space) && isWallHolding)
        {
            float yWallJumpForce = -25f;
            //StartCoroutine(WallJumpGravity());
            GetComponent<Rigidbody2D>().velocity = new Vector2(xWallJumpForce * -2, yWallJumpForce);
        }
        if (!isFacingRight && Input.GetKeyDown(KeyCode.Space) && isWallHolding)
        {
            float yWallJumpForce = -25f;
            //StartCoroutine(WallJumpGravity());
            GetComponent<Rigidbody2D>().velocity = new Vector2(xWallJumpForce * 2, yWallJumpForce);
        }

    }
    void CheckIfGrounded()
    {
        
        //tambien se puede hacer con un Physics.Raycast(osición,dirección,longitud)
        //if (Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, WhatIsGround))
        if (Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround))
        {
            isGrounded = true;
            //resetear doble salto
            canDoubleJump = true;
            if (CooldownRanOut)
            {
                canDash = true;
            }

        }
        else
        {
            isGrounded = false;
        }
    }
    void HoldOnCeeling()
    {
        if (Physics2D.OverlapCircle(ceilingCheck.position, groundedRadius, whatIsGround) && (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W)))
        {
            canDoubleJump = true;
            canDash = true;
            GetComponent<Rigidbody2D>().gravityScale = initialGravity;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
        }
        if (!Physics2D.OverlapCircle(ceilingCheck.position, groundedRadius, whatIsGround) || Input.GetKeyUp(KeyCode.W))
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    //DASH
    private IEnumerator Dash()
    {
        CooldownRanOut = false;
        canDash = false;
        isDashing = true;
        //esto es importante para cuando haces un dash con la gravedad invertida, que al poner la gravedad a 0.01
        //te detecta los controles de cuando la gravedad está normal durante el lapso del dash
        //y si da la casualidad que chocas contra un muro reproduce la animacion wallHolding normal en vez de WallHoldingReversed
        if (initialGravity > 0)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0.01f;
        }
        else if (initialGravity < 0)
        {
            GetComponent<Rigidbody2D>().gravityScale = -0.01f;
        }

        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        //GetComponent<Rigidbody2D>().velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        GetComponent<Rigidbody2D>().AddForce(Vector2.right * transform.localScale.x * dashingPower, ForceMode2D.Impulse);
        //tr.startColor = GetComponent<SpriteRenderer>().color;
        //tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        //tr.emitting = false;
        GetComponent<Rigidbody2D>().gravityScale = initialGravity;
        isDashing = false;
        if (jumpAfterDashing && GetComponent<Rigidbody2D>().gravityScale > 0)
        {
            float jumpForce = 25f;
            //reseta su velocidad en y para que no se acumulen las velocidades de ambos saltos
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
            jumpAfterDashing = false;

        }
        else if (jumpAfterDashing && GetComponent<Rigidbody2D>().gravityScale < 0)
        {
            float jumpForce = -25f;
            //reseta su velocidad en y para que no se acumulen las velocidades de ambos saltos
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0);
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
            jumpAfterDashing = false;
        }
        yield return new WaitForSeconds(dashingCooldown);
        CooldownRanOut = true;

    }
    private void LimitFallSpeed()
    {
        if (GetComponent<Rigidbody2D>().velocity.y < -maxFallSpeed)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, -maxFallSpeed);
        }
    }
    private void LimitFallSpeedReversed()
    {
        if (GetComponent<Rigidbody2D>().velocity.y > maxFallSpeed)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, maxFallSpeed);
        }
    }
    void reverseGravity()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // no se porque pero si esto(setTriggerReverse) se ejecuta cuando esta wallHolding se invierte la gravedad mal
            if (!isWallHolding)
            {
                GetComponent<Animator>().SetTrigger("Reverse");
            }
            
            GetComponent<Rigidbody2D>().gravityScale *= -1;
            initialGravity *= -1;
            StartCoroutine(LowGravityWhenReversing());
            isReversed = !isReversed;


        }
    }
    private IEnumerator LowGravityWhenReversing()
    {
        GetComponent<Rigidbody2D>().gravityScale /= 3;
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody2D>().gravityScale = initialGravity;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            GetComponent<SpriteRenderer>().material.color = collision.gameObject.GetComponent<SpriteRenderer>().color * 20;
            //el número es la inttensidad del color (material.color.HDRintensity(URP))
            
        }
        if (collision.gameObject.layer == 7)
        {
            //si te pinchas o te caes te mueres (colisionas con la layer spikes(8));
            StartCoroutine(death());
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "CameraSizeChangerBig")
        {
            //cam.m_Lens.OrthographicSize = 30;
            camAnimator.SetBool("Shrink", false);
            camAnimator.SetBool("Grow", true);
        }
        if (collision.name == "CameraSizeChangerSmall")
        {
            //cam.m_Lens.OrthographicSize = 15;
            camAnimator.SetBool("Grow", false);
            camAnimator.SetBool("Shrink", true);
        }
        if (collision.gameObject.layer == 8)
        {
            lastCheckpoint = collision.gameObject;
        }
    }

    IEnumerator death()
    {
        //animacion del personaje de morirse
        GetComponent<Animator>().SetTrigger("DeathAnimation");
        Debug.Log("nos morimo");
        //transicion de pantalla al morirse
        uiAnimator.SetTrigger("DeathAnimation");
        //esperas medio segundo mientras se ejecuta la animacion de muerte y te tepea al ultimo checkpoint
        yield return new WaitForSeconds(0.50f);
        transform.position = new Vector2(lastCheckpoint.transform.position.x, lastCheckpoint.transform.position.y);
    }

}
