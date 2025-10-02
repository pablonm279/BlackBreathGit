using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIbotonesPasivas : MonoBehaviour
{

    public GameObject actionButtonPrefab;

   
    // Start is called before the first frame update

    private List<BotonHabilidad> listaBotonesHabilidad;
    

    private void Awake()
    {
        listaBotonesHabilidad = new List<BotonHabilidad>();
    }
    
    private void Start()
    {
       ActualizarBotonesPasivas();
    }

 
    public void ActualizarBotonesPasivas()
    {     
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
            Destroy(buttonTransform.gameObject);
        }

       if(listaBotonesHabilidad != null)
       {
        listaBotonesHabilidad.Clear();
       }
        
      if(BattleManager.Instance.unidadActiva != null)
      {
        GameObject unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject;
       
       foreach (Habilidad habilidad in unidadSeleccionada.GetComponents<Habilidad>())
       {
           if (habilidad is RetrasarTurno retrasar && retrasar.yaRetraso)
           {
                 continue;
           }

            if(!habilidad.GetType().Name.Contains("REPRESENTACION"))
            {
                continue;
            }

          
           GameObject actionButtonTransform =  Instantiate(actionButtonPrefab, transform);
           BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
           habilidadBotonUI.HabilidadRepresentada = habilidad;
       
           habilidadBotonUI.UpdateCooldownMuestra();
          
        
          if(listaBotonesHabilidad != null)
          {
           listaBotonesHabilidad.Add(habilidadBotonUI);
          }
       } 
      }
     
    }




    public void UIDesactivarHabilidades()
    {
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
           buttonTransform.GetComponent<BotonHabilidad>().DesactivarHabilidad();
        }
    }

     public void UIDesactivarBotones()
    {  
        foreach (Transform buttonTransform in transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
          buttonTransform.GetComponent<BotonHabilidad>().DesactivarBoton();
        }
    }
}
