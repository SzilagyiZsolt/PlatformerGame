using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("Kapcsolat")]
    public Teleporter destination; // Húzd be ide a MÁSIK kapu objektumát!

    [Header("Beállítások")]
    public float cooldown = 1.0f;    // Mennyi ideig ne teleportáljon újra (hogy ne ragadjon be a két kapu közé)
    public bool keepMomentum = true; // Megmaradjon-e a játékos sebessége? (True = Portal stílus)

    [Header("Effektek (Opcionális)")]
    public GameObject teleportEffect; // Particle System prefab, ha van
    public AudioClip teleportSound;   // Hang, ha van

    private bool isReady = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Csak akkor teleportálunk, ha a kapu készen áll és a Játékos lépett bele
        if (isReady && collision.CompareTag("Player"))
        {
            if (destination != null)
            {
                StartCoroutine(TeleportPlayer(collision.gameObject));
            }
            else
            {
                Debug.LogWarning("Nincs beállítva célállomás (Destination) ennek a teleportnak!");
            }
        }
    }

    IEnumerator TeleportPlayer(GameObject player)
    {
        // 1. A célállomást "kikapcsoljuk" egy pillanatra, hogy ne küldje vissza azonnal a játékost
        destination.StartCooldown();

        // 2. Effekt és Hang lejátszása (Indulás)
        PlayEffects(transform.position);

        // 3. Játékos áthelyezése
        // Fontos: Azonnal átrakjuk a pozícióját a célállomáséra
        player.transform.position = destination.transform.position;

        // 4. Effekt lejátszása (Érkezés)
        PlayEffects(destination.transform.position);

        // 5. Sebesség kezelése
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && !keepMomentum)
        {
            rb.linearVelocity = Vector2.zero; // Ha false, akkor megállítjuk a játékost érkezéskor
        }

        yield return null;
    }

    // Ezt a függvényt hívja a másik kapu, amikor küld valakit
    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        isReady = false;
        yield return new WaitForSeconds(cooldown);
        isReady = true;
    }

    void PlayEffects(Vector3 position)
    {
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, position, Quaternion.identity);
        }

        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, position);
        }
    }
}