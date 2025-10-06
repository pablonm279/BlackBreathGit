using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class AtaqueVaina : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Ataque Vaina";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 3;
      esMelee = false;
      hAlcance = 5;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 2;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 6;
      XdDanio = 2;
      daniodX = 8; //2d8
      tipoDanio = 1; //Cortante


      

    
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

          
    
      await Task.Delay(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(Objetivo);
     
   }
       void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AtaqueLiana");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }
      public override void AplicarEfectosHabilidad(object obj)
    {
   
     if(obj is Unidad)
     {

     
        Unidad objetivo = (Unidad)obj;
     
        float defensaObjetivo = objetivo.ObtenerdefensaActual();

        int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;
     
   

     if(resultadoTirada == -1)
     {//PIFIA 
       print("Pifia");
       objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
       //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
     }
     else if (resultadoTirada == 0)
     {//FALLO
       print("Fallo");
       objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
     }
     else if (resultadoTirada == 1)
     {//ROCE
       print("Roce");
       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
        danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

       danio -= danio/2; //Reduce 50% por roce

       objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);

        VFXAplicar(objetivo.gameObject);
     }
     else if (resultadoTirada == 2)
     {//GOLPE
       print("Golpe");

       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

       objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);
      
       AplicaAtraerVaina(objetivo);

        VFXAplicar(objetivo.gameObject);
     }
     else if (resultadoTirada == 3)
     {//CRITICO
       print("Critico");

       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
      
       objetivo.RecibirDanio(danio, tipoDanio, true,  scEstaUnidad);
       AplicaAtraerVaina(objetivo);
        VFXAplicar(objetivo.gameObject);
     }
     
      objetivo.AplicarDebuffPorAtaquesreiterados(1);
      
     }
      else if(obj is Obstaculo)
     {
          Obstaculo objetivo = (Obstaculo)obj;
          
          float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
           danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
           
           objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
     
    }

   
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
    
      // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;
   
    // Filtrar las unidades
     print("Sel objPosibles "+objPosibles.Count);
   
     
    var unidades = objPosibles.OfType<Unidad>().ToList();
    print("Sel obj unidades cant: "+unidades.Count);
    // Filtrar los obstáculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();
     print("Sel obj obstaculos cant: "+obstaculos.Count);

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
   
    var unidadesOrdenadas = unidades
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
       
        return unidadesOrdenadas.FirstOrDefault();
    }else{print("lista unidades vacia");}

    // Si no hay unidades, devolver el obstáculo
     if (obstaculos.Any())
     {
       var obstaculo = obstaculos.FirstOrDefault();
      
       return obstaculo;
     }

  
     return null;
   }

   void AplicaAtraerVaina(Unidad unidad)
   {
    
    if(unidad.TiradaSalvacion(unidad.mod_TSFortaleza, 14))
    {
      unidad.ForzarMoverAPrimeraFila();
    }

   }

}

  
 
  