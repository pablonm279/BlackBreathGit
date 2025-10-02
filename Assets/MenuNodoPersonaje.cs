using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuNodoPersonaje : MonoBehaviour
{
    public GameObject goMenuPersonaje;
    public TextMeshProUGUI Claseboton1;
    public TextMeshProUGUI Claseboton2;
    public TextMeshProUGUI Claseboton3;

    public int IDClase1;
    public int IDClase2;
    public int IDClase3;

    public void ElegirBoton(int boton)
    {
        switch (boton)
        {
            case 1:
                CampaignManager.Instance.AgregarHeroe(IDClase1);
                break;
            case 2:
                CampaignManager.Instance.AgregarHeroe(IDClase2);
                break;
            case 3:
                CampaignManager.Instance.AgregarHeroe(IDClase3);
                break;
            default:
                Debug.LogError("Botón no válido: " + boton);
                break;
        }

        goMenuPersonaje.SetActive(false);
    }


    void OnEnable()
    {
        List<int> ids = new List<int> { 1, 2, 3, 4, 5 };
        for (int i = 0; i < ids.Count; i++)
        {
            int rnd =UnityEngine.Random.Range(i, ids.Count);
            int temp = ids[i];
            ids[i] = ids[rnd];
            ids[rnd] = temp;
        }
        IDClase1 = ids[0];
        IDClase2 = ids[1];
        IDClase3 = ids[2];

        ActualizarTextoBoton(Claseboton1, IDClase1);
        ActualizarTextoBoton(Claseboton2, IDClase2);
        ActualizarTextoBoton(Claseboton3, IDClase3);

        //----
        GenerarSequitoPropuesto();

    }

    void ActualizarTextoBoton(TextMeshProUGUI txt, int ID)
    {
        switch (ID)
        {
            case 1:
                txt.text = "Caballero";
                break;
            case 2:
                txt.text = "Explorador";
                break;
            case 3:
                txt.text = "Purificadora";
                break;
            case 4:
                txt.text = "Acechador";
                break;
            case 5:
                txt.text = "Canalizador";
                break;

        }


    }


    GameObject GOSequitoPropuesto;
    [SerializeField] Image imagensequito;
    [SerializeField] TextMeshProUGUI descSequito;

    int sequitoPropuesto = 0;
    void GenerarSequitoPropuesto()
    {
        // Lógica para generar el séquito propuesto
        sequitoPropuesto =UnityEngine.Random.Range(4, 12); // AUMENTAR AL CREAR MAS SEQUITOS
        switch (sequitoPropuesto)
        {
            case 4:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito004Artistas, transform.GetChild(1).gameObject.transform);
                break;
            case 5:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito005Herboristas, transform.GetChild(1).gameObject.transform);
                break;
            case 6:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito006Desertores, transform.GetChild(1).gameObject.transform);
                break;
            case 7:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito007Cronistas, transform.GetChild(1).gameObject.transform);
                break;
            case 8:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito008Refugiados, transform.GetChild(1).gameObject.transform);
                break;
            case 9:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito009Nobles, transform.GetChild(1).gameObject.transform);
                break;
            case 10:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito010Clerigos, transform.GetChild(1).gameObject.transform);
                break;
            case 11:
                GOSequitoPropuesto = Instantiate(CampaignManager.Instance.scMenuSequito.Sequito011Esclavos, transform.GetChild(1).gameObject.transform);
                break;
                //agregar nuevos!!
        }

        GOSequitoPropuesto.SetActive(false); //lo esconde
        imagensequito.sprite = GOSequitoPropuesto.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite;
        descSequito.text = GOSequitoPropuesto.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text + "\n\n" + GOSequitoPropuesto.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text;
        descSequito.text += "\n\n<size=75%><color=#CCCCCC>" + GOSequitoPropuesto.GetComponent<Sequito>().txtmecanicas.text + "</color></size>";
    }

    void OnDisable()
    {
        if (GOSequitoPropuesto != null)
        {
            Destroy(GOSequitoPropuesto);
        }
    }

    public void AceptarSequito()
    {
        if (GOSequitoPropuesto != null)
        {
            CampaignManager.Instance.scMenuSequito.AgregarSequito(sequitoPropuesto);
            GOSequitoPropuesto = null; // Evita destruirlo en OnDisable
        }
        
        goMenuPersonaje.SetActive(false);
    }


}
