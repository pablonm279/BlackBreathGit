using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class CariciadelBosque : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Caricia del Bosque";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 3;
      esMelee = false;
      hAlcance = 6;
      hCooldownMax = 3;
      esHostil = false;
      prioridad = pPrioridad;
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

        VFXAplicar(objetivo.gameObject);

        int cura = 5+UnityEngine.Random.Range(1,20);
        objetivo.RecibirCuracion(cura, true);    
   

   
     
   
      
     }
         
    }
   void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CariciaDelBosque");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }
public override List<object> ListaHayObjetivosAlAlcance()
{
  //Se overridea la obtención de objetivos posibles, para poder remover los sanos
  base.ListaHayObjetivosAlAlcance();

  List<object> unidadesAlAlcance = objPosibles;

        // Filtrar las unidades que no estén "sanas" (HP_actual < mod_maxHP)
        unidadesAlAlcance = unidadesAlAlcance
        .OfType<Unidad>() // Asegurarse de trabajar con objetos de tipo Unidad
        .Where(u => u.HP_actual < u.mod_maxHP) // Solo unidades no sanas
        .Cast<object>() // Volver a convertir a List<object>
        .ToList();

  return unidadesAlAlcance;
} 


public override object EstablecerObjetivoPrioritario()
{
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;

    // Filtrar, ordenar y seleccionar el objetivo prioritario
    var objetivo = objPosibles.OfType<Unidad>()
        .Where(u => u.HP_actual < u.mod_maxHP) // Solo unidades con HP no completo
        .OrderByDescending(u => u.mod_maxHP - u.HP_actual) // Ordenar por vida faltante (en puntos de HP)
        .ThenBy(u => u.CasillaPosicion.posX) // En caso de empate, ordenar por posX
        .ThenBy(u => Mathf.Abs(u.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY)) // Luego por distancia vertical
        .FirstOrDefault(); // Tomar el primero después del ordenamiento

    return objetivo;
}



}

  
 
  