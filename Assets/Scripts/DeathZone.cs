using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private bool isDead = false; // Prevents the sound from playing multiple times

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Use the isDead variable here!
        if (collision.CompareTag("Player") && !isDead)
        {
            isDead = true;
            // ... rest of your code
        }
    }


    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}