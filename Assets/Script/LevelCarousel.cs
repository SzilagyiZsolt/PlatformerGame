using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems; // <-- EZ KELL A KONTROLLERHEZ!

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
    public LevelData[] levels; // FONTOS: Itt majd legyen 10 elem a Unity-ben!

    [Header("Kontroller Navigáció")]
    // Ide húzd be a PLAY gombot az Inspectorban!
    public GameObject firstSelectedObject;

    private int currentIndex = 0;
    private int unlockedLevelIndex;

    private Vector3 defaultScale;
    private bool isAnimating = false;

    // Változó a "spam" elkerülésére (hogy ne pörgessen túl gyorsan)
    private float inputCooldown = 0f;

    void Start()
    {
        if (levelImageDisplay != null)
        {
            defaultScale = levelImageDisplay.transform.localScale;
        }

        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 1);
        unlockedLevelIndex = SaveSystem.GetSavedLevel(currentSlot);

        // --- MÓDOSÍTÁS: Ha végigvitte a játékot (unlockedLevelIndex > levels.Length) ---
        // Akkor is csak a lista végére ugorjon, ne crasheljen
        currentIndex = Mathf.Clamp(unlockedLevelIndex - 1, 0, levels.Length - 1);

        UpdateUI(false);

        // Amikor a Level Selector elindul, rátesszük a jelölést a Play gombra
        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
    }

    void Update()
    {
        if (inputCooldown > 0) inputCooldown -= Time.deltaTime;

        // Joystick Input (Balra/Jobbra)
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0.5f && inputCooldown <= 0)
        {
            NextLevel();
            inputCooldown = 0.4f; // Fél másodpercet várni kell a kövi lapozásig
        }
        else if (horizontalInput < -0.5f && inputCooldown <= 0)
        {
            PreviousLevel();
            inputCooldown = 0.4f;
        }

        // A Kontroller 'A' vagy 'X' gombja elindítja a pályát (ha a Play gomb aktív)
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Submit"))
        {
            // Opcionális: Ha nem Button-ként használod a Play-t, itt meghívhatod direktben:
            // LoadCurrentLevel(); 
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

    // --- ITT A LÉNYEGES VÁLTOZÁS ---
    public void LoadCurrentLevel()
    {
        // Csak akkor engedjük, ha fel van oldva
        if (unlockedLevelIndex >= currentIndex + 1)
        {
            // Ellenõrizzük, hogy a 10. pálya van-e kiválasztva
            // (Mivel a lista 0-tól indul, a 9-es index a 10. pálya)
            if (currentIndex == 9)
            {
                // Ez a 10. Jubileumi pálya -> Speciális indítás!
                Debug.Log("Jubilee Mode Selected via Carousel");
                GameManager.StartJubileeMode();
            }
            else
            {
                // Ez egy sima pálya (1-9) -> Normál indítás
                GameManager.ResetStaticVariablesForNewGame();
                SceneManager.LoadScene(levels[currentIndex].sceneIndex);
            }
        }
    }
    // -------------------------------

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateUI(bool animate)
    {
        // Biztonsági ellenõrzés, ha véletlenül nincs beállítva elég LevelData
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