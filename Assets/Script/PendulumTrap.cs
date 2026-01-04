using UnityEngine;

public class PendulumTrap : MonoBehaviour
{
    [Header("Inga Beállítások")]
    [Tooltip("Mekkora szögben lengjen ki maximálisan jobbra/balra (pl. 45 fok).")]
    public float swingAngle = 45f;

    [Tooltip("Milyen gyorsan lengjen.")]
    public float speed = 2f;

    [Tooltip("Idõbeli eltolás. Ha több ingát raksz egymás mellé, ezzel állíthatod be, hogy ne egyszerre mozogjanak.")]
    public float timeOffset = 0f;

    void Update()
    {
        // Kiszámoljuk az aktuális szöget az idõ és a szinusz függvény alapján
        // A Time.time folyamatosan nõ, a Sin pedig -1 és 1 között hullámzik
        float currentAngle = swingAngle * Mathf.Sin((Time.time + timeOffset) * speed);

        // A Z tengelyen forgatjuk az objektumot (2D-ben ez a forgás)
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
}