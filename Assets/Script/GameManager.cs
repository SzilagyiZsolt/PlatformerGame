using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems; // <--- FONTOS!

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Jubileumi (10-es) Pálya Beállítások")]
    public static bool isJubileeMode = false;   // Ez jelzi, hogy a 10. pályán vagyunk-e
    public static int jubileeCurrentLevel = 1;  // Melyik pályánál tartunk épp a maratonban (1-9)

    [Header("Kontroller Navigáció")]
    public GameObject pauseFirstButton; // Húzd be ide a "Resume" gombot!
    public GameObject winFirstButton;   // Húzd be ide a "Next Level" vagy "Menu" gombot!

    [Header("Menü Panelek")]
    public GameObject settingsPanel; // Húzd be a Beállítások ablakot

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

    void Awake()
    {
        instance = this;
    }

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

        // --- JUBILEUMI MÓD KEZELÉSE ---
        if (isJubileeMode)
        {
            HandleJubileeStart();
        }
        // ------------------------------

        if (roundText != null)
        {
            if (isJubileeMode)
            {
                // Maraton módban nem a köröket írjuk ki, hanem hogy melyik pályán vagy
                roundText.text = $"GAUNTLET: Level {jubileeCurrentLevel}/9";
            }
            else
            {
                int diff = PlayerPrefs.GetInt("Difficulty", 1);
                string diffText = diff == 0 ? "(Normal)" : "(Hard)";
                roundText.text = $"Round:{currentRound}/{maxRounds} {diffText}";
            }
        }

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

    // Külön függvény a Jubileumi start logikának a tisztaság kedvéért
    void HandleJubileeStart()
    {
        // 1. Csak 1 kört kell menni
        maxRounds = 1;
        currentRound = 1;

        // 2. Betöltjük a mentett tüskéket az adott pályához (pl. Level 1-hez)
        // Megjegyzés: A scene nevének végén lévõ számot feltételezzük, vagy a jubileeCurrentLevel változót használjuk
        trapPositions = SaveSystem.LoadTrapsForLevel(jubileeCurrentLevel);

        Debug.Log($"Jubilee Mode: Loaded {trapPositions.Count} traps for Level {jubileeCurrentLevel}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Submit"))
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

    public void ResumeGame() { if (pauseMenuUI != null) pauseMenuUI.SetActive(false); if (gameUI != null) gameUI.SetActive(true); Time.timeScale = 1f; isPaused = false; }
    void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        // --- KONTROLLER FÓKUSZ BEÁLLÍTÁSA ---
        // Töröljük az elõzõt
        EventSystem.current.SetSelectedGameObject(null);
        // Beállítjuk az újat (pl. Resume gomb)
        if (pauseFirstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        ResetStaticVariables();
        isJubileeMode = false; // Kilépéskor reseteljük a módot
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

        if (hit.collider.GetComponent<MovingPlatform>() != null) return null;
        if (hit.collider.GetComponent<FallingPlatform>() != null) return null;
        if (hit.collider.GetComponent<JumpPad>() != null) return null;
        if (hit.collider.GetComponent<ConveyorBelt>() != null) return null;

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

    public void RestartGame()
    {
        if (isJubileeMode)
        {
            // --- JUBILEUMI HALÁL ---
            // Ha meghalsz a maratonban, visszadob a LEGELEJÉRE (Level 1)
            Debug.Log("Jubilee Mode Failed! Restarting form Level 1.");
            jubileeCurrentLevel = 1;
            ResetStaticVariablesForNewGame();

            // Betöltjük az 1. pályát (amelynek a build indexe feltételezhetõen 1, vagy a neve "Level1")
            SceneManager.LoadScene("Level1");
        }
        else
        {
            // --- NORMÁL MÓD HALÁL ---
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
            if (difficulty == 1) // Hard
            {
                ResetStaticVariablesForNewGame();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else // Normal
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    public void LevelComplete()
    {
        if (isLevelFinished) return;
        isLevelFinished = true;

        if (isJubileeMode)
        {
            // --- JUBILEUMI PÁLYA TELJESÍTVE ---
            // Nem rakunk le új csapdát, hanem lépünk a következõ pályára
            if (jubileeCurrentLevel < 9) // Ha még nem a 9. volt az utolsó
            {
                StartCoroutine(JubileeNextLevelSequence());
            }
            else
            {
                // Ha megcsináltuk a 9.-et is a maratonban -> GYÕZELEM!
                StartCoroutine(WinSequence());
            }
        }
        else
        {
            // --- NORMÁL MÓD ---
            // Tüske lerakása logika...
            if (validPositionsHistory.Count > 0)
            {
                Vector3 chosenPosition = Vector3.zero;
                bool foundValidSpot = false;
                int attempts = 0;
                int maxAttempts = 20;

                while (!foundValidSpot && attempts < maxAttempts)
                {
                    int randomIndex = Random.Range(0, validPositionsHistory.Count);
                    Vector3 candidatePos = validPositionsHistory[randomIndex];
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(candidatePos, 0.8f);

                    bool occupied = false;
                    foreach (var col in hitColliders)
                    {
                        if (col.CompareTag("Trap") || col.CompareTag("Goal") || col.CompareTag("Obstacle")) { occupied = true; break; }
                        if (col.GetComponent<MovingPlatform>() != null || col.GetComponent<FallingPlatform>() != null || col.GetComponent<ConveyorBelt>() != null || col.GetComponent<JumpPad>() != null) { occupied = true; break; }
                    }

                    if (!occupied) { chosenPosition = candidatePos; foundValidSpot = true; }
                    attempts++;
                }

                if (foundValidSpot) trapPositions.Add(chosenPosition);
            }

            currentRound++;
            if (currentRound > maxRounds) StartCoroutine(WinSequence());
            else { Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        }
    }

    IEnumerator JubileeNextLevelSequence()
    {
        if (winText != null)
        {
            winPanel.SetActive(true);
            winText.text = "NEXT LEVEL!";
        }
        if (playerScript != null) playerScript.gameObject.SetActive(false);

        yield return new WaitForSeconds(2); // Rövid szünet

        jubileeCurrentLevel++; // Lépünk a kövi pályára

        // Resetelünk, de a jubilee változókat NEM!
        ResetStaticVariables();
        MovingPlatform.ResetAllPlatforms();

        // Betöltjük a következõ pályát (pl. "Level2")
        SceneManager.LoadScene("Level" + jubileeCurrentLevel);
    }

    IEnumerator WinSequence()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (playerScript != null) playerScript.gameObject.SetActive(false);

        // --- CSAPDÁK MENTÉSE (Marad a régi) ---
        if (!isJubileeMode)
        {
            int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            SaveSystem.SaveTrapsForLevel(currentLevelIndex, trapPositions);
        }

        // --- PROGRESSZIÓ MENTÉSE (ITT A JAVÍTÁS!) ---
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasNextLevel = nextSceneIndex < SceneManager.sceneCountInBuildSettings;
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        if (hasNextLevel && !isJubileeMode)
        {
            // 1. Lekérjük, hol tartottunk eddig (pl. Level 5)
            int currentlySavedLevel = SaveSystem.GetSavedLevel(currentSlot);

            // 2. Azt mentjük el, amelyik a NAGYOBB. 
            // Ha Level 1-et csináltad meg (next=2), de már 5-nél tartasz: Max(2, 5) = 5. (Marad az 5)
            // Ha Level 5-öt csináltad meg (next=6), és 5-nél tartasz: Max(6, 5) = 6. (Fejlõdsz)
            int levelToSave = Mathf.Max(nextSceneIndex, currentlySavedLevel);

            SaveSystem.SaveGame(currentSlot, levelToSave, difficulty, 1);
        }
        // ---------------------------------------------

        if (isJubileeMode)
        {
            if (winText != null) winText.text = "10TH ANNIVERSARY COMPLETE!";
        }
        else
        {
            if (winText != null) winText.text = hasNextLevel ? "LEVEL COMPLETE!" : "YOU WON THE GAME!";
        }

        yield return new WaitForSeconds(4);

        ResetStaticVariables();
        isJubileeMode = false;
        MovingPlatform.ResetAllPlatforms();

        if (hasNextLevel && !isJubileeMode) SceneManager.LoadScene(nextSceneIndex);
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

    // Ezt hívd meg a 10. pálya gombjával (pl. LevelSelect menübõl)
    public static void StartJubileeMode()
    {
        isJubileeMode = true;
        jubileeCurrentLevel = 1;
        ResetStaticVariablesForNewGame();
        SceneManager.LoadScene("Level1");
    }

    // Ezt hívd meg a Pause menü "Settings" gombjával
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);

        // Fókusz a Settings elsõ elemére (a SettingsMenu.cs OnEnable intézi, de biztosra megyünk)
        // (Nem kötelezõ ide, ha a SettingsMenu.cs-ben megvan az OnEnable)
    }

    // --- EZ A HIÁNYZÓ LÁNCSZEM ---
    // Ezt húzd rá a Beállítások panel "Vissza" (Back) gombjára a Pause menüben!
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);

        // VISSZAADJUK A FÓKUSZT A RESUME GOMBRA
        EventSystem.current.SetSelectedGameObject(null);
        if (pauseFirstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        }
    }
}