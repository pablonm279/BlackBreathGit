using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EncounterZoneType
{
   BosqueAngustiante,
   PasoVientoHelado,
   Nedukazal,
   Generico,
   Subterraneo
}

public enum BattleEncounterType
{
   Normal,
   Elite,
   AtaqueCaravana,
   Subterraneo
}

[Serializable]
public class EnemyTierPool
{
   public List<GameObject> tier1 = new List<GameObject>();
   public List<GameObject> tier2 = new List<GameObject>();
   public List<GameObject> tier3 = new List<GameObject>();
   public List<GameObject> tier4 = new List<GameObject>();
   public List<GameObject> tier5 = new List<GameObject>();
}

[Serializable]
public class EnemyFactionConfig
{
   public string factionId;
   public string displayName;
   public EnemyTierPool tiers = new EnemyTierPool();
}

[Serializable]
public class BattleFactionPool
{
   public BattleEncounterType battleType;
   public List<EnemyFactionConfig> factions = new List<EnemyFactionConfig>();
}

[Serializable]
public class EncounterZoneConfig
{
   public string inspectorLabel;
   [Range(0f, 100f)] public float chanceEncuentroPropio = 70f;
   public List<BattleFactionPool> battlePools = new List<BattleFactionPool>();

   public BattleFactionPool GetPool(BattleEncounterType type)
   {
      return battlePools.Find(pool => pool != null && pool.battleType == type);
   }
}

public class AtributosZona : MonoBehaviour
{
   public string Nombre;
   public int ID; //1 Bosque Ardiente, 2 Paso Vientohelado, 3 Nedukazal

   public TextMeshProUGUI txtNombreZona;
   public int FASE; //En que posición sale la zona, para determinar dificultad de encuentros
   public int modRecoleccionMateriales;
   public int modRecoleccionSuministros;
   public int modChanceEmboscada;

   public int modChanceExploracion;

   public int Clima_chances_Sol;
   public int Clima_chances_Calor;
   public int Clima_chances_Lluvia;
   public int Clima_chances_Nieve;
   public int Clima_chances_Niebla;
   public int Clima_chances_EspecialZona1;
   public int Clima_chances_EspecialZona2;
   public int PasoVientoHelado_FuerzaKaleTav = 0;

   [Header("Encuentros dinámicos")]
   public EncounterZoneConfig bosqueAngustianteEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig pasoVientoHeladoEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig NedukazalEncuentros = new EncounterZoneConfig();

   public EncounterZoneConfig genericosEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig subterraneosEncuentros = new EncounterZoneConfig();

   [Header("Debug de encuentros")]
   public List<GameObject> debugEncounterUnits = new List<GameObject>();

   MapDecorator scMapDecorator;

   void Awake()
   {
      scMapDecorator = GetComponent<MapDecorator>();
      EnsureEncounterLabels();
   }

   void OnValidate()
   {
      EnsureEncounterLabels();
   }

   void EnsureEncounterLabels()
   {
      if (bosqueAngustianteEncuentros != null && string.IsNullOrWhiteSpace(bosqueAngustianteEncuentros.inspectorLabel))
      {
         bosqueAngustianteEncuentros.inspectorLabel = "Bosque Angustiante";
      }
      if (pasoVientoHeladoEncuentros != null && string.IsNullOrWhiteSpace(pasoVientoHeladoEncuentros.inspectorLabel))
      {
         pasoVientoHeladoEncuentros.inspectorLabel = "Paso Vientohelado";
      }
      if (genericosEncuentros != null && string.IsNullOrWhiteSpace(genericosEncuentros.inspectorLabel))
      {
         genericosEncuentros.inspectorLabel = "Genéricos";
      }
      if (subterraneosEncuentros != null && string.IsNullOrWhiteSpace(subterraneosEncuentros.inspectorLabel))
      {
         subterraneosEncuentros.inspectorLabel = "Subterráneos";
      }
   }

   public EncounterZoneConfig GetEncounterConfig(EncounterZoneType zoneType)
   {
      switch (zoneType)
      {
         case EncounterZoneType.BosqueAngustiante:
            return bosqueAngustianteEncuentros;
         case EncounterZoneType.PasoVientoHelado:
            return pasoVientoHeladoEncuentros;
         case EncounterZoneType.Nedukazal:
            return NedukazalEncuentros;
         case EncounterZoneType.Generico:
            return genericosEncuentros;
         case EncounterZoneType.Subterraneo:
            return subterraneosEncuentros;
         default:
            return null;
      }
   }

