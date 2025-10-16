using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public bool tutorialActivo = true;
    public int pasoActual = 0;

    public GameObject[] pasosTutorial; // Array de objetos del tutorial

    void Start()
    {
        if (tutorialActivo && pasosTutorial.Length > 0)
        {
            MostrarPaso(pasoActual);
        }
    }

    public void SiguientePaso()
    {
        if (!tutorialActivo) return;

        pasosTutorial[pasoActual].SetActive(false); // Oculta el paso actual
        pasoActual++;

        if (pasoActual < pasosTutorial.Length)
        {
            MostrarPaso(pasoActual);
        }
        else
        {
            tutorialActivo = false; // Finaliza el tutorial
        }
    }

    private HashSet<int> pasosUsadosPorEstablecer = new HashSet<int>();

    public void establecerPasoEspecifico(int x)
    {

        if (!tutorialActivo) return;
        if (x < 0 || x > pasosTutorial.Length) return;
        if (pasosUsadosPorEstablecer.Contains(x)) return;

        pasosUsadosPorEstablecer.Add(x);

        pasosTutorial[pasoActual].SetActive(false);
        pasoActual = x;
        MostrarPaso(pasoActual);
    }

    public void cerrarPasoEspecifico(int x)
    {
        if (!tutorialActivo) return;
        if (x < 0 || x > pasosTutorial.Length) return;
        if (pasoActual != x) return;

        pasosTutorial[pasoActual].SetActive(false);


    }



    public void anteriorPaso()
    {

        if (!tutorialActivo) return;

        // Evitar ir más atrás del primer paso
        if (pasoActual <= 0)
        {
            print(333);
            pasoActual = 0;
            MostrarPaso(pasoActual);
            return;
        }

        // Ocultar el paso actual antes de retroceder
        pasosTutorial[pasoActual].SetActive(false);

        // Retroceder un paso
        pasoActual--;

        // Mostrar el nuevo paso actual
        pasosTutorial[pasoActual].SetActive(true);
        MostrarPaso(pasoActual);
    }
    void MostrarPaso(int index)
    {
        if (pasosTutorial == null || pasosTutorial.Length == 0) return;
        if (index < 0 || index >= pasosTutorial.Length) return;

        pasosTutorial[index].SetActive(true); // Muestra el paso actual
        GameObject tutoActual = pasosTutorial[index];

        if (TRADU.i.nIdioma == 1) //Esp
        {  
            tutoActual.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (TRADU.i.nIdioma == 2) //Ing
        {
            if (tutoActual.transform.GetChild(1) != null)
            { tutoActual.transform.GetChild(1).gameObject.SetActive(true); }
        }

    }

    public void OmitirTutorial()
    {
        tutorialActivo = false;
        foreach (var paso in pasosTutorial)
        {
            paso.SetActive(false); // Oculta todos los pasos
        }
    }
    
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
