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



    private void Start()
    {
        OcultarTodosLosPasos();
    }

    /// <summary>
    /// Llama este método cuando se dispare el primer combate.
    /// </summary>
    public void IniciarPrimerCombate()
    {

        if (primerCombateProcesado) return;
        primerCombateProcesado = true;

        if (tutorialManager != null && tutorialManager.tutorialActivo)
        {
            ComenzarTutorialCombate();
        }
    }

    public void IniciarCombateDesdePaso(int paso)
    {
        if (tutorialManager != null && !tutorialManager.tutorialActivo) return;
        if (!TienePasosCombate()) return;

        tutorialCombateActivo = true;
        pasoActual = Mathf.Clamp(paso, 0, pasosCombate.Length - 1);
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);
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
        if (!TienePasosCombate()) return;

        tutorialCombateActivo = true;
        pasoActual = 0;
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);
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
            if (pasosCombate[i] == null) { continue; }
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

    private void MostrarPanelIdioma(GameObject paso)
    {
        if (paso == null) return;
        int childCount = paso.transform.childCount;
        if (childCount <= 0) return;

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
        int childIdioma = idioma == 2 ? 1 : 0;

        int totalPanelesIdioma = Mathf.Min(2, childCount);
        for (int i = 0; i < totalPanelesIdioma; i++)
        {
            paso.transform.GetChild(i).gameObject.SetActive(i == childIdioma);
        }

        if (childIdioma >= totalPanelesIdioma && totalPanelesIdioma > 0)
        {
            paso.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}


