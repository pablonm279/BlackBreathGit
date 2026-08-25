using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
      omitirAnimacionDeUso = true;
      cooldownMax = 0;
      bAfectaObstaculos = true;

      bonusAtaque = 1;
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
      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Power ({poderActual})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage", "dano_arcano");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          "Arcane Discharge",
          "Launches a focused burst of Arcane energy at one target.",
          new[]
          {
            LineaDescripcion("Target", "1 unit or obstacle"),
            LineaDescripcion("Effect", $"On hit, deals {rangoDanioEs} + {poder} as {danioArcano}."),
            LineaDescripcion("Attack Roll", $"1d20 + {poder}{ataqueTxt} vs {defensa}. Fumble: {pifiaPorcentaje}%. {critico}: {criticoPorcentaje}%."),
            LineaDescripcion("Effort", $"Up to {esforzable} AP.")
          });
        return;
      }
      {
        bool pt = TRADU.i != null && TRADU.i.nIdioma == 3;
        string poder = TerminoDescripcion(TerminoDescripcionId.Poder, $"Poder ({poderActual})");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, pt ? "Defesa" : "Defensa", "IconoDefensa");
        string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano", "dano_arcano");
        string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        string pifia = pt ? "Falha crítica" : "Pifia";
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          "Descarga Arcana",
          pt ? "Lança uma rajada concentrada de energia Arcana contra um alvo." : "Lanza una descarga concentrada de energía Arcana contra un objetivo.",
          new[]
          {
            LineaDescripcion(pt ? "Alvo" : "Objetivo", pt ? "1 unidade ou obstáculo" : "1 unidad u obstáculo"),
            LineaDescripcion(pt ? "Efeito" : "Efecto", $"{(pt ? "Ao acertar, causa" : "Al impactar, inflige")} {rangoDanioEs} + {poder} como {danioArcano}."),
            LineaDescripcion(pt ? "Rolagem de ataque" : "Tirada de ataque", $"1d20 + {poder}{ataqueTxt} vs {defensa}. {pifia}: {pifiaPorcentaje}%. {critico}: {criticoPorcentaje}%."),
            LineaDescripcion(pt ? "Esforço" : "Esfuerzo", $"{(pt ? "Até" : "Hasta")} {esforzable} AP.")
          });
        return;
      }
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
        cuerpo += $"<b>Daño:</b> 1d10 + 1 + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Arcano";

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
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Pod ({poderActual})</color> + Ataque ({ataqueActual}){bonusTexto} vs Defensa. Pifia: 1. Crítico: {criticoMin}-20\n";
        cuerpo += $"<b>Daño:</b> {rangoDanioEs} + <color=#ea0606>Pod ({poderActual})</color> | <b>Tipo:</b> Arcano";

        string costos = $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM} ";

        txtDescripcion = ConstruirDescripcionEstandar(
          "Descarga Arcana",
          "El canalizador lanza una descarga de energía a un enemigo, haciendo daño arcano.",
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

    public override async Task Resolver(List<object> objetivos, Casilla casillaOrigenTrampas = null)
    {
      UnidadPoseController poseController = scEstaUnidad != null
        ? scEstaUnidad.GetComponent<UnidadPoseController>()
        : null;

      poseController?.EnterAttackPoseHold();
      try
      {
        await base.Resolver(objetivos, casillaOrigenTrampas);
      }
      finally
      {
        poseController?.ExitAttackPoseHold();
      }
    }
    
      

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return BattleManager.DelayCombateAsync(180);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return BattleManager.DelayCombateAsync(320 + ObtenerNivelEnergia() * 25);
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
       int energia = ObtenerNivelEnergia();
       Vector3 puntoImpacto = DescargaArcanaRayoFx.ObtenerPuntoDestino(objetivo.gameObject);
       DescargaArcanaRayoFx.Crear(scEstaUnidad, objetivo.gameObject, energia, resultadoTirada <= 0);
            
     
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

         DescargaArcanaImpactoFx.Crear(objetivo, puntoImpacto);
         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

         DescargaArcanaImpactoFx.Crear(objetivo, puntoImpacto);
         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Crítico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+1+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje+danioMarca);
      
         DescargaArcanaImpactoFx.Crear(objetivo, puntoImpacto);
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---

       DescargaArcanaRayoFx.Crear(scEstaUnidad, objetivo.gameObject, ObtenerNivelEnergia(), false);


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
        DesacoplarAudioDelProyectil(proyectil);
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

    private static void DesacoplarAudioDelProyectil(GameObject proyectil)
    {
        if (proyectil == null)
        {
            return;
        }

        AudioSource[] audioSources = proyectil.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source == null || source.clip == null)
            {
                continue;
            }

            GameObject audioGo = new GameObject("SFX_DescargaArcana");
            audioGo.transform.position = source.transform.position;

            AudioSource audioIndependiente = audioGo.AddComponent<AudioSource>();
            audioIndependiente.clip = source.clip;
            audioIndependiente.outputAudioMixerGroup = source.outputAudioMixerGroup;
            audioIndependiente.volume = source.volume;
            audioIndependiente.pitch = source.pitch;
            audioIndependiente.priority = source.priority;
            audioIndependiente.spatialBlend = source.spatialBlend;
            audioIndependiente.rolloffMode = source.rolloffMode;
            audioIndependiente.minDistance = source.minDistance;
            audioIndependiente.maxDistance = source.maxDistance;
            audioIndependiente.dopplerLevel = source.dopplerLevel;
            audioIndependiente.Play();

            source.Stop();
            source.enabled = false;

            float duracion = source.clip.length;
            float pitch = Mathf.Abs(audioIndependiente.pitch);
            if (pitch > 0.001f)
            {
                duracion /= pitch;
            }

            Destroy(audioGo, duracion + 0.05f);
        }
    }

    private int ObtenerNivelEnergia()
    {
        ClaseCanalizador canalizador = scEstaUnidad as ClaseCanalizador;
        return canalizador != null ? Mathf.Clamp(canalizador.ObtenerEnergia(), 0, 3) : 0;
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


public sealed class DescargaArcanaRayoFx : MonoBehaviour
{
  private const float DuracionBase = 0.28f;
  private const int Segmentos = 15;
  private const int OrdenRayo = 5000;

  private Vector3 origen;
  private Vector3 destino;
  private int energia;
  private float tiempo;
  private float proximaVariacion;
  private LineRenderer nucleo;
  private LineRenderer halo;
  private LineRenderer[] rayosSecundarios;
  private LineRenderer[] ramificaciones;
  private Vector3[] puntosPrincipales;

  private static Material materialRayo;
  private static Material materialParticulas;
  private static Texture2D texturaParticula;
  private static AudioClip sonidoDescarga;

  public static void Crear(Unidad usuario, GameObject objetivo, int nivelEnergia, bool fallo)
  {
    if (usuario == null || objetivo == null)
    {
      return;
    }

    Vector3 puntoOrigen = usuario.puntoSaliente != null
      ? usuario.puntoSaliente.position
      : usuario.transform.position + Vector3.up * 0.25f;
    Vector3 puntoDestino = ObtenerPuntoDestino(objetivo);
    if (fallo)
    {
      puntoDestino = DesviarCercaDelObjetivo(puntoOrigen, puntoDestino);
    }

    GameObject go = new GameObject("VFX_DescargaArcana_Rayo");
    if (BattleManager.Instance != null)
    {
      go.transform.SetParent(BattleManager.Instance.transform, true);
    }

    DescargaArcanaRayoFx fx = go.AddComponent<DescargaArcanaRayoFx>();
    fx.Inicializar(puntoOrigen, puntoDestino, Mathf.Clamp(nivelEnergia, 0, 3));

    if (sonidoDescarga == null)
    {
      sonidoDescarga = Resources.Load<AudioClip>("Sonidos/Efectos/Descargaarcana");
    }
    if (sonidoDescarga != null)
    {
      AjustesAudio.ReproducirClipEnPunto(sonidoDescarga, puntoOrigen, 0.72f);
    }
  }

  private void Inicializar(Vector3 puntoOrigen, Vector3 puntoDestino, int nivelEnergia)
  {
    origen = puntoOrigen;
    destino = puntoDestino;
    energia = nivelEnergia;
    puntosPrincipales = new Vector3[Segmentos];

    nucleo = CrearLinea(
      "Nucleo",
      0.011f + energia * 0.0015f,
      OrdenRayo + 1,
      new Color(0.76f, 0.98f, 1.35f, 1f),
      new Color(0.18f, 0.58f, 1.25f, 0.92f));
    halo = CrearLinea(
      "Halo",
      0.025f + energia * 0.0025f,
      OrdenRayo,
      new Color(0.16f, 0.62f, 1.25f, 0.24f),
      new Color(0.08f, 0.24f, 1.05f, 0.08f));

    rayosSecundarios = new LineRenderer[energia + 1];
    for (int i = 0; i < rayosSecundarios.Length; i++)
    {
      rayosSecundarios[i] = CrearLinea(
        "RayoSecundario_" + i,
        0.0048f + energia * 0.00045f,
        OrdenRayo + 1,
        new Color(0.48f, 0.88f, 1.30f, 0.66f),
        new Color(0.10f, 0.36f, 1.15f, 0.28f));
    }

    ramificaciones = new LineRenderer[energia * 2 + 1];
    for (int i = 0; i < ramificaciones.Length; i++)
    {
      ramificaciones[i] = CrearLinea(
        "Ramificacion_" + i,
        0.0075f + energia * 0.0011f,
        OrdenRayo + 1,
        new Color(0.50f, 0.90f, 1.35f, 0.92f),
        new Color(0.08f, 0.34f, 1.10f, 0.14f));
      ramificaciones[i].widthCurve = new AnimationCurve(
        new Keyframe(0f, 0.82f),
        new Keyframe(0.18f, 1f),
        new Keyframe(0.72f, 0.68f),
        new Keyframe(1f, 0.28f));
    }

    Redibujar();
    CrearEfectoOrigen();
    CrearMicroparticulas();
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float duracion = DuracionBase + energia * 0.025f;
    if (tiempo >= duracion)
    {
      Destroy(gameObject);
      return;
    }

    if (tiempo >= proximaVariacion)
    {
      Redibujar();
      proximaVariacion = tiempo + 0.028f;
    }

    float alpha = 1f - Mathf.InverseLerp(duracion * 0.58f, duracion, tiempo);
    float brilloEnergia = 1f + energia * 0.14f;
    nucleo.startColor = new Color(0.76f, 0.98f, 1.35f, alpha * brilloEnergia);
    nucleo.endColor = new Color(0.18f, 0.58f, 1.25f, alpha * 0.92f * brilloEnergia);
    halo.startColor = new Color(0.16f, 0.62f, 1.25f, alpha * (0.22f + energia * 0.025f));
    halo.endColor = new Color(0.08f, 0.24f, 1.05f, alpha * 0.07f);

    for (int i = 0; i < rayosSecundarios.Length; i++)
    {
      LineRenderer secundario = rayosSecundarios[i];
      secundario.startColor = new Color(0.48f, 0.88f, 1.30f, alpha * (0.56f + energia * 0.05f));
      secundario.endColor = new Color(0.10f, 0.36f, 1.15f, alpha * 0.24f);
    }

    for (int i = 0; i < ramificaciones.Length; i++)
    {
      LineRenderer rama = ramificaciones[i];
      rama.startColor = new Color(0.50f, 0.90f, 1.35f, alpha * (0.84f + energia * 0.05f));
      rama.endColor = new Color(0.08f, 0.34f, 1.10f, alpha * 0.12f);
    }
  }

  private void Redibujar()
  {
    Vector3 direccion = destino - origen;
    Vector3 normal = Vector3.Cross(direccion.normalized, Vector3.up);
    if (normal.sqrMagnitude < 0.001f)
    {
      normal = Vector3.Cross(direccion.normalized, Vector3.right);
    }
    normal.Normalize();
    Vector3 binormal = Vector3.Cross(direccion.normalized, normal).normalized;
    float desvio = 0.022f + energia * 0.006f;

    for (int i = 0; i < puntosPrincipales.Length; i++)
    {
      float t = i / (float)(puntosPrincipales.Length - 1);
      Vector3 punto = Vector3.Lerp(origen, destino, t);
      float intensidad = Mathf.Sin(t * Mathf.PI) * desvio;
      punto += normal * UnityEngine.Random.Range(-intensidad, intensidad);
      punto += binormal * UnityEngine.Random.Range(-intensidad, intensidad) * 0.38f;
      puntosPrincipales[i] = punto;
    }

    nucleo.SetPositions(puntosPrincipales);
    halo.SetPositions(puntosPrincipales);

    for (int i = 0; i < rayosSecundarios.Length; i++)
    {
      DibujarRayoSecundario(rayosSecundarios[i], i, normal, binormal);
    }

    for (int i = 0; i < ramificaciones.Length; i++)
    {
      DibujarRamificacion(ramificaciones[i], i, normal, binormal);
    }
  }

  private void DibujarRayoSecundario(LineRenderer linea, int indice, Vector3 normal, Vector3 binormal)
  {
    Vector3[] puntos = new Vector3[Segmentos];
    float signo = indice % 2 == 0 ? 1f : -1f;
    float separacion = 0.017f + (indice / 2) * 0.011f;

    for (int i = 0; i < puntos.Length; i++)
    {
      float t = i / (float)(puntos.Length - 1);
      float envolvente = Mathf.Sin(t * Mathf.PI);
      Vector3 punto = puntosPrincipales[i];
      punto += binormal * signo * separacion * envolvente;
      punto += normal * UnityEngine.Random.Range(-0.018f, 0.018f) * envolvente;
      punto += binormal * UnityEngine.Random.Range(-0.013f, 0.013f) * envolvente;
      puntos[i] = punto;
    }

    linea.positionCount = puntos.Length;
    linea.SetPositions(puntos);
  }

  private void CrearEfectoOrigen()
  {
    GameObject esferaGo = new GameObject("EsferaOrigen");
    esferaGo.transform.SetParent(transform, false);
    ParticleSystem esfera = esferaGo.AddComponent<ParticleSystem>();
    esfera.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystem.MainModule mainEsfera = esfera.main;
    mainEsfera.duration = 0.5f;
    mainEsfera.loop = false;
    mainEsfera.startSpeed = 0f;
    mainEsfera.simulationSpace = ParticleSystemSimulationSpace.World;
    mainEsfera.maxParticles = 1;

    ParticleSystem.EmissionModule emisionEsfera = esfera.emission;
    emisionEsfera.enabled = false;

    ParticleSystemRenderer rendererEsfera = esferaGo.GetComponent<ParticleSystemRenderer>();
    rendererEsfera.renderMode = ParticleSystemRenderMode.Billboard;
    rendererEsfera.alignment = ParticleSystemRenderSpace.View;
    rendererEsfera.material = ObtenerMaterialParticulas();
    rendererEsfera.sortingOrder = OrdenRayo + 2;
    rendererEsfera.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    rendererEsfera.receiveShadows = false;

    ParticleSystem.EmitParams esferaParams = new ParticleSystem.EmitParams
    {
      position = origen,
      velocity = Vector3.zero,
      startLifetime = DuracionBase + energia * 0.025f,
      startSize = 0.030f + energia * 0.0025f,
      startColor = new Color(0.42f, 0.88f, 1f, 0.58f + energia * 0.04f)
    };
    esfera.Emit(esferaParams, 1);
    esfera.Play();

    GameObject chispasGo = new GameObject("ChispasOrigen");
    chispasGo.transform.SetParent(transform, false);
    ParticleSystem chispas = chispasGo.AddComponent<ParticleSystem>();
    chispas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystem.MainModule mainChispas = chispas.main;
    mainChispas.duration = 0.5f;
    mainChispas.loop = false;
    mainChispas.startSpeed = 0f;
    mainChispas.simulationSpace = ParticleSystemSimulationSpace.World;
    mainChispas.maxParticles = 24;

    ParticleSystem.EmissionModule emisionChispas = chispas.emission;
    emisionChispas.enabled = false;

    ParticleSystemRenderer rendererChispas = chispasGo.GetComponent<ParticleSystemRenderer>();
    rendererChispas.renderMode = ParticleSystemRenderMode.Billboard;
    rendererChispas.alignment = ParticleSystemRenderSpace.View;
    rendererChispas.material = ObtenerMaterialParticulas();
    rendererChispas.sortingOrder = OrdenRayo + 3;
    rendererChispas.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    rendererChispas.receiveShadows = false;

    int cantidadChispas = 7 + energia * 2;
    for (int i = 0; i < cantidadChispas; i++)
    {
      ParticleSystem.EmitParams chispaParams = new ParticleSystem.EmitParams
      {
        position = origen + UnityEngine.Random.insideUnitSphere * 0.009f,
        velocity = UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.035f, 0.10f),
        startLifetime = UnityEngine.Random.Range(0.11f, 0.24f),
        startSize = UnityEngine.Random.Range(0.002f, 0.0055f),
        startColor = Color.Lerp(
          new Color(0.78f, 1f, 1f, 0.86f),
          new Color(0.16f, 0.48f, 1f, 0.68f),
          UnityEngine.Random.value)
      };
      chispas.Emit(chispaParams, 1);
    }
    chispas.Play();
  }

  private void DibujarRamificacion(LineRenderer linea, int indice, Vector3 normal, Vector3 binormal)
  {
    const int puntosRama = 4;
    Vector3[] puntos = new Vector3[puntosRama];
    int indiceInicio = 3 + (indice * 5 + energia) % (Segmentos - 6);
    Vector3 inicio = puntosPrincipales[indiceInicio];
    float signo = indice % 2 == 0 ? 1f : -1f;
    float longitud = 0.10f + energia * 0.035f + UnityEngine.Random.Range(0f, 0.04f);
    Vector3 direccionRama = (binormal * signo + normal * UnityEngine.Random.Range(-0.48f, 0.48f)).normalized;

    for (int i = 0; i < puntosRama; i++)
    {
      float t = i / (float)(puntosRama - 1);
      Vector3 punto = inicio + direccionRama * longitud * t;
      if (i > 0)
      {
        punto += normal * UnityEngine.Random.Range(-0.025f, 0.025f) * t;
        punto += binormal * UnityEngine.Random.Range(-0.021f, 0.021f) * t;
      }
      puntos[i] = punto;
    }

    linea.positionCount = puntos.Length;
    linea.SetPositions(puntos);
  }

  private LineRenderer CrearLinea(string nombre, float ancho, int orden, Color inicio, Color fin)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(transform, false);
    LineRenderer linea = go.AddComponent<LineRenderer>();
    linea.useWorldSpace = true;
    linea.alignment = LineAlignment.View;
    linea.material = ObtenerMaterialRayo();
    linea.widthMultiplier = ancho;
    linea.widthCurve = new AnimationCurve(
      new Keyframe(0f, 0.72f),
      new Keyframe(0.12f, 1f),
      new Keyframe(0.76f, 0.72f),
      new Keyframe(1f, 0.12f));
    linea.positionCount = Segmentos;
    linea.textureMode = LineTextureMode.Stretch;
    linea.numCapVertices = 3;
    linea.numCornerVertices = 2;
    linea.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    linea.receiveShadows = false;
    linea.sortingOrder = orden;
    linea.startColor = inicio;
    linea.endColor = fin;
    return linea;
  }

  private void CrearMicroparticulas()
  {
    GameObject go = new GameObject("Microparticulas");
    go.transform.SetParent(transform, false);
    ParticleSystem particulas = go.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystem.MainModule main = particulas.main;
    main.duration = 0.5f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.10f, 0.28f);
    main.startSpeed = 0f;
    main.startSize = new ParticleSystem.MinMaxCurve(0.0025f, 0.007f);
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = 96;

    ParticleSystem.EmissionModule emision = particulas.emission;
    emision.enabled = false;

    ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.alignment = ParticleSystemRenderSpace.View;
    renderer.material = ObtenerMaterialParticulas();
    renderer.sortingOrder = OrdenRayo + 2;
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = false;

    int cantidad = 18 + energia * 11;
    for (int i = 0; i < cantidad; i++)
    {
      float t = UnityEngine.Random.Range(0.06f, 1f);
      Vector3 posicion = Vector3.Lerp(origen, destino, t) + UnityEngine.Random.insideUnitSphere * 0.018f;
      ParticleSystem.EmitParams parametros = new ParticleSystem.EmitParams
      {
        position = posicion,
        velocity = UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.025f, 0.095f),
        startLifetime = UnityEngine.Random.Range(0.10f, 0.28f),
        startSize = UnityEngine.Random.Range(0.0025f, 0.007f),
        startColor = Color.Lerp(
          new Color(0.62f, 0.96f, 1f, 0.9f),
          new Color(0.16f, 0.42f, 1f, 0.72f),
          UnityEngine.Random.value)
      };
      particulas.Emit(parametros, 1);
    }

    particulas.Play();
  }

  public static Vector3 ObtenerPuntoDestino(GameObject objetivo)
  {
    Unidad unidad = objetivo.GetComponent<Unidad>();
    if (unidad != null)
    {
      return unidad.puntoEntrante != null ? unidad.puntoEntrante.position : unidad.transform.position + Vector3.up * 0.20f;
    }

    Obstaculo obstaculo = objetivo.GetComponent<Obstaculo>();
    if (obstaculo != null)
    {
      return obstaculo.puntoEntrante != null ? obstaculo.puntoEntrante.position : obstaculo.transform.position;
    }

    return objetivo.transform.position;
  }

  private static Vector3 DesviarCercaDelObjetivo(Vector3 puntoOrigen, Vector3 puntoObjetivo)
  {
    Vector3 direccion = puntoObjetivo - puntoOrigen;
    Vector3 lateral = Vector3.Cross(direccion.normalized, Vector3.up);
    if (lateral.sqrMagnitude < 0.001f)
    {
      lateral = Vector3.right;
    }
    lateral.Normalize();

    float lado = UnityEngine.Random.value < 0.5f ? -1f : 1f;
    return puntoObjetivo
      + Vector3.up * lado * UnityEngine.Random.Range(0.16f, 0.24f)
      + lateral * UnityEngine.Random.Range(-0.045f, 0.045f);
  }

  private static Material ObtenerMaterialRayo()
  {
    if (materialRayo == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }
      materialRayo = new Material(shader)
      {
        name = "Mat_DescargaArcana_RayoRuntime",
        hideFlags = HideFlags.HideAndDontSave,
        mainTexture = Texture2D.whiteTexture
      };
    }
    return materialRayo;
  }

  private static Material ObtenerMaterialParticulas()
  {
    if (materialParticulas == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }
      materialParticulas = new Material(shader)
      {
        name = "Mat_DescargaArcana_MicroParticulasRuntime",
        hideFlags = HideFlags.HideAndDontSave,
        mainTexture = ObtenerTexturaParticula()
      };
    }
    return materialParticulas;
  }

  private static Texture2D ObtenerTexturaParticula()
  {
    if (texturaParticula != null)
    {
      return texturaParticula;
    }

    const int tamano = 16;
    texturaParticula = new Texture2D(tamano, tamano, TextureFormat.ARGB32, false)
    {
      name = "Tex_DescargaArcana_MicroParticulaRuntime",
      hideFlags = HideFlags.HideAndDontSave,
      filterMode = FilterMode.Bilinear,
      wrapMode = TextureWrapMode.Clamp
    };

    Color[] pixeles = new Color[tamano * tamano];
    float centro = (tamano - 1) * 0.5f;
    for (int y = 0; y < tamano; y++)
    {
      for (int x = 0; x < tamano; x++)
      {
        float dx = (x - centro) / centro;
        float dy = (y - centro) / centro;
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy)), 2.4f);
        pixeles[y * tamano + x] = new Color(1f, 1f, 1f, alpha);
      }
    }
    texturaParticula.SetPixels(pixeles);
    texturaParticula.Apply(false, true);
    return texturaParticula;
  }
}


