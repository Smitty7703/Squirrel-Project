using UnityEngine;

public class FocusObject : MonoBehaviour
{
    public enum FocusType { Level, Player, Object }
    public FocusType type;
    public Transform focusTransform;
    public float orthoZoom; //How zoomed in the camera should be
    public float transDuration; //How long camera takes to transition to this object
    public Vector3 offset;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool shouldFollow;
}
