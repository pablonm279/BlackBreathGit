using System;
using System.Collections.Generic;
using System.IO;

// Dobles mínimos para probar selección de textos en .NET. No simulan escenas, audio ni renderizado.
namespace UnityEngine
{
    public class Object
    {
        public static void Destroy(Object value) { }
        public static void DontDestroyOnLoad(Object value) { }
        public static T[] FindObjectsOfType<T>(bool includeInactive) => Array.Empty<T>();
        public static T FindFirstObjectByType<T>(FindObjectsInactive inactive) where T : class => null;
    }
    public class MonoBehaviour : Object
    {
        public Object gameObject = new Object();
        public object associatedComponent;
        public bool enabled = true;
        public T GetComponent<T>() where T : class => associatedComponent as T;
        public void Invoke(string method, float delay) { }
    }
    public class ScriptableObject : Object { }
    public class TextAsset { public string text; }
    public enum RuntimeInitializeLoadType { SubsystemRegistration, AfterAssembliesLoaded }
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { }
    }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute(int min, int max) { } }
    public class DisallowMultipleComponent : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string value) { } }
    public class InspectorNameAttribute : Attribute { public InspectorNameAttribute(string value) { } }
    public class SerializeField : Attribute { }
    public enum FindObjectsInactive { Include }
    public class CreateAssetMenuAttribute : Attribute { public string menuName; public string fileName; }
    public enum SystemLanguage { Spanish, English, Portuguese, ChineseSimplified, ChineseTraditional, Chinese, Unknown }
    public static class Application { public static bool isEditor = true; public static SystemLanguage systemLanguage; }
    public static class Debug
    {
        public static bool isDebugBuild;
        public static readonly List<string> errors = new();
        public static void LogError(string message) => errors.Add(message);
        public static void LogError(string message, object context) => errors.Add(message);
    }
    public static class PlayerPrefs
    {
        public static readonly Dictionary<string, int> values = new();
        public static bool HasKey(string key) => values.ContainsKey(key);
        public static int GetInt(string key, int fallback = 0) => values.GetValueOrDefault(key, fallback);
        public static void SetInt(string key, int value) => values[key] = value;
        public static void Save() { }
    }
    public static class Resources
    {
        public static string root;
        public static T Load<T>(string path) where T : class => new TextAsset
        {
            text = File.ReadAllText(Path.Combine(root, "Assets/Resources", path + ".json"))
        } as T;
    }
    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json,
            new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
    }
}

namespace TMPro
{
    public class TMP_Text { public string text; }
    public class TextMeshProUGUI : TMP_Text { }
}

namespace UnityEngine.EventSystems
{
    public class PointerEventData { }
    public interface IPointerEnterHandler { void OnPointerEnter(PointerEventData eventData); }
    public interface IPointerExitHandler { void OnPointerExit(PointerEventData eventData); }
}

public class PrePartidaTooltipManager
{
    public void Mostrar(PrePartidaTooltip owner) { }
    public void Ocultar(PrePartidaTooltip owner) { }
}
