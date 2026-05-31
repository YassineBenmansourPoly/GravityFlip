using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Fireball")]
    public GameObject playerFireballPrefab;
    public Transform firePoint;
    public float fireballSpeed = 12f;
    public float fireballLifetime = 2f;
    public float shootCooldown = 0.25f;

    private float nextShootTime;

    private void Awake()
    {
        // Create a simple fire point if one was not assigned in the Inspector.
        if (firePoint == null)
        {
            GameObject firePointObject = new GameObject("FirePoint");
            firePointObject.transform.SetParent(transform);
            firePointObject.transform.localPosition = new Vector3(0.75f, 0.05f, 0f);
            firePoint = firePointObject.transform;
        }
    }

    private void Update()
    {
        // Left or right mouse button shoots the player's fireball.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            Shoot();
    }

    private void Shoot()
    {
        if (Time.time < nextShootTime)
            return;

        float facingDirection = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 direction = Vector2.right * facingDirection;

        GameObject fireballObject = playerFireballPrefab != null
            ? Instantiate(playerFireballPrefab, firePoint.position, Quaternion.identity)
            : CreateFallbackFireball(firePoint.position);

        fireballObject.transform.localScale = new Vector3(facingDirection, 1f, 1f);

        PlayerFireball fireball = fireballObject.GetComponent<PlayerFireball>();
        if (fireball != null)
            fireball.Launch(direction, fireballSpeed, fireballLifetime);

        nextShootTime = Time.time + shootCooldown;
    }

    private GameObject CreateFallbackFireball(Vector3 position)
    {
        GameObject fireballObject = new GameObject("PlayerFireball");
        fireballObject.transform.position = position;

        SpriteRenderer renderer = fireballObject.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite(new Color(1f, 0.5f, 0.05f, 1f));
        renderer.sortingOrder = 20;

        Rigidbody2D rb = fireballObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        CircleCollider2D collider = fireballObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.18f;

        fireballObject.AddComponent<PlayerFireball>();
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
