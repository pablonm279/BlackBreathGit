using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ClaseCaballero : Unidad
{
   
   public int PASIVA_Acorazado; //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Determinacion;  //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Implacable;  //0 no tiene, 1 nv 1, 2 nv 2, 3nv 3,       4 nv 4a ° 5 nv 4b
   public int PASIVA_Implacable_CARGAS; 
   




  public override void SumarValentia(int cant)
  {
  

   base.SumarValentia(cant); //hace todo lo mismo que el metodo original, y agrega lo de abajo al final

   if(ValentiaP_actual < 0) //Pasiva - "Coraje Inquebrantable: Sus puntos de valentía no pueden ser negativos."
   {
        ValentiaP_actual = 0;
   }

   //PASIVA_Implacable-------------------------------------
   if(PASIVA_Implacable > 0 && ValentiaP_actual == mod_maxValentiaP && PASIVA_Implacable_CARGAS > 0) //Pasiva -Aumenta stats si Valentía al máximo, por 2 Turnos, 1 vez. 
   {
       bool yaTieneElBuff = false;
       Buff[] buffs = gameObject.GetComponents<Buff>();
       foreach(Buff b in buffs)
       {
        if(b.buffNombre == "Implacable"){  yaTieneElBuff = true;}
   
       }

       if(!yaTieneElBuff)
       {
        PASIVA_Implacable_CARGAS--;
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Implacable";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtFue += 3;
        buff.cantAPMax += 2;
        buff.cantDanioPorcentaje += 20;
        buff.cantTsMental += 3;
        if(PASIVA_Implacable > 1){ buff.cantAtFue += 1;  buff.cantTsFortaleza += 2;}
        if(PASIVA_Implacable > 2){ buff.cantCritDado += 1; }
        if(PASIVA_Implacable == 5){ buff.DuracionBuffRondas += 1; }
        buff.AplicarBuff(this);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject);
       }
      
   }
   
  }

  public override void ReducirArmaduraPorGolpe(float danioFinal)
  {

   switch(PASIVA_Acorazado)
   {
      case 0: if(danioFinal > 0){estado_armaduraModificador++;} break; //No tiene la pasiva
      //-----------------------------------------------------//
      case 1: if(danioFinal > 3){estado_armaduraModificador++;} break; //Nv 1  4 daño o mas para reducir armadura
      case 2: if(danioFinal > 4){estado_armaduraModificador++;} break; //Nv 2  5 daño o mas para reducir armadura
      case 3: if(danioFinal > 5){estado_armaduraModificador++;} break; //Nv 3  6 daño o mas para reducir armadura
      case 4: if(danioFinal > 7){estado_armaduraModificador++;} break; //Nv 4a  8 daño o mas para reducir armadura
      case 5: if(danioFinal > 5 && (ObtenerArmaduraActual() > mod_Armadura/2)) //Nv 4b 6 daño o mas, y no se puede reducir a menos de la mitad de la armadura inicial.
      {estado_armaduraModificador++;} break;
   }


  }

  public override void ActualizarClaseComienzoTurno()
  {
    ChequearBuffPASIVADeterminacion();




  }

    void ChequearBuffPASIVADeterminacion()
    {
      if(PASIVA_Determinacion > 0 && ValentiaP_actual > 0)
      {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff buffDet = new Buff();
       buffDet.buffNombre = "Determinación "+ValentiaP_actual;
       buffDet.boolfDebufftBuff = true;
       buffDet.DuracionBuffRondas = 1; 
       buffDet.cantDanioPorcentaje = 5*ValentiaP_actual;
       if(PASIVA_Determinacion == 5) //PASIVA_Determinacion Nv 4b +2% daño por Valentía
       {
         buffDet.cantDanioPorcentaje += 2*ValentiaP_actual;
       }
       buffDet.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
     
       Buff buffComponent = ComponentCopier.CopyComponent(buffDet, gameObject);
       

      }
    }

    public override void AplicarMotivado()
    {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Motivado";
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantTsMental += 2;
       if(PASIVA_Determinacion > 1) //Determinacion nv 2 o +
       {
         motivado.cantTsMental += 1;
         motivado.cantTsFortaleza += 1;
         motivado.cantTsReflejos += 1;
       }
       motivado.cantTsFortaleza += 1;
       motivado.cantTsReflejos += 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
    }
    public override void AplicarEuforico()
   {
   /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Euforia";
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantAtFue += 1;
       motivado.cantAtPod += 1;
       motivado.cantAtAgi += 1;
      if(PASIVA_Determinacion > 2) //Determinacion nv 3 o +
       {
         motivado.cantAtaque += 1;
       }
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
   }
    

    public override void ComienzoBatallaClase()
    {
       base.ComienzoBatallaClase();


       //PASIVA_Determinacion Nv 4a
       if(PASIVA_Determinacion == 4)
       {
         ValentiaP_actual = 5;
         AplicarMotivado();
         AplicarEuforico(); //Se llaman desde aca porque si no no se agregan, al no detectar el valor anterior por que es turno 1

       }

       //PASIVA_Implacable 
       if(PASIVA_Implacable > 0 && PASIVA_Implacable != 4)
       {
         PASIVA_Implacable_CARGAS = 1;
         mod_maxValentiaP += 2;
       }
       else if(PASIVA_Implacable > 0 && PASIVA_Implacable == 4)
       {
         PASIVA_Implacable_CARGAS = 2;
         mod_maxValentiaP += 1;
       }

    }


    public bool tieneCorazaDeLlamas; //esto se pone TRUE al inicio del combate en AdministradorEscenas "AplicarEfectosItemsEspecificos"

  public async override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayefectos = 0)
  {
    base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayefectos);
    //------------------------------------------------------------

    //Reaccion Postura Defensiva
    if (gameObject != null)
    {
      if (gameObject.GetComponent<ReaccionPosturaDefensiva>() != null)
      {
        ReaccionPosturaDefensiva reacc = gameObject.GetComponent<ReaccionPosturaDefensiva>();
        if (reacc.NIVEL != 4) //Si tiene la reaccion activa y no es nivel 4a, la remueve
        {
          Destroy(reacc);
          await gameObject.GetComponent<Unidad>().GenerarTextoFlotante("<s>" + "Postura Defensiva" + "</s>", Color.blue);


        }

      }
    }

    //Armadura de Coraza de Llamas +1
    if (tieneCorazaDeLlamas && uCausante.CasillaPosicion.posX == 3)
    {
      if (uCausante != null)
      {
        if (uCausante.TiradaSalvacion(uCausante.mod_TSReflejos, 10))
        {
          uCausante.estado_ardiendo += 1; //Agrega el estado ardiente
          uCausante.GenerarTextoFlotante("Ardiendo +1", Color.red);

        }


      }




    }
  
    }

}
