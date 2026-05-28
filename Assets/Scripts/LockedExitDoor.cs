using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedExitDoor : MonoBehaviour
{
    [Header("Exit")]
    public string nextSceneName = "EndingCutscene";
    public bool unlocked;

    private SpriteRenderer spriteRenderer;
    private Collider2D exitCollider;
    private GUIStyle lockedStyle;
    private float lockedMessageTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        exitCollider = GetComponent<Collider2D>();
        UpdateVisualState();
    }

    public void Unlock()
    {
        unlocked = true;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = unlocked ? Color.green : Color.red;

        if (exitCollider != null)
            exitCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryUseExit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryUseExit(collision);
    }

    private void TryUseExit(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!unlocked)
        {
            lockedMessageTimer = 1.2f;
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    private void Update()
    {
        if (lockedMessageTimer > 0f)
            lockedMessageTimer -= Time.deltaTime;
    }

    private void OnGUI()
    {
        if (lockedMessageTimer <= 0f)
            return;

        if (lockedStyle == null)
        {
            lockedStyle = new GUIStyle(GUI.skin.label);
            lockedStyle.fontSize = 22;
            lockedStyle.fontStyle = FontStyle.Bold;
            lockedStyle.alignment = TextAnchor.MiddleCenter;
            lockedStyle.normal.textColor = new Color(1f, 0.85f, 0.15f, 1f);
        }

        GUI.Label(new Rect((Screen.width - 420f) * 0.5f, 80f, 420f, 40f), "Defeat the boss first", lockedStyle);
    }
}
