using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Menük (ÚJ)")]
    public GameObject pauseMenuUI; // A szünet menü panelje
    public GameObject gameUI;      // A játék közbeni feliratok (pl. Round számláló)

    [Header("Játék Elemek")]
    public GameObject restartButton; // Game Over gomb
    public GameObject trapPrefab;
    public PlayerMovement playerScript;
    public int maxRounds = 7;

    [Header("Kulcs Rendszer")]
    public bool levelRequiresKey = true;
    public GameObject keyPrefab;
    public Transform manualKeyLocation; // Fix pont

    // Random beállítások
    public float levelMinX = -10f;
    public float levelMaxX = 20f;
    public float keySpawnHeight = 10f;

    [Header("Idõzítés")]
    public float minTrapTime = 5.0f;
    public float maxTrapTime = 10.0f;

    [Header("Biztonsági Zónák")]
    public float safeZoneRadius = 3.0f;
    public float minCeilingHeight = 4.0f;
    public float edgeCheckDistance = 1.0f;
    public float minTrapDistance = 1.5f;
    public float trapOffsetY = -0.2f;

    [Header("HUD Elemek")]
    public TextMeshProUGUI roundText;
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    // --- BELSÕ VÁLTOZÓK ---
    private static List<Vector3> trapPositions = new List<Vector3>();
    private static int currentRound = 1;
    private Vector3 tempTrapPosition;
    private bool tempPosRecorded = false;
    private bool isLevelFinished = false;
    private Vector3 startPosition;
    private float currentTrapTime;
    private bool hasKey = false;

    // Pause állapot
    private bool isPaused = false;

    private List<Vector3> validPositionsHistory = new List<Vector3>();
    private float historySampleRate = 0.2f;
    private float nextSampleTime = 0f;

    void Start()
    {
        isLevelFinished = false;
        tempPosRecorded = false;
        hasKey = false;
        isPaused = false;
        Time.timeScale = 1f; // Biztos ami biztos, elindítjuk az idõt

        validPositionsHistory.Clear();
        currentTrapTime = Random.Range(minTrapTime, maxTrapTime);

        if (playerScript != null) startPosition = playerScript.transform.position;
        if (roundText != null) roundText.text = $"Round: {currentRound} / {maxRounds}";

        // Menük alaphelyzete
        if (winPanel != null) winPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);

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
        // --- PAUSE KEZELÉS (ÚJ) ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (isLevelFinished || playerScript == null) return;

        // Csapda rögzítés logika (a régi)
        if (!tempPosRecorded && Time.timeSinceLevelLoad >= currentTrapTime)
        {
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);
            if (groundPos.HasValue)
            {
                tempTrapPosition = groundPos.Value;
                tempPosRecorded = true;
            }
        }

        // History logika
        if (Time.time >= nextSampleTime)
        {
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);
            if (groundPos.HasValue) validPositionsHistory.Add(groundPos.Value);
            nextSampleTime = Time.time + historySampleRate;
        }
    }

    // --- MENÜ FUNKCIÓK ---

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        Time.timeScale = 1f; // Idõ újraindítása
        isPaused = false;
    }

    void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false); // Eltüntetjük a HUD-ot zavaró tényezõként
        Time.timeScale = 0f; // Idõ megállítása
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Fontos: vissza kell állítani az idõt kilépés elõtt!

        // Opcionális: Ha kilépsz a fõmenübe, törlöd a csapdákat? 
        // Általában igen, tiszta lappal kezdjen legközelebb.
        ResetStaticVariables();

        SceneManager.LoadScene(0); // A 0. index a MainMenu
    }

    // --- KULCS & JÁTÉK LOGIKA (Változatlan) ---

    public void CollectKey()
    {
        hasKey = true;
        // --- HANG ---
        if (AudioManager.instance != null) AudioManager.instance.PlayKeyPickup();
    }
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
            Vector3? validPos = GetValidGroundPos(new Vector3(randomX, keySpawnHeight, 0));
            if (validPos.HasValue)
            {
                Instantiate(keyPrefab, validPos.Value + Vector3.up * 0.5f, Quaternion.identity);
                return;
            }
        }
    }

    Vector3? GetValidGroundPos(Vector3 startSearchPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(startSearchPos, Vector2.down, 20f, playerScript.groundLayer);
        if (hit.collider == null) return null;
        if (IsPositionSafe(hit.point)) return hit.point;
        return null;
    }

    bool IsPositionSafe(Vector3 pos)
    {
        if (Vector3.Distance(pos, startPosition) < safeZoneRadius) return false;
        foreach (Vector3 existingTrap in trapPositions)
        {
            if (Vector3.Distance(pos, existingTrap) < minTrapDistance) return false;
        }
        if (Physics2D.Raycast(pos, Vector2.up, minCeilingHeight, playerScript.groundLayer).collider != null) return false;

        Vector2 checkLeft = new Vector2(pos.x - edgeCheckDistance, pos.y + 0.5f);
        Vector2 checkRight = new Vector2(pos.x + edgeCheckDistance, pos.y + 0.5f);
        if (!Physics2D.Raycast(checkLeft, Vector2.down, 1.5f, playerScript.groundLayer) ||
            !Physics2D.Raycast(checkRight, Vector2.down, 1.5f, playerScript.groundLayer)) return false;

        return true;
    }

    public void GameOver()
    {
        if (restartButton != null) restartButton.SetActive(true);
        // --- HANG ---
        if (AudioManager.instance != null) AudioManager.instance.PlayDeath();
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
        if (tempPosRecorded) trapPositions.Add(tempTrapPosition);
        else if (validPositionsHistory.Count > 0) trapPositions.Add(validPositionsHistory[Random.Range(0, validPositionsHistory.Count)]);

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
        if (gameUI != null) gameUI.SetActive(false); // HUD elrejtése ünnepléskor

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasNextLevel = nextSceneIndex < SceneManager.sceneCountInBuildSettings;

        if (winText != null) winText.text = hasNextLevel ? "LEVEL COMPLETE!" : "YOU WON THE GAME!";
        if (playerScript != null) playerScript.gameObject.SetActive(false);

        // --- HANG ---
        if (AudioManager.instance != null) AudioManager.instance.PlayWin();

        yield return new WaitForSeconds(4);
        ResetStaticVariables();

        if (hasNextLevel) SceneManager.LoadScene(nextSceneIndex);
        else SceneManager.LoadScene(0); // Vissza a Fõmenübe
    }

    private void ResetStaticVariables()
    {
        trapPositions.Clear();
        currentRound = 1;
        Time.timeScale = 1;
    }
}