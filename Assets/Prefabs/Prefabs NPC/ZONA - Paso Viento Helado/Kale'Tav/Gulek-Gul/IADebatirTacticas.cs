using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

public class IADebatirTacticas : IAHabilidad
{

  [SerializeField] public int pPrioridad;

  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

  // Tacticas posibles
  private const int TacticaDefensiva = 1;
  private const int TacticaAgresiva = 2;
  private const int TacticaDescanso = 3;
  private const int TacticaFrenetica = 4;

  // Bajo este porcentaje de vida "discuten" descansar en lugar de redoblar el ataque.
  private const float UmbralDescansoPorcentaje = 0.4f;

  private int ultimaTactica = 0;

  void Awake()
  {
    nombre = "Discutir Tácticas";
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 3;
    esHostil = false;
    prioridad = 25;
    costoAP = 0; // accion gratuita: no consume AP (antes figuraba 1 pero nunca se cobraba)
    afectaObstaculos = false;

    hActualCooldown = 0;

    bonusAtaque = 0;
    XdDanio = 0;
    daniodX = 0;
    tipoDanio = 0;
  }

  void Start()
  {
    prioridad = 25;
  }

  object Objetivo;
  public async override Task ActivarHabilidad()
  {

    //scEstaUnidad.ReproducirAnimacionAtaque();

    Objetivo = scEstaUnidad;
    PrepararInicioAnimacion(null, Objetivo);//Despues de establecer objetivo

    await BattleManager.DelayCombateAsync(450);
    hActualCooldown = hCooldownMax;
    VFXAplicar(this.gameObject);

    // El VFX dura ~3 s y sigue solo; la accion se resuelve a mitad de camino y termina dentro del
    // margen de tiempo del IA (HabilidadTimeoutMs + gracia), para no disparar el "tiempo limite".
    await BattleManager.DelayCombateAsync(1000);

    if (!PuedeResolverAccionIA())
    {
      return;
    }

    //Esto es cuando el objetivo es uno solo,
    AplicarEfectosHabilidad(Objetivo);

    await BattleManager.DelayCombateAsync(250);

  }


    void VFXAplicar(GameObject objetivo)
    {
    GameObject  VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_DebatirTacticas");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;

   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);

    }
  public GameObject VFXEstadoPrefab;

  // Elige una tactica al azar sin repetir la anterior ni redundar con una que ya esta activa.
  private int ElegirTactica(Unidad objetivo)
  {
    bool vidaBaja = objetivo.HP_actual < objetivo.mod_maxHP * UmbralDescansoPorcentaje;

    List<int> candidatas = new List<int>();
    if (!objetivo.TieneBuffNombre("Enfoque Defensivo")) { candidatas.Add(TacticaDefensiva); }
    if (!objetivo.TieneBuffNombre("Enfoque Agresivo")) { candidatas.Add(TacticaAgresiva); }

    if (vidaBaja)
    {
      candidatas.Add(TacticaDescanso);
    }
    else if (!objetivo.TieneBuffNombre("Enfoque Frenético"))
    {
      candidatas.Add(TacticaFrenetica);
    }

    if (candidatas.Count > 1)
    {
      candidatas.Remove(ultimaTactica);
    }

    if (candidatas.Count == 0)
    {
      candidatas.Add(TacticaDefensiva);
    }

    int elegida = candidatas[UnityEngine.Random.Range(0, candidatas.Count)];
    ultimaTactica = elegida;
    return elegida;
  }

  public override void AplicarEfectosHabilidad(object obj)
  {

    if (obj is Unidad)
    {

      Unidad objetivo = (Unidad)obj;

      int tactica = ElegirTactica(objetivo);

      if (tactica == TacticaDefensiva)
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Enfoque Defensivo";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 4;
        buff.cantArmadura += 3;
        buff.cantDefensa += 3;
        buff.cantDanioPorcentaje -= 10;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque defensivo."));
      }
      else if (tactica == TacticaAgresiva)
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Enfoque Agresivo";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 4;
        buff.cantCritDado += 2;
        buff.cantAtaque += 3;
        buff.cantDefensa -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque agresivo."));
      }
      else if (tactica == TacticaDescanso)
      {
        // La duracion corre al terminar el turno de la unidad: 2 hace que el efecto cubra su PROXIMO turno
        // (con 1 el buff expiraba antes de refrescar los AP y el descanso no costaba nada).
        Buff buff = new Buff();
        buff.buffNombre = "Descansando";
        buff.boolfDebufftBuff = true;
        buff.cantAPMax = -4;
        buff.DuracionBuffRondas = 2;
        buff.AplicarBuff(objetivo);

        objetivo.RecibirCuracion(75, false);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven descansar para recuperar fuerzas."));
      }
      else
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Enfoque Frenético";
        buff.boolfDebufftBuff = true;
        buff.cantAPMax = 1;
        buff.DuracionBuffRondas = 3;
        buff.cantCritDado += 2;
        buff.cantAtaque += 3;
        buff.cantDefensa -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven atacar sin descanso."));
      }

    }

  }

  public override object EstablecerObjetivoPrioritario()
  {

    return scEstaUnidad;
  }

  public override List<object> ListaHayObjetivosAlAlcance() //necesario para self buffearse
  {
    return new List<object> { scEstaUnidad };

  }

}
