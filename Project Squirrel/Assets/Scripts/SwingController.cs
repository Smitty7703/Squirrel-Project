using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.VisualScripting;

public enum SwingMode { Basic, Reel }

public class SwingController : MonoBehaviour, IInteractable
{
    //Runtime component refs - assigned on Interact, cleared on Cleanup
    private Rigidbody rb;
    private LineRenderer line;
    private GameObject player;
    private FocusObject originalCamFocus;

    [Header("References")]
    [SerializeField] private CameraTarget cam;

    [Header("Mode")]
    public SwingMode swingMode = SwingMode.Basic;

    [Header("Swing")]
    public float initialSwingSpeed = 180f;
    public float maxSwingSpeed = 360f;
    public float swingAcceleration = 90f;

    [Header("Reel")]
    public float reelInSpeed = 3f;
    public float reelOutSpeed = 2f;
    public float minRopeLength = 1.5f;

    [Header("Boost")]
    public float boostSpeedThreshold = 8f;
    public float boostMaxSpeed = 720f;
    public float boostAcceleration = 360f;

    [Header("Boost Aim")]
    [SerializeField] private GameObject arrowPrefab;
    [Tooltip("Arrow Transform whose local X axis points toward the arrowhead tip.")]
    [SerializeField] private Transform boostArrow;
    [Tooltip("Distance the arrow orbits from this sphere's centre.")]
    [SerializeField] private float boostArrowRadius = 2.0f;
    [Tooltip("How fast A/D rotate the aim arrow while boosted (radians per second).")]
    [SerializeField] private float boostAimSpeed = 2.0f;
    [Tooltip("If true, camera follows the player after a boost launch instead of restoring the previous focus.")]
    [SerializeField] private bool refocusPlayerOnBoostLaunch = false;

    // Read-only state - exposed for animation hooks
    public float Angle => angle;
    public float AngularVelocity => angularVelocity;
    public float CurrentRadius => currentRadius;
    public bool IsBoosted => isBoosted;

    //Runtime state
    private bool isSwinging;
    private bool isBoosted;
    private float angle;
    private float angularVelocity;
    private float currentRadius;
    private float maxRadius;
    private float currentSpeedCap;
    private float entryAngularSpeed;
    private bool wasKinematic;

    //Boost aim angle in radians, seeded from orbit angle when boost activates.
    //While boosted, reelIn/reelOut actions are repurposed to rotate this instead.
    private float aimAngle;

    private SphereCollider sphereCollider;
    private InputAction reelInAction;
    private InputAction reelOutAction;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();

