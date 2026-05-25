using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private Vector2 direction = Vector2.right;
    private float speed = 12f;
    private float destroyTime;
    private LayerMask hitLayers;

    public void Initialize(Vector2 shotDirection, float shotSpeed, float lifetime, LayerMask collisionLayers)
    {
        direction = shotDirection.sqrMagnitude > 0.001f
            ? shotDirection.normalized
            : Vector2.right;

        speed = shotSpeed;
        hitLayers = collisionLayers;
        destroyTime = Time.time + lifetime;
    }

    private void Update()
    {
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + direction * speed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, hitLayers);
        if (hit.collider != null)
        {
            BreakableTilemap breakableTilemap = hit.collider.GetComponent<BreakableTilemap>();

            if (breakableTilemap != null)
                breakableTilemap.TryBreakAtWorldPoint(hit.point, direction);

            Destroy(gameObject);
            return;
        }

        transform.position = endPosition;

        if (Time.time >= destroyTime)
            Destroy(gameObject);
    }
}
