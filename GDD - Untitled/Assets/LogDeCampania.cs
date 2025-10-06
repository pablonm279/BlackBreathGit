using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class LogDeCampania : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI txtLog;

    [Header("Comportamiento")]
    [Tooltip("Máxima cantidad de entradas (eventos). Se recorta por FIFO.")]
    [SerializeField] private int maxEntradas = 80;

    [Tooltip("Día actual (se puede setear desde CampaignManager).")]
    [SerializeField] private int diaActual = 1;

    [Header("Estilos")]
    [SerializeField] private string colorDia = "#2c81b9ff";     // Encabezado de día (prefijo de línea)
    [SerializeField] private string colorActual = "#000000ff";  // Texto día actual
    [SerializeField] private string colorPasado = "#565656ff";  // Texto días previos
    [SerializeField] private int sizeActualPct = 115;         // % tamaño día actual
    [SerializeField] private int sizePasadoPct = 80;          // % tamaño días pasados

    private readonly List<EntradaLog> _entradas = new();

    private static readonly Regex _regexTagsTMP =
        new Regex(@"<\/?((b|i|u|s|mark|align|alpha|color|font|indent|line-height|link|lowercase|uppercase|smallcaps|size|space|sprite|style|sup|sub|voffset)(=[^>]*)?)>",
                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private struct EntradaLog
    {
        public int Dia;
        public string Texto; // Siempre sin tags
    }

    // --- API pública ---

    /// <summary>Actualiza el día actual (p.ej., al avanzar nodo/viaje).</summary>
    public void SetDiaActual(int numeroTurno)
    {
        diaActual = numeroTurno;
        ReconstruirTextoAjustado();
    }

    /// <summary>Agrega una entrada y refresca el texto.</summary>
    public void Escribir(string mensaje, bool esCombate = false)
    {
        if (txtLog == null) return;
        // Si estamos en combate y el BattleManager pide silenciar logs (preparación), no escribir
        if (esCombate && BattleManager.Instance != null && BattleManager.Instance.silenciarLogCombate)
            return;

        var limpia = Sanitizar(mensaje);
       
            _entradas.Add(new EntradaLog { Dia = diaActual, Texto = limpia });
        

        RecortarSiExcede();
        ReconstruirTextoAjustado(esCombate);
    }

    string _textoCampañaAnterior = "";

    /// <summary>Limpia todo el log (opcional).</summary>
    public void Limpiar()
    {
        _entradas.Clear();
        txtLog.text = "";
    }

    public void LimpiarDesdeCampania()
    {
        _textoCampañaAnterior = txtLog.text;
        _entradas.Clear();
        txtLog.text = "";
    }

    public void LimpiarDesdeBatalla()
    {
        _textoCampañaAnterior = "";
        _entradas.Clear();
        txtLog.text = _textoCampañaAnterior;
    }

    // --- Internos ---

    private void RecortarSiExcede()
    {
        if (_entradas.Count <= maxEntradas) return;

        int removeCount = _entradas.Count - maxEntradas;
        _entradas.RemoveRange(0, removeCount);
    }

   private void ReconstruirTextoAjustado(bool esCombate = false)
{
    if (txtLog == null) return;

    // 1) Construir una versión completa del texto (sin recortar)
    string RenderizarEntradas(List<EntradaLog> entradas)
    {
        var sb = new StringBuilder(entradas.Count * 64);
        for (int i = 0; i < entradas.Count; i++)
        {
            var e = entradas[i];
                string prefijoDia;
                if (esCombate)
                {
                     prefijoDia = $"<color={colorDia}>- Ronda {e.Dia}</color>";
                }
                else
                {
                     prefijoDia = $"<color={colorDia}>- Día {e.Dia}</color>";
                }

            if (e.Dia == diaActual)
            {
                sb.Append("<size=").Append(sizeActualPct).Append("%>")
                  .Append("<color=").Append(colorActual).Append(">")
                  .Append(prefijoDia).Append(" — ").Append(e.Texto)
                  .Append("</color></size>");
            }
            else
            {
                sb.Append("<i>")
                  .Append("<size=").Append(sizePasadoPct).Append("%>")
                  .Append("<color=").Append(colorPasado).Append(">")
                  .Append(prefijoDia).Append(" — ").Append(e.Texto)
                  .Append("</color></size>")
                  .Append("</i>");
            }
            if (i < entradas.Count - 1) sb.Append('\n');
        }
        return sb.ToString();
    }

    // 2) Intentar con todas las entradas y medir
    txtLog.enableWordWrapping = true;                       // que haga wrap
    txtLog.richText = true;                                 // usamos tags
    txtLog.overflowMode = TextOverflowModes.Truncate;       // por si acaso
    txtLog.enableAutoSizing = false;                        // fuente fija

    // Rect visible del TMP
    var rt = (RectTransform)txtLog.transform;
    float maxWidth  = rt.rect.width;
    float maxHeight = rt.rect.height;

    // Construcción inicial
    string texto = RenderizarEntradas(_entradas);
    txtLog.text = texto;

    // 3) Medir altura preferida; si excede, vamos borrando lo más viejo
    //    (FIFO) hasta que entre por completo.
    //    Para medir: GetPreferredValues(con width acotado) devuelve la altura real que necesita.
    Vector2 pref = txtLog.GetPreferredValues(txtLog.text, maxWidth, 0);

    // margen de seguridad para que no “respire” al pixel
    const float margen = 2f;

    if (pref.y > maxHeight - margen)
    {
        // Copia de trabajo
        var tmpLista = new List<EntradaLog>(_entradas);

        // Vamos quitando del principio hasta que entre
        // (quitamos la entrada 0 que es la más vieja).
        while (tmpLista.Count > 0)
        {
            tmpLista.RemoveAt(0);
            texto = RenderizarEntradas(tmpLista);
            txtLog.text = texto;
            pref = txtLog.GetPreferredValues(txtLog.text, maxWidth, 0);

            if (pref.y <= maxHeight - margen)
            {
                // Aplicamos el recorte definitivo al estado
                _entradas.Clear();
                _entradas.AddRange(tmpLista);
                break;
            }
        }
    }
}


    private static string Sanitizar(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        // Quita todas las etiquetas TMP comunes para evitar anidaciones raras
        string limpio = _regexTagsTMP.Replace(s, string.Empty);
        // Normalizar saltos de línea a una sola línea (log = 1 evento por línea)
        limpio = limpio.Replace("\r\n", " ").Replace('\n', ' ').Trim();
        return limpio;
    }
}
