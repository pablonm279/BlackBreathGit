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
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
    int disparosPorUso = NIVEL == 5 ? 3 : 2;
    int bonoTiradaReaccion = (NIVEL > 1 ? 1 : 0) + (NIVEL > 2 ? 1 : 0);
    string rangoDanioReaccion = FormatearRangoDados(1, 10, 1);

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

    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string colorAgilidad = "#7fa35a";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
    string atributo = esIngles
      ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
      : esPortugues
        ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
        : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
    string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonoTiradaReaccion);

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Reactive ranged setup ({6} range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy; creates a 3x3 watch zone centered on its tile\n";
      cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> {costoPM} Valour; requires {requiereRecurso} Arrows\n";
      cuerpo += $"<color={colorEncabezado}><b>Setup:</b></color> empty tiles in the zone become 1-turn traps, 1 use each\n";
      cuerpo += $"<color={colorEncabezado}><b>Reaction limit:</b></color> up to {disparosPorUso} shots total; consumes 1 Arrow per shot\n";
      cuerpo += $"<color={colorEncabezado}><b>Reaction roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Reaction damage:</b></color> {rangoDanioReaccion} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Slashing\n";
      cuerpo += $"<color={colorEncabezado}><b>Turn flow:</b></color> ends turn";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Preparacao reativa a distancia ({6} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo; cria uma zona 3x3 centrada na casa dele\n";
      cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> {costoPM} Valentia; requer {requiereRecurso} Flechas\n";
      cuerpo += $"<color={colorEncabezado}><b>Preparacao:</b></color> casas vazias da zona viram armadilhas de 1 turno, 1 uso cada\n";
      cuerpo += $"<color={colorEncabezado}><b>Limite reativo:</b></color> ate {disparosPorUso} disparos no total; consome 1 Flecha por disparo\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem reativa:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano reativo:</b></color> {rangoDanioReaccion} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> termina turno";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Preparacion reactiva a distancia ({6} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo; crea una zona 3x3 centrada en su casilla\n";
      cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> {costoPM} Valentia; requiere {requiereRecurso} Flechas\n";
      cuerpo += $"<color={colorEncabezado}><b>Preparacion:</b></color> casillas vacias de la zona se vuelven trampas de 1 turno, 1 uso cada una\n";
      cuerpo += $"<color={colorEncabezado}><b>Limite reactivo:</b></color> hasta {disparosPorUso} disparos en total; consume 1 Flecha por disparo\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada reactiva:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Danio reactivo:</b></color> {rangoDanioReaccion} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno";
    }

    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    string subtitulo = esIngles
      ? "Set a short watch zone that fires reaction shots."
      : esPortugues
        ? "Cria uma zona curta que dispara reacoes."
        : "Crea una zona breve que dispara reacciones.";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (mostrarProximoNivel)
    {
      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 reaction roll bonus.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 reaction roll bonus.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 Valour Cost) or Option B (+1 reaction shot and +1 required Arrow).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus da rolagem reativa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no bonus da rolagem reativa.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de Valentia) ou Opcao B (+1 disparo reativo e +1 Flecha requerida).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bonus de tirada reactiva.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al bonus de tirada reactiva.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de Valentia) u Opcion B (+1 disparo reactivo y +1 Flecha requerida).</color>"; }
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

  private string TextoModificadorDescripcion(int valor)
  {
    if (valor > 0) { return $" + {valor}"; }
    if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
    return "";
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




