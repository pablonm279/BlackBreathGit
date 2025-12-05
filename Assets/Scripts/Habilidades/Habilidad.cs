using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Reflection;

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

  public List<Casilla> lCasillasafectadas = new List<Casilla>();
  public bool esZonal; //true si afecta a todas las unidades en el rango, false si es individual.

  //TARGETEO ESPECIAL - 1: Misma Fila  - 2: Misma Columna - 3: Dos Casillas (Vertical) - 4: Tres Casillas (Vertical) - 5: Dos Casillas (AtrÃ¡s)
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

  [Tooltip("0=Sin mostrar probabilidad, 1=Ataque melee, 2=Ataque rango")]
  public int tipoPorcentaje = 0;

  // Unidades resaltadas al previsualizar la habilidad (se limpia al cancelar/resolver)
  private readonly HashSet<Unidad> unidadesMarcadasPrevisualizacion = new HashSet<Unidad>();

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

  /// <summary>
  /// Permite disparar manualmente el flujo de pre-impacto (proyectiles, delays, etc.) sin pasar por Resolver.
  /// </summary>
  public Task PrepararImpactoManualAsync(List<object> objetivos, Casilla casillaOrigenTrampas = null)
  {
    return EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
  }

  /// <summary>
  /// Permite disparar manualmente el flujo de post-impacto (esperas, limpieza) sin pasar por Resolver.
  /// </summary>
  public Task FinalizarImpactoManualAsync(List<object> objetivos, Casilla casillaOrigenTrampas = null)
  {
    return EsperarPostImpactoAsync(objetivos, casillaOrigenTrampas);
  }

  public abstract void Activar();

  public static event EventHandler OnUsarHabilidad;

  public abstract void ActualizarDescripcion();
  public void ActualizarNivel()
  {
    Invoke("Awake", 0.5f);
  }
  public abstract void Awake();

  // MÃ©todo abstracto para activar la habilidad.
  public virtual async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {
    BattleManager.Instance.bOcupado = true;
    // Al confirmar la habilidad, limpiar marcas de previsualizacion en unidades.
    LimpiarMarcasUnidadesPosibles();
    // AnimaciÃ³n/pose:
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

    if (scEstaUnidad.valorCargando > 0) //si estÃ¡ cargando y le alcanza que reste ap igual a lo que le faltaba
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
    int indexAPyEsfuerzo = (int)scEstaUnidad.ObtenerAPActual() + esforzable - costoAP;

    if ((int)scEstaUnidad.ObtenerAPActual() < 1) { return false; } //Si no tiene AP no puede esforzarse
    if (indexAPyEsfuerzo < 0) { return false; } //Devuelve false, como resultado de que no tiene AP suficiente

    if (indexAPyEsfuerzo < esforzable && esforzable > 0)
    { //Esto significa que para hacer la habilidad, se Esfuerza (debe AP para la siguiente ronda)

      seEsforzaria = esforzable - indexAPyEsfuerzo;
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
    //CrÃ­tico = 3

    int resultado = 0;

    float tiradaBase = tirada;
    float iTiradaAtaque = tirada;
    float deltaClima = 0f;

    //Efectos de clima en Ataques
    if (CampaignManager.Instance.intTipoClima == 3) // Lluvia -1 ataque rango
    {
      if (!esMelee)
      {
        iTiradaAtaque -= 1;
        deltaClima -= 1;
      }

    }
    if (CampaignManager.Instance.intTipoClima == 5) // Niebla -2 ataque rango
    {
      if (!esMelee)
      {
        iTiradaAtaque -= 2;
        deltaClima -= 2;
      }

    }


    string objetivoNombre = unidadAtacada != null ? unidadAtacada.uNombre : TRADU.i.Traducir("objetivo");
    int umbralPifia = 1 + sumaPifia;
    float umbralCritico = 19 - modificadorDadoCritico;
    string textoResultado;
    CombatLogFormatter.CombatOutcome outcome;

    if (iTiradaAtaque <= umbralPifia)//Pifia
    {
      scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir("<b>Pifia</b>"), Color.red);
      textoResultado = TRADU.i.Traducir("Pifia");
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          scEstaUnidad.mod_Ataque,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Pifia,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico),
          deltaClima));
      return -1;
    }

    if (iTiradaAtaque >= umbralCritico) //Golpe crítico
    {
      textoResultado = TRADU.i.Traducir("Crítico");
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          scEstaUnidad.mod_Ataque,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Critico,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico),
          deltaClima,
          TRADU.i.Traducir("Impacto crítico")));
      return 3;
    }

    float efectosAlAtaque = atributoAtaca + modificadorHabilidadaAtaque + scEstaUnidad.mod_Ataque;
    float iResultadoAtaque = iTiradaAtaque + efectosAlAtaque;

    if (iResultadoAtaque > defensaObjetivo)
    {
      resultado = 2; //Golpe
      outcome = CombatLogFormatter.CombatOutcome.Golpe;
      textoResultado = TRADU.i.Traducir("Golpe");
    }
    else if (Mathf.Approximately(iResultadoAtaque, defensaObjetivo))
    {
      resultado = 1; //Roce
      outcome = CombatLogFormatter.CombatOutcome.Roce;
      textoResultado = TRADU.i.Traducir("Roce");
    }
    else
    {
      resultado = 0; //Fallo
      outcome = CombatLogFormatter.CombatOutcome.Fallo;
      textoResultado = TRADU.i.Traducir("Fallo");
      unidadAtacada.GenerarTextoFlotante(TRADU.i.Traducir("Fallo"), new Color(0.8f, 0.8f, 0.8f), FloatingTextContext.Miss);
    }

    BattleManager.Instance.EscribirLog(
      CombatLogFormatter.FormatearAtaque(
        scEstaUnidad.uNombre,
        objetivoNombre,
        tiradaBase,
        iTiradaAtaque,
        atributoAtaca,
        modificadorHabilidadaAtaque,
        scEstaUnidad.mod_Ataque,
        defensaObjetivo,
        textoResultado,
        outcome,
        umbralPifia,
        Mathf.RoundToInt(umbralCritico),
        deltaClima));


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
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de SalvaciÃ³n: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: No se salva.");
    }
    else
    {
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} realiza Tirada de SalvaciÃ³n: 1d20 = {iTiradaDefensa} +{atributoDefiende} vs Tirada Dificultad: {iResultadoAtaque}. Resultado: Se salva.");
    }

    return resultado;
  }*/

    public abstract void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa); //la tirada se determina antes de entrar a cada objetivo, para que sea la misma

  public bool EsEscenaCampaña()
  {

    if (SceneManager.GetActiveScene().name == "ES-Campaña")
    {
      return true;
    }
    else { return false; }

  }

  /// <summary>
  /// Marca las unidades actualmente en rango segun lUnidadesPosiblesHabilidadActiva y limpia las que ya no aplican.
  /// </summary>
  public void SincronizarMarcasUnidadesPosibles()
  {
    SincronizarMarcasUnidades(BattleManager.Instance != null ? BattleManager.Instance.lUnidadesPosiblesHabilidadActiva : null);
  }

  /// <summary>
  /// Limpia las unidades marcadas en la ultima previsualizacion de la habilidad.
  /// </summary>
  public void LimpiarMarcasUnidadesPosibles()
  {
    if (unidadesMarcadasPrevisualizacion.Count == 0)
    {
      return;
    }

    foreach (Unidad unidad in unidadesMarcadasPrevisualizacion)
    {
      unidad?.Marcar(0);
      unidad?.OcultarProbabilidad();
    }

    unidadesMarcadasPrevisualizacion.Clear();
  }

  private void SincronizarMarcasUnidades(IEnumerable<Unidad> unidadesObjetivo)
  {
    if (unidadesObjetivo == null)
    {
      LimpiarMarcasUnidadesPosibles();
      return;
    }

    HashSet<Unidad> nuevas = new HashSet<Unidad>();
    foreach (Unidad unidad in unidadesObjetivo)
    {
      if (unidad == null)
      {
        continue;
      }

      if (unidadesMarcadasPrevisualizacion.Add(unidad))
      {
        unidad.Marcar(1);
      }

      float? prob = CalcularProbabilidadSobreObjetivo(unidad);
      unidad.MostrarProbabilidad(prob);

      nuevas.Add(unidad);
    }

    if (unidadesMarcadasPrevisualizacion.Count == 0)
    {
      return;
    }

    List<Unidad> paraRemover = new List<Unidad>();
    foreach (Unidad unidadMarcada in unidadesMarcadasPrevisualizacion)
    {
      if (!nuevas.Contains(unidadMarcada))
      {
        unidadMarcada?.Marcar(0);
        unidadMarcada?.OcultarProbabilidad();
        paraRemover.Add(unidadMarcada);
      }
    }

    foreach (Unidad unidad in paraRemover)
    {
      unidadesMarcadasPrevisualizacion.Remove(unidad);
    }
  }

  public float? CalcularProbabilidadSobreObjetivo(Unidad objetivo)
  {
    if (objetivo == null || scEstaUnidad == null)
    {
      return null;
    }

    switch (tipoPorcentaje)
    {
      case 1:
        return CalcularProbAtaque(objetivo, true);
      case 2:
        return CalcularProbAtaque(objetivo, false);
      default:
        return null;
    }
  }

  private float CalcularProbAtaque(Unidad objetivo, bool esMeleePorcentaje)
  {
    float atributoAtacante = esMeleePorcentaje ? scEstaUnidad.mod_CarFuerza : scEstaUnidad.mod_CarAgilidad;
    float ataqueTotal = scEstaUnidad.mod_Ataque + atributoAtacante + ObtenerBonusAtaque();
    if (!esMeleePorcentaje && CampaignManager.Instance != null)
    {
      // Penalidades de clima a ataques a distancia
      if (CampaignManager.Instance.intTipoClima == 3) // Lluvia
      {
        ataqueTotal -= 1f;
      }
      else if (CampaignManager.Instance.intTipoClima == 5) // Niebla
      {
        ataqueTotal -= 2f;
      }
    }
    float defensaObjetivo = objetivo.ObtenerdefensaActual();

    int exitos = 0;
    for (int dado = 1; dado <= 20; dado++)
    {
      bool pifia = dado == 1;
      bool critico = dado >= 19;
      if (pifia)
      {
        continue;
      }

      if (critico)
      {
        exitos++;
        continue;
      }

      float resultado = dado + ataqueTotal;
      if (resultado > defensaObjetivo)
      {
        exitos++;
      }
    }

    return exitos / 20f;
  }

  protected virtual float ObtenerBonusAtaque()
  {
    // Busca un campo/propiedad llamado "bonusAtaque" en la habilidad concreta (serializado privado o público).
    BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    var campo = GetType().GetField("bonusAtaque", flags);
    if (campo != null && (campo.FieldType == typeof(int) || campo.FieldType == typeof(float)))
    {
      object val = campo.GetValue(this);
      if (val is int i) return i;
      if (val is float f) return f;
    }

    var prop = GetType().GetProperty("bonusAtaque", flags);
    if (prop != null && prop.CanRead)
    {
      object val = prop.GetValue(this, null);
      if (val is int i) return i;
      if (val is float f) return f;
    }

    return 0f;
  }

}
