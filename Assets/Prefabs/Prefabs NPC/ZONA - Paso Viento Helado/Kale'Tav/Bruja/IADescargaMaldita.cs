using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

public class IADescargaMaldita : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


  void Awake()
   {
      nombre = "Descarga Maldita";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 4;
      hCooldownMax = 2;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 3;
      omitirAnimacionDeUso = true; // sin animacion de ataque (vaiven)
      afectaObstaculos = true;



      hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1); // arranque aleatorio

      bonusAtaque = 3;
      XdDanio = 2;
      daniodX = 6; //1d6
      tipoDanio = 8; //Arcano
   }

    void Start()
    {
      prioridad = pPrioridad;
    }

  object Objetivo;
   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);

      hActualCooldown = hCooldownMax; // antes nunca se aplicaba: la podia usar todos los turnos

      Objetivo = EstablecerObjetivoPrioritario();
      PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

      Task impactoPendiente = Objetivo != null ? CrearProyectil(Objetivo) : Task.CompletedTask;
      await impactoPendiente;

      if (Objetivo != null)
      {
          AplicarEfectosHabilidad(Objetivo);
      }
   }

   internal Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private const string NombreMaldicion = "Maldición Ritual";
    private const int TipoSalvacionFortaleza = 1;

    private static Vector3 PosicionObjetivo(object objetivo)
    {
        if (objetivo is Unidad u)
        {
            return u.uImage != null ? u.uImage.transform.position : u.transform.position;
        }

        if (objetivo is Obstaculo o)
        {
            return o.transform.position;
        }

        return Vector3.zero;
    }

    private static Vector3 PiesObjetivo(object objetivo)
    {
        if (objetivo is Unidad u)
        {
            return u.bGrande || u.CasillaPosicion == null ? u.transform.position : u.CasillaPosicion.transform.position;
        }

        if (objetivo is Obstaculo o)
        {
            return o.CasillaPosicion != null ? o.CasillaPosicion.transform.position : o.transform.position;
        }

        return Vector3.zero;
    }

    // Ritual: circulo runico y brasas en las manos, y despues el orbe sangriento viaja hasta el objetivo.
    private async Task LanzarProyectilAsync(object objetivo)
    {
        float f = DracoHieloVfx.FactorTiempo();
        float cas = GulekGulVfx.TamanoCasilla();

        Vector3 destino = PosicionObjetivo(objetivo);
        Vector3 centroBruja = scEstaUnidad.uImage != null ? scEstaUnidad.uImage.transform.position : scEstaUnidad.transform.position;
        float dirX = Mathf.Sign(destino.x - centroBruja.x);
        if (Mathf.Approximately(dirX, 0f)) { dirX = -1f; }

        Vector3 manos = centroBruja + new Vector3(dirX * cas * 0.3f, cas * 0.25f, 0f);
        Vector3 pies = scEstaUnidad.CasillaPosicion != null ? scEstaUnidad.CasillaPosicion.transform.position : scEstaUnidad.transform.position;

        BrujaMaldicionVfx.ConvocarRitual(pies, manos, 0.45f * f);
        await BattleManager.DelayCombateAsync(Mathf.RoundToInt(500 * f));

        if (!PuedeResolverAccionIA())
        {
            return;
        }

        await BrujaMaldicionVfx.LanzarOrbe(manos, destino, 0.5f * f);
    }

    public override void AplicarEfectosHabilidad(object obj)
    {

     if(obj is Unidad)
     {


        Unidad objetivo = (Unidad)obj;

        float defensaObjetivo = objetivo.ObtenerdefensaActual();

        int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;

        Vector3 pos = PosicionObjetivo(objetivo);
        Vector3 pies = PiesObjetivo(objetivo);

      if (resultadoTirada == -1)
      {//PIFIA
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        scEstaUnidad.EstablecerAPActualA(0);
      }
      else if (resultadoTirada == 0)
      {//FALLO
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
      }
      else if (resultadoTirada == 1)
      {//ROCE: mitad de daño, sin maldicion
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        BrujaMaldicionVfx.ImpactoMaldicion(pos, pies, false);
      }
      else if (resultadoTirada == 2)
      {//GOLPE
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        BrujaMaldicionVfx.ImpactoMaldicion(pos, pies, false);
        IntentarMaldecir(objetivo, 12);
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
        BrujaMaldicionVfx.ImpactoMaldicion(pos, pies, true);
        IntentarMaldecir(objetivo, 14);
      }

      objetivo.AplicarDebuffPorAtaquesreiterados(1);

     }
      else if(obj is Obstaculo)
     {
          Obstaculo objetivo = (Obstaculo)obj;

          float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
           danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

          objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
          BrujaMaldicionVfx.ImpactoMaldicion(PosicionObjetivo(objetivo), PiesObjetivo(objetivo), false);
     }

    }

    // Maldicion ritual: resta ataque y defensa durante 2 turnos del objetivo. Salvacion de Fortaleza.
    // La magnitud (-1 a -3) crece cuanto menos vida le queda. No se acumula ni se renueva.
    void IntentarMaldecir(Unidad objetivo, int dificultad)
    {
      if (objetivo == null || objetivo.HP_actual <= 0f || objetivo.TieneBuffNombre(NombreMaldicion))
      {
        return;
      }

      if (!objetivo.TiradaSalvacion(TipoSalvacionFortaleza, dificultad)) //TRUE = no se salva
      {
        return;
      }

      float fraccionVida = Mathf.Clamp01(objetivo.HP_actual / Mathf.Max(1f, objetivo.mod_maxHP));
      int magnitud = Mathf.Clamp(1 + Mathf.FloorToInt((1f - fraccionVida) * 3f), 1, 3);

      // La duracion corre al terminar el turno del maldito: 2 cubre sus dos proximos turnos.
      Buff buff = new Buff();
      buff.buffNombre = NombreMaldicion;
      buff.boolfDebufftBuff = false;
      buff.DuracionBuffRondas = 2;
      buff.cantAtaque -= magnitud;
      buff.cantDefensa -= magnitud;
      buff.AplicarBuff(objetivo);
      ComponentCopier.CopyComponent(buff, objetivo.gameObject);

      BrujaMaldicionVfx.AdjuntarMaldicion(objetivo, NombreMaldicion);
    }


    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
    
      // Obtener la unidad duea
    Unidad unidadDuea = gameObject.GetComponent<Unidad>();
    if (unidadDuea == null) return null;
   
    // Filtrar las unidades
     print("Sel objPosibles "+objPosibles.Count);
   
    
    var unidades = objPosibles.OfType<Unidad>().ToList();
    print("Sel obj unidades cant: "+unidades.Count);
    // Filtrar los obstculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();
     print("Sel obj obstaculos cant: "+obstaculos.Count);

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
   
    var unidadesOrdenadas = unidades
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuea.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
       
        return unidadesOrdenadas.FirstOrDefault();
    }else{print("lista unidades vacia");}

    // Si no hay unidades, devolver el obstculo
     if (obstaculos.Any())
     {
       var obstaculo = obstaculos.FirstOrDefault();
      
       return obstaculo;
     }

  
     return null;
   }



}

  
 
  

