using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BotConfig))]
public class BotConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Fetch current target object
        BotConfig config = (BotConfig) target;

        // Ensure any serialized object changes are tracked
        serializedObject.Update();

        // Adding header
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);

        // Draw the custom sliders
        DrawMinMaxSlider(ref config.refreshPositionTimeRange, 0f, 5f, "Refresh Position Time");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minDistToPlayer"), new GUIContent("Min Dist To Players"));

        // Manually draw other properties here if needed, excluding the custom-handled Vector2 fields
        EditorGUILayout.Space(); // Adds some spacing
        EditorGUILayout.LabelField("Ball", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockChance"), new GUIContent("Block Chance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetClosestPlayerChance"), new GUIContent("Target Closest Player Chance"));

        // Apply any changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawMinMaxSlider(ref Vector2 range, float minLimit, float maxLimit, string label)
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField(label);
        EditorGUILayout.BeginHorizontal();
        range.x = Mathf.Clamp(EditorGUILayout.FloatField(range.x, GUILayout.Width(50)), minLimit, range.y);
        EditorGUILayout.MinMaxSlider(ref range.x, ref range.y, minLimit, maxLimit);
        range.y = Mathf.Clamp(EditorGUILayout.FloatField(range.y, GUILayout.Width(50)), range.x, maxLimit);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}