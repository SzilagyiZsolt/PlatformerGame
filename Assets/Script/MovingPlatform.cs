using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    [Header("Halálos Zóna")]
    public float killZoneY = -0.1f;

    private Vector3 targetPos;
    private Rigidbody2D rb;
    private bool movingToB = true; // Ez követi az irányt

    private static Dictionary<string, PlatformData> platformMemory = new Dictionary<string, PlatformData>();

    [System.Serializable]
    public struct PlatformData
    {
        public Vector3 position;
        public bool movingToB;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ellenõrizzük, hogy van-e mentett adat
        if (platformMemory.ContainsKey(gameObject.name))
        {
            PlatformData data = platformMemory[gameObject.name];

            // Pozíció visszaállítása
            transform.position = data.position;
            // Rigidbody szinkronizálása azonnal, hogy ne ugráljon
            rb.position = data.position;

            // Irány visszaállítása
            movingToB = data.movingToB;

            Debug.Log($"<color=green>BETÖLTVE: {gameObject.name}</color> | Irány B felé: {movingToB}");
        }
        else
        {
            // Ha nincs mentés, alapértelmezett (felfelé/B felé)
            movingToB = true;
        }

        // Cél beállítása a (betöltött vagy alap) irány alapján
        if (pointA != null && pointB != null)
        {
            targetPos = movingToB ? pointB.position : pointA.position;
        }
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null) return;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Ha odaértünk a célhoz
        if (Vector2.Distance(rb.position, targetPos) < 0.1f)
        {
            movingToB = !movingToB; // Irányváltás
            targetPos = movingToB ? pointB.position : pointA.position;
        }
    }

    // OnDestroy helyett OnDisable-t használunk, ez biztonságosabb scene váltásnál
    void OnDisable()
    {
        SaveState();
    }

    void SaveState()
    {
        PlatformData data = new PlatformData();
        data.position = transform.position;
        data.movingToB = movingToB;

        if (platformMemory.ContainsKey(gameObject.name))
        {
            platformMemory[gameObject.name] = data;
        }
        else
        {
            platformMemory.Add(gameObject.name, data);
        }

        // Debug.Log($"MENTVE: {gameObject.name} | B felé tartott: {movingToB}");
    }

    public static void ResetAllPlatforms()
    {
        platformMemory.Clear();
        Debug.Log("<color=red>LIFT MEMÓRIA TÖRLÉS!</color>");
    }

    // --- ÜTKÖZÉS KEZELÉS (Változatlan) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Amikor a játékos RÁLÉP (vagy nekimegy)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Megnézzük, hogy a játékos a platform FELETT van-e (hogy ne tapadjon rá, ha alulról fejeli meg)
            // A contact point vizsgálata pontosabb, mint a transform pozíció kivonása
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Ha a normál vektor lefelé mutat (a platform szemszögébõl), azaz a játékos fentrõl érkezik
                // A contact.normal a játékos felé mutat. Ha Y > 0.5, akkor a játékos a platform tetején van.
                // DE VIGYÁZAT: A contact.normal iránya attól függ, melyik objektumot vizsgáljuk.
                // A Collision2D-ben a 'contact.normal' az ütközés felületére merõleges.

                // Egyszerûbb módszer: Ha a játékos talpa (Y min) magasabban van, mint a platform közepe.
                if (collision.transform.position.y > transform.position.y + 0.1f)
                {
                    collision.transform.SetParent(transform);
                    return; // Megvan a szülõ beállítás, mehetünk
                }
            }

            // Ha a "Kill Zone" logikát meg akarod tartani a lefelé mozgó lifteknél:
            bool isMovingDown = targetPos.y < transform.position.y;
            float relativeY = collision.transform.position.y - transform.position.y;

            if (isMovingDown && relativeY < killZoneY)
            {
                PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    collision.transform.SetParent(null); // Elengedjük, mielõtt meghal
                    player.Die();
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Ez biztosítja, hogy ha valamiért leesne a szülõ (pl. ugrás után), de még rajta áll, visszakerüljön
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.position.y > transform.position.y + 0.1f && collision.transform.parent != transform)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Csak akkor vesszük le a szülõt, ha az TÉNYLEG mi vagyunk
            // (Nehogy levegyük, ha átugrott egy másik mozgó platformra)
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }

            // FONTOS: Mivel a DontDestroyOnLoad miatt a játékos átkerülhet a DontDestroyOnLoad scene-be,
            // amikor levesszük a szülõt, érdemes lehet visszaállítani a Scene-be (opcionális, de ajánlott).
            // De az alap SetParent(null) is mûködik a legtöbb esetben.
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}