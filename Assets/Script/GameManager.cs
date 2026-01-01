using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ... (A változók és a Start/Update változatlanok maradnak) ...
    [Header("UI Menük")]
    public GameObject pauseMenuUI;
    public GameObject gameUI;
    public GameObject restartButton;
    public GameObject trapPrefab;
    public PlayerMovement playerScript;
    public int maxRounds = 7;
    public bool levelRequiresKey = true;
    public GameObject keyPrefab;
    public Transform manualKeyLocation;
    public float levelMinX = -10f;
    public float levelMaxX = 20f;
    public float keySpawnHeight = 10f;
    public float safeZoneRadius = 3.0f;
    public float safeGoalRadius = 3.0f;
    public float minCeilingHeight = 4.0f;
    public float edgeCheckDistance = 1.0f;
    public float minTrapDistance = 1.5f;
    public float trapOffsetY = -0.2f;
    public float historySampleRate = 0.01f;
    public float minRecordDistance = 0.5f;
    public TextMeshProUGUI roundText;
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    private static List<Vector3> trapPositions = new List<Vector3>();
    private static int currentRound = 1;
    private bool isLevelFinished = false;
    private Vector3 startPosition;
    private Vector3 goalPosition;
    private bool hasKey = false;
    private bool isPaused = false;
    private List<Vector3> validPositionsHistory = new List<Vector3>();
    private float nextSampleTime = 0f;

    void Start()
    {
        isLevelFinished = false;
        hasKey = false;
        isPaused = false;
        Time.timeScale = 1f;
        validPositionsHistory.Clear();

        if (playerScript != null) startPosition = playerScript.transform.position;
        GameObject goalObj = GameObject.FindGameObjectWithTag("Goal");
        if (goalObj != null) goalPosition = goalObj.transform.position;

        if (roundText != null)
        {
            int diff = PlayerPrefs.GetInt("Difficulty", 1);
            string diffText = diff == 0 ? "(Normal)" : "(Hard)";
            roundText.text = $"Round:{currentRound}/{maxRounds} {diffText}";
        }

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame(); else PauseGame();
        }
        if (isLevelFinished || playerScript == null) return;

        if (Time.time >= nextSampleTime)
        {
            if (playerScript.transform.parent != null)
            {
                nextSampleTime = Time.time + historySampleRate;
                return;
            }
            Vector3? validPos = GetValidGroundPos(playerScript.transform.position);
            if (validPos.HasValue)
            {
                if (validPositionsHistory.Count == 0 || Vector3.Distance(validPos.Value, validPositionsHistory[validPositionsHistory.Count - 1]) > minRecordDistance)
                    validPositionsHistory.Add(validPos.Value);
            }
            nextSampleTime = Time.time + historySampleRate;
        }
    }

    // ... (Helper functions: ResumeGame, PauseGame, LoadMainMenu, stb. változatlanok) ...
    public void ResumeGame() { if (pauseMenuUI != null) pauseMenuUI.SetActive(false); if (gameUI != null) gameUI.SetActive(true); Time.timeScale = 1f; isPaused = false; }
    void PauseGame() { if (pauseMenuUI != null) pauseMenuUI.SetActive(true); if (gameUI != null) gameUI.SetActive(false); Time.timeScale = 0f; isPaused = true; }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        ResetStaticVariables();
        MovingPlatform.ResetAllPlatforms();
        SceneManager.LoadScene(0);
    }
    public void CollectKey() { hasKey = true; }
    public bool IsKeyCollected() { return !levelRequiresKey || hasKey; }

    void SpawnKey()
    {
        if (keyPrefab == null) return;
        if (manualKeyLocation != null) { Instantiate(keyPrefab, manualKeyLocation.position, Quaternion.identity); return; }
        for (int i = 0; i < 30; i++)
        {
            float randomX = Random.Range(levelMinX, levelMaxX);
            Vector3? validPos = GetValidGroundPos(new Vector3(randomX, keySpawnHeight, 0));
            if (validPos.HasValue) { Instantiate(keyPrefab, validPos.Value + Vector3.up * 0.5f, Quaternion.identity); return; }
        }
    }
    Vector3? GetValidGroundPos(Vector3 searchPos)
    {
        Vector2 origin = new Vector2(searchPos.x, searchPos.y + 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 10f, playerScript.groundLayer);

        if (hit.collider == null) return null;

        // --- TILTOTT OBJEKTUMOK ---
        if (hit.collider.GetComponent<MovingPlatform>() != null) return null;
        if (hit.collider.GetComponent<FallingPlatform>() != null) return null;
        if (hit.collider.GetComponent<JumpPad>() != null) return null;

        // ÚJ: Ha futószalag van ott, ne tegyél rá csapdát!
        if (hit.collider.GetComponent<ConveyorBelt>() != null) return null;
        // --------------------------

        if (IsPositionSafe(hit.point)) return hit.point;
        return null;
    }
    bool IsPositionSafe(Vector3 pos)
    {
        if (Vector3.Distance(pos, startPosition) < safeZoneRadius) return false;
        if (goalPosition != Vector3.zero && Vector3.Distance(pos, goalPosition) < safeGoalRadius) return false;
        foreach (Vector3 existingTrap in trapPositions) { if (Vector3.Distance(pos, existingTrap) < minTrapDistance) return false; }
        if (Physics2D.Raycast(pos, Vector2.up, minCeilingHeight, playerScript.groundLayer).collider != null) return false;
        Vector2 checkLeft = new Vector2(pos.x - edgeCheckDistance, pos.y + 0.5f);
        Vector2 checkRight = new Vector2(pos.x + edgeCheckDistance, pos.y + 0.5f);
        if (!Physics2D.Raycast(checkLeft, Vector2.down, 1.5f, playerScript.groundLayer) || !Physics2D.Raycast(checkRight, Vector2.down, 1.5f, playerScript.groundLayer)) return false;
        return true;
    }

    public void GameOver() { if (isLevelFinished) return; StartCoroutine(AutoRestartSequence()); }
    IEnumerator AutoRestartSequence() { yield return new WaitForSeconds(2f); RestartGame(); }

    // --- ITT TÖRTÉNT A MÓDOSÍTÁS ---
    public void RestartGame()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        if (difficulty == 1)
        {
            // HARD MODE:
            // 1. NEM töröljük a mentést (SaveSystem.DeleteSave kivéve!)

            // 2. Teljesen reseteljük a pályát (mintha most léptünk volna be elõször)
            // Ez törli a csapdákat (trapPositions) és a körszámlálót (currentRound = 1)
            // És a liftek memóriáját is törli.
            ResetStaticVariablesForNewGame();

            // 3. Az AKTUÁLIS pályát töltjük újra (nem a Level 1-et)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // NORMAL MODE: 
            // Csak a kör indul újra, a csapdák és a körszám megmaradnak.
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    // --------------------------------

    public void LevelComplete()
    {
        if (isLevelFinished) return;
        isLevelFinished = true;

        if (validPositionsHistory.Count > 0)
        {
            // --- BOMBABIZTOS HELYKERESÉS ---
            Vector3 chosenPosition = Vector3.zero;
            bool foundValidSpot = false;
            int attempts = 0;
            int maxAttempts = 20; // Kicsit növeltem a próbálkozást

            while (!foundValidSpot && attempts < maxAttempts)
            {
                // 1. Véletlen pont kiválasztása
                int randomIndex = Random.Range(0, validPositionsHistory.Count);
                Vector3 candidatePos = validPositionsHistory[randomIndex];

                // 2. ELLENÕRZÉS: Van-e bármi a közelben?
                // Megnöveltem a sugarat 0.8f-re, hogy biztosan tartson távolságot
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(candidatePos, 0.8f);

                bool occupied = false;
                foreach (var col in hitColliders)
                {
                    // Itt a lényeg: Ha "Trap", "Goal" VAGY "Obstacle" van ott -> FOGLALT!
                    if (col.CompareTag("Trap") || col.CompareTag("Goal") || col.CompareTag("Obstacle"))
                    {
                        occupied = true;
                        break;
                    }

                    // Extra védelem: Ha mozgó dolgok scriptje van rajta
                    if (col.GetComponent<MovingPlatform>() != null ||
                        col.GetComponent<FallingPlatform>() != null ||
                        col.GetComponent<ConveyorBelt>() != null ||
                        col.GetComponent<JumpPad>() != null)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    chosenPosition = candidatePos;
                    foundValidSpot = true;
                }

                attempts++;
            }

            // Ha találtunk jó helyet, elmentjük
            if (foundValidSpot)
            {
                trapPositions.Add(chosenPosition);
            }
            else
            {
                // Ha 20-szorra sem találtunk (nagyon ritka), akkor inkább nem rakunk le semmit ebben a körben,
                // hogy ne rontsuk el a pályát. (Vagy rakhatod a validPositionsHistory[0]-ra kockázatként).
                Debug.LogWarning("Nem találtam üres helyet a csapdának, ebben a körben nem született új csapda.");
            }
            // ----------------------------------------
        }

        currentRound++;

        if (currentRound > maxRounds) StartCoroutine(WinSequence());
        else { Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    }

    IEnumerator WinSequence()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (playerScript != null) playerScript.gameObject.SetActive(false);

        // --- MENTÉS ---
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasNextLevel = nextSceneIndex < SceneManager.sceneCountInBuildSettings;
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        if (hasNextLevel)
        {
            SaveSystem.SaveGame(currentSlot, nextSceneIndex, difficulty, 1);
        }

        if (winText != null) winText.text = hasNextLevel ? "LEVEL COMPLETE!" : "YOU WON THE GAME!";

        yield return new WaitForSeconds(4);

        ResetStaticVariables();
        MovingPlatform.ResetAllPlatforms();

        if (hasNextLevel) SceneManager.LoadScene(nextSceneIndex);
        else SceneManager.LoadScene(0);
    }

    private void ResetStaticVariables()
    {
        trapPositions.Clear();
        MovingObject.ResetAllSaws();
        SecretArea.ResetSecrets();
        currentRound = 1;
        Time.timeScale = 1;
    }

    public static void ResetStaticVariablesForNewGame()
    {
        trapPositions.Clear();
        MovingObject.ResetAllSaws();
        SecretArea.ResetSecrets();
        MovingPlatform.ResetAllPlatforms();
        currentRound = 1;
        Time.timeScale = 1;
    }
}