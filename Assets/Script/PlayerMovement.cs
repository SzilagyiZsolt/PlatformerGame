using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // �ll�that� sebess�g �s ugr�er� a Unity Inspector ablak�ban
    public float moveSpeed = 5f;        // Sebess�g Unit/m�sodpercben (5 Unit/m�sodperc)
    public float jumpForce = 10f;       // Ugr�er� (gravit�ci�t�l f�gg�en �ll�tsd)

    // Referencia a Rigidbody 2D komponensre
    private Rigidbody2D rb;

    // A talaj ellen�rz�s�hez sz�ks�ges v�ltoz�k
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.1f; // A vizsg�lt k�r sugara

    void Start()
    {
        // A Rigidbody 2D komponens beolvas�sa indul�skor
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Talaj ellen�rz�se
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // ------------------
        // 1. V�zszintes Mozg�s
        // ------------------
        float moveInput = Input.GetAxisRaw("Horizontal"); // -1 (bal) �s 1 (jobb) k�z�tti �rt�k
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // ------------------
        // 2. Ugr�s
        // ------------------
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Er� (Force) alkalmaz�sa a y-tengely ment�n
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}
