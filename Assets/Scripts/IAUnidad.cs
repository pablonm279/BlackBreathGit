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
   private const int MaxIntentosPorTurno = 8;
   private readonly List<Task> _habilidadesEnCurso = new List<Task>();
   private bool _turnoIAEnCurso;

   private struct AITurnTimings
   {
      public int DelaySinObjetivosMs;
      public int DelayPreAccionMs;
      public int DelayPostAccionMs;
      public int DelayMovimientoFallbackMs;
      public int DelayFinTurnoMs;
      public int HabilidadTimeoutMs;
      public int GraciaHabilidadMs;
      public int ExtraRangoMs;

      public static AITurnTimings Crear(bool modoRapido)
      {
         float factor = modoRapido ? 0.65f : 1f;
         return new AITurnTimings
         {
            DelaySinObjetivosMs = Mathf.RoundToInt(800f * factor),
            DelayPreAccionMs = Mathf.RoundToInt(1150f * factor),
            DelayPostAccionMs = Mathf.RoundToInt(250f * factor),
            DelayMovimientoFallbackMs = Mathf.RoundToInt(260f * factor),
            DelayFinTurnoMs = Mathf.RoundToInt(950f * factor),
            HabilidadTimeoutMs = Mathf.RoundToInt(1100f * factor),
            GraciaHabilidadMs = Mathf.RoundToInt(800f * factor),
            ExtraRangoMs = Mathf.RoundToInt(200f * factor)
         };
      }
   }
   
   public void Awake()
   {
      scUnidad = gameObject.GetComponent<Unidad>();
   
   }
   public void RealizarTurnoIA()
   {
      EjecutarTurnoIASeguro();
   }

   private async void EjecutarTurnoIASeguro()
   {
      try
      {
         await RealizarTurnoIAAsync();
      }
      catch (Exception ex)
      {
         Debug.LogException(ex, this);
      }
   }

   private async Task RealizarTurnoIAAsync()
   {
      if (_turnoIAEnCurso)
      {
         return;
      }

      _turnoIAEnCurso = true;
      try
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
         AITurnTimings timings = ObtenerTimingsActuales();
         contadorIteraciones--;

         List<IAHabilidad> habilidadesDisponibles = HayHabilidadesPosibles();
         if (BattleManager.Instance.unidadActiva != scUnidad)
         {
            turnoTerminado = true;
            break;
         }

         if (habilidadesDisponibles.Count == 0) // No hay habilidades posibles
         {
            await DelayIA(timings.DelaySinObjetivosMs);

            if (scUnidad.esInmobil)
            {
               scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual()); // Agota AP restante
               turnoTerminado = await TerminarTurnoSeguro(false, timings, "unidad inamovible sin acciones disponibles");
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
               Casilla reposicionLateral = BuscarReposicionLateralGenerica(lado, scUnidad.CasillaPosicion);
               if (reposicionLateral != null)
               {
                  await MoverACasilla(reposicionLateral);
                  await DelayIA(timings.DelayMovimientoFallbackMs);
                  continue;
               }

               tendenciaMovX = 0;
               tendenciaMovY = 0;
               await DelayIA(timings.DelayMovimientoFallbackMs);
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
                  await ChequearCasillasAlrededorParaMover(destPosible, timings); // Intento alternativo para moverse
                  turnoTerminado = await TerminarTurnoSeguro(false, timings, "sin casillas libres para moverse");
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
            int delay = timings.DelayPreAccionMs + (esRango ? timings.ExtraRangoMs : 0);
            await DelayIA(delay); // Intervalo entre acciones
            bool ejecutoHabilidad = await EjecutarHabilidadConSalvaguarda(habilidadesDisponibles, timings);
            await EsperarHabilidadesPendientes(timings.GraciaHabilidadMs);
            await DelayIA(timings.DelayPostAccionMs + (esRango ? timings.ExtraRangoMs / 2 : 0));

            if (!ejecutoHabilidad)
            {
               scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
               forzarFinalPorHabilidad = true;
               turnoTerminado = await TerminarTurnoSeguro(false, timings, "no se pudo ejecutar ninguna habilidad disponible");
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
      AITurnTimings finTimings = ObtenerTimingsActuales();

      if (BattleManager.Instance.unidadActiva == scUnidad)
      {
         if (forzarFinalPorHabilidad)
         {
            await TerminarTurnoSeguro(false, finTimings, "salida por fallo al resolver habilidades");
         }
         else if (sinIntentosRestantes)
         {
            scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
            await TerminarTurnoSeguro(false, finTimings, "limite de iteraciones alcanzado");
         }
         else if (sinAP)
         {
            await TerminarTurnoSeguro(false, finTimings);
         }
      }
      }
      finally
      {
         _turnoIAEnCurso = false;
      }
   }

   private static Task DelayIA(int milliseconds)
   {
      return Task.Delay(Mathf.Max(0, milliseconds));
   }

   private AITurnTimings ObtenerTimingsActuales()
   {
      bool modoRapido = BattleManager.Instance != null && BattleManager.Instance.modoRapidoActivado;
      return AITurnTimings.Crear(modoRapido);
   }

   private void RegistrarHabilidadEnCurso(Task habilidadTask)
   {
      if (habilidadTask == null) { return; }

      lock (_habilidadesEnCurso)
      {
         _habilidadesEnCurso.Add(habilidadTask);
      }

      habilidadTask.ContinueWith(_ =>
      {
         lock (_habilidadesEnCurso)
         {
            _habilidadesEnCurso.Remove(habilidadTask);
         }
      }, TaskScheduler.Default);
   }

   private async Task EsperarHabilidadesPendientes(int maxEsperaMs)
   {
      Task[] snapshot;
      lock (_habilidadesEnCurso)
      {
         snapshot = _habilidadesEnCurso.Where(t => t != null && !t.IsCompleted && !t.IsCanceled).ToArray();
      }

      if (snapshot.Length == 0)
      {
         return;
      }

      Task allTasks = Task.WhenAll(snapshot);
      Task limitTask = DelayIA(maxEsperaMs);
      Task finished = await Task.WhenAny(allTasks, limitTask);

      if (finished != allTasks)
      {
         Debug.LogWarning($"[IAUnidad] {scUnidad?.uNombre} finaliza con {snapshot.Length} tareas de habilidad pendientes (timeout de espera).");
      }
      else
      {
         await allTasks; // Repropaga si hubo excepciones
      }
   }
   
   async Task ChequearCasillasAlrededorParaMover(Casilla casilla, AITurnTimings timings)
   {
       LadoManager lado;
       if(scUnidad.CasillaPosicion.lado == 1)
       {
          lado = BattleManager.Instance.ladoA;
       }else { lado = BattleManager.Instance.ladoB;} 

       Casilla casillaActual = scUnidad.CasillaPosicion;
       if (casillaActual == null)
       {
         await DelayIA(timings.DelaySinObjetivosMs);
         return;
       }

       Casilla mejorOpcion = null;
       int mejorDistancia = int.MaxValue;
       int mejorPrioridadEmpate = int.MaxValue;

       int objetivoX = casilla != null ? casilla.posX : casillaActual.posX;
       int objetivoY = casilla != null ? casilla.posY : casillaActual.posY;
       bool preferirCambioFila = casilla != null && casilla.posX > casillaActual.posX && casilla.posY == casillaActual.posY;

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
         int prioridadEmpate = ObtenerPrioridadEnEmpate(dx, dy, preferirCambioFila);

         if (distancia < mejorDistancia || (distancia == mejorDistancia && prioridadEmpate < mejorPrioridadEmpate))
         {
           mejorDistancia = distancia;
           mejorPrioridadEmpate = prioridadEmpate;
           mejorOpcion = candidata;
         }
       }

       if (mejorOpcion != null && scUnidad.ObtenerAPActual() >= costoMovimientoAP)
       {
         await MoverACasilla(mejorOpcion);
       }

       await DelayIA(timings.DelayPostAccionMs);
   }

   private Casilla BuscarReposicionLateralGenerica(LadoManager lado, Casilla casillaActual)
   {
      if (lado == null || casillaActual == null || scUnidad == null)
      {
         return null;
      }

      int direccionPreferida = ObtenerDireccionFilaHaciaEnemigo(casillaActual);
      int[] intentos = direccionPreferida != 0
         ? new int[] { direccionPreferida, -direccionPreferida }
         : new int[] { 1, -1 };

      foreach (int dy in intentos)
      {
         Casilla candidata = ObtenerCasillaLateralDisponible(lado, casillaActual, dy);
         if (candidata != null)
         {
            return candidata;
         }
      }

      if (!HayBloqueoFrontal(lado, casillaActual))
      {
         return null;
      }

      Casilla lateralArriba = ObtenerCasillaLateralDisponible(lado, casillaActual, 1);
      if (lateralArriba != null)
      {
         return lateralArriba;
      }

      return ObtenerCasillaLateralDisponible(lado, casillaActual, -1);
   }

   private Casilla ObtenerCasillaLateralDisponible(LadoManager lado, Casilla origen, int deltaY)
   {
      if (lado == null || origen == null || deltaY == 0)
      {
         return null;
      }

      int nuevoY = origen.posY + deltaY;
      if (nuevoY < 1 || nuevoY > 5)
      {
         return null;
      }

      Casilla candidata = lado.ObtenerCasillaPorIndex(origen.posX, nuevoY);
      if (candidata == null || candidata.Presente != null)
      {
         return null;
      }

      return candidata;
   }

   private int ObtenerDireccionFilaHaciaEnemigo(Casilla casillaActual)
   {
      if (BattleManager.Instance == null || BattleManager.Instance.lUnidadesTotal == null || casillaActual == null)
      {
         return 0;
      }

      Unidad enemigoMasCercano = null;
      int mejorDistanciaVertical = int.MaxValue;
      int mejorDistanciaHorizontal = int.MaxValue;

      foreach (Unidad unidad in BattleManager.Instance.lUnidadesTotal)
      {
         if (unidad == null || unidad.HP_actual <= 0 || unidad.CasillaPosicion == null)
         {
            continue;
         }

         if (unidad.CasillaPosicion.lado == casillaActual.lado)
         {
            continue;
         }

         if (!bPuedeVerEscondidos && unidad.ObtenerEstaEscondido() > 0)
         {
            continue;
         }

         int distanciaVertical = Math.Abs(unidad.CasillaPosicion.posY - casillaActual.posY);
         int distanciaHorizontal = Math.Abs(3 - unidad.CasillaPosicion.posX);

         if (distanciaVertical < mejorDistanciaVertical ||
            (distanciaVertical == mejorDistanciaVertical && distanciaHorizontal < mejorDistanciaHorizontal))
         {
            mejorDistanciaVertical = distanciaVertical;
            mejorDistanciaHorizontal = distanciaHorizontal;
            enemigoMasCercano = unidad;
         }
      }

      if (enemigoMasCercano == null || enemigoMasCercano.CasillaPosicion == null)
      {
         return 0;
      }

      return Math.Sign(enemigoMasCercano.CasillaPosicion.posY - casillaActual.posY);
   }

   private bool HayBloqueoFrontal(LadoManager lado, Casilla casillaActual)
   {
      if (lado == null || casillaActual == null || casillaActual.posX >= 3)
      {
         return false;
      }

      Casilla frontal = lado.ObtenerCasillaPorIndex(casillaActual.posX + 1, casillaActual.posY);
      return frontal != null && frontal.Presente != null;
   }

   private static int ObtenerPrioridadEnEmpate(int dx, int dy, bool preferirCambioFila)
   {
      if (dy != 0)
      {
         return preferirCambioFila ? 0 : 1;
      }

      if (dx > 0)
      {
         return preferirCambioFila ? 1 : 0;
      }

      if (dx < 0)
      {
         return 2;
      }

      return 3;
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
      { if (!obj.OfType<Unidad>().Any()) { obj.Clear(); } }

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


   async Task<bool> EjecutarHabilidadConSalvaguarda(List<IAHabilidad> habilidadesDisponibles, AITurnTimings timings)
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

            RegistrarHabilidadEnCurso(habilidadTask);

            Task tareaCompletada = await Task.WhenAny(habilidadTask, DelayIA(timings.HabilidadTimeoutMs));

            if (tareaCompletada == habilidadTask)
            {
               await habilidadTask; // Repropaga excepciones si las hay
               await EsperarSecuenciaVisualAsync(habilidad, timings);
               return true;
            }

            Task gracia = await Task.WhenAny(habilidadTask, DelayIA(timings.GraciaHabilidadMs));
            if (gracia == habilidadTask)
            {
               await habilidadTask;
               await EsperarSecuenciaVisualAsync(habilidad, timings);
               return true;
            }

            Debug.LogWarning($"[IAUnidad] {scUnidad.uNombre} supero el tiempo limite al ejecutar {habilidad.nombre}.");
            await EsperarSecuenciaVisualAsync(habilidad, timings);
         }
         catch (Exception ex)
         {
            Debug.LogError($"[IAUnidad] Error ejecutando {habilidad?.nombre} para {scUnidad.uNombre}: {ex.Message}");
         }
      }

      return false;
   }

   private async Task EsperarSecuenciaVisualAsync(IAHabilidad habilidad, AITurnTimings timings)
   {
      if (habilidad == null)
      {
         return;
      }

      Task visualTask = habilidad.EsperarSecuenciaVisualAsync();
      if (visualTask == null || visualTask.IsCompleted)
      {
         return;
      }

      int maxEsperaMs = Mathf.Max(timings.HabilidadTimeoutMs, timings.DelayPostAccionMs) + timings.GraciaHabilidadMs + 1200;
      Task timeout = DelayIA(maxEsperaMs);
      Task terminado = await Task.WhenAny(visualTask, timeout);

      if (terminado != visualTask)
      {
         Debug.LogWarning($"[IAUnidad] {scUnidad?.uNombre} supera la espera visual de {habilidad.nombre}.");
         return;
      }

      await visualTask;
   }

   async Task<bool> TerminarTurnoSeguro(bool agotarAP, AITurnTimings timings, string motivo = null)
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

      await EsperarHabilidadesPendientes(timings.GraciaHabilidadMs);
      await ForzarRetornoMeleeVisualSiCorresponde();
      await DelayIA(timings.DelayFinTurnoMs);

      if (BattleManager.Instance.unidadActiva == scUnidad)
      {
         BattleManager.Instance.TerminarTurno();
         return true;
      }

      return false;
   }

   private async Task ForzarRetornoMeleeVisualSiCorresponde()
   {
      if (scUnidad == null)
      {
         return;
      }

      MeleeApproachMover mover = scUnidad.GetComponent<MeleeApproachMover>();
      if (mover == null)
      {
         return;
      }

      try
      {
         await mover.VolverAPosicionInicialAsync(true);
      }
      catch (Exception ex)
      {
         Debug.LogWarning($"[IAUnidad] No se pudo forzar retorno visual melee de {scUnidad.uNombre}: {ex.Message}");
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
        await Task.Delay(450);

        scUnidad.CasillaPosicion =  scUnidad.CasillaDeseadaMov;
        scUnidad.CasillaDeseadaMov = null;


    }


}
