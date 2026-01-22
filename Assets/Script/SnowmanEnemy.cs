using UnityEngine;
using System.Collections;

public class SnowmanEnemy : MonoBehaviour
{
    [Header("Dobás Beállítások")]
    public GameObject snowballPrefab;
    public Transform throwPoint;
    public float throwInterval = 5f;

    [Header("Hógolyó Fizika")]
    // X: sebesség (negatív = balra, pozitív = jobbra), Y: 0 (vízszintes)
    public Vector2 throwForce = new Vector2(-15f, 0f);
    public float snowballTorque = 10f;

    [Header("Animáció")]
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ThrowRoutine());
    }

    IEnumerator ThrowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwInterval);
            ThrowSnowball();
        }
    }

    void ThrowSnowball()
    {
        if (snowballPrefab == null || throwPoint == null) return;

        if (anim != null) anim.SetTrigger("Throw");

        GameObject snowball = Instantiate(snowballPrefab, throwPoint.position, Quaternion.identity);

        Rigidbody2D rb = snowball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // FONTOS: Ha azt akarod, hogy nyílegyenesen repüljön és SOHA ne essen le:
            // rb.gravityScale = 0; 

            rb.AddForce(throwForce, ForceMode2D.Impulse);
            rb.AddTorque(snowballTorque, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (throwPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(throwPoint.position, throwPoint.position + (Vector3)throwForce * 0.1f);
        }
    }
}