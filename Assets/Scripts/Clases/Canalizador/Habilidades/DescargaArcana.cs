using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class DescargaArcana : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private int hAlcance = 4;
    private int hAncho = 1; //1 - adyancentes también
     public override void  Awake()
    {
      nombre = "Descarga Arcana";
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
      forzarPoseHabilidad = false;
      fuerzaPoseAtaque = true;
      cooldownMax = 0;
      bAfectaObstaculos = true;

      bonusAtaque = 0;
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 8; //Arcano
      criticoRangoHab = 0;



    tipoPorcentaje = 3;

    imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaArcana");
    ActualizarDescripcion();
  }
     public override void ActualizarDescripcion()
    {
      var statsUI = ObtenerStatsDescripcionUI();
      int poderActual = statsUI.Poder;
      int ataqueActual = statsUI.Ataque;
      int criticoBonusUnidad = statsUI.CriticoRango;
      string bonusTexto = bonusAtaque != 0 ? $" + {bonusAtaque}" : "";
      string rangoDanioEs = FormatearRangoDados(1, 10, 1);
      int criticoMin = 19 - (criticoBonusUnidad + criticoRangoHab);
      criticoMin = Mathf.Clamp(criticoMin, 2, 20);
      int pifiaPorcentaje = 5;
      int criticoPorcentaje = Mathf.Clamp(21 - criticoMin, 0, 20) * 5;
      int modificadorAtaqueExtra = ataqueActual + bonusAtaque;
      string ataqueTxt = modificadorAtaqueExtra == 0
        ? string.Empty
        : modificadorAtaqueExtra > 0 ? $" + {modificadorAtaqueExtra}" : $" - {Mathf.Abs(modificadorAtaqueExtra)}";
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}";

      // Valor visible para la descripcion: nunca mostrar mas de 5.
      int alcanceVisible = Mathf.Clamp(hAlcance + 1, 1, 5);
      string lineaTipoEs = esMelee ? "<b>Tipo:</b> Melee" : $"<b>Tipo:</b> Rango ({alcanceVisible} alcance)";
      string lineaTipoEn = esMelee ? "<b>Type:</b> Melee" : $"<b>Type:</b> Ranged ({alcanceVisible} range)";
      string anchoDetalleEs = hAncho == 0 ? "solo fila objetivo" : "fila objetivo + adyacentes";
      string anchoDetalleEn = hAncho == 0 ? "target row only" : "target row + adjacent";
      string titulo = TRADU.i != null && TRADU.i.nIdioma == 2 ? "Arcane Discharge" : "Descarga Arcana";
      string subtitulo = TRADU.i != null && TRADU.i.nIdioma == 2
        ? "Ranged arcane attack against one enemy."
        : TRADU.i != null && TRADU.i.nIdioma == 3
          ? "Ataque arcano à distância contra um inimigo."
          : "Ataque arcano a distancia contra un enemigo.";
      string cuerpoFormato = "";
      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged attack ({alcanceVisible} range)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 enemy in range</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Width:</b></color> <color={colorValor}>{hAncho} ({anchoDetalleEn})</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Roll:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Power ({poderActual})</color>{ataqueTxt} vs Defense. Fumble: {pifiaPorcentaje}%. Crit: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Damage:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Power ({poderActual})</color>. Type: Arcane</color>";
      }
      else if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque à distância ({alcanceVisible} alcance)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 inimigo em alcance</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Largura:</b></color> <color={colorValor}>{hAncho} ({(hAncho == 0 ? "apenas fila alvo" : "fila alvo + adjacentes")})</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Rolagem:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Poder ({poderActual})</color>{ataqueTxt} vs Defesa. Falha crítica: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Dano:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>";
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Ataque a distancia ({alcanceVisible} alcance)</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 enemigo en rango</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Ancho:</b></color> <color={colorValor}>{hAncho} ({anchoDetalleEs})</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Tirada:</b></color> <color={colorValor}>1d20 + <color={colorPoder}>Poder ({poderActual})</color>{ataqueTxt} vs Defensa. Pifia: {pifiaPorcentaje}%. Crítico: {criticoPorcentaje}%</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Daño:</b></color> <color={colorValor}>{rangoDanioEs} + <color={colorPoder}>Poder ({poderActual})</color>. Tipo: Arcano</color>";
      }

      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        string cuerpo = "";
        cuerpo += lineaTipoEn + "\n";
        cuerpo += "<b>Target:</b> 1 enemy in range\n";
        cuerpo += $"<b>Width:</b> {hAncho} ({anchoDetalleEn})\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Power ({poderActual})</color>  {bonusTexto} vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
        cuerpo += $"<b>Damage:</b> 1d10 + 1 + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Arcane";

        string costos = $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM} ";

        txtDescripcion = ConstruirDescripcionEstandar(
          "Arcane Discharge",
          "The channeler launches a burst of energy at an enemy, dealing arcane damage.",
          cuerpo,
          costos,
          "#5dade2");
        txtDescripcion =
          $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
          $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
          "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
          cuerpoFormato;
        return;
      }
      if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
        string cuerpo = "";
        cuerpo += lineaTipoEs.Replace("Rango", "Distancia") + "\n";
        cuerpo += "<b>Alvo:</b> 1 inimigo em alcance\n";
        cuerpo += $"<b>Largura:</b> {hAncho} ({(hAncho == 0 ? "apenas fila alvo" : "fila alvo + adjacentes")})\n";
        cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}){bonusTexto} vs Defesa. Falha critica: 1. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Dano:</b> 1d10 + 1 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Arcano";

        string costos = $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM} ";

        txtDescripcion = ConstruirDescripcionEstandar(
          "Descarga Arcana",
          "O canalizador lanca uma descarga de energia em um inimigo, causando dano arcano.",
          cuerpo,
          costos,
          "#5dade2");
        txtDescripcion =
          $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
          $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
          "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
          cuerpoFormato;
        return;
      }

      {
        string cuerpo = "";
        cuerpo += lineaTipoEs + "\n";
        cuerpo += "<b>Objetivo:</b> 1 enemigo en rango\n";
        cuerpo += $"<b>Ancho:</b> {hAncho} ({anchoDetalleEs})\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Pod ({poderActual})</color> + Ataque ({ataqueActual}){bonusTexto} vs Defensa. Pifia: 1. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Danio:</b> {rangoDanioEs} + <color=#ea0606>Pod ({poderActual})</color> | <b>Tipo:</b> Arcano";

        string costos = $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM} ";

        txtDescripcion = ConstruirDescripcionEstandar(
          "Descarga Arcana",
          "El canalizador lanza una descarga de energia a un enemigo, haciendo danio arcano.",
          cuerpo,
          costos,
          "#5dade2");
        txtDescripcion =
          $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
          $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
          "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
          cuerpoFormato;
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
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
             
       int danioMarca = 0;
      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       

       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
            
     
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }
    internal Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private async Task LanzarProyectilAsync(object objetivo)
    {
        await BattleManager.DelayCombateAsync(50);

        GameObject proyectilPrefab = BattleManager.Instance.contenedorPrefabs.DescargaArcana;
        if (proyectilPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(proyectilPrefab);
        ArrowFlight flight = proyectil.GetComponent<ArrowFlight>();

        Transform destino = null;
        if (objetivo is Unidad unidad)
        {
            destino = unidad.transform;
        }
        else if (objetivo is Obstaculo obstaculo)
        {
            destino = obstaculo.transform;
        }

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.30f, 5.3f);
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




