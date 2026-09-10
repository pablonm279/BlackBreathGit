using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Capa escenica: solo observa el combate. No cambia unidades, camara, RNG o tiempo.
[DisallowMultipleComponent]
[DefaultExecutionOrder(720)]
public sealed class CampoVivoBatalla : MonoBehaviour
{
  private static CampoVivoBatalla actual;
  private static readonly int RelojId = Shader.PropertyToID("_Reloj");
  private static readonly int ColorId = Shader.PropertyToID("_Color");
  private static readonly int VientoId = Shader.PropertyToID("_Viento");
  private BattleManager batalla;
  private MeshRenderer fondo;
  private EncounterZoneType zona;
  private bool subterraneo, noche, configurado, areaLista;
  private PaletaCampoVivo paleta;
  private GameObject raiz;
  private Mesh malla;
  private readonly List<Material> materiales = new List<Material>(7);
  private readonly List<ParticleSystem> sistemas = new List<ParticleSystem>(5);
  private ParticleSystem motasLejanas, motasCercanas, bruma, polvo, fragmentos;
  private readonly ParticleSystem.Particle[] particulas = new ParticleSystem.Particle[180];
  private MeshRenderer sombraSuelo, luzSuelo;
  private readonly Dictionary<Unidad, Rastro> rastros = new Dictionary<Unidad, Rastro>();
  private readonly HashSet<Unidad> presentes = new HashSet<Unidad>();
  private readonly List<Unidad> retiradas = new List<Unidad>();
  private System.Random azar;
  private Vector3 centro, derecha = Vector3.right, normal = Vector3.up, adelante = Vector3.forward;
  private Vector2 extension;
  private float celda = .65f, reloj, mezcla, creditoMotas, creditoBruma, revision;
  private int presupuestoEmisiones;

  private sealed class Rastro
  {
    public Vector3 posicion;
    public bool moviendo;
  }

  public bool SustituyeParticulas => isActiveAndEnabled && configurado && areaLista && raiz != null
    && AjustesCampoVivo.Activo && AjustesCampoVivo.Actual.atmosfera;

  public static void ConfigurarEn(BattleAmbientLife ambiente, MeshRenderer fondo, EncounterZoneType zona, bool subterraneo)
  {
    var campo = ambiente.GetComponent<CampoVivoBatalla>();
    if (campo == null) campo = ambiente.gameObject.AddComponent<CampoVivoBatalla>();
    campo.Configurar(fondo, zona, subterraneo);
  }

  public void Configurar(MeshRenderer rendererFondo, EncounterZoneType tipoZona, bool bajoTierra)
  {
    LimpiarTransitorios();
    actual = this;
    batalla = BattleManager.Instance;
    fondo = rendererFondo;
    zona = tipoZona;
    subterraneo = bajoTierra;
    // RNG exclusivo del arte: ninguna llamada a UnityEngine.Random.
    azar = new System.Random(197903 + (int)zona * 7919);
    reloj = mezcla = creditoMotas = creditoBruma = revision = 0f;
    configurado = true;
    areaLista = false;
    ResolverPaleta();
  }

  public void EstablecerNoche(bool activa) { noche = activa; ResolverPaleta(); }

  private void ResolverPaleta()
  {
    string nombre = fondo != null && fondo.sharedMaterial != null ? fondo.sharedMaterial.name.ToLowerInvariant() : "";
    bool quemado = nombre.Contains("ardiente") || nombre.Contains("quemad") || nombre.Contains("incend");
    if (fondo != null && fondo.sharedMaterial != null && AjustesCampoVivo.Actual != null)
      foreach (Texture textura in AjustesCampoVivo.Actual.fondosQuemados)
        if (textura != null && textura == fondo.sharedMaterial.mainTexture) { quemado = true; break; }
    paleta = PaletaCampoVivo.Resolver(zona, subterraneo, noche, quemado);
  }

  private void LateUpdate() { Avanzar(Time.deltaTime); }

