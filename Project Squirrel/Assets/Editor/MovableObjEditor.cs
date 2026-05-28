#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovableObj))]
public class MovableObjEditor : Editor
{
    //Mode
    SerializedProperty mode;

    //Anchor
    SerializedProperty anchorPoint;
    SerializedProperty targetRotation;

    //Translate
    SerializedProperty targetPosition;

    //Passenger
    SerializedProperty playerTag;
    SerializedProperty carriersPassengers;

    //Settings
    SerializedProperty delay;
    SerializedProperty duration;
    SerializedProperty isLoop;
    SerializedProperty easeCurve;

    private void OnEnable()
    {
        mode               = serializedObject.FindProperty("mode");
        anchorPoint        = serializedObject.FindProperty("anchorPoint");
        targetRotation     = serializedObject.FindProperty("targetRotation");
        targetPosition     = serializedObject.FindProperty("targetPosition");
        playerTag          = serializedObject.FindProperty("playerTag");
        carriersPassengers = serializedObject.FindProperty("carriersPassengers");
        delay              = serializedObject.FindProperty("delay");
        duration           = serializedObject.FindProperty("duration");
        isLoop             = serializedObject.FindProperty("isLoop");
        easeCurve          = serializedObject.FindProperty("easeCurve");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mode);

        EditorGUILayout.Space();

        MovableObj.MovementMode currentMode = (MovableObj.MovementMode)mode.enumValueIndex;

        if (currentMode == MovableObj.MovementMode.RotateAroundAnchor)
        {
            EditorGUILayout.LabelField("Anchor", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(anchorPoint);
            EditorGUILayout.PropertyField(targetRotation);
        }
        else if (currentMode == MovableObj.MovementMode.Translate)
        {
            EditorGUILayout.LabelField("Platform", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(targetPosition);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Platform Passengers", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerTag);
        EditorGUILayout.PropertyField(carriersPassengers);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(delay);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(isLoop);
        EditorGUILayout.PropertyField(easeCurve);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif