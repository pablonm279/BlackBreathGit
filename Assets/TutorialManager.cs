using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private const string TutorialTerminadoKey = "Tutorial_Terminado";

    public bool tutorialActivo = true;
    public int pasoActual = 0;

    public Nodo NodoPelea1;
    public Nodo Nodotut2;
    public Nodo Nodotut3;
    public Nodo Nodotut4;
    public Nodo Nodotut5;
    public Nodo Nodotut6;

    public GameObject[] pasosTutorial; // Array de objetos del tutorial
    private readonly HashSet<int> pasosUsadosPorEstablecer = new HashSet<int>();

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
        if (!tutorialActivo || !TienePasosTutorial()) return;

        pasosUsadosPorEstablecer.Clear();
        pasoActual = Mathf.Clamp(pasoActual, 0, pasosTutorial.Length - 1);

        DesactivarOtrosNodosTuto();
        ConfigurarConexionesLinealesTuto();
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);

        ConfigurarVisualNodoTutorial(Nodotut2, 1, 2);
        ConfigurarVisualNodoTutorial(Nodotut3, 5, 3);
        ConfigurarVisualNodoTutorial(Nodotut4, 14, 4);
        ConfigurarVisualNodoTutorial(Nodotut5, 3, 5);
        ConfigurarVisualNodoTutorial(Nodotut6, 8, 6);
    }

    public void SiguientePaso()
    {
        if (!tutorialActivo || !TienePasosTutorial()) return;
        if (!IndicePasoValido(pasoActual)) pasoActual = 0;

        SetPasoActivo(pasoActual, false); // Oculta el paso actual
        pasoActual++;

        if (pasoActual < pasosTutorial.Length)
        {
            MostrarPaso(pasoActual);
        }
        else
        {
            FinalizarTutorial();
        }
    }

    public void establecerPasoEspecifico(int x)
    {

        if (!tutorialActivo || !TienePasosTutorial()) return;
        if (!IndicePasoValido(x)) return;
        if (pasosUsadosPorEstablecer.Contains(x)) return;

        pasosUsadosPorEstablecer.Add(x);

        SetPasoActivo(pasoActual, false);
        pasoActual = x;
        MostrarPaso(pasoActual);
    }

    public void cerrarPasoEspecifico(int x)
    {
        if (!tutorialActivo || !TienePasosTutorial()) return;
        if (!IndicePasoValido(x)) return;
        if (pasoActual != x) return;

        SetPasoActivo(pasoActual, false);


    }



    public void anteriorPaso()
    {

        if (!tutorialActivo || !TienePasosTutorial()) return;
        if (!IndicePasoValido(pasoActual)) pasoActual = 0;

        // Evitar ir más atrás del primer paso
        if (pasoActual <= 0)
        {
            pasoActual = 0;
            MostrarPaso(pasoActual);
            return;
        }

        // Ocultar el paso actual antes de retroceder
        SetPasoActivo(pasoActual, false);

        // Retroceder un paso
        pasoActual--;

        // Mostrar el nuevo paso actual
        MostrarPaso(pasoActual);
    }

    void MostrarPaso(int index)
    {
        if (!TienePasosTutorial()) return;
        if (!IndicePasoValido(index)) return;
        if (pasosTutorial[index] == null) return;

        OcultarTodosLosPasos();
        pasosTutorial[index].SetActive(true); // Muestra el paso actual
        GameObject tutoActual = pasosTutorial[index];

        MostrarPanelIdioma(tutoActual);

    }

    public void OmitirTutorial()
    {
        FinalizarTutorial();
    }
    
    public void SalirDelJuego()
    {
        Application.Quit();
    }

    private bool TienePasosTutorial()
    {
        return pasosTutorial != null && pasosTutorial.Length > 0;
    }

    private bool IndicePasoValido(int index)
    {
        return index >= 0 && index < pasosTutorial.Length;
    }

    private void SetPasoActivo(int index, bool activo)
    {
        if (!IndicePasoValido(index)) return;
        GameObject paso = pasosTutorial[index];
        if (paso == null) return;
        paso.SetActive(activo);
    }

    private void OcultarTodosLosPasos()
    {
        if (!TienePasosTutorial()) return;
        for (int i = 0; i < pasosTutorial.Length; i++)
        {
            if (pasosTutorial[i] == null) continue;
            pasosTutorial[i].SetActive(false);
        }
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

    private void ConfigurarVisualNodoTutorial(Nodo nodo, int tipoNodo, int visualId)
    {
        if (nodo == null) return;
        nodo.tipoNodo = tipoNodo;
        nodo.ActivarNodoVisual(visualId, false, true);
    }

    private void FinalizarTutorial()
    {
        tutorialActivo = false;
        pasosUsadosPorEstablecer.Clear();
        OcultarTodosLosPasos();
        PlayerPrefs.SetInt(TutorialTerminadoKey, 1);
        PlayerPrefs.Save();
    }
}