  // Reloj explicito permite verificar el sistema real en Editor sin ejecutar una partida.
  public void Avanzar(float delta)
  {
    if (!configurado || !AjustesCampoVivo.Activo || !BatallaVisible())
    {
      if (raiz != null && raiz.activeSelf) raiz.SetActive(false);
      LimpiarTransitorios();
      mezcla = 0f;
      return;
    }
    if (!areaLista && !ResolverArea()) return;
    if (raiz == null && !CrearRecursos()) return;
    if (!raiz.activeSelf) raiz.SetActive(true);
    actual = this;

    if (batalla.PausaCombateActiva || delta <= 0f) return;
    float dt = Mathf.Min(delta, .08f);
    reloj += dt;
    mezcla = Mathf.MoveTowards(mezcla, 1f, dt / 1.4f);
    presupuestoEmisiones = 44;
    var ajustes = AjustesCampoVivo.Actual;
    float calidad = QualitySettings.GetQualityLevel() <= 0 ? .55f : QualitySettings.GetQualityLevel() == 1 ? .8f : 1f;
    float viento = paleta.viento * (1f + .7f * Rafaga(reloj));

    foreach (Material m in materiales)
    {
      m.SetFloat(RelojId, reloj * .4f);
      m.SetVector(VientoId, new Vector4(viento * .12f, .012f, 0, 0));
    }
    AnimarSuelo(ajustes, viento);
    if (ajustes.atmosfera)
    {
      EmitirAmbiente(dt, ajustes.densidad * calidad, viento);
      AdvectarMotas(motasLejanas, viento);
      AdvectarMotas(motasCercanas, viento);
    }
    else
    {
      motasLejanas.Clear(); motasCercanas.Clear(); bruma.Clear();
      creditoMotas = creditoBruma = 0f;
    }
    if (ajustes.desplazamientos) ObservarDesplazamientos();
    else rastros.Clear();
    foreach (ParticleSystem ps in sistemas) ps.Simulate(dt, false, false, false);
    revision -= dt;
    if (revision <= 0f) { DepurarRastros(); revision = .75f; }
  }

  private bool BatallaVisible()
  {
    if (batalla == null || !batalla.isActiveAndEnabled || !isActiveAndEnabled) return false;
    var campania = CampaignManager.Instance;
    return campania == null || campania.scAdministradorEscenas == null || campania.scAdministradorEscenas.escenaActual == 1;
  }

  public static float Rafaga(float t)
  {
    float oleada = Mathf.Max(0f, Mathf.Sin(t * .43f) * .7f + Mathf.Sin(t * .19f + 1.4f) * .3f);
    return oleada * oleada;
  }

  private bool ResolverArea()
  {
    var casillas = batalla != null ? batalla.lCasillasTotal : null;
    if (casillas == null || casillas.Count < 3) return false;
    Casilla primera = null;
    foreach (var c in casillas) if (c != null) { primera = c; break; }
    if (primera == null) return false;
    Vector3 origen = primera.transform.position, eje = Vector3.zero, transversal = Vector3.zero;
    float distanciaMaxima = 0f, separacionMaxima = 0f, pasoMinimo = float.MaxValue;
    foreach (var c in casillas)
    {
      if (c == null) continue;
      Vector3 v = c.transform.position - origen;
      if (v.sqrMagnitude > distanciaMaxima) { distanciaMaxima = v.sqrMagnitude; eje = v; }
      if (v.sqrMagnitude > .0001f) pasoMinimo = Mathf.Min(pasoMinimo, v.magnitude);
    }
    foreach (var c in casillas)
    {
      if (c == null) continue;
      Vector3 v = c.transform.position - origen;
      float separacion = Vector3.Cross(eje, v).sqrMagnitude;
      if (separacion > separacionMaxima) { separacionMaxima = separacion; transversal = v; }
    }
    if (separacionMaxima < .00001f) return false;
    normal = Vector3.Cross(eje, transversal).normalized;
    if (Vector3.Dot(normal, Vector3.up) < 0f) normal = -normal;
    derecha = Vector3.ProjectOnPlane(Vector3.right, normal).normalized;
    adelante = Vector3.Cross(derecha, normal).normalized;
    celda = Mathf.Clamp(pasoMinimo, .05f, 5f);
    Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
    foreach (var c in casillas)
    {
      if (c == null) continue;
      Vector3 d = c.transform.position - origen;
      Vector2 p = new Vector2(Vector3.Dot(d, derecha), Vector3.Dot(d, adelante));
      min = Vector2.Min(min, p); max = Vector2.Max(max, p);
    }
    Vector2 medio = (min + max) * .5f;
    centro = origen + derecha * medio.x + adelante * medio.y;
    extension = (max - min) * .5f + Vector2.one * celda * .9f;
    areaLista = true;
    if (raiz != null) ColocarRaiz();
    return true;
  }

