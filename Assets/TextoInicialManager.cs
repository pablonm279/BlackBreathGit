using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextoInicialManager : MonoBehaviour
{

    public GameObject versionEspañol;
    public GameObject versionIngles;


    void Start()
    {
        if (TRADU.i.nIdioma == 1)
        {
            versionEspañol.SetActive(true);
            versionIngles.SetActive(false);
        }
        if (TRADU.i.nIdioma == 2)
        {
            versionEspañol.SetActive(false);
            versionIngles.SetActive(true);
        }
    }



    public void Continuar()
    {
        gameObject.SetActive(false);
        CampaignManager.Instance.scTutorialManager.ComenzarTutorial();
    }

}
