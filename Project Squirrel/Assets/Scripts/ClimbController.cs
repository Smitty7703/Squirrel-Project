using UnityEngine;

public class ClimbController : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 attachOffset;
    Vector3 targetPos;
    PlayerController player;
    private Collider _collider;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public void Interact(GameObject interactObj)
    {
        targetPos = transform.TransformPoint(attachOffset);
        interactObj.transform.position = targetPos;
        player = interactObj.GetComponent<PlayerController>();
        _rigidbody = player.GetComponent<Rigidbody>();
        player.SetClamp(interactObj.transform.position.x, interactObj.transform.position.x, _collider.bounds.min.y, _collider.bounds.max.y, true);
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
    }

    public void Release(GameObject interactObj)
    {

    }

    //disable gravity

}
