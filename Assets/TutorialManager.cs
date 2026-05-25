using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private const string TutorialTerminadoKey = "Tutorial_Terminado";
    private const int TutorialOrigenX = 0;
    private const int TutorialOrigenY = 0;
    private const int TutorialPrimerCombateX = 1;
    private const int TutorialPrimerCombateY = 3;
    private const int TutorialRecursosX = 2;
    private const int TutorialRecursosY = 2;
    private const int TutorialEliteX = 2;
    private const int TutorialEliteY = 4;
    private const int TutorialEventoX = 3;
    private const int TutorialEventoY = 2;
    private const int TutorialEmboscadaX = 4;
    private const int TutorialEmboscadaY = 2;
    private const int TutorialClaroX = 4;
    private const int TutorialClaroY = 3;
    private const int TutorialBatallaFinalX = 5;
    private const int TutorialBatallaFinalY = 4;

    public bool tutorialActivo = true;
    public int pasoActual = 0;

    public Nodo NodoPelea1;
    public Nodo Nodotut2;
    public Nodo Nodotut3;
    public Nodo Nodotut4;
    public Nodo Nodotut5;
    public Nodo Nodotut6;
    public Nodo Nodotut7;

    public GameObject[] pasosTutorial; // Array de objetos del tutorial
    private readonly HashSet<int> pasosUsadosPorEstablecer = new HashSet<int>();

    List<Nodo> ObtenerNodosTutorial()
    {
        ContenedorDeNodos contenedor = CampaignManager.Instance != null
            ? CampaignManager.Instance.scMapaManager?.scContenedordeNodos
            : null;

        ActualizarRutaMapaTutorial(contenedor);

        var nodos = new List<Nodo>();
        AgregarNodoSiExiste(nodos, ObtenerNodoMapaTutorial(contenedor, TutorialOrigenX, TutorialOrigenY, null));
        AgregarNodoSiExiste(nodos, NodoPelea1);
        AgregarNodoSiExiste(nodos, Nodotut3);
        AgregarNodoSiExiste(nodos, ObtenerNodoMapaTutorial(contenedor, TutorialEliteX, TutorialEliteY, null));
        AgregarNodoSiExiste(nodos, Nodotut4);
        AgregarNodoSiExiste(nodos, Nodotut5);
        AgregarNodoSiExiste(nodos, Nodotut6);
        AgregarNodoSiExiste(nodos, Nodotut7);

        return nodos;
    }

    void AgregarNodoSiExiste(List<Nodo> nodos, Nodo nodo)
    {
        if (nodo == null || nodos.Contains(nodo))
        {
            return;
        }

        nodos.Add(nodo);
    }

    Nodo ObtenerNodoMapaTutorial(ContenedorDeNodos contenedor, int x, int y, Nodo fallback)
    {
        if (contenedor != null)
        {
            Nodo nodo = contenedor.ObtenerNodoSegunXY(x, y);
            if (nodo != null)
            {
                return nodo;
            }
        }

        return fallback;
    }

    void ActualizarRutaMapaTutorial(ContenedorDeNodos contenedor)
    {
        Nodo primerCombate = ObtenerNodoMapaTutorial(contenedor, TutorialPrimerCombateX, TutorialPrimerCombateY, NodoPelea1);
        Nodo recursos = ObtenerNodoMapaTutorial(contenedor, TutorialRecursosX, TutorialRecursosY, Nodotut3);
        Nodo evento = ObtenerNodoMapaTutorial(contenedor, TutorialEventoX, TutorialEventoY, Nodotut4);
        Nodo emboscada = ObtenerNodoMapaTutorial(contenedor, TutorialEmboscadaX, TutorialEmboscadaY, Nodotut5);
        Nodo claro = ObtenerNodoMapaTutorial(contenedor, TutorialClaroX, TutorialClaroY, Nodotut6);
        Nodo batallaFinal = ObtenerNodoMapaTutorial(contenedor, TutorialBatallaFinalX, TutorialBatallaFinalY, Nodotut7);

        NodoPelea1 = primerCombate;
        Nodotut2 = primerCombate;
        Nodotut3 = recursos;
        Nodotut4 = evento;
        Nodotut5 = emboscada;
        Nodotut6 = claro;
        Nodotut7 = batallaFinal;
    }

    void Start()
    {
       
    }
    void LimpiarConexionesNodo(Nodo nodo)
    {
        if (nodo == null) return;

        nodo.LimpiarEstadosEspecialesTutorial();
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

        ActualizarRutaMapaTutorial(contenedor);

        Nodo origen = ObtenerNodoMapaTutorial(contenedor, TutorialOrigenX, TutorialOrigenY, null);
        if (origen == null) return;

        foreach (var nodo in ObtenerNodosTutorial())
        {
            nodo.gameObject.SetActive(true);
            LimpiarConexionesNodo(nodo);
        }

        if (NodoPelea1 != null)
        {
            origen.ConectarConNodo(NodoPelea1, false, false);
        }

        if (NodoPelea1 != null && Nodotut3 != null)
        {
            NodoPelea1.ConectarConNodo(Nodotut3, false, false);
        }

        Nodo elite = ObtenerNodoMapaTutorial(contenedor, TutorialEliteX, TutorialEliteY, null);
        if (NodoPelea1 != null && elite != null)
        {
            NodoPelea1.ConectarConNodo(elite, false, false);
        }

        MostrarEleccionInicialTutorial(elite);

        if (Nodotut3 != null && Nodotut4 != null)
        {
            Nodotut3.ConectarConNodo(Nodotut4, false, false);
        }

        if (Nodotut4 != null && Nodotut5 != null)
        {
            Nodotut4.ConectarConNodo(Nodotut5, false, false);
        }

        if (Nodotut4 != null && Nodotut6 != null)
        {
            Nodotut4.ConectarConNodo(Nodotut6, false, false);
        }

        if (Nodotut6 != null && Nodotut7 != null)
        {
            Nodotut6.ConectarConNodo(Nodotut7, false, false);
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

        ConfigurarMapaLinealTutorial();
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);
    }

    public void ConfigurarMapaLinealTutorial()
    {
        DesactivarOtrosNodosTuto();
        ConfigurarConexionesLinealesTuto();

        ConfigurarVisualNodoTutorial(NodoPelea1, 1);
        ConfigurarVisualNodoTutorial(Nodotut3, 5);
        ConfigurarVisualNodoTutorial(ObtenerNodoMapaTutorial(CampaignManager.Instance?.scMapaManager?.scContenedordeNodos, TutorialEliteX, TutorialEliteY, null), 8);
        ConfigurarVisualNodoTutorial(Nodotut4, 2);
        ConfigurarVisualNodoTutorial(Nodotut5, 11);
        ConfigurarVisualNodoTutorial(Nodotut6, 3);
        ConfigurarVisualNodoTutorial(Nodotut7, 1);
        ConfigurarClaroMisteriosoTutorial();

        ForzarEleccionInicialVisible();
    }

    public bool DebeForzarEventoDesaparicionesMisteriosas(Nodo nodo)
    {
        return CampaignManager.Instance != null
            && CampaignManager.Instance.DebeUsarConfiguracionTutorial()
            && nodo != null
            && nodo.posXNodo == TutorialEventoX
            && nodo.posYNodo == TutorialEventoY;
    }

    public bool EsClaroMisteriosoTutorial(Nodo nodo)
    {
        return CampaignManager.Instance != null
            && CampaignManager.Instance.DebeUsarConfiguracionTutorial()
            && nodo != null
            && nodo.posXNodo == TutorialClaroX
            && nodo.posYNodo == TutorialClaroY;
    }

    public bool EsBatallaFinalTutorial(Nodo nodo)
    {
        return CampaignManager.Instance != null
            && CampaignManager.Instance.DebeUsarConfiguracionTutorial()
            && nodo != null
            && nodo.posXNodo == TutorialBatallaFinalX
            && nodo.posYNodo == TutorialBatallaFinalY;
    }

    public void ConfigurarSoloMapaLinealTutorial()
    {
        pasosUsadosPorEstablecer.Clear();
        OcultarTodosLosPasos();
        ConfigurarMapaLinealTutorial();
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

    private void ConfigurarVisualNodoTutorial(Nodo nodo, int tipoNodo)
    {
        if (nodo == null) return;
        nodo.LimpiarEstadosEspecialesTutorial();
        nodo.tipoNodo = tipoNodo;
        nodo.ActivarNodoVisual(tipoNodo, false, true);
    }

    private void ConfigurarClaroMisteriosoTutorial()
    {
        if (Nodotut6 == null) return;
        Nodotut6.ForzarMisteriosoTutorial();
    }

    private void ForzarEleccionInicialVisible()
    {
        var contenedor = CampaignManager.Instance != null ? CampaignManager.Instance.scMapaManager?.scContenedordeNodos : null;
        Nodo elite = ObtenerNodoMapaTutorial(contenedor, TutorialEliteX, TutorialEliteY, null);

        RevelarNodoTutorial(Nodotut3);
        RevelarNodoTutorial(elite);
        MostrarEleccionInicialTutorial(elite);
    }

    private void RevelarNodoTutorial(Nodo nodo)
    {
        if (nodo == null) return;

        nodo.ForzarVisiblePorReveladoEspecial();
    }

    private void MostrarEleccionInicialTutorial(Nodo elite)
    {
        if (NodoPelea1 == null) return;

        if (Nodotut3 != null)
        {
            NodoPelea1.MostrarCaminoPorVisionHacia(Nodotut3);
        }

        if (elite != null)
        {
            NodoPelea1.MostrarCaminoPorVisionHacia(elite);
        }
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
