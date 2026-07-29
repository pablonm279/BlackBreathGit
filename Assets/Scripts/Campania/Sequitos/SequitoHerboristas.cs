using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoHerboristas : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;

    [SerializeField] TextMeshProUGUI txtTituloBalsamoFort;
    [SerializeField] TextMeshProUGUI txtDescripcionBalsamoFort;
    [SerializeField] TextMeshProUGUI txtTituloBalsamoReflej;
    [SerializeField] TextMeshProUGUI txtDescripcionBalsamoReflej;
    [SerializeField] TextMeshProUGUI txtTituloBalsamoMental;
    [SerializeField] TextMeshProUGUI txtDescripcionBalsamoMental;

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

    public void Actualizar()
    {
        txtTituloBalsamoFort.text = TraducirLocal("Bálsamo Reforzante:\n", "Fortifying Balm:\n", "Bálsamo Fortalecedor:\n");
        txtDescripcionBalsamoFort.text = TraducirLocal("Consumible: +2 TS Fortaleza durante el combate.", "Consumable: +2 Fortitude Save during combat.", "Consumível: +2 Fortitude durante o combate.");
        txtTituloBalsamoReflej.text = TraducirLocal("Bálsamo Energizante:\n", "Energizing Balm:\n", "Bálsamo Energizante:\n");
        txtDescripcionBalsamoReflej.text = TraducirLocal("Consumible: +2 TS Reflejos durante el combate.", "Consumable: +2 Reflex Save during combat.", "Consumível: +2 Reflexos durante o combate.");
        txtTituloBalsamoMental.text = TraducirLocal("Bálsamo de Claridad:\n", "Balm of Clarity:\n", "Bálsamo de Clareza:\n");
        txtDescripcionBalsamoMental.text = TraducirLocal("Consumible: +2 TS Mental durante el combate.", "Consumable: +2 Mental Save during combat.", "Consumível: +2 Mental durante o combate.");


        txtDesc.text = TRADU.i.Traducir("Un grupo de especialistas en recolectar hierbas y crear con ellas bélsamos especiales para vender. \nAdemás, sus hierbas proporcionarán beneficios curativos a la caravana.\nPero quizás no sean demasiado cuidadosos al adentrarse en regiones peligrosas para recolectar hierbas.\n\n");
        txtMecanica.text = TRADU.i.Traducir("EFECTOS PASIVOS:\n\n-Hierbas curativas: Mejoran ") + (3 + vecesEnClaro * 3) + TRADU.i.Traducir("% la curación pasiva de la Caravana.\n\nEste índice aumenta un 3% cada vez que la Caravana visite un Claro.\n\n-A veces son descuidados al recolectar hierbas. +2% chances de que se de un ataque a la caravana tras descansar.");

        int oroActual = CampaignManager.Instance.GetOroActuales();
        
        
        string precioFort = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";
        string precioReflej = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";
        string precioMental = oroActual >= 50 ? TRADU.i.Traducir("50 de oro") : "<color=#FF0000>"+TRADU.i.Traducir("50 de oro")+"</color>";

        txstCantBalsamoFort.text = $"{cantBalsamoFort}/2 - {precioFort}";
        txstCantBalsamoReflej.text = $"{cantBalsamoReflej}/2 - {precioReflej}";
        txstCantBalsamoMental.text = $"{cantBalsamoMental}/2 - {precioMental}";
    }

    private string TraducirLocal(string espanol, string ingles, string portugues)
    {
        if (TRADU.i == null || TRADU.i.nIdioma == TRADU.IdiomaEspanol)
        {
            return espanol;
        }

        return TRADU.i.nIdioma == TRADU.IdiomaPortugues ? portugues : ingles;
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
                Item item = consumible.GetComponent<Item>();
                RuntimeAnalytics.TrackResourceSink("gold", 50, "merchant_item", RuntimeAnalytics.ItemToken(item));
                RuntimeAnalytics.TrackItemAcquired(item, "herbalists");
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
                Item item = consumible.GetComponent<Item>();
                RuntimeAnalytics.TrackResourceSink("gold", 50, "merchant_item", RuntimeAnalytics.ItemToken(item));
                RuntimeAnalytics.TrackItemAcquired(item, "herbalists");
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
                Item item = consumible.GetComponent<Item>();
                RuntimeAnalytics.TrackResourceSink("gold", 50, "merchant_item", RuntimeAnalytics.ItemToken(item));
                RuntimeAnalytics.TrackItemAcquired(item, "herbalists");
            }
        }



        Actualizar();
    }
}


