using UnityEngine;

public class FallingHeadTrap : MonoBehaviour
{
    [Header("Detection")]
    public float detectionWidth = 2f;
    public float detectionDistance = 8f;

    [Header("Movement")]
    public float fallSpeed = 18f;
    public float returnSpeed = 6f;
    public float waitTime = 2f;
    public float skinWidth = 0.03f;

    private Rigidbody2D rb;
    private Collider2D trapCollider;
    private Vector2 startPosition;

    private bool falling;
    private bool waiting;
    private bool returning;
    private float waitTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trapCollider = GetComponent<Collider2D>();
        startPosition = rb.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (!falling && !waiting && !returning)
        {
            CheckForPlayerBelow();
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                returning = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (falling)
        {
            FallWithGroundCheck();
        }
        else if (returning)
        {
            Vector2 newPosition = Vector2.MoveTowards(
                rb.position,
                startPosition,
                returnSpeed * Time.fixedDeltaTime
            );

            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, startPosition) < 0.03f)
            {
                rb.MovePosition(startPosition);
                returning = false;
            }
        }
    }

    void CheckForPlayerBelow()
    {
        Vector2 boxCenter = (Vector2)transform.position + Vector2.down * (detectionDistance / 2f);
        Vector2 boxSize = new Vector2(detectionWidth, detectionDistance);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                falling = true;
                return;
            }
        }
    }

    void FallWithGroundCheck()
    {
        float moveDistance = fallSpeed * Time.fixedDeltaTime;

        Bounds bounds = trapCollider.bounds;
        Vector2 castCenter = bounds.center;
        Vector2 castSize = bounds.size * 0.95f;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            castCenter,
            castSize,
            0f,
            Vector2.down,
            moveDistance + skinWidth
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == trapCollider) continue;

            if (hit.collider.CompareTag("Player"))
            {
                KillPlayer(hit.collider.gameObject);
                return;
            }

            if (hit.collider.CompareTag("Ground"))
            {
                float safeDistance = Mathf.Max(0f, hit.distance - skinWidth);
                rb.MovePosition(rb.position + Vector2.down * safeDistance);

                falling = false;
                waiting = true;
                waitTimer = waitTime;
                return;
            }
        }

        rb.MovePosition(rb.position + Vector2.down * moveDistance);
    }

    void KillPlayer(GameObject player)
    {
        player.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 boxCenter = transform.position + Vector3.down * (detectionDistance / 2f);
        Vector3 boxSize = new Vector3(detectionWidth, detectionDistance, 0.1f);

        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}