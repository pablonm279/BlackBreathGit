using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;


public class Estados : MonoBehaviour
{
  
  public async static void  Efecto_Ardiendo(Unidad unidad)
  {
        unidad.RecibirDanio(1*unidad.estado_ardiendo, 4,false, null); 
       
        
        while(unidad.ObtenerAPActual() > 0 && unidad.estado_ardiendo > 0)
        {
            Aplicar_Ardiendo(unidad, -3);
           unidad.CambiarAPActual(-1); 
            
            // Retraso de 0.75 segundo
            await Task.Delay(750);
            
            unidad.GenerarTextoFlotante("Apagando!", Color.red);
            BattleManager.Instance.EscribirLog($"{unidad.uNombre} gasta 1 AP para apagar el fuego.");

           

            if(  unidad.estado_ardiendo < 0) {  unidad.estado_ardiendo = 0;}
            BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
        }
  }

  public static void  Efecto_Congelado(Unidad unidad)
  {
     int APValorSaca = (int)unidad.ObtenerAPActual()*2;

     unidad.CambiarAPActual(-(int)unidad.estado_congelado/2);
     unidad.estado_congelado -= APValorSaca;
     BattleManager.Instance.EscribirLog($"{unidad.uNombre} está congelado.");


     if(unidad.estado_congelado < 0)
     {
       unidad.estado_congelado = 0;

       unidad.GenerarTextoFlotante("Descongelado!", Color.red);
      BattleManager.Instance.EscribirLog($"{unidad.uNombre} se libró del congelamiento.");


     }
     BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }

  public static void  Efecto_Aturdido(Unidad unidad)
  {
     unidad.EstablecerAPActualA(0);
     unidad.estado_aturdido--;

     BattleManager.Instance.EscribirLog($"{unidad.uNombre} está aturdido.");

     BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }
   public static void  Efecto_RegeneraVida(Unidad unidad) //Regenera X Vida por turno
  {

    unidad.RecibirCuracion(unidad.estado_regeneravida, false);
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);

  }
   public static void  Efecto_RegeneraArmadura(Unidad unidad) //Regenera X Armadura por turno (si perdió armadura al recibir daño)
  {
  
    if(unidad.estado_armaduraModificador > 0)
    {
      unidad.estado_armaduraModificador--;
      BattleManager.Instance.EscribirLog($"{unidad.uNombre} regenera {unidad.estado_regeneraarmadura} Armadura.");
    }

    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }
  public static void  Efecto_Inmovil(Unidad unidad)
  {
     
     unidad.estado_inmovil--;

     BattleManager.Instance.EscribirLog($"{unidad.uNombre} está inmovilizado.");

     BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }

  public static void  Efecto_Sangrado(Unidad unidad)
  {
    
   
    unidad.mod_maxHP -=  unidad.estado_sangrado;
    if(unidad.HP_actual > unidad.mod_maxHP)  //Si al perder max HP, su vida actual es mayor a la amx, recibe daño verdadero para equiparar.
    {
       float cant = unidad.HP_actual - unidad.mod_maxHP ;
       unidad.RecibirDanio(cant, 10,false, null); 
    }
    unidad.estado_sangrado--;


    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }

  public static void  Efecto_Veneno(Unidad unidad)
  {
    unidad.RecibirDanio(1*unidad.estado_veneno, 10,false, null); 
    BattleManager.Instance.EscribirLog($"{unidad.uNombre} recibe {1*unidad.estado_veneno} daño veneno.");


   if(unidad.TiradaSalvacion(unidad.mod_TSFortaleza, unidad.estado_veneno)) //Cada turno se puede salvar del veneno, pero si peirde se suma 1 stack
   {
     unidad.estado_veneno = 0; unidad.GenerarTextoFlotante("<s>" + "Veneno" + "</s>", Color.green);
    BattleManager.Instance.EscribirLog($"{unidad.uNombre} resiste totalmente al veneno.");

   }
   else
   {
      Estados.Aplicar_Veneno(unidad, 1);
      BattleManager.Instance.EscribirLog($"{unidad.uNombre} falla su Tirada de salvación y el veneno empeora.");

   }
   
    
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }

  public static void  Aplicar_Ardiendo(Unidad unidad, int stacks)
  {
     if(unidad.estado_ardiendo > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_ardiendo += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+" arde", Color.red);

       BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
     } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }

  public static void  Aplicar_Veneno(Unidad unidad, int stacks)
  {
     if(unidad.estado_veneno > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_veneno += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+" veneno", Color.green);
       BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
     } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }

  public static void  Aplicar_Congelado(Unidad unidad, int stacks)
  {
     if(unidad.estado_congelado > -1) //-1 Es si es inmune al estado.
     {
      unidad.estado_congelado += stacks;
      unidad.GenerarTextoFlotante("+"+stacks+" frio", Color.cyan);

      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
     } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }

  public static void  Aplicar_Aturdido(Unidad unidad, int stacks)
  {
     if(unidad.estado_aturdido > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_aturdido < stacks)
      {
        unidad.estado_aturdido = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+" aturde", Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
     } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }
  public static void  Aplicar_Inmovil(Unidad unidad, int stacks)
  {
     if(unidad.estado_inmovil > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_inmovil < stacks)
      {
        unidad.estado_inmovil = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+" inmóvil", Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
     } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }


  public static void  Aplicar_Sangrado(Unidad unidad, int stacks)
  {
    if(unidad.estado_sangrado > -1) //-1 Es si es inmune al estado.
    {
       unidad.estado_sangrado += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+" sangrado", Color.red);
     

      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
    } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }

  public static void  Aplicar_Acido(Unidad unidad, int stacks)
  {
     if(unidad.estado_acido > -1) //-1 Es si es inmune al estado.
    {
     
       unidad.estado_acido += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+" acido", Color.green);
     

     BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
    } else{unidad.GenerarTextoFlotante("Inmune", Color.green);}
  }




}