  private Vector3 Mundo(float x, float y, float z) { return centro + derecha*x + normal*y + adelante*z; }
  private Vector3 Apoyo(Vector3 p) { return p - normal * Vector3.Dot(p - centro, normal) + normal * celda * .045f; }

  private bool CrearRecursos()
  {
    Shader shader = Resources.Load<Shader>("CampoVivo/CampoVivo");
    if (shader == null || !shader.isSupported) return false;
    raiz = new GameObject("Campo vivo (reversible)");
    raiz.transform.SetParent(transform, true);
    ColocarRaiz();
    malla = new Mesh { name = "CampoVivo_Quad", hideFlags = HideFlags.DontSave };
    malla.vertices = new[] { new Vector3(-.5f,-.5f), new Vector3(.5f,-.5f), new Vector3(.5f,.5f), new Vector3(-.5f,.5f) };
    malla.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
    malla.colors = new[] { Color.white, Color.white, Color.white, Color.white };
    malla.triangles = new[] { 0,2,1,0,3,2 };
    malla.RecalculateBounds();
    Material humo = CrearMaterial(shader, "Bruma", 0);
    Material mota = CrearMaterial(shader, "Motas", 1);
    Material esquirla = CrearMaterial(shader, "Fragmentos", 2);
    sombraSuelo = CrearPlano("Sombras de nubes", CrearMaterial(shader, "Sombra suelo", 4), -8);
    luzSuelo = CrearPlano("Luz rasante", CrearMaterial(shader, "Luz suelo", 4), -7);
    motasLejanas = CrearSistema("Ceniza nieve y polvo", mota, 180, -3, false);
    motasCercanas = CrearSistema("Particulas cercanas", esquirla, 40, 2, false);
    bruma = CrearSistema("Bancos de bruma", humo, 14, -4, true);
    polvo = CrearSistema("Polvo de movimiento e impacto", humo, 60, 1, true);
    fragmentos = CrearSistema("Esquirlas", esquirla, 100, 2, false);
    return true;
  }

  private void ColocarRaiz()
  {
    raiz.transform.SetPositionAndRotation(centro, Quaternion.LookRotation(adelante, normal));
  }

  private Material CrearMaterial(Shader shader, string nombre, float modo)
  {
    var m = new Material(shader) { name = "CampoVivo_" + nombre, hideFlags = HideFlags.DontSave };
    m.SetFloat("_Modo", modo);
    materiales.Add(m);
    return m;
  }

  private MeshRenderer CrearPlano(string nombre, Material material, int orden)
  {
    var go = new GameObject(nombre, typeof(MeshFilter), typeof(MeshRenderer));
    go.transform.SetParent(raiz.transform, false);
    go.transform.localRotation = Quaternion.Euler(90,0,0);
    go.GetComponent<MeshFilter>().sharedMesh = malla;
    var r = go.GetComponent<MeshRenderer>();
    r.sharedMaterial = material;
    r.sortingOrder = orden;
    r.shadowCastingMode = ShadowCastingMode.Off;
    r.receiveShadows = false;
    r.lightProbeUsage = LightProbeUsage.Off;
    r.reflectionProbeUsage = ReflectionProbeUsage.Off;
    return r;
  }

