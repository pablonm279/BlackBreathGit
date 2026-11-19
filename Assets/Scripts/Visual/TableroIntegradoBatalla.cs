using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Solo presentacion: reutiliza las capas que Casilla ya activa y no escribe estados de combate.
[DisallowMultipleComponent]
[DefaultExecutionOrder(710)]
public sealed class TableroIntegradoBatalla : MonoBehaviour
{
  private static readonly int ColorId = Shader.PropertyToID("_Color");
  private readonly List<CambioMaterial> cambios = new List<CambioMaterial>();
  private readonly List<KeyValuePair<Renderer, bool>> sueloOriginal = new List<KeyValuePair<Renderer, bool>>();
  private readonly List<Material> materiales = new List<Material>();
  private readonly Dictionary<int, Material> bordes = new Dictionary<int, Material>();
  private readonly List<CambioMalla> cambiosMalla = new List<CambioMalla>();
  private readonly Dictionary<Mesh, Mesh> planosBorde = new Dictionary<Mesh, Mesh>();
  private readonly List<GameObject> fondosVida = new List<GameObject>();
  private readonly List<BarraObstaculo> barrasObstaculos = new List<BarraObstaculo>();
  private readonly HashSet<Obstaculo> obstaculosVistos = new HashSet<Obstaculo>();
  private readonly List<SombraObstaculo> sombrasObstaculos = new List<SombraObstaculo>();
  private MaterialPropertyBlock bloqueFondo;
  private IReadOnlyList<Casilla> casillas;
  private MeshRenderer fondo;
  private Camera camaraBarras;
  private Material materialSuelo;
  private Shader shaderBorde;
  private Mesh malla;
  private GameObject raiz;
  private EncounterZoneType zona;
  private bool configurado, subterraneo, aplicado;
  private float revisarObstaculos;

  private sealed class CambioMaterial
  {
    public Renderer renderer;
    public Material[] anterior, nuevo;
    public bool habilitado, ocultar;
  }

  private sealed class CambioMalla
  {
    public MeshFilter filtro;
    public Mesh anterior, nueva;
  }

  private sealed class SombraObstaculo
  {
    public Casilla casilla;
    public Obstaculo obstaculo;
    public GameObject objeto;
  }

  private sealed class BarraObstaculo
  {
    public Slider slider;
    public Graphic[] graficos;
    public bool[] habilitados;
    public Mesh mesh;
    public Transform transform;
    public float ancho;
    public readonly Vector3[] vertices = new Vector3[8];
  }

  public bool Preparado => raiz != null;

  public static bool Soporta(EncounterZoneType tipoZona, bool bajoTierra)
  {
    return !bajoTierra && (tipoZona == EncounterZoneType.PasoVientoHelado
      || tipoZona == EncounterZoneType.BosqueAngustiante || tipoZona == EncounterZoneType.Nedukazal);
  }

  public static void ConfigurarEn(BattleAmbientLife ambiente, MeshRenderer rendererFondo, EncounterZoneType tipoZona, bool bajoTierra)
  {
    var tablero = ambiente.GetComponent<TableroIntegradoBatalla>();
    if (tablero == null) tablero = ambiente.gameObject.AddComponent<TableroIntegradoBatalla>();
    tablero.Limpiar();
    tablero.fondo = rendererFondo;
    tablero.zona = tipoZona;
    tablero.subterraneo = bajoTierra;
    tablero.configurado = true;
    tablero.IntentarPreparar();
  }

  private void IntentarPreparar()
  {
    BattleManager batalla = BattleManager.Instance;
    if (batalla == null || batalla.lCasillasTotal == null || batalla.lCasillasTotal.Count == 0) return;
    var primera = batalla.lCasillasTotal.Find(c => c != null);
    Transform grilla = primera != null && primera.transform.parent != null ? primera.transform.parent.parent : null;
    Preparar(batalla.lCasillasTotal, grilla != null ? grilla.Find("Suelo") : null, fondo, zona, subterraneo);
  }

