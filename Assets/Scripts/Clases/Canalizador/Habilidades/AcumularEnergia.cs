using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class AcumularEnergia : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acumular Energía";
      IDenClase = 0; // Intrínseca
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumularEnergia");

       
      ActualizarDescripcion();
    
    }

        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();
      int poderActual = statsUI.Poder;
      ClaseCanalizador canalizador = scEstaUnidad as ClaseCanalizador;
      int nivelAcumulacionProtegida = canalizador != null ? canalizador.PASIVA_AcumulacionProtegida : 0;
      int energiaActual = canalizador != null ? canalizador.ObtenerEnergia() : 0;
      int barreraProtegida = 1 + poderActual + 3 * energiaActual;
      if (nivelAcumulacionProtegida > 1) { barreraProtegida += 2; }
      if (nivelAcumulacionProtegida == 4) { barreraProtegida += 4; }
      int tsMentalProtegida = nivelAcumulacionProtegida > 2 ? 2 : 1;
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}";
      string dcConcentracion = "10 + damage / 3";
      string dcConcentracionPt = "10 + dano / 3";
      string dcConcentracionEs = "10 + daño / 3";

      string titulo = esIngles ? "Gather Energy" : esPortugues ? "Acumular Energia" : "Acumular Energia";
      string subtitulo = esIngles
        ? "The Channeler enters concentration to increase their Energy tier at the start of the next turn."
        : esPortugues
          ? "O Canalizador entra em concentracao para aumentar seu Nivel de Energía no inicio do próximo turno."
          : "El Canalizador entra en concentracion para aumentar su Nivel de Energía al inicio de su siguiente turno.";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Self\n";
        cuerpo += "<b>Target:</b> Self\n";
        cuerpo += "<b>Effect on cast:</b> Applies <b>Gathering</b> buff (1 round)\n";
        cuerpo += "<b>If concentration is maintained:</b> +1 Energy Tier on next turn\n";
        cuerpo += "<b>Energy I:</b> +10% Damage, +5% Critical\n";
        cuerpo += "<b>Energy II:</b> +15% Damage, +1 Max AP\n";
        cuerpo += "<b>Energy III:</b> +15% Damage, +1 Max AP, +5% Critical";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Propria\n";
        cuerpo += "<b>Alvo:</b> O proprio usuario\n";
        cuerpo += "<b>Efeito ao ativar:</b> Aplica buff <b>Acumulando</b> (1 rodada)\n";
        cuerpo += "<b>Se mantiver a concentracao:</b> +1 Nivel de Energía no próximo turno\n";
        cuerpo += "<b>Energia I:</b> +10% Dano, +5% Critico\n";
        cuerpo += "<b>Energia II:</b> +15% Dano, +1 AP Maximo\n";
        cuerpo += "<b>Energia III:</b> +15% Dano, +1 AP Maximo, +5% Critico";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Propia\n";
        cuerpo += "<b>Objetivo:</b> Propio usuario\n";
        cuerpo += "<b>Efecto al activar:</b> Aplica buff <b>Acumulando</b> (1 ronda)\n";
        cuerpo += "<b>Si mantiene la concentracion:</b> +1 Nivel de Energía al siguiente turno\n";
        cuerpo += "<b>Energía I:</b> +10% Daño, +5% Crítico\n";
        cuerpo += "<b>Energía II:</b> +15% Daño, +1 AP Máximo\n";
        cuerpo += "<b>Energía III:</b> +15% Daño, +1 AP Máximo, +5% Crítico";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}"
          : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(titulo, subtitulo, cuerpo, costos, "#5dade2");
      string subtituloFormato = esIngles
        ? "Start gathering energy; if concentration holds, gain +1 Energy Level next turn."
        : esPortugues
          ? "Começa a acumular energia; se mantiver concentração, ganha +1 Energia no próximo turno."
          : "Empieza a acumular energía; si mantiene concentración, gana +1 Energía el próximo turno.";

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Self buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Self</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>On cast:</b></color> <color={colorValor}>{iconoEnergia} Gathering for 1 round; ends turn</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>If maintained:</b></color> <color={colorValor}>{iconoEnergia} +1 Energy Tier next turn</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>When damaged:</b></color> <color={colorValor}>Mental Save vs DC {dcConcentracion}; on failed save loses Gathering</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy I:</b></color> <color={colorValor}>+10% Damage, +5% Critical</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy II:</b></color> <color={colorValor}>+15% Damage, +1 Max AP</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energy III:</b></color> <color={colorValor}>+15% Damage, +1 Max AP, +5% Critical</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Protected Gathering:</b></color> <color={colorValor}>{barreraProtegida} Barrier, +{tsMentalProtegida} Mental Save{(nivelAcumulacionProtegida == 5 ? ", +1 Max AP" : "")}</color>";
        }
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>O próprio usuário</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao ativar:</b></color> <color={colorValor}>{iconoEnergia} Acumulando por 1 rodada; termina o turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Se mantiver:</b></color> <color={colorValor}>{iconoEnergia} +1 Nível de Energía no próximo turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ao receber dano:</b></color> <color={colorValor}>Resistência Mental vs CD {dcConcentracionPt}; se falhar, perde Acumulando</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia I:</b></color> <color={colorValor}>+10% Dano, +5% Crítico</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia II:</b></color> <color={colorValor}>+15% Dano, +1 AP Máximo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energia III:</b></color> <color={colorValor}>+15% Dano, +1 AP Máximo, +5% Crítico</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Acumulação Protegida:</b></color> <color={colorValor}>{barreraProtegida} Barreira, +{tsMentalProtegida} Resistência Mental{(nivelAcumulacionProtegida == 5 ? ", +1 AP Max" : "")}</color>";
        }
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Propio usuario</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Al activar:</b></color> <color={colorValor}>{iconoEnergia} Acumulando por 1 ronda; termina el turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Si mantiene:</b></color> <color={colorValor}>{iconoEnergia} +1 Nivel de Energía el próximo turno</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Al recibir daño:</b></color> <color={colorValor}>TS Mental vs DC {dcConcentracionEs}; si falla, pierde Acumulando</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía I:</b></color> <color={colorValor}>+10% Daño, +5% Crítico</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía II:</b></color> <color={colorValor}>+15% Daño, +1 AP Máximo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Energía III:</b></color> <color={colorValor}>+15% Daño, +1 AP Máximo, +5% Crítico</color>";
        if (nivelAcumulacionProtegida > 0)
        {
          cuerpoFormato += $"\n<color={colorEncabezado}><b>Acumulación Protegida:</b></color> <color={colorValor}>{barreraProtegida} Barrera, +{tsMentalProtegida} TS Mental{(nivelAcumulacionProtegida == 5 ? ", +1 AP Max" : "")}</color>";
        }
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoFormato;
    }

    public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
      await  base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

     ClaseCanalizador scClaseCana = (ClaseCanalizador)scEstaUnidad;
     int NivelAcumulacionProtegida = scClaseCana.PASIVA_AcumulacionProtegida;
    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acumulando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
      
       if (NivelAcumulacionProtegida > 0)
       {
      
        int factorBarrera = (int)(1 + scEstaUnidad.mod_CarPoder + 3 * scClaseCana.ObtenerEnergia());
        buff.cantBarrera += factorBarrera;
        if (NivelAcumulacionProtegida > 1) { buff.cantBarrera += 2; }
        if (NivelAcumulacionProtegida == 4) { buff.cantBarrera += 4; }
        if (NivelAcumulacionProtegida == 5) { buff.cantAPMax += 1; }


        buff.cantTsMental += 1;
        if (NivelAcumulacionProtegida > 2) {  buff.cantTsMental += 1; }

       }
       
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);
        // Mantener pose de habilidad mientras dura "Acumulando"
        var poseCtrl = objetivo.GetComponent<UnidadPoseController>();
        if (poseCtrl != null)
        {
            poseCtrl.EnterSkillPoseHold();
        }







      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();

       
      }
    }
    
         void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AcumularEnergia");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  
   AcumularEnergiaCanalizadorFx.Crear(objetivo.GetComponent<Unidad>());

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
        if(c.Presente == null)
        {
            continue;
        }
        
       
        if(c.Presente.GetComponent<Unidad>() == null)
        {
            continue;
        }
           
        if(c.Presente.GetComponent<Unidad>() != null)
        {
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }
 
}

