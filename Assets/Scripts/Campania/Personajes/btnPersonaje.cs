using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class btnPersonaje : MonoBehaviour
{
  private static readonly string[] IconosLegacyCampania =
  {
    "BajaMoral",
    "Herido",
    "Corrupto",
    "Fatigado",
    "Enfermo",
    "AltaMoral",
    "Bendecido",
    "Avergonzado",
    "Vacio-dejar"
  };

  public Personaje personajeRepresentado;

  public TextMeshProUGUI txtPersonajeRepresentado;
  [SerializeField] private Transform contenedorEstadosCampania;
  [SerializeField] private GameObject prefabEstadoCampania;
  [SerializeField] private Image iconoMuerto;

  private void Awake()
  {
    AsegurarReferenciasEstadosCampania();
  }


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
         if (TRADU.i.nIdioma == 3) //Poertu
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
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
    AsegurarReferenciasEstadosCampania();
    OcultarIconosLegacyCampania();

    if (iconoMuerto != null)
    {
      iconoMuerto.gameObject.SetActive(personajeRepresentado != null && personajeRepresentado.Camp_Muerto);
    }

    LimpiarEstadosCampania();

    if (personajeRepresentado != null)
    {
      if (personajeRepresentado.Camp_Muerto || contenedorEstadosCampania == null)
      {
        return;
      }

      if (personajeRepresentado.Camp_Herido)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Herido);
      }

      if (personajeRepresentado.Camp_Corrupto)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Corrupto);
      }

      if (personajeRepresentado.Camp_Enfermo > 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Enfermo);
      }

      if (personajeRepresentado.Camp_Moral < 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.BajaMoral);
      }

      if (personajeRepresentado.Camp_Moral > 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.AltaMoral);
      }

      if (personajeRepresentado.Camp_Fatigado)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Fatigado);
      }

      if (personajeRepresentado.TieneCampBendecido())
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Bendecido);
      }

      if (personajeRepresentado.Camp_Avergonzado)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Avergonzado);
      }
    }
  }

  private void AsegurarReferenciasEstadosCampania()
  {
    if (contenedorEstadosCampania == null)
    {
      Transform estados = transform.Find("Estados");
      if (estados != null)
      {
        contenedorEstadosCampania = estados;
      }
    }

    if (iconoMuerto == null)
    {
      Transform muerto = transform.Find("Muerto");
      if (muerto != null)
      {
        iconoMuerto = muerto.GetComponent<Image>();
      }
    }
  }

  private void OcultarIconosLegacyCampania()
  {
    for (int i = 0; i < IconosLegacyCampania.Length; i++)
    {
      Transform icono = transform.Find(IconosLegacyCampania[i]);
      if (icono != null)
      {
        icono.gameObject.SetActive(false);
      }
    }
  }

  private void LimpiarEstadosCampania()
  {
    if (contenedorEstadosCampania == null)
    {
      return;
    }

    for (int i = contenedorEstadosCampania.childCount - 1; i >= 0; i--)
    {
      Destroy(contenedorEstadosCampania.GetChild(i).gameObject);
    }
  }

  private void CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania tipoEstado)
  {
    if (contenedorEstadosCampania == null)
    {
      return;
    }

    GameObject goEstado = prefabEstadoCampania != null
      ? Instantiate(prefabEstadoCampania, contenedorEstadosCampania, false)
      : CrearEstadoCampaniaFallback();

    if (goEstado == null)
    {
      return;
    }

    goEstado.transform.localScale = Vector3.one;

    UIEstadoPersonajeCamp estadoUI = goEstado.GetComponent<UIEstadoPersonajeCamp>();
    if (estadoUI == null)
    {
      estadoUI = goEstado.AddComponent<UIEstadoPersonajeCamp>();
    }

    estadoUI.Representar(tipoEstado, personajeRepresentado);
  }

  private GameObject CrearEstadoCampaniaFallback()
  {
    GameObject goEstado = new GameObject(
      "camp_estado",
      typeof(RectTransform),
      typeof(CanvasRenderer),
      typeof(Image),
      typeof(UIEstadoPersonajeCamp));

    goEstado.transform.SetParent(contenedorEstadosCampania, false);

    RectTransform rectTransform = goEstado.GetComponent<RectTransform>();
    if (rectTransform != null)
    {
      rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
      rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
      rectTransform.pivot = new Vector2(0.5f, 0.5f);
      rectTransform.sizeDelta = new Vector2(25f, 25f);
    }

    Image image = goEstado.GetComponent<Image>();
    if (image != null)
    {
      image.preserveAspect = true;
      image.raycastTarget = true;
    }

    return goEstado;
  }
}


