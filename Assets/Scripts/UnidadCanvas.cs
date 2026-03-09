using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnidadCanvas : MonoBehaviour
{
    private const string RutaFuenteTextoFlotanteDanio = "Fuentes/SpectralSC/TextoFlotanteDaño";
    private const float EscalaTextoProbabilidad = 0.85f;

    public GameObject unidadCanvas;
    public TextMeshProUGUI txtDaño;
    public TextMeshProUGUI txtArmadura;
    public GameObject PrefabtxtDaño;
    public GameObject PrefabtxtValentia;
    public GameObject imMarcador;
    public TextMeshProUGUI txtProbabilidad;
    public GameObject PrefabtxtProbabilidad;

    public TextMeshProUGUI txtVida;


    public RectTransform barraVida;
    private Vector3 escalaBase;
    [SerializeField] private Image barraVidaDamageFill;
    [SerializeField] private Color barraVidaDamageColor = new Color(0.75f, 0.12f, 0.12f, 0.6f);
    [SerializeField] private float barraVidaDamageDelay = 0.12f;
    [SerializeField] private float barraVidaDamageSpeed = 0.9f;
    [SerializeField] private Image barraVidaHealFill;
    [SerializeField] private Color barraVidaHealColor = new Color(1f, 0.85f, 0.2f, 0.6f);
    [SerializeField] private float barraVidaHealDelay = 0.08f;
    [SerializeField] private float barraVidaHealSpeed = 0.9f;
    private float barraVidaLastRatio = -1f;
    private float barraVidaDamageRatio = -1f;
    private float barraVidaHealRatio = -1f;
    private Coroutine barraVidaDamageCoroutine;
    private Coroutine barraVidaHealCoroutine;
    private RectTransform barraVidaFillRect;
    private Slider barraVidaSlider;
    private TMP_FontAsset fuenteTextoProbabilidad;

    void Start()
    {
        if(barraVida != null)
        {
            escalaBase = barraVida.localScale;   // guardás la escala original
        }
        PrepararBarraDanio();
    }

    void Update()
    {
        Unidad unidad = GetComponentInParent<Unidad>();


        if (unidad == null) { return; }

        if (txtArmadura != null) { txtArmadura.text = unidad.ObtenerArmaduraActual().ToString(); }

        if (txtVida != null)
        {

            txtVida.text = ((int)unidad.HP_actual).ToString();
            txtVida.text += "/" + ((int)unidad.mod_maxHP).ToString();

            // Mostrar barrera de daño en azul si existe
            if (unidad.barreraDeDanio > 0)
            {
                int barreraMostrada = Mathf.RoundToInt(unidad.barreraDeDanio);
                if (barreraMostrada > 0)
                {
                    txtVida.text += $" <size=90%><color=#4FC3F7>+({barreraMostrada})</color></size>";
                }
            }
        }
        if (barraVida != null)
        {
            barraVida.gameObject.GetComponent<Slider>().value = unidad.HP_actual / unidad.mod_maxHP * 100;
            
            int posEstaUnidad = BattleManager.Instance.lUnidadesTotal.IndexOf(unidad);
            int indexTurno = BattleManager.Instance.indexTurno;
            bool yapasosuturno = indexTurno > posEstaUnidad+1;


            Image barraFillImage = barraVida.gameObject.transform.GetChild(2).GetChild(0).GetComponentInChildren<Image>();
            if (yapasosuturno)
            {
                barraFillImage.color = new Color(barraFillImage.color.r, barraFillImage.color.g, barraFillImage.color.b, 0.45f); // oscurecer (alpha 0.5)
            }
            else
            {
                barraFillImage.color = new Color(barraFillImage.color.r, barraFillImage.color.g, barraFillImage.color.b, 1f); // normal (alpha 1)
            }
        }
        ActualizarBarraDanio(unidad);
        ActualizarEscalaBarra(unidad);

        ActualizarEstadosIconos();
    }

    void PrepararBarraDanio()
    {
        if (barraVida == null) { return; }
        if (barraVidaSlider == null)
        {
            barraVidaSlider = barraVida.GetComponent<Slider>();
        }
        if (barraVidaSlider == null || barraVidaSlider.fillRect == null) { return; }

        barraVidaFillRect = barraVidaSlider.fillRect;

        RectTransform parent = barraVidaFillRect.parent as RectTransform;
        if (parent == null) { return; }

        Image fillImg = barraVidaFillRect.GetComponent<Image>();

        if (barraVidaDamageFill == null)
        {
            GameObject go = new GameObject("DamageFill", typeof(RectTransform), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = barraVidaFillRect.anchorMin;
            rt.anchorMax = barraVidaFillRect.anchorMax;
            rt.pivot = barraVidaFillRect.pivot;
            rt.offsetMin = barraVidaFillRect.offsetMin;
            rt.offsetMax = barraVidaFillRect.offsetMax;
            rt.localScale = Vector3.one;

            Image img = go.GetComponent<Image>();
            if (fillImg != null)
            {
                img.sprite = fillImg.sprite;
                img.type = fillImg.type;
                img.fillMethod = fillImg.fillMethod;
                img.fillOrigin = fillImg.fillOrigin;
                img.fillClockwise = fillImg.fillClockwise;
                img.preserveAspect = fillImg.preserveAspect;
                img.material = fillImg.material;
            }
            img.color = barraVidaDamageColor;
            img.raycastTarget = false;

            int fillIndex = barraVidaFillRect.GetSiblingIndex();
            rt.SetSiblingIndex(fillIndex);

            barraVidaDamageFill = img;
        }

        if (barraVidaHealFill != null)
        {
            barraVidaHealFill.color = new Color(barraVidaHealColor.r, barraVidaHealColor.g, barraVidaHealColor.b, 0f);
            barraVidaHealFill.gameObject.SetActive(false);
        }
    }

    void ActualizarBarraDanio(Unidad unidad)
    {
        if (unidad == null || barraVida == null) { return; }
        if (barraVidaDamageFill == null || barraVidaFillRect == null)
        {
            PrepararBarraDanio();
            if (barraVidaDamageFill == null || barraVidaFillRect == null) { return; }
        }

        if (unidad.mod_maxHP <= 0f) { return; }
        float ratio = Mathf.Clamp01(unidad.HP_actual / unidad.mod_maxHP);

        if (barraVidaLastRatio < 0f)
        {
            barraVidaLastRatio = ratio;
            barraVidaDamageRatio = ratio;
            SetBarraDanioRatio(ratio);
            return;
        }

        if (ratio < barraVidaLastRatio - 0.0001f)
        {
            barraVidaDamageRatio = Mathf.Max(barraVidaDamageRatio, barraVidaLastRatio);
            if (barraVidaDamageCoroutine != null) { StopCoroutine(barraVidaDamageCoroutine); }
            barraVidaDamageCoroutine = StartCoroutine(AnimarBarraDanio(ratio));

        }
        else if (ratio > barraVidaLastRatio + 0.0001f)
        {
            if (barraVidaDamageCoroutine != null) { StopCoroutine(barraVidaDamageCoroutine); barraVidaDamageCoroutine = null; }
            barraVidaDamageRatio = ratio;
            SetBarraDanioRatio(ratio);
        }

        barraVidaLastRatio = ratio;
    }

    IEnumerator AnimarBarraDanio(float targetRatio)
    {
        if (barraVidaDamageFill == null) { yield break; }
        float startRatio = barraVidaDamageRatio;
        if (targetRatio >= startRatio)
        {
            barraVidaDamageRatio = targetRatio;
            SetBarraDanioRatio(targetRatio);
            yield break;
        }

        if (barraVidaDamageDelay > 0f) { yield return new WaitForSeconds(barraVidaDamageDelay); }

        while (barraVidaDamageRatio > targetRatio + 0.0001f)
        {
            barraVidaDamageRatio = Mathf.MoveTowards(barraVidaDamageRatio, targetRatio, barraVidaDamageSpeed * Time.deltaTime);
            SetBarraDanioRatio(barraVidaDamageRatio);
            yield return null;
        }

        barraVidaDamageRatio = targetRatio;
        SetBarraDanioRatio(targetRatio);
    }

    IEnumerator AnimarBarraCuracion(float targetRatio)
    {
        if (barraVidaHealFill == null) { yield break; }
        float startRatio = barraVidaHealRatio;
        if (targetRatio <= startRatio)
        {
            barraVidaHealRatio = targetRatio;
            SetBarraCuracionRatio(targetRatio);
            yield break;
        }

        if (barraVidaHealDelay > 0f) { yield return new WaitForSeconds(barraVidaHealDelay); }

        while (barraVidaHealRatio < targetRatio - 0.0001f)
        {
            barraVidaHealRatio = Mathf.MoveTowards(barraVidaHealRatio, targetRatio, barraVidaHealSpeed * Time.deltaTime);
            SetBarraCuracionRatio(barraVidaHealRatio);
            yield return null;
        }

        barraVidaHealRatio = targetRatio;
        SetBarraCuracionRatio(targetRatio);
        if (barraVidaHealFill != null)
        {
            barraVidaHealFill.color = new Color(barraVidaHealColor.r, barraVidaHealColor.g, barraVidaHealColor.b, 0f);
        }
    }

    void SetBarraDanioRatio(float ratio)
    {
        if (barraVidaDamageFill == null || barraVidaFillRect == null) { return; }
        RectTransform rt = barraVidaDamageFill.rectTransform;
        Vector2 anchorMin = rt.anchorMin;
        Vector2 anchorMax = rt.anchorMax;
        anchorMin.x = barraVidaFillRect.anchorMin.x;
        anchorMax.x = Mathf.Clamp01(ratio);
        anchorMin.y = barraVidaFillRect.anchorMin.y;
        anchorMax.y = barraVidaFillRect.anchorMax.y;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = barraVidaFillRect.offsetMin;
        rt.offsetMax = barraVidaFillRect.offsetMax;
    }

    void SetBarraCuracionRatio(float ratio)
    {
        if (barraVidaHealFill == null || barraVidaFillRect == null) { return; }
        RectTransform rt = barraVidaHealFill.rectTransform;
        Vector2 anchorMin = rt.anchorMin;
        Vector2 anchorMax = rt.anchorMax;
        anchorMin.x = barraVidaFillRect.anchorMin.x;
        anchorMax.x = Mathf.Clamp01(ratio);
        anchorMin.y = barraVidaFillRect.anchorMin.y;
        anchorMax.y = barraVidaFillRect.anchorMax.y;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = barraVidaFillRect.offsetMin;
        rt.offsetMax = barraVidaFillRect.offsetMax;
    }


    [SerializeField] private GameObject casillaEstadoPrefab;
    [SerializeField] private GameObject contenedorCasillasEstados;
    private int firmaEstadosUI = int.MinValue;

    void OnEnable()
    {
        firmaEstadosUI = int.MinValue;
    }

    void ActualizarEscalaBarra(Unidad unidad)
    {
        Casilla casilla = unidad.CasillaPosicion;
        if (casilla == null) return;

        // filas X = 1..5
        float minScale = 1.4f;   // 100%
        float maxScale = 2.0f;   // 120%
        float t = Mathf.InverseLerp(1f, 5f, Mathf.Clamp(casilla.posY, 1f, 5f));
        float factor = Mathf.Lerp(minScale, maxScale, t);

        if (barraVida == null) return;
        // Escala uniforme: mismo factor en X e Y (Z queda en 1)
        barraVida.localScale = new Vector3(
        escalaBase.x * factor,
        escalaBase.y * factor,
        1f
    );
    }


    void ActualizarEstadosIconos()
    {
        if (contenedorCasillasEstados == null) return;
        Unidad scUnidadMostrada = GetComponentInParent<Unidad>();
        if (scUnidadMostrada == null) return;

        int firmaActual = CalcularFirmaEstadosUI(scUnidadMostrada);
        if (firmaActual == firmaEstadosUI) { return; }
        firmaEstadosUI = firmaActual;

        if (TooltipBatalla.Instance != null)
        {
            TooltipBatalla.Instance.HideTooltipSinAnim();
        }

        // Limpiar los iconos previos antes de agregar los nuevos
        foreach (Transform child in contenedorCasillasEstados.transform)
        {
            Destroy(child.gameObject);
        }

        // Mostrar Buffs
        List<BuffUIHelper.BuffStack> buffStacks = BuffUIHelper.GetVisibleBuffStacks(scUnidadMostrada);
        foreach (BuffUIHelper.BuffStack stack in buffStacks)
        {
            if (!stack.AggregatedBuff.esRemovible) { continue; }

            GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarBuff(stack.AggregatedBuff, true, stack.StackCount);
        }

        // Mostrar Reacciones
        foreach (Reaccion buff in scUnidadMostrada.gameObject.GetComponents<Reaccion>())
        {
            GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarReaccion(buff, true);
        }

        // Mostrar Marcas
        foreach (Marca buff in scUnidadMostrada.gameObject.GetComponents<Marca>())
        {
            GameObject buffCuadro = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
            buffCuadro.GetComponent<UIEstadoCuadro>().RepresentarMarca(buff, true);
        }

        MostrarEstados(scUnidadMostrada);
    }

    private static int MezclarHash(int actual, int valor)
    {
        unchecked { return (actual * 31) + valor; }
    }

    private static int HashTexto(string texto)
    {
        return string.IsNullOrEmpty(texto) ? 0 : texto.GetHashCode();
    }

    private int CalcularFirmaEstadosUI(Unidad unidad)
    {
        unchecked
        {
            int h = 17;
            h = MezclarHash(h, unidad.GetInstanceID());

            // Estados mostrados como iconos en barra de vida
            h = MezclarHash(h, unidad.estado_ardiendo);
            h = MezclarHash(h, unidad.estado_aturdido);
            h = MezclarHash(h, unidad.estado_acido);
            h = MezclarHash(h, unidad.estado_congelado);
            h = MezclarHash(h, unidad.estado_ResistenciasReducidas);
            h = MezclarHash(h, unidad.estado_sangrado);
            h = MezclarHash(h, unidad.estado_veneno);
            h = MezclarHash(h, unidad.estado_regeneravida);
            h = MezclarHash(h, unidad.estado_regeneraarmadura);
            h = MezclarHash(h, unidad.estado_evasion);
            h = MezclarHash(h, unidad.bonusdam_acido);
            h = MezclarHash(h, unidad.bonusdam_arcano);
            h = MezclarHash(h, unidad.bonusdam_fuego);
            h = MezclarHash(h, unidad.bonusdam_hielo);
            h = MezclarHash(h, unidad.bonusdam_necro);
            h = MezclarHash(h, unidad.bonusdam_rayo);
            h = MezclarHash(h, unidad.bonusdam_divino);
            h = MezclarHash(h, (int)unidad.tejidoCuracMagica);
            h = MezclarHash(h, unidad.ObtenerEstaEscondido());
            h = MezclarHash(h, unidad.estado_Corrupto ? 1 : 0);
            h = MezclarHash(h, unidad.estado_Volando ? 1 : 0);
            h = MezclarHash(h, unidad.estado_Condenado);
            h = MezclarHash(h, unidad.estado_Escudado);

            // Buffs visibles en barra (mismo filtro de render)
            List<BuffUIHelper.BuffStack> buffStacks = BuffUIHelper.GetVisibleBuffStacks(unidad);
            int visibles = 0;
            for (int i = 0; i < buffStacks.Count; i++)
            {
                Buff b = buffStacks[i].AggregatedBuff;
                if (ReferenceEquals(b, null) || !b.esRemovible) { continue; }

                visibles++;
                h = MezclarHash(h, HashTexto(b.buffNombre));
                h = MezclarHash(h, HashTexto(b.buffDescr));
                h = MezclarHash(h, b.boolfDebufftBuff ? 1 : 0);
                h = MezclarHash(h, b.DuracionBuffRondas);
                h = MezclarHash(h, buffStacks[i].StackCount);
            }
            h = MezclarHash(h, visibles);

            // Reacciones mostradas
            Reaccion[] reacciones = unidad.gameObject.GetComponents<Reaccion>();
            h = MezclarHash(h, reacciones.Length);
            for (int i = 0; i < reacciones.Length; i++)
            {
                Reaccion r = reacciones[i];
                if (r == null) { continue; }
                h = MezclarHash(h, r.usos);
                h = MezclarHash(h, HashTexto(r.descripcion));
            }

            // Marcas mostradas
            Marca[] marcas = unidad.gameObject.GetComponents<Marca>();
            h = MezclarHash(h, marcas.Length);
            for (int i = 0; i < marcas.Length; i++)
            {
                Marca m = marcas[i];
                if (m == null) { continue; }
                h = MezclarHash(h, m.duracion);
                h = MezclarHash(h, HashTexto(m.descripcion));
            }

            return h;
        }
    }

    public void CrearTextoProbabilidad()
    {
        if (txtProbabilidad != null)
        {
            ConfigurarTextoProbabilidad(txtProbabilidad);
            return;
        }

        GameObject goNuevo = null;
        if (PrefabtxtProbabilidad != null && unidadCanvas != null)
        {
            goNuevo = Instantiate(PrefabtxtProbabilidad, unidadCanvas.transform);
        }
        if (goNuevo == null)
        {
            goNuevo = new GameObject("txtProbabilidad");
            goNuevo.transform.SetParent(unidadCanvas != null ? unidadCanvas.transform : transform, false);
            TextMeshProUGUI tmp = goNuevo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
          
        }

        txtProbabilidad = goNuevo.GetComponent<TextMeshProUGUI>() ?? goNuevo.GetComponentInChildren<TextMeshProUGUI>();
        ConfigurarTextoProbabilidad(txtProbabilidad);

        RectTransform rt = txtProbabilidad != null ? txtProbabilidad.GetComponent<RectTransform>() : null;
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(0f, 18f);
        }

        if (txtProbabilidad != null)
        {
            txtProbabilidad.gameObject.SetActive(false);
        }
    }

    private void ConfigurarTextoProbabilidad(TextMeshProUGUI texto)
    {
        if (texto == null)
        {
            return;
        }

        TextMeshProUGUI textoReferencia = PrefabtxtDaño != null
            ? (PrefabtxtDaño.GetComponent<TextMeshProUGUI>() ?? PrefabtxtDaño.GetComponentInChildren<TextMeshProUGUI>())
            : null;

        if (textoReferencia != null)
        {
            texto.font = textoReferencia.font;
            texto.fontSharedMaterial = textoReferencia.fontSharedMaterial;
            texto.fontSize = textoReferencia.fontSize * EscalaTextoProbabilidad;
            texto.enableAutoSizing = textoReferencia.enableAutoSizing;
            texto.fontSizeMin = textoReferencia.fontSizeMin * EscalaTextoProbabilidad;
            texto.fontSizeMax = textoReferencia.fontSizeMax * EscalaTextoProbabilidad;
            texto.fontStyle = textoReferencia.fontStyle;
            texto.characterSpacing = textoReferencia.characterSpacing;
            texto.wordSpacing = textoReferencia.wordSpacing;
            texto.lineSpacing = textoReferencia.lineSpacing;
            texto.alignment = textoReferencia.alignment;
            return;
        }

        if (fuenteTextoProbabilidad == null)
        {
            fuenteTextoProbabilidad = Resources.Load<TMP_FontAsset>(RutaFuenteTextoFlotanteDanio);
        }

        if (fuenteTextoProbabilidad != null)
        {
            texto.font = fuenteTextoProbabilidad;
            texto.fontSharedMaterial = fuenteTextoProbabilidad.material;
        }
    }

   void MostrarEstados(Unidad scUnidadMostrada)
   {
      if (scUnidadMostrada.estado_ardiendo > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(1, -1, true);
      }
      if (scUnidadMostrada.estado_aturdido > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(2, -1, true);
      }
      if (scUnidadMostrada.estado_acido > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(3, -1, true);
      }
      if (scUnidadMostrada.estado_congelado > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(4, -1, true);
      }
      if (scUnidadMostrada.estado_ResistenciasReducidas > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(5, -1, true);
      }
      /*if(scUnidadMostrada.estado_armaduraModificador > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(6,-1,true);
      }*/
      if (scUnidadMostrada.estado_sangrado > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(8, -1, true);
      }
      if (scUnidadMostrada.estado_veneno > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(9, -1, true);
      }
      if (scUnidadMostrada.estado_APModificador > 0)
      {
         /*GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(7,scUnidadMostrada.estado_APModificador); */
      }
      if (scUnidadMostrada.estado_regeneravida > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(10, -1, true);
      }
      if (scUnidadMostrada.estado_regeneraarmadura > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(11, -1, true);
      }
      if (scUnidadMostrada.estado_evasion > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(12, -1, true);
      }

      /*  if (scUnidadMostrada is ClaseExplorador)
        {
               ClaseExplorador exp = (ClaseExplorador)scUnidadMostrada;
               GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
               GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(13, -1,true);
        }*/
      if (scUnidadMostrada.bonusdam_acido > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(14, -1, true);
      }
      if (scUnidadMostrada.bonusdam_arcano > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(15, scUnidadMostrada.bonusdam_arcano, true);
      }
      if (scUnidadMostrada.bonusdam_fuego > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(16, scUnidadMostrada.bonusdam_fuego, true);
      }
      if (scUnidadMostrada.bonusdam_hielo > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(17, scUnidadMostrada.bonusdam_hielo, true);
      }
      if (scUnidadMostrada.bonusdam_necro > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(18, scUnidadMostrada.bonusdam_necro, true);
      }
      if (scUnidadMostrada.bonusdam_rayo > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(19, scUnidadMostrada.bonusdam_rayo, true);
      }
      /*  if(scUnidadMostrada is ClasePurificadora)
       { 
          ClasePurificadora exp = (ClasePurificadora)scUnidadMostrada;
          GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
          GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(20,-1,true);
       }*/
      if (scUnidadMostrada.bonusdam_divino > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(21, -1, true);
      }
      /*  if(scUnidadMostrada.barreraDeDanio > 0)
       { 
          GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
          GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(22,-1,true);
       }*/
      if (scUnidadMostrada.tejidoCuracMagica > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(23, -1, true);
      }
      if (scUnidadMostrada.ObtenerEstaEscondido() == 1)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(24, -1, true);
      }
      if (scUnidadMostrada.ObtenerEstaEscondido() == 2)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(25, -1, true);
      }
      /* if(scUnidadMostrada is ClaseCanalizador)
      { 
         ClaseCanalizador exp = (ClaseCanalizador)scUnidadMostrada;
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(26,-1,true);
      }*/
      if (scUnidadMostrada.estado_Corrupto)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(27, -1, true);
      }
      if (scUnidadMostrada.estado_Volando)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(28, -1, true);
      }
      if (scUnidadMostrada.estado_Condenado > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(29, -1, true);
      }
      if (scUnidadMostrada.estado_Escudado > 0)
      {
         GameObject GTarjeta = Instantiate(casillaEstadoPrefab, contenedorCasillasEstados.transform);
         GTarjeta.GetComponent<UIEstadoCuadro>().RepresentarEstado(30, -1, true);
      }
    }

}


