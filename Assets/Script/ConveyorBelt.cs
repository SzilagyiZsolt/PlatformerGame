using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Beállítások")]
    public float speed = 3f; // Pozitív = Jobbra, Negatív = Balra

    [Header("Vizuális Finomhangolás")]
    public float visualMultiplier = -0.2f;

    private Material material;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            material = sr.material;
        }
    }

    void Update()
    {
        // --- VIZUÁLIS RÉSZ (Animáció) ---
        if (material != null)
        {
            float offset = Time.time * speed * visualMultiplier;
            material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        }
    }

    // --- FIZIKAI RÉSZ (Lendület átadása) ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Ahelyett, hogy közvetlenül mozgatnánk (Translate),
                // beállítjuk a játékos "külsõ lendület" változóját a szalag sebességére.
                // Mivel a PlayerMovement-ben ez folyamatosan csökken (Lerp), 
                // itt minden frame-ben "újratöltjük", amíg rajta áll.
                player.externalMomentum = speed;
            }
        }
    }
}