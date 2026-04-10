using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class btnActividad : MonoBehaviour, IPointerClickHandler
{
   public Image actImage;
   public Actividad actividadRepresentada;
   public Actividades scActividades;
   public Personaje personajeSeleccionado;
   public GameObject Recuadro;

   void Awake()
   {
      scActividades = transform.parent.parent.GetComponent<Actividades>();
      

   }


   public void OnHover(int n)
   {
      if(n ==1)
      {
        scActividades.textdesc.text = actividadRepresentada.desc;
      }
      else{  scActividades.ActualizarRecuadros();}
     
     
   }

   public void OnClick()
   {
      CambiarActividadPersonaje(personajeSeleccionado);

      scActividades.ActualizarRecuadros();
      RuntimeAnalytics.TrackDesign(
        "campaign",
        "activity_selected",
        RuntimeAnalytics.ActivityToken(actividadRepresentada) + "_" + RuntimeAnalytics.ClassToken(personajeSeleccionado));

      
   }

   public void OnPointerClick(PointerEventData eventData)
   {
      if (eventData == null || eventData.button != PointerEventData.InputButton.Right)
      {
        return;
      }

      OnClickCambiarATodos();
   }

   public void OnClickCambiarATodos()
   {
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
        if (personaje == null || personaje.Camp_Muerto || !personaje.PuedeRealizarActividades())
        {
          continue;
        }

        CambiarActividadPersonaje(personaje);
      }

      scActividades.ActualizarRecuadros();

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



}
