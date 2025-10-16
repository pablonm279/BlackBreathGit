using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class btnItemEnVenta : MonoBehaviour
{
    
    public Item itemRepresentado;

    public Image imageMuestraItem;

    public TextMeshProUGUI txtItemVentaDescripcion;
    SequitoMercaderes scSequitoMercaderes;

    
    void Start()
    {
       txtItemVentaDescripcion = transform.parent.parent.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
       scSequitoMercaderes = transform.parent.parent.parent.parent.gameObject.GetComponent<SequitoMercaderes>();
    }
    public void ClickearItem()
    {
      if(CampaignManager.Instance.GetOroActuales() >= itemRepresentado.iPrecio)
      {
        CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(itemRepresentado.gameObject);
        scSequitoMercaderes.ItemsVendidos.Remove(itemRepresentado);
        scSequitoMercaderes.MostrarInventarioVenta();

        CampaignManager.Instance.CambiarOroActual(-itemRepresentado.iPrecio);
      }

    }
    public void HoverItem(int n)
    { 
        if(n == 1)
        {
        
         string precio ="";
         if(CampaignManager.Instance.GetOroActuales() >= itemRepresentado.iPrecio)
         {
           precio =TRADU.i.Traducir("<Color=#e6b50f>\nPrecio: ")+itemRepresentado.iPrecio+"</Color>";
         }
         else
         {
           precio =TRADU.i.Traducir("<Color=#e60f0f>\nPrecio: ")+itemRepresentado.iPrecio+"</Color>";
         }
          




         txtItemVentaDescripcion.text = TRADU.i.Traducir(itemRepresentado.itemDescripcion)+precio;
         
        }
        if(n == 0)
        {
                     
          txtItemVentaDescripcion.text = "";

            

        }
       
    }



}
