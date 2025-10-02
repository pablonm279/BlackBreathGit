using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Actividades : MonoBehaviour
{
   
   public MenuPersonajes scMenuPersonajes;
   public GameObject prefabBtnActividad;
   public Transform listaActividades;
   public TextMeshProUGUI textdesc;

   public Sprite spriteActDescansar;
   public Sprite spriteActEntrenar;
   public Sprite spriteActGuardia;

   public Sprite spriteActCaballeroRelatosBatalla;
   public Sprite spriteActCaballeroMantenimientoArmadura;
   public Sprite spriteActCaballeroVigilar;
   public Sprite spriteActCazadorCazaNocturna;
   public Sprite spriteActPrepararFlechas;
   public Sprite spriteActExploracion;
   public Sprite spritePurificadoraRitualDeLimpieza;
   public Sprite spritePurificadoraAyudarDesamparados;
   public Sprite spritePurificadoraColaborarCuranderos;
   public Sprite spriteAfilarArmas;
   public Sprite spriteVigilarDesdeSombras;
   public Sprite spriteActCoercion;
   public Sprite spriteActConcArcana;
   public Sprite spriteActTelekinesis;
   public Sprite spriteActSimboloArcanoProt;

  public void ActualizarActividades()
  {
    foreach (Transform transform in listaActividades.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      Destroy(transform.gameObject);
    }
    foreach (Actividad act in scMenuPersonajes.pSel.gameObject.GetComponents<Actividad>())
    {
      GameObject btnPers = Instantiate(prefabBtnActividad, listaActividades);
      btnPers.GetComponent<btnActividad>().actividadRepresentada = act;
      btnPers.GetComponent<btnActividad>().personajeSeleccionado = scMenuPersonajes.pSel;

      switch (act.IDActividad)
      {
        case 1: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActDescansar; break; //Descansar
        case 2: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActEntrenar; break; //Entrenar
        case 3: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActGuardia; break; //Guardia
        case 4: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActCaballeroRelatosBatalla; break; //Caballero: Relatos
        case 5: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActCaballeroMantenimientoArmadura; break; //Caballero: Mantener Armadura
        case 6: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActCaballeroVigilar; break; //Caballero: Vigilar
        case 7: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActCazadorCazaNocturna; break; //Cazador: Caza Nocturna
        case 8: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActPrepararFlechas; break; //Cazador: Preparar Flechas
        case 9: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActExploracion; break; //Cazador: Exploración
        case 10: btnPers.GetComponent<btnActividad>().actImage.sprite = spritePurificadoraRitualDeLimpieza; break; //Purificadora: Ritual de Limpieza
        case 11: btnPers.GetComponent<btnActividad>().actImage.sprite = spritePurificadoraAyudarDesamparados; break; //Purificadora: Ayudar a los Desamparados
        case 12: btnPers.GetComponent<btnActividad>().actImage.sprite = spritePurificadoraColaborarCuranderos; break; //Purificadora: Colaborar con los Curanderos
        case 13: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteAfilarArmas; break; //Acechador: Afilar Armas
        case 14: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteVigilarDesdeSombras; break; //Acechador: Vigilar Desde Las Sombras
        case 15: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActCoercion; break; //Acechador: Coerción
        case 16: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActConcArcana; break; //Canalizador: Concentración Arcana
        case 17: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActTelekinesis; break; //Canalizador: Telekinesis
        case 18: btnPers.GetComponent<btnActividad>().actImage.sprite = spriteActSimboloArcanoProt; break; //Canalizador: Crear Símbolo de Proteccion Arcano
      }

    }

    ActualizarRecuadros();

   

  }



   public void ActualizarRecuadros()
   {

    foreach (btnActividad btn in listaActividades.GetComponentsInChildren<btnActividad>())
    {
      if (scMenuPersonajes.pSel.ActividadSeleccionada == btn.actividadRepresentada.IDActividad)
      {
        btn.Recuadro.SetActive(true);
        textdesc.text = btn.actividadRepresentada.desc;

      }
      else { btn.Recuadro.SetActive(false); }


      //--- Para actualizar ststs de campaña al seleccionar alguna
      CampaignManager.Instance.GetCapacidadDeCargaActual();
      CampaignManager.Instance.CambiarBueyesActuales(0);//para forzar ui
    }

   

   }
}
