using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Játék Elemek")]
    public GameObject restartButton;
    public GameObject trapPrefab;
    public PlayerMovement playerScript;
    public int maxRounds = 7;

    [Header("Idõzítés (Random)")]
    public float minTrapTime = 5.0f;
    public float maxTrapTime = 10.0f;

    [Header("Biztonsági Zónák")]
    public float safeZoneRadius = 3.0f;
    public float minCeilingHeight = 4.0f;
    public float edgeCheckDistance = 1.0f;
    public float minTrapDistance = 1.5f;
    public float trapOffsetY = -0.2f;

    [Header("UI Elemek")]
    public TextMeshProUGUI roundText;
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    // --- STATIKUS VÁLTOZÓK ---
    private static List<Vector3> trapPositions = new List<Vector3>();
    private static int currentRound = 1;

    // --- IDEIGLENES VÁLTOZÓK ---
    private Vector3 tempTrapPosition;
    private bool tempPosRecorded = false;
    private bool isLevelFinished = false;
    private Vector3 startPosition;
    private float currentTrapTime;

    // --- NAPLÓZÁS ---
    private List<Vector3> validPositionsHistory = new List<Vector3>();
    private float historySampleRate = 0.2f;
    private float nextSampleTime = 0f;

    void Start()
    {
        isLevelFinished = false;
        tempPosRecorded = false;
        validPositionsHistory.Clear();

        currentTrapTime = Random.Range(minTrapTime, maxTrapTime);
        Debug.Log($"Célzott idõ: {currentTrapTime:F2} mp");

        if (playerScript != null)
        {
            startPosition = playerScript.transform.position;
        }

        if (roundText != null) roundText.text = $"Round: {currentRound} / {maxRounds}";
        if (winPanel != null) winPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        if (trapPrefab != null)
        {
            foreach (Vector3 pos in trapPositions)
            {
                Vector3 spawnPos = new Vector3(pos.x, pos.y + trapOffsetY, pos.z);
                Instantiate(trapPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    void Update()
    {
        if (isLevelFinished || playerScript == null) return;

        // --- 1. IDÕZÍTETT RÖGZÍTÉS ---
        if (!tempPosRecorded && Time.timeSinceLevelLoad >= currentTrapTime)
        {
            // Megpróbálunk talajt találni a játékos ALATT
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);

            if (groundPos.HasValue) // Ha találtunk érvényes talajt
            {
                tempTrapPosition = groundPos.Value;
                tempPosRecorded = true;
                Debug.Log("SIKER! Csapda rögzítve (Levegõbõl vetítve is).");
            }
        }

        // --- 2. FOLYAMATOS NAPLÓZÁS (History) ---
        if (Time.time >= nextSampleTime)
        {
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);

            if (groundPos.HasValue)
            {
                validPositionsHistory.Add(groundPos.Value);
            }
            nextSampleTime = Time.time + historySampleRate;
        }
    }

    // Ez az ÚJ FÜGGVÉNY a lelke mindennek!
    // Megkeresi a földet a játékos alatt, és ellenõrzi, hogy jó-e a hely.
    Vector3? GetValidGroundPos(Vector3 playerPos)
    {
        // 1. RAYCAST LEFELÉ (max 20 méter mélyre)
        RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.down, 20f, playerScript.groundLayer);

        // Ha nem találtunk földet (pl. szakadék felett vagyunk), akkor NULL-t adunk vissza
        if (hit.collider == null) return null;

        Vector3 candidatePos = hit.point; // Ez a pont a talaj teteje

        // 2. VALIDÁCIÓ (Most már a TALÁLATI PONTOT vizsgáljuk)
        if (IsPositionSafe(candidatePos))
        {
            return candidatePos;
        }

        return null;
    }

    // Ez ellenõrzi a szabályokat (távolság, plafon, stb.)
    bool IsPositionSafe(Vector3 pos)
    {
        // A. Start zóna ellenõrzés
        if (Vector3.Distance(pos, startPosition) < safeZoneRadius) return false;

        // B. Más csapdák közelsége
        foreach (Vector3 existingTrap in trapPositions)
        {
            if (Vector3.Distance(pos, existingTrap) < minTrapDistance) return false;
        }

        // C. Plafon ellenõrzés (Innen felfelé lövünk)
        RaycastHit2D hitCeiling = Physics2D.Raycast(pos, Vector2.up, minCeilingHeight, playerScript.groundLayer);
        if (hitCeiling.collider != null) return false;

        // D. Szakadék széle ellenõrzés
        Vector2 checkLeft = new Vector2(pos.x - edgeCheckDistance, pos.y + 0.5f);
        Vector2 checkRight = new Vector2(pos.x + edgeCheckDistance, pos.y + 0.5f);

        bool groundLeft = Physics2D.Raycast(checkLeft, Vector2.down, 1.5f, playerScript.groundLayer);
        bool groundRight = Physics2D.Raycast(checkRight, Vector2.down, 1.5f, playerScript.groundLayer);

        if (!groundLeft || !groundRight) return false;

        return true;
    }

    public void GameOver()
    {
        if (restartButton != null) restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        ResetStaticVariables();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LevelComplete()
    {
        if (isLevelFinished) return;
        isLevelFinished = true;

        if (tempPosRecorded)
        {
            trapPositions.Add(tempTrapPosition);
        }
        else
        {
            // VÉSZTERV: Ha még a levegõ-vizsgálattal sem sikerült (nagyon ritka),
            // veszünk egyet a naplóból.
            if (validPositionsHistory.Count > 0)
            {
                int randomIndex = Random.Range(0, validPositionsHistory.Count);
                trapPositions.Add(validPositionsHistory[randomIndex]);
                Debug.Log("Vészterv: Random pozíció a naplóból.");
            }
            else
            {
                Debug.LogWarning("HIHETETLEN: Egész körben nem volt érvényes hely!");
            }
        }

        currentRound++;

        if (currentRound > maxRounds)
        {
            StartCoroutine(WinSequence());
        }
        else
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator WinSequence()
    {
        if (winPanel != null) winPanel.SetActive(true);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        bool hasNextLevel = nextSceneIndex < SceneManager.sceneCountInBuildSettings;

        if (winText != null)
        {
            winText.text = hasNextLevel ? "LEVEL COMPLETE!" : "YOU WON THE GAME!";
        }

        if (playerScript != null)
        {
            playerScript.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(4);

        ResetStaticVariables();

        if (hasNextLevel)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private void ResetStaticVariables()
    {
        trapPositions.Clear();
        currentRound = 1;
        Time.timeScale = 1;
    }
}