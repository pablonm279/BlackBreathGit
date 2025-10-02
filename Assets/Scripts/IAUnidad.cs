using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Serialization;
using System;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;

public class IAUnidad : MonoBehaviour
{
   Unidad scUnidad;
  

   public bool esRango; //Tiende a mantenerse atras
   public bool bPuedeVerEscondidos = false; //Si puede ver unidades escondidas, si no, no las ataca
   public int costoMovimientoAP = 1; //Cuanto gasta de AP al moverse 
   
   public int tendenciaMovY;
   public int tendenciaMovX;

   public List<IAHabilidad> HabPosibles = new List<IAHabilidad>();
   
   public void Awake()
   {
      scUnidad = gameObject.GetComponent<Unidad>();
   
   }
   public async void RealizarTurnoIA()
   {
   
    int contadorIteraciones = 10; // Ajusta este valor según sea necesario

    while(scUnidad.ObtenerAPActual() > 0 && contadorIteraciones > 0)
    {  
      contadorIteraciones--;
      if(contadorIteraciones == 0){break;}

     if(HayHabilidadesPosibles().Count == 0) //No hay habilidades posibles
     {
       if(scUnidad.esInmobil)
       {
         scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual()); //Si no tiene habilidades para hacer y es inmobil, gasta un AP
         await Task.Delay(500);
        // Invoke("TerminarTurnoDesdeWhile", 1.0f);
         await Task.Delay(500); //Si no tiene habilidades para hacer y es inmobil termina turno
         BattleManager.Instance.TerminarTurno();
         break;
       }
     
        // Llamar al método para moverse a la casilla objetivo de forma asíncrona
       LadoManager lado;
       if(scUnidad.CasillaPosicion.lado == 1)
       {
          lado = BattleManager.Instance.ladoA;
       }else { lado = BattleManager.Instance.ladoB;} 

      int casX = 0;
      int casY = 0;

     
      //Y
      if(tendenciaMovY < 0)
      {
        casY = scUnidad.CasillaPosicion.posY-1; 
        tendenciaMovY++;
      }
      else if(tendenciaMovY > 0)
      {
        casY = scUnidad.CasillaPosicion.posY+1;
        tendenciaMovY--;
      }
      else if(tendenciaMovY == 0)
      {
         casY = scUnidad.CasillaPosicion.posY;
      }
      
      //X
      if(tendenciaMovX > 0 && scUnidad.CasillaPosicion.posX < 3)
      {
         if (!esRango || scUnidad.CasillaPosicion.posX == 1) //los de rango se mueven solo si estan en columna 1, nunca van a ir al frente
        {
        casX = scUnidad.CasillaPosicion.posX + 1;
        tendenciaMovX--;
        }
      }
      else{casX = scUnidad.CasillaPosicion.posX; tendenciaMovX = 0;}
      
      if(casX > 3){casX = 3;}
      if(casX < 1){casX = 1;}
      if(casY > 5){casY = 5;}
      if(casY < 1){casY = 1;}

   Casilla destPosible = lado.ObtenerCasillaPorIndex(casX,casY);
   if(destPosible != null)
    {
      
      if(destPosible.Presente == null)
      {
        await MoverACasilla(destPosible);
      }
      else
      {
         await ChequearCasillasAlrededorParaMover(destPosible); //Medio aleatorio que sea, siempre priorizando "adelante"
         //Si no se puede mover porque esta ocupada, que haga otra cosa. VER
         scUnidad.CambiarAPActual(costoMovimientoAP); //por ahora esto para que no quede en loop
        // BattleManager.Instance.TerminarTurno();
         await Task.Delay(600);
         Invoke("TerminarTurnoDesdeWhile", 1.5f);
         await Task.Delay(600);
         BattleManager.Instance.TerminarTurno();

         break;

      }
     
    }
     }
     else if(scUnidad.HP_actual > 0) //Hay Habilidades posibles y no murio por algun efecto
     {
//       print("HAY hab cant: "+HayHabilidadesPosibles().Count);
        await Task.Delay(1600);//Intervalo entre acciones
        await ElegirQueHabilidadPosiblesHacer();

     }

    
           
    }
   
