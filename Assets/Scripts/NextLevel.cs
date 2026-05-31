using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "Level2";
    public float delayBeforeLoad = 1.5f;
    public float missingStarsMessageDistance = 2.5f;
    public Vector3 missingStarsLabelOffset = new Vector3(0f, 1.2f, 0f);

    private bool triggered = false;
    private Transform playerTransform;
    private Collider2D exitCollider;
    private Collider2D playerCollider;
    private bool shouldShowMissingStarsLabel;
    private GUIStyle missingStarsStyle;

    private void Awake()
    {
        exitCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        shouldShowMissingStarsLabel = false;

        if (triggered)
            return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerCollider = player.GetComponent<Collider2D>();
            }
        }

        if (playerTransform == null)
            return;

        if (!IsPlayerCloseToExit())
            return;

        StarManager starManager = StarManager.EnsureInstance();
        if (starManager != null && !starManager.CanExitLevel())
        {
            shouldShowMissingStarsLabel = true;
            starManager.ShowMissingStarsMessage();
        }
    }

    private void OnGUI()
    {
        if (!shouldShowMissingStarsLabel)
            return;

        BuildMissingStarsStyle();

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 labelWorldPosition = GetLabelWorldPosition();
        Vector3 screenPosition = camera.WorldToScreenPoint(labelWorldPosition);

        if (screenPosition.z < 0f)
            return;

        float labelWidth = 300f;
        float labelHeight = 34f;
        Rect labelRect = new Rect(
            screenPosition.x - labelWidth * 0.5f,
            Screen.height - screenPosition.y - labelHeight * 0.5f,
            labelWidth,
            labelHeight
        );

        GUI.Label(labelRect, "Missing stars", missingStarsStyle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryUseExit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryUseExit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryUseExit(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryUseExit(collision.collider);
    }

    private void TryUseExit(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            StarManager starManager = StarManager.EnsureInstance();

            if (starManager != null && !starManager.CanExitLevel())
            {
                shouldShowMissingStarsLabel = true;
                starManager.ShowMissingStarsMessage();
                return;
            }

            triggered = true;

            // 1. Play Victory Audio
            if (AudioManager.instance != null)
                AudioManager.instance.TriggerVictory();

            if (starManager != null)
                starManager.OnLevelComplete();

            // 2. Stop the player
            PlayerMovement pm = collision.GetComponent<PlayerMovement>();
            if (pm != null) pm.CompleteLevel();

            // 3. Go to next scene
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    private bool IsPlayerCloseToExit()
    {
        if (exitCollider != null && playerCollider != null)
        {
            Bounds expandedBounds = exitCollider.bounds;
            expandedBounds.Expand(missingStarsMessageDistance);
            return expandedBounds.Intersects(playerCollider.bounds);
        }

        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
        return sqrDistance <= missingStarsMessageDistance * missingStarsMessageDistance;
    }

    private Vector3 GetLabelWorldPosition()
    {
        if (exitCollider != null)
            return exitCollider.bounds.center + new Vector3(0f, exitCollider.bounds.extents.y, 0f) + missingStarsLabelOffset;

        return transform.position + missingStarsLabelOffset;
    }

    private void BuildMissingStarsStyle()
    {
        if (missingStarsStyle != null)
            return;

        missingStarsStyle = new GUIStyle(GUI.skin.label);
        missingStarsStyle.fontSize = 22;
        missingStarsStyle.fontStyle = FontStyle.Bold;
        missingStarsStyle.alignment = TextAnchor.MiddleCenter;
        missingStarsStyle.normal.textColor = new Color(1f, 0.85f, 0.15f, 1f);
    }
}
