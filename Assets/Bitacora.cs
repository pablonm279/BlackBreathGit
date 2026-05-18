using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Bitacora : MonoBehaviour
{
    private const int RegistrosPorPaginaCampania = 20;

    private readonly List<string> entradasCampania = new List<string>();
    private readonly List<EntradaCombate> entradasBatalla = new List<EntradaCombate>();
    private readonly List<string> bufferNombresCaidos = new List<string>();

    private ContextoBatalla contextoBatallaPendiente;
    private bool debugTextoFlotanteRecursosEmitido;
    private bool ancladoALaUltimaPagina = true;
    private bool mostrandoCombate;
    private int diaActual = 1;
    private int paginaVisible = 1;
    private int rondaActual = 1;
    private int ultimoDiaRegistrado = 0;

    private static readonly Regex RegexTagsNoPermitidos =
        new Regex(@"</?(?!\s*(?:b|i|color|size|mark|sprite)\b)[^>]+>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RegexGuionInicial =
        new Regex(@"^((?:\s*<[^>]+>\s*)*)-\s*", RegexOptions.Compiled);

    private static readonly Regex RegexColorTags =
        new Regex(@"</?color(?:=[^>]+)?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private struct EntradaCombate
    {
        public int Ronda;
        public string Texto;
    }

    private sealed class ContextoBatalla
    {
        public string FactionId;
        public string FactionName;
        public int BattleId;
        public int EmboscadaId;
        public readonly List<string> ParticipantIds = new List<string>();
    }

    protected abstract TextMeshProUGUI LogText { get; }
    public abstract TMP_SpriteAsset SpriteAssetRecursos { get; }
    public abstract TMP_SpriteAsset SpriteAssetCombate { get; }
    protected abstract int MaxEntradasCombate { get; }
    protected abstract string ColorDia { get; }
    protected abstract string ColorActual { get; }
    protected abstract string ColorPasado { get; }
    protected abstract int SizeActualPct { get; }
    protected abstract int SizePasadoPct { get; }
    protected abstract bool DebugTextoFlotanteRecursos { get; set; }
    protected abstract string DebugMensajeTextoFlotanteRecursos { get; }
    protected int CantidadEntradasCampania => entradasCampania.Count;

    private void Awake()
    {
        AplicarSpriteAssetLog(false);
    }

    private void OnValidate()
    {
        AplicarSpriteAssetLog(false);
    }

    private void Update()
    {
        ProcesarDebugTextoFlotanteRecursos();
    }

    public void SetDiaActual(int numeroTurno, bool esCombate = false)
    {
        int valorNormalizado = Mathf.Max(1, numeroTurno);
        if (esCombate)
        {
            rondaActual = valorNormalizado;
            if (mostrandoCombate)
            {
                ReconstruirTexto(false);
            }

            return;
        }

        diaActual = valorNormalizado;
        if (ancladoALaUltimaPagina || paginaVisible <= 0)
        {
            paginaVisible = GetUltimaPagina();
        }

        if (!mostrandoCombate)
        {
            ReconstruirTexto(false);
        }
    }

    public int GetDiaActual()
    {
        return diaActual;
    }

    public int GetPaginaVisible()
    {
        return paginaVisible > 0 ? paginaVisible : 1;
    }

    public int GetCantidadPaginas()
    {
        return GetUltimaPagina();
    }

    public bool MostrarPagina(int pagina)
    {
        int paginaNormalizada = Mathf.Clamp(pagina, 1, GetUltimaPagina());
        if (paginaNormalizada != pagina)
        {
            return false;
        }

        mostrandoCombate = false;
        paginaVisible = paginaNormalizada;
        ancladoALaUltimaPagina = paginaVisible >= GetUltimaPagina();
        ReconstruirTexto(false);
        return true;
    }

    public void MostrarPaginaActual()
    {
        mostrandoCombate = false;
        paginaVisible = GetUltimaPagina();
        ancladoALaUltimaPagina = true;
        ReconstruirTexto(false);
    }

    public bool MostrarPaginaAnterior()
    {
        return MostrarPagina(GetPaginaVisible() - 1);
    }

    public bool MostrarPaginaSiguiente()
    {
        return MostrarPagina(GetPaginaVisible() + 1);
    }

    public int GetDiaVisible()
    {
        return GetPaginaVisible();
    }

    public int GetCantidadDias()
    {
        return GetCantidadPaginas();
    }

    public bool MostrarDia(int dia)
    {
        return MostrarPagina(dia);
    }

    public void MostrarDiaActual()
    {
        MostrarPaginaActual();
    }

    public bool MostrarDiaAnterior()
    {
        return MostrarPaginaAnterior();
    }

    public bool MostrarDiaSiguiente()
    {
        return MostrarPaginaSiguiente();
    }

    public void RegistrarInicioDia(int dia, int esperanza, int oro, int materiales, int suministros, int tipoClima = 0)
    {
        diaActual = Mathf.Max(1, dia);
        RegistrarCabeceraDeDiaSiHaceFalta(diaActual, esperanza, oro, materiales, suministros);
    }

    public void AsegurarDiaActualConSnapshotSiFalta(int dia, int esperanza, int oro, int materiales, int suministros, int tipoClima = 0)
    {
        diaActual = Mathf.Max(1, dia);
        if (diaActual > ultimoDiaRegistrado)
        {
            RegistrarCabeceraDeDiaSiHaceFalta(diaActual, esperanza, oro, materiales, suministros);
            return;
        }

        if (paginaVisible <= 0 || ancladoALaUltimaPagina)
        {
            paginaVisible = GetUltimaPagina();
        }

        if (!mostrandoCombate)
        {
            ReconstruirTexto(false);
        }
    }

    public void RegistrarLlegadaNodo(int tipoNodo)
    {
        AgregarEntradaCampania(ConstruirTextoLlegadaNodo(tipoNodo));
    }

    public void RegistrarDescanso()
    {
        AgregarEntradaCampania(TraducirTextoBitacora(TextoBitacoraId.CaravanaDescanso));
    }

    public void PrepararContextoBatalla(string factionId, string factionName, int battleId, int emboscadaId, IEnumerable<Personaje> participantes)
    {
        contextoBatallaPendiente = new ContextoBatalla
        {
            FactionId = factionId ?? string.Empty,
            FactionName = factionName ?? string.Empty,
            BattleId = battleId,
            EmboscadaId = emboscadaId
        };

        if (participantes == null)
        {
            return;
        }

        foreach (Personaje participante in participantes)
        {
            if (participante == null)
            {
                continue;
            }

            string persistentId = participante.EnsurePersistentId();
            if (string.IsNullOrWhiteSpace(persistentId) || contextoBatallaPendiente.ParticipantIds.Contains(persistentId))
            {
                continue;
            }

            contextoBatallaPendiente.ParticipantIds.Add(persistentId);
        }
    }

    public void RegistrarResumenBatalla(int resultado, IList<Personaje> personajes)
    {
        if (contextoBatallaPendiente == null)
        {
            return;
        }

        string nombreFaccion = string.IsNullOrWhiteSpace(contextoBatallaPendiente.FactionName)
            ? TraducirTextoBitacora(TextoBitacoraId.FaccionEnemigaGenerica)
            : contextoBatallaPendiente.FactionName.Trim();

        bufferNombresCaidos.Clear();
        if (personajes != null)
        {
            foreach (string persistentId in contextoBatallaPendiente.ParticipantIds)
            {
                if (string.IsNullOrWhiteSpace(persistentId))
                {
                    continue;
                }

                Personaje personajeEncontrado = BuscarPersonajePorId(personajes, persistentId);
                if (personajeEncontrado == null || !personajeEncontrado.Camp_Muerto)
                {
                    continue;
                }

                bufferNombresCaidos.Add(personajeEncontrado.sNombre);
            }
        }

        string resumen = ConstruirResumenBatalla(resultado, nombreFaccion, bufferNombresCaidos);
        contextoBatallaPendiente = null;
        if (!string.IsNullOrWhiteSpace(resumen))
        {
            AgregarEntradaCampania(resumen);
        }
    }

    public void Escribir(string mensaje, bool esCombate = false)
    {
        if (esCombate && BattleManager.Instance != null && BattleManager.Instance.silenciarLogCombate)
        {
            return;
        }

        string limpia = Sanitizar(mensaje);
        if (string.IsNullOrWhiteSpace(limpia))
        {
            return;
        }

        if (esCombate)
        {
            entradasBatalla.Add(new EntradaCombate { Ronda = rondaActual, Texto = limpia });
            mostrandoCombate = true;
            RecortarEntradasCombateSiExcede();
            ReconstruirTexto(false);
            return;
        }

        AgregarEntradaCampania(limpia);
    }

    public void Limpiar()
    {
        entradasCampania.Clear();
        entradasBatalla.Clear();
        bufferNombresCaidos.Clear();
        contextoBatallaPendiente = null;
        mostrandoCombate = false;
        ancladoALaUltimaPagina = true;
        diaActual = 1;
        paginaVisible = 1;
        rondaActual = 1;
        ultimoDiaRegistrado = 0;

        if (LogText != null)
        {
            LogText.text = string.Empty;
        }

        LimpiarRenderCampaniaPersonalizado();
    }

    public void LimpiarDesdeCampania()
    {
        entradasBatalla.Clear();
        mostrandoCombate = true;
        if (LogText != null)
        {
            LogText.text = string.Empty;
        }

        LimpiarRenderCampaniaPersonalizado();
    }

    public void LimpiarDesdeBatalla()
    {
        entradasBatalla.Clear();
        mostrandoCombate = false;
        if (ancladoALaUltimaPagina || paginaVisible <= 0)
        {
            paginaVisible = GetUltimaPagina();
        }

        ReconstruirTexto(false);
    }

    public BitacoraSaveData ExportarSaveData()
    {
        BitacoraSaveData data = new BitacoraSaveData();
        data.ultimoDiaRegistrado = ultimoDiaRegistrado;

        for (int i = 0; i < entradasCampania.Count; i++)
        {
            data.entradasCampania.Add(new BitacoraEntradaSaveData { texto = entradasCampania[i] });
        }

        return data;
    }

    public void ImportarSaveData(BitacoraSaveData data, int diaFallback, int esperanza, int oro, int materiales, int suministros, int tipoClimaActual = 0)
    {
        entradasCampania.Clear();
        entradasBatalla.Clear();
        bufferNombresCaidos.Clear();
        contextoBatallaPendiente = null;
        mostrandoCombate = false;
        ancladoALaUltimaPagina = true;
        diaActual = Mathf.Max(1, diaFallback);
        paginaVisible = 1;
        rondaActual = 1;
        ultimoDiaRegistrado = 0;

        bool importoFormatoPlano = false;
        if (data != null && data.entradasCampania != null && data.entradasCampania.Count > 0)
        {
            for (int i = 0; i < data.entradasCampania.Count; i++)
            {
                BitacoraEntradaSaveData entrada = data.entradasCampania[i];
                if (entrada == null || string.IsNullOrWhiteSpace(entrada.texto))
                {
                    continue;
                }

                entradasCampania.Add(Sanitizar(entrada.texto));
            }

            ultimoDiaRegistrado = Mathf.Max(0, data.ultimoDiaRegistrado);
            importoFormatoPlano = entradasCampania.Count > 0;
        }

        if (!importoFormatoPlano && data != null && data.dias != null && data.dias.Count > 0)
        {
            List<BitacoraDiaSaveData> diasLegados = new List<BitacoraDiaSaveData>(data.dias);
            diasLegados.Sort((a, b) =>
            {
                int diaA = a != null ? a.dia : 0;
                int diaB = b != null ? b.dia : 0;
                return diaA.CompareTo(diaB);
            });

            for (int i = 0; i < diasLegados.Count; i++)
            {
                BitacoraDiaSaveData diaData = diasLegados[i];
                if (diaData == null)
                {
                    continue;
                }

                int dia = Mathf.Max(1, diaData.dia);
                AgregarEntradaCampaniaInterna(ConstruirEncabezadoDia(dia));
                if (diaData.tieneSnapshotRecursos)
                {
                    AgregarEntradaCampaniaInterna(ConstruirEntradaRecursos(
                        diaData.esperanzaInicial,
                        diaData.oroInicial,
                        diaData.materialesIniciales,
                        diaData.suministrosIniciales));
                }

                if (diaData.entradas != null)
                {
                    for (int j = 0; j < diaData.entradas.Count; j++)
                    {
                        BitacoraEntradaSaveData entrada = diaData.entradas[j];
                        if (entrada == null || string.IsNullOrWhiteSpace(entrada.texto))
                        {
                            continue;
                        }

                        AgregarEntradaCampaniaInterna(Sanitizar(entrada.texto));
                    }
                }

                ultimoDiaRegistrado = Mathf.Max(ultimoDiaRegistrado, dia);
            }
        }

        AsegurarDiaActualConSnapshotSiFalta(diaActual, esperanza, oro, materiales, suministros);
    }

    public static string ConstruirTextoSubidaDeNivelNarrativo(string nombrePersonaje)
    {
        string nombreSeguro = string.IsNullOrWhiteSpace(nombrePersonaje) ? "Alguien" : nombrePersonaje.Trim();
        switch (ObtenerIdiomaBitacoraEstatico())
        {
            case TRADU.IdiomaIngles:
                return nombreSeguro + " has shown improvement in their skills.";
            case TRADU.IdiomaPortugues:
                return nombreSeguro + " demonstrou melhora em suas habilidades.";
            default:
                return nombreSeguro + " ha mostrado mejor\u00EDa en sus habilidades.";
        }
    }

    public static string ObtenerNombreAmenazaSubterraneaBitacora()
    {
        switch (ObtenerIdiomaBitacoraEstatico())
        {
            case TRADU.IdiomaIngles:
                return "Underground threats";
            case TRADU.IdiomaPortugues:
                return "Amea\u00E7as subterr\u00E2neas";
            default:
                return "Amenazas del subsuelo";
        }
    }

    private void RegistrarCabeceraDeDiaSiHaceFalta(int dia, int esperanza, int oro, int materiales, int suministros)
    {
        if (dia <= ultimoDiaRegistrado)
        {
            if (paginaVisible <= 0 || ancladoALaUltimaPagina)
            {
                paginaVisible = GetUltimaPagina();
            }

            if (!mostrandoCombate)
            {
                ReconstruirTexto(false);
            }

            return;
        }

        bool estabaAnclado = ancladoALaUltimaPagina || paginaVisible <= 0;
        AgregarEntradaCampaniaInterna(ConstruirEncabezadoDia(dia));
        AgregarEntradaCampaniaInterna(ConstruirEntradaRecursos(esperanza, oro, materiales, suministros));
        ultimoDiaRegistrado = dia;

        if (estabaAnclado)
        {
            paginaVisible = GetUltimaPagina();
            ancladoALaUltimaPagina = true;
        }

        if (!mostrandoCombate)
        {
            ReconstruirTexto(false);
        }
    }

    private string ConstruirEncabezadoDia(int dia)
    {
        return "---------------- " + TraducirTextoBitacora(TextoBitacoraId.EtiquetaDia) + " " + dia;
    }

    private string ConstruirEntradaRecursos(int esperanza, int oro, int materiales, int suministros)
    {
        StringBuilder sb = new StringBuilder(96);
        sb.Append(TraducirTextoBitacora(TextoBitacoraId.RecursoEsperanza)).Append(": ").Append(esperanza);
        sb.Append(" | ").Append(TraducirTextoBitacora(TextoBitacoraId.RecursoOro)).Append(": ").Append(oro);
        sb.Append(" | ").Append(TraducirTextoBitacora(TextoBitacoraId.RecursoMateriales)).Append(": ").Append(materiales);
        sb.Append(" | ").Append(TraducirTextoBitacora(TextoBitacoraId.RecursoSuministros)).Append(": ").Append(suministros);
        return sb.ToString();
    }

    private void AgregarEntradaCampania(string mensaje)
    {
        string limpia = Sanitizar(mensaje);
        if (string.IsNullOrWhiteSpace(limpia))
        {
            return;
        }

        AgregarEntradaCampaniaInterna(limpia);
        if (ancladoALaUltimaPagina || paginaVisible <= 0)
        {
            paginaVisible = GetUltimaPagina();
            ancladoALaUltimaPagina = true;
        }

        if (!mostrandoCombate)
        {
            ReconstruirTexto(false);
        }
    }

    private void AgregarEntradaCampaniaInterna(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        entradasCampania.Add(mensaje);
    }

    private int GetUltimaPagina()
    {
        if (TryGetCantidadPaginasCampania(out int cantidadPaginasPersonalizada))
        {
            return Mathf.Max(1, cantidadPaginasPersonalizada);
        }

        if (entradasCampania.Count <= 0)
        {
            return 1;
        }

        return Mathf.CeilToInt(entradasCampania.Count / (float)RegistrosPorPaginaCampania);
    }

    private void RecortarEntradasCombateSiExcede()
    {
        int maximo = Mathf.Max(1, MaxEntradasCombate);
        if (entradasBatalla.Count <= maximo)
        {
            return;
        }

        entradasBatalla.RemoveRange(0, entradasBatalla.Count - maximo);
    }

    private void ReconstruirTexto(bool forzarCombate)
    {
        bool renderizarCombate = forzarCombate || mostrandoCombate;
        if (renderizarCombate)
        {
            LimpiarRenderCampaniaPersonalizado();
        }
        else if (TryRenderizarCampaniaPersonalizada())
        {
            if (LogText != null)
            {
                LogText.text = string.Empty;
            }

            return;
        }
        else
        {
            LimpiarRenderCampaniaPersonalizado();
        }

        if (LogText == null)
        {
            return;
        }

        LogText.textWrappingMode = TextWrappingModes.Normal;
        LogText.richText = true;
        LogText.enableAutoSizing = false;
        LogText.overflowMode = TextOverflowModes.Truncate;
        AplicarSpriteAssetLog(renderizarCombate);
        LogText.text = renderizarCombate ? ConstruirTextoCombate() : ConstruirTextoCampania();
    }

    private string ConstruirTextoCombate()
    {
        if (entradasBatalla.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(entradasBatalla.Count * 64);
        for (int i = 0; i < entradasBatalla.Count; i++)
        {
            EntradaCombate entrada = entradasBatalla[i];
            string mensajeRender = TextoIconosCombate.FormatearIconos(NormalizarMensajeRender(entrada.Texto), SpriteAssetCombate != null);
            string etiquetaRonda = TraducirTextoBitacora(TextoBitacoraId.EtiquetaRonda);
            string prefijo = $"<color={ColorDia}>- {etiquetaRonda} {entrada.Ronda}</color>";
            bool esRondaActual = entrada.Ronda == rondaActual;

            if (esRondaActual)
            {
                sb.Append("<size=").Append(SizeActualPct).Append("%><color=").Append(ColorActual).Append(">")
                  .Append(prefijo).Append(" - ").Append(mensajeRender)
                  .Append("</color></size>");
            }
            else
            {
                sb.Append("<i><size=").Append(SizePasadoPct).Append("%><color=").Append(ColorPasado).Append(">")
                  .Append(prefijo).Append(" - ").Append(mensajeRender)
                  .Append("</color></size></i>");
            }

            if (i < entradasBatalla.Count - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    private string ConstruirTextoCampania()
    {
        if (entradasCampania.Count == 0)
        {
            return AplicarFiltroTextoNegroBitacora(TraducirTextoBitacora(TextoBitacoraId.SinRegistros));
        }

        int pagina = Mathf.Clamp(GetPaginaVisible(), 1, GetUltimaPagina());
        if (pagina != paginaVisible)
        {
            paginaVisible = pagina;
            ancladoALaUltimaPagina = paginaVisible >= GetUltimaPagina();
        }

        ObtenerRangoPaginaCampania(pagina, out int indiceInicial, out int indiceFinal);
        StringBuilder sb = new StringBuilder((indiceFinal - indiceInicial) * 64);

        for (int i = indiceInicial; i < indiceFinal; i++)
        {
            string mensajeRender = ObtenerEntradaCampaniaFormateada(i, false);
            sb.Append(mensajeRender);

            if (i < indiceFinal - 1)
            {
                sb.Append('\n');
            }
        }

        return AplicarFiltroTextoNegroBitacora(sb.ToString());
    }

    protected static string AplicarFiltroTextoNegroBitacora(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        string sinColores = RegexColorTags.Replace(texto, string.Empty);
        return "<color=#000000>" + sinColores + "</color>";
    }

    private void AplicarSpriteAssetLog(bool esCombate)
    {
        if (LogText == null)
        {
            return;
        }

        TMP_SpriteAsset spriteAsset = esCombate ? SpriteAssetCombate : SpriteAssetRecursos;
        if (spriteAsset != null)
        {
            LogText.spriteAsset = spriteAsset;
        }
    }

    private void ProcesarDebugTextoFlotanteRecursos()
    {
        if (!DebugTextoFlotanteRecursos)
        {
            debugTextoFlotanteRecursosEmitido = false;
            return;
        }

        if (debugTextoFlotanteRecursosEmitido)
        {
            return;
        }

        debugTextoFlotanteRecursosEmitido = true;
        if (CampaignManager.Instance == null)
        {
            Debug.LogWarning("No se pudo imprimir el texto flotante debug de recursos: CampaignManager.Instance es null.");
            return;
        }

        CampaignManager.Instance.GenerarTextoFlotanteCampa\u00F1a(DebugMensajeTextoFlotanteRecursos, Color.cyan);
    }

    private static string Sanitizar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        string limpio = RegexTagsNoPermitidos.Replace(texto, string.Empty);
        return limpio.Replace("\r\n", " ").Replace('\n', ' ').Trim();
    }

    protected string ObtenerEntradaCampania(int indice)
    {
        if (indice < 0 || indice >= entradasCampania.Count)
        {
            return string.Empty;
        }

        return entradasCampania[indice];
    }

    protected string ObtenerEntradaCampaniaFormateada(int indice, bool incluirIconos)
    {
        return TextoRecursosCampania.FormatearRecursos(
            NormalizarMensajeRender(ObtenerEntradaCampania(indice)),
            incluirIconos);
    }

    protected virtual bool TryGetCantidadPaginasCampania(out int cantidadPaginas)
    {
        cantidadPaginas = 0;
        return false;
    }

    protected virtual bool TryGetRangoPaginaCampania(int pagina, out int indiceInicial, out int indiceFinal)
    {
        indiceInicial = 0;
        indiceFinal = 0;
        return false;
    }

    protected virtual bool TryRenderizarCampaniaPersonalizada()
    {
        return false;
    }

    protected virtual void LimpiarRenderCampaniaPersonalizado()
    {
    }

    private void ObtenerRangoPaginaCampania(int pagina, out int indiceInicial, out int indiceFinal)
    {
        if (TryGetRangoPaginaCampania(pagina, out indiceInicial, out indiceFinal))
        {
            indiceInicial = Mathf.Clamp(indiceInicial, 0, entradasCampania.Count);
            indiceFinal = Mathf.Clamp(indiceFinal, indiceInicial, entradasCampania.Count);
            return;
        }

        indiceInicial = (pagina - 1) * RegistrosPorPaginaCampania;
        indiceFinal = Mathf.Min(indiceInicial + RegistrosPorPaginaCampania, entradasCampania.Count);
    }

    protected static string NormalizarMensajeRender(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return RegexGuionInicial.Replace(texto, "$1");
    }

    private static Personaje BuscarPersonajePorId(IList<Personaje> personajes, string persistentId)
    {
        if (personajes == null || string.IsNullOrWhiteSpace(persistentId))
        {
            return null;
        }

        for (int i = 0; i < personajes.Count; i++)
        {
            Personaje personaje = personajes[i];
            if (personaje == null)
            {
                continue;
            }

            string personajeId = personaje.EnsurePersistentId();
            if (string.Equals(personajeId, persistentId, StringComparison.Ordinal))
            {
                return personaje;
            }
        }

        return null;
    }

    private string ConstruirTextoLlegadaNodo(int tipoNodo)
    {
        switch (tipoNodo)
        {
            case 1: return TraducirTextoBitacora(TextoBitacoraId.NodoBatalla);
            case 2: return TraducirTextoBitacora(TextoBitacoraId.NodoEvento);
            case 3: return TraducirTextoBitacora(TextoBitacoraId.NodoClaro);
            case 4: return TraducirTextoBitacora(TextoBitacoraId.NodoAsentamiento);
            case 5: return TraducirTextoBitacora(TextoBitacoraId.NodoRecursos);
            case 6: return TraducirTextoBitacora(TextoBitacoraId.NodoPuestoComercial);
            case 7: return TraducirTextoBitacora(TextoBitacoraId.NodoViajeros);
            case 8: return TraducirTextoBitacora(TextoBitacoraId.NodoBatallaElite);
            case 10: return TraducirTextoBitacora(TextoBitacoraId.NodoBatallaFinal);
            case 11: return TraducirTextoBitacora(TextoBitacoraId.NodoAtaqueCaravana);
            case 12: return TraducirTextoBitacora(TextoBitacoraId.NodoSubterraneo);
            case 14: return TraducirTextoBitacora(TextoBitacoraId.NodoSantuario);
            case 15: return TraducirTextoBitacora(TextoBitacoraId.NodoRitual);
            case 16: return TraducirTextoBitacora(TextoBitacoraId.NodoSalvamento);
            default: return TraducirTextoBitacora(TextoBitacoraId.NodoGenerico);
        }
    }

    private string ConstruirResumenBatalla(int resultado, string faccion, List<string> nombresCaidos)
    {
        string resumenBase = resultado == 1
            ? string.Format(TraducirTextoBitacora(TextoBitacoraId.ResumenVictoria), faccion)
            : string.Format(TraducirTextoBitacora(TextoBitacoraId.ResumenDerrota), faccion);

        if (nombresCaidos == null || nombresCaidos.Count == 0)
        {
            return resumenBase;
        }

        string listaNombres = FormatearListaNombres(nombresCaidos);
        if (nombresCaidos.Count == 1)
        {
            return resumenBase + " " + string.Format(TraducirTextoBitacora(TextoBitacoraId.CayoCompaneroSingular), listaNombres);
        }

        return resumenBase + " " + string.Format(TraducirTextoBitacora(TextoBitacoraId.CayoCompaneroPlural), listaNombres);
    }

    private string FormatearListaNombres(List<string> nombres)
    {
        if (nombres == null || nombres.Count == 0)
        {
            return string.Empty;
        }

        if (nombres.Count == 1)
        {
            return nombres[0];
        }

        string conector = TraducirTextoBitacora(TextoBitacoraId.ConectorLista);
        if (nombres.Count == 2)
        {
            return nombres[0] + " " + conector + " " + nombres[1];
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nombres.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(i == nombres.Count - 1 ? " " + conector + " " : ", ");
            }

            sb.Append(nombres[i]);
        }

        return sb.ToString();
    }

    private int ObtenerIdiomaBitacora()
    {
        if (TRADU.i == null)
        {
            return TRADU.IdiomaEspanol;
        }

        return TRADU.i.nIdioma;
    }

    private static int ObtenerIdiomaBitacoraEstatico()
    {
        if (TRADU.i == null)
        {
            return TRADU.IdiomaEspanol;
        }

        return TRADU.i.nIdioma;
    }

    private string TraducirTextoBitacora(TextoBitacoraId id)
    {
        switch (ObtenerIdiomaBitacora())
        {
            case TRADU.IdiomaIngles:
                return ObtenerTextoIngles(id);
            case TRADU.IdiomaPortugues:
                return ObtenerTextoPortugues(id);
            default:
                return ObtenerTextoEspanol(id);
        }
    }

    private static string ObtenerTextoEspanol(TextoBitacoraId id)
    {
        switch (id)
        {
            case TextoBitacoraId.EtiquetaDia: return "D\u00EDa";
            case TextoBitacoraId.EtiquetaRonda: return "Ronda";
            case TextoBitacoraId.SinRegistros: return "Sin registros.";
            case TextoBitacoraId.RecursoEsperanza: return "Esperanza";
            case TextoBitacoraId.RecursoOro: return "Oro";
            case TextoBitacoraId.RecursoMateriales: return "Materiales";
            case TextoBitacoraId.RecursoSuministros: return "Suministros";
            case TextoBitacoraId.CaravanaDescanso: return "La caravana descans\u00F3.";
            case TextoBitacoraId.ResumenVictoria: return "Victoria frente a {0}.";
            case TextoBitacoraId.ResumenDerrota: return "Derrota frente a {0}.";
            case TextoBitacoraId.CayoCompaneroSingular: return "Ha ca\u00EDdo {0}.";
            case TextoBitacoraId.CayoCompaneroPlural: return "Han ca\u00EDdo {0}.";
            case TextoBitacoraId.ConectorLista: return "y";
            case TextoBitacoraId.FaccionEnemigaGenerica: return "el enemigo";
            case TextoBitacoraId.NodoBatalla: return "La caravana ha llegado a un campo de batalla.";
            case TextoBitacoraId.NodoEvento: return "La caravana ha llegado a un punto de inter\u00E9s.";
            case TextoBitacoraId.NodoClaro: return "La caravana ha llegado a un claro.";
            case TextoBitacoraId.NodoAsentamiento: return "La caravana ha llegado a un asentamiento.";
            case TextoBitacoraId.NodoRecursos: return "La caravana ha llegado a una zona de recursos.";
            case TextoBitacoraId.NodoPuestoComercial: return "La caravana ha llegado a un puesto comercial.";
            case TextoBitacoraId.NodoViajeros: return "La caravana ha encontrado viajeros en el camino.";
            case TextoBitacoraId.NodoBatallaElite: return "La caravana ha llegado a un frente peligroso.";
            case TextoBitacoraId.NodoBatallaFinal: return "La caravana ha llegado al enfrentamiento decisivo.";
            case TextoBitacoraId.NodoAtaqueCaravana: return "La caravana ha entrado en una zona expuesta.";
            case TextoBitacoraId.NodoSubterraneo: return "La tierra se ha abierto bajo la caravana.";
            case TextoBitacoraId.NodoSantuario: return "La caravana ha llegado a un santuario.";
            case TextoBitacoraId.NodoRitual: return "La caravana ha llegado a un sitio ritual.";
            case TextoBitacoraId.NodoSalvamento: return "La caravana ha llegado a un punto de rescate.";
            default: return "La caravana ha llegado a un nuevo tramo del camino.";
        }
    }

    private static string ObtenerTextoIngles(TextoBitacoraId id)
    {
        switch (id)
        {
            case TextoBitacoraId.EtiquetaDia: return "Day";
            case TextoBitacoraId.EtiquetaRonda: return "Round";
            case TextoBitacoraId.SinRegistros: return "No records.";
            case TextoBitacoraId.RecursoEsperanza: return "Hope";
            case TextoBitacoraId.RecursoOro: return "Gold";
            case TextoBitacoraId.RecursoMateriales: return "Materials";
            case TextoBitacoraId.RecursoSuministros: return "Supplies";
            case TextoBitacoraId.CaravanaDescanso: return "The caravan rested.";
            case TextoBitacoraId.ResumenVictoria: return "Victory against {0}.";
            case TextoBitacoraId.ResumenDerrota: return "Defeat against {0}.";
            case TextoBitacoraId.CayoCompaneroSingular: return "{0} fell.";
            case TextoBitacoraId.CayoCompaneroPlural: return "{0} fell.";
            case TextoBitacoraId.ConectorLista: return "and";
            case TextoBitacoraId.FaccionEnemigaGenerica: return "the enemy";
            case TextoBitacoraId.NodoBatalla: return "The caravan reached a battlefield.";
            case TextoBitacoraId.NodoEvento: return "The caravan reached a point of interest.";
            case TextoBitacoraId.NodoClaro: return "The caravan reached a clearing.";
            case TextoBitacoraId.NodoAsentamiento: return "The caravan reached a settlement.";
            case TextoBitacoraId.NodoRecursos: return "The caravan reached a resource area.";
            case TextoBitacoraId.NodoPuestoComercial: return "The caravan reached a trading post.";
            case TextoBitacoraId.NodoViajeros: return "The caravan met travelers on the road.";
            case TextoBitacoraId.NodoBatallaElite: return "The caravan reached a dangerous front.";
            case TextoBitacoraId.NodoBatallaFinal: return "The caravan reached the decisive confrontation.";
            case TextoBitacoraId.NodoAtaqueCaravana: return "The caravan entered an exposed area.";
            case TextoBitacoraId.NodoSubterraneo: return "The ground opened beneath the caravan.";
            case TextoBitacoraId.NodoSantuario: return "The caravan reached a sanctuary.";
            case TextoBitacoraId.NodoRitual: return "The caravan reached a ritual site.";
            case TextoBitacoraId.NodoSalvamento: return "The caravan reached a rescue point.";
            default: return "The caravan reached another stretch of road.";
        }
    }

    private static string ObtenerTextoPortugues(TextoBitacoraId id)
    {
        switch (id)
        {
            case TextoBitacoraId.EtiquetaDia: return "Dia";
            case TextoBitacoraId.EtiquetaRonda: return "Rodada";
            case TextoBitacoraId.SinRegistros: return "Sem registros.";
            case TextoBitacoraId.RecursoEsperanza: return "Esperan\u00E7a";
            case TextoBitacoraId.RecursoOro: return "Ouro";
            case TextoBitacoraId.RecursoMateriales: return "Materiais";
            case TextoBitacoraId.RecursoSuministros: return "Suprimentos";
            case TextoBitacoraId.CaravanaDescanso: return "A caravana descansou.";
            case TextoBitacoraId.ResumenVictoria: return "Vit\u00F3ria contra {0}.";
            case TextoBitacoraId.ResumenDerrota: return "Derrota contra {0}.";
            case TextoBitacoraId.CayoCompaneroSingular: return "{0} caiu.";
            case TextoBitacoraId.CayoCompaneroPlural: return "{0} ca\u00EDram.";
            case TextoBitacoraId.ConectorLista: return "e";
            case TextoBitacoraId.FaccionEnemigaGenerica: return "o inimigo";
            case TextoBitacoraId.NodoBatalla: return "A caravana chegou a um campo de batalha.";
            case TextoBitacoraId.NodoEvento: return "A caravana chegou a um ponto de interesse.";
            case TextoBitacoraId.NodoClaro: return "A caravana chegou a uma clareira.";
            case TextoBitacoraId.NodoAsentamiento: return "A caravana chegou a um assentamento.";
            case TextoBitacoraId.NodoRecursos: return "A caravana chegou a uma \u00E1rea de recursos.";
            case TextoBitacoraId.NodoPuestoComercial: return "A caravana chegou a um posto comercial.";
            case TextoBitacoraId.NodoViajeros: return "A caravana encontrou viajantes na estrada.";
            case TextoBitacoraId.NodoBatallaElite: return "A caravana chegou a uma frente perigosa.";
            case TextoBitacoraId.NodoBatallaFinal: return "A caravana chegou ao confronto decisivo.";
            case TextoBitacoraId.NodoAtaqueCaravana: return "A caravana entrou em uma \u00E1rea exposta.";
            case TextoBitacoraId.NodoSubterraneo: return "A terra se abriu sob a caravana.";
            case TextoBitacoraId.NodoSantuario: return "A caravana chegou a um santu\u00E1rio.";
            case TextoBitacoraId.NodoRitual: return "A caravana chegou a um local ritual.";
            case TextoBitacoraId.NodoSalvamento: return "A caravana chegou a um ponto de resgate.";
            default: return "A caravana chegou a outro trecho da estrada.";
        }
    }

    private enum TextoBitacoraId
    {
        EtiquetaDia,
        EtiquetaRonda,
        SinRegistros,
        RecursoEsperanza,
        RecursoOro,
        RecursoMateriales,
        RecursoSuministros,
        CaravanaDescanso,
        ResumenVictoria,
        ResumenDerrota,
        CayoCompaneroSingular,
        CayoCompaneroPlural,
        ConectorLista,
        FaccionEnemigaGenerica,
        NodoBatalla,
        NodoEvento,
        NodoClaro,
        NodoAsentamiento,
        NodoRecursos,
        NodoPuestoComercial,
        NodoViajeros,
        NodoBatallaElite,
        NodoBatallaFinal,
        NodoAtaqueCaravana,
        NodoSubterraneo,
        NodoSantuario,
        NodoRitual,
        NodoSalvamento,
        NodoGenerico
    }
}
