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
   private const int MaxIntentosPorTurno = 10;
   private const int TimeoutHabilidadMs = 6000;
   private const int DelayFinTurnoMs = 600;
   
   public void Awake()
   {
      scUnidad = gameObject.GetComponent<Unidad>();
   
   }
   public async void RealizarTurnoIA()
   {
      if (scUnidad == null)
      {
         scUnidad = gameObject.GetComponent<Unidad>();
      }

      if (scUnidad == null)
      {
         Debug.LogWarning("[IAUnidad] No se pudo iniciar el turno porque no se encontro la componente Unidad.");
         return;
      }

      if (BattleManager.Instance == null)
      {
         Debug.LogWarning($"[IAUnidad] {scUnidad.uNombre} no puede actuar sin BattleManager.");
         return;
      }

      int contadorIteraciones = MaxIntentosPorTurno;
      bool turnoTerminado = false;
      bool forzarFinalPorHabilidad = false;

      while (scUnidad.ObtenerAPActual() > 0 && contadorIteraciones > 0 && !turnoTerminado)
      {
         contadorIteraciones--;

         List<IAHabilidad> habilidadesDisponibles = HayHabilidadesPosibles();
         if (BattleManager.Instance.unidadActiva != scUnidad)
         {
            turnoTerminado = true;
            break;
         }

         if (habilidadesDisponibles.Count == 0) // No hay habilidades posibles
         {
            await Task.Delay(1500);

            if (scUnidad.esInmobil)
            {
               scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual()); // Agota AP restante
               turnoTerminado = await TerminarTurnoSeguro(false, "unidad inamovible sin acciones disponibles");
               break;
            }

            // Llamar al metodo para moverse a la casilla objetivo de forma asincrona
            LadoManager lado = scUnidad.CasillaPosicion.lado == 1 ? BattleManager.Instance.ladoA : BattleManager.Instance.ladoB;

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
                  await ChequearCasillasAlrededorParaMover(destPosible); // Intento alternativo para moverse
                  scUnidad.CambiarAPActual(costoMovimientoAP); // Evita loops si no se mueve
                  turnoTerminado = await TerminarTurnoSeguro(false, "sin casillas libres para moverse");
                  break;
               }
            }
            else
            {
               tendenciaMovX = 0;
               tendenciaMovY = 0;
            }
         }
         else if (scUnidad.HP_actual > 0) // Hay habilidades posibles y no murio por algun efecto
         {
            await Task.Delay(1500); // Intervalo entre acciones
            bool ejecutoHabilidad = await EjecutarHabilidadConSalvaguarda(habilidadesDisponibles);
            await Task.Delay(1000);

            if (!ejecutoHabilidad)
            {
               scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
               forzarFinalPorHabilidad = true;
               turnoTerminado = await TerminarTurnoSeguro(false, "no se pudo ejecutar ninguna habilidad disponible");
            }
         }
         else
         {
            break;
         }
      }

      if (turnoTerminado || BattleManager.Instance == null)
      {
         return;
      }

      bool sinIntentosRestantes = contadorIteraciones <= 0 && scUnidad.ObtenerAPActual() > 0;
      bool sinAP = scUnidad.ObtenerAPActual() < 1;

      if (BattleManager.Instance.unidadActiva == scUnidad)
      {
         if (forzarFinalPorHabilidad)
         {
            await TerminarTurnoSeguro(false, "salida por fallo al resolver habilidades");
         }
         else if (sinIntentosRestantes)
         {
            scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
            await TerminarTurnoSeguro(false, "limite de iteraciones alcanzado");
         }
         else if (sinAP)
         {
            await TerminarTurnoSeguro(false);
         }
      }
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
         await Task.Delay(1000);
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

       await Task.Delay(1400);
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
        if (hab == null)
        {
          continue;
        }

        List<object> obj;
        try
        {
          obj = hab.ListaHayObjetivosAlAlcance() ?? new List<object>();
        }
        catch (Exception ex)
        {
          Debug.LogError($"[IAUnidad] {scUnidad?.uNombre} fallo al obtener objetivos para {hab.nombre}: {ex.Message}");
          continue;
        }

        if(hab.hActualCooldown > 0){continue;}
        if(hab.costoAP > scUnidad.ObtenerAPActual()+1){continue;} //Si no tiene AP suficiente, no se agrega

        if (!hab.afectaObstaculos)
      { if (!obj.OfType<Unidad>().Any()) { obj.Clear(); } print("Se limpio lista porque no habia unidades: "+hab.nombre); }

        if(obj.Count == 0)
        {
          continue;  //Si esta habilidad no tiene objetivos al alcance, se descarta, incluye obstaculos
        }
        else
        {
       
         if (!HabPosibles.Contains(hab))
         {
           HabPosibles.Add(hab); //Si esta habilidad tiene objetivos al alcance, se agrega a posibles
         }
        }
      }
       
      return HabPosibles;

   }


   async Task<bool> EjecutarHabilidadConSalvaguarda(List<IAHabilidad> habilidadesDisponibles)
   {
      if (habilidadesDisponibles == null || habilidadesDisponibles.Count == 0)
      {
         return false;
      }

      List<IAHabilidad> habilidadesOrdenadas = habilidadesDisponibles.OrderByDescending(h => h.prioridad).ToList();

      foreach (IAHabilidad habilidad in habilidadesOrdenadas)
      {
         if (habilidad == null)
         {
            continue;
         }

         try
         {
            Task habilidadTask = habilidad.ActivarHabilidad();

            if (habilidadTask == null)
            {
               Debug.LogWarning($"[IAUnidad] {scUnidad.uNombre} recibio una tarea nula al activar {habilidad.nombre}.");
               continue;
            }

            Task timeoutTask = Task.Delay(TimeoutHabilidadMs);
            Task tareaCompletada = await Task.WhenAny(habilidadTask, timeoutTask);

            if (tareaCompletada == habilidadTask)
            {
               await habilidadTask; // Repropaga excepciones si las hay
               return true;
            }

            Debug.LogWarning($"[IAUnidad] {scUnidad.uNombre} supero el tiempo limite al ejecutar {habilidad.nombre}.");
         }
         catch (Exception ex)
         {
            Debug.LogError($"[IAUnidad] Error ejecutando {habilidad?.nombre} para {scUnidad.uNombre}: {ex.Message}");
         }
      }

      return false;
   }

   async Task<bool> TerminarTurnoSeguro(bool agotarAP, string motivo = null)
   {
      if (scUnidad == null || BattleManager.Instance == null)
      {
         return false;
      }

      if (agotarAP && scUnidad.ObtenerAPActual() > 0)
      {
         scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
      }

      if (!string.IsNullOrEmpty(motivo))
      {
         Debug.LogWarning($"[IAUnidad] {scUnidad.uNombre} finaliza turno de IA: {motivo}");
      }

      await Task.Delay(DelayFinTurnoMs);

      if (BattleManager.Instance.unidadActiva == scUnidad)
      {
         BattleManager.Instance.TerminarTurno();
         return true;
      }

      return false;
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
        await Task.Delay(500);

        scUnidad.CasillaPosicion =  scUnidad.CasillaDeseadaMov;
        scUnidad.CasillaDeseadaMov = null;


    }


}
