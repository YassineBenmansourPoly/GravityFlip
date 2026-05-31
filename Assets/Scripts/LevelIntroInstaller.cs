using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically creates a LevelIntroUI at the start of each level scene.
/// 
/// The game works immediately with the built-in story texts below.
/// 
/// To customize texts and timing via the Inspector:
///   1. Right-click in Project window → Create → GravityFlip → Level Intro Settings
///   2. Name the asset exactly "LevelIntroSettings"
///   3. Move it into any folder named "Resources" (create one if needed, e.g. Assets/Resources)
///   4. Fill in the level entries — they will override the defaults below
///
/// If a LevelIntroUI already exists in a scene (placed manually), the installer
/// skips that scene, allowing full per-scene overrides from the Inspector.
/// </summary>
public static class LevelIntroInstaller
{
    private static LevelIntroSettings cachedSettings;
    private static bool settingsSearched;
    private static bool skipNextIntro;

    public static void SkipNextIntro()
    {
        skipNextIntro = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene.name);
    }

    private static void TryInstall(string sceneName)
    {
        if (skipNextIntro)
        {
            skipNextIntro = false;
            return;
        }

        // Don't add a second intro if one already exists in the scene
        if (Object.FindAnyObjectByType<LevelIntroUI>() != null)
            return;

        // Try loading custom settings from Resources (once)
        if (!settingsSearched)
        {
            cachedSettings = Resources.Load<LevelIntroSettings>("LevelIntroSettings");
            settingsSearched = true;
        }

        // ---- Gather configuration ----
        string text = null;
        float displayDuration = 5f;

        // Global timing defaults
        float delayBeforeFadeIn = 0.5f;
        float textFadeInDuration = 1.5f;
        float textFadeOutDuration = 1.0f;
        float delayBeforeBackgroundFade = 0.3f;
        float backgroundFadeOutDuration = 1.0f;
        float fontSize = 36f;
        Color textColor = Color.white;
        TMPro.TMP_FontAsset fontAsset = null;
        Sprite backgroundImage = null;
        Color backgroundTint = new Color(0.4f, 0.4f, 0.4f, 1f);

        // Check the ScriptableObject first
        if (cachedSettings != null)
        {
            backgroundImage = cachedSettings.globalBackgroundImage;
            backgroundTint = cachedSettings.backgroundTint;

            if (cachedSettings.levels != null)
            {
                foreach (LevelIntroSettings.LevelEntry entry in cachedSettings.levels)
                {
                    if (entry.sceneName == sceneName)
                    {
                        text = entry.introText;
                        displayDuration = entry.displayDuration;
                        if (entry.backgroundImageOverride != null)
                            backgroundImage = entry.backgroundImageOverride;
                        break;
                    }
                }
            }

            // Use global timing from the ScriptableObject
            delayBeforeFadeIn = cachedSettings.delayBeforeFadeIn;
            textFadeInDuration = cachedSettings.textFadeInDuration;
            textFadeOutDuration = cachedSettings.textFadeOutDuration;
            delayBeforeBackgroundFade = cachedSettings.delayBeforeBackgroundFade;
            backgroundFadeOutDuration = cachedSettings.backgroundFadeOutDuration;
            fontSize = cachedSettings.fontSize;
            textColor = cachedSettings.textColor;
            fontAsset = cachedSettings.fontAsset;
        }

        // Fall back to built-in defaults if no ScriptableObject or no entry for this scene
        if (string.IsNullOrEmpty(text))
            text = GetDefaultText(sceneName, out displayDuration);

        // Not a level scene — no intro needed
        if (text == null)
            return;

        // ---- Create the intro ----
        GameObject introObj = new GameObject("LevelIntro");
        LevelIntroUI intro = introObj.AddComponent<LevelIntroUI>();

        // Set all configuration BEFORE calling BeginIntro
        intro.introText = text;
        intro.displayDuration = displayDuration;
        intro.delayBeforeFadeIn = delayBeforeFadeIn;
        intro.textFadeInDuration = textFadeInDuration;
        intro.textFadeOutDuration = textFadeOutDuration;
        intro.delayBeforeBackgroundFade = delayBeforeBackgroundFade;
        intro.backgroundFadeOutDuration = backgroundFadeOutDuration;
        intro.fontSize = fontSize;
        intro.textColor = textColor;
        intro.fontAsset = fontAsset;
        intro.backgroundImage = backgroundImage;
        intro.backgroundTint = backgroundTint;

        // Start the intro sequence
        intro.BeginIntro();
    }

    // =================================================================
    //  DEFAULT STORY TEXTS
    //
    //  Level order:
    //  Level1 → ActualLevel2 → Level2 → Level3 → Level4 → Level5 → Level6 → BossLevel1
    //
    //  Edit these directly to change the story without needing a ScriptableObject.
    // =================================================================

    private static string GetDefaultText(string sceneName, out float displayDuration)
    {
        switch (sceneName)
        {
            // ---- LEVEL 1: The Journey Begins ----
            case "Level1":
                displayDuration = 7f;
                return
                    "The Gravity Stars have kept our world in balance for as long as anyone can remember.\n\n" +
                    "But the Dragon Warrior has stolen them,\nand now gravity itself is falling apart.\n\n" +
                    "Someone has to bring them back.\nI guess that someone is me.";

            // ---- LEVEL 2: Into the Unknown ----
            case "ActualLevel2":
                displayDuration = 5.5f;
                return
                    "The first stars I found pulsed with energy when I touched them.\n" +
                    "The world steadied for just a moment.\n\n" +
                    "But there are more out there, scattered deeper into the chaos.";

            // ---- LEVEL 3: Shifting Ground ----
            case "Level2":
                displayDuration = 6f;
                return
                    "Nothing stays still anymore.\n" +
                    "The ground trembles and the air feels heavy,\n" +
                    "like the world is forgetting which way is down.\n\n" +
                    "Ancient traps still guard these paths\n blades and fire that never stop.";

            // ---- LEVEL 4: The Deep ----
            case "Level3":
                displayDuration = 6f;
                return
                    "Deeper and deeper.\n" +
                    "The traps grow more vicious, the paths more twisted.\n\n" +
                    "I keep flipping gravity just to survive\n walking on ceilings, falling upward.\n\n" +
                    "This power is the only thing keeping me alive.";

            // ---- LEVEL 5: Awakening (Double Jump unlocked) ----
            case "Level4":
                displayDuration = 6.5f;
                return
                    "With each star I recover,\nI feel gravity bending further to my will.\n\n" +
                    "Something new stirs inside me, I can push off the air itself,\n" +
                    "leaping twice before touching the ground.\n\n" +
                    "I'm getting stronger.";

            // ---- LEVEL 6: No Turning Back ----
            case "Level5":
                displayDuration = 5f;
                return
                    "The path behind me has collapsed.\n" +
                    "The only way out is forward, through the worst of it.\n\n" +
                    "Saw blades, fire, falling stone\n— every step forward is earned.";

            // ---- LEVEL 7: The Dragon's Shadow ----
            case "Level6":
                displayDuration = 5.5f;
                return
                    "I can feel him now — the Dragon Warrior.\n\n" +
                    "His presence is like a weight pressing down on everything.\n\n" +
                    "He knows I'm coming.\nAnd he's waiting.";

            // ---- LEVEL 10: The Dragon Warrior ----
            case "Level10":
                displayDuration = 5f;
                return
                    "He stands waiting, flames in his hands.\n" +
                    "All the stolen gravity bends around him.\n\n" +
                    "One of us will fall.\n" +
                    "I didn't come this far to lose.";

            default:
                displayDuration = 0f;
                return null;
        }
    }
}
