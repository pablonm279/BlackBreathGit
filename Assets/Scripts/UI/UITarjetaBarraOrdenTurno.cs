using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.TerrainTools;

public class UITarjetaBarraOrdenTurno : MonoBehaviour
{
    public Slider BarraVida;
    public Image Retrato;
    public Unidad scUnidad;
    public UIInfoChar scUInfochar;
    public GameObject seleccionado;
    public GameObject Actual;
    public GameObject oscurecedor;

    private BattleManager battleManager;
    private int ordenIndex = -1;
    private bool eventosRegistrados;
    private Vector3 escalaBaseTarjeta = Vector3.one;
    private bool escalaBaseInicializada;
    private Coroutine animacionTurnoRoutine;
    private bool eraTurnoActual;
    private float vidaObjetivo = 1f;
    private float velocidadVida;
    private Outline marcadorSiguiente;
    private bool esSiguiente;
    private Color colorBaseRetrato = Color.white;
    private RectTransform rectRetrato;
    private Vector3 escalaBaseRetrato = Vector3.one;
    private Quaternion rotacionBaseTarjeta = Quaternion.identity;
    private float ultimaVidaDetectada = -1f;
    private Coroutine flashVidaRoutine;
    private Coroutine salidaDerrotadaRoutine;
    private CanvasGroup canvasGroupTarjeta;
    private const float MultiplicadorEscalaTurnoActual = 1.15f;
    public const float DuracionSalidaDerrotada = 0.32f;

    private void Awake()
    {
        InicializarEscalaBaseTarjeta();
        ActualizarReferencias();
        PrepararMarcadorSiguiente();
        canvasGroupTarjeta = GetComponent<CanvasGroup>();
        if (canvasGroupTarjeta == null)
        {
            canvasGroupTarjeta = gameObject.AddComponent<CanvasGroup>();
        }
        if (Retrato != null)
        {
            colorBaseRetrato = Retrato.color;
            rectRetrato = Retrato.rectTransform;
            escalaBaseRetrato = rectRetrato.localScale;
        }
        rotacionBaseTarjeta = transform.localRotation;
        if (Actual != null)
        {
            UIGlowPulse pulsoActual = Actual.GetComponent<UIGlowPulse>();
            if (pulsoActual != null)
            {
                pulsoActual.minAlpha = 0.35f;
                pulsoActual.maxAlpha = 1f;
                pulsoActual.pulseSpeed = 3.4f;
            }
        }
    }

    private void OnEnable()
    {
        ActualizarReferencias();
        SuscribirEventos();
        RefrescarVisuales();
    }

    private void OnDisable()
    {
        DesuscribirEventos();
        if (animacionTurnoRoutine != null)
        {
            StopCoroutine(animacionTurnoRoutine);
            animacionTurnoRoutine = null;
        }
        if (flashVidaRoutine != null)
        {
            StopCoroutine(flashVidaRoutine);
            flashVidaRoutine = null;
        }
        if (salidaDerrotadaRoutine != null)
        {
            StopCoroutine(salidaDerrotadaRoutine);
            salidaDerrotadaRoutine = null;
        }
        RestaurarTarjetaVisual();
        transform.localRotation = rotacionBaseTarjeta;
        transform.localScale = eraTurnoActual
            ? escalaBaseTarjeta * MultiplicadorEscalaTurnoActual
            : escalaBaseTarjeta;
    }

    void Update()
    {
        if (scUnidad == null || scUInfochar == null)
        {
            return;
        }

        ActualizarSeleccionado();
        ActualizarOscurecedor();
        ActualizarVidaSuavemente();
        ActualizarPulsoSiguiente();
    }

    private void ActualizarReferencias()
    {
        if (battleManager == null)
        {
            battleManager = BattleManager.Instance;
        }

        if (scUInfochar == null && battleManager != null)
        {
            scUInfochar = battleManager.scUIInfoChar;
        }
    }

    private void SuscribirEventos()
    {
        if (eventosRegistrados)
        {
            return;
        }

        Habilidad.OnUsarHabilidad += Habilidad_OnUsarHabilidad;
        if (battleManager != null)
        {
            battleManager.OnTurnoNuevo += BattleManager_OnTurnoNuevo;
        }

        eventosRegistrados = true;
    }

