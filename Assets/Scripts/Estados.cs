using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;


public class Estados : MonoBehaviour
{
  private const int TurnosCondenadoParaEjecucion = 10;

  public async static void Efecto_Ardiendo(Unidad unidad)
  {
    unidad.RecibirDanio(2 * unidad.estado_ardiendo, 4, false, null, 400);

    Aplicar_Ardiendo(unidad, -1);
    if (unidad.estado_ardiendo < 0) { unidad.estado_ardiendo = 0; }
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    
       /* while(unidad.ObtenerAPActual() > 0 && unidad.estado_ardiendo > 0)
  {
      Aplicar_Ardiendo(unidad, -3);
     unidad.CambiarAPActual(-1);

      // Retraso de 1.15 segundos
      await BattleManager.DelayCombateAsync(1150);

      unidad.GenerarTextoFlotante(TRADU.i.Traducir("Apagando!"), Color.red);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" gasta 1 PA para apagar el fuego.")));



      if(  unidad.estado_ardiendo < 0) {  unidad.estado_ardiendo = 0;}
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }*/
  }

  public static void  Efecto_Congelado(Unidad unidad)
  {
     

     unidad.CambiarAPActual(-(int)unidad.estado_congelado);
     unidad.estado_congelado -= 1;
     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está congelado.")));


     if(unidad.estado_congelado < 0)
     {
       unidad.estado_congelado = 0;

       unidad.GenerarTextoFlotante(TRADU.i.Traducir("Descongelado!"), Color.red);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" se libró del congelamiento.")));


     }
     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Aturdido(Unidad unidad)
  {
     unidad.EstablecerAPActualA(0);
     unidad.estado_aturdido--;
    unidad.GenerarTextoFlotante(TRADU.i.Traducir("Aturdido!"), Color.yellow);
     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está aturdido.")));

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
   public static void  Efecto_RegeneraVida(Unidad unidad) //Regenera X Vida por turno
  {

    unidad.RecibirCuracion(unidad.estado_regeneravida, false);
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);

  }
  public static void Efecto_Condenado(Unidad unidad) //Cuando stacks llegue a 0 recibe 10% hpmax por turno activo (danio verdadero)
  {
    if (unidad == null || unidad.HP_actual <= 0)
    {
      return;
    }

    unidad.estado_CondenadoTurnosSeguidos++;

    if (unidad.estado_CondenadoTurnosSeguidos >= TurnosCondenadoParaEjecucion)
    {
      unidad.estado_Condenado = 0;
      float danioEjecucion = Mathf.Max(unidad.HP_actual + 1f, unidad.mod_maxHP * 10f);
      unidad.RecibirDanio(danioEjecucion, 10, false, null, 400);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" es ejecutado por la Condena.")));
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
      unidad.estado_CondenadoTurnosSeguidos = 0;
      return;
    }

    unidad.estado_Condenado--;
    if (unidad.estado_Condenado < 1)
    {
      float porcentajeAcumulado = 0.10f * unidad.estado_CondenadoTurnosSeguidos;
      unidad.RecibirDanio(unidad.mod_maxHP * porcentajeAcumulado, 10, false, null,400);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" es dañado por la Condena.")));
      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);

      unidad.estado_Condenado = 0;
      unidad.estado_CondenadoTurnosSeguidos = 0;
    }

  }

  
   public static void Efecto_RegeneraArmadura(Unidad unidad) //Regenera X Armadura por turno (si perdió armadura al recibir daño)
  {

    if (unidad.estado_armaduraModificador > 0)
    {
      int armaduraRecuperada = Mathf.Min(unidad.estado_armaduraModificador, unidad.estado_regeneraarmadura);
      if (armaduraRecuperada > 0)
      {
        unidad.estado_armaduraModificador -= armaduraRecuperada;
        BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre + TRADU.i.Traducir(" regenera ") + armaduraRecuperada + TRADU.i.Traducir(" Armadura.")));
      }
    }

    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
  public static void  Efecto_Inmovil(Unidad unidad)
  {
     
     unidad.estado_inmovil--;

     BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" está inmovilizado.")));

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Sangrado(Unidad unidad)
  {
    
   
    unidad.mod_maxHP -=  unidad.estado_sangrado;
    if(unidad.HP_actual > unidad.mod_maxHP)  //Si al perder max HP, su vida actual es mayor a la amx, recibe daño verdadero para equiparar.
    {
       float cant = unidad.HP_actual - unidad.mod_maxHP ;
       unidad.RecibirDanio(cant, 10,false, null,400); 
    }
    unidad.estado_sangrado--;


    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Efecto_Veneno(Unidad unidad)
  {
    unidad.RecibirDanio(1*unidad.estado_veneno, 10,false, null,400); 
    BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" recibe ") + (1*unidad.estado_veneno) + TRADU.i.Traducir(" daño veneno.")));


   bool noSeSalva = unidad.TiradaSalvacion(unidad.mod_TSFortaleza, 7+unidad.estado_veneno);
   if(!noSeSalva) //Cada turno se puede salvar del veneno; si falla, se suma 1 stack.
   {
     unidad.estado_veneno = 0; unidad.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Veneno") + "</s>", Color.green);
    BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" resiste totalmente al veneno.")));

   }
   else
   {
      Estados.Aplicar_Veneno(unidad, 1);
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(unidad.uNombre+TRADU.i.Traducir(" falla su Tirada de salvación y el veneno empeora.")));

   }
   
    
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  public static void  Aplicar_Ardiendo(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Ardiendo", origen))
     {
       return;
     }

     if(unidad.estado_ardiendo > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_ardiendo += stacks;

       if(stacks > 0)
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" arde"), Color.red);
      

       BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Veneno(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Veneno", origen))
     {
       return;
     }

     if(unidad.estado_veneno > -1) //-1 Es si es inmune al estado.
     {
       unidad.estado_veneno += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" veneno"), Color.green);
       BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Congelado(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Congelado", origen))
     {
       return;
     }

     if(unidad.estado_congelado > -1) //-1 Es si es inmune al estado.
     {
      unidad.estado_congelado += stacks;
      unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" frio"), Color.cyan);

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Aturdido(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Aturdido", origen))
     {
       return;
     }

     if(unidad.estado_aturdido > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_aturdido < stacks)
      {
        unidad.estado_aturdido = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" aturde"), Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }
  public static void  Aplicar_Inmovil(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Inmovil", origen))
     {
       return;
     }

     if(unidad.estado_inmovil > -1) //-1 Es si es inmune al estado.
     {
      if(unidad.estado_inmovil < stacks)
      {
        unidad.estado_inmovil = stacks;
        unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" inmóvil"), Color.yellow);
      }

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
     } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }


  public static void  Aplicar_Sangrado(Unidad unidad, int stacks, Unidad origen = null)
  {
    if (stacks > 0 && unidad.IntentarResistenciaEstado("Sangrado", origen))
    {
      return;
    }

    if(unidad.estado_sangrado > -1) //-1 Es si es inmune al estado.
    {
       unidad.estado_sangrado += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" sangrado"), Color.red);
     

      BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }

  public static void  Aplicar_Acido(Unidad unidad, int stacks, Unidad origen = null)
  {
     if (stacks > 0 && unidad.IntentarResistenciaEstado("Acido", origen))
     {
       return;
     }

     if(unidad.estado_acido > -1) //-1 Es si es inmune al estado.
    {
     
       unidad.estado_acido += stacks;
       unidad.GenerarTextoFlotante("+"+stacks+TRADU.i.Traducir(" acido"), Color.green);

     BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
    } else{unidad.GenerarTextoFlotante(TRADU.i.Traducir("Inmune"), Color.green);}
  }




  public static void Aplicar_MovimientoAbaratado(Unidad unidad, int stacks, Unidad origen = null, bool mostrarTextoFlotante = true)
  {
    if (unidad == null || stacks == 0)
    {
      return;
    }

    unidad.estado_MovimientoAbaratado += stacks;
    if (unidad.estado_MovimientoAbaratado < 0)
    {
      unidad.estado_MovimientoAbaratado = 0;
    }

    if (stacks > 0 && mostrarTextoFlotante)
    {
      unidad.GenerarTextoFlotante("+" + stacks + " " + TRADU.i.Traducir("impulso"), new Color(0.2f, 0.95f, 1f));
    }

    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }
}




