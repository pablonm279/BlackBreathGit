using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class Acechar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acechar";
      costoAP = 2;
      costoPM = 0;
      IDenClase = 7;

      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_Acechar");

       
      ActualizarDescripcion();
    
    }

    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Acechar I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador se esconde y gana un buff a su próximo ataque exitoso.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff:</b> +15% daño, +2 Ataque. Por 1 ataque exitoso o 2 turnos. Escondido. </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Acechar II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador se esconde y gana un buff a sus ataques.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff:</b> +15% daño, +3 Ataque. Por 1 ataque exitoso o 2 turnos. Escondido. </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
         {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Otorga +1 Rango Crítico</color>\n\n";
          }
         }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Acechar III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador se esconde y gana un buff a sus ataques.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff:</b> +15% daño, +3 Ataque, +1 Rango Crítico. Por 1 ataque exitoso o 2 turnos. Escondido. </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";
        
         if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
           {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +2 Rango Crítico</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: El buff no se elimina al dañar a un enemigo.</color>\n";
          }
           }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Acechar IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador se esconde y gana un buff a sus ataques.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff:</b> +15% daño, +3 Ataque, +3 Rango Crítico. Por 1 ataque exitoso o 2 turnos. Escondido. </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";  
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Acechar IV b</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador se esconde y gana un buff a sus ataques.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff:</b> +15% daño, +3 Ataque, +1 Rango Crítico. Por 2 turnos. Escondido. </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";    
       }


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

    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre}");
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acechando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque += 2;
        buff.cantDanioPorcentaje += 15;
        if(NIVEL > 1){ buff.cantAtaque += 1;}
        if(NIVEL > 2){ buff.cantCritDado += 1;}
        if(NIVEL == 4){ buff.cantCritDado += 2;}
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);

        
        //Usarla termina el turno
        BattleManager.Instance.TerminarTurno();

        //Agrega acechar
        objetivo.GanarEscondido(1);
      }
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Acechar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder = 200;  

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
