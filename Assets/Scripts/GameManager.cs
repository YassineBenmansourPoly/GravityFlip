using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    private bool gameOverActive;
    private GUIStyle gameOverBoxStyle;
    private GUIStyle gameOverTitleStyle;
    private GUIStyle gameOverButtonStyle;
    private Texture2D dimTexture;
    private Texture2D panelTexture;
    private GameObject playerWaitingForRespawn;

    private void Update()
    {
        if (gameOverActive && Input.GetKeyDown(KeyCode.R))
            RestartLevel();
    }

    public void GameOver()
    {
        GameOver(null);
    }

    public void GameOver(GameObject defeatedPlayer)
    {
        gameOverActive = true;
        playerWaitingForRespawn = defeatedPlayer;

        // 1. Tell Audio to stop music and play death sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.TriggerGameOver(); // Name must match AudioManager exactly!
        }

        // 2. Show the UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            HideLegacyRestartButtons();
        }

        // 3. Stop Time 
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        gameOverActive = false;
        playerWaitingForRespawn = null;
        Time.timeScale = 1f;
        LevelIntroInstaller.SkipNextIntro();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RespawnAtCheckpoint()
    {
        if (playerWaitingForRespawn == null || !CheckpointManager.TryRespawnPlayer(playerWaitingForRespawn))
            return;

        gameOverActive = false;
        playerWaitingForRespawn = null;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnGUI()
    {
        if (!gameOverActive)
            return;

        BuildGameOverStyles();

        if (gameOverPanel == null)
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), dimTexture);

        bool canUseCheckpoint = playerWaitingForRespawn != null && CheckpointManager.HasTouchedCheckpointAvailable();
        Rect panelRect = new Rect((Screen.width - 460f) * 0.5f, (Screen.height - 290f) * 0.5f, 460f, 290f);

        if (gameOverPanel == null)
        {
            GUI.Box(panelRect, "", gameOverBoxStyle);
            GUI.Label(new Rect(panelRect.x, panelRect.y + 42f, panelRect.width, 58f), "Game Over", gameOverTitleStyle);
        }

        float restartY = canUseCheckpoint ? panelRect.y + 140f : panelRect.y + 155f;

        if (GUI.Button(new Rect(panelRect.x + 110f, restartY, 240f, 48f), "Restart Full Level", gameOverButtonStyle))
            RestartLevel();

        if (canUseCheckpoint && GUI.Button(new Rect(panelRect.x + 110f, panelRect.y + 200f, 240f, 48f), "Back to Checkpoint", gameOverButtonStyle))
            RespawnAtCheckpoint();
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

    private void HideLegacyRestartButtons()
    {
        Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLowerInvariant();

            if (buttonName.Contains("restart"))
                button.gameObject.SetActive(false);
        }
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
