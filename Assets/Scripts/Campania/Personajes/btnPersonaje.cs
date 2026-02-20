using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class btnPersonaje : MonoBehaviour
{
  public Personaje personajeRepresentado;

  public TextMeshProUGUI txtPersonajeRepresentado;


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
        CampaignManager.Instance.goSequitos.transform.GetChild(2).GetChild(1).gameObject.GetComponent<SequitoCuranderos>().TratarHerida(personajeRepresentado);
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

      float vidaMaxEscalada = personajeRepresentado.ObtenerVidaMaximaConFuerza();
      float vidaActualEscalada = personajeRepresentado.ObtenerVidaActualConFuerza();
      float valor = 1 - (vidaActualEscalada / vidaMaxEscalada);

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
  public void representarinfo()
  {
    if (CampaignManager.Instance.goMenuBatallas.activeInHierarchy)
    {
      if (personajeRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        if (TRADU.i.nIdioma == 1) //Español
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
        }
        if (TRADU.i.nIdioma == 2) //Inglés
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>Lv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
        }



      }
      else
      {
        txtPersonajeRepresentado.gameObject.SetActive(false);
      }
    }
    else
      {
        txtPersonajeRepresentado.gameObject.SetActive(false);
      }

  }
  private void OnEnable()
  {

    RepresentarTodo();


  }
  public void RepresentarTodo()
  {
    representarinfo();
    representarVida();
    RepresentarIconos();
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
