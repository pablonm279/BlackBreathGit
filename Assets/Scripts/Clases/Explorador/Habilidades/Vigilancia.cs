using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

public class Vigilancia : Habilidad
{
   

   
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de crpitico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
  public override void Awake()
  {
    nombre = "Vigilancia";
    costoAP = 2;
    costoPM = 2;
    IDenClase = 6;

    if (NIVEL == 4)
    { costoPM--; }

    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 1;//esto es solamente para que marque de azul las casillas afectadas, no tiene otro efecto
    poneTrampas = true;
    esforzable = 0;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 3;
    bAfectaObstaculos = false;



    requiereRecurso = 2; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
    if (NIVEL == 5) { requiereRecurso++; }


    imHab = Resources.Load<Sprite>("imHab/Explorador_Vigilancia");
    ActualizarDescripcion();
  }


  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - statsUI.CriticoRango, 2, 20);

    int disparosPorUso = NIVEL == 5 ? 3 : 2;
    int bonoTiradaReaccion = (NIVEL > 1 ? 1 : 0) + (NIVEL > 2 ? 1 : 0);
    string rangoDanioReaccionEs = FormatearRangoDados(1, 10, 1);

    string tituloEs = "Vigilancia I";
    string tituloEn = "Vigilance I";
    string tituloPt = "Vigilancia I";
    if (NIVEL == 2) { tituloEs = "Vigilancia II"; tituloEn = "Vigilance II"; }
    if (NIVEL == 3) { tituloEs = "Vigilancia III"; tituloEn = "Vigilance III"; }
    if (NIVEL == 4) { tituloEs = "Vigilancia IV a"; tituloEn = "Vigilance IV a"; }
    if (NIVEL == 5) { tituloEs = "Vigilancia IV b"; tituloEn = "Vigilance IV b"; }
    if (NIVEL == 2) { tituloPt = "Vigilancia II"; }
    if (NIVEL == 3) { tituloPt = "Vigilancia III"; }
    if (NIVEL == 4) { tituloPt = "Vigilancia IV a"; }
    if (NIVEL == 5) { tituloPt = "Vigilancia IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Reactive Ranged (6 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy in range to place a 3x3 watch zone centered on that tile\n";
      cuerpo += "<b>Roll/Save:</b> no save check on cast\n";
      cuerpo += $"<b>Watch setup:</b> places traps on empty tiles of that 3x3 area (1 turn, 1 use each), up to {disparosPorUso} reaction shots total\n";
      cuerpo += $"<b>Reaction shot:</b> uses Bow Shot roll with +{bonoTiradaReaccion} to the d20 result. Base roll shown in UI: 1d20 + <color=#ea0606>Agility ({agilidadActual})</color>  vs Defense. Base crit: {criticoBaseMin}-20\n";
      cuerpo += "<b>Reaction damage:</b> same as Bow Shot (1d10 + 1 + Agility) | <b>Type:</b> Slashing\n";
      cuerpo += $"<b>Resource:</b> needs at least {requiereRecurso} Arrows to activate, consumes 1 Arrow per reaction shot\n";
      cuerpo += "<b>Turn flow:</b> using this skill ends your turn";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Distancia Reativa (6 alcance)\n";
      cuerpo += "<b>Alvo:</b> 1 inimigo em alcance para criar uma zona de vigilancia 3x3 centrada nessa casa\n";
      cuerpo += "<b>Rolagem/TS:</b> sem teste de resistencia ao usar\n";
      cuerpo += $"<b>Preparacao:</b> coloca armadilhas em casas vazias dessa area 3x3 (1 turno, 1 uso cada), ate {disparosPorUso} disparos reativos no total\n";
      cuerpo += $"<b>Disparo reativo:</b> usa a rolagem de Tiro com Arco com +{bonoTiradaReaccion} no resultado do d20. Rolagem base exibida: 1d20 + <color=#ea0606>Agilidade ({agilidadActual})</color> + Ataque ({ataqueActual}) vs Defesa. Critico base: {criticoBaseMin}-20\n";
      cuerpo += "<b>Dano reativo:</b> igual ao Tiro com Arco (1d10 + 1 + Agilidade) | <b>Tipo:</b> Cortante\n";
      cuerpo += $"<b>Recurso:</b> requer ao menos {requiereRecurso} Flechas para ativar, consome 1 Flecha por disparo reativo\n";
      cuerpo += "<b>Fluxo de turno:</b> usar esta habilidade termina seu turno";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango Reactivo (6 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo en rango para colocar una zona de vigilancia 3x3 centrada en esa casilla\n";
      cuerpo += "<b>Tirada/TS:</b> no tiene TS al lanzar\n";
      cuerpo += $"<b>Preparacion:</b> coloca trampas en casillas vacias de esa area 3x3 (1 turno, 1 uso cada una), hasta {disparosPorUso} disparos reactivos en total\n";
      cuerpo += $"<b>Disparo reactivo:</b> usa la tirada de Tiro con Arco con +{bonoTiradaReaccion} al resultado del d20. Tirada base mostrada: 1d20 + <color=#ea0606>Agi ({agilidadActual})</color> + Ataque ({ataqueActual}) vs Defensa. Critico base: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Danio reactivo:</b> igual a Tiro con Arco ({rangoDanioReaccionEs} + Agi) | <b>Tipo:</b> Cortante\n";
      cuerpo += $"<b>Recurso:</b> requiere al menos {requiereRecurso} Flechas para activar, consume 1 Flecha por disparo reactivo\n";
      cuerpo += "<b>Flujo de turno:</b> usar esta habilidad termina tu turno";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "Controls space with reaction fire over a short zone window."
        : esPortugues
          ? "Controla espaco com fogo de reacao durante uma janela curta de zona."
        : "Controla espacio con fuego de reaccion durante una ventana corta.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (mostrarProximoNivel)
    {
      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 reaction attack roll.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 reaction attack roll.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+1 reaction shot and +1 required Arrow).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na rolagem de ataque reativa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na rolagem de ataque reativa.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (+1 disparo reativo e +1 Flecha requerida).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 a la tirada de ataque reactiva.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 a la tirada de ataque reactiva.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de Valentía) u Opcion B (+1 disparo reactivo y +1 Flecha requerida).</color>"; }
      }
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.gameObject != null && CampaignManager.Instance.gameObject.transform.parent != null && CampaignManager.Instance.gameObject.transform.parent.parent != null)
    {
      AdministradorEscenas admin = CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>();
      if (admin != null && admin.escenaActual == 1)
      {
        ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
        if (clase != null && clase.ObtenerCantidadFlechas() < requiereRecurso)
        {
          txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
        }
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
    
    
    public int disparosEsteTurno = 0;
    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {
     
     disparosEsteTurno = 2; //2 disparos por uso de habilidad
     if(NIVEL == 5){disparosEsteTurno++;}

     Casilla casillaDestino = casillaObjetivo;
     if (casillaDestino == null)
     {
      if (obj is Unidad unidadObjetivo && unidadObjetivo.CasillaPosicion != null)
      {
        casillaDestino = unidadObjetivo.CasillaPosicion;
      }
      else if (obj is Obstaculo obstaculoObjetivo && obstaculoObjetivo.CasillaPosicion != null)
      {
        casillaDestino = obstaculoObjetivo.CasillaPosicion;
      }
      else if (obj is Casilla casillaClickeada)
      {
        casillaDestino = casillaClickeada;
      }
     }

     if (casillaDestino == null)
     {
      Debug.LogWarning("Vigilancia.AplicarEfectosHabilidad no pudo determinar casilla objetivo.");
      return;
     }
     List<Casilla> lCasillas = casillaDestino.ObtenerCasillasAlrededor(1);
     lCasillas.Add(casillaDestino);

     foreach(Casilla cas in lCasillas)
     {
      if (cas.Presente != null)
      {
        continue; //Si la casilla tiene unidad, no se pone trampa
      }
      
        cas.AddComponent<VigilanciaTrampa>();
        cas.GetComponent<VigilanciaTrampa>().InicializarCreador(scEstaUnidad);
      

     }

      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();

    }


    public List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
   
    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lObstaculosPosibles.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,3);
    
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

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

    

}




