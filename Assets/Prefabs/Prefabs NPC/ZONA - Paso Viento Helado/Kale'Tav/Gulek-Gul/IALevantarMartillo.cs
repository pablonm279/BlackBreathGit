using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IALevantarMartillo : IAHabilidad
{

  [SerializeField] public int pPrioridad;

  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano



  void Awake()
  {
    nombre = "Levantar Martillo";
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 1;
    esHostil = false;
    prioridad = 15;
    costoAP = 3;
    afectaObstaculos = false;



    hActualCooldown = 0;

    bonusAtaque = 0;
    XdDanio = 0;
    daniodX = 0;
    tipoDanio = 0;





  }

  void Start()
  {
    prioridad = 15;
  }

  object Objetivo;
  public async override Task ActivarHabilidad()
  {

    if (scEstaUnidad.TieneBuffNombre("Martillo Listo"))
    {
      hActualCooldown = 1;
      gameObject.GetComponent<IAMartilloPesado>().hActualCooldown = 0;
      return;
    }


    //scEstaUnidad.ReproducirAnimacionAtaque();

    Objetivo = scEstaUnidad;
    PrepararInicioAnimacion(null, Objetivo);//Despues de establecer objetivo


    hActualCooldown = hCooldownMax;



    await BattleManager.DelayCombateAsync(1300);
    //Esto es cuando el objetivo es uno solo,
    AplicarEfectosHabilidad(Objetivo);

    await BattleManager.DelayCombateAsync(1300);
    await BattleManager.DelayCombateAsync(600);
    scEstaUnidad.EstablecerAPActualA(0);
    BattleManager.Instance.TerminarTurno();

  }



  public GameObject VFXEstadoPrefab;
  public override void AplicarEfectosHabilidad(object obj)
  {

    if (obj is Unidad)
    {


      Unidad objetivo = (Unidad)obj;

      // VFXAplicar(objetivo.gameObject);

      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Martillo Listo";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = -1;
      buff.cantAtaque += 2;
      buff.cantDanioPorcentaje += 15;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

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

  
 
  


