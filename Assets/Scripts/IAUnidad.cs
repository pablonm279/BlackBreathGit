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
   
    int contadorIteraciones = 10; // Ajusta este valor segun sea necesario

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
     

      // Llamar al metodo para moverse a la casilla objetivo de forma asincrona
      LadoManager lado;
      if (scUnidad.CasillaPosicion.lado == 1)
      {
        lado = BattleManager.Instance.ladoA;
      }
      else { lado = BattleManager.Instance.ladoB; }

      int posXActual = scUnidad.CasillaPosicion.posX;
      int posYActual = scUnidad.CasillaPosicion.posY;

      int destinoX = posXActual;
      int destinoY = posYActual;
      bool intentoDeMovimiento = false;

      int pasoX = Math.Sign(tendenciaMovX);
      int pasoY = Math.Sign(tendenciaMovY);

      if (pasoX != 0)
      {
        bool puedeMoverHorizontal = true;

        if (pasoX > 0)
        {
          if (posXActual >= 3 || (esRango && posXActual != 1))
          {
            puedeMoverHorizontal = false;
          }
        }
        else
        {
          if (posXActual <= 1)
          {
            puedeMoverHorizontal = false;
          }
        }

        if (puedeMoverHorizontal)
        {
          destinoX = posXActual + pasoX;
          tendenciaMovX -= pasoX;
          intentoDeMovimiento = true;
        }
        else
        {
          tendenciaMovX = 0;
        }
      }

      if (!intentoDeMovimiento && pasoY != 0)
      {
        int candidatoY = posYActual + pasoY;
        if (candidatoY >= 1 && candidatoY <= 5)
        {
          destinoY = candidatoY;
          tendenciaMovY -= pasoY;
          intentoDeMovimiento = true;
        }
        else
        {
          tendenciaMovY = 0;
        }
      }

      if (!intentoDeMovimiento)
      {
        tendenciaMovX = 0;
        tendenciaMovY = 0;
        await Task.Delay(300);
        continue;
      }

      Casilla destPosible = lado.ObtenerCasillaPorIndex(destinoX, destinoY);
      if (destPosible != null)
      {
        if (destPosible.Presente == null)
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
      else
      {
        tendenciaMovX = 0;
        tendenciaMovY = 0;
      }
    }
     else if(scUnidad.HP_actual > 0) //Hay Habilidades posibles y no murio por algun efecto
     {
//       print("HAY hab cant: "+HayHabilidadesPosibles().Count);
        await Task.Delay(2000);//Intervalo entre acciones
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

       Casilla casillaActual = scUnidad.CasillaPosicion;
       if (casillaActual == null)
       {
         await Task.Delay(1200);
         return;
       }

       Casilla mejorOpcion = null;
       int mejorDistancia = int.MaxValue;

       int objetivoX = casilla != null ? casilla.posX : casillaActual.posX;
       int objetivoY = casilla != null ? casilla.posY : casillaActual.posY;

       int[,] offsets = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

       for (int i = 0; i < offsets.GetLength(0); i++)
       {
         int dx = offsets[i, 0];
         int dy = offsets[i, 1];

         int nx = casillaActual.posX + dx;
         int ny = casillaActual.posY + dy;

         if (nx < 1 || nx > 3 || ny < 1 || ny > 5)
         {
           continue;
         }

         if (dx > 0 && esRango && casillaActual.posX != 1)
         {
           continue;
         }

         Casilla candidata = lado.ObtenerCasillaPorIndex(nx, ny);
         if (candidata == null || candidata.Presente != null)
         {
           continue;
         }

         int distancia = Math.Abs(objetivoX - nx) + Math.Abs(objetivoY - ny);

         if (distancia < mejorDistancia)
         {
           mejorDistancia = distancia;
           mejorOpcion = candidata;
         }
       }

       if (mejorOpcion != null && scUnidad.ObtenerAPActual() >= costoMovimientoAP)
       {
         await MoverACasilla(mejorOpcion);
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
             print("No se encontro ninguna habilidad.");
             BattleManager.Instance.TerminarTurno();

    }
        
   }

   public async Task MoverACasilla(Casilla casillaObjetivo)
    {
        if (casillaObjetivo == null || scUnidad.CasillaPosicion == null)
        {
            return;
        }

        int deltaX = Math.Abs(casillaObjetivo.posX - scUnidad.CasillaPosicion.posX);
        int deltaY = Math.Abs(casillaObjetivo.posY - scUnidad.CasillaPosicion.posY);

        if (deltaX + deltaY != 1)
        {
            Debug.LogWarning($"{scUnidad.uNombre} intento moverse a una casilla no adyacente.");
            return;
        }

        if (scUnidad.ObtenerAPActual() < costoMovimientoAP)
        {
            return;
        }

        scUnidad.CasillaDeseadaMov = casillaObjetivo;
        scUnidad.CambiarAPActual(-costoMovimientoAP);
        // Simulacion de un retraso de movimiento
        await Task.Delay(900);

        scUnidad.CasillaPosicion =  scUnidad.CasillaDeseadaMov;
        scUnidad.CasillaDeseadaMov = null;


    }


}
