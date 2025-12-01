using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Források")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Zene Beállítások")]
    public AudioClip menuMusic;
    public AudioClip[] gameMusicPlaylist;

    [Header("Effektek")]
    public AudioClip jumpSound;
    public AudioClip keyPickupSound;
    public AudioClip deathSound;
    public AudioClip levelWinSound;
    public AudioClip buttonClickSound;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    // ÚJ VÁLTOZÓ: Eltároljuk, melyik pályán voltunk legutóbb
    private string lastSceneName;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. HANGERÕK BETÖLTÉSE
        float savedMusicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float savedSFXVol = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        if (musicSource != null) musicSource.volume = savedMusicVol;
        if (sfxSource != null) sfxSource.volume = savedSFXVol;

        AudioClip clipToPlay = null;

        // 2. DÖNTÉS: MENÜ VAGY JÁTÉK?
        if (scene.name == "MainMenu" || scene.name == "Main Menu")
        {
            clipToPlay = menuMusic;
        }
        else // JÁTÉKBAN VAGYUNK
        {
            // --- OKOS ZENE VÁLTÁS ---
            // Csak akkor választunk új zenét, ha TÉNYLEG másik pályára léptünk!
            // Ha a név megegyezik a legutóbbival (lastSceneName), az azt jelenti, 
            // hogy Restart történt -> Ilyenkor NEM választunk újat, marad a régi.
            if (scene.name != lastSceneName)
            {
                if (gameMusicPlaylist != null && gameMusicPlaylist.Length > 0)
                {
                    int randomIndex = Random.Range(0, gameMusicPlaylist.Length);
                    clipToPlay = gameMusicPlaylist[randomIndex];
                }
            }
        }

        // 3. PÁLYANÉV FRISSÍTÉSE
        lastSceneName = scene.name;

        // 4. ZENE LEJÁTSZÁSA (Ha választottunk valamit)
        if (clipToPlay != null)
        {
            PlayMusic(clipToPlay);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        // Ha már ez a zene szól, nem indítjuk újra (így restartnál folytonos marad!)
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // --- BEÁLLÍTÁSOK ÉS EFFEKTEK (VÁLTOZATLAN) ---

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat(MUSIC_KEY, volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat(SFX_KEY, volume);
            PlayerPrefs.Save();
        }
    }

    public void PlayJump() { PlaySFX(jumpSound); }
    public void PlayKeyPickup() { PlaySFX(keyPickupSound); }
    public void PlayDeath() { PlaySFX(deathSound); }
    public void PlayWin() { PlaySFX(levelWinSound); }
    public void PlayButtonSound() { PlaySFX(buttonClickSound); }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip);
        }
    }
}