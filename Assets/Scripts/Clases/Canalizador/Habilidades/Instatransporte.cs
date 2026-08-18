using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Instatransporte : Habilidad
{


  [SerializeField] private GameObject VFXenObjetivo;

    public override void  Awake()
    {
    nombre = "Instatransporte";
    IDenClase = 3;
    costoAP = 1;
    costoPM = 1;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 0;
    esforzable = 0;
    esCargable = false;
    esMelee = false;
    esHostil = false;
    cooldownMax = 5;
    if (NIVEL > 1) { cooldownMax--; }
    bAfectaObstaculos = false;
    poneTrampas = true;

    imHab = Resources.Load<Sprite>("imHab/Canalizador_Instatransporte");




  }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    string tituloEs = "Instatransporte I";
    string tituloEn = "Instatransport I";
    string tituloPt = "Instatransporte I";
    if (NIVEL == 2) { tituloEs = "Instatransporte II"; tituloEn = "Instatransport II"; }
    if (NIVEL == 3) { tituloEs = "Instatransporte III"; tituloEn = "Instatransport III"; }
    if (NIVEL == 4) { tituloEs = "Instatransporte IV a"; tituloEn = "Instatransport IV a"; }
    if (NIVEL == 5) { tituloEs = "Instatransporte IV b"; tituloEn = "Instatransport IV b"; }
    if (NIVEL == 2) { tituloPt = "Instatransporte II"; }
    if (NIVEL == 3) { tituloPt = "Instatransporte III"; }
    if (NIVEL == 4) { tituloPt = "Instatransporte IV a"; }
    if (NIVEL == 5) { tituloPt = "Instatransporte IV b"; }

    int alcance = NIVEL > 2 ? 4 : 3;
    int bonusEvasion = NIVEL == 5 ? 2 : 1;
    int duracionResiduo = NIVEL == 4 ? 3 : 2;
    int bonusDanioArcano = NIVEL > 1 ? 4 : 3;
    int apRestaurado = NIVEL > 2 ? 2 : 1;
    if (esIngles)
    {
      string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, "Energy Residues", "Estado_acumularenergia");
      string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage", "dano_arcano");
      string danioArcanoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage");
      string evasion = TerminoDescripcion(TerminoDescripcionId.Evasion, "Evasion", "Estado_evasion");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string patronResiduos = NIVEL == 4
        ? "on all adjacent tiles"
        : "on adjacent tiles in a + shape";
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "-1 cooldown."; }
        else if (NIVEL == 2) { proximaMejora = "+1 targeting range."; }
        else if (NIVEL == 3) { proximaMejora = "Option A: creates residues on all adjacent tiles.\nOption B: +1 Evasion."; }
      }

      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        tituloEn,
        "Teleports to an empty tile and leaves volatile Energy Residues at the destination.",
        new[]
        {
          LineaDescripcion("Target", "1 empty tile"),
          LineaDescripcion("Effect", "Teleports to the target tile and destroys traps on it."),
          LineaDescripcion("Residues", $"Creates {residuo} {patronResiduos}; they last {duracionResiduo} turns."),
          LineaDescripcion("On contact", $"Gains +1 Attack and +{bonusDanioArcano} {danioArcano} ({duracionResiduo} turns); restores {apRestaurado} {ap}.", 1),
          LineaDescripcion("Channeler", "Also restores 1-8 HP.", 2),
          LineaDescripcion("Other units", $"Also suffer 1-8 {danioArcanoSinIcono}.", 2),
          LineaDescripcion("Self", $"Gains +{bonusEvasion} {evasion}.")
        },
        proximaMejora);
      return;
    }
    {
      bool pt = esPortugues;
      string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, pt ? "Resíduos Energéticos" : "Residuos Energéticos", "Estado_acumularenergia");
      string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano", "dano_arcano");
      string danioArcanoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano");
      string evasion = TerminoDescripcion(TerminoDescripcionId.Evasion, pt ? "Evasão" : "Evasión", "Estado_evasion");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string patronResiduosLocalizado = NIVEL == 4 ? (pt ? "em todas as casas adjacentes" : "en todas las casillas adyacentes") : (pt ? "nas casas adjacentes em forma de +" : "en las casillas adyacentes en forma de +");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "-1 de recarga."; }
        else if (NIVEL == 2) { proximaMejora = pt ? "+1 de alcance de seleção." : "+1 de alcance de selección."; }
        else if (NIVEL == 3) { proximaMejora = pt ? "Opção A: cria resíduos em todas as casas adjacentes.\nOpção B: +1 Evasão." : "Opción A: crea residuos en todas las casillas adyacentes.\nOpción B: +1 Evasión."; }
      }
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        pt ? tituloPt : tituloEs,
        pt ? "Teletransporta-se para uma casa vazia e deixa Resíduos Energéticos voláteis no destino." : "Se teletransporta a una casilla vacía y deja Residuos Energéticos volátiles en el destino.",
        new[]
        {
          LineaDescripcion(pt ? "Alvo" : "Objetivo", pt ? "1 casa vazia" : "1 casilla vacía"),
          LineaDescripcion(pt ? "Efeito" : "Efecto", pt ? "Teletransporta-se para a casa escolhida e destrói as armadilhas nela." : "Se teletransporta a la casilla elegida y destruye las trampas que contiene."),
          LineaDescripcion(pt ? "Resíduos" : "Residuos", $"{(pt ? "Cria" : "Crea")} {residuo} {patronResiduosLocalizado}; {(pt ? "duram" : "duran")} {duracionResiduo} turnos."),
          LineaDescripcion(pt ? "Ao entrar em contato" : "Al entrar en contacto", $"{(pt ? "Recebe" : "Obtiene")} +1 Ataque {(pt ? "e" : "y")} +{bonusDanioArcano} {danioArcano} ({duracionResiduo} turnos); recupera {apRestaurado} {ap}.", 1),
          LineaDescripcion(pt ? "Canalizador" : "Canalizador", pt ? "Também recupera 1-8 PV." : "También recupera 1-8 PV.", 2),
          LineaDescripcion(pt ? "Outras unidades" : "Otras unidades", $"{(pt ? "Também sofrem" : "También sufren")} 1-8 {danioArcanoSinIcono}.", 2),
          LineaDescripcion(pt ? "Próprio" : "Propio", $"{(pt ? "Recebe" : "Obtiene")} +{bonusEvasion} {evasion}.")
        },
        proximaMejora);
      return;
    }
    string residuosEs = NIVEL == 4
      ? "Genera Residuos Energeticos en todo alrededor del destino."
      : "Genera Residuos Energeticos en cruz adyacente al destino.";
    string residuosEn = NIVEL == 4
      ? "Generates Energy Residues all around the destination."
      : "Generates Energy Residues in an adjacent cross at destination.";
    string residuosPt = NIVEL == 4
      ? "Gera Residuos Energeticos em volta de todo o destino."
      : "Gera Residuos Energeticos em cruz adjacente ao destino.";
    string colorEncabezado = "#44d3ec";
    string colorValor = "#ffffff";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string iconoEvasion = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_evasion\"></voffset></size><space=-0.35em>";
    string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0
      ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
      : $"{costoAP} {iconoAP}";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<b>Type:</b> Ranged ({alcance} range)\n";
      cuerpo += "<b>Target:</b> 1 empty tile in range\n";
      cuerpo += "<b>Effect:</b> Instant teleport to target tile\n";
      cuerpo += "<b>On arrival:</b> Destroys traps on destination tile\n";
      cuerpo += $"<b>Extra:</b> {residuosEn}\n";
      cuerpo += $"<b>Self buff:</b> +{bonusEvasion} Evasion";
    }
    else if (esPortugues)
    {
      cuerpo += $"<b>Tipo:</b> Distancia ({alcance} alcance)\n";
      cuerpo += "<b>Alvo:</b> 1 casa vazia em alcance\n";
      cuerpo += "<b>Efeito:</b> Teletransporte instantaneo para a casa alvo\n";
      cuerpo += "<b>Ao chegar:</b> Destroi armadilhas na casa de destino\n";
      cuerpo += $"<b>Extra:</b> {residuosPt}\n";
      cuerpo += $"<b>Buff proprio:</b> +{bonusEvasion} Evasao";
    }
    else
    {
      cuerpo += $"<b>Tipo:</b> Rango ({alcance} alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 casilla vacia en rango\n";
      cuerpo += "<b>Efecto:</b> Teletransporte instantaneo a la casilla objetivo\n";
      cuerpo += "<b>Al llegar:</b> Destruye trampas en la casilla destino\n";
      cuerpo += $"<b>Extra:</b> {residuosEs}\n";
      cuerpo += $"<b>Buff propio:</b> +{bonusEvasion} Evasion";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "The Channeler blinks through arcane space and leaves unstable residue behind."
        : esPortugues
          ? "O Canalizador se desloca pelo espaco arcano e deixa residuos instaveis para tras."
        : "El Canalizador se desplaza por el espacio arcano y deja residuo inestable atras.",
      cuerpo,
      costos,
      "#5dade2");

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? $"Teleport to an empty tile and leave {iconoEnergia} Energy Residues."
      : esPortugues
        ? $"Teletransporta para uma casa vazia e deixa {iconoEnergia} Resíduos Energéticos."
        : $"Teletransporta a una casilla vacía y deja {iconoEnergia} Residuos Energéticos.";
    string residuosFormato = esIngles
      ? (NIVEL == 4 ? "All around the destination" : "Adjacent cross at destination")
      : esPortugues
        ? (NIVEL == 4 ? "Ao redor de todo o destino" : "Em cruz adjacente ao destino")
        : (NIVEL == 4 ? "Alrededor de todo el destino" : "En cruz adyacente al destino");

    string cuerpoFormato = "";
    if (esIngles)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Mobility ({alcance} range)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 empty tile in range</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Effect:</b></color> <color={colorValor}>Teleport to target tile</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>On arrival:</b></color> <color={colorValor}>Destroys traps on destination tile</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Residues:</b></color> <color={colorValor}>{iconoEnergia} {residuosFormato}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Self buff:</b></color> <color={colorValor}>{iconoEvasion} +{bonusEvasion} Evasion</color>";
    }
    else if (esPortugues)
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Mobilidade ({alcance} alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 casa vazia em alcance</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Efeito:</b></color> <color={colorValor}>Teletransporte para a casa alvo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Ao chegar:</b></color> <color={colorValor}>Destrói armadilhas na casa de destino</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Resíduos:</b></color> <color={colorValor}>{iconoEnergia} {residuosFormato}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Buff próprio:</b></color> <color={colorValor}>{iconoEvasion} +{bonusEvasion} Evasão</color>";
    }
    else
    {
      cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Movilidad ({alcance} alcance)</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 casilla vacía en rango</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Efecto:</b></color> <color={colorValor}>Teletransporte a la casilla objetivo</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Al llegar:</b></color> <color={colorValor}>Destruye trampas en la casilla destino</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Residuos:</b></color> <color={colorValor}>{iconoEnergia} {residuosFormato}</color>\n";
      cuerpoFormato += $"<color={colorEncabezado}><b>Buff propio:</b></color> <color={colorValor}>{iconoEvasion} +{bonusEvasion} Evasión</color>";
    }

    txtDescripcion =
      $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
      $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
      "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
      cuerpoFormato;

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 range.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (residues all around) or Option B (+1 Evasion).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 alcance.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (residuos em toda volta) ou Opcao B (+1 Evasao).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 alcance.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (residuos en todo alrededor) u Opción B (+1 Evasion).</color>"; }
    }

  }



  Casilla Origen;
  public override void Activar()
  {
    Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
    ObtenerObjetivos();


    BattleManager.Instance.SeleccionandoObjetivo = true;
    BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());



  }



  public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
  {
    scEstaUnidad.estado_evasion = 1;
    if (NIVEL == 5) { scEstaUnidad.estado_evasion += 1; }

    float alphaOriginal = scEstaUnidad.ObtenerMultiplicadorAlphaVisual();
    ReproducirVfxSonidoOculto(scEstaUnidad.gameObject);
    InstatransporteBlinkVFX.Crear(scEstaUnidad.transform.position, true, scEstaUnidad.transform);
    scEstaUnidad.EstablecerMultiplicadorAlphaVisual(Mathf.Min(alphaOriginal, 0.18f));
    

    Trampa[] trampas = cas.transform.GetComponentsInChildren<Trampa>();
    foreach (Trampa trmp in trampas)
    {
      trmp.DestruirTrampa();

    }

    scEstaUnidad.TeletransportarACasilla(cas);
    InstatransporteBlinkVFX.Crear(scEstaUnidad.transform.position, false, scEstaUnidad.transform);
    
    int alre = 1;
    if (NIVEL == 4) { alre = 2; }
    foreach (Casilla ady in cas.ObtenerCasillasAlrededor(alre))
    {
      if (ady.Presente != null && ady.Presente.GetComponent<Obstaculo>() != null)
      {
        continue;
      }

      ady.AddComponent<ResiduoEnergetico>();
      ady.GetComponent<ResiduoEnergetico>().InicializarCreador(scEstaUnidad, NIVEL);

    }



    await Task.Delay(140);
    if (scEstaUnidad != null)
    {
      scEstaUnidad.EstablecerMultiplicadorAlphaVisual(alphaOriginal);
    }


  }
      void ReproducirVfxSonidoOculto(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Instatransporte");
      if (VFXenObjetivo == null || objetivo == null) { return; }

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
    VFXSoloSonido.OcultarVisuales(vfx);
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

  //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
  private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();


  private void ObtenerObjetivos()
  {

    lObjetivosPosibles.Clear();
    lCasillasafectadas.Clear();

    List<Casilla> alCasillasafectadas = new List<Casilla>();
    //Casillas Alrededor al origen
    int alre = 3;
    if (NIVEL > 2) { alre++; }
    alCasillasafectadas = Origen.ObtenerCasillasAlrededor(alre);
    alCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear

    foreach (Casilla c in alCasillasafectadas)
    {
      c.ActivarCapaColorAzul();
      if (c.Presente != null)
      {
        continue;
      }

      lCasillasafectadas.Add(c);


    }


  }

}
    

 
public class InstatransporteBlinkVFX : MonoBehaviour
{
  private const float Duracion = 0.72f;
  private const float OffsetVertical = 0.34f;
  private const int SegmentosAnillo = 56;

