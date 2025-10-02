using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SiguesTu : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
    public override void  Awake()
    {
      nombre = "Sigues Tú";
      IDenClase = 8;
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
      cooldownMax = 3;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Caballero_SiguesTu");
      ActualizarDescripcion();
    }

   public override void ActualizarDescripcion()
   {
      if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Sigues Tú I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero señala a un enemigo, y lo amenaza de muerte.</i>\n";
        txtDescripcion += "<i>Marca: El Caballero obtiene +5 Ataque y +8 Daño contra el objetivo si utiliza Partir o Corte Vertical. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff: enemigo TS Mental vs 3. -2 Ataque por 2 Turnos.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";


         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Rango Crítico al Ataque</color>\n\n";
          }
          }
        }
   
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Sigues Tú II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero señala a un enemigo, y lo amenaza de muerte.</i>\n";
        txtDescripcion += "<i>Marca: El Caballero obtiene +5 Ataque, +2 Rango Crítico, +8 Daño contra el objetivo si utiliza Partir o Corte Vertical. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff: enemigo TS Mental vs 3. -2 Ataque por 2 Turnos.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Sigues Tú III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero señala a un enemigo, y lo amenaza de muerte.</i>\n";
        txtDescripcion += "<i>Marca: El Caballero obtiene +5 Ataque, +2 Rango Crítico, +10 Daño contra el objetivo si utiliza Partir o Corte Vertical. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff: enemigo TS Mental vs 3. -2 Ataque por 2 Turnos.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";

          if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: los enemigos no podrán salvarse del debuff</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +3 Turnos duración al debuff</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Sigues Tú IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero señala a un enemigo, y lo amenaza de muerte.</i>\n";
        txtDescripcion += "<i>Marca: El Caballero obtiene +5 Ataque, +2 Rango Crítico, +10 Daño contra el objetivo si utiliza Partir o Corte Vertical. Dura 3 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff: enemigo -2 Ataque por 2 Turnos.</i>\n\n";


        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>";
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Sigues Tú IV b</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero señala a un enemigo, y lo amenaza de muerte.</i>\n";
        txtDescripcion += "<i>Marca: El Caballero obtiene +5 Ataque, +2 Rango Crítico, +10 Daño contra el objetivo si utiliza Partir o Corte Vertical. Dura 6 Turnos.</i>\n";
        txtDescripcion += "<i>Debuff: enemigo TS Mental vs 3. -2 Ataque por 2 Turnos.</i>\n\n";


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

                BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre} en {objetivo.uNombre}");
                VFXAplicar(objetivo.gameObject);
                MarcaSiguesTu marca = new MarcaSiguesTu();
                marca.nombre = "Sigues Tu";
                marca.quienMarco = scEstaUnidad;
                marca.NIVEL = NIVEL;
                marca.duracion = 3;

                MarcaSiguesTu buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
                objetivo.Marcar(0);

                objetivo.GenerarTextoFlotante("Marcado", Color.yellow);

                int salvDC = 3;
                if(NIVEL == 4){salvDC += 100;} //Si nivel 4a, "no hay tirada de salvacion"
                int durDebuff = 2;
                if(NIVEL == 5){durDebuff += 2;} //Si nivel 4b, dura 2 turnos+ debuff

                if(objetivo.TiradaSalvacion(objetivo.mod_TSMental, salvDC))
                {
                    //BUFF ---- Así se aplica un buff/debuff
                    Buff debuff = new Buff();
                    debuff.buffNombre = "Amedrentado";
                    debuff.boolfDebufftBuff = false;
                    debuff.DuracionBuffRondas = durDebuff;
                    debuff.cantAtaque = -2;
                    debuff.AplicarBuff(objetivo);
                    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
                    ComponentCopier.CopyComponent(debuff, objetivo.gameObject);

                }
                
            }
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SiguesTu");

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