public class AcumularEnergiaCanalizadorFx : MonoBehaviour
{
  private const float Duracion = 1.55f;
  private const int CantidadDescargas = 5;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private RectTransform imagenUnidad;
  private Image auraExterior;
  private Image auraInterior;
  private Image brilloNucleo;
  private readonly Image[] descargas = new Image[CantidadDescargas];
  private readonly float[] fasesDescarga = new float[CantidadDescargas];
  private readonly float[] angulosDescarga = new float[CantidadDescargas];
  private readonly float[] largosDescarga = new float[CantidadDescargas];
  private float tiempo;
  private Vector2 tamanoBase;
  private bool inestable;

  private static Sprite spriteSuave;
  private static Sprite spriteDescarga;
  private static Texture2D texturaSuave;
  private static Texture2D texturaDescarga;

  public static void Crear(Unidad unidad)
  {
    Crear(unidad, false);
  }

  public static void CrearInestable(Unidad unidad)
  {
    Crear(unidad, true);
  }

  private static void Crear(Unidad unidad, bool modoInestable)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    RectTransform imagen = unidad.uImage.rectTransform;
    if (imagen == null || !(imagen.parent is RectTransform padre))
    {
      return;
    }

    GameObject go = new GameObject("AcumularEnergiaCanalizadorFx", typeof(RectTransform), typeof(CanvasGroup), typeof(AcumularEnergiaCanalizadorFx));
    AcumularEnergiaCanalizadorFx fx = go.GetComponent<AcumularEnergiaCanalizadorFx>();
    fx.Inicializar(padre, imagen, modoInestable);

