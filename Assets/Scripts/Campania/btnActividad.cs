using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class btnActividad : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
   public Image actImage;
   public Actividad actividadRepresentada;
   public Actividades scActividades;
   public Personaje personajeSeleccionado;
   public GameObject Recuadro;
   [SerializeField] private bool usarComoIndicadorRetrato;

   void Awake()
   {
      if (scActividades == null)
      {
        scActividades = GetComponentInParent<Actividades>();
      }
   }


   public void OnHover(int n)
   {
      if (n == 1)
      {
        MostrarDescripcionHover();
      }
      else
      {
        RestaurarDescripcionSeleccionada();
      }
   }

   public void OnPointerEnter(PointerEventData eventData)
   {
      MostrarDescripcionHover();
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      RestaurarDescripcionSeleccionada();
   }

   public void OnClick()
   {
      if (usarComoIndicadorRetrato)
      {
        btnPersonaje btnRetrato = GetComponentInParent<btnPersonaje>();
        if (btnRetrato != null)
        {
          btnRetrato.SeleccionarPJ();
        }
        return;
      }

      CambiarActividadPersonaje(personajeSeleccionado);

      scActividades.ActualizarRecuadros();
      if (scActividades.scMenuPersonajes != null)
      {
        scActividades.scMenuPersonajes.RefrescarListaVisual();
      }
      RuntimeAnalytics.TrackDesign(
        "campaign",
        "activity_selected",
        RuntimeAnalytics.ActivityToken(actividadRepresentada) + "_" + RuntimeAnalytics.ClassToken(personajeSeleccionado));

      
   }

   public void OnPointerClick(PointerEventData eventData)
   {
      if (usarComoIndicadorRetrato)
      {
        if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
        {
          ToggleActividadFijadaDesdeRetrato();
        }
        return;
      }

      if (eventData == null || eventData.button != PointerEventData.InputButton.Right)
      {
        return;
      }

      OnClickCambiarATodos();
   }

   public void OnClickCambiarATodos()
   {
      if (usarComoIndicadorRetrato)
      {
        return;
      }

      if (!EsActividadBaseCompartida())
      {
        return;
      }

      if (scActividades == null || scActividades.scMenuPersonajes == null || scActividades.scMenuPersonajes.listaPersonajes == null)
      {
        return;
      }

      foreach (Personaje personaje in scActividades.scMenuPersonajes.listaPersonajes)
      {
        if (personaje == null || personaje.Camp_Muerto || !personaje.PuedeRealizarActividades() || personaje.ActividadFijada)
        {
          continue;
        }

        CambiarActividadPersonaje(personaje);
      }

      scActividades.ActualizarRecuadros();
      if (scActividades.scMenuPersonajes != null)
      {
        scActividades.scMenuPersonajes.RefrescarListaVisual();
      }

      if (CampaignManager.Instance != null)
      {
        CampaignManager.Instance.EscribirLog(TRADU.i != null
          ? TRADU.i.Traducir("-Se ha cambiado la actividad de todos los personajes.")
          : "-Se ha cambiado la actividad de todos los personajes.");
      }
   }

   private void CambiarActividadPersonaje(Personaje personaje)
   {
      if (personaje == null)
      {
        return;
      }

      int actividadAnterior = personaje.ActividadSeleccionada;
      personaje.ActividadSeleccionada = actividadRepresentada.IDActividad;

      if (actividadAnterior != actividadRepresentada.IDActividad
        && personaje.TieneRasgo(PersonajeTraitCatalog.TraitDesganado))
      {
        personaje.Camp_Moral = Mathf.Min(personaje.Camp_Moral, -2);
      }
   }

   private bool EsActividadBaseCompartida()
   {
      return actividadRepresentada != null
        && (actividadRepresentada.IDActividad == 1
        || actividadRepresentada.IDActividad == 2
        || actividadRepresentada.IDActividad == 3);
   }

   private void MostrarDescripcionHover()
   {
      if (actividadRepresentada == null)
      {
        return;
      }

      if (usarComoIndicadorRetrato)
      {
        if (TooltipStats.Instance != null)
        {
          TooltipStats.Instance.ShowTooltipRaw(actividadRepresentada.desc, Input.mousePosition);
        }
        else if (TooltipItems.Instance != null)
        {
          TooltipItems.Instance.ShowTooltip(actividadRepresentada.desc, Input.mousePosition);
        }
      }

      if (scActividades == null || scActividades.textdesc == null)
      {
        return;
      }

      scActividades.textdesc.text = scActividades.FormatearDescripcionPanel(actividadRepresentada.desc);
   }

   private void RestaurarDescripcionSeleccionada()
   {
      if (usarComoIndicadorRetrato)
      {
        TooltipStats.Instance?.HideTooltip();
        TooltipItems.Instance?.HideTooltip();
      }

      if (scActividades == null)
      {
        return;
      }

      scActividades.ActualizarRecuadros();
   }

   public void ConfigurarIndicadorRetrato(Actividad actividad, Actividades actividades, Personaje personaje)
   {
      actividadRepresentada = actividad;
      scActividades = actividades;
      personajeSeleccionado = personaje;
      usarComoIndicadorRetrato = true;
   }

   private void ToggleActividadFijadaDesdeRetrato()
   {
      if (!usarComoIndicadorRetrato || personajeSeleccionado == null)
      {
        return;
      }

      personajeSeleccionado.ActividadFijada = !personajeSeleccionado.ActividadFijada;

      btnPersonaje btnRetrato = GetComponentInParent<btnPersonaje>();
      if (btnRetrato != null)
      {
        btnRetrato.RepresentarTodo();
      }

      if (personajeSeleccionado.ActividadFijada && CampaignManager.Instance != null)
      {
        CampaignManager.Instance.EscribirLog(TRADU.i != null
          ? TRADU.i.Traducir("-Actividad fijada.")
          : "-Actividad fijada.", true);
      }
   }



}
