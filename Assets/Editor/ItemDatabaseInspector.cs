using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseInspector : Editor
{
    private readonly Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawToolbar();
        EditorGUILayout.Space(6f);

        SerializedProperty itemsProp = serializedObject.FindProperty("items");
        if (itemsProp == null)
        {
            EditorGUILayout.HelpBox("No se encontro la propiedad 'items'.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        if (itemsProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox(
                "La base esta vacia. Usa Tools/Items/Create Or Refresh Item Database para cargar prefabs.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        Dictionary<string, List<int>> groups = BuildGroups(itemsProp);

        List<string> categories = new List<string>(groups.Keys);
        categories.Sort((a, b) => string.Compare(a, b, true));

        foreach (string category in categories)
        {
            List<int> indices = groups[category];

            bool isOpen = GetCategoryFoldout(category);
            EditorGUILayout.BeginHorizontal();
            isOpen = EditorGUILayout.Foldout(isOpen, $"{category} ({indices.Count})", true);
            if (GUILayout.Button("Agregar Item", GUILayout.Width(110f)))
            {
                ItemDatabaseTools.CreatePrefabFromCategoryTemplate(category);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            categoryFoldouts[category] = isOpen;

            if (!isOpen)
            {
                continue;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                SerializedProperty element = itemsProp.GetArrayElementAtIndex(index);
                string label = BuildEntryLabel(element, index);
                EditorGUILayout.PropertyField(element, new GUIContent(label), true);
                EditorGUILayout.Space(4f);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2f);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.HelpBox(
            "Vista agrupada por categoria. Para sincronizar usa los botones de abajo.",
            MessageType.None);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Database"))
        {
            ItemDatabaseTools.CreateOrRefreshItemDatabase();
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Sync From Prefabs"))
        {
            ItemDatabaseTools.SyncDatabaseFromPrefabsOverwriteFields();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply DB -> Prefabs"))
        {
            ItemDatabaseTools.ApplyDatabaseToPrefabs();
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Open Asset"))
        {
            ItemDatabaseTools.OpenItemDatabase();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();
    }

    private Dictionary<string, List<int>> BuildGroups(SerializedProperty itemsProp)
    {
        Dictionary<string, List<int>> groups = new Dictionary<string, List<int>>();

        for (int i = 0; i < itemsProp.arraySize; i++)
        {
            SerializedProperty element = itemsProp.GetArrayElementAtIndex(i);
            SerializedProperty categoryProp = element.FindPropertyRelative("categoria");
            string category = categoryProp != null ? categoryProp.stringValue : string.Empty;
            if (string.IsNullOrWhiteSpace(category))
            {
                category = "Item";
            }

            if (!groups.TryGetValue(category, out List<int> list))
            {
                list = new List<int>();
                groups.Add(category, list);
            }

            list.Add(i);
        }

        return groups;
    }

    private string BuildEntryLabel(SerializedProperty element, int fallbackIndex)
    {
        SerializedProperty titleProp = element.FindPropertyRelative("listaTitulo");
        if (titleProp != null && !string.IsNullOrWhiteSpace(titleProp.stringValue))
        {
            return titleProp.stringValue;
        }

        SerializedProperty categoryProp = element.FindPropertyRelative("categoria");
        SerializedProperty nameProp = element.FindPropertyRelative("nombre");

        string category = categoryProp != null ? categoryProp.stringValue : "Item";
        string name = nameProp != null ? nameProp.stringValue : string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"Entry {fallbackIndex + 1}";
        }

        return $"{category} - {name}";
    }

    private bool GetCategoryFoldout(string category)
    {
        if (!categoryFoldouts.TryGetValue(category, out bool value))
        {
            value = true;
            categoryFoldouts[category] = value;
        }

        return value;
    }
}