  private static Material materialParticulas;
  private static Material materialAnillo;
  private static Texture2D texturaCirculoSuave;

  private LineRenderer anilloExterior;
  private LineRenderer anilloInterior;
  private LineRenderer haloGlow;
  private bool salida;
  private float tiempo;
  private Color colorBase;
  private Color colorSecundario;

  public static void Crear(Vector3 posicion, bool salida, Transform referenciaOrden)
  {
    GameObject root = new GameObject(salida ? "VFX_Instatransporte_Origen" : "VFX_Instatransporte_Destino");
    root.transform.position = posicion + new Vector3(0f, OffsetVertical, 0f);

    InstatransporteBlinkVFX fx = root.AddComponent<InstatransporteBlinkVFX>();
    fx.salida = salida;
    fx.colorBase = salida ? new Color(0.42f, 0.95f, 1f, 1f) : new Color(0.66f, 0.76f, 1f, 1f);
    fx.colorSecundario = salida ? new Color(0.74f, 0.46f, 1f, 1f) : new Color(0.36f, 1f, 0.88f, 1f);
    fx.Inicializar(referenciaOrden);
  }

  private void Inicializar(Transform referenciaOrden)
  {
    int sortingLayerId;
    int sortingOrder;
    ObtenerOrden(referenciaOrden, out sortingLayerId, out sortingOrder);

    haloGlow = CrearAnillo("HaloGlow", 0.378f, 0.055f, sortingLayerId, sortingOrder + 1);
    anilloExterior = CrearAnillo("AnilloExterior", 0.306f, 0.022f, sortingLayerId, sortingOrder + 2);
    anilloInterior = CrearAnillo("AnilloInterior", 0.198f, 0.015f, sortingLayerId, sortingOrder + 3);

    CrearParticulas("ChispasDeBorde", 24, 0.18f, 0.48f, 0.033f, 0.62f, sortingLayerId, sortingOrder + 4);
    CrearParticulas("PolvoArcano", 14, 0.07f, 0.62f, 0.066f, 0.22f, sortingLayerId, sortingOrder + 1);
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float t = Mathf.Clamp01(tiempo / Duracion);
    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.14f));
    float salidaAlpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.48f) / 0.52f));
    float intensidad = entrada * salidaAlpha;
    float pulso = 0.5f + (0.5f * Mathf.Sin(tiempo * 18f));
    float escala = salida
      ? Mathf.Lerp(1.04f, 0.38f, Mathf.SmoothStep(0f, 1f, t))
      : Mathf.Lerp(0.42f, 1.08f, Mathf.SmoothStep(0f, 1f, t));

    ActualizarAnillo(haloGlow, 0.378f * Mathf.Lerp(escala, 1.12f, 0.28f), 0.055f, Color.Lerp(colorBase, colorSecundario, 0.35f), 0.28f * intensidad);
    ActualizarAnillo(anilloExterior, 0.306f * escala, 0.022f, Color.Lerp(colorBase, colorSecundario, pulso), 0.92f * intensidad);
    ActualizarAnillo(anilloInterior, 0.198f * Mathf.Lerp(escala, 1f, 0.22f), 0.015f, colorSecundario, 0.72f * intensidad);
    transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(salida ? 14f : -18f, salida ? -12f : 10f, t));

    if (tiempo >= Duracion)
    {
      Destroy(gameObject);
    }
  }

  private LineRenderer CrearAnillo(string nombre, float radio, float ancho, int sortingLayerId, int sortingOrder)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(transform, false);

    LineRenderer lr = go.AddComponent<LineRenderer>();
    lr.useWorldSpace = false;
    lr.loop = true;
    lr.positionCount = SegmentosAnillo;
    lr.sharedMaterial = ObtenerMaterialAnillo();
    lr.sortingLayerID = sortingLayerId;
    lr.sortingOrder = sortingOrder;
    lr.numCornerVertices = 3;
    lr.numCapVertices = 3;
    ActualizarAnillo(lr, radio, ancho, colorBase, 0f);
    return lr;
  }

  private void ActualizarAnillo(LineRenderer lr, float radio, float ancho, Color color, float alpha)
  {
    if (lr == null)
    {
      return;
    }

    radio = Mathf.Max(0.01f, radio);
    for (int i = 0; i < SegmentosAnillo; i++)
    {
      float angulo = (i / (float)SegmentosAnillo) * Mathf.PI * 2f;
      lr.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio * 0.46f, 0f));
    }

    Color colorFinal = new Color(color.r, color.g, color.b, alpha);
    lr.startWidth = ancho;
    lr.endWidth = ancho;
    lr.startColor = colorFinal;
    lr.endColor = colorFinal;
  }

  private void CrearParticulas(string nombre, int cantidad, float radio, float vida, float tamano, float velocidad, int sortingLayerId, int sortingOrder)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(transform, false);

    ParticleSystem ps = go.AddComponent<ParticleSystem>();
    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.sharedMaterial = ObtenerMaterialParticulas();
    renderer.sortingLayerID = sortingLayerId;
    renderer.sortingOrder = sortingOrder;
    renderer.renderMode = ParticleSystemRenderMode.Billboard;

    var main = ps.main;
    main.playOnAwake = false;
    main.loop = false;
    main.duration = 0.18f;
    main.startLifetime = new ParticleSystem.MinMaxCurve(vida * 0.72f, vida);
    main.startSpeed = new ParticleSystem.MinMaxCurve(velocidad * 0.45f, velocidad);
    main.startSize = new ParticleSystem.MinMaxCurve(tamano * 0.62f, tamano);
    main.startColor = new ParticleSystem.MinMaxGradient(colorBase, colorSecundario);
    main.gravityModifier = 0f;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = Mathf.Max(8, cantidad + 4);

    var emission = ps.emission;
    emission.enabled = false;

    var shape = ps.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = radio;

    var colorOverLifetime = ps.colorOverLifetime;
    colorOverLifetime.enabled = true;
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new GradientColorKey[] {
        new GradientColorKey(colorBase, 0f),
        new GradientColorKey(colorSecundario, 0.55f),
        new GradientColorKey(colorBase, 1f)
      },
      new GradientAlphaKey[] {
        new GradientAlphaKey(0f, 0f),
        new GradientAlphaKey(0.62f, 0.14f),
        new GradientAlphaKey(0.34f, 0.62f),
        new GradientAlphaKey(0f, 1f)
      });
    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

    var sizeOverLifetime = ps.sizeOverLifetime;
    sizeOverLifetime.enabled = true;
    AnimationCurve curvaTamano = new AnimationCurve();
    curvaTamano.AddKey(0f, salida ? 0.74f : 0.42f);
    curvaTamano.AddKey(0.42f, 1f);
    curvaTamano.AddKey(1f, salida ? 0.28f : 0.62f);
    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTamano);

    ps.Play();
    ps.Emit(cantidad);
  }

  private static void ObtenerOrden(Transform referencia, out int sortingLayerId, out int sortingOrder)
  {
    sortingLayerId = 0;
    sortingOrder = 70;
    if (referencia == null)
    {
      return;
    }

    Canvas canvas = referencia.GetComponentInChildren<Canvas>(true);
    if (canvas != null)
    {
      sortingLayerId = canvas.sortingLayerID;
      sortingOrder = canvas.sortingOrder + 7;
      return;
    }

    SpriteRenderer spriteRenderer = referencia.GetComponentInChildren<SpriteRenderer>(true);
    if (spriteRenderer != null)
    {
      sortingLayerId = spriteRenderer.sortingLayerID;
      sortingOrder = spriteRenderer.sortingOrder + 7;
      return;
    }

    Renderer renderer = referencia.GetComponentInChildren<Renderer>(true);
    if (renderer != null)
    {
      sortingLayerId = renderer.sortingLayerID;
      sortingOrder = renderer.sortingOrder + 7;
    }
  }

  private static Material ObtenerMaterialParticulas()
  {
    if (materialParticulas != null)
    {
      return materialParticulas;
    }

    Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
    if (shader == null)
    {
      shader = Shader.Find("Particles/Standard Unlit");
    }
    if (shader == null)
    {
      shader = Shader.Find("Sprites/Default");
    }
    if (shader == null)
    {
      return null;
    }

    materialParticulas = new Material(shader);
    materialParticulas.name = "Instatransporte_Particulas";
    materialParticulas.hideFlags = HideFlags.HideAndDontSave;
    if (materialParticulas.HasProperty("_MainTex"))
    {
      materialParticulas.mainTexture = ObtenerTexturaCirculoSuave();
    }
    if (materialParticulas.HasProperty("_Color"))
    {
      materialParticulas.color = Color.white;
    }

    return materialParticulas;
  }

  private static Material ObtenerMaterialAnillo()
  {
    if (materialAnillo != null)
    {
      return materialAnillo;
    }

    Shader shader = Shader.Find("Sprites/Default");
    if (shader == null)
    {
      shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
    }
    if (shader == null)
    {
      return ObtenerMaterialParticulas();
    }

    materialAnillo = new Material(shader);
    materialAnillo.name = "Instatransporte_Anillo";
    materialAnillo.hideFlags = HideFlags.HideAndDontSave;
    if (materialAnillo.HasProperty("_MainTex"))
    {
      materialAnillo.mainTexture = ObtenerTexturaCirculoSuave();
    }
    if (materialAnillo.HasProperty("_Color"))
    {
      materialAnillo.color = Color.white;
    }

    return materialAnillo;
  }

  private static Texture2D ObtenerTexturaCirculoSuave()
  {
    if (texturaCirculoSuave != null)
    {
      return texturaCirculoSuave;
    }

    const int size = 32;
    texturaCirculoSuave = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
    texturaCirculoSuave.name = "Instatransporte_CirculoSuave";
    texturaCirculoSuave.wrapMode = TextureWrapMode.Clamp;
    texturaCirculoSuave.filterMode = FilterMode.Bilinear;
    texturaCirculoSuave.hideFlags = HideFlags.HideAndDontSave;

    float half = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float nx = (x - half) / half;
        float ny = (y - half) / half;
        float r = Mathf.Sqrt((nx * nx) + (ny * ny));
        float alpha = Mathf.Clamp01(1f - r);
        alpha = alpha * alpha * (3f - 2f * alpha);
        texturaCirculoSuave.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
      }
    }

    texturaCirculoSuave.Apply(false, false);
    return texturaCirculoSuave;
  }
}

 
 
   /*  private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();

     int alcance = 3;
     if (NIVEL > 2) { alcance++; }
      //Casillas Alrededor al origen
     List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(alcance);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
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
             if(c.Presente.GetComponent<Unidad>() != null)
           {
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


   
    

 
}*/




