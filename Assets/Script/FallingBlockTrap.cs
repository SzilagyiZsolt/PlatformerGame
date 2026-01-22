using UnityEngine;
using System.Collections;

public class FallingBlockTrap : MonoBehaviour
{
    [Header("Beállítások")]
    public float detectionLength = 10f;
    public float fallSpeed = 5f;
    public float returnSpeed = 2f;
    public float resetDelay = 2f;

    [Header("Rétegek")]
    public LayerMask detectionLayer;

    private bool isFalling = false;
    private bool isResetting = false;

    private Rigidbody2D rb;
    private Vector3 startPos;
    private BoxCollider2D col;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        startPos = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;

        // Zároljuk az X mozgást és a forgást, hogy ne lehessen ellökni
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (!isFalling && !isResetting)
        {
            DetectPlayer();
        }
    }

    void DetectPlayer()
    {
        Vector2 rayStart = new Vector2(transform.position.x, transform.position.y - (col.size.y / 2) - 0.1f);
        Debug.DrawRay(rayStart, Vector2.down * detectionLength, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, detectionLength, detectionLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                StartCoroutine(FallSequence());
            }
        }
    }

    IEnumerator FallSequence()
    {
        isFalling = true;

        // --- NINCS REMEGÉS, AZONNAL ZUHANÁS ---
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallSpeed;

        // Megvárjuk, amíg megáll (becsapódik)
        yield return new WaitForSeconds(0.1f);
        while (rb.linearVelocity.magnitude > 0.01f)
        {
            yield return null;
        }

        // Várakozás lent
        yield return new WaitForSeconds(resetDelay);

        // Visszatérés
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        isResetting = true;
        isFalling = false;

        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPos;
        isResetting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Csak akkor vizsgáljuk, ha a folyamat elindult
            if (isFalling)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // Ha a normálvektor felfelé mutat, akkor a játékos ALULRÓL kapta az ütést
                    if (contact.normal.y > 0.5f)
                    {
                        collision.gameObject.GetComponent<PlayerMovement>().Die();
                        return;
                    }
                }
            }
        }
    }
}