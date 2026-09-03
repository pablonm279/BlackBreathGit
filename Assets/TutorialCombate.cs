using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCombate : MonoBehaviour
{
    private const string PasoSeleccionBallesta = "Paso3 - Accion 1";
    private const string PasoAtaqueBallesta = "Paso4 - Accion 2";
    private const string PasoMovimiento = "Paso6 - Accion 4";
    private const string PasoSeleccionDistraer = "Paso7 - Accion 5";
    private const string PasoNuevaRonda = "Paso8 - Vacío";

    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject[] pasosCombate;

    public bool tutorialCombateActivo;
    private bool primerCombateProcesado;
    [SerializeField]private int pasoActual = -1;
    private Coroutine coroutineForzarPaso;
    private RectTransform flechaHabilidad;
    private RectTransform botonHabilidadSenalada;
    private int pasoFlechaCache = -1;
    private readonly Dictionary<Selectable, bool> interactablesPrevios = new Dictionary<Selectable, bool>();



    private void Start()
    {
        OcultarTodosLosPasos();
    }

    private void OnEnable()
    {
        Habilidad.OnUsarHabilidad += AlUsarHabilidad;
    }

    private void LateUpdate()
    {
        if (EsPasoSeleccionBallesta())
        {
            PosicionarFlechaSobreHabilidad<TiroBallestaDeMano>();
            BloquearInteraccionesUIExceptoBallesta();
            return;
        }

        RestaurarInteraccionesUI();
        if (EsPasoSeleccionDistraer())
        {
            PosicionarFlechaSobreHabilidad<Distraer>();
        }
    }

    private void OnDisable()
    {
        Habilidad.OnUsarHabilidad -= AlUsarHabilidad;
        RestaurarInteraccionesUI();
    }

    private void AlUsarHabilidad(object sender, System.EventArgs e)
    {
        if (!EsPasoSeleccionDistraer() || !(sender is Distraer))
        {
            return;
        }

        PosicionarFlechaSobreHabilidad<Distraer>();
        if (flechaHabilidad != null)
        {
            flechaHabilidad.gameObject.SetActive(false);
        }
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

    public void IniciarCombateDesdePasoNombre(string nombrePaso, int pasoFallback, bool forzarInicio = false)
    {
        int paso = ObtenerIndicePasoPorNombre(nombrePaso);
        IniciarCombateDesdePaso(paso >= 0 ? paso : pasoFallback, forzarInicio);
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
        if (EsPasoSeleccionBallesta()) return;
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
        if (EsPasoSeleccionBallesta()) return;

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

    public bool EsPasoSeleccionBallesta()
    {
        return EsPasoActual(PasoSeleccionBallesta);
    }

    public bool EsPasoAtaqueBallesta()
    {
        return EsPasoActual(PasoAtaqueBallesta);
    }

    public bool EsPasoMovimiento()
    {
        return EsPasoActual(PasoMovimiento);
    }

    public bool EstaAntesDelPasoMovimiento()
    {
        return EstaAntesDelPaso(PasoMovimiento);
    }

    public bool EstaAntesDelPasoAtaqueBallesta()
    {
        return EstaAntesDelPaso(PasoAtaqueBallesta);
    }

    public bool EsPasoFinTurno()
    {
        return EsPasoActual(PasoSeleccionDistraer);
    }

    public bool EsPasoNuevaRonda()
    {
        return EsPasoActual(PasoNuevaRonda);
    }

    public bool PermiteHabilidadEnPasoActual(Habilidad habilidad)
    {
        return !EsPasoSeleccionBallesta() || habilidad is TiroBallestaDeMano;
    }

    private bool EsPasoSeleccionDistraer()
    {
        return EsPasoActual(PasoSeleccionDistraer);
    }

    private bool EsPasoActual(string nombrePaso)
    {
        return tutorialCombateActivo
            && IndicePasoValido(pasoActual)
            && pasosCombate[pasoActual] != null
            && pasosCombate[pasoActual].name == nombrePaso;
    }

    private bool EstaAntesDelPaso(string nombrePaso)
    {
        int indicePaso = ObtenerIndicePasoPorNombre(nombrePaso);
        return tutorialCombateActivo
            && IndicePasoValido(pasoActual)
            && indicePaso >= 0
            && pasoActual < indicePaso;
    }

    private bool TienePasosCombate()
    {
        return pasosCombate != null && pasosCombate.Length > 0;
    }

    private bool IndicePasoValido(int index)
    {
        return index >= 0 && index < pasosCombate.Length;
    }

    private int ObtenerIndicePasoPorNombre(string nombrePaso)
    {
        if (!TienePasosCombate() || string.IsNullOrEmpty(nombrePaso))
        {
            return -1;
        }

        for (int i = 0; i < pasosCombate.Length; i++)
        {
            GameObject paso = pasosCombate[i];
            if (paso != null && paso.name == nombrePaso)
            {
                return i;
            }
        }

        return -1;
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
        flechaHabilidad = null;
        botonHabilidadSenalada = null;
        pasoFlechaCache = -1;
        OcultarTodosLosPasos();
        MostrarPaso(pasoActual);
    }

    private void PosicionarFlechaSobreHabilidad<T>() where T : Habilidad
    {
        if (pasoFlechaCache != pasoActual)
        {
            flechaHabilidad = null;
            botonHabilidadSenalada = null;
            pasoFlechaCache = pasoActual;
        }

        if (flechaHabilidad == null)
        {
            flechaHabilidad = BuscarHijoPorNombre(pasosCombate[pasoActual].transform, "FLECHA") as RectTransform;
            if (flechaHabilidad != null)
            {
                Graphic[] graficosFlecha = flechaHabilidad.GetComponentsInChildren<Graphic>(true);
                for (int i = 0; i < graficosFlecha.Length; i++)
                {
                    graficosFlecha[i].raycastTarget = false;
                }
            }
        }

        if (botonHabilidadSenalada == null || !botonHabilidadSenalada.gameObject.activeInHierarchy)
        {
            BotonHabilidad[] botones = FindObjectsByType<BotonHabilidad>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < botones.Length; i++)
            {
                if (botones[i].HabilidadRepresentada is T)
                {
                    botonHabilidadSenalada = botones[i].transform as RectTransform;
                    break;
                }
            }
        }

        if (flechaHabilidad == null || botonHabilidadSenalada == null)
        {
            return;
        }

        RectTransform padreFlecha = flechaHabilidad.parent as RectTransform;
        if (padreFlecha == null)
        {
            return;
        }

        Canvas canvasBoton = botonHabilidadSenalada.GetComponentInParent<Canvas>();
        Camera camaraBoton = canvasBoton != null && canvasBoton.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvasBoton.worldCamera
            : null;
        Vector3 puntoSuperior = botonHabilidadSenalada.TransformPoint(
            new Vector3(botonHabilidadSenalada.rect.center.x, botonHabilidadSenalada.rect.yMax, 0f));
        Vector2 puntoPantalla = RectTransformUtility.WorldToScreenPoint(camaraBoton, puntoSuperior);

        Canvas canvasFlecha = flechaHabilidad.GetComponentInParent<Canvas>();
        Camera camaraFlecha = canvasFlecha != null && canvasFlecha.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvasFlecha.worldCamera
            : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            padreFlecha, puntoPantalla, camaraFlecha, out Vector2 puntoLocal))
        {
            float separacion = flechaHabilidad.rect.height * 0.5f + 10f;
            flechaHabilidad.anchoredPosition = puntoLocal + Vector2.up * separacion;
        }
    }

    private static Transform BuscarHijoPorNombre(Transform raiz, string nombre)
    {
        for (int i = 0; i < raiz.childCount; i++)
        {
            Transform hijo = raiz.GetChild(i);
            if (hijo.name.Contains(nombre))
            {
                return hijo;
            }

            Transform encontrado = BuscarHijoPorNombre(hijo, nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private void BloquearInteraccionesUIExceptoBallesta()
    {
        Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < selectables.Length; i++)
        {
            Selectable selectable = selectables[i];
            if (selectable == null)
            {
                continue;
            }

            if (EsParteDelBotonBallesta(selectable.transform))
            {
                if (interactablesPrevios.TryGetValue(selectable, out bool interactablePrevio))
                {
                    selectable.interactable = interactablePrevio;
                    interactablesPrevios.Remove(selectable);
                }
                continue;
            }

            if (!interactablesPrevios.ContainsKey(selectable))
            {
                interactablesPrevios.Add(selectable, selectable.interactable);
            }

            selectable.interactable = false;
        }
    }

    private bool EsParteDelBotonBallesta(Transform elemento)
    {
        return botonHabilidadSenalada != null
            && (elemento == botonHabilidadSenalada
                || elemento.IsChildOf(botonHabilidadSenalada)
                || botonHabilidadSenalada.IsChildOf(elemento));
    }

    private void RestaurarInteraccionesUI()
    {
        if (interactablesPrevios.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<Selectable, bool> estado in interactablesPrevios)
        {
            if (estado.Key != null)
            {
                estado.Key.interactable = estado.Value;
            }
        }

        interactablesPrevios.Clear();
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
