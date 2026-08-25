using System.Collections.Generic;
using UnityEngine;

public class MenuSequitos : MonoBehaviour
{
   [Header("Prefabs")]
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

   [Header("Referencias UI")]
   [SerializeField] Transform placeholderContenido;
   [SerializeField] Transform contenedorInstancias;

   public List<int> lstSequitos = new List<int>();

   readonly Dictionary<int, Sequito> secuenciasActivasPorId = new Dictionary<int, Sequito>();
   Sequito contenidoActivo;

   public Transform ObtenerPlaceholderContenido()
   {
      return placeholderContenido;
   }

   public Transform ObtenerContenedorInstancias()
   {
      return contenedorInstancias;
   }

   public void RegistrarInstancia(Sequito sequito)
   {
      if (sequito == null)
      {
         return;
      }

      secuenciasActivasPorId[sequito.ID] = sequito;
      sequito.PrepararContenido(placeholderContenido);
   }

   public void DesregistrarInstancia(Sequito sequito)
   {
      if (sequito == null)
      {
         return;
      }

      if (secuenciasActivasPorId.TryGetValue(sequito.ID, out Sequito actual) && actual == sequito)
      {
         secuenciasActivasPorId.Remove(sequito.ID);
      }

      if (contenidoActivo == sequito)
      {
         contenidoActivo = null;
      }
   }

   public void MostrarContenido(Sequito sequito)
   {
      if (sequito == null)
      {
         return;
      }

      RegistrarInstancia(sequito);

      if (contenidoActivo != null && contenidoActivo != sequito)
      {
         contenidoActivo.OcultarContenido();
      }

      contenidoActivo = sequito;
      contenidoActivo.MostrarContenido();
   }

   public void OcultarContenidosInstancias()
   {
      foreach (Sequito sequito in secuenciasActivasPorId.Values)
      {
         if (sequito != null)
         {
            sequito.OcultarContenido();
         }
      }

      contenidoActivo = null;
   }

   public void MostrarContenidoSequitoTutorial(string stepId)
   {
      int idSequito = stepId switch
      {
         "Sequitos2Herreros" => 1,
         "Sequitos3Curanderos" => 2,
         "Sequitos4Mercaderes" => 3,
         _ => 0
      };

      if (idSequito == 0)
      {
         return;
      }

      Sequito sequito = ObtenerInstanciaSequito(idSequito);
      if (sequito != null)
      {
         OcultarContenidosInstancias();
         sequito.clickRepresentar();
      }
   }

   Sequito ObtenerInstanciaSequito(int idSequito)
   {
      if (secuenciasActivasPorId.TryGetValue(idSequito, out Sequito sequito) && sequito != null)
      {
         return sequito;
      }

      Transform raizBusqueda = contenedorInstancias != null ? contenedorInstancias : transform;
      Sequito[] instancias = raizBusqueda.GetComponentsInChildren<Sequito>(true);
      for (int i = 0; i < instancias.Length; i++)
      {
         Sequito instancia = instancias[i];
         if (instancia != null && instancia.ID == idSequito)
         {
            RegistrarInstancia(instancia);
            return instancia;
         }
      }

      return null;
   }

   public void LimpiarInstanciasParaCarga()
   {
      OcultarContenidosInstancias();

      List<Sequito> instancias = new List<Sequito>(secuenciasActivasPorId.Values);
      foreach (Sequito sequito in instancias)
      {
         if (sequito != null)
         {
            Destroy(sequito.gameObject);
         }
      }

      secuenciasActivasPorId.Clear();
      contenidoActivo = null;
      lstSequitos.Clear();
   }

   public SequitoCuranderos ObtenerSequitoCuranderosActivo()
   {
      if (secuenciasActivasPorId.TryGetValue(2, out Sequito sequito) && sequito != null)
      {
         return sequito.GetComponent<SequitoCuranderos>();
      }

      return null;
   }

   public bool TieneSequito(int ID)
   {
      return lstSequitos.Contains(ID);
   }

