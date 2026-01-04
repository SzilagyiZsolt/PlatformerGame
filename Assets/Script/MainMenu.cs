using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Szükség lesz rá a gombok feliratához
using UnityEngine.EventSystems; // --- EZT ADD HOZZÁ FENTRE! ---

public class MainMenu : MonoBehaviour
{
    [Header("Panelek")]
    public GameObject saveSlotsPanel;
    public GameObject difficultyPanel;
    public GameObject mainMenuPanel; // Ha van külön panel a fõmenü gomboknak
    public GameObject settingsPanel; // Húzd be a Beállítások ablakot

    [Header("Kontroller Navigáció")]
    // Ide húzd be azokat a gombokat, amiknek ELSÕKÉNT kell aktívnak lenniük
    public GameObject playButtonObj;      // A Fõmenü "Play" gombja
    public GameObject slot1ButtonObj;     // A Slot választó "Slot 1" gombja
    public GameObject normalDiffButtonObj;// A Nehézség "Normal" gombja

    [Header("Slot Gombok Szövegei")]
    public TextMeshProUGUI[] slotTexts; // Húzd be ide a 3 gomb szövegét!

    

    private int selectedSlot = 1; // Melyik slotot nyomtuk meg épp

    void Start()
    {
        ShowMainMenu();
    }

    // --- NAVIGÁCIÓ ---
    public void ShowMainMenu()
    {
        saveSlotsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        // KONTROLLER: Beállítjuk az elsõ gombot aktívnak
        SetFirstSelected(playButtonObj);
    }

    public void OnPlayButtonClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);

        UpdateSlotUI();

        // KONTROLLER: Amikor átlépünk a Slot választóba, jelöljük ki az 1-est
        SetFirstSelected(slot1ButtonObj);
    }

    // Ezt hívják a Slot gombok (1, 2, 3)
    public void OnSlotClicked(int slotIndex)
    {
        selectedSlot = slotIndex;

        // Ha van mentés, betöltjük és indítjuk
        if (SaveSystem.HasSave(slotIndex))
        {
            LoadGameFromSlot(slotIndex);
        }
        else
        {
            // Ha üres, jöhet a nehézség választás
            saveSlotsPanel.SetActive(false);
            difficultyPanel.SetActive(true);

            // KONTROLLER: Amikor átlépünk a nehézségre, jelöljük ki a Normalt
            SetFirstSelected(normalDiffButtonObj);
        }
    }

    // --- SEGÉDFÜGGVÉNY A KIVÁLASZTÁSHOZ ---
    void SetFirstSelected(GameObject buttonToSelect)
    {
        // Töröljük az elõzõ kiválasztást
        EventSystem.current.SetSelectedGameObject(null);
        // Beállítjuk az újat
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

    // Ezt is módosítani kell: Új játéknál is a választóra vagy az 1. pályára vigyen?
    // Általában új játéknál logikus az 1. pályára dobni egybõl.
    // De ha azt akarod, hogy õk is lássák a választót (ahol csak az 1-es aktív), akkor írd át ezt is.
    public void OnDifficultySelected(int difficulty)
    {
        SaveSystem.SaveGame(selectedSlot, 1, difficulty, 1);

        PlayerPrefs.SetInt("CurrentSlot", selectedSlot);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        GameManager.ResetStaticVariablesForNewGame();

        // DÖNTÉS:
        // A) Kezdje el azonnal az 1. pályát (Klasszikus):
        SceneManager.LoadScene(1);

        // B) Vigye a Pályaválasztóra (ahol csak a Level 1 gomb aktív):
        // SceneManager.LoadScene("LevelSelect");
    }

    // --- LOGIKA ---
    // Ezt kell módosítani: Ne a mentett szintet töltse be, hanem a Pályaválasztót!
    private void LoadGameFromSlot(int slotIndex)
    {
        int savedLevel = SaveSystem.GetSavedLevel(slotIndex); // Csak hogy tudjuk, van mentés
        int difficulty = SaveSystem.GetSavedDifficulty(slotIndex);

        PlayerPrefs.SetInt("CurrentSlot", slotIndex);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        GameManager.ResetStaticVariablesForNewGame();

        // --- MÓDOSÍTÁS ITT: ---
        // SceneManager.LoadScene(savedLevel); // HELYETT:
        SceneManager.LoadScene("LevelSelect"); // Töltsük be a választót!
        // (Vagy használd az indexét, ha tudod, pl. SceneManager.LoadScene(6);)
    }

    // Törlés gombhoz (ha akarsz ilyet a slot mellé)
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

    // Ezt húzd a Slot Panel "Back" gombjára!
    public void BackToMainMenu()
    {
        saveSlotsPanel.SetActive(false);
        // Ha van más panel is nyitva, azt is zárd be itt
        ShowMainMenu(); // Ez a függvényed (amit korábban írtunk) már beállítja a fókuszt a Play gombra!
    }

    // Ezt húzd a Difficulty Panel "Back" gombjára!
    public void BackToSlots()
    {
        difficultyPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);

        // VISSZAADJUK A FÓKUSZT A SLOT 1-RE
        SetFirstSelected(slot1ButtonObj);
    }

    // Ezt húzd a Settings Panel "Back" gombjára a Fõmenüben!
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        ShowMainMenu(); // Visszadob a fõmenübe és fókuszál
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}