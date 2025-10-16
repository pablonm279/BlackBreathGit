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
void Start()
{
  BattleManager.Instance.OnRondaNueva  += BattleManager_OnRondaNueva;
}


private void BattleManager_OnRondaNueva(object sender, EventArgs empty)
{
 
 //---

}


public async void OnMouseDown() 
{   
  
  
  
    //Unidad seleccionada - Movimiento
    Unidad unidad = BattleManager.Instance.unidadActiva;
    
    //!!!
    unidad.CasillaPosicion.CalcularDistanciaACasilla(this, out int x, out int y, out bool lado);
    //!!!
    if(BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente == null && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil <1)
    {
       int costoMovimientoTotal = costoMovimiento;

       int diferenciaX = Mathf.Abs(unidad.CasillaPosicion.posX - posX);
       int diferenciaY = Mathf.Abs(unidad.CasillaPosicion.posY - posY);

       // Comprueba si las casillas están en posiciones diagonales
       if (diferenciaX == 1 && diferenciaY == 1)
       {
          costoMovimientoTotal++;
       }

       if(unidad.ObtenerAPActual() >= costoMovimientoTotal)
       { 
         unidad.CambiarAPActual(-costoMovimientoTotal); 
         BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
         unidad.CasillaDeseadaMov = this;
       }
    }
    // Intercambio con aliado: mover a casilla ocupada por aliado y que el aliado vaya a la casilla original
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente != null && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      Unidad aliado = Presente != null ? Presente.GetComponent<Unidad>() : null;
      if (aliado != null)
      {
        if (aliado.CasillaPosicion.lado != unidad.CasillaPosicion.lado)
        {
          BattleManager.Instance.EscribirLog("No puedes intercambiar con enemigos.");
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
   
    //Para habilidades en Área
    if(BattleManager.Instance.HabilidadActiva != null && !BattleManager.Instance.bOcupado)
    {
      
      if(BattleManager.Instance.HabilidadActiva.enArea > 0 || BattleManager.Instance.HabilidadActiva.targetEspecial > 0  && BattleManager.Instance.SeleccionandoObjetivo)
      {
        print(00);
      if(!BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this)){ print(11);  return;} //Si no está en el área, no hace nada


       List<Unidad> lUnidadesEnArea = new List<Unidad>();
       List<Obstaculo> lObstaculosEnArea = new List<Obstaculo>();

       foreach(Unidad enAzul in unidadesEnCasAzul)
       { 
        if(lUnidadesEnArea.Contains(enAzul) == false) //Que no se duplique
        {lUnidadesEnArea.Add(enAzul);}
       }
       foreach(Obstaculo enAzul in obstaculosEnCasAzul)
       { 
         if(lObstaculosEnArea.Contains(enAzul) == false) //Que no se duplique
         {lObstaculosEnArea.Add(enAzul);}
       }

       foreach(Casilla u in BattleManager.Instance.HabilidadActiva.lCasillasafectadas)
       {
          if(u.Presente != null)
          {
            if (u.Presente.GetComponent<Unidad>() != null)
            {
              if(unidadesEnCasAzul.Contains(u.Presente.GetComponent<Unidad>()) && lUnidadesEnArea.Contains(u.Presente.GetComponent<Unidad>()) == false)
              {
               lUnidadesEnArea.Add(u.Presente.GetComponent<Unidad>());  
              }
           
            }
            if (u.Presente.GetComponent<Obstaculo>() != null)
            {
               if(obstaculosEnCasAzul.Contains(u.Presente.GetComponent<Obstaculo>())&& lObstaculosEnArea.Contains(u.Presente.GetComponent<Obstaculo>()) == false)
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
      else if(BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.poneTrampas&& BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
      { 
            BattleManager.Instance.casillaClickHabilidad = this;
            await BattleManager.Instance.HabilidadActiva.Resolver(null, this); 
            BattleManager.Instance.casillaClickHabilidad = null;
      }
      else if(BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.esZonal&& BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
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
      print("Casilla Ocupada, no se puede colocar objeto");
      return false;
    }

    GO.transform.position = transform.position;
    NuevoObjetoPresenteEnCasilla(GO);
 

    if(GO.GetComponent<Unidad>() != null)
    {
      GO.GetComponent<Unidad>().CasillaPosicion = this;
    }
    if(GO.GetComponent<Obstaculo>() != null)
    {
      GO.GetComponent<Obstaculo>().CasillaPosicion = this;
    }

     //Esto pone en la capa del canvas del objeto/unidad segun Y de esta casilla
     Canvas canvasObjeto = GO.GetComponentInChildren<Canvas>();
     if (canvasObjeto != null)
     {
      canvasObjeto.overrideSorting = true;
      canvasObjeto.sortingOrder = 60 - posY * 10;
     }
            //---

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

      // Asegurarse de que el objeto esté exactamente en la posición final
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

      // Asegurarse de que el objeto esté exactamente en la posición final
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
                else if (x > 2 && ((Mathf.Abs(i) == 2 && Mathf.Abs(j) != 2 || Mathf.Abs(j) == 2 && Mathf.Abs(i) != 2)||(Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)))
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
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if(cas.posY == posY && cas.lado == lado)
      {
           lCasillas.Add(cas);
      }

    }
  
  return lCasillas;
}

public Casilla ObtenerCasillasMasAtrasEnFila()
{
    Casilla lCas = this;
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if(cas.posY == posY && cas.lado == lado)
      {
           if(cas.posX < posX)
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
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if(cas.lado == lado)
      {
           lCasillas.Add(cas);
      }

    }
  
  return lCasillas;
}

public List<Casilla> ObtenerCasillasLadoOpuesto()
{
     List<Casilla> lCasillas = new List<Casilla>();
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
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
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if(cas.posX == posX && cas.lado == lado)
      {
           lCasillas.Add(cas);
      }

    }
  
  return lCasillas;
}
public List<Casilla> ObtenerCasillasAdyacentesEnColumna()
{
   List<Casilla> lCasillas = new List<Casilla>();
    foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if(cas.posX == posX && cas.lado == lado)
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

    int RangoEnOtroLado = alcance - (3-posX);
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
  if(Presente == null)
  {
       return false;
  }

  if(Presente.GetComponent<Unidad>() != null)
  {
       return true;
  }
  else{ return false;}

}

public bool bTieneObstaculo()
{
  if(Presente == null)
  {
       return false;
  }

  if(Presente.GetComponent<Obstaculo>() != null)
  {
       return true;
  }
  else{ return false;}

}

public bool bTieneUnidadoObstaculo()
{
  if(Presente == null)
  {
       return false;
  }

  if(Presente.GetComponent<Unidad>() != null)
  {
    if(Presente.GetComponent<Unidad>().ObtenerEstaEscondido() == 0)
    {return true;} //Si la unidad no está escondida, la toma como que hay algo
    else
    {return false;}//Si la unidad está escondida, la toma como que no hay nada
  }
  else if(Presente.GetComponent<Obstaculo>() != null)
  {
       return true;
  }
  else{ return false;}

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
   GetComponent<MeshRenderer>().enabled = true;
  //Agregar mas
}

 List<Casilla> casAlre = new List<Casilla>();

[SerializeField]public List<Unidad> unidadesEnCasAzul =  new List<Unidad>();
[SerializeField]public List<Obstaculo> obstaculosEnCasAzul =  new List<Obstaculo>();
public void OnMouseEnter() 
{   
    unidadesEnCasAzul.Clear();
    obstaculosEnCasAzul.Clear();
    casAlre.Clear();
  //Controlar se esta haciendo hablidad en Area, marca las casillas en la zona de alcance y en el area
  if(BattleManager.Instance.HabilidadActiva != null)
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
        print(11);
        foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
        { 
          if ((cas.posY == posY && cas.posX == posX - 1) && (cas.lado == lado))
          {
            casAlre.Add(cas);print(01);
          }
          if ((cas.posY == posY && cas.posX == posX - 2) && (cas.lado == lado))
          {
            casAlre.Add(cas);print(02);
          }
          if ((cas.posY == posY + 1 && cas.posX == posX - 2) && (cas.lado == lado))
          {
            casAlre.Add(cas);print(03);
          }
          if ((cas.posY == posY - 1 && cas.posX == posX - 2) && (cas.lado == lado))
          {
            casAlre.Add(cas);print(04);
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

        // 5 casillas en la última columna (posX + 2)
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
  if(BattleManager.Instance.HabilidadActiva != null)
  {
    if(BattleManager.Instance.HabilidadActiva.enArea > 0 && BattleManager.Instance.SeleccionandoObjetivo)
     { DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul();}
      else if(BattleManager.Instance.HabilidadActiva.targetEspecial > 0)  
     {
        DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul();
     }
    
  }
}

private void MarcarCasillasAzul( List<Casilla> casillasZonahab )
{
    if(casillasZonahab.Contains(this)) //Marca casilla actual si esta en la zona de la habilidad
    {
        ActivarCapaColorAzul();
    }


    foreach(Casilla cas in casAlre) //Marca casillas alrededor si la central está en la zona de la habilidad
    {   if(casillasZonahab.Contains(this) )
       {
        cas.ActivarCapaColorAzul();
       }
    }
}

private void DesmarcarCasillasAlreAzul()
{
    foreach(Casilla cas in casAlre)
    {
        cas.DesactivarCapaColorAzul();
    }
}

public void CalcularDistanciaACasilla(Casilla casObjetivo, out int yVert, out int xHor, out bool mismoLado)
{
 
  
  if(lado == casObjetivo.lado) //Casillas del mismo lado
  {
    mismoLado = true;
    xHor =  posX - casObjetivo.posX; //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia afuera, negativo, hacia centro.
  }
  else
  { 
    mismoLado = false;
    xHor =  7 - posX - casObjetivo.posX; //La diferencia siempre va a dar positiva al estar del otro lado
    
  }

  
  yVert = (posY - casObjetivo.posY); //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia arriba, negativo, hacia abajo.

 if(Presente != null){
 
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
      catch {}

      //------------- TRIGGER DE TRAMPAS
      if (GetComponent<Trampa>() != null)
      {
        Trampa scTramp = gameObject.GetComponent<Trampa>();
        Unidad scUnidad = obj.GetComponent<Unidad>();

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
  
      // Si el objeto tiene Canvas, permite override y ajusta el sort order
      Canvas canvas = obj.GetComponentInChildren<Canvas>();
      if (canvas != null)
      {
        canvas.overrideSorting = true;
        canvas.sortingOrder = 60 - 10 * posY;
      }


}




}
