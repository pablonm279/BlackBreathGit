using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IACondenaRaices : IAHabilidad
{
  const int TipoDanioNecrotico = 9;
  const int DadosCantidad = 1;
  const int DadosCaras = 8;
  const int DificultadSalvacion = 13;
  const int TurnosCondena = 2;
  const int CantidadObjetivos = 2;

  const float DuracionAnillo = 3f;
  const float AnchoAnillo = 0.00765f;
  const int SegmentosAnillo = 72;
  const float RadioAnillo = 0.221f;
  const float AlturaAnillo = 0.238f;

  [SerializeField] AudioClip sfxCondenaBosque;
  [SerializeField, Range(0f, 1f)] float volumenSfxCondenaBosque = 1f;

  static Material materialAnillo;

  void Awake()
  {
    nombre = "Condena del bosque";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 5;
    esMelee = false;
    hAlcance = 8; //Se ignora en ListaHayObjetivosAlAlcance, se usa alcance global
    hCooldownMax = 0;
    esHostil = true;
    prioridad = 8;
    costoAP = 4;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 8;
  }

  public async override Task ActivarHabilidad()
  {
    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();

    List<Unidad> objetivos = ListaHayObjetivosAlAlcance()
      .OfType<Unidad>()
      .OrderBy(_ => UnityEngine.Random.value)
      .Take(CantidadObjetivos)
      .ToList();

    if (scEstaUnidad == null || objetivos.Count == 0)
    {
      return;
    }

    ReproducirSfxCondenaBosque();
    scEstaUnidad.CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(objetivos.Cast<object>().ToList(), null);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

    await BattleManager.DelayCombateAsync(450);

    hActualCooldown = hCooldownMax;

    foreach (Unidad objetivo in objetivos)
    {
      AplicarEfectosHabilidad(objetivo);
      await BattleManager.DelayCombateAsync(120);
    }
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo) || scEstaUnidad == null)
    {
      return;
    }

    DibujarAnillo(objetivo);

    float danio = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);

    bool noSeSalva = objetivo.TiradaSalvacion(3, DificultadSalvacion);
    if (noSeSalva)
    {    objetivo.RecibirDanio(danio, TipoDanioNecrotico, false, scEstaUnidad);

      objetivo.estado_Condenado = Mathf.Min(3, objetivo.estado_Condenado + Mathf.Max(objetivo.estado_Condenado, TurnosCondena));
      objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Condenado"), new Color(0.4f, 0.24f, 0.5f));
      BattleManager.Instance?.EscribirLog($"{objetivo.uNombre} {TRADU.i.Traducir("es condenado por")} {TurnosCondena} {TRADU.i.Traducir("turnos.")}");
    }
    else
    {
      BattleManager.Instance?.EscribirLog($"{objetivo.uNombre} {TRADU.i.Traducir("resiste la condena, pero sufre el latido necrotico.")}");
    }
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return objPosibles.OfType<Unidad>().OrderBy(_ => UnityEngine.Random.value).FirstOrDefault();
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    LadoManager ladoEnemigo = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoEnemigo == null)
    {
      return objPosibles;
    }

    ladoEnemigo.ActualizarListaDeUnidadesEnLado();

    foreach (Unidad u in ladoEnemigo.unidadesLado)
    {
      if (u == null)
      {
        continue;
      }

      if (u.HP_actual < 1)
      {
        continue;
      }

      if (u.ObtenerEstaEscondido() > 0 && !Usuario.GetComponent<IAUnidad>().bPuedeVerEscondidos)
      {
        continue;
      }

      objPosibles.Add(u);
    }

    return objPosibles;
  }

  void DibujarAnillo(Unidad objetivo)
  {
    Vector3 destino = objetivo.puntoEntrante != null
      ? objetivo.puntoEntrante.position
      : objetivo.transform.position + Vector3.up * AlturaAnillo;

    LineRenderer anilloExterior = CrearAnilloEspectral("VFX_CondenaRaices_Exterior");
    LineRenderer anilloInterior = CrearAnilloEspectral("VFX_CondenaRaices_Interior");
    StartCoroutine(AnimarAnillo(anilloExterior, destino, RadioAnillo, 1f, new Color(0.61f, 0.28f, 0.9f)));
    StartCoroutine(AnimarAnillo(anilloInterior, destino, RadioAnillo * 0.64f, -1.45f, new Color(0.78f, 0.32f, 1f)));
    StartCoroutine(GenerarEsporas(destino, Vector3.right, Vector3.forward, Vector3.up));
    StartCoroutine(GenerarApariciones(destino));
  }

  LineRenderer CrearAnilloEspectral(string nombreObjeto)
  {
    GameObject anilloGO = new GameObject(nombreObjeto);
    if (BattleManager.Instance != null)
    {
      anilloGO.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer anillo = anilloGO.AddComponent<LineRenderer>();
    anillo.useWorldSpace = true;
    anillo.alignment = LineAlignment.View;
    anillo.material = ObtenerMaterial();
    anillo.widthMultiplier = AnchoAnillo;
    anillo.loop = true;
    anillo.textureMode = LineTextureMode.Stretch;
    anillo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    anillo.receiveShadows = false;
    anillo.numCapVertices = 0;
    anillo.numCornerVertices = 4;
    anillo.positionCount = SegmentosAnillo;
    return anillo;
  }

  IEnumerator AnimarAnillo(LineRenderer anillo, Vector3 centro, float radio, float velocidadGiro, Color colorBase)
  {
    float tiempo = 0f;
    Vector3[] puntos = new Vector3[SegmentosAnillo];
    while (tiempo < DuracionAnillo)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / DuracionAnillo);
      if (anillo != null)
      {
        float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(factor / 0.1f));
        float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((factor - 0.72f) / 0.28f));
        float intensidad = entrada * salida;
        float pulso = 0.84f + 0.16f * Mathf.Sin(tiempo * 9f);
        float giro = tiempo * velocidadGiro;

        for (int i = 0; i < SegmentosAnillo; i++)
        {
          float angulo = (i / (float)SegmentosAnillo) * Mathf.PI * 2f + giro;
          float ruido = Mathf.Sin((angulo * 4f) + tiempo * 3f) * 0.02975f;
          float radioVivo = radio + ruido;
          Vector3 offset = new Vector3(Mathf.Cos(angulo) * radioVivo, Mathf.Sin((angulo * 3f) + tiempo * 4f) * 0.03825f + AlturaAnillo, Mathf.Sin(angulo) * radioVivo);
          puntos[i] = centro + offset;
        }

        anillo.SetPositions(puntos);
        anillo.startColor = new Color(colorBase.r, colorBase.g, colorBase.b, 0.68f * intensidad);
        anillo.endColor = new Color(colorBase.r * 0.45f, colorBase.g * 0.45f, colorBase.b * 0.45f, 0.26f * intensidad);
        anillo.widthMultiplier = AnchoAnillo * pulso * Mathf.Lerp(1.35f, 0.55f, factor);
      }
      yield return null;
    }

    if (anillo != null)
    {
      Destroy(anillo.gameObject);
    }
  }

  IEnumerator GenerarEsporas(Vector3 centro, Vector3 binormal, Vector3 tangent, Vector3 normal)
  {
    int particulas = 14;
    for (int i = 0; i < particulas; i++)
    {
      GameObject esporaGO = new GameObject("VFX_CondenaRaices_Espora");
      LineRenderer espora = esporaGO.AddComponent<LineRenderer>();
      espora.useWorldSpace = true;
      espora.alignment = LineAlignment.View;
      espora.material = ObtenerMaterial();
      espora.widthMultiplier = 0.02125f;
      espora.positionCount = 3;
      espora.textureMode = LineTextureMode.Stretch;
      espora.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      espora.receiveShadows = false;
      espora.numCapVertices = 2;
      espora.startColor = new Color(0.72f, 0.3f, 0.95f, 0.62f);
      espora.endColor = new Color(0.42f, 0.24f, 0.68f, 0f);

      float angulo = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      Vector3 direccion = (Mathf.Cos(angulo) * binormal + Mathf.Sin(angulo) * tangent).normalized;
      float alturaBase = AlturaAnillo + UnityEngine.Random.Range(0.017f, 0.136f);
      Vector3 inicio = centro + direccion * (RadioAnillo * 0.45f) + normal * alturaBase;
      Vector3 medio = inicio + direccion * UnityEngine.Random.Range(0.068f, 0.153f) + normal * UnityEngine.Random.Range(0.034f, 0.102f);
      Vector3 fin = inicio + direccion * UnityEngine.Random.Range(0.102f, 0.2125f) + normal * UnityEngine.Random.Range(-0.0425f, 0.017f);

      espora.SetPosition(0, inicio);
      espora.SetPosition(1, medio);
      espora.SetPosition(2, fin);

      StartCoroutine(DesvanecerEspora(espora, UnityEngine.Random.Range(0.65f, 0.9f)));
      yield return new WaitForSeconds(0.07f);
    }
  }

  IEnumerator GenerarApariciones(Vector3 centro)
  {
    yield return new WaitForSeconds(0.18f);

    const int cantidadApariciones = 6;
    for (int i = 0; i < cantidadApariciones; i++)
    {
      float angulo = (i / (float)cantidadApariciones) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.22f, 0.22f);
      Vector3 origen = centro + new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * UnityEngine.Random.Range(RadioAnillo * 0.25f, RadioAnillo * 0.85f);

      GameObject aparicionGO = new GameObject("VFX_CondenaRaices_Aparicion");
      LineRenderer aparicion = aparicionGO.AddComponent<LineRenderer>();
      aparicion.useWorldSpace = true;
      aparicion.alignment = LineAlignment.View;
      aparicion.material = ObtenerMaterial();
      aparicion.widthMultiplier = 0.0238f;
      aparicion.positionCount = 6;
      aparicion.textureMode = LineTextureMode.Stretch;
      aparicion.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      aparicion.receiveShadows = false;
      aparicion.numCapVertices = 4;
      aparicion.startColor = new Color(0.72f, 0.38f, 1f, 0f);
      aparicion.endColor = new Color(0.12f, 1f, 0.5f, 0f);

      StartCoroutine(AnimarAparicion(aparicion, origen, UnityEngine.Random.Range(1.05f, 1.35f), UnityEngine.Random.Range(0f, Mathf.PI * 2f)));
      yield return new WaitForSeconds(0.19f);
    }
  }

  IEnumerator AnimarAparicion(LineRenderer aparicion, Vector3 origen, float duracion, float fase)
  {
    float tiempo = 0f;
    Vector3[] puntos = new Vector3[6];
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / duracion);
      float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(factor / 0.22f));
      float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((factor - 0.58f) / 0.42f));
      float intensidad = entrada * salida;

      if (aparicion != null)
      {
        for (int i = 0; i < puntos.Length; i++)
        {
          float tramo = i / (float)(puntos.Length - 1);
          float ondulacion = Mathf.Sin((tramo * 7f) + (tiempo * 8f) + fase) * (0.0153f + tramo * 0.02125f);
          float profundidad = Mathf.Cos((tramo * 5f) + (tiempo * 6f) + fase) * 0.0153f;
          puntos[i] = origen + new Vector3(ondulacion, AlturaAnillo + tramo * 0.408f + factor * 0.102f, profundidad);
        }

        aparicion.SetPositions(puntos);
        aparicion.startColor = new Color(0.76f, 0.34f, 0.96f, 0.48f * intensidad);
        aparicion.endColor = new Color(0.42f, 0.26f, 0.72f, 0.05f * intensidad);
        aparicion.widthMultiplier = Mathf.Lerp(0.02975f, 0.00765f, factor) * intensidad;
      }
      yield return null;
    }

    if (aparicion != null)
    {
      Destroy(aparicion.gameObject);
    }
  }

  IEnumerator DesvanecerEspora(LineRenderer espora, float duracion)
  {
    float tiempo = 0f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / duracion);
      if (espora != null)
      {
        Color inicio = espora.startColor;
        Color fin = espora.endColor;
        espora.startColor = new Color(inicio.r, inicio.g, inicio.b, Mathf.Lerp(0.7f, 0f, factor));
        espora.endColor = new Color(fin.r, fin.g, fin.b, Mathf.Lerp(0.15f, 0f, factor));
        espora.widthMultiplier = Mathf.Lerp(0.02125f, 0f, factor);
      }
      yield return null;
    }

    if (espora != null)
    {
      Destroy(espora.gameObject);
    }
  }

  static Material ObtenerMaterial()
  {
    if (materialAnillo == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialAnillo = new Material(shader);
      materialAnillo.name = "Mat_CondenaRaices";

      if (materialAnillo.HasProperty("_TintColor"))
      {
        materialAnillo.SetColor("_TintColor", new Color(0.65f, 0.3f, 0.9f, 0.72f));
      }

      if (materialAnillo.HasProperty("_EmissionColor"))
      {
        materialAnillo.EnableKeyword("_EMISSION");
        materialAnillo.SetColor("_EmissionColor", new Color(0.32f, 0.12f, 0.48f) * 1.8f);
      }
    }

    return materialAnillo;
  }

  void ReproducirSfxCondenaBosque()
  {
    if (sfxCondenaBosque == null)
    {
      return;
    }

    Vector3 posicion = scEstaUnidad != null && scEstaUnidad.puntoEntrante != null
      ? scEstaUnidad.puntoEntrante.position
      : transform.position;
    AjustesAudio.ReproducirClipEnPunto(sfxCondenaBosque, posicion, Mathf.Clamp01(volumenSfxCondenaBosque));
  }
}
