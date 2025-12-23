using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Szükség lesz rá a gombok feliratához

public class MainMenu : MonoBehaviour
{
    [Header("Panelek")]
    public GameObject saveSlotsPanel;
    public GameObject difficultyPanel;

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
    }

    public void OnPlayButtonClicked()
    {
        // --- EZT A SORT TÖRÖLD KI VAGY KOMMENTELD KI: ---
        // mainMenuPanel.SetActive(false); 
        // ------------------------------------------------

        // Csak a Save Panel jelenjen meg (a Main Menu felett)
        saveSlotsPanel.SetActive(true);

        UpdateSlotUI();
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
        }
    }

    // Ezt hívják a Nehézség gombok (0=Normal, 1=Hard)
    // Csak ÚJ játéknál fut le
    public void OnDifficultySelected(int difficulty)
    {
        // 1. Elmentjük az új játék adatait a slotba (Level 1, Round 1)
        // Fontos: Itt inicializáljuk a mentést!
        SaveSystem.SaveGame(selectedSlot, 1, difficulty, 1);

        // 2. Beállítjuk a PlayerPrefs-et a GameManagernek is (hogy tudja, mi van)
        PlayerPrefs.SetInt("CurrentSlot", selectedSlot);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        // 3. Indítás (Reseteljük a statikus változókat)
        GameManager.ResetStaticVariablesForNewGame();
        SceneManager.LoadScene(1); // 1. pálya betöltése (ellenõrizd a Build Settings-ben!)
    }

    // --- LOGIKA ---
    private void LoadGameFromSlot(int slotIndex)
    {
        int levelToLoad = SaveSystem.GetSavedLevel(slotIndex);
        int difficulty = SaveSystem.GetSavedDifficulty(slotIndex);

        // Beállítjuk az aktuális játékmenet adatait
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);
        PlayerPrefs.SetInt("Difficulty", difficulty);

        GameManager.ResetStaticVariablesForNewGame();
        SceneManager.LoadScene(levelToLoad);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}