  // Tambien permite comprobar las mismas mallas y materiales en una escena de preview aislada.
  public bool Preparar(IReadOnlyList<Casilla> tiles, Transform suelo, MeshRenderer rendererFondo, EncounterZoneType tipoZona, bool bajoTierra)
  {
    Limpiar();
    fondo = rendererFondo;
    zona = tipoZona;
    subterraneo = bajoTierra;
    if (!Soporta(tipoZona, bajoTierra) || tiles == null || tiles.Count == 0) return false;
    string textura = tipoZona == EncounterZoneType.PasoVientoHelado ? "PasoSuelo"
      : tipoZona == EncounterZoneType.Nedukazal ? "NedukazalSuelo" : "BosqueSuelo";
    var imagen = Resources.Load<Texture2D>("TablerosIntegrados/" + textura);
    var shader = Resources.Load<Shader>("TablerosIntegrados/Suelo");
    shaderBorde = Resources.Load<Shader>("TablerosIntegrados/Borde");
    if (imagen == null || shader == null || !shader.isSupported || shaderBorde == null || !shaderBorde.isSupported) return false;
    Casilla primera = null;
    foreach (var c in tiles) if (c != null) { primera = c; break; }
    if (primera == null) return false;
    casillas = tiles;

    Vector3 origen = primera.transform.position;
    Vector3 derecha = primera.transform.right.normalized, adelante = primera.transform.forward.normalized;
    Vector3 normal = Vector3.Cross(adelante, derecha).normalized;
    Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
    float altura = 0f, celda = 0f;
    foreach (var c in tiles)
    {
      if (c == null) continue;
      Vector3 delta = c.transform.position - origen;
      Vector2 p = new Vector2(Vector3.Dot(delta, derecha), Vector3.Dot(delta, adelante));
      float ancho = c.transform.lossyScale.x, largo = c.transform.lossyScale.z * .76f;
      celda = Mathf.Max(celda, ancho * 2.5f);
      min = Vector2.Min(min, p - new Vector2(ancho, largo));
      max = Vector2.Max(max, p + new Vector2(ancho, largo));
      altura = Mathf.Min(altura, Vector3.Dot(delta, normal));
    }
    celda = Mathf.Max(celda, .1f);
    Vector2 mitad = (max - min) * .5f + Vector2.one * celda * .06f;
    Vector2 extension = mitad + Vector2.one * celda * .546f;
    Vector2 centro = (min + max) * .5f;
    Vector3 apoyo = origen + derecha * centro.x + adelante * centro.y + normal * (altura - celda * .012f);
    raiz = new GameObject("Tablero integrado - " + tipoZona, typeof(MeshFilter), typeof(MeshRenderer));
    raiz.transform.SetParent(transform, false);
    malla = new Mesh { name = "Suelo integrado sin collider", hideFlags = HideFlags.DontSave };
    malla.vertices = new[] {
      raiz.transform.InverseTransformPoint(apoyo-derecha*extension.x-adelante*extension.y),
      raiz.transform.InverseTransformPoint(apoyo+derecha*extension.x-adelante*extension.y),
      raiz.transform.InverseTransformPoint(apoyo+derecha*extension.x+adelante*extension.y),
      raiz.transform.InverseTransformPoint(apoyo-derecha*extension.x+adelante*extension.y) };
    malla.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
    malla.triangles = new[] { 0, 2, 1, 0, 3, 2 };
    malla.RecalculateBounds();
    raiz.GetComponent<MeshFilter>().sharedMesh = malla;
    materialSuelo = new Material(shader) { name = textura + " (instancia reversible)", hideFlags = HideFlags.DontSave };
    materialSuelo.mainTexture = imagen;
    materialSuelo.SetVector("_Interior", new Vector4(mitad.x / extension.x, mitad.y / extension.y, 0, 0));
    materiales.Add(materialSuelo);
    var renderer = raiz.GetComponent<MeshRenderer>();
    renderer.sharedMaterial = materialSuelo;
    renderer.sortingOrder = -9;
    renderer.shadowCastingMode = ShadowCastingMode.Off;
    renderer.receiveShadows = false;
    renderer.lightProbeUsage = LightProbeUsage.Off;
    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    if (suelo != null)
      foreach (var r in suelo.GetComponentsInChildren<Renderer>(true)) sueloOriginal.Add(new KeyValuePair<Renderer, bool>(r, r.enabled));

    foreach (var c in tiles)
    {
      if (c == null) continue;
      Estilizar(c.Borde, 0, new Color(.53f, .49f, .43f, .48f));
      Estilizar(c.Mover, 1, new Color(.96f, .97f, 1f, .96f));
      Estilizar(c.MoverCostoso, 2, new Color(1f, .67f, .73f, .96f));
      Estilizar(c.Desplazable, 3, new Color(.69f, .35f, .94f, .95f));
      Estilizar(c.Actual, 4, new Color(1f, .82f, .12f, 1f));
      Estilizar(c.OcupadoNegro, 5, new Color(.055f, .045f, .04f, .78f));
      foreach (var t in c.GetComponentsInChildren<Transform>(true))
      {
        if (t.name == "GraficoBorde Habilidad") Estilizar(t.gameObject, 6, new Color(.95f, .1f, .13f, .94f));
        else if (t.name == "GraficoBorde Habilidad amiga") Estilizar(t.gameObject, 7, new Color(.26f, .53f, 1f, .94f));
      }
      // El quad base era un velo marron; los bordes y las trampas siguen siendo capas independientes.
      var baseRenderer = c.GetComponent<MeshRenderer>();
      if (baseRenderer != null && baseRenderer.sharedMaterial != null)
        Cambiar(baseRenderer, MaterialBorde(8, Color.clear));
    }
    Activar(true);
    ActualizarTinte();
    PrepararObstaculos();
    return true;
  }

