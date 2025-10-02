using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class Perdicion : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Perdicion";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 3;
      hCooldownMax = 3;
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
     
      scEstaUnidad.ReproducirAnimacionAtaque();

      Objetivo = EstablecerObjetivoPrioritario();
                PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

    
     hActualCooldown = hCooldownMax;

     
    
      await Task.Delay(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(Objetivo);
     
   }

  public GameObject VFXEstadoPrefab;
    public override void AplicarEfectosHabilidad(object obj)
    {
   
     if(obj is Unidad)
     {

     
        Unidad objetivo = (Unidad)obj;
     
         if(objetivo.TiradaSalvacion(objetivo.mod_TSMental, 13))
          {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Perdición";
            buff.boolfDebufftBuff = false;
            buff.DuracionBuffRondas = 3;
            buff.cantAPMax -= 1;
            buff.cantAtaque -= 2;
            buff.cantTsMental -= 2;
            buff.cantResNec= -10;
            buff.AplicarBuff(objetivo);

            //Aplica VFX del estado
            GameObject goVFX = Instantiate(VFXEstadoPrefab, objetivo.transform.position, objetivo.transform.rotation);
            goVFX.transform.parent = objetivo.transform;
            buff.goVFX =  goVFX;  
            //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
            Canvas canvasObjeto = goVFX.GetComponentInChildren<Canvas>();
            canvasObjeto.overrideSorting = true;
            canvasObjeto.sortingOrder =  objetivo.GetComponentInChildren<Canvas>().sortingOrder + 3; 
            //---
            


            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

          

          }
         

            
   

   
     
   
      
     }
         
    }

   
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
    
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;
  
    var unidades = objPosibles.OfType<Unidad>().ToList();
  
  // Remover las unidades inmóviles recorriendo de atrás hacia adelante
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
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la ultima (la mas cercana)
    if (unidadesOrdenadas.Any())
    {
      return unidadesOrdenadas.LastOrDefault();
    }

   
     return null;
   }



}

  
 
  