using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanterCampaignDirector : MonoBehaviour
{
    private struct EstadoVida
    {
        public float actual;
        public float maxima;

        public EstadoVida(Personaje personaje)
        {
            actual = personaje != null ? personaje.fVidaActual : 0f;
            maxima = personaje != null ? personaje.fVidaMaxima : 0f;
        }
    }

    private static BanterCampaignDirector instance;

    private readonly Dictionary<string, string> ultimaLineaPorCombinacion =
        new Dictionary<string, string>();
    private readonly Dictionary<Personaje, EstadoVida> vidaAnterior =
        new Dictionary<Personaje, EstadoVida>();
    private readonly List<Personaje> personajesTemporales = new List<Personaje>();

    private CampaignManager campaignManager;
    private Nodo nodoIdle;
    private float segundosIdle;
    private bool idleEmitido;
    private bool pocosSuministros;
    private bool sobrecarga;
    private bool cansancio;
    private bool viajandoAnteriormente;
    private float distanciaAlientoAnterior;
    private float proximaRevisionEstado;
    private Coroutine viajePendiente;
    private Coroutine descansoPendiente;
    private Personaje ultimoHablante;

    public static void Instalar(CampaignManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (instance != null)
        {
            instance.campaignManager = manager;
            return;
        }

        BanterCampaignDirector director = manager.GetComponent<BanterCampaignDirector>();
        if (director == null)
        {
            director = manager.gameObject.AddComponent<BanterCampaignDirector>();
        }

        instance = director;
        director.Inicializar(manager);
    }

    public static void Finalizar()
    {
        if (instance == null)
        {
            return;
        }

        BanterCampaignDirector director = instance;
        instance = null;
        Destroy(director);
    }

    public static void NotificarNodoRevelado(Nodo nodo, bool porAtajoSubterraneo = false)
    {
        instance?.ProcesarNodoRevelado(nodo, porAtajoSubterraneo);
    }

    public static void NotificarDescansoIniciado()
    {
        instance?.IniciarBanterDescanso();
    }

    public static void NotificarLlegadaAsentamiento()
    {
        instance?.ProcesarLlegadaAsentamiento();
    }

    private void Inicializar(CampaignManager manager)
    {
        campaignManager = manager;
        nodoIdle = ObtenerNodoActual();
        segundosIdle = 0f;
        idleEmitido = false;
        viajandoAnteriormente = manager.MoviendoCaravana;
        distanciaAlientoAnterior = manager.GetDistanciaAlientoACaravana();
        InicializarLineaBase();
    }

    private void InicializarLineaBase()
    {
        pocosSuministros = HayPocosSuministros();
        sobrecarga = HaySobrecarga();
        cansancio = HayCansancio();
        vidaAnterior.Clear();

        List<Personaje> personajes = ObtenerPersonajesDisponibles();
        for (int i = 0; i < personajes.Count; i++)
        {
            Personaje personaje = personajes[i];
            vidaAnterior[personaje] = new EstadoVida(personaje);
        }
    }

    private void Update()
    {
        if (campaignManager == null)
        {
            return;
        }

        if (CampaniaNoDisponible())
        {
            ReiniciarIdle();
            return;
        }

        ActualizarViaje();
        ActualizarIdle();

        if (Time.unscaledTime < proximaRevisionEstado)
        {
            return;
        }

        proximaRevisionEstado = Time.unscaledTime + 0.2f;
        ActualizarDisparadoresDeEstado();
        ActualizarCuracionesCompletas();
    }

    private void ActualizarViaje()
    {
        bool viajando = campaignManager.MoviendoCaravana;
        if (viajando && !viajandoAnteriormente)
        {
            if (viajePendiente != null)
            {
                StopCoroutine(viajePendiente);
            }
            viajePendiente = StartCoroutine(EmitirBanterViajeTrasDemora());
        }
        else if (!viajando && viajandoAnteriormente && viajePendiente != null)
        {
            StopCoroutine(viajePendiente);
            viajePendiente = null;
        }

        viajandoAnteriormente = viajando;
    }

    private IEnumerator EmitirBanterViajeTrasDemora()
    {
        yield return new WaitForSecondsRealtime(2f);
        viajePendiente = null;

        if (campaignManager == null || !campaignManager.MoviendoCaravana)
        {
            yield break;
        }

        if (Random.value >= 0.7f)
        {
            yield break;
        }

        Personaje hablante = ElegirPersonajeAleatorio();
        if (hablante == null)
        {
            yield break;
        }

        int esperanza = campaignManager.GetEsperanzaActual();
        if (esperanza > 80)
        {
            Emitir(hablante, BanterCampaniaDisparador.ViajeEsperanzaAlta, 0);
        }
        else if (esperanza < 30)
        {
            Emitir(hablante, BanterCampaniaDisparador.ViajeEsperanzaBaja, 0);
        }
        else
        {
            EmitirActividad(hablante);
        }
    }

    private void ActualizarIdle()
    {
        Nodo nodoActual = ObtenerNodoActual();
        if (campaignManager.MoviendoCaravana || nodoActual == null)
        {
            nodoIdle = nodoActual;
            ReiniciarIdle();
            return;
        }

        if (nodoActual != nodoIdle)
        {
            nodoIdle = nodoActual;
            ReiniciarIdle();
        }

        if (idleEmitido)
        {
            return;
        }

        segundosIdle += Time.unscaledDeltaTime;
        if (segundosIdle >= 40f)
        {
            idleEmitido = EmitirAleatorio(BanterCampaniaDisparador.IdleNodo, 0);
        }
    }

    private void ReiniciarIdle()
    {
        segundosIdle = 0f;
        idleEmitido = false;
    }

    private void ActualizarDisparadoresDeEstado()
    {
        float distanciaAlientoActual = campaignManager.GetDistanciaAlientoACaravana();
        if (distanciaAlientoAnterior > 0f && distanciaAlientoActual <= 0f)
        {
            EmitirAleatorio(BanterCampaniaDisparador.AlientoNegroDistanciaCero, 3);
        }
        distanciaAlientoAnterior = distanciaAlientoActual;

        bool pocosAhora = HayPocosSuministros();
        if (pocosAhora && !pocosSuministros)
        {
            EmitirAleatorio(BanterCampaniaDisparador.PocosSuministros, 1);
        }
        pocosSuministros = pocosAhora;

        bool sobrecargaAhora = HaySobrecarga();
        if (sobrecargaAhora && !sobrecarga)
        {
            EmitirAleatorio(BanterCampaniaDisparador.Sobrecarga, 1);
        }
        sobrecarga = sobrecargaAhora;

        bool cansancioAhora = HayCansancio();
        if (cansancioAhora && !cansancio)
        {
            EmitirAleatorio(BanterCampaniaDisparador.Cansancio, 1);
        }
        cansancio = cansancioAhora;
    }

    private void ActualizarCuracionesCompletas()
    {
        List<Personaje> disponibles = ObtenerPersonajesDisponibles();
        personajesTemporales.Clear();

        for (int i = 0; i < disponibles.Count; i++)
        {
            Personaje personaje = disponibles[i];
            EstadoVida actual = new EstadoVida(personaje);
            if (vidaAnterior.TryGetValue(personaje, out EstadoVida anterior))
            {
                bool estabaHerido = anterior.maxima > 0f && anterior.actual < anterior.maxima;
                bool estaCompleto = actual.maxima > 0f && actual.actual >= actual.maxima;
                if (estabaHerido && estaCompleto)
                {
                    Emitir(personaje, BanterCampaniaDisparador.CuradoCompleto, 1);
                }
            }

            vidaAnterior[personaje] = actual;
        }

        foreach (KeyValuePair<Personaje, EstadoVida> estado in vidaAnterior)
        {
            if (estado.Key == null || !disponibles.Contains(estado.Key))
            {
                personajesTemporales.Add(estado.Key);
            }
        }

        for (int i = 0; i < personajesTemporales.Count; i++)
        {
            vidaAnterior.Remove(personajesTemporales[i]);
        }
    }

    private void ProcesarNodoRevelado(Nodo nodo, bool porAtajoSubterraneo)
    {
        if (nodo == null
            || campaignManager == null
            || !nodo.EstaReveladoParaExploradores()
            || CampaniaNoDisponible())
        {
            return;
        }

        if (porAtajoSubterraneo)
        {
            EmitirGenerico(BanterCampaniaDisparador.AtajoSubterraneoRevelado, 2);
            return;
        }

        if (nodo.tipoNodo == 4)
        {
            EmitirAleatorio(BanterCampaniaDisparador.AsentamientoRevelado, 1);
            return;
        }

        Nodo actual = ObtenerNodoActual();
        CaminoConexion conexion = actual != null ? actual.ObtenerConexionHacia(nodo) : null;
        bool esAdyacente = conexion != null && !conexion.EsAtajoSubterraneo;
        if (!esAdyacente)
        {
            return;
        }

        if (nodo.tipoNodo == 11)
        {
            EmitirAleatorio(BanterCampaniaDisparador.EmboscadaRevelada, 2);
        }
        else if (nodo.tipoNodo == 14)
        {
            Personaje purificadora = ElegirPersonajeAleatorio(3);
            if (purificadora != null)
            {
                Emitir(purificadora, BanterCampaniaDisparador.SantuarioPurificadoraRevelado, 2);
            }
        }
    }

    private void ProcesarLlegadaAsentamiento()
    {
        if (campaignManager == null || CampaniaNoDisponible())
        {
            return;
        }

        EmitirAleatorio(BanterCampaniaDisparador.LlegadaAsentamiento, 2);
    }

    private bool CampaniaNoDisponible()
    {
        return campaignManager.IntroCampaniaActivaOPendiente
            || (campaignManager.scAdministradorEscenas != null
                && campaignManager.scAdministradorEscenas.escenaActual == 1);
    }

    private void IniciarBanterDescanso()
    {
        if (campaignManager == null)
        {
            return;
        }

        if (viajePendiente != null)
        {
            StopCoroutine(viajePendiente);
            viajePendiente = null;
        }
        if (descansoPendiente != null)
        {
            StopCoroutine(descansoPendiente);
        }

        BanterBattleUI.CancelarCampania(true);
        descansoPendiente = StartCoroutine(EmitirBanterDescansoTrasDemora());
    }

    private IEnumerator EmitirBanterDescansoTrasDemora()
    {
        yield return new WaitForSecondsRealtime(2f);
        descansoPendiente = null;

        List<Personaje> personajes = ObtenerPersonajesDisponibles();
        if (personajes.Count == 0)
        {
            yield break;
        }

        int primerIndice = ElegirIndiceEvitando(personajes, ultimoHablante);
        Personaje primero = personajes[primerIndice];
        BanterLineaCampaniaLocal primeraLinea = ElegirLineaClase(
            primero.IDClase,
            BanterCampaniaDisparador.Descanso);
        if (primeraLinea == null)
        {
            yield break;
        }

        Personaje segundo = null;
        BanterLineaCampaniaLocal segundaLinea = null;
        if (personajes.Count > 1)
        {
            int segundoIndice = Random.Range(0, personajes.Count - 1);
            if (segundoIndice >= primerIndice)
            {
                segundoIndice++;
            }

            segundo = personajes[segundoIndice];
            segundaLinea = ElegirLineaClase(segundo.IDClase, BanterCampaniaDisparador.Descanso);
        }

        bool emitido = BanterBattleUI.EmitirCampaniaDoble(
            primero,
            primeraLinea.ObtenerTextoActual(),
            segundo,
            segundaLinea != null ? segundaLinea.ObtenerTextoActual() : null);
        if (emitido)
        {
            ultimoHablante = segundo != null ? segundo : primero;
        }
    }

    private bool EmitirAleatorio(BanterCampaniaDisparador disparador, int prioridad)
    {
        Personaje personaje = ElegirPersonajeAleatorio();
        return personaje != null && Emitir(personaje, disparador, prioridad);
    }

    private bool EmitirGenerico(BanterCampaniaDisparador disparador, int prioridad)
    {
        Personaje personaje = ElegirPersonajeAleatorio();
        BanterLineaCampaniaLocal linea = ElegirLinea(
            BanterContenidoCampania.ObtenerLineas(0, disparador),
            "generico:" + disparador);
        bool emitido = personaje != null
            && linea != null
            && BanterBattleUI.EmitirCampania(
                personaje,
                linea.ObtenerTextoActual(),
                3.2f,
                prioridad);
        if (emitido)
        {
            ultimoHablante = personaje;
        }
        return emitido;
    }

    private bool Emitir(
        Personaje personaje,
        BanterCampaniaDisparador disparador,
        int prioridad)
    {
        if (personaje == null)
        {
            return false;
        }

        BanterLineaCampaniaLocal linea = ElegirLineaClase(personaje.IDClase, disparador);
        bool emitido = linea != null
            && BanterBattleUI.EmitirCampania(
                personaje,
                linea.ObtenerTextoActual(),
                3.2f,
                prioridad);
        if (emitido)
        {
            ultimoHablante = personaje;
        }
        return emitido;
    }

    private bool EmitirActividad(Personaje personaje)
    {
        if (personaje == null || !ActividadCompatibleConClase(personaje.ActividadSeleccionada, personaje.IDClase))
        {
            return false;
        }

        IReadOnlyList<BanterLineaCampaniaLocal> lineas =
            BanterContenidoCampania.ObtenerLineasActividad(personaje.ActividadSeleccionada);
        BanterLineaCampaniaLocal linea = ElegirLinea(
            lineas,
            "actividad:" + personaje.ActividadSeleccionada);
        bool emitido = linea != null
            && BanterBattleUI.EmitirCampania(personaje, linea.ObtenerTextoActual());
        if (emitido)
        {
            ultimoHablante = personaje;
        }
        return emitido;
    }

    private BanterLineaCampaniaLocal ElegirLineaClase(
        int idClase,
        BanterCampaniaDisparador disparador)
    {
        return ElegirLinea(
            BanterContenidoCampania.ObtenerLineas(idClase, disparador),
            "clase:" + idClase + ":" + disparador);
    }

    private BanterLineaCampaniaLocal ElegirLinea(
        IReadOnlyList<BanterLineaCampaniaLocal> lineas,
        string clave)
    {
        if (lineas == null || lineas.Count == 0)
        {
            return null;
        }

        int indice = Random.Range(0, lineas.Count);
        if (lineas.Count > 1
            && ultimaLineaPorCombinacion.TryGetValue(clave, out string ultimaId)
            && lineas[indice].Id == ultimaId)
        {
            indice = (indice + 1) % lineas.Count;
        }

        BanterLineaCampaniaLocal elegida = lineas[indice];
        ultimaLineaPorCombinacion[clave] = elegida.Id;
        return elegida;
    }

    private Personaje ElegirPersonajeAleatorio(int idClase = 0)
    {
        List<Personaje> personajes = ObtenerPersonajesDisponibles(idClase);
        int indice = ElegirIndiceEvitando(personajes, ultimoHablante);
        return indice >= 0 ? personajes[indice] : null;
    }

    private static int ElegirIndiceEvitando(List<Personaje> personajes, Personaje personajeEvitado)
    {
        if (personajes == null || personajes.Count == 0)
        {
            return -1;
        }

        int indice = Random.Range(0, personajes.Count);
        if (personajes.Count > 1 && personajes[indice] == personajeEvitado)
        {
            indice = (indice + Random.Range(1, personajes.Count)) % personajes.Count;
        }
        return indice;
    }

    private List<Personaje> ObtenerPersonajesDisponibles(int idClase = 0)
    {
        List<Personaje> resultado = new List<Personaje>();
        if (campaignManager == null
            || campaignManager.scMenuPersonajes == null
            || campaignManager.scMenuPersonajes.listaPersonajes == null)
        {
            return resultado;
        }

        List<Personaje> personajes = campaignManager.scMenuPersonajes.listaPersonajes;
        for (int i = 0; i < personajes.Count; i++)
        {
            Personaje personaje = personajes[i];
            if (personaje == null
                || personaje.Camp_Muerto
                || personaje.fVidaActual <= 0f
                || !personaje.gameObject.activeInHierarchy
                || (idClase > 0 && personaje.IDClase != idClase))
            {
                continue;
            }

            resultado.Add(personaje);
        }

        return resultado;
    }

    private Nodo ObtenerNodoActual()
    {
        return campaignManager != null && campaignManager.scMapaManager != null
            ? campaignManager.scMapaManager.nodoActual
            : null;
    }

    private bool HayPocosSuministros()
    {
        return campaignManager != null
            && campaignManager.GetSuministrosActuales() < campaignManager.GetCivilesActual();
    }

    private bool HaySobrecarga()
    {
        return campaignManager != null
            && campaignManager.GetCargaLlevadaActual() > campaignManager.GetCapacidadDeCargaActual();
    }

    private bool HayCansancio()
    {
        return campaignManager != null && campaignManager.GetFatigaActual() > 4;
    }

    private static bool ActividadCompatibleConClase(int idActividad, int idClase)
    {
        if (idActividad >= 1 && idActividad <= 3)
        {
            return true;
        }
        if (idActividad < 4 || idActividad > 21)
        {
            return false;
        }

        int claseEsperada = ((idActividad - 4) / 3) + 1;
        return claseEsperada == idClase;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
