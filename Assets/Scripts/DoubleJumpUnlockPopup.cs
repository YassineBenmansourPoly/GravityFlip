using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoubleJumpUnlockPopup : MonoBehaviour
{
    private float timer;
    private string popupTitle;
    private string popupBody;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle boxStyle;
    private Texture2D boxTexture;
    private bool isShowing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForLevel4()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreate(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreate(scene);
    }

    private static void TryCreate(Scene scene)
    {
        string title;
        string body;

        if (!TryGetPopupText(scene.name, out title, out body))
            return;

        if (FindFirstObjectByType<DoubleJumpUnlockPopup>() != null)
            return;

        GameObject popupObject = new GameObject("Ability Unlock Popup");
        DoubleJumpUnlockPopup popup = popupObject.AddComponent<DoubleJumpUnlockPopup>();
        popup.Initialize(title, body);
    }

    private static bool TryGetPopupText(string sceneName, out string title, out string body)
    {
        switch (sceneName)
        {
            case "Level1":
                title = "Gravity Flip Unlocked";
                body = "Press G to flip gravity and walk on ceilings.";
                return true;

            case "Level4":
                title = "Double Jump Unlocked";
                body = "Press Space again while in the air to jump twice.";
                return true;

            case "Level6":
                title = "Dash Unlocked";
                body = "Press Left Shift while on the ground to dash forward.";
                return true;

            case "BossLevel1":
                title = "Fireball Unlocked";
                body = "Press Right Click to shoot fireballs and break brown tiles.";
                return true;

            case "Level9":
                title = "Fireball Unlocked";
                body = "Press Left Click or Right Click to shoot fireballs.";
                return true;

            default:
                title = "";
                body = "";
                return false;
        }
    }

    private void Initialize(string title, string body)
    {
        popupTitle = title;
        popupBody = body;
    }

    private void Awake()
    {
        timer = 5f;
        StartCoroutine(WaitForIntroThenShow());
    }

    private void Update()
    {
        if (!isShowing)
            return;

        timer -= Time.unscaledDeltaTime;

        if (timer <= 0f)
            Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (!isShowing)
            return;

        BuildStyles();

        float width = 520f;
        float height = 130f;
        Rect boxRect = new Rect((Screen.width - width) * 0.5f, 90f, width, height);

        GUI.Box(boxRect, "", boxStyle);
        GUI.Label(new Rect(boxRect.x, boxRect.y + 18f, boxRect.width, 36f), popupTitle, titleStyle);
        GUI.Label(new Rect(boxRect.x + 30f, boxRect.y + 64f, boxRect.width - 60f, 44f), popupBody, bodyStyle);
    }

    private IEnumerator WaitForIntroThenShow()
    {
        // The popup can be created before the intro object finishes initializing.
        yield return null;

        // Wait until the level intro/cutscene overlay finishes and removes itself.
        while (LevelIntroUI.IsIntroPlaying || FindFirstObjectByType<LevelIntroUI>() != null)
            yield return null;

        yield return new WaitForSecondsRealtime(0.35f);

        timer = 5f;
        isShowing = true;
    }

    private void BuildStyles()
    {
        if (titleStyle != null)
            return;

        boxTexture = MakeTexture(new Color(0.05f, 0.055f, 0.07f, 0.88f));

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = boxTexture;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 30;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.15f, 1f);

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 20;
        bodyStyle.alignment = TextAnchor.MiddleCenter;
        bodyStyle.normal.textColor = Color.white;
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
