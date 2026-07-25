using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrePartidaManager))]
public class PrePartidaManagerEditor : Editor
{
    private int presagio1Id;
    private int presagio2Id;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Debug corrupción", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            PrePartidaManager manager = (PrePartidaManager)target;

            if (GUILayout.Button("Sumar corrupción"))
            {
                manager.DebugSumarCorrupcion();
                Repaint();
            }

            if (GUILayout.Button("Restar corrupción"))
            {
                manager.DebugRestarCorrupcion();
                Repaint();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Debug presagios", EditorStyles.boldLabel);
            DibujarSelectorPresagios(manager);
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Estos botones se habilitan durante Play Mode.", MessageType.Info);
        }
    }

    private void DibujarSelectorPresagios(PrePartidaManager manager)
    {
        List<PresagioDefinition> disponibles =
            PresagioCatalog.ObtenerDisponiblesParaDebug(manager.DebugObtenerZonaSeleccionada());
        List<int> ids = new List<int> { 0 };
        List<string> nombres = new List<string> { "— Ninguno —" };

        for (int i = 0; i < disponibles.Count; i++)
        {
            PresagioDefinition definicion = disponibles[i];
            ids.Add(definicion.id);
            nombres.Add(
                "[" + definicion.categoria + "] "
                + (definicion.positivo ? "+ " : "− ")
                + PresagioCatalog.ObtenerNombreLocalizado(definicion.id));
        }

        int indice1 = Mathf.Max(0, ids.IndexOf(presagio1Id));
        int indice2 = Mathf.Max(0, ids.IndexOf(presagio2Id));
        indice1 = EditorGUILayout.Popup("Presagio 1", indice1, nombres.ToArray());
        indice2 = EditorGUILayout.Popup("Presagio 2", indice2, nombres.ToArray());
        presagio1Id = ids[indice1];
        presagio2Id = ids[indice2];

        if (GUILayout.Button("Forzar presagios seleccionados"))
        {
            manager.DebugForzarPresagios(presagio1Id, presagio2Id);
            Repaint();
        }

        if (GUILayout.Button("Sortear nuevamente"))
        {
            manager.DebugSortearNuevamentePresagios();
            Repaint();
        }
    }
}
