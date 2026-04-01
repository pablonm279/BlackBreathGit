using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using Unity.VisualScripting;

public class MenuPersonajes : MonoBehaviour
{
  private const string COLOR_RASGO_TEMP_NEG = "#ff9e9e";
  private const string COLOR_RASGO_TEMP_POS = "#a8ff9e";
  private const string COLOR_RASGO_LEGACY_NEG = "#d80404";
  private const string COLOR_RASGO_LEGACY_POS = "#2a9c71";

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
  public Sprite Female002; //Duelista


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
  [SerializeField] TextMeshProUGUI txtCapacidadPersonajes;

  [SerializeField] Image imCorazon;
  [SerializeField] Image imMedalla;

  private void Awake()
  {
    AsegurarTextoCapacidadPersonajes();
  }

  public void PrepararYAbrirMenu(Personaje personajeInicial = null)
  {
    AsegurarTextoCapacidadPersonajes();
    ActualizarTextoCapacidadPersonajes();

    if (listaPersonajes.Count == 0)
    {
      ActualizarLista();
      ForzarRebuildInmediato();
      return;
    }

    Personaje personajeBase = personajeInicial != null && !personajeInicial.Camp_Muerto
      ? personajeInicial
      : listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
    pSel = personajeBase != null ? personajeBase : listaPersonajes[0];
    if (scEquipo != null)
    {
      scEquipo.ConfigurarClickDerechoSlots(this);
    }
    ActualizarLista();
    ActualizarTextoCapacidadPersonajes();
    CancelInvoke("ActualizarInfo");
    ActualizarInfo();
    ForzarRebuildInmediato();
  }

  private void AsegurarTextoCapacidadPersonajes()
  {
    if (txtCapacidadPersonajes != null || txtNombre == null)
    {
      return;
    }

    Transform parent = txtNombre.transform.parent;
    if (parent == null)
    {
      return;
    }

    GameObject goCapacidad = Instantiate(txtNombre.gameObject, parent);
    goCapacidad.name = "txtCapacidadPersonajes";
    txtCapacidadPersonajes = goCapacidad.GetComponent<TextMeshProUGUI>();
    if (txtCapacidadPersonajes == null)
    {
      return;
    }

    RectTransform rt = txtCapacidadPersonajes.rectTransform;
    if (rt != null)
    {
      rt.anchorMin = new Vector2(1f, 1f);
      rt.anchorMax = new Vector2(1f, 1f);
      rt.pivot = new Vector2(1f, 1f);
      rt.anchoredPosition = new Vector2(-12f, -12f);
      rt.sizeDelta = new Vector2(120f, rt.sizeDelta.y);
    }

    txtCapacidadPersonajes.alignment = TextAlignmentOptions.TopRight;
    txtCapacidadPersonajes.fontSize = txtNombre.fontSize * 0.8f;
    txtCapacidadPersonajes.raycastTarget = false;
    txtCapacidadPersonajes.text = string.Empty;
  }

  private void ActualizarTextoCapacidadPersonajes()
  {
    if (txtCapacidadPersonajes == null)
    {
      AsegurarTextoCapacidadPersonajes();
    }

    if (txtCapacidadPersonajes == null)
    {
      return;
    }

    int actuales = CampaignManager.Instance != null ? CampaignManager.Instance.CuantosPersonajesActivos() : 0;
    int maximos = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerCapacidadMaximaPersonajes() : 4;
    txtCapacidadPersonajes.text = actuales + "/" + maximos;
  }

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

         if (btn.personajeRepresentado.Camp_Fatigado)
        {
          child.GetChild(10).gameObject.SetActive(true);
        }
        else { child.GetChild(10).gameObject.SetActive(false); }





