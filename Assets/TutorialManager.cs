using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public bool tutorialActivo = true;
    public int pasoActual = 0;

    public Nodo NodoPelea1;
    public Nodo Nodotut2;
    public Nodo Nodotut3;
    public Nodo Nodotut4;
    public Nodo Nodotut5;
    public Nodo Nodotut6;

    public GameObject[] pasosTutorial; // Array de objetos del tutorial

    List<Nodo> ObtenerNodosTutorial()
    {
        var nodos = new List<Nodo>
        {
            NodoPelea1,
            Nodotut2,
            Nodotut3,
            Nodotut4,
            Nodotut5,
            Nodotut6
        };

        nodos.RemoveAll(n => n == null);
        return nodos;
    }

    void Start()
    {
       
    }
    void LimpiarConexionesNodo(Nodo nodo)
    {
        if (nodo == null) return;

        nodo.DestinosPosibles.Clear();
        nodo.cantidadConexiones = 0;

        var destruir = new List<GameObject>();
        foreach (Transform child in nodo.transform)
        {
            if (child.name.Contains("LineaCaminos"))
            {
                destruir.Add(child.gameObject);
            }
        }

        foreach (var go in destruir)
        {
            Destroy(go);
        }
    }

    public void ConfigurarConexionesLinealesTuto()
    {
        var contenedor = CampaignManager.Instance != null ? CampaignManager.Instance.scMapaManager?.scContenedordeNodos : null;
        if (contenedor == null) return;

        var nodosOrdenados = ObtenerNodosTutorial();
        if (nodosOrdenados.Count == 0) return;

        foreach (var nodo in nodosOrdenados)
        {
            nodo.gameObject.SetActive(true);
            LimpiarConexionesNodo(nodo);
        }

        for (int i = 0; i < nodosOrdenados.Count - 1; i++)
        {
            var origen = nodosOrdenados[i];
            var destino = nodosOrdenados[i + 1];
            origen.ConectarConNodo(destino, false, false);
        }
    }

    public void DesactivarOtrosNodosTuto()
    {
        var contenedor = CampaignManager.Instance != null ? CampaignManager.Instance.scMapaManager?.scContenedordeNodos : null;
        if (contenedor == null) return;

        contenedor.RecolectarNodos();

        var nodosPermitidos = new HashSet<Nodo>(ObtenerNodosTutorial());

        foreach (var nodo in contenedor.listTodosNodos)
        {
            if (nodo == null) continue;
            if (nodosPermitidos.Contains(nodo)) continue;

            nodo.gameObject.SetActive(false);
        }
    }

    public void ComenzarTutorial()
    {
        if (tutorialActivo && pasosTutorial.Length > 0)
        {
              DesactivarOtrosNodosTuto();
            ConfigurarConexionesLinealesTuto();
            MostrarPaso(pasoActual);

         
            Nodotut2.tipoNodo = 1;
            Nodotut2.ActivarNodoVisual(2, false, true);

            Nodotut3.tipoNodo = 5;
            Nodotut3.ActivarNodoVisual(3, false, true);

            Nodotut4.tipoNodo = 14;
            Nodotut4.ActivarNodoVisual(4, false, true);

            Nodotut5.tipoNodo = 3;
            Nodotut5.ActivarNodoVisual(5, false, true);

            Nodotut6.tipoNodo = 8;
            Nodotut6.ActivarNodoVisual(6, false, true);
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
