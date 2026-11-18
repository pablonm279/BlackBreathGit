using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandbookManager : MonoBehaviour
{
    private const int SolapaInicial = 1;

    // Guarda el texto español original de cada TextMeshProUGUI del handbook la primera vez que
    // se traduce, para poder re-traducir siempre desde el original real (nunca desde un texto
    // que ya podría estar en chino) sin depender del barrido global de traducción, que corre una
    // sola vez al iniciar/cambiar idioma y puede no haber alcanzado a este contenido.
    private static readonly Dictionary<TextMeshProUGUI, string> textosOriginalesHandbook = new Dictionary<TextMeshProUGUI, string>();

    public GameObject solapaZonas;
    public GameObject solapaMapa;
    public GameObject solapaCiviles;
    public GameObject solapaRecursos;
    public GameObject solapaAlientoNegro;
    public GameObject solapaPersonajes;
    public GameObject solapaExploracion;

    public GameObject solapaEstados;



    public GameObject elOtroHandbook;

    public bool esdeBatalla;

    void OnEnable()
    {
        AbrirSolapa(SolapaInicial);
    }

    public void AbrirSolapa(int ID)
    {
        int nIdioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
        CerrartodasSolapas();

        GameObject target = null;
        switch (ID)
        {
            case 1: target = solapaZonas; break;
            case 2: target = solapaMapa; break;
            case 3: target = solapaCiviles; break;
            case 4: target = solapaRecursos; break;
            case 5: target = solapaAlientoNegro; break;
            case 6: target = solapaPersonajes; break;
            case 7: target = solapaEstados; break;
            case 8: target = solapaExploracion; break;
            default: return;
        }

        if (target == null) return;

        GameObject hijoActivado = ActivarContenidoPorIdioma(target, nIdioma);

        // Para chino no existe un hijo propio de idioma: ActivarContenidoPorIdioma cae al hijo
        // en español (childIdioma = 1 por defecto). En vez de depender del barrido global de
        // traducción (que corre una sola vez al iniciar/cambiar idioma y puede no haber
        // alcanzado a este contenido si el handbook se abre después), traducimos directamente
        // los textos de ese hijo, cacheando el original la primera vez para no retraducir un
        // texto que ya está en chino en aperturas posteriores de la misma solapa.
        if (TRADU.i != null && nIdioma == TRADU.IdiomaChinoSimplificado && hijoActivado != null)
        {
            TextMeshProUGUI[] textos = hijoActivado.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < textos.Length; i++)
            {
                TextMeshProUGUI texto = textos[i];
                if (texto == null)
                {
                    continue;
                }

                if (!textosOriginalesHandbook.TryGetValue(texto, out string original))
                {
                    original = texto.text;
                    textosOriginalesHandbook[texto] = original;
                }

                if (!string.IsNullOrEmpty(original))
                {
                    texto.text = TRADU.i.Traducir(original);
                }
            }
        }

        if (target.transform.childCount > 0)
        {
            var tm = target.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (tm != null) tm.color = Color.white;
        }
    }

    void CerrartodasSolapas()
    {
        GameObject[] solapas = {
            solapaZonas, solapaMapa, solapaCiviles, solapaRecursos,
            solapaAlientoNegro, solapaPersonajes, solapaEstados, solapaExploracion
        };
        for (int i = 0; i < solapas.Length; i++)
        {
            var s = solapas[i];
            if (s == null) continue;
            if (s.transform.childCount > 0)
            {
                var tm = s.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (tm != null) tm.color = esdeBatalla ? Color.red : Color.cyan; // restore inactive/original color
            }
        }
        foreach (var s in solapas)
        {
            if (s == null) continue;
            for (int i = 1; i < s.transform.childCount; i++)
            {
                s.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }


    public void AbrirElOtroHandbook()
    {
        if (elOtroHandbook != null)
        {
            elOtroHandbook.SetActive(true);
            this.gameObject.SetActive(false);

            HandbookManager otro = elOtroHandbook.GetComponent<HandbookManager>();
            if (otro != null)
            {
                otro.AbrirSolapa(SolapaInicial);
            }
        }
    }
    
    public void cerrarHandbook()
    {
        transform.parent.gameObject.SetActive(false);
    }

    private static GameObject ActivarContenidoPorIdioma(GameObject solapa, int nIdioma)
    {
        if (solapa == null) return null;

        int childCount = solapa.transform.childCount;
        int childIdioma = 1;

        if (nIdioma == TRADU.IdiomaIngles && childCount > 2)
        {
            childIdioma = 2;
        }
        else if (nIdioma == TRADU.IdiomaPortugues && childCount > 3)
        {
            childIdioma = 3;
        }

        if (childCount > childIdioma)
        {
            GameObject hijo = solapa.transform.GetChild(childIdioma).gameObject;
            hijo.SetActive(true);
            return hijo;
        }

        if (childCount > 1)
        {
            GameObject hijo = solapa.transform.GetChild(1).gameObject;
            hijo.SetActive(true);
            return hijo;
        }

        return null;
    }
}
