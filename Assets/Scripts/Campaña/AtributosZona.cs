using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtributosZona : MonoBehaviour
{
   public string Nombre; //En que posición sale la zona, para determinar dificultad de encuentros
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
   public GameObject BosqueAngustiante_ArbolQuemado1;
   public GameObject BosqueAngustiante_ArbolQuemado2;
   public GameObject BosqueAngustiante_ArbolQuemado3;

   public GameObject BosqueAngustiante_ManchaCeniza1;
   public GameObject BosqueAngustiante_Maleza1;
   public GameObject BosqueAngustiante_Piedra1;
   public GameObject BosqueAngustiante_Piedra2;
   public GameObject BosqueAngustiante_Llama;

   public void ConstruirZonaBosqueAngustiante(int iFASE)
   {
      Nombre = "Bosque Angustiante";
      FASE = iFASE;
      ID = 1;
      modRecoleccionMateriales = -10;
      modRecoleccionSuministros = 5;
      modChanceEmboscada = 10;

      modChanceExploracion = 5;

      Clima_chances_Sol = 40;
      Clima_chances_Calor = 42;
      Clima_chances_Lluvia = 65;
      Clima_chances_Nieve = 75;
      Clima_chances_Niebla = 100;



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

   void AdornarBosqueArdiente()
   {

      TexturaTerreno.material = MaterialBosqueAngustiante_Terreno;
      TexturaBordeMapa.material = MaterialBosqueAngustiante_BordeMapa;
      
      RenderSettings.fog = true;
      RenderSettings.fogMode = FogMode.Exponential;
      RenderSettings.fogDensity = 0.03f;
      RenderSettings.fogColor = new Color(0.10f, 0.10f, 0.12f, 1f);

      scMapDecorator.Generar(
      BosqueAngustiante_ArbolQuemado1,
      cantidad: 750,
      distCamino: 0.1f,
      distNodo: 0.1f,
      r: 1.1f, //Separacion
      k: 20
      );

      scMapDecorator.Generar(
      BosqueAngustiante_ArbolQuemado2,
      cantidad: 135,
      distCamino: 0.12f,
      distNodo: 0.12f,
      r: 5.8f, //Separacion
      k: 20
      );

      scMapDecorator.Generar(
      BosqueAngustiante_ManchaCeniza1,
      cantidad: 45,
      distCamino: 0.10f,
      distNodo: 0.10f,
      r: 10.8f, //Separacion
      k: 20
      );


      scMapDecorator.Generar(
      BosqueAngustiante_Piedra1,
      cantidad: 35,
      distCamino: 0.6f,
      distNodo: 0.8f,
      r: 12.0f, //Separacion
      k: 20
      );

      scMapDecorator.Generar(
      BosqueAngustiante_Piedra2,
      cantidad: 8,
      distCamino: 2.6f,
      distNodo: 2.8f,
      r: 15.0f, //Separacion
      k: 20
      );

      scMapDecorator.Generar(
      BosqueAngustiante_Maleza1,
      cantidad: 120,
      distCamino: 0.2f,
      distNodo: 0.8f,
      r: 4.0f, //Separacion
      k: 20
      );
      
      scMapDecorator.Generar(
      BosqueAngustiante_Llama,
      cantidad: 22,
      distCamino: 0.6f,
      distNodo: 0.9f,
      r: 13.0f, //Separacion
      k: 20
      );

       


   }

   IEnumerator AdornarBosqueArdienteConFadeAsync()
   {
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
         cantidad: 750,
         distCaminoOverride: 0.1f,
         distNodoOverride: 0.1f,
         rOverride: 1.1f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ArbolQuemado2,
         cantidad: 135,
         distCaminoOverride: 0.12f,
         distNodoOverride: 0.12f,
         rOverride: 5.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_ManchaCeniza1,
         cantidad: 45,
         distCaminoOverride: 0.10f,
         distNodoOverride: 0.10f,
         rOverride: 10.8f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra1,
         cantidad: 35,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.8f,
         rOverride: 12.0f,
         kOverride: 20);

      yield return scMapDecorator.GenerarAsyncCR(
         BosqueAngustiante_Piedra2,
         cantidad: 8,
         distCaminoOverride: 2.6f,
         distNodoOverride: 2.8f,
         rOverride: 15.0f,
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
         cantidad: 22,
         distCaminoOverride: 0.6f,
         distNodoOverride: 0.9f,
         rOverride: 13.0f,
         kOverride: 20);

      if (admin != null)
      {
         // Liberar bloqueo y volver a mostrar la escena
         admin.SetFaderHold(false);
         yield return admin.FadeOut(0.25f);
      }
   }
}
