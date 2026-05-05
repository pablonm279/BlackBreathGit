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
    private const float MultiplicadorEscalaTurnoActual = 1.15f;

    private void Awake()
    {
        InicializarEscalaBaseTarjeta();
        ActualizarReferencias();
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
    }

    void Update()
    {
        if (scUnidad == null || scUInfochar == null)
        {
            return;
        }

        ActualizarSeleccionado();
        ActualizarOscurecedor();
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
        scUnidad = unidad;
        ordenIndex = indiceOrden;
        ActualizarReferencias();
        ActualizarInfo();
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

    public void ActualizarInfo()
    {
        if (scUnidad == null)
        {
            return;
        }

        if (BarraVida != null)
        {
            BarraVida.value = scUnidad.mod_maxHP > 0 ? scUnidad.HP_actual / scUnidad.mod_maxHP : 0f;
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

        ActualizarInfo();
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
        transform.localScale = mostrarActual
            ? escalaBaseTarjeta * MultiplicadorEscalaTurnoActual
            : escalaBaseTarjeta;

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
