using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


public abstract class IAHabilidad : MonoBehaviour
{

  public String nombre;
  public GameObject vfxImpacto;
  public GameObject vfxCasteo;
  public int hAncho; //Solo para habilidades hostiles
  public int hAlcance; //En habilidades no hostiles, funciona como alcance (distancia) a la casilla del aliado
  public bool esMelee; //Aumenta el rango si esta en la columna del frente
  public bool afectaObstaculos; //Aumenta el rango si esta en la columna del frente
  [Header("Animacion")]
  [SerializeField] public bool fuerzaPoseAtaque = false;

  public int hCooldownMax;
  public int hActualCooldown;

  public GameObject Usuario;
  public Unidad scEstaUnidad;

  public bool esHostil; //Si es para enemigos o aliados

  public int prioridad; //Mientras mas, mas chances que la elija

  public int costoAP;

  Casilla casillaOrigen;
  private Task secuenciaVisualEnCurso = Task.CompletedTask;
  private readonly object secuenciaVisualLock = new object();
  private const int PausaPostAproximacionMs = 150;
  private const int PausaAntesVolverMs = 2100;
  private const float VentanaAnimacionDuplicada = 1.1f;
  private float inicioSecuenciaVisual = -999f;

  public List<object> objPosibles = new List<object>(); //Esta variable guarda los objetivos posibles de la habilidad

  public abstract object EstablecerObjetivoPrioritario(); //Dentro de la lista objPosibles, segun la habilidad, determinar cual es la mejor opcion 


  public abstract Task ActivarHabilidad();

  public abstract void AplicarEfectosHabilidad(object unidad);

