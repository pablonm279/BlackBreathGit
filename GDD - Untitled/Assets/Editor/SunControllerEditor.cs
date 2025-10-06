#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SunController))]
public class SunControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Controles de Depuración", EditorStyles.boldLabel);

        var controllers = new System.Collections.Generic.List<SunController>();
        foreach (var t in targets)
        {
            if (t is SunController sc && sc != null)
                controllers.Add(sc);
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Avanzar Sol (Play Mode)"))
            {
                foreach (var sc in controllers)
                    sc.OnTravelStart();
            }
            if (GUILayout.Button("Reset Sol (Play Mode)"))
            {
                foreach (var sc in controllers)
                    sc.ResetSun();
            }
            EditorGUILayout.EndHorizontal();
        }

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Step Instantáneo (Edit Mode)"))
            {
                foreach (var sc in controllers)
                {
                    Undo.RecordObject(sc.transform, "Step Instantáneo Sol");
                    sc.EditorStepInstant();
                }
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
