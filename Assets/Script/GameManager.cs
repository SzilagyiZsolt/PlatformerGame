using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Menük")]
    public GameObject pauseMenuUI;
    public GameObject gameUI;

    [Header("Játék Elemek")]
    public GameObject restartButton;
    public GameObject trapPrefab;
    public PlayerMovement playerScript;
    public int maxRounds = 7;

    [Header("Kulcs Rendszer")]
    public bool levelRequiresKey = true;
    public GameObject keyPrefab;
    public Transform manualKeyLocation;
    public float levelMinX = -10f;
    public float levelMaxX = 20f;
    public float keySpawnHeight = 10f;

    [Header("Biztonsági Zónák")]
    public float safeZoneRadius = 3.0f;     // Start körüli védett zóna
    public float safeGoalRadius = 3.0f;     // Cél körüli védett zóna
    public float minCeilingHeight = 4.0f;
    public float edgeCheckDistance = 1.0f;
    public float minTrapDistance = 1.5f;
    public float trapOffsetY = -0.2f;

    [Header("Útvonal Rögzítés (Breadcrumbs)")]
    public float historySampleRate = 0.01f; // Milyen sûrûn (mp) rögzítsünk
    public float minRecordDistance = 0.5f; // Minimum távolság az elõzõ ponttól

    [Header("HUD Elemek")]
    public TextMeshProUGUI roundText;
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    // --- BELSÕ VÁLTOZÓK ---
    private static List<Vector3> trapPositions = new List<Vector3>();
    private static int currentRound = 1;

    private bool isLevelFinished = false;
    private Vector3 startPosition;
    private Vector3 goalPosition;
    private bool hasKey = false;
    private bool isPaused = false;

    // Ebben gyûjtjük a játékos által bejárt ÉRVÉNYES pontokat
    private List<Vector3> validPositionsHistory = new List<Vector3>();
    private float nextSampleTime = 0f;

    void Start()
    {
        isLevelFinished = false;
        hasKey = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Lista törlése minden kör elején
        validPositionsHistory.Clear();

        if (playerScript != null) startPosition = playerScript.transform.position;

        // Megkeressük a célt a védelemhez
        GameObject goalObj = GameObject.FindGameObjectWithTag("Goal");
        if (goalObj != null) goalPosition = goalObj.transform.position;

        if (roundText != null) roundText.text = $"Round: {currentRound}/{maxRounds}";

        // UI alaphelyzet
        if (winPanel != null) winPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);

        // Csapdák lerakása
        if (trapPrefab != null)
        {
            foreach (Vector3 pos in trapPositions)
            {
                Vector3 spawnPos = new Vector3(pos.x, pos.y + trapOffsetY, pos.z);
                Instantiate(trapPrefab, spawnPos, Quaternion.identity);
            }
        }

        if (levelRequiresKey) SpawnKey();
    }

    void Update()
    {
        // Pause kezelés
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (isLevelFinished || playerScript == null) return;

        // --- ÚTVONAL RÖGZÍTÉS (A LÉNYEG!) ---
        if (Time.time >= nextSampleTime)
        {
            // Megnézzük, hogy az aktuális pozíció alatt van-e érvényes talaj
            Vector3? validPos = GetValidGroundPos(playerScript.transform.position);

            if (validPos.HasValue)
            {
                // Csak akkor adjuk hozzá, ha elég messze van az elõzõtõl (ne spameljük tele egy helyben állva)
                if (validPositionsHistory.Count == 0 ||
                    Vector3.Distance(validPos.Value, validPositionsHistory[validPositionsHistory.Count - 1]) > minRecordDistance)
                {
                    validPositionsHistory.Add(validPos.Value);
                }
            }

            nextSampleTime = Time.time + historySampleRate;
        }
    }

    // --- MENÜ FUNKCIÓK ---
    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        ResetStaticVariables();
        SceneManager.LoadScene(0);
    }

    // --- JÁTÉK LOGIKA ---

    public void CollectKey() { hasKey = true; }
    public bool IsKeyCollected() { return !levelRequiresKey || hasKey; }

    void SpawnKey()
    {
        if (keyPrefab == null) return;
        if (manualKeyLocation != null)
        {
            Instantiate(keyPrefab, manualKeyLocation.position, Quaternion.identity);
            return;
        }
        for (int i = 0; i < 30; i++)
        {
            float randomX = Random.Range(levelMinX, levelMaxX);
            // Itt is a javított függvényt használjuk
            Vector3? validPos = GetValidGroundPos(new Vector3(randomX, keySpawnHeight, 0));
            if (validPos.HasValue)
            {
                Instantiate(keyPrefab, validPos.Value + Vector3.up * 0.5f, Quaternion.identity);
                return;
            }
        }
    }

    // --- VALIDÁCIÓS FÜGGVÉNYEK ---

    Vector3? GetValidGroundPos(Vector3 searchPos)
    {
        // Kicsit fentrõl (0.5) indítjuk a sugarat, hogy biztosan lássa a talajt a lábunk alatt
        Vector2 origin = new Vector2(searchPos.x, searchPos.y + 0.5f);

        // Lefelé lövünk egy sugarat
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 10f, playerScript.groundLayer);

        // Debug vonal a Scene nézetben (piros)
        Debug.DrawRay(origin, Vector2.down * 2f, Color.red, 0.1f);

        if (hit.collider == null) return null; // Levegõben vagyunk

        // Ha találtunk talajt, megnézzük, hogy biztonságos-e a pont
        if (IsPositionSafe(hit.point))
        {
            return hit.point;
        }

        return null;
    }

    bool IsPositionSafe(Vector3 pos)
    {
        // 1. Start zóna védelme
        if (Vector3.Distance(pos, startPosition) < safeZoneRadius) return false;

        // 2. Cél zóna védelme (FONTOS!)
        if (goalPosition != Vector3.zero && Vector3.Distance(pos, goalPosition) < safeGoalRadius) return false;

        // 3. Másik csapda közelsége
        foreach (Vector3 existingTrap in trapPositions)
        {
            if (Vector3.Distance(pos, existingTrap) < minTrapDistance) return false;
        }

        // 4. Plafon ellenõrzés
        if (Physics2D.Raycast(pos, Vector2.up, minCeilingHeight, playerScript.groundLayer).collider != null) return false;

        // 5. Szakadék széle ellenõrzés
        Vector2 checkLeft = new Vector2(pos.x - edgeCheckDistance, pos.y + 0.5f);
        Vector2 checkRight = new Vector2(pos.x + edgeCheckDistance, pos.y + 0.5f);
        if (!Physics2D.Raycast(checkLeft, Vector2.down, 1.5f, playerScript.groundLayer) ||
            !Physics2D.Raycast(checkRight, Vector2.down, 1.5f, playerScript.groundLayer)) return false;

        return true;
    }

    public void GameOver()
    {
        if (isLevelFinished) return;
        StartCoroutine(AutoRestartSequence());
    }

    IEnumerator AutoRestartSequence()
    {
        yield return new WaitForSeconds(2f);
        RestartGame();
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

        // --- ÚJ KIVÁLASZTÁS A RÖGZÍTETT LISTÁBÓL ---
        if (validPositionsHistory.Count > 0)
        {
            // Választunk egy véletlenszerû pontot a listából
            // Mivel a listába eleve csak valid pontok kerültek be (IsPositionSafe-fel ellenõrizve),
            // ez a pont 100%-ig biztonságos lesz!
            int randomIndex = Random.Range(0, validPositionsHistory.Count);
            trapPositions.Add(validPositionsHistory[randomIndex]);

            Debug.Log($"Új csapda kiválasztva {validPositionsHistory.Count} rögzített pont közül.");
        }
        else
        {
            // Ha valamiért üres maradt a lista (pl. végig a levegõben voltál),
            // akkor NEM rakunk le csapdát, hogy ne rontsuk el a pályát.
            Debug.LogWarning("Nem sikerült érvényes útvonalat rögzíteni ebben a körben.");
        }

        currentRound++;
        if (currentRound > maxRounds) StartCoroutine(WinSequence());
        else
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator WinSequence()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasNextLevel = nextSceneIndex < SceneManager.sceneCountInBuildSettings;

        if (winText != null) winText.text = hasNextLevel ? "LEVEL COMPLETE!" : "YOU WON THE GAME!";
        if (playerScript != null) playerScript.gameObject.SetActive(false);

        yield return new WaitForSeconds(4);
        ResetStaticVariables();

        if (hasNextLevel) SceneManager.LoadScene(nextSceneIndex);
        else SceneManager.LoadScene(0);
    }

    private void ResetStaticVariables()
    {
        trapPositions.Clear();
        currentRound = 1;
        Time.timeScale = 1;
    }
}