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
      personajeSeleccionado.ActividadSeleccionada = actividadRepresentada.IDActividad;
      scActividades.ActualizarRecuadros();

      
   }



}
