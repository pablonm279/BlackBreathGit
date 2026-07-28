using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IADebatirTacticas : IAHabilidad
{

  [SerializeField] public int pPrioridad;

  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano



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
    costoAP = 1;
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



    await BattleManager.DelayCombateAsync(3000);
    //Esto es cuando el objetivo es uno solo,
    AplicarEfectosHabilidad(Objetivo);

    await BattleManager.DelayCombateAsync(2300);
    await BattleManager.DelayCombateAsync(600);

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
  public override void AplicarEfectosHabilidad(object obj)
  {

    if (obj is Unidad)
    {


      Unidad objetivo = (Unidad)obj;
     
      int tactica = UnityEngine.Random.Range(1, 4); // 1 a 3

      if (tactica == 1)
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
        BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque defensivo."));
      }
      else if (tactica == 2)
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
        BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque agresivo."));
      }
      else if (tactica == 3)
      {


        if (objetivo.GetComponent<Unidad>().HP_actual < 150)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Descansando";
          buff.boolfDebufftBuff = true;
          buff.cantAPMax = -4;
          buff.DuracionBuffRondas = 1;
          buff.AplicarBuff(objetivo);

          objetivo.GetComponent<Unidad>().RecibirCuracion(75, false);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
          BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven descansar para recuperar fuerzas."));
        }
        else
        { 
            // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Enfoque Agresivo";
        buff.boolfDebufftBuff = true;
        buff.cantAPMax = 1;
        buff.DuracionBuffRondas = 3;
        buff.cantCritDado += 2;
        buff.cantAtaque += 3;
        buff.cantDefensa -= 1;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir("Gulek y Gul discuten tácticas, y resuelven adoptar un enfoque agresivo."));
 




        }
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

  
 
  