    private void DesuscribirEventos()
    {
        if (!eventosRegistrados)
        {
            return;
        }

        Habilidad.OnUsarHabilidad -= Habilidad_OnUsarHabilidad;
        if (battleManager != null)
        {
            battleManager.OnTurnoNuevo -= BattleManager_OnTurnoNuevo;
        }

        eventosRegistrados = false;
    }

    public void Configurar(Unidad unidad, int indiceOrden)
    {
        bool cambioUnidad = scUnidad != unidad;
        scUnidad = unidad;
        ordenIndex = indiceOrden;
        if (cambioUnidad)
        {
            eraTurnoActual = false;
            transform.localScale = escalaBaseTarjeta;
        }
        RestaurarTarjetaVisual();
        ActualizarReferencias();
        ActualizarInfo(true);
        RefrescarVisuales();
    }

    public void ActualizarColores()
    {
     /*   if (this == null || gameObject == null)
        {
            return;
        }

        var image = GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        GameObject marcadorSeleccion = null;
        if (transform != null && transform.childCount > 0)
        {
            marcadorSeleccion = transform.GetChild(0)?.gameObject;
        }

        var battleManager = BattleManager.Instance;
        if (battleManager == null)
        {
            return;
        }

        scUInfochar = battleManager.scUIInfoChar;
        if (scUInfochar == null)
        {
            return;
        }

        if (battleManager.unidadActiva == scUnidad)
        {
            image.color = new Color(0.95f, 0.95f, 0.75f);
            if (marcadorSeleccion != null)
            {
                marcadorSeleccion.SetActive(true);
            }
            return;
        }

        if (scUnidad?.CasillaPosicion != null)
        {
            image.color = scUnidad.CasillaPosicion.lado == 1
                ? new Color(0.4f, 0.1f, 0.1f)
                : new Color(0.2f, 0.2f, 0.7f);
        }

        if (marcadorSeleccion != null)
        {
            marcadorSeleccion.SetActive(false);
        }*/
    }

    public void ActualizarInfo(bool instantaneo = true)
    {
        if (scUnidad == null)
        {
            return;
        }

        if (BarraVida != null)
        {
            vidaObjetivo = scUnidad.mod_maxHP > 0 ? scUnidad.HP_actual / scUnidad.mod_maxHP : 0f;
            if (instantaneo)
            {
                BarraVida.value = vidaObjetivo;
                velocidadVida = 0f;
                ultimaVidaDetectada = vidaObjetivo;
            }
        }

        if (Retrato != null)
        {
            Retrato.sprite = scUnidad.uRetrato;
        }

        ActualizarColores();
    }

