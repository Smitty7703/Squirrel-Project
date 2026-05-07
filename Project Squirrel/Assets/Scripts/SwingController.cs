using UnityEngine;

public class SwingController : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private LineRenderer line;
    private GameObject player;
    [SerializeField] private CameraTarget cam;
    [SerializeField] private float swingForce = 30f;
    [SerializeField] private float maxSwingSpeed = 25f;
    [SerializeField] private float inwardForce = 5f;

    SpringJoint joint;
    private bool isSwinging;

    public void Interact(GameObject interactor)
    {
        player = interactor;
        rb = player.GetComponent<Rigidbody>();
        line = player.GetComponent<LineRenderer>();

        joint = player.AddComponent<SpringJoint>();

        isSwinging = true;

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = transform.position;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        joint.maxDistance = dist;
        joint.minDistance = dist;

        joint.spring = 1000f;
        joint.damper = 50f;
        joint.massScale = 4.5f;
        //cam.SetFocus(transform);
    }

    public void Release(GameObject interactor)
    {
        Debug.Log("Swing release called");
        if (joint != null)
        {
            Debug.Log("Joint null check passed");
            //cam.SetFocus(interactor.transform);
            isSwinging = false;
            line.enabled = false;
            Destroy(joint);
            player = null;
            rb = null;
            line = null;
        }
    }

    private void LateUpdate()
    {
        if (!isSwinging) return;
        if (joint == null)
        {
            line.enabled = false;
            return;
        }
        line.enabled = true;

        if (player == null) return;
        line.SetPosition(0, player.transform.position);
        line.SetPosition(1, joint.connectedAnchor);
    }

    private void FixedUpdate()
    {
        if (joint == null || player == null) return;

        Vector3 ropeDir = (player.transform.position - joint.connectedAnchor).normalized;
        Vector3 tangent = Vector3.Cross(Vector3.forward, ropeDir).normalized;

        // determine which direction along the tangent we are moving
        float dir = Mathf.Sign(Vector3.Dot(rb.linearVelocity, tangent));

        // apply swing force in direction of motion
        rb.AddForce(tangent * dir * swingForce, ForceMode.Acceleration);

        // small inward pull for rope tightness
        rb.AddForce(-ropeDir * inwardForce, ForceMode.Acceleration);

        //Clamp max speed
        if (rb.linearVelocity.magnitude > maxSwingSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;
        }
    }
}
