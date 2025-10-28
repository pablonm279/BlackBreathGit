using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAMordiscoFaagdan : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro
   
  void Awake()
   {
      nombre = "Mordisco Faagdan";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = true;
      hAlcance = 1;
      hCooldownMax = 2;
      esHostil = true;
      prioridad = 10;
      costoAP = 4;
      afectaObstaculos = true;
      


      hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1);

      bonusAtaque = 0;
      XdDanio = 3;
      daniodX = 10; //3d10
      tipoDanio = 2; //Perforante


   
    
   }

    void Start()
    {
      prioridad = 10;
    }


   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
      hActualCooldown = hCooldownMax;
      
      scEstaUnidad.ReproducirAnimacionAtaque();
            object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
                PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

      await Task.Delay(700);
      AplicarEfectosHabilidad(Objetivo);
     
   }

      void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_MordiscoFaagdan");

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

          int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;



      if (resultadoTirada == -1)
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
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        //AplicaDerribado(objetivo, -2);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 2)
      {//GOLPE
       //   print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX)+5;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

        AplicarEfecto(objetivo, 0);
        VFXAplicar(objetivo.gameObject);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
       //  print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX)+5;
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);




        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);

        AplicarEfecto(objetivo, 2);
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
    
    void AplicarEfecto(Unidad objetivo, int intensidad)
    {
      if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, 9+intensidad))
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Armadura masticada";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.cantArmadura -= 3;
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
      }
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





}

  
 
  