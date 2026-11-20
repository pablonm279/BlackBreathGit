using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data.Common;
using UnityEngine.UI;
using System.Threading.Tasks;

public class EnemigoUnidadBrujaKaleTav : Unidad
{

    public Sprite imagenConCuervo;
    public Sprite imagenSinCuervo;

    private UnidadPoseController poseControllerBruja;
    private Sprite poseIdleOriginal;
    private bool cuervoAusente;

    /// <summary>True mientras el cuervo esta fuera (volando, picando o volviendo).</summary>
    public bool CuervoOcupado => cuervoAusente;

    // Muestra u oculta el cuervo del hombro de la bruja. Se hace por la pose idle para que otras
    // poses/refrescos no lo vuelvan a mostrar mientras el cuervo esta fuera.
    private void PonerCuervoAusente(bool ausente)
    {
        if (poseControllerBruja == null)
        {
            poseControllerBruja = GetComponent<UnidadPoseController>();
        }

        if (poseControllerBruja != null && poseIdleOriginal == null)
        {
            poseIdleOriginal = poseControllerBruja.poseIdle;
        }

        Sprite conCuervo = poseIdleOriginal != null ? poseIdleOriginal : imagenConCuervo;
        Sprite nuevo = ausente ? imagenSinCuervo : conCuervo;
        if (nuevo == null)
        {
            return;
        }

        Sprite mostrandoAhora = uImage != null ? uImage.sprite : null;
        bool estabaEnIdle = mostrandoAhora == (ausente ? conCuervo : imagenSinCuervo);

        if (poseControllerBruja != null)
        {
            poseControllerBruja.poseIdle = nuevo;
            poseControllerBruja.RefrescarPoseActual();
        }

        if (uImage != null && estabaEnIdle)
        {
            uImage.sprite = nuevo;
        }
    }

    private Vector3 ObtenerPuntoCuervoBruja()
    {
        float cas = GulekGulVfx.TamanoCasilla();
        Vector3 centro = uImage != null ? uImage.transform.position : transform.position;
        return centro + new Vector3(0f, cas * 0.5f, 0f);
    }

    /// <summary>
    /// Secuencia legible del cuervo: rotulo, el cuervo sale del hombro, vuela en arco hasta la cabeza del objetivo,
    /// pica (el efecto se aplica en el primer picotazo) y vuelve. alImpactar se invoca una sola vez.
    /// </summary>
    public async Task AnimarAtaqueCuervoAsync(Unidad objetivo, Action alImpactar, bool versionCorta = false)
    {
        if (cuervoAusente || objetivo == null || HP_actual <= 0f)
        {
            return;
        }

        cuervoAusente = true;
        BrujaCuervo cuervo = null;
        bool impactado = false;
        // El ataque activo usa la misma secuencia tranquila de la reaccion, apenas mas ajustada (~1,5 s)
        // para entrar en el margen de tiempo del IA (1,9 s).
        float f = DracoHieloVfx.FactorTiempo() * (versionCorta ? 0.8f : 1f);
        int picotazos = 2;
        float cas = GulekGulVfx.TamanoCasilla();

        try
        {
            // 1) Anticipacion: se anuncia la accion y la bruja "senala" al objetivo.
            MostrarRotuloHabilidadIA(TRADU.i.Traducir("Ataque de Cuervo"), new Color(0.78f, 0.7f, 1f, 1f));
            await BattleManager.DelayCombateAsync(Mathf.RoundToInt(350 * f));

            if (HP_actual <= 0f || objetivo == null)
            {
                return;
            }

            // 2) El cuervo sale del hombro (la bruja pasa a mostrarse sin cuervo).
            Vector3 hombro = ObtenerPuntoCuervoBruja();
            PonerCuervoAusente(true);
            BrujaCuervoVfx.Plumas(hombro, 6, 1f);
            cuervo = BrujaCuervoVfx.Crear(hombro);

            // 3) Vuelo en arco hasta la cabeza del objetivo.
            Vector3 cabeza = (objetivo.uImage != null ? objetivo.uImage.transform.position : objetivo.transform.position)
                + new Vector3(0f, cas * 0.45f, 0f);
            await cuervo.VolarAAsync(cabeza, 0.6f * f, cas * 0.9f);

            // 4) Dos picotazos; el efecto cae en el primero.
            for (int i = 0; i < picotazos; i++)
            {
                if (cuervo == null) { break; }

                Task picotazo = cuervo.PicarAsync(0.14f * f);
                await BattleManager.DelayCombateAsync(Mathf.RoundToInt(70 * f)); // el pico toca a mitad del cabeceo

                if (!impactado)
                {
                    impactado = true;
                    BrujaCuervoVfx.Plumas(cabeza, 8, 1.2f);
                    GulekGulVfx.SacudirCamara(0.15f, 0.1f);
                    if (objetivo != null && objetivo.HP_actual > 0f)
                    {
                        alImpactar?.Invoke();
                    }
                }
                else
                {
                    BrujaCuervoVfx.Plumas(cabeza, 4, 0.8f);
                }

                await picotazo;
                await BattleManager.DelayCombateAsync(Mathf.RoundToInt(60 * f));
            }

            // 5) Regresa al hombro y la bruja vuelve a mostrar el cuervo.
            if (cuervo != null)
            {
                await cuervo.VolarAAsync(ObtenerPuntoCuervoBruja(), 0.5f * f, cas * 0.6f);
                BrujaCuervoVfx.Plumas(ObtenerPuntoCuervoBruja(), 4, 0.7f);
            }
        }
        finally
        {
            if (cuervo != null) { cuervo.Quitar(); }
            if (this != null) { PonerCuervoAusente(false); }
            cuervoAusente = false;

            // Si el ataque se cancelo antes de llegar, el efecto no debe perderse en silencio: no se aplica.
        }
    }

}
