using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;

public class PosturaDefensiva : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
     public override void  Awake()
    {
      nombre = "Postura Defensiva";
      IDenClase = 7;
      costoAP = 2; //Termina turno
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

      imHab = Resources.Load<Sprite>("imHab/Caballero_PosturaDefensiva");
      ActualizarDescripcion();
    }

   
    public override void ActualizarDescripcion()
    {
        if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Postura Defensiva I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero se posiciona en una postura especial, listo para recibir ataques y contraatacar.</i>\n";
        txtDescripcion += "<i>Reacción: Aumenta 1 la Defensa, y contraataca al enemigo si falla un ataque melee contra el Caballero. Si es dañado, cancela la reacción.</i>\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE -</b> -Contraataca con: Corte Vertical -1 Ataque. 1 vez.</color>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} - termina Turno \n- Costo Val: {costoPM} \n</color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Defensa</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Postura Defensiva II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero se posiciona en una postura especial, listo para recibir ataques y contraatacar.</i>\n";
        txtDescripcion += "<i>Reacción: Aumenta 2 la Defensa, y contraataca al enemigo si falla un ataque melee contra el Caballero. Si es dañado, cancela la reacción.</i>\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE -</b> -Contraataca con: Corte Vertical -1 Ataque. 1 vez.</color>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} - termina Turno \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque a Reacción</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Postura Defensiva III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero se posiciona en una postura especial, listo para recibir ataques y contraatacar.</i>\n";
        txtDescripcion += "<i>Reacción: Aumenta 1 la Defensa, y contraataca al enemigo si falla un ataque melee contra el Caballero. Si es dañado, cancela la reacción.</i>\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE -</b> -Contraataca con: Corte Vertical. 1 vez. </color>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} - termina Turno \n- Costo Val: {costoPM} \n</color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Ya no se cancela al recibir un golpe</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 contraataque</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Postura Defensiva IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero se posiciona en una postura especial, listo para recibir ataques y contraatacar.</i>\n";
        txtDescripcion += "<i>Reacción: Aumenta 1 la Defensa, y contraataca al enemigo si falla un ataque melee contra el Caballero.</i>\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE -</b> -Contraataca con: Corte Vertical. 1 vez. </color>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} - termina Turno \n- Costo Val: {costoPM} \n</color>";
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Postura Defensiva III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Caballero se posiciona en una postura especial, listo para recibir ataques y contraatacar.</i>\n";
        txtDescripcion += "<i>Reacción: Aumenta 1 la Defensa, y contraataca al enemigo si falla un ataque melee contra el Caballero. Si es dañado, cancela la reacción.</i>\n";
        txtDescripcion += $"<color=#c8c8c8><b>MELEE -</b> -Contraataca con: Corte Vertical. 2 veces. </color>\n\n";

        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} - termina Turno \n- Costo Val: {costoPM} \n</color>";
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
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre}");

      VFXAplicar(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Postura Defensiva";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantDefensa += 1;
       if(NIVEL > 1){ buff.cantDefensa += 1;}
       if(NIVEL > 2){ buff.cantAtaque += 1;}
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacción 
       ReaccionPosturaDefensiva reaccion = new ReaccionPosturaDefensiva();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Postura Defensiva";
       ReaccionPosturaDefensiva reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       //Usarla termina el turno
       BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PosturaDefensiva");

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
