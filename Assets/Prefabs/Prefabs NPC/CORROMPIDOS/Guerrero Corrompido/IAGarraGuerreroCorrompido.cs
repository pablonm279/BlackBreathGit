using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAGarraGuerreroCorrompido : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro

  
  void Awake()
   {
      nombre = "Ataque de Garra";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = true;
      hAlcance = 1;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = 4;
      costoAP = 3;
      afectaObstaculos = true;
      


      hActualCooldown = 0;


      bonusAtaque = 0;
      XdDanio = 3;
      daniodX = 6+5; //
      tipoDanio = 2; //Cortante


   
    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


  public async override Task ActivarHabilidad()
  {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;
    tiradaDeAtaqueGeneralparaZona = UnityEngine.Random.Range(1, 21); //la tirada es la misma para toda la habilidad, no para cada objetivo

    scEstaUnidad.ReproducirAnimacionAtaque();

     object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
     List<object> unidadesEnZona = ObtenerUnidadesEnZona(ObtenerAfectadosZonaObjetivo(Objetivo));
     unidadesEnZona.Add(Objetivo);
     PrepararInicioAnimacion(unidadesEnZona,null);//Despues de establecer objetivo

    await Task.Delay(500);

    AplicarEfectosHabilidad(Objetivo);
    AplicarEfectosEnZona(ObtenerAfectadosZonaObjetivo(Objetivo)); //aca se determina la zona del Ataque relativo al objetivo

   }
    public List<object> ObtenerUnidadesEnZona(List<Casilla> casillas)
    {
      List<object> unidades = new List<object>();
      foreach (Casilla cas in casillas)
      {
        if (cas.Presente != null)
        {
          Unidad unidad = cas.Presente.GetComponent<Unidad>();
          if (unidad != null)
          {
            unidades.Add(unidad);
          }
        }
      }
      return unidades;
    }
     float tiradaDeAtaqueGeneralparaZona = 0;
    public void AplicarEfectosEnZona(List<Casilla> casillas)
    {
     
     foreach(Casilla cas in casillas)
     {
        //-----
        //Acá aplicar efectos visuales a la casilla si corresponde
        //-----
       
      if(cas.Presente != null)
      {
        if(cas.Presente.GetComponent<Unidad>() != null)
        {
            Unidad obj = cas.Presente.GetComponent<Unidad>();
            AplicarEfectosHabilidad(obj);
        }
        if(cas.Presente.GetComponent<Obstaculo>() != null)
        {
            Obstaculo obj = cas.Presente.GetComponent<Obstaculo>();
            AplicarEfectosHabilidad(obj);
        }
     }
       
    }

    }
         void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GarraDevorador");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }
     private List<Casilla> ObtenerAfectadosZonaObjetivo(object obj) //Obtiene las casillas a una determinada distancia del objetivo en X o/y en Y
    {
      List<Casilla> casillasEnZona = new List<Casilla>();

      if (obj is Unidad)
      {
        Unidad objetivo = (Unidad)obj;
        Casilla casOrigen = objetivo.CasillaPosicion;

        casillasEnZona.AddRange(casOrigen.ObtenerCasillasAdyacentesEnColumna());

      }
      if (obj is Obstaculo)
      {
        Obstaculo objetivo2 = (Obstaculo)obj;
        Casilla casOrigen = objetivo2.CasillaPosicion;

        casillasEnZona.AddRange(casOrigen.ObtenerCasillasAdyacentesEnColumna());

      }

    return casillasEnZona;
    }

    
    public override void AplicarEfectosHabilidad(object obj)
  {
    if (obj is Unidad)
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();

      int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo, tiradaDeAtaqueGeneralparaZona);



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

        objetivo.RecibirDanio(danio + 3, tipoDanio, false, scEstaUnidad);

        VFXAplicar(objetivo.gameObject);
      }
      else if (resultadoTirada == 2)
      {//GOLPE
       //   print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        VFXAplicar(objetivo.gameObject);

        objetivo.RecibirDanio(danio + 5, tipoDanio, false, scEstaUnidad);

      }
      else if (resultadoTirada == 3)
      {//CRITICO
       //  print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
        
        VFXAplicar(objetivo.gameObject);


        objetivo.RecibirDanio(danio + 10, tipoDanio, true, scEstaUnidad);

      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }
    else if (obj is Obstaculo)
    {
      Obstaculo objetivo = (Obstaculo)obj;

      float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
      objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
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

  
 
  