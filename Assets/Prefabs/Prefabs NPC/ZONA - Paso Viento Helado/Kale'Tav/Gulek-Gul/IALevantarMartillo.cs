using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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
    costoAP = 2;
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
      return; // ya lo tiene levantado (ListaHayObjetivosAlAlcance ya lo excluye; esto es solo una guarda)
    }

    //scEstaUnidad.ReproducirAnimacionAtaque();

    Objetivo = scEstaUnidad;
    scEstaUnidad.CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(null, Objetivo);//Despues de establecer objetivo

    hActualCooldown = hCooldownMax;

    await BattleManager.DelayCombateAsync(700);

    if (!PuedeResolverAccionIA())
    {
      return;
    }

    //Esto es cuando el objetivo es uno solo,
    AplicarEfectosHabilidad(Objetivo);

    // Efecto visual: el suelo tiembla un poco al alzar el martillo.
    GulekGulVfx.PolvoYEscombros(scEstaUnidad.transform.position, 0.8f, 1.1f);
    GulekGulVfx.OndaDeChoque(scEstaUnidad.transform.position, 0.9f, new Color(0.85f, 0.8f, 0.7f, 0.5f), 0.4f, 1);
    GulekGulVfx.SacudirCamara(0.3f, 0.15f);

    await BattleManager.DelayCombateAsync(500);
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

      EnemigoGulekGul gulek = objetivo as EnemigoGulekGul;
      if (gulek != null) { gulek.RefrescarPoseMartillo(); }

    }
   
  }




  public override object EstablecerObjetivoPrioritario()
  {

    return scEstaUnidad;
  }

  public override List<object> ListaHayObjetivosAlAlcance() //necesario para self buffearse
  {
    if (scEstaUnidad.TieneBuffNombre("Martillo Listo"))
    {
      return new List<object>(); // ya tiene el martillo levantado: no es una accion posible
    }

    return new List<object> { scEstaUnidad };

  }



}

  
 
  


