using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAAlimaniaProliferar : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Proliferar Corrupción";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 2;
      esMelee = false;
      hAlcance = 6;
      hCooldownMax = 3;
      esHostil = false;
      prioridad = 4;
      costoAP = 4;
      afectaObstaculos = false;
      


      hActualCooldown = UnityEngine.Random.Range(0, 4); //Cooldown aleatorio entre 0 y 2

      


      

    
   }

    void Start()
    {
      prioridad = 4;
    }

   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
      scEstaUnidad.ReproducirAnimacionAtaque();
            PrepararInicioAnimacion(null,scEstaUnidad);//Despues de establecer objetivo


      hActualCooldown = hCooldownMax;
       
    
      await Task.Delay(1000);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(scEstaUnidad);
     
   }

   
    public override void AplicarEfectosHabilidad(object obj)
    {
     CrearCorrsionAlrededor(obj as Unidad);
    }

  void CrearCorrsionAlrededor(Unidad obj)
  {
    Casilla pos = obj.CasillaPosicion;
    List<Casilla> casillas = pos.ObtenerCasillasAlrededor(3);

    if (casillas != null && casillas.Count > 0)
    {
      // Filtrar casillas que no tengan ningún componente de tipo Trampa
      var casillasLibres = casillas
        .Where(c => c.GetComponents<Trampa>().Length == 0)
        .OrderBy(x => UnityEngine.Random.value)
        .Take(3)
        .ToList();

      foreach (var c in casillasLibres)
      {
        c.AddComponent<TrampaMasaContaminada>();
        c.GetComponent<TrampaMasaContaminada>().Inicializar();
      }
    }
  }
  public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
  {
    return this; 

  } 


}

  
 
  