  private void Estilizar(GameObject capa, int tipo, Color color)
  {
    if (capa == null) return;
    foreach (var r in capa.GetComponentsInChildren<MeshRenderer>(true))
    {
      // No sustituye circulos, sombras, iconos de intercambio ni las marcas melee.
      if (r.sharedMaterial == null || !r.sharedMaterial.name.StartsWith("Ma_BordeCasilla")) continue;
      bool duplicado = r.name.StartsWith("Engrosador");
      Cambiar(r, MaterialBorde(tipo, color), duplicado);
      if (!duplicado) PrepararPlanoBorde(r.GetComponent<MeshFilter>());
    }
  }

  private void PrepararPlanoBorde(MeshFilter filtro)
  {
    if (filtro == null || filtro.sharedMesh == null) return;
    foreach (var cambio in cambiosMalla) if (cambio.filtro == filtro) return;
    Mesh anterior = filtro.sharedMesh;
    if (!planosBorde.TryGetValue(anterior, out var plano))
    {
      Bounds b = anterior.bounds;
      plano = new Mesh { name = "Marco plano desgastado", hideFlags = HideFlags.DontSave };
      plano.vertices = new[] { new Vector3(b.min.x,b.center.y,b.min.z), new Vector3(b.max.x,b.center.y,b.min.z),
        new Vector3(b.max.x,b.center.y,b.max.z), new Vector3(b.min.x,b.center.y,b.max.z) };
      plano.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
      plano.triangles = new[] { 0,2,1,0,3,2 };
      plano.RecalculateBounds();
      planosBorde.Add(anterior, plano);
    }
    cambiosMalla.Add(new CambioMalla { filtro = filtro, anterior = anterior, nueva = plano });
  }

  private Material MaterialBorde(int tipo, Color color)
  {
    if (bordes.TryGetValue(tipo, out var material)) return material;
    material = new Material(shaderBorde) { name = "Borde integrado " + tipo, hideFlags = HideFlags.DontSave };
    material.SetColor(ColorId, color);
    material.SetColor("_BaseColor", color);
    material.SetColor("_EmissionColor", Color.black);
    material.SetFloat("_DibujarCasilla", tipo < 8 ? 1f : 0f);
    material.SetFloat("_Grosor", tipo == 0 ? .005f : .007f);
    material.SetFloat("_Desgaste", tipo == 0 ? .68f : .26f);
    material.SetFloat("_Relleno", tipo == 0 || tipo == 5 ? .045f : .022f);
    materiales.Add(material);
    bordes.Add(tipo, material);
    return material;
  }

  private void Cambiar(Renderer renderer, Material nuevo, bool ocultar = false)
  {
    foreach (var cambio in cambios) if (cambio.renderer == renderer) return;
    Material[] anteriores = renderer.sharedMaterials;
    var nuevos = new Material[anteriores.Length];
    for (int i = 0; i < nuevos.Length; i++) nuevos[i] = nuevo;
    cambios.Add(new CambioMaterial { renderer = renderer, anterior = anteriores, nuevo = nuevos,
      habilitado = renderer.enabled, ocultar = ocultar });
  }

  public void Activar(bool activo)
  {
    if (raiz == null) return;
    raiz.SetActive(activo);
    if (aplicado == activo) return;
    foreach (var cambio in cambios)
      if (cambio.renderer != null)
      {
        cambio.renderer.sharedMaterials = activo ? cambio.nuevo : cambio.anterior;
        if (cambio.ocultar) cambio.renderer.enabled = activo ? false : cambio.habilitado;
      }
    foreach (var cambio in cambiosMalla)
      if (cambio.filtro != null) cambio.filtro.sharedMesh = activo ? cambio.nueva : cambio.anterior;
    foreach (var par in sueloOriginal) if (par.Key != null) par.Key.enabled = activo ? false : par.Value;
    foreach (var go in fondosVida) if (go != null) go.SetActive(activo);
    ActualizarSombras(activo);
    foreach (var barra in barrasObstaculos)
      for (int i = 0; i < barra.graficos.Length; i++)
        if (barra.graficos[i] != null) barra.graficos[i].enabled = !activo && barra.habilitados[i];
    aplicado = activo;
    if (activo) AlinearBarras(camaraBarras);
  }

