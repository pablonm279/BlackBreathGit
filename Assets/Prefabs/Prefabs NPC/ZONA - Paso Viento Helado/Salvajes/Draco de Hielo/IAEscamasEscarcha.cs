using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Escamas de Escarcha: el draco se cubre de hielo cuando esta herido (menos del 75% de vida).
/// +4 armadura y +2 defensa durante 3 turnos. Solo se usa si no esta activa.
/// </summary>
public class IAEscamasEscarcha : IAHabilidad
{
  private const string NombreBuff = "Escamas de Escarcha";
  private const float UmbralVida = 0.75f;

  void Awake()
  {
    nombre = "Escamas de Escarcha";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 5;
    esHostil = false;
    prioridad = 28; // justo por debajo del Aliento (30): primero respira, despues se protege
    costoAP = 1;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 28;
  }

  public override List<object> ListaHayObjetivosAlAlcance() // habilidad sobre si mismo
  {
    if (scEstaUnidad == null
      || scEstaUnidad.TieneBuffNombre(NombreBuff)
      || scEstaUnidad.HP_actual > scEstaUnidad.mod_maxHP * UmbralVida)
    {
      return new List<object>();
    }

    return new List<object> { scEstaUnidad };
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }

  public override async Task ActivarHabilidad()
  {
    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, scEstaUnidad);

    float f = DracoHieloVfx.FactorTiempo();
    await BattleManager.DelayCombateAsync(Mathf.RoundToInt(350 * f));

    if (!PuedeResolverAccionIA())
    {
      return;
    }

    DracoHieloVfx.AuraEscamas(scEstaUnidad.transform, 2.2f * f);
    AplicarEfectosHabilidad(scEstaUnidad);

    await BattleManager.DelayCombateAsync(Mathf.RoundToInt(500 * f));
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    Unidad objetivo = obj as Unidad;
    if (objetivo == null || objetivo.TieneBuffNombre(NombreBuff))
    {
      return;
    }

    // La duracion corre al terminar el turno del draco: 3 cubre este turno y los dos siguientes.
    Buff buff = new Buff();
    buff.buffNombre = NombreBuff;
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = 3;
    buff.cantArmadura += 4;
    buff.cantDefensa += 2;
    buff.AplicarBuff(objetivo);
    ComponentCopier.CopyComponent(buff, objetivo.gameObject);
  }
}