    private void Habilidad_OnUsarHabilidad(object sender, EventArgs empty)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        ActualizarInfo(false);
    }

     private void BattleManager_OnTurnoNuevo(object sender, EventArgs empty)
    {
        RefrescarVisuales();
        ActualizarColores();
    }

   public void MarcarTurnoActual()
    {
        bool mostrarActual = battleManager != null && battleManager.unidadActiva == scUnidad;
        InicializarEscalaBaseTarjeta();
        if (mostrarActual != eraTurnoActual)
        {
            if (animacionTurnoRoutine != null)
            {
                StopCoroutine(animacionTurnoRoutine);
            }
            animacionTurnoRoutine = StartCoroutine(AnimarCambioTurno(mostrarActual));
            eraTurnoActual = mostrarActual;
        }

        if (Actual != null)
        {
            Actual.SetActive(mostrarActual);
        }
    }

    private void InicializarEscalaBaseTarjeta()
    {
        if (escalaBaseInicializada)
        {
            return;
        }

        escalaBaseTarjeta = transform.localScale;
        escalaBaseInicializada = true;
    }

    private IEnumerator AnimarCambioTurno(bool mostrarActual)
    {
        Vector3 escalaInicial = transform.localScale;
        Vector3 escalaObjetivo = mostrarActual
            ? escalaBaseTarjeta * MultiplicadorEscalaTurnoActual
            : escalaBaseTarjeta;
        float duracion = mostrarActual ? 0.24f : 0.14f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float suavizado = Mathf.SmoothStep(0f, 1f, t);
            float rebote = mostrarActual ? Mathf.Sin(t * Mathf.PI) * 0.08f : 0f;
            transform.localScale = Vector3.LerpUnclamped(escalaInicial, escalaObjetivo, suavizado) * (1f + rebote);
            yield return null;
        }

        transform.localScale = escalaObjetivo;
        animacionTurnoRoutine = null;
    }

    private void ActualizarVidaSuavemente()
    {
        if (BarraVida == null || scUnidad == null)
        {
            return;
        }

        vidaObjetivo = scUnidad.mod_maxHP > 0 ? Mathf.Clamp01(scUnidad.HP_actual / scUnidad.mod_maxHP) : 0f;
        if (ultimaVidaDetectada >= 0f && Mathf.Abs(vidaObjetivo - ultimaVidaDetectada) > 0.001f)
        {
            ReproducirFlashVida(vidaObjetivo > ultimaVidaDetectada);
        }
        ultimaVidaDetectada = vidaObjetivo;
        if (Mathf.Abs(BarraVida.value - vidaObjetivo) < 0.001f)
        {
            BarraVida.value = vidaObjetivo;
            velocidadVida = 0f;
            return;
        }

        BarraVida.value = Mathf.SmoothDamp(
            BarraVida.value,
            vidaObjetivo,
            ref velocidadVida,
            0.16f,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
    }

    private void ReproducirFlashVida(bool esCuracion)
    {
        if (Retrato == null || salidaDerrotadaRoutine != null)
        {
            return;
        }

        if (flashVidaRoutine != null)
        {
            StopCoroutine(flashVidaRoutine);
        }
        flashVidaRoutine = StartCoroutine(FlashVida(esCuracion));
    }

    private IEnumerator FlashVida(bool esCuracion)
    {
        Color colorFlash = esCuracion
            ? new Color(0.35f, 1f, 0.48f, colorBaseRetrato.a)
            : new Color(1f, 0.24f, 0.18f, colorBaseRetrato.a);
        const float duracion = 0.28f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float intensidad = Mathf.Sin(t * Mathf.PI) * 0.92f;
            Retrato.color = Color.Lerp(colorBaseRetrato, colorFlash, intensidad);
            if (rectRetrato != null)
            {
                rectRetrato.localScale = escalaBaseRetrato * (1f + Mathf.Sin(t * Mathf.PI) * 0.12f);
            }
            yield return null;
        }

        Retrato.color = colorBaseRetrato;
        if (rectRetrato != null)
        {
            rectRetrato.localScale = escalaBaseRetrato;
        }
        flashVidaRoutine = null;
    }

    public void ReproducirSalidaDerrotada()
    {
        if (!isActiveAndEnabled || salidaDerrotadaRoutine != null)
        {
            return;
        }

        if (flashVidaRoutine != null)
        {
            StopCoroutine(flashVidaRoutine);
            flashVidaRoutine = null;
        }
        if (animacionTurnoRoutine != null)
        {
            StopCoroutine(animacionTurnoRoutine);
            animacionTurnoRoutine = null;
        }
        salidaDerrotadaRoutine = StartCoroutine(SalidaDerrotada());
    }

    private IEnumerator SalidaDerrotada()
    {
        Vector3 escalaInicial = transform.localScale;
        float inclinacion = ordenIndex % 2 == 0 ? -7f : 7f;
        float tiempo = 0f;

        while (tiempo < DuracionSalidaDerrotada)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / DuracionSalidaDerrotada));
            transform.localScale = Vector3.LerpUnclamped(escalaInicial, escalaBaseTarjeta * 0.45f, t);
            transform.localRotation = Quaternion.LerpUnclamped(
                rotacionBaseTarjeta,
                rotacionBaseTarjeta * Quaternion.Euler(0f, 0f, inclinacion),
                t);
            if (canvasGroupTarjeta != null)
            {
                canvasGroupTarjeta.alpha = 1f - t;
            }
            if (Retrato != null)
            {
                Retrato.color = Color.Lerp(colorBaseRetrato, new Color(0.15f, 0.15f, 0.15f, colorBaseRetrato.a), t);
            }
            yield return null;
        }

        salidaDerrotadaRoutine = null;
    }

    private void RestaurarTarjetaVisual()
    {
        if (canvasGroupTarjeta != null)
        {
            canvasGroupTarjeta.alpha = 1f;
        }
        if (Retrato != null)
        {
            Retrato.color = colorBaseRetrato;
        }
        if (rectRetrato != null)
        {
            rectRetrato.localScale = escalaBaseRetrato;
        }
        transform.localRotation = rotacionBaseTarjeta;
    }

    private void PrepararMarcadorSiguiente()
    {
        if (Retrato == null || marcadorSiguiente != null)
        {
            return;
        }

        marcadorSiguiente = Retrato.gameObject.AddComponent<Outline>();
        marcadorSiguiente.effectDistance = new Vector2(2f, -2f);
        marcadorSiguiente.useGraphicAlpha = true;
        marcadorSiguiente.enabled = false;
    }

    private void ActualizarMarcadorSiguiente()
    {
        if (battleManager == null || scUnidad == null)
        {
            esSiguiente = false;
        }
        else
        {
            int indiceActual = battleManager.lUnidadesTotal.IndexOf(battleManager.unidadActiva);
            esSiguiente = indiceActual >= 0 && ordenIndex == indiceActual + 1;
        }

        if (marcadorSiguiente != null)
        {
            marcadorSiguiente.enabled = esSiguiente;
        }
    }

    private void ActualizarPulsoSiguiente()
    {
        if (!esSiguiente || marcadorSiguiente == null)
        {
            return;
        }

        float pulso = 0.5f + Mathf.Sin(Time.unscaledTime * 3.2f) * 0.5f;
        marcadorSiguiente.effectColor = new Color(1f, 0.78f, 0.2f, Mathf.Lerp(0.32f, 0.72f, pulso));
    }

    private void ActualizarSeleccionado()
    {
        bool mostrarSeleccionado = scUInfochar.hayUnidadSeleccionadaParaInfo
            && scUInfochar.unidadFijadaActual != null
            && scUInfochar.unidadFijadaActual == scUnidad;

        if (seleccionado != null)
        {
            seleccionado.SetActive(mostrarSeleccionado);
        }
    }

    private void ActualizarOscurecedor()
    {
        if (battleManager == null || scUnidad == null)
        {
            return;
        }

        int indiceUnidad = ordenIndex;
        if (indiceUnidad < 0)
        {
            indiceUnidad = battleManager.lUnidadesTotal.IndexOf(scUnidad);
        }

        int indexTurno = battleManager.indexTurno - 1;
        bool mostrarOscurecedor = indiceUnidad >= 0 && indiceUnidad < indexTurno;
        if (oscurecedor != null)
        {
            oscurecedor.SetActive(mostrarOscurecedor);
        }
    }

    private void RefrescarVisuales()
    {
        ActualizarSeleccionado();
        MarcarTurnoActual();
        ActualizarMarcadorSiguiente();
        ActualizarOscurecedor();
    }

    public void MarcarUnidadRepresentada(int n) //1 es entra mouse, 0 es sale
    {
        if (scUnidad == null)
        {
            return;
        }

        ActualizarReferencias();
        if (scUInfochar == null)
        {
            return;
        }

        if(n == 1) //entra mouse
        {
          scUInfochar.MostrarHover(scUnidad);
          if (battleManager != null && battleManager.SeleccionandoObjetivo && scUnidad.CasillaPosicion != null)
          {
              scUnidad.CasillaPosicion.OnMouseOver();
          }
          else
          {
              TooltipBatalla.Instance?.HideTooltipSinAnim();
          }
        }
        else if(n == 0) //sale mouse
        {
          scUInfochar.LimpiarHover(scUnidad);
          if (battleManager != null && battleManager.SeleccionandoObjetivo && scUnidad.CasillaPosicion != null)
          {
              scUnidad.CasillaPosicion.OnMouseExit();
          }
        }
    }

    public void ClickMarcarUnidadRepresentada(int n)
    {
        if (scUnidad == null)
        {
            return;
        }

        ActualizarReferencias();
        if (battleManager != null && battleManager.SeleccionandoObjetivo)
        {
            scUnidad.OnMouseDown();
            return;
        }

        if (scUInfochar != null)
        {
            scUInfochar.ToggleFijado(scUnidad);
        }
    }

    private void OnDestroy()
    {
        DesuscribirEventos();
    }
}
