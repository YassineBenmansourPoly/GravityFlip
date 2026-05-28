using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    private Rigidbody2D rb;
    private float destroyTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        EnsureVisibleFireball();
    }

    public void Launch(Vector2 direction, float speed, float lifetime)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;
        destroyTime = Time.time + lifetime;
    }

    private void Update()
    {
        // Remove missed enemy shots after a short lifetime.
        if (Time.time >= destroyTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy fireballs should not hurt the boss.
        if (collision.CompareTag("Enemy"))
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);

            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Ground") || collision.CompareTag("Rock") || collision.CompareTag("Grass") || collision.CompareTag("Metal"))
            Destroy(gameObject);
    }

    private void EnsureVisibleFireball()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        if (renderer.sprite == null)
            renderer.sprite = CreateSquareSprite(new Color(1f, 0.5f, 0.05f, 1f));

        renderer.color = Color.white;
        renderer.sortingOrder = 20;
    }

    private Sprite CreateSquareSprite(Color color)
    {
        Texture2D texture = new Texture2D(8, 8);
        Color[] pixels = new Color[64];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 16f);
    }
}
