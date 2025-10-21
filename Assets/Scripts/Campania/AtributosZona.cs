using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributosZona : MonoBehaviour
{
   public string Nombre;
   public int ID;

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

   //ENCUENTROS FASE 1
   public int FASE1IDEncuentroNormal1;
   public int FASE1IDEncuentroNormal2;
   public int FASE1IDEncuentroNormal3;
   public int FASE1IDEncuentroNormal4;
   public int FASE1IDEncuentroNormal5;
   public int FASE1IDEncuentroNormal6;
   public int FASE1IDEncuentroNormal7;
   public int FASE1IDEncuentroElite1;
   public int FASE1IDEncuentroElite2;
   public int FASE1IDEncuentroElite3;
   public int FASE1IDEncuentroElite4;
   public int FASE1IDEncuentroJefe1;
   public int FASE1IDEncuentroJefe2;
   public int FASE1IDAtaqueCaravana1;
   public int FASE1IDAtaqueCaravana2;

   //ENCUENTROS FASE 2
   public int FASE2IDEncuentroNormal1;
   public int FASE2IDEncuentroNormal2;
   public int FASE2IDEncuentroNormal3;
   public int FASE2IDEncuentroNormal4;
   public int FASE2IDEncuentroNormal5;
   public int FASE2IDEncuentroNormal6;
   public int FASE2IDEncuentroNormal7;
   public int FASE2IDEncuentroElite1;
   public int FASE2IDEncuentroElite2;
   public int FASE2IDEncuentroElite3;
   public int FASE2IDEncuentroElite4;
   public int FASE2IDEncuentroJefe1;
   public int FASE2IDEncuentroJefe2;
   public int FASE2IDAtaqueCaravana1;
   public int FASE2IDAtaqueCaravana2;

   //ENCUENTROS FASE 3
   public int FASE3IDEncuentroNormal1;
   public int FASE3IDEncuentroNormal2;
   public int FASE3IDEncuentroNormal3;
   public int FASE3IDEncuentroNormal4;
   public int FASE3IDEncuentroNormal5;
   public int FASE3IDEncuentroNormal6;
   public int FASE3IDEncuentroNormal7;
   public int FASE3IDEncuentroElite1;
   public int FASE3IDEncuentroElite2;
   public int FASE3IDEncuentroElite3;
   public int FASE3IDEncuentroElite4;
   public int FASE3IDEncuentroJefe1;
   public int FASE3IDEncuentroJefe2;
   public int FASE3IDAtaqueCaravana1;
   public int FASE3IDAtaqueCaravana2;


   MapDecorator scMapDecorator;

   void Awake()
   {
      scMapDecorator = GetComponent<MapDecorator>();
   }

   public MeshRenderer TexturaTerreno;
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

     


      FASE1IDEncuentroNormal1 = 1;
      FASE1IDEncuentroNormal2 = 2;
      FASE1IDEncuentroNormal3 = 3;
      FASE1IDEncuentroNormal4 = 4;
      FASE1IDEncuentroNormal5 = 5;
      FASE1IDEncuentroNormal6 = 6;
      FASE1IDEncuentroNormal7 = 7;
      FASE1IDEncuentroElite1 = 8;
      FASE1IDEncuentroElite2 = 9;
      FASE1IDEncuentroElite3 = 10;
      FASE1IDEncuentroJefe1 = 11;
      FASE1IDEncuentroJefe2 = 11; //!! cambiar cuando este el segundo jefe de fase 1
      FASE1IDAtaqueCaravana1 = 13;
      FASE1IDAtaqueCaravana2 = 14;

      FASE2IDEncuentroNormal1 = 000;
      FASE2IDEncuentroNormal2 = 000;
      FASE2IDEncuentroNormal3 = 000;
      FASE2IDEncuentroNormal4 = 000;
      FASE2IDEncuentroNormal5 = 000;
      FASE2IDEncuentroNormal6 = 000;
      FASE2IDEncuentroNormal7 = 000;
      FASE2IDEncuentroElite1 = 000;
      FASE2IDEncuentroElite2 = 000;
      FASE2IDEncuentroElite3 = 000;
      FASE2IDEncuentroElite4 = 000;
      FASE2IDEncuentroJefe1 = 000;
      FASE2IDEncuentroJefe2 = 000;
      FASE2IDAtaqueCaravana1 = 000;
      FASE2IDAtaqueCaravana2 = 000;


      FASE3IDEncuentroNormal1 = 000;
      FASE3IDEncuentroNormal2 = 000;
      FASE3IDEncuentroNormal3 = 000;
      FASE3IDEncuentroNormal4 = 000;
      FASE3IDEncuentroNormal5 = 000;
      FASE3IDEncuentroNormal6 = 000;
      FASE3IDEncuentroNormal7 = 000;
      FASE3IDEncuentroElite1 = 000;
      FASE3IDEncuentroElite2 = 000;
      FASE3IDEncuentroElite3 = 000;
      FASE3IDEncuentroElite4 = 000;
      FASE3IDEncuentroJefe1 = 000;
      FASE3IDEncuentroJefe2 = 000;
      FASE3IDAtaqueCaravana1 = 000;
      FASE3IDAtaqueCaravana2 = 000;






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
         cantidad: 1350,
         distCaminoOverride: 0.08f,
         distNodoOverride: 0.1f,
         rOverride: 0.85f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado2,
         cantidad: 145,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.12f,
         rOverride: 5.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ManchaCeniza1,
         cantidad: 65,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra1,
         cantidad: 40,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.8f,
         rOverride: 7.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra2,
         cantidad: 5,
         distCaminoOverride: 2.0f,
         distNodoOverride: 2.2f,
         rOverride: 11.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Maleza1,
         cantidad: 120,
         distCaminoOverride: 0.2f,
         distNodoOverride: 0.8f,
         rOverride: 4.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Llama,
         cantidad: 45,
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
      Nombre = "Paso Viento Helado";
      FASE = iFASE;
      ID = 2;
      modRecoleccionMateriales = +10;
      modRecoleccionSuministros = -15;
      modChanceEmboscada = 0;

      modChanceExploracion = -10;

       Clima_chances_Sol = 40;
       Clima_chances_Calor = 40;
       Clima_chances_Lluvia = 43;
       Clima_chances_Nieve = 70;
       Clima_chances_Niebla = 93;
       Clima_chances_EspecialZona1 = 100;
       



      FASE1IDEncuentroNormal1 = 1;
      FASE1IDEncuentroNormal2 = 2;
      FASE1IDEncuentroNormal3 = 3;
      FASE1IDEncuentroNormal4 = 4;
      FASE1IDEncuentroNormal5 = 5;
      FASE1IDEncuentroNormal6 = 6;
      FASE1IDEncuentroNormal7 = 7;
      FASE1IDEncuentroElite1 = 8;
      FASE1IDEncuentroElite2 = 9;
      FASE1IDEncuentroElite3 = 10;
      FASE1IDEncuentroJefe1 = 11;
      FASE1IDEncuentroJefe2 = 11; //!! cambiar cuando este el segundo jefe de fase 1
      FASE1IDAtaqueCaravana1 = 13;
      FASE1IDAtaqueCaravana2 = 14;

      FASE2IDEncuentroNormal1 = 000;
      FASE2IDEncuentroNormal2 = 000;
      FASE2IDEncuentroNormal3 = 000;
      FASE2IDEncuentroNormal4 = 000;
      FASE2IDEncuentroNormal5 = 000;
      FASE2IDEncuentroNormal6 = 000;
      FASE2IDEncuentroNormal7 = 000;
      FASE2IDEncuentroElite1 = 000;
      FASE2IDEncuentroElite2 = 000;
      FASE2IDEncuentroElite3 = 000;
      FASE2IDEncuentroElite4 = 000;
      FASE2IDEncuentroJefe1 = 000;
      FASE2IDEncuentroJefe2 = 000;
      FASE2IDAtaqueCaravana1 = 000;
      FASE2IDAtaqueCaravana2 = 000;


      FASE3IDEncuentroNormal1 = 000;
      FASE3IDEncuentroNormal2 = 000;
      FASE3IDEncuentroNormal3 = 000;
      FASE3IDEncuentroNormal4 = 000;
      FASE3IDEncuentroNormal5 = 000;
      FASE3IDEncuentroNormal6 = 000;
      FASE3IDEncuentroNormal7 = 000;
      FASE3IDEncuentroElite1 = 000;
      FASE3IDEncuentroElite2 = 000;
      FASE3IDEncuentroElite3 = 000;
      FASE3IDEncuentroElite4 = 000;
      FASE3IDEncuentroJefe1 = 000;
      FASE3IDEncuentroJefe2 = 000;
      FASE3IDAtaqueCaravana1 = 000;
      FASE3IDAtaqueCaravana2 = 000;






      Invoke("PlayMusic", 0.2f);
      // Usar fader como tapón mientras se adorna el mapa (async, sin freeze)
      StartCoroutine(AdornarPasoVientoHeladoConFadeAsync());





   }
 IEnumerator AdornarPasoVientoHeladoConFadeAsync()
   {
     
      TexturaTerreno.material = MaterialPasoVientoHelado_Terreno;
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
         cantidad: 14,
         distCaminoOverride: 0.11f,
         distNodoOverride: 0.15f,
         rOverride: 6.25f,
         kOverride: 20);
         
       yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Arbol1,
        cantidad: 25,
        distCaminoOverride: 0.11f,
        distNodoOverride: 0.15f,
        rOverride: 5.95f,
        kOverride: 20);
      
       yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Arbol2,
        cantidad: 40,
        distCaminoOverride: 0.11f,
        distNodoOverride: 0.15f,
        rOverride: 5.85f,
        kOverride: 20);
      
   
      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Manchahielo,
        cantidad: 4,
        distCaminoOverride: 1.1f,
        distNodoOverride: 0.83f,
        rOverride: 13.85f,
        kOverride: 20);
      yield return scMapDecorator.GenerarAsyncCR(
        PasoVientoHelado_Mancha2,
        cantidad: 5,
        distCaminoOverride: 0.11f,
        distNodoOverride: 0.15f,
        rOverride: 7.85f,
        kOverride: 20);
        
      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Maleza1,
         cantidad: 650,
         distCaminoOverride: 0.1f,
         distNodoOverride: 0.8f,
         rOverride: 1.2f,
         kOverride: 30);
   
     yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ManchaCeniza1,
         cantidad: 45,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);
      
       yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra1,
         cantidad: 60,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 8.8f,
         kOverride: 20);
      
       yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra2,
         cantidad: 45,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);
         
      yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_Piedra3,
         cantidad: 6,
         distCaminoOverride: 1.60f,
         distNodoOverride: 1.80f,
         rOverride: 16.8f,
         kOverride: 20);
      
        yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_grieta1,
         cantidad: 2,
         distCaminoOverride: 1.4f,
         distNodoOverride: 1.40f,
         rOverride: 15.8f,
         kOverride: 20);

        yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_aldeatribal,
         cantidad: 7,
         distCaminoOverride: 0.85f,
         distNodoOverride: 1.30f,
         rOverride: 7.8f,
         kOverride: 20);
      
       yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_efigie,
         cantidad: 10,
         distCaminoOverride: 0.3f,
         distNodoOverride: 0.50f,
         rOverride: 7.8f,
         kOverride: 20);
      
       yield return scMapDecorator.GenerarAsyncCR(
         PasoVientoHelado_simbolopagano,
         cantidad: 6,
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
