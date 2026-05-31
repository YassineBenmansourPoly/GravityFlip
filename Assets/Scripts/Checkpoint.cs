using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool activated;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        activated = true;
        CheckpointManager.EnsureInstance().SetCheckpoint(transform.position);

        if (activatedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = activatedSprite;
    }
}
