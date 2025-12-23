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
    public GameObject deathEffect; // <--- EZ AZ ÚJ VÁLTOZÓ (Prefab)

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

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsJumping", !isGrounded);
        }

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

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (transform.position.y < fallThreshold) Die();
    }

    void Jump()
    {
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
        // --- EZT A RÉSZT TÖRÖLD KI VAGY KOMMENTELD KI: ---
        // if (collision.CompareTag("Goal")) FindAnyObjectByType<GameManager>().LevelComplete();
        // -------------------------------------------------

        // Csak a csapdát figyeljük itt, a célt a Goal.cs intézi!
        if (collision.CompareTag("Trap"))
        {
            Die();
        }
    }

    public void Die()
    {
        // Ha már halottak vagyunk, ne fusson le újra (hogy ne legyen dupla hang/effekt)
        if (isDead) return;

        isDead = true;

        // --- HALÁL EFFEKT LÉTREHOZÁSA ---
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Eltüntetjük a karaktert (hogy csak az effekt maradjon)
        // Nem Destroy-oljuk azonnal, mert akkor a script is leállna!
        // Csak a kinézetét kapcsoljuk le:
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        FindAnyObjectByType<GameManager>().GameOver();
        if (AudioManager.instance != null) AudioManager.instance.PlayDeath();
    }
}