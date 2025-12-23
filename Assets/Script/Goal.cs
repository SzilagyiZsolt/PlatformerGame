using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Képek beállítása")]
    public Sprite closedSprite; // Ide húzd be a "ZÁRT" állapot képét (pl. piros zászló)
    public Sprite openSprite;   // Ide húzd be a "NYITOTT" állapot képét (pl. zöld zászló)

    private SpriteRenderer sr;
    private GameManager gameManager;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        gameManager = FindAnyObjectByType<GameManager>();

        // Visszaállítjuk a színezést fehérre, hogy a sprite-ok eredeti színe látszódjon
        // (Mert a régi kód elszínezte pirosra/zöldre a képet)
        sr.color = Color.white;
    }

    void Update()
    {
        if (gameManager != null && sr != null)
        {
            // Ellenõrizzük a kulcs állapotát
            if (gameManager.IsKeyCollected())
            {
                // HA NYITVA: Lecseréljük a képet a nyitott verzióra
                // (A feltétel azért kell, hogy ne cserélgesse minden képkockában feleslegesen)
                if (sr.sprite != openSprite)
                {
                    sr.sprite = openSprite;
                }
            }
            else
            {
                // HA ZÁRVA: Lecseréljük a képet a zárt verzióra
                if (sr.sprite != closedSprite)
                {
                    sr.sprite = closedSprite;
                }
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
                // Ide tehetünk egy kis visszajelzést (opcionális)
                Debug.Log("Zárva! Keresd meg a kulcsot!");
            }
        }
    }
}