using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Self-contained cutscene slideshow player.
///
/// Builds its own Canvas and all UI elements at runtime — no manual scene
/// setup required. Fades panels smoothly, handles player input, plays
/// background music, and loads the next scene when finished.
///
/// Can be placed manually in a scene, or auto-created by CutsceneAutoLoader.
/// If no CutsceneData is assigned, it tries to load one from Resources
/// based on the current scene name (e.g. "OpeningCutsceneData").
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Data")]
    [Tooltip("Assign a CutsceneData asset. Leave empty to auto-load from Resources.")]
    public CutsceneData cutsceneData;

    // ---- Fallback config (used when no CutsceneData asset is found) ----
    [HideInInspector] public string fallbackNextScene = "Level1";

    // ---- Override slides set by CutsceneAutoLoader ----
    private CutsceneData.Slide[] overrideSlides;
    private string overrideContinuePrompt;
    private string overrideFinalPrompt;

    // ---- Runtime UI references ----
    private Image slideImage;
    private CanvasGroup slideGroup;
    private CanvasGroup textPanelGroup;
    private TextMeshProUGUI narrativeText;
    private TextMeshProUGUI promptText;
    private CanvasGroup promptGroup;

    // ---- State ----
    private bool isRunning;
    private bool waitingForInput;
    private bool inputReceived;
    private Coroutine promptPulseCoroutine;
    private AudioSource musicSource;

    // ---- Placeholder colors for slides without images ----
    private static readonly Color[] PlaceholderColors =
    {
        new Color(0.06f, 0.10f, 0.22f), // deep blue
        new Color(0.22f, 0.10f, 0.04f), // dark orange
        new Color(0.22f, 0.18f, 0.04f), // dark amber
        new Color(0.18f, 0.04f, 0.04f), // dark red
        new Color(0.04f, 0.16f, 0.18f), // dark teal
        new Color(0.12f, 0.04f, 0.20f), // dark purple
    };

    // =================================================================
    //  PUBLIC API
    // =================================================================

    /// <summary>
    /// Sets default slides for when no CutsceneData asset exists.
    /// Called by CutsceneAutoLoader.
    /// </summary>
    public void SetOverrideSlides(CutsceneData.Slide[] slides,
                                   string continuePrompt = null,
                                   string finalPrompt = null)
    {
        overrideSlides = slides;
        overrideContinuePrompt = continuePrompt;
        overrideFinalPrompt = finalPrompt;
    }

    /// <summary>
    /// Starts the cutscene sequence. Called automatically in Start(),
    /// or earlier by CutsceneAutoLoader.
    /// </summary>
    public void BeginCutscene()
    {
        isRunning = true;

        // Auto-load from Resources if nothing was assigned
        if (cutsceneData == null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            cutsceneData = Resources.Load<CutsceneData>(sceneName + "Data");
        }

        StartCoroutine(RunCutscene());
    }

    // =================================================================
    //  LIFECYCLE
    // =================================================================

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        if (!isRunning)
            BeginCutscene();
    }

    private void Update()
    {
        if (waitingForInput && Input.anyKeyDown)
            inputReceived = true;
    }

    // =================================================================
    //  UI CONSTRUCTION  (fully programmatic — zero manual setup)
    // =================================================================

    private void BuildUI()
    {
        // ---- Canvas (renders on top of everything) ----
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // ---- Full-screen black background (always present) ----
        GameObject bgObj = MakeChild("CutsceneBG");
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        StretchFill(bgObj);

        // ---- Slide image (fills entire screen) ----
        GameObject slideObj = MakeChild("SlideImage");
        slideImage = slideObj.AddComponent<Image>();
        slideImage.color = Color.white;
        slideImage.preserveAspect = false; // stretch to fill for cinematic look
        StretchFill(slideObj);

        slideGroup = slideObj.AddComponent<CanvasGroup>();
        slideGroup.alpha = 0f;

        // ---- Gradient overlay at the bottom for text readability ----
        // Transparent at top → semi-opaque black at bottom
        GameObject panelObj = MakeChild("TextOverlay");
        Image panelImage = panelObj.AddComponent<Image>();

        Texture2D gradientTex = new Texture2D(1, 8, TextureFormat.ARGB32, false);
        gradientTex.filterMode = FilterMode.Bilinear;
        gradientTex.wrapMode = TextureWrapMode.Clamp;
        float[] alphas = { 0.92f, 0.88f, 0.78f, 0.62f, 0.42f, 0.22f, 0.08f, 0f };
        for (int y = 0; y < 8; y++)
            gradientTex.SetPixel(0, y, new Color(0, 0, 0, alphas[y]));
        gradientTex.Apply();

        panelImage.sprite = Sprite.Create(
            gradientTex,
            new Rect(0, 0, 1, 8),
            new Vector2(0.5f, 0.5f),
            1f
        );
        panelImage.type = Image.Type.Simple;

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0.42f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        textPanelGroup = panelObj.AddComponent<CanvasGroup>();
        textPanelGroup.alpha = 0f;

        // ---- Narrative text (centered in the overlay panel) ----
        GameObject textObj = MakeChildOf("NarrativeText", panelObj);
        narrativeText = textObj.AddComponent<TextMeshProUGUI>();
        narrativeText.fontSize = 34f;
        narrativeText.color = Color.white;
        narrativeText.alignment = TextAlignmentOptions.Center;
        narrativeText.enableWordWrapping = true;
        narrativeText.overflowMode = TextOverflowModes.Overflow;
        narrativeText.richText = true;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.06f, 0.18f);
        textRect.anchorMax = new Vector2(0.94f, 0.90f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // ---- Continue prompt (small text at bottom of panel) ----
        GameObject promptObj = MakeChildOf("PromptText", panelObj);
        promptText = promptObj.AddComponent<TextMeshProUGUI>();
        promptText.fontSize = 20f;
        promptText.color = new Color(0.85f, 0.85f, 0.85f, 0.7f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontStyle = FontStyles.Italic;
        promptText.text = "";

        RectTransform promptRect = promptObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.15f, 0.02f);
        promptRect.anchorMax = new Vector2(0.85f, 0.14f);
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;

        promptGroup = promptObj.AddComponent<CanvasGroup>();
        promptGroup.alpha = 0f;
    }

    private GameObject MakeChild(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform, false);
        return obj;
    }

    private GameObject MakeChildOf(string name, GameObject parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        return obj;
    }

    private static void StretchFill(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // =================================================================
    //  CUTSCENE SEQUENCE
    // =================================================================

    private IEnumerator RunCutscene()
    {
        CutsceneData.Slide[] slides = GetSlideData();
        string nextScene = GetNextScene();

        if (slides == null || slides.Length == 0)
        {
            TransitionToNextScene(nextScene);
            yield break;
        }

        ApplyTextSettings();
        StartBackgroundMusic();

        // Brief pause on black to let the scene settle
        yield return new WaitForSecondsRealtime(0.8f);

        for (int i = 0; i < slides.Length; i++)
        {
            CutsceneData.Slide slide = slides[i];
            bool isLast = (i == slides.Length - 1);

            // ---- Set content ----
            slideImage.sprite = slide.image != null ? slide.image : CreatePlaceholder(i);
            slideImage.color = Color.white;
            narrativeText.text = slide.narrativeText ?? "";
            promptText.text = isLast ? GetFinalPrompt() : GetContinuePrompt();

            // ---- Fade in the slide image ----
            yield return Fade(slideGroup, 0f, 1f, GetFadeDuration());

            // ---- Short pause, then fade in the text overlay ----
            yield return new WaitForSecondsRealtime(GetTextDelay());
            yield return Fade(textPanelGroup, 0f, 1f, GetFadeDuration() * 0.6f);

            // ---- Wait minimum display time (prevents accidental skips) ----
            float minTime = slide.minimumDisplayTime > 0f ? slide.minimumDisplayTime : 2f;
            yield return new WaitForSecondsRealtime(minTime);

            // ---- Show prompt and wait for player input ----
            promptPulseCoroutine = StartCoroutine(PulsePrompt());
            yield return WaitForPlayerInput();

            // ---- Cleanup prompt ----
            if (promptPulseCoroutine != null)
            {
                StopCoroutine(promptPulseCoroutine);
                promptPulseCoroutine = null;
            }
            promptGroup.alpha = 0f;

            // ---- Fade out text, then image ----
            yield return Fade(textPanelGroup, 1f, 0f, GetFadeDuration() * 0.4f);
            yield return Fade(slideGroup, 1f, 0f, GetFadeDuration() * 0.8f);

            // ---- Brief black between slides ----
            if (!isLast)
                yield return new WaitForSecondsRealtime(0.3f);
        }

        // Fade out music gracefully
        if (musicSource != null && musicSource.isPlaying)
            yield return FadeAudio(musicSource, musicSource.volume, 0f, 1.5f);

        // Final pause before scene transition
        yield return new WaitForSecondsRealtime(0.5f);

        TransitionToNextScene(nextScene);
    }

    /// <summary>
    /// Handles transitioning to the next scene, with special support for exiting the game
    /// if the target scene name is set to "QUIT" or "EXIT".
    /// </summary>
    private void TransitionToNextScene(string nextScene)
    {
        if (string.Equals(nextScene, "QUIT", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(nextScene, "EXIT", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[CutsceneManager] Exiting the game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    // =================================================================
    //  INPUT HANDLING
    // =================================================================

    private IEnumerator WaitForPlayerInput()
    {
        inputReceived = false;
        waitingForInput = true;

        // Skip one frame to avoid picking up the key press that just happened
        yield return null;

        while (!inputReceived)
            yield return null;

        waitingForInput = false;
        inputReceived = false;
    }

    // =================================================================
    //  ANIMATIONS
    // =================================================================

    /// <summary>
    /// Fades the prompt text in, then pulses it gently until stopped.
    /// </summary>
    private IEnumerator PulsePrompt()
    {
        // Fade in the prompt
        promptGroup.alpha = 0f;
        float fadeIn = 0f;
        while (fadeIn < 0.5f)
        {
            fadeIn += Time.unscaledDeltaTime;
            promptGroup.alpha = Mathf.Lerp(0f, 0.7f, fadeIn / 0.5f);
            yield return null;
        }

        // Pulse continuously
        while (true)
        {
            promptGroup.alpha = 0.4f + 0.3f * Mathf.Sin(Time.unscaledTime * 2.5f);
            yield return null;
        }
    }

    /// <summary>
    /// Smoothly fades a CanvasGroup's alpha using a SmoothStep curve.
    /// Uses unscaledDeltaTime so it works even if Time.timeScale is 0.
    /// </summary>
    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = t * t * (3f - 2f * t); // SmoothStep
            group.alpha = Mathf.Lerp(from, to, smooth);
            yield return null;
        }

        group.alpha = to;
    }

    /// <summary>
    /// Smoothly fades an AudioSource's volume.
    /// </summary>
    private static IEnumerator FadeAudio(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        source.volume = to;
    }

    // =================================================================
    //  CONFIGURATION HELPERS
    // =================================================================

    private CutsceneData.Slide[] GetSlideData()
    {
        if (cutsceneData != null && cutsceneData.slides != null && cutsceneData.slides.Length > 0)
            return cutsceneData.slides;

        return overrideSlides;
    }

    private string GetNextScene()
    {
        if (cutsceneData != null && !string.IsNullOrEmpty(cutsceneData.nextSceneName))
            return cutsceneData.nextSceneName;

        return fallbackNextScene;
    }

    private float GetFadeDuration()
    {
        return cutsceneData != null ? cutsceneData.fadeDuration : 1.2f;
    }

    private float GetTextDelay()
    {
        return cutsceneData != null ? cutsceneData.textAppearDelay : 0.4f;
    }

    private string GetContinuePrompt()
    {
        if (cutsceneData != null && !string.IsNullOrEmpty(cutsceneData.continuePrompt))
            return cutsceneData.continuePrompt;

        return overrideContinuePrompt ?? "\u25B6  Click or press any key to continue";
    }

    private string GetFinalPrompt()
    {
        if (cutsceneData != null && !string.IsNullOrEmpty(cutsceneData.finalSlidePrompt))
            return cutsceneData.finalSlidePrompt;

        return overrideFinalPrompt ?? "\u25B6  Press any key to begin";
    }

    /// <summary>
    /// Applies font, size, and color from CutsceneData or LevelIntroSettings.
    /// Falls back to the same pixel font used by the level intro system for
    /// visual consistency.
    /// </summary>
    private void ApplyTextSettings()
    {
        float size = cutsceneData != null ? cutsceneData.fontSize : 34f;
        Color color = cutsceneData != null ? cutsceneData.textColor : Color.white;
        TMP_FontAsset font = cutsceneData != null ? cutsceneData.fontAsset : null;

        // Auto-match the pixel font from the level intro system
        if (font == null)
        {
            LevelIntroSettings introSettings = Resources.Load<LevelIntroSettings>("LevelIntroSettings");
            if (introSettings != null && introSettings.fontAsset != null)
                font = introSettings.fontAsset;
        }

        narrativeText.fontSize = size;
        narrativeText.color = color;

        if (font != null)
        {
            narrativeText.font = font;
            promptText.font = font;
        }
    }

    /// <summary>
    /// Starts background music if available.
    /// Ensures there is an AudioListener in the scene so audio actually plays.
    /// </summary>
    private void StartBackgroundMusic()
    {
        AudioClip clip = cutsceneData != null ? cutsceneData.backgroundMusic : null;
        float vol = cutsceneData != null ? cutsceneData.musicVolume : 0.35f;

        if (clip == null) return;

        // Ensure there is an AudioListener in the scene so audio can be heard
        if (Object.FindAnyObjectByType<AudioListener>() == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.gameObject.AddComponent<AudioListener>();
                Debug.Log("[CutsceneManager] Added missing AudioListener to Main Camera.");
            }
            else
            {
                gameObject.AddComponent<AudioListener>();
                Debug.Log("[CutsceneManager] No Main Camera found. Added AudioListener to CutsceneManager.");
            }
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = clip;
        musicSource.volume = vol;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.Play();
    }

    /// <summary>
    /// Creates a solid-color placeholder sprite for slides without images.
    /// Each panel gets a different subtle dark color so you can tell them apart.
    /// </summary>
    private Sprite CreatePlaceholder(int index)
    {
        Color c = PlaceholderColors[index % PlaceholderColors.Length];
        Texture2D tex = new Texture2D(4, 4);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = c;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
