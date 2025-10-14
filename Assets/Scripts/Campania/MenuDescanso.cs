using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;

public class MenuDescanso : MonoBehaviour
{
   

 public TextMeshProUGUI tareaCivilDescripcion;

  // SFX de descanso: arrastrar clip desde el Inspector
  public AudioClip sfxDescanso;
  [Range(0f,1f)] public float sfxDescansoVolumen = 0.9f;
  private AudioSource sfxDescansoSource;

  // Maneja audio al descansar: iniciar/terminar, con control de emboscada
  void IniciarAudioDescansoSimple()
  {
    if (MusicManager.Instance != null && sfxDescanso != null)
    {
      MusicManager.Instance.PlaySFXYReanudar(sfxDescanso, sfxDescansoVolumen, 0.8f);
    }
  }

 [SerializeField] private GameObject btnRecoleccionSum;
 [SerializeField] private GameObject btnRecoleccionMat;
 [SerializeField] private GameObject btnFiesta;
 [SerializeField] private GameObject btnDiaLibre;
 [SerializeField] private GameObject btnAlerta;

 [SerializeField] private TextMeshProUGUI textExploracionChances;
 [SerializeField] private TextMeshProUGUI textEmboscadaChances;
 
 public int chancesAtaqueACaravana;
 private int chancesExploracion;
 private int tareaCivilSeleccionada;

