using System.Collections;
using UnityEngine;

public class MovableObj : Activatable
{
    public enum MovementMode { RotateAroundAnchor, Translate }

    //Mode
    [SerializeField] private MovementMode mode = MovementMode.RotateAroundAnchor;

    //Anchor Rotate Only
    [Tooltip("Assign a child GameObject to use as the hinge point. Position it freely in the Scene view.")]
    [SerializeField] private Transform anchorPoint;
    [SerializeField] private Vector3 targetRotation;

    //Platform Translate Only
    [Tooltip("World-space position the object moves toward when activated.")]
    [SerializeField] private Vector3 targetPosition;

    //Passenger Specific Settings
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool carriersPassengers = false;

    //Settings
    [SerializeField] private float delay;
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool isLoop = false;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Vector3 _anchorWorld;
    private Vector3 _anchorLocalOffset;

    public Vector3 Velocity { get; private set; }
    private Vector3 _previousPosition;
    private bool isMoving;

    private Coroutine _activeCoroutine;

    private void Start()
    {
        _originPosition = transform.position;
        _originRotation = transform.rotation;
        _anchorWorld = anchorPoint != null ? anchorPoint.position : transform.position;
    }

    public override void SetActiveStatus(Button Button, bool IsActive)
    {
        if (IsActive)
        {
            Play();
        }
        else
        {
            if (isLoop)
            {
                StopAllCoroutines();
                _activeCoroutine = null;
            }
            else
            {
                ReturnToOrigin();
            }
        }
    }

    public void Play()
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(AnimateTransform());
    }

    public void ReturnToOrigin()
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = mode == MovementMode.RotateAroundAnchor
            ? StartCoroutine(LerpAroundAnchor(transform.rotation, _originRotation, duration))
            : StartCoroutine(LerpPosition(transform.position, _originPosition, duration));
    }

    #region Passenger Logic
    private void OnCollisionEnter(Collision col)
    {
        if (!carriersPassengers) return;
        if (mode != MovementMode.Translate) return;
        Debug.Log("Passenger Collision Triggered");
        if (col.gameObject.CompareTag(playerTag))
            MountPassenger(col.transform);
    }

    private void OnCollisionExit(Collision col)
    {
        if (!carriersPassengers) return;
        if (col.gameObject.CompareTag(playerTag))
            DismountPassenger(col.transform);
    }

    private void MountPassenger(Transform passenger)
    {
        passenger.SetParent(transform, worldPositionStays: true);
        passenger.GetComponent<PlayerController>()?.OnMountPlatform(this);
    }

    private void DismountPassenger(Transform passenger)
    {
        passenger.SetParent(null, worldPositionStays: true);
        passenger.GetComponent<PlayerController>()?.OnDismountPlatform();
    }
    #endregion

    #region Animate
    private IEnumerator AnimateTransform()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        isMoving = true;

        if (mode == MovementMode.RotateAroundAnchor)
            yield return StartCoroutine(AnimateRotation());
        else
            yield return StartCoroutine(AnimateTranslation());

        isMoving = false;
        Velocity = Vector3.zero;
        _activeCoroutine = null;
    }

    private void LateUpdate()
    {
        if (!isMoving) return;
        Velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    private IEnumerator AnimateRotation()
    {
        Quaternion targetRot = Quaternion.Euler(targetRotation);

        yield return StartCoroutine(LerpAroundAnchor(_originRotation, targetRot, duration));

        while (isLoop)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(LerpAroundAnchor(targetRot, _originRotation, duration));
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(LerpAroundAnchor(_originRotation, targetRot, duration));
        }
    }

    private IEnumerator AnimateTranslation()
    {
        yield return StartCoroutine(LerpPosition(_originPosition, targetPosition, duration));

        while (isLoop)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(LerpPosition(targetPosition, _originPosition, duration));
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(LerpPosition(_originPosition, targetPosition, duration));
        }
    }
    #endregion

    #region Lerp Primitives
    private IEnumerator LerpAroundAnchor(Quaternion fromRot, Quaternion toRot, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            float t = easeCurve.Evaluate(elapsed / dur);
            ApplyRotationAroundAnchor(Quaternion.Lerp(fromRot, toRot, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        ApplyRotationAroundAnchor(toRot);
    }

    private IEnumerator LerpPosition(Vector3 from, Vector3 to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            float t = easeCurve.Evaluate(elapsed / dur);
            transform.position = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
    }

    private void ApplyRotationAroundAnchor(Quaternion rotation)
    {
        Quaternion deltaRotation = rotation * Quaternion.Inverse(_originRotation);

        transform.rotation = rotation;

        transform.position = _anchorWorld + deltaRotation * (_originPosition - _anchorWorld);
    }
    #endregion
}
