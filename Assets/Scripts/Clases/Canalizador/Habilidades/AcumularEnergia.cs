using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class AcumularEnergia : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acumular Energía";
      IDenClase = 0; // Intrínseca
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumularEnergia");

       
      ActualizarDescripcion();
    
    }

        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

      string titulo = esIngles ? "Gather Energy" : "Acumular Energia";
      string subtitulo = esIngles
        ? "The Channeler enters concentration to increase their Energy tier at the start of the next turn."
        : "El Canalizador entra en concentracion para aumentar su Nivel de Energia al inicio de su siguiente turno.";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Self\n";
        cuerpo += "<b>Target:</b> Self\n";
        cuerpo += "<b>Effect on cast:</b> Applies <b>Gathering</b> buff (2 rounds)\n";
        cuerpo += "<b>If concentration is maintained:</b> +1 Energy Tier on next turn\n";
        cuerpo += "<b>Energy I:</b> +10% Damage, +1 Critical Die, -1 Arcane Resistance\n";
        cuerpo += "<b>Energy II:</b> +15% Damage, +1 Max AP, -5 Arcane Resistance\n";
        cuerpo += "<b>Energy III:</b> +15% Damage, +1 Max AP, +1 Critical Die, -8 Arcane Resistance";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Propia\n";
        cuerpo += "<b>Objetivo:</b> Propio usuario\n";
        cuerpo += "<b>Efecto al activar:</b> Aplica buff <b>Acumulando</b> (2 rondas)\n";
        cuerpo += "<b>Si mantiene la concentracion:</b> +1 Nivel de Energia al siguiente turno\n";
        cuerpo += "<b>Energia I:</b> +10% Danio, +1 Dado Critico, -1 Resistencia Arcana\n";
        cuerpo += "<b>Energia II:</b> +15% Danio, +1 AP Maximo, -5 Resistencia Arcana\n";
        cuerpo += "<b>Energia III:</b> +15% Danio, +1 AP Maximo, +1 Dado Critico, -8 Resistencia Arcana";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(titulo, subtitulo, cuerpo, costos, "#5dade2");
    }

    public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
      await  base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

     ClaseCanalizador scClaseCana = (ClaseCanalizador)scEstaUnidad;
     int NivelAcumulacionProtegida = scClaseCana.PASIVA_AcumulacionProtegida;
    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acumulando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
      
       if (NivelAcumulacionProtegida > 0)
       {
      
        int factorBarrera = (int)(1 + scEstaUnidad.mod_CarPoder + 3 * scClaseCana.ObtenerEnergia());
        buff.cantBarrera += factorBarrera;
        if (NivelAcumulacionProtegida > 1) { buff.cantBarrera += 2; }
        if (NivelAcumulacionProtegida == 4) { buff.cantBarrera += 4; }
        if (NivelAcumulacionProtegida == 5) { buff.cantAPMax += 1; }


        buff.cantTsMental += 1;
        if (NivelAcumulacionProtegida > 2) {  buff.cantTsMental += 1; }

       }
       
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);
        // Mantener pose de habilidad mientras dura "Acumulando"
        var poseCtrl = objetivo.GetComponent<UnidadPoseController>();
        if (poseCtrl != null)
        {
            poseCtrl.EnterSkillPoseHold();
        }







      //Usarla termina el turno
      BattleManager.Instance.TerminarTurno();

       
      }
    }
    
         void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AcumularEnergia");

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
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
        if(c.Presente == null)
        {
            continue;
        }
        
       
        if(c.Presente.GetComponent<Unidad>() == null)
        {
            continue;
        }
           
        if(c.Presente.GetComponent<Unidad>() != null)
        {
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }
 
}





