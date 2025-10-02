using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btnPersonaje : MonoBehaviour
{
  public Personaje personajeRepresentado;


  public void SeleccionarPJ()
  {

    if (!CampaignManager.Instance.goMenuBatallas.activeInHierarchy)
    {
      if (!CampaignManager.Instance.goSequitos.activeInHierarchy)
      {
        transform.parent.parent.GetComponent<MenuPersonajes>().SeleccionarPersonaje(personajeRepresentado, gameObject);
      }
      else //Si estan los sequitos activos, por lo tanto se asume que es el de curanderos curando.
      {
        CampaignManager.Instance.goSequitos.transform.GetChild(1).GetChild(1).gameObject.GetComponent<SequitoCuranderos>().TratarHerida(personajeRepresentado);
      }
    }
    else if (CampaignManager.Instance.scMenuBatallas.UIEmpezarBatalla.activeInHierarchy || CampaignManager.Instance.scMenuBatallas.UIEmpezarBatallaACaravana.activeInHierarchy) //Si esta en la pantalla de batalla, se selecciona el personaje para la batalla
    {
      CampaignManager.Instance.scMenuBatallas.Seleccionar(personajeRepresentado);
    }

    representarVida();

  }

  public Image vidaRepresenta;
  public void representarVida()
  {
    if (personajeRepresentado != null)
    {

      float valor = 1 - (personajeRepresentado.fVidaActual / personajeRepresentado.fVidaMaxima);

      vidaRepresenta.fillAmount = valor;


      if (!CampaignManager.Instance.goMenuBatallas.activeInHierarchy) //Muestra efecto de subida pendiente
      {
        bool sube = false;
        if (personajeRepresentado != null)
        {
          if (personajeRepresentado.NivelPuntoAtributo > 0) { sube = true; }
          if (personajeRepresentado.NivelPuntoHabilidad > 0) { sube = true; }
          if (personajeRepresentado.NivelPuntoTS > 0) { sube = true; }

          if (sube)
          {
            transform.GetChild(2).gameObject.SetActive(true);
          }
          else { transform.GetChild(2).gameObject.SetActive(false); }
        }
      }
    }

  }

  private void OnEnable()
  {

    representarVida();


  }

  public void RepresentarIconos()
  {
    if (personajeRepresentado != null)
    {
      //pone los graficos de muerte (?) y heridas
      if (personajeRepresentado.Camp_Herido)
      {
        transform.GetChild(3).gameObject.SetActive(true);
      }
      else { transform.GetChild(3).gameObject.SetActive(false); }

      if (personajeRepresentado.Camp_Corrupto)
      {
        transform.GetChild(5).gameObject.SetActive(true);
      }
      else { transform.GetChild(5).gameObject.SetActive(false); }

      if (personajeRepresentado.Camp_Enfermo > 0)
      {
        transform.GetChild(6).gameObject.SetActive(true);
      }
      else { transform.GetChild(6).gameObject.SetActive(false); }

      if (personajeRepresentado.Camp_Muerto)
      {
        transform.GetChild(3).gameObject.SetActive(false);
        transform.GetChild(5).gameObject.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(true);
      }
      else { transform.GetChild(4).gameObject.SetActive(false); }

      if (personajeRepresentado.Camp_Moral < 0)
      {
        transform.GetChild(7).gameObject.SetActive(true);
      }
      else { transform.GetChild(7).gameObject.SetActive(false); }

      if (personajeRepresentado.Camp_Moral > 0)
      {
        transform.GetChild(8).gameObject.SetActive(true);
      }
      else { transform.GetChild(8).gameObject.SetActive(false); }

    }
  }
}
