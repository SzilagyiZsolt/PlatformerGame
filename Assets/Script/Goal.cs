using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ha a játékos (PlayerMovement scripttel rendelkezõ tárgy) ért hozzá
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            // Megkeressük a GameManagert és szólunk neki, hogy nyertünk
            FindAnyObjectByType<GameManager>().LevelComplete();
        }
    }
}