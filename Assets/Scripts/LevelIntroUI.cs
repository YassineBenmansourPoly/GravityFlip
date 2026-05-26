using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays a black-screen intro with story text at the beginning of a level.
/// 
/// Flow:
///   1. Black screen covers the level immediately (Awake)
///   2. Story text fades in
///   3. Text stays visible for 'displayDuration' seconds
///   4. Text fades out
///   5. Black background fades out to reveal the level
///   6. Player movement is re-enabled
///   7. This GameObject self-destructs
///
/// All values are adjustable in the Inspector.
/// </summary>
public class LevelIntroUI : MonoBehaviour
{
    [Header("Story Text")]
    [TextArea(5, 15)]
    [Tooltip("The narrative text displayed on the black screen at the start of the level.")]
    public string introText = "";

    [Header("Timing (Seconds)")]
    [Tooltip("Delay on the black screen before the text starts fading in.")]
    public float delayBeforeFadeIn = 0.5f;

    [Tooltip("How long it takes for the text to fade in.")]
    public float textFadeInDuration = 1.5f;

    [Tooltip("How long the text stays fully visible on screen. Adjust per level based on text length.")]
    public float displayDuration = 5.0f;

    [Tooltip("How long it takes for the text to fade out.")]
    public float textFadeOutDuration = 1.0f;

    [Tooltip("Brief pause after text fades out, before the background starts fading.")]
    public float delayBeforeBackgroundFade = 0.3f;

    [Tooltip("How long it takes for the black background to fade out and reveal the level.")]
    public float backgroundFadeOutDuration = 1.0f;

    [Header("Text Appearance")]
    [Tooltip("Font size for the intro text.")]
    public float fontSize = 36f;

    [Tooltip("Color of the intro text.")]
    public Color textColor = Color.white;

    [Tooltip("Optional: Assign a custom TextMeshPro font asset. Leave empty for the default TMP font.")]
    public TMP_FontAsset fontAsset;

    [Header("Background")]
    [Tooltip("Optional background image. If empty, the screen is black.")]
    public Sprite backgroundImage;
    [Tooltip("Tint applied to the background image. Gray/Dark Gray helps text stand out.")]
    public Color backgroundTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    // ---- Runtime UI references (built programmatically) ----
    private GameObject backgroundObj;
    private CanvasGroup backgroundGroup;
    private Image bgImageComponent;
    private CanvasGroup textGroup;
    private TextMeshProUGUI textComponent;

    // ---- Player freeze state ----
    private Rigidbody2D playerRigidbody;
    private readonly List<MonoBehaviour> frozenScripts = new List<MonoBehaviour>();
    private bool isInitialized;

    // ==========================================================
    //  LIFECYCLE
    // ==========================================================

    private void Awake()
    {
        // Create the black screen IMMEDIATELY so the player never sees
        // a flash of the level before the intro plays.
        CreateBlackScreen();
    }

    private void Start()
    {
        // If BeginIntro() was already called by the installer, skip.
        // Otherwise, this component was placed manually in the scene —
        // use the serialized Inspector values.
        if (!isInitialized)
        {
            BeginIntro();
        }
    }

    /// <summary>
    /// Called by LevelIntroInstaller after setting all public fields.
    /// Builds the text UI, freezes the player, and starts the intro sequence.
    /// </summary>
    public void BeginIntro()
    {
        isInitialized = true;

        // If there is no text, skip the intro entirely
        if (string.IsNullOrWhiteSpace(introText))
        {
            Destroy(gameObject);
            return;
        }

        // Apply the background image now that all public fields are set
        if (backgroundImage != null && bgImageComponent != null)
        {
            bgImageComponent.sprite = backgroundImage;
            bgImageComponent.color = backgroundTint;
        }

        CreateTextElement();
        FreezePlayer();
        StartCoroutine(RunIntroSequence());
    }

