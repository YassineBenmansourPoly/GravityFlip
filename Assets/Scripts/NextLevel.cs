using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "Level2";
    public float delayBeforeLoad = 1.5f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;

            // 1. Play Victory Audio
            if (AudioManager.instance != null)
                AudioManager.instance.TriggerVictory();

            // 2. Stop the player
            PlayerMovement pm = collision.GetComponent<PlayerMovement>();
            if (pm != null) pm.CompleteLevel();

            // 3. Go to next scene
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}