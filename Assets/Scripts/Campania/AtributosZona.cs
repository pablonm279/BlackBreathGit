using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EncounterZoneType
{
   BosqueAngustiante,
   PasoVientoHelado,
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
   public List<BattleFactionPool> battlePools = new List<BattleFactionPool>();

   public BattleFactionPool GetPool(BattleEncounterType type)
   {
      return battlePools.Find(pool => pool != null && pool.battleType == type);
   }
}

public class AtributosZona : MonoBehaviour
{
   public string Nombre;
   public int ID;

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

   public int PasoVientoHelado_FuerzaKaleTav = 0;

   [Header("Encuentros dinámicos")]
   public EncounterZoneConfig bosqueAngustianteEncuentros = new EncounterZoneConfig();
   public EncounterZoneConfig pasoVientoHeladoEncuentros = new EncounterZoneConfig();
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
         case EncounterZoneType.Generico:
            return genericosEncuentros;
         case EncounterZoneType.Subterraneo:
            return subterraneosEncuentros;
         default:
            return null;
      }
   }

   public EncounterZoneType GetZoneTypeById(int zoneId)
   {
      switch (zoneId)
      {
         case 1:
            return EncounterZoneType.BosqueAngustiante;
         case 2:
            return EncounterZoneType.PasoVientoHelado;
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

   public GameObject bosqueardienteContenedorGameObjects;
   public GameObject pasovientoheladoContenedorGameObjects;
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
   public void ConstruirZonaBosqueAngustiante(int iFASE)
   {
      Nombre = "Bosque Angustiante"; //dejar asi por ahora
      FASE = iFASE;
      ID = 1;
      modRecoleccionMateriales = -10;
      modRecoleccionSuministros = 5;
      modChanceEmboscada = 10;

      modChanceExploracion = 5;

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 50;
      Clima_chances_Lluvia = 60;
      Clima_chances_Nieve = 60;
      Clima_chances_Niebla = 80;
      Clima_chances_EspecialZona1 = 100;


      txtNombreZona.text = TRADU.i.Traducir("El Bosque Ardiente");

      BosqueArdiente_Descripcion.SetActive(true);
      Pasovientohelado_Descripcion.SetActive(false);

      CampaignManager.Instance.BosqueArdienteMecanicaIncendio(100);


      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      StartCoroutine(AdornarBosqueArdienteConFadeAsync());





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
         cantidad: 2350,
         distCaminoOverride: 0.08f,
         distNodoOverride: 0.1f,
         rOverride: 0.87f,
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
      modChanceEmboscada = 0;

      modChanceExploracion = -10;

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





   }
 IEnumerator AdornarPasoVientoHeladoConFadeAsync()
   {
     
      TexturaTerreno.material = MaterialPasoVientoHelado_Terreno;
      TexturaTerrenoExtension.material = MaterialPasoVientoHelado_Terreno;
      TexturaBordeMapa.material = MaterialPasoVientoHelado_BordeMapa;
      pasovientoheladoContenedorGameObjects.SetActive(true);
      bosqueardienteContenedorGameObjects.SetActive(false);
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
         cantidad: 1450,
         distCaminoOverride: 0.1f,
         distNodoOverride: 0.8f,
         rOverride: 1.1f,
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




}
