using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class MarcarPresa : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
     public override void  Awake()
    {
      nombre = "Marcar Presa";
      IDenClase = 4;
      costoAP = 1;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_MarcarPresa");
      ActualizarDescripcion();
    }

   public override void ActualizarDescripcion()
   {
       if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Marcar Presa I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador marca un objetivo como su presa, obteniendo bonificaciones al atacarlo.</i>\n";
        txtDescripcion += "<i>Marca:+2 Ataque +1 Rango Crítico, +15% daño crítico. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff:-2 Ataque contra otros objetivos que no sean la presa.</i>\n\n";
        txtDescripcion += "<i>Al matar la presa: +1 AP +2 TS Mental por 3 Turnos. +1 Valentía.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";


         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% daño crítico</color>\n\n";
          }
          }
        }
   
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Marcar Presa II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador marca un objetivo como su presa, obteniendo bonificaciones al atacarlo.</i>\n";
        txtDescripcion += "<i>Marca:+2 Ataque +1 Rango Crítico, +20% daño crítico. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff:-2 Ataque contra otros objetivos que no sean la presa.</i>\n\n";
        txtDescripcion += "<i>Al matar la presa: +1 AP +2 TS Mental por 3 Turnos. +1 Valentía.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Rango Crítico</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Marcar Presa III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador marca un objetivo como su presa, obteniendo bonificaciones al atacarlo.</i>\n";
        txtDescripcion += "<i>Marca:+2 Ataque +2 Rango Crítico, +20% daño crítico. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff:-2 Ataque contra otros objetivos que no sean la presa.</i>\n\n";
        txtDescripcion += "<i>Al matar la presa: +1 AP +2 TS Mental por 3 Turnos. +1 Valentía.</i>\n\n";



        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";

          if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: ya no disminuye Ataque contra otros objetivos.</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: al matar a la presa obtiene +2 valentía extra.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Marcar Presa IVa</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador marca un objetivo como su presa, obteniendo bonificaciones al atacarlo.</i>\n";
        txtDescripcion += "<i>Marca:+2 Ataque +2 Rango Crítico, +20% daño crítico. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Al matar la presa: +1 AP +2 TS Mental por 3 Turnos. +1 Valentía.</i>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Marcar Presa IVb</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador marca un objetivo como su presa, obteniendo bonificaciones al atacarlo.</i>\n";
        txtDescripcion += "<i>Marca:+2 Ataque +2 Rango Crítico, +20% daño crítico. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff:-2 Ataque contra otros objetivos que no sean la presa.</i>\n\n";
        txtDescripcion += "<i>Al matar la presa: +2 AP +3 TS Mental por 3 Turnos. +2 Valentía.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>";
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

      if(obj is Unidad) //Acá van los efectos a Unidades.
      {
          
          Unidad objetivo = (Unidad)obj;
          VFXAplicar(objetivo.gameObject);

          BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre} en {objetivo.uNombre}");

          MarcaMarcarPresa marca = new MarcaMarcarPresa();
          marca.nombre = "Presa Marcada";
          marca.quienMarco = scEstaUnidad;
          marca.NIVEL = NIVEL;
          marca.duracion = 3;

          MarcaMarcarPresa buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
          objetivo.Marcar(0);

          objetivo.GenerarTextoFlotante("Marcado", Color.yellow);

                        
      }
      
      cooldownActual = cooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP); 

      if(NIVEL != 4) // a Nivel IVa, no recibe el debuff
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Marcando Presa";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 2;
        buff.AplicarBuff(scEstaUnidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
      }
    }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_MarcarPresa");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
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
     
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
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
             c.ActivarCapaColorRojo();
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
