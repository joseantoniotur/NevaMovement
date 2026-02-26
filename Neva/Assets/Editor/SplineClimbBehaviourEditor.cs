using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineClimbBehaviour))]
public class SplineClimbBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        SplineClimbBehaviour spline = (SplineClimbBehaviour)target;

        GUILayout.Space(20);

        if (GUILayout.Button("UPDATE SPLINE"))
        {
            spline.UpdateCollider();
            spline.UpdateVissuals();
        }
    }
}