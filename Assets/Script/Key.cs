using UnityEngine;

public class Key : MonoBehaviour
{
    // Ide húzzuk be az effektet (Prefabot)
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            FindAnyObjectByType<GameManager>().CollectKey();

            // --- EFFEKT LÉTREHOZÁSA ---
            if (pickupEffect != null)
            {
                // Létrehozzuk az effektet a kulcs pozícióján
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Hang (ha nincs az AudioManagerben, itt is szólhatna, de ott már megvan)
            if (AudioManager.instance != null) AudioManager.instance.PlayKeyPickup();

            Destroy(gameObject);
        }
    }
}