public class DescargaArcanaImpactoFx : MonoBehaviour
{
  private const float Duracion = 0.36f;
  private const int CantidadChispas = 4;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private RectTransform imagenUnidad;
  private Image halo;
  private Image nucleo;
  private Image anillo;
  private readonly Image[] chispas = new Image[CantidadChispas];
  private readonly float[] angulosChispa = new float[CantidadChispas];
  private readonly float[] largosChispa = new float[CantidadChispas];
  private float tiempo;
  private Vector2 tamanoBase;
  private Vector2 posicionImpacto;

  private static Sprite spriteSuave;
  private static Sprite spriteAnillo;
  private static Sprite spriteChispa;
  private static Texture2D texturaSuave;
  private static Texture2D texturaAnillo;
  private static Texture2D texturaChispa;

  public static void Crear(Unidad unidad)
  {
    if (unidad != null)
    {
      Crear(unidad, DescargaArcanaRayoFx.ObtenerPuntoDestino(unidad.gameObject));
    }
  }

  public static void Crear(Unidad unidad, Vector3 puntoImpactoMundo)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    RectTransform imagen = unidad.uImage.rectTransform;
    if (imagen == null || !(imagen.parent is RectTransform padre))
    {
      return;
    }

    Canvas canvas = unidad.uImage.GetComponentInParent<Canvas>(true);
    Camera camaraMundo = Camera.main;
    Camera camaraCanvas = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
      ? canvas.worldCamera != null ? canvas.worldCamera : camaraMundo
      : null;
    Vector2 puntoPantalla = RectTransformUtility.WorldToScreenPoint(camaraMundo, puntoImpactoMundo);
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(padre, puntoPantalla, camaraCanvas, out Vector2 puntoImpactoLocal))
    {
      return;
    }

    GameObject go = new GameObject("DescargaArcanaImpactoFx", typeof(RectTransform), typeof(CanvasGroup), typeof(DescargaArcanaImpactoFx));
    DescargaArcanaImpactoFx fx = go.GetComponent<DescargaArcanaImpactoFx>();
    fx.Inicializar(padre, imagen, puntoImpactoLocal);

    RenderOrderHelper.OrdenarCanvasEncima(canvas, unidad.transform, 10);
  }

  private void Inicializar(RectTransform padre, RectTransform imagen, Vector2 puntoImpactoLocal)
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

    posicionImpacto = puntoImpactoLocal;
    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.localPosition = new Vector3(posicionImpacto.x, posicionImpacto.y, 0f);
    root.localScale = imagenUnidad.localScale;
    root.sizeDelta = tamanoBase * 0.82f;

    int targetSibling = Mathf.Min(padre.childCount - 1, imagenUnidad.GetSiblingIndex() + 1);
    root.SetSiblingIndex(targetSibling);

    halo = CrearImagen("Halo", ObtenerSpriteSuave(), root);
    anillo = CrearImagen("Anillo", ObtenerSpriteAnillo(), root);
    nucleo = CrearImagen("Nucleo", ObtenerSpriteSuave(), root);

    for (int i = 0; i < chispas.Length; i++)
    {
      chispas[i] = CrearImagen("Chispa" + i, ObtenerSpriteChispa(), root);
      angulosChispa[i] = (i * (360f / chispas.Length)) + UnityEngine.Random.Range(-18f, 18f);
      largosChispa[i] = UnityEngine.Random.Range(0.18f, 0.32f);
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
    float t = Mathf.Clamp01(tiempo / Duracion);
    ActualizarVisual(t);

    if (tiempo >= Duracion)
    {
      Destroy(gameObject);
    }
  }

  private void ActualizarVisual(float t)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.08f));
    float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.28f) / 0.72f));
    float intensidad = entrada * salida;
    float golpe = 1f - Mathf.SmoothStep(0f, 1f, t);
    float escalaPulso = Mathf.Lerp(0.42f, 1.08f, Mathf.SmoothStep(0f, 1f, t));

    canvasGroup.alpha = intensidad;
    root.localPosition = new Vector3(posicionImpacto.x, posicionImpacto.y, 0f);
    root.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * 45f) * 2.2f * golpe);

    Configurar(halo, Vector2.zero, tamanoBase * (0.62f * escalaPulso), 0f, new Color(0.38f, 0.78f, 1f, 0.22f * intensidad));
    Configurar(anillo, Vector2.zero, tamanoBase * (0.38f + (0.46f * t)), 0f, new Color(0.74f, 0.96f, 1f, 0.42f * intensidad * golpe));
    Configurar(nucleo, Vector2.zero, tamanoBase * (0.18f + (0.08f * golpe)), 0f, new Color(0.92f, 1f, 1f, 0.58f * intensidad));

    for (int i = 0; i < chispas.Length; i++)
    {
      float rad = angulosChispa[i] * Mathf.Deg2Rad;
      Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
      Vector2 posicion = dir * tamanoBase.x * Mathf.Lerp(0.02f, 0.26f, t);
      Vector2 tamano = new Vector2(tamanoBase.x * largosChispa[i] * Mathf.Lerp(0.85f, 0.35f, t), Mathf.Max(1.6f, tamanoBase.y * 0.035f));
      Configurar(chispas[i], posicion, tamano, angulosChispa[i], new Color(0.68f, 0.96f, 1f, 0.44f * intensidad * golpe));
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

    const int size = 48;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "DescargaArcanaImpactoSoftRuntime";
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
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.4f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "DescargaArcanaImpactoSoftRuntime";
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
    texturaAnillo.name = "DescargaArcanaImpactoRingRuntime";
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
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - (borde / 0.18f)), 1.65f) * Mathf.Clamp01(1f - distancia);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAnillo.SetPixels(pixels);
    texturaAnillo.Apply(false, true);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteAnillo.name = "DescargaArcanaImpactoRingRuntime";
    return spriteAnillo;
  }

  private static Sprite ObtenerSpriteChispa()
  {
    if (spriteChispa != null)
    {
      return spriteChispa;
    }

    const int width = 64;
    const int height = 12;
    texturaChispa = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaChispa.name = "DescargaArcanaImpactoSparkRuntime";
    texturaChispa.wrapMode = TextureWrapMode.Clamp;
    texturaChispa.filterMode = FilterMode.Bilinear;
    texturaChispa.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = x / (width - 1f);
        float distanciaY = Mathf.Abs(y - centroY);
        float grosor = Mathf.Clamp01(1f - (distanciaY / 3.2f));
        float extremos = Mathf.SmoothStep(0f, 0.16f, nx) * (1f - Mathf.SmoothStep(0.76f, 1f, nx));
        float alpha = Mathf.Pow(grosor, 1.7f) * extremos;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaChispa.SetPixels(pixels);
    texturaChispa.Apply(false, true);
    spriteChispa = Sprite.Create(texturaChispa, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteChispa.name = "DescargaArcanaImpactoSparkRuntime";
    return spriteChispa;
  }
}
