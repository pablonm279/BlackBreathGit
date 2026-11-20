using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data.Common;

public class EnemigoGulekGul : Unidad
{
    private const string BuffMartillo = "Martillo Listo";
    private const string BuffFuria = "Furia de Gul";

    private UnidadPoseController poseControllerGulek;
    private bool poseMartilloLevantadoActiva;
    private Sprite poseIdleBaseGulek;
    private Sprite poseIdleMartilloGulek;
    private Sprite poseTemporal;
    private int versionPoseTemporal;
    private float proximoChequeoPose;

    void LateUpdate()
    {
        if (poseTemporal != null)
        {
            if (!PuedeMostrarPoseTemporal())
            {
                RestaurarPoseTemporal(versionPoseTemporal);
            }
            else if (poseControllerGulek != null && poseControllerGulek.targetImage != null
                && poseControllerGulek.targetImage.sprite != poseTemporal)
            {
                // El retorno de una aproximacion melee previa puede refrescar el sprite.
                poseControllerGulek.RefrescarPoseActual();
            }
        }

        // La pose solo cambia cuando cambia el buff: se revisa unas pocas veces por segundo
        // (los cambios conocidos refrescan la pose de inmediato desde RemoverBuffNombre/SoltarMartillo).
        if (Time.unscaledTime < proximoChequeoPose)
        {
            return;
        }

        proximoChequeoPose = Time.unscaledTime + 0.2f;
        ActualizarPoseMartillo();
    }

    public void RefrescarPoseMartillo()
    {
        ActualizarPoseMartillo(true);
    }

    private bool PuedeMostrarPoseTemporal()
    {
        return HP_actual > 0f && isActiveAndEnabled
            && BattleManager.Instance != null && BattleManager.Instance.unidadActiva == this;
    }

    public int MostrarPoseTemporal(Sprite sprite)
    {
        if (sprite == null || !PuedeMostrarPoseTemporal())
        {
            return 0;
        }

        ActualizarPoseMartillo();
        if (poseControllerGulek == null)
        {
            return 0;
        }

        poseTemporal = sprite;
        poseControllerGulek.EnterSkillPoseHold(sprite);
        return ++versionPoseTemporal;
    }

    public void RestaurarPoseTemporal(int version)
    {
        if (poseTemporal == null || version != versionPoseTemporal)
        {
            return;
        }

        poseTemporal = null;
        ActualizarPoseMartillo();
        if (poseControllerGulek != null)
        {
            poseControllerGulek.ExitPoseHold(HP_actual > 0f && gameObject.activeInHierarchy);
        }
    }

    public static float FactorTiempoHabilidad()
    {
        BattleManager batalla = BattleManager.Instance;
        return batalla != null && batalla.modoRapidoActivado
            ? GameSpeedSettings.CombatAiTimingFactor(batalla.NivelVelocidadCombateEfectivo)
            : 1f;
    }

    private void ActualizarPoseMartillo(bool forzar = false)
    {
        if (poseControllerGulek == null)
        {
            poseControllerGulek = GetComponent<UnidadPoseController>();
        }

        if (poseControllerGulek == null)
        {
            return;
        }

        if (poseIdleBaseGulek == null)
        {
            poseIdleBaseGulek = poseControllerGulek.poseIdle;
        }

        if (poseIdleMartilloGulek == null)
        {
            poseIdleMartilloGulek = poseControllerGulek.poseHabilidad;
        }

        bool debeMantenerPoseMartilloLevantado = TieneBuffNombre(BuffMartillo);
        if (!forzar && poseMartilloLevantadoActiva == debeMantenerPoseMartilloLevantado)
        {
            return;
        }

        poseMartilloLevantadoActiva = debeMantenerPoseMartilloLevantado;
        Sprite idleActual = poseMartilloLevantadoActiva && poseIdleMartilloGulek != null
            ? poseIdleMartilloGulek
            : poseIdleBaseGulek;

        poseControllerGulek.poseIdle = idleActual;
        poseControllerGulek.poseHabilidad = idleActual;
        if (HP_actual > 0f)
        {
            poseControllerGulek.RefrescarPoseActual();
        }
    }

    // Cualquier remocion del buff (golpe con el martillo, dispel, caida) refresca la pose al instante.
    public override void RemoverBuffNombre(string nombreBuff)
    {
        base.RemoverBuffNombre(nombreBuff);
        if (nombreBuff == BuffMartillo)
        {
            ActualizarPoseMartillo(true);
        }
    }

    /// <summary>
    /// Unica via por la que Gulek suelta el martillo (por daño o por voluntad).
    /// Resetea el ciclo para que vuelva a levantarlo y, si sigue vivo, se enfurece.
    /// </summary>
    public void SoltarMartillo(bool porDanio)
    {
        if (!TieneBuffNombre(BuffMartillo))
        {
            return;
        }

        RemoverBuffNombre(BuffMartillo);

        IALevantarMartillo levantar = GetComponent<IALevantarMartillo>();
        if (levantar != null)
        {
            levantar.hActualCooldown = 0; // puede volver a levantarlo en su proximo turno
        }

        string clave = porDanio
            ? "Gulek Gul pierde el buff 'Martillo Listo' tras recibir daño y no podrá utilizarlo."
            : "Gulek Gul pierde el control y deja caer el martillo.";
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(clave));

        if (HP_actual > 0f)
        {
            AplicarFuria();
        }
    }

    private void AplicarFuria()
    {
        if (TieneBuffNombre(BuffFuria))
        {
            return;
        }

        // La duracion corre al terminar SU turno: 2 cubre el turno de volver a levantar y el del golpe.
        Buff buff = new Buff();
        buff.buffNombre = BuffFuria;
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque += 3;
        buff.cantDanioPorcentaje += 20;
        buff.cantDefensa -= 1;
        buff.AplicarBuff(this);
        ComponentCopier.CopyComponent(buff, gameObject);

        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("Gul se enfurece al perder el martillo."));
        GulekGulVfx.AuraFuria(transform, 2.6f);
        GulekGulVfx.SacudirCamara(0.45f, 0.22f);
    }

    public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0, bool ignorarEscudo = false)
    {
        float hpAntes = HP_actual;
        bool teniaMartillo = TieneBuffNombre(BuffMartillo);
        bool eraInvulnerable = estado_invulnerable != 0;

        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos, ignorarEscudo);

        if (teniaMartillo && !eraInvulnerable)
        {
            EvaluarCaidaMartilloPorDanio(danio, hpAntes, delayEfectos);
        }
    }

    // base.RecibirDanio es asincrono (proyectiles, delays, redirecciones): se espera a que la vida
    // realmente baje antes de tirar la salvacion, para no soltar el martillo por un golpe que no entro.
    private async void EvaluarCaidaMartilloPorDanio(float danio, float hpAntes, int delayEfectos)
    {
        int esperado = 0;
        int maximo = 1500 + Mathf.Max(0, delayEfectos);
        while (this != null && HP_actual >= hpAntes && esperado < maximo)
        {
            await BattleManager.DelayCombateAsync(50);
            esperado += 50;
        }

        if (this == null || HP_actual <= 0f || HP_actual >= hpAntes || !TieneBuffNombre(BuffMartillo))
        {
            return;
        }

        if (TiradaSalvacion(1, (danio / 2) + 2)) //TRUE = no se salva
        {
            SoltarMartillo(true);
        }
    }

    public override bool TiradaSalvacion(int tipoSalvacion, float dificultadHabilidada, bool porValourGlobal = false) //TRUE no se salva FALSE se salva (xd)
    {
        bool resultado = false;
        int intentos = tipoSalvacion == 3 ? 2 : 1;

        for (int i = 0; i < intentos; i++)
        {
            resultado = base.TiradaSalvacion(tipoSalvacion, dificultadHabilidada, porValourGlobal);

            if (!resultado)
            {
                return resultado;
            }

            if (tipoSalvacion == 3 && i == 0)
            {
                BattleManager.Instance.EscribirLog(uNombre + TRADU.i.Traducir(" obtiene un intento adicional de Tirada de Salvación."));
            }
        }

        if (tipoSalvacion == 3 && resultado)
        {
            SoltarMartillo(false); //Si pierde una tirada de voluntad, deja el martillo
        }

        return resultado;
    }
}
