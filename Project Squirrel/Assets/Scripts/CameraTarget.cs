using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraTarget : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private FocusObject currentFocus;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void SetFocus(FocusObject newFocus)
    {
        if (newFocus == null)
        {
            Debug.LogWarning("CameraController: SetFocus called with a null FocusObject.");
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        currentFocus = newFocus;
        transitionRoutine = StartCoroutine(TransitionToFocus(newFocus));
    }

    public FocusObject GetFocus()
    {
        return currentFocus;
    }

    private IEnumerator TransitionToFocus(FocusObject focus)
    {
        Vector3 startPosition = transform.position;
        float startZoom = cam.orthographicSize;

        Vector3 targetPosition = GetTargetPosition(focus);
        float elapsed = 0f;

        while (elapsed < focus.transDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / focus.transDuration);
            float curveT = focus.transitionCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, curveT);
            cam.orthographicSize = Mathf.Lerp(startZoom, focus.orthoZoom, curveT);

            if (focus.shouldFollow)
                targetPosition = GetTargetPosition(focus);

            yield return null;
        }

        transform.position = targetPosition;
        cam.orthographicSize = focus.orthoZoom;
        transitionRoutine = null;
    }

    private void LateUpdate()
    {
        //Only run continuous follow logic outside of an active transition
        if (currentFocus == null || !currentFocus.shouldFollow || transitionRoutine != null)
            return;

        transform.position = GetTargetPosition(currentFocus);
    }

    private Vector3 GetTargetPosition(FocusObject focus)
    {
        Vector3 target = focus.transform.position + focus.offset;
        target.z = transform.position.z;
        return target;
    }
}
