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

    public GameObject solapaEstados;



    public GameObject elOtroHandbook;

    public bool esdeBatalla;
    public void AbrirSolapa(int ID)
    {
        int nIdioma = TRADU.i.nIdioma;
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
            default: return;
        }

        if (nIdioma == 1)
        {
            if (target.transform.childCount > 1)
                target.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (nIdioma == 2)
        {
            if (target.transform.childCount > 2)
                target.transform.GetChild(2).gameObject.SetActive(true);
        }

        if (target != null && target.transform.childCount > 0)
        {
            var tm = target.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (tm != null) tm.color = Color.white;
        }
    }

    void CerrartodasSolapas()
    {
        GameObject[] solapas = {
            solapaZonas, solapaMapa, solapaCiviles, solapaRecursos,
            solapaAlientoNegro, solapaPersonajes, solapaEstados
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

            elOtroHandbook.GetComponent<HandbookManager>().AbrirSolapa(1);
        }
    }
    
    public void cerrarHandbook()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