    if(scUnidad.ObtenerAPActual() < 1 && BattleManager.Instance.unidadActiva == gameObject.GetComponent<Unidad>())
    { await Task.Delay(1500); TerminarTurnoDesdeWhile();}
   
   }
   
   async Task ChequearCasillasAlrededorParaMover(Casilla casilla)
   {
       LadoManager lado;
       if(scUnidad.CasillaPosicion.lado == 1)
       {
          lado = BattleManager.Instance.ladoA;
       }else { lado = BattleManager.Instance.ladoB;} 

       Casilla aMover = null;
       bool yaseAsigno = false;
       
      if((!esRango)&&(lado.ObtenerCasillaPorIndex(casilla.posX+1, casilla.posY))!= null) //Prueba primero con la casilla adelante de la que no se pudo
      {
         if(lado.ObtenerCasillaPorIndex(casilla.posX+1, casilla.posY).Presente == null)
         {
           aMover = lado.ObtenerCasillaPorIndex(casilla.posX+1, casilla.posY);
           yaseAsigno = true;
         }
      }
      
      if(!(yaseAsigno)&&(lado.ObtenerCasillaPorIndex(casilla.posX-1, casilla.posY)!= null)) //Prueba luego con la de atrás
      {
        if(lado.ObtenerCasillaPorIndex(casilla.posX-1, casilla.posY).Presente == null)
         {
           aMover = lado.ObtenerCasillaPorIndex(casilla.posX-1, casilla.posY);
           yaseAsigno = true;
         }

      }

       if(!(yaseAsigno)&&(lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY+1)!= null)) //Prueba luego con la de arriba
      {
        if(lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY+1).Presente == null)
         {
           aMover = lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY+1);
           yaseAsigno = true;
         }

      }

       if(!(yaseAsigno)&&(lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY-1)!= null)) //Prueba luego con la de abajo
      {
        if(lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY-1).Presente == null)
         {
           aMover = lado.ObtenerCasillaPorIndex(casilla.posX, casilla.posY-1);
           yaseAsigno = true;
         }

      }
      
        if(aMover != null && scUnidad.ObtenerAPActual() >= costoMovimientoAP)
        {
           await MoverACasilla(aMover);

        }
        await Task.Delay(1200);
   }




   void TerminarTurnoDesdeWhile()
   {
    Unidad activa = BattleManager.Instance.unidadActiva;
   
    if(activa == gameObject.GetComponent<Unidad>())
    { 
      if(activa.GetComponent<IAUnidad>() != null && activa.ObtenerAPActual() < activa.mod_maxAccionP)
      { 
         BattleManager.Instance.TerminarTurno();
      }
    }
   }
   List<IAHabilidad> HayHabilidadesPosibles()
   {
     
      IAHabilidad[] habilidades = GetComponents<IAHabilidad>();
      HabPosibles.Clear();
   
      foreach(IAHabilidad hab in habilidades)
      {
        List<object> obj = new List<object>();
        obj = hab.ListaHayObjetivosAlAlcance();
        if(hab.hActualCooldown > 0){continue;}
        

        if (!hab.afectaObstaculos)
      { if (!obj.OfType<Unidad>().Any()) { obj.Clear(); } print("Se limpio lista porque no habia unidades"); }

        if(obj.Count == 0)
        {
          continue;  //Si esta habilidad no tiene objetivos al alcance, se descarta, incluye obstaculos
        }
        else
        {
       
         HabPosibles.Add(hab); //Si esta habilidad tiene objetivos al alcance, se agrega a posibles
        }
      }
       
      return HabPosibles;

   }


   async Task ElegirQueHabilidadPosiblesHacer()
   {
    // Obtener la habilidad con la mayor prioridad

        IAHabilidad habilidadConMayorPrioridad = HabPosibles.OrderByDescending(h => h.prioridad).FirstOrDefault();



        var sortedHabPosibles = HabPosibles.OrderBy(h => h.prioridad).ToList();

        foreach (var hab in sortedHabPosibles)
        {
         // Debug.Log(hab + "  "+hab.prioridad); // Usar Debug.Log en lugar de print en Unity
        }


    if (habilidadConMayorPrioridad != null)
    {
      await habilidadConMayorPrioridad.ActivarHabilidad();
    }
    else
    {
             print("No se encontró ninguna habilidad.");
             BattleManager.Instance.TerminarTurno();

    }
        
   }

   public async Task MoverACasilla(Casilla casillaObjetivo)
    {
        scUnidad.CasillaDeseadaMov = casillaObjetivo;
        scUnidad.CambiarAPActual(-costoMovimientoAP);
        // Simulación de un retraso de movimiento
        await Task.Delay(900);
      
       
        
        scUnidad.CasillaPosicion =  scUnidad.CasillaDeseadaMov;
        scUnidad.CasillaDeseadaMov = null;


    }


}
