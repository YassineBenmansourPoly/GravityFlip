using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Gravity")]
    public float gravityDirection = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleGravityFlip();
        UpdateAnimations();
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce * gravityDirection, ForceMode2D.Impulse);
        }
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
    }

    void UpdateAnimations()
    {
        float move = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        float verticalVelocity = rb.linearVelocity.y * gravityDirection;

        animator.SetFloat("Speed", move);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", verticalVelocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}