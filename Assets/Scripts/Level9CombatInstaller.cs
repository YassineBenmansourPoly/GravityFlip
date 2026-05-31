using UnityEngine;
using UnityEngine.SceneManagement;

public class Level9CombatInstaller : MonoBehaviour
{
    private static bool subscribedToSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForLoadedScene()
    {
        if (!subscribedToSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            subscribedToSceneLoaded = true;
        }

        SetupLevel9Combat();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupLevel9Combat();
    }

    private static void SetupLevel9Combat()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Level10")
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetupPlayer(player);
            SetupCamera(player);
        }

        LockedExitDoor exitDoor = FindOrCreateLockedExit();
        GameObject boss = FindOrCreateBoss(player);
        if (boss == null)
            return;

        BossEnemyHealth bossHealth = boss.GetComponent<BossEnemyHealth>();
        if (bossHealth != null)
            bossHealth.lockedExitDoor = exitDoor;
    }

    private static void SetupPlayer(GameObject player)
    {
        PlayerShooter shooter = player.GetComponent<PlayerShooter>();
        if (shooter == null)
            shooter = player.AddComponent<PlayerShooter>();

        Transform firePoint = player.transform.Find("FirePoint");
        if (firePoint == null)
        {
            GameObject sceneFirePoint = GameObject.Find("FirePoint");
            if (sceneFirePoint != null)
            {
                firePoint = sceneFirePoint.transform;
                firePoint.SetParent(player.transform, true);
            }
        }

        if (firePoint == null)
        {
            GameObject firePointObject = new GameObject("FirePoint");
            firePointObject.transform.SetParent(player.transform);
            firePointObject.transform.localPosition = new Vector3(0.75f, 0.05f, 0f);
            firePoint = firePointObject.transform;
        }

        shooter.firePoint = firePoint;
        shooter.playerFireballPrefab = LoadPrefabFromProject("Assets/Prefabs/PlayerFireball.prefab");
    }

    private static GameObject FindOrCreateBoss(GameObject player)
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Enemy");
        if (boss != null && boss.CompareTag("Player"))
            boss = null;

        if (boss == null)
            boss = GameObject.Find("FireBoss");

        if (boss == null)
            return null;

        boss.tag = "Enemy";
        ApplyFireBossSprite(boss);

        if (boss.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (boss.GetComponent<Collider2D>() == null)
            boss.AddComponent<BoxCollider2D>();

        FireBossShooter shooter = boss.GetComponent<FireBossShooter>();
        if (shooter == null)
            shooter = boss.AddComponent<FireBossShooter>();

        Transform enemyFirePoint = boss.transform.Find("EnemyFirePoint");
        if (enemyFirePoint == null)
        {
            GameObject sceneFirePoint = GameObject.Find("EnemyFirePoint");
            if (sceneFirePoint != null)
            {
                enemyFirePoint = sceneFirePoint.transform;
                enemyFirePoint.SetParent(boss.transform, true);
            }
        }

        if (enemyFirePoint == null)
        {
            GameObject firePointObject = new GameObject("EnemyFirePoint");
            firePointObject.transform.SetParent(boss.transform);
            firePointObject.transform.localPosition = new Vector3(-0.65f, 0.1f, 0f);
            enemyFirePoint = firePointObject.transform;
        }

        shooter.enemyFirePoint = enemyFirePoint;
        shooter.enemyFireballPrefab = LoadPrefabFromProject("Assets/Prefabs/EnemyFireball.prefab");

        if (boss.GetComponent<FireBossJump>() == null)
            boss.AddComponent<FireBossJump>();

        FireBossFacePlayer facePlayer = boss.GetComponent<FireBossFacePlayer>();
        if (facePlayer == null)
            facePlayer = boss.AddComponent<FireBossFacePlayer>();

        if (player != null)
            facePlayer.player = player.transform;

        if (boss.GetComponent<BossEnemyHealth>() == null)
            boss.AddComponent<BossEnemyHealth>();

        return boss;
    }

    private static void SetupCamera(GameObject player)
    {
        Camera camera = Camera.main;
        if (camera == null)
            camera = FindFirstObjectByType<Camera>();

        if (camera == null)
            return;

        camera.gameObject.tag = "MainCamera";

        CameraFollow cameraFollow = camera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
            cameraFollow = camera.gameObject.AddComponent<CameraFollow>();

        cameraFollow.target = player.transform;
        cameraFollow.offset = new Vector3(0f, 0.8f, -10f);
        cameraFollow.smoothSpeed = 5f;
    }

    private static void ApplyFireBossSprite(GameObject boss)
    {
        SpriteRenderer renderer = boss.GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        Sprite fireBossSprite = LoadSpriteFromProject("Assets/Dragon Warrior Files/Dragon Warrior PNG/idle_01.png");
        if (fireBossSprite != null)
            renderer.sprite = fireBossSprite;

        renderer.color = Color.white;
        renderer.sortingOrder = 10;
    }

    private static LockedExitDoor FindOrCreateLockedExit()
    {
        LockedExitDoor exitDoor = FindFirstObjectByType<LockedExitDoor>();
        if (exitDoor != null)
            return exitDoor;

        GameObject exitObject = GameObject.Find("ExitDoor");
        if (exitObject == null)
            exitObject = GameObject.Find("Portal");

        if (exitObject == null)
        {
            exitObject = new GameObject("ExitDoor");
            exitObject.transform.position = new Vector3(12f, -2f, 0f);

            SpriteRenderer renderer = exitObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSquareSprite(Color.red);
            renderer.sortingOrder = 8;

            BoxCollider2D collider = exitObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1f, 1.6f);
        }

        LevelExit oldExit = exitObject.GetComponent<LevelExit>();
        if (oldExit != null)
            oldExit.enabled = false;

        exitDoor = exitObject.GetComponent<LockedExitDoor>();
        if (exitDoor == null)
            exitDoor = exitObject.AddComponent<LockedExitDoor>();

        exitDoor.unlocked = false;
        return exitDoor;
    }

    private static Sprite CreateSquareSprite(Color color)
    {
        Texture2D texture = new Texture2D(8, 8);
        Color[] pixels = new Color[64];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 16f);
    }

    private static GameObject LoadPrefabFromProject(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        return null;
#endif
    }

    private static Sprite LoadSpriteFromProject(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
        return null;
#endif
    }
}
