using System.Collections.Generic;
using UnityEngine;

public class Escapar : Habilidad
{
  private const int CostoEscapeBase = 4;
  private const int CostoEscapeFugaz = 1;

  public override void Awake()
  {
    nombre = "Escapar";
    costoAP = ObtenerCostoAPSegunTrait();
    costoPM = 0;
    cooldownMax = 0;
    cooldownActual = 0;
    Usuario = gameObject;
    scEstaUnidad = GetComponent<Unidad>();
    esZonal = false;
    enArea = 0;
    esforzable = 0;
    esCargable = false;
    esMelee = false;
    esHostil = false;
    bAfectaObstaculos = false;
    omitirAnimacionDeUso = true;

    imHab = Resources.Load<Sprite>("imHab/Escaparicono");
    if (imHab == null)
    {
      imHab = Resources.Load<Sprite>("imHab/DestruirObstaculo");
    }

    ActualizarDescripcion();
  }

  public override void Activar()
  {
    if (scEstaUnidad == null)
    {
      return;
    }

    costoAP = ObtenerCostoAPSegunTrait();
    ActualizarDescripcion();

    if (scEstaUnidad.CasillaPosicion == null || scEstaUnidad.CasillaPosicion.GetComponent<TrampaEscape>() == null)
    {
      BattleManager.Instance?.EscribirLog(TRADU.i != null
        ? TRADU.i.Traducir("Solo puedes escapar desde una Via de Escape.")
        : "Solo puedes escapar desde una Via de Escape.");
      return;
    }

    if (scEstaUnidad.CasillaPosicion.lado != 2)
    {
      return;
    }

    if (!tieneAPSuficientes(out _))
    {
      return;
    }

    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null)
    {
      return;
    }

    battleManager.LimpiarCapasCasillas();
    LimpiarMarcasUnidadesPosibles();

    if (scEstaUnidad.CasillaPosicion != null)
    {
      scEstaUnidad.CasillaPosicion.ActivarCapaColorAzul();
    }

    battleManager.HabilidadActiva = this;
    battleManager.SeleccionandoObjetivo = true;
    battleManager.lUnidadesPosiblesHabilidadActiva.Clear();
    battleManager.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
    battleManager.ActualizarVisibilidadBarrasVidaObjetivos();
    battleManager.lObstaculosPosiblesHabilidadActiva.Clear();
    SincronizarMarcasUnidadesPosibles();
  }

  public override void ActualizarDescripcion()
  {
    int costoActual = ObtenerCostoAPSegunTrait();
    costoAP = costoActual;
    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool enPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    if (enIngles)
    {
      txtDescripcion = "Spend " + costoActual + " AP to flee the battle in shame.";
      return;
    }

    if (enPortugues)
    {
      txtDescripcion = "Gasta " + costoActual + " AP para fugir da batalha envergonhado.";
      return;
    }

    txtDescripcion = "Gasta " + costoActual + " AP para huir avergonzado de la batalla.";
  }

  public override void AplicarEfectosHabilidad(object objetivo, int tirada, Casilla casillaOrigenTrampa)
  {
    if (scEstaUnidad == null)
    {
      return;
    }

    BattleManager battleManager = BattleManager.Instance;
    int ladoQueEscapa = scEstaUnidad.CasillaPosicion != null ? scEstaUnidad.CasillaPosicion.lado : 0;
    bool eraUnidadActiva = battleManager != null && battleManager.unidadActiva == scEstaUnidad;
    if (!scEstaUnidad.RetirarseDeBatallaPorMoral())
    {
      return;
    }

    AplicarPenalizacionValentiaAliadosPorEscape(battleManager, ladoQueEscapa);

    if (ladoQueEscapa == 2)
    {
      BanterBattleDirector.NotificarEscapeAliado(scEstaUnidad);
    }

    AdministradorEscenas admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
    admin?.MarcarAvergonzadoDesdeUnidad(scEstaUnidad);

    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool enPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    string nombreUnidad = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
    string mensaje = enIngles
      ? nombreUnidad + " flees the battle in shame."
      : enPortugues
        ? nombreUnidad + " foge da batalha envergonhado."
        : nombreUnidad + " huye avergonzado de la batalla.";
    battleManager?.EscribirLog(CombatLogFormatter.EventoValour(mensaje));

    if (eraUnidadActiva)
    {
      AvanzarCombateTrasEscapeActivo(battleManager);
    }
  }

  void AvanzarCombateTrasEscapeActivo(BattleManager battleManager)
  {
    if (battleManager == null)
    {
      return;
    }

    bool derrotaSinRefuerzos = battleManager.ladoB != null
      && battleManager.ladoB.unidadesLado.Count < 1
      && battleManager.aliadosRefuerzos.Count < 1;
    bool victoriaSinRefuerzos = battleManager.ladoA != null
      && battleManager.ladoA.unidadesLado.Count < 1
      && battleManager.enemigosRefuerzos.Count < 1;
    if (derrotaSinRefuerzos || victoriaSinRefuerzos)
    {
      return;
    }

    if (battleManager.indexTurno >= 0 && battleManager.indexTurno < battleManager.lUnidadesTotal.Count)
    {
      battleManager.unidadActiva = battleManager.lUnidadesTotal[battleManager.indexTurno];
      battleManager.ArrancarTurno();
    }
    else
    {
      battleManager.RondaNueva();
    }
  }

  void AplicarPenalizacionValentiaAliadosPorEscape(BattleManager battleManager, int ladoQueEscapa)
  {
    if (battleManager == null || ladoQueEscapa != 2 || battleManager.ladoB == null || battleManager.ladoB.unidadesLado == null)
    {
      return;
    }

    List<Unidad> aliados = battleManager.ladoB.unidadesLado;
    if (aliados.Count < 1)
    {
      return;
    }

    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool enPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
    string nombreEscapa = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;

    foreach (Unidad aliado in new List<Unidad>(aliados))
    {
      if (aliado == null || aliado.HP_actual <= 0 || !aliado.gameObject.activeInHierarchy)
      {
        continue;
      }

      string nombreAliado = TRADU.i != null ? TRADU.i.Traducir(aliado.uNombre) : aliado.uNombre;
      string motivo = enIngles
        ? nombreAliado + " loses morale after " + nombreEscapa + " flees"
        : enPortugues
          ? nombreAliado + " perde animo quando " + nombreEscapa + " foge"
          : nombreAliado + " pierde animo al ver huir a " + nombreEscapa;
      aliado.SumarValentia(-1, motivo, mostrarTextoFlotante: false);
    }
  }

  int ObtenerCostoAPSegunTrait()
  {
    if (scEstaUnidad == null || CampaignManager.Instance == null || CampaignManager.Instance.scAdministradorEscenas == null)
    {
      return CostoEscapeBase;
    }

    Personaje personaje = CampaignManager.Instance.scAdministradorEscenas.ObtenerPersonajeAliadoSeleccionadoPorUnidad(scEstaUnidad);
    if (personaje != null && personaje.TieneRasgo(PersonajeTraitCatalog.TraitFugaz))
    {
      return CostoEscapeFugaz;
    }

    return CostoEscapeBase;
  }
}
