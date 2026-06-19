using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class SifonArcano : Habilidad
{
   
      public override void  Awake()
    {
      nombre = "Sifón Arcano";
      IDenClase = 7;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_SifonArcano");
      ActualizarDescripcion();
     
  }
  public override void ActualizarDescripcion()
  {
    int duracionTurnos = NIVEL == 5 ? 4 : 3;
    int bonusDanioBase = NIVEL > 1 ? 5 : 3;
    string rangoDanioEs = FormatearRangoDados(1, 10, bonusDanioBase);

    string tituloEs = "Sifon Arcano I";
    string tituloEn = "Arcane Siphon I";
    if (NIVEL == 2) { tituloEs = "Sifon Arcano II"; tituloEn = "Arcane Siphon II"; }
    if (NIVEL == 3) { tituloEs = "Sifon Arcano III"; tituloEn = "Arcane Siphon III"; }
    if (NIVEL == 4) { tituloEs = "Sifon Arcano IV a"; tituloEn = "Arcane Siphon IV a"; }
    if (NIVEL == 5) { tituloEs = "Sifon Arcano IV b"; tituloEn = "Arcane Siphon IV b"; }

    string lineaDanioEs = bonusDanioBase > 0
      ? $"<b>Daño por turno:</b> ({rangoDanioEs}) x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano"
      : "<b>Daño por turno:</b> 1-10 x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano";
    string lineaDanioEn = bonusDanioBase > 0
      ? $"<b>Turn Damage:</b> (1d10 + {bonusDanioBase}) x (1 + Energy Residues) | <b>Type:</b> Arcane"
      : "<b>Turn Damage:</b> 1d10 x (1 + Energy Residues) | <b>Type:</b> Arcane";

    bool esInglesFormato = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortuguesFormato = TRADU.i != null && TRADU.i.nIdioma == 3;
    string tituloPtFormato = "Sifao Arcano I";
    if (NIVEL == 2) { tituloPtFormato = "Sifao Arcano II"; }
    if (NIVEL == 3) { tituloPtFormato = "Sifao Arcano III"; }
    if (NIVEL == 4) { tituloPtFormato = "Sifao Arcano IV a"; }
    if (NIVEL == 5) { tituloPtFormato = "Sifao Arcano IV b"; }
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
    string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";
    string tituloFormato = esInglesFormato ? tituloEn : esPortuguesFormato ? tituloPtFormato : tituloEs;
    string subtituloFormato = esInglesFormato
      ? "Applies a turn-damage siphon; damage scales with Energy Residues."
      : esPortuguesFormato
        ? "Aplica dano por turno; o dano escala com Residuos Energeticos."
        : "Aplica daño por turno; el daño escala con Residuos Energeticos.";
    string cuerpoFormato = "";
    if (esInglesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged debuff (5 range)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy on the opposite side</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Effect:</b></color> <color={colorValor}>{iconoDebuff} Applies Arcane Siphon for {duracionTurnos} turns</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Turn damage:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Energy Residues). Type: Arcane</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On kill:</b></color> <color={colorValor}>{iconoBuff} +1 permanent Max AP, +10% Damage, {iconoEnergia} +1 Energy</color>";
    }
    else if (esPortuguesFormato)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Debuff a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo no lado oposto</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Efeito:</b></color> <color={colorValor}>{iconoDebuff} Aplica Sifao Arcano por {duracionTurnos} turnos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Dano por turno:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Residuos Energeticos). Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao matar:</b></color> <color={colorValor}>{iconoBuff} +1 AP Max permanente, +10% Dano, {iconoEnergia} +1 Energia</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Debuff a distancia (5 alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo del lado opuesto</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Efecto:</b></color> <color={colorValor}>{iconoDebuff} Aplica Sifon Arcano por {duracionTurnos} turnos</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Daño por turno:</b></color> <color={colorValor}>{rangoDanioEs} x (1 + {iconoEnergia} Residuos Energeticos). Tipo: Arcano</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al matar:</b></color> <color={colorValor}>{iconoBuff} +1 AP Max permanente, +10% Daño, {iconoEnergia} +1 Energía</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{tituloFormato}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
    {
      if (esInglesFormato)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base turn damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: unlocks IV upgrade path.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A or B (IV upgrades).</color>"; }
      }
      else if (esPortuguesFormato)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no dano base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: desbloqueia melhorias IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A ou Opcao B (melhorias IV).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al daño base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: desbloquea mejoras IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A u Opción B (mejoras IV).</color>"; }
      }
    }
    if (!string.IsNullOrEmpty(txtDescripcion))
    {
      return;
    }

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string cuerpo = "";
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy unit on the opposite side\n";
      cuerpo += $"<b>Effect:</b> Applies Arcane Siphon for {duracionTurnos} turns\n";
      cuerpo += lineaDanioEn + "\n";
      cuerpo += "<b>On kill by this effect:</b> +1 permanent AP max, +10% Damage and +1 Energy";

      string costos = $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEn,
        "Marks the target with an unstable link that drains vitality over time, amplified by Energy Residues.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base turn damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: unlocks IV upgrade path.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A or B (IV upgrades).</color>"; }
      }
      return;
    }
    if (TRADU.i != null && TRADU.i.nIdioma == 3)
    {
      string corpo = "";
      corpo += "<b>Tipo:</b> Distancia (5 alcance)\n";
      corpo += "<b>Alvo:</b> 1 unidade inimiga do lado oposto\n";
      corpo += $"<b>Efeito:</b> aplica Sifao Arcano por {duracionTurnos} turnos\n";
      corpo += (bonusDanioBase > 0
        ? $"<b>Dano por turno:</b> (1d10 + {bonusDanioBase}) x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano"
        : "<b>Dano por turno:</b> 1d10 x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano");
      corpo += "\n";
      corpo += "<b>Se matar com este efeito:</b> +1 AP max permanente, +10% Dano e +1 Energia";

      string costos = $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        "Sifao Arcano",
        "Marca o alvo com um vinculo instavel que drena vitalidade por turnos, amplificado por Residuos Energeticos.",
        corpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no dano base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: desbloqueia melhorias IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A ou Opcao B (melhorias IV).</color>"; }
      }
      return;
    }

    {
      string cuerpo = "";
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 unidad enemiga del lado opuesto\n";
      cuerpo += $"<b>Efecto:</b> aplica Sifon Arcano por {duracionTurnos} turnos\n";
      cuerpo += lineaDanioEs + "\n";
      cuerpo += "<b>Si mata con este efecto:</b> +1 AP max permanente, +10% Daño y +1 Energía";

      string costos = $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEs,
        "Marca al objetivo con un vinculo inestable que drena vitalidad por turnos, amplificado por Residuos Energeticos.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al daño base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: desbloquea mejoras IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A u Opción B (mejoras IV).</color>"; }
      }
    }
  }
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if (obj is Unidad uni) //Acá van los efectos a Unidades.
    {
      //Agrega la reacción 
      ReaccionSifonArcano reaccion = new ReaccionSifonArcano();
      reaccion.variableUnidad = scEstaUnidad;
      reaccion.NIVEL = NIVEL;
      reaccion.nombre = "Sifón Arcano";
      reaccion.variableUnidad = scEstaUnidad;
      ReaccionSifonArcano reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, uni.gameObject);
      reaccionPosturaDefensiva.vfxSifon = VFXAplicar(uni.gameObject);

    }
     
    }

  protected override float? CalcularProbabilidadEspecialSobreObjetivo(Unidad objetivo)
  {
    if (objetivo == null || scEstaUnidad == null)
    {
      return null;
    }

    return 1f;
  }

  protected override string ObtenerTextoProbabilidadSobreObjetivo(Unidad objetivo, float probabilidad)
  {
    return FormatearTextoProbabilidadExito(probabilidad);
  }
    
       SifonArcanoObjetivoFx VFXAplicar(GameObject objetivo)
  {
   GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_SifonArcano");
   if (vfxPrefab != null)
   {
    GameObject vfx = Instantiate(vfxPrefab, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
    VFXSoloSonido.OcultarVisuales(vfx);
   }

   return SifonArcanoObjetivoFx.Crear(objetivo.GetComponent<Unidad>());

  }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
     
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
      foreach(Casilla c in lCasillasafectadas)
      {
        c.ActivarCapaColorRojo();
        
        if(c.Presente == null)
        {
            continue;
        }
       
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          c.ActivarCapaColorRojo();
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
         
        }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
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


public class SifonArcanoObjetivoFx : MonoBehaviour
{
  private const float DuracionEntrada = 0.18f;
  private const float DuracionSalida = 0.28f;
  private const int CantidadHebras = 5;
  private const float EscalaVisual = 0.75f;
  private const float OpacidadVisual = 0.5f;
  private const float VelocidadVisual = 0.72f;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private RectTransform imagenUnidad;
  private Image columna;
  private Image nucleo;
  private Image anilloSuperior;
  private Image anilloInferior;
  private readonly Image[] hebras = new Image[CantidadHebras];
  private readonly float[] fasesHebra = new float[CantidadHebras];
  private readonly float[] offsetHebraX = new float[CantidadHebras];
  private readonly float[] velocidadHebra = new float[CantidadHebras];
  private readonly float[] escalaHebra = new float[CantidadHebras];
  private float tiempo;
  private float tiempoSalida;
  private bool deteniendo;
  private Vector2 tamanoBase;
  private Vector2 posicionBase;

  private static Sprite spriteSuave;
  private static Sprite spriteAnillo;
  private static Sprite spriteHebra;
  private static Texture2D texturaSuave;
  private static Texture2D texturaAnillo;
  private static Texture2D texturaHebra;

  public static SifonArcanoObjetivoFx Crear(Unidad unidad)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return null;
    }

    RectTransform imagen = unidad.uImage.rectTransform;
    if (imagen == null || !(imagen.parent is RectTransform padre))
    {
      return null;
    }

    GameObject go = new GameObject("SifonArcanoObjetivoFx", typeof(RectTransform), typeof(CanvasGroup), typeof(SifonArcanoObjetivoFx));
    SifonArcanoObjetivoFx fx = go.GetComponent<SifonArcanoObjetivoFx>();
    fx.Inicializar(padre, imagen);

    Canvas canvas = unidad.uImage.GetComponentInParent<Canvas>(true);
    RenderOrderHelper.OrdenarCanvasEncima(canvas, unidad.transform, 8);
    return fx;
  }

  private void Inicializar(RectTransform padre, RectTransform imagen)
  {
    root = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    imagenUnidad = imagen;
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

    posicionBase = new Vector2(0f, tamanoBase.y * 0.08f);
    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.anchoredPosition = imagenUnidad.anchoredPosition + posicionBase;
    root.localScale = imagenUnidad.localScale * EscalaVisual;
    root.sizeDelta = new Vector2(tamanoBase.x * 0.7f, tamanoBase.y * 1.55f);

    int targetSibling = Mathf.Min(padre.childCount - 1, imagenUnidad.GetSiblingIndex() + 1);
    root.SetSiblingIndex(targetSibling);

    columna = CrearImagen("Columna", ObtenerSpriteSuave(), root);
    nucleo = CrearImagen("Nucleo", ObtenerSpriteSuave(), root);
    anilloSuperior = CrearImagen("AnilloSuperior", ObtenerSpriteAnillo(), root);
    anilloInferior = CrearImagen("AnilloInferior", ObtenerSpriteAnillo(), root);

    for (int i = 0; i < hebras.Length; i++)
    {
      hebras[i] = CrearImagen("Hebra" + i, ObtenerSpriteHebra(), root);
      fasesHebra[i] = UnityEngine.Random.Range(0f, 1f);
      offsetHebraX[i] = UnityEngine.Random.Range(-0.32f, 0.32f);
      velocidadHebra[i] = UnityEngine.Random.Range(0.65f, 1.35f);
      escalaHebra[i] = UnityEngine.Random.Range(0.68f, 1.08f);
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
    float intensidad = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / DuracionEntrada));

    if (deteniendo)
    {
      tiempoSalida += Time.deltaTime;
      intensidad *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempoSalida / DuracionSalida));
    }

    ActualizarVisual(intensidad);

    if (deteniendo && tiempoSalida >= DuracionSalida)
    {
      Destroy(gameObject);
    }
  }

  public void Detener()
  {
    if (deteniendo)
    {
      return;
    }

    deteniendo = true;
    tiempoSalida = 0f;
  }

  private void ActualizarVisual(float intensidad)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float tiempoFx = Time.time * VelocidadVisual;
    intensidad *= OpacidadVisual;
    float pulso = 0.9f + (0.08f * Mathf.Sin(tiempoFx * 10f)) + (0.04f * Mathf.Sin(tiempoFx * 21f));
    float giro = Mathf.Sin(tiempoFx * 7.5f) * 7f;
    float deriva = Mathf.Sin(tiempoFx * 13f) * tamanoBase.x * 0.025f;

    canvasGroup.alpha = intensidad;
    root.anchoredPosition = imagenUnidad != null ? imagenUnidad.anchoredPosition + posicionBase + new Vector2(deriva, 0f) : root.anchoredPosition;
    root.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(tiempoFx * 11f) * 2.5f * intensidad);

    Configurar(columna, new Vector2(Mathf.Sin(tiempoFx * 18f) * tamanoBase.x * 0.018f, 0f), new Vector2(tamanoBase.x * 0.5f * pulso, tamanoBase.y * 1.48f), 0f, new Color(0.08f, 0.42f, 1f, 0.14f * intensidad));
    Configurar(nucleo, new Vector2(Mathf.Sin(tiempoFx * 15f) * tamanoBase.x * 0.025f, tamanoBase.y * 0.02f), new Vector2(tamanoBase.x * 0.14f, tamanoBase.y * 1.22f), 0f, new Color(0.36f, 0.68f, 1f, 0.25f * intensidad));
    Configurar(anilloSuperior, new Vector2(Mathf.Sin(tiempoFx * 9f) * tamanoBase.x * 0.03f, tamanoBase.y * 0.44f), new Vector2(tamanoBase.x * (0.5f + (0.08f * pulso)), tamanoBase.y * 0.15f), giro, new Color(0.24f, 0.56f, 1f, 0.32f * intensidad));
    Configurar(anilloInferior, new Vector2(Mathf.Sin(tiempoFx * 8f + 1.3f) * tamanoBase.x * 0.025f, -tamanoBase.y * 0.38f), new Vector2(tamanoBase.x * (0.46f + (0.06f * pulso)), tamanoBase.y * 0.12f), -giro * 1.25f, new Color(0.12f, 0.44f, 1f, 0.22f * intensidad));

    for (int i = 0; i < hebras.Length; i++)
    {
      float avance = Mathf.Repeat((tiempoFx * velocidadHebra[i] * 0.45f) + fasesHebra[i], 1f);
      float alphaHebra = Mathf.Sin(avance * Mathf.PI) * intensidad;
      float onda = Mathf.Sin((tiempoFx * (8f + i)) + (i * 1.9f)) * tamanoBase.x * 0.06f;
      Vector2 posicion = new Vector2((tamanoBase.x * offsetHebraX[i]) + onda, Mathf.Lerp(-tamanoBase.y * 0.56f, tamanoBase.y * 0.6f, avance));
      Vector2 tamano = new Vector2(tamanoBase.y * 0.5f * escalaHebra[i], Mathf.Max(1.3f, tamanoBase.x * 0.04f));
      Configurar(hebras[i], posicion, tamano, 90f + (Mathf.Sin(tiempoFx * (7f + i) + fasesHebra[i]) * 10f), new Color(0.44f, 0.74f, 1f, 0.28f * alphaHebra));
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
    texturaSuave.name = "SifonArcanoSoftRuntime";
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
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.1f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "SifonArcanoSoftRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteAnillo()
  {
    if (spriteAnillo != null)
    {
      return spriteAnillo;
    }

    const int size = 64;
    texturaAnillo = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAnillo.name = "SifonArcanoRingRuntime";
    texturaAnillo.wrapMode = TextureWrapMode.Clamp;
    texturaAnillo.filterMode = FilterMode.Bilinear;
    texturaAnillo.hideFlags = HideFlags.HideAndDontSave;

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
        float borde = Mathf.Abs(distancia - 0.58f);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - (borde / 0.15f)), 1.75f) * Mathf.Clamp01(1f - distancia);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAnillo.SetPixels(pixels);
    texturaAnillo.Apply(false, true);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteAnillo.name = "SifonArcanoRingRuntime";
    return spriteAnillo;
  }

  private static Sprite ObtenerSpriteHebra()
  {
    if (spriteHebra != null)
    {
      return spriteHebra;
    }

    const int width = 72;
    const int height = 12;
    texturaHebra = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaHebra.name = "SifonArcanoStrandRuntime";
    texturaHebra.wrapMode = TextureWrapMode.Clamp;
    texturaHebra.filterMode = FilterMode.Bilinear;
    texturaHebra.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = x / (width - 1f);
        float distanciaY = Mathf.Abs(y - centroY);
        float grosor = Mathf.Clamp01(1f - (distanciaY / 3.4f));
        float extremos = Mathf.SmoothStep(0f, 0.2f, nx) * (1f - Mathf.SmoothStep(0.8f, 1f, nx));
        float alpha = Mathf.Pow(grosor, 1.8f) * extremos;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaHebra.SetPixels(pixels);
    texturaHebra.Apply(false, true);
    spriteHebra = Sprite.Create(texturaHebra, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteHebra.name = "SifonArcanoStrandRuntime";
    return spriteHebra;
  }
}
