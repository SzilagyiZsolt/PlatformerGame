using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // <--- FONTOS: Ez kell a Image kezeléséhez!
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Panelek")]
    public GameObject saveSlotsPanel;
    public GameObject difficultyPanel;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Téli Téma Beállítások (ÚJ)")]
    public Sprite snowyBackground; // Húzd be ide a havas képet!

    // Húzd be ide azokat az Image komponenseket, amiknek a hátterét cserélni akarod!
    // (Pl. a MainMenuPanel-en lévõ Image, a SaveSlotsPanel-en lévõ Image, stb.)
    public Image menuBackgroundImage;
    public Image saveSlotsBackgroundImage;
    public Image difficultyBackgroundImage;

    [Header("Kontroller Navigáció")]
    public GameObject playButtonObj;
    public GameObject slot1ButtonObj;
    public GameObject normalDiffButtonObj;

    [Header("Slot Gombok Szövegei")]
    public TextMeshProUGUI[] slotTexts;

    private int selectedSlot = 1;

    void Start()
    {
        // --- ÚJ RÉSZ: Téli háttér ellenõrzése ---
        CheckForSnowTheme();
        // ----------------------------------------

        ShowMainMenu();
    }

    // Leellenõrizzük, hogy bármelyik slotban elértük-e a 11. szintet
    void CheckForSnowTheme()
    {
        bool isSnowUnlocked = false;

        // Megnézzük az 1-es, 2-es, 3-as slotot
        for (int i = 1; i <= 3; i++)
        {
            if (SaveSystem.HasSave(i))
            {
                // Ha a mentett szint 11 vagy nagyobb (vagyis a 10. pályát már teljesítette)
                if (SaveSystem.GetSavedLevel(i) >= 11)
                {
                    isSnowUnlocked = true;
                    break;
                }
            }
        }

        // Ha fel van oldva és be van állítva a havas kép, kicseréljük
        if (isSnowUnlocked && snowyBackground != null)
        {
            if (menuBackgroundImage != null) menuBackgroundImage.sprite = snowyBackground;
            if (saveSlotsBackgroundImage != null) saveSlotsBackgroundImage.sprite = snowyBackground;
            if (difficultyBackgroundImage != null) difficultyBackgroundImage.sprite = snowyBackground;
        }
    }

    public void ShowMainMenu()
    {
        saveSlotsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        SetFirstSelected(playButtonObj);
    }

    public void OnPlayButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);

        UpdateSlotUI();

        SetFirstSelected(slot1ButtonObj);
    }

    public void OnSlotClicked(int slotIndex)
    {
        selectedSlot = slotIndex;

        if (SaveSystem.HasSave(slotIndex))
        {
            LoadGameFromSlot(slotIndex);
        }
        else
        {
            saveSlotsPanel.SetActive(false);
            difficultyPanel.SetActive(true);
            SetFirstSelected(normalDiffButtonObj);
        }
    }

    void SetFirstSelected(GameObject buttonToSelect)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

    public void OnDifficultySelected(int difficulty)
    {
        SaveSystem.SaveGame(selectedSlot, 1, difficulty, 1);

        PlayerPrefs.SetInt("CurrentSlot", selectedSlot);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        GameManager.ResetStaticVariablesForNewGame();
        SceneManager.LoadScene(1);
    }

    private void LoadGameFromSlot(int slotIndex)
    {
        int savedLevel = SaveSystem.GetSavedLevel(slotIndex);
        int difficulty = SaveSystem.GetSavedDifficulty(slotIndex);

        PlayerPrefs.SetInt("CurrentSlot", slotIndex);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        GameManager.ResetStaticVariablesForNewGame();

        // Fontos: Mivel van Level Selectorod, oda visszük a játékost, nem direkt a pályára!
        SceneManager.LoadScene("LevelSelect");
    }

    public void DeleteSlot(int slotIndex)
    {
        SaveSystem.DeleteSave(slotIndex);
        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            int slotIndex = i + 1;
            if (SaveSystem.HasSave(slotIndex))
            {
                int lvl = SaveSystem.GetSavedLevel(slotIndex);
                int diff = SaveSystem.GetSavedDifficulty(slotIndex);
                string diffText = (diff == 0) ? "Normal" : "Hard";
                slotTexts[i].text = $"Level {lvl} - ({diffText})";
            }
            else
            {
                slotTexts[i].text = $"Empty";
            }
        }
    }

    public void BackToMainMenu()
    {
        saveSlotsPanel.SetActive(false);
        ShowMainMenu();
    }

    public void BackToSlots()
    {
        difficultyPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);
        SetFirstSelected(slot1ButtonObj);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        ShowMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}