using UnityEngine;
using UnityEngine.UI;

public class StarManager : MonoBehaviour
{
    public static StarManager instance;

    [Header("HUD (The icons in the corner)")]
    public Image[] hudStars; 
    public Sprite filledStarSprite; 

    private int starsCollected = 0;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void CollectStar()
    {
        // 1. Fill the HUD star outline
        if (starsCollected < hudStars.Length)
        {
            hudStars[starsCollected].sprite = filledStarSprite;

            // Tiny "pop" effect so the player notices it filled up
            hudStars[starsCollected].transform.localScale = Vector3.one * 1.3f;
            starsCollected++;
        }

        Debug.Log("Stars found: " + starsCollected);
    }

    // This allows other scripts to see how many stars we have if needed
    public int GetStarCount() { return starsCollected; }
}