using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIContadorAP : MonoBehaviour
{

    public GameObject circuloAPprefab;
    public GameObject esfuerzoAPprefab;
    private Sprite spriteAPUsado;
    private Sprite spriteEsforzar;
 
   
   
    private void Start()
    {
       spriteAPUsado = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/ap_usado");
       spriteEsforzar = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/esforzar");
       ActualizarAPCirculos();

    }

 
    public void ActualizarAPCirculos()
    {
      if (!isActiveAndEnabled || !gameObject.scene.isLoaded)
      {
        return;
      }

      BattleManager battleManager = BattleManager.Instance;
      if (battleManager == null || !battleManager.isActiveAndEnabled)
      {
        return;
      }

        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
            Destroy(buttonTransform.gameObject);
        }
    
      
      if(battleManager.unidadActiva != null)
      {
        Unidad unidadSeleccionada = battleManager.unidadActiva.gameObject.GetComponent<Unidad>();

      for (int i = 0; i < unidadSeleccionada.ObtenerAPActual(); i++)
      {
        GameObject circuloAP = Instantiate(circuloAPprefab, transform);
          //circuloAP.GetComponent<Image>().color = new Color(0.7f, 0.0f, 0.0f); // Rojo oscuro
        
       }
      }

    // Luego de actualizar la UI, revisar si debe indicarse pasar turno
     battleManager.RevisarAPUnidadActiva();
     battleManager.ActualizarCasillasMelee();
    }

  public void MarcarCirculos(int n)
  {
    if (!isActiveAndEnabled || !gameObject.scene.isLoaded || BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
    {
      return;
    }

    // Obtén la cantidad total de elementos en el GridLayoutGroup
    int totalCirculos = transform.childCount;

    Unidad unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject.GetComponent<Unidad>();
    int apSinEsfuerzo = (int)unidadSeleccionada.ObtenerAPActual();

    // Asegúrate de que n está dentro de los límites y no sea mayor que apSinEsfuerzo
    n = Mathf.Clamp(n, 0, Mathf.Min(totalCirculos, apSinEsfuerzo));

    // Itera sobre los últimos N elementos y cambia su color a azul
    for (int i = totalCirculos - n; i < totalCirculos; i++)
    {
      Transform circuloTransform = transform.GetChild(i);
      Image circuloImage = circuloTransform.GetComponent<Image>();

      if (circuloImage == null)
      {
        continue;
      }

      circuloImage.color = Color.white;
      if (spriteAPUsado != null)
      {
        circuloImage.sprite = spriteAPUsado;
      }
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
        if (circuloImage == null)
        {
          continue;
        }

        circuloImage.color = Color.white;
    }
  }

public void SeEsforzaria(int n)
{
    if(n > 0)
    {
      for (int i = 0; i < n; i++)
      {
        GameObject nuevoCirculo = Instantiate(esfuerzoAPprefab, transform);
        if (spriteEsforzar != null)
        {
          Image img = nuevoCirculo.GetComponent<Image>();
          if (img != null)
          {
            img.sprite = spriteEsforzar;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
          }
        }
      }
    }
}


}

