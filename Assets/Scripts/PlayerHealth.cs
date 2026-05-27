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

    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        UpdateHearts();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return;

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
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.GameOver();
        }

        gameObject.SetActive(false);
    }
}