   public float GetChanceEncuentroPropio(EncounterZoneType zoneType)
   {
      var config = GetEncounterConfig(zoneType);
      return config != null ? config.chanceEncuentroPropio : 70f;
   }

   public EncounterZoneType GetZoneTypeById(int zoneId)
   {
      switch (zoneId)
      {
         case 1:
            return EncounterZoneType.BosqueAngustiante;
         case 2:
            return EncounterZoneType.PasoVientoHelado;
         case 3:
            return EncounterZoneType.Nedukazal;
         default:
            return EncounterZoneType.Generico;
      }
   }

   public MeshRenderer TexturaTerreno;
   public MeshRenderer TexturaTerrenoExtension;
   public MeshRenderer TexturaBordeMapa;





   public Material MaterialBosqueAngustiante_Terreno;
   public Material MaterialBosqueAngustiante_BordeMapa;

   public Material MaterialPasoVientoHelado_Terreno;
   public Material MaterialPasoVientoHelado_BordeMapa;

   public Material MaterialNedukazal_Terreno;
   public Material MaterialNedukazal_BordeMapa;

   public GameObject bosqueardienteContenedorGameObjects;
   public GameObject pasovientoheladoContenedorGameObjects;
   public GameObject nedukazalContenedorGameObjects;

   public GameObject BosqueAngustiante_ArbolQuemado1;
   public GameObject BosqueAngustiante_ArbolQuemado2;
   public GameObject BosqueAngustiante_ArbolQuemado3;

   public GameObject BosqueAngustiante_ManchaCeniza1;
   public GameObject BosqueAngustiante_Maleza1;
   public GameObject BosqueAngustiante_Piedra1;
   public GameObject BosqueAngustiante_Piedra2;
   public GameObject BosqueAngustiante_Llama;

   public GameObject PasoVientoHelado_Arbol1;
   public GameObject PasoVientoHelado_Arbol2;
   public GameObject PasoVientoHelado_Mancha2;
   public GameObject PasoVientoHelado_Manchahielo;
   public GameObject PasoVientoHelado_Maleza1;
   public GameObject PasoVientoHelado_Piedra1;
   public GameObject PasoVientoHelado_Piedra2;
   public GameObject PasoVientoHelado_Piedra3;
   public GameObject PasoVientoHelado_grieta1;
   public GameObject PasoVientoHelado_aldeatribal;
   public GameObject PasoVientoHelado_simbolopagano;
   public GameObject PasoVientoHelado_efigie;


   public GameObject BosqueArdiente_Descripcion;
   public GameObject Pasovientohelado_Descripcion;
   public GameObject Nedukazal_Descripcion;

   public void ConstruirZonaBosqueAngustiante(int iFASE)
   {
      Nombre = "Bosque Angustiante"; //dejar asi por ahora
      FASE = iFASE;
      ID = 1;
      modRecoleccionMateriales = -10;
      modRecoleccionSuministros = 5;
      modChanceEmboscada = 15;


      Invoke("AumentarDifconDelayPorPeligroBosqueArdiente", 1.5f);

      modChanceExploracion = 5;

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 50;
      Clima_chances_Lluvia = 60;
      Clima_chances_Nieve = 60;
      Clima_chances_Niebla = 80;
      Clima_chances_EspecialZona1 = 100;



      if (TRADU.i != null)
      { txtNombreZona.text = TRADU.i.Traducir("El Bosque Ardiente"); }

      BosqueArdiente_Descripcion.SetActive(true);
      Pasovientohelado_Descripcion.SetActive(false);

      CampaignManager.Instance.BosqueArdienteMecanicaIncendio(100);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
     StartCoroutine(AdornarBosqueArdienteConFadeAsync());


      Nedukazal_CaravanaLuz.SetActive(false);
      VFX_AlientoNegroNedukazal.SetActive(true);

   }

