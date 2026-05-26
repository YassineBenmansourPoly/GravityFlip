using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically creates a CutsceneManager when a cutscene scene loads.
/// Works with scenes named "OpeningCutscene" and "EndingCutscene".
///
/// The game works out-of-the-box with built-in default story text and
/// colored placeholders. To customize:
///   1. Create a CutsceneData asset (Create → GravityFlip → Cutscene Data)
///   2. Name it "OpeningCutsceneData" or "EndingCutsceneData"
///   3. Move it to Assets/Resources/
///   4. Drag your panel images into the slide entries
/// </summary>
public static class CutsceneAutoLoader
{
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
        // Only react to known cutscene scene names
        if (sceneName != "OpeningCutscene" && sceneName != "EndingCutscene")
            return;

        // Don't add a second manager if one already exists
        if (Object.FindAnyObjectByType<CutsceneManager>() != null)
            return;

        // Create the CutsceneManager GameObject
        GameObject obj = new GameObject("CutsceneManager");
        CutsceneManager manager = obj.AddComponent<CutsceneManager>();

        // Try loading a CutsceneData asset from Resources
        CutsceneData data = Resources.Load<CutsceneData>(sceneName + "Data");

        if (data != null)
        {
            manager.cutsceneData = data;
        }
        else
        {
            // Fall back to hardcoded default story text
            if (sceneName == "OpeningCutscene")
                SetupOpeningDefaults(manager);
            else
                SetupEndingDefaults(manager);
        }

        manager.BeginCutscene();
    }

    // =================================================================
    //  DEFAULT OPENING CUTSCENE — "The Shattering"
    //
    //  Six panels telling how the Dragon Warrior stole the Gravity Stars.
    //  Images are null = colored placeholders until the user imports art.
    // =================================================================

    private static void SetupOpeningDefaults(CutsceneManager manager)
    {
        manager.fallbackNextScene = "Level1";

        manager.SetOverrideSlides(new CutsceneData.Slide[]
        {
            // Panel 1: The Gravity Stars
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "For as long as anyone can remember, the Gravity Stars " +
                    "have held our world together.\n\n" +
                    "Three shining crystals in every land, keeping the ground " +
                    "below and the sky above."
            },

            // Panel 2: The Dragon Warrior Appears
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "But a powerful warrior, the Dragon Warrior, grew hungry " +
                    "for their power.\n\n" +
                    "He believed gravity was a chain \u2014 and he would break it."
            },

            // Panel 3: Stars Scattered
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "In one terrible moment, he stole the Gravity Stars and " +
                    "scattered them across the most dangerous lands.\n\n" +
                    "The world began to break."
            },

            // Panel 4: World in Chaos
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 2.5f,
                narrativeText =
                    "Gravity fractured. The ground floated. The sky fell.\n\n" +
                    "And no one could stop it."
            },

            // Panel 5: The Hero
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "No one except a small, brave adventurer who could feel " +
                    "the pull of gravity \u2014 and bend it."
            },

            // Panel 6: Title Card
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 2f,
                narrativeText =
                    "<size=60>G R A V I T Y   F L I P</size>"
            }
        },
        "\u25B6  Click or press any key to continue",
        "\u25B6  Press any key to begin"
        );
    }

    // =================================================================
    //  DEFAULT ENDING CUTSCENE — "The Mending"
    //
    //  Six panels telling how the hero restored the Gravity Stars.
    // =================================================================

    private static void SetupEndingDefaults(CutsceneManager manager)
    {
        manager.fallbackNextScene = "QUIT";

        manager.SetOverrideSlides(new CutsceneData.Slide[]
        {
            // Panel 1: The Dragon Warrior Falls
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "The Dragon Warrior falls to his knees.\n" +
                    "His fire goes out. His grip on gravity fades."
            },

            // Panel 2: The Last Star
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "The final Gravity Star pulses in the air.\n\n" +
                    "I reach for it \u2014 and the world holds its breath."
            },

            // Panel 3: Stars Reunited
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "The stars remember each other. They pull together,\n" +
                    "singing with energy, and gravity remembers what " +
                    "it\u2019s supposed to do."
            },

            // Panel 4: World Restored
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "The ground settles. The sky clears.\n\n" +
                    "Somewhere, a river finds its way downhill for the " +
                    "first time in a long while.\n\nThe world exhales."
            },

            // Panel 5: The Hero Walks Away
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 3f,
                narrativeText =
                    "The stars are home. The world is whole.\n\n" +
                    "And a small adventurer walks on \u2014\n" +
                    "ready for whatever comes next."
            },

            // Panel 6: Thank You
            new CutsceneData.Slide
            {
                image = null,
                minimumDisplayTime = 2f,
                narrativeText =
                    "Thank you for playing\n\n" +
                    "<size=60>G R A V I T Y   F L I P</size>"
            }
        },
        "\u25B6  Click or press any key to continue",
        "\u25B6  Press any key"
        );
    }
}
