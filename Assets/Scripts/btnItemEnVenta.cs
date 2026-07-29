using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class btnItemEnVenta : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    
    public Item itemRepresentado;

    public Image imageMuestraItem;
    [SerializeField] private Image imagePin;

    public TextMeshProUGUI txtItemVentaDescripcion;
    SequitoMercaderes scSequitoMercaderes;
    private Sprite pinSprite;
    private bool estaPineado;
    private bool bloquearSiguienteClickDeCompra;

    private void Awake()
    {
      AsegurarReferenciaPin();
    }

    void Start()
    {
       txtItemVentaDescripcion = transform.parent.parent.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
       scSequitoMercaderes = transform.parent.parent.parent.parent.gameObject.GetComponent<SequitoMercaderes>();
       AsegurarReferenciaPin();
       ActualizarPinVisual();
    }

    public void ConfigurarPin(Sprite pin, bool pineado)
    {
      pinSprite = pin;
      estaPineado = pineado;
      AsegurarReferenciaPin();
      ActualizarPinVisual();
    }

    private void AsegurarReferenciaPin()
    {
      if (imagePin != null)
      {
        return;
      }

      Transform pinTransform = transform.Find("Pin");
      if (pinTransform != null)
      {
        imagePin = pinTransform.GetComponent<Image>();
      }

      if (imagePin == null)
      {
        GameObject goPin = new GameObject("Pin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        goPin.transform.SetParent(transform, false);
        goPin.transform.SetAsLastSibling();

        RectTransform rt = goPin.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-4f, -4f);
        rt.sizeDelta = new Vector2(24f, 24f);

        imagePin = goPin.GetComponent<Image>();
        imagePin.raycastTarget = false;
      }
    }

    private void ActualizarPinVisual()
    {
      if (imagePin == null)
      {
        return;
      }

      imagePin.sprite = pinSprite;
      imagePin.enabled = estaPineado && pinSprite != null;
    }
    public void ClickearItem()
    {
      if (bloquearSiguienteClickDeCompra)
      {
        bloquearSiguienteClickDeCompra = false;
        return;
      }

      int costoCompra = CampaignManager.Instance.ObtenerCostoCompraConPresagios(itemRepresentado.iPrecio);
      if(CampaignManager.Instance.GetOroActuales() >= costoCompra)
      {
        scSequitoMercaderes.DespinearItem(itemRepresentado);
        CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(itemRepresentado.gameObject);
        scSequitoMercaderes.ItemsVendidos.Remove(itemRepresentado);
        scSequitoMercaderes.MostrarInventarioVenta();

        CampaignManager.Instance.CambiarOroActual(-costoCompra);
        RuntimeAnalytics.TrackResourceSink("gold", costoCompra, "merchant_item", RuntimeAnalytics.ItemToken(itemRepresentado));
        RuntimeAnalytics.TrackDesign("merchant", "buy", RuntimeAnalytics.ItemToken(itemRepresentado));
        RuntimeAnalytics.TrackItemAcquired(itemRepresentado, "merchant");
      }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
      bloquearSiguienteClickDeCompra = eventData != null
        && eventData.button != PointerEventData.InputButton.Left;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      if (eventData.button != PointerEventData.InputButton.Right)
      {
        return;
      }

      PinearItem();
    }

    public void PinearItem()
    {
      if (scSequitoMercaderes == null || itemRepresentado == null)
      {
        return;
      }

      scSequitoMercaderes.IntentarPinearItem(itemRepresentado);
    }

    public void HoverItem(int n)
    { 
        if(n == 1)
        {
        
         string precio ="";
         int costoCompra = CampaignManager.Instance.ObtenerCostoCompraConPresagios(itemRepresentado.iPrecio);
         if(CampaignManager.Instance.GetOroActuales() >= costoCompra)
         {
           precio =TRADU.i.Traducir("<Color=#e6b50f>\nPrecio: ")+costoCompra+"</Color>";
         }
         else
         {
           precio =TRADU.i.Traducir("<Color=#e60f0f>\nPrecio: ")+costoCompra+"</Color>";
         }
          

         txtItemVentaDescripcion.text = ItemTooltipFormatter.ConstruirTooltip(itemRepresentado, true) + precio;
         
        }
        if(n == 0)
        {
                     
          txtItemVentaDescripcion.text = "";

            

        }
       
    }



}
