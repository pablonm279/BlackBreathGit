using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSequitos : MonoBehaviour
{

   public GameObject Sequito001Herreros;
   public GameObject Sequito002Curanderos;
   public GameObject Sequito003Mercaderes;
   public GameObject Sequito004Artistas;
   public GameObject Sequito005Herboristas;
   public GameObject Sequito006Desertores;
   public GameObject Sequito007Cronistas;
   public GameObject Sequito008Refugiados;
   public GameObject Sequito009Nobles;
   public GameObject Sequito010Clerigos;
   public GameObject Sequito011Esclavos;
   public List<int> lstSequitos = new List<int>();
   public void AgregarSequito(int ID) //Agregarlos tmb en MenuNodoPersonaje!!
   {
      if (ID == 1)
      {
         GameObject goSequito = Instantiate(Sequito001Herreros, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Herreros");
         lstSequitos.Add(1);
      }

      if (ID == 2)
      {
         GameObject goSequito = Instantiate(Sequito002Curanderos, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         scSequito.txtNombre.text =  TRADU.i.Traducir("Séquito de Curanderos");
         lstSequitos.Add(2);
      }
      if (ID == 3)
      {
         GameObject goSequito = Instantiate(Sequito003Mercaderes, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoMercaderes = goSequito.GetComponent<SequitoMercaderes>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Mercaderes");
         lstSequitos.Add(3);
      }
      if (ID == 4 && !CampaignManager.Instance.scMenuSequito.TieneSequito(4))
      {
         GameObject goSequito = Instantiate(Sequito004Artistas, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoArtistas = goSequito.GetComponent<SequitoArtistas>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Artistas");
         lstSequitos.Add(4);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha unido el Séquito de Artistas a la caravana. +25 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(25);
         CampaignManager.Instance.CambiarEsperanzaActual(+15);

      }
      if (ID == 5 && !CampaignManager.Instance.scMenuSequito.TieneSequito(5))
      {
         GameObject goSequito = Instantiate(Sequito005Herboristas, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoHerboristas = goSequito.GetComponent<SequitoHerboristas>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Herboristas");
         lstSequitos.Add(5);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha unido el Séquito de Herboristas a la caravana. +10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(10);
      }
      if (ID == 6 && !CampaignManager.Instance.scMenuSequito.TieneSequito(6))
      {
         GameObject goSequito = Instantiate(Sequito006Desertores, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoDesertores = goSequito.GetComponent<SequitoDesertores>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Desertores");
         lstSequitos.Add(6);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Desertores se han unido a la Caravana. +15 Civiles -8 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(15);
         CampaignManager.Instance.CambiarEsperanzaActual(-8);
      }
      if (ID == 7 && !CampaignManager.Instance.scMenuSequito.TieneSequito(7))
      {
         GameObject goSequito = Instantiate(Sequito007Cronistas, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoCronistas = goSequito.GetComponent<SequitoCronistas>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Cronistas");
         lstSequitos.Add(7);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas se han unido a la Caravana. +10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(10);
      }
      if (ID == 8 && !CampaignManager.Instance.scMenuSequito.TieneSequito(8))
      {
         GameObject goSequito = Instantiate(Sequito008Refugiados, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoRefugiados = goSequito.GetComponent<SequitoRefugiados>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Refugiados");
         lstSequitos.Add(8);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Refugiados se han unido a la Caravana. +35 Civiles  +30 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(35);
         CampaignManager.Instance.CambiarEsperanzaActual(30);
      }
      if (ID == 9 && !CampaignManager.Instance.scMenuSequito.TieneSequito(9))
      {
         GameObject goSequito = Instantiate(Sequito009Nobles, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoNobles = goSequito.GetComponent<SequitoNobles>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Nobles");
         lstSequitos.Add(9);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Nobles se han unido a la Caravana. +25 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(25);
      }
      if (ID == 10 && !CampaignManager.Instance.scMenuSequito.TieneSequito(10))
      {
         GameObject goSequito = Instantiate(Sequito010Clerigos, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoClerigos = goSequito.GetComponent<SequitoClerigos>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Clérigos");
         lstSequitos.Add(10);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Clérigos del Sol Purificador se han unido a la Caravana. +20 Civiles +15 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(20);
         CampaignManager.Instance.CambiarEsperanzaActual(15);
      }
      if (ID == 11 && !CampaignManager.Instance.scMenuSequito.TieneSequito(11))
      {
         GameObject goSequito = Instantiate(Sequito011Esclavos, transform.GetChild(1).gameObject.transform);
         Sequito scSequito = goSequito.GetComponent<Sequito>();

         CampaignManager.Instance.scSequitoEsclavos = goSequito.GetComponent<SequitoEsclavos>();
         scSequito.txtNombre.text = TRADU.i.Traducir("Séquito de Esclavos");
         lstSequitos.Add(11);

         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Esclavos se han unido a la Caravana. +30 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(30);
         CampaignManager.Instance.CambiarBueyesActuales(0); // Actualiza la capacidad de carga al liberar esclavos, 0 porque no cambia nada


      }


   }

   public bool TieneSequito(int ID)
   {
      return lstSequitos.Contains(ID);
   }

   public void RemoverSequito(int ID)
   {
      //Iniciales
      if (ID == 1)
      {
         lstSequitos.Remove(1);
      }
      if (ID == 2)
      {
         lstSequitos.Remove(2);
      }
      if (ID == 3)
      {
         lstSequitos.Remove(3);
      }

      //Artistas
      if (ID == 4)
      {
         lstSequitos.Remove(4);
         CampaignManager.Instance.scSequitoArtistas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Artistas ha abandonado la caravana. -25 Civiles -15 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(-25);
         CampaignManager.Instance.CambiarEsperanzaActual(-15);

      }
      //Herboristas
      if (ID == 5)
      {
         lstSequitos.Remove(5);
         CampaignManager.Instance.scSequitoHerboristas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Herboristas ha abandonado la caravana. -10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-10);

      }
      //Desertores
      if (ID == 6)
      {
         lstSequitos.Remove(6);
         CampaignManager.Instance.scSequitoDesertores = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Desertores han abandonado la Caravana. -15 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-15);

      }
      //Cronistas
      if (ID == 7)
      {
         lstSequitos.Remove(7);
         CampaignManager.Instance.scSequitoCronistas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas han abandonado la Caravana. -10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-10);

      }
      //Refugiados
      if (ID == 8)
      {
         lstSequitos.Remove(8);
         CampaignManager.Instance.scSequitoRefugiados = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Refugiados han abandonado la Caravana. -35 Civiles -40 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(-35);
         CampaignManager.Instance.CambiarEsperanzaActual(-40);

      }
      //Nobles
      if (ID == 9)
      {
         lstSequitos.Remove(9);
         CampaignManager.Instance.scSequitoNobles = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Nobles han abandonado la Caravana. -25 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-25);

      }
      //Clérigos
      if (ID == 10)
      {
         lstSequitos.Remove(10);
         CampaignManager.Instance.scSequitoClerigos = null;
         //CampaignManager.Instance.EscribirLog("-Los Clérigos han abandonado la Caravana. -20 Civiles -20 Esperanza");
         CampaignManager.Instance.CambiarCivilesActuales(-20);
         CampaignManager.Instance.CambiarEsperanzaActual(-20);

      }
      //Esclavos
      if (ID == 11)
      {
         lstSequitos.Remove(11);
         CampaignManager.Instance.scSequitoEsclavos = null;
         //CampaignManager.Instance.EscribirLog("-Los Esclavos han abandonado la Caravana. -30 Civiles");
         CampaignManager.Instance.CambiarCivilesActuales(-30);
         CampaignManager.Instance.CambiarBueyesActuales(0); // Actualiza la capacidad de carga al liberar esclavos, 0 porque no cambia nada

      }



   }

   public bool SequitoAlAzarPerdido(out string nombre)
   {
      nombre = "";
   // Buscar todos los séquitos activos con intRepresentacionciviles > -1
   List<Sequito> sequitosValidos = new List<Sequito>();
   foreach (Transform child in transform.GetChild(1))
   {
      Sequito sc = child.GetComponent<Sequito>();
      if (sc != null && sc.intRepresentacionciviles > -1)
      {
         sequitosValidos.Add(sc);
      }
   }
      print(sequitosValidos.Count+" seqval");
      if (sequitosValidos.Count > 0)
      {
         // Elegir uno al azar
         int idx =UnityEngine.Random.Range(0, sequitosValidos.Count);
         Sequito sequitoAEliminar = sequitosValidos[idx];

         // Obtener el ID del séquito (asumiendo que tienes una forma de obtenerlo)
         int id = sequitoAEliminar.ID; // Asegúrate de que Sequito tiene intID

         // Remover el séquito
         RemoverSequito(id);
         nombre = sequitoAEliminar.txtNombre.text;

         // Destruir el GameObject
         Destroy(sequitoAEliminar.gameObject);

         return true;
   }
   return false;


   }
}
