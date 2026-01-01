using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int extraJumps = 1;
    public float fallThreshold = -10f;

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

        // --- TALAJ ÉRZÉKELÉS ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            CreateDust();
        }

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            jumpCounter = extraJumps;
        }

        // --- MOZGÁS ÉS LENDÜLET LOGIKA (EZT JAVÍTOTTUK) ---
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 1. A lendület folyamatos csökkentése
        externalMomentum = Mathf.Lerp(externalMomentum, 0, Time.deltaTime * momentumDrag);

        // --- ÚJ RÉSZ: A "VÁGÁS" ---
        // Ha már nagyon kicsi a szám (pl. kisebb mint 0.1), akkor legyen simán 0.
        // Ezzel megáll a "számolás".
        if (Mathf.Abs(externalMomentum) < 0.1f)
        {
            externalMomentum = 0f;
        }
        // --------------------------

        // 2. A sebesség kiszámítása: (Gombnyomás * Sebesség) + (Szalag lendület)
        rb.linearVelocity = new Vector2((moveInput * moveSpeed) + externalMomentum, rb.linearVelocity.y);

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

        // Ugrás levágása (ha elengedi a gombot)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (transform.position.y < fallThreshold) Die();
    }

    void Jump()
    {
        // Ugrás előtt nullázzuk az Y sebességet a konzisztens ugrásmagasságért
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
        // Csak a csapdát figyeljük itt, a célt a Goal.cs intézi!
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