        if (pSel == btn.personajeRepresentado)
        {
          btn.transform.GetChild(12).gameObject.SetActive(true);

        }
        else
        {
          btn.transform.GetChild(12).gameObject.SetActive(false);
        }
      }
    }

    ActualizarTextoCapacidadPersonajes();


  }


  public void SeleccionarPersonaje(Personaje pers, GameObject btnPers)
  {
    if (pers == null) return;

    pSel = pers;
    RuntimeAnalytics.TrackDesign("characters", "select", RuntimeAnalytics.ClassToken(pSel));
    ActualizarLista();
    if (btnPers != null && btnPers.transform.childCount > 0)
    { btnPers.transform.GetChild(12).gameObject.SetActive(true); }

    CancelInvoke("ActualizarInfo");
    ActualizarInfo();
    if (scEquipo != null)
    {
      scEquipo.RefrescarInventarioSiAbierto();
    }
    ForzarRebuildInmediato();

  }



  public void ActualizarInfo()
  {
    pSel.NormalizarPuntosPendientesPorNivelActual();
    SelPos(pSel.iPuestoDeseado);
    //Clase
    switch (pSel.IDClase)
    {
      case 1: txtClase.text = TRADU.i.Traducir("Caballero"); break;
      case 2: txtClase.text = TRADU.i.Traducir("Explorador"); break;
      case 3: txtClase.text = TRADU.i.Traducir("Purificadora"); break;
      case 4: txtClase.text = TRADU.i.Traducir("Acechador"); break;
      case 5: txtClase.text = TRADU.i.Traducir("Canalizador"); break;
      case 6: txtClase.text = TRADU.i.Traducir("Duelista"); break;
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
    float experienciaNecesaria = pSel.ObtenerExperienciaNecesariaParaProximoNivel();
    txtExperiencia.text = $"" + pSel.fExperienciaActual + "/" + experienciaNecesaria;
    txtNivel.text = "" + pSel.fNivelActual;
    float vidaActualEscalada = pSel.ObtenerVidaActualConFuerza(scEquipo.BuffTOTALEQUIPOhpMax, scEquipo.BuffTOTALEQUIPOFuerza);
    float vidaMaxEscalada = pSel.ObtenerVidaMaximaConFuerza(scEquipo.BuffTOTALEQUIPOhpMax, scEquipo.BuffTOTALEQUIPOFuerza);
    imCorazon.fillAmount = Mathf.Clamp01(vidaActualEscalada / vidaMaxEscalada);
    txtHP.text = "" + (int)vidaActualEscalada + "/" + (int)vidaMaxEscalada;
    imMedalla.fillAmount = Mathf.Clamp01((float)pSel.fExperienciaActual / experienciaNecesaria);
    txtFuerza.text = TRADU.i.Traducir("Fuerza: ") + (pSel.iFuerza + scEquipo.BuffTOTALEQUIPOFuerza);
    txtAgi.text = TRADU.i.Traducir("Agilidad: ") + (pSel.iAgi + scEquipo.BuffTOTALEQUIPOAgi);
    txtPoder.text = TRADU.i.Traducir("Poder: ") + (pSel.iPoder + scEquipo.BuffTOTALEQUIPOPoder);
    txtIniciativa.text = TRADU.i.Traducir("Iniciativa: ") + (pSel.iIniciativa + scEquipo.BuffTOTALEQUIPOIniciativa);
    txtApMax.text = TRADU.i.Traducir("PA: ") + (pSel.iApMax + scEquipo.BuffTOTALEQUIPOApMax);
    txtValMax.text = TRADU.i.Traducir("Valentía: ") + (pSel.iValMax + scEquipo.BuffTOTALEQUIPOValMax);
    txtArmadura.text = TRADU.i.Traducir("Armadura: ") + (pSel.iArmadura + scEquipo.BuffTOTALEQUIPOArmadura);
    txtDefensa.text = TRADU.i.Traducir("Defensa: ") + Mathf.RoundToInt(pSel.ObtenerDefensaTotalConAgilidad(scEquipo.BuffTOTALEQUIPODefensa, scEquipo.BuffTOTALEQUIPOAgi));
    txtTSReflejo.text = TRADU.i.Traducir("-Reflejos: ") + (pSel.iTSReflejo + scEquipo.BuffTOTALEQUIPOTSReflejo);
    txtTSFortaleza.text = TRADU.i.Traducir("-Fortaleza: ") + (pSel.iTSFortaleza + scEquipo.BuffTOTALEQUIPOTSFortaleza);
    txtTSMental.text = TRADU.i.Traducir("-Mental: ") + (pSel.iTSMental + scEquipo.BuffTOTALEQUIPOTSMental);
    txtResFuego.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResFuego, scEquipo.BuffTOTALEQUIPOResFuego, scEquipo.BuffTOTALEQUIPOPoder);
    txtResRayo.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResRayo, scEquipo.BuffTOTALEQUIPOResRayo, scEquipo.BuffTOTALEQUIPOPoder);
    txtResHielo.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResHielo, scEquipo.BuffTOTALEQUIPOResHielo, scEquipo.BuffTOTALEQUIPOPoder);
    txtResArcano.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResArcano, scEquipo.BuffTOTALEQUIPOResArcano, scEquipo.BuffTOTALEQUIPOPoder);
    txtResAcido.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResAcido, scEquipo.BuffTOTALEQUIPOResAcido, scEquipo.BuffTOTALEQUIPOPoder);
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
    if (pSel.Camp_Fatigado)
    {
      string fatigado = TRADU.i.Traducir("<color=#2a9c71>\n\nFatigado: -1 Atributos hasa próximo descanso. </color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(fatigado, false);
    }

    if (pSel.Camp_Bendecido_SequitoClerigos)
    {
      string bendecido = "<color=#2a9c71>\n\n" + TRADU.i.Traducir("Bendecido por Plegaria: +1 Ataque +1 Defensa +5 Res.Necro +2 TSMental.</color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(bendecido, true);
    }

    if (pSel.Camp_Herido)
    {
      string herido = TRADU.i.Traducir("<color=#d80404>\n\nHerido:-1 Atributos. Si cae en combate, muere. </color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(herido, false);
    }

    if (pSel.Camp_Corrupto)
    {
      string corrupto = TRADU.i.Traducir("<color=#d80404>\n\nCorrupto: Los enemigos corrompidos se curan al atacarlo, le infligen mas daño, y si lo derriban en combate, muere. </color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(corrupto, false);
    }

    if (pSel.Camp_Enfermo > 0)
    {
      string enfermo = TRADU.i.Traducir("<color=#d80404>\n\nEnfermo por ") + pSel.Camp_Enfermo + TRADU.i.Traducir(" días. -15% daño, -3 TS Fortaleza, -1 PA </color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(enfermo, false);
    }

    if (pSel.Camp_Moral < 0)
    {
      string bajaMoral = TRADU.i.Traducir("<color=#d80404>\n\nBaja Moral por ") + -pSel.Camp_Moral + TRADU.i.Traducir(" días. -1 Ataque y Defensa, -3 TS Mental, -2 Valentía Inicial</color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(bajaMoral, false);
    }

    if (pSel.Camp_Moral > 0)
    {
      string altaMoral = TRADU.i.Traducir("<color=#d80404>\n\nAlta Moral por ") + pSel.Camp_Moral + TRADU.i.Traducir(" días. +1 Ataque, +2 TS Mental, +2 Valentía Inicial</color>");
      txtContenedorRasgos.text += RecolorEstadoTemporal(altaMoral, true);
    }

    if (pSel.Camp_Avergonzado)
    {
      bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      string avergonzado = enIngles
        ? "<color=#d80404>\n\nAshamed: -2 Mental Save, -10% max HP. Removed on zone change.</color>"
        : "<color=#d80404>\n\nAvergonzado: -2 TS Mental, -10% HP máxima. Se limpia al cambiar de zona.</color>";
      txtContenedorRasgos.text += RecolorEstadoTemporal(avergonzado, false);
    }
  }

  private static string RecolorEstadoTemporal(string texto, bool positivo)
  {
    if (string.IsNullOrEmpty(texto)) { return texto; }

    string colorObjetivo = positivo ? COLOR_RASGO_TEMP_POS : COLOR_RASGO_TEMP_NEG;
    return texto
      .Replace(COLOR_RASGO_LEGACY_NEG, colorObjetivo)
      .Replace(COLOR_RASGO_LEGACY_POS, colorObjetivo);
  }

  string DevolverRasgo(int id)
  {
    string rasgoDesc = "";

    if (id == 1) { rasgoDesc = TRADU.i.Traducir("Torpe: +1 Rango Pifias"); }
    if (id == 2) { rasgoDesc = TRADU.i.Traducir("Valiente: +2 Valentía Máxima."); }
    if (id == 3) { rasgoDesc = TRADU.i.Traducir("Alegre: +2 Esperanza al Descansar."); }
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
  private bool ignorarClickIzquierdoPorClickDerechoEquipo;
  private float tiempoBloqueoClickDerechoEquipoHasta;

  public void OnClickCofre()
  {
    if (!scEquipo.goInventario.activeInHierarchy)
    {
      RuntimeAnalytics.TrackDesign("characters", "inventory_open", "backpack");
      scEquipo.MostrarInventario(5);
    }
    else { scEquipo.goInventario.SetActive(false); }


  }
  public void OnClickArma()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "weapon");
    AbrirInventarioDeEquipo(1);
  }
  public void OnClickArmadura()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "armor");
    AbrirInventarioDeEquipo(2);
  }


  public void OnHoverArma()
  {
    if (pSel.itemArma != null)
    {
      //itemDesc.text = pSel.itemArma.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.itemArma, true);
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
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.itemArmadura, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

      }
    }
  }


  public void OnClickAccesorio1()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    scEquipo.accesorioACambiar = 1;
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "accessory_1");
    AbrirInventarioDeEquipo(3);
  }

  public void OnHoverAccesorio1()
  {
    if (pSel.Accesorio1 != null)
    {
      //itemDesc.text = pSel.Accesorio1.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Accesorio1, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void OnClickAccesorio2()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    scEquipo.accesorioACambiar = 2;
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "accessory_2");
    AbrirInventarioDeEquipo(3);
  }

  public void OnHoverAccesorio2()
  {
    if (pSel.Accesorio2 != null)
    {
      //itemDesc.text = pSel.Accesorio2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Accesorio2, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }


  public void OnClickConsumible1()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    scEquipo.consumibleACambiar = 1;
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "consumable_1");
    AbrirInventarioDeEquipo(4);
  }

  public void OnHoverConsumible1()
  {
    if (pSel.Consumible1 != null)
    {
      //itemDesc.text = pSel.Consumible1.itemDescrpicion;
        Vector3 pos = Input.mousePosition;
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Consumible1, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void OnClickConsumible2()
  {
    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    scEquipo.consumibleACambiar = 2;
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "consumable_2");
    AbrirInventarioDeEquipo(4);
  }

  public void OnHoverConsumible2()
  {
    if (pSel.Consumible2 != null)
    {
      //itemDesc.text = pSel.Consumible2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Consumible2, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

    }
  }

  public void RegistrarClickDerechoEnSlotEquipo()
  {
    ignorarClickIzquierdoPorClickDerechoEquipo = true;
    tiempoBloqueoClickDerechoEquipoHasta = Time.unscaledTime + 0.25f;
  }

  public void LimpiarBloqueoClickDerechoEnSlotEquipo()
  {
    StartCoroutine(LimpiarBloqueoClickDerechoEnSlotEquipoDelay());
  }

  private IEnumerator LimpiarBloqueoClickDerechoEnSlotEquipoDelay()
  {
    yield return null;
    ignorarClickIzquierdoPorClickDerechoEquipo = false;
    tiempoBloqueoClickDerechoEquipoHasta = 0f;
  }

  private bool DebeIgnorarClickSlotPorBloqueoDerecho()
  {
    if (!ignorarClickIzquierdoPorClickDerechoEquipo)
    {
      return false;
    }

    if (Time.unscaledTime > tiempoBloqueoClickDerechoEquipoHasta)
    {
      ignorarClickIzquierdoPorClickDerechoEquipo = false;
      tiempoBloqueoClickDerechoEquipoHasta = 0f;
      return false;
    }

    return true;
  }

  private void AbrirInventarioDeEquipo(int tipo)
  {
    if (scEquipo == null)
    {
      return;
    }

    scEquipo.MostrarInventario(tipo);
  }

  private void RefrescarInventarioDeEquipoSiEstaAbierto(int tipo)
  {
    if (scEquipo != null && scEquipo.goInventario != null && scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(tipo);
    }
  }

  private void AgregarAlInventarioSiHaceFalta(GameObject itemGO)
  {
    if (itemGO == null || scEquipo == null)
    {
      return;
    }

    if (!scEquipo.listInventario.Contains(itemGO))
    {
      scEquipo.listInventario.Add(itemGO);
    }
  }

  public bool EquiparArmaDesdeInventario(Arma nuevaArma)
  {
    if (nuevaArma == null || pSel == null)
    {
      return false;
    }

    if (!nuevaArma.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevaArma.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevaArma.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevaArma.requisitoPoder > pSel.iPoder) { return false; }

    Arma armaAnterior = pSel.itemArma;
    if (armaAnterior != null)
    {
      pSel.QuitarArma(armaAnterior);
      AgregarAlInventarioSiHaceFalta(armaAnterior.gameObject);
    }

    pSel.itemArma = nuevaArma;
    scEquipo.listInventario.Remove(nuevaArma.gameObject);
    return true;
  }

  public bool EquiparArmaduraDesdeInventario(Armadura nuevaArmadura)
  {
    if (nuevaArmadura == null || pSel == null)
    {
      return false;
    }

    if (!nuevaArmadura.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevaArmadura.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevaArmadura.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevaArmadura.requisitoPoder > pSel.iPoder) { return false; }

    Armadura armaduraAnterior = pSel.itemArmadura;
    if (armaduraAnterior != null)
    {
      pSel.QuitarArmadura(armaduraAnterior);
      AgregarAlInventarioSiHaceFalta(armaduraAnterior.gameObject);
    }

    pSel.itemArmadura = nuevaArmadura;
    scEquipo.listInventario.Remove(nuevaArmadura.gameObject);
    return true;
  }

  public bool EquiparAccesorioDesdeInventario(Accesorio nuevoAccesorio)
  {
    if (nuevoAccesorio == null || pSel == null)
    {
      return false;
    }

    if (!nuevoAccesorio.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevoAccesorio.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevoAccesorio.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevoAccesorio.requisitoPoder > pSel.iPoder) { return false; }

    bool cambiarSlot2 = scEquipo != null && scEquipo.accesorioACambiar == 2;
    if (cambiarSlot2)
    {
      Accesorio accesorioAnterior = pSel.Accesorio2;
      if (accesorioAnterior != null)
      {
        pSel.QuitarAccesorio2(accesorioAnterior);
        AgregarAlInventarioSiHaceFalta(accesorioAnterior.gameObject);
      }

      pSel.Accesorio2 = nuevoAccesorio;
    }
    else
    {
      Accesorio accesorioAnterior = pSel.Accesorio1;
      if (accesorioAnterior != null)
      {
        pSel.QuitarAccesorio1(accesorioAnterior);
        AgregarAlInventarioSiHaceFalta(accesorioAnterior.gameObject);
      }

      pSel.Accesorio1 = nuevoAccesorio;
    }

    scEquipo.listInventario.Remove(nuevoAccesorio.gameObject);
    return true;
  }

  public bool EquiparConsumibleDesdeInventario(Consumible nuevoConsumible)
  {
    if (nuevoConsumible == null || pSel == null)
    {
      return false;
    }

    if (!nuevoConsumible.PuedeUsarClase(pSel.IDClase)) { return false; }
    bool cambiarSlot2 = scEquipo != null && scEquipo.consumibleACambiar == 2;
    if (cambiarSlot2)
    {
      Consumible consumibleAnterior = pSel.Consumible2;
      if (consumibleAnterior != null)
      {
        pSel.QuitarConsumible2(consumibleAnterior);
        AgregarAlInventarioSiHaceFalta(consumibleAnterior.gameObject);
      }

      pSel.Consumible2 = nuevoConsumible;
    }
    else
    {
      Consumible consumibleAnterior = pSel.Consumible1;
      if (consumibleAnterior != null)
      {
        pSel.QuitarConsumible1(consumibleAnterior);
        AgregarAlInventarioSiHaceFalta(consumibleAnterior.gameObject);
      }

      pSel.Consumible1 = nuevoConsumible;
    }

    scEquipo.listInventario.Remove(nuevoConsumible.gameObject);
    return true;
  }

  public void OnRightClickArma()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.itemArma == null)
    {
      return;
    }

    Arma armaAQuitar = pSel.itemArma;
    pSel.QuitarArma(armaAQuitar);
    AgregarAlInventarioSiHaceFalta(armaAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(1);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickArmadura()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.itemArmadura == null)
    {
      return;
    }

    Armadura armaduraAQuitar = pSel.itemArmadura;
    pSel.QuitarArmadura(armaduraAQuitar);
    AgregarAlInventarioSiHaceFalta(armaduraAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(2);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickAccesorio1()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Accesorio1 == null)
    {
      return;
    }

    Accesorio accesorioAQuitar = pSel.Accesorio1;
    pSel.QuitarAccesorio1(accesorioAQuitar);
    AgregarAlInventarioSiHaceFalta(accesorioAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(3);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickAccesorio2()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Accesorio2 == null)
    {
      return;
    }

    Accesorio accesorioAQuitar = pSel.Accesorio2;
    pSel.QuitarAccesorio2(accesorioAQuitar);
    AgregarAlInventarioSiHaceFalta(accesorioAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(3);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickConsumible1()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Consumible1 == null)
    {
      return;
    }

    Consumible consumibleAQuitar = pSel.Consumible1;
    pSel.QuitarConsumible1(consumibleAQuitar);
    AgregarAlInventarioSiHaceFalta(consumibleAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(4);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickConsumible2()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Consumible2 == null)
    {
      return;
    }

    Consumible consumibleAQuitar = pSel.Consumible2;
    pSel.QuitarConsumible2(consumibleAQuitar);
    AgregarAlInventarioSiHaceFalta(consumibleAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(4);
    Invoke("ActualizarInfo", 0.05f);
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
        if (pSel.IDClase == 6) //Duelista
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAtaquesReveladores>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAtaquesReveladores>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONEvasionMaestra>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONEvasionMaestra>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<CargaDeEstoque>() == null)
          {
            ListaElegirHabilidad.AddComponent<CargaDeEstoque>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<Riposte>() == null)
          {
            ListaElegirHabilidad.AddComponent<Riposte>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<AFondo>() == null)
          {
            ListaElegirHabilidad.AddComponent<AFondo>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<EnGarde>() == null)
          {
            ListaElegirHabilidad.AddComponent<EnGarde>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<PuntaHiriente>() == null)
          {
            ListaElegirHabilidad.AddComponent<PuntaHiriente>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<RecuperarAire>() == null)
          {
            ListaElegirHabilidad.AddComponent<RecuperarAire>();
          }
        }



        foreach (Habilidad habilidad in ListaElegirHabilidad.gameObject.GetComponents<Habilidad>())
        {

          poolSortear.Add(habilidad);

        }
        if (poolSortear.Count == 0)
        {
          HabilidadBaseNueva.SetActive(false);
          yaTiroHabRand = false;
          return;
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
    string atributoAnalytics = null;
    if (i == 1)//1-Fuerza
    {
      pSel.iFuerza++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "fuerza";
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iAgi++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "agilidad";
    }
    if (i == 3)//3-Poder
    {
      pSel.iPoder++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "poder";
    }

    if (!string.IsNullOrEmpty(atributoAnalytics))
    {
      RuntimeAnalytics.TrackDesign("characters", "stat_up", atributoAnalytics);
    }

    if (CampaignManager.Instance.scTutorialManager.tutorialActivo) { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }

  }

  public void SubirTiradaSalvacion(int i)
  {
    string tsAnalytics = null;
    if (i == 1)//1-Fuerza
    {
      pSel.iTSFortaleza++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "fortaleza";
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iTSReflejo++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "reflejos";
    }
    if (i == 3)//3-Poder
    {
      pSel.iTSMental++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "mental";
    }

    if (!string.IsNullOrEmpty(tsAnalytics))
    {
      RuntimeAnalytics.TrackDesign("characters", "save_up", tsAnalytics);
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

  void ForzarRebuildInmediato()
  {
    Canvas.ForceUpdateCanvases();

    RectTransform root = transform as RectTransform;
    if (root != null)
      LayoutRebuilder.ForceRebuildLayoutImmediate(root);

    if (contenedorUIPersonajes != null)
    {
      RectTransform rtLista = contenedorUIPersonajes.transform as RectTransform;
      if (rtLista != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rtLista);
    }

    if (listaHab != null)
    {
      RectTransform rtHab = listaHab as RectTransform;
      if (rtHab != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rtHab);
    }

    Canvas.ForceUpdateCanvases();
  }

}



