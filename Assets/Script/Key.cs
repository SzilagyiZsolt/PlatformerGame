using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ha a játékos ér hozzá
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            // Szólunk a központnak, hogy megvan a kulcs
            FindAnyObjectByType<GameManager>().CollectKey();

            // Hangeffekt helye (késõbb): AudioSource.PlayClipAtPoint(...)

            // Eltüntetjük a kulcsot
            Destroy(gameObject);
        }
    }
}