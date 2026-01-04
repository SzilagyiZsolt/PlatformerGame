using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RotatingTrap : MonoBehaviour
{
    [Header("Beállítások")]
    [Tooltip("Milyen gyorsan forogjon (fok/másodperc).")]
    public float rotationSpeed = 150f;

    [Tooltip("Ha be van pipálva, az óramutató járásával megegyezõen (jobbra) indul.")]
    public bool startClockwise = false;

    [Header("Nehezítés")]
    [Tooltip("Ha ez 0-nál nagyobb, akkor ennyi másodpercenként automatikusan irányt vált.")]
    public float changeInterval = 0f;

    // Belsõ változók
    private int currentDirection = 1;
    private float timer;

    // Statikus memória
    private static Dictionary<string, TrapData> savedTrapStates = new Dictionary<string, TrapData>();
    private static bool isQuitting = false; // Figyeljük, hogy kilépés történik-e

    struct TrapData
    {
        public Quaternion rotation;
        public int direction;
        public float timer;
    }

    void Start()
    {
        // Alaphelyzet beállítása
        currentDirection = startClockwise ? -1 : 1;
        timer = 0f;

        string id = GetTrapID();

        // Ha van mentett állapot, töltsük be!
        if (savedTrapStates.ContainsKey(id))
        {
            TrapData data = savedTrapStates[id];
            transform.rotation = data.rotation;
            currentDirection = data.direction;
            timer = data.timer;
        }
    }

    void Update()
    {
        if (changeInterval > 0)
        {
            timer += Time.deltaTime;
            if (timer >= changeInterval)
            {
                ToggleDirection();
                timer = 0f;
            }
        }

        transform.Rotate(0, 0, rotationSpeed * currentDirection * Time.deltaTime);
    }

    // Ez a függvény jelzi, ha a játékos kilép a programból (ilyenkor ne mentsünk)
    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        // Ha kilépünk a játékból, ne mentsük el az állapotot, mert felesleges
        if (isQuitting) return;

        // Minden más esetben (pl. pálya újratöltés, halál) MENTSÜNK!
        string id = GetTrapID();
        TrapData data;
        data.rotation = transform.rotation;
        data.direction = currentDirection;
        data.timer = timer;

        if (savedTrapStates.ContainsKey(id))
        {
            savedTrapStates[id] = data;
        }
        else
        {
            savedTrapStates.Add(id, data);
        }
    }

    public void ToggleDirection()
    {
        currentDirection *= -1;
    }

    string GetTrapID()
    {
        // Egyedi azonosító a pálya neve és a csapda pozíciója alapján
        return SceneManager.GetActiveScene().name + "_" + transform.position.ToString();
    }

    // Ezt kell hívni a FÕMENÜBEN a játék indításakor!
    public static void ResetAllTraps()
    {
        savedTrapStates.Clear();
        isQuitting = false; // Fontos: reseteljük a kilépés jelzõt is új játéknál
    }
}