 float valor = 0; 
 public void SeleccionarActividadCivil(int n)
 {
   if(CampaignManager.Instance.intTipoClima == 3) //Lluvia desactiva fiesta
   {
     btnFiesta.transform.GetChild(0).gameObject.SetActive(false);

    if(n == 3)
    {n = 1;}
   }
   

    Actualizar();
    if(n == 1) //Suministros
    {
        btnRecoleccionSum.transform.GetChild(0).gameObject.SetActive(false);
        btnRecoleccionMat.transform.GetChild(0).gameObject.SetActive(true);
        btnFiesta.transform.GetChild(0).gameObject.SetActive(true);
        btnDiaLibre.transform.GetChild(0).gameObject.SetActive(true);
        btnAlerta.transform.GetChild(0).gameObject.SetActive(true);
       
      

        tareaCivilSeleccionada = n;
        valor = (CampaignManager.Instance.GetCivilesActual()/3)/100* (100+CampaignManager.Instance.scAtributosZona.modRecoleccionSuministros);

       if(CampaignManager.Instance.intTipoClima == 3) //Lluvia
       {
        valor = valor*0.85f; // -15% recoleccion suministors si llueve
       }
       if(CampaignManager.Instance.intTipoClima == 4) //Nieve
       {
        valor = valor*0.85f; // -15% recoleccion suministors si neva
       }
       if(CampaignManager.Instance.intTipoClima == 5) //Niebla
       {
        valor = valor*0.80f; // -20% recoleccion suministors si hay niebla
       }
       if(CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 5) //Bonus recoleccion nodo recursos
       {
        valor = valor*1.2f; // +20% recoleccion 
       }


        tareaCivilDescripcion.text ="<b><u>Recolección de Suministros</b></u>\n\n\n";
        tareaCivilDescripcion.text +="Los civiles se dedicarán a recolectar distintos suministros de las inmediaciones al campamento.\n\n";
        tareaCivilDescripcion.text +=$"<color=#d8a205>Se juntarán entre {(int)valor} y {(int)valor+20} suministros. </color>\n\n\n";

        chancesAtaqueACaravana = 25 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
        chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }
    else if(n == 2) //Materiales
    {
        btnRecoleccionSum.transform.GetChild(0).gameObject.SetActive(true);
        btnRecoleccionMat.transform.GetChild(0).gameObject.SetActive(false);
        btnFiesta.transform.GetChild(0).gameObject.SetActive(true);
        btnDiaLibre.transform.GetChild(0).gameObject.SetActive(true);
        btnAlerta.transform.GetChild(0).gameObject.SetActive(true);

        tareaCivilSeleccionada = n;
        valor = (CampaignManager.Instance.GetCivilesActual()/5)/100* (100+CampaignManager.Instance.scAtributosZona.modRecoleccionMateriales);
        if(CampaignManager.Instance.intTipoClima == 5) //Niebla
        {
          valor = valor*0.80f; // -20% recoleccion materiales si hay niebla
        }
        if(CampaignManager.Instance.intTipoClima == 4) //Nieve
        {
          valor = valor*0.85f; // -15% recoleccion materiales si hay Nieve
        }
        if(CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 5) //Bonus recoleccion nodo recursos
        {
          valor = valor*1.2f; // +20% recoleccion 
        }


        tareaCivilDescripcion.text ="<b><u>Recolección de Materiales</b></u>\n\n\n";
        tareaCivilDescripcion.text +="Los civiles se dedicarán a recolectar materiales básicos en la zona.\n\n";
        tareaCivilDescripcion.text +=$"<color=#d8a205>Se juntarán entre {(int)valor} y {(int)valor+10} materiales. </color>\n\n\n";

        chancesAtaqueACaravana = 25 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
        chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;




    }
    else if(n == 3) //Fiesta
    {
        btnRecoleccionSum.transform.GetChild(0).gameObject.SetActive(true);
        btnRecoleccionMat.transform.GetChild(0).gameObject.SetActive(true);
        btnFiesta.transform.GetChild(0).gameObject.SetActive(false);
        btnDiaLibre.transform.GetChild(0).gameObject.SetActive(true);
        btnAlerta.transform.GetChild(0).gameObject.SetActive(true);

        tareaCivilSeleccionada = n;

        tareaCivilDescripcion.text ="<b><u>Feria</b></u>\n\n\n";
        tareaCivilDescripcion.text +="Los civiles dedicarán el día a organizar una feria con varios juegos y celebraciones.\n\n";
        tareaCivilDescripcion.text +=$"<color=#d8a205>Se conseguirá entre 15 y 25 de Esperanza y se consumirán 20% más de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n";

        chancesAtaqueACaravana = 30 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
        chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }
    else if(n == 4) //Dia Libre
    {
        btnRecoleccionSum.transform.GetChild(0).gameObject.SetActive(true);
        btnRecoleccionMat.transform.GetChild(0).gameObject.SetActive(true);
        btnFiesta.transform.GetChild(0).gameObject.SetActive(true);
        btnDiaLibre.transform.GetChild(0).gameObject.SetActive(false);
        btnAlerta.transform.GetChild(0).gameObject.SetActive(true);

        tareaCivilSeleccionada = n;

        tareaCivilDescripcion.text ="<b><u>Día Libre</b></u>\n\n\n";
        tareaCivilDescripcion.text +="Los civiles se tomarán el día para descansar y recobrar fuerzas.\n\n";
        tareaCivilDescripcion.text +=$"<color=#d8a205>Se conseguirá 10 de Esperanza y el día siguiente arrancará con -1 Fatiga.</color>\n\n\n";

        chancesAtaqueACaravana = 20 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
        chancesExploracion = 50 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;



    }
    else if(n == 5) //Alerta
    {
        btnRecoleccionSum.transform.GetChild(0).gameObject.SetActive(true);
        btnRecoleccionMat.transform.GetChild(0).gameObject.SetActive(true);
        btnFiesta.transform.GetChild(0).gameObject.SetActive(true);
        btnDiaLibre.transform.GetChild(0).gameObject.SetActive(true);
        btnAlerta.transform.GetChild(0).gameObject.SetActive(false);

        tareaCivilSeleccionada = n;

        tareaCivilDescripcion.text ="<b><u>Estado de Alerta</b></u>\n\n\n";
        tareaCivilDescripcion.text +="Durante el descanso, se asignarán a los civiles mas aptos físicamente a la vigilancia del area circundante al campamento.\n\n";
        tareaCivilDescripcion.text +=$"<color=#d8a205>Previene cualquier Emboscada en este descanso. +20% a Exploración. -10 Esperanza.</color>\n\n\n";

        chancesAtaqueACaravana = 0;
        chancesExploracion =  80 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }
    
  Actualizar();

 }

