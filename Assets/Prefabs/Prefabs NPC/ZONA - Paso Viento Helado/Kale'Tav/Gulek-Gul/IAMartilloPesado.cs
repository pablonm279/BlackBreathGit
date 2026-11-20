using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

public class IAMartilloPesado : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro

   
  // Leve avance visual hacia el objetivo (mas cuanto mas lejos), luego vuelve. Solo visual.
  protected override float AproximacionLeveFraccion => 0.4f;
  protected override float AproximacionLeveMaxima => GulekGulVfx.TamanoCasilla() * 1.1f;

  void Awake()
   {
      nombre = "Martillo Grande";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 2; // unidad grande 2x2: alcanza toda la zona frontal (+-2 filas desde su ancla)
      esMelee = true;
      hAlcance = 1;
      hCooldownMax = 1;
      esHostil = true;
      prioridad = 20; // por encima de Pisoton/Lanzar Roca: con el martillo listo, siempre se usa
      costoAP = 4;
      afectaObstaculos = true;
      


      hActualCooldown = 1;

      bonusAtaque = 3;
      XdDanio = 4;
      daniodX = 10; 
      tipoDanio = 3; //Perforante


   
    
   }

    void Start()
    {
      prioridad = 20; // por encima de Pisoton/Lanzar Roca: con el martillo listo, siempre se usa
    }


   public async override Task ActivarHabilidad()
   {
     if (!MartilloListoParaGolpear())
    {
      return; // guarda: ListaHayObjetivosAlAlcance ya la excluye sin el martillo
    }
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
      hActualCooldown = hCooldownMax;
      
      scEstaUnidad.ReproducirAnimacionAtaque();
            object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
            PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

      await BattleManager.DelayCombateAsync(450);
      AplicarEfectosHabilidad(Objetivo);
      await BattleManager.DelayCombateAsync(600);
     
   }
   
     void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GolpeMartilloPesado");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }
     void VFXAplicar2(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GolpeMartilloPesadoZona");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (obj is Unidad)
    {
      Unidad objetivo = (Unidad)obj;
      float defensaObjetivo = objetivo.ObtenerdefensaActual();

      int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo);



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

        objetivo.RecibirDanio(danio + 4, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
       //   print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);


        objetivo.RecibirDanio(danio + 4, tipoDanio, false, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);

        AplicarEfectoZona(objetivo);


      }
      else if (resultadoTirada == 3)
      {//CRITICO
       //  print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);




        objetivo.RecibirDanio(danio + 4, tipoDanio, true, scEstaUnidad);
        VFXAplicar(objetivo.gameObject);

        AplicarEfectoZona(objetivo);


      }

      // Grietas y polvo en la casilla golpeada (solo si el golpe hizo daño).
      if (resultadoTirada >= 1 && objetivo.CasillaPosicion != null)
      {
        GulekGulVfx.ImpactoMartilloEnCasilla(objetivo.CasillaPosicion.transform.position, true);
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

    scEstaUnidad.RemoverBuffNombre("Martillo Listo");
  }
  

 void AplicarEfectoZona(Unidad objetivoInicial)
  {
    List<Casilla> casillasCercanas = objetivoInicial.CasillaPosicion.ObtenerCasillasAlrededor(2);

    foreach (Casilla casilla in casillasCercanas)
    {
      // Grietas y polvo en TODA la zona afectada (ocupada o no) para que se entienda donde pega;
      // la onda viaja hacia afuera con la distancia al golpe.
      int distanciaAlGolpe = Mathf.Abs(casilla.posX - objetivoInicial.CasillaPosicion.posX)
        + Mathf.Abs(casilla.posY - objetivoInicial.CasillaPosicion.posY);
      GulekGulVfx.ImpactoMartilloEnCasilla(casilla.transform.position, false, distanciaAlGolpe);

      if (casilla.Presente != null)
      {
        if (casilla.Presente.GetComponent<Unidad>() != null)
        { 
          Unidad unidadObjetivo = casilla.Presente.GetComponent<Unidad>();

          float danio = TiradaDeDados.TirarDados(XdDanio, daniodX)/2; //Daño reducido en un 50% a unidades cercanas
          danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

          unidadObjetivo.RecibirDanio(danio , tipoDanio, false, scEstaUnidad); 
          VFXAplicar2(unidadObjetivo.gameObject);


        }

      }


      
    }
  }

// Con el martillo y SIN haberlo levantado en este mismo turno. Levantar le deja cooldown 1 y ese cooldown
  // baja recien al empezar su proximo turno; asi, aunque le sobren AP (Frenetico, Descansado), no levanta y golpea a la vez.
  private bool MartilloListoParaGolpear()
  {
    if (!scEstaUnidad.TieneBuffNombre("Martillo Listo"))
    {
      return false;
    }

    IALevantarMartillo levantar = GetComponent<IALevantarMartillo>();
    return levantar == null || levantar.hActualCooldown <= 0;
  }

public override List<object> ListaHayObjetivosAlAlcance()
  {
    List<object> lista = base.ListaHayObjetivosAlAlcance(); // conserva la tendencia de movimiento hacia el enemigo
    if (!MartilloListoParaGolpear())
    {
      objPosibles.Clear();
      return objPosibles; // sin el martillo levantado (o recien levantado este turno) no es una accion posible
    }

    return lista;
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


 public GameObject VFXEstadoPrefab;


}

  
 
  


