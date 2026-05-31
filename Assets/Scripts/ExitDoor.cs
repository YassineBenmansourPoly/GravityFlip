using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("LEVEL COMPLETE");

            if (AudioManager.instance != null)
                AudioManager.instance.TriggerVictory();

            if (StarManager.instance != null) StarManager.instance.OnLevelComplete();
        }
    }
}