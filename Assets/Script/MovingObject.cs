using UnityEngine;
using System.Collections.Generic;

public class MovingObject : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public Transform pointA; // Ahonnan indul
    public Transform pointB; // Ahová megy
    public float speed = 3f; // Milyen gyorsan mozogjon

    [Header("Forgás")]
    public float rotationSpeed = 360f; // Milyen gyorsan forogjon a fûrész

    [Header("Vizuális Extrák")]
    public LineRenderer lineRenderer; // <--- Húzd be ide a Line Renderert!

    private Vector3 targetPos;

    // --- MEMÓRIA RENDSZER ---
    private static Dictionary<string, SawData> sawMemory = new Dictionary<string, SawData>();

    [System.Serializable]
    public struct SawData
    {
        public Vector3 position;
        public Vector3 target;
    }

    void Start()
    {
        // Vonal beállítása induláskor
        if (lineRenderer != null && pointA != null && pointB != null)
        {
            lineRenderer.positionCount = 2; // Két végpontja van a vonalnak
            lineRenderer.useWorldSpace = true; // Világkoordinátákat használunk
            lineRenderer.SetPosition(0, pointA.position); // A pont
            lineRenderer.SetPosition(1, pointB.position); // B pont
        }

        // Memória betöltése vagy alaphelyzet
        if (sawMemory.ContainsKey(gameObject.name))
        {
            SawData data = sawMemory[gameObject.name];
            transform.position = data.position;
            targetPos = data.target;
        }
        else
        {
            if (pointB != null)
            {
                targetPos = pointB.position;
            }
        }
    }

    void Update()
    {
        // Forgatás
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (pointA == null || pointB == null) return;

        // Ha mozgathatóak a pontok játék közben is, akkor folyamatosan frissítjük a vonalat
        // (Ha statikusak a pontok, ez a rész kivehetõ/optimalizálható)
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, pointA.position);
            lineRenderer.SetPosition(1, pointB.position);
        }

        // Pozíció mozgatása
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Irányváltás
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (targetPos == pointA.position) targetPos = pointB.position;
            else targetPos = pointA.position;
        }
    }

    void OnDestroy()
    {
        SawData data = new SawData();
        data.position = transform.position;
        data.target = targetPos;

        if (sawMemory.ContainsKey(gameObject.name))
        {
            sawMemory[gameObject.name] = data;
        }
        else
        {
            sawMemory.Add(gameObject.name, data);
        }
    }

    public static void ResetAllSaws()
    {
        sawMemory.Clear();
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
    }
}