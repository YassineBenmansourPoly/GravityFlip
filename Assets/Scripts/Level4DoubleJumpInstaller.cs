using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level4DoubleJumpInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWhenLevelLoads()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddDoubleJumpIfThisIsLevel4(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddDoubleJumpIfThisIsLevel4(scene);
    }

    private static void AddDoubleJumpIfThisIsLevel4(Scene scene)
    {
        if (scene.name != "Level4" && scene.name != "Level5")
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<DoubleJump>() != null)
            return;

        // Level 4 and Level 5 allow one extra jump in mid-air.
        player.AddComponent<DoubleJump>();
    }
}
