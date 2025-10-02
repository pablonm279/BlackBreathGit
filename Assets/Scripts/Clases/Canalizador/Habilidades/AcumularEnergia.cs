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
      
       txtDescripcion = "<color=#5dade2><b>Acumular Energía</b></color>\n\n";
       txtDescripcion += "<i>El Canalizador concentra su fuerza interior para potenciar sus poderes. " +
                  "Cada vez que completa la concentración sin interrupciones, gana un nivel de Energía al inicio de su siguiente turno.</i>\n\n";
       txtDescripcion += "<color=#c8c8c8><b>Niveles de Energía:</b>\n" +
                  "• <b>I</b>: +10% Daño, +1 Dado crítico, -1 Res. Arcano\n" +
                  "• <b>II</b>: +15% Daño, +1 AP, -5 Res. Arcano\n" +
                  "• <b>III</b>: +15% Daño, +1 AP, +1 Dado crítico, -8 Res. Arcano</color>\n\n";
       txtDescripcion += "<color=#c8c8c8><b>Costo:</b> 3 AP • Termina turno</color>\n\n";
       txtDescripcion += "<color=#c8c8c8><b>Mecánica:</b> Al activar “Acumular Energía”, el Canalizador recibe el Buff “<b>Acumulando Energía</b>”. " +
                  "Si recibe daño mientras mantiene este Buff, debe superar una Tirada de Salvación Mental o lo pierde. " +
                  "Si inicia su siguiente turno con el Buff activo, sube un Nivel de Energía.</color>\n";  
      
    }

   
    public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
        base.Resolver(Objetivos);
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
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

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
