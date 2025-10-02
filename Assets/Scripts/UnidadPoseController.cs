using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Controlador genérico de poses por sprite para unidades NO IA
public class UnidadPoseController : MonoBehaviour
{
    [Header("Destino")]
    public Image targetImage; // Si es null, se usa Unidad.uImage

    [Header("Poses")]
    public Sprite poseIdle;
    public Sprite poseMover;
    public Sprite poseAtacar;
  //  public Sprite poseDanyo;
    public Sprite poseHabilidad; // Para habilidades no hostiles

    [Header("Tiempos Pose Transitoria (seg)")]
    public float duracionPoseAtacar = 0.5f;
    public float duracionPoseDanyo = 0.4f;
    public float duracionPoseHabilidad = 0.5f;

    Unidad unidad;
    Coroutine revertCoroutine;
    bool mantenerPoseHabilidad = false;

    void Awake()
    {
        unidad = GetComponent<Unidad>();
        if (targetImage == null && unidad != null)
        {
            targetImage = unidad.uImage;
        }
        // Si no se definió una pose idle, usa la sprite actual asignada
        if (poseIdle == null && targetImage != null)
        {
            poseIdle = targetImage.sprite;
        }
    }

    bool DebeAplicar()
    {
        // Solo aplica a unidades NO IA (por ahora)
        if (GetComponent<IAUnidad>() != null) return false;
        if (targetImage == null) return false;
        return true;
    }

    void SetSprite(Sprite sp)
    {
        if (!DebeAplicar()) return;
        if (sp != null) targetImage.sprite = sp;
    }

    public void SetIdle()
    {
        if (mantenerPoseHabilidad) return;
        SetSprite(poseIdle);
    }

    public void OnStartMove()
    {
        SetSprite(poseMover);
    }

    public void OnStopMove()
    {
        SetSprite(poseIdle);
    }

    public void PlayAttackPose()
    {
        if (mantenerPoseHabilidad) return; // Mantener pose de habilidad tiene prioridad
        SetSprite(poseAtacar);
        IniciarReversion(duracionPoseAtacar);
    }

    public void PlaySkillPose()
    {
        if (mantenerPoseHabilidad)
        {
            if (revertCoroutine != null) { StopCoroutine(revertCoroutine); revertCoroutine = null; }
            SetSprite(poseHabilidad);
            return;
        }
        SetSprite(poseHabilidad);
        IniciarReversion(duracionPoseHabilidad);
    }

    // Mantiene la pose de habilidad fija hasta que se libere manualmente
    public void EnterSkillPoseHold()
    {
        mantenerPoseHabilidad = true;
        if (revertCoroutine != null) { StopCoroutine(revertCoroutine); revertCoroutine = null; }
        SetSprite(poseHabilidad);
    }

    // Sale del modo de pose fija y vuelve a Idle
    public void ExitPoseHold()
    {
        mantenerPoseHabilidad = false;
        if (revertCoroutine != null) { StopCoroutine(revertCoroutine); revertCoroutine = null; }
        SetIdle();
    }

    void IniciarReversion(float delay)
    {
        if (revertCoroutine != null) StopCoroutine(revertCoroutine);
        revertCoroutine = StartCoroutine(RevertirTras(delay));
    }

    IEnumerator RevertirTras(float seg)
    {
        yield return new WaitForSeconds(seg);
        SetIdle();
        revertCoroutine = null;
    }
}
