using UnityEngine;
using UnityEngine.UI;
using TMPro; // Kell a TextMeshPro miatt!
using System.Collections.Generic;
using UnityEngine.EventSystems; // --- EZT ADD HOZZÁ! ---

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elemek")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Dropdown resolutionDropdown;

    [Header("Százalék Kijelzõk (ÚJ)")]
    public TextMeshProUGUI musicPercentText; // Zene %
    public TextMeshProUGUI sfxPercentText;   // SFX %

    [Header("Kontroller Navigáció")]
    // Húzd be ide a Zene Csúszkát (Music Slider) az Inspectorban!
    public GameObject firstSelectedObject;

    private List<Resolution> filteredResolutions;

    void Start()
    {
        // --- HANGERÕK ÉS SZÖVEGEK BETÖLTÉSE ---
        if (AudioManager.instance != null)
        {
            float currentMusicVol = AudioManager.instance.musicSource.volume;
            float currentSFXVol = AudioManager.instance.sfxSource.volume;

            // Beállítjuk a csúszkákat a mentett értékre
            if (musicSlider != null) musicSlider.value = currentMusicVol;
            if (sfxSlider != null) sfxSlider.value = currentSFXVol;

            // Beállítjuk a szövegeket is indításkor
            UpdateLabels(currentMusicVol, currentSFXVol);
        }

        // ... (A felbontásos rész VÁLTOZATLAN, itt hagytam a mûködéshez) ...
        if (resolutionDropdown != null)
        {
            Resolution[] allResolutions = Screen.resolutions;
            filteredResolutions = new List<Resolution>();
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < allResolutions.Length; i++)
            {
                string option = allResolutions[i].width + " x " + allResolutions[i].height;
                if (!options.Contains(option))
                {
                    options.Add(option);
                    filteredResolutions.Add(allResolutions[i]);
                    if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
                    {
                        currentResolutionIndex = filteredResolutions.Count - 1;
                    }
                }
            }
            filteredResolutions.Sort((a, b) => b.width.CompareTo(a.width));

            // Újra kell generálni az opciókat a rendezés után
            options.Clear();
            currentResolutionIndex = 0;
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                options.Add(filteredResolutions[i].width + " x " + filteredResolutions[i].height);
                if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                    currentResolutionIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    private void OnEnable()
    {
        // Amikor a Beállítások ablak megjelenik (akár Fõmenüben, akár Pause-ban)
        // Azonnal rátesszük a fókuszt az elsõ elemre.
        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Elõzõ törlése
            EventSystem.current.SetSelectedGameObject(firstSelectedObject); // Új beállítása
        }
    }

    // --- HANGERÕ FÜGGVÉNYEK (FRISSÍTETT) ---

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetMusicVolume(volume);

        // Frissítjük a szöveget: 0.5 -> "50%"
        if (musicPercentText != null)
        {
            musicPercentText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetSFXVolume(volume);

        // Frissítjük a szöveget
        if (sfxPercentText != null)
        {
            sfxPercentText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    // Segédfüggvény a Start-hoz
    void UpdateLabels(float musicVol, float sfxVol)
    {
        if (musicPercentText != null) musicPercentText.text = Mathf.RoundToInt(musicVol * 100) + "%";
        if (sfxPercentText != null) sfxPercentText.text = Mathf.RoundToInt(sfxVol * 100) + "%";
    }

    // ... (A többi függvény változatlan) ...
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }
}