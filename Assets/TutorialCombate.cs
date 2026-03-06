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
        if (pasosCombate == null || pasosCombate.Length == 0) return;

        tutorialCombateActivo = true;
        pasoActual = Mathf.Clamp(paso, 0, pasosCombate.Length - 1);
        MostrarPaso(pasoActual);
    }

    public void SiguientePasoCombate()
    {

        if (!tutorialCombateActivo || pasosCombate == null || pasosCombate.Length == 0) return;
        if (pasoActual < 0 || pasoActual >= pasosCombate.Length) return;

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
        if (!tutorialCombateActivo || pasosCombate == null || pasosCombate.Length == 0) return;
        if (pasoActual <= 0)
        {
            pasoActual = 0;
            MostrarPaso(pasoActual);
            return;
        }

        if (pasoActual < pasosCombate.Length && pasosCombate[pasoActual] != null)
        {
            pasosCombate[pasoActual].SetActive(false);
        }

        pasoActual--;
        MostrarPaso(pasoActual);
    }
    public void omitirTutorialCombate()
    {

        FinalizarTutorialCombate();
    }
    private void ComenzarTutorialCombate()
    {
        if (pasosCombate == null || pasosCombate.Length == 0) return;

        tutorialCombateActivo = true;
        pasoActual = 0;
        MostrarPaso(pasoActual);
    }

    private void FinalizarTutorialCombate()
    {
        tutorialCombateActivo = false;
        pasoActual = pasosCombate.Length - 1;

        gameObject.SetActive(false);

    }

    private void MostrarPaso(int index)
    {
        if (pasosCombate == null || pasosCombate.Length == 0) return;
        if (index < 0 || index >= pasosCombate.Length) return;
        for (int i = 0; i < pasosCombate.Length; i++)
        {
            if (pasosCombate[i] == null) { continue; }
            pasosCombate[i].SetActive(i == index);
        }
        GameObject tutoActual = pasosCombate[index];
        if (TRADU.i.nIdioma == 1) //Esp
        {
            tutoActual.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (TRADU.i.nIdioma == 2) //Ing
        {
            tutoActual.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    
    public int ObtenerPasoActual()
    {
        return pasoActual;
    }

    
}


