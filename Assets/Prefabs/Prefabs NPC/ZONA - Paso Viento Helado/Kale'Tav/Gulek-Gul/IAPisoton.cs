using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Gulek-Gul golpea el suelo y sacude la primera fila enemiga.
/// Tirada de Reflejos: quien falla recibe el daño completo y pierde 1 AP en su proximo turno;
/// quien la supera recibe la mitad. Cooldown largo.
/// </summary>
public class IAPisoton : IAHabilidad
{
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //3: Contundente
  [SerializeField] private Sprite posePisoton;

  private const int TipoSalvacionReflejos = 2;

  void Awake()
  {
    nombre = "Pisotón";
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 5; // zonal: la camara encuadra toda la fila
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 5;
    esHostil = true;
    prioridad = 12;
    costoAP = 2;
    afectaObstaculos = false;
    ignoraProvocacion = true;
    omitirAnimacionDeUso = true;

    hActualCooldown = 2;

    XdDanio = 2;
    daniodX = 10;
    tipoDanio = 3;
  }

  void Start()
  {
    prioridad = 12;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    IAUnidad ia = GetComponent<IAUnidad>();
    foreach (Unidad u in BattleManager.Instance.lUnidadesTotal)
    {
      if (u == null || u.HP_actual <= 0f || u.CasillaPosicion == null || u.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      if (u.CasillaPosicion.posX != 3) // solo primera fila
      {
        continue;
      }

      if (u.ObtenerEstaEscondido() > 0 && (ia == null || !ia.bPuedeVerEscondidos))
      {
        continue;
      }

      objPosibles.Add(u);
    }

    return objPosibles;
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return objPosibles.OfType<Unidad>().FirstOrDefault();
  }

  public async override Task ActivarHabilidad()
  {
    ListaHayObjetivosAlAlcance();
    List<Unidad> objetivos = objPosibles.OfType<Unidad>().ToList();
    if (objetivos.Count == 0)
    {
      hActualCooldown = 1;
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(objetivos.Cast<object>().ToList(), null);

    float factorTiempo = EnemigoGulekGul.FactorTiempoHabilidad();
    EnemigoGulekGul gulek = scEstaUnidad as EnemigoGulekGul;
    int versionPose = gulek != null ? gulek.MostrarPoseTemporal(posePisoton) : 0;
    try
    {
      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(500 * factorTiempo)); // alza el pie

      if (!PuedeResolverAccionIA())
      {
        return;
      }

      // Centro de la onda: promedio de las posiciones de la primera fila.
      Vector3 centro = Vector3.zero;
      int presentes = 0;
      foreach (Unidad u in objetivos)
      {
        if (u == null) { continue; }
        centro += u.transform.position;
        presentes++;
      }
      if (presentes == 0) { return; }
      centro /= presentes;

      GulekGulVfx.OndaDeChoque(scEstaUnidad.transform.position, 0.5f, new Color(0.95f, 0.85f, 0.62f, 0.35f), 0.5f, 2);
      GulekGulVfx.PolvoYEscombros(scEstaUnidad.transform.position, 1.4f, 1.4f);
      GulekGulVfx.SacudirCamara(1f, 0.45f);
      ScreenFlash.FlashImpact(new Color(1f, 0.9f, 0.7f), 0.05f, 0.012f, 0.1f);

      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(260 * factorTiempo)); // la onda alcanza la primera fila
      if (!PuedeResolverAccionIA())
      {
        return;
      }

      GulekGulVfx.OndaDeChoque(centro, 0.7f, new Color(0.95f, 0.8f, 0.55f, 0.32f), 0.5f, 2);
      AplicarEfectosHabilidad(objetivos);
    }
    finally
    {
      if (gulek != null) { gulek.RestaurarPoseTemporal(versionPose); }
    }

    await BattleManager.DelayCombateAsync(Mathf.RoundToInt(350 * factorTiempo));
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    List<Unidad> objetivos = obj as List<Unidad>;
    if (objetivos == null)
    {
      Unidad unica = obj as Unidad;
      if (unica == null) { return; }
      objetivos = new List<Unidad> { unica };
    }

    float dificultad = 10f + scEstaUnidad.mod_CarFuerza / 2f;

    foreach (Unidad objetivo in objetivos)
    {
      if (objetivo == null || objetivo.HP_actual <= 0f)
      {
        continue;
      }

      bool falla = objetivo.TiradaSalvacion(TipoSalvacionReflejos, dificultad);

      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
      if (!falla)
      {
        danio /= 2f;
      }

      GulekGulVfx.PolvoYEscombros(objetivo.transform.position, falla ? 1.1f : 0.6f, 0.7f);
      objetivo.RecibirDanio(danio + 2, tipoDanio, false, scEstaUnidad);

      if (falla && objetivo.HP_actual > 0f)
      {
        // Tambaleo: 1 AP menos en su proximo turno (la duracion corre al terminar SU turno).
        Buff buff = new Buff();
        buff.buffNombre = "Tambaleante";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 1;
        buff.cantAPMax = -1;
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
      }
    }
  }
}
