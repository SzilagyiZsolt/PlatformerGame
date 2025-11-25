using UnityEngine;

public class Goal : MonoBehaviour
{
    private SpriteRenderer sr;
    private GameManager gameManager;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        // Folyamatosan színezzük a kaput az állapotnak megfelelõen
        if (gameManager != null && sr != null)
        {
            if (gameManager.IsKeyCollected())
            {
                sr.color = Color.green; // NYITVA (Zöld)
            }
            else
            {
                sr.color = Color.red;   // ZÁRVA (Piros)
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            // CSAK AKKOR enged tovább, ha a kulcs megvan!
            if (gameManager != null && gameManager.IsKeyCollected())
            {
                gameManager.LevelComplete();
            }
            else
            {
                Debug.Log("Zárva! Keresd meg a kulcsot!");
            }
        }
    }
}