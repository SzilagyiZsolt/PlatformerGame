using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCarousel : MonoBehaviour
{
    [Header("UI Referenciák")]
    public Image levelImageDisplay;
    public TextMeshProUGUI levelNameText;
    public Button leftArrow;
    public Button rightArrow;
    public Button playButton;
    public GameObject lockIcon;

    [Header("Animáció")]
    public float transitionSpeed = 0.2f;

    [Header("Pálya Adatok")]
    public LevelData[] levels;

    private int currentIndex = 0;
    private int unlockedLevelIndex;

    // --- JAVÍTÁS ITT: Tároljuk a fix méretet és az állapotot ---
    private Vector3 defaultScale;
    private bool isAnimating = false; // Spam védelem

    void Start()
    {
        // Elmentjük a kezdeti méretet
        if (levelImageDisplay != null)
        {
            defaultScale = levelImageDisplay.transform.localScale;
        }

        // 1. Betöltjük a mentést
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        unlockedLevelIndex = SaveSystem.GetSavedLevel(currentSlot);

        // --- ITT A VÁLTOZÁS ---
        // Ahelyett, hogy 0-ról indulnánk, beugrunk a legutolsó elért szintre.
        // Kivonunk 1-et, mert a mentés 1-esrõl indul, a tömb pedig 0-ról.
        // A Mathf.Clamp biztosítja, hogy ne próbáljunk olyan pályára ugrani, ami nincs a listában (pl. ha a játék végére értél).
        currentIndex = Mathf.Clamp(unlockedLevelIndex - 1, 0, levels.Length - 1);
        // ----------------------

        UpdateUI(false); // Frissítjük a képet (animáció nélkül)
    }

    public void NextLevel()
    {
        // Ha épp animálunk, vagy nincs több pálya, nem csinálunk semmit
        if (isAnimating || currentIndex >= levels.Length - 1) return;

        currentIndex++;
        StartCoroutine(AnimateTransition());
    }

    public void PreviousLevel()
    {
        // Ha épp animálunk, vagy az elején vagyunk, nem csinálunk semmit
        if (isAnimating || currentIndex <= 0) return;

        currentIndex--;
        StartCoroutine(AnimateTransition());
    }

    public void LoadCurrentLevel()
    {
        if (unlockedLevelIndex >= currentIndex + 1)
        {
            GameManager.ResetStaticVariablesForNewGame();
            SceneManager.LoadScene(levels[currentIndex].sceneIndex);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateUI(bool animate)
    {
        LevelData data = levels[currentIndex];

        levelNameText.text = data.levelName;
        levelImageDisplay.sprite = data.levelImage;

        // Gombok állapotának frissítése (de az animáció alatt úgyis le vannak tiltva a logikában)
        leftArrow.interactable = (currentIndex > 0);
        rightArrow.interactable = (currentIndex < levels.Length - 1);

        bool isUnlocked = (unlockedLevelIndex >= currentIndex + 1);

        if (isUnlocked)
        {
            levelImageDisplay.color = Color.white;
            if (lockIcon != null) lockIcon.SetActive(false);
            playButton.interactable = true;
        }
        else
        {
            levelImageDisplay.color = Color.gray;
            if (lockIcon != null) lockIcon.SetActive(true);
            playButton.interactable = false;
        }
    }

    // --- JAVÍTOTT ANIMÁCIÓ ---
    IEnumerator AnimateTransition()
    {
        isAnimating = true; // Zárjuk a bemenetet

        float timer = 0;

        // 1. Kiemelkedés (Kicsinyítés)
        // Most már mindig a defaultScale-hez viszonyítunk!
        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionSpeed;
            // A "defaultScale"-bõl indulunk, nem a "transform.localScale"-ból
            levelImageDisplay.transform.localScale = Vector3.Lerp(defaultScale, defaultScale * 0.8f, progress);
            yield return null;
        }

        // 2. Adatcsere (amikor a legkisebb)
        UpdateUI(true);

        // 3. Visszatérés (Nagyítás)
        timer = 0;
        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionSpeed;
            // Visszatérünk a fix defaultScale-re
            levelImageDisplay.transform.localScale = Vector3.Lerp(defaultScale * 0.8f, defaultScale, progress);
            yield return null;
        }

        // Biztos ami biztos: beállítjuk pontosra a végén
        levelImageDisplay.transform.localScale = defaultScale;

        isAnimating = false; // Újra engedjük a gombokat
    }
}