using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanterBattleDirector : MonoBehaviour
{
    private enum EtapaMoral
    {
        MuyBaja,
        Baja,
        Media,
        Alta,
        MuyAlta
    }

    private static BanterBattleDirector instance;
    private readonly HashSet<string> lineasUsadas = new HashSet<string>();
    private readonly HashSet<int> muertesProcesadas = new HashSet<int>();
    private readonly HashSet<int> primerosTurnosProcesados = new HashSet<int>();
    private readonly HashSet<int> vidaCriticaProcesada = new HashSet<int>();
    private readonly HashSet<int> criticosRecibidosPendientes = new HashSet<int>();
    private static readonly BanterLineaLocal[] lineasProtectorCaravana =
    {
        new BanterLineaLocal(
            "protector_inicio_01",
            BanterDisparador.ComenzarBatalla,
            "Protejamos a los que no pueden hacerlo...",
            "Let us protect those who cannot protect themselves...",
            "Vamos proteger aqueles que não podem se proteger..."),
        new BanterLineaLocal(
            "protector_inicio_02",
            BanterDisparador.ComenzarBatalla,
            "¡Por la caravana!",
            "For the caravan!",
            "Pela caravana!"),
        new BanterLineaLocal(
            "protector_inicio_03",
            BanterDisparador.ComenzarBatalla,
            "¡Luchemos! Por llegar a Serria...",
            "Let us fight! So we may reach Serria...",
            "Lutemos! Para chegarmos a Serria...")
    };
    private BattleManager battleManager;
    private AdministradorEscenas administradorEscenas;
    private Coroutine idleTurnoRoutine;
    private Unidad unidadTurnoObservada;
    private bool batallaIniciada;
    private bool etapaMoralInicializada;
    private EtapaMoral ultimaEtapaMoral;
    private int ultimoFramePifia = -1;
    private int ultimoActorPifia;
    private int ultimoFrameCritico = -1;
    private int ultimoActorCritico;

    public static void Instalar(BattleManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (instance != null)
        {
            instance.Reiniciar(manager);
            return;
        }

        BanterBattleDirector director = manager.GetComponent<BanterBattleDirector>();
        if (director == null)
        {
            director = manager.gameObject.AddComponent<BanterBattleDirector>();
        }
        director.Reiniciar(manager);
        instance = director;
    }

    public static void Finalizar()
    {
        instance?.CerrarBatalla();
    }

    public static void NotificarInicioTurno(Unidad unidad)
    {
        instance?.ProcesarInicioTurno(unidad);
    }

    private void Reiniciar(BattleManager manager)
    {
        if (idleTurnoRoutine != null)
        {
            StopCoroutine(idleTurnoRoutine);
            idleTurnoRoutine = null;
        }

        battleManager = manager;
        administradorEscenas =
            manager != null ? manager.GetComponentInParent<AdministradorEscenas>() : null;
        lineasUsadas.Clear();
        muertesProcesadas.Clear();
        primerosTurnosProcesados.Clear();
        vidaCriticaProcesada.Clear();
        criticosRecibidosPendientes.Clear();
        unidadTurnoObservada = null;
        batallaIniciada = false;
        etapaMoralInicializada = false;
        ultimoFramePifia = -1;
        ultimoActorPifia = 0;
        ultimoFrameCritico = -1;
        ultimoActorCritico = 0;
    }

    private void CerrarBatalla()
    {
        if (idleTurnoRoutine != null)
        {
            StopCoroutine(idleTurnoRoutine);
            idleTurnoRoutine = null;
        }
        unidadTurnoObservada = null;
        battleManager = null;
    }

    public static void NotificarResultadoAtaque(Unidad atacante, Unidad receptor, int resultado)
    {
        if (instance == null)
        {
            return;
        }

        if (resultado < 0 && instance.EsAliadoJugador(atacante))
        {
            int actorId = atacante != null ? atacante.GetInstanceID() : 0;
            if (instance.ultimoFramePifia == Time.frameCount
                && instance.ultimoActorPifia == actorId)
            {
                return;
            }
            instance.ultimoFramePifia = Time.frameCount;
            instance.ultimoActorPifia = actorId;
            instance.IntentarDisparar(
                BanterDisparador.Pifia,
                0.5f,
                instance.ObtenerCompanerosVivos(atacante));
        }
        else if (resultado == 3)
        {
            if (instance.EsAliadoJugador(atacante))
            {
                int actorId = atacante.GetInstanceID();
                if (instance.ultimoFrameCritico != Time.frameCount
                    || instance.ultimoActorCritico != actorId)
                {
                    instance.ultimoFrameCritico = Time.frameCount;
                    instance.ultimoActorCritico = actorId;
                    instance.IntentarDisparar(
                        BanterDisparador.CriticoRealizado,
                        0.5f,
                        new List<Unidad> { atacante });
                }
            }

            if (instance.EsAliadoVivo(receptor))
            {
                instance.criticosRecibidosPendientes.Add(receptor.GetInstanceID());
                instance.IntentarDisparar(
                    BanterDisparador.CriticoRecibido,
                    0.3f,
                    new List<Unidad> { receptor });
            }
        }
    }

    public static void NotificarDanio(
        Unidad causante,
        Unidad receptor,
        bool esCritico,
        bool causaMuerte)
    {
        if (instance == null || receptor == null)
        {
            return;
        }

        bool entraEnVidaCritica = !causaMuerte
            && instance.EsAliadoVivo(receptor)
            && receptor.mod_maxHP > 0f
            && receptor.HP_actual / receptor.mod_maxHP < 0.3f
            && instance.vidaCriticaProcesada.Add(receptor.GetInstanceID());
        bool criticoYaProcesado = esCritico
            && instance.criticosRecibidosPendientes.Remove(receptor.GetInstanceID());

        if (entraEnVidaCritica)
        {
            instance.IntentarDisparar(
                BanterDisparador.VidaCritica,
                0.8f,
                new List<Unidad> { receptor });
        }
        else if (esCritico
            && !criticoYaProcesado
            && instance.EsAliadoVivo(receptor))
        {
            instance.IntentarDisparar(
                BanterDisparador.CriticoRecibido,
                0.3f,
                new List<Unidad> { receptor });
        }
        if (!causaMuerte || !instance.muertesProcesadas.Add(receptor.GetInstanceID()))
        {
            return;
        }

        BanterBattleUI.InvalidarHablante(receptor);

        if (instance.EsEnemigo(receptor) && instance.EsAliadoJugador(causante))
        {
            instance.IntentarDisparar(
                BanterDisparador.EnemigoDerrotado,
                0.6f,
                instance.ObtenerCompanerosVivos(causante));
        }
        else if (instance.EsAliadoJugador(receptor))
        {
            instance.IntentarDisparar(
                BanterDisparador.AliadoDerrotado,
                1f,
                instance.ObtenerAliadosVivos(receptor));
        }
    }

    public static void NotificarRefuerzoEnemigo()
    {
        if (instance == null)
        {
            return;
        }

        instance.IntentarDisparar(
            BanterDisparador.RefuerzoEnemigo,
            0.5f,
            instance.ObtenerAliadosVivos());
    }

    public static void NotificarMoral(float porcentaje)
    {
        instance?.ProcesarMoral(porcentaje);
    }

    public static void NotificarHabilidadAliada(
        Unidad usuario,
        List<object> objetivos,
        bool esHostil)
    {
        if (instance == null || esHostil || !instance.EsAliadoJugador(usuario) || objetivos == null)
        {
            return;
        }

        List<Unidad> receptores = new List<Unidad>();
        for (int i = 0; i < objetivos.Count; i++)
        {
            Unidad receptor = objetivos[i] as Unidad;
            if (receptor != null
                && receptor != usuario
                && instance.EsAliadoJugador(receptor)
                && receptor.HP_actual > 0
                && !receptores.Contains(receptor))
            {
                receptores.Add(receptor);
            }
        }

        if (receptores.Count > 0)
        {
            Unidad receptorElegido = receptores[Random.Range(0, receptores.Count)];
            instance.IntentarDisparar(
                BanterDisparador.HabilidadAliadaRecibida,
                0.4f,
                new List<Unidad> { receptorElegido });
        }
    }

    private void ProcesarInicioTurno(Unidad unidad)
    {
        if (!batallaIniciada)
        {
            batallaIniciada = true;
            if (!etapaMoralInicializada && battleManager != null)
            {
                ProcesarMoral(battleManager.ObtenerValourGlobalAliadosPctActual());
            }
        }

        if (EsAliadoVivo(unidad)
            && primerosTurnosProcesados.Add(unidad.GetInstanceID()))
        {
            if (EsProtectorCaravana(unidad))
            {
                BanterLineaLocal lineaProtector =
                    lineasProtectorCaravana[Random.Range(0, lineasProtectorCaravana.Length)];
                EmitirLinea(
                    unidad,
                    lineaProtector,
                    BanterDisparador.ComenzarBatalla);
            }
            else
            {
                IntentarDisparar(
                    BanterDisparador.ComenzarBatalla,
                    0.6f,
                    new List<Unidad> { unidad });
            }
        }

        unidadTurnoObservada = unidad;
        if (idleTurnoRoutine != null)
        {
            StopCoroutine(idleTurnoRoutine);
        }
        idleTurnoRoutine = StartCoroutine(EsperarIdleTurno(unidad));
    }

    private IEnumerator EsperarIdleTurno(Unidad unidad)
    {
        yield return new WaitForSecondsRealtime(30f);

        while (battleManager != null
            && battleManager.unidadActiva == unidad
            && battleManager.bOcupado)
        {
            yield return null;
        }

        if (battleManager != null
            && battleManager.unidadActiva == unidad
            && unidadTurnoObservada == unidad)
        {
            IntentarDisparar(
                BanterDisparador.IdleTurno,
                1f,
                ObtenerAliadosVivos());
        }

        idleTurnoRoutine = null;
    }

    private void ProcesarMoral(float porcentaje)
    {
        EtapaMoral etapaActual = CalcularEtapaMoral(porcentaje);
        if (!etapaMoralInicializada)
        {
            etapaMoralInicializada = true;
            ultimaEtapaMoral = etapaActual;
            return;
        }
        if (!batallaIniciada)
        {
            ultimaEtapaMoral = etapaActual;
            return;
        }

        if (etapaActual > ultimaEtapaMoral)
        {
            IntentarDisparar(
                BanterDisparador.MoralAumentaEtapa,
                1f,
                ObtenerAliadosVivos());
        }
        else if (etapaActual < ultimaEtapaMoral)
        {
            IntentarDisparar(
                BanterDisparador.MoralDisminuyeEtapa,
                1f,
                ObtenerAliadosVivos());
        }

        ultimaEtapaMoral = etapaActual;
    }

    private void IntentarDisparar(
        BanterDisparador disparador,
        float probabilidad,
        List<Unidad> candidatos)
    {
        if (candidatos == null || candidatos.Count == 0)
        {
            return;
        }

        List<Unidad> hablantes = new List<Unidad>();
        List<List<BanterLineaLocal>> lineasDisponibles = new List<List<BanterLineaLocal>>();
        for (int i = 0; i < candidatos.Count; i++)
        {
            Unidad candidato = candidatos[i];
            if (!EsAliadoVivo(candidato))
            {
                continue;
            }

            IReadOnlyList<BanterLineaLocal> lineas =
                BanterContenidoLocal.ObtenerLineas(candidato, disparador);
            List<BanterLineaLocal> disponibles = new List<BanterLineaLocal>();
            for (int j = 0; j < lineas.Count; j++)
            {
                BanterLineaLocal linea = lineas[j];
                if (linea != null
                    && !string.IsNullOrWhiteSpace(linea.Id)
                    && !string.IsNullOrWhiteSpace(linea.ObtenerTextoActual())
                    && !lineasUsadas.Contains(ClaveLinea(candidato, linea)))
                {
                    disponibles.Add(linea);
                }
            }

            if (disponibles.Count > 0)
            {
                hablantes.Add(candidato);
                lineasDisponibles.Add(disponibles);
            }
        }

        if (hablantes.Count == 0 || Random.value > Mathf.Clamp01(probabilidad))
        {
            return;
        }

        int indiceHablante = Random.Range(0, hablantes.Count);
        List<BanterLineaLocal> opciones = lineasDisponibles[indiceHablante];
        BanterLineaLocal elegida = opciones[Random.Range(0, opciones.Count)];
        EmitirLinea(hablantes[indiceHablante], elegida, disparador);

        if (hablantes.Count > 1
            && PermiteRespuesta(disparador)
            && Random.value <= 0.1f)
        {
            int indiceRespuesta = Random.Range(0, hablantes.Count - 1);
            if (indiceRespuesta >= indiceHablante)
            {
                indiceRespuesta++;
            }

            List<BanterLineaLocal> opcionesRespuesta = lineasDisponibles[indiceRespuesta];
            BanterLineaLocal respuesta =
                opcionesRespuesta[Random.Range(0, opcionesRespuesta.Count)];
            EmitirLinea(hablantes[indiceRespuesta], respuesta, disparador);
        }
    }

    private void EmitirLinea(
        Unidad hablante,
        BanterLineaLocal linea,
        BanterDisparador disparador)
    {
        string texto = linea.ObtenerTextoActual();
        lineasUsadas.Add(ClaveLinea(hablante, linea));
        BanterBattleUI.Emitir(
            hablante,
            texto,
            DuracionParaTexto(texto),
            Prioridad(disparador),
            false);
    }

    private static float DuracionParaTexto(string texto)
    {
        int caracteresExtra = Mathf.Max(0, (texto?.Length ?? 0) - 70);
        return Mathf.Clamp(4f + caracteresExtra * 0.018f, 4f, 5.5f);
    }

    private static bool PermiteRespuesta(BanterDisparador disparador)
    {
        return disparador == BanterDisparador.ComenzarBatalla
            || disparador == BanterDisparador.AliadoDerrotado
            || disparador == BanterDisparador.RefuerzoEnemigo
            || disparador == BanterDisparador.MoralAumentaEtapa
            || disparador == BanterDisparador.MoralDisminuyeEtapa;
    }

    private List<Unidad> ObtenerCompanerosVivos(Unidad unidadExcluida)
    {
        return ObtenerAliadosVivos(unidadExcluida);
    }

    private List<Unidad> ObtenerAliadosVivos(Unidad unidadExcluida = null)
    {
        List<Unidad> aliados = new List<Unidad>();
        if (battleManager == null || battleManager.ladoB == null || battleManager.ladoB.unidadesLado == null)
        {
            return aliados;
        }

        for (int i = 0; i < battleManager.ladoB.unidadesLado.Count; i++)
        {
            Unidad unidad = battleManager.ladoB.unidadesLado[i];
            if (unidad != unidadExcluida && EsAliadoVivo(unidad))
            {
                aliados.Add(unidad);
            }
        }
        return aliados;
    }

    private bool EsAliadoVivo(Unidad unidad)
    {
        return EsAliadoJugador(unidad)
            && unidad.HP_actual > 0
            && !muertesProcesadas.Contains(unidad.GetInstanceID())
            && unidad.gameObject.activeInHierarchy;
    }

    private bool EsProtectorCaravana(Unidad unidad)
    {
        if (unidad == null)
        {
            return false;
        }

        if (administradorEscenas == null && battleManager != null)
        {
            administradorEscenas =
                battleManager.GetComponentInParent<AdministradorEscenas>();
        }

        Personaje personaje = administradorEscenas != null
            ? administradorEscenas.ObtenerPersonajeDesdeUnidad(unidad)
            : null;
        return personaje != null
            && personaje.TieneRasgo(PersonajeTraitCatalog.TraitLiderCaravana);
    }

    private bool EsAliadoJugador(Unidad unidad)
    {
        return unidad != null
            && battleManager != null
            && battleManager.ladoB != null
            && battleManager.ladoB.unidadesLado != null
            && battleManager.ladoB.unidadesLado.Contains(unidad);
    }

    private bool EsEnemigo(Unidad unidad)
    {
        return unidad != null
            && battleManager != null
            && battleManager.ladoA != null
            && battleManager.ladoA.unidadesLado != null
            && battleManager.ladoA.unidadesLado.Contains(unidad);
    }

    private static EtapaMoral CalcularEtapaMoral(float porcentaje)
    {
        if (porcentaje < 15f) return EtapaMoral.MuyBaja;
        if (porcentaje < 40f) return EtapaMoral.Baja;
        if (porcentaje < 70f) return EtapaMoral.Media;
        if (porcentaje < 90f) return EtapaMoral.Alta;
        return EtapaMoral.MuyAlta;
    }

    private static int Prioridad(BanterDisparador disparador)
    {
        switch (disparador)
        {
            case BanterDisparador.AliadoDerrotado:
                return 3;
            case BanterDisparador.MoralAumentaEtapa:
            case BanterDisparador.MoralDisminuyeEtapa:
            case BanterDisparador.RefuerzoEnemigo:
            case BanterDisparador.VidaCritica:
                return 2;
            case BanterDisparador.CriticoRecibido:
            case BanterDisparador.CriticoRealizado:
            case BanterDisparador.EnemigoDerrotado:
                return 1;
            default:
                return 0;
        }
    }

    private static string ClaveLinea(Unidad unidad, BanterLineaLocal linea)
    {
        string clase = unidad != null ? unidad.GetType().FullName : "Unidad";
        return clase + ":" + linea.Id;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
