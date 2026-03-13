using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Asesinar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
     public override void  Awake()
    {


      nombre = "Asesinar";
      costoAP = 3; 
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 6;
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 4; 
      bAfectaObstaculos = false;

      bonusAtaque = 0;
    
      XdDanio = 2;
      daniodX = 8; //2d8+2
      tipoDanio = 1; //Cortante
      criticoRangoHab = 0;


      tipoPorcentaje = 2;

      requiereRecurso = 1; //No requiere recurso


      imHab = Resources.Load<Sprite>("imHab/Acechador_Asesinar");
      ActualizarDescripcion();
    }
    
   void Start()
   {
  
   }

   public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    var statsUI = ObtenerStatsDescripcionUI();

    int agilidadActual = statsUI.Agilidad;
    int ataqueActual = statsUI.Ataque;
    int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);

    int danioFijo = 2 + (NIVEL > 1 ? 2 : 0) + (NIVEL == 5 ? 3 : 0);
    int bonoAtaqueAislado = 2 + (NIVEL > 2 ? 1 : 0);

    string tituloEs = "Asesinar I";
    string tituloEn = "Assassinate I";
    if (NIVEL == 2) { tituloEs = "Asesinar II"; tituloEn = "Assassinate II"; }
    if (NIVEL == 3) { tituloEs = "Asesinar III"; tituloEn = "Assassinate III"; }
    if (NIVEL == 4) { tituloEs = "Asesinar IV a"; tituloEn = "Assassinate IV a"; }
    if (NIVEL == 5) { tituloEs = "Asesinar IV b"; tituloEn = "Assassinate IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (4 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy\n";
      cuerpo += "<b>Requirement:</b> Hidden (1)\n";
      cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Agility ({agilidadActual})</color>   + {bonusAtaque} vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Damage:</b> 2d8 + {danioFijo} + <color=#ea0606>Agility ({agilidadActual})</color> | <b>Type:</b> Piercing\n";
      cuerpo += "<b>Humanoid bonus:</b> +2 flat damage\n";
      cuerpo += $"<b>If isolated:</b> +{bonoAtaqueAislado} attack and x2 final damage\n";
      cuerpo += "<b>On kill:</b> gains Hidden (1), skill cooldown is set to 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valour";
      }
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (4 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo\n";
      cuerpo += "<b>Requisito:</b> Escondido (1)\n";
      cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Agilidad ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonusAtaque} vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
      cuerpo += $"<b>Danio:</b> 2d8 + {danioFijo} + <color=#ea0606>Agilidad ({agilidadActual})</color> | <b>Tipo:</b> Perforante\n";
      cuerpo += "<b>Bono contra humanoides:</b> +2 danio plano\n";
      cuerpo += $"<b>Si esta aislado:</b> +{bonoAtaqueAislado} ataque y x2 al danio final\n";
      cuerpo += "<b>Al matar:</b> gana Escondido (1), el cooldown de la habilidad se fija en 1";
      if (NIVEL == 4)
      {
        cuerpo += ", +2 Valentía";
      }
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A burst finisher from stealth that spikes hard on isolated targets."
        : "Un remate explosivo desde sigilo que pega muy fuerte a objetivos aislados.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 flat damage.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack if target is isolated.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Valour on kill) or Option B (+3 flat damage).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de danio plano.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 ataque si el objetivo esta aislado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 Valentía al matar) u Opcion B (+3 de danio plano).</color>"; }
    }
  }

  int damExtra;
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
        float delay = 0.6f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return BattleManager.DelayCombateAsync(250);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();
      print("Defensa: " + defensaObjetivo);

      int danioMarca = 0;

      if (NIVEL > 1) { damExtra += 2; } //A partir del nivel 2, +2 de daño extra
      if (NIVEL == 5) { damExtra += 3; } //A Nv 5, +3 de daño extra

      if (objetivo.ChequearEstaAislado(2))
      {
        bonusAtaque += 2; //Si está aislado, +2 Ataque
        if (NIVEL > 2) { bonusAtaque++; } //A partir del nivel 3, +3 Ataque si está aislado
      }

      float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;

      int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, criticoRango, objetivo, 0);
      print("Resultado tirada " + resultadoTirada);


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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        VFXAplicar(objetivo.gameObject);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }

         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        }
        
         if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

      

      }
      else if (resultadoTirada == 3)
      {//CRITICO
                print("Critico");
        VFXAplicar(objetivo.gameObject);
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 2 + damExtra + scEstaUnidad.mod_CarAgilidad + danioMarca;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        if (objetivo.TieneTag("Humanoide"))
        { 
            danio += 2; //Si es humanoide, +2 de daño
        } 
        
        if (objetivo.ChequearEstaAislado(2))
        {
          danio *= 2; //Si está aislado, duplica el daño
        }


        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

       
      }

      fueElObjetivoAsesinado = objetivo;
      Invoke("ChequeoMuerteObjetivo", 3.0f); //Chequea si el objetivo murió, y aplica efectos de ser así.

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
    {
      Obstaculo objetivo = (Obstaculo)obj;
      //---

      VFXAplicar(objetivo.gameObject);
      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + 3 + damExtra + scEstaUnidad.mod_CarAgilidad;
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    }
  }
 Unidad fueElObjetivoAsesinado;
  void ChequeoMuerteObjetivo()
  {
    bool aplicarEfectos = false;
    if (fueElObjetivoAsesinado == null)
    {
      aplicarEfectos = true; //Si no existe se asume que murio
    } //Si no había objetivo, no hace nada
    else if (fueElObjetivoAsesinado.HP_actual < 1)
    {
      aplicarEfectos = true; //Si no tiene vida, murio
    }

    if (aplicarEfectos)
    { 
      scEstaUnidad.GanarEscondido(1);
      cooldownActual = 1; //Si mata, reduce el cooldown a 1 turno.

      if (NIVEL == 4) { scEstaUnidad.SumarValentia(2); }
    }
    fueElObjetivoAsesinado = null;
  }



 
       void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ASesinar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
    
  
    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(4,0);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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

   
 
}










