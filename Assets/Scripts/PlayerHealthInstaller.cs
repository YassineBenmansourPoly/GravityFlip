using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerHealthInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWhenLevelLoads()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddHealthIfNeeded(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddHealthIfNeeded(scene);
    }

    private static void AddHealthIfNeeded(Scene scene)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<PlayerHealth>() != null)
            return;

        // Every gameplay level should give the player health, even if the scene uses an older Player prefab.
        player.AddComponent<PlayerHealth>();
    }
}
