using UnityEngine;
using UnityEngine.UI;

public class StarManager : MonoBehaviour
{
    public static StarManager instance;

    [Header("HUD (The icons in the corner)")]
    public Image[] hudStars;
    public Sprite filledStarSprite;

    [Header("Level Complete UI")]
    public GameObject victoryPanel;
    public GameObject[] uiStars;

    private int starsCollected = 0;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void CollectStar()
    {
        int starIndex = starsCollected;
        starsCollected++;

        if (hudStars != null && starIndex < hudStars.Length && hudStars[starIndex] != null)
        {
            if (filledStarSprite != null)
                hudStars[starIndex].sprite = filledStarSprite;

            hudStars[starIndex].transform.localScale = Vector3.one * 1.3f;
        }

        Debug.Log("Stars found: " + starsCollected);
    }

    public void OnLevelComplete()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (uiStars == null) return;

        for (int i = 0; i < starsCollected && i < uiStars.Length; i++)
        {
            if (uiStars[i] != null)
                uiStars[i].SetActive(true);
        }
    }

    // This allows other scripts to see how many stars we have if needed
    public int GetStarCount() { return starsCollected; }
}
