using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int extraJumps = 1;
    public float fallThreshold = -10f;

    [Header("Jég Mechanika")]
    public float acceleration = 60f;      // Normál talajon milyen gyorsan éri el a max sebességet (legyen magas!)
    public float deceleration = 60f;      // Normál talajon milyen gyorsan áll meg
    public float iceAcceleration = 8f;    // Jégen nehezebb elindulni (kisebb érték)
    public float iceDeceleration = 2f;    // Jégen nagyon lassan áll meg (csúszik)
    private bool onIce = false;           // Belső változó: épp jégen vagyunk-e?

    [Header("Ugrás Finomhangolás")]
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f;
    public float gravityScale = 2.5f;

    [Header("Animáció & Effektek")]
    public Animator animator;
    public ParticleSystem dust;
    public GameObject deathEffect;

    [Header("Külső Hatások")]
    public float externalMomentum; // Ezt tölti fel a szalag
    public float momentumDrag = 5f; // Milyen gyorsan fogyjon el a lendület

    private Rigidbody2D rb;
    public bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;

    private int jumpCounter;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpCounter = extraJumps;
        rb.gravityScale = gravityScale;
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- TALAJ ÉRZÉKELÉS ÉS JÉG DETEKTÁLÁS ---
        bool wasGrounded = isGrounded;

        // Elmentjük a talaj colliderét, hogy megnézzük a Tag-jét
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isGrounded = groundCollider != null;

        // Megnézzük, hogy a talaj, amin állunk, jég-e
        if (isGrounded)
        {
            if (groundCollider.CompareTag("Ice"))
            {
                onIce = true;
            }
            else
            {
                onIce = false;
            }
        }
        // Ha levegőben vagyunk, megőrizzük az utolsó állapotot (hogy ugrás közben is megmaradjon a lendület/irányítás jellege)
        // Vagy beállíthatod false-ra, ha levegőben mindig normál irányítást akarsz: onIce = false;

        if (isGrounded && !wasGrounded)
        {
            CreateDust();
        }

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            jumpCounter = extraJumps;
        }

        // --- MOZGÁS LOGIKA (JÉG + NORMÁL) ---
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 1. Külső lendület (szalag) csökkentése
        externalMomentum = Mathf.Lerp(externalMomentum, 0, Time.deltaTime * momentumDrag);
        if (Mathf.Abs(externalMomentum) < 0.1f)
        {
            externalMomentum = 0f;
        }

        // 2. Célsebesség kiszámítása
        float targetSpeed = moveInput * moveSpeed;

        // 3. Gyorsulás mértékének kiválasztása (Jég vs Normál)
        float accelRate;
        if (onIce)
        {
            // Ha a játékos nyom gombot (van targetSpeed), akkor gyorsulunk, ha nem, akkor csúszunk (lassulunk)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? iceAcceleration : iceDeceleration;
        }
        else
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }

        // 4. A sebesség alkalmazása Inerciával (MoveTowards)
        // Kivonjuk a külső lendületet, hogy csak a játékos saját sebességét módosítsuk
        float currentOwnSpeed = rb.linearVelocity.x - externalMomentum;

        // Fokozatosan közelítjük a jelenlegi sebességet a célsebességhez
        float newOwnSpeed = Mathf.MoveTowards(currentOwnSpeed, targetSpeed, accelRate * Time.deltaTime);

        // Visszaírjuk a Rigidbody-ba: (Saját új sebesség) + (Külső lendület)
        rb.linearVelocity = new Vector2(newOwnSpeed + externalMomentum, rb.linearVelocity.y);

        // ----------------------------------------------------

        // Forgatás
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Animáció
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsJumping", !isGrounded);
        }

        // Ugrás
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (jumpCounter > 0)
            {
                Jump();
                jumpCounter--;
            }
        }

        // Ugrás levágása
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (transform.position.y < fallThreshold) Die();
    }

    void Jump()
    {
        // Ugrás előtt nullázzuk az Y sebességet
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        CreateDust();
        if (AudioManager.instance != null) AudioManager.instance.PlayJump();
    }

    void CreateDust()
    {
        if (dust != null) dust.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        FindAnyObjectByType<GameManager>().GameOver();
        if (AudioManager.instance != null) AudioManager.instance.PlayDeath();
    }
}