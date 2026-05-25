using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.4f;

    [Header("Gravity")]
    public float gravityDirection = 1f;

    [Header("Collision Safety")]
    [SerializeField] private bool useContinuousCollision = true;
    [SerializeField] private float maxFallSpeed = 25f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private float groundNormalThreshold = 0.75f;
    [SerializeField] private float groundContactTolerance = 0.12f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;
    private bool isLevelComplete = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();

        if (useContinuousCollision)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (isLevelComplete) return;

        if (!isDashing)
        {
            HandleMovement();
            HandleJump();
            HandleGravityFlip();
        }

        HandleDash();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isLevelComplete || isDashing) return;

        ClampFallSpeed();
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
        else if (move < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsActuallyGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce * gravityDirection, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && IsActuallyGrounded())
        {
            StartCoroutine(Dash());
        }
    }

    void ClampFallSpeed()
    {
        if (maxFallSpeed <= 0f) return;

        Vector2 velocity = rb.linearVelocity;
        float fallingSpeed = -velocity.y * gravityDirection;

        if (fallingSpeed > maxFallSpeed)
        {
            velocity.y = -maxFallSpeed * gravityDirection;
            rb.linearVelocity = velocity;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;

        float inputDirection = Input.GetAxisRaw("Horizontal");

        float dashDirection = inputDirection != 0
            ? inputDirection
            : (transform.localScale.x > 0f ? 1f : -1f);

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravityScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        if (IsActuallyGrounded())
        {
            canDash = true;
        }
    }

    bool IsActuallyGrounded()
    {
        if (playerCollider == null) return false;

        Vector2 checkDirection = gravityDirection > 0f
            ? Vector2.down
            : Vector2.up;

        RaycastHit2D hit = Physics2D.BoxCast(
            playerCollider.bounds.center,
            new Vector2(playerCollider.bounds.size.x * 0.55f, playerCollider.bounds.size.y * 0.9f),
            0f,
            checkDirection,
            0.10f,
            groundLayer
        );

        return hit.collider != null;
    }

    void HandleGravityFlip()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            FlipGravity();
        }
    }

    void FlipGravity()
    {
        gravityDirection *= -1f;
        rb.gravityScale *= -1f;

        Vector3 scale = transform.localScale;
        scale.y *= -1f;
        transform.localScale = scale;

        isGrounded = false;
    }

    void UpdateAnimations()
    {
        float move = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        float verticalVelocity = rb.linearVelocity.y * gravityDirection;

        if (animator != null)
        {
            animator.SetFloat("Speed", move);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", verticalVelocity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            isGrounded = false;
        }
    }

    void CheckGroundContact(Collision2D collision)
    {
        if (!IsGroundLayer(collision.gameObject)) return;
        if (playerCollider == null) return;

        Vector2 validGroundNormal = gravityDirection > 0f
            ? Vector2.up
            : Vector2.down;

        Bounds bounds = playerCollider.bounds;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            float normalMatch = Vector2.Dot(contact.normal, validGroundNormal);
            if (normalMatch < groundNormalThreshold)
                continue;

            bool contactIsAtFeet;

            if (gravityDirection > 0f)
            {
                contactIsAtFeet = contact.point.y <= bounds.min.y + groundContactTolerance;
            }
            else
            {
                contactIsAtFeet = contact.point.y >= bounds.max.y - groundContactTolerance;
            }

            if (contactIsAtFeet)
            {
                isGrounded = true;
                canDash = true;
                return;
            }
        }
    }

    bool IsGroundLayer(GameObject obj)
    {
        if (groundLayer.value == 0)
        {
            return obj.CompareTag("Ground");
        }

        return (groundLayer.value & (1 << obj.layer)) != 0;
    }

    public void CompleteLevel()
    {
        isLevelComplete = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        if (sr != null)
            sr.enabled = false;
    }
}
