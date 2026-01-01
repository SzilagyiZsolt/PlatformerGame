using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [Header("Beállítások")]
    public float fallDelay = 1.0f;    // Mennyi idõ után essen le (miután ráálltál)
    public float destroyDelay = 2.0f; // Mennyi idõvel a zuhanás után tûnjön el
    public float shakeAmount = 0.05f; // Mennyire remegjen

    private Rigidbody2D rb;
    private Vector3 originalPos;
    private bool isFalling = false;
    private bool isShaking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPos = transform.position; // Megjegyezzük, hol volt
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Csak akkor indul be, ha a játékos ér hozzá, és felülrõl (kb.)
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            // Ellenõrizzük, hogy a játékos felülrõl érkezett-e (hogy ne essen le, ha csak alulról kifejeled)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // A játékos lefelé nyomja a platformot
                {
                    StartCoroutine(FallRoutine());
                    break;
                }
            }
        }
    }

    IEnumerator FallRoutine()
    {
        isFalling = true;
        isShaking = true;
        float timer = 0f;

        // --- REMEGÉS FÁZIS ---
        while (timer < fallDelay)
        {
            timer += Time.deltaTime;
            // Kicsit elmozdítjuk véletlenszerû irányba az eredeti helyéhez képest
            transform.position = originalPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            yield return null;
        }

        isShaking = false;
        transform.position = originalPos; // Visszaugrunk középre a zuhanás elõtt

        // --- ZUHANÁS FÁZIS ---
        // Bekapcsoljuk a fizikát, hogy leessen
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f; // Gyorsan zuhanjon

        // Kikapcsoljuk az ütközést a többi tárggyal (opcionális, ha azt akarod, átessen a padlón)
        // GetComponent<Collider2D>().isTrigger = true; 

        // --- ELTÛNÉS ---
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false); // Eltüntetjük (a következõ körben a Scene újratöltés visszahozza)
    }
}