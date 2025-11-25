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

    [Header("Kulcs Rendszer")]
    public bool levelRequiresKey = true;
    public GameObject keyPrefab;

    // --- ÚJ VÁLTOZÓ: A te általad lerakott pont ---
    public Transform manualKeyLocation;

    // A régi random beállítások (maradhatnak, de nem használjuk õket, ha van fix pont)
    [Header("Random beállítások (Csak ha nincs fix pont)")]
    public float levelMinX = -10f;
    public float levelMaxX = 20f;
    public float keySpawnHeight = 10f;

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

    private static List<Vector3> trapPositions = new List<Vector3>();
    private static int currentRound = 1;
    private Vector3 tempTrapPosition;
    private bool tempPosRecorded = false;
    private bool isLevelFinished = false;
    private Vector3 startPosition;
    private float currentTrapTime;
    private bool hasKey = false;

    private List<Vector3> validPositionsHistory = new List<Vector3>();
    private float historySampleRate = 0.2f;
    private float nextSampleTime = 0f;

    void Start()
    {
        isLevelFinished = false;
        tempPosRecorded = false;
        hasKey = false;
        validPositionsHistory.Clear();

        currentTrapTime = Random.Range(minTrapTime, maxTrapTime);

        if (playerScript != null) startPosition = playerScript.transform.position;
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

        if (levelRequiresKey)
        {
            SpawnKey();
        }
    }

    void Update()
    {
        if (isLevelFinished || playerScript == null) return;

        if (!tempPosRecorded && Time.timeSinceLevelLoad >= currentTrapTime)
        {
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);
            if (groundPos.HasValue)
            {
                tempTrapPosition = groundPos.Value;
                tempPosRecorded = true;
            }
        }

        if (Time.time >= nextSampleTime)
        {
            Vector3? groundPos = GetValidGroundPos(playerScript.transform.position);
            if (groundPos.HasValue) validPositionsHistory.Add(groundPos.Value);
            nextSampleTime = Time.time + historySampleRate;
        }
    }

    public void CollectKey()
    {
        hasKey = true;
    }

    public bool IsKeyCollected()
    {
        if (!levelRequiresKey) return true;
        return hasKey;
    }

    // --- EZT MÓDOSÍTOTTUK ---
    void SpawnKey()
    {
        if (keyPrefab == null) return;

        // 1. ESET: HA VAN KÉZZEL BEÁLLÍTOTT HELY
        if (manualKeyLocation != null)
        {
            // Egyszerûen lerakjuk oda a kulcsot
            Instantiate(keyPrefab, manualKeyLocation.position, Quaternion.identity);
            Debug.Log("Kulcs lerakva a fix pontra.");
            return; // Kilépünk, nem kell randomolás
        }

        // 2. ESET: HA NINCS FIX HELY -> MARAD A RANDOM (Régi logika)
        for (int i = 0; i < 30; i++)
        {
            float randomX = Random.Range(levelMinX, levelMaxX);
            Vector3 searchPos = new Vector3(randomX, keySpawnHeight, 0);
            Vector3? validPos = GetValidGroundPos(searchPos);
            if (validPos.HasValue)
            {
                Instantiate(keyPrefab, validPos.Value + Vector3.up * 0.5f, Quaternion.identity);
                return;
            }
        }
    }

    // ... (A többi függvény - GetValidGroundPos, IsPositionSafe, stb. - VÁLTOZATLAN) ...

    Vector3? GetValidGroundPos(Vector3 startSearchPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(startSearchPos, Vector2.down, 20f, playerScript.groundLayer);
        if (hit.collider == null) return null;
        Vector3 candidatePos = hit.point;
        if (IsPositionSafe(candidatePos)) return candidatePos;
        return null;
    }

    bool IsPositionSafe(Vector3 pos)
    {
        if (Vector3.Distance(pos, startPosition) < safeZoneRadius) return false;
        foreach (Vector3 existingTrap in trapPositions)
        {
            if (Vector3.Distance(pos, existingTrap) < minTrapDistance) return false;
        }
        RaycastHit2D hitCeiling = Physics2D.Raycast(pos, Vector2.up, minCeilingHeight, playerScript.groundLayer);
        if (hitCeiling.collider != null) return false;
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

        if (tempPosRecorded) trapPositions.Add(tempTrapPosition);
        else if (validPositionsHistory.Count > 0)
        {
            int randomIndex = Random.Range(0, validPositionsHistory.Count);
            trapPositions.Add(validPositionsHistory[randomIndex]);
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
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
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