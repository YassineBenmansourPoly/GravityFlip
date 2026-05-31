using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level4DoubleJumpInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWhenLevelLoads()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddDoubleJumpIfUnlocked(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddDoubleJumpIfUnlocked(scene);
    }

    private static void AddDoubleJumpIfUnlocked(Scene scene)
    {
        if (!ShouldHaveDoubleJump(scene.name))
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<DoubleJump>() != null)
            return;

        // Double jump unlocks in Level 4 and stays available for later levels.
        player.AddComponent<DoubleJump>();
    }

    private static bool ShouldHaveDoubleJump(string sceneName)
    {
        if (!sceneName.StartsWith("Level"))
            return false;

        string numberText = sceneName.Substring("Level".Length);
        return int.TryParse(numberText, out int levelNumber) && levelNumber >= 4;
    }
}
