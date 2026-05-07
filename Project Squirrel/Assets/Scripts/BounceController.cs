using UnityEngine;

public class BounceController : MonoBehaviour, IInteractable
{
    [SerializeField] private float cooldown;
    private Rigidbody rb;
    private CooldownManager _CDManager;
    [SerializeField] private float baseStrength;
    [SerializeField] private float bounceStrength;
    PlayerController _player;
    private int index;

    private void Start()
    {
        _CDManager = CooldownManager.Instance;
        index = _CDManager.AddCooldown(cooldown);
    }
    public void Interact(GameObject interactObj)
    {
        Debug.Log("isCooldownActive: " + _CDManager.InquireCooldownStatus(index));
        if (_CDManager.InquireCooldownStatus(index)) return; //check if cooldown is active

        _CDManager.StartCooldown(index); //start cooldown
        _player = interactObj.GetComponent<PlayerController>();
        rb = interactObj.GetComponent<Rigidbody>();
        if (rb != null && _player != null)
        {
            Vector3 baseForce = Vector3.up * baseStrength;
            Vector3 bounceForce = transform.right * bounceStrength;

            rb.linearVelocity = Vector3.zero;

            rb.AddForce(baseForce, ForceMode.Impulse);
            rb.AddForce(bounceForce, ForceMode.Impulse);
            //Debug.Log("Base Force: " + baseForce + ", Bounce Force: " + bounceForce);
            _player.ReleaseInteract();
        }
    }
    public void Release(GameObject interactObj)
    {
    }

}