        if (swingMode == SwingMode.Reel)
            boostArrow = Instantiate(arrowPrefab, transform).transform;
        if (boostArrow != null)
            boostArrow.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!isSwinging || player == null || rb == null) return;

        if (swingMode == SwingMode.Reel)
        {
            //While boosted the reel buttons are repurposed for aim - skip normal reel
            if (isBoosted)
                HandleBoostAim();
            else
                HandleReel();

            HandleBoost();
            if (!isSwinging) return;
        }

        //Speed cap - boost manages its own cap internally, so skip this block
        if (!isBoosted)
        {
            if (swingMode == SwingMode.Reel && currentRadius > 0.001f)
            {
                //Conserve linear speed cap across radius changes (reel)
                float linearCap = maxSwingSpeed * Mathf.Deg2Rad * maxRadius;
                currentSpeedCap = Mathf.Max(entryAngularSpeed, linearCap / currentRadius);
            }
            else
            {
                currentSpeedCap = Mathf.Max(entryAngularSpeed, maxSwingSpeed * Mathf.Deg2Rad);
            }
        }

        float sign = Mathf.Sign(angularVelocity);
        if (sign == 0f) sign = 1f;

        angularVelocity = sign * Mathf.MoveTowards(
            Mathf.Abs(angularVelocity),
            currentSpeedCap,
            swingAcceleration * Mathf.Deg2Rad * Time.fixedDeltaTime
        );

        angle += angularVelocity * Time.fixedDeltaTime;

        Vector3 newPos = transform.position + new Vector3(
            Mathf.Cos(angle) * currentRadius,
            Mathf.Sin(angle) * currentRadius,
            0f
        );
        newPos.z = player.transform.position.z;
        rb.MovePosition(newPos);
    }

    private void LateUpdate()
    {
        if (!isSwinging || player == null) return;

        //Rope line
        if (line != null)
        {
            line.enabled = true;
            line.SetPosition(0, player.transform.position);
            line.SetPosition(1, transform.position);
        }

        //Aim arrow - only visible and relevant during boost
        if (isBoosted && boostArrow != null)
        {
            Vector2 dir = new Vector2(Mathf.Cos(aimAngle), Mathf.Sin(aimAngle));
            boostArrow.position = transform.position + (Vector3)(dir * boostArrowRadius);
            boostArrow.rotation = Quaternion.Euler(0f, 0f, (aimAngle * Mathf.Rad2Deg)+90);
        }
    }

    public void Interact(GameObject interactor)
    {
        //If another player is in a boosted state on this sphere, eject them first
        if (isBoosted) DoRelease();

        player = interactor;
        rb = player.GetComponent<Rigidbody>();
        line = player.GetComponent<LineRenderer>();

        Vector3 offset = player.transform.position - transform.position;
        angle = Mathf.Atan2(offset.y, offset.x);
        maxRadius = sphereCollider != null ? sphereCollider.radius : offset.magnitude;
        currentRadius = Mathf.Clamp(offset.magnitude, minRopeLength, maxRadius);

        //Seed angular velocity from actual incoming linear speed - no artificial jump
        Vector3 tangent = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f);
        float velAlong = Vector3.Dot(rb.linearVelocity, tangent);
        float swingDir = velAlong >= 0f ? 1f : -1f;

        angularVelocity = swingDir * (rb.linearVelocity.magnitude / currentRadius);
        entryAngularSpeed = Mathf.Abs(angularVelocity);

        //Entry speed is never suppressed - take whichever cap is higher
        float naturalCap = (maxSwingSpeed * Mathf.Deg2Rad * maxRadius) / currentRadius;
        currentSpeedCap = Mathf.Max(entryAngularSpeed, naturalCap);

        isSwinging = true;
        isBoosted = false;
        wasKinematic = rb.isKinematic;
        rb.isKinematic = true;

        if (swingMode == SwingMode.Reel)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                reelInAction = playerInput.actions.FindAction("Special Interact 1");
                reelOutAction = playerInput.actions.FindAction("Special Interact 2");
            }
            else
            {
                Debug.LogWarning("SwingController: No PlayerInput on interactor - reel will not function.");
            }
        }
    }

    public void Release(GameObject interactor)
    {
        //isBoosted no longer blocks release - it's the intended launch trigger
        if (!isSwinging) return;
        DoRelease();
    }

    private void DoRelease()
    {
        Rigidbody cachedRb = rb;
        bool cachedKinematic = wasKinematic;
        bool wasBoosted = isBoosted; // cache before CleanupSwing clears it

        FocusObject postLaunchFocus = (wasBoosted && refocusPlayerOnBoostLaunch)
            ? player.GetComponent<FocusObject>()
            : originalCamFocus;

        Vector3 exitVelocity;

        if (wasBoosted)
        {
            //Aim-directed launch: direction from arrow, speed magnitude preserved from orbit
            Vector2 launchDir = new Vector2(Mathf.Cos(aimAngle), Mathf.Sin(aimAngle));
            float speed = Mathf.Abs(angularVelocity) * currentRadius;
            exitVelocity = new Vector3(launchDir.x * speed, launchDir.y * speed, 0f);
        }
        else
        {
            //Normal release: eject along the current swing tangent
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            exitVelocity = tangent * (angularVelocity * currentRadius);
        }

        //Only restore camera focus if we actually changed it (i.e. during boost)
        if (wasBoosted)
            cam.SetFocus(postLaunchFocus);

        CleanupSwing();

        if (cachedRb == null) return;

        cachedRb.isKinematic = cachedKinematic;

        if (!cachedKinematic)
        {
            if (wasBoosted)
            {
                //Teleport to sphere centre so the velocity direction is exact,
                //eliminating any positional offset from the orbital arc.
                cachedRb.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    cachedRb.position.z //preserve Z - stay on 2.5D plane
                );
            }

            cachedRb.linearVelocity = exitVelocity;
        }
    }

    private void CleanupSwing()
    {
        isSwinging = false;
        isBoosted = false;
        entryAngularSpeed = 0f;
        currentSpeedCap = maxSwingSpeed * Mathf.Deg2Rad;
        originalCamFocus = null;

        if (line != null) line.enabled = false;
        if (boostArrow != null) boostArrow.gameObject.SetActive(false);

        player = null;
        rb = null;
        line = null;
        reelInAction = null;
        reelOutAction = null;
    }

    private void HandleReel()
    {
        if (reelInAction == null || reelOutAction == null) return;

        float delta = 0f;
        if (reelInAction.IsPressed()) delta = -reelInSpeed * Time.fixedDeltaTime;
        else if (reelOutAction.IsPressed()) delta = reelOutSpeed * Time.fixedDeltaTime;

        if (delta == 0f) return;

        float oldRadius = currentRadius;
        currentRadius = Mathf.Clamp(currentRadius + delta, minRopeLength, maxRadius);

        //Angular momentum conservation
        if (currentRadius > 0.001f && currentRadius != oldRadius)
            angularVelocity *= (oldRadius * oldRadius) / (currentRadius * currentRadius);
    }

    private void HandleBoost()
    {
        bool atMinRadius = currentRadius <= minRopeLength + 0.05f;
        float linearSpeed = Mathf.Abs(angularVelocity) * currentRadius;

        if (!isBoosted)
        {
            if (atMinRadius && linearSpeed >= boostSpeedThreshold)
            {
                Debug.Log("BoostActivated");
                isBoosted = true;
                currentRadius = minRopeLength;

                aimAngle = Mathf.PI * 0.5f;

                originalCamFocus = cam.GetFocus();
                cam.SetFocus(this.GetComponent<FocusObject>());

                if (boostArrow != null)
                {
                    boostArrow.gameObject.SetActive(true);
                }
            }
            return;
        }

        currentRadius = minRopeLength;

        float boostCapRad = boostMaxSpeed * Mathf.Deg2Rad;
        currentSpeedCap = Mathf.MoveTowards(
            currentSpeedCap,
            boostCapRad,
            boostAcceleration * Mathf.Deg2Rad * Time.fixedDeltaTime
        );

        angularVelocity = Mathf.MoveTowards(
            angularVelocity,
            Mathf.Sign(angularVelocity) * currentSpeedCap,
            boostAcceleration * Mathf.Deg2Rad * Time.fixedDeltaTime
        );
    }

    //Repurposes the reel buttons for aim direction while boosted.
    //reelIn -> rotate aim counter-clockwise
    //reelOut -> rotate aim clockwise

    private void HandleBoostAim()
    {
        if (reelInAction == null || reelOutAction == null) return;

        float input = 0f;
        if (reelInAction.IsPressed()) input += 1f;
        if (reelOutAction.IsPressed()) input -= 1f;

        aimAngle += input * boostAimSpeed * Time.fixedDeltaTime;
    }
}
