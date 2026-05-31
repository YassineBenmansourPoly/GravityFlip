using UnityEngine;

public class FireBossShooter : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject enemyFireballPrefab;
    public Transform enemyFirePoint;
    public float shootInterval = 1.5f;
    public float fireballSpeed = 8f;
    public float fireballLifetime = 3f;

    private Transform player;
    private float nextShootTime;

    private void Awake()
    {
        // Create a simple enemy fire point if one was not assigned.
        if (enemyFirePoint == null)
        {
            GameObject firePointObject = new GameObject("EnemyFirePoint");
            firePointObject.transform.SetParent(transform);
            firePointObject.transform.localPosition = new Vector3(-0.6f, 0.1f, 0f);
            enemyFirePoint = firePointObject.transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null || Time.time < nextShootTime)
            return;

        ShootAtPlayer();
        nextShootTime = Time.time + shootInterval;
    }

    private void ShootAtPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)enemyFirePoint.position).normalized;

        GameObject fireballObject = enemyFireballPrefab != null
            ? Instantiate(enemyFireballPrefab, enemyFirePoint.position, Quaternion.identity)
            : CreateFallbackFireball(enemyFirePoint.position);

        EnemyFireball fireball = fireballObject.GetComponent<EnemyFireball>();
        if (fireball != null)
            fireball.Launch(direction, fireballSpeed, fireballLifetime);
    }

    private GameObject CreateFallbackFireball(Vector3 position)
    {
        GameObject fireballObject = new GameObject("EnemyFireball");
        fireballObject.transform.position = position;
        fireballObject.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

        SpriteRenderer renderer = fireballObject.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite(new Color(1f, 0.5f, 0.05f, 1f));
        renderer.color = Color.white;
        renderer.sortingOrder = 20;

        Rigidbody2D rb = fireballObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        CircleCollider2D collider = fireballObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.18f;

        fireballObject.AddComponent<EnemyFireball>();
        return fireballObject;
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
