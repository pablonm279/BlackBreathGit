using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCombate : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject[] pasosCombate;

    public bool tutorialCombateActivo;
    private bool primerCombateProcesado;
    [SerializeField]private int pasoActual = -1;
    private Coroutine coroutineForzarPaso;



    private void Start()
    {
        OcultarTodosLosPasos();
    }

    /// <summary>
    /// Llama este método cuando se dispare el primer combate.
    /// </summary>
    public void IniciarPrimerCombate()
    {
        ReactivarTutorialSiHaceFalta();

        if (primerCombateProcesado) return;
        primerCombateProcesado = true;

        if (tutorialManager != null && tutorialManager.tutorialActivo)
        {
            IniciarCombateDesdePaso(0);
        }
    }

    public void IniciarCombateDesdePaso(int paso, bool forzarInicio = false)
    {
        ReactivarTutorialSiHaceFalta();

        if (!forzarInicio && tutorialManager != null && !tutorialManager.tutorialActivo) return;
        if (!TienePasosCombate()) return;

        AplicarPasoTutorial(paso);
        ReforzarPasoTutorialSiguienteFrame();
    }

    public void SiguientePasoCombate()
    {

        if (!tutorialCombateActivo || !TienePasosCombate()) return;
        if (!IndicePasoValido(pasoActual)) return;

        if (pasosCombate[pasoActual] != null)
        {
            pasosCombate[pasoActual].SetActive(false);
        }
        pasoActual++;

        if (pasoActual < pasosCombate.Length)
        {
            MostrarPaso(pasoActual);
        }
        else
        {
            FinalizarTutorialCombate();
        }
    }

    public void PasoAnteriorCombate()
    {
        if (!tutorialCombateActivo || !TienePasosCombate()) return;
        if (pasoActual <= 0)
        {
            pasoActual = 0;
            MostrarPaso(pasoActual);
            return;
        }

        SetPasoActivo(pasoActual, false);

        pasoActual--;
        MostrarPaso(pasoActual);
    }
    public void omitirTutorialCombate()
    {

        FinalizarTutorialCombate();
    }
    private void ComenzarTutorialCombate()
    {
        ReactivarTutorialSiHaceFalta();

        if (!TienePasosCombate()) return;

        AplicarPasoTutorial(0);
        ReforzarPasoTutorialSiguienteFrame();
    }

    private void FinalizarTutorialCombate()
    {
        tutorialCombateActivo = false;
        pasoActual = -1;
        OcultarTodosLosPasos();

        gameObject.SetActive(false);

    }

  private void MostrarPaso(int index)
{
    if (!TienePasosCombate()) return;
    if (!IndicePasoValido(index)) return;

    for (int i = 0; i < pasosCombate.Length; i++)
    {
        if (pasosCombate[i] == null) continue;
        pasosCombate[i].SetActive(i == index);
    }

    GameObject tutoActual = pasosCombate[index];
    if (tutoActual == null) return;

    MostrarPanelIdioma(tutoActual);
}
    
    public int ObtenerPasoActual()
    {
        return pasoActual;
    }

    private bool TienePasosCombate()
    {
        return pasosCombate != null && pasosCombate.Length > 0;
    }

    private bool IndicePasoValido(int index)
    {
        return index >= 0 && index < pasosCombate.Length;
    }

    private void SetPasoActivo(int index, bool activo)
    {
        if (!IndicePasoValido(index)) return;
        if (pasosCombate[index] == null) return;
        pasosCombate[index].SetActive(activo);
    }

    private void OcultarTodosLosPasos()
    {
        if (!TienePasosCombate()) return;
        for (int i = 0; i < pasosCombate.Length; i++)
        {
            if (pasosCombate[i] == null) continue;
            pasosCombate[i].SetActive(false);
        }
    }

    private void ReactivarTutorialSiHaceFalta()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    private void AplicarPasoTutorial(int paso)
    {
        tutorialCombateActivo = true;
        pasoActual = Mathf.Clamp(paso, 0, pasosCombate.Length - 1);
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);
    }

    private void ReforzarPasoTutorialSiguienteFrame()
    {
        if (coroutineForzarPaso != null)
        {
            StopCoroutine(coroutineForzarPaso);
        }

        coroutineForzarPaso = StartCoroutine(ReforzarPasoTutorialRutina());
    }

    private IEnumerator ReforzarPasoTutorialRutina()
    {
        yield return null;

        if (!tutorialCombateActivo || !IndicePasoValido(pasoActual))
        {
            coroutineForzarPaso = null;
            yield break;
        }

        SetPasoActivo(pasoActual, true);
        MostrarPanelIdioma(pasosCombate[pasoActual]);
        coroutineForzarPaso = null;
    }

  private void MostrarPanelIdioma(GameObject paso)
{
    if (paso == null) return;

    Transform t = paso.transform;
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;

    int childIdioma = idioma - 1;
    int indiceTexto = 0;
    bool algunoActivo = false;

    for (int i = 0; i < t.childCount; i++)
    {
        Transform child = t.GetChild(i);

        if (!child.name.Contains("TEXTO"))
            continue;

        bool activar = indiceTexto == childIdioma;
        child.gameObject.SetActive(activar);

        if (activar)
            algunoActivo = true;

        indiceTexto++;
    }

    // Fallback: si no existe ese idioma, activa el primer TEXTO
    if (!algunoActivo)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);

            if (!child.name.Contains("TEXTO"))
                continue;

            child.gameObject.SetActive(true);
            break;
        }
    }
}
}