 private void Actualizar()
 {
    if(CampaignManager.Instance.intTipoClima == 3 || CampaignManager.Instance.intTipoClima == 4 || CampaignManager.Instance.intTipoClima == 5) //Lluvia, Nieve o Niebla
    {
        chancesAtaqueACaravana -= 20;
    }
    if(CampaignManager.Instance.intTipoClima == 5) //Niebla
    {
        chancesExploracion -= 20;
    }
    //Aliento negro aumenta chances de ataque a caravana
    chancesAtaqueACaravana += (int)(CampaignManager.Instance.GetValorAlientoNegro()/2);

    //Actividades que toquen emboscada y exploracion van aca 
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.ActividadSeleccionada == 7) //Explorador: Caza Nocturna
      {
        chancesAtaqueACaravana += 3;

      }
      if (pers.ActividadSeleccionada == 14) //Acechador: Vigilar Desde Las Sombras
      {
        chancesAtaqueACaravana -= 5;

      }
   
    }

    if (CampaignManager.Instance.scMenuSequito.TieneSequito(5)) { chancesAtaqueACaravana += 2; } //Herboristas, aumentan chances 2%
    
    chancesAtaqueACaravana -= CampaignManager.Instance.mejoraCaravanaAntorchas*5;
    chancesExploracion += CampaignManager.Instance.mejoraCaravanaCatalejos*5;

    chancesExploracion += CampaignManager.Instance.ExploracionSumadaPorActividades();

    chancesAtaqueACaravana = Mathf.Clamp(chancesAtaqueACaravana, 0, 100);
    chancesExploracion = Mathf.Clamp(chancesExploracion, 0, 100);

    textEmboscadaChances.text = "Las probabilidades de sufrir un ataque a la Caravana "+chancesAtaqueACaravana+"%";
    textExploracionChances.text = "Las probabilidades de exploración: "+chancesExploracion+"%";

 }



  public async void Descansar()
  {
    // Audio: al presionar Descansar, cortar música con fade, reproducir SFX y reanudar.
    IniciarAudioDescansoSimple();
    CampaignManager.Instance.numeroTurno++;

    CampaignManager.Instance.scSequitoMercaderes.GenerarItemsVendidos();

    gameObject.SetActive(false);

    CampaignManager.Instance.scAdministradorEscenas.PlayFadeInOut(1.2f, 4.0f);
    await Task.Delay(TimeSpan.FromSeconds(6.0f));

    if (tareaCivilSeleccionada == 1)
    {
      int random = UnityEngine.Random.Range(0, 21);
      float total = valor + random;

      CampaignManager.Instance.CambiarSuministrosActuales((int)total);

    } //Recoleccion Suministros

    if (tareaCivilSeleccionada == 2)
    {
      int random = UnityEngine.Random.Range(0, 11);
      CampaignManager.Instance.CambiarMaterialesActuales((int)valor + random);

    } //Recoleccion Materiales

    float consumo = CampaignManager.Instance.GetCivilesActual()+ CampaignManager.Instance.GetBueyesActual()*2;
    if (tareaCivilSeleccionada == 3)
    {
      consumo = consumo * 1.2f;
      int random = UnityEngine.Random.Range(0, 11);
      CampaignManager.Instance.CambiarEsperanzaActual(15 + random);

    } //Fiesta

    consumo = consumo / 100 * (100 - CampaignManager.Instance.mejoraCaravanaAlmacen * 5);

    //Modificadores de Climas
    if (CampaignManager.Instance.intTipoClima == 2) //Calor
    {
      if (tareaCivilSeleccionada == 4) // y se descansa, da +5 esperanza
      {
        CampaignManager.Instance.CambiarEsperanzaActual(5);
        CampaignManager.Instance.EscribirLog($"-El tener un Día Libre en plena Ola de Calor, ha caído bien en los Civiles. +5 Esperanza");
      }
      else
      {
        CampaignManager.Instance.CambiarEsperanzaActual(-3); //si se hace otra cosa, -3
        CampaignManager.Instance.EscribirLog($"-El tener que trabajar en plena Ola de Calor, ha caído mal en los Civiles. -3 Esperanza");
      }

    }


    //Actividades Personajes Al Descansar
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      //Saca Fatiga de campaña
      pers.Camp_Fatigado = false;
      //Saca Bendicion Plegaria
      pers.Camp_Bendecido_SequitoClerigos = false;

      if (pers.ActividadSeleccionada == 4) //Caballero: Relatos de Batalla
      {
        CampaignManager.Instance.CambiarEsperanzaActual(4);
        CampaignManager.Instance.EscribirLog($"-{pers.sNombre} comparte sus historias de batalla con los civiles. +4 Esperanza");
      }
      if (pers.Camp_Enfermo > 0) //Disminuye Enfermedad
      {
        pers.Camp_Enfermo -= 1; //Se cura un día

        //Sequito Curanderos ayuda a disminuir enfermedad 1 extra
        int rand = UnityEngine.Random.Range(1, 100);
        float tierCuranderos =((CampaignManager.Instance.sequitoCuranderosMejoraCuracion*100)-10)/5;
        if (pers.Camp_Enfermo > 0 && rand <= 20 + (int)tierCuranderos * 10)
        {
          pers.Camp_Enfermo -= 1;
          CampaignManager.Instance.EscribirLog($"-El Séquito de Curanderos ha reducido la enfermedad de {pers.sNombre} en 1 extra.");

        }
      }
      if (pers.Camp_Moral > 0) //Moral tiende a cero
      {
        pers.Camp_Moral -= 1; 
      }
      if (pers.Camp_Moral < 0) //Moral tiende a cero
      {
        pers.Camp_Moral += 1; 
      }



      //Curacion General por descansar


      int cantPurificadorasColaborando = CampaignManager.Instance.CuantosPersonajesHacenTalActividad(12); //Colaborar con los Curanderos

      float porcentajeVidaMax = pers.fVidaMaxima * (CampaignManager.Instance.sequitoCuranderosMejoraCuracion + (cantPurificadorasColaborando * 0.05f)); //5% por cada Purificadora colaborando



      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 4) //Bonus descansar en claro
      { porcentajeVidaMax = porcentajeVidaMax * 1.1f; }
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 5) //Bonus descansar en Asentamiento
      { porcentajeVidaMax = porcentajeVidaMax * 1.2f; }

      if (pers.fVidaMaxima > pers.fVidaActual)
      {
        CampaignManager.Instance.EscribirLog($"-{pers.sNombre} se cura {(int)porcentajeVidaMax} PV tras el Descanso.");
      }
      pers.RecibirCuracion(porcentajeVidaMax);


    }

    //Efectos de Sequitos al descansar
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(4) && tareaCivilSeleccionada == 3) //Artistas
    {
      CampaignManager.Instance.CambiarEsperanzaActual(10);
      CampaignManager.Instance.EscribirLog($"-En la Feria, los Artistas han realizado un espectáculo que ha levantado el ánimo de los Civiles. +10 Esperanza");
    }
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(5)) //Herboristas
    {
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoFort = 2;
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoReflej = 2;
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoMental = 2;
      CampaignManager.Instance.EscribirLog($"-Los Herboristas han preparado sus Bálsamos.");
    }
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(11)) //Esclavos
    {
      int random = UnityEngine.Random.Range(10, 16);

      CampaignManager.Instance.EscribirLog($"-Los Esclavos han recolectado {random} Materiales.");
      CampaignManager.Instance.CambiarMaterialesActuales(random);
    }








    int fatiga = CampaignManager.Instance.GetFatigaActual();
    if (tareaCivilSeleccionada == 4)
    { fatiga++; CampaignManager.Instance.CambiarEsperanzaActual(10); } //Día libre

    if (tareaCivilSeleccionada == 5)
    { CampaignManager.Instance.CambiarEsperanzaActual(-10); } //Alerta



    if (CampaignManager.Instance.scMenuSequito.TieneSequito(8)) { consumo -= 18; } //Refugiados, consumen menos suministros(son 35 civiles, pero consumen la mitad)

    CampaignManager.Instance.CambiarFatigaActual(-fatiga);
    int cantSum = CampaignManager.Instance.GetSuministrosActuales();
    if (cantSum < consumo)
    {
      CampaignManager.Instance.CambiarSuministrosActuales(-(int)cantSum);

      float faltaSum = consumo - cantSum;
      CampaignManager.Instance.CambiarEsperanzaActual(-(int)faltaSum);

      float mueren = faltaSum / 20;
      CampaignManager.Instance.CambiarCivilesActuales(-(int)mueren);

      CampaignManager.Instance.EscribirLog($"-La falta de Suministros ha provocado la muerte de {mueren} Civiles.");


    }
    else
    {
      CampaignManager.Instance.CambiarSuministrosActuales(-(int)consumo);
    }

    gameObject.SetActive(false);

    CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(chancesExploracion, true);

    //Efectos Esperanza en Descanso - Se van Civiles
    if (CampaignManager.Instance.GetEsperanzaActual() < 20 && CampaignManager.Instance.GetEsperanzaActual() > 10)
    {
      int random = UnityEngine.Random.Range(1, 5);
      CampaignManager.Instance.CambiarCivilesActuales(-random);
      CampaignManager.Instance.EscribirLog($"-Por la baja Esperanza {random} Civiles han abandonado la Caravana.");

    }
    else if (CampaignManager.Instance.GetEsperanzaActual() <= 10)
    {
      int random = UnityEngine.Random.Range(1, 11);
      CampaignManager.Instance.CambiarCivilesActuales(-random);
      CampaignManager.Instance.EscribirLog($"-Por la muy baja Esperanza {random} Civiles han abandonado la Caravana.");
    }

    //Efectos Esperanza en Descanso 
    if (CampaignManager.Instance.GetEsperanzaActual() > 79 && CampaignManager.Instance.GetEsperanzaActual() < 90)
    {
      float random = UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 3;
      CampaignManager.Instance.CambiarOroActual((int)random);

      CampaignManager.Instance.EscribirLog($"-Debido al optimismo que rodea la Caravana, los Civiles han donado {random} Oro.");
    }
    else if (CampaignManager.Instance.GetEsperanzaActual() >= 90)
    {
      float random = UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 2;
      CampaignManager.Instance.CambiarOroActual((int)random);
      CampaignManager.Instance.EscribirLog($"-Debido al gran optimismo que rodea la Caravana, los Civiles han donado {random} Oro.");

    }

    TiradaClima();
     CampaignManager.Instance.sunController.ResetSun();

    #region Acechadores Sueldo
    //Sueldo Acechadores
    int sueldoAcechadores = 0;
    if (CampaignManager.Instance.GetEsperanzaActual() < 70) //Si la esperanza es menor a 70, los Acechadores cobran su sueldo.
    {
      sueldoAcechadores = CampaignManager.Instance.CuantosPersonajesSonDeTalClase(4) * 20; //Acechadores
      CampaignManager.Instance.CambiarOroActual(-sueldoAcechadores);
      CampaignManager.Instance.EscribirLog($"-Los Acechadores en la Caravana se han cobrado su sueldo por {sueldoAcechadores} de Oro.");
    }
    else //Si la esperanza es mayor o igual a 70, no cobran.
    {
      int cantidadacechadores = CampaignManager.Instance.CuantosPersonajesSonDeTalClase(4); //Acechadores
      CampaignManager.Instance.EscribirLog($"-Debido a la alta Esperanza, los Acechadores han decidido no cobrar su sueldo esta vez.");

    }
    #endregion

    #region   Avance Aliento Negro al Descansar
    bool sePrevieneAvanceAliento = false;
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      int random = UnityEngine.Random.Range(0, 100);
      if (pers.ActividadSeleccionada == 10 && random < 25) //Purificadora: Ritual de Limpieza 
      {
        sePrevieneAvanceAliento = true;
        CampaignManager.Instance.EscribirLog($"-{pers.sNombre} ha realizado con éxito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro.");
        break;
      }
    }

    if (!sePrevieneAvanceAliento)
    {
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 4) //Bonus descansar en claro
      {
        CampaignManager.Instance.CambiarValorAlientoNegro(1);
        CampaignManager.Instance.EscribirLog("-Durante el descanso en el Claro, el Aliento Negro ha avanzado 1.");
      }
      else
      {
        CampaignManager.Instance.CambiarValorAlientoNegro(2);
        CampaignManager.Instance.EscribirLog("-Durante el descanso, el Aliento Negro ha avanzado 2.");

      }
    }
    #endregion

    float randomEvento = UnityEngine.Random.Range(0, 100);
    float factorEventoBuenoMalo = 36 + CampaignManager.Instance.GetEsperanzaActual() / 3;

    CampaignManager.Instance.CambiarEsperanzaActual(CampaignManager.Instance.mejoraCaravanaTiendas * 5);




    //Probabilidad emboscada
    // (Audio) Se maneja al principio de Descansar()

    int randomEmboscada = UnityEngine.Random.Range(1, 101);
    if (randomEmboscada <= chancesAtaqueACaravana) 
    {
      CampaignManager.Instance.EscribirLog($"-La caravana han sufrido un Ataque durante el descanso. Probabilidades {chancesAtaqueACaravana}% - Tirada: 1d100 = {randomEmboscada}");

      CampaignManager.Instance.scMenuBatallas.EventoBatallaCaravana(0, 3);
      // (Audio) Ignorado: la música de batalla se maneja en AdministradorEscenas
    }
    else //no puede haber evento y emboscada
    { 
       if(randomEvento < factorEventoBuenoMalo)
      {
         if(CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 4) //En Claros no hay eventos negativos.
         {CampaignManager.Instance.EmpezarEventoBueno();}
         else{ CampaignManager.Instance.EmpezarEventoMalo();} //evento negativo de nodo normal

        

      }else{ CampaignManager.Instance.EmpezarEventoBueno();}

      // (Audio) Ya se reanudó desde MusicManager tras terminar el SFX

    }


 }

 