  /// <summary>
  /// Ejecuta la animaciÃ³n/pose segÃºn el tipo de alcance de la habilidad IA.
  /// </summary>
  protected void ReproducirAnimacionSegunTipo(bool ignorarDuplicado = false)
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad == null)
    {
      return;
    }

    bool usarAtaque = fuerzaPoseAtaque || esMelee;
    if (usarAtaque)
    {
      if (!ignorarDuplicado && scEstaUnidad.EsAnimacionAtaqueRecienteDesde(inicioSecuenciaVisual, VentanaAnimacionDuplicada)) { return; }
      scEstaUnidad.ReproducirAnimacionAtaque(true);
    }
    else
    {
      if (!ignorarDuplicado && scEstaUnidad.EsAnimacionHabilidadRecienteDesde(inicioSecuenciaVisual, VentanaAnimacionDuplicada)) { return; }
      scEstaUnidad.ReproducirAnimacionHabilidadNoHostil(true);
    }
  }

  public Task EsperarSecuenciaVisualAsync()
  {
    lock (secuenciaVisualLock)
    {
      return secuenciaVisualEnCurso ?? Task.CompletedTask;
    }
  }

  public virtual List<object> ListaHayObjetivosAlAlcance() //Si se devuelve vacia, es porque no hay  if (lCasillasTotal.Count == 0)
  {
    casillaOrigen = gameObject.GetComponent<Unidad>().CasillaPosicion;
    objPosibles.Clear();

    if (esHostil)//Habilidad Hostil
    {
      int alcanceReal = hAlcance;

      if (esMelee)
      {
        if (Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
        {
          alcanceReal += DeterminarAlcanceMeleeSegunColumnasOcupadas();
        }

        if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
        {
          alcanceReal += DeterminarAlcanceMeleeSegunColumnasOcupadas() + 1;
        }
      }

      foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
      {
        if (cas.lado != casillaOrigen.lado) //Solo incluye casillas del lado opuesto, al ser hostil
        {
          //Controles salida
          if (cas.Presente != null)
          {
            if (cas.Presente.GetComponent<Unidad>() == null && cas.Presente.GetComponent<Obstaculo>() == null)
            {  //Si la casilla no posee una unidad u obstaculo, se descarta

              continue;
            }

            if (cas.Presente.GetComponent<Obstaculo>() != null && afectaObstaculos == false) //Si no afecta obstaculos y la casilla tiene uno, la descarta
            {

              continue;
            }

            if (cas.Presente.GetComponent<Unidad>() != null) //Si la unidad esta escondida, la descarta
            {
              if (cas.Presente.GetComponent<Unidad>().ObtenerEstaEscondido() > 0 && !Usuario.GetComponent<IAUnidad>().bPuedeVerEscondidos)
              {
                continue;
              }

            }
          }
          //---
          cas.CalcularDistanciaACasilla(casillaOrigen, out int vertY, out int horX, out bool vLado);


          if (cas.Presente != null)
          {
            if (cas.Presente.GetComponent<Unidad>() != null)
            {

              if (gameObject.GetComponent<IAUnidad>().tendenciaMovY != 0)
              {


                int varYcontrol = vertY;
                if (esMelee) { varYcontrol += (varYcontrol >= 0) ? horX * 3 : -horX * 3; }

                // print("Calculo " +varYcontrol+" "+cas.Presente.GetComponent<Unidad>().uNombre);

                //  print("11 VET "+ Math.Abs(varYcontrol) + "<" +Math.Abs(gameObject.GetComponent<IAUnidad>().tendenciaMovY)+" "+cas.Presente.GetComponent<Unidad>().uNombre);

                if (Math.Abs(varYcontrol) < Math.Abs(gameObject.GetComponent<IAUnidad>().tendenciaMovY))
                {
                  gameObject.GetComponent<IAUnidad>().tendenciaMovY = vertY;

                }
              }
              else
              {
                //   print("22 VET " +gameObject.GetComponent<IAUnidad>().tendenciaMovY+" "+cas.Presente.GetComponent<Unidad>().uNombre+" estab  "+ vertY);
                gameObject.GetComponent<IAUnidad>().tendenciaMovY = vertY;
              }

            }

          }


          int distY = Math.Abs(vertY);
          int distX = Math.Abs(horX);



          if (distY > hAncho)  //No está al alcance de ancho
          {

            continue;
          }



          if (distX > alcanceReal) //No está al alcance de largo
          {
            gameObject.GetComponent<IAUnidad>().tendenciaMovX = horX; //Si la habilidad se quedó corta, se pone en tendencia por cuanto

            continue;
          }


          /* if(cas.Presente != null)
           {
             if(cas.Presente.GetComponent<Unidad>() == null ) //Si la casilla no posee una unidad, se descarta
             {  
               print("salida problema");
                continue;
             }
           }*/

      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Unidad>() != null)
        {
          objPosibles.Add(cas.Presente.GetComponent<Unidad>());
        }
        else if (cas.Presente.GetComponent<Obstaculo>() != null)
        {
          objPosibles.Add(cas.Presente.GetComponent<Obstaculo>());
        }
      }

    }
  }

      if (afectaObstaculos && objPosibles.Any(x => x is Unidad))
      {
        objPosibles.RemoveAll(x => x is Obstaculo);
      }

      return objPosibles;
    }
    else
    {

      foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
      {
        if (cas.lado == casillaOrigen.lado) //Solo incluye casillas del lado popio
        {
          if (cas.Presente != null)
          {
            if ((cas.Presente.GetComponent<Unidad>() == null) || (cas.Presente == gameObject)) //Si la casilla no posee una unidad o es ella misma, se descarta
            {
              continue;
            }
          }
        }
        else { continue; }

        cas.CalcularDistanciaACasilla(casillaOrigen, out int vertY, out int horX, out bool vLado);

        int distanciaTotal = vertY + horX;
        //print(nombre+" alcance:  "+hAlcance+"  distancia: "+distanciaTotal+" ("+vertY+" + "+horX+")");
        if (hAlcance < Math.Abs(distanciaTotal))
        {

          gameObject.GetComponent<IAUnidad>().tendenciaMovX = (casillaOrigen.posX > cas.posX) ? -1 : 1;
          gameObject.GetComponent<IAUnidad>().tendenciaMovY = (casillaOrigen.posY > cas.posY) ? -1 : 1;

          continue;
        }

        if (cas.Presente != null)
        {
          if (cas.Presente.GetComponent<Unidad>() == null) //Si la casilla no posee una unidad, se descarta
          {
            continue;
          }
        }

        if (cas.Presente != null)
        {
          objPosibles.Add(cas.Presente.GetComponent<Unidad>());
        }



      }





    }
    return objPosibles;
  }

  int DeterminarAlcanceMeleeSegunColumnasOcupadas()
  {
    LadoManager scLado = casillaOrigen.ladoOpuesto.GetComponent<LadoManager>();

    int posYorigen = scEstaUnidad.CasillaPosicion.posY;


    List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
    List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

    foreach (Transform child in casillaOrigen.ladoOpuesto.transform) //Itera en cada casilla del lado opuesto
    {
      Casilla cas = child.GetComponent<Casilla>();

      if (cas.posX == 3) //Columna 1 (frente)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna1.Add(cas);
        }
      }

      if (cas.posX == 2) //Columna 2 (medio)
      {
        int calculo = Math.Abs(cas.posY - posYorigen); //distancia en Y al origen para calcular adyacentes o frontal

        if (calculo < 2)
        {
          casillasAdyacentesyFrenteColumna2.Add(cas);
        }
      }


    }

    //Se fija si las 3 casillas de la columna 1 están vacias
    foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
    {
      if (cas.bTieneUnidadoObstaculoParaMelee()) //si alguna de las 3 tiene algo, no aumenta el rango melee
      {
        return 0;
      }
    }





    foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
    {
      if (cas.bTieneUnidadoObstaculoParaMelee()) //y si alguna de las 3 tiene algo, aumenta solo en 1 
      {
        return 1;
      }
    }


    return 2; //si ninguna de las 2 columnas tiene algo, aumenta al maximo
  }

  //Ataque vs Defensa convencional
  public int TiradaAtaque(float defensaObjetivo, float atributoAtaca, float modificadorHabilidadaAtaque, float modificadorDadoCritico, Unidad unidadAtacada, float tiradaAtaque = -1, int rangoPifiaExtra = 0)
  {
   //Pifia = -1
    //Fallo = 0
    //Roce = 1
    //Golpe = 2
    //Crítico = 3
    //Si la habilidad es hostil, no Discreta, y tiene Escondido tier 1, se revela
    
    if (scEstaUnidad.ObtenerEstaEscondido() > 0)
    {
      scEstaUnidad.PerderEscondido();
    }

    int resultado = 0;
    float iTiradaAtaque = 0;
    float tiradaBase = tiradaAtaque;
    
    if (tiradaAtaque == -1)
    {
      iTiradaAtaque = UnityEngine.Random.Range(1, 21);
      tiradaBase = iTiradaAtaque;
    }
    else { iTiradaAtaque = tiradaAtaque; }
    float iResultadoAtaque = iTiradaAtaque + atributoAtaca + modificadorHabilidadaAtaque;

    string objetivoNombre = unidadAtacada != null ? unidadAtacada.uNombre : TRADU.i.Traducir("objetivo");
    float limitePifia = 2 + rangoPifiaExtra;
    int umbralPifia = Mathf.RoundToInt(limitePifia - 1);
    float umbralCritico = 19 - modificadorDadoCritico;
    string textoResultado;
    CombatLogFormatter.CombatOutcome outcome;

    if (iTiradaAtaque < limitePifia)//Pifia
    {
      scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir("Pifia"), Color.red);
      textoResultado = TRADU.i.Traducir("Pifia");
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          0f,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Pifia,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico)));

      return -1;
    }

    if (iTiradaAtaque >= umbralCritico)
    {
      textoResultado = TRADU.i.Traducir("Crítico");
      ScreenFlash.FlashCritical();
      BattleManager.Instance.EscribirLog(
        CombatLogFormatter.FormatearAtaque(
          scEstaUnidad.uNombre,
          objetivoNombre,
          tiradaBase,
          iTiradaAtaque,
          atributoAtaca,
          modificadorHabilidadaAtaque,
          0f,
          defensaObjetivo,
          textoResultado,
          CombatLogFormatter.CombatOutcome.Critico,
          umbralPifia,
          Mathf.RoundToInt(umbralCritico),
          0f,
          TRADU.i.Traducir("Impacto crítico")));
      return 3; //Golpe crítico
    }



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
      unidadAtacada.GenerarTextoFlotante(TRADU.i.Traducir("Fallo"), Color.grey, FloatingTextContext.Miss);
    }

    BattleManager.Instance.EscribirLog(
      CombatLogFormatter.FormatearAtaque(
        scEstaUnidad.uNombre,
        objetivoNombre,
        tiradaBase,
        iTiradaAtaque,
        atributoAtaca,
        modificadorHabilidadaAtaque,
        0f,
        defensaObjetivo,
        textoResultado,
        outcome,
        umbralPifia,
        Mathf.RoundToInt(umbralCritico)));



    if (resultado < 2 && esMelee)
    {
      Vector3 pos = scEstaUnidad != null ? scEstaUnidad.transform.position : transform.position;
      AudioSource.PlayClipAtPoint(BattleManager.Instance.contenedorPrefabs.sonidoErrar, pos);
    }

    return resultado;
  }

  //Tiradas Salvacion vs Atributo 
  /*
  public bool TiradaSalvacion(float atributoDefiende, float atributoAtaca, float modificadorHabilidadaAtaque)
  {
    bool resultado = false;

    float iTiradaAtaque = UnityEngine.Random.Range(1,21);
    float iTiradaDefensa = UnityEngine.Random.Range(1,21);

    float iResultadoAtaque = iTiradaAtaque + atributoAtaca + modificadorHabilidadaAtaque;
    float iResultadoDefensa = iTiradaDefensa + atributoDefiende;


    resultado = iResultadoAtaque > iResultadoDefensa;

    return resultado;
  }*/

  int TieneObstaculooUnidadAdelanteDeSuLado()
  {
    int orX = casillaOrigen.posX;
    int orY = casillaOrigen.posY;
    GameObject lado = casillaOrigen.ladoGO;


    if (orX != 2) //solamente relevante en la columna del medio
    {
      return 0;
    }

    Casilla casillaRevisar = null;
    foreach (Transform child in lado.transform)
    {
      Casilla cas = child.GetComponent<Casilla>();
      if ((cas.posY == orY) && (cas.posX == orX + 1))
      {
        casillaRevisar = cas;
      }

    }

    if (casillaRevisar.Presente != null)
    {
      if (casillaRevisar.Presente.GetComponent<Unidad>() != null)
      {
        return 1; //Devuelve 1 si es unidad
      }

      if (casillaRevisar.Presente.GetComponent<Obstaculo>() != null)
      {
        if (casillaRevisar.Presente.GetComponent<Obstaculo>().bPermiteAtacarDetras)
        {
          return 2; //Devuelve 2 si es obstaculo
        }
        else { return 0; }
      }
    }
    return 0; //Devuelve 0 si no hay nada 
  }
