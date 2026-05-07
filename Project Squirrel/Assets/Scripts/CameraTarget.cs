using System.Collections;
using UnityEngine;
public class CameraTarget : MonoBehaviour
{
    public Transform focusObject;    // current gameplay focus

    public Transform player;
    public Rigidbody playerRb;

    public Camera cam;

    [SerializeField] private float transDuration;
    bool transitioning;
    private Vector3 camZeroPos = new Vector3(0, 0, -10);

    [SerializeField] private float minZoom = 6f;
    [SerializeField] private float maxZoom = 12f;
    [SerializeField] private float zoomSpeedThreshold = 12f;
    [SerializeField] private float zoomSmooth = 3f;
    [SerializeField] private float maxSpeedForZoom = 30f;
    private void Awake()
    {
        transform.SetParent(focusObject);
        transform.localPosition = camZeroPos;
    }

    private void LateUpdate()
    {
        HandleZoom();
    }
    public void SetFocus(Transform newFocus)
    {
        if (focusObject == newFocus) return;

        StopAllCoroutines();
        focusObject = newFocus;
        transform.SetParent(focusObject);
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        float elapsed = 0f; float xPos; float yPos; float t;
        while (elapsed < transDuration)
        {
            elapsed += Time.deltaTime; t = elapsed / transDuration;
            xPos = Mathf.Lerp(transform.localPosition.x, 0f, t);
            yPos = Mathf.Lerp(transform.localPosition.y, 0f, t);
            transform.localPosition = new Vector3(xPos, yPos, camZeroPos.z);
            yield return null;
        }
        transform.localPosition = camZeroPos;
    }

    private void HandleZoom()
    {
        float targetZoom = minZoom;

        if (focusObject == player)
        {
            float speed = playerRb.linearVelocity.magnitude;

            if (speed > zoomSpeedThreshold)
            {
                float percent = Mathf.Clamp01(speed / maxSpeedForZoom);
                targetZoom = Mathf.Lerp(minZoom, maxZoom, percent);
            }
        }

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSmooth * Time.deltaTime
        );
    }
}
