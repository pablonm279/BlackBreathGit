using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creditos : MonoBehaviour
{
    [SerializeField] private GameObject panelEspaniol;
    [SerializeField] private GameObject panelPortugues;
    [SerializeField] private GameObject panelIngles;

    public void AbrirCreditos(bool activar)
    {
        gameObject.SetActive(activar);

        if (activar)
        {
            int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);

            switch (idioma)
            {
                case TRADU.IdiomaIngles:
                    panelIngles.SetActive(true);
                    panelEspaniol.SetActive(false);
                    panelPortugues.SetActive(false);
                    break;
                case TRADU.IdiomaPortugues:
                    panelPortugues.SetActive(true);
                    panelEspaniol.SetActive(false);
                    panelIngles.SetActive(false);
                    break;
                default:
                    panelEspaniol.SetActive(true);
                    panelIngles.SetActive(false);
                    panelPortugues.SetActive(false);
                    break;
            }
        }
    }
}
