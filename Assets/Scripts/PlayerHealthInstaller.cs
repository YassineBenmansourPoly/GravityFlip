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
        if (scene.name == "Level1" || scene.name == "Level2" || scene.name == "ActualLevel2")
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<PlayerHealth>() != null)
            return;

        player.AddComponent<PlayerHealth>();
    }
}
