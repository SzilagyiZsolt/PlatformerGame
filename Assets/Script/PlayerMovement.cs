using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;       // Ezt állítsd be az Inspectorban (pl. 12-14)
    public int extraJumps = 1;
    public float fallThreshold = -10f;

    [Header("Ugrás Finomhangolás")]
    [Range(0, 1)] public float jumpCutMultiplier = 0.5f; // Mennyire vágja el az ugrást, ha elengeded a gombot (0.5 = felére)
    public float gravityScale = 2.5f; // Az alap gravitáció erőssége (Ezt használjuk a Rigidbody-n)

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

        // Beállítjuk a Rigidbody gravitációját a kód alapján, hogy itt tudd szabályozni
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- Talaj ellenőrzés ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Reseteljük az ugrásokat
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            jumpCounter = extraJumps;
        }

        // --- Mozgás ---
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Forgatás
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // --- UGRÁS (INDÍTÁS) ---
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

        // --- VÁLTOZTATHATÓ UGRÁSMAGASSÁG (EZ A LÉNYEG!) ---
        // Ha elengedjük a gombot ÉS éppen felfelé mozgunk...
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            // ...akkor a sebességet megfelezzük (levágjuk az ugrást)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (transform.position.y < fallThreshold)
        {
            Die();
        }
    }

    void Jump()
    {
        // Nullázzuk az Y sebességet a konzisztens ugrásért
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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