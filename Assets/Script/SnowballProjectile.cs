using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    [Header("Beállítások")]
    public float lifetime = 5f; // Hány másodperc múlva tûnjön el magától (hogy ne teljen meg a pálya)
    public GameObject hitEffect; // Opcionális: por effekt becsapódáskor

    void Start()
    {
        // Biztonsági törlés, ha kirepülne a pályáról
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Ha a játékost találja el
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.Die();
            }
            Smash();
        }
        // 2. Ha falat/talajt talál el (Default vagy Ground réteg)
        // Fontos: A hóembernek (Thrower) adjunk saját taget vagy layert, 
        // hogy a hógolyó ne robbanjon fel azonnal a hóember hasában!
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Default") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Smash();
        }
    }

    void Smash()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}