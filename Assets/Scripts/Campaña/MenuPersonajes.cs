using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using Unity.VisualScripting;

public class MenuPersonajes : MonoBehaviour
{

  public List<Personaje> listaPersonajes = new List<Personaje>(); //La lista que posee los personajes activos


  public GameObject prefabBtnPersonaje;
  public GameObject contenedorUIPersonajes;
  public Equipo scEquipo;

  public Actividades scActividades;


  public Sprite Male001;
  public Sprite Male002;
  public Sprite Male003; //Explorador
  public Sprite Male004; //Acechador
  public Sprite Male005; //Canalizador

  public Sprite Female001; //Purificadora

  public Personaje pSel;

  //Base Stats personaje
  [SerializeField] TextMeshProUGUI txtNombre;
  [SerializeField] TextMeshProUGUI txtClase;
  [SerializeField] TextMeshProUGUI txtExperiencia;
  [SerializeField] TextMeshProUGUI txtNivel;
  [SerializeField] TextMeshProUGUI txtHP;
  [SerializeField] TextMeshProUGUI txtFuerza;
  [SerializeField] TextMeshProUGUI txtAgi;
  [SerializeField] TextMeshProUGUI txtPoder;
  [SerializeField] TextMeshProUGUI txtIniciativa;
  [SerializeField] TextMeshProUGUI txtApMax;
  [SerializeField] TextMeshProUGUI txtValMax;
  [SerializeField] TextMeshProUGUI txtArmadura;
  [SerializeField] TextMeshProUGUI txtDefensa;
  [SerializeField] TextMeshProUGUI txtTSReflejo;
  [SerializeField] TextMeshProUGUI txtTSFortaleza;
  [SerializeField] TextMeshProUGUI txtTSMental;
  [SerializeField] TextMeshProUGUI txtResFuego;
  [SerializeField] TextMeshProUGUI txtResRayo;
  [SerializeField] TextMeshProUGUI txtResHielo;
  [SerializeField] TextMeshProUGUI txtResArcano;
  [SerializeField] TextMeshProUGUI txtResAcido;
  [SerializeField] TextMeshProUGUI txtResNecro;
  [SerializeField] TextMeshProUGUI txtResDivino;

  [SerializeField] TextMeshProUGUI txtContenedorRasgos;

  [SerializeField] Image imCorazon;
  [SerializeField] Image imMedalla;

  public void ActualizarLista()
  {

    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      Destroy(transform.gameObject);
    }

    foreach (Personaje pers in listaPersonajes)
    {
      if (!pers.Camp_Muerto)
      {
        GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
        btnPers.GetComponent<Image>().sprite = pers.spRetrato;
        btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
      }

    }

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      // Intenta obtener el componente btnPersonaje del hijo
      btnPersonaje btn = child.GetComponent<btnPersonaje>();

