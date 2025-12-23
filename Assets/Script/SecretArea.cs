using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class SecretArea : MonoBehaviour
{
    [Header("Beállítások")]
    public Tilemap secretWall; // A fal, amit el kell tüntetni
    public float fadeSpeed = 4f; // Milyen gyorsan tûnjön el
    public float transparency = 0.2f; // Mennyire legyen áttetszõ

    [Header("Hangeffekt")]
    public AudioClip discoverySound; // A hang, ami megszólal

    // Statikus memória: megjegyzi a pályák között, hogy mit találtál már meg
    private static HashSet<string> discoveredSecrets = new HashSet<string>();

    private void Start()
    {
        if (secretWall == null) secretWall = GetComponent<Tilemap>();
        // Itt már nem kell AudioSource-t létrehozni!
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Ha még nem találtuk meg ezt a titkot
            if (!discoveredSecrets.Contains(gameObject.name))
            {
                discoveredSecrets.Add(gameObject.name);

                // A központi AudioManager-en keresztül játsszuk le a hangot!
                // Így a hangerõ a beállításokhoz igazodik.
                if (discoverySound != null && AudioManager.instance != null && AudioManager.instance.sfxSource != null)
                {
                    AudioManager.instance.sfxSource.PlayOneShot(discoverySound);
                }
            }

            // A fal vizuális eltüntetése mindig lefut
            StopAllCoroutines();
            StartCoroutine(FadeTo(transparency));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (secretWall == null) yield break;

        Color currentColor = secretWall.color;
        while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
        {
            currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            secretWall.color = currentColor;
            yield return null;
        }
        currentColor.a = targetAlpha;
        secretWall.color = currentColor;
    }

    public static void ResetSecrets()
    {
        discoveredSecrets.Clear();
    }
}