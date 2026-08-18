using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class TiroconArcoAcido : Habilidad
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
    nombre = "Tiro con Arco Acido";
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
    daniodX = 8; //igual que Tiro con Arco
    tipoDanio = 2; //Cortante, igual que Tiro con Arco
    criticoRangoHab = 0;

    requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.



     tipoPorcentaje = 2;





    imHab = Resources.Load<Sprite>("imHab/Explorador_Tiroconarco");

    txtDescripcion = "<color=#5dade2><b>Tiro con Arco Ácido</b></color>\n\n";
    txtDescripcion += "<i>El explorador ataca con su arco al enemigo.</i>\n\n";
    txtDescripcion += "<i>+1d6 daño ácido.</i>\n\n";
    txtDescripcion += $"<color=#c8c8c8><b>Alcance: 7</b> -Ataque: <color=#ea0606>Agilidad +{bonusAtaque}</color> - Daño: Cortante 1d8+1 + daño ácido - Requiere 1 Flecha</color>\n\n";
    txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Valentía: {costoPM} </color>";
      
      if(TRADU.i.nIdioma == 2)
      {
        nombre = "Acid Bow Shot";
        txtDescripcion = "<color=#5dade2><b>Acid Bow Shot</b></color>\n\n";
        txtDescripcion += "<i>The ranger attacks the enemy with his bow.</i>\n\n";
        txtDescripcion += "<i>+1d6 Acid damage.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Range: 7</b> -Attack: <color=#ea0606>Agility +{bonusAtaque}</color> - Damage: Slashing 1d8+1 + Acid damage - Requires 1 Arrow</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valour Cost: {costoPM} </color>";
      }
      else if(TRADU.i.nIdioma == 3)
      {
        nombre = "Tiro com Arco Acido";
        txtDescripcion = "<color=#5dade2><b>Tiro com Arco Acido</b></color>\n\n";
        txtDescripcion += "<i>O explorador ataca o inimigo com seu arco.</i>\n\n";
        txtDescripcion += "<i>+1d6 de dano acido.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Alcance: 7</b> -Ataque: <color=#ea0606>Agilidade +{bonusAtaque}</color> - Dano: Cortante 1d8+1 + dano acido - Requer 1 Flecha</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Recarga: {cooldownMax} \n- Custo AP: {costoAP} \n- Custo Valentia: {costoPM} </color>";
      }
    ActualizarDescripcion();
    }
     public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      StatsDescripcionUI stats = ObtenerStatsDescripcionUI();
      int criticoMin = Mathf.Clamp(19 - (stats.CriticoRango + criticoRangoHab), 2, 20);
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      string rangoDanio = FormatearRangoDados(XdDanio, daniodX, 1);
      string bonusTirada = FormatoModificadorDescripcion(stats.Ataque) + FormatoModificadorDescripcion(bonusAtaque);
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorAgilidad = "#7fa35a";
      string atributo = esIngles
        ? $"<color={colorAgilidad}>Agility ({stats.Agilidad})</color>"
        : esPortugues
          ? $"<color={colorAgilidad}>Agilidade ({stats.Agilidad})</color>"
          : $"<color={colorAgilidad}>Agilidad ({stats.Agilidad})</color>";
      string titulo = esIngles ? "Acid Bow Shot" : esPortugues ? "Tiro com Arco Acido" : "Tiro con Arco Acido";
      string subtitulo = esIngles ? "Bow shot with extra Acid damage." : esPortugues ? "Disparo de arco com dano Acido extra." : "Disparo de arco con dano Acido extra.";
      string efecto = esIngles ? "Extra damage: 1-6 Acid; on graze 1-2 Acid" : esPortugues ? "Dano extra: 1-6 Acido; em raspao 1-2 Acido" : "Dano extra: 1-6 Acido; en roce 1-2 Acido";

      if (esIngles)
      {
        string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agility ({stats.Agilidad})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
        string danioAcido = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "Acid damage", "dano_acido");
        string danioAcidoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "Acid damage");
        string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Arrow");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          "Acid Bow Shot",
          "Fires an arrow coated with acid.",
          new[]
          {
            LineaDescripcion("Target", "1 enemy or obstacle"),
            LineaDescripcion("Effect", $"On hit, deals {rangoDanio} + {agilidad} as {danioCortante} and 1-6 {danioAcido}."),
            LineaDescripcion("Graze", $"Deals 1-2 additional {danioAcidoSinIcono}.", 1),
            LineaDescripcion("Attack Roll", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
            LineaDescripcion("Cost", $"1 {flecha}")
          });
        return;
      }

      if (esPortugues)
      {
        string agilidade = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidade ({stats.Agilidad})");
        string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
        string danoCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "dano Cortante", "dano_cortante");
        string danoAcido = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "dano Ácido", "dano_acido");
        string danoAcidoSemIcone = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "dano Ácido");
        string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Tiro com Arco Ácido", "Dispara uma flecha revestida de ácido.", new[]
        {
          LineaDescripcion("Alvo", "1 inimigo ou obstáculo"),
          LineaDescripcion("Efeito", $"Ao acertar, causa {rangoDanio} + {agilidade} como {danoCortante} e 1-6 de {danoAcido}."),
          LineaDescripcion("De raspão", $"Causa 1-2 de {danoAcidoSemIcone} adicional.", 1),
          LineaDescripcion("Rolagem de Ataque", $"1d20 + {agilidade}{bonusTirada} vs {defesa}. Falha crítica: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion("Custo", $"1 {flecha}")
        });
        return;
      }

      {
        string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidad ({stats.Agilidad})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
        string danoCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "daño Cortante", "dano_cortante");
        string danoAcido = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "daño Ácido", "dano_acido");
        string danoAcidoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioAcido, "daño Ácido");
        string flecha = TerminoDescripcion(TerminoDescripcionId.Flecha, "Flecha");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Tiro con Arco Ácido", "Dispara una flecha recubierta de ácido.", new[]
        {
          LineaDescripcion("Objetivo", "1 enemigo u obstáculo"),
          LineaDescripcion("Efecto", $"Al impactar, inflige {rangoDanio} + {agilidad} como {danoCortante} y 1-6 de {danoAcido}."),
          LineaDescripcion("Roce", $"Inflige 1-2 de {danoAcidoSinIcono} adicional.", 1),
          LineaDescripcion("Tirada de Ataque", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Pifia: 5%. {critico}: {criticoPorcentaje}%."),
          LineaDescripcion("Costo", $"1 {flecha}")
        });
        return;
      }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged attack</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Range:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy or obstacle in range</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defense. Fumble: 5%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Type: Slashing</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Cost:</b></color> <color={colorValor}>1 Arrow</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Weapon effect:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else if (esPortugues)
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alcance:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo ou obstaculo no alcance</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defesa. Falha critica: 5%. Critico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Custo:</b></color> <color={colorValor}>1 Flecha</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efeito da arma:</b></color> <color={colorValor}>{efecto}</color>";
      }
      else
      {
        cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Alcance:</b></color> <color={colorValor}>{hAlcance}</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo u obstáculo en alcance</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + {atributo}{bonusTirada} vs Defensa. Pifia: 5%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanio} + {atributo}. Tipo: Cortante</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Costo:</b></color> <color={colorValor}>1 Flecha</color>\n";
        cuerpo += $"<color={colorEncabezado}><b>Efecto del arma:</b></color> <color={colorValor}>{efecto}</color>";
      }

      txtDescripcion = ConstruirDescripcionTooltipNueva(titulo, subtitulo, cuerpo);
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

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    {
    
     if(obj is Unidad) // van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;
       
       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int bonusAtaqueTotal = bonusAtaque;

       MarcaMarcarPresa.AplicarBonosContraMarca(objetivo, scEstaUnidad, ref bonusAtaqueTotal, ref criticoRango, ref danioMarca);

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

        float danioAcido = TiradaDeDados.TirarDados(1, 2); //1d2 de
        objetivo.RecibirDanioSinBonusElemental(danioAcido, 7, false, scEstaUnidad);



      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        float danioAcido = TiradaDeDados.TirarDados(1, 6); //1d6 de
        objetivo.RecibirDanioSinBonusElemental(danioAcido, 7, false, scEstaUnidad);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Crítico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 1 + scEstaUnidad.mod_CarAgilidad;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioMarca);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        float danioAcido = TiradaDeDados.TirarDados(1, 6); //1d6 de 
        objetivo.RecibirDanioSinBonusElemental(danioAcido, 7, true, scEstaUnidad);

      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //A van los efectos a Obstaculos
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




