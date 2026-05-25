using UnityEngine;

public class MovingSawTrap : MonoBehaviour
{
    private const float ForcedRotationSpeed = 150f;

    [Header("Movement")]
    public float moveDistance = 4f;
    public float speed = 2f;
    public bool moveRightFirst = true;

    [Header("Damage")]
    public bool disablePlayerOnHit = true;

    [Header("Animation")]
    public bool spinSaw = true;
    public float rotationSpeed = 150f;
    public bool reverseSpinWithMovement = true;

    private Vector3 startPosition;
    private int direction;

    void Awake()
    {
        rotationSpeed = ForcedRotationSpeed;
    }

    void OnValidate()
    {
        rotationSpeed = ForcedRotationSpeed;
    }

    void Start()
    {
        startPosition = transform.position;
        direction = moveRightFirst ? 1 : -1;
    }

    void Update()
    {
        AnimateSaw();

        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        float distanceFromStart = transform.position.x - startPosition.x;

        if (moveRightFirst)
        {
            if (distanceFromStart >= moveDistance)
                direction = -1;
            else if (distanceFromStart <= 0f)
                direction = 1;
        }
        else
        {
            if (distanceFromStart <= -moveDistance)
                direction = 1;
            else if (distanceFromStart >= 0f)
                direction = -1;
        }
    }

    void AnimateSaw()
    {
        if (!spinSaw) return;

        // Rotating the saw GameObject gives the blade a simple spinning animation.
        float spinDirection = reverseSpinWithMovement ? -direction : -1f;
        transform.Rotate(0f, 0f, rotationSpeed * spinDirection * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(1);
        }
    }
}
