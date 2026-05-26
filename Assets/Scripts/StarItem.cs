using UnityEngine;

public class StarItem : MonoBehaviour
{
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;

            // 1. Tell the star manager to update the HUD icons
            if (StarManager.instance != null)
                StarManager.instance.CollectStar();

            // 2. Play the star collection sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.starSound);

            // 3. Remove the star from the level
            Destroy(gameObject);
        }
    }
}