    Canvas canvas = unidad.uImage.GetComponentInParent<Canvas>(true);
    RenderOrderHelper.OrdenarCanvasEncima(canvas, unidad.transform, modoInestable ? 9 : 7);
  }

  private void Inicializar(RectTransform padre, RectTransform imagen, bool modoInestable)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    imagenUnidad = imagen;
    inestable = modoInestable;
    root.SetParent(padre, false);
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    tamanoBase = imagenUnidad.rect.size;
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = imagenUnidad.sizeDelta;
    }
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = new Vector2(36f, 42f);
    }

    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.anchoredPosition = imagenUnidad.anchoredPosition + new Vector2(0f, 2f);
    root.localScale = imagenUnidad.localScale;
    root.sizeDelta = tamanoBase * 1.42f;

    int targetSibling = Mathf.Min(padre.childCount - 1, imagenUnidad.GetSiblingIndex() + 1);
    root.SetSiblingIndex(targetSibling);

    auraExterior = CrearImagen("AuraExterior", ObtenerSpriteSuave(), root);
    auraInterior = CrearImagen("AuraInterior", ObtenerSpriteSuave(), root);
    brilloNucleo = CrearImagen("BrilloNucleo", ObtenerSpriteSuave(), root);

    for (int i = 0; i < descargas.Length; i++)
    {
      descargas[i] = CrearImagen("Descarga" + i, ObtenerSpriteDescarga(), root);
      fasesDescarga[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      angulosDescarga[i] = UnityEngine.Random.Range(inestable ? -76f : -38f, inestable ? 76f : 38f) + (i % 2 == 0 ? 90f : 0f);
      largosDescarga[i] = UnityEngine.Random.Range(inestable ? 0.32f : 0.42f, inestable ? 0.9f : 0.72f);
    }

    ActualizarVisual(0f);
  }

  private Image CrearImagen(string nombre, Sprite sprite, RectTransform padre)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float t = Mathf.Clamp01(tiempo / Duracion);
    ActualizarVisual(t);

    if (tiempo >= Duracion)
    {
      Destroy(gameObject);
    }
  }

  private void ActualizarVisual(float t)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.18f));
    float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.48f) / 0.52f));
    float intensidad = entrada * salida;
    float velocidadPulso = inestable ? 19f : 12f;
    float pulso = 0.92f + ((inestable ? 0.16f : 0.08f) * Mathf.Sin(Time.time * velocidadPulso));
    float temblor = inestable ? Mathf.Sin((Time.time * 31f) + fasesDescarga[0]) * 1.8f : 0f;
    float escala = Mathf.Lerp(inestable ? 0.7f : 0.74f, inestable ? 1.28f : 1.18f, Mathf.SmoothStep(0f, 1f, t));

    canvasGroup.alpha = intensidad;
    root.anchoredPosition = imagenUnidad != null ? imagenUnidad.anchoredPosition + new Vector2(0f, 2f) : root.anchoredPosition;
    root.localEulerAngles = new Vector3(0f, 0f, temblor);

    Color colorAuraExterior = inestable ? new Color(0.34f, 0.68f, 1f, 0.2f * intensidad) : new Color(0.22f, 0.72f, 1f, 0.18f * intensidad);
    Color colorAuraInterior = inestable ? new Color(0.84f, 0.32f, 0.38f, 0.2f * intensidad) : new Color(0.65f, 0.94f, 1f, 0.28f * intensidad);
    Color colorNucleo = inestable ? new Color(0.9f, 0.7f, 0.78f, 0.38f * intensidad) : new Color(0.82f, 0.98f, 1f, 0.34f * intensidad);

    Configurar(auraExterior, Vector2.zero, tamanoBase * (1.72f * escala), 0f, colorAuraExterior);
    Configurar(auraInterior, Vector2.zero, tamanoBase * (1.08f * escala * pulso), 0f, colorAuraInterior);
    Configurar(brilloNucleo, new Vector2(0f, tamanoBase.y * 0.05f), tamanoBase * (0.52f + (0.16f * pulso)), 0f, colorNucleo);

    for (int i = 0; i < descargas.Length; i++)
    {
      float velocidadChispa = inestable ? 31f : 18f;
      float chispa = Mathf.Clamp01(Mathf.Sin((Time.time * velocidadChispa) + fasesDescarga[i]) * 0.5f + 0.5f);
      chispa = Mathf.Pow(chispa, inestable ? 2.1f : 3.1f) * intensidad;
      float desplazamientoErratico = inestable ? Mathf.Sin((Time.time * 23f) + (fasesDescarga[i] * 1.7f)) * tamanoBase.x * 0.08f : 0f;
      float radioX = tamanoBase.x * Mathf.Lerp(-0.34f, 0.34f, (i + 0.5f) / descargas.Length) + desplazamientoErratico;
      float radioY = tamanoBase.y * (i % 2 == 0 ? 0.22f : -0.08f);
      Vector2 posicion = new Vector2(radioX, radioY + (Mathf.Sin((Time.time * (inestable ? 8.6f : 4.2f)) + fasesDescarga[i]) * (inestable ? 2.7f : 1.2f)));
      Vector2 tamano = new Vector2(tamanoBase.x * largosDescarga[i] * (0.86f + (0.16f * pulso)), Mathf.Max(2f, tamanoBase.y * (inestable ? 0.058f : 0.045f)));
      Color colorDescarga = inestable && i % 3 == 0
        ? new Color(1f, 0.34f, 0.32f, 0.34f * chispa)
        : new Color(0.7f, 0.95f, 1f, (inestable ? 0.5f : 0.42f) * chispa);
      Configurar(descargas[i], posicion, tamano, angulosDescarga[i] + (Mathf.Sin((Time.time * (inestable ? 15f : 7.5f)) + fasesDescarga[i]) * (inestable ? 18f : 7f)), colorDescarga);
    }
  }

  private static void Configurar(Image image, Vector2 posicion, Vector2 tamano, float rotacionZ, Color color)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacionZ);
    rect.localScale = Vector3.one;
    image.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 64;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "AcumularEnergiaSoftGlowRuntime";
    texturaSuave.wrapMode = TextureWrapMode.Clamp;
    texturaSuave.filterMode = FilterMode.Bilinear;
    texturaSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.25f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "AcumularEnergiaSoftGlowRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteDescarga()
  {
    if (spriteDescarga != null)
    {
      return spriteDescarga;
    }

    const int width = 96;
    const int height = 16;
    texturaDescarga = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaDescarga.name = "AcumularEnergiaElectricRuntime";
    texturaDescarga.wrapMode = TextureWrapMode.Clamp;
    texturaDescarga.filterMode = FilterMode.Bilinear;
    texturaDescarga.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = x / (width - 1f);
        float centroLinea = centroY + Mathf.Sin(nx * Mathf.PI * 5.5f) * 2.2f + Mathf.Sin(nx * Mathf.PI * 13f) * 0.8f;
        float distanciaY = Mathf.Abs(y - centroLinea);
        float grosor = Mathf.Clamp01(1f - (distanciaY / 3.6f));
        float extremos = Mathf.SmoothStep(0f, 0.18f, nx) * (1f - Mathf.SmoothStep(0.82f, 1f, nx));
        float alpha = Mathf.Pow(grosor, 1.65f) * extremos;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaDescarga.SetPixels(pixels);
    texturaDescarga.Apply(false, true);
    spriteDescarga = Sprite.Create(texturaDescarga, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteDescarga.name = "AcumularEnergiaElectricRuntime";
    return spriteDescarga;
  }
}





