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


    imHab = Resources.Load<Sprite>("imHab/Acechador_DisparoEnvenenado");

 

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
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño.</i>\n\n";
     

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico, -1 AP.</i>\n\n";


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      hAlcance += 1; //Alcance +1
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Alcance +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      cooldownMax -= 1; //Cooldown -1
      costoAP -= 1; //costo AP -1
      cooldownActual = 0;
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: Remueve Cooldown, +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";

    }

  }






  public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Disparo Envenenado I</b></color>\n\n";
      txtDescripcion += "<i>Con la ballesta de mano dispara un virote envenenado al enemigo.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Agilidad</color><i> Daño Perforante: 3d4 + Agilidad. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 12: Aplica 2 Veneno.</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Ballesta de Mano -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Aplica +1 Veneno</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Disparo Envenenado II</b></color>\n\n";
      txtDescripcion += "<i>Con la ballesta de mano dispara un virote envenenado al enemigo.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Agilidad</color><i> Daño Perforante: 3d4 + Agilidad. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 12: Aplica 3 Veneno.</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Ballesta de Mano -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Disparo Envenenado III</b></color>\n\n";
      txtDescripcion += "<i>Con la ballesta de mano dispara un virote envenenado al enemigo.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Agilidad</color><i> Daño Perforante: 3d4 + Agilidad. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 13: Aplica 3 Veneno.</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Ballesta de Mano -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Aplica +2 Veneno</color>\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: -1 Cooldown</color>\n";
          }
        }
      }

    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Disparo Envenenado IV a</b></color>\n\n";
      txtDescripcion += "<i>Con la ballesta de mano dispara un virote envenenado al enemigo.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Agilidad</color><i> Daño Perforante: 3d4 + Agilidad. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 13: Aplica 5 Veneno.</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Ballesta de Mano -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Disparo Envenenado IV b</b></color>\n\n";
      txtDescripcion += "<i>Con la ballesta de mano dispara un virote envenenado al enemigo.</i>\n\n";
      txtDescripcion += $"-Ataque: <color=#ea0606>Agilidad</color><i> Daño Perforante: 3d4 + Agilidad. </i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 13: Aplica 3 Veneno.</color>\n";
      txtDescripcion += $"<color=#44d3ec>-Usa Ballesta de Mano -Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
    }

    if (SceneManager.GetActiveScene().name != "ES-Campaña")
    {


      int NivelMaestria = claseAcechador.PASIVA_MaestriaConBallestaMano;
      if (NivelMaestria == 1)
      {

        txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño.</i>\n\n";
      }
      else if (NivelMaestria == 2)
      {
        txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";
      }
      else if (NivelMaestria == 3)
      {

        txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +1 Rango Crítico, -1 AP.</i>\n\n";
      }
      else if (NivelMaestria == 4)
      {

        txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Alcance +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";
      }
      else if (NivelMaestria == 5)
      {
        txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: Remueve Cooldown, +1 Ataque +2 Daño +1 Rango Crítico.</i>\n\n";
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