    // ==========================================================
    //  UI CONSTRUCTION
    // ==========================================================

    /// <summary>
    /// Creates the Canvas and a full-screen black Image.
    /// Called in Awake() so the screen is covered from the very first frame.
    /// </summary>
    private void CreateBlackScreen()
    {
        // --- Canvas (renders on top of everything) ---
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // --- Full-screen black panel ---
        backgroundObj = new GameObject("IntroBackground");
        backgroundObj.transform.SetParent(transform, false);

        bgImageComponent = backgroundObj.AddComponent<Image>();
        bgImageComponent.color = Color.black; // Default to black until BeginIntro updates it
        bgImageComponent.raycastTarget = true;

        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        StretchToFill(bgRect);

        backgroundGroup = backgroundObj.AddComponent<CanvasGroup>();
        backgroundGroup.alpha = 1f;
        backgroundGroup.blocksRaycasts = true;
        backgroundGroup.interactable = false;
    }

    /// <summary>
    /// Creates the TextMeshPro element inside the existing black background.
    /// Called in BeginIntro() AFTER public fields have been set by the installer.
    /// </summary>
    private void CreateTextElement()
    {
        GameObject textObj = new GameObject("IntroText");
        textObj.transform.SetParent(backgroundObj.transform, false);

        textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = introText;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Overflow;

        if (fontAsset != null)
            textComponent.font = fontAsset;

        // Center the text with comfortable margins
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.2f);
        textRect.anchorMax = new Vector2(0.9f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        textGroup = textObj.AddComponent<CanvasGroup>();
        textGroup.alpha = 0f; // start invisible
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ==========================================================
    //  PLAYER FREEZE / UNFREEZE
    // ==========================================================

    private void FreezePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Stop all physics
        playerRigidbody = playerObj.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.simulated = false;
        }

        // Disable every gameplay script on the player
        // (PlayerMovement, PlayerFireballShooter, DoubleJump, PlayerHealth, etc.)
        foreach (MonoBehaviour script in playerObj.GetComponents<MonoBehaviour>())
        {
            if (script != null && script.enabled)
            {
                script.enabled = false;
                frozenScripts.Add(script);
            }
        }
    }

    private void UnfreezePlayer()
    {
        // Re-enable physics
        if (playerRigidbody != null)
            playerRigidbody.simulated = true;

        // Re-enable all scripts we disabled
        foreach (MonoBehaviour script in frozenScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        frozenScripts.Clear();
    }

    // ==========================================================
    //  INTRO SEQUENCE
    // ==========================================================

    private IEnumerator RunIntroSequence()
    {
        // Step 1 — Pause on black screen
        yield return new WaitForSecondsRealtime(delayBeforeFadeIn);

        // Step 2 — Fade in the story text
        yield return FadeGroup(textGroup, 0f, 1f, textFadeInDuration);

        // Step 3 — Hold the text on screen
        yield return new WaitForSecondsRealtime(displayDuration);

        // Step 4 — Fade out the story text
        yield return FadeGroup(textGroup, 1f, 0f, textFadeOutDuration);

        // Step 5 — Brief pause, then fade out the black background to reveal the level
        yield return new WaitForSecondsRealtime(delayBeforeBackgroundFade);
        yield return FadeGroup(backgroundGroup, 1f, 0f, backgroundFadeOutDuration);

        // Step 6 — Unfreeze the player and clean up
        UnfreezePlayer();
        Destroy(gameObject);
    }

    /// <summary>
    /// Smoothly fades a CanvasGroup's alpha using a SmoothStep curve.
    /// Uses unscaledDeltaTime so it works even if Time.timeScale is 0.
    /// </summary>
    private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
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

            // SmoothStep for a polished ease-in / ease-out feel
            float smooth = t * t * (3f - 2f * t);
            group.alpha = Mathf.Lerp(from, to, smooth);

            yield return null;
        }

        group.alpha = to;
    }
}
