using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class LlamaDivina : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private int hAlcance = 5;
    private int hAncho = 1; //1 - Adyacentes

    private int danioFijo;
     public override void  Awake()
    {
      nombre = "Llama Divina";
      costoAP = 3; 
      costoPM = 0;
      IDenClase = 7;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 3; 
      bAfectaObstaculos = true;

      bonusAtaque = 0; //0
      XdDanio = 3;
      daniodX = 6; 
      danioFijo = 4;
      if(NIVEL > 2){danioFijo += 3;}
      tipoDanio = 11; //Divino

      criticoRangoHab = 0;

      requiereRecurso = 0; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.

      
        tipoPorcentaje = 3;



     imHab = Resources.Load<Sprite>("imHab/Purificadora_LlamaDivina");
      
  


        
    }

   
          public override void ActualizarDescripcion()
     {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int ataqueActual = statsUI.Ataque;
      int criticoMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
      int dcBase = NIVEL > 1 ? 9 : 8;
      int quemadura = NIVEL == 5 ? 5 : 3;
      int danioFijoActual = NIVEL > 2 ? 7 : 4;
      bool ganaFervorAlMatar = NIVEL == 4;

      string tituloEs = "Llama Divina I";
      string tituloEn = "Divine Flame I";
      if (NIVEL == 2) { tituloEs = "Llama Divina II"; tituloEn = "Divine Flame II"; }
      if (NIVEL == 3) { tituloEs = "Llama Divina III"; tituloEn = "Divine Flame III"; }
      if (NIVEL == 4) { tituloEs = "Llama Divina IV a"; tituloEn = "Divine Flame IV a"; }
      if (NIVEL == 5) { tituloEs = "Llama Divina IV b"; tituloEn = "Divine Flame IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcBase, "Poder", "Power", poderActual);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcBase, "Poder", "Power", poderActual);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (5 range)\n";
        cuerpo += "<b>Target:</b> 1 unit in range\n";
        cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Power ({poderActual})</colo r>  vs Defense. Fumble: 1. Crit: {criticoMin}-20\n";
        cuerpo += $"<b>Damage:</b> 3d6 + {danioFijoActual} + <color=#ea0606>Power ({poderActual})</color> | <b>Type:</b> Divine\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += $"<b>On failed save:</b> Burning {quemadura}. Undead and Ethereal are instantly killed";
        if (ganaFervorAlMatar)
        {
          cuerpo += "\n<b>IV a Extra:</b> On kill, gain +1 Fervor";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad en rango\n";
        cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Poder ({poderActual})</color> + Ataque ({ataqueActual}) vs Defensa. Pifia: 1. Critico: {criticoMin}-20\n";
        cuerpo += $"<b>Danio:</b> 3d6 + {danioFijoActual} + <color=#ea0606>Poder ({poderActual})</color> | <b>Tipo:</b> Divino\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Si falla TS:</b> Ardiendo {quemadura}. Nomuerto y Etereo mueren instantaneamente";
        if (ganaFervorAlMatar)
        {
          cuerpo += "\n<b>Extra IV a:</b> Al matar, gana +1 Fervor";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "A divine projectile that tests endurance and burns impure targets."
          : "Un proyectil divino que pone a prueba la resistencia y quema objetivos impuros.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = EsEscenaCampaña()
        && CampaignManager.Instance != null
        && CampaignManager.Instance.scMenuPersonajes != null
        && CampaignManager.Instance.scMenuPersonajes.pSel != null
        && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +3 base damage.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Fervor on kill) or Option B (+2 Burning).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +3 de danio base.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 Fervor al matar) u Opcion B (+2 Ardiendo).</color>"; }
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
       float defensaObjetivo = objetivo.ObtenerdefensaActual();float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       
      
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);
         

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);

       
        objVerMurio = objetivo;

        if(NIVEL == 4)
        {Invoke("ChequearMurio", 1.5f);}



       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    Unidad objVerMurio;
    void ChequearMurio()
    {
        if(objVerMurio == null)
        {
          scEstaUnidad.gameObject.GetComponent<ClasePurificadora>().CambiarFervor(1);
          BattleManager.Instance.EscribirLog(
            TRADU.i.Traducir(scEstaUnidad.uNombre) + " " +
            TRADU.i.Traducir("gana 1 Fervor por matar con ") +
            TRADU.i.Traducir(nombre) + ".");
        }
        else if(objVerMurio.HP_actual < 1)
        {
          scEstaUnidad.gameObject.GetComponent<ClasePurificadora>().CambiarFervor(1);
          BattleManager.Instance.EscribirLog(
            TRADU.i.Traducir(scEstaUnidad.uNombre) + " " +
            TRADU.i.Traducir("gana 1 Fervor por matar con ") +
            TRADU.i.Traducir(nombre) + ".");
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
        await Task.Delay(200);

        GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.LlamaDivina;
        if (flechaPrefab == null)
        {
            await Task.Delay(200);
            return;
        }

        GameObject proyectil = Instantiate(flechaPrefab);
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
            flight.Configure(transform, destino, 0.22f, 5.7f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await Task.Delay(200);
        }
    }
    void EfectoAdicional(Unidad objetivo)
    { 

      int DC = (int)(8+scEstaUnidad.mod_CarPoder);

      if(NIVEL > 1){DC++;}
      if(objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, DC))
     {
       if(objetivo.TieneTag("Nomuerto") || objetivo.TieneTag("Etereo")) // Si los nomuertos no se salvan los mata.
       {
          if(objetivo.HP_actual > 0)
          {
           objetivo.RecibirDanio(objetivo.mod_maxHP+1,10, false, scEstaUnidad); //Muerte instantanea
          }
       }
       else
       {
         objetivo.estado_ardiendo += 3;
         if(NIVEL == 5)
         {
           objetivo.estado_ardiendo += 2 ;
         }
       }


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

   
}




