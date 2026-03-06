using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;


public class DisparoEnvenenado : Habilidad
{


  [SerializeField] private GameObject VFXenObjetivo;
  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
  [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

  private int hAlcance = 3;
  private int hAncho = 1; //1 - adyancentes también
  ClaseAcechador claseAcechador;
    public override void  Awake()
    {
    nombre = "Disparo Envenenado";
    costoAP = 3;
    costoPM = 0;
    IDenClase = 3;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    claseAcechador = scEstaUnidad as ClaseAcechador;
    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 5;
    if (NIVEL == 5) {cooldownMax -= 1; } //Nivel 5 reduce cooldown en 1
    bAfectaObstaculos = true;


    XdDanio = 3;
    daniodX = 4; //1d10
    tipoDanio = 2; //Perforante
    criticoRangoHab = 0;

     tipoPorcentaje = 2;
    imHab = Resources.Load<Sprite>("imHab/Acechador_DisparoEnvenenado");
    ActualizarDescripcion();

 

  }

  void Start()
  {
    if (claseAcechador != null)
    { Invoke("ChequearMaestria", 0.2f); }
  }

  int damExtra;
  void ChequearMaestria()
  {
    int NivelMaestria = claseAcechador.PASIVA_MaestriaConBallestaMano;



    if (NivelMaestria == 1)
    {
      bonusAtaque = 1;
      damExtra += 2;

      if (TRADU.i.nIdioma == 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño.</i>\n\n"; }
      if (TRADU.i.nIdioma == 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage.</i>\n\n"; }

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      if (TRADU.i.nIdioma == 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n"; }
      if (TRADU.i.nIdioma == 2)
      {  txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage +1 Critical Range.</i>\n\n"; }

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico, -1 AP.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Attack +2 Damage +1 Critical Range, -1 AP.</i>\n\n"; }


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      hAlcance += 1; //Alcance +1
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Alcance +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: +1 Range +1 Attack +2 Damage +1 Critical Range.</i>\n\n"; }

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      cooldownMax -= 1; //Cooldown -1
      costoAP -= 1; //costo AP -1
      cooldownActual = 0;
      if(TRADU.i.nIdioma== 1)
      { txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: Remueve Cooldown, +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n"; }
      if(TRADU.i.nIdioma== 2)
      { txtDescripcion += "\n\n<i>Hand Crossbow Mastery adds: Removes Cooldown, +1 Attack +2 Damage +1 Critical Range.</i>\n\n"; }

    }

      ActualizarDescripcion();

  }






  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int dcBase = NIVEL > 2 ? 13 : 12;
    int venenoAplicado = 2 + (NIVEL > 1 ? 1 : 0) + (NIVEL == 4 ? 2 : 0);
    int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConBallestaMano : 0;

    string tituloEs = "Disparo Envenenado I";
    string tituloEn = "Poison Shot I";
    if (NIVEL == 2) { tituloEs = "Disparo Envenenado II"; tituloEn = "Poison Shot II"; }
    if (NIVEL == 3) { tituloEs = "Disparo Envenenado III"; tituloEn = "Poison Shot III"; }
    if (NIVEL == 4) { tituloEs = "Disparo Envenenado IV a"; tituloEn = "Poison Shot IV a"; }
    if (NIVEL == 5) { tituloEs = "Disparo Envenenado IV b"; tituloEn = "Poison Shot IV b"; }

    string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Fortaleza, dcBase);

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<b>Type:</b> Ranged ({hAlcance} range)\n";
      cuerpo += "<b>Target:</b> 1 enemy in range\n"; 
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Agility ({agilidadActual})</color> + {bonusAtaque} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Damage:</b> 3d4 + {damExtra} + <color=#ea0606>Agility ({agilidadActual})</color> | <b>Type:</b> Piercing\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>On failed save:</b> applies {venenoAplicado} Poison";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<b>Passive applied:</b> Hand Crossbow Mastery (Tier {nivelMaestria})";
      }
    }
    else
    {
      cuerpo += $"<b>Tipo:</b> Rango ({hAlcance} alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo en rango\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Agilidad ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Danio:</b> 3d4 + {damExtra} + <color=#ea0606>Agilidad ({agilidadActual})</color> | <b>Tipo:</b> Perforante\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>Si falla TS:</b> aplica {venenoAplicado} Veneno";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<b>Pasiva aplicada:</b> Maestria con Ballesta de Mano (Tier {nivelMaestria})";
      }
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A control shot that layers poison through a Fortitude save check."
        : "Un disparo de control que acumula veneno mediante chequeo de TS Fortaleza.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Poison on failed save.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Poison) or Option B (-1 cooldown).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Veneno si falla TS.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC base de TS.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 Veneno) u Opcion B (-1 enfriamiento).</color>"; }
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



    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        if (objetivos == null || objetivos.Count == 0)
        {
            return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
        }

        List<Task> impactos = new List<Task>();
        foreach (var objetivo in objetivos)
        {
            var impacto = CrearProyectil(objetivo);
            if (impacto != null)
            {
                impactos.Add(impacto);
            }
        }

        if (impactos.Count == 0)
        {
            return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
        }

        return Task.WhenAll(impactos);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();

      int danioMarca = 0;float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;



      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0);


      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
      }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioMarca);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        EfectoAdicional(objetivo);
      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---


      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + damExtra + scEstaUnidad.mod_CarAgilidad;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
    private Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private async Task LanzarProyectilAsync(object objetivo)
    {
        await Task.Delay(100);

        GameObject proyPrefab = BattleManager.Instance.contenedorPrefabs.ViroteBallestadeManoVeneno;
        if (proyPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(proyPrefab);
        ArrowFlight flight = proyectil.GetComponent<ArrowFlight>();

        Transform destino = null;
        if (objetivo is Unidad unidadObjetivo)
        {
            destino = unidadObjetivo.transform;
        }
        else if (objetivo is Obstaculo obstaculoObjetivo)
        {
            destino = obstaculoObjetivo.transform;
        }

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.12f, 5.8f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await Task.Delay(200);
        }
    }

    void VFXAplicar(GameObject objetivo)
  {
    //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

  }

  //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
  private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

  private void ObtenerObjetivos()
  {
    //Cualquier objetivo en 1 de alcance 3 de ancho
    lObjetivosPosibles.Clear();



    List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance, hAncho);

    foreach (Casilla c in lCasillasafectadas)
    {


      c.ActivarCapaColorRojo();

      if (c.Presente == null)
      {
        continue;
      }

      if (!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
      {
        if (c.Presente.GetComponent<Unidad>() == null)
        {
          continue;
        }
        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>()); ;
        }


      }
      else
      {
        if (c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
        {
          continue;
        }

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>()); ;
        }

        if (c.Presente.GetComponent<Obstaculo>() != null)
        {
          lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>()); ;
        }

      }

    }


    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);




  }

 
  void EfectoAdicional(Unidad objetivo)
  {

    int DC = 12;

    if (NIVEL > 2) { DC++; }


    if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, DC))
    {

      objetivo.estado_veneno += 2;
      if (NIVEL > 1) { objetivo.estado_veneno += 1; }
      if (NIVEL == 4) { objetivo.estado_veneno += 2;}



    }

  }
  


}






