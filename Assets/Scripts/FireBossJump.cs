using UnityEngine;

public class FireBossJump : MonoBehaviour
{
    [Header("Jumping")]
    public float jumpForce = 7f;
    public float jumpIntervalMin = 1.2f;
    public float jumpIntervalMax = 2.8f;
    public float arenaLeftX = -12f;
    public float arenaRightX = 12f;

    private Rigidbody2D rb;
    private float nextJumpTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ScheduleNextJump();
    }

    private void Update()
    {
        KeepInsideArena();

        if (Time.time >= nextJumpTime && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            ScheduleNextJump();
        }
    }

    private void KeepInsideArena()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, arenaLeftX, arenaRightX);
        transform.position = position;
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.75f);
        return hit.collider != null && (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Rock") || hit.collider.CompareTag("Grass") || hit.collider.CompareTag("Metal"));
    }

    private void ScheduleNextJump()
    {
        nextJumpTime = Time.time + Random.Range(jumpIntervalMin, jumpIntervalMax);
    }
}
