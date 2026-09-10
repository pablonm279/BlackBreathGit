using UnityEngine;

[CreateAssetMenu(menuName = "Combate/Ajustes de campo vivo")]
public sealed class AjustesCampoVivo : ScriptableObject
{
  [Tooltip("Desmarcar restaura el ambiente anterior. No modifica unidades, escenas ni partidas.")]
  public bool activo = true;
  public bool atmosfera = true;
  public bool suelo = true;
  public bool desplazamientos = true;
  public bool impactos = true;
  [Range(0.1f, 1.5f)] public float intensidad = 1f;
  [Range(0.25f, 1.5f)] public float densidad = 1f;
  [Tooltip("Referencias del fondo quemado: funcionan tambien cuando el material es una instancia.")]
  public Texture[] fondosQuemados = new Texture[0];

  private static AjustesCampoVivo instancia;
  private static bool? overrideActivo;
  public static AjustesCampoVivo Actual
  {
    get
    {
      if (instancia == null) instancia = Resources.Load<AjustesCampoVivo>("CampoVivo/AjustesCampoVivo");
      return instancia;
    }
  }
  public static bool Activo => Actual != null && (overrideActivo ?? Actual.activo) && BattleVisualJuice.Enabled;
  // Comparacion temporal en Play Mode; el asset controla el valor del juego y del build.
  public static void Comparar(bool? activo) { overrideActivo = activo; }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void Reiniciar() { instancia = null; overrideActivo = null; }
}
