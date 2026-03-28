using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Analytics;
using System.Linq;


public class Casilla : MonoBehaviour
{
  public int lado; //1 A Enemigo  -   2 B PC
  public int posX;
  public int posY;

  public int costoMovimiento = 1;
  public GameObject ladoOpuesto;
  public GameObject ladoGO;

  public GameObject Presente; // El GameObject presente en la casilla

  public bool TEST = false;
  public int TESTINT;
  public int TESTINT2;

  public GameObject TESTGO;

  public GameObject MarcaMovX0Y1;
  public GameObject MarcaMovX1Y1;
  public GameObject MarcaMovX1Y0;
  public GameObject MarcaMovXv1Y1;
  public GameObject MarcaMovXv1Y0;
  public GameObject MarcaMovX1Yv1;
  public GameObject MarcaMovX0Yv1;
  public GameObject MarcaMovXv1Yv1;
  public GameObject MarcaMeleeAtraviesa;
 
  void Start()
  {
    BattleManager.Instance.OnRondaNueva += BattleManager_OnRondaNueva;
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(false);
    }
  }


  private void BattleManager_OnRondaNueva(object sender, EventArgs empty)
  {

    //---

  }

  private bool TryGetUnidadActiva(out Unidad unidad)
  {
    unidad = BattleManager.Instance != null ? BattleManager.Instance.unidadActiva : null;
    return unidad != null && unidad.CasillaPosicion != null;
  }

  public bool PuedeIntercambiarConUnidadActiva()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.bOcupado)
    {
      return false;
    }

    if (!BattleManager.Instance.lCasillasMovimiento.Contains(this))
    {
      return false;
    }

    if (!TryGetUnidadActiva(out Unidad unidadActiva))
    {
      return false;
    }

    if (unidadActiva.movimientoEnCurso || BattleManager.Instance.SeleccionandoObjetivo || unidadActiva.estado_inmovil > 0)
    {
      return false;
    }

    if (Presente == null)
    {
      return false;
    }

    Unidad unidadPresente = Presente.GetComponent<Unidad>();
    if (unidadPresente == null || unidadPresente.CasillaPosicion == null)
    {
      return false;
    }

    if (unidadPresente.CasillaPosicion.lado != unidadActiva.CasillaPosicion.lado)
    {
      return false;
    }

    if (unidadPresente.estado_inmovil > 0 || unidadPresente.TieneBuffNombre("Desplazado"))
    {
      return false;
    }

    int costoMovimientoTotal = costoMovimiento;
    int diferenciaX = Mathf.Abs(unidadActiva.CasillaPosicion.posX - posX);
    int diferenciaY = Mathf.Abs(unidadActiva.CasillaPosicion.posY - posY);
    if (diferenciaX == 1 && diferenciaY == 1)
    {
      costoMovimientoTotal++;
    }

    return unidadActiva.ObtenerAPActual() >= costoMovimientoTotal;
  }

  public void ActualizarSenialadores()
  {
    Invoke("ActualizarSenialadoresmetod", 0.05f);
  }

  public void ActualizarSenialadoresmetod()
  { 
     if (!TryGetUnidadActiva(out Unidad unidad))
    {
      DesactivarSenialadores();
      return;
    }

     if (unidad.CasillaPosicion != this || unidad.GetComponent<IAUnidad>() != null)
    {
      DesactivarSenialadores();
      return;
    }

    ActualizarSenialadorDireccion(0, 1, MarcaMovX0Y1);
    ActualizarSenialadorDireccion(1, 1, MarcaMovX1Y1);
    ActualizarSenialadorDireccion(1, 0, MarcaMovX1Y0);
    ActualizarSenialadorDireccion(-1, 1, MarcaMovXv1Y1);
    ActualizarSenialadorDireccion(-1, 0, MarcaMovXv1Y0);
    ActualizarSenialadorDireccion(1, -1, MarcaMovX1Yv1);
    ActualizarSenialadorDireccion(0, -1, MarcaMovX0Yv1);
    ActualizarSenialadorDireccion(-1, -1, MarcaMovXv1Yv1);


  }

  public void DesactivarSenialadores()
  {
    if (MarcaMovX0Y1 != null) MarcaMovX0Y1.SetActive(false);
    if (MarcaMovX1Y1 != null) MarcaMovX1Y1.SetActive(false);
    if (MarcaMovX1Y0 != null) MarcaMovX1Y0.SetActive(false);
    if (MarcaMovXv1Y1 != null) MarcaMovXv1Y1.SetActive(false);
    if (MarcaMovXv1Y0 != null) MarcaMovXv1Y0.SetActive(false);
    if (MarcaMovX1Yv1 != null) MarcaMovX1Yv1.SetActive(false);
    if (MarcaMovX0Yv1 != null) MarcaMovX0Yv1.SetActive(false);
    if (MarcaMovXv1Yv1 != null) MarcaMovXv1Yv1.SetActive(false);






  }

  private void ActualizarSenialadorDireccion(int deltaX, int deltaY, GameObject marca)
  {
    if (marca == null) { return; }
    if (!TryGetUnidadActiva(out Unidad unidad))
    {
      marca.SetActive(false);
      return;
    }

    if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) > 1)
    {
      if (unidad.ObtenerAPActual() <= 1)
      {
         marca.SetActive(false);
        return;
      }
    }

    if (unidad.ObtenerAPActual() <= 0)
    { marca.SetActive(false);
      return;
   }

    Casilla destino = EncontrarCasillaEnPosicion(posX + deltaX, posY + deltaY);
    bool puedeMover = false;

    if (destino != null)
    {
      if (destino.Presente == null)
      {
        puedeMover = true;
      }
      else
      {
        if (destino.Presente.GetComponent<Obstaculo>() != null)
        {
          puedeMover = false;
        }
        else
        {
          Unidad unidadDestino = destino.Presente.GetComponent<Unidad>();
          if (unidadDestino != null && !unidadDestino.TieneBuffNombre("Desplazado"))
          {
            puedeMover = true;
          }
        }
      }
    }

    marca.SetActive(puedeMover);
  }

  public TooltipBatalla scTooltipBatalla;
  private bool EsMovimientoDiagonal(Unidad unidad)
  {
    if (unidad == null || unidad.CasillaPosicion == null)
    {
      return false;
    }

    int diferenciaX = Mathf.Abs(unidad.CasillaPosicion.posX - posX);
    int diferenciaY = Mathf.Abs(unidad.CasillaPosicion.posY - posY);
    return diferenciaX == 1 && diferenciaY == 1;
  }

  private bool DebeSumarCostoBarroAlEntrar(Unidad unidad)
  {
    if (unidad == null)
    {
      return false;
    }

    Trampa trampa = GetComponent<Trampa>();
    if (trampa is not TrampaBarro trampaBarro)
    {
      return false;
    }

    if (unidad.inmunidad_Trampas && !trampaBarro.esTrampaFavorable)
    {
      return false;
    }

    REPRESENTACIONPasoCauteloso pasoCauteloso = unidad.GetComponent<REPRESENTACIONPasoCauteloso>();
    if (pasoCauteloso != null && !pasoCauteloso.seusoEsteTurno)
    {
      return false;
    }

    return true;
  }

  private int ObtenerCostoMovimientoTotal(Unidad unidad)
  {
    int costoMovimientoTotal = costoMovimiento;
    if (EsMovimientoDiagonal(unidad))
    {
      costoMovimientoTotal++;
    }
    if (DebeSumarCostoBarroAlEntrar(unidad))
    {
      costoMovimientoTotal += 2;
    }
    return costoMovimientoTotal;
  }

  private string ObtenerTextoCostoMovimiento(Unidad unidad)
  {
    return TRADU.i.Traducir("Coste: ") + ObtenerCostoMovimientoTotal(unidad) + " " + TRADU.i.Traducir("PA");
  }
  public void MostrarAPparaMovimiento()
  {
    if (lado == 1) { return; } //Solo para aliados
    if (BattleManager.Instance == null || BattleManager.Instance.bOcupado) { return; }
    if (!TryGetUnidadActiva(out Unidad unidad)) { return; }

    //Unidad seleccionada - Movimiento
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente == null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      if (unidad.ObtenerAPActual() >= costoMovimientoTotal)
      {
        string text = ObtenerTextoCostoMovimiento(unidad);
        scTooltipBatalla.ShowTooltipTextSinAnim(text);
      }
      else
      {
        string text = "<color=red>" + ObtenerTextoCostoMovimiento(unidad) + "</color>";
        scTooltipBatalla.ShowTooltipTextSinAnim(text);

      }
    }


  }

  public void MostrarTooltipIntercambiar()
  {
    if (scTooltipBatalla == null) { return; }

    string texto = TRADU.i != null ? TRADU.i.Traducir("Intercambiar") : "Intercambiar";
    scTooltipBatalla.ShowTooltipTextSinAnim(texto);
  }

  public async void OnMouseDown()
  {
    if (BattleManager.Instance == null)
    {
      return;
    }

    //----
    if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() < 6)
    {

      return;
    }
    if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() == 6 && (posX != 3 || posY != 3))
    {

      return;
    }
    else if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() == 6)
    {
      BattleManager.Instance.scTutorialCombate.SiguientePasoCombate();
    }

    if (!TryGetUnidadActiva(out Unidad unidad))
    {
      return;
    }



    // --- Cancelar habilidad activa si se hace clic en el campo ---
    if (BattleManager.Instance.HabilidadActiva != null)
    {
      if (BattleManager.Instance.HabilidadActiva.esHostil && unidad.CasillaPosicion.lado == this.lado)
      {
        // Cancela la selección de la habilidad al clikear casilla, solo si es hostil y es una casilla del mismo lado
        BattleManager.Instance.HabilidadActiva = null;
        BattleManager.Instance.SeleccionandoObjetivo = false;
        BattleManager.Instance.LimpiarCapasCasillas();
        BattleManager.Instance.scUIContadorAP.ResetearCirculos();
        BattleManager.Instance.scUIBotonesHab.DeseleccionarTodas();
        // return; // Sale sin ejecutar nada más
      }
    }





    //Unidad seleccionada - Movimiento
    //!!!
    unidad.CasillaPosicion.CalcularDistanciaACasilla(this, out int x, out int y, out bool lado);
    //!!!
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente == null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      if (unidad.ObtenerAPActual() >= costoMovimientoTotal)
      {

        unidad.CambiarAPActual(-costoMovimientoTotal);
        BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
        unidad.CasillaDeseadaMov = this;
        RuntimeAnalytics.TrackDesign("combat", "move_confirmed", "step");
        await unidad.GenerarTextoFlotante("<size=70%>-" + costoMovimientoTotal + " " + TRADU.i.Traducir(" PA") + "</size>", new Color(1.0f, 0.5f, 0.0f)); // Naranja
      }
    }
    // Intercambio con aliado: mover a casilla ocupada por aliado y que el aliado vaya a la casilla original
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente != null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      Unidad aliado = Presente != null ? Presente.GetComponent<Unidad>() : null;
      if (aliado != null)
      {
        if (aliado.CasillaPosicion.lado != unidad.CasillaPosicion.lado)
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con enemigos."));
          return;
        }
        else if (aliado.estado_inmovil > 0)
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con una unidad inmovilizada."));
          return;
        }
        else if (aliado.TieneBuffNombre("Desplazado"))
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con una unidad que ya está Desplazada."));
          return;
        }
        else
        {

          int costoMovimientoTotal = costoMovimiento;
          int diferenciaX = Mathf.Abs(unidad.CasillaPosicion.posX - posX);
          int diferenciaY = Mathf.Abs(unidad.CasillaPosicion.posY - posY);
          if (diferenciaX == 1 && diferenciaY == 1) { costoMovimientoTotal++; }

          if (unidad.ObtenerAPActual() >= costoMovimientoTotal)
          {
            unidad.CambiarAPActual(-costoMovimientoTotal);
            BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

            Casilla origen = unidad.CasillaPosicion;

            // Forzar movimiento del aliado hacia el origen del activo
            aliado.CasillaForzadoaMover = origen;

            // Aplicar Debuff Desplazado (-1 AP max por 1 turno)
            Buff desplazado = new Buff();
            desplazado.buffNombre = "Desplazado";
            desplazado.buffDescr = "AP máx -1 por 1 turno";
            desplazado.boolfDebufftBuff = false;
            desplazado.cantAPMax = -1;
            desplazado.DuracionBuffRondas = 1;
            desplazado.AplicarBuff(aliado);
            Buff buffComponent = ComponentCopier.CopyComponent(desplazado, aliado.gameObject);

            // Mover la unidad activa a esta casilla
            unidad.CasillaDeseadaMov = this;
            RuntimeAnalytics.TrackDesign("combat", "move_confirmed", "swap");
          }
          else
          {
            BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No tienes PA suficientes para intercambiar."));
          }
        }
      }
      else
      {
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con obstáculos."));
      }

    }

    //Para habilidades en área
    if (BattleManager.Instance.HabilidadActiva != null && !BattleManager.Instance.bOcupado)
    {

      if (BattleManager.Instance.HabilidadActiva.enArea > 0 || BattleManager.Instance.HabilidadActiva.targetEspecial > 0 && BattleManager.Instance.SeleccionandoObjetivo)
      {

        if (!BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this)) { return; } //Si no está en el área, no hace nada


        List<Unidad> lUnidadesEnArea = new List<Unidad>();
        List<Obstaculo> lObstaculosEnArea = new List<Obstaculo>();

        foreach (Unidad enAzul in unidadesEnCasAzul)
        {
          if (lUnidadesEnArea.Contains(enAzul) == false) //Que no se duplique
          { lUnidadesEnArea.Add(enAzul); }
        }
        foreach (Obstaculo enAzul in obstaculosEnCasAzul)
        {
          if (lObstaculosEnArea.Contains(enAzul) == false) //Que no se duplique
          { lObstaculosEnArea.Add(enAzul); }
        }

        foreach (Casilla u in BattleManager.Instance.HabilidadActiva.lCasillasafectadas)
        {
          if (u.Presente != null)
          {
            if (u.Presente.GetComponent<Unidad>() != null)
            {
              if (unidadesEnCasAzul.Contains(u.Presente.GetComponent<Unidad>()) && lUnidadesEnArea.Contains(u.Presente.GetComponent<Unidad>()) == false)
              {
                lUnidadesEnArea.Add(u.Presente.GetComponent<Unidad>());
              }

            }
            if (u.Presente.GetComponent<Obstaculo>() != null)
            {
              if (obstaculosEnCasAzul.Contains(u.Presente.GetComponent<Obstaculo>()) && lObstaculosEnArea.Contains(u.Presente.GetComponent<Obstaculo>()) == false)
              {
                lObstaculosEnArea.Add(u.Presente.GetComponent<Obstaculo>());
              }
            }

          }


        }

        if (BattleManager.Instance.HabilidadActiva.poneTrampas) //Habilidad que pone "trampas" en casillas
        {
          BattleManager.Instance.casillaClickHabilidad = this;
          await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
          BattleManager.Instance.casillaClickHabilidad = null;
        }
        else if (BattleManager.Instance.HabilidadActiva.poneObstaculo)
        {
          if (Presente == null) // Si es habilidad que pone obstaculo, debe estar vacia la casilla
          {
            BattleManager.Instance.casillaClickHabilidad = this;
            await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
            BattleManager.Instance.casillaClickHabilidad = null;
          }
        }
        else //Habilidades Normales
        {
          // List<object> listResolverUnidades = new List<object>();
          // listResolverUnidades.AddRange(lUnidadesEnArea); print(11);
          // await BattleManager.Instance.HabilidadActiva.Resolver(listResolverUnidades);

          //---La idea es que resolver se llame 1 sola vez para evitar efectos de algunas habilidades duplicados.
          List<object> listResolverUnidades = new List<object>();
          listResolverUnidades.AddRange(lUnidadesEnArea);
          BattleManager.Instance.casillaClickHabilidad = this;
          if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
          {
            await BattleManager.Instance.HabilidadActiva.Resolver(listResolverUnidades);
          }
          else
          {
            List<object> listResolverObstaculos = new List<object>();
            listResolverObstaculos.AddRange(lObstaculosEnArea);
            List<object> combinedList = new List<object>();
            combinedList.AddRange(listResolverObstaculos);
            combinedList.AddRange(listResolverUnidades);
            await BattleManager.Instance.HabilidadActiva.Resolver(combinedList);

          }

          BattleManager.Instance.casillaClickHabilidad = null;
        }


        BattleManager.Instance.scUIBotonesHab.UIDesactivarBotones();


      }
      else if (BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.poneTrampas && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
      {
        BattleManager.Instance.casillaClickHabilidad = this;
        await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
        BattleManager.Instance.casillaClickHabilidad = null;
      }
      else if (BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.esZonal && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
      {
        List<object> objetos = BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Cast<object>().ToList();
        BattleManager.Instance.casillaClickHabilidad = this;
        await BattleManager.Instance.HabilidadActiva.Resolver(objetos, this);
        BattleManager.Instance.casillaClickHabilidad = null;
      }
      else if (BattleManager.Instance.HabilidadActiva.poneObstaculo)
      {
        if (Presente == null && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this)) // Si es habilidad que pone obstaculo, debe estar vacia la casilla
        {
          BattleManager.Instance.casillaClickHabilidad = this;
          await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
          BattleManager.Instance.casillaClickHabilidad = null;
        }
      }
    }



  }




  public bool PonerObjetoEnCasilla(GameObject GO)
  {

    if (Presente != null)
    {
      return false;
    }
    GO.transform.position = transform.position;
    NuevoObjetoPresenteEnCasilla(GO);


    if (GO.GetComponent<Unidad>() != null)
    {
      GO.GetComponent<Unidad>().CasillaPosicion = this;
    }
    if (GO.GetComponent<Obstaculo>() != null)
    {
      GO.GetComponent<Obstaculo>().CasillaPosicion = this;
    }

    //---
    // Reduce the scale of the object to 87% of its original size
    GO.transform.localScale = new Vector3(GO.transform.localScale.x * 0.9f, GO.transform.localScale.y * 0.9f, GO.transform.localScale.z * 0.9f);
    return true;

  }

  public void PonerObjetoEnCasillaAnimado(GameObject GO, int lado)
  {

    if (Presente != null)
    {
      print("Casilla Ocupada, no se puede colocar objeto");
      return;
    }

    // Iniciar la corrutina para mover el objeto de forma animada
    StartCoroutine(MoverObjetoAnimado(GO, lado));

    // Asignar el objeto como presente en la casilla
    NuevoObjetoPresenteEnCasilla(GO);

    // Si es una Unidad u Obstáculo, actualizar su casilla de posición
    if (GO.GetComponent<Unidad>() != null)
    {
      GO.GetComponent<Unidad>().CasillaPosicion = this;
    }
    if (GO.GetComponent<Obstaculo>() != null)
    {
      GO.GetComponent<Obstaculo>().CasillaPosicion = this;
    }
    // Reduce the scale of the object to 87% of its original size
    GO.transform.localScale = new Vector3(GO.transform.localScale.x * 0.9f, GO.transform.localScale.y * 0.9f, GO.transform.localScale.z * 0.9f);
  }

  IEnumerator MoverObjetoAnimado(GameObject GO, int lado)
  {
    if (lado == 2)//Enemigos
    {
      Vector3 posicionFinal = transform.position; // Posición de la casilla
      Vector3 posicionInicial = posicionFinal + new Vector3(3f, 0, 0); // Posición inicial (desplazada a la derecha)

      float duracion = 0.7f; // Duración del movimiento
      float tiempo = 0;

      while (tiempo < duracion)
      {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / duracion); // Normalizar el tiempo (0 a 1)

        // Interpolar la posición entre la inicial y la final
        GO.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

        yield return null; // Esperar al siguiente frame
      }

      // Asegurarse de que el objeto está exactamente en la posición final
      GO.transform.position = posicionFinal;
    }
    else if (lado == 1) //Aliados
    {
      Vector3 posicionFinal = transform.position; // Posición de la casilla
      Vector3 posicionInicial = posicionFinal + new Vector3(-3f, 0, 0); // Posición inicial (desplazada a la izquierda)

      float duracion = 0.7f; // Duración del movimiento
      float tiempo = 0;

      while (tiempo < duracion)
      {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / duracion); // Normalizar el tiempo (0 a 1)

        // Interpolar la posición entre la inicial y la final
        GO.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

        yield return null; // Esperar al siguiente frame
      }

      // Asegurarse de que el objeto está exactamente en la posición final
      GO.transform.position = posicionFinal;
    }
  }




  public List<Casilla> ObtenerCasillasAlrededor(int x)
  {
    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    // Recorre las casillas en el rango especificado (x)
    for (int i = -x; i <= x; i++)
    {
      for (int j = -x; j <= x; j++)
      {
        // Calcula las coordenadas de la casilla vecina
        int xVecina = posXActual + i;
        int yVecina = posYActual + j;

        // Verifica si la casilla vecina está dentro del rango especificado (distancia x)
        if (Mathf.Abs(xVecina - posXActual) + Mathf.Abs(yVecina - posYActual) <= x)
        {
          // Asegúrate de no agregar la casilla actual a la lista
          if (xVecina == posXActual && yVecina == posYActual)
            continue;

          // Si x es 1, agrega las casillas adyacentes (sin diagonales)
          if (x == 1 && (Mathf.Abs(i) == 1 || Mathf.Abs(j) == 1) && !(Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1))
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
          // Si x es 2, agrega las casillas adyacentes y las diagonales inmediatas
          else if (x == 2 && Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
          // Si x es mayor que 2, agrega las casillas no diagonales a distancia 2
          else if (x > 2 && ((Mathf.Abs(i) == 2 && Mathf.Abs(j) != 2 || Mathf.Abs(j) == 2 && Mathf.Abs(i) != 2) || (Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)))
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
        }
      }
    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasenMismaFila()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posY == posY && cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }

  public Casilla ObtenerCasillasMasAtrasEnFila()
  {
    Casilla lCas = this;
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posY == posY && cas.lado == lado)
      {
        if (cas.posX < posX)
        {
          lCas = cas;

        }

      }

    }

    return lCas;
  }

  public List<Casilla> ObtenerCasillasMismoLado()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasLadoOpuesto()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.lado != lado)
      {
        lCasillas.Add(cas);

      }

    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasenMismaColumna()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posX == posX && cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }
  public List<Casilla> ObtenerCasillasAdyacentesEnColumna()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posX == posX && cas.lado == lado)
      {
        // Comprueba si la casilla es adyacente en la columna (diferencia de 1 en posY)
        if (Mathf.Abs(cas.posY - posY) == 1)
        {
          lCasillas.Add(cas);
        }
      }

    }

    return lCasillas;

  }
  public List<Casilla> ObtenerCasillasAlrededorParaMovimiento()
  {
    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    // Recorre las casillas en el rango especificado (x)
    for (int i = -1; i <= 1; i++)
    {
      for (int j = -1; j <= 1; j++)
      {
        // Asegúrate de no agregar la casilla actual a la lista
        if (i == 0 && j == 0)
          continue;

        // Calcula las coordenadas de la casilla vecina
        int xVecina = posXActual + i;
        int yVecina = posYActual + j;

        // Agrega la casilla vecina a la lista si está dentro del rango especificado (distancia x)
        if (Mathf.Abs(xVecina - posXActual) <= 1 && Mathf.Abs(yVecina - posYActual) <= 1)
        {
          // Encuentra la casilla vecina en la posición (xVecina, yVecina) y agrégala a la lista
          Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
          if (casillaVecina != null)
          {
            lCasillas.Add(casillaVecina);
          }
        }
      }
    }

    return lCasillas;
  }
  public List<Casilla> ObtenerCasillasRango(int alcance, int ancho/*0 es en la misma fila, 1 tmb en adyacentes*/) //Segun la posicion en su lado obtiene hasta que casillas del lado opuesto llega la habilidad según el alcance
  {

    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    int RangoEnOtroLado = alcance - (3 - posX);
    lCasillas = ladoOpuesto.GetComponent<LadoManager>().filaCasillasSegunRango(posY, RangoEnOtroLado, ancho);



    return lCasillas;
  }

  private Casilla EncontrarCasillaEnPosicion(int posX, int posY)
  {
    // Obtenemos el transform del padre de las casillas (supongamos que todas las casillas están en el mismo padre)
    Transform padreDeCasillas = transform.parent;

    // Recorremos todos los objetos hijos del padre
    foreach (Transform hijo in padreDeCasillas)
    {
      // Comprobamos si el hijo tiene un componente Casilla
      Casilla casilla = hijo.GetComponent<Casilla>();
      if (casilla != null)
      {
        // Comparamos las coordenadas
        if (casilla.posX == posX && casilla.posY == posY)
        {
          // Devolvemos la casilla encontrada
          return casilla;
        }
      }
    }

    // Si no se encontró ninguna casilla en la posición dada, devolvemos null
    return null;
  }

  public bool bTieneUnidad()
  {
    if (Presente == null)
    {
      return false;
    }

    if (Presente.GetComponent<Unidad>() != null)
    {
      return true;
    }
    else { return false; }

  }

  public bool bTieneObstaculo()
  {
    if (Presente == null)
    {
      return false;
    }

    if (Presente.GetComponent<Obstaculo>() != null)
    {
      return true;
    }
    else { return false; }

  }

  public bool bTieneUnidadoObstaculoParaMelee()
  {
    // Compatibilidad: conserva el comportamiento previo (obstaculo siempre bloquea)
    // evaluando contra su propia fila.
    return BloqueaAvanceMeleeDesdeFila(posY);
  }

  // Regla central de avance melee:
  // - Las unidades visibles/no volando siempre bloquean.
  // - Los obstaculos solo bloquean si estan en la misma fila (posY) que el origen.
  public bool BloqueaAvanceMeleeDesdeFila(int posYorigen)
  {
    if (Presente == null)
    {
      return false;
    }

    Unidad unidad = Presente.GetComponent<Unidad>();
    if (unidad != null)
    {
      return unidad.ObtenerEstaEscondido() == 0 && !unidad.estado_Volando;
    }

    if (Presente.GetComponent<Obstaculo>() != null)
    {
      return posY == posYorigen;
    }

    return false;
  }

  public void ActivarCapaColorRojo()
  {
    transform.GetChild(1).gameObject.SetActive(true);
  }

  public void DesactivarCapaColorRojo()
  {
    transform.GetChild(1).gameObject.SetActive(false);
  }
  public void ActivarCapaColorNegro()
  {
    transform.GetChild(1).gameObject.SetActive(false); //desactiva la capa roja también
    GetComponent<MeshRenderer>().enabled = false; //desactiva la casilla en si
    transform.GetChild(2).gameObject.SetActive(true);
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(true);
    }
  }
  public void ActivarCapaColorAzul()
  {
    transform.GetChild(0).gameObject.SetActive(true);
  }
  public void DesactivarCapaColorAzul()
  {
    transform.GetChild(0).gameObject.SetActive(false);
  }



  public void DesactivarCapas()
  {
    transform.GetChild(0).gameObject.SetActive(false);
    transform.GetChild(1).gameObject.SetActive(false);
    transform.GetChild(2).gameObject.SetActive(false);
    transform.GetChild(2).gameObject.SetActive(false);
    transform.GetChild(9).gameObject.SetActive(false);
    GetComponent<MeshRenderer>().enabled = true;
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(false);
    }
    //Agregar mas
  }

  List<Casilla> casAlre = new List<Casilla>();

  [SerializeField] public List<Unidad> unidadesEnCasAzul = new List<Unidad>();
  [SerializeField] public List<Obstaculo> obstaculosEnCasAzul = new List<Obstaculo>();
  public void OnMouseOver()
  {

    MostrarAPparaMovimiento();
    unidadesEnCasAzul.Clear();
    obstaculosEnCasAzul.Clear();
    casAlre.Clear();


    string text = "";
   
    if (MarcaMelee.activeInHierarchy)
    {
       text += "" + TRADU.i.Traducir("Melee disponible");
      scTooltipBatalla.ShowTooltipTextSinAnim(text);
    }
    else if (BattleManager.Instance != null && !BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva == null && PuedeIntercambiarConUnidadActiva())
    {
      MostrarTooltipIntercambiar();
    }
    //Controlar se esta haciendo hablidad en Area, marca las casillas en la zona de alcance y en el area
      if (BattleManager.Instance.HabilidadActiva != null)
      {
        if (BattleManager.Instance.HabilidadActiva.enArea > 0 && BattleManager.Instance.SeleccionandoObjetivo)
        {

          casAlre = ObtenerCasillasAlrededor(BattleManager.Instance.HabilidadActiva.enArea);
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 1)  //Target Especial 1: misma fila (horizontal)
        {
          casAlre = ObtenerCasillasenMismaFila();
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }

        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 2)  //Target Especial 2: misma columna (Vertical)
        {
          casAlre = ObtenerCasillasenMismaColumna();
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 3) //Target Especial 3: Dos Casillas (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.posY == posY + 1 && cas.posX == posX && cas.lado == lado)
            {
              casAlre.Add(cas);
            }

          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 4) //Target Especial 4: Tres Casillas (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (((cas.posY == posY + 1 && cas.posX == posX) || (cas.posY == posY - 1 && cas.posX == posX)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 5) //Target Especial 5: Dos Casillas (Atrás)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if ((cas.posY == posY && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 6) //Target Especial 6: Tres Casillas y las de atras (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (((cas.posY == posY + 1 && cas.posX == posX) || (cas.posY == posY - 1 && cas.posX == posX)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if (((cas.posY == posY + 1 && cas.posX == posX - 1) || (cas.posY == posY - 1 && cas.posX == posX - 1)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if (cas.posY == posY && cas.posX == posX - 1 && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 7) //Target Especial 7: La del origen y diagonales adyacentes X
        {
          casAlre.Add(this); //Agrega la casilla de origen
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {

            if ((cas.posY == posY + 1 && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY + 1 && cas.posX == posX + 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX + 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 8) //Target Especial 8: T horizontal
        {

          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if ((cas.posY == posY && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY + 1 && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 9) //Target Especial 9: Pirámide invertida
        {
          // Casilla de origen (punta de la pirámide)
          casAlre.Add(this);

          // 3 casillas en la columna siguiente (posX + 1)
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.lado == lado && cas.posX == posX - 1 &&
              (cas.posY == posY - 1 || cas.posY == posY || cas.posY == posY + 1))
            {
              casAlre.Add(cas);
            }
          }

          // 5 casillas en la éltima columna (posX + 2)
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.lado == lado && cas.posX == posX - 2 &&
              (cas.posY == posY - 2 || cas.posY == posY - 1 || cas.posY == posY || cas.posY == posY + 1 || cas.posY == posY + 2))
            {
              casAlre.Add(cas);
            }
          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
            }
          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }

        //---
        if (Presente != null)
        {
          if (Presente.GetComponent<Unidad>() != null)
          {
            unidadesEnCasAzul.Add(Presente.GetComponent<Unidad>());
          }
          if (Presente.GetComponent<Obstaculo>() != null)
          {
            obstaculosEnCasAzul.Add(Presente.GetComponent<Obstaculo>());
          }
        }



      }

  }

  public void OnMouseExit()
  {
    scTooltipBatalla.HideTooltipSinAnim();
    if (BattleManager.Instance.HabilidadActiva != null)
    {
      if (BattleManager.Instance.HabilidadActiva.enArea > 0 && BattleManager.Instance.SeleccionandoObjetivo)
      { DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul(); }
      else if (BattleManager.Instance.HabilidadActiva.targetEspecial > 0)
      {
        DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul();
      }

    }
  }

  private void MarcarCasillasAzul(List<Casilla> casillasZonahab)
  {
    if (casillasZonahab.Contains(this)) //Marca casilla actual si esta en la zona de la habilidad
    {
      ActivarCapaColorAzul();
    }


    foreach (Casilla cas in casAlre) //Marca casillas alrededor si la central está en la zona de la habilidad
    {
      if (casillasZonahab.Contains(this))
      {
        cas.ActivarCapaColorAzul();
      }
    }
  }

  private void DesmarcarCasillasAlreAzul()
  {
    foreach (Casilla cas in casAlre)
    {
      cas.DesactivarCapaColorAzul();
    }
  }

  public void CalcularDistanciaACasilla(Casilla casObjetivo, out int yVert, out int xHor, out bool mismoLado)
  {


    if (lado == casObjetivo.lado) //Casillas del mismo lado
    {
      mismoLado = true;
      xHor = posX - casObjetivo.posX; //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia afuera, negativo, hacia centro.
    }
    else
    {
      mismoLado = false;
      xHor = 7 - posX - casObjetivo.posX; //La diferencia siempre va a dar positiva al estar del otro lado

    }


    yVert = (posY - casObjetivo.posY); //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia arriba, negativo, hacia abajo.

    if (Presente != null)
    {

    }
  }

  public void NuevoObjetoPresenteEnCasilla(GameObject obj)
  {

    if (obj.GetComponent<Unidad>() != null)
    {

      Presente = obj;

      // Aplicar handicap de dificultad (invisible en UI) al aparecer una unidad en esta casilla
      try
      {
        var hd = Sistema.HandicapDificultad.Instance;
        if (hd != null)
        {
          var unidad = obj.GetComponent<Unidad>();
          hd.AplicarSiCorresponde(unidad, lado);
        }
      }
      catch { }

      //------------- TRIGGER DE TRAMPAS
      if (GetComponent<Trampa>() != null)
      {
        Trampa scTramp = gameObject.GetComponent<Trampa>();
        Unidad scUnidad = obj.GetComponent<Unidad>();

        // Si la unidad es inmune y la trampa no es favorable, no aplica efectos.
        if (scUnidad != null && scUnidad.inmunidad_Trampas && !scTramp.esTrampaFavorable)
        {
          return;
        }

        bool seEvadeEfecto = false;

        if (obj.GetComponent<REPRESENTACIONPasoCauteloso>() != null)
        {
          if (!obj.GetComponent<REPRESENTACIONPasoCauteloso>().seusoEsteTurno)
          {
            obj.GetComponent<REPRESENTACIONPasoCauteloso>().seusoEsteTurno = true;
            seEvadeEfecto = true;
          }
        }

        if ((!seEvadeEfecto) /*|| (scTramp.esTrampaFavorable)*/)
        {
          scTramp.AplicarEfectosTrampa(scUnidad);
        }
      }


    }
    else if (obj.GetComponent<Obstaculo>() != null)
    {

      Presente = obj;
      //------------



    }

    string sortingLayerCanvas = obj.GetComponent<Unidad>() != null ? "UI3D" : null;
    RenderOrderHelper.AplicarOrdenPorY(obj, posY, sortingLayerCanvas);


  }

  public GameObject resaltadorBordeActivo;
  public void ResaltarCasillaActiva(bool res)
  {
    resaltadorBordeActivo.SetActive(res);
  }

  public GameObject Borde;
  public GameObject Sombra;
  public GameObject Actual;
  public GameObject Mover;
  public GameObject MoverCostoso;
  public GameObject Desplazable;
  public GameObject MarcaMelee;
  void Update()
  {
    if (Borde != null)
    {
      if (Presente != null)
      {
        if (Presente.GetComponent<Obstaculo>() != null)
        {
          Borde.SetActive(false);
          Actual.SetActive(false);
          Sombra.SetActive(false);
          Desplazable.SetActive(false);

        }
        else if (Presente.GetComponent<Unidad>() != null && esMovible() >= 10)
        {
          Mover.SetActive(false);
          MoverCostoso.SetActive(false);
          Borde.SetActive(false);

          Desplazable.SetActive(true);
        }
        else
        {
          Borde.SetActive(false);
          if (Sombra != null)
          {
            Sombra.SetActive(true);
            Desplazable.SetActive(false);

            if (BattleManager.Instance.unidadActiva == Presente.GetComponent<Unidad>())
            {
              Actual.SetActive(true);
            }
            else { Actual.SetActive(false); }

          }
        }
      }
      else
      {

        Actual.SetActive(false);
        if (Sombra != null)
        {
          Sombra.SetActive(false);
        }
        Mover.SetActive(false);
        MoverCostoso.SetActive(false);
        Borde.SetActive(false);
        Desplazable.SetActive(false);




        if (esMovible() == 1)
        {
          Mover.SetActive(true);
        }
        else if (esMovible() > 1 && esMovible() < 10)
        {
          MoverCostoso.SetActive(true);
        }
        else if (esMovible() >= 10)
        {

          Desplazable.SetActive(true);
        }
        else
        {
          if (gameObject.GetComponent<Trampa>() == null)
          { Borde.SetActive(true); Desplazable.SetActive(false); }
        }
      }
    }
  }

  int esMovible()
  {
    int res = 0;
    if (lado == 1) { return 0; } //Solo para aliados
    if (BattleManager.Instance == null || BattleManager.Instance.bOcupado) { return 0; }

    //Unidad seleccionada - Movimiento
    Unidad unidad = BattleManager.Instance.unidadActiva;

    if (unidad == null)
    {
      return 0;
    }
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente == null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      if (unidad.ObtenerAPActual() < costoMovimientoTotal)
      {
        res = 0;
      }
      else
      {
        res = costoMovimientoTotal;

      }
    }
    else if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente != null && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      if (Presente.GetComponent<Unidad>() != null)
      {
        if (!Presente.GetComponent<Unidad>().TieneBuffNombre("Desplazado"))
        {

          res = 10; //Desplazable
        }
        else
        {
          res = 0;
        }


      }

    }
    else
    {
      res = 0;
    }


    return res;
  }


  public void activarCapaMelee(bool activar)
  {

    MarcaMelee.SetActive(activar);

  }

}








