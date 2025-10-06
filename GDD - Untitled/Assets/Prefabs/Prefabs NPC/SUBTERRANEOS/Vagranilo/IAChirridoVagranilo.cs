using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAChirridoVagranilo : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Chirrido de Vagranilo";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 3;
      hCooldownMax = 3;
      esHostil = true;
      prioridad = 5;
      costoAP = 1;
      afectaObstaculos = false;
      


      hActualCooldown = UnityEngine.Random.Range(0, 3);

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
     
         if(objetivo.TiradaSalvacion(objetivo.mod_TSMental, 11))
          {
            //Si falla, se aturde y hace 2d4 daño verdadero
            objetivo.estado_aturdido = 1;
            objetivo.RecibirDanio(TiradaDeDados.TirarDados(2,4), 10, false,scEstaUnidad);

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

  
 
  