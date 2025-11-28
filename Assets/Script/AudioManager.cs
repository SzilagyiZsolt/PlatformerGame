using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Források")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Hangfájlok")]
    public AudioClip backgroundMusic;
    public AudioClip jumpSound;
    public AudioClip keyPickupSound;
    public AudioClip deathSound;
    public AudioClip levelWinSound;

    // --- ÚJ: Gombnyomás hang ---
    public AudioClip buttonClickSound;

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
            return;
        }
    }

    void Start()
    {
        // Zene indítása
        if (backgroundMusic != null && musicSource != null)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }

    // --- LEJÁTSZÓ FÜGGVÉNYEK ---

    public void PlayJump() { PlaySFX(jumpSound); }
    public void PlayKeyPickup() { PlaySFX(keyPickupSound); }
    public void PlayDeath() { PlaySFX(deathSound); }
    public void PlayWin() { PlaySFX(levelWinSound); }

    // --- ÚJ: Ezt kötjük be a Gombokra ---
    public void PlayButtonSound()
    {
        PlaySFX(buttonClickSound);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // Random pitch (Opcionális extra): 
            // Kicsit eltorzítjuk a hangmagasságot, hogy ne legyen gépies a kattogás
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip);
        }
    }
}