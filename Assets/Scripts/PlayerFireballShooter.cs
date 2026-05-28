using UnityEngine;

public class PlayerFireballShooter : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode shootKey = KeyCode.F;
    [SerializeField] private KeyCode alternateShootKey = KeyCode.E;
    [SerializeField] private bool allowMouseButton = true;

    [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private string fireballResourcePrefabName = "FireballProjectile";
    [SerializeField] private Sprite fireballSprite;
    [SerializeField] private float fireballSpeed = 12f;
    [SerializeField] private float fireballLifetime = 2f;
    [SerializeField] private float shootCooldown = 0.25f;
    [SerializeField] private float fireballVisualScale = 0.6f;
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0.75f, 0.05f);
    [SerializeField] private LayerMask fireballHitLayers = 1 << 3;

    private SpriteRenderer playerSpriteRenderer;
    private Sprite fallbackSprite;
    private GameObject cachedResourceFireballPrefab;
    private float nextAllowedShotTime;

    private void Awake()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        bool shootPressed = Input.GetKeyDown(shootKey)
            || Input.GetKeyDown(alternateShootKey)
            || (allowMouseButton && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)));

        if (shootPressed)
            Shoot();
    }

    private void Shoot()
    {
        if (Time.time < nextAllowedShotTime)
            return;

        float facingDirection = transform.localScale.x >= 0f ? 1f : -1f;
        float gravityDirection = transform.localScale.y >= 0f ? 1f : -1f;
        Vector3 spawnOffset = new Vector3(muzzleOffset.x * facingDirection, muzzleOffset.y * gravityDirection, 0f);

        GameObject fireball = CreateFireballObject();
        fireball.name = "Fireball";
        fireball.transform.position = transform.position + spawnOffset;
        fireball.transform.localScale = new Vector3(facingDirection * fireballVisualScale, fireballVisualScale, 1f);

        SpriteRenderer fireballRenderer = fireball.GetComponent<SpriteRenderer>();
        if (fireballRenderer == null)
            fireballRenderer = fireball.AddComponent<SpriteRenderer>();

        if (fireballRenderer.sprite == null)
            fireballRenderer.sprite = fireballSprite != null ? fireballSprite : GetFallbackSprite();

        bool usingGeneratedFallback = fireballRenderer.sprite == fallbackSprite;
        fireballRenderer.color = usingGeneratedFallback ? new Color(1f, 0.55f, 0.08f, 1f) : Color.white;

        if (playerSpriteRenderer != null)
        {
            fireballRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            fireballRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1;
        }

        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile == null)
            projectile = fireball.AddComponent<FireballProjectile>();

        projectile.Initialize(Vector2.right * facingDirection, fireballSpeed, fireballLifetime, fireballHitLayers);

        nextAllowedShotTime = Time.time + shootCooldown;
    }

    private GameObject CreateFireballObject()
    {
        GameObject prefab = fireballPrefab != null ? fireballPrefab : GetResourceFireballPrefab();

        if (prefab != null)
            return Instantiate(prefab);

        return new GameObject("Fireball");
    }

    private GameObject GetResourceFireballPrefab()
    {
        if (cachedResourceFireballPrefab != null || string.IsNullOrWhiteSpace(fireballResourcePrefabName))
            return cachedResourceFireballPrefab;

        cachedResourceFireballPrefab = Resources.Load<GameObject>(fireballResourcePrefabName);
        return cachedResourceFireballPrefab;
    }

    private Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
            return fallbackSprite;

        Texture2D texture = new Texture2D(8, 8);
        Color[] pixels = new Color[64];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(1f, 0.45f, 0.05f, 1f);

        texture.SetPixels(pixels);
        texture.Apply();
        fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 16f);

        return fallbackSprite;
    }
}
