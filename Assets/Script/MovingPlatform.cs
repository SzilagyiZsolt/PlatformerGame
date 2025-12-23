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
    private void OnCollisionEnter2D(Collision2D collision) => CheckCollision(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckCollision(collision);

    private void CheckCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            bool isMovingDown = targetPos.y < transform.position.y;
            float relativeY = collision.transform.position.y - transform.position.y;

            if (isMovingDown && relativeY < killZoneY)
            {
                if (player != null)
                {
                    collision.transform.SetParent(null);
                    player.Die();
                }
            }
            else if (relativeY > 0f)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
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