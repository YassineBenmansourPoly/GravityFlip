#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tools for GravityFlip project setup.
/// Accessible from the Unity menu bar: GravityFlip → ...
/// </summary>
public static class GravityFlipEditorTools
{
    // =================================================================
    //  CREATE CUTSCENE SCENES
    //
    //  Creates empty OpeningCutscene and EndingCutscene scenes
    //  in Assets/Scenes/. These are intentionally empty — the
    //  CutsceneAutoLoader will build all UI at runtime.
    // =================================================================

    [MenuItem("GravityFlip/Create Cutscene Scenes")]
    public static void CreateCutsceneScenes()
    {
        bool createdAny = false;
        createdAny |= CreateEmptyScene("Assets/Scenes/OpeningCutscene.unity");
        createdAny |= CreateEmptyScene("Assets/Scenes/EndingCutscene.unity");

        string message = createdAny
            ? "Created cutscene scenes in Assets/Scenes/.\n\n" +
              "These are empty scenes — the CutsceneAutoLoader\n" +
              "will automatically build all UI when they load.\n\n" +
              "Next step: Run 'GravityFlip → Setup All Build Scenes'\n" +
              "to add them to your Build Settings."
            : "Both cutscene scenes already exist!\n\n" +
              "If you want to recreate them, delete the existing\n" +
              "files first and run this again.";

        EditorUtility.DisplayDialog("GravityFlip — Cutscene Scenes", message, "OK");
    }

    // =================================================================
    //  SETUP BUILD SCENES
    //
    //  Configures Unity's Build Settings with all scenes in the
    //  correct play order: Opening → 8 Levels → Ending.
    // =================================================================

    [MenuItem("GravityFlip/Setup All Build Scenes")]
    public static void SetupBuildScenes()
    {
        string[] sceneOrder = new string[]
        {
            "Assets/Scenes/OpeningCutscene.unity",  // 0 — Game starts here
            "Assets/Scenes/Level1.unity",            // 1
            "Assets/Scenes/ActualLevel2.unity",      // 2
            "Assets/Scenes/Level2.unity",            // 3
            "Assets/Scenes/Level3.unity",            // 4
            "Assets/Scenes/Level4.unity",            // 5
            "Assets/Scenes/Level5.unity",            // 6
            "Assets/Scenes/Level6.unity",            // 7
            "Assets/Scenes/BossLevel1.unity",        // 8
            "Assets/Scenes/EndingCutscene.unity",    // 9
        };

        var scenes = new EditorBuildSettingsScene[sceneOrder.Length];
        int missing = 0;

        for (int i = 0; i < sceneOrder.Length; i++)
        {
            bool exists = System.IO.File.Exists(sceneOrder[i]);
            scenes[i] = new EditorBuildSettingsScene(sceneOrder[i], exists);

            if (!exists)
            {
                Debug.LogWarning("[GravityFlip] Scene not found: " + sceneOrder[i]);
                missing++;
            }
        }

        EditorBuildSettings.scenes = scenes;

        string summary =
            "Build Settings updated!\n\n" +
            "Scene order:\n" +
            "  0.  OpeningCutscene\n" +
            "  1.  Level1\n" +
            "  2.  ActualLevel2\n" +
            "  3.  Level2\n" +
            "  4.  Level3\n" +
            "  5.  Level4\n" +
            "  6.  Level5\n" +
            "  7.  Level6\n" +
            "  8.  BossLevel1\n" +
            "  9.  EndingCutscene";

        if (missing > 0)
            summary += "\n\n⚠ " + missing + " scene(s) were not found on disk.\n" +
                       "Run 'GravityFlip → Create Cutscene Scenes' first!";

        Debug.Log("[GravityFlip] Build scenes configured. " + sceneOrder.Length + " scenes set.");

        EditorUtility.DisplayDialog("GravityFlip — Build Scenes", summary, "OK");
    }

    // =================================================================
    //  HELPER: Create an empty scene with just a black-background camera
    // =================================================================

    private static bool CreateEmptyScene(string path)
    {
        if (System.IO.File.Exists(path))
        {
            Debug.Log("[GravityFlip] Scene already exists, skipping: " + path);
            return false;
        }

        // Ensure the directory exists
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        // Create a minimal scene with a camera
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.tag = "MainCamera";

        // Move the camera into our new scene
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObj, scene);

        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.CloseScene(scene, true);

        Debug.Log("[GravityFlip] Created scene: " + path);

        // Refresh the asset database so Unity sees the new file
        AssetDatabase.Refresh();
        return true;
    }
}
#endif
