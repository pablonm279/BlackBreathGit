using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandbookManager : MonoBehaviour
{
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

        ActivarContenidoPorIdioma(target, nIdioma);

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
                otro.AbrirSolapa(1);
            }
        }
    }
    
    public void cerrarHandbook()
    {
        transform.parent.gameObject.SetActive(false);
    }

    private static void ActivarContenidoPorIdioma(GameObject solapa, int nIdioma)
    {
        if (solapa == null) return;

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
            solapa.transform.GetChild(childIdioma).gameObject.SetActive(true);
            return;
        }

        if (childCount > 1)
        {
            solapa.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
