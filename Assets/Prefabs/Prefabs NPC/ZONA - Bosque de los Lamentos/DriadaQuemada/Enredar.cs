using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class Enredar : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: �cido - 8: Arcano


    
  void Awake()
   {
      nombre = "Enredar";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 4;
      hCooldownMax = 2;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 2;
      afectaObstaculos = true;
      


      hActualCooldown = 0;

      bonusAtaque = 0;
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 0; 


      

    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }

  object Objetivo;
   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
     // scEstaUnidad.ReproducirAnimacionAtaque();

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
     
         if(objetivo.TiradaSalvacion(2, 12)&& objetivo.estado_inmovil < 1)
          {
            /////////////////////////////////////////////
            //BUFF ---- As� se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Enredadera Ardiente";
            buff.buffDescr = "Inm�vil, Melee solo adyacente.";
            buff.boolfDebufftBuff = false;
            buff.DuracionBuffRondas = 2;
            buff.cantAPMax -= 1;
            buff.cantDefensa -= 2;
            buff.AplicarBuff(objetivo);

            //Aplica VFX del estado
            GameObject goVFX = Instantiate(VFXEstadoPrefab, objetivo.transform.position, objetivo.transform.rotation);
            goVFX.transform.parent = objetivo.transform;
            buff.goVFX =  goVFX;
            //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
            Canvas canvasObjeto = goVFX.GetComponentInChildren<Canvas>();
            RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, objetivo.transform, 5);
            //---

            // Agrega el componente Buff al objeto objetivo y asigna la configuraci�n del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

            objetivo.estado_inmovil = buff.DuracionBuffRondas + 1;
            objetivo.estado_ardiendo = buff.DuracionBuffRondas*2;

          }
         

            
   

   
     
   
      
     }
         
    }

   
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun l�gica
   {
    
    // Obtener la unidad duea
    Unidad unidadDuea = gameObject.GetComponent<Unidad>();
    if (unidadDuea == null) return null;
  
    var unidades = objPosibles.OfType<Unidad>().ToList();
  
  // Remover las unidades inmviles recorriendo de atrs hacia adelante
    for (int i = unidades.Count - 1; i >= 0; i--)
    {
      if (unidades[i].estado_inmovil > 0)
      {
          unidades.RemoveAt(i);
      }
    }
    // Ordenar las unidades primero por posX y luego por la diferencia en posY
    var unidadesOrdenadas = unidades
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuea.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la ultima (la mas cercana)
    if (unidadesOrdenadas.Any())
    {
      return unidadesOrdenadas.LastOrDefault();
    }

   
     return null;
   }



}

  
 
  



