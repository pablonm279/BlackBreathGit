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

  public int hCooldownMax;
  public int hActualCooldown;

  public GameObject Usuario;
  public Unidad scEstaUnidad;

  public bool esHostil; //Si es para enemigos o aliados

  public int prioridad; //Mientras mas, mas chances que la elija

  public int costoAP;

  Casilla casillaOrigen;

  public List<object> objPosibles = new List<object>(); //Esta variable guarda los objetivos posibles de la habilidad

  public abstract object EstablecerObjetivoPrioritario(); //Dentro de la lista objPosibles, segun la habilidad, determinar cual es la mejor opcion 


  public abstract Task ActivarHabilidad();

  public abstract void AplicarEfectosHabilidad(object unidad);

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
      if (cas.bTieneUnidadoObstaculo()) //si alguna de las 3 tiene algo, no aumenta el rango melee
      {
        return 0;
      }
    }





    foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
    {
      if (cas.bTieneUnidadoObstaculo()) //y si alguna de las 3 tiene algo, aumenta solo en 1 
      {
        return 1;
      }
    }


    return 2; //si ninguna de las 2 columnas tiene algo, aumenta al maximo
  }

  //Ataque vs Defensa convencional
  public int TiradaAtaque(float defensaObjetivo, float atributoAtaca, float modificadorHabilidadaAtaque, float modificadorDadoCritico, Unidad unidadAtacada, float tiradaAtaque = -1)
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
    
    if (tiradaAtaque == -1)
    {
      iTiradaAtaque = UnityEngine.Random.Range(1, 21);
    }
    else { iTiradaAtaque = tiradaAtaque; }
    float iResultadoAtaque = iTiradaAtaque + atributoAtaca + modificadorHabilidadaAtaque;

    if (iTiradaAtaque == 1)//Pifia
    {
      scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir("Pifia"), Color.red);
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ") + iResultadoAtaque + TRADU.i.Traducir(". Resultado: Pifia."));

      return -1;
    }

    if (iTiradaAtaque >= 19 - modificadorDadoCritico) { return 3; } //Golpe crítico



    if (iResultadoAtaque == defensaObjetivo)
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ") + iResultadoAtaque + TRADU.i.Traducir(". Resultado: Roce."));
      return 1; //Roce
    }
    if (iResultadoAtaque > defensaObjetivo)
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ") + iResultadoAtaque + TRADU.i.Traducir(". Resultado: Golpe."));
      return 2; //Golpe
    }

    if (resultado == 0) //Fallo
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir("-Tirada de Ataque: 1d20 = ") + iResultadoAtaque + TRADU.i.Traducir(". Resultado: Fallo."));
      unidadAtacada.GenerarTextoFlotante(TRADU.i.Traducir("Fallo"), Color.grey);
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
    // Log de uso de habilidad de IA
    if (BattleManager.Instance != null && scEstaUnidad != null)
    {
      BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir(" usa ") + nombre + ".</color>");
    }
    unidadesNoParticipantes = new List<object>(BattleManager.Instance.lUnidadesTotal);
    unidadesNoParticipantes.Remove(scEstaUnidad);

    scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir(nombre), Color.magenta);

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

    BattleManager.Instance.SombrearANoParticipantesHabilidad(unidadesNoParticipantes);
    await Task.Delay(1100);
    BattleManager.Instance.DesombrearANoParticipantesHabilidad(unidadesNoParticipantes);
}




}
