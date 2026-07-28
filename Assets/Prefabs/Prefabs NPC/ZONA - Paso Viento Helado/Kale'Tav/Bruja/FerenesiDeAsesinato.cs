using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class FerenesiDeAsesinato : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Frenesí del Asesinato";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 8;
      esMelee = false;
      hAlcance = 10;
      hCooldownMax = 4;
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
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
      scEstaUnidad.ReproducirAnimacionAtaque();

      Objetivo = EstablecerObjetivoPrioritario();
                      PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

    
     hActualCooldown = hCooldownMax;

     
    
      await BattleManager.DelayCombateAsync(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(Objetivo);
     
   }

  public GameObject VFXEstadoPrefab;
    public override void AplicarEfectosHabilidad(object obj)
    {
   
     if(obj is Unidad)
     {

     
        Unidad objetivo = (Unidad)obj;

        VFXAplicar(objetivo.gameObject);

       // BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Frenesí del Asesinato";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDanioPorcentaje += 15;
       buff.cantAtaque += 2;
       buff.cantDefensa -= 2;
       buff.cantAPMax += 1;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
   
      
     }
         
    }
   void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_FrenesiAsesinato");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
    



  public override object EstablecerObjetivoPrioritario()
  {
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;

    // Filtrar, ordenar y seleccionar el objetivo prioritario
    var random = new System.Random();
    var objetivo = objPosibles.OfType<Unidad>()
      .Where(u => !u.TieneBuffNombre("Frenesí del Asesinato")) // Filtrar unidades que no tengan el buff
      .OrderBy(u => random.Next()) // Ordenar aleatoriamente
      .FirstOrDefault(); // Tomar el primero después del ordenamiento

    return objetivo;
  }



}

  
 
  


