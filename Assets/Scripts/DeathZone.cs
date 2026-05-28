using UnityEngine;
public class DeathZone : MonoBehaviour
{
    private bool isDead = false; // Prevents the sound from playing multiple times

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Use the isDead variable here!
        if (collision.CompareTag("Player") && !isDead)
        {
            isDead = true;

            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
                gameManager = new GameObject("GameManager").AddComponent<GameManager>();

            gameManager.GameOver(collision.gameObject);
        }
    }
}