  private void ActualizarTinte()
  {
    if (materialSuelo == null || fondo == null) return;
    if (bloqueFondo == null) bloqueFondo = new MaterialPropertyBlock();
    fondo.GetPropertyBlock(bloqueFondo);
    Color tinte = bloqueFondo.GetColor(ColorId);
    if (tinte.a <= 0f) tinte = Color.white;
    // Solo acompana hora/noche; la textura ya contiene el albedo aprobado de cada zona.
    materialSuelo.SetColor(ColorId, new Color(Mathf.Clamp(tinte.r, .5f, 1.12f),
      Mathf.Clamp(tinte.g, .5f, 1.12f), Mathf.Clamp(tinte.b, .5f, 1.12f), 1f));
  }

  private void PrepararObstaculos()
  {
    if (casillas == null) return;
    foreach (var c in casillas)
    {
      if (c == null || c.Presente == null || !c.Presente.TryGetComponent<Obstaculo>(out var obstaculo) || !obstaculosVistos.Add(obstaculo)) continue;
      CrearSombraObstaculo(c, obstaculo);
      var slider = obstaculo.GetComponentInChildren<Slider>(true);
      if (slider == null) continue;
      // Representa el mismo valor en un plano independiente: la jerarquia original tiene escala no uniforme.
      var go = new GameObject("Fondo vida integrado", typeof(MeshFilter), typeof(MeshRenderer));
      go.transform.SetParent(raiz.transform, false);
      var mesh = new Mesh { name = "Barra de vida sin collider", hideFlags = HideFlags.DontSave };
      mesh.MarkDynamic();
      var esquinas = new Vector3[4];
      ((RectTransform)slider.transform).GetWorldCorners(esquinas);
      var graficos = slider.GetComponentsInChildren<Graphic>(true);
      var habilitados = new bool[graficos.Length];
      for (int i = 0; i < graficos.Length; i++) { habilitados[i] = graficos[i].enabled; graficos[i].enabled = false; }
      var barra = new BarraObstaculo { slider = slider, graficos = graficos, habilitados = habilitados,
        mesh = mesh, transform = go.transform, ancho = Vector3.Distance(esquinas[0], esquinas[3]) };
      mesh.vertices = barra.vertices;
      var relleno = slider.fillRect != null ? slider.fillRect.GetComponent<Graphic>() : null;
      Color verde = relleno != null ? relleno.color : new Color(.12f, .78f, .22f, 1f);
      Color fondoBarra = new Color(.025f, .03f, .025f, .94f);
      mesh.colors = new[] { fondoBarra, fondoBarra, fondoBarra, fondoBarra, verde, verde, verde, verde };
      mesh.triangles = new[] { 0, 2, 1, 0, 3, 2, 4, 6, 5, 4, 7, 6 };
      go.GetComponent<MeshFilter>().sharedMesh = mesh;
      var renderer = go.GetComponent<MeshRenderer>();
      var material = MaterialBorde(9, Color.white);
      material.SetFloat("_UsarColorVertice", 1f);
      renderer.sharedMaterial = material;
      renderer.sortingOrder = RenderOrderHelper.CalcularOrdenPorY(c.posY) + 4;
      renderer.shadowCastingMode = ShadowCastingMode.Off;
      renderer.receiveShadows = false;
      barrasObstaculos.Add(barra);
      fondosVida.Add(go);
    }
  }

  public void AlinearBarras(Camera camara)
  {
    if (camara == null || !aplicado) return;
    camaraBarras = camara;
    foreach (var barra in barrasObstaculos)
    {
      if (barra.transform == null) continue;
      bool visible = barra.slider != null && barra.slider.gameObject.activeInHierarchy;
      if (visible)
      {
        var canvas = barra.slider.GetComponentInParent<Canvas>();
        visible = canvas == null || canvas.enabled;
      }
      if (barra.transform.gameObject.activeSelf != visible) barra.transform.gameObject.SetActive(visible);
      if (!visible) continue;
      var rect = (RectTransform)barra.slider.transform;
      Vector3 centro = rect.TransformPoint(rect.rect.center);
      float mitad = barra.ancho * .5f, altura = barra.ancho * .065f;
      Vector3 derecha = camara.transform.right, arriba = camara.transform.up;
      float izquierda = -mitad + altura * .25f, anchoInterior = barra.ancho - altura * .5f;
      float valor = Mathf.Clamp01(barra.slider.normalizedValue);
      if (barra.slider.direction == Slider.Direction.RightToLeft) izquierda += anchoInterior * (1f - valor);
      for (int i = 0; i < 8; i++)
      {
        bool relleno = i >= 4;
        int esquina = i % 4;
        float x = relleno ? izquierda + ((esquina == 1 || esquina == 2) ? anchoInterior * valor : 0f)
          : ((esquina == 1 || esquina == 2) ? mitad : -mitad);
        float y = ((esquina >= 2) ? 1f : -1f) * altura * (relleno ? .28f : .5f);
        Vector3 punto = centro + derecha * x + arriba * y;
        // El relleno esta apenas delante del fondo, sin depender del ZWrite de la UI original.
        if (relleno) punto -= camara.transform.forward * .0001f;
        barra.vertices[i] = barra.transform.InverseTransformPoint(punto);
      }
      barra.mesh.vertices = barra.vertices;
      barra.mesh.RecalculateBounds();
    }
  }

