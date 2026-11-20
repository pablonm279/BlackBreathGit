using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Ataque de Cuervo: el cuervo de la bruja vuela hasta cualquier enemigo (alcance total), lo pica y puede cegarlo.
/// Como habilidad activa es barata y de poco daño (rellena los turnos en que sus otras habilidades estan en cooldown);
/// la reaccion ReaccionAtaqueCuervo reutiliza los mismos efectos con su dano original.
/// </summary>
public class IAAtaqueCuervo : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private const string NombreCiego = "Ciego";
    private const int TipoSalvacionReflejos = 2;
    private const int DificultadReflejos = 13;

    // Dano segun el origen: la reaccion mantiene su valor original; el ataque activo es mas suave.
    private const int DanioReaccionMin = 4;
    private const int DanioReaccionMax = 12;
    private const int DanioActivoMin = 2;
    private const int DanioActivoMax = 6;

  void Awake()
   {
      nombre = "Ataque de Cuervo";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 10; // alcance total: cualquier fila
      esMelee = false;
      hAlcance = 10; // alcance total: cualquier columna
      hCooldownMax = 2;
      esHostil = true;
      prioridad = 5;
      costoAP = 1;
      omitirAnimacionDeUso = true; // sin animacion de ataque (vaiven)
      omitirFocoCamara = true;     // como la reaccion: la camara no viaja hasta el enemigo
      afectaObstaculos = false;

      hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1); // arranque aleatorio

      bonusAtaque = 0;
      XdDanio = 0;
      daniodX = 0;
      tipoDanio = 0;
   }

    void Start()
    {
      prioridad = 5; // por debajo de Frenesi (15) y Descarga (10): rellena los turnos libres
    }

  object Objetivo;
  public async override Task ActivarHabilidad()
  {
    Objetivo = EstablecerObjetivoPrioritario();
    Unidad objetivoUnidad = Objetivo as Unidad;
    if (objetivoUnidad == null)
    {
      hActualCooldown = 1; // sin objetivo valido: no gasta AP
      return;
    }

    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(null, Objetivo);//Despues de establecer objetivo

    hActualCooldown = hCooldownMax;
    EnemigoUnidadBrujaKaleTav scEstaBruja = scEstaUnidad.GetComponent<EnemigoUnidadBrujaKaleTav>();

    if (scEstaBruja != null)
    {
      // Misma secuencia legible que la reaccion, en version corta para entrar en el margen de tiempo del IA.
      await scEstaBruja.AnimarAtaqueCuervoAsync(objetivoUnidad, () => AplicarEfectosCuervo(objetivoUnidad, true), true);
      return;
    }

    await BattleManager.DelayCombateAsync(900);
    AplicarEfectosCuervo(objetivoUnidad, true);
    await BattleManager.DelayCombateAsync(500);
  }

  public GameObject VFXEstadoPrefab;

    // Usado por la reaccion (dano original).
    public override void AplicarEfectosHabilidad(object obj)
    {
      if (obj is Unidad objetivo)
      {
        AplicarEfectosCuervo(objetivo, false);
      }
    }

    void AplicarEfectosCuervo(Unidad objetivo, bool ataqueActivo)
    {
      if (objetivo == null || objetivo.HP_actual <= 0f)
      {
        return;
      }

      VFXAplicar(objetivo.gameObject);

      if (objetivo.TiradaSalvacion(TipoSalvacionReflejos, DificultadReflejos)) //TRUE = no se salva
      {
        int danio = ataqueActivo
          ? UnityEngine.Random.Range(DanioActivoMin, DanioActivoMax + 1)
          : UnityEngine.Random.Range(DanioReaccionMin, DanioReaccionMax + 1);
        objetivo.RecibirDanio(danio, 1, false, scEstaUnidad);

        // No se acumula: si ya esta ciego solo recibe el daño.
        if (objetivo.HP_actual > 0f && !objetivo.TieneBuffNombre(NombreCiego))
        {
          /////////////////////////////////////////////
          //BUFF ---- As se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = NombreCiego;
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = 2;
          buff.cantAtaque -= 3;
          buff.cantDefensa -= 2;
          buff.cantTsReflejos -= 1;
          buff.AplicarBuff(objetivo);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        }
      }
    }

    void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AtaqueCuervo");
      if (VFXenObjetivo == null) { return; }

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;

   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);

    }

    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
    var unidades = objPosibles.OfType<Unidad>().Where(u => u != null && u.HP_actual > 0f).ToList();
    if (unidades.Count == 0)
    {
      return null;
    }

    // Prefiere a quien todavia ve (no ciego); entre ellos, al azar.
    var sinCiego = unidades.Where(u => !u.TieneBuffNombre(NombreCiego)).ToList();
    var candidatas = sinCiego.Count > 0 ? sinCiego : unidades;
    return candidatas[UnityEngine.Random.Range(0, candidatas.Count)];
   }

}
