using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    private GameObject menuPanel;
    private GameObject controlsPanel;
    private float previousTimeScale = 1f;
    private bool isOpen;
    private bool showControls;
    private GUIStyle hintStyle;
    private GUIStyle menuStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private static bool subscribedToSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWhenSceneLoads()
    {
        if (!subscribedToSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            subscribedToSceneLoaded = true;
        }

        CreateForCurrentScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateForCurrentScene();
    }

    private static void CreateForCurrentScene()
    {
        // StarManager owns the shared HUD/menu now. This older menu builder is kept
        // disabled so builds do not show duplicate "Press M" prompts.
    }

    private static bool ShouldShowMenuInScene(string sceneName)
    {
        return sceneName.StartsWith("Level") || sceneName == "BossLevel1" || sceneName == "ActualLevel2";
    }

    private void Awake()
    {
        BuildMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMenu();
    }

    private void OnGUI()
    {
        BuildGUIStyles();

        GUI.Label(new Rect(Screen.width - 330f, 20f, 310f, 32f), "Press M to open Main Menu", hintStyle);

        if (!isOpen)
            return;

        Rect menuRect = new Rect((Screen.width - 420f) * 0.5f, (Screen.height - 360f) * 0.5f, 420f, 360f);
        GUI.Box(menuRect, "", menuStyle);
        GUI.Label(new Rect(menuRect.x, menuRect.y + 24f, menuRect.width, 44f), "Main Menu", titleStyle);

        if (GUI.Button(new Rect(menuRect.x + 100f, menuRect.y + 94f, 220f, 42f), "Retry"))
            RetryLevel();

        if (GUI.Button(new Rect(menuRect.x + 100f, menuRect.y + 148f, 220f, 42f), "Controls"))
            ToggleControls();

        if (GUI.Button(new Rect(menuRect.x + 100f, menuRect.y + 202f, 220f, 42f), "Close"))
            CloseMenu();

        if (!showControls)
            return;

        string controlsText = "Move: A / D or Left / Right\nJump: Space\nFlip Gravity: G\nDash: Left Shift\nShoot: Left Mouse Button\nMenu: M";
        GUI.Label(new Rect(menuRect.x + 44f, menuRect.y + 258f, 340f, 92f), controlsText, textStyle);
    }

    private void BuildMenu()
    {
        EnsureEventSystem();

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        CreateHintText(transform);
        menuPanel = CreateMenuPanel(transform);
        controlsPanel = CreateControlsPanel(menuPanel.transform);

        menuPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void CreateHintText(Transform parent)
    {
        Text hintText = CreateText(parent, "MainMenuHint", "Press M to open Main Menu", 18, Color.white);
        RectTransform rect = hintText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(320f, 32f);
        hintText.alignment = TextAnchor.UpperRight;
    }

    private GameObject CreateMenuPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "MainMenuPanel", new Color(0f, 0f, 0f, 0.75f));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 360f);

        Text title = CreateText(panel.transform, "Title", "Main Menu", 32, Color.white);
        SetRect(title.GetComponent<RectTransform>(), new Vector2(0f, 112f), new Vector2(360f, 48f));
        title.alignment = TextAnchor.MiddleCenter;

        Button retryButton = CreateButton(panel.transform, "RetryButton", "Retry", new Vector2(0f, 46f));
        retryButton.onClick.AddListener(RetryLevel);

        Button controlsButton = CreateButton(panel.transform, "ControlsButton", "Controls", new Vector2(0f, -18f));
        controlsButton.onClick.AddListener(ToggleControls);

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(0f, -82f));
        closeButton.onClick.AddListener(CloseMenu);

        return panel;
    }

    private GameObject CreateControlsPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "ControlsPanel", new Color(0.08f, 0.08f, 0.08f, 0.92f));
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0f, -112f), new Vector2(360f, 122f));

        Text controlsText = CreateText(
            panel.transform,
            "ControlsText",
            "Move: A / D or Left / Right\nJump: Space\nFlip Gravity: G\nDash: Left Shift\nShoot: Left Mouse Button\nMenu: M",
            18,
            Color.white
        );

        SetRect(controlsText.GetComponent<RectTransform>(), Vector2.zero, new Vector2(330f, 104f));
        controlsText.alignment = TextAnchor.MiddleLeft;

        return panel;
    }

    private GameObject CreatePanel(Transform parent, string objectName, Color color)
    {
        GameObject panel = new GameObject(objectName);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.2f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.28f, 0.32f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.12f, 1f);
        button.colors = colors;

        SetRect(buttonObject.GetComponent<RectTransform>(), anchoredPosition, new Vector2(220f, 46f));

        Text buttonText = CreateText(buttonObject.transform, "Text", label, 22, Color.white);
        SetRect(buttonText.GetComponent<RectTransform>(), Vector2.zero, new Vector2(220f, 46f));
        buttonText.alignment = TextAnchor.MiddleCenter;

        return button;
    }

    private Text CreateText(Transform parent, string objectName, string content, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.raycastTarget = false;

        return text;
    }

    private void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isOpen = true;
        menuPanel.SetActive(true);
    }

    private void CloseMenu()
    {
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        isOpen = false;
        showControls = false;
        menuPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    private void RetryLevel()
    {
        Time.timeScale = 1f;
        LevelIntroInstaller.SkipNextIntro();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ToggleControls()
    {
        showControls = !showControls;
        controlsPanel.SetActive(showControls);
    }

    private void BuildGUIStyles()
    {
        if (hintStyle != null)
            return;

        hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 18;
        hintStyle.alignment = TextAnchor.UpperRight;
        hintStyle.normal.textColor = Color.white;

        menuStyle = new GUIStyle(GUI.skin.box);
        menuStyle.normal.background = Texture2D.blackTexture;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 32;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 18;
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.normal.textColor = Color.white;
    }
}
