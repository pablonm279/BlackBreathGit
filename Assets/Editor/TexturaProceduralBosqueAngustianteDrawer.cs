using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TexturaProceduralBosqueAngustianteConfig))]
public class TexturaProceduralBosqueAngustianteDrawer : PropertyDrawer
{
   const float Separacion = 2f;
   const int FilasDesplegadas = 32;

   public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
   {
      return property.isExpanded
         ? FilasDesplegadas * (EditorGUIUtility.singleLineHeight + Separacion)
         : EditorGUIUtility.singleLineHeight;
   }

   public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
   {
      EditorGUI.BeginProperty(position, label, property);
      Rect fila = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
      property.isExpanded = EditorGUI.Foldout(fila, property.isExpanded, label, true);

      if (!property.isExpanded)
      {
         EditorGUI.EndProperty();
         return;
      }

      EditorGUI.indentLevel++;
      Siguiente(ref fila);
      Rect ayuda = fila;
      ayuda.height = EditorGUIUtility.singleLineHeight * 2f;
      EditorGUI.HelpBox(ayuda, "Construye la identidad del suelo combinando ceniza, carbon, tierra caliente, brasas y cicatrices.", MessageType.Info);
      Siguiente(ref fila, 2);

      Titulo(ref fila, "PERFIL RAPIDO (OPCIONAL)");
      Rect botones = fila;
      float ancho = (botones.width - 8f) / 3f;
      if (GUI.Button(new Rect(botones.x, botones.y, ancho, botones.height), "Ceniza")) AplicarPreset(property, 0);
      if (GUI.Button(new Rect(botones.x + ancho + 4f, botones.y, ancho, botones.height), "Carbon")) AplicarPreset(property, 1);
      if (GUI.Button(new Rect(botones.x + (ancho + 4f) * 2f, botones.y, ancho, botones.height), "Brasas")) AplicarPreset(property, 2);
      Siguiente(ref fila);

      Titulo(ref fila, "SALIDA");
      Campo(ref fila, property, "tamanoTextura", "Resolucion");
      Campo(ref fila, property, "tintMaterial", "Tinte final");

      Titulo(ref fila, "PALETA");
      Campo(ref fila, property, "tierraQuemadaBase", "Suelo quemado");
      Campo(ref fila, property, "cenizaClara", "Ceniza");
      Campo(ref fila, property, "carbonOscuro", "Carbon");
      Campo(ref fila, property, "tierraRojiza", "Tierra caliente");
      Campo(ref fila, property, "brasaApagada", "Brasa");

      Titulo(ref fila, "PRESENCIA VISUAL");
      Campo(ref fila, property, "intensidadCarbon", "Cantidad de carbon");
      Campo(ref fila, property, "intensidadCeniza", "Cantidad de ceniza");
      Campo(ref fila, property, "intensidadRojiza", "Calor de la tierra");
      Campo(ref fila, property, "intensidadBrasas", "Brasas visibles");
      Campo(ref fila, property, "intensidadVetas", "Cicatrices quemadas");
      Campo(ref fila, property, "intensidadGrano", "Rugosidad fina");

      Titulo(ref fila, "FORMA (BAJO = GRANDE / ALTO = PEQUENO)");
      Campo(ref fila, property, "escalaManchasGrandes", "Frecuencia de manchas");
      Campo(ref fila, property, "escalaCenizaMedia", "Frecuencia de ceniza");
      Campo(ref fila, property, "escalaVetasQuemadas", "Frecuencia de cicatrices");
      Campo(ref fila, property, "escalaVetasY", "Cortes en las cicatrices");
      Campo(ref fila, property, "escalaGrano", "Frecuencia del grano");

      Titulo(ref fila, "IRREGULARIDAD");
      Campo(ref fila, property, "intensidadWarp", "Deformacion de formas");
      Campo(ref fila, property, "escalaWarpX", "Variacion horizontal");
      Campo(ref fila, property, "escalaWarpY", "Variacion vertical");

      if (Application.isPlaying && property.serializedObject.targetObject is AtributosZona zona)
      {
         if (GUI.Button(fila, "Aplicar al terreno ahora"))
         {
            property.serializedObject.ApplyModifiedProperties();
            zona.ReaplicarTexturaBosqueArdiente();
         }
      }

      EditorGUI.indentLevel--;
      EditorGUI.EndProperty();
   }

   static void Titulo(ref Rect fila, string texto)
   {
      EditorGUI.LabelField(fila, texto, EditorStyles.boldLabel);
      Siguiente(ref fila);
   }

   static void Campo(ref Rect fila, SerializedProperty raiz, string nombre, string etiqueta)
   {
      SerializedProperty campo = raiz.FindPropertyRelative(nombre);
      EditorGUI.PropertyField(fila, campo, new GUIContent(etiqueta, campo.tooltip));
      Siguiente(ref fila);
   }

   static void AplicarPreset(SerializedProperty raiz, int preset)
   {
      raiz.FindPropertyRelative("intensidadCeniza").floatValue = preset == 0 ? 0.62f : preset == 1 ? 0.24f : 0.34f;
      raiz.FindPropertyRelative("intensidadCarbon").floatValue = preset == 0 ? 0.38f : preset == 1 ? 0.72f : 0.50f;
      raiz.FindPropertyRelative("intensidadRojiza").floatValue = preset == 0 ? 0.12f : preset == 1 ? 0.16f : 0.42f;
      raiz.FindPropertyRelative("intensidadBrasas").floatValue = preset == 0 ? 0.06f : preset == 1 ? 0.10f : 0.34f;
      raiz.FindPropertyRelative("intensidadVetas").floatValue = preset == 0 ? 0.18f : preset == 1 ? 0.38f : 0.44f;
      raiz.FindPropertyRelative("escalaManchasGrandes").floatValue = preset == 0 ? 4.8f : preset == 1 ? 3.2f : 4.0f;
      raiz.FindPropertyRelative("escalaCenizaMedia").floatValue = preset == 0 ? 8f : preset == 1 ? 10f : 12f;
      raiz.FindPropertyRelative("escalaVetasQuemadas").floatValue = preset == 0 ? 16f : preset == 1 ? 13f : 10f;
      raiz.FindPropertyRelative("intensidadWarp").floatValue = preset == 0 ? 0.07f : preset == 1 ? 0.12f : 0.15f;
   }

   static void Siguiente(ref Rect fila, int cantidad = 1)
   {
      fila.y += cantidad * (EditorGUIUtility.singleLineHeight + Separacion);
   }
}
