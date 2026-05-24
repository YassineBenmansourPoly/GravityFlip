using UnityEngine;

public class StarManager : MonoBehaviour
{
    public static StarManager instance;

    public GameObject victoryPanel;
    public GameObject[] uiStars; // The 3 stars on your "Game Over/Win" screen

    private int starsCollected = 0;

    void Awake() { instance = this; }

    public void CollectStar()
    {
        starsCollected++;
        Debug.Log("Stars found: " + starsCollected);
    }

    public void OnLevelComplete()
    {
        victoryPanel.SetActive(true);

        // Show only the stars the player actually physically touched in the level
        for (int i = 0; i < starsCollected; i++)
        {
            if (i < uiStars.Length) uiStars[i].SetActive(true);
        }
    }
}