      if (btn != null) // Asegúrate de que el componente btnPersonaje exista
      {
        btn.representarVida();
        if (btn.personajeRepresentado.Camp_Herido)
        {
          child.GetChild(3).gameObject.SetActive(true);
        }
        else { child.GetChild(3).gameObject.SetActive(false); }

        if (btn.personajeRepresentado.Camp_Corrupto)
        {
          child.GetChild(5).gameObject.SetActive(true);
        }
        else { child.GetChild(5).gameObject.SetActive(false); }




        if (pSel == btn.personajeRepresentado)
        {
          btn.transform.GetChild(0).gameObject.SetActive(true);

        }
        else
        {
          btn.transform.GetChild(0).gameObject.SetActive(false);
        }
      }
    }


  }


  public void SeleccionarPersonaje(Personaje pers, GameObject btnPers)
  {

    pSel = pers;
    ActualizarLista();
    if (btnPers != null)
    { btnPers.transform.GetChild(0).gameObject.SetActive(true); }

    Invoke("ActualizarInfo", 0.15f);

  }



  public void ActualizarInfo()
  {
    SelPos(pSel.iPuestoDeseado);
    //Clase
    switch (pSel.IDClase)
    {
      case 1: txtClase.text = "Caballero"; break;
      case 2: txtClase.text = "Explorador"; break;
      case 3: txtClase.text = "Purificadora"; break;
      case 4: txtClase.text = "Acechador"; break;
      case 5: txtClase.text = "Canalizador"; break;
        //----


    }
    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      transform.gameObject.GetComponent<btnPersonaje>().representarVida();
      transform.gameObject.GetComponent<btnPersonaje>().RepresentarIconos();
    }

    RepresentarRasgos();
    scEquipo.ActualizarEquipo(pSel);
    scActividades.ActualizarActividades();
    ActualizarListaHabilidades();

      

    //Info
    txtNombre.text = pSel.sNombre;
    txtExperiencia.text = $"" + pSel.fExperienciaActual + "/" + (100 + (pSel.fNivelActual * 25));
    txtNivel.text = "" + pSel.fNivelActual;
    imCorazon.fillAmount = Mathf.Clamp01((float)pSel.fVidaActual / (pSel.fVidaMaxima + scEquipo.BuffTOTALEQUIPOhpMax));
    txtHP.text = "" + (int)pSel.fVidaActual + "/" + (pSel.fVidaMaxima + scEquipo.BuffTOTALEQUIPOhpMax);
    imMedalla.fillAmount = Mathf.Clamp01((float)pSel.fExperienciaActual / (100 + (pSel.fNivelActual*25)));
    txtFuerza.text = "Fuerza: " + (pSel.iFuerza + scEquipo.BuffTOTALEQUIPOFuerza);
    txtAgi.text = "Agilidad: " + (pSel.iAgi + scEquipo.BuffTOTALEQUIPOAgi);
    txtPoder.text = "Poder: " + (pSel.iPoder + scEquipo.BuffTOTALEQUIPOPoder);
    txtIniciativa.text = "Iniciativa: " + (pSel.iIniciativa + scEquipo.BuffTOTALEQUIPOIniciativa);
    txtApMax.text = "AP: " + (pSel.iApMax + scEquipo.BuffTOTALEQUIPOApMax);
    txtValMax.text = "Valentía: " + (pSel.iValMax + scEquipo.BuffTOTALEQUIPOValMax);
    txtArmadura.text = "Armadura: " + (pSel.iArmadura + scEquipo.BuffTOTALEQUIPOArmadura);
    txtDefensa.text = "Defensa: " + (pSel.iDefensa + scEquipo.BuffTOTALEQUIPODefensa);
    txtTSReflejo.text = "-Reflejos: " + (pSel.iTSReflejo + scEquipo.BuffTOTALEQUIPOTSReflejo);
    txtTSFortaleza.text = "-Fortaleza: " + (pSel.iTSFortaleza + scEquipo.BuffTOTALEQUIPOTSFortaleza);
    txtTSMental.text = "-Mental: " + (pSel.iTSMental + scEquipo.BuffTOTALEQUIPOTSMental);
    txtResFuego.text = "" + (pSel.iResFuego + scEquipo.BuffTOTALEQUIPOResFuego);
    txtResRayo.text = "" + (pSel.iResRayo + scEquipo.BuffTOTALEQUIPOResRayo);
    txtResHielo.text = "" + (pSel.iResHielo + scEquipo.BuffTOTALEQUIPOResHielo);
    txtResArcano.text = "" + (pSel.iResArcano + scEquipo.BuffTOTALEQUIPOResArcano);
    txtResAcido.text = "" + (pSel.iResAcido + scEquipo.BuffTOTALEQUIPOResAcido);
    txtResNecro.text = "" + (pSel.iResNecro + scEquipo.BuffTOTALEQUIPOResNecro);
    txtResDivino.text = "" + (pSel.iResDivino + scEquipo.BuffTOTALEQUIPOResDivino);

    Invoke("ActualizarInfoNivel", 0.05f);
  }


  void RepresentarRasgos()
  {
    txtContenedorRasgos.text = "";
    //Rasgos
    for (int i = 0; i < 300; i++)
    {
      if (pSel.aRasgos[i] == 1)
      {
        txtContenedorRasgos.text += DevolverRasgo(i);
        txtContenedorRasgos.text += "\n";

      }


    }


    //Estados Campaña
    if (pSel.Camp_Fatigado) { txtContenedorRasgos.text += "<color=#2a9c71>\n\nFatigado: -1 AP máximo. </color>"; }
    if (pSel.Camp_Bendecido_SequitoClerigos) { txtContenedorRasgos.text += "<color=#2a9c71>\n\nBendecido por Plegaria: +1 Ataque +1 Defensa +5 Res.Necro +2 TSMental.</color>"; }

    if (pSel.Camp_Herido) { txtContenedorRasgos.text += "<color=#d80404>\n\nHerido:-1 Atributos. Si cae en combate, muere. </color>"; }
    if (pSel.Camp_Corrupto) { txtContenedorRasgos.text += "<color=#d80404>\n\nCorrupto: Los enemigos corrompidos se curan al atacarlo, le infligen mas daño, y si lo derriban en combate, muere. </color>"; }
    if (pSel.Camp_Enfermo > 0) { txtContenedorRasgos.text += $"<color=#d80404>\n\nEnfermo por {pSel.Camp_Enfermo} días. -15% daño, -3 TS Fortaleza, -1 AP </color>"; }
    if (pSel.Camp_Moral < 0) { txtContenedorRasgos.text += $"<color=#d80404>\n\nBaja Moral por {-pSel.Camp_Moral} días. -1 Ataque y Defensa, -3 TS Mental, -2 Valentía Inicial</color>"; }
    if (pSel.Camp_Moral > 0) { txtContenedorRasgos.text += $"<color=#d80404>\n\nAlta Moral por {pSel.Camp_Moral} días. +1 Ataque, +2 TS Mental, +2 Valentía Inicial</color>"; }
  }

  string DevolverRasgo(int id)
  {
    string rasgoDesc = "";

    if (id == 1) { rasgoDesc = "Torpe: +1 Rango Pifias"; }
    if (id == 2) { rasgoDesc = "Valiente: +2 Valentía Máxima."; }
    if (id == 3) { rasgoDesc = "Alegre: +2 Esperanza al Descansar."; }
    //.....


    return rasgoDesc;
  }

  public Transform listaHab;
  public GameObject actionButtonPrefab;
  public void ActualizarListaHabilidades()
  {
    foreach (Transform buttonTransform in listaHab)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      Destroy(buttonTransform.gameObject);
    }

    foreach (Habilidad habilidad in pSel.gameObject.GetComponents<Habilidad>())
    {
      //Habilidades que no se muestran
      if (habilidad is RetrasarTurno)
      {
        continue;
      }
      if (habilidad is AtaqueBasico)
      {
        continue;
      }

      habilidad.ActualizarDescripcion();

      GameObject actionButtonTransform = Instantiate(actionButtonPrefab, listaHab);
      BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
      habilidadBotonUI.HabilidadRepresentada = habilidad;

      if (pSel.NivelPuntoHabilidad > 0 && habilidadBotonUI.HabilidadRepresentada.NIVEL < 4 && habilidadBotonUI.HabilidadRepresentada.NIVEL > 0)
      {
        actionButtonTransform.transform.GetChild(3).gameObject.SetActive(true);
        if (habilidadBotonUI.HabilidadRepresentada.NIVEL == 3)
        {
          actionButtonTransform.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
          actionButtonTransform.transform.GetChild(3).GetChild(1).gameObject.SetActive(true);
        }
      }
      else
      {
        actionButtonTransform.transform.GetChild(3).gameObject.SetActive(false);
      }
      actionButtonTransform.GetComponent<BotonHabilidad>().scMenuPersonajes = this;





    }


  }

  public TextMeshProUGUI itemDesc;

  public void OnClickCofre()
  {
    if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(5);
    }
    else { scEquipo.goInventario.SetActive(false); }


  }
  public void OnClickArma()
  {  TooltipItems.Instance.HideTooltip();
    if (pSel.itemArma != null) //quita arma actual
    {
      GameObject armaaQuitar = pSel.itemArma.gameObject;
      pSel.QuitarArma(pSel.itemArma);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);
    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(1);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }
  public void OnClickArmadura()
  {  TooltipItems.Instance.HideTooltip();
    if (pSel.itemArmadura != null) //quita armadura actual
    {

      GameObject armaaQuitar = pSel.itemArmadura.gameObject;
      pSel.QuitarArmadura(pSel.itemArmadura);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);
    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(2);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }


  public void OnHoverArma()
  {
    if (pSel.itemArma != null)
    {
      //itemDesc.text = pSel.itemArma.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = pSel.itemArma.sNombreItem + "\n\n" +  pSel.itemArma.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void OnHoverArmadura()
  {
    if (pSel != null)
    {
      if (pSel.itemArmadura != null)
      {
        // itemDesc.text = pSel.itemArmadura.itemDescrpicion;
         Vector3 pos = Input.mousePosition;
     string total = pSel.itemArmadura.sNombreItem + "\n\n" +  pSel.itemArmadura.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

      }
    }
  }


  public void OnClickAccesorio1()
  {  TooltipItems.Instance.HideTooltip();
    scEquipo.accesorioACambiar = 1;
    if (pSel.Accesorio1 != null) //quita acc1 actual
    {

      GameObject armaaQuitar = pSel.Accesorio1.gameObject;
      pSel.QuitarAccesorio1(pSel.Accesorio1);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);

    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(3);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }

  public void OnHoverAccesorio1()
  {
    if (pSel.Accesorio1 != null)
    {
      //itemDesc.text = pSel.Accesorio1.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = pSel.Accesorio1.sNombreItem + "\n\n" +  pSel.Accesorio1.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void OnClickAccesorio2()
  { TooltipItems.Instance.HideTooltip();
    scEquipo.accesorioACambiar = 2;
    if (pSel.Accesorio2 != null) //quita acc1 actual
    {

      GameObject armaaQuitar = pSel.Accesorio2.gameObject;
      pSel.QuitarAccesorio2(pSel.Accesorio2);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);

    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(3);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }

  public void OnHoverAccesorio2()
  {
    if (pSel.Accesorio2 != null)
    {
      //itemDesc.text = pSel.Accesorio2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = pSel.Accesorio2.sNombreItem + "\n\n" +  pSel.Accesorio2.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }


  public void OnClickConsumible1()
  { TooltipItems.Instance.HideTooltip();
    scEquipo.consumibleACambiar = 1;
    if (pSel.Consumible1 != null) //quita c1 actual
    {

      GameObject armaaQuitar = pSel.Consumible1.gameObject;
      pSel.QuitarConsumible1(pSel.Consumible1);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);

    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(4);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }

  public void OnHoverConsumible1()
  {
    if (pSel.Consumible1 != null)
    {
      //itemDesc.text = pSel.Consumible1.itemDescrpicion;
        Vector3 pos = Input.mousePosition;
     string total = pSel.Consumible1.sNombreItem + "\n\n" +  pSel.Consumible1.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void OnClickConsumible2()
  { TooltipItems.Instance.HideTooltip();
    scEquipo.consumibleACambiar = 2;
    if (pSel.Consumible2 != null) //quita c1 actual
    {

      GameObject armaaQuitar = pSel.Consumible2.gameObject;
      pSel.QuitarConsumible2(pSel.Consumible2);
      scEquipo.listInventario.Add(armaaQuitar);

      Invoke("ActualizarInfo", 0.05f);

    }
    else if (!scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(4);
    }
    else { scEquipo.goInventario.SetActive(false); }
  }

  public void OnHoverConsumible2()
  {
    if (pSel.Consumible2 != null)
    {
      //itemDesc.text = pSel.Consumible2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
     string total = pSel.Consumible2.sNombreItem + "\n\n" +  pSel.Consumible2.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  [SerializeField] GameObject SubirNivelAtributo;
  [SerializeField] GameObject SubirNivelTS;
  [SerializeField] GameObject SubirNivelHabilidad;
  [SerializeField] GameObject HabilidadBaseNueva;
  [SerializeField] Transform ListaElegirHabilidad;

  public List<Habilidad> poolSortear;

  public bool yaTiroHabRand = false;

  public void LimpiarComponentesHab()
  {
    // Remover todos los componentes de tipo Habilidad de ListaElegirHabilidad
    foreach (var habilidad in ListaElegirHabilidad.GetComponents<Habilidad>())
    {
      Destroy(habilidad);
    }
    poolSortear.Clear();
  }
  public void notHoverItem()
  { 
     TooltipItems.Instance.HideTooltip();

  }
  void ActualizarInfoNivel()
  {
    if (pSel.NivelPuntoAtributo > 0)
    {
      SubirNivelAtributo.SetActive(true);

    }
    else { SubirNivelAtributo.SetActive(false); }

    if (pSel.NivelPuntoTS > 0)
    {
      SubirNivelTS.SetActive(true);

    }
    else { SubirNivelTS.SetActive(false); }

    if (pSel.NivelPuntoHabilidad > 0)
    {
      SubirNivelHabilidad.SetActive(true);

    }
    else { SubirNivelHabilidad.SetActive(false); }

    //Habilidad Base Nueva
    if (pSel.NivelNuevaHabilidadBase > 0)
    {
      if (!yaTiroHabRand)
      {
        yaTiroHabRand = true;
        HabilidadBaseNueva.SetActive(true);
        foreach (Transform buttonTransform in ListaElegirHabilidad)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
          Destroy(buttonTransform.gameObject);
        }
        LimpiarComponentesHab();


        if (pSel.IDClase == 1) //Caballero
        {

          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAcorazado>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcorazado>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<GritoMotivador>() == null)
          {
            ListaElegirHabilidad.AddComponent<GritoMotivador>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<CorteHorizontal>() == null)
          {
            ListaElegirHabilidad.AddComponent<CorteHorizontal>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<PrimerosAuxilios>() == null)
          {
            ListaElegirHabilidad.AddComponent<PrimerosAuxilios>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<REPRESENTACIONDeterminacion>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONDeterminacion>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Partir>() == null)
          {
            ListaElegirHabilidad.AddComponent<Partir>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<PosturaDefensiva>() == null)
          {
            ListaElegirHabilidad.AddComponent<PosturaDefensiva>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<SiguesTu>() == null)
          {
            ListaElegirHabilidad.AddComponent<SiguesTu>();
          }
        }
        if (pSel.IDClase == 2) //Explorador
        {

          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONVistaLejana>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONVistaLejana>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONAcrobatico>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcrobatico>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<MarcarPresa>() == null)
          {
            ListaElegirHabilidad.AddComponent<MarcarPresa>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<DisparoPotente>() == null)
          {
            ListaElegirHabilidad.AddComponent<DisparoPotente>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<Vigilancia>() == null)
          {
            ListaElegirHabilidad.AddComponent<Vigilancia>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Acechar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Acechar>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<Fogata>() == null)
          {
            ListaElegirHabilidad.AddComponent<Fogata>();
          }
          //Explorador tiene una menos por la de crear flechas que es intrinseca.
        }
        if (pSel.IDClase == 3) //Purificadora
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAuraSagrada>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAuraSagrada>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONEcosDivinos>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONEcosDivinos>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<Enmendar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Enmendar>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<LuzCegadora>() == null)
          {
            ListaElegirHabilidad.AddComponent<LuzCegadora>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<PilaresDeLuz>() == null)
          {
            ListaElegirHabilidad.AddComponent<PilaresDeLuz>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<SalmoPurificador>() == null)
          {
            ListaElegirHabilidad.AddComponent<SalmoPurificador>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<LlamaDivina>() == null)
          {
            ListaElegirHabilidad.AddComponent<LlamaDivina>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<CastigaraLosMalvados>() == null)
          {
            ListaElegirHabilidad.AddComponent<CastigaraLosMalvados>();
          }
        }
        if (pSel.IDClase == 4) //Acechador
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONMaestriaBallesta>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONMaestriaBallesta>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONMaestriaEspadaCorta>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONMaestriaEspadaCorta>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<DisparoEnvenenado>() == null)
          {
            ListaElegirHabilidad.AddComponent<DisparoEnvenenado>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<CorteIncapacitante>() == null)
          {
            ListaElegirHabilidad.AddComponent<CorteIncapacitante>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<BombaDeHumo>() == null)
          {
            ListaElegirHabilidad.AddComponent<BombaDeHumo>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Asesinar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Asesinar>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<Distraer>() == null)
          {
            ListaElegirHabilidad.AddComponent<Distraer>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<ArrojarAbrojos>() == null)
          {
            ListaElegirHabilidad.AddComponent<ArrojarAbrojos>();
          }
        }
        if (pSel.IDClase == 5) //Canalizador
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAcumulacionProtegida>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcumulacionProtegida>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<DescargaDePoder>() == null)
          {
            ListaElegirHabilidad.AddComponent<DescargaDePoder>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<Instatransporte>() == null)
          {
            ListaElegirHabilidad.AddComponent<Instatransporte>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<AcumulacionInestable>() == null)
          {
            ListaElegirHabilidad.AddComponent<AcumulacionInestable>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<HojaDeEnergia>() == null)
          {
            ListaElegirHabilidad.AddComponent<HojaDeEnergia>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<EscudoEnergetico>() == null)
          {
            ListaElegirHabilidad.AddComponent<EscudoEnergetico>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<SifonArcano>() == null)
          {
            ListaElegirHabilidad.AddComponent<SifonArcano>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<REPRESENTACIONExcesoDePoder>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONExcesoDePoder>();
          }

        }



        foreach (Habilidad habilidad in ListaElegirHabilidad.gameObject.GetComponents<Habilidad>())
        {

          poolSortear.Add(habilidad);

        }
        // Verificar si hay más de 3 habilidades en la lista
        if (poolSortear.Count > 3)
        {
          // Crear una instancia de Random
          System.Random random = new System.Random();

          // Crear una nueva lista que contendrá las habilidades seleccionadas al azar
          List<Habilidad> habilidadesSeleccionadas = new List<Habilidad>();

          // Seleccionar 3 elementos al azar
          for (int i = 0; i < 3; i++)
          {
            int indexAleatorio =random.Next(poolSortear.Count);
            habilidadesSeleccionadas.Add(poolSortear[indexAleatorio]);
            poolSortear.RemoveAt(indexAleatorio); // Eliminar el elemento seleccionado de la lista original
          }

          // Asignar la lista de habilidades seleccionadas de vuelta a poolSortear
          poolSortear = habilidadesSeleccionadas;
        }

        foreach (Habilidad habilidad in poolSortear)
        {
          GameObject actionButtonTransform = Instantiate(actionButtonPrefab, ListaElegirHabilidad);
          BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
          habilidadBotonUI.HabilidadRepresentada = habilidad;
          actionButtonTransform.transform.GetChild(4).gameObject.SetActive(true);

        }
      }
    }
    else { HabilidadBaseNueva.SetActive(false); }
  }

  public void SubirAtributo(int i)
  {
    if (i == 1)//1-Fuerza
    {
      pSel.iFuerza++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iAgi++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
    }
    if (i == 3)//3-Poder
    {
      pSel.iPoder++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
    }

  }

  public void SubirTiradaSalvacion(int i)
  {
    if (i == 1)//1-Fuerza
    {
      pSel.iTSFortaleza++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iTSReflejo++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
    }
    if (i == 3)//3-Poder
    {
      pSel.iTSMental++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
    }

  }


  public void subirNivel()
  {
    pSel.RecibirExperiencia(100);

    ActualizarInfo();

  }

  public GameObject btnPos1;
  public GameObject btnPos2;
  public GameObject btnPos3;

  public void SelPos(int pos)
  {
    // Restablecer el tamaño de todos los botones a su escala original
    btnPos1.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
    btnPos2.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
    btnPos3.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);

    // Aumentar el tamaño del botón seleccionado en un 25%
    switch (pos)
    {
      case 1:
        btnPos1.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
      case 2:
        btnPos2.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
      case 3:
        btnPos3.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
    }

    pSel.iPuestoDeseado = pos;

  }

}
