using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class LogDeCampania : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI txtLog;

    [Header("Comportamiento")]
    [Tooltip("Máxima cantidad de entradas (eventos). Se recorta por FIFO.")]
    [SerializeField] private int maxEntradas = 80;

    [Tooltip("Día actual de campaña (se puede setear desde CampaignManager).")]
    [SerializeField] private int diaActual = 1;

    private int rondaActual = 1;

    [Header("Estilos")]
    [SerializeField] private string colorDia = "#2c81b9ff";
    [SerializeField] private string colorActual = "#ffffffff";
    [SerializeField] private string colorPasado = "#d4d4d4ff";
    [SerializeField] private int sizeActualPct = 115;
    [SerializeField] private int sizePasadoPct = 80;

    private readonly List<EntradaLog> entradasCampania = new();
    private readonly List<EntradaLog> entradasBatalla = new();
    private bool mostrandoCombate;

    private static readonly Regex regexTagsNoPermitidos =
        new Regex(@"</?(?!\s*(?:b|i|color|size|mark)\b)[^>]+>",
                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex regexGuionInicial =
        new Regex(@"^((?:\s*<[^>]+>\s*)*)-\s*", RegexOptions.Compiled);

    private struct EntradaLog
    {
        public int Dia;
        public string Texto;
    }

    public void SetDiaActual(int numeroTurno, bool esCombate = false)
    {
        if (esCombate)
        {
            rondaActual = numeroTurno;
            if (mostrandoCombate)
            {
                ReconstruirTextoAjustado(true);
            }
            return;
        }

        diaActual = numeroTurno;
        if (!mostrandoCombate)
        {
            ReconstruirTextoAjustado(false);
        }
    }

    public int GetDiaActual()
    {
        return diaActual;
    }

    public void Escribir(string mensaje, bool esCombate = false)
    {
        if (txtLog == null)
        {
            return;
        }

        if (esCombate && BattleManager.Instance != null && BattleManager.Instance.silenciarLogCombate)
        {
            return;
        }

        string limpia = Sanitizar(mensaje);
        List<EntradaLog> entradasObjetivo = ObtenerEntradas(esCombate);
        int diaEntrada = esCombate ? rondaActual : diaActual;

        entradasObjetivo.Add(new EntradaLog { Dia = diaEntrada, Texto = limpia });
        mostrandoCombate = esCombate;

        RecortarSiExcede(entradasObjetivo);
        ReconstruirTextoAjustado(esCombate);
    }

    public void Limpiar()
    {
        entradasCampania.Clear();
        entradasBatalla.Clear();
        mostrandoCombate = false;
        if (txtLog != null)
        {
            txtLog.text = "";
        }
    }

    public void LimpiarDesdeCampania()
    {
        entradasBatalla.Clear();
        mostrandoCombate = true;
        if (txtLog != null)
        {
            txtLog.text = "";
        }
    }

    public void LimpiarDesdeBatalla()
    {
        entradasBatalla.Clear();
        mostrandoCombate = false;
        ReconstruirTextoAjustado(false);
    }

    private List<EntradaLog> ObtenerEntradas(bool esCombate)
    {
        return esCombate ? entradasBatalla : entradasCampania;
    }

    private void RecortarSiExcede(List<EntradaLog> entradas)
    {
        if (entradas.Count <= maxEntradas)
        {
            return;
        }

        int removeCount = entradas.Count - maxEntradas;
        entradas.RemoveRange(0, removeCount);
    }

    private void ReconstruirTextoAjustado(bool esCombate = false)
    {
        if (txtLog == null)
        {
            return;
        }

        List<EntradaLog> entradasActivas = ObtenerEntradas(esCombate);
        int diaActualContexto = esCombate ? rondaActual : diaActual;

        string RenderizarEntradas(List<EntradaLog> entradas)
        {
            var sb = new StringBuilder(entradas.Count * 64);
            for (int i = 0; i < entradas.Count; i++)
            {
                EntradaLog entrada = entradas[i];
                string mensajeRender = NormalizarMensajeRender(entrada.Texto);
                string etiqueta = esCombate
                    ? (TRADU.i != null ? TRADU.i.Traducir("Ronda") : "Ronda")
                    : (TRADU.i != null ? TRADU.i.Traducir("Día") : "Día");
                string prefijoDia = $"<color={colorDia}>- {etiqueta} {entrada.Dia}</color>";

                if (entrada.Dia == diaActualContexto)
                {
                    sb.Append("<size=").Append(sizeActualPct).Append("%>")
                      .Append("<color=").Append(colorActual).Append(">")
                      .Append(prefijoDia).Append(" - ").Append(mensajeRender)
                      .Append("</color></size>");
                }
                else
                {
                    sb.Append("<i>")
                      .Append("<size=").Append(sizePasadoPct).Append("%>")
                      .Append("<color=").Append(colorPasado).Append(">")
                      .Append(prefijoDia).Append(" - ").Append(mensajeRender)
                      .Append("</color></size>")
                      .Append("</i>");
                }

                if (i < entradas.Count - 1)
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }

        txtLog.enableWordWrapping = true;
        txtLog.richText = true;
        txtLog.overflowMode = TextOverflowModes.Truncate;
        txtLog.enableAutoSizing = false;

        var rt = (RectTransform)txtLog.transform;
        float maxWidth = rt.rect.width;
        float maxHeight = rt.rect.height;

        string texto = RenderizarEntradas(entradasActivas);
        txtLog.text = texto;

        Vector2 pref = txtLog.GetPreferredValues(txtLog.text, maxWidth, 0);
        const float margen = 2f;

        if (pref.y <= maxHeight - margen)
        {
            return;
        }

        var tmpLista = new List<EntradaLog>(entradasActivas);
        while (tmpLista.Count > 0)
        {
            tmpLista.RemoveAt(0);
            texto = RenderizarEntradas(tmpLista);
            txtLog.text = texto;
            pref = txtLog.GetPreferredValues(txtLog.text, maxWidth, 0);

            if (pref.y <= maxHeight - margen)
            {
                entradasActivas.Clear();
                entradasActivas.AddRange(tmpLista);
                break;
            }
        }
    }

    private static string Sanitizar(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return string.Empty;
        }

        string limpio = regexTagsNoPermitidos.Replace(s, string.Empty);
        limpio = limpio.Replace("\r\n", " ").Replace('\n', ' ').Trim();
        return limpio;
    }

    private static string NormalizarMensajeRender(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return string.Empty;
        }

        return regexGuionInicial.Replace(s, "$1");
    }
}
