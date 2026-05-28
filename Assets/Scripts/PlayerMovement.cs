using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float gravityFlipStartDelay = 0.45f;
    [SerializeField] private float gravityFlipCooldown = 3f;

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

    // --- MERGED VARIABLES ---
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;
    private string currentSurface = "";
    private float footstepTimer = 0f;
    private bool isLevelComplete = false;
    private float gravityFlipLockedUntil;
    private bool gravityFlipDelayStarted;
    private bool useGravityFlipCooldown;
    private bool dashUnlocked;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();

        if (useContinuousCollision)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        string sceneName = SceneManager.GetActiveScene().name;
        useGravityFlipCooldown = ShouldDelayGravityFlip(sceneName);
        dashUnlocked = ShouldUnlockDash(sceneName);
        StartGravityFlipDelayIfNeeded();
    }

    private void OnEnable()
    {
        StartGravityFlipDelayIfNeeded();
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

    // --- DASHING LOGIC ---
    void HandleDash()
    {
        if (!dashUnlocked)
            return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && IsActuallyGrounded())
        {
            StartCoroutine(Dash());
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.dashSound);
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
        if (Time.unscaledTime < gravityFlipLockedUntil)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            gravityDirection *= -1f;
            rb.gravityScale *= -1f;

            Vector3 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;

            isGrounded = false;
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.flipSound);

            if (useGravityFlipCooldown)
                gravityFlipLockedUntil = Time.unscaledTime + gravityFlipCooldown;
        }
    }

    private void StartGravityFlipDelayIfNeeded()
    {
        if (gravityFlipDelayStarted || !ShouldDelayGravityFlip(SceneManager.GetActiveScene().name))
            return;

        gravityFlipDelayStarted = true;
        StartCoroutine(DelayGravityFlipAfterIntro());
    }

    private IEnumerator DelayGravityFlipAfterIntro()
    {
        yield return null;

        while (LevelIntroUI.IsIntroPlaying || FindFirstObjectByType<LevelIntroUI>() != null)
            yield return null;

        gravityFlipLockedUntil = Time.unscaledTime + gravityFlipStartDelay;
    }

    private bool ShouldDelayGravityFlip(string sceneName)
    {
        return sceneName == "Level1"
            || sceneName == "Level2"
            || sceneName == "Level3"
            || sceneName == "Level5"
            || sceneName == "Level6"
            || sceneName == "Level7";
    }

    private bool ShouldUnlockDash(string sceneName)
    {
        if (sceneName == "BossLevel1")
            return true;

        if (!sceneName.StartsWith("Level"))
            return false;

        string numberText = sceneName.Substring("Level".Length);
        return int.TryParse(numberText, out int levelNumber) && levelNumber >= 6;
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
        // If we hit a Hazard, just tell the GameManager to handle it
        if (collision.gameObject.CompareTag("Hazard"))
        {
            if (!CheckpointManager.TryRespawnPlayer(gameObject))
                FindAnyObjectByType<GameManager>().GameOver();

            return;
        }

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

        // Corrected a small brace syntax error from the conflict block here:
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
                canDash = true; // Kept your dash reset
                currentSurface = collision.gameObject.tag; // Kept Alnoohy's footstep surface update
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

    // --- LEVEL COMPLETE POLISH (Kept Yassine's cleaner version) ---
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

    public void ResetAfterRespawn()
    {
        StopAllCoroutines();
        isLevelComplete = false;
        isDashing = false;
        canDash = true;
        isGrounded = false;

        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = Mathf.Abs(rb.gravityScale) * gravityDirection;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (sr != null)
            sr.enabled = true;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", false);
            animator.SetFloat("VerticalVelocity", 0f);
        }
    }
}
