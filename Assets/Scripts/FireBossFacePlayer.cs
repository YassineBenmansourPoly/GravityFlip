using UnityEngine;

public class FireBossFacePlayer : MonoBehaviour
{
    public Transform player;
    private float originalXScale;

    private void Awake()
    {
        originalXScale = Mathf.Abs(transform.localScale.x);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        float facingSign = player.position.x >= transform.position.x ? 1f : -1f;
        scale.x = originalXScale * facingSign;
        transform.localScale = scale;
    }
}
