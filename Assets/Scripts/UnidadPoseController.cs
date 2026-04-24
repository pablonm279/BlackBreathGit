using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Controlador generico de poses por sprite para unidades (jugador o IA)
public class UnidadPoseController : MonoBehaviour
{
    public enum TipoPoseActual
    {
        Idle,
        Mover,
        Atacar,
        Habilidad
    }

    [Header("Destino")]
    public Image targetImage; // Si es null, se usa Unidad.uImage

    [Header("Poses")]
    public Sprite poseIdle;
    public Sprite poseMover;
    public Sprite poseAtacar;
    // public Sprite poseDanyo;
    public Sprite poseHabilidad; // Para habilidades no hostiles

    [Header("Tiempos Pose Transitoria (seg)")]
    public float duracionPoseAtacar = 1.0f;
    public float duracionPoseDanyo = 1.0f;
    public float duracionPoseHabilidad = 1.0f;

    Unidad unidad;
    Coroutine revertCoroutine;
    bool mantenerPoseHabilidad = false;
    bool mantenerPoseAtaque = false;
    TipoPoseActual poseActual = TipoPoseActual.Idle;

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

    public void SetIdle()
    {
        if (mantenerPoseHabilidad || mantenerPoseAtaque)
        {
            return;
        }

        CancelarReversionAutomatica();
        poseActual = TipoPoseActual.Idle;
        SetSprite(poseIdle);
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
        SetSprite(poseIdle);
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

    public void ConfigurarPoses(Sprite idle, Sprite mover, Sprite atacar, Sprite habilidad, bool refrescarPoseActual = true)
    {
        poseIdle = idle;
        poseMover = mover;
        poseAtacar = atacar;
        poseHabilidad = habilidad;

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
                SetSprite(poseMover != null ? poseMover : poseIdle);
                break;
            case TipoPoseActual.Atacar:
                SetSprite(poseAtacar != null ? poseAtacar : poseIdle);
                break;
            case TipoPoseActual.Habilidad:
                SetSprite(poseHabilidad != null ? poseHabilidad : poseIdle);
                break;
            default:
                SetSprite(poseIdle);
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
