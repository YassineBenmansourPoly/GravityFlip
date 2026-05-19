using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Gravity")]
    public float gravityDirection = 1f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private float groundNormalThreshold = 0.75f;
    [SerializeField] private float groundContactTolerance = 0.12f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private string currentSurface = "";
    private float footstepTimer = 0f;
    private bool isGrounded; 
    private bool isLevelComplete = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isLevelComplete) return;

        HandleMovement();
        HandleJump();
        HandleGravityFlip();
        UpdateAnimations();
    }

    void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip Sprite based on direction
        if (move > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        else if (move < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);

        // 1. FOOTSTEP SOUND LOGIC
        if (move != 0 && isGrounded)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                if (AudioManager.instance != null) AudioManager.instance.PlayFootstep(currentSurface);
                footstepTimer = 0.35f;
            }
        }
    }

    void HandleJump()
    {
        // 2. JUMP FIX: Use the 'isGrounded' variable that the collision logic calculates
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce * gravityDirection, ForceMode2D.Impulse);
            isGrounded = false; // Set to false immediately so we can't double jump

            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.jumpSound);
        }
    }

    void HandleGravityFlip()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gravityDirection *= -1f;
            rb.gravityScale *= -1f;

            Vector3 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;

            isGrounded = false;
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.flipSound);
        }
    }

    void UpdateAnimations()
    {
        float moveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        float verticalVelocity = rb.linearVelocity.y * gravityDirection;

        if (animator != null)
        {
            // 3. ANIMATION FIX: Ensuring these match your Animator exactly
            animator.SetFloat("Speed", moveInput);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", verticalVelocity);
        }
    }

    // --- PHYSICS & COLLISION LOGIC ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            currentSurface = collision.gameObject.tag;
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.landingSound);
        }
        CheckGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) { CheckGroundContact(collision); }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject)) isGrounded = false;
    }

    void CheckGroundContact(Collision2D collision)
    {
        if (!IsGroundLayer(collision.gameObject)) return;

        Vector2 validGroundNormal = gravityDirection > 0f ? Vector2.up : Vector2.down;
        Bounds bounds = playerCollider.bounds;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (Vector2.Dot(contact.normal, validGroundNormal) < groundNormalThreshold) continue;

            bool contactIsAtFeet = (gravityDirection > 0f)
                ? contact.point.y <= bounds.min.y + groundContactTolerance
                : contact.point.y >= bounds.max.y - groundContactTolerance;

            if (contactIsAtFeet)
            {
                isGrounded = true;
                currentSurface = collision.gameObject.tag; // Update surface tag for steps
                return;
            }
        }
    }

    bool IsGroundLayer(GameObject obj)
    {
        // 4. TAG RECOGNITION FIX
        if (obj.CompareTag("Grass") || obj.CompareTag("Rock") || obj.CompareTag("Ground")) return true;
        return (groundLayer.value & (1 << obj.layer)) != 0;
    }

    public void CompleteLevel() { isLevelComplete = true; rb.simulated = false; }
}