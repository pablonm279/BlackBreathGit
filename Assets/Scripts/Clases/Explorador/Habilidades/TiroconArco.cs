using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class TiroconArco : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Acido - 8: Arcano

    private int hAlcance = 7;
    private int hAncho = 2; //1 - adyancentes tambien
    public override void Awake()
  {
    nombre = "Tiro con Arco";
    costoAP = 3;
    costoPM = 0;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = false;
    esHostil = true;
    cooldownMax = 0;
    bAfectaObstaculos = true;

    bonusAtaque = 0;
    XdDanio = 1;
    daniodX = 8; //1d10
    tipoDanio = 2; //Cortante
    criticoRangoHab = 0;

    requiereRecurso = 1;
    tipoPorcentaje = 2;

    imHab = Resources.Load<Sprite>("imHab/Explorador_Tiroconarco");
    ActualizarDescripcion();
  }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
    string rangoDanio = FormatearRangoDados(1, 10, 1);
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string colorAgilidad = "#7fa35a";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}";
    string atributo = esIngles
      ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
      : esPortugues
        ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
        : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
    string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

    if (esIngles)
    {
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agility ({agilidadActual})");
      string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
      string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Arrow");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string rangoDanioReal = FormatearRangoDados(XdDanio, daniodX, 1);
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        "Bow Shot",
        "Fires a single arrow at one target.",
        new[]
        {
          LineaDescripcion("Target", "1 enemy or obstacle"),
          LineaDescripcion("Effect", $"On hit, deals {rangoDanioReal} + {agilidad} as {danioCortante}."),
          LineaDescripcion("Attack Roll", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion("Cost", $"1 {flecha}")
        });

      if (EsEscenaCampaña())
      {
        ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
        if (clase != null && clase.ObtenerCantidadFlechas() < 1)
        {
          txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
        }
      }
      return;
    }

    if (esPortugues)
    {
      string agilidade = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidade ({agilidadActual})");
      string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
      string danoCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "dano Cortante", "dano_cortante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string rangoDanioReal = FormatearRangoDados(XdDanio, daniodX, 1);
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        "Tiro com Arco",
        "Dispara uma única flecha contra um alvo.",
        new[]
        {
          LineaDescripcion("Alvo", "1 inimigo ou obstáculo"),
          LineaDescripcion("Efeito", $"Ao acertar, causa {rangoDanioReal} + {agilidade} como {danoCortante}."),
          LineaDescripcion("Rolagem de ataque", $"1d20 + {agilidade}{bonusTirada} vs {defesa}. Falha crítica: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion("Custo", $"1 {flecha}")
        });
      return;
    }

    {
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidad ({agilidadActual})");
      string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
      string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "daño Cortante", "dano_cortante");
      string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string rangoDanioReal = FormatearRangoDados(XdDanio, daniodX, 1);
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
        "Tiro con Arco",
        "Dispara una sola flecha contra un objetivo.",
        new[]
        {
          LineaDescripcion("Objetivo", "1 enemigo u obstáculo"),
          LineaDescripcion("Efecto", $"Al impactar, causa {rangoDanioReal} + {agilidad} como {danioCortante}."),
          LineaDescripcion("Tirada de ataque", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Pifia: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion("Costo", $"1 {flecha}")
        });
      return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged attack ({hAlcance} range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy or obstacle in range\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Slashing\n";
      cuerpo += $"<color={colorEncabezado}><b>Resource:</b></color> consumes 1 Arrow on shot\n";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo ou obstaculo em alcance\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>Recurso:</b></color> consome 1 Flecha por disparo\n";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo u obstáculo en rango\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Cortante\n";
      cuerpo += $"<color={colorEncabezado}><b>Recurso:</b></color> consume 1 Flecha por disparo\n";
    }

    string titulo = esIngles ? "Bow Shot" : esPortugues ? "Tiro com Arco" : "Tiro con Arco";
    string subtitulo = esIngles
      ? "Single-target bow attack; spends 1 Arrow."
      : esPortugues
        ? "Ataque de arco contra um alvo; gasta 1 Flecha."
        : "Ataque con arco a un objetivo; gasta 1 Flecha.";

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    if (CampaignManager.Instance != null && CampaignManager.Instance.gameObject != null)
    {
      if (CampaignManager.Instance.gameObject.transform.parent != null && CampaignManager.Instance.gameObject.transform.parent.parent != null)
      {
        AdministradorEscenas admin = CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>();
        if (admin != null && admin.escenaActual == 1)
        {
          ClaseExplorador clase = Usuario.GetComponent<ClaseExplorador>();
          if (clase != null && clase.ObtenerCantidadFlechas() < 1)
          {
            txtDescripcion += $"\n\n<color=#ea0606><b>{TRADU.i.Traducir("No tienes flechas para usar esta habilidad.")}</b></color>";
          }
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
        if(Usuario.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() > 0)
        {
          Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
          ObtenerObjetivos();

        
          BattleManager.Instance.SeleccionandoObjetivo = true;
          BattleManager.Instance.HabilidadActiva = this;
        }

        
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        if (objetivos == null || objetivos.Count == 0)
        {
            return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
        }

        List<Task> impactos = new List<Task>();
        var clase = Usuario.GetComponent<ClaseExplorador>();
        if (clase != null)
        {
            clase.CambiarCantidadFlechas(-1);
        }

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

    public void AplicarEfectosHabilidadConTipoDanio(object obj, int tirada, int tipoDanioTemporal, Casilla casillaOrigenTrampas = null)
    {
      int tipoDanioOriginal = tipoDanio;
      tipoDanio = tipoDanioTemporal;
      try
      {
        AplicarEfectosHabilidad(obj, tirada, casillaOrigenTrampas);
      }
      finally
      {
        tipoDanio = tipoDanioOriginal;
      }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    {
    
     if(obj is Unidad) //Aca van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;

         
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
        int bonusAtaqueTotal = bonusAtaque;
       
       //Chequear si tiene Marcar Presa
       MarcaMarcarPresa.AplicarBonosContraMarca(objetivo, scEstaUnidad, ref bonusAtaqueTotal, ref criticoRango, ref danioMarca);
       //----

       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaqueTotal, criticoRango, objetivo, 0);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;

        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        
     
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioMarca);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Aca van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarAgilidad;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
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
        await BattleManager.DelayCombateAsync(200);

    GameObject flechaPrefab = null;
    if (scEstaUnidad.bonusdam_fuego > 0)
    {
       flechaPrefab = BattleManager.Instance.contenedorPrefabs.FlechaFuego;
    }
    else
    {
       flechaPrefab = BattleManager.Instance.contenedorPrefabs.Flecha;
    }
        
        if (flechaPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(flechaPrefab);
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
            flight.Configure(transform, destino, 0.45f, 5.8f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(150);
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
      
      
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance,hAncho);
    
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        
         
    }

    bool ChequearTieneMarcarPresa(Unidad obj)
    { 
      if(obj.GetComponent<MarcaMarcarPresa>() != null)
      {
        if(obj.GetComponent<MarcaMarcarPresa>().quienMarco == scEstaUnidad)
        {
          return true;
        }
      
      }
      return false;
    }
 
}













