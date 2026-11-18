using UnityEngine;
public sealed class AtributosZona : MonoBehaviour
{
    public int ID = 1;
    public int FASE = 1;
    public GameObject bosqueardienteContenedorGameObjects;
    public GameObject pasovientoheladoContenedorGameObjects;
    public GameObject BosqueAngustiante_Llama;
    public MeshRenderer TexturaTerreno, TexturaTerrenoExtension;
}
public sealed class Nodo : MonoBehaviour { }
public sealed class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance;
    public MapaManager scMapaManager;
    public float hora = 13;
    public float ObtenerHoraActual() => hora;
}
public sealed class MapaManager : MonoBehaviour { public float ObtenerPasoMapa() => 5; }
// El fixture no activa el Paso ni su lógica: solo satisface referencias del decorador y del sol reales.
public sealed class AdornosPasoVientoHelado : MonoBehaviour
{
    public void RetirarSobreCamino(LineRenderer l, float margen, bool forzar = false) { }
}
public sealed class AmbientePasoVientoHelado : MonoBehaviour
{
    public void ArmonizarDecoracion(GameObject o) { }
    public static void EvaluarHora(float hora, out float intensidad, out Color color, out Color ambiente)
    { intensidad = 1; color = ambiente = Color.white; }
}
public static class FondoPasoVientoHelado { public static void ActualizarSueloAldea(AtributosZona z, Mesh m) { } }
public static class ClimasPasoVientoHelado { public static float CoberturaLuz => 0; }
public sealed class CaminoMesh : MonoBehaviour
{
    public bool VisibleParaDecoracion => true;
    public float GetWidth() => .4f;
}