   void PlayMusic()
   {
      MusicManager.Instance.PlayCampania(ID);
   }
   IEnumerator AdornarBosqueArdienteConFadeAsync()
   {

      TexturaTerreno.material = MaterialBosqueAngustiante_Terreno;
      TexturaTerrenoExtension.material = MaterialBosqueAngustiante_Terreno;
      TexturaBordeMapa.material = MaterialBosqueAngustiante_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(false);
      bosqueardienteContenedorGameObjects.SetActive(true);
      CampaignManager.Instance.sunController = bosqueardienteContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();
      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }

      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado1,
         cantidad: 2850,
         distCaminoOverride: 0.08f,
         distNodoOverride: 0.1f,
         rOverride: 0.78f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado2,
         cantidad: 345,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.12f,
         rOverride: 5.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ManchaCeniza1,
         cantidad: 85,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra1,
         cantidad: 70,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.8f,
         rOverride: 7.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra2,
         cantidad: 10,
         distCaminoOverride: 2.0f,
         distNodoOverride: 2.2f,
         rOverride: 11.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Maleza1,
         cantidad: 200,
         distCaminoOverride: 0.2f,
         distNodoOverride: 0.8f,
         rOverride: 4.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Llama,
         cantidad: 25,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.9f,
         rOverride: 8.0f,
         kOverride: 20);

      if (admin != null)
      {
         // Liberar bloqueo y volver a mostrar la escena
         admin.SetFaderHold(false);
         yield return admin.FadeOut(0.25f);
      }
   }


   public void ConstruirZonaPasoVientoHelado(int iFASE)
   {
      Nombre = "Paso Vientohelado";
      FASE = iFASE;
      ID = 2;
      modRecoleccionMateriales = 10;
      modRecoleccionSuministros = -15;
      modChanceEmboscada = 10;
      PasoVientoHelado_FuerzaKaleTav = 0;

      modChanceExploracion = -10;

      Invoke("AumentarDifconDelayPorPeligroPasoVientoHelado", 1.5f);

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 40;
      Clima_chances_Lluvia = 43;
      Clima_chances_Nieve = 70;
      Clima_chances_Niebla = 93;
      Clima_chances_EspecialZona1 = 100;



      Pasovientohelado_Descripcion.SetActive(true);
      BosqueArdiente_Descripcion.SetActive(false);

      txtNombreZona.text = TRADU.i.Traducir(Nombre);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      StartCoroutine(AdornarPasoVientoHeladoConFadeAsync());

      Nedukazal_CaravanaLuz.SetActive(false);
      VFX_AlientoNegroNedukazal.SetActive(true);




   }
   IEnumerator AdornarPasoVientoHeladoConFadeAsync()
   {

      TexturaTerreno.material = MaterialPasoVientoHelado_Terreno;
      TexturaTerrenoExtension.material = MaterialPasoVientoHelado_Terreno;
      TexturaBordeMapa.material = MaterialPasoVientoHelado_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(true);
      bosqueardienteContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(false);
      CampaignManager.Instance.sunController = pasovientoheladoContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();

      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }


      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado1,
         cantidad: 28,
         distCaminoOverride: 0.11f,
         distNodoOverride: 0.15f,
         rOverride: 6.25f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_Arbol1,
       cantidad: 39,
       distCaminoOverride: 0.11f,
       distNodoOverride: 0.15f,
       rOverride: 5.95f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_Arbol2,
       cantidad: 55,
       distCaminoOverride: 0.11f,
       distNodoOverride: 0.15f,
       rOverride: 5.85f,
       kOverride: 20);


      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Manchahielo,
        cantidad: 7,
        distCaminoOverride: 1.1f,
        distNodoOverride: 0.83f,
        rOverride: 13.85f,
        kOverride: 20);
      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Mancha2,
        cantidad: 7,
        distCaminoOverride: 0.11f,
        distNodoOverride: 0.15f,
        rOverride: 7.85f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Maleza1,
         cantidad: 1650,
         distCaminoOverride: 0.1f,
         distNodoOverride: 0.8f,
         rOverride: 0.9f,
         kOverride: 30);

      yield return scMapDecorator.GenerarAsyncCR(
          BosqueAngustiante_ManchaCeniza1,
          cantidad: 60,
          distCaminoOverride: 0.10f,
          distNodoOverride: 0.10f,
          rOverride: 10.8f,
          kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Piedra1,
        cantidad: 85,
        distCaminoOverride: 0.10f,
        distNodoOverride: 0.10f,
        rOverride: 8.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Piedra2,
        cantidad: 68,
        distCaminoOverride: 0.10f,
        distNodoOverride: 0.10f,
        rOverride: 10.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra3,
         cantidad: 11,
         distCaminoOverride: 1.60f,
         distNodoOverride: 1.80f,
         rOverride: 16.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_grieta1,
       cantidad: 4,
       distCaminoOverride: 1.4f,
       distNodoOverride: 1.40f,
       rOverride: 15.8f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
       PasoVientoHelado_aldeatribal,
       cantidad: 12,
       distCaminoOverride: 0.85f,
       distNodoOverride: 1.30f,
       rOverride: 7.8f,
       kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_efigie,
        cantidad: 15,
        distCaminoOverride: 0.3f,
        distNodoOverride: 0.50f,
        rOverride: 7.8f,
        kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_simbolopagano,
        cantidad: 4,
        distCaminoOverride: 0.8f,
        distNodoOverride: 0.50f,
        rOverride: 10.8f,
        kOverride: 20);

      if (admin != null)
      {
         // Liberar bloqueo y volver a mostrar la escena
         admin.SetFaderHold(false);
         yield return admin.FadeOut(0.25f);
      }
   }

   public GameObject Nedukazal_CaravanaLuz;
   public void ConstruirZonaNedukazal(int iFASE)
   {
      Nombre = "Nedukazal";
      FASE = iFASE;
      ID = 3;
      modRecoleccionMateriales = 20;
      modRecoleccionSuministros = -25;
      modChanceEmboscada = 20;

      modChanceExploracion = -25;


      Invoke("AumentarDifconDelayPorPeligroNedukazal", 1.5f);


      Clima_chances_Sol = 00;
      Clima_chances_Calor = 00;
      Clima_chances_Lluvia = 00;
      Clima_chances_Nieve = 00;
      Clima_chances_Niebla = 00;
      Clima_chances_EspecialZona1 = 60; //60
      Clima_chances_EspecialZona2 = 100;



      Pasovientohelado_Descripcion.SetActive(false);
      BosqueArdiente_Descripcion.SetActive(false);
      Nedukazal_Descripcion.SetActive(true);

      txtNombreZona.text = TRADU.i.Traducir(Nombre);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      StartCoroutine(AdornarNedukazalConFadeAsync());


      Nedukazal_CaravanaLuz.SetActive(true);
      ActualizarLuzNedukazal();
      VFX_AlientoNegroNedukazal.SetActive(false);



   }

   public void ActualizarLuzNedukazal()
   {
      if (Nedukazal_CaravanaLuz != null)
      {
         var luz = Nedukazal_CaravanaLuz.GetComponent<Light>();
         if (luz != null)
         {
            luz.range = 6 + CampaignManager.Instance.mejoraCaravanaAntorchas;
         }
      }
   }


   public GameObject Nedukazal_Escombro1;
   public GameObject Nedukazal_Escombro2;
   public GameObject Nedukazal_Escombro3;
   public GameObject Nedukazal_Edificio1;
   public GameObject Nedukazal_Edificio2;
   public GameObject Nedukazal_Maleza1;
   public GameObject Nedukazal_Aldea1;
   public GameObject VFX_AlientoNegroNedukazal;





   IEnumerator AdornarNedukazalConFadeAsync()
   {

      TexturaTerreno.material = MaterialNedukazal_Terreno;
      TexturaTerrenoExtension.material = MaterialNedukazal_Terreno;
      //TexturaBordeMapa.material = MaterialNedukazal_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(false);
      bosqueardienteContenedorGameObjects.SetActive(false);
      nedukazalContenedorGameObjects.SetActive(true);
      CampaignManager.Instance.sunController = nedukazalContenedorGameObjects.transform.GetChild(0).gameObject.GetComponent<SunController>();
      // Respetar timing previo y dejar terminar el fade inicial del AdministradorEscenas
      yield return new WaitForSecondsRealtime(0.5f);

      var admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
      if (admin != null)
      {
         // Tapón negro inmediato (sin fade-in) y bloqueo de fades concurrentes
         admin.SetFaderHold(true); // fuerza alpha=1 inmediatamente
      }

      // Async sin congelar: replicamos las llamadas Generar pero con yield
      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Escombro1,
         cantidad: 120,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.125f,
         rOverride: 6.7f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
           BosqueAngustiante_ManchaCeniza1,
           cantidad: 105,
           distCaminoOverride: 0.10f,
           distNodoOverride: 0.40f,
           rOverride: 10.8f,
           kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Escombro3,
         cantidad: 1005,
         distCaminoOverride: 0.09f,
         distNodoOverride: 0.6f,
         rOverride: 1.6f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Edificio1,
         cantidad: 5,
         distCaminoOverride: 1.5f,
         distNodoOverride: 1.9f,
         rOverride: 13.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         Nedukazal_Edificio2,
         cantidad: 3,
         distCaminoOverride: 1.5f,
         distNodoOverride: 2.2f,
         rOverride: 27.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
           Nedukazal_Maleza1,
           cantidad: 300,
           distCaminoOverride: 0.7f,
           distNodoOverride: 2.5f,
           rOverride: 1.67f,
           kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
          Nedukazal_Aldea1,
          cantidad: 15,
          distCaminoOverride: 0.28f,
          distNodoOverride: 0.85f,
          rOverride: 7.5f,
          kOverride: 20);

      /* yield return scMapDecorator.GenerarAsyncCR(
          BosqueAngustiante_Llama,
          cantidad: 25,
          distCaminoOverride: 0.6f,
          distNodoOverride: 0.9f,
          rOverride: 8.0f,
          kOverride: 20);
 */
      if (admin != null)
      {
         // Liberar bloqueo y volver a mostrar la escena
         admin.SetFaderHold(false);
         yield return admin.FadeOut(0.25f);
      }
   }



   // Lista para llevar registro del estado de las zonas
   // 0: No cruzada, 1: Cruzada, 2: Descartada
   public List<int> ZonasEstado = new List<int>();

   /// <summary>
   /// Inicializa la lista de estados de las zonas.
   /// Debe llamarse al inicio del juego o cuando se reinicia la campaña.
   /// </summary>



   /// <summary>
   /// Actualiza el estado de una zona específica.
   /// </summary>
   /// <param name="zonaID">El ID de la zona a actualizar (índice en la lista).</param>
   /// <param name="estado">El nuevo estado de la zona (0: No cruzada, 1: Cruzada, 2: Descartada).</param>
   public void ActualizarEstadoZona(int zonaID, int estado)
   {
      zonaID -= 1; // Ajustar para índice basado en cero
      if (zonaID >= 0 && zonaID < ZonasEstado.Count)
      {
         ZonasEstado[zonaID] = estado;
      }
      else
      {
         Debug.LogWarning($"ZonaID {zonaID} está fuera de rango.");
      }
   }


   public void GenerarZona(int ID = 0)
   {
      int zona = ID;

      FASE++;
      // Si no se pasa ID, seleccionar aleatoriamente de las zonas con estado 0
      if (zona == 0)
      {
         var zonasDisponibles = new List<int>();
         for (int i = 0; i < ZonasEstado.Count; i++)
         {
            if (ZonasEstado[i] == 0)
            {
               zonasDisponibles.Add(i + 1); // Los IDs de las zonas comienzan desde 1
            }
         }

         if (zonasDisponibles.Count > 0)
         {
            zona = zonasDisponibles[UnityEngine.Random.Range(0, zonasDisponibles.Count)];
         }
         else
         {
            Debug.LogWarning("No hay zonas disponibles con estado 0.");
            return;
         }
      }



      switch (zona)
      {
         case 1:
            ConstruirZonaBosqueAngustiante(FASE);
            break;
         case 2:
            ConstruirZonaPasoVientoHelado(FASE);
            break;
         case 3:
            ConstruirZonaNedukazal(FASE);
            break;

      }

      CampaignManager.Instance.scMapaManager.GenerarNodos();
      CampaignManager.Instance.ForzarTiradaClima();
      CampaignManager.Instance.AplicarEfectosMejorasPuerto();

   }


   void AumentarDifconDelayPorPeligroNedukazal()
   {
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroNedukazal);
      MetaprogresionManager.Instance.NivelPeligroNedukazal++;
   }
   void AumentarDifconDelayPorPeligroBosqueArdiente()
   {
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroBosqueArdiente);
      MetaprogresionManager.Instance.NivelPeligroBosqueArdiente++;
   }
   void AumentarDifconDelayPorPeligroPasoVientoHelado()
   { 
      CampaignManager.Instance.IncrementarDificultadSegunPeligroRegion(MetaprogresionManager.Instance.NivelPeligroPasoVientohelado);
      MetaprogresionManager.Instance.NivelPeligroPasoVientohelado++;
   }
}