  private ParticleSystem CrearSistema(string nombre, Material material, int maximo, int orden, bool estirado)
  {
    var go = new GameObject(nombre);
    go.transform.SetParent(raiz.transform, false);
    var ps = go.AddComponent<ParticleSystem>();
    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    ps.useAutoRandomSeed = false;
    ps.randomSeed = (uint)(1979 + sistemas.Count * 37);
    var main = ps.main;
    main.playOnAwake = false;
    main.loop = false;
    main.maxParticles = maximo;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.scalingMode = ParticleSystemScalingMode.Shape;
    main.startSize3D = true;
    var emission = ps.emission; emission.enabled = false;
    var shape = ps.shape; shape.enabled = false;
    var color = ps.colorOverLifetime;
    color.enabled = true;
    var gradiente = new Gradient();
    gradiente.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
      new[] { new GradientAlphaKey(0,0), new GradientAlphaKey(1,.12f), new GradientAlphaKey(.65f,.65f), new GradientAlphaKey(0,1) });
    color.color = gradiente;
    var size = ps.sizeOverLifetime;
    size.enabled = true;
    size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0,estirado ? .7f : 1f,1,estirado ? 1.65f : .55f));
    var r = ps.GetComponent<ParticleSystemRenderer>();
    r.sharedMaterial = material;
    r.renderMode = ParticleSystemRenderMode.Billboard;
    r.alignment = ParticleSystemRenderSpace.View;
    r.sortMode = ParticleSystemSortMode.Distance;
    r.sortingOrder = orden;
    r.shadowCastingMode = ShadowCastingMode.Off;
    r.receiveShadows = false;
    r.maxParticleSize = .35f;
    sistemas.Add(ps);
    return ps;
  }

  private void AnimarSuelo(AjustesCampoVivo ajustes, float viento)
  {
    sombraSuelo.enabled = luzSuelo.enabled = ajustes.suelo;
    if (!ajustes.suelo) return;
    float visibilidad = mezcla * ajustes.intensidad;
    sombraSuelo.transform.localPosition = new Vector3(Mathf.Sin(reloj*.04f)*celda*.3f, celda*.025f, 0);
    sombraSuelo.transform.localScale = new Vector3(extension.x*2.4f,extension.y*2.4f,1);
    sombraSuelo.sharedMaterial.SetColor(ColorId, new Color(.06f,.09f,.12f,.064f*visibilidad));
    luzSuelo.transform.localPosition = new Vector3(-extension.x*.2f,celda*.03f,extension.y*.2f);
    luzSuelo.transform.localScale = new Vector3(extension.x*2.2f,extension.y*2f,1);
    Color luz = paleta.luz;
    luz.a = (subterraneo ? .028f : .034f) * visibilidad * (1f + .07f*Mathf.Sin(reloj*.32f));
    luzSuelo.sharedMaterial.SetColor(ColorId, luz);
    luzSuelo.sharedMaterial.SetVector(VientoId, new Vector4(-viento*.09f,.018f,0,0));
  }

  private void EmitirAmbiente(float dt, float densidad, float viento)
  {
    float area = Mathf.Clamp(extension.x * extension.y / (celda*celda*22f), .7f, 1.4f);
    creditoMotas += dt * 8f * densidad * paleta.densidad * area;
    while (creditoMotas >= 1f)
    {
      creditoMotas--;
      bool cerca = Azar(0,1) < .22f;
      Vector3 p = Mundo(Azar(-extension.x*1.1f,extension.x*1.1f),Azar(.1f,3.4f)*celda,Azar(-extension.y,extension.y));
      Vector3 v = derecha * viento*celda + normal*paleta.caida*celda + adelante*Azar(-.035f,.035f)*celda;
      Color c = paleta.motas;
      if (paleta.brasas && Azar(0,1) < .6f) c = new Color(.65f,.61f,.54f);
      c.a = (cerca ? .48f : .6f) * mezcla * AjustesCampoVivo.Actual.intensidad;
      float size = Azar(.035f,.075f)*celda*(cerca ? 1.4f : 1f);
      Vector3 forma = paleta.nieve && cerca ? new Vector3(size*.55f,size*1.5f,size) : Vector3.one*size;
      Emitir(cerca ? motasCercanas : motasLejanas,p,v,c,forma,Azar(4f,8f));
    }
    creditoBruma += dt * .24f * densidad;
    if (creditoBruma >= 1f)
    {
      creditoBruma--;
      Color c = paleta.aire; c.a = .072f*mezcla*AjustesCampoVivo.Actual.intensidad;
      Vector3 p = Mundo(Azar(-extension.x,extension.x),Azar(.15f,.7f)*celda,Azar(-extension.y*.65f,extension.y));
      Emitir(bruma,p,derecha*viento*celda*.16f,c,new Vector3(celda*Azar(3f,5f),celda*Azar(.65f,1.1f),1),Azar(6f,10f), false);
    }
  }

  private void AdvectarMotas(ParticleSystem ps, float viento)
  {
    int cantidad = ps.GetParticles(particulas);
    for (int i = 0; i < cantidad; i++)
    {
      float fase = (particulas[i].randomSeed % 1024) * .013f;
      float remolino = Mathf.Sin(reloj * .9f + fase);
      particulas[i].velocity = derecha * celda * (viento + remolino*.06f)
        + normal * celda * (paleta.caida + Mathf.Sin(reloj*.65f+fase)*.045f)
        + adelante * celda * remolino*.035f;
    }
    ps.SetParticles(particulas,cantidad);
  }

  private void Emitir(ParticleSystem ps, Vector3 posicion, Vector3 velocidad, Color color, Vector3 escala, float vida, bool rotar = true)
  {
    if (ps == null || presupuestoEmisiones-- <= 0) return;
    if (ps == motasLejanas || ps == motasCercanas || ps == fragmentos) escala *= .5f;
    var e = new ParticleSystem.EmitParams
    {
      position = posicion, velocity = velocidad, startColor = color, startLifetime = vida,
      startSize3D = escala, rotation = rotar ? Azar(0,360) : 0,
      randomSeed = (uint)azar.Next(1,int.MaxValue), applyShapeToPosition = false
    };
    ps.Emit(e, 1);
  }

  private float Azar(float min, float max) { return min + (max-min)*(float)azar.NextDouble(); }

  private bool UnidadVisible(Unidad u)
  {
    return u != null && u.gameObject.activeInHierarchy && u.uImage != null && u.uImage.isActiveAndEnabled
      && !u.EstaOcultoVisualmenteParaJugador();
  }

  private void ObservarDesplazamientos()
  {
    foreach (Unidad u in batalla.lUnidadesTotal)
    {
      if (!UnidadVisible(u) || u.HP_actual <= 0 || u.esInmobil || u.esEtereo || u.unidadVoladora || u.estado_Volando)
      { if (u != null) rastros.Remove(u); continue; }
      Vector3 p = Apoyo(u.transform.position);
      if (!rastros.TryGetValue(u, out Rastro estado))
      { rastros[u] = new Rastro { posicion=p, moviendo=u.movimientoEnCurso }; continue; }
      float distancia = Vector3.Distance(p,estado.posicion);
      bool congelada = u.estado_congelado > 0 || u.estado_aturdido > 0;
      if (!congelada && distancia < celda*3.5f)
      {
        Vector3 direccion = (p - estado.posicion).normalized;
        if (u.movimientoEnCurso && !estado.moviendo)
        {
          // Dos bocanadas atras del apoyo: el cuerpo no tapa todo el polvo del impulso.
          LevantarPolvo(estado.posicion - direccion*celda*.12f, -direccion, 1.3f, 5);
          LevantarPolvo(estado.posicion - direccion*celda*.3f, -direccion, .85f, 0);
        }
        if (!u.movimientoEnCurso && estado.moviendo) LevantarPolvo(p,direccion,.85f,7);
      }
      estado.moviendo = u.movimientoEnCurso;
      estado.posicion = p;
    }
  }

  private void DepurarRastros()
  {
    presentes.Clear(); retiradas.Clear();
    foreach (var u in batalla.lUnidadesTotal) if (u != null) presentes.Add(u);
    foreach (var par in rastros) if (par.Key == null || !presentes.Contains(par.Key)) retiradas.Add(par.Key);
    foreach (var u in retiradas) rastros.Remove(u);
  }

  private void LevantarPolvo(Vector3 punto, Vector3 direccion, float fuerza, int cantidad, float rapidez = 1f)
  {
    Color c = paleta.polvo * .65f; c.a = .43f*fuerza*AjustesCampoVivo.Actual.intensidad;
    Vector3 velocidad = (direccion*celda*.3f + normal*celda*.1f)*rapidez;
    Emitir(polvo,punto+normal*celda*.06f,velocidad,c,new Vector3(celda*fuerza*.85f,celda*fuerza*.42f,1)*.51f,.65f/rapidez, false);
    for (int i=0;i<Mathf.RoundToInt(cantidad*.4f);i++)
    {
      float angulo = Azar(0,Mathf.PI*2f);
      Vector3 v = (derecha*Mathf.Cos(angulo)+adelante*Mathf.Sin(angulo))*celda*Azar(.15f,.7f)*fuerza;
      v += normal*celda*Azar(.08f,.4f)*fuerza;
      Color grano = paleta.polvo * .65f; grano.a = .65f*AjustesCampoVivo.Actual.intensidad;
      float tamano = celda*Azar(.025f,.055f)*fuerza*.51f;
      Emitir(fragmentos,punto,v*rapidez,grano,Vector3.one*tamano,Azar(.22f,.5f)/rapidez);
    }
  }

  public static void ReproducirArranqueMelee(Unidad unidad, Vector3 destino)
  {
    CampoVivoBatalla campo = actual;
    if (campo == null || !AjustesCampoVivo.Activo || !AjustesCampoVivo.Actual.desplazamientos
      || !campo.BatallaVisible() || campo.batalla.PausaCombateActiva || !campo.areaLista
      || campo.raiz == null || !campo.raiz.activeSelf || !campo.UnidadVisible(unidad)
      || unidad.HP_actual <= 0 || unidad.esInmobil || unidad.esEtereo || unidad.unidadVoladora
      || unidad.estado_Volando || unidad.estado_congelado > 0 || unidad.estado_aturdido > 0) return;

    Vector3 direccion = Vector3.ProjectOnPlane(destino-unidad.transform.position,campo.normal);
    if (direccion.sqrMagnitude <= .0001f) return;
    direccion.Normalize();
    Vector3 punto = campo.Apoyo(unidad.transform.position);
    campo.LevantarPolvo(punto-direccion*campo.celda*.12f,-direccion,1.5f,5,1.3f);
    campo.LevantarPolvo(punto-direccion*campo.celda*.3f,-direccion,.98f,0,1.3f);
  }

  public static void ReproducirImpacto(Unidad causante, Unidad objetivo, bool critico, bool muerte, float danio)
  {
    if (actual != null) actual.Impacto(causante,objetivo,critico,muerte,danio);
  }

  private void Impacto(Unidad causante, Unidad objetivo, bool critico, bool muerte, float danio)
  {
    if (!AjustesCampoVivo.Activo || !AjustesCampoVivo.Actual.impactos || !BatallaVisible()
      || batalla.PausaCombateActiva || !areaLista || raiz == null || !raiz.activeSelf || danio <= 0 || !UnidadVisible(objetivo)) return;
    // Destello localizado, acotado: no agrega flashes de pantalla ni sacudidas.
    if (critico || muerte)
    {
      Vector3 centroImpacto = objetivo.uImage.rectTransform.TransformPoint(objetivo.uImage.rectTransform.rect.center);
      for (int i=0;i<Mathf.RoundToInt(9*.4f);i++)
      {
        Color c = Color.Lerp(paleta.luz,new Color(1f,.83f,.56f),.65f); c.a = .85f;
        Vector3 v = (derecha*Azar(-1,1)+normal*Azar(-.2f,1.2f))*celda*1.6f;
        Emitir(fragmentos,centroImpacto,v,c,Vector3.one*celda*Azar(.035f,.065f),Azar(.14f,.3f));
      }
    }
  }

  public static void AjustarSangre(GameObject instancia)
  {
    if (instancia == null || !AjustesCampoVivo.Activo || !AjustesCampoVivo.Actual.impactos) return;
    foreach (var ps in instancia.GetComponentsInChildren<ParticleSystem>())
    {
      // Ajusta solo la instancia: conserva cantidad, dispersion, velocidad y tamano del prefab.
      ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
      var principal = ps.main;
      principal.startDelay = 0f;
      principal.simulationSpace = ParticleSystemSimulationSpace.World;
      var tamano = ps.sizeOverLifetime;
      tamano.enabled = true;
      tamano.separateAxes = false;
      tamano.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
        new Keyframe(0f, .55f), new Keyframe(.12f, .85f),
        new Keyframe(.55f, .6f), new Keyframe(1f, .15f)));
      var color = ps.colorOverLifetime;
      color.enabled = true;
      color.color = new ParticleSystem.MinMaxGradient(new Gradient
      {
        colorKeys = new[] {
          new GradientColorKey(new Color(.78f, .035f, .025f), 0f),
          new GradientColorKey(new Color(.46f, .018f, .012f), .5f),
          new GradientColorKey(new Color(.28f, .01f, .008f), 1f) },
        alphaKeys = new[] {
          new GradientAlphaKey(.9f, 0f), new GradientAlphaKey(.75f, .35f),
          new GradientAlphaKey(0f, 1f) }
      });
      ps.Play(false);
    }
  }

  private void LimpiarTransitorios()
  {
    rastros.Clear(); presentes.Clear(); retiradas.Clear();
    foreach (var ps in sistemas) if (ps != null && ps.particleCount > 0) ps.Clear();
  }

  private void OnDisable()
  {
    LimpiarTransitorios();
    mezcla = 0f;
    if (raiz != null) raiz.SetActive(false);
    if (actual == this) actual = null;
  }

  private void OnDestroy()
  {
    foreach (Material m in materiales) Liberar(m);
    Liberar(malla);
    if (raiz != null) Liberar(raiz);
    if (actual == this) actual = null;
  }

  private static void Liberar(Object o)
  {
    if (o == null) return;
    if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
  }
}
