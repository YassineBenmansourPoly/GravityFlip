using UnityEngine;
using System.Collections;

public class BossEnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public LockedExitDoor lockedExitDoor;

    [Header("Hit Animation")]
    public float hitFlashDuration = 0.12f;
    public float hitScaleAmount = 1.18f;

    [Header("Death Animation")]
    public float deathDuration = 0.75f;
    public float deathSpinSpeed = 540f;
    public float deathFloatAmount = 1.2f;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private bool isDead;
    private Coroutine hitRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathAnimation());
            return;
        }

        PlayHitAnimation();
    }

    private void PlayHitAnimation()
    {
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitAnimation());
    }

    private IEnumerator HitAnimation()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        transform.localScale = originalScale * hitScaleAmount;

        yield return new WaitForSeconds(hitFlashDuration);

        if (!isDead)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            transform.localScale = originalScale;
        }
    }

    private IEnumerator DeathAnimation()
    {
        isDead = true;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        if (bossCollider != null)
            bossCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < deathDuration)
        {
            float progress = timer / deathDuration;

            transform.Rotate(0f, 0f, deathSpinSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.up * deathFloatAmount, progress);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

            if (spriteRenderer != null)
            {
                Color color = Color.Lerp(originalColor, new Color(1f, 0.35f, 0.05f, 1f), progress);
                color.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (lockedExitDoor != null)
            lockedExitDoor.Unlock();

        Destroy(gameObject);
    }
}
