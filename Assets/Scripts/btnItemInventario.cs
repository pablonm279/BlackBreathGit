using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class btnItemInventario : MonoBehaviour
{
    
    public Item itemRepresentado;

    public Image imageMuestraItem;
    [SerializeField] private Color colorItemNormal = Color.white;
    [SerializeField] private Color colorItemOscurecido = new Color(0.55f, 0.55f, 0.55f, 1f);

    public MenuPersonajes scMenuPersonajes;

    
    void Start()
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
    }

    public void SetOscurecido(bool oscurecido)
    {
      if (imageMuestraItem == null)
      {
        return;
      }

      imageMuestraItem.color = oscurecido ? colorItemOscurecido : colorItemNormal;
    }

    public void ClickearItem()
    {
       TooltipItems.Instance.HideTooltip();
    // Si no hay clases configuradas o hay -1, el item lo pueden usar todas las clases.
    if (!itemRepresentado.PuedeUsarClase(scMenuPersonajes.pSel.IDClase)) { return; }

       if (itemRepresentado.GetComponent<Arma>() != null)
      {
        if (!scMenuPersonajes.EquiparArmaDesdeInventario((Arma)itemRepresentado)) { return; }
        RuntimeAnalytics.TrackDesign("characters", "equip", RuntimeAnalytics.ItemToken(itemRepresentado));
        scMenuPersonajes.scEquipo.CerrarInventario();
        Invoke("ActualizarInfoDelay", 0.1f);
        return;
      }

         if(itemRepresentado.GetComponent<Armadura>() != null)
        {
          if (!scMenuPersonajes.EquiparArmaduraDesdeInventario((Armadura)itemRepresentado)) { return; }
          RuntimeAnalytics.TrackDesign("characters", "equip", RuntimeAnalytics.ItemToken(itemRepresentado));
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
          return;
        }

         if(itemRepresentado.GetComponent<Accesorio>() != null)
        {
          if (!scMenuPersonajes.EquiparAccesorioDesdeInventario((Accesorio)itemRepresentado)) { return; }
          RuntimeAnalytics.TrackDesign("characters", "equip", RuntimeAnalytics.ItemToken(itemRepresentado));
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
          return;
        }

         if(itemRepresentado.GetComponent<Consumible>() != null)
        {
          if (!scMenuPersonajes.EquiparConsumibleDesdeInventario((Consumible)itemRepresentado)) { return; }
          RuntimeAnalytics.TrackDesign("characters", "equip", RuntimeAnalytics.ItemToken(itemRepresentado));
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
          return;
        }

      





    }
    void ActualizarInfoDelay()
    {
        scMenuPersonajes.ActualizarInfo();
        scMenuPersonajes.ActualizarListaHabilidades();
        
    }
    public void HoverItem(int n)
    {
    if (n == 1)
    {
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(itemRepresentado, true);
      TooltipItems.Instance.ShowTooltip(total, pos);

        }
        if(n == 0)
        {

         TooltipItems.Instance.HideTooltip();

            

        }
       
    }



}
