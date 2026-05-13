using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact(GameObject interactor);
    void Release(GameObject interactor);
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputSubscription GetInput;
    [SerializeField] private GameObject activeCheckpoint;
    [SerializeField] private GameObject interactableObj;

    [SerializeField] private Vector3 jumpForce;

    [SerializeField] private float airNudgeForce;
    [SerializeField] private float moveForce;
    [SerializeField] private float maxAirSpeed;
    [SerializeField] private float maxGroundSpeed;

    private Rigidbody rb;
    private Vector3 moveDir;
    private float maxSpeed;

    private string interTag = "InteractPoint";
    private string gTag = "Ground";
    [SerializeField] private bool isInteracting = false;
    [SerializeField] private bool isOnGround = false;
    private bool forceGroundMovement = false;

    [Header("Movement")]
    private bool movementClamped;
    private float minXClamp; private float maxXClamp;
    private float minYClamp; private float maxYClamp;
    private float currentSpeed = 0f;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    private Vector3 previousInputVel = Vector3.zero;

    #region Updates and Triggers
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        maxSpeed = maxAirSpeed;
    }

    private void FixedUpdate()
    {
        float force;
        if (forceGroundMovement) force = moveForce;
        else
        {
            force = isOnGround ? moveForce : airNudgeForce;
        }

        if (movementClamped)
        {
            ClampCheck();
            rb.AddForce(new Vector3(moveDir.x * force, moveDir.y * force, 0f));
        }
        else
        {
            rb.AddForce(new Vector3(moveDir.x * force, moveDir.y * force, 0f));
        }

        if (isOnGround || forceGroundMovement)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = Mathf.Clamp(vel.x, -maxGroundSpeed, maxGroundSpeed);
            vel.y = Mathf.Clamp(vel.y, -maxGroundSpeed, maxGroundSpeed);
            rb.linearVelocity = vel;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(interTag) && !isInteracting)
        {
            interactableObj = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(interTag) && !isInteracting)
        {
            ResetInteraction();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(gTag))
        {
            isOnGround = true;
            maxSpeed = maxGroundSpeed;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(gTag))
        {
            isOnGround = false;
            maxSpeed = maxAirSpeed;
        }
    }
    #endregion

    #region Interaction
    public void PressInteract()
    {
        if (isInteracting) return;
        if (interactableObj == null && isOnGround)
        {
            Jump();
            return;
        }
        if (interactableObj != null)
        {
            IInteractable interactable = interactableObj.GetComponent<IInteractable>();
            if (interactable != null)
            {
                isInteracting = true;
                interactable.Interact(gameObject);
            }
        }
    }

    public void ReleaseInteract()
    {
        if (interactableObj == null || !isInteracting) return;
        Debug.Log("Release Called");
        IInteractable interactable = interactableObj.GetComponent<IInteractable>();
        if (interactable != null)
        {
            Debug.Log("interactable null check passed");
            isInteracting = false;
            interactable.Release(gameObject);
            ResetInteraction();
        }
    }
    public void ResetInteraction()
    {
        isInteracting = false;
        interactableObj = null;
    }
    #endregion

    #region Variable Changes
    public void SetMovement(Vector2 vel)
    {
        moveDir = vel;
    }

    public void ChangeCheckpoint(GameObject newPoint)
    {
        activeCheckpoint = newPoint;
    }

    public void SetClamp(float xMin, float xMax, float yMin, float yMax, bool isClamped)
    {
        minXClamp = xMin;
        minYClamp = yMin;
        maxXClamp = xMax;
        maxYClamp = yMax;
        movementClamped = isClamped;
    }

    public void SetGroundMovement(bool isGround)
    {
        forceGroundMovement = isGround;
    }
    #endregion

    private void ClampCheck()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minXClamp, maxXClamp);
        pos.y = Mathf.Clamp(pos.y, minYClamp, maxYClamp);
        transform.position = pos;

        bool atYMin = rb.position.y <= minYClamp;
        bool atYMax = rb.position.y >= maxYClamp;
        if (atYMin || atYMax)
        {
            if (atYMin)
            {
                transform.position += new Vector3(0f, .01f, 0f);
            }
            if (atYMax)
            {
                transform.position += new Vector3(0f, -.01f, 0f);
            }
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void Jump()
    {
        rb.AddForce(jumpForce, ForceMode.Impulse);
    }

    public void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = activeCheckpoint.transform.position;
    }
}
