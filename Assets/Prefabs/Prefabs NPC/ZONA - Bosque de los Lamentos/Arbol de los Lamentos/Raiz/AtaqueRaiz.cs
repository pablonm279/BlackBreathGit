using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class AtaqueRaiz : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro
   
  void Awake()
   {
      nombre = "Ataque Raiz";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = true;
      hAlcance = 4;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = 1;
      costoAP = 1;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 0;
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 2; //Perforante


   
    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
      hActualCooldown = hCooldownMax;
      
      scEstaUnidad.ReproducirAnimacionAtaque();
      await Task.Delay(700);
      object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(Objetivo);
     
   }

    
    public override void AplicarEfectosHabilidad(object obj)
    {
     if(obj is Unidad)
     {
          Unidad objetivo = (Unidad)obj;
          float defensaObjetivo = objetivo.ObtenerdefensaActual();

          int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;
          
        

          if(resultadoTirada == -1)
          {//PIFIA 
        //    print("Pifia");
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
            
          }
          else if (resultadoTirada == 0)
          {//FALLO
           // print("Fallo");

            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

          }
          else if (resultadoTirada == 1)
          {//ROCE
         //   print("Roce");
            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
             danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

            danio -= danio/2; //Reduce 50% por roce

            objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);
          VFXAplicar(objetivo.gameObject);
            AplicaSangradoTirada(objetivo, -2);
          }
          else if (resultadoTirada == 2)
          {//GOLPE
         //   print("Golpe");

            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
             danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);


            objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);
            
            AplicaSangradoTirada(objetivo, 0);
          VFXAplicar(objetivo.gameObject);
          }
          else if (resultadoTirada == 3)
          {//CRITICO
          //  print("Critico");

            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

            
            
            
            objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
            
            AplicaSangradoTirada(objetivo, 1);
          }
          VFXAplicar(objetivo.gameObject);
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
public override object EstablecerObjetivoPrioritario() 
{
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;

    // Filtrar las unidades
    var unidades = objPosibles.OfType<Unidad>().ToList();
    // Filtrar los obstáculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
    var unidadesOrdenadas = unidades
        .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
        return unidadesOrdenadas.FirstOrDefault();
    }

    // Si no hay unidades, devolver el obstáculo
    var obstaculo = obstaculos.FirstOrDefault();
    return obstaculo;
}



void AplicaSangradoTirada(Unidad unidad, int extraDC)
{
   if(unidad.TiradaSalvacion(unidad.mod_TSMental, 9+extraDC))
     {
         unidad.estado_sangrado = 1;
     }

}

}

  
 
  