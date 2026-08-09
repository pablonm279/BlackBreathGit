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
   private static readonly Vector2Int[] DireccionesOrtogonales =
   {
      new Vector2Int(1, 0),
      new Vector2Int(0, 1),
      new Vector2Int(0, -1),
      new Vector2Int(-1, 0)
   };
  

   public bool esRango; //Tiende a mantenerse atras
   public bool bPuedeVerEscondidos = false; //Si puede ver unidades escondidas, si no, no las ataca
   [Header("Composicion")]
   public bool unicoEnCombate = false; // Si esta activo, no puede haber mas de 1 en la composicion inicial enemiga.
   public int tierEnemigo = 1;
   public bool siempreEnRetaguardiaInicial = false;

   public int costoMovimientoAP = 1; //Cuanto gasta de AP al moverse 

   public int tendenciaMovY;
   public int tendenciaMovX;

   public List<IAHabilidad> HabPosibles = new List<IAHabilidad>();
   public IAHabilidad HabilidadIAEnEjecucion { get; private set; }
   private const int MaxIntentosPorTurno = 8;
   private readonly List<Task> _habilidadesEnCurso = new List<Task>();
   private readonly HashSet<Casilla> _casillasVisitadasTurnoIA = new HashSet<Casilla>();
   private Casilla _casillaInicialTurnoIA;
   private bool _memoriaMovimientoTurnoActiva;
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

      if (AbortarTurnoSiUnidadNoPuedeActuar())
      {
         return;
      }

      ReiniciarMemoriaMovimientoTurno();

      int contadorIteraciones = MaxIntentosPorTurno;
      bool turnoTerminado = false;
      bool forzarFinalPorHabilidad = false;
      bool realizoAccion = false;

      while (PuedeContinuarTurnoIA() && scUnidad.ObtenerAPActual() > 0 && contadorIteraciones > 0 && !turnoTerminado)
      {
         AITurnTimings timings = ObtenerTimingsActuales();
         contadorIteraciones--;

         if (await IntentarEscaparSiDisponible(timings))
         {
            realizoAccion = true;
            turnoTerminado = true;
            break;
         }

         List<IAHabilidad> habilidadesDisponibles = HayHabilidadesPosibles();
         if (BattleManager.Instance.unidadActiva != scUnidad)
         {
            turnoTerminado = true;
            break;
         }

         if (!PuedeAtacarUnidadDesdePosicionActual(habilidadesDisponibles)
            && await IntentarDestruirObstaculoBloqueante(timings))
         {
            realizoAccion = true;
            turnoTerminado = true;
            break;
         }

         FiltrarAtaquesDeRangoSoloAObstaculosParaBuscarEnemigos(habilidadesDisponibles);

         if (habilidadesDisponibles.Count == 0) // No hay habilidades posibles
         {
            BattleManager.Instance.RestaurarCamaraHabilidad();
            await ForzarRetornoMeleeVisualSiCorresponde();
            await DelayIA(timings.DelaySinObjetivosMs);

            if (AbortarTurnoSiUnidadNoPuedeActuar() || scUnidad.ObtenerAPActual() <= 0)
            {
               break;
            }

            if (scUnidad.esInmobil)
            {
               tendenciaMovX = 0;
               tendenciaMovY = 0;
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
               if (reposicionLateral != null && await MoverACasilla(reposicionLateral))
               {
                  await DelayIA(timings.DelayMovimientoFallbackMs);
                  continue;
               }

               Casilla intercambioLateral = BuscarIntercambioLateralGenerico(lado, scUnidad.CasillaPosicion);
               if (intercambioLateral != null && await IntentarIntercambioLateralConAliado(intercambioLateral))
               {
                  await DelayIA(timings.DelayMovimientoFallbackMs);
                  continue;
               }

               tendenciaMovX = 0;
               tendenciaMovY = 0;
               await DelayIA(timings.DelayMovimientoFallbackMs);
               break;
            }

            Casilla destPosible = lado.ObtenerCasillaPorIndex(destinoX, destinoY);
            bool seMovioEnDireccionDeseada = false;
            if (destPosible != null)
            {
               if (destPosible.Presente == null && PuedeVisitarCasillaEnTurno(destPosible))
               {
                  seMovioEnDireccionDeseada = await MoverACasilla(destPosible);
               }
               else
               {
                  if (await ChequearCasillasAlrededorParaMover(destPosible, timings))
                  {
                     continue;
                  }
               }
            }
            else
            {
               tendenciaMovX = 0;
               tendenciaMovY = 0;
            }

            if (seMovioEnDireccionDeseada)
            {
               if (destinoX != posXActual)
               {
                  tendenciaMovX -= pasoX;
               }
               else if (destinoY != posYActual)
               {
                  tendenciaMovY -= pasoY;
               }
            }
            else
            {
               break;
            }
         }
         else if (scUnidad.HP_actual > 0) // Hay habilidades posibles y no murio por algun efecto
         {
            int delay = timings.DelayPreAccionMs + (esRango ? timings.ExtraRangoMs : 0);
            await DelayIA(delay); // Intervalo entre acciones

            if (AbortarTurnoSiUnidadNoPuedeActuar() || scUnidad.ObtenerAPActual() <= 0)
            {
               break;
            }

            bool ejecutoHabilidad = await EjecutarHabilidadConSalvaguarda(habilidadesDisponibles, timings);

            if (AbortarTurnoSiUnidadNoPuedeActuar())
            {
               return;
            }

            await EsperarHabilidadesPendientes(timings.GraciaHabilidadMs);
            await DelayIA(timings.DelayPostAccionMs + (esRango ? timings.ExtraRangoMs / 2 : 0));

            if (AbortarTurnoSiUnidadNoPuedeActuar())
            {
               return;
            }

            if (!ejecutoHabilidad)
            {
               scUnidad.CambiarAPActual(-(int)scUnidad.ObtenerAPActual());
               forzarFinalPorHabilidad = true;
               turnoTerminado = await TerminarTurnoSeguro(false, timings, "no se pudo ejecutar ninguna habilidad disponible");
            }
            else
            {
               realizoAccion = true;
            }
         }
         else
         {
            break;
         }
      }

      if (AbortarTurnoSiUnidadNoPuedeActuar())
      {
         return;
      }

      if (!realizoAccion && BattleManager.Instance != null && BattleManager.Instance.unidadActiva == scUnidad)
      {
         AITurnTimings timingsFinales = ObtenerTimingsActuales();
         if (await IntentarDestruirObstaculoBloqueante(timingsFinales))
         {
            return;
         }

         await TerminarTurnoSeguro(false, timingsFinales, "sin acciones valiosas; no se pudo destruir obstaculo");
         return;
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
         else
         {
            await TerminarTurnoSeguro(false, finTimings, "sin movimientos utiles disponibles");
         }
      }
      }
      finally
      {
         _memoriaMovimientoTurnoActiva = false;
         _turnoIAEnCurso = false;
      }
   }

   private static Task DelayIA(int milliseconds)
   {
      return BattleManager.DelayCombateAsync(Mathf.Max(0, milliseconds));
   }

   private void ReiniciarMemoriaMovimientoTurno()
   {
      tendenciaMovX = 0;
      tendenciaMovY = 0;
      _casillaInicialTurnoIA = scUnidad != null ? scUnidad.CasillaPosicion : null;
      _casillasVisitadasTurnoIA.Clear();
      if (_casillaInicialTurnoIA != null)
      {
         _casillasVisitadasTurnoIA.Add(_casillaInicialTurnoIA);
      }

      _memoriaMovimientoTurnoActiva = true;
   }

   private bool PuedeVisitarCasillaEnTurno(Casilla casilla)
   {
      if (casilla == null)
      {
         return false;
      }

      if (!_memoriaMovimientoTurnoActiva)
      {
         return true;
      }

      if (casilla == _casillaInicialTurnoIA && scUnidad != null && scUnidad.CasillaPosicion != _casillaInicialTurnoIA)
      {
         return false;
      }

      return !_casillasVisitadasTurnoIA.Contains(casilla);
   }

   private void RegistrarMovimientoTurno(Casilla origen, Casilla destino)
   {
      if (!_memoriaMovimientoTurnoActiva)
      {
         return;
      }

      if (origen != null)
      {
         _casillasVisitadasTurnoIA.Add(origen);
      }

      if (destino != null)
      {
         _casillasVisitadasTurnoIA.Add(destino);
      }
   }

   private bool PuedeContinuarTurnoIA()
   {
      return scUnidad != null
         && scUnidad.HP_actual > 0f
         && scUnidad.gameObject.activeInHierarchy
         && BattleManager.Instance != null
         && BattleManager.Instance.unidadActiva == scUnidad;
   }

   private bool AbortarTurnoSiUnidadNoPuedeActuar()
   {
      if (PuedeContinuarTurnoIA())
      {
         return false;
      }

      BattleManager battleManager = BattleManager.Instance;
      if (battleManager != null && scUnidad != null && battleManager.unidadActiva == scUnidad)
      {
         battleManager.TerminarTurno();
      }

      return true;
   }

   private async Task<bool> IntentarEscaparSiDisponible(AITurnTimings timings)
   {
      if (scUnidad == null || BattleManager.Instance == null || BattleManager.Instance.unidadActiva != scUnidad)
      {
         return false;
      }

      if (scUnidad.GetComponent<IAUnidad>() != null)
      {
         return false;
      }

      Escapar habilidadEscape = scUnidad.GetComponent<Escapar>();
      if (habilidadEscape == null || scUnidad.CasillaPosicion == null || scUnidad.CasillaPosicion.lado != 2)
      {
         return false;
      }

      if (scUnidad.ObtenerAPActual() < habilidadEscape.costoAP)
      {
         return false;
      }

      await DelayIA(timings.DelayPreAccionMs);
      await habilidadEscape.Resolver(new List<object> { scUnidad });
      await DelayIA(timings.DelayPostAccionMs);
      return true;
   }

   private async Task<bool> IntentarDestruirObstaculoBloqueante(AITurnTimings timings)
   {
      if (scUnidad == null || scUnidad.CasillaPosicion == null || BattleManager.Instance == null)
      {
         return false;
      }

      List<Obstaculo> obstaculosAdyacentes = ObtenerObstaculosAdyacentesOrtogonales();
      Obstaculo objetivo = ObtenerObstaculoEnDireccionDeseada(obstaculosAdyacentes);
      if (objetivo == null && !HaySalidaMovimientoDisponible())
      {
         objetivo = ObtenerObstaculoDestruiblePreferido(obstaculosAdyacentes);
      }

      if (objetivo == null)
      {
         return false;
      }

      await DelayIA(timings.DelayPreAccionMs);
      if (!PuedeContinuarTurnoIA())
      {
         return false;
      }

      await ResolverDestruccionObstaculoIA(objetivo, timings, "obstaculo bloqueando movimiento de IA");
      return true;
   }

   private async Task ResolverDestruccionObstaculoIA(Obstaculo objetivo, AITurnTimings timings, string motivo)
   {
      if (objetivo == null || !PuedeContinuarTurnoIA())
      {
         return;
      }

      string nombreHabilidad = TraducirTexto("Destruir Obstaculo");
      string nombreUnidad = NombreUnidadParaLogLocal(scUnidad);

      scUnidad.MostrarRotuloHabilidadIA(nombreHabilidad, new Color(1f, 0.86f, 0.62f, 1f));
      BattleManager.Instance.EscribirLog($"{nombreUnidad} {TraducirTexto("usa ")}{nombreHabilidad}.");

      scUnidad.ReproducirAnimacionAtaque(true);
      await DelayIA(250);

      if (AbortarTurnoSiUnidadNoPuedeActuar())
      {
         return;
      }

      if (objetivo.CasillaPosicion != null && objetivo.CasillaPosicion.Presente == objetivo.gameObject)
      {
         objetivo.CasillaPosicion.Presente = null;
      }

      objetivo.ReproducirSonidoImpactoRoca();
      objetivo.ForzarDestruccion(true);

      string nombreObstaculo = TRADU.i != null ? TRADU.i.Traducir(objetivo.oName) : objetivo.oName;
      BattleManager.Instance.EscribirLog($"{TraducirTexto("Destruyes")} {nombreObstaculo}.");
      BattleManager.Instance.CalcularCasillasAMovimiento();

      await TerminarTurnoSeguro(true, timings, motivo);
   }

   private bool PuedeAtacarUnidadDesdePosicionActual(List<IAHabilidad> habilidadesDisponibles)
   {
      if (habilidadesDisponibles == null || habilidadesDisponibles.Count == 0 || scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return false;
      }

      foreach (IAHabilidad habilidad in habilidadesDisponibles)
      {
         if (habilidad == null || !habilidad.esHostil || habilidad.objPosibles == null)
         {
            continue;
         }

         foreach (Unidad unidad in habilidad.objPosibles.OfType<Unidad>())
         {
            if (unidad == null || unidad.HP_actual <= 0 || unidad.CasillaPosicion == null)
            {
               continue;
            }

            if (unidad.CasillaPosicion.lado != scUnidad.CasillaPosicion.lado)
            {
               return true;
            }
         }
      }

      return false;
   }

   private void FiltrarAtaquesDeRangoSoloAObstaculosParaBuscarEnemigos(List<IAHabilidad> habilidadesDisponibles)
   {
      if (!esRango || habilidadesDisponibles == null || habilidadesDisponibles.Count == 0)
      {
         return;
      }

      if (!HayUnidadEnemigaVisibleViva())
      {
         return;
      }

      habilidadesDisponibles.RemoveAll(EsAtaqueDeRangoSoloAObstaculos);
   }

   private bool EsAtaqueDeRangoSoloAObstaculos(IAHabilidad habilidad)
   {
      if (habilidad == null || !habilidad.esHostil || habilidad.esMelee || habilidad.objPosibles == null)
      {
         return false;
      }

      bool tieneObstaculo = habilidad.objPosibles.OfType<Obstaculo>().Any();
      bool tieneUnidad = habilidad.objPosibles.OfType<Unidad>().Any(unidad =>
         unidad != null
         && unidad.HP_actual > 0
         && scUnidad != null
         && scUnidad.CasillaPosicion != null
         && unidad.CasillaPosicion != null
         && unidad.CasillaPosicion.lado != scUnidad.CasillaPosicion.lado);

      return tieneObstaculo && !tieneUnidad;
   }

   private bool HayUnidadEnemigaVisibleViva()
   {
      if (BattleManager.Instance == null || BattleManager.Instance.lUnidadesTotal == null || scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return false;
      }

      foreach (Unidad unidad in BattleManager.Instance.lUnidadesTotal)
      {
         if (unidad == null || unidad.HP_actual <= 0 || unidad.CasillaPosicion == null)
         {
            continue;
         }

         if (unidad.CasillaPosicion.lado == scUnidad.CasillaPosicion.lado)
         {
            continue;
         }

         if (!bPuedeVerEscondidos && unidad.ObtenerEstaEscondido() > 0)
         {
            continue;
         }

         return true;
      }

      return false;
   }

   private Obstaculo ObtenerObstaculoEnDireccionDeseada(List<Obstaculo> obstaculosAdyacentes)
   {
      if (obstaculosAdyacentes == null || obstaculosAdyacentes.Count == 0)
      {
         return null;
      }

      Vector2Int direccion = ObtenerDireccionMovimientoDeseada();
      if (direccion == Vector2Int.zero)
      {
         return null;
      }

      LadoManager lado = ObtenerLadoActual();
      Casilla casillaObjetivo = ObtenerCasillaVecina(lado, direccion);
      if (casillaObjetivo == null || casillaObjetivo.Presente == null)
      {
         return null;
      }

      Obstaculo obstaculo = casillaObjetivo.Presente.GetComponent<Obstaculo>();
      return obstaculo != null && obstaculosAdyacentes.Contains(obstaculo) ? obstaculo : null;
   }

   private Vector2Int ObtenerDireccionMovimientoDeseada()
   {
      if (scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return Vector2Int.zero;
      }

      Casilla actual = scUnidad.CasillaPosicion;
      int pasoX = Math.Sign(tendenciaMovX);
      if (pasoX > 0 && (actual.posX >= 3 || (esRango && actual.posX != 1)))
      {
         pasoX = 0;
      }
      else if (pasoX < 0 && actual.posX <= 1)
      {
         pasoX = 0;
      }

      if (pasoX != 0)
      {
         return new Vector2Int(pasoX, 0);
      }

      int pasoY = Math.Sign(tendenciaMovY);
      int destinoY = actual.posY + pasoY;
      return pasoY != 0 && destinoY >= 1 && destinoY <= 5
         ? new Vector2Int(0, pasoY)
         : Vector2Int.zero;
   }

   private bool HaySalidaMovimientoDisponible()
   {
      if (scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return false;
      }

      LadoManager lado = ObtenerLadoActual();
      if (lado == null)
      {
         return false;
      }

      foreach (Vector2Int direccion in DireccionesOrtogonales)
      {
         if (direccion.x > 0 && esRango && scUnidad.CasillaPosicion.posX != 1)
         {
            continue;
         }

         Casilla candidata = ObtenerCasillaVecina(lado, direccion);
         if (candidata == null || !PuedeVisitarCasillaEnTurno(candidata))
         {
            continue;
         }

         if (candidata.Presente == null)
         {
            return true;
         }

         if (direccion.x == 0
            && scUnidad.EsEnemigoParaJugador()
            && ObtenerAliadoIntercambiableParaIntercambioLateral(candidata) != null)
         {
            return true;
         }
      }

      return false;
   }

   private List<Obstaculo> ObtenerObstaculosAdyacentesOrtogonales()
   {
      List<Obstaculo> obstaculos = new List<Obstaculo>();
      if (scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return obstaculos;
      }

      LadoManager lado = ObtenerLadoActual();
      if (lado == null)
      {
         return obstaculos;
      }

      foreach (Vector2Int direccion in DireccionesOrtogonales)
      {
         Casilla casillaVecina = ObtenerCasillaVecina(lado, direccion);
         if (casillaVecina == null || casillaVecina.Presente == null)
         {
            continue;
         }

         Obstaculo obstaculo = casillaVecina.Presente.GetComponent<Obstaculo>();
         if (obstaculo == null)
         {
            continue;
         }

         if (obstaculo.CasillaPosicion == null || obstaculo.CasillaPosicion.lado != scUnidad.CasillaPosicion.lado)
         {
            continue;
         }

         if (!obstaculo.destruiblePorMismoLado)
         {
            continue;
         }

         obstaculos.Add(obstaculo);
      }

      return obstaculos;
   }

   private LadoManager ObtenerLadoActual()
   {
      if (scUnidad == null || scUnidad.CasillaPosicion == null || scUnidad.CasillaPosicion.ladoGO == null)
      {
         return null;
      }

      return scUnidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>();
   }

   private Casilla ObtenerCasillaVecina(LadoManager lado, Vector2Int direccion)
   {
      if (lado == null || scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return null;
      }

      int nx = scUnidad.CasillaPosicion.posX + direccion.x;
      int ny = scUnidad.CasillaPosicion.posY + direccion.y;
      if (nx < 1 || nx > 3 || ny < 1 || ny > 5)
      {
         return null;
      }

      return lado.ObtenerCasillaPorIndex(nx, ny);
   }

   private static Obstaculo ObtenerObstaculoDestruiblePreferido(List<Obstaculo> obstaculos)
   {
      if (obstaculos == null || obstaculos.Count == 0)
      {
         return null;
      }

      return obstaculos
         .Where(obstaculo => obstaculo != null && obstaculo.destruiblePorMismoLado)
         .OrderByDescending(obstaculo => obstaculo.CasillaPosicion != null ? obstaculo.CasillaPosicion.posX : int.MinValue)
         .FirstOrDefault();
   }

   private string TraducirTexto(string texto)
   {
      if (TRADU.i != null)
      {
         return TRADU.i.Traducir(texto);
      }

      return texto;
   }

   private string NombreUnidadParaLogLocal(Unidad unidad)
   {
      if (unidad == null)
      {
         return string.Empty;
      }

      return TRADU.i != null ? TRADU.i.Traducir(unidad.uNombre) : unidad.uNombre;
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
   
   async Task<bool> ChequearCasillasAlrededorParaMover(Casilla casilla, AITurnTimings timings)
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
          return false;
       }

       if (await IntentarIntercambioLateralConAliado(casilla))
       {
         return true;
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
         if (candidata == null || candidata.Presente != null || !PuedeVisitarCasillaEnTurno(candidata))
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
         return await MoverACasilla(mejorOpcion);
       }

        await DelayIA(timings.DelayPostAccionMs);
        return false;
    }

   private Casilla BuscarReposicionLateralGenerica(LadoManager lado, Casilla casillaActual)
   {
      if (lado == null || casillaActual == null || scUnidad == null)
      {
         return null;
      }

      int direccionPreferida = ObtenerDireccionFilaHaciaEnemigo(casillaActual);
      if (direccionPreferida == 0)
      {
         return null;
      }

      return ObtenerCasillaLateralDisponible(lado, casillaActual, direccionPreferida);
   }

   private Casilla BuscarIntercambioLateralGenerico(LadoManager lado, Casilla casillaActual)
   {
      if (lado == null || casillaActual == null || scUnidad == null)
      {
         return null;
      }

      if (!scUnidad.EsEnemigoParaJugador() || !HayBloqueoFrontal(lado, casillaActual))
      {
         return null;
      }

      int direccionPreferida = ObtenerDireccionFilaHaciaEnemigo(casillaActual);
      if (direccionPreferida == 0)
      {
         return null;
      }

      return ObtenerCasillaLateralConAliadoIntercambiable(lado, casillaActual, direccionPreferida);
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
      if (candidata == null || candidata.Presente != null || !PuedeVisitarCasillaEnTurno(candidata))
      {
         return null;
      }

      return candidata;
   }

   private Casilla ObtenerCasillaLateralConAliadoIntercambiable(LadoManager lado, Casilla origen, int deltaY)
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
      return PuedeVisitarCasillaEnTurno(candidata) && ObtenerAliadoIntercambiableParaIntercambioLateral(candidata) != null
         ? candidata
         : null;
   }

   private async Task<bool> IntentarIntercambioLateralConAliado(Casilla casillaObjetivo)
   {
      if (casillaObjetivo == null || scUnidad == null || scUnidad.CasillaPosicion == null || !PuedeContinuarTurnoIA())
      {
         return false;
      }

      if (!scUnidad.EsEnemigoParaJugador())
      {
         return false;
      }

      if (scUnidad.estado_inmovil > 0 || scUnidad.esInmobil)
      {
         return false;
      }

      if (casillaObjetivo.posX != scUnidad.CasillaPosicion.posX)
      {
         return false;
      }

      if (Math.Abs(casillaObjetivo.posY - scUnidad.CasillaPosicion.posY) != 1)
      {
         return false;
      }

      if (!PuedeVisitarCasillaEnTurno(casillaObjetivo))
      {
         return false;
      }

      Unidad aliado = ObtenerAliadoIntercambiableParaIntercambioLateral(casillaObjetivo);
      if (aliado == null)
      {
         return false;
      }

      if (scUnidad.ObtenerAPActual() < costoMovimientoAP)
      {
         return false;
      }

      Casilla origen = scUnidad.CasillaPosicion;
      aliado.CasillaDeseadaMov = null;
      aliado.CasillaForzadoaMover = origen;
      scUnidad.CasillaDeseadaMov = casillaObjetivo;
      scUnidad.CambiarAPActual(-costoMovimientoAP);

      await BattleManager.DelayCombateAsync(450);

      if (AbortarTurnoSiUnidadNoPuedeActuar())
      {
         return false;
      }

      if (scUnidad.CasillaDeseadaMov != null)
      {
         scUnidad.CasillaPosicion = scUnidad.CasillaDeseadaMov;
         scUnidad.CasillaDeseadaMov = null;
      }

      RegistrarMovimientoTurno(origen, casillaObjetivo);
      return true;
   }

   private Unidad ObtenerAliadoIntercambiableParaIntercambioLateral(Casilla casillaObjetivo)
   {
      if (casillaObjetivo == null || casillaObjetivo.Presente == null || scUnidad == null || scUnidad.CasillaPosicion == null)
      {
         return null;
      }

      Unidad aliado = casillaObjetivo.Presente.GetComponent<Unidad>();
      if (aliado == null || aliado == scUnidad)
      {
         return null;
      }

      if (aliado.CasillaPosicion == null || aliado.CasillaPosicion.lado != scUnidad.CasillaPosicion.lado)
      {
         return null;
      }

      if (aliado.estado_inmovil > 0 || aliado.esInmobil)
      {
         return null;
      }

      if (aliado.movimientoEnCurso || aliado.CasillaForzadoaMover != null || aliado.CasillaDeseadaMov != null)
      {
         return null;
      }

      return aliado.HP_actual > 0f ? aliado : null;
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

        if (hab is DestruirObstaculo)
        {
          continue;
        }

        if(hab.hActualCooldown > 0){continue;}
        if(hab.costoAP > scUnidad.ObtenerAPActual()+1){continue;} //Si no tiene AP suficiente, no se agrega

        if (!AplicarRestriccionProvocadoAHabilidad(hab, obj))
        {
          continue;
        }
        obj = hab.objPosibles;

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

   private bool AplicarRestriccionProvocadoAHabilidad(IAHabilidad habilidad, List<object> objetivosOriginales)
   {
      if (habilidad == null)
      {
         return false;
      }

      List<object> objetivosBase = objetivosOriginales ?? habilidad.objPosibles;
      if (BattleManager.Instance == null)
      {
         return SincronizarObjetivosHabilidad(habilidad, objetivosBase);
      }

      if (habilidad.ignoraProvocacion)
      {
         return SincronizarObjetivosHabilidad(habilidad, objetivosBase);
      }

      if (!BattleManager.Instance.TryFiltrarObjetivosHostilesPorProvocacion(scUnidad, habilidad.esHostil, objetivosBase, out List<object> objetivosFiltrados))
      {
         habilidad.objPosibles.Clear();
         return false;
      }

      return SincronizarObjetivosHabilidad(habilidad, objetivosFiltrados);
   }

   private static bool SincronizarObjetivosHabilidad(IAHabilidad habilidad, List<object> objetivos)
   {
      if (habilidad == null)
      {
         return false;
      }

      if (ReferenceEquals(habilidad.objPosibles, objetivos))
      {
         return habilidad.objPosibles != null && habilidad.objPosibles.Count > 0;
      }

      habilidad.objPosibles.Clear();
      if (objetivos != null && objetivos.Count > 0)
      {
         habilidad.objPosibles.AddRange(objetivos);
      }

      return habilidad.objPosibles.Count > 0;
   }


   async Task<bool> EjecutarHabilidadConSalvaguarda(List<IAHabilidad> habilidadesDisponibles, AITurnTimings timings)
   {
      if (habilidadesDisponibles == null || habilidadesDisponibles.Count == 0 || !PuedeContinuarTurnoIA() || scUnidad.ObtenerAPActual() <= 0)
      {
         return false;
      }

      List<IAHabilidad> habilidadesOrdenadas = habilidadesDisponibles.OrderByDescending(h => h.prioridad).ToList();

      foreach (IAHabilidad habilidad in habilidadesOrdenadas)
      {
         if (!PuedeContinuarTurnoIA() || scUnidad.ObtenerAPActual() <= 0)
         {
            return false;
         }

         if (habilidad == null)
         {
            continue;
         }

         Task habilidadTask = null;
         try
         {
            HabilidadIAEnEjecucion = habilidad;
            habilidadTask = habilidad.ActivarHabilidad();

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
               if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva != scUnidad)
               {
                  return true;
               }
               await EsperarSecuenciaVisualAsync(habilidad, timings);
               return true;
            }

            Task gracia = await Task.WhenAny(habilidadTask, DelayIA(timings.GraciaHabilidadMs));
            if (gracia == habilidadTask)
            {
               await habilidadTask;
               if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva != scUnidad)
               {
                  return true;
               }
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
         finally
         {
            if (habilidadTask == null || habilidadTask.IsCompleted)
            {
               if (ReferenceEquals(HabilidadIAEnEjecucion, habilidad))
               {
                  HabilidadIAEnEjecucion = null;
               }
            }
            else
            {
               _ = LimpiarHabilidadIAEnEjecucionCuandoFinalizeAsync(habilidad, habilidadTask);
            }
         }
      }

      return false;
   }

   private async Task LimpiarHabilidadIAEnEjecucionCuandoFinalizeAsync(IAHabilidad habilidad, Task habilidadTask)
   {
      if (habilidadTask == null)
      {
         return;
      }

      try
      {
         await habilidadTask;
      }
      catch
      {
      }

      if (ReferenceEquals(HabilidadIAEnEjecucion, habilidad))
      {
         HabilidadIAEnEjecucion = null;
      }
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
      BattleManager.Instance.RestaurarCamaraHabilidad();
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

   public async Task<bool> MoverACasilla(Casilla casillaObjetivo)
    {
        if (casillaObjetivo == null || scUnidad.CasillaPosicion == null || !PuedeContinuarTurnoIA() || !PuedeVisitarCasillaEnTurno(casillaObjetivo))
        {
            return false;
        }

        if (scUnidad.estado_inmovil > 0 || scUnidad.esInmobil)
        {
            return false;
        }

        int deltaX = Math.Abs(casillaObjetivo.posX - scUnidad.CasillaPosicion.posX);
        int deltaY = Math.Abs(casillaObjetivo.posY - scUnidad.CasillaPosicion.posY);

        if (deltaX + deltaY != 1)
        {
            Debug.LogWarning($"{scUnidad.uNombre} intento moverse a una casilla no adyacente.");
            return false;
        }

        if (scUnidad.ObtenerAPActual() < costoMovimientoAP)
        {
            return false;
        }

        Casilla origen = scUnidad.CasillaPosicion;
        scUnidad.CasillaDeseadaMov = casillaObjetivo;
        scUnidad.CambiarAPActual(-costoMovimientoAP);
        // Simulacion de un retraso de movimiento
        await BattleManager.DelayCombateAsync(450);

        if (AbortarTurnoSiUnidadNoPuedeActuar())
        {
            return false;
        }

        if (scUnidad.CasillaDeseadaMov != null)
        {
            scUnidad.CasillaPosicion = scUnidad.CasillaDeseadaMov;
            scUnidad.CasillaDeseadaMov = null;
        }

        bool movimientoCompletado = scUnidad.CasillaPosicion == casillaObjetivo;
        if (movimientoCompletado)
        {
            RegistrarMovimientoTurno(origen, casillaObjetivo);
        }

        return movimientoCompletado;
    }


}
