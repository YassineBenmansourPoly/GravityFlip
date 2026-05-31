using UnityEngine;
using TMPro;

/// <summary>
/// ScriptableObject that holds cutscene slide data (images + narrative text).
///
/// HOW TO USE:
///   1. In Unity, right-click in the Project window
///   2. Select: Create → GravityFlip → Cutscene Data
///   3. Name it "OpeningCutsceneData" or "EndingCutsceneData"
///   4. Move it into any folder called "Resources" (e.g. Assets/Resources/)
///   5. Drag your panel images into the Slide entries and edit the text
///   6. The CutsceneAutoLoader will find and use it automatically!
///
/// If no asset is found in Resources, the system falls back to hardcoded
/// default story text with colored placeholders for images.
/// </summary>
[CreateAssetMenu(fileName = "CutsceneData", menuName = "GravityFlip/Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    [System.Serializable]
    public class Slide
    {
        [Tooltip("Panel artwork. Leave empty to use a colored placeholder.")]
        public Sprite image;

        [TextArea(3, 10)]
        [Tooltip("Narrative text displayed at the bottom of the screen.")]
        public string narrativeText;

        [Tooltip("Minimum seconds this slide stays visible before the player can advance.")]
        public float minimumDisplayTime = 2f;
    }

    [Header("Slides")]
    [Tooltip("The cutscene panels, played in order.")]
    public Slide[] slides;

    [Header("Scene Flow")]
    [Tooltip("Scene to load when the cutscene finishes.")]
    public string nextSceneName = "Level1";

    [Header("Transition Timing (Seconds)")]
    [Tooltip("Duration of fade in/out transitions between slides.")]
    public float fadeDuration = 1.2f;

    [Tooltip("Delay after the image fades in before the text appears.")]
    public float textAppearDelay = 0.4f;

    [Header("Text Appearance")]
    [Tooltip("Font size for narrative text.")]
    public float fontSize = 34f;

    [Tooltip("Color of the narrative text.")]
    public Color textColor = Color.white;

    [Tooltip("Optional TextMeshPro font asset (e.g. a pixel font). Leave empty for default.")]
    public TMP_FontAsset fontAsset;

    [Header("Prompt")]
    [Tooltip("Text shown to prompt the player to advance.")]
    public string continuePrompt = "\u25B6  Click or press any key to continue";

    [Tooltip("Text shown on the final slide.")]
    public string finalSlidePrompt = "\u25B6  Press any key to begin";

    [Header("Audio")]
    [Tooltip("Optional background music clip to play during the cutscene.")]
    public AudioClip backgroundMusic;

    [Range(0f, 1f)]
    [Tooltip("Volume of the background music.")]
    public float musicVolume = 0.35f;
}
