using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint;
    private string activeSceneName;
    private bool shouldUseCheckpointsInScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallWhenSceneLoads()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureInstance();
        instance.SetupForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
        instance.SetupForScene(scene);
    }

    public static CheckpointManager EnsureInstance()
    {
        if (instance != null)
            return instance;

        CheckpointManager existingManager = FindFirstObjectByType<CheckpointManager>();
        if (existingManager != null)
        {
            instance = existingManager;
            return instance;
        }

        GameObject managerObject = new GameObject("CheckpointManager");
        instance = managerObject.AddComponent<CheckpointManager>();
        return instance;
    }

    public static bool TryRespawnPlayer(GameObject player)
    {
        if (player == null)
            return false;

        CheckpointManager manager = EnsureInstance();
        if (manager == null || !manager.HasCheckpointForCurrentScene())
            return false;

        manager.RespawnPlayer(player);
        return true;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        checkpointPosition.z = 0f;
        hasCheckpoint = true;
        activeSceneName = SceneManager.GetActiveScene().name;
    }

    private void SetupForScene(Scene scene)
    {
        activeSceneName = scene.name;
        hasCheckpoint = false;
        shouldUseCheckpointsInScene = ShouldUseCheckpoints(scene.name);

        if (!shouldUseCheckpointsInScene)
            return;

        TrySetStartCheckpointFromPlayer();
    }

    private void Update()
    {
        if (!hasCheckpoint && shouldUseCheckpointsInScene)
            TrySetStartCheckpointFromPlayer();
    }

    private void TrySetStartCheckpointFromPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            SetCheckpoint(player.transform.position);
    }

    private bool HasCheckpointForCurrentScene()
    {
        return hasCheckpoint && activeSceneName == SceneManager.GetActiveScene().name;
    }

    private void RespawnPlayer(GameObject player)
    {
        Time.timeScale = 1f;
        player.SetActive(true);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.transform.position = checkpointPosition;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.ResetAfterRespawn();

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.RestoreFullHealth();
    }

    private static bool ShouldUseCheckpoints(string sceneName)
    {
        if (sceneName == "BossLevel1")
            return true;

        if (!sceneName.StartsWith("Level"))
            return false;

        string numberText = sceneName.Substring("Level".Length);
        return int.TryParse(numberText, out int levelNumber) && levelNumber >= 3;
    }
}
