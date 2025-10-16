using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public abstract class Habilidad : MonoBehaviour
{
    public string nombre;
    public int costoAP;
   
    public GameObject vfxImpacto;
    public GameObject vfxCasteo;

    public int requiereRecurso; // Habilidades que requieren un recurso externo para funcionar, ej flechas.
    
    public int costoPM;
    public int cooldownMax;
    public int cooldownActual;
    public GameObject Usuario;
    public Unidad scEstaUnidad;

    public int NIVEL;
    public int IDenClase; //Del 1 al 10, que ID tiene esta habilidad en la clase
    
    public Item agregaDesdeArmaUI = null;
    public string txtDescripcion;
    public Sprite imHab;
    
    public  List<Casilla> lCasillasafectadas = new List<Casilla>();
    public bool esZonal; //true si afecta a todas las unidades en el rango, false si es individual.
   
    //TARGETEO ESPECIAL - 1: Misma Fila  - 2: Misma Columna - 3: Dos Casillas (Vertical) - 4: Tres Casillas (Vertical) - 5: Dos Casillas (Atrás)
    public int targetEspecial = 0;  //1: Misma fila  2: Misma Columna 3: Dos Casillas (Vert) 4: Tres Casillas (Vert)5: Dos Casillas (Atras) 
    //6: 3 casillas Vertical y las de atras
    public int enArea = 0; //Si este valor es mayor a 0, permite tarjetear celdas, afectando a unidades alrededor. 1, cruz, 2 cuadrado, 3 todo
    public int esforzable; //Hasta cuantos AP de su costo permite "deber"
    public bool esCargable; //Si no alcanzan los AP del turno, se castea otro turno cuando se paguen todos.
    public bool esMelee; 
    public bool bAfectaObstaculos; 

    public bool poneTrampas; //Si la habilidad pone trampas 
    public bool poneObstaculo; //Si la habilidad pone obstaculo 

    public bool esHostil; //Si es para enemigos o aliados
    public bool esDiscreta = false; //No quita sigilo

    protected virtual int DelayPreImpactoMs => 1000;
    protected virtual int DelayPostImpactoMs => 700;

    protected virtual Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return DelayPreImpactoMs > 0 ? Task.Delay(DelayPreImpactoMs) : Task.CompletedTask;
    }

    protected virtual Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return DelayPostImpactoMs > 0 ? Task.Delay(DelayPostImpactoMs) : Task.CompletedTask;
    }

    public abstract void Activar();
    
    public static event EventHandler OnUsarHabilidad;
    
    public abstract void ActualizarDescripcion();
    public void ActualizarNivel()
    {
      Invoke("Awake", 0.5f);
    }
    public abstract void Awake();

  // Método abstracto para activar la habilidad.
  public virtual async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {
    BattleManager.Instance.bOcupado = true;
    // Animación/pose:
    // - Canalizador: toda habilidad hostil usa ataque (melee o rango)
    // - Resto: hostil melee usa ataque; hostil a distancia y no hostil usan pose de habilidad
    if (scEstaUnidad is ClaseCanalizador && esHostil)
    {
      scEstaUnidad.ReproducirAnimacionAtaque();
    }
    else if (esHostil)
    {
      if (esMelee)
      {
        scEstaUnidad.ReproducirAnimacionAtaque();
      }
      else
      {
        scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
      }
    }
    else
    {
      scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
    }
    // Log de uso de habilidad
    if (BattleManager.Instance != null && scEstaUnidad != null)
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
    }

    List<Unidad> lUnidadesPosibles = new List<Unidad>(BattleManager.Instance.lUnidadesTotal);
    lUnidadesPosibles.Remove(scEstaUnidad);

    if (Objetivos != null)
    {
      List<Unidad> lUnidadesObjetivos = new List<Unidad>(Objetivos.FindAll(x => x is Unidad).ConvertAll(x => (Unidad)x));
      foreach (Unidad unidad in lUnidadesObjetivos)
      {
        lUnidadesPosibles.Remove(unidad);
      }
      BattleManager.Instance.SombrearANoParticipantesHabilidad(lUnidadesPosibles.ConvertAll(x => (object)x));
    }

    await Task.Delay(250);
    await EsperarPreImpactoAsync(Objetivos, casillaOrigenTrampas);

    int tirada = UnityEngine.Random.Range(1, 21); //la tirada es la misma para toda la habilidad, no para cada objetivo

    if (Objetivos != null)
    {
      foreach (var objeto in Objetivos) //puede ser Unidad o Obstaculo
      {
        AplicarEfectosHabilidad(objeto, tirada, casillaOrigenTrampas);
      }
    }
    else
    {
      AplicarEfectosHabilidad(null, tirada, casillaOrigenTrampas);
    }

    await EsperarPostImpactoAsync(Objetivos, casillaOrigenTrampas);
    if (Objetivos != null)
    {
      BattleManager.Instance.DesombrearANoParticipantesHabilidad(lUnidadesPosibles.ConvertAll(x => (object)x));
    }

    //Si la habilidad es hostil, no Discreta, y tiene Escondido tier 1, se revela

    if (esHostil && !esDiscreta && (scEstaUnidad.ObtenerEstaEscondido() > 0))
    {
      scEstaUnidad.PerderEscondido();
    }

    BattleManager.Instance.SeleccionandoObjetivo = false;
    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.scUIBotonesHab.UIDesactivarHabilidades();

    BattleManager.Instance.LimpiarCapasCasillas();

    if (scEstaUnidad.valorCargando > 0) //si está cargando y le alcanza que reste ap igual a lo que le faltaba
    {
      scEstaUnidad.CambiarAPActual(-scEstaUnidad.valorCargando);
      scEstaUnidad.valorCargando = 0;
      scEstaUnidad.estaCargando = null;
    }
    else
    {
      scEstaUnidad.CambiarAPActual(-costoAP);
    }
    scEstaUnidad.AccionP_SeEsforzo = seEsforzaria;


    cooldownActual = cooldownMax;

    OnUsarHabilidad?.Invoke(this, EventArgs.Empty);



    Invoke("ActualizarCirculosDelay", 0.5f); //Para que se actualice el UI de AP luego de un delay, para que no se vea el cambio de golpe

    if (costoPM != 0)
    {
      scEstaUnidad.SumarValentia(-costoPM);
    }
     Invoke("desocuparDelay", 0.5f); 
     BattleManager.Instance.bOcupado = false;
    }
     
          
    void ActualizarCirculosDelay()
    {
        BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
    }

    void desocuparDelay()
    {
             BattleManager.Instance.bOcupado = false;

    }

   
    public int seEsforzaria;
    public bool tieneAPSuficientes(out int esforzo)
    {
        seEsforzaria = 0;
        esforzo = 0;
        int indexAPyEsfuerzo = (int)scEstaUnidad.ObtenerAPActual() + esforzable  - costoAP;
        
        if((int)scEstaUnidad.ObtenerAPActual() < 1){ return false;} //Si no tiene AP no puede esforzarse
        if(indexAPyEsfuerzo < 0){ return false;} //Devuelve false, como resultado de que no tiene AP suficiente

        if(indexAPyEsfuerzo < esforzable && esforzable > 0)
        { //Esto significa que para hacer la habilidad, se Esfuerza (debe AP para la siguiente ronda)
                
         seEsforzaria = esforzable-indexAPyEsfuerzo;
         esforzo = seEsforzaria;
         
        
        }

        return true;
      
       
    } 

    //Ataque vs Defensa convencional
    public int TiradaAtaque(int tirada, float defensaObjetivo, float atributoAtaca, float modificadorHabilidadaAtaque, float modificadorDadoCritico, Unidad unidadAtacada, int sumaPifia)
    {
      //Pifia = -1
      //Fallo = 0
      //Roce = 1
      //Golpe = 2
      //Crítico = 3

      int resultado = 0;

      float iTiradaAtaque = tirada;

      string sAtaqueModClima = iTiradaAtaque+"";
      //Efectos de clima en Ataques
      if(CampaignManager.Instance.intTipoClima == 3) // Lluvia -1 ataque rango
      {
        if(!esMelee)
        {
          iTiradaAtaque -= 1; sAtaqueModClima += TRADU.i.Traducir(" (-1 Lluvia)");
        }
        
      }
      if(CampaignManager.Instance.intTipoClima == 5) // Niebla -2 ataque rango
      {
        if(!esMelee)
        {
          iTiradaAtaque -= 2; sAtaqueModClima += TRADU.i.Traducir(" (-2 Niebla)");
        }
        
      }

     
    
      if(iTiradaAtaque <= 1 + sumaPifia)//Pifia
      { 
        scEstaUnidad.GenerarTextoFlotante( TRADU.i.Traducir("<b>Pifia</b>"), Color.red);
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ")+sAtaqueModClima+TRADU.i.Traducir(". Resultado: Pifia."));
        return -1;
      }

      if(iTiradaAtaque >= 19-modificadorDadoCritico){ return 3;} //Golpe crítico

      float efectosAlAtaque = atributoAtaca+modificadorHabilidadaAtaque+scEstaUnidad.mod_Ataque;
      float iResultadoAtaque = iTiradaAtaque + efectosAlAtaque;

      if(iResultadoAtaque == defensaObjetivo)
      {   
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ")+sAtaqueModClima+TRADU.i.Traducir(". Resultado: Fallo."));
          return 1; //Roce
      }
      if(iResultadoAtaque > defensaObjetivo)
      {
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ")+sAtaqueModClima+TRADU.i.Traducir(". Resultado: Roce."));

          return 2; //Golpe
      }
     
      if(resultado == 0)
      {  
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ")+sAtaqueModClima+TRADU.i.Traducir(". Resultado: Golpe."));
         unidadAtacada.GenerarTextoFlotante(TRADU.i.Traducir("Fallo"), new Color(0.8f, 0.8f, 0.8f));
      }
        

     if (resultado < 2 && BattleManager.Instance.HabilidadActiva.esMelee)
    { AudioSource.PlayClipAtPoint(BattleManager.Instance.contenedorPrefabs.sonidoErrar, transform.position); }

      return resultado;
    }

    //Tiradas Salvacion vs Atributo  boolean TRUE falla tirada  FALSE gana tirada.
    /*
    public bool TiradaSalvacion(float atributoDefiende, float atributoAtaca, float modificadorHabilidadaAtaque)
    {
      bool resultado = false;

      float iTiradaAtaque = UnityEngine.Random.Range(1,21);
      float iTiradaDefensa = UnityEngine.Random.Range(1,21);

      float iResultadoAtaque = iTiradaAtaque + atributoAtaca + modificadorHabilidadaAtaque;
      float iResultadoDefensa = iTiradaDefensa + atributoDefiende;
     

      resultado = iResultadoAtaque > iResultadoDefensa;

      if(resultado)
      {
         BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de Salvación: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: No se salva.");
      }
      else
      {
         BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de Salvación: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: Se salva.");
      }

      return resultado;
    }*/

    public abstract void  AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa); //la tirada se determina antes de entrar a cada objetivo, para que sea la misma

    public bool EsEscenaCampaña()
    {

     if (SceneManager.GetActiveScene().name == "ES-Campaña")
    {
        return true;
    }
    else{return false;}

    }


}


