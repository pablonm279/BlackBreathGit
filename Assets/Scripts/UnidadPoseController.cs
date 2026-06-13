using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AparienciaAlternativaUnidad
{
    public string nombre;
    public Sprite retrato;
    public Sprite poseIdle;
    public Sprite poseMover;
    public Sprite poseAtacar;
    public Sprite poseHabilidad;
    public Sprite poseRecibirDanio;
    public Sprite poseTurnoActivo;

    public bool TieneContenido()
    {
        return retrato != null || poseIdle != null || poseMover != null || poseAtacar != null || poseHabilidad != null || poseRecibirDanio != null || poseTurnoActivo != null;
    }
}

// Controlador generico de poses por sprite para unidades (jugador o IA)
public class UnidadPoseController : MonoBehaviour
{
    public enum TipoPoseActual
    {
        Idle,
        Mover,
        Atacar,
        Habilidad,
        RecibirDanio
    }

    [Header("Destino")]
    public Image targetImage; // Si es null, se usa Unidad.uImage

    [Header("Poses")]
    public Sprite poseIdle;
    public Sprite poseMover;
    public Sprite poseAtacar;
    public Sprite poseHabilidad; // Para habilidades no hostiles
    public Sprite poseRecibirDanio;
    public Sprite poseTurnoActivo;

    [Header("Tiempos Pose Transitoria (seg)")]
    public float duracionPoseAtacar = 1.0f;
    public float duracionPoseDanyo = 0.4f;
    public float duracionPoseHabilidad = 1.0f;

    [Header("Timing Melee Centralizado")]
    [Range(0f, 1f)] public float meleeFraccionImpacto = 0.35f;
    [Min(0f)] public float meleePreImpactoFallback = 0.22f;
    [Min(0f)] public float meleePreImpactoMin = 0.08f;
    [Min(0f)] public float meleePreImpactoMax = 0.42f;
    [Min(0f)] public float meleePostImpacto = 0.16f;
    [Min(0f)] public float meleePostImpactoMin = 0.05f;
    [Min(0f)] public float meleePostImpactoMax = 0.30f;

    Unidad unidad;
    Coroutine revertCoroutine;
    bool mantenerPoseHabilidad = false;
    bool mantenerPoseAtaque = false;
    int contadorObjetivoHostilActivo = 0;
    bool objetivoHostilTemporalActivo = false;
    TipoPoseActual poseActual = TipoPoseActual.Idle;
    Sprite poseIdleBaseConfigurada;
    Sprite poseMoverBaseConfigurada;
    Sprite poseAtacarBaseConfigurada;
    Sprite poseHabilidadBaseConfigurada;
    Sprite poseRecibirDanioBaseConfigurada;
    Sprite poseTurnoActivoBaseConfigurada;
    Coroutine objetivoHostilTemporalCoroutine;

    void Awake()
    {
        unidad = GetComponent<Unidad>();
        if (targetImage == null && unidad != null)
        {
            targetImage = unidad.uImage;
        }

        // Si no se definio una pose idle, usa la sprite actual asignada
        if (poseIdle == null && targetImage != null)
        {
            poseIdle = targetImage.sprite;
        }

        poseIdleBaseConfigurada = poseIdle;
        poseMoverBaseConfigurada = poseMover;
        poseAtacarBaseConfigurada = poseAtacar;
        poseHabilidadBaseConfigurada = poseHabilidad;
        poseRecibirDanioBaseConfigurada = poseRecibirDanio;
        poseTurnoActivoBaseConfigurada = poseTurnoActivo;
    }

    bool DebeAplicar()
    {
        // Aplica tanto a unidades de jugador como IA mientras haya imagen de destino
        return targetImage != null;
    }

    void SetSprite(Sprite sp)
    {
        if (!DebeAplicar() || sp == null)
        {
            return;
        }

        targetImage.sprite = sp;
    }

