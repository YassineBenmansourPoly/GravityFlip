using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    private bool gameOverActive;
    private GUIStyle gameOverBoxStyle;
    private GUIStyle gameOverTitleStyle;
    private GUIStyle gameOverButtonStyle;
    private Texture2D dimTexture;
    private Texture2D panelTexture;

    public void GameOver()
    {
        gameOverActive = true;

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
        gameOverActive = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (!gameOverActive || gameOverPanel != null)
            return;

        BuildGameOverStyles();

        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), dimTexture);

        Rect panelRect = new Rect((Screen.width - 420f) * 0.5f, (Screen.height - 250f) * 0.5f, 420f, 250f);
        GUI.Box(panelRect, "", gameOverBoxStyle);
        GUI.Label(new Rect(panelRect.x, panelRect.y + 42f, panelRect.width, 58f), "Game Over", gameOverTitleStyle);

        if (GUI.Button(new Rect(panelRect.x + 110f, panelRect.y + 135f, 200f, 48f), "Retry", gameOverButtonStyle))
            RestartLevel();
    }

    private void BuildGameOverStyles()
    {
        if (gameOverBoxStyle != null)
            return;

        dimTexture = MakeTexture(new Color(0f, 0f, 0f, 0.6f));
        panelTexture = MakeTexture(new Color(0.06f, 0.06f, 0.075f, 0.96f));

        gameOverBoxStyle = new GUIStyle(GUI.skin.box);
        gameOverBoxStyle.normal.background = panelTexture;

        gameOverTitleStyle = new GUIStyle(GUI.skin.label);
        gameOverTitleStyle.fontSize = 42;
        gameOverTitleStyle.fontStyle = FontStyle.Bold;
        gameOverTitleStyle.alignment = TextAnchor.MiddleCenter;
        gameOverTitleStyle.normal.textColor = new Color(1f, 0.2f, 0.25f, 1f);

        gameOverButtonStyle = new GUIStyle(GUI.skin.button);
        gameOverButtonStyle.fontSize = 22;
        gameOverButtonStyle.alignment = TextAnchor.MiddleCenter;
        gameOverButtonStyle.normal.textColor = Color.white;
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
