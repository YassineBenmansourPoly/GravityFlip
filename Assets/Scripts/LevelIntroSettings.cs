using UnityEngine;
using TMPro; // Needed for TMP_FontAsset

/// <summary>
/// ScriptableObject for customizing level intro texts and timing via the Inspector.
/// 
/// HOW TO USE:
///   1. In Unity, right-click in the Project window
///   2. Select: Create → GravityFlip → Level Intro Settings
///   3. Name the asset exactly "LevelIntroSettings"
///   4. Move it inside any folder called "Resources" (e.g. Assets/Resources/)
/// </summary>
[CreateAssetMenu(fileName = "LevelIntroSettings", menuName = "GravityFlip/Level Intro Settings")]
public class LevelIntroSettings : ScriptableObject
{
    [System.Serializable]
    public class LevelEntry
    {
        [Tooltip("The exact Unity scene name (e.g. Level1, ActualLevel2, BossLevel1).")]
        public string sceneName;

        [TextArea(3, 10)]
        [Tooltip("The story text displayed at the start of this level.")]
        public string introText;

        [Tooltip("How long the text stays fully visible on screen (seconds). Adjust based on text length.")]
        public float displayDuration = 5f;

        [Tooltip("Optional: A specific background image for this level. Overrides the global one.")]
        public Sprite backgroundImageOverride;
    }

    [Header("Background Image")]
    [Tooltip("Optional: A background image to show instead of the black screen for all levels.")]
    public Sprite globalBackgroundImage;
    [Tooltip("Tint applied to the background image. Darker tint helps white text stand out.")]
    public Color backgroundTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Global Timing (Seconds)")]
    public float delayBeforeFadeIn = 0.5f;
    public float textFadeInDuration = 1.5f;
    public float textFadeOutDuration = 1.0f;
    public float delayBeforeBackgroundFade = 0.3f;
    public float backgroundFadeOutDuration = 1.0f;

    [Header("Text Appearance")]
    [Tooltip("Font size for the intro text.")]
    public float fontSize = 36f;
    
    [Tooltip("Color of the intro text.")]
    public Color textColor = Color.white;

    [Tooltip("Assign a custom TextMeshPro font asset (like a Pixel font). If empty, uses default.")]
    public TMP_FontAsset fontAsset;

    [Header("Level Entries")]
    [Tooltip("One entry per level. These are pre-filled with the defaults!")]
    public LevelEntry[] levels = new LevelEntry[]
    {
        new LevelEntry { 
            sceneName = "Level1", 
            displayDuration = 7f, 
            introText = "The Gravity Stars have kept our world in balance for as long as anyone can remember.\n\nBut the Dragon Warrior has stolen them,\nand now gravity itself is falling apart.\n\nSomeone has to bring them back.\nI guess that someone is me." 
        },
        new LevelEntry { 
            sceneName = "ActualLevel2", 
            displayDuration = 5.5f, 
            introText = "The first stars I found pulsed with energy when I touched them.\nThe world steadied for just a moment.\n\nBut there are more out there, scattered deeper into the chaos." 
        },
        new LevelEntry { 
            sceneName = "Level2", 
            displayDuration = 6f, 
            introText = "Nothing stays still anymore.\nThe ground trembles and the air feels heavy,\nlike the world is forgetting which way is down.\n\nAncient traps still guard these paths\n— blades and fire that never stop." 
        },
        new LevelEntry { 
            sceneName = "Level3", 
            displayDuration = 6f, 
            introText = "Deeper and deeper.\nThe traps grow more vicious, the paths more twisted.\n\nI keep flipping gravity just to survive\n— walking on ceilings, falling upward.\n\nThis power is the only thing keeping me alive." 
        },
        new LevelEntry { 
            sceneName = "Level4", 
            displayDuration = 6.5f, 
            introText = "With each star I recover,\nI feel gravity bending further to my will.\n\nSomething new stirs inside me — I can push off the air itself,\nleaping twice before touching the ground.\n\nI'm getting stronger." 
        },
        new LevelEntry { 
            sceneName = "Level5", 
            displayDuration = 5f, 
            introText = "The path behind me has collapsed.\nThe only way out is forward, through the worst of it.\n\nSaw blades, fire, falling stone\n— every step forward is earned." 
        },
        new LevelEntry { 
            sceneName = "Level6", 
            displayDuration = 5.5f, 
            introText = "I can feel him now — the Dragon Warrior.\n\nHis presence is like a weight pressing down on everything.\n\nHe knows I'm coming.\nAnd he's waiting." 
        },
        new LevelEntry { 
            sceneName = "BossLevel1", 
            displayDuration = 5f, 
            introText = "He stands waiting, flames in his hands.\nAll the stolen gravity bends around him.\n\nOne of us will fall.\nI didn't come this far to lose." 
        }
    };

    [ContextMenu("Load Default Story Texts")]
    public void LoadDefaults()
    {
        levels = new LevelEntry[]
        {
            new LevelEntry { 
                sceneName = "Level1", 
                displayDuration = 7f, 
                introText = "The Gravity Stars have kept our world in balance for as long as anyone can remember.\n\nBut the Dragon Warrior has stolen them,\nand now gravity itself is falling apart.\n\nSomeone has to bring them back.\nI guess that someone is me." 
            },
            new LevelEntry { 
                sceneName = "ActualLevel2", 
                displayDuration = 5.5f, 
                introText = "The first stars I found pulsed with energy when I touched them.\nThe world steadied for just a moment.\n\nBut there are more out there, scattered deeper into the chaos." 
            },
            new LevelEntry { 
                sceneName = "Level2", 
                displayDuration = 6f, 
                introText = "Nothing stays still anymore.\nThe ground trembles and the air feels heavy,\nlike the world is forgetting which way is down.\n\nAncient traps still guard these paths\n— blades and fire that never stop." 
            },
            new LevelEntry { 
                sceneName = "Level3", 
                displayDuration = 6f, 
                introText = "Deeper and deeper.\nThe traps grow more vicious, the paths more twisted.\n\nI keep flipping gravity just to survive\n— walking on ceilings, falling upward.\n\nThis power is the only thing keeping me alive." 
            },
            new LevelEntry { 
                sceneName = "Level4", 
                displayDuration = 6.5f, 
                introText = "With each star I recover,\nI feel gravity bending further to my will.\n\nSomething new stirs inside me — I can push off the air itself,\nleaping twice before touching the ground.\n\nI'm getting stronger." 
            },
            new LevelEntry { 
                sceneName = "Level5", 
                displayDuration = 5f, 
                introText = "The path behind me has collapsed.\nThe only way out is forward, through the worst of it.\n\nSaw blades, fire, falling stone\n— every step forward is earned." 
            },
            new LevelEntry { 
                sceneName = "Level6", 
                displayDuration = 5.5f, 
                introText = "I can feel him now — the Dragon Warrior.\n\nHis presence is like a weight pressing down on everything.\n\nHe knows I'm coming.\nAnd he's waiting." 
            },
            new LevelEntry { 
                sceneName = "BossLevel1", 
                displayDuration = 5f, 
                introText = "He stands waiting, flames in his hands.\nAll the stolen gravity bends around him.\n\nOne of us will fall.\nI didn't come this far to lose." 
            }
        };
    }

    private void Reset()
    {
        LoadDefaults();
    }
}