   public void AgregarSequito(int ID, bool aplicarEfectosEntrada = true)
   {
      if (TieneSequito(ID))
      {
         return;
      }

      switch (ID)
      {
         case 1:
            CrearInstanciaSequito(ID, Sequito001Herreros, "Séquito de Herreros");
            break;
         case 2:
            CrearInstanciaSequito(ID, Sequito002Curanderos, "Séquito de Curanderos");
            break;
         case 3:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito003Mercaderes, "Séquito de Mercaderes");
            CampaignManager.Instance.scSequitoMercaderes = goSequito != null ? goSequito.GetComponent<SequitoMercaderes>() : null;
            break;
         }
         case 4:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito004Artistas, "Séquito de Artistas");
            CampaignManager.Instance.scSequitoArtistas = goSequito != null ? goSequito.GetComponent<SequitoArtistas>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha unido el Séquito de Artistas a la caravana. +25 Civiles"));
               CampaignManager.Instance.CambiarCivilesActuales(25);
               CampaignManager.Instance.CambiarEsperanzaActual(15);
            }
            break;
         }
         case 5:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito005Herboristas, "Séquito de Herboristas");
            CampaignManager.Instance.scSequitoHerboristas = goSequito != null ? goSequito.GetComponent<SequitoHerboristas>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha unido el Séquito de Herboristas a la caravana. +10 Civiles"));
               CampaignManager.Instance.CambiarCivilesActuales(10);
            }
            break;
         }
         case 6:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito006Desertores, "Séquito de Desertores");
            CampaignManager.Instance.scSequitoDesertores = goSequito != null ? goSequito.GetComponent<SequitoDesertores>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Desertores se han unido a la Caravana. +15 Civiles -8 Esperanza"));
               CampaignManager.Instance.CambiarCivilesActuales(15);
               CampaignManager.Instance.CambiarEsperanzaActual(-8);
            }
            break;
         }
         case 7:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito007Cronistas, "Séquito de Cronistas");
            CampaignManager.Instance.scSequitoCronistas = goSequito != null ? goSequito.GetComponent<SequitoCronistas>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas se han unido a la Caravana. +10 Civiles"));
               CampaignManager.Instance.CambiarCivilesActuales(10);
            }
            break;
         }
         case 8:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito008Refugiados, "Séquito de Refugiados");
            CampaignManager.Instance.scSequitoRefugiados = goSequito != null ? goSequito.GetComponent<SequitoRefugiados>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Refugiados se han unido a la Caravana. +35 Civiles  +30 Esperanza"));
               CampaignManager.Instance.CambiarCivilesActuales(35);
               CampaignManager.Instance.CambiarEsperanzaActual(30);
            }
            break;
         }
         case 9:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito009Nobles, "Séquito de Nobles");
            CampaignManager.Instance.scSequitoNobles = goSequito != null ? goSequito.GetComponent<SequitoNobles>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Nobles se han unido a la Caravana. +25 Civiles"));
               CampaignManager.Instance.CambiarCivilesActuales(25);
            }
            break;
         }
         case 10:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito010Clerigos, "Séquito de Clérigos");
            CampaignManager.Instance.scSequitoClerigos = goSequito != null ? goSequito.GetComponent<SequitoClerigos>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Clérigos del Sol Purificador se han unido a la Caravana. +20 Civiles +15 Esperanza"));
               CampaignManager.Instance.CambiarCivilesActuales(20);
               CampaignManager.Instance.CambiarEsperanzaActual(15);
            }
            break;
         }
         case 11:
         {
            GameObject goSequito = CrearInstanciaSequito(ID, Sequito011Esclavos, "Séquito de Esclavos");
            CampaignManager.Instance.scSequitoEsclavos = goSequito != null ? goSequito.GetComponent<SequitoEsclavos>() : null;

            if (aplicarEfectosEntrada)
            {
               CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Esclavos se han unido a la Caravana. +30 Civiles"));
               CampaignManager.Instance.CambiarCivilesActuales(30);
               CampaignManager.Instance.CambiarBueyesActuales(0);
            }
            break;
         }
      }
   }

   GameObject CrearInstanciaSequito(int id, GameObject prefab, string nombreTraducible)
   {
      if (prefab == null)
      {
         Debug.LogWarning($"[MenuSequitos] Falta prefab para el séquito {id}.", this);
         return null;
      }

      if (contenedorInstancias == null)
      {
         Debug.LogWarning("[MenuSequitos] Falta asignar contenedorInstancias.", this);
         return null;
      }

      GameObject goSequito = Instantiate(prefab, contenedorInstancias);
      Sequito scSequito = goSequito.GetComponent<Sequito>();
      if (scSequito != null)
      {
         scSequito.txtNombre.text = TRADU.i.Traducir(nombreTraducible);
         RegistrarInstancia(scSequito);
      }

      lstSequitos.Add(id);
      return goSequito;
   }

   public void RemoverSequito(int ID)
   {
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

      if (ID == 4)
      {
         lstSequitos.Remove(4);
         CampaignManager.Instance.scSequitoArtistas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Artistas ha abandonado la caravana. -25 Civiles -15 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(-25);
         CampaignManager.Instance.CambiarEsperanzaActual(-15);
      }
      if (ID == 5)
      {
         lstSequitos.Remove(5);
         CampaignManager.Instance.scSequitoHerboristas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Herboristas ha abandonado la caravana. -10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-10);
      }
      if (ID == 6)
      {
         lstSequitos.Remove(6);
         CampaignManager.Instance.scSequitoDesertores = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Desertores han abandonado la Caravana. -15 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-15);
      }
      if (ID == 7)
      {
         lstSequitos.Remove(7);
         CampaignManager.Instance.scSequitoCronistas = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas han abandonado la Caravana. -10 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-10);
      }
      if (ID == 8)
      {
         lstSequitos.Remove(8);
         CampaignManager.Instance.scSequitoRefugiados = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Refugiados han abandonado la Caravana. -35 Civiles -40 Esperanza"));
         CampaignManager.Instance.CambiarCivilesActuales(-35);
         CampaignManager.Instance.CambiarEsperanzaActual(-40);
      }
      if (ID == 9)
      {
         lstSequitos.Remove(9);
         CampaignManager.Instance.scSequitoNobles = null;
         CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Nobles han abandonado la Caravana. -25 Civiles"));
         CampaignManager.Instance.CambiarCivilesActuales(-25);
      }
      if (ID == 10)
      {
         lstSequitos.Remove(10);
         CampaignManager.Instance.scSequitoClerigos = null;
         CampaignManager.Instance.CambiarCivilesActuales(-20);
         CampaignManager.Instance.CambiarEsperanzaActual(-20);
      }
      if (ID == 11)
      {
         lstSequitos.Remove(11);
         CampaignManager.Instance.scSequitoEsclavos = null;
         CampaignManager.Instance.CambiarCivilesActuales(-30);
         CampaignManager.Instance.CambiarBueyesActuales(0);
      }

      if (secuenciasActivasPorId.TryGetValue(ID, out Sequito sequitoActivo) && sequitoActivo != null)
      {
         secuenciasActivasPorId.Remove(ID);
         Destroy(sequitoActivo.gameObject);
      }
      else
      {
         secuenciasActivasPorId.Remove(ID);
      }
   }

   public bool SequitoAlAzarPerdido(out string nombre)
   {
      nombre = "";
      List<Sequito> sequitosValidos = new List<Sequito>();

      foreach (Sequito sc in secuenciasActivasPorId.Values)
      {
         if (sc != null && sc.intRepresentacionciviles > -1)
         {
            sequitosValidos.Add(sc);
         }
      }

      print(sequitosValidos.Count + " seqval");
      if (sequitosValidos.Count <= 0)
      {
         return false;
      }

      int idx = UnityEngine.Random.Range(0, sequitosValidos.Count);
      Sequito sequitoAEliminar = sequitosValidos[idx];
      int id = sequitoAEliminar.ID;
      nombre = sequitoAEliminar.txtNombre.text;
      RemoverSequito(id);
      return true;
   }
}
