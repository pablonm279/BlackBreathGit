using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class btnItemInventario : MonoBehaviour
{
    
    public Item itemRepresentado;

    public Image imageMuestraItem;

    public MenuPersonajes scMenuPersonajes;

    
    void Start()
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
    }
    public void ClickearItem()
    {
       TooltipItems.Instance.HideTooltip();
    //Si la clase del personaje no puede usar el item, no hace nada
    if (!itemRepresentado.GetComponent<Item>().IDClasesQuePuedenUsarEsteItem.Contains(-1)) //Si es -1 lo pueden usar todos
    {
     
      if (!itemRepresentado.GetComponent<Item>().IDClasesQuePuedenUsarEsteItem.Contains(scMenuPersonajes.pSel.IDClase)) { return; } //Si no es -1 y no posee la clase, no puede usarlo
    }

       if (itemRepresentado.GetComponent<Arma>() != null)
      {


        if (itemRepresentado.GetComponent<Arma>().requisitoAgi > scMenuPersonajes.pSel.iAgi) { return; }
        if (itemRepresentado.GetComponent<Arma>().requisitoFue > scMenuPersonajes.pSel.iFuerza) { return; }
        if (itemRepresentado.GetComponent<Arma>().requisitoPoder > scMenuPersonajes.pSel.iPoder) { return; }

        scMenuPersonajes.pSel.itemArma = (Arma)itemRepresentado;

        scMenuPersonajes.scEquipo.listInventario.Remove(itemRepresentado.gameObject);
        scMenuPersonajes.scEquipo.CerrarInventario();
        Invoke("ActualizarInfoDelay", 0.1f);

      }

         if(itemRepresentado.GetComponent<Armadura>() != null)
        {
         
         if(itemRepresentado.GetComponent<Armadura>().requisitoAgi > scMenuPersonajes.pSel.iAgi){ return;}
         if(itemRepresentado.GetComponent<Armadura>().requisitoFue > scMenuPersonajes.pSel.iFuerza){ return;}
         if(itemRepresentado.GetComponent<Armadura>().requisitoPoder > scMenuPersonajes.pSel.iPoder){ return;}
          
          scMenuPersonajes.pSel.itemArmadura = (Armadura)itemRepresentado;
            
          scMenuPersonajes.scEquipo.listInventario.Remove(itemRepresentado.gameObject);
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
            
        }

         if(itemRepresentado.GetComponent<Accesorio>() != null)
        {
         
         if(itemRepresentado.GetComponent<Accesorio>().requisitoAgi > scMenuPersonajes.pSel.iAgi){ return;}
         if(itemRepresentado.GetComponent<Accesorio>().requisitoFue > scMenuPersonajes.pSel.iFuerza){ return;}
         if(itemRepresentado.GetComponent<Accesorio>().requisitoPoder > scMenuPersonajes.pSel.iPoder){ return;}
          
          if(transform.parent.parent.gameObject.GetComponent<Equipo>().accesorioACambiar == 1)
          {
            scMenuPersonajes.pSel.Accesorio1 = (Accesorio)itemRepresentado;
          }
          if(transform.parent.parent.gameObject.GetComponent<Equipo>().accesorioACambiar == 2)
          {
            scMenuPersonajes.pSel.Accesorio2 = (Accesorio)itemRepresentado;
          }
            
          scMenuPersonajes.scEquipo.listInventario.Remove(itemRepresentado.gameObject);
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
            
        }

         if(itemRepresentado.GetComponent<Consumible>() != null)
        {
          
          if(transform.parent.parent.gameObject.GetComponent<Equipo>().consumibleACambiar == 1)
          {
            scMenuPersonajes.pSel.Consumible1 = (Consumible)itemRepresentado;
          }
         if(transform.parent.parent.gameObject.GetComponent<Equipo>().consumibleACambiar == 2)
          {
            scMenuPersonajes.pSel.Consumible2 = (Consumible)itemRepresentado;
          }
            
          scMenuPersonajes.scEquipo.listInventario.Remove(itemRepresentado.gameObject);
          scMenuPersonajes.scEquipo.CerrarInventario();
          Invoke("ActualizarInfoDelay", 0.1f);
            
        }

      





    }
    void ActualizarInfoDelay()
    {
        scMenuPersonajes.ActualizarInfo();
        scMenuPersonajes. ActualizarListaHabilidades();
        
    }
    public void HoverItem(int n)
    {
    if (n == 1)
    {
      Vector3 pos = Input.mousePosition;
      string total = itemRepresentado.sNombreItem + "\n\n" + itemRepresentado.itemDescripcion;
      TooltipItems.Instance.ShowTooltip(total, pos);

        }
        if(n == 0)
        {

         TooltipItems.Instance.HideTooltip();

            

        }
       
    }



}
