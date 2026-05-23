using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum StartDirection
    {
        Right,
        Left
    }

    [Header("Movement")]
    public float moveDistance = 4f;
    public float speed = 2f;
    public StartDirection startDirection = StartDirection.Right;

    private Vector3 startPosition;
    private int direction;

    void Start()
    {
        startPosition = transform.position;
        direction = startDirection == StartDirection.Right ? 1 : -1;
    }

    void FixedUpdate()
    {
        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;

        float distanceFromStart = transform.position.x - startPosition.x;

        if (startDirection == StartDirection.Right)
        {
            if (distanceFromStart >= moveDistance)
            {
                direction = -1;
            }
            else if (distanceFromStart <= 0f)
            {
                direction = 1;
            }
        }
        else
        {
            if (distanceFromStart <= -moveDistance)
            {
                direction = 1;
            }
            else if (distanceFromStart >= 0f)
            {
                direction = -1;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}