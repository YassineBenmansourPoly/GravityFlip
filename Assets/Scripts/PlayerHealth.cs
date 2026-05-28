using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("UI Hearts")]
    public Image[] hearts;

    [Header("Damage Feedback")]
    public float invincibilityTime = 1f;
    public float flashInterval = 0.1f;
    public Color hitColor = Color.red;
    public float knockbackForce = 6f;

    [Header("Death")]
    public string deathAnimationStateName = "Player_Death";
    public float deathAnimationDuration = 0.8f;
    public float deathSpinSpeed = 360f;

    private bool isInvincible;
    private bool isDead;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement playerMovement;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        UpdateHearts();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitFeedback());
        }
    }

    void UpdateHearts()
    {
        if (hearts == null)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].gameObject.SetActive(i < currentHealth);
            }
        }
    }

    IEnumerator HitFeedback()
    {
        isInvincible = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(-transform.localScale.x * knockbackForce, rb.linearVelocity.y);
        }

        float timer = 0f;

        while (timer < invincibilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = hitColor;
            }

            yield return new WaitForSeconds(flashInterval);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
    }

    void Die()
    {
        if (isDead)
            return;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        isInvincible = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (animator != null && !string.IsNullOrEmpty(deathAnimationStateName))
            animator.Play(deathAnimationStateName, 0, 0f);

        float timer = 0f;
        Vector3 startScale = transform.localScale;

        while (timer < deathAnimationDuration)
        {
            float progress = deathAnimationDuration <= 0f ? 1f : timer / deathAnimationDuration;

            // Simple fallback death animation in case the Animator clip has no visible frames yet.
            transform.Rotate(0f, 0f, deathSpinSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.65f, progress);

            if (spriteRenderer != null)
            {
                Color color = originalColor;
                color.a = Mathf.Lerp(1f, 0.25f, progress);
                spriteRenderer.color = color;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            gameManager = new GameObject("GameManager").AddComponent<GameManager>();

        if (gameManager != null)
            gameManager.GameOver();
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        isDead = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        transform.rotation = Quaternion.identity;

        if (playerMovement != null)
            playerMovement.enabled = true;

        UpdateHearts();
    }
}
