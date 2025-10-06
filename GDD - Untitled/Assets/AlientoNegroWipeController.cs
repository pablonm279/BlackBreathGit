using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlientoNegroWipeController : MonoBehaviour
{
    [Header("Destino del material")]
    public Graphic uiTarget;          // Image o RawImage (opcional)
    public Renderer worldTarget;      // Quad en mundo (opcional)
    public Slider sliderAlientoNegro; // UI slider (opcional)

    [Header("Animación")]
    public float duracion = 3f;
    [Range(0,1)] public float cutoffActual = 0f;

    Material _mat;
    static readonly int ID_Cutoff = Shader.PropertyToID("_Cutoff");

    void Awake()
    {
        if (uiTarget)
        {
            _mat = Instantiate(uiTarget.material); // instancia propia para no modificar el shared
            uiTarget.material = _mat;
        }
        else if (worldTarget)
        {
            _mat = worldTarget.material; // en mundo ya suele estar instanciado
        }
    }

    public void AvanzarAlientoNegro(int cant)
    {
        // Tu lógica de estado (0..20):
        float estado = CampaignManager.Instance.GetValorAlientoNegro(); // 0..20
        float destino = Mathf.Clamp01(estado / 20f);

        StopAllCoroutines();
        StartCoroutine(AnimarCutoff(destino));
    }

    IEnumerator AnimarCutoff(float destino)
    {
        float ini = cutoffActual;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duracion);
            cutoffActual = Mathf.Lerp(ini, destino, t);
            if (_mat) _mat.SetFloat(ID_Cutoff, cutoffActual);
            if (sliderAlientoNegro) sliderAlientoNegro.value = cutoffActual;
            yield return null;
        }
        cutoffActual = destino;
        if (_mat) _mat.SetFloat(ID_Cutoff, cutoffActual);
        if (sliderAlientoNegro) sliderAlientoNegro.value = cutoffActual;
    }
}
