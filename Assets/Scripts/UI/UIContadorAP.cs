using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIContadorAP : MonoBehaviour
{

    public GameObject circuloAPprefab;
    public GameObject esfuerzoAPprefab;

 
   
    private void Start()
    {
       ActualizarAPCirculos();

    }

 
    public void ActualizarAPCirculos()
    {     
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
            Destroy(buttonTransform.gameObject);
        }

      
      if(BattleManager.Instance.unidadActiva != null)
      {
        Unidad unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject.GetComponent<Unidad>();
      
        for (int i = 0; i < unidadSeleccionada.ObtenerAPActual(); i++)
        {
           GameObject circuloAP =  Instantiate(circuloAPprefab, transform);
        
        }
      }
      
      // Luego de actualizar la UI, revisar si debe indicarse pasar turno
      if (BattleManager.Instance != null)
      {
        BattleManager.Instance.RevisarAPUnidadActiva();
      }
    }

   public void MarcarCirculos(int n)
  {
    // Obtén la cantidad total de elementos en el GridLayoutGroup
    int totalCirculos = transform.childCount;

    Unidad unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject.GetComponent<Unidad>();
    int apSinEsfuerzo = (int)unidadSeleccionada.ObtenerAPActual();

    // Asegúrate de que n esté dentro de los límites y no sea mayor que apSinEsfuerzo
    n = Mathf.Clamp(n, 0, Mathf.Min(totalCirculos, apSinEsfuerzo));

    // Itera sobre los últimos N elementos y cambia su color a azul
    for (int i = totalCirculos - n; i < totalCirculos; i++)
    {
        Transform circuloTransform = transform.GetChild(i);
        Image circuloImage = circuloTransform.GetComponent<Image>();

        // Cambia el color a azul (puedes ajustar esto según tus necesidades)
        circuloImage.color = Color.blue;
    }
  }


  public void ResetearCirculos()
  {
    ActualizarAPCirculos();
    int totalCirculos = transform.childCount;

    for (int i = 0; i < totalCirculos; i++)
    {
        Transform circuloTransform = transform.GetChild(i);
        Image circuloImage = circuloTransform.GetComponent<Image>();

        // Restablece el color a blanco
        circuloImage.color = Color.white;
    }
  }

public void SeEsforzaría(int n)
{
    if(n > 0)
    {
      for (int i = 0; i < n; i++)
      {
        GameObject nuevoCirculo = Instantiate(esfuerzoAPprefab, transform);
      }
    }
}


}
