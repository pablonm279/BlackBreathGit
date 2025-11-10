using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class IAEnterrarRaizFuego : IAHabilidad
{
  const int TurnosOculta = 2;
  const int CuracionMin = 10;
  const int CuracionMaxExclusive = 26;
  const float BonusDanioPorcentaje = 15f;

  static readonly Dictionary<int, HashSet<Unidad>> unidadesEnterradasPorLado = new Dictionary<int, HashSet<Unidad>>
  {
    { 1, new HashSet<Unidad>() },
    { 2, new HashSet<Unidad>() }
  };

  static readonly Dictionary<Unidad, int> ladoPorUnidad = new Dictionary<Unidad, int>();

  Buff buffEnterradoActivo;
  bool esperandoEmerger;
  bool eventoSuscripto;
  bool imagenOculta;
  int turnosOcultaRestantes;
  int ladoOriginal;
  Casilla casillaOrigenAlEnterrar;

  void Awake()
  {
    nombre = "Enterrarse";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 5;
    esHostil = false;
    prioridad = 15;
    costoAP = 4;
    afectaObstaculos = false;

    hActualCooldown = 3;
  }

  void OnEnable()
  {
    if (esperandoEmerger)
    {
      SuscribirEventos();
    }
    else
    {
      RemoverUnidadEnterrada(scEstaUnidad);
    }
  }

  void OnDisable()
  {
    DesuscribirEventos();
    RemoverUnidadEnterrada(scEstaUnidad);
  }

  void OnDestroy()
  {
    DesuscribirEventos();
    RemoverUnidadEnterrada(scEstaUnidad);
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    return new List<object> { scEstaUnidad };
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }
  public AudioClip enterrarSFX;
  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = Usuario.GetComponent<Unidad>();
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, scEstaUnidad);
    AudioSource  audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.PlayOneShot(enterrarSFX);

    await Task.Delay(350);

    AplicarEfectosHabilidad(scEstaUnidad);


    await Task.Delay(250);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad unidad) || esperandoEmerger)
    {
      return;
    }

    EnterrarUnidad(unidad);
  }

  void EnterrarUnidad(Unidad unidad)
  {
    casillaOrigenAlEnterrar = unidad.CasillaPosicion;
    ladoOriginal = casillaOrigenAlEnterrar != null ? casillaOrigenAlEnterrar.lado : 1;

    if (casillaOrigenAlEnterrar != null && casillaOrigenAlEnterrar.Presente == unidad.gameObject)
    {
      casillaOrigenAlEnterrar.Presente = null;
    }

    if (unidad.uImage != null && unidad.uImage.enabled)
    {
      unidad.uImage.enabled = false;
      imagenOculta = true;
      unidad.uImage.gameObject.transform.parent.gameObject.SetActive(false);
    }

    unidad.estado_invulnerable = Mathf.Max(unidad.estado_invulnerable, TurnosOculta);
    unidad.estado_aturdido = Mathf.Max(unidad.estado_aturdido, 1);

    if (!unidad.TieneBuffNombre("Enterrado"))
    {
      Buff buff = new Buff
      {
        buffNombre = "Enterrado",
        buffDescr = "",
        boolfDebufftBuff = true,
        suprimeTextoFlotante = true,
        DuracionBuffRondas = -1,
        esStackeable = false,
        esRemovible = false
      };

      buff.AplicarBuff(unidad);
      buffEnterradoActivo = ComponentCopier.CopyComponent(buff, unidad.gameObject);
      buffEnterradoActivo.esRemovible = false;
    }

    string textoEnterrar = TRADU.i.Traducir("se entierra y desaparece del campo.");
    BattleManager.Instance?.EscribirLog($"{unidad.uNombre} {textoEnterrar}");

    turnosOcultaRestantes = TurnosOculta;
    esperandoEmerger = true;
    RegistrarUnidadEnterrada(unidad, ladoOriginal);
    SuscribirEventos();

    BattleManager.Instance?.scUIInfoChar.ActualizarInfoChar(unidad);
    BattleManager.Instance?.scUIBarraOrdenTurno?.ActualizarBarraOrdenTurno();
  }

  void SuscribirEventos()
  {
    if (eventoSuscripto || BattleManager.Instance == null)
    {
      return;
    }

    BattleManager.Instance.OnTurnoNuevo += BattleManager_OnTurnoNuevo;
    eventoSuscripto = true;
  }

  void DesuscribirEventos()
  {
    if (!eventoSuscripto || BattleManager.Instance == null)
    {
      return;
    }

    BattleManager.Instance.OnTurnoNuevo -= BattleManager_OnTurnoNuevo;
    eventoSuscripto = false;
  }

  void BattleManager_OnTurnoNuevo(object sender, EventArgs e)
  {
    if (!esperandoEmerger || BattleManager.Instance == null)
    {
      return;
    }

    if (BattleManager.Instance.unidadActiva != scEstaUnidad)
    {
      return;
    }

    turnosOcultaRestantes--;

    if (turnosOcultaRestantes > 0)
    {
      return;
    }

    Emerger();
  }

  void Emerger()
  {
    esperandoEmerger = false;
    DesuscribirEventos();
    RemoverUnidadEnterrada(scEstaUnidad);

     AudioSource  audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.PlayOneShot(enterrarSFX);

    if (buffEnterradoActivo != null)
    {
      buffEnterradoActivo.esRemovible = true;
      buffEnterradoActivo.RemoverBuff(scEstaUnidad);
      buffEnterradoActivo = null;
    }
    else
    {
      Buff existente = scEstaUnidad.GetComponents<Buff>().FirstOrDefault(b => b.buffNombre == "Enterrado");
      if (existente != null)
      {
        existente.esRemovible = true;
        existente.RemoverBuff(scEstaUnidad);
      }
    }

    if (imagenOculta && scEstaUnidad.uImage != null)
    {
      scEstaUnidad.uImage.enabled = true;
      scEstaUnidad.uImage.gameObject.transform.parent.gameObject.SetActive(true);
    }
    imagenOculta = false;

    Casilla nuevaCasilla = ElegirCasillaReaparicion();
    if (nuevaCasilla != null)
    {
      scEstaUnidad.transform.position = nuevaCasilla.transform.position;
      scEstaUnidad.CasillaPosicion = nuevaCasilla;
      nuevaCasilla.NuevoObjetoPresenteEnCasilla(scEstaUnidad.gameObject);
    }
    else if (casillaOrigenAlEnterrar != null)
    {
      casillaOrigenAlEnterrar.NuevoObjetoPresenteEnCasilla(scEstaUnidad.gameObject);
      scEstaUnidad.CasillaPosicion = casillaOrigenAlEnterrar;
    }

    int curacion = UnityEngine.Random.Range(CuracionMin, CuracionMaxExclusive);
    scEstaUnidad.RecibirCuracion(curacion, false);

    // BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Emergida";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = 1;
    buff.cantDanioPorcentaje += 15;
    buff.cantAtaque += 15;
    buff.AplicarBuff(scEstaUnidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);

    string textoEmerger = TRADU.i.Traducir("emerge de vuelta.");
    BattleManager.Instance?.EscribirLog($"{scEstaUnidad.uNombre} {textoEmerger}");

    BattleManager.Instance?.scUIInfoChar.ActualizarInfoChar(scEstaUnidad);
    BattleManager.Instance?.CalcularCasillasAMovimiento();
    BattleManager.Instance?.scUIBarraOrdenTurno?.ActualizarBarraOrdenTurno();
  }

  Casilla ElegirCasillaReaparicion()
  {
    if (BattleManager.Instance == null)
    {
      return casillaOrigenAlEnterrar;
    }

    List<Casilla> candidatas = BattleManager.Instance.lCasillasTotal
      .Where(c => c.lado == ladoOriginal && c.Presente == null)
      .ToList();

    if (candidatas.Count == 0)
    {
      return casillaOrigenAlEnterrar != null && casillaOrigenAlEnterrar.Presente == null
        ? casillaOrigenAlEnterrar
        : scEstaUnidad.CasillaPosicion;
    }

    int indice = UnityEngine.Random.Range(0, candidatas.Count);
    return candidatas[indice];
  }

  static void RegistrarUnidadEnterrada(Unidad unidad, int lado)
  {
    if (unidad == null)
    {
      return;
    }

    if (!unidadesEnterradasPorLado.TryGetValue(lado, out HashSet<Unidad> conjunto))
    {
      conjunto = new HashSet<Unidad>();
      unidadesEnterradasPorLado[lado] = conjunto;
    }

    conjunto.Add(unidad);
    ladoPorUnidad[unidad] = lado;
  }

  static void RemoverUnidadEnterrada(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    if (!ladoPorUnidad.TryGetValue(unidad, out int lado))
    {
      return;
    }

    if (unidadesEnterradasPorLado.TryGetValue(lado, out HashSet<Unidad> conjunto))
    {
      conjunto.Remove(unidad);
    }

    ladoPorUnidad.Remove(unidad);
  }

  public static IEnumerable<Unidad> ObtenerUnidadesEnterradas(int lado)
  {
    if (!unidadesEnterradasPorLado.TryGetValue(lado, out HashSet<Unidad> conjunto))
    {
      yield break;
    }

    foreach (Unidad unidad in conjunto)
    {
      if (unidad != null)
      {
        yield return unidad;
      }
    }
  }
}