  private void CrearSombraObstaculo(Casilla c, Obstaculo obstaculo)
  {
    // Reutiliza solo el grafico de la sombra existente, sin copiar colliders ni scripts.
    var fuente = c.Sombra != null ? c.Sombra.GetComponent<MeshFilter>() : null;
    var rendererFuente = c.Sombra != null ? c.Sombra.GetComponent<MeshRenderer>() : null;
    if (fuente == null || rendererFuente == null) return;
    var go = new GameObject("Contacto obstaculo integrado", typeof(MeshFilter), typeof(MeshRenderer));
    go.transform.SetParent(c.Sombra.transform.parent, false);
    go.transform.localPosition = c.Sombra.transform.localPosition;
    go.transform.localRotation = c.Sombra.transform.localRotation;
    go.transform.localScale = c.Sombra.transform.localScale;
    go.GetComponent<MeshFilter>().sharedMesh = fuente.sharedMesh;
    var renderer = go.GetComponent<MeshRenderer>();
    renderer.sharedMaterials = rendererFuente.sharedMaterials;
    renderer.sortingOrder = -1;
    renderer.shadowCastingMode = ShadowCastingMode.Off;
    renderer.receiveShadows = false;
    sombrasObstaculos.Add(new SombraObstaculo { casilla = c, obstaculo = obstaculo, objeto = go });
  }

  private void ActualizarSombras(bool activo)
  {
    foreach (var sombra in sombrasObstaculos)
    {
      if (sombra.objeto == null) continue;
      bool visible = activo && sombra.casilla != null && sombra.obstaculo != null
        && sombra.obstaculo.gameObject.activeInHierarchy && sombra.casilla.Presente == sombra.obstaculo.gameObject;
      if (sombra.objeto.activeSelf != visible) sombra.objeto.SetActive(visible);
    }
  }

  private void LateUpdate()
  {
    if (!configurado || !Soporta(zona, subterraneo)) return;
    var batalla = BattleManager.Instance;
    if (batalla == null || !batalla.gameObject.activeInHierarchy) { Activar(false); return; }
    if (raiz == null) IntentarPreparar();
    Activar(true);
    ActualizarTinte();
    ActualizarSombras(true);
    revisarObstaculos -= Time.unscaledDeltaTime;
    if (revisarObstaculos <= 0f) { revisarObstaculos = .5f; PrepararObstaculos(); }
    AlinearBarras(batalla.goCamara != null ? batalla.goCamara.GetComponent<Camera>() : null);
  }

  private void OnDisable() { Limpiar(); }
  private void OnDestroy() { Limpiar(); }

  public void Limpiar()
  {
    Activar(false);
    foreach (var go in fondosVida) Destruir(go);
    fondosVida.Clear();
    foreach (var barra in barrasObstaculos) Destruir(barra.mesh);
    barrasObstaculos.Clear();
    camaraBarras = null;
    obstaculosVistos.Clear();
    foreach (var sombra in sombrasObstaculos) Destruir(sombra.objeto);
    sombrasObstaculos.Clear();
    foreach (var material in materiales) Destruir(material);
    materiales.Clear();
    bordes.Clear();
    cambios.Clear();
    cambiosMalla.Clear();
    foreach (var plano in planosBorde.Values) Destruir(plano);
    planosBorde.Clear();
    sueloOriginal.Clear();
    Destruir(malla);
    Destruir(raiz);
    malla = null;
    raiz = null;
    materialSuelo = null;
    casillas = null;
    aplicado = false;
  }

  private static void Destruir(Object objeto)
  {
    if (objeto == null) return;
    if (Application.isPlaying) Object.Destroy(objeto); else Object.DestroyImmediate(objeto);
  }
}
