using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "Level2";
    public float delayBeforeLoad = 2.0f; // Increased so player can see the stars!

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Safety Gate
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("LEVEL COMPLETE - TRIGERRED");

            // 2. TRIGGER AUDIO (The new thing)
            if (AudioManager.instance != null)
                AudioManager.instance.TriggerVictory();

            // 3. TRIGGER STAR UI (The new thing)
            if (StarManager.instance != null)
                StarManager.instance.OnLevelComplete();

            // 4. STOP THE PLAYER
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.CompleteLevel(); // This script likely freezes player physics
            }

            // 5. START THE COUNTDOWN TO THE NEXT LEVEL
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        // Wait so the player can hear the music and feel happy about their stars
        yield return new WaitForSeconds(delayBeforeLoad);

        // Reset TimeScale in case the game was paused
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }
}