public void TiradaClima()
{
    int random = UnityEngine.Random.Range(1,101);

    if(random < CampaignManager.Instance.scAtributosZona.Clima_chances_Sol)
    {
        CampaignManager.Instance.intTipoClima = 1;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_sol;

        CampaignManager.Instance.EscribirLog("-Es un día hermoso. +5 Esperanza.");
        CampaignManager.Instance.CambiarEsperanzaActual(5);

    }
    else if(random < CampaignManager.Instance.scAtributosZona.Clima_chances_Calor)
    {
        CampaignManager.Instance.intTipoClima = 2;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_calor;

        CampaignManager.Instance.EscribirLog("-La Ola de Calor se hace insoportable. +1 Fatiga.");
        CampaignManager.Instance.CambiarFatigaActual(+1);
    }
    else if(random < CampaignManager.Instance.scAtributosZona.Clima_chances_Lluvia)
    {
        CampaignManager.Instance.intTipoClima = 3;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_lluvia;

        CampaignManager.Instance.EscribirLog("-La Lluvia hace el viaje más difícil. -5 Esperanza.");
        CampaignManager.Instance.CambiarEsperanzaActual(-5);
    }
    else if(random < CampaignManager.Instance.scAtributosZona.Clima_chances_Nieve)
    {
        CampaignManager.Instance.intTipoClima = 4;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_nieve;
        CampaignManager.Instance.EscribirLog("-La Nieve mejora el ánimo. +3 Esperanza.");
        CampaignManager.Instance.CambiarEsperanzaActual(3);
    }
    else if(random < CampaignManager.Instance.scAtributosZona.Clima_chances_Niebla)
    {
        CampaignManager.Instance.intTipoClima = 5;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_niebla;
    }
    



}

}
