using UnityEngine;

public class ClimbController : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 attachOffset;
    [SerializeField] private float minOutForce; [SerializeField] private float maxOutForce;
    [SerializeField] private float minUpForce; [SerializeField] private float maxUpForce;

    Vector3 targetPos;
    PlayerController player;
    private Collider _collider;
    private Rigidbody _rigidbody;
    private PowerMeterUI _powerMeter;

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public void Interact(GameObject interactObj)
    {
        //determine target position and force player position to target position
        targetPos = transform.TransformPoint(attachOffset);
        interactObj.transform.position = targetPos;

        //Get references and subscribe to them
        player = interactObj.GetComponent<PlayerController>();
        _powerMeter = interactObj.GetComponentInChildren<PowerMeterUI>();
        _rigidbody = player.GetComponent<Rigidbody>();

        //set proper variables
        player.SetClamp(interactObj.transform.position.x, interactObj.transform.position.x, _collider.bounds.min.y, _collider.bounds.max.y, true);
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        player.SetGroundMovement(true);

        //Activate power gauge slider
        _powerMeter.Activate();
    }

    public void Release(GameObject interactObj)
    {
        player.SetClamp(0, 0, 0, 0, false);
        _rigidbody.useGravity = true;
        player.SetGroundMovement(false);

        Vector3 upForce = Vector3.up;
        Vector3 outForce = Vector3.zero;
        if (_powerMeter != null)
        {
            float outVal;
            float upVal;

            float val = _powerMeter.Release();
            outVal = Mathf.Lerp(minOutForce, maxOutForce, val);
            upVal = Mathf.Lerp(minUpForce, maxUpForce, val);

            upForce = Vector3.up * upVal;
            outForce = (transform.rotation * Vector3.left) * outVal;

            _powerMeter.Deactivate();
        }
        if (_rigidbody != null)
        {
            _rigidbody.AddForce(upForce, ForceMode.Impulse);
            _rigidbody.AddForce(outForce, ForceMode.Impulse);
        }
    }
}
