using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DoubleJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private int extraJumps = 1;
    [SerializeField] private float doubleJumpForceMultiplier = 0.9f;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private int jumpsRemaining;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        if (playerCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (collider.enabled)
                {
                    playerCollider = collider;
                    break;
                }
            }
        }

        jumpsRemaining = extraJumps;
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        // Touching the floor or ceiling restores the extra jump.
        if (grounded)
        {
            jumpsRemaining = extraJumps;
            return;
        }

        if (Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
        {
            DoDoubleJump();
        }
    }

    private void DoDoubleJump()
    {
        float gravityDirection = playerMovement != null ? playerMovement.gravityDirection : Mathf.Sign(rb.gravityScale);
        float jumpForce = playerMovement != null ? playerMovement.jumpForce : 10f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce * doubleJumpForceMultiplier * gravityDirection, ForceMode2D.Impulse);

        jumpsRemaining--;
    }

    private bool IsGrounded()
    {
        if (playerCollider == null)
            return false;

        float gravityDirection = playerMovement != null ? playerMovement.gravityDirection : Mathf.Sign(rb.gravityScale);
        Vector2 checkDirection = gravityDirection > 0f ? Vector2.down : Vector2.up;

        if (groundLayer.value == 0)
            return IsTouchingTaggedGround(checkDirection);

        RaycastHit2D hit = Physics2D.BoxCast(
            playerCollider.bounds.center,
            new Vector2(playerCollider.bounds.size.x * 0.55f, playerCollider.bounds.size.y * 0.9f),
            0f,
            checkDirection,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private bool IsTouchingTaggedGround(Vector2 checkDirection)
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            playerCollider.bounds.center,
            new Vector2(playerCollider.bounds.size.x * 0.55f, playerCollider.bounds.size.y * 0.9f),
            0f,
            checkDirection,
            groundCheckDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
                return true;
        }

        return false;
    }
}
