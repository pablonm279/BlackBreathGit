using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class btnActividad : MonoBehaviour
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
      int actividadAnterior = personajeSeleccionado.ActividadSeleccionada;
      personajeSeleccionado.ActividadSeleccionada = actividadRepresentada.IDActividad;

      if (actividadAnterior != actividadRepresentada.IDActividad
        && personajeSeleccionado.TieneRasgo(PersonajeTraitCatalog.TraitDesganado))
      {
        personajeSeleccionado.Camp_Moral = Mathf.Min(personajeSeleccionado.Camp_Moral, -2);
      }

      scActividades.ActualizarRecuadros();
      RuntimeAnalytics.TrackDesign(
        "campaign",
        "activity_selected",
        RuntimeAnalytics.ActivityToken(actividadRepresentada) + "_" + RuntimeAnalytics.ClassToken(personajeSeleccionado));

      
   }



}
