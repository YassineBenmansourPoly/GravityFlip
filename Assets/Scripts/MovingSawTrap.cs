using UnityEngine;

public class MovingSawTrap : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 4f;
    public float speed = 2f;
    public bool moveRightFirst = true;

    [Header("Damage")]
    public bool disablePlayerOnHit = true;

    private Vector3 startPosition;
    private int direction;

    void Start()
    {
        startPosition = transform.position;
        direction = moveRightFirst ? 1 : -1;
    }

    void Update()
    {
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
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.GameOver();
        }

        if (disablePlayerOnHit)
        {
            player.SetActive(false);
        }
    }
}