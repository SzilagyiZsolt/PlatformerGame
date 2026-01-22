using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class LevelCarousel : MonoBehaviour
{
    [Header("UI Referenciák")]
    public Image levelImageDisplay;
    public TextMeshProUGUI levelNameText;
    public Button leftArrow;
    public Button rightArrow;
    public Button playButton;
    public GameObject lockIcon;

    // --- ÚJ RÉSZ: HÁTTÉR ---
    [Header("Téli Téma")]
    public Image backgroundPanelImage; // Húzd be ide a Canvas háttér paneljének Image komponensét!
    public Sprite snowyBackground;     // Húzd be ide a havas képet!
    // -----------------------

    [Header("Animáció")]
    public float transitionSpeed = 0.2f;

    [Header("Pálya Adatok")]
    public LevelData[] levels;

    [Header("Kontroller Navigáció")]
    public GameObject firstSelectedObject;

    private int currentIndex = 0;
    private int unlockedLevelIndex;

    private Vector3 defaultScale;
    private bool isAnimating = false;
    private float inputCooldown = 0f;

    void Start()
    {
        if (levelImageDisplay != null)
        {
            defaultScale = levelImageDisplay.transform.localScale;
        }

        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        unlockedLevelIndex = SaveSystem.GetSavedLevel(currentSlot);

        // --- ÚJ RÉSZ: Téli háttér beállítása ---
        // Ha az aktuális mentésben elértük a 11-et (vagyis a 10. pálya kész)
        if (unlockedLevelIndex >= 11 && backgroundPanelImage != null && snowyBackground != null)
        {
            backgroundPanelImage.sprite = snowyBackground;
        }
        // ----------------------------------------

        currentIndex = Mathf.Clamp(unlockedLevelIndex - 1, 0, levels.Length - 1);

        UpdateUI(false);

        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
    }

    void Update()
    {
        if (inputCooldown > 0) inputCooldown -= Time.deltaTime;

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0.5f && inputCooldown <= 0)
        {
            NextLevel();
            inputCooldown = 0.4f;
        }
        else if (horizontalInput < -0.5f && inputCooldown <= 0)
        {
            PreviousLevel();
            inputCooldown = 0.4f;
        }

        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Submit"))
        {
            // Opcionális: LoadCurrentLevel(); 
        }
    }

    public void NextLevel()
    {
        if (isAnimating || currentIndex >= levels.Length - 1) return;
        currentIndex++;
        StartCoroutine(AnimateTransition());
    }

    public void PreviousLevel()
    {
        if (isAnimating || currentIndex <= 0) return;
        currentIndex--;
        StartCoroutine(AnimateTransition());
    }

    public void LoadCurrentLevel()
    {
        if (unlockedLevelIndex >= currentIndex + 1)
        {
            if (currentIndex == 9)
            {
                Debug.Log("Jubilee Mode Selected via Carousel");
                GameManager.StartJubileeMode();
            }
            else
            {
                GameManager.ResetStaticVariablesForNewGame();
                SceneManager.LoadScene(levels[currentIndex].sceneIndex);
            }
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateUI(bool animate)
    {
        if (currentIndex >= levels.Length) return;

        LevelData data = levels[currentIndex];

        levelNameText.text = data.levelName;
        levelImageDisplay.sprite = data.levelImage;

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

    IEnumerator AnimateTransition()
    {
        isAnimating = true;
        float timer = 0;

        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionSpeed;
            levelImageDisplay.transform.localScale = Vector3.Lerp(defaultScale, defaultScale * 0.8f, progress);
            yield return null;
        }

        UpdateUI(true);

        timer = 0;
        while (timer < transitionSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionSpeed;
            levelImageDisplay.transform.localScale = Vector3.Lerp(defaultScale * 0.8f, defaultScale, progress);
            yield return null;
        }

        levelImageDisplay.transform.localScale = defaultScale;
        isAnimating = false;
    }
}