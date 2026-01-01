using UnityEngine;
using System.Collections;

public class JumpPad : MonoBehaviour
{
    [Header("Beállítások")]
    public float bounceForce = 15f; // Milyen magasra dobjon (az ugr�s kb. 1.5x-ese legyen)

    [Header("Grafika")]
    public Sprite springIdle;      // Nyugalmi �llapot (spring.png)
    public Sprite springActive;    // Kipattant �llapot (spring_out.png)
    public float resetTime = 0.5f; // Mennyi id� ut�n �lljon vissza

    [Header("Hang")]
    public AudioClip bounceSound;  // Hang, amikor r�ugrasz

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (springIdle != null) spriteRenderer.sprite = springIdle;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Megn�zz�k, hogy a j�t�kos esik-e (lefel� mozog a sebess�ge)
            // �gy elker�lj�k, hogy akkor is feldobjon, ha oldalr�l m�sz neki, de felfel� ugrasz.
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Csak akkor dobjuk fel, ha �pp lefel� esik, vagy nagyon kicsi a f�gg�leges sebess�ge
                if (rb.linearVelocity.y <= 0.1f)
                {
                    PerformBounce(rb);
                }
            }
        }
    }

    void PerformBounce(Rigidbody2D playerRb)
    {
        // 1. Meg�ll�tjuk a zuhan�st, hogy a rug� ereje mindig konzisztens legyen
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);

        // 2. Felfel� l�k�s (Impulse = hirtelen er�)
        playerRb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

        // 3. Hang lej�tsz�sa (a k�zponti AudioManager-en kereszt�l)
        if (bounceSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.sfxSource.PlayOneShot(bounceSound);
        }

        // 4. Anim�ci� ind�t�sa
        StopAllCoroutines();
        StartCoroutine(SpringAnimation());
    }

    IEnumerator SpringAnimation()
    {
        // K�pcsere a kipattant verzi�ra
        if (springActive != null) spriteRenderer.sprite = springActive;

        // V�runk egy picit
        yield return new WaitForSeconds(resetTime);

        // Vissza az eredeti k�pre
        if (springIdle != null) spriteRenderer.sprite = springIdle;
    }
}