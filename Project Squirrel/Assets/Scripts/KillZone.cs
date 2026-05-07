using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private string tag;
    [SerializeField] private PlayerController player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tag))
        {
            if (player == null)
                player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Respawn();
            }
        }
    }
}