protected List<object> unidadesNoParticipantes; // Lo almacenamos por si hace falta desombrear después

  protected async void PrepararInicioAnimacion(List<object> objetivos, object solo)
  {
    TaskCompletionSource<bool> visualTcs = new TaskCompletionSource<bool>();
    lock (secuenciaVisualLock)
    {
      secuenciaVisualEnCurso = visualTcs.Task;
    }

    try
    {
      inicioSecuenciaVisual = Time.time;
      if (scEstaUnidad == null)
      {
        scEstaUnidad = GetComponent<Unidad>();
      }

      if (scEstaUnidad != null)
      {
        scEstaUnidad.SetSuprimirAnimacionIA(true);
      }

      // Log de uso de habilidad de IA
      if (BattleManager.Instance != null && scEstaUnidad != null)
      {
        string nombreHab = TRADU.i != null ? TRADU.i.Traducir(nombre) : nombre;
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + TRADU.i.Traducir(" usa ") + TRADU.i.Traducir(nombreHab) + ".</color>");
      }
    unidadesNoParticipantes = new List<object>(BattleManager.Instance.lUnidadesTotal);
    unidadesNoParticipantes.Remove(scEstaUnidad);

    scEstaUnidad.GenerarTextoFlotante("<i>" + TRADU.i.Traducir(nombre) + "</i>", Color.red);

    if (objetivos != null && objetivos.Count > 0)
    {

      foreach (var objetivo in objetivos)
      {
        unidadesNoParticipantes.Remove(objetivo);
      }
    }

    if (solo != null)
    {
      unidadesNoParticipantes.Remove(solo);
    }

    List<Obstaculo> obstaculosObjetivos = new List<Obstaculo>();
    if (objetivos != null && objetivos.Count > 0)
    {
      obstaculosObjetivos.AddRange(objetivos.FindAll(x => x is Obstaculo).ConvertAll(x => (Obstaculo)x));
    }
    if (solo is Obstaculo obstaculoSolo)
    {
      obstaculosObjetivos.Add(obstaculoSolo);
    }
    foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }
      if (!obstaculosObjetivos.Contains(obstaculo))
      {
        unidadesNoParticipantes.Add(obstaculo);
      }
    }

    BattleManager.Instance.SombrearANoParticipantesHabilidad(unidadesNoParticipantes);

    // Aproximación visual si es melee; usa primer objetivo o "solo"
    object objetivoVisual = solo ?? (objetivos != null && objetivos.Count > 0 ? objetivos[0] : null);
    bool seAproximo = await IntentarAproximarVisualMeleeAsync(objetivoVisual);
    if (seAproximo && PausaPostAproximacionMs > 0)
    {
      await Task.Delay(PausaPostAproximacionMs);
    }

    ReproducirAnimacionSegunTipo(true);

    if (PausaAntesVolverMs > 0)
    {
      await Task.Delay(PausaAntesVolverMs);
    }
    await VolverTrasAproximacionVisualAsync(seAproximo, true);

      BattleManager.Instance.DesombrearANoParticipantesHabilidad(unidadesNoParticipantes);
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"[IAHabilidad] Error en PrepararInicioAnimacion: {ex.Message}");
    }
    finally
    {
      if (scEstaUnidad != null)
      {
        scEstaUnidad.SetSuprimirAnimacionIA(false);
      }

      visualTcs.TrySetResult(true);
    }
}

  protected Task<bool> IntentarAproximarVisualMeleeAsync(object objetivo, bool mantenerAdelante = false)
  {
    MeleeApproachMover mover = MeleeApproachMover.ObtenerOCrear(scEstaUnidad);
    if (mover == null)
    {
      return Task.FromResult(false);
    }

    return mover.PrepararAproximacionIAAsync(esMelee, hAncho, objetivo, mantenerAdelante);
  }

  protected Task VolverTrasAproximacionVisualAsync(bool seAproximo, bool forzar = false)
  {
    MeleeApproachMover mover = MeleeApproachMover.ObtenerOCrear(scEstaUnidad);
    if (!seAproximo || mover == null)
    {
      return Task.CompletedTask;
    }

    return mover.VolverAPosicionInicialAsync(forzar);
  }

  /// <summary>
  /// Ejecuta la habilidad con aproximaci󮠶isual si es melee. Reduce boilerplate en las IA melee.
  /// </summary>
  protected async Task EjecutarMeleeConAproximacionAsync(object objetivo, Func<Task> resolver, bool mantenerAdelante = false, bool forzarRetornoDespues = true)
  {
    bool seAproximo = await IntentarAproximarVisualMeleeAsync(objetivo, mantenerAdelante);
    if (resolver != null)
    {
      await resolver();
    }
    await VolverTrasAproximacionVisualAsync(seAproximo, forzarRetornoDespues || !mantenerAdelante);
  }

  protected bool TieneOtraHabilidadMeleeDisponible(float apDisponiblePost)
  {
    IAHabilidad[] habilidades = scEstaUnidad.GetComponents<IAHabilidad>();
    foreach (var hab in habilidades)
    {
      if (hab == this) continue;
      if (!hab.esMelee) continue;
      if (hab.hActualCooldown > 0) continue;
      if (apDisponiblePost < hab.costoAP) continue;
      return true;
    }
    return false;
  }



}
