using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoHerboristas : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;

    public int vecesEnClaro = 0;
    [SerializeField] TextMeshProUGUI txtMecanica;


    public int cantBalsamoFort = 2;
    [SerializeField] TextMeshProUGUI txstCantBalsamoFort;

    public int cantBalsamoReflej = 2;
    [SerializeField] TextMeshProUGUI txstCantBalsamoReflej;
    public int cantBalsamoMental = 2;
    [SerializeField] TextMeshProUGUI txstCantBalsamoMental;
    void OnEnable()
    {
        Actualizar();
    }

    void Actualizar()
    {


        txtDesc.text = TRADU.i.Traducir("Un grupo de especialistas en recolectar hierbas y crear con ellas bálsamos especiales para vender. \nAdemás, sus hierbas proporcionarán beneficios curativos a la caravana.\nPero quizás no sean demasiado cuidadosos al adentrarse en zonas peligrosas para recolectar hierbas.\n\n");
        txtMecanica.text = TRADU.i.Traducir("EFECTOS PASIVOS:\n\n-Hierbas curativas: Mejoran ") + (3 + vecesEnClaro * 3) + TRADU.i.Traducir("% la curación pasiva de la Caravana.\n\nEste índice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.");

        int oroActual = CampaignManager.Instance.GetOroActuales();
        
        
        string precioFort = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";
        string precioReflej = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";
        string precioMental = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";

        txstCantBalsamoFort.text = $"{cantBalsamoFort}/2 - {precioFort}";
        txstCantBalsamoReflej.text = $"{cantBalsamoReflej}/2 - {precioReflej}";
        txstCantBalsamoMental.text = $"{cantBalsamoMental}/2 - {precioMental}";
    }



    public void EcharSequito()
    {
        //--- Destruye el séquito de artistas
        CampaignManager.Instance.scMenuSequito.RemoverSequito(5);
        Destroy(gameObject);
    }

    public void comprarbalsamos(int n)
    {
        if (n == 1)
        {
            if (CampaignManager.Instance.GetOroActuales() >= 50 && cantBalsamoFort > 0)
            {
                CampaignManager.Instance.CambiarOroActual(-50);
                cantBalsamoFort--;
                GameObject consumible = Instantiate( CampaignManager.Instance.scContprefab.BalsamoFortalecedor.gameObject);
                CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(consumible);
            }
        }
        
        if (n == 2)
        {
            if (CampaignManager.Instance.GetOroActuales() >= 50 && cantBalsamoReflej > 0)
            {
                CampaignManager.Instance.CambiarOroActual(-50);
                cantBalsamoReflej--;
                GameObject consumible = Instantiate( CampaignManager.Instance.scContprefab.BalsamoEnergizante.gameObject);
                CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(consumible);
            }
        }

        if (n == 3)
        {
            if (CampaignManager.Instance.GetOroActuales() >= 50 && cantBalsamoMental > 0)
            {
                CampaignManager.Instance.CambiarOroActual(-50);
                cantBalsamoMental--;
                GameObject consumible = Instantiate( CampaignManager.Instance.scContprefab.BalsamoClaridad.gameObject);
                CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(consumible);
            }
        }



        Actualizar();
    }
}