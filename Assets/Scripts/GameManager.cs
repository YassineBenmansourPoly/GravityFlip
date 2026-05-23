using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    public void GameOver()
    {
        // 1. Tell Audio to stop music and play death sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.TriggerGameOver(); // Name must match AudioManager exactly!
        }

        // 2. Show the UI
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // 3. Stop Time 
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}