    bool EstaUnidadEnTurnoActivo()
    {
        return unidad != null && BattleManager.Instance != null && BattleManager.Instance.unidadActiva == unidad;
    }

    Sprite ResolverPoseIdle()
    {
        if (poseTurnoActivo != null && (EstaUnidadEnTurnoActivo() || contadorObjetivoHostilActivo > 0))
        {
            return poseTurnoActivo;
        }

        return poseIdle;
    }

    public void EnterPoseObjetivoHostil()
    {
        contadorObjetivoHostilActivo++;
        RefrescarPoseActual();
    }

    public void ExitPoseObjetivoHostil()
    {
        contadorObjetivoHostilActivo = Mathf.Max(0, contadorObjetivoHostilActivo - 1);
        RefrescarPoseActual();
    }

    public void PlayPoseObjetivoHostilTemporal(float duracion)
    {
        if (!objetivoHostilTemporalActivo)
        {
            objetivoHostilTemporalActivo = true;
            EnterPoseObjetivoHostil();
        }
        else
        {
            RefrescarPoseActual();
        }

        if (objetivoHostilTemporalCoroutine != null)
        {
            StopCoroutine(objetivoHostilTemporalCoroutine);
        }

        objetivoHostilTemporalCoroutine = StartCoroutine(LiberarPoseObjetivoHostilTemporal(duracion));
    }

    IEnumerator LiberarPoseObjetivoHostilTemporal(float duracion)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, duracion));
        objetivoHostilTemporalCoroutine = null;
        if (objetivoHostilTemporalActivo)
        {
            objetivoHostilTemporalActivo = false;
            ExitPoseObjetivoHostil();
        }
    }

    public void SetIdle()
    {
        if (mantenerPoseHabilidad || mantenerPoseAtaque)
        {
            return;
        }

        CancelarReversionAutomatica();
        poseActual = TipoPoseActual.Idle;
        SetSprite(ResolverPoseIdle());
    }

    public void OnStartMove()
    {
        CancelarReversionAutomatica();
        poseActual = TipoPoseActual.Mover;
        SetSprite(poseMover);
    }

    public void OnStopMove()
    {
        CancelarReversionAutomatica();
        poseActual = TipoPoseActual.Idle;
        SetSprite(ResolverPoseIdle());
    }

    public void PlayAttackPose()
    {
        if (mantenerPoseHabilidad || mantenerPoseAtaque)
        {
            return; // Mantener pose de habilidad tiene prioridad
        }

        poseActual = TipoPoseActual.Atacar;
        SetSprite(poseAtacar);
        IniciarReversion(duracionPoseAtacar);
    }

    public void PlaySkillPose()
    {
        if (mantenerPoseHabilidad)
        {
            CancelarReversionAutomatica();

            poseActual = TipoPoseActual.Habilidad;
            SetSprite(poseHabilidad);
            return;
        }

        poseActual = TipoPoseActual.Habilidad;
        SetSprite(poseHabilidad);
        IniciarReversion(duracionPoseHabilidad);
    }

    public void PlayDamagePose()
    {
        if (poseRecibirDanio == null)
        {
            if (poseTurnoActivo != null)
            {
                PlayPoseObjetivoHostilTemporal(duracionPoseDanyo);
            }
            return;
        }

        CancelarReversionAutomatica();
        poseActual = TipoPoseActual.RecibirDanio;
        SetSprite(poseRecibirDanio);
        IniciarReversion(duracionPoseDanyo);
    }

    // Mantiene la pose de habilidad fija hasta que se libere manualmente
    public void EnterSkillPoseHold()
    {
        mantenerPoseHabilidad = true;
        CancelarReversionAutomatica();

        poseActual = TipoPoseActual.Habilidad;
        SetSprite(poseHabilidad);
    }

    // Sale del modo de pose fija y vuelve a Idle
    public void ExitPoseHold()
    {
        mantenerPoseHabilidad = false;
        CancelarReversionAutomatica();

        SetIdle();
    }

    public void EnterAttackPoseHold()
    {
        mantenerPoseAtaque = true;
        CancelarReversionAutomatica();

        poseActual = TipoPoseActual.Atacar;
        SetSprite(poseAtacar);
    }

    public void ExitAttackPoseHold(bool restaurarIdle = true)
    {
        mantenerPoseAtaque = false;
        CancelarReversionAutomatica();

        if (restaurarIdle)
        {
            SetIdle();
        }
    }

    public void ConfigurarPoses(Sprite idle, Sprite mover, Sprite atacar, Sprite habilidad, Sprite recibirDanio = null, Sprite turnoActivo = null, bool refrescarPoseActual = true)
    {
        poseIdle = idle;
        poseMover = mover;
        poseAtacar = atacar;
        poseHabilidad = habilidad;
        poseRecibirDanio = recibirDanio;
        poseTurnoActivo = turnoActivo;

        if (refrescarPoseActual)
        {
            RefrescarPoseActual();
        }
    }

    public Sprite ObtenerPoseIdleBase()
    {
        return poseIdleBaseConfigurada;
    }

    public Sprite ObtenerPoseMoverBase()
    {
        return poseMoverBaseConfigurada;
    }

    public Sprite ObtenerPoseAtacarBase()
    {
        return poseAtacarBaseConfigurada;
    }

    public Sprite ObtenerPoseHabilidadBase()
    {
        return poseHabilidadBaseConfigurada;
    }

    public Sprite ObtenerPoseRecibirDanioBase()
    {
        return poseRecibirDanioBaseConfigurada;
    }

    public Sprite ObtenerPoseTurnoActivoBase()
    {
        return poseTurnoActivoBaseConfigurada;
    }

    public void RestaurarPosesBase(bool refrescarPoseActual = true)
    {
        poseIdle = poseIdleBaseConfigurada;
        poseMover = poseMoverBaseConfigurada;
        poseAtacar = poseAtacarBaseConfigurada;
        poseHabilidad = poseHabilidadBaseConfigurada;
        poseRecibirDanio = poseRecibirDanioBaseConfigurada;
        poseTurnoActivo = poseTurnoActivoBaseConfigurada;

        if (refrescarPoseActual)
        {
            RefrescarPoseActual();
        }
    }

    public void RefrescarPoseActual()
    {
        if (mantenerPoseHabilidad)
        {
            poseActual = TipoPoseActual.Habilidad;
        }
        else if (mantenerPoseAtaque)
        {
            poseActual = TipoPoseActual.Atacar;
        }

        switch (poseActual)
        {
            case TipoPoseActual.Mover:
                SetSprite(poseMover != null ? poseMover : ResolverPoseIdle());
                break;
            case TipoPoseActual.Atacar:
                SetSprite(poseAtacar != null ? poseAtacar : ResolverPoseIdle());
                break;
            case TipoPoseActual.Habilidad:
                SetSprite(poseHabilidad != null ? poseHabilidad : ResolverPoseIdle());
                break;
            case TipoPoseActual.RecibirDanio:
                SetSprite(poseRecibirDanio != null ? poseRecibirDanio : ResolverPoseIdle());
                break;
            default:
                SetSprite(ResolverPoseIdle());
                break;
        }
    }

    void IniciarReversion(float delay)
    {
        if (revertCoroutine != null)
        {
            StopCoroutine(revertCoroutine);
        }

        revertCoroutine = StartCoroutine(RevertirTras(delay));
    }

    void CancelarReversionAutomatica()
    {
        if (revertCoroutine != null)
        {
            StopCoroutine(revertCoroutine);
            revertCoroutine = null;
        }
    }

    IEnumerator RevertirTras(float seg)
    {
        yield return new WaitForSeconds(seg);
        SetIdle();
        revertCoroutine = null;
    }
}
