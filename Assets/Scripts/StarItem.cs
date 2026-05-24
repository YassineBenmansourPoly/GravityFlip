using UnityEngine;

public class StarItem : MonoBehaviour
{
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure your Player object has the tag "Player"
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;

            // 1. Tell the manager to count this star
            if (StarManager.instance != null)
                StarManager.instance.CollectStar();

            // 2. Play a sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.starSound);

            // 3. Vanish!
            Destroy(gameObject);
        }
    }
}