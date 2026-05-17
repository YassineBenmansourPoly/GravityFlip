using UnityEngine;

public class HazardTilemap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindFirstObjectByType<GameManager>().GameOver();
        }
    }
}