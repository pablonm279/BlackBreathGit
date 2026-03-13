using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DescargaDesintegradora : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
    public override void  Awake()
    {
      nombre = "Descarga Desintegradora";
      IDenClase = 9;
      costoAP = 6;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = true;

      targetEspecial = 9; //Piramide
       tipoPorcentaje = 3;
      bonusAtaque = 5;
      XdDanio = 3;
      daniodX = 12; //3d12
      tipoDanio = 8; //Arcano
      criticoRangoHab = 2;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_DescargaDesintegradora");
      

      requiereRecurso = 2; //Requiere tener 2 Tier energía 

    }

    public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    var statsUI = ObtenerStatsDescripcionUI();

    int poderActual = statsUI.Poder;
    int ataqueActual = statsUI.Ataque;
    int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + 2), 2, 20);
    int danioFijo = NIVEL > 1 ? 8 : 0;
    int dcDesintegracion = NIVEL > 2 ? 10 : 9;
    int energiaRequerida = NIVEL == 5 ? 1 : 2;
    bool consumeEnergia = NIVEL != 5;
    bool aturdeCaster = NIVEL != 4;
    string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcDesintegracion);
    string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcDesintegracion);

    string tituloEs = "Descarga Desintegradora I";
    string tituloEn = "Disintegrating Discharge I";
    if (NIVEL == 2) { tituloEs = "Descarga Desintegradora II"; tituloEn = "Disintegrating Discharge II"; }
    if (NIVEL == 3) { tituloEs = "Descarga Desintegradora III"; tituloEn = "Disintegrating Discharge III"; }
    if (NIVEL == 4) { tituloEs = "Descarga Desintegradora IV a"; tituloEn = "Disintegrating Discharge IV a"; }
    if (NIVEL == 5) { tituloEs = "Descarga Desintegradora IV b"; tituloEn = "Disintegrating Discharge IV b"; }

    string danioEs = danioFijo > 0
      ? $"3d12 + {danioFijo} + <color=#ea0606>Poder ({poderActual})</color>"
      : $"3d12 + <color=#ea0606>Poder ({poderActual})</color>";
    string danioEn = danioFijo > 0
      ? $"3d12 + {danioFijo} + <color=#ea0606>Power ({poderActual})</color>"
      : $"3d12 + <color=#ea0606>Power ({poderActual})</color>";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> Pyramid area\n";
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Power ({poderActual})</color>   + 5 vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
      cuerpo += $"<b>Damage:</b> {danioEn} | <b>Type:</b> Arcane\n";
      cuerpo += $"{lineaSalvacionEn}. On failed save: disintegrated (instant kill)\n";
      cuerpo += consumeEnergia
        ? "<b>Cast Drawback:</b> -1 Energy Tier"
        : "<b>Cast Drawback:</b> Does not consume Energy Tier";
      cuerpo += "\n";
      cuerpo += aturdeCaster
        ? "<b>Cast Drawback:</b> User is Stunned for 1 turn"
        : "<b>Cast Drawback:</b> Does not Stun the user";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> Area en piramide\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}) + 5 vs Defensa. Pifia: 1. Critico: {criticoMin}-20\n";
      cuerpo += $"<b>Danio:</b> {danioEs} | <b>Tipo:</b> Arcano\n";
      cuerpo += $"{lineaSalvacionEs}. Si falla TS: desintegrado (muerte instantanea)\n";
      cuerpo += consumeEnergia
        ? "<b>Costo al lanzar:</b> -1 Nivel de Energia"
        : "<b>Costo al lanzar:</b> No consume Nivel de Energia";
      cuerpo += "\n";
      cuerpo += aturdeCaster
        ? "<b>Costo al lanzar:</b> El usuario queda Aturdido 1 turno"
        : "<b>Costo al lanzar:</b> No Aturde al usuario";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})\n- Requires Energy Tier: {energiaRequerida}+"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})\n- Requiere Nivel de Energia: {energiaRequerida}+";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "At peak charge, the Channeler unleashes a high-risk detonation that can erase targets outright."
        : "Con la energia al maximo, el Canalizador libera una detonacion de alto riesgo capaz de borrar objetivos.",
      cuerpo,
      costos,
      "#e67e22");

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +8 damage.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no stun) or Option B (no Energy loss).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +8 de danio.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (sin aturdimiento) u Opcion B (sin perdida de Energia).</color>"; }
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

  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
  {
    // El log de uso ahora está centralizado en Habilidad.Resolver
  await  base.Resolver(Objetivos);

    if (NIVEL != 4) { scEstaUnidad.estado_aturdido+=1; print(6565); }
    if(scEstaUnidad is ClaseCanalizador can){ if (NIVEL != 5) { can.CambiarEnergia(-1); } }
  }
    
    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        float delay = 0.7f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseHabilidad;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     { 
      
        Unidad objetivo = (Unidad)obj;       
       int danioExtra = 0;
       if (NIVEL > 1) { danioExtra += 3; }

       float defensaObjetivo = objetivo.ObtenerdefensaActual();
       print("Defensa: "+ defensaObjetivo);

       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
       print("Resultado tirada "+resultadoTirada);


      //----

      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
        scEstaUnidad.EstablecerAPActualA(0);
       VFXAplicar(objetivo.gameObject);
       }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
       VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (NIVEL > 1) { danio += 5; }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        EfectoAdicional(objetivo);
       VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + danioExtra + scEstaUnidad.mod_CarPoder;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + danioExtra);
        if (NIVEL > 1) { danio += 5; }

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
        EfectoAdicional(objetivo);
       VFXAplicar(objetivo.gameObject);
      }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);
       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder+2;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    void EfectoAdicional(Unidad Objetivo)
    {
        int dc = 9;
        if (NIVEL > 2) { dc += 1; }
    if (Objetivo.TiradaSalvacion(Objetivo.mod_TSFortaleza, dc))
    {
      Objetivo.RecibirDanio(Objetivo.mod_maxHP, 10, false, scEstaUnidad);
      string objetivoNombre = TRADU.i != null ? TRADU.i.Traducir(Objetivo.uNombre) : Objetivo.uNombre;
      string textoDesintegrado = TRADU.i != null ? TRADU.i.Traducir("fue Desintegrado.") : "fue Desintegrado.";
      BattleManager.Instance.EscribirLog(objetivoNombre + " " + textoDesintegrado);
    }
       
    }
  
    
    void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_DescargaDesintegradora");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

  }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

private void ObtenerObjetivos()
    {
      
     //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(6,0);
    
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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
    
         
    }

    
}






