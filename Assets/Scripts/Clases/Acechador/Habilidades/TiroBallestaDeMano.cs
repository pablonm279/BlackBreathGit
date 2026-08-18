using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class TiroBallestaDeMano : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Acido - 8: Arcano

    private int hAlcance = 3;
    private int hAncho = 1; //1 - adyancentes tambien
    ClaseAcechador claseAcechador;
     public override void  Awake()
    {
      nombre = "Tiro Ballesta de Mano";
      costoAP = 2;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 2;
      bAfectaObstaculos = true;

      bonusAtaque = 0;
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 2; //Perforante
      criticoRangoHab = 0;

      tipoPorcentaje = 2;

      imHab = Resources.Load<Sprite>("imHab/Acechador_BallestaDeMano");
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
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño.</i>\n\n";

    }
    else if (NivelMaestria == 2)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +5% Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 3)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Ataque +2 Daño +5% Crítico, -1 AP.</i>\n\n";


    }
    else if (NivelMaestria == 4)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      costoAP -= 1; //costo AP -1
      hAlcance += 1; //Alcance +1
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: +1 Alcance +1 Ataque +2 Daño +5% Crítico.</i>\n\n";

    }
    else if (NivelMaestria == 5)
    {
      bonusAtaque = 1;
      damExtra += 2;
      criticoRangoHab = 1;
      cooldownMax -= 1; //Cooldown -1
      costoAP -= 1; //costo AP -1
      cooldownActual = 0;
      txtDescripcion += "\n\n<i>Maestría con Ballesta de Mano agrega: Remueve Cooldown, +1 Ataque +2 Daño +5% Critico.</i>\n\n";

    }

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
    int nivelMaestria = claseAcechador != null ? claseAcechador.PASIVA_MaestriaConBallestaMano : 0;
    string rangoDanio = FormatearRangoDados(1, 10, damExtra);
    int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string colorAgilidad = "#7fa35a";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = cooldownMax > 0 ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}" : $"{costoAP} {iconoAP}";
    string titulo = esIngles ? "Hand Crossbow Shot" : esPortugues ? "Tiro de Besta de Mao" : "Tiro Ballesta de Mano";
    string subtitulo = esIngles
      ? "Ranged shot against one target."
      : esPortugues
        ? "Disparo a distancia contra um alvo."
        : "Disparo a distancia contra un objetivo.";
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
      string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Target", "1 target or obstacle"),
        LineaDescripcion("Effect", $"On hit, deals {rangoDanio} + {agilidad} as {danioPerforante}."),
        LineaDescripcion("Attack Roll", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%.")
      };
      if (nivelMaestria > 0)
      {
        lineas.Add(LineaDescripcion("Passive", $"Hand Crossbow Mastery (Tier {nivelMaestria})."));
      }
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        "Hand Crossbow Shot",
        "Hand crossbow attack. Fires at one target.",
        lineas);
      return;
    }

    if (esPortugues)
    {
      string agilidade = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidade ({agilidadActual})"); string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa"); string dano = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "dano Perfurante", "dano_perforante"); string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      var lineas = new List<LineaDescripcionNormalizada> { LineaDescripcion("Alvo", "1 alvo ou obstáculo"), LineaDescripcion("Efeito", $"Ao acertar, causa {rangoDanio} + {agilidade} como {dano}."), LineaDescripcion("Rolagem de Ataque", $"1d20 + {agilidade}{bonusTirada} vs {defesa}. Falha crítica: 5%. {critico}: {criticoPorcentaje}%.") };
      if (nivelMaestria > 0) lineas.Add(LineaDescripcion("Passiva", $"Maestria com Besta de Mão (Nível {nivelMaestria})."));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Disparo de Besta de Mão", "Ataque com besta de mão. Dispara contra um alvo.", lineas); return;
    }
    {
      string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agilidad ({agilidadActual})"); string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa"); string dano = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "daño Perforante", "dano_perforante"); string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      var lineas = new List<LineaDescripcionNormalizada> { LineaDescripcion("Objetivo", "1 objetivo u obstáculo"), LineaDescripcion("Efecto", $"Al impactar, inflige {rangoDanio} + {agilidad} como {dano}."), LineaDescripcion("Tirada de Ataque", $"1d20 + {agilidad}{bonusTirada} vs {defensa}. Pifia: 5%. {critico}: {criticoPorcentaje}%.") };
      if (nivelMaestria > 0) lineas.Add(LineaDescripcion("Pasiva", $"Maestría con Ballesta de Mano (Nivel {nivelMaestria})."));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Disparo de Ballesta de Mano", "Ataque con ballesta de mano. Dispara a un objetivo.", lineas); return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged attack ({hAlcance} range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy or obstacle in range\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Type:</b></color> Piercing";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passive:</b></color> Hand Crossbow Mastery (Tier {nivelMaestria})";
      }
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} de alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo ou obstaculo no alcance\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Passiva:</b></color> Maestria com Besta de Mao (Tier {nivelMaestria})";
      }
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque a distancia ({hAlcance} alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo u obstáculo en rango\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
      cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante";
      if (nivelMaestria > 0)
      {
        cuerpo += $"\n<color={colorEncabezado}><b>Pasiva:</b></color> Maestria con Ballesta de Mano (Tier {nivelMaestria})";
      }
    }

    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;
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
    
     if(obj is Unidad) //Aca van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       
      

       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0); 
            
     
       if(resultadoTirada == -1)
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+damExtra+scEstaUnidad.mod_CarAgilidad;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+damExtra+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Crítico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+damExtra+scEstaUnidad.mod_CarAgilidad;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Aca van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+damExtra+scEstaUnidad.mod_CarAgilidad;
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
        await BattleManager.DelayCombateAsync(100);

        GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.ViroteBallestadeMano;
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
            flight.Configure(transform, destino, 0.12f, 6.1f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(200);
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








