#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SwingController))]
public class SwingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Always visible ────────────────────────────────────────────────────
        SerializedProperty modeProp = serializedObject.FindProperty("swingMode");
        SerializedProperty camProp = serializedObject.FindProperty("cam");

        // ── Swing ─────────────────────────────────────────────────────────────
        SerializedProperty initialSpeedProp = serializedObject.FindProperty("initialSwingSpeed");
        SerializedProperty maxSpeedProp = serializedObject.FindProperty("maxSwingSpeed");
        SerializedProperty accelProp = serializedObject.FindProperty("swingAcceleration");

        // ── Reel ──────────────────────────────────────────────────────────────
        SerializedProperty reelInSpeedProp = serializedObject.FindProperty("reelInSpeed");
        SerializedProperty reelOutSpeedProp = serializedObject.FindProperty("reelOutSpeed");
        SerializedProperty minRopeProp = serializedObject.FindProperty("minRopeLength");

        // ── Boost ─────────────────────────────────────────────────────────────
        SerializedProperty boostThreshProp = serializedObject.FindProperty("boostSpeedThreshold");
        SerializedProperty boostMaxProp = serializedObject.FindProperty("boostMaxSpeed");
        SerializedProperty boostAccelProp = serializedObject.FindProperty("boostAcceleration");

        // ── Boost Aim ─────────────────────────────────────────────────────────
        SerializedProperty arrowPrefabProp = serializedObject.FindProperty("arrowPrefab");
        SerializedProperty boostArrowRadiusProp = serializedObject.FindProperty("boostArrowRadius");
        SerializedProperty boostAimSpeedProp = serializedObject.FindProperty("boostAimSpeed");
        SerializedProperty refocusPlayerProp = serializedObject.FindProperty("refocusPlayerOnBoostLaunch");

        // ── Draw ──────────────────────────────────────────────────────────────
        EditorGUILayout.PropertyField(modeProp);
        EditorGUILayout.PropertyField(camProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Swing Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(initialSpeedProp);
        EditorGUILayout.PropertyField(maxSpeedProp);
        EditorGUILayout.PropertyField(accelProp);

        if ((SwingMode)modeProp.enumValueIndex == SwingMode.Reel)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reel Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(reelInSpeedProp);
            EditorGUILayout.PropertyField(reelOutSpeedProp);
            EditorGUILayout.PropertyField(minRopeProp);
            EditorGUILayout.HelpBox(
                "Max rope length is derived from the object's Sphere Collider radius at attach time.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Boost Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(boostThreshProp);
            EditorGUILayout.PropertyField(boostMaxProp);
            EditorGUILayout.PropertyField(boostAccelProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Boost Aim Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(arrowPrefabProp);

            // Only show numeric fields when an arrow is actually assigned —
            // they have no effect without one
            if (arrowPrefabProp.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(boostArrowRadiusProp);
                EditorGUILayout.PropertyField(boostAimSpeedProp);
                EditorGUILayout.PropertyField(refocusPlayerProp);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Assign an arrow Transform to configure aim radius and speed.",
                    MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif