using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class GritoMotivador : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Grito Motivador";
      IDenClase = 2;
      costoAP = 2;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 3;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Caballero_GritoMotivador");

       
      ActualizarDescripcion();
    
    }

    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Grito Motivador I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero anima a sus aliados a luchar con un grito motivador.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff a todos los Aliados:</b> +10% daño por 3 Turnos. +1 Valentía </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% Daño</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Grito Motivador II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero anima a sus aliados a luchar con un grito motivador.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff a todos los Aliados:</b> +15% daño por 3 Turnos. +1 Valentía </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
         {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Otorga +1 Valentía</color>\n\n";
          }
         }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Grito Motivador III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero anima a sus aliados a luchar con un grito motivador.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff a todos los Aliados:</b> +15% daño por 3 Turnos. +2 Valentía </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";
        
         if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
           {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +2 Valentía al Caballero</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Debuff Enemigos: -10% Daño</color>\n";
          }
           }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Grito Motivador IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero anima a sus aliados a luchar con un grito motivador.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff a todos los Aliados:</b> +15% daño por 3 Turnos. +2 Valentía </color>\n";
        txtDescripcion += $"+2 Valentía al Caballero. \n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>";   
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Grito Motivador IV b</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero anima a sus aliados a luchar con un grito motivador.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff a todos los Aliados:</b> +15% daño por 3 Turnos. +2 Valentía </color>\n";
        txtDescripcion += $"-10% daño a enemigos que no superen una TS Mental por 2 Turnos. \n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>";  
       }


    }


  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
  {
    // El log de uso ahora está centralizado en Habilidad.Resolver
    base.Resolver(Objetivos);
    VFXAplicarPropio(Usuario.gameObject);
  }


    void VFXAplicarPropio(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorOrigen");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---

  }
  void VFXAplicarAliado(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoAliado");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---
  }
   void VFXAplicarEnemigo(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoEnemigo");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200; 
            //---
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
      if(objetivo.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado) //Chequea si son aliados para buffearlos o enemigos para debuffearlos (si nv5)
      { 
      
       if(NIVEL > 2)
       {
        objetivo.SumarValentia(1);
       }
       if(NIVEL == 4)
       {
        scEstaUnidad.SumarValentia(2);
       }

        if (objetivo != scEstaUnidad)
        {
            VFXAplicarAliado(objetivo.gameObject);
        }
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Motivador";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDanioPorcentaje += 10;
       if( NIVEL > 1)
       {
        buff.cantDanioPorcentaje = +5;
       }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        objetivo.Marcar(0);
     }
     else if(NIVEL == 5)
     {
        VFXAplicarEnemigo(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Desmotivador";
       buff.boolfDebufftBuff = false;
       buff.DuracionBuffRondas = 1;
       buff.cantDanioPorcentaje -= 10;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       
       objetivo.Marcar(0);
     }
    }
    }
    
 

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
      if(NIVEL == 5)
      {
         List<Casilla> lCasillop = Origen.ObtenerCasillasLadoOpuesto();
        lCasillasafectadas.AddRange(lCasillop);

      }
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
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
             c.ActivarCapaColorAzul();
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

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}
