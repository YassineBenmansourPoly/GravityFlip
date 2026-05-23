using UnityEngine;

public class FireToggleTrap : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite offSprite;
    public Sprite onSprite;

    [Header("Timing")]
    public float toggleInterval = 1f;
    public bool startsOn = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D fireCollider;
    private bool isOn;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fireCollider = GetComponent<Collider2D>();

        isOn = startsOn;
        ApplyState();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= toggleInterval)
        {
            timer = 0f;
            isOn = !isOn;
            ApplyState();
        }
    }

    void ApplyState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOn ? onSprite : offSprite;
        }

        if (fireCollider != null)
        {
            fireCollider.enabled = isOn;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOn) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(1);
            }
        }
    }
}