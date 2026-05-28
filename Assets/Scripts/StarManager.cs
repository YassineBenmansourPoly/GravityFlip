using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StarManager : MonoBehaviour
{
    public static StarManager instance;

    [Header("Exit Requirement")]
    public int requiredStarsToExit = 5;

    [Header("HUD (The icons in the corner)")]
    public Image[] hudStars;
    public Sprite filledStarSprite;

    [Header("Counter UI")]
    public Text starsLeftText;
    public Text warningText;

    [Header("Level Complete UI")]
    public GameObject victoryPanel;
    public GameObject[] uiStars;

    private int starsCollected = 0;
    private int starsNeededForExit = 0;
    private Canvas generatedCanvas;
    private float warningTimer;
    private string warningMessage = "";
    private GUIStyle counterStyle;
    private GUIStyle warningStyle;
    private GUIStyle menuHintStyle;
    private GUIStyle menuBoxStyle;
    private GUIStyle menuTitleStyle;
    private GUIStyle menuTextStyle;
    private GUIStyle menuButtonStyle;
    private GUIStyle menuHeaderStyle;
    private Texture2D dimTexture;
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private bool mainMenuOpen;
    private bool controlsOpen;
    private float previousTimeScale = 1f;
    private static bool subscribedToSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForLoadedScene()
    {
        if (!subscribedToSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            subscribedToSceneLoaded = true;
        }

        EnsureInstance();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
    }

    public static StarManager EnsureInstance()
    {
        if (instance != null)
            return instance;

        StarManager[] existingManagers = FindObjectsByType<StarManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existingManagers.Length > 0)
        {
            instance = existingManagers[0];

            for (int i = 1; i < existingManagers.Length; i++)
                Destroy(existingManagers[i].gameObject);

            return instance;
        }

        GameObject managerObject = new GameObject("StarManager");
        instance = managerObject.AddComponent<StarManager>();
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        RefreshStarRequirement();
        HideLegacyStarHud();
        PositionCanvasHearts();
        BuildCounterUIIfNeeded();
        UpdateCounterUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMainMenu();

        if (warningTimer <= 0f)
            return;

        warningTimer -= Time.deltaTime;

        if (warningTimer <= 0f)
        {
            warningMessage = "";

            if (warningText != null)
                warningText.text = "";
        }
    }

    void OnGUI()
    {
        RefreshStarRequirement();
        DrawMainMenu();

        if (starsNeededForExit == 0 || starsLeftText != null)
            return;

        BuildGUIStyles();

        int starsLeft = Mathf.Max(0, starsNeededForExit - starsCollected);
        string counterMessage = starsLeft == 0
            ? "All stars collected"
            : "Stars left: " + starsLeft;

        GUI.Label(new Rect(20f, 20f, 420f, 40f), counterMessage, counterStyle);

        if (!string.IsNullOrEmpty(warningMessage))
            GUI.Label(new Rect(20f, 58f, 700f, 40f), warningMessage, warningStyle);
    }

    public void CollectStar()
    {
        int starIndex = starsCollected;
        starsCollected++;
        UpdateCounterUI();

        if (hudStars != null && starIndex < hudStars.Length && hudStars[starIndex] != null)
        {
            if (filledStarSprite != null)
                hudStars[starIndex].sprite = filledStarSprite;

            hudStars[starIndex].transform.localScale = Vector3.one * 1.3f;
        }

        Debug.Log("Stars found: " + starsCollected);
    }

    public bool CanExitLevel()
    {
        RefreshStarRequirement();
        return starsNeededForExit == 0 || starsCollected >= starsNeededForExit;
    }

    public void ShowMissingStarsMessage()
    {
        RefreshStarRequirement();
        BuildCounterUIIfNeeded();
        UpdateCounterUI();

        int starsLeft = Mathf.Max(0, starsNeededForExit - starsCollected);
        string starWord = starsLeft == 1 ? "star" : "stars";
        warningMessage = "Missing stars: collect all 5 stars first. " + starsLeft + " " + starWord + " left.";

        if (warningText != null)
            warningText.text = warningMessage;

        warningTimer = 2f;
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

    private void RefreshStarRequirement()
    {
        StarItem[] starsInLevel = FindObjectsByType<StarItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int totalStarsInLevel = starsCollected + starsInLevel.Length;

        // Empty scenes such as cutscenes or boss rooms should not get blocked by star rules.
        starsNeededForExit = totalStarsInLevel == 0
            ? 0
            : Mathf.Min(requiredStarsToExit, totalStarsInLevel);
    }

    private void BuildCounterUIIfNeeded()
    {
        if (starsNeededForExit == 0 || starsLeftText != null)
            return;

        GameObject canvasObject = new GameObject("Star Counter Canvas");
        generatedCanvas = canvasObject.AddComponent<Canvas>();
        generatedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        generatedCanvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        starsLeftText = CreateCounterText("StarsLeftText", new Vector2(20f, -20f), 22);
        warningText = CreateCounterText("StarWarningText", new Vector2(20f, -52f), 18);
        warningText.color = new Color(1f, 0.85f, 0.15f, 1f);
        warningText.text = "";
    }

    private Text CreateCounterText(string objectName, Vector2 anchoredPosition, int fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(generatedCanvas.transform, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.raycastTarget = false;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(320f, 36f);

        return text;
    }

    private void UpdateCounterUI()
    {
        if (starsLeftText == null)
            return;

        int starsLeft = Mathf.Max(0, starsNeededForExit - starsCollected);
        starsLeftText.text = starsLeft == 0
            ? "All stars collected"
            : "Stars left: " + starsLeft;
    }

    private void PositionCanvasHearts()
    {
        PositionHeart("Heart1", 145f);
        PositionHeart("Heart2", 195f);
        PositionHeart("Heart3", 245f);
    }

    private void PositionHeart(string heartName, float x)
    {
        RectTransform[] rectTransforms = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (RectTransform rect in rectTransforms)
        {
            if (rect.name != heartName)
                continue;

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -14f);
            rect.localScale = Vector3.one * 1.5f;
        }
    }

    private void HideLegacyStarHud()
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform foundTransform in transforms)
        {
            if (foundTransform.name == "HUD_Stars")
                foundTransform.gameObject.SetActive(false);
        }
    }

    private void BuildGUIStyles()
    {
        if (counterStyle != null && warningStyle != null && menuHintStyle != null)
            return;

        counterStyle = new GUIStyle(GUI.skin.label);
        counterStyle.fontSize = 22;
        counterStyle.normal.textColor = Color.white;

        warningStyle = new GUIStyle(GUI.skin.label);
        warningStyle.fontSize = 20;
        warningStyle.normal.textColor = new Color(1f, 0.85f, 0.15f, 1f);

        menuHintStyle = new GUIStyle(GUI.skin.label);
        menuHintStyle.fontSize = 17;
        menuHintStyle.alignment = TextAnchor.UpperRight;
        menuHintStyle.normal.textColor = Color.white;

        dimTexture = MakeTexture(new Color(0f, 0f, 0f, 0.45f));
        panelTexture = MakeTexture(new Color(0.05f, 0.055f, 0.065f, 0.96f));
        buttonTexture = MakeTexture(new Color(0.16f, 0.17f, 0.19f, 1f));
        buttonHoverTexture = MakeTexture(new Color(0.26f, 0.28f, 0.32f, 1f));

        menuBoxStyle = new GUIStyle(GUI.skin.box);
        menuBoxStyle.normal.background = panelTexture;

        menuTitleStyle = new GUIStyle(GUI.skin.label);
        menuTitleStyle.fontSize = 34;
        menuTitleStyle.alignment = TextAnchor.MiddleCenter;
        menuTitleStyle.normal.textColor = Color.white;

        menuHeaderStyle = new GUIStyle(GUI.skin.label);
        menuHeaderStyle.fontSize = 16;
        menuHeaderStyle.alignment = TextAnchor.MiddleCenter;
        menuHeaderStyle.normal.textColor = new Color(0.78f, 0.85f, 1f, 1f);

        menuTextStyle = new GUIStyle(GUI.skin.label);
        menuTextStyle.fontSize = 18;
        menuTextStyle.alignment = TextAnchor.MiddleLeft;
        menuTextStyle.normal.textColor = Color.white;

        menuButtonStyle = new GUIStyle(GUI.skin.button);
        menuButtonStyle.fontSize = 20;
        menuButtonStyle.alignment = TextAnchor.MiddleCenter;
        menuButtonStyle.normal.textColor = Color.white;
        menuButtonStyle.hover.textColor = Color.white;
        menuButtonStyle.active.textColor = Color.white;
        menuButtonStyle.normal.background = buttonTexture;
        menuButtonStyle.hover.background = buttonHoverTexture;
        menuButtonStyle.active.background = buttonHoverTexture;
    }

    private void DrawMainMenu()
    {
        BuildGUIStyles();

        GUI.Label(new Rect(Screen.width - 330f, 20f, 310f, 32f), "Press M to open Main Menu", menuHintStyle);

        if (!mainMenuOpen)
            return;

        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), dimTexture);

        Rect menuRect = new Rect((Screen.width - 500f) * 0.5f, (Screen.height - 500f) * 0.5f, 500f, 500f);
        GUI.Box(menuRect, "", menuBoxStyle);
        GUI.Label(new Rect(menuRect.x, menuRect.y + 30f, menuRect.width, 44f), "Main Menu", menuTitleStyle);
        GUI.Label(new Rect(menuRect.x, menuRect.y + 72f, menuRect.width, 28f), "Gravity Flip paused", menuHeaderStyle);

        if (GUI.Button(new Rect(menuRect.x + 130f, menuRect.y + 122f, 240f, 48f), "Retry", menuButtonStyle))
            RetryLevel();

        if (GUI.Button(new Rect(menuRect.x + 130f, menuRect.y + 188f, 240f, 48f), "Controls", menuButtonStyle))
            controlsOpen = !controlsOpen;

        if (GUI.Button(new Rect(menuRect.x + 130f, menuRect.y + 254f, 240f, 48f), "Close", menuButtonStyle))
            CloseMainMenu();

        if (!controlsOpen)
            return;

        string controlsText = "Move: A / D or Left / Right\nJump: Space\nFlip Gravity: G\nDash: Left Shift\nShoot: Left Mouse Button\nMenu: M";
        GUI.Label(new Rect(menuRect.x + 58f, menuRect.y + 320f, 400f, 150f), controlsText, menuTextStyle);
    }

    private void ToggleMainMenu()
    {
        if (mainMenuOpen)
            CloseMainMenu();
        else
            OpenMainMenu();
    }

    private void OpenMainMenu()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        mainMenuOpen = true;
    }

    private void CloseMainMenu()
    {
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        mainMenuOpen = false;
        controlsOpen = false;
    }

    private void RetryLevel()
    {
        Time.timeScale = 1f;
        LevelIntroInstaller.SkipNextIntro();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
