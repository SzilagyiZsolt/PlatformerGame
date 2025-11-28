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

    // --- ÚJ VÁLTOZÓ ---
    [Header("Animáció")]
    public Animator animator; // Ide húzzuk be az Animatort

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

        // Ha elfelejtetted volna behúzni, megpróbálja automatikusan megtalálni
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- HIBAJAVÍTÁS: HA ÁLL AZ IDŐ (PAUSE), NE CSINÁLJON SEMMIT ---
        if (Time.timeScale == 0f) return;

        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            // Opcionális: Ha van halál animáció, itt játszanánk le
            return;
        }

        // --- Talaj ellenőrzés ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            jumpCounter = extraJumps;
        }

        // --- Mozgás ---
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Forgatás (Flip)
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // --- ANIMÁCIÓ FRISSÍTÉSE ---
        if (animator != null)
        {
            // Futás sebesség átadása (Ez már megvolt)
            animator.SetFloat("Speed", Mathf.Abs(moveInput));

            // --- EZT ADTUK HOZZÁ: ---
            // Ha NEM vagyunk a földön (!isGrounded), akkor ugrunk (true)
            // Ha a földön vagyunk (isGrounded), akkor nem ugrunk (false)
            animator.SetBool("IsJumping", !isGrounded);
        }

        // --- UGRÁS ---
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

        if (transform.position.y < fallThreshold)
        {
            Die();
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // --- HANG LEJÁTSZÁSA ---
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayJump();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            FindAnyObjectByType<GameManager>().LevelComplete();
        }
        else if (collision.CompareTag("Trap"))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        FindAnyObjectByType<GameManager>().GameOver();
        Destroy(gameObject);
    }
}