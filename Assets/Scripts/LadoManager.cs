using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;


public class LadoManager : MonoBehaviour
{
   
  public int esLado; //1 izquierda 2 derecha
  public Casilla c1x1;
  public Casilla c1x2;
  public Casilla c1x3;
  public Casilla c1x4;
  public Casilla c1x5;

  public Casilla c2x1;
  public Casilla c2x2;
  public Casilla c2x3;
  public Casilla c2x4;
  public Casilla c2x5;

  public Casilla c3x1;
  public Casilla c3x2;
  public Casilla c3x3;
  public Casilla c3x4;
  public Casilla c3x5;


  public List<Unidad> unidadesLado = new List<Unidad>();
  public List<Casilla> casillasLado = new List<Casilla>();

  

  void Start()
  {
     BattleManager.Instance.OnRondaNueva  += BattleManager_OnRondaNueva;

     ActualizarListaDeUnidadesEnLado();
    

  }

   public void ActualizarListaDeUnidadesEnLado()
   {
        unidadesLado.Clear();
        foreach  (Transform hijos in transform)
        {
            Casilla scCasilla = hijos.GetComponent<Casilla>();
            

               if (scCasilla == null)
              {
                continue;
              }
             
               if (scCasilla.Presente == null)
              {
                continue;
              }

               if (scCasilla.Presente.GetComponent<Unidad>() != null)
              {
                unidadesLado.Add(scCasilla.Presente.GetComponent<Unidad>());
              
                
              }

            


        }

    
   }

    public void ActualizarListaDeCasillasEnLado()
   {
        casillasLado.Clear();
       foreach (Transform hijo in transform)
    {
        Casilla casilla = hijo.GetComponent<Casilla>();
        if (casilla != null)
        {
            casillasLado.Add(casilla);
        }
    }
   }



private void BattleManager_OnRondaNueva(object sender, EventArgs empty)
{
 
 //---

}


public bool ColocarEnCasilla(GameObject GO, int x, int y)
{
  Casilla cas = ObtenerCasillaPorIndex(x, y);
  if (cas == null)
  {
    print("Casilla no encontrada");
    return false;
  }
  if (cas.Presente != null)
  {
    print("Casilla Ocupada, no se puede colocar objeto");
    return false;
  }

  GO.transform.position = cas.transform.position;
  cas.NuevoObjetoPresenteEnCasilla(GO);

  // Si el objeto tiene Canvas, permite override y ajusta el sort order
  Canvas canvas = GO.GetComponentInChildren<Canvas>();
  if (canvas != null)
  {
    canvas.overrideSorting = true;
    canvas.sortingOrder = 60 - 10 * cas.posY;
  }

  if (GO.GetComponent<Unidad>() != null)
  {
    GO.GetComponent<Unidad>().CasillaPosicion = cas;
  }
  if (GO.GetComponent<Obstaculo>() != null)
  {
    GO.GetComponent<Obstaculo>().CasillaPosicion = cas;
  }

  return true;
}


public Casilla ObtenerCasillaPorIndex(int posX, int posY)
  {
    
    foreach  (Transform hijos in transform)
    {
      
        Casilla scCasilla = hijos.GetComponent<Casilla>();

        if (scCasilla != null)
        {
            // Comparamos las coordenadas
            if (scCasilla.posX == posX && scCasilla.posY == posY)
            {
                // Devolvemos la casilla encontrada
                return scCasilla;
            }
        }

    }

    return null;
  }


public List<Casilla> filaCasillasSegunRango(int fila, int rango, int ancho)  //Se llama desde ObtenerCasillasRango en Casilla
{
    List<Casilla> lCasillas = new List<Casilla>();

     foreach  (Transform hijos in transform)
    {
      
        Casilla scCasilla = hijos.GetComponent<Casilla>();

       

        if (scCasilla != null)
        {
            int difAncho = Math.Abs(scCasilla.posY - fila);


            if ((scCasilla.posX + rango) > 3 && difAncho <= ancho)
            {
               lCasillas.Add(scCasilla);
            }
        }

    }

    return lCasillas;
} 


public Casilla ObtenerCasillaAleatoria(bool debeEstarVacia)
{
   int intentos = 0; // Para limitar los intentos de colocar la unidad
bool correcto = false;
Casilla casilla = null;

while (!correcto && intentos < 100) // Limitar los intentos para evitar bucles infinitos
{
    intentos++; // Se incrementa el contador en cada iteración

    int rX = UnityEngine.Random.Range(1, 4);  // Rango de x
    int rY = UnityEngine.Random.Range(1, 6);  // Rango de y

    Casilla casillaRevisar = ObtenerCasillaPorIndex(rX, rY);
    
    if (debeEstarVacia)
    {
        if (casillaRevisar.GetComponent<Unidad>() != null ||
            casillaRevisar.GetComponent<Obstaculo>() != null)
        {
            continue; // Si está ocupada, pasa a la siguiente iteración
        }
    }
    
    correcto = true;
    casilla = casillaRevisar;
}
   
 return